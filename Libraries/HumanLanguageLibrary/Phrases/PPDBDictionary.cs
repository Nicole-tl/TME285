using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.Phrases
{
    [DataContract]
    public class PPDBDictionary
    {
        private List<PPDBDictionaryItem> itemList;

        public PPDBDictionary()
        {
            itemList = new List<PPDBDictionaryItem>();
        }

        public List<PPDBDictionaryItem> FindTerms(string searchString)
        {
            searchString = searchString.ToLower();
            List<PPDBDictionaryItem> matchingTermList = new List<PPDBDictionaryItem>();
            foreach (PPDBDictionaryItem term in itemList)
            {
                foreach (string synonymousPhrase in term.SynonymousPhraseList)
                {
                    if (synonymousPhrase.ToLower().Contains(searchString))
                    {
                        matchingTermList.Add(term);
                        break;
                    }
                }
            }
            return matchingTermList;
        }

        // Saves the dictionary in binary format
        public void Save(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            foreach (PPDBDictionaryItem item in itemList)
            {
                int phraseCount = item.SynonymousPhraseList.Count;
                binaryWriter.Write(phraseCount);
                foreach (string synonymousPhrase in item.SynonymousPhraseList)
                {
                    binaryWriter.Write(synonymousPhrase);
                }
            }
            binaryWriter.Close();
            stream.Close();
        }

        public void Load(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(stream);
            while (binaryReader.PeekChar() != -1)
            {
                PPDBDictionaryItem item = new PPDBDictionaryItem();
                int phraseCount = binaryReader.ReadInt32();
                for (int ii = 0; ii < phraseCount; ii++)
                {
                    string synonymousPhrase = binaryReader.ReadString();
                    item.SynonymousPhraseList.Add(synonymousPhrase);
                }
                itemList.Add(item);
            }
            binaryReader.Close();
            stream.Close();
        }

        [DataMember]
        public List<PPDBDictionaryItem> ItemList
        {
            get { return itemList; }
            set { itemList = value; }
        }
    }
}
