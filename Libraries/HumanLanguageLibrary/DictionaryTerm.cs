using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary
{
    [DataContract]
    public class DictionaryTerm
    {
        private int id;
        private string word;
        private WordClass wordClass;
        private List<int> synonymIDList;
        private List<int> similarTermIDList;
        private List<int> antonymIDList;
        private List<string> synonymList;
        private List<string> similarTermList;

        public DictionaryTerm()
        {
            synonymIDList = new List<int>();
            similarTermIDList = new List<int>();
            antonymIDList = new List<int>();
            synonymList = new List<string>();
            similarTermList = new List<string>();
        }

        public DictionaryTerm(string word, WordClass wordClass)
        {
            synonymIDList = new List<int>();
            similarTermIDList = new List<int>();
            antonymIDList = new List<int>();
            synonymList = new List<string>();
            similarTermList = new List<string>();
            this.word = word;
            this.wordClass = wordClass;
        }

        public List<string> SynonymList
        {
            get { return synonymList; }
            set { synonymList = value; }
        }

        public List<string> SimilarTermList
        {
            get { return similarTermList; }
            set { similarTermList = value; }
        }

        [DataMember]
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember]
        public string Word
        {
            get { return word; }
             set { word = value; }
        }

        [DataMember]
        public WordClass WordClass
        {
            get { return wordClass; }
            set { wordClass = value; }
        }

        [DataMember]
        public List<int> SynonymIDList
        {
            get { return synonymIDList; }
            set { synonymIDList = value; }
        }

        [DataMember]
        public List<int> SimilarTermIDList
        {
            get { return similarTermIDList; }
            set { similarTermIDList = value; }
        }

        [DataMember]
        public List<int> AntonymIDList
        {
            get { return antonymIDList; }
            set { antonymIDList = value; }
        }
    }
}
