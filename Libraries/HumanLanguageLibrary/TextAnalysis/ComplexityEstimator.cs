using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.TextAnalysis
{
    public class ComplexityEstimator
    {
        private const double FLESCH_KINCAID_A = 206.835;
        private const double FLESCH_KINCAID_B = 1.015;
        private const double FLESCH_KINCAID_C = 84.6;

        protected char[] interpunctionList = new char[] { ' ', ',', ';', ':', '.', '?', '!' };
        private char[] vowelList = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' };
        private string[] triphthongList = new string[] { "ure", "ial" }; // As in "curious" => 4 vowels, but 3 syllables and "pure" (2 vowels, 1 syllable)
        private string[] diphthongList = new string[] { "ei", "oe", "ea", "ou", "oi", "ai", "ay", "oo" };
        private List<string> exceptionWordList = new List<string>() { "trial" };
        private List<int> exceptionSyllableCountList = new List<int> { 2 };
        // "ei" as in "hEIght", "oe" as in "tOE", "ea" as in "hEAr", "ou" as in "hOUr", "oi" as in "avOId", "ai" as in "hAIr",
        // "ay" as in "stAY", "oo" as in "lOOk"

        public ComplexityEstimator()
        {
            interpunctionList = new char[] { ' ', ',', ';', ':', '.', '?', '!' };
            vowelList = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' };
            triphthongList = new string[] { "ure", "ial", "ion" };
            diphthongList = new string[] { "ei", "oe", "ea", "ou", "oi", "ai", "ay", "oo" };
            exceptionWordList = new List<string>() { "trial", "ion" };
            exceptionSyllableCountList = new List<int> { 2, 2 };
        }

        //
        // See 
        // https://www.waikato.ac.nz/__data/assets/pdf_file/0005/314456/Word-lists-for-Vowels-Diphthongs.pdf
        // and 
        // https://www.howmanysyllables.com/
        // and (perhaps, albeit with errors:
        // https://stackoverflow.com/questions/405161/detecting-syllables-in-a-word
        //
        public int GetNumberOfSyllables(string word)
        {
            int numberOfSyllables = 0;
            int indexInExceptionList = exceptionWordList.FindIndex(w => w == word);
            if (indexInExceptionList >= 0)
            {
                numberOfSyllables = exceptionSyllableCountList[indexInExceptionList];
            }
            else
            {
                if (word.Length > 0)
                {
                    int iChar = 0;
                    // First count the number of vowels
                    while (iChar < word.Length)
                    {
                        Char c = word[iChar];
                        if (vowelList.Contains(c))
                        {
                            numberOfSyllables++;
                        }
                        iChar++;
                    }
                    // Next, remove syllables incorrectly added due to triphtongs
                    if (word.Length > 2)
                    {
                        int ii = 0;
                        while (ii < (word.Length - 2))
                        {
                            string substring = word.Substring(ii, 3);
                            if (triphthongList.Contains(substring))
                            {
                                numberOfSyllables--;
                                ii += 3;
                            }
                            else { ii++; }
                        }
                        Char finalCharacter = word.Last();
                        if (finalCharacter == 's')
                        {
                            Char previousCharacter = word[word.Length - 2];
                            if (previousCharacter == 'e')
                            {
                                Char penultimateCharacter = word[word.Length - 3];
                                if (!vowelList.Contains(penultimateCharacter))
                                {
                                    numberOfSyllables--;
                                }
                            }
                        }
                    }
                    if (word.Length > 1)
                    {
                        int ii = 0;
                        while (ii < (word.Length - 1))
                        {
                            string substring = word.Substring(ii, 2);
                            if (diphthongList.Contains(substring))
                            {
                                numberOfSyllables--;
                                ii += 2;
                            }
                            else { ii++; }
                        }
                        Char finalCharacter = word.Last();
                        if (finalCharacter == 'e')
                        {
                            Char previousCharacter = word[word.Length - 2];
                            if (!vowelList.Contains(previousCharacter))
                            {
                                numberOfSyllables--;
                            }
                        }
                    }
                }
                if (numberOfSyllables < 1) { numberOfSyllables = 1; }
            }
            return numberOfSyllables;
        }

        public double computeFleschKincaidScore(List<string> sentenceSplit)
        {
            int numberOfWords = sentenceSplit.Count;
            int numberOfSyllables = 0;
            foreach (string word in sentenceSplit)
            {
                numberOfSyllables += GetNumberOfSyllables(word);
            }
            double fleschKincaidScore = FLESCH_KINCAID_A - FLESCH_KINCAID_B * numberOfWords - FLESCH_KINCAID_C * (numberOfSyllables / (double)numberOfWords);
            return fleschKincaidScore;
        }

        public double ComputeFleschKincaidScore(string sentence)
        {
            List<string> sentenceSplit = sentence.Split(interpunctionList, StringSplitOptions.RemoveEmptyEntries).ToList();
            int numberOfWords = sentenceSplit.Count;
            int numberOfSyllables = 0;
            foreach (string word in sentenceSplit)
            {
                numberOfSyllables += GetNumberOfSyllables(word);
            }
            double fleschKincaidScore = FLESCH_KINCAID_A - FLESCH_KINCAID_B * numberOfWords - FLESCH_KINCAID_C * (numberOfSyllables / (double)numberOfWords);
            return fleschKincaidScore;
        }
    }
}
