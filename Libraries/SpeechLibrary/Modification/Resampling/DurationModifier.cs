using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioLibrary;

namespace SpeechLibrary.Modification.Resampling
{
    public class DurationModifier
    {
        private const double DEFAULT_ZERO_CROSSING_SEARCH_TIME_INTERVAL = 0.01;
        private const double DEFAULT_SECTION_DURATION = 0.01;
        private const double DEFAULT_MINIMUM_ENERGY_RATIO = 0; //  0.15; // For removal consideration.

        private double zeroCrossingSearchTimeInterval = DEFAULT_ZERO_CROSSING_SEARCH_TIME_INTERVAL;
        private double sectionDuration = DEFAULT_SECTION_DURATION;
        private double minimumEnergyRatio = DEFAULT_MINIMUM_ENERGY_RATIO;
        private List<ZeroToZeroPairDeviation> zeroToZeroPairDeviationList = null;
        private List<int> spacedZeroCrossingIndexList;
        private List<double> spacedZeroCrossingTimeList;

        private Stopwatch stopWatch = null;

        // Here, relative speed should be < 1.
        private WAVSound Shorten(WAVSound sound, double relativeDuration)
        {
            // The changes must be distributed over the entire sound, to avoid removing just
            // parts of the sound. Thus, sections adjacent to a given section are removed
            // from consideration in removal.
            double duration = sound.GetDuration();
            double requiredDuration = relativeDuration * duration;
            double requiredRemovalDuration = duration - requiredDuration;
            double durationRemoved = 0;
            List<List<double>> removalRangeList = new List<List<double>>(); ;
            while (durationRemoved < requiredRemovalDuration)
            {
                ZeroToZeroPairDeviation removalPair = zeroToZeroPairDeviationList[0];
                double removalStartTime = removalPair.StartTime1;
                double removalEndTime = removalPair.EndTime1;
                removalRangeList.Add(new List<double>() { removalStartTime, removalEndTime });
                zeroToZeroPairDeviationList.RemoveAt(0);
                List<ZeroToZeroPairDeviation> excludedPairList = zeroToZeroPairDeviationList.FindAll(
                    t => ((Math.Abs(t.EndTime1 - removalStartTime) < sectionDuration) ||
                           Math.Abs(t.StartTime1 - removalEndTime) < sectionDuration));
                foreach (ZeroToZeroPairDeviation excludedPair in excludedPairList)
                {
                    zeroToZeroPairDeviationList.Remove(excludedPair);
                }
                durationRemoved += (removalEndTime - removalStartTime);
                if (zeroToZeroPairDeviationList.Count == 0) { break; }
            }

            removalRangeList.Sort((a, b) => a[0].CompareTo(b[0]));

            List<Int16> shortenedSoundSamplesList = new List<Int16>();
            int removalStartIndex = int.MaxValue;
            int removalEndIndex = int.MaxValue;
            if (removalRangeList.Count > 0)
            {
                removalStartIndex = sound.GetSampleIndexAtTime(removalRangeList[0][0]);
                removalEndIndex = sound.GetSampleIndexAtTime(removalRangeList[0][1]);
            }
            int ii = 0;
            while (ii  < sound.Samples[0].Count)
            {
                if (ii >= removalStartIndex)
                {
                    ii = removalEndIndex + 1;
                    removalRangeList.RemoveAt(0);
                    removalStartIndex = int.MaxValue;
                    removalEndIndex = int.MaxValue;
                    if (removalRangeList.Count > 0)
                    {
                        removalStartIndex = sound.GetSampleIndexAtTime(removalRangeList[0][0]);
                        removalEndIndex = sound.GetSampleIndexAtTime(removalRangeList[0][1]);
                    }
                }
                shortenedSoundSamplesList.Add(sound.Samples[0][ii]);
                ii++;
            }

            WAVSound shortenedSound = new WAVSound("", sound.SampleRate, sound.NumberOfChannels, sound.BitsPerSample);
            shortenedSound.GenerateFromSamples(new List<List<Int16>>() { shortenedSoundSamplesList });
            return shortenedSound;
        }

