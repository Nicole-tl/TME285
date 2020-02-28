using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary
{
    [DataContract]
    public class Dictionary
    {
        private List<DictionaryTerm> termList;
        private List<string> wordList; // For fast searching

        public Dictionary()
        {
            termList = new List<DictionaryTerm>();
        }

        // Takes as input a file of the format
        //  <word> <meaning index> <wordclass> <term1> <term2> ...
        //  where <term1> etc. can be either synonyms, 
        //  generic terms etc. 
        // 
        // Terms OTHER than synonyms (e.g. "generic term") are
        // always indicated with parenthesis. Thus, process
        // the term list until a term within parenthesis appears.
        //
        //  Specific data file: Thesaurus.txt.
        public void GenerateFromRawData(List<string> rawDataList)
        {
            for (int ii = 0; ii < rawDataList.Count; ii++)
            {
                string rawData = rawDataList[ii];
                if (rawData != "")
                {
                    List<string> dataSplit = rawData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    string word = dataSplit[0];
                    if (dataSplit.Count > 3)
                    {
                        string wordClassString = dataSplit[2].Replace("(", "").Replace(")", "");
                        if (wordClassString == "adj") { wordClassString = "Adjective"; }
                        else if (wordClassString == "adv") { wordClassString = "Adverb"; }
                        wordClassString = Char.ToUpper(wordClassString[0]) + wordClassString.Substring(1);
                        WordClass wordClass = (WordClass)Enum.Parse(typeof(WordClass), wordClassString);
                        DictionaryTerm term = new DictionaryTerm(word, wordClass);
                        int index = 3;
                        while (index < dataSplit.Count)
                        {
                            string dataTerm = dataSplit[index];
                            if (dataTerm.Contains("("))
                            {
                                List<string> dataTermSplit = dataTerm.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                if (dataTermSplit.Count == 2)
                                {
                                    if (dataTermSplit[1] == "similar term")
                                    {
                                        string similarTerm = dataTermSplit[0].Trim();
                                        if (similarTerm != "")
                                        {
                                            term.SimilarTermList.Add(similarTerm);
                                        }
                                    }
                                }
                            }
                          //  { break; }
                            else
                            {
                                term.SynonymList.Add(dataTerm);
                            }
                            index++;
                        }
                        termList.Add(term);
                    }
                    else
                    {
                        DictionaryTerm term = new DictionaryTerm(word, WordClass.Unknown);
                        termList.Add(term);
                    }
                }
            }
            ProcessTermList();
        }

        public void AppendRawData(List<string> rawDataList)
        {
            for (int ii = 0; ii < rawDataList.Count; ii++)
            {
                string rawData = rawDataList[ii];
                if (rawData != "")
                {
                    List<string> dataSplit = rawData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    string word = dataSplit[0];
                    if (dataSplit.Count == 1)
                    {
                        int indexInWordList = wordList.BinarySearch(word);
                        if (indexInWordList < 0)
                        {
                            DictionaryTerm term = new DictionaryTerm(word, WordClass.Unknown);
                            termList.Add(term);
                        }
                    }
                }
            }
            ProcessTermList();
        }

        public Boolean ContainsWord(string word)
        {
            int index = wordList.BinarySearch(word);
            if (index >= 0) { return true; }
            else { return false; }
        }
        

        // Fast search for all DictionaryTerm instances matching a given word.
        public List<DictionaryTerm> FindTerms(string word)
        {
            List<DictionaryTerm> searchResultTermList = new List<DictionaryTerm>();
            int index = wordList.BinarySearch(word);
            if (index >= 0)
            {
                int startIndex = index;
                while (wordList[startIndex] == word)
                {
                    startIndex--;
                    if (startIndex < 0) { break; }
                }
                startIndex++;
                int endIndex = index;
                while (wordList[endIndex] == word)
                {
                    endIndex++;
                    if (endIndex >= wordList.Count) { break; }
                }
                endIndex--; // Needed since
                searchResultTermList = termList.GetRange(startIndex, endIndex - startIndex + 1);
            }
            return searchResultTermList;
        }

        private List<int> GetIDRange(string word)
        {
            List<int> idRange = new List<int>();
            int id = wordList.BinarySearch(word);
            int firstID = id;
            int lastID = id;
            if (id >= 0)
            {
                int backwardID = id - 1;
                while (backwardID >= 0)
                {
                    string backwardWord = wordList[backwardID];
                    if (backwardWord != word)
                    {
                        firstID = backwardID + 1;
                        break;
                    }
                    backwardID--;
                }
                if (backwardID < 0) { backwardID = 0; }
                int forwardID = id + 1;
                while (forwardID < wordList.Count)
                {
                    string forwardWord = wordList[forwardID];
                    if (forwardWord != word)
                    {
                        lastID = forwardID - 1;
                        break;
                    }
                    forwardID++;
                }
                if (forwardID >= wordList.Count) { forwardID = wordList.Count - 1; }
                idRange = new List<int>() { firstID, lastID };
            }
            return idRange;
        }

        // Given raw data, generate all IDs and assign numbers etc.
        public void ProcessTermList()
        {
            termList = termList.OrderBy(a => a.Word).ThenBy(b => b.WordClass).ThenByDescending(c => c.SynonymList.Count).ToList();
            RemoveDuplicates();
            for (int ii = 0; ii < termList.Count; ii++)
            {
                termList[ii].ID = ii;
            }
            wordList = new List<string>();
            foreach (DictionaryTerm term in termList)
            {
                wordList.Add(term.Word);
            }
            for (int ii = 0; ii < termList.Count; ii++)
            {
                termList[ii].SynonymIDList = new List<int>();
                int jj = 0;
                while (jj < termList[ii].SynonymList.Count)
                {
                    string synonym = termList[ii].SynonymList[jj];
                    List<int> idRange = GetIDRange(synonym);
                    if (idRange.Count > 0)
                  //  int id = wordList.BinarySearch(synonym);
                  //  if (id >= 0)
                    {
                        Boolean matchFound = false;
                        int matchingID = -1;
                        int id = idRange[0];
                        while (id <= idRange[1])
                        {
                            if (termList[id].WordClass == termList[ii].WordClass)
                            {
                                if (termList[id].SynonymList.Contains(termList[ii].Word))
                                {
                                    matchFound = true;
                                    matchingID = id;
                                    break;
                                }
                            }
                            id++;
                        }
                        if (matchFound)
                        {
                            termList[ii].SynonymIDList.Add(matchingID);
                            jj++;
                        }
                        else { termList[ii].SynonymList.RemoveAt(jj); }
                    }
                    else { termList[ii].SynonymList.RemoveAt(jj); }
                }

                termList[ii].SimilarTermIDList = new List<int>();
                jj = 0;
                if (termList[ii].Word=="competent")
                {

                }
                while (jj < termList[ii].SimilarTermList.Count)
                {
                    string similarTerm = termList[ii].SimilarTermList[jj];
                    List<int> idRange = GetIDRange(similarTerm);
                    if (idRange.Count > 0)
                    {
                        Boolean matchFound = false;
                        int matchingID = -1;
                        int id = idRange[0];
                        while (id <= idRange[1])
                        {
                            if (termList[id].WordClass == termList[ii].WordClass)
                            {
                              //  if (termList[id].SimilarTermList.Contains(termList[ii].Word))
                                {
                                    matchFound = true;
                                    matchingID = id;
                                    break;
                                }
                            }
                            id++;
                        }
                        if (matchFound)
                        {
                            termList[ii].SimilarTermIDList.Add(matchingID);
                            jj++;
                        }
                        else { termList[ii].SimilarTermList.RemoveAt(jj); }
                    }
                    else { termList[ii].SimilarTermList.RemoveAt(jj); }
                }
            }
        }

        private void RemoveDuplicates()
        {
            int index = 0;
            string currentWord = termList[index].Word;
            WordClass currentWordClass = termList[index].WordClass;
            List<List<string>> synonymListList = new List<List<string>>();
            if (termList[index].SynonymList.Count > 0)
            {
                synonymListList.Add(termList[index].SynonymList);
            }
            while (index < termList.Count)
            {
                index++;
                if (index >= termList.Count) { break; }
                string word = termList[index].Word;
                WordClass wordClass = termList[index].WordClass;
                if ((word == currentWord) && (wordClass == currentWordClass))
                {
                    if (termList[index].SynonymList.Count == 0)
                    {
                        termList.RemoveAt(index);
                        index--;
                    }
                    else
                    {
                        List<string> wordSynonymList = termList[index].SynonymList;
                        foreach (List<string> synonymList in synonymListList)
                        {
                            if (wordSynonymList.Count == synonymList.Count)
                            {
                                List<string> difference = wordSynonymList.Except(synonymList).ToList();
                                if (difference.Count == 0)  // equal synomym lists
                                {
                                    termList.RemoveAt(index);
                                    index--;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    currentWord = word;
                    currentWordClass = wordClass;
                    synonymListList = new List<List<string>>();
                    if (termList[index].SynonymList.Count > 0)
                    {
                        synonymListList.Add(termList[index].SynonymList);
                    }
                }
            }
        }

        private void ProcessSynonyms()
        {
            foreach (DictionaryTerm term in termList)
            {
               /* if (term.Word == "happy")
                {

                }  */
                term.SynonymList = new List<string>();
                foreach (int synonymID in term.SynonymIDList)
                {
                    DictionaryTerm synonymTerm = termList[synonymID];
                    term.SynonymList.Add(synonymTerm.Word);
                }
            }
        }

        private void ProcessSimilarTerms()
        {
            foreach (DictionaryTerm term in termList)
            {
                term.SimilarTermList = new List<string>();
                foreach (int similarTermID in term.SimilarTermIDList)
                {
                    DictionaryTerm similarTerm = termList[similarTermID];
                    term.SimilarTermList.Add(similarTerm.Word);
                }
            }
        }

        // Must run whenever a dictionary is loaded, since the
        // wordList is not serialized.
        public void Initialize()
        {
            termList.Sort((a, b) => a.Word.CompareTo(b.Word));
            for (int ii = 0; ii < termList.Count; ii++)
            {
                termList[ii].ID = ii;
            }
            wordList = new List<string>();
            foreach (DictionaryTerm term in termList)
            {
                wordList.Add(term.Word);
            }
            ProcessSynonyms();
            ProcessSimilarTerms();
        }

        [DataMember]
        public List<DictionaryTerm> TermList
        {
            get { return termList; }
            set { termList = value; }
        }
    }
}
