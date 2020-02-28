using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.TextProcessing
{
    [DataContract]
    public class FrequencySet
    {
        private List<TermFrequency> termFrequencyList;
        private List<string> wordList;  // single list, for quick searching
        private List<double> frequencyList; // Defined as instances per million words

        public void GenerateFromRawData(List<string> rawDataStringList)
        {
            termFrequencyList = new List<TermFrequency>();
            foreach (string rawDataString in rawDataStringList)
            {
                List<string> rawDataSplit = rawDataString.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (rawDataSplit.Count == 2)
                {
                    string term = rawDataSplit[0].Replace("*", "").Trim().ToLower();  // Remove "*" that is added to some words...
                    double frequency = double.Parse(rawDataSplit[1]);
                    TermFrequency termFrequency = new TermFrequency(term, frequency);
                    termFrequencyList.Add(termFrequency);
                }
            }
            termFrequencyList.Sort((a, b) => a.Term.CompareTo(b.Term));
            // Next, merge different instances with the same SPELLING, e.g. "present" as noun + "present" as verb
            int index = 0;
            string currentTerm = termFrequencyList[index].Term;
            index++;
            while (index < termFrequencyList.Count)
            {
                string nextTerm = termFrequencyList[index].Term;
                if (nextTerm == currentTerm)
                {
                    termFrequencyList[index - 1].FrequencyPerMillionWords += termFrequencyList[index].FrequencyPerMillionWords;
                    termFrequencyList.RemoveAt(index);
                }
                else
                {
                    currentTerm = nextTerm;
                    index++;
                }
            }
            wordList = new List<string>();
            foreach (TermFrequency termFrequency in termFrequencyList)
            {
                wordList.Add(termFrequency.Term); // Already sorted...
            }
        }

        public double GetFrequencyPerMillionWords(string word)
        {
            int indexInWordList = wordList.BinarySearch(word);
            if (indexInWordList >= 0)
            {
                double frequencyPerMillionWords = termFrequencyList[indexInWordList].FrequencyPerMillionWords;
                return frequencyPerMillionWords;
            }
            else { return 0; }
        }

        public void TrimUsingDictionary(Dictionary dictionary)
        {
            int index = 0;
            while (index < termFrequencyList.Count)
            {
                string term = termFrequencyList[index].Term;
                if (dictionary.ContainsWord(term)) { index++; }
                else
                {
                    termFrequencyList.RemoveAt(index);
                    wordList.RemoveAt(index);
                }
            }
        }

        public void SortAlphabetically()
        {
            termFrequencyList.Sort((a, b) => a.Term.CompareTo(b.Term));
            wordList = new List<string>();
            foreach (TermFrequency termFrequency in termFrequencyList)
            {
                wordList.Add(termFrequency.Term); // Already sorted...
            }
        }

        public void SortByFrequency()
        {
            termFrequencyList.Sort((a, b) => b.FrequencyPerMillionWords.CompareTo(a.FrequencyPerMillionWords));
            wordList = new List<string>();
            foreach (TermFrequency termFrequency in termFrequencyList)
            {
                wordList.Add(termFrequency.Term); // Already sorted...
            }
        }

        [DataMember]
        public List<TermFrequency> TermFrequencyList
        {
            get { return termFrequencyList; }
            set { termFrequencyList = value; }
        }

        public List<string> WordList
        {
            get { return wordList; }
            set { wordList = value; }
        }
    }
}
