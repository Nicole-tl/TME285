using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AgentApplication
{
    [DataContract]
    public class AgentConfiguration
    {
       // [DataMember]
       // public string AgentFileName { get; set; }
        [DataMember]
        public string BaseVoiceName { get; set; }
        [DataMember]
        public string VoiceModifierFileName { get; set; }
        [DataMember]
        public string AgentVisualizerFileName { get; set; }
        [DataMember]
        public string RecordingDeviceName { get; set; }
        [DataMember]
        public Boolean RecordOnDemandOnly { get; set; }
        [DataMember]
        public string SpeechRecognitionGrammarFileName { get; set; }
        [DataMember]
        public Boolean UseSpeechOutput { get; set; }
        [DataMember]
        public Boolean UseVision { get; set; }
        [DataMember]
        public string VisionRelativeFilePath { get; set; }
        [DataMember]
        public string IpAddress { get; set; }
        [DataMember]
        public int Port { get; set; }
        [DataMember]
        public double CameraDistance { get; set; }
        [DataMember]
        public double CameraLatitude { get; set; }
        [DataMember]
        public double CameraLongitude { get; set; }

        public AgentConfiguration()
        {
       //     AgentFileName = null; // Per default, the agent configuration does not contain the file name for the agent.
            BaseVoiceName = "Microsoft Server Speech Text to Speech Voice (en-GB, Hazel)";
            RecordingDeviceName = "";
            RecordOnDemandOnly = false;
            VoiceModifierFileName = "MaleVoice1.xml"; //  VoiceModifier1.xml";
            AgentVisualizerFileName = "AgentFace1.xml";
            SpeechRecognitionGrammarFileName = "SpeechRecognitionGrammar.txt";
            VisionRelativeFilePath = ""; // Path relative to the directory where the agent application exe file resides.
            UseVision = false;
            UseSpeechOutput = true;
            IpAddress = "127.0.0.1";
            Port = 7;
            CameraDistance = 1;
            CameraLatitude = 0;
            CameraLongitude = 0;
        }
    }
}
