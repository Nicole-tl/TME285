using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.TextProcessing
{
    public class TextCorpusProcessingSettings
    {
        public Boolean RemoveSpeakerNames { get; set; }
        public Boolean RemoveCapitalizedLines { get; set; }
        public List<Char> SentenceRemovalCharacters { get; set; }
        public List<List<string>> ReplacementPairs { get; set; }
        public List<string> SentenceEndExceptionStrings { get; set; }

        public TextCorpusProcessingSettings()
        {
            RemoveSpeakerNames = true;
            RemoveCapitalizedLines = true;
            SentenceRemovalCharacters = new List<Char>() { '(', ')', '[', ']', '{', '}', '"', '-' };
            ReplacementPairs = new List<List<string>>() { new List<string>() { "...", "." },
                                                          new List<string>() { "..","." } };
            SentenceEndExceptionStrings = new List<string>() { "mr.", "mrs.", "ms.", "dr.", "prof.", "sgt." };
        }
    }
}
