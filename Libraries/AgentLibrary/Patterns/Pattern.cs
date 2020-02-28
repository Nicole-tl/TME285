using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Memories;
using HumanLanguageLibrary;
using HumanLanguageLibrary.Phrases;

namespace AgentLibrary.Patterns
{
    [DataContract]
    public class Pattern
    {
        protected char[] interpunctionList = new char[] { ' ', ',', ';', ':', '.', '?', '!' };

        protected string patternSpecification;
        protected Random randomNumberGenerator;
        protected List<List<string>> phraseList;   // pattern specification split into phrases - required for generating pattern alternatives (see Initialize()).
        protected List<PatternAlternative> patternAlternativeList;

        protected Boolean useVerbatim = false;

        public Pattern()
        {
            // Nothing to do here.
        }

        public string RemoveEndPunctuation(string inputString)
        {
            string outputString = inputString.TrimEnd(new char[] { '.', '!', '?' });
            return outputString;
        }

        public Pattern(string patternSpecification)
        {
            this.patternSpecification = patternSpecification;
        }

        public List<string> GetDynamicInformation()
        {
            List<string> dynamicInformationList = new List<string>();
            int index = 0;
            Boolean inDynamicInformation = false;
            string dynamicInformation = "";
            while (index < patternSpecification.Length)
            {
                if (!inDynamicInformation)
                {
                    if (patternSpecification[index] == Constants.TAG_LEFT) { inDynamicInformation = true; }
                }
                else
                {
                    if (patternSpecification[index] == Constants.TAG_RIGHT)
                    {
                        dynamicInformationList.Add(dynamicInformation);
                        dynamicInformation = "";
                        inDynamicInformation = false;
                    }
                    else { dynamicInformation += patternSpecification[index]; }
                }
                index++;
            }
            return dynamicInformationList;
        }

