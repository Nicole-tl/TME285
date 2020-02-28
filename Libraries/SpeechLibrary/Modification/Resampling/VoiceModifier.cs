using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AudioLibrary;
// using DigitalFilterLibrary;
// using Microsoft.Speech.Synthesis;

namespace SpeechLibrary.Modification.Resampling
{
    [DataContract]
    public class VoiceModifier
    {
        private const string MODIFIED_SOUND_NAME = "ModifiedSound";
        private const double DEFAULT_ZERO_CROSSINGS_TIME_INTERVAL = 0.02;
        private const double DEFAULT_TYPICAL_ZERO_CROSSINGS_RATE = 0.10;

        private string name;

        private double relativeDuration;
        private double relativeBrightness;
        private List<double> kneadingFactors;
        private List<double> alphaList;
        private Boolean applyFilters = true; // Change to "false" per default?
        private double zeroCrossingsTimeInterval = DEFAULT_ZERO_CROSSINGS_TIME_INTERVAL;
        private int zeroCrossingsIndexInterval;
        private double typicalZeroCrossingsRate = DEFAULT_TYPICAL_ZERO_CROSSINGS_RATE;

        private WAVSound interpolatedSound; // Needed only when intermediate stages are visualized.
        private WAVSound adjustedComponentSound; // Needed only when intermediate stages are visualized.
        private WAVSound finalModifiedSound;
        private List<Tuple<int, int, double>> zeroCrossingsTupleList = null;

        private Stopwatch stopWatch = null;

        public VoiceModifier()
        {
            kneadingFactors = new List<double>();
            alphaList = new List<double>();
            relativeBrightness = 1;
            relativeDuration = 1;
        }

        private int GetNumberOfZeroCrossingsInInterval(List<double> sampleList, int startIndex, int endIndex)
        {
            int numberOfZeroCrossings = 0;
            if (startIndex + 1 >= sampleList.Count) { return 0; }
            if (Math.Sign(sampleList[startIndex]) != Math.Sign(sampleList[startIndex + 1])) { numberOfZeroCrossings++; }
            for (int ii = startIndex + 2; ii < endIndex; ii++)
            {
                if (sampleList[ii] == 0)
                {
                    if (Math.Sign(sampleList[ii - 1]) != Math.Sign(sampleList[ii + 1])) { numberOfZeroCrossings++; }
                }
                else if (Math.Sign(sampleList[ii]) != Math.Sign(sampleList[ii - 1])) { numberOfZeroCrossings++; }
            }
            return numberOfZeroCrossings;
        }

        private void ComputeZeroCrossingsVariation(List<double> sampleList)
        {
            double average = 0;  // Not used - only here for test purposes (to determine a typical zero crossings rate)
            zeroCrossingsTupleList = new List<Tuple<int, int, double>>();
            int intervalIndex = 0;
            int endIndex = 0;
            while (endIndex < (sampleList.Count-1))
            {
                int startIndex = intervalIndex * zeroCrossingsIndexInterval;
                endIndex = (intervalIndex + 1) * zeroCrossingsIndexInterval - 1;
                if (endIndex >= sampleList.Count) { endIndex = sampleList.Count - 1; }
                int numberOfZeroCrossingsInInterval = GetNumberOfZeroCrossingsInInterval(sampleList, startIndex, endIndex);
                double zeroCrossingsRateInInterval = (1+numberOfZeroCrossingsInInterval) / (double)(endIndex - startIndex);
                double modificationFactor = Math.Log10((zeroCrossingsRateInInterval) / typicalZeroCrossingsRate);
                Tuple<int, int, double> zeroCrossingsTuple = new Tuple<int, int, double>(endIndex, numberOfZeroCrossingsInInterval, modificationFactor);
                zeroCrossingsTupleList.Add(zeroCrossingsTuple);
                intervalIndex++;
                average += (numberOfZeroCrossingsInInterval/(double)(endIndex-startIndex));
            }
            average /= (double)intervalIndex;
        }

