using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using HumanLanguageLibrary.TextProcessing;

namespace HumanLanguageLibrary.Phrases
{
    [DataContract]
    public class PPDBDictionaryItem
    {
        private List<string> synonymousPhraseList;

        public PPDBDictionaryItem()
        {
            synonymousPhraseList = new List<string>();
        }

        public int GetFirstPhraseWordCount(char[] interpunctionList)
        {
            List<string> phrase1AsWords = synonymousPhraseList[0].Split(interpunctionList, StringSplitOptions.RemoveEmptyEntries).ToList();
            return phrase1AsWords.Count;
        }

        [DataMember]
        public List<string> SynonymousPhraseList
        {
            get { return synonymousPhraseList; }
            set { synonymousPhraseList = value; }
        }
    }
}