        public void Initialize()
        {
            interpunctionList = new char[] { ' ', ',', ';', ':', '.', '?', '!' };
            //  this.randomNumberGenerator = randomNumberGenerator;   // Only needed for output patterns - see below
            if (!useVerbatim)
            {
                patternSpecification = patternSpecification.ToLower();
                patternSpecification = RemoveEndPunctuation(patternSpecification);

                phraseList = new List<List<string>>();

                string processingString = patternSpecification;
                int index = 0;
                Boolean inBracket = false;
                string subString = "";
                while (index < processingString.Length)
                {
                    if (!inBracket)
                    {
                        if (processingString[index] == '[')
                        {
                            inBracket = true;
                            subString = subString.Trim();
                            if (subString != "")
                            {
                                List<string> subStringSplit = subString.Split(interpunctionList, StringSplitOptions.RemoveEmptyEntries).ToList();  // 20190322
                                                                                                                                                   //     List<string> subStringSplit = subString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                foreach (string subStringSplitItem in subStringSplit)
                                {
                                    phraseList.Add(new List<string>() { subStringSplitItem });
                                }
                            }
                            subString = "";
                        }
                        subString += processingString[index];
                    }
                    else
                    {
                        subString += processingString[index];
                        if (processingString[index] == ']')
                        {
                            List<string> subStringSplit = subString.Split(new char[] { '[', ',', ']' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            for (int ii = 0; ii < subStringSplit.Count; ii++)
                            {
                                subStringSplit[ii] = subStringSplit[ii].Trim();
                            }
                            phraseList.Add(subStringSplit);
                            subString = "";
                            inBracket = false;
                        }
                    }
                    index++;
                }
                if (subString != "")
                {
                    List<string> subStringSplit = subString.Split(interpunctionList, StringSplitOptions.RemoveEmptyEntries).ToList(); // 20190322
                                                                                                                                      //   List<string> subStringSplit = subString.Split(new char[] { ' ', ',', '.','!','?' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (string subStringSplitItem in subStringSplit)
                    {
                        phraseList.Add(new List<string>() { subStringSplitItem });
                    }
                }
                for (int ii = 0; ii < phraseList.Count; ii++)
                {
                    if (phraseList[ii].Count > 1)
                    {
                        // Sort in descending order based on the number of words (= number of spaces + 1) in each phrase.
                        phraseList[ii].Sort((a2, a1) => a1.Where(c => Char.IsWhiteSpace(c)).Count().CompareTo(a2.Where(c => Char.IsWhiteSpace(c)).Count()));
                    }
                }

                GenerateAlternatives(); // 20190321: Generate all alternative patterns as strings
            }
        }

        // 20190402: Used for output patterns.
        public void Initialize(Random randomNumberGenerator)
        {
            Initialize();
            this.randomNumberGenerator = randomNumberGenerator;
        }

        /*  public virtual double TestMatch(string message, out List<TagValueUnit> dynamicInformationList)
          {

          }  */

        //
        // 20190321: New matching method: Handles both single and multiple wildcards.
        //
        public virtual double Match(string message, out List<TagValueUnit> dynamicInformationList)
        {
            dynamicInformationList = new List<TagValueUnit>();
            List<string> messageSplit = message.ToLower().Split(interpunctionList, StringSplitOptions.RemoveEmptyEntries).ToList();
            int index = 0;
            Boolean matchFound = false;
            while (!matchFound)
            {
                List<string> patternAlternativeSplit = patternAlternativeList[index].SentenceSplit; //  patternAlternativeSplitList[index];
                List<PatternTermType> patternAlternativeTermTypeSplit = patternAlternativeList[index].TermTypeList; // patternAlternativeTermTypeSplitList[index];
                int indexInPattern = 0;
                int indexInMessage = 0;
                while (indexInPattern < patternAlternativeSplit.Count)
                {
                    if (indexInMessage >= messageSplit.Count) { break; }  // end of message before end of pattern.
                    PatternTermType termType = patternAlternativeTermTypeSplit[indexInPattern];
                    if (termType == PatternTermType.Exact)
                    {
                        if (messageSplit[indexInMessage] != patternAlternativeSplit[indexInPattern]) { break; }
                        else
                        {
                            indexInPattern++;
                            indexInMessage++;
                        }
                    }
                    else if (termType == PatternTermType.SingleWildcard)
                    {
                        string messageTerm = messageSplit[indexInMessage];
                        string patternTerm = patternAlternativeSplit[indexInPattern];
                        string tag = BracketRemover.Process(patternTerm).ToLower();
                        TagValueUnit dynamicInformation = new TagValueUnit(tag, messageTerm);
                        dynamicInformationList.Add(dynamicInformation);
                        indexInPattern++;
                        indexInMessage++;
                    }
                    else  // Multiple wild card
                    {
                        string patternTerm = patternAlternativeSplit[indexInPattern];
                        if (indexInPattern == (patternAlternativeSplit.Count - 1))
                        {
                            // Multiple wildcard ends the pattern => match found (see also lines ... below)
                            string wildCardString = messageSplit[indexInMessage];
                            for (int ii = indexInMessage + 1; ii < messageSplit.Count; ii++) { wildCardString += " " + messageSplit[ii]; }
                            string tag = BracketRemover.Process(patternTerm).ToLower().TrimEnd(Constants.MULTIPLE_WILDCARD_INDICATOR);
                            TagValueUnit dynamicInformation = new TagValueUnit(tag, wildCardString);
                            dynamicInformationList.Add(dynamicInformation);
                            indexInPattern = patternAlternativeSplit.Count;
                            indexInMessage = messageSplit.Count;
                        }
                        else
                        {
                            string nextPatternTerm = patternAlternativeSplit[indexInPattern + 1];
                            string wildCardString = messageSplit[indexInMessage];
                            indexInMessage++;
                            Boolean multipleWildcardMatchFound = false;
                            while (indexInMessage < messageSplit.Count)
                            {
                                string messageTerm = messageSplit[indexInMessage];
                                if (messageTerm == nextPatternTerm)  // End of wildcard match found
                                {
                                    string tag = BracketRemover.Process(patternTerm).ToLower().TrimEnd(Constants.MULTIPLE_WILDCARD_INDICATOR);
                                    TagValueUnit dynamicInformation = new TagValueUnit(tag, wildCardString);
                                    dynamicInformationList.Add(dynamicInformation);
                                    indexInMessage--; // Required here, since the (exact) message term is supposed to match the next pattern term
                                    multipleWildcardMatchFound = true;
                                    break;
                                }
                                else
                                {
                                    wildCardString += " " + messageTerm;
                                    indexInMessage++;
                                }
                            }
                            if (!multipleWildcardMatchFound) { break; }
                            else
                            {
                                indexInPattern++;
                                indexInMessage++;
                            }
                        }
                    }
                    if ((indexInPattern == patternAlternativeSplit.Count) && (indexInMessage == messageSplit.Count))
                    {
                        matchFound = true;
                    }
                }
                if (!matchFound)
                {
                    index++;
                    if (index >= patternAlternativeList.Count) { break; } // patternAlternativeSplitList.Count) { break; }
                }
            }

            // Fix this: Change to Boolean, since I only use 0 or 1 anyway.
            if (!matchFound) { return 0; }
            else { return 1; }
        }

        public List<string> GetTagList()
        {
            List<string> tagList = new List<string>();
            for (int ii = 0; ii < phraseList.Count; ii++)
            {
                if (phraseList[ii].Count == 1)
                {
                    string phrase = phraseList[ii][0];
                    if (phrase.Contains(Constants.TAG_LEFT)) { tagList.Add(phrase); }
                }
            }
            /*  foreach (string patternItem in patternItemList)
              {
                  if (patternItem.Contains(Constants.TAG_LEFT)) { tagList.Add(patternItem); }
              }  */
            return tagList;
        }

        // 20190322
        public virtual List<string> GenerateAllOutputAlternatives(List<TagValueUnit> dynamicInformationList)
        {
            List<string> outputAlternativeList = new List<string>();
            foreach (PatternAlternative patternAlternative in patternAlternativeList)
            {
                string outputAlternative = "";
                List<string> patternAlternativeAsStringList = patternAlternative.SentenceSplit;
                foreach (string word in patternAlternativeAsStringList)
                {
                    if (!word.Contains(Constants.TAG_LEFT)) { outputAlternative += word + " "; }
                    else
                    {
                        string dynamicTermTag = BracketRemover.Process(word);
                        TagValueUnit dynamicInformation = dynamicInformationList.Find(t => t.Tag == dynamicTermTag);
                        outputAlternative += dynamicInformation.Value + " ";
                    }
                }
                outputAlternative.Trim();
                outputAlternativeList.Add(outputAlternative);
            }
            return outputAlternativeList;
        }

        public virtual string GenerateOutput(List<TagValueUnit> dynamicInformationList)
        {
            string output = "";
            for (int ii = 0; ii < phraseList.Count; ii++)
            {
                if (phraseList[ii].Count == 1)
                {
                    string phrase = phraseList[ii][0];
                    if (!phrase.Contains(Constants.TAG_LEFT))
                    {
                        output += phraseList[ii][0] + " ";
                    }
                    else
                    {
                        string patternItem = BracketRemover.Process(phraseList[ii][0]);  // 20190201: Remove < >
                        TagValueUnit dynamicInformation = dynamicInformationList.Find(d => d.Tag.ToLower() == patternItem.ToLower());
                        string value = dynamicInformation.Value.ToString();
                        output += value + " ";
                    }
                }
                else
                {
                    int randomIndex = randomNumberGenerator.Next(0, phraseList[ii].Count);
                    if (phraseList[ii][randomIndex] != "{}")
                    {
                        output += phraseList[ii][randomIndex] + " ";
                    }
                }
            }
            output = output.TrimEnd(new char[] { ' ' });
            return output;
        }

        protected void GenerateAlternatives()
        {
            if (!useVerbatim)
            {
                this.patternAlternativeList = new List<PatternAlternative>();
                List<string> patternAlternativeList = new List<string>();
                List<List<string>> patternAlternativeSplitList = new List<List<string>>();
                List<List<PatternTermType>> patternAlternativeTermTypeSplitList = new List<List<PatternTermType>>();
                List<int> countList = new List<int>();
                int totalCount = 1;
                for (int kk = 0; kk < phraseList.Count; kk++)
                {
                    totalCount *= phraseList[kk].Count;
                    countList.Add(phraseList[kk].Count);
                }

                for (int iAlternative = 0; iAlternative < totalCount; iAlternative++)
                {
                    string sentence = "";
                    int ii = iAlternative;
                    List<int> indexList = new List<int>();
                    for (int jj = 0; jj < phraseList.Count; jj++)
                    {
                        int index = ii % phraseList[jj].Count;
                        if (!phraseList[jj][index].Contains(Constants.EMPTY_PATTERN_TERM_LEFT))
                        {
                            sentence += phraseList[jj][index] + " ";
                        }
                        indexList.Add(index);
                        ii = (ii / phraseList[jj].Count);
                    }
                    sentence = sentence.Trim();
                    List<string> sentenceSplit = sentence.ToLower().Split(interpunctionList, StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<PatternTermType> termTypeSplit = GenerateTermTypeList(sentenceSplit);

                    PatternAlternative patternAlternative = new PatternAlternative(sentence, sentenceSplit, termTypeSplit);
                    this.patternAlternativeList.Add(patternAlternative);
                }
            }
        }

        private List<PatternTermType> GenerateTermTypeList(List<string> termList)
        {
            List<PatternTermType> termTypeList = new List<PatternTermType>();
            foreach (string term in termList)
            {
                if (!term.Contains(Constants.TAG_LEFT)) { termTypeList.Add(PatternTermType.Exact); }
                else
                {
                    if (term.Contains(Constants.MULTIPLE_WILDCARD_INDICATOR)) { termTypeList.Add(PatternTermType.MultipleWildcard); }
                    else { termTypeList.Add(PatternTermType.SingleWildcard); }
                }
            }
            return termTypeList;
        }

        public string ExpandWithSynonyms(Dictionary dictionary)
        {
            string expandedString = "";
            for (int ii = 0; ii < phraseList.Count; ii++)
            {
                List<string> addedSynonymList = new List<string>();
                for (int kk = 0; kk < phraseList[ii].Count; kk++)
                {
                    if (!phraseList[ii][kk].Contains(Constants.TAG_LEFT))
                    {
                        if (!phraseList[ii][kk].Contains(' '))
                        {
                            string word = phraseList[ii][kk];
                            List<DictionaryTerm> termList = dictionary.FindTerms(word);
                            if (termList.Count == 1)
                            {
                                for (int jj = 0; jj < termList[0].SynonymList.Count; jj++)
                                {
                                    addedSynonymList.Add(termList[0].SynonymList[jj]);
                                }
                            }
                        }
                    }
                }
                phraseList[ii].AddRange(addedSynonymList);
                if (phraseList[ii].Count == 1) { expandedString += phraseList[ii][0] + " "; }
                else
                {
                    expandedString += '[';
                    for (int kk = 0; kk < phraseList[ii].Count; kk++) { expandedString += phraseList[ii][kk] + ","; }
                    expandedString = expandedString.TrimEnd(',');
                    expandedString += ']';
                }
            }
            return expandedString;
        }

        // 20191112
        // Here it is assumed that the phrase dictionary has been expanded 
        //
        // NOTE: This method does not (yet) handle numerical replacements.
        public void Extend(PhraseDictionary phraseDictionary)
        {
            string expandedPatternSpecification = patternSpecification;
            foreach (PhraseDictionaryItem item in phraseDictionary.ItemList)
            {
                if (patternSpecification.Contains(item.Phrase1))
                {
                    string extendedPhrase = "[" + item.Phrase1 + "," + item.Phrase2 + "]";
                    patternSpecification.Replace(item.Phrase1, extendedPhrase);
                }
                else if (patternSpecification.Contains(item.Phrase2))
                {
                    string extendedPhrase = "[" + item.Phrase1 + "," + item.Phrase2 + "]";
                    patternSpecification.Replace(item.Phrase2, extendedPhrase);
                }
            }
        }

        // 20190322
        public List<string> GetAllPatternAlternativesAsString()
        {
            List<string> patternAlternativesAsStringList = new List<string>();
            foreach (PatternAlternative alternative in patternAlternativeList)
            {
                patternAlternativesAsStringList.Add(alternative.Sentence);
            }
            return patternAlternativesAsStringList;
        }

        // 20190322
        public List<List<string>> GetAllPatternAlternativesAsStringList()
        {
            List<List<string>> patternAlternativesAsStringListList = new List<List<string>>();
            foreach (PatternAlternative alternative in patternAlternativeList)
            {
                patternAlternativesAsStringListList.Add(alternative.SentenceSplit);
            }
            return patternAlternativesAsStringListList;
        }


        public List<List<string>> PhraseList
        {
            get { return phraseList; }
        }

        public List<PatternAlternative> PatternAlternativeList
        {
            get { return patternAlternativeList; }
        }

        [DataMember]
        public string PatternSpecification
        {
            get { return patternSpecification; }
            set { patternSpecification = value; }
        }

        [DataMember]
        public Boolean UseVerbatim
        { 
            get { return useVerbatim; }
            set { useVerbatim = value; }
        }
    }
}
