using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.TextProcessing
{
    [DataContract]
    public class TextCorpus
    {
        private Char[] sentenceEndCharList = new Char[] { '.', '?', '!' };
        private Char[] wordSplitCharList = new Char[] { ' ', ',', ';' };
        private Char[] completeSplitCharList = new Char[] { '.', '?', '!', ' ', ',', ';' };

        private List<string> sentenceList; // All sentences from the corpus
        private List<Exchange> exchangeList;  // All two-person exchanges from the corpus
        private List<string> wordList; // alphabetically sorted list of words in the corpus
        private List<double> relativeFrequencyList; // indexed like the wordList


        public void ProcessRawTextData(RawTextData rawTextData, TextCorpusProcessingSettings settings)
        {
            GenerateSentences(rawTextData, settings);
            GenerateExchanges(rawTextData, settings);
        }

        private void GenerateExchanges(RawTextData rawTextData, TextCorpusProcessingSettings settings)
        {
            exchangeList = new List<Exchange>();
            List<string> textDataList = rawTextData.TextDataList;
            List<string> speakerList = rawTextData.SpeakerList;
            int index = 0;
            Boolean inExchange = false;
            Boolean firstSpeakerDone = false;
            string firstSpeaker = "";
            string firstSpeakerStatement = "";
            string secondSpeaker = "";
            string secondSpeakerStatement = "";
            while (index < textDataList.Count)
            {
                string textData = textDataList[index];
                if (!inExchange)
                {
                    if (speakerList.Contains(textData))  // Speakers on separate lines (assumed)
                    {
                        inExchange = true;
                        firstSpeaker = textData;
                    }
                }
                else
                {
                    if (!firstSpeakerDone)
                    {
                        if (!speakerList.Contains(textData))
                        {
                            firstSpeakerStatement += textData + " ";
                        }
                        else
                        {
                            firstSpeakerStatement = firstSpeakerStatement.Trim();
                            firstSpeakerDone = true;
                            secondSpeaker = textData;
                        }
                    }
                    else
                    {
                        if (!speakerList.Contains(textData))
                        {
                            secondSpeakerStatement += textData + " ";
                        }
                        else
                        {
                            secondSpeakerStatement = secondSpeakerStatement.Trim();
                            inExchange = false;
                            firstSpeakerDone = false;
                            // Remove (for simplicity) any sentences containing ANY removal pair character 
                            // (In some rare cases, a sentence may, for example, contain "(" but not ")" etc.
                            Boolean includeExchange = true;
                            foreach (char c in settings.SentenceRemovalCharacters)
                            {
                                if (firstSpeakerStatement.Contains(c) || secondSpeakerStatement.Contains(c))
                                {
                                    includeExchange = false;
                                }
                            }
                            foreach (string speaker in speakerList)
                            {
                                if (firstSpeakerStatement.Contains(speaker) || secondSpeakerStatement.Contains(speaker))
                                {
                                    includeExchange = false;
                                }
                            }
                            if (!firstSpeakerStatement.EndsWith("?")) { includeExchange = false; }
                            if (includeExchange)
                            {
                                firstSpeakerStatement = CarryOutReplacements(firstSpeakerStatement, settings);
                                secondSpeakerStatement = CarryOutReplacements(secondSpeakerStatement, settings);
                                Exchange exchange = new Exchange();
                                exchange.Statement1 = firstSpeakerStatement;
                                exchange.Statement2 = secondSpeakerStatement;
                                exchangeList.Add(exchange);
                            }
                            firstSpeakerStatement = "";
                            secondSpeakerStatement = "";
                            index--; // To identify the new exchange.
                        }
                    }
                }
                index++;
            }
        }

        public void TrimSentencesWithDictionary(Dictionary dictionary)
        {
            int index = 0;
            while (index < sentenceList.Count)
            {
                string sentence = sentenceList[index];
                List<string> sentenceAsWords = sentence.Split(completeSplitCharList, StringSplitOptions.RemoveEmptyEntries).ToList();
                Boolean keepSentence = true;
                foreach (string word in sentenceAsWords)
                {
                    if (!dictionary.ContainsWord(word.ToLower()))
                    {
                        keepSentence = false;
                        break;
                    }
                }
                if (keepSentence) { index++; }
                else { sentenceList.RemoveAt(index); }
            }
        }

        public void TrimExchangesWithDictionary(Dictionary dictionary)
        {
            int index = 0;
            while (index < exchangeList.Count)
            {
                Boolean keepExchange = true;

                string speaker1Statement = exchangeList[index].Statement1;
                List<string> sentenceAsWords = speaker1Statement.Split(completeSplitCharList, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string word in sentenceAsWords)
                {
                    if (!dictionary.ContainsWord(word.ToLower()))
                    {
                        keepExchange = false;
                        break;
                    }
                }
                string speaker2Statement = exchangeList[index].Statement2;
                sentenceAsWords = speaker2Statement.Split(completeSplitCharList, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string word in sentenceAsWords)
                {
                    if (!dictionary.ContainsWord(word.ToLower()))
                    {
                        keepExchange = false;
                        break;
                    }
                }

                if (keepExchange) { index++; }
                else { exchangeList.RemoveAt(index); }
            }
        }


        private void GenerateSentences(RawTextData rawTextData, TextCorpusProcessingSettings settings)
        {
            List<string> textDataList = rawTextData.TextDataList;
            List<string> speakerList = rawTextData.SpeakerList;
            if (sentenceList == null) { sentenceList = new List<string>(); }
            Boolean inSentence = false;
            string sentence = "";
            int index = 0;
            Boolean speakerFound = false;
            while (index < textDataList.Count)
            {
                string textData = textDataList[index];
                if (!speakerFound)
                {
                    if (speakerList.Contains(textData))
                    {
                        speakerFound = true;
                    }
                }
                else
                {
                    if (!speakerList.Contains(textData))  // Not a speaker
                    {
                        if (textData.Any(Char.IsLower))  // Not a capitalized line
                        {
                            if (textData != "")
                            {
                                List<string> rawDataAsWords = textData.Split(wordSplitCharList, StringSplitOptions.RemoveEmptyEntries).ToList();
                                for (int jj = 0; jj < rawDataAsWords.Count; jj++)
                                {
                                    if (!inSentence)
                                    {
                                        if (Char.IsUpper(rawDataAsWords[jj][0]))
                                        {
                                            sentence = rawDataAsWords[jj] + " ";
                                            inSentence = true;
                                        }
                                        if (sentenceEndCharList.Contains(rawDataAsWords[jj].Last())) // Handle single-word sentences
                                        {
                                            inSentence = false;
                                            sentenceList.Add(sentence);
                                            speakerFound = false;
                                        }
                                    }
                                    else
                                    {
                                        sentence += rawDataAsWords[jj] + " ";
                                        if (sentenceEndCharList.Contains(rawDataAsWords[jj].Last()))
                                        {
                                            if (!settings.SentenceEndExceptionStrings.Contains(rawDataAsWords[jj].ToLower()))
                                            {
                                                inSentence = false;
                                                sentenceList.Add(sentence.Trim());
                                                speakerFound = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                index++;
            }
            //    if (sentence != "") { sentenceList.Add(sentence); }

            // Remove (for simplicity) any sentences containing ANY removal pair character 
            // (In some rare cases, a sentence may, for example, contain "(" but not ")" etc.
            index = 0;
            while (index < sentenceList.Count)
            {
                Boolean removeSentence = false;
                sentence = sentenceList[index];
                foreach (Char c in settings.SentenceRemovalCharacters)
                {
                    if (sentence.Contains(c))
                    {
                        removeSentence = true;
                        break;
                    }
                }
                if (removeSentence) { sentenceList.RemoveAt(index); }
                else { index++; }
            }

            // Remove any sentences containing speaker names
            index = 0;
            while (index < sentenceList.Count)
            {
                sentence = sentenceList[index];
                List<string> sentenceAsWords = sentence.Split(wordSplitCharList, StringSplitOptions.RemoveEmptyEntries).ToList();
                Boolean removeSentence = false;
                foreach (string word in sentenceAsWords)
                {
                    if (speakerList.Contains(word.ToUpper()))
                    {
                        removeSentence = true;
                        break;
                    }
                }
                if (removeSentence) { sentenceList.RemoveAt(index); }
                else { index++; }
            }

            // Make replacements (removing slang words etc.)
            for (int ii = 0; ii < sentenceList.Count; ii++)
            {
                sentenceList[ii] = CarryOutReplacements(sentenceList[ii], settings);
            }
        }

        private string CarryOutReplacements(string sentence, TextCorpusProcessingSettings settings)
        {
            string trimmedSentence = sentence;
            foreach (List<string> replacementPair in settings.ReplacementPairs)
            {
                trimmedSentence = trimmedSentence.Replace(replacementPair[0], replacementPair[1]);
            }
            return trimmedSentence;
        }


        // Generates more sentences, but many that are non-dialogue-related
        private void OLDGenerateSentences(RawTextData rawTextData, TextCorpusProcessingSettings settings)
        {
            List<string> textDataList = rawTextData.TextDataList;
            List<string> speakerList = rawTextData.SpeakerList;
            if (sentenceList == null) { sentenceList = new List<string>(); }
            Boolean inSentence = false;
            string sentence = "";
            int index = 0;
            while (index < textDataList.Count)
            {
                string textData = textDataList[index];
                if (!speakerList.Contains(textData))  // Not a speaker
                {
                    if (textData.Any(Char.IsLower))  // Not a capitalized line
                    {
                        if (textData != "")
                        {
                            List<string> rawDataAsWords = textData.Split(wordSplitCharList, StringSplitOptions.RemoveEmptyEntries).ToList();
                            for (int jj = 0; jj < rawDataAsWords.Count; jj++)
                            {
                                if (!inSentence)
                                {
                                    if (Char.IsUpper(rawDataAsWords[jj][0]))
                                    {
                                        sentence = rawDataAsWords[jj] + " ";
                                        inSentence = true;
                                    }
                                    if (sentenceEndCharList.Contains(rawDataAsWords[jj].Last())) // Handle single-word sentences
                                    {
                                        inSentence = false;
                                        sentenceList.Add(sentence);
                                    }
                                }
                                else
                                {
                                    sentence += rawDataAsWords[jj] + " ";
                                    if (sentenceEndCharList.Contains(rawDataAsWords[jj].Last()))
                                    {
                                        inSentence = false;
                                        sentenceList.Add(sentence.Trim());
                                    }
                                }
                            }
                        }
                    }
                }
                index++;
            }
            if (sentence != "") { sentenceList.Add(sentence); }

            // Remove (for simplicity) any sentences containing ANY removal pair character 
            // (In some rare cases, a sentence may, for example, contain "(" but not ")" etc.
            index = 0;
            while (index < sentenceList.Count)
            {
                Boolean removeSentence = false;
                sentence = sentenceList[index];
                foreach (Char c in settings.SentenceRemovalCharacters)
                {
                    if (sentence.Contains(c))
                    {
                        removeSentence = true;
                        break;
                    }
                }
                if (removeSentence) { sentenceList.RemoveAt(index); }
                else { index++; }
            }

            // Remove any sentences containing speaker names
            index = 0;
            while (index < sentenceList.Count)
            {
                sentence = sentenceList[index];
                List<string> sentenceAsWords = sentence.Split(wordSplitCharList, StringSplitOptions.RemoveEmptyEntries).ToList();
                Boolean removeSentence = false;
                foreach (string word in sentenceAsWords)
                {
                    if (speakerList.Contains(word.ToUpper()))
                    {
                        removeSentence = true;
                        break;
                    }
                }
                if (removeSentence) { sentenceList.RemoveAt(index); }
                else { index++; }
            }

            // Make replacements (removing slang words etc.)

        }

        [DataMember]
        public List<string> SentenceList
        {
            get { return sentenceList; }
            set { sentenceList = value; }
        }

        [DataMember]
        public List<Exchange> ExchangeList
        {
            get { return exchangeList; }
            set { exchangeList = value; }
        }

    }
}