        // To be removed (replaced by class containing marks)
        private List<ZeroToZeroPairDeviation> zeroToZeroPairDeviationList;

        private List<double> Resample(double scaleFactor, double alpha, List<double> sampleList)
        {
            int zeroCrossingsTupleIndex = 0;
            double baseScaleFactor = scaleFactor; // The scale factor for alpha = 0.
            List<double> resampledSoundSamples = new List<double>();
            double originalNumberOfSamples = sampleList.Count;
            List<double> resampledSoundSamplesAsListDouble = new List<double>();
            double resampleIndex = 0;
            while (resampleIndex < (originalNumberOfSamples - 1))
            {
                int previousIndex = (int)Math.Truncate(resampleIndex);
                int nextIndex = previousIndex + 1;
                double previousSample = sampleList[previousIndex];
                double nextSample = sampleList[nextIndex];
                double relativeDelta = resampleIndex - previousIndex;
                double newSample = previousSample * (1 - relativeDelta) + nextSample * relativeDelta;
                resampledSoundSamples.Add(newSample);

                if (Math.Abs(alpha) > double.Epsilon)
                {
                    double currentModificationFactor = zeroCrossingsTupleList[zeroCrossingsTupleIndex].Item3;
                    scaleFactor = baseScaleFactor * (1 + alpha * currentModificationFactor);
                }

                resampleIndex += scaleFactor;

                if (Math.Abs(alpha) > double.Epsilon)
                {
                    if (resampleIndex > zeroCrossingsTupleList[zeroCrossingsTupleIndex].Item1)
                    {
                        if (zeroCrossingsTupleIndex < (zeroCrossingsTupleList.Count - 1))
                        {
                            zeroCrossingsTupleIndex += 1;
                        }
                    }
                }
            }
            return resampledSoundSamples;
        }

        //
        // The modification consists of three steps:
        //
        // (1) resampling to obtain a given relative brightness level.
        //
        // (2) kneading: sequences of upsampling and downsampling pairs, where the
        //     sampling interval varies dynamically with the zero-crossing rate (if alpha != 0).
        //
        //     In this case, the resampling scale factor (s)  at sample i varies as
        //     
        //     s(i) = r * (1 + alpha * Log((1+z(i))/z0)), 
        //
        //     where r is the scale factor in the absence of zero-crossings-based variation,
        //     z(i) is the zero crossings rate at sample i, and z0 is the typical average
        //     z0 crossing rate (around 0.10, i.e. 1 zero crossing every 10 samples).
        //     The "1 + ..." in the denominator is included to avoid problems where z(i) = 0.
        //
        //     z(i) is measured over intervals of 0.02 s (441 samples for the standard sampling rate).
        //     That is, the value of z(i) is piecewise constant over intervals of 0.2 s.
        //
        // (3) duration change
        public WAVSound Modify(WAVSound sound)
        {
            if ((Math.Abs(relativeDuration-1) < double.Epsilon) && (Math.Abs(relativeBrightness-1) < double.Epsilon))
            {
                return sound.Copy();
            }
            else
            {
                double alpha = 0;  // Always zero for the first resampling. Non-zero alpha used during kneading; see below.
                stopWatch = new Stopwatch();
                stopWatch.Start();
                double originalVolume = sound.GetAverageAbsoluteVolume();
                // Resample with interpolation:
                zeroCrossingsIndexInterval = (int)Math.Round(sound.SampleRate * zeroCrossingsTimeInterval);
                List<double> samplesAsListDouble = sound.GetSamplesAsListDouble(0);
                List<double> resampledSoundSamplesAsListDouble = Resample(relativeBrightness, alpha, samplesAsListDouble);
                double originalNumberOfSamples = samplesAsListDouble.Count;

                // Kneading:         
                for (int ii = 0; ii < kneadingFactors.Count; ii++)
                {
                    double kneadingFactor = kneadingFactors[ii];
                    alpha = alphaList[ii];
                    ComputeZeroCrossingsVariation(resampledSoundSamplesAsListDouble);
                    List<double> firstStageSampleList = Resample(kneadingFactor, alpha, resampledSoundSamplesAsListDouble);
                    resampledSoundSamplesAsListDouble = Resample(1 / kneadingFactor, alpha, firstStageSampleList);
                }

                interpolatedSound = new WAVSound(MODIFIED_SOUND_NAME, sound.SampleRate, sound.NumberOfChannels,
                                                 sound.BitsPerSample);
                interpolatedSound.GenerateFromListDouble(resampledSoundSamplesAsListDouble);

                // Duration change:
                double modifiedNumberOfSamples = resampledSoundSamplesAsListDouble.Count;
                double nonAdjustedRelativeDuration = modifiedNumberOfSamples / (double)originalNumberOfSamples;
                double requiredRelativeDuration = relativeDuration / nonAdjustedRelativeDuration;
                DurationModifier durationModifier = new DurationModifier();
                finalModifiedSound = durationModifier.Modify(interpolatedSound, requiredRelativeDuration);
                zeroToZeroPairDeviationList = durationModifier.ZeroToZeroPairDeviationList;


                finalModifiedSound.SetAverageAbsoluteVolume(originalVolume);

                stopWatch.Stop();
                double elapsedTime = stopWatch.ElapsedTicks / (double)Stopwatch.Frequency;

                return finalModifiedSound;
            }
        }

