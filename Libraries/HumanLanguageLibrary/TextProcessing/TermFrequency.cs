using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.TextProcessing
{
    [DataContract]
    public class TermFrequency
    {
        [DataMember]
        public string Term { get; set; }
        [DataMember]
        public double FrequencyPerMillionWords { get; set; }

        public TermFrequency(string term, double frequencyPerMillionWords)
        {
            Term = term;
            FrequencyPerMillionWords = frequencyPerMillionWords;
        }
    }
}