        private WAVSound Lengthen(WAVSound sound, double relativeDuration)
        {
            // The changes must be distributed over the entire sound, to avoid removing just
            // parts of the sound. Thus, sections adjacent to a given section are removed
            // from consideration in removal.
            double duration = sound.GetDuration();
            double requiredDuration = relativeDuration * duration;
            double requiredAdditionDuration = requiredDuration - duration;
            double durationAdded = 0;
            List<List<double>> additionRangeList = new List<List<double>>(); ;
            while (durationAdded < requiredAdditionDuration)
            {
                ZeroToZeroPairDeviation additionPair = zeroToZeroPairDeviationList[0];
                double additionStartTime = additionPair.StartTime1;
                double additionEndTime = additionPair.EndTime1;
                additionRangeList.Add(new List<double>() { additionStartTime, additionEndTime });
                zeroToZeroPairDeviationList.RemoveAt(0);
                List<ZeroToZeroPairDeviation> excludedPairList = zeroToZeroPairDeviationList.FindAll(
                    t => ((Math.Abs(t.EndTime1 - additionStartTime) < sectionDuration) ||
                           Math.Abs(t.StartTime1 - additionEndTime) < sectionDuration));
                foreach (ZeroToZeroPairDeviation excludedPair in excludedPairList)
                {
                    zeroToZeroPairDeviationList.Remove(excludedPair);
                }
                durationAdded += (additionEndTime - additionStartTime);
                if (zeroToZeroPairDeviationList.Count == 0) { break; }
            }

            additionRangeList.Sort((a, b) => a[0].CompareTo(b[0]));

            List<Int16> lengthenedSoundSamplesList = new List<Int16>();
            int additionStartIndex = int.MaxValue;
            int additionEndIndex = int.MaxValue;
            if (additionRangeList.Count > 0)
            {
                additionStartIndex = sound.GetSampleIndexAtTime(additionRangeList[0][0]);
                additionEndIndex = sound.GetSampleIndexAtTime(additionRangeList[0][1]);
            }
            int ii = 0;
            while (ii < sound.Samples[0].Count)
            {
                if (ii >= additionStartIndex)
                {
                    for (int jj = additionStartIndex; jj <= additionEndIndex; jj++)
                    {
                        lengthenedSoundSamplesList.Add(sound.Samples[0][jj]);
                    }
                    additionRangeList.RemoveAt(0);
                    additionStartIndex = int.MaxValue;
                    additionEndIndex = int.MaxValue;
                    if (additionRangeList.Count > 0)
                    {
                        additionStartIndex = sound.GetSampleIndexAtTime(additionRangeList[0][0]);
                        additionEndIndex = sound.GetSampleIndexAtTime(additionRangeList[0][1]);
                    }
                }
                lengthenedSoundSamplesList.Add(sound.Samples[0][ii]);
                ii++;
            }

            WAVSound lengthenedSound = new WAVSound("", sound.SampleRate, sound.NumberOfChannels, sound.BitsPerSample);
            lengthenedSound.GenerateFromSamples(new List<List<Int16>>() { lengthenedSoundSamplesList });
            return lengthenedSound;
        }

        public WAVSound Modify(WAVSound sound, double relativeDuration)
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
       //     speechTypeEstimator = new SpeechTypeEstimator();
        //    speechTypeEstimator.FindSpeechTypeVariation(sound);

        //    stopWatch.Stop();
        //    double elapsedTime = stopWatch.ElapsedTicks / (double)Stopwatch.Frequency;

