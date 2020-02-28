using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioLibrary.SoundFeatures;

namespace SpeechLibrary.Recognition
{
    // Instances of this class contain the results obtained by calling IsolatedWordRecognizer.Recognize().
    public class IWRRecognitionResult
    {
        #region Fields
        private List<Tuple<string, double>> deviationList;
        private SoundFeatureSet soundFeatureSet; // The sound features computed for the sound in question.
        #endregion

        #region Constructor
        public IWRRecognitionResult()
        {
            deviationList = new List<Tuple<string, double>>();
        }
        #endregion

        #region Properties
        public List<Tuple<string, double>> DeviationList
        {
            get { return deviationList; }
            set { deviationList = value; }
        }

        public SoundFeatureSet SoundFeatureSet
        {
            get { return soundFeatureSet; }
            set { soundFeatureSet = value; }
        }
        #endregion
    }
}