        public void Randomize(double minimumRelativeBrightness, double maximumRelativeBrightness, double minimumRelativeDuration, double maximumRelativeDuration,
                              double maximumAbsoluteAlpha, double maximumKneadingFactor, Random randomNumberGenerator)
        {
            relativeBrightness = minimumRelativeBrightness + (maximumRelativeBrightness - minimumRelativeBrightness) * randomNumberGenerator.NextDouble();
            relativeDuration = minimumRelativeDuration + (maximumRelativeDuration - minimumRelativeDuration) * randomNumberGenerator.NextDouble();
            double alphaPlus = randomNumberGenerator.NextDouble() * maximumAbsoluteAlpha;
            alphaList = new List<double>() { alphaPlus, -alphaPlus };
            double kneadingFactor =  1 + randomNumberGenerator.NextDouble() * (maximumKneadingFactor-1);
            kneadingFactors = new List<double>() { kneadingFactor, 1.0 / kneadingFactor };
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public double RelativeDuration
        {
            get { return relativeDuration; }
            set { relativeDuration = value; }
        }

        [DataMember]
        public double RelativeBrightness
        {
            get { return relativeBrightness; }
            set { relativeBrightness = value; }
        }

        [DataMember]
        public List<double> KneadingFactors
        {
            get { return kneadingFactors; }
            set { kneadingFactors = value; }
        }

        [DataMember]
        public List<double> AlphaList
        {
            get { return alphaList; }
            set { alphaList = value; }
        }

        [DataMember]
        public double ZeroCrossingsTimeInterval
        {
            get { return zeroCrossingsTimeInterval; }
            set { zeroCrossingsTimeInterval = value; }
        }

        [DataMember]
        public double TypicalZeroCrossingsRate
        {
            get { return typicalZeroCrossingsRate; }
            set { typicalZeroCrossingsRate = value; }
        }

        public List<ZeroToZeroPairDeviation> ZeroToZeroPairDeviationList
        {
            get { return zeroToZeroPairDeviationList; }
        }

        public WAVSound InterpolatedSound
        {
            get { return interpolatedSound; }
        }

        public WAVSound FinalModifiedSound
        {
            get { return finalModifiedSound; }
        }

        public WAVSound AdjustedComponentSound
        {
            get { return adjustedComponentSound; }
        }
    }
}
