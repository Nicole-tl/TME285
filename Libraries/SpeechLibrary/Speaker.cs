using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioLibrary;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Synthesis;

namespace SpeechLibrary
{
    public class Speaker
    { 
        public static WAVSound Speak(SpeechSynthesizer speechSynthesizer, string utterance)
        {
            WAVSound sound = new WAVSound(utterance, 22050, 1, 16);
            using (MemoryStream audioStream = new MemoryStream())
            {
                speechSynthesizer.SetOutputToWaveStream(audioStream);
                speechSynthesizer.Speak(utterance);
                sound.WAVMemoryStream = audioStream;
                sound.GenerateFromMemoryStream();
            }
            return sound;
        }
    }
}