            WAVSound durationModifiedSound = null;
            // First find suitable zero crossings
            spacedZeroCrossingIndexList = new List<int>();
            spacedZeroCrossingTimeList = new List<double>();
            int index = 1;
            while (index < sound.Samples[0].Count - 1)
            {
                if (sound.Samples[0][index + 1] * sound.Samples[0][index - 1] < 0)
                {
                    double time = sound.GetTimeAtSampleIndex(index);
                    if (spacedZeroCrossingIndexList.Count > 0)
                    {
                        if ((time - spacedZeroCrossingTimeList.Last()) > zeroCrossingSearchTimeInterval)
                        {
                            spacedZeroCrossingIndexList.Add(index);
                            spacedZeroCrossingTimeList.Add(time);
                        }
                    }
                    else
                    {
                        spacedZeroCrossingIndexList.Add(index);
                        spacedZeroCrossingTimeList.Add(time);
                    }
                }
                index++;
            }

            // Next find correlations between the current zero-to-zero and the following samples
            Double averageEnergy = sound.GetAverageEnergy(0);
            zeroToZeroPairDeviationList = new List<ZeroToZeroPairDeviation>();
            for (int ii = 1; ii < spacedZeroCrossingIndexList.Count - 1; ii++)
            {
                int indexInterval = spacedZeroCrossingIndexList[ii + 1] - spacedZeroCrossingIndexList[ii];
                int startIndex1 = spacedZeroCrossingIndexList[ii];
                int startindex2 = spacedZeroCrossingIndexList[ii + 1];
                double energyInRange = sound.GetLocalEnergy(startIndex1, startindex2 - 1) / (double)(startindex2 - startIndex1);
           //     if ((energyInRange / averageEnergy) > minimumEnergyRatio)
           //     {
                    Boolean allSamplesAvailable = true;
                    double totalDeviation = 0;
                    for (int jj = 0; jj <= indexInterval; jj++)
                    {
                        if (startindex2 + jj >= sound.Samples[0].Count)
                        {
                            allSamplesAvailable = false;
                            break;
                        }
                        //   double deviation = Math.Abs(sound.Samples[0][startIndex1 + jj] - sound.Samples[0][startindex2 + jj]);
                        double deviation = (sound.Samples[0][startIndex1 + jj] - sound.Samples[0][startindex2 + jj]) *
                            (sound.Samples[0][startIndex1 + jj] - sound.Samples[0][startindex2 + jj]);
                        totalDeviation += deviation;
                    }
                    totalDeviation /= (startindex2 - startIndex1);
                    if (allSamplesAvailable)
                    {
                        ZeroToZeroPairDeviation pairDeviation = new ZeroToZeroPairDeviation();
                        pairDeviation.StartIndex1 = startIndex1;
                        pairDeviation.EndIndex1 = startindex2 - 1;
                        pairDeviation.StartTime1 = sound.GetTimeAtSampleIndex(startIndex1);
                        pairDeviation.EndTime1 = sound.GetTimeAtSampleIndex(startindex2 - 1);
                        pairDeviation.Deviation = totalDeviation;
                        pairDeviation.RelativeDeviation = totalDeviation / energyInRange;
                        zeroToZeroPairDeviationList.Add(pairDeviation);
                    }
          //      }
            }

            zeroToZeroPairDeviationList.Sort((a,b) => a.RelativeDeviation.CompareTo(b.RelativeDeviation));


        //    zeroToZeroPairDeviationList = zeroToZeroPairDeviationList.GetRange(0, 30);
                    // Next define sections
            if (relativeDuration < 1)
            {
                durationModifiedSound = Shorten(sound, relativeDuration);
            }
            else if (relativeDuration > 1)
            {
                durationModifiedSound = Lengthen(sound, relativeDuration);
            }
            else { durationModifiedSound = sound.Copy(); }

            stopWatch.Stop();
            double elapsedTime = stopWatch.ElapsedTicks / (double)Stopwatch.Frequency;
            return durationModifiedSound; // Change here ...
        }

        public List<int> SpacedZeroCrossingIndexList
        {
            get { return spacedZeroCrossingIndexList; }
        }

        public List<double> SpacedZeroCrossingTimeList
        {
            get { return spacedZeroCrossingTimeList; }
        }

        public List<ZeroToZeroPairDeviation> ZeroToZeroPairDeviationList
        {
            get { return zeroToZeroPairDeviationList; }
        }
    }
}
