using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.TextProcessing
{
    public class RawTextData
    {
        private string url;
        private List<string> speakerList;
        private List<string> textDataList;

        private const string SPEAKER_STRING = "SPEAKER:";

        public RawTextData()
        {
            url = null;
            speakerList = new List<string>();
            textDataList = new List<string>();
        }

        public void Generate(List<string> rawDataStringList)
        {
            for (int ii = 0; ii < rawDataStringList.Count; ii++)
            {
                string rawDataString = rawDataStringList[ii];
                if (rawDataString.StartsWith(SPEAKER_STRING))
                {
                    rawDataString = rawDataString.Replace(SPEAKER_STRING, "").Trim();
                    if (rawDataString != "") { speakerList.Add(rawDataString); }
                }
                else if (rawDataString.StartsWith("URL:"))
                {
                    url = rawDataString.Replace("URL:", "").Trim();
                }
                else
                {
                    textDataList.Add(rawDataString);
                }
            }
        }

        public List<string> FindSpeakers(Boolean speakerOnSeparateLine)
        {
            List<string> tentativeSpeakerList = new List<string>();
            for (int ii = 0; ii < textDataList.Count; ii++)
            {
                string rawDataString = textDataList[ii];
                if (rawDataString != "")
                {
                    string tentativeSpeaker = "";
                    if (speakerOnSeparateLine)
                    {
                        if (!rawDataString.Any(Char.IsLower))
                        {
                            tentativeSpeaker = rawDataString;
                        }
                    }
                    else
                    {
                        tentativeSpeaker = "";
                        List<string> rawDataStringSplit =
                            rawDataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        int index = 0;
                        while (index < rawDataStringSplit.Count)
                        {
                            string tentativeSpeakerFragment = rawDataStringSplit[index];
                            if (tentativeSpeaker.Any(Char.IsLower))
                            {
                                break;
                            }
                            tentativeSpeaker += tentativeSpeakerFragment + " ";
                            index++;
                        }
                        tentativeSpeaker = tentativeSpeaker.Trim();
                    }
                    if (tentativeSpeaker != "")
                    {
                        if (tentativeSpeaker.Any(x => char.IsLetter(x)))  // Require at least one letter in a name...
                        {
                            if (!tentativeSpeaker.StartsWith("URL:"))
                            {
                                if (!tentativeSpeaker.EndsWith("!"))  // Avoid including exclamations as names ..
                                {
                                    if (!tentativeSpeaker.EndsWith("?"))  // ..as well as emphasized questions.
                                    {
                                        if (tentativeSpeaker.EndsWith(":"))
                                        {
                                            tentativeSpeaker = tentativeSpeaker.TrimEnd(new char[] { ':' });
                                        }
                                        if (!tentativeSpeakerList.Contains(tentativeSpeaker))
                                        {
                                            tentativeSpeakerList.Add(tentativeSpeaker);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return tentativeSpeakerList;
        }

        public void MergeSpeakerLists(List<string> tentativeSpeakerList)
        {
            if (speakerList == null) { speakerList = new List<string>(); }
            foreach (string tentativeSpeaker in tentativeSpeakerList)
            {
                if (!speakerList.Contains(tentativeSpeaker))
                {
                    speakerList.Add(tentativeSpeaker);
                }
            }
        }

        public List<string> InRawFormat()
        {
            List<string> dataInRawFormat = new List<string>();
            if (url != null)
            {
                dataInRawFormat.Add("URL: " + url);
            }
            if (speakerList != null)
            {
                foreach (string speaker in speakerList)
                {
                    dataInRawFormat.Add(SPEAKER_STRING + " " + speaker);
                }
                dataInRawFormat.Add("");
            }
            foreach (string rawDataString in textDataList)
            {
                dataInRawFormat.Add(rawDataString);
            }
            return dataInRawFormat;
        }

        public string URL
        {
            get { return url; }
            set { url = value; }
        }

        public List<string> SpeakerList
        {
            get { return speakerList; }
            set { speakerList = value; }
        }

        public List<string> TextDataList
        {
            get { return textDataList; }
            set { textDataList = value; }
        }
    }
}
