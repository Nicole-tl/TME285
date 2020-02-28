using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Memories;
using HumanLanguageLibrary.TextAnalysis;
//using TextProcessingLibrary;

namespace AgentLibrary.Patterns
{
    public class PatternAlternative
    {
        private int id;
        private string sentence;
        private List<string> sentenceSplit;
        private List<PatternTermType> termTypeList;
        private int nonWildcardWordCount;
        private int nonWildcardSyllableCount;
        private int ranking; // ranking among ALL the pattern alternatives (Note: ALL alternatives for the input item, not just for the inputAction where the pattern resides).
                             // See also InputItem.RankPatternAlternatives().

        public PatternAlternative(string sentence, List<string> sentenceSplit, List<PatternTermType> termTypeList)
        {
            this.sentence = sentence;  // Pattern alternative as a single string, generated dynamically(once and for all upon initialization).
            this.sentenceSplit = sentenceSplit; // Pattern alternative as a List<string>, generated dynamically (once and for all upon initialization)
            this.termTypeList = termTypeList; // term type lists for the pattern alternative string list, also generated dynamically (once and for all upon initialization)
            nonWildcardWordCount = 0;
            nonWildcardSyllableCount = 0;
            ComplexityEstimator complexityEstimator = new ComplexityEstimator();
            foreach (string word in sentenceSplit)
            {
                if (!word.Contains(Constants.TAG_LEFT))
                {
                    nonWildcardWordCount++;
                    nonWildcardSyllableCount += complexityEstimator.GetNumberOfSyllables(word);
                }
            }
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public string Sentence
        {
            get { return sentence; }
        }

        public List<string> SentenceSplit
        {
            get { return sentenceSplit; }
        }

        public List<PatternTermType> TermTypeList
        {
            get { return termTypeList; }
        }

        public int NonWildcardWordCount
        {
            get { return nonWildcardWordCount; }
        }

        public int NonWildcardSyllableCount
        {
            get { return nonWildcardSyllableCount; }
        }

        public int Ranking
        {
            get { return ranking; }
            set { ranking = value; }
        }
    }
}
