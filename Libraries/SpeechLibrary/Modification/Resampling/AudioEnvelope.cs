using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioLibrary;

namespace SpeechLibrary.Modification.Resampling
{
    public class AudioEnvelope
    {
        private double timeStep = 0.01;
        private List<double> timeList;
        private List<double> upperEnvelopeList;
        private List<double> lowerEnvelopeList;

        public double GetMaximum()
        {
            double maximum = 0;
            for (int ii = 0; ii<upperEnvelopeList.Count; ii++)
            {
                if (upperEnvelopeList[ii] > maximum)
                {
                    maximum = upperEnvelopeList[ii];
                }
            }
            return maximum;
        }

        public void Compute(WAVSound sound)
        {
            int indexStep = (int)Math.Round(timeStep*sound.SampleRate);
            timeList = new List<double>() { 0 };
            upperEnvelopeList = new List<double>() { 0 };
            lowerEnvelopeList = new List<double>() { 0 };
            int nextStartIndex = indexStep;
            int currentStartIndex = 0;
            while (nextStartIndex < sound.Samples[0].Count)
            {
                float top = 0;
                float bottom = 0;
                for (int ii = currentStartIndex; ii < nextStartIndex; ii++)
                {
                    if (sound.Samples[0][ii] > top) { top = sound.Samples[0][ii]; }
                    else if (sound.Samples[0][ii] < bottom) { bottom = sound.Samples[0][ii]; }
                }
                double time = sound.GetTimeAtSampleIndex(nextStartIndex - 1);
                timeList.Add(time);
                upperEnvelopeList.Add(top);
                lowerEnvelopeList.Add(bottom);
                nextStartIndex += indexStep;
                currentStartIndex += indexStep;
            }
        }

        public List<double> TimeList
        {
            get { return timeList; }
            set { timeList = value; }
        }

        public List<double> UpperEnvelopeList
        {
            get { return upperEnvelopeList; }
            set { upperEnvelopeList = value; }
        }

        public List<double> LowerEnvelopeList
        {
            get { return lowerEnvelopeList; }
            set { lowerEnvelopeList = value; }
        }

        public double TimeStep
        {
            get { return timeStep; }
            set { timeStep = value; }
        }
    }
}
