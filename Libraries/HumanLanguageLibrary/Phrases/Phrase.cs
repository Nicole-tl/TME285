using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.Phrases
{
    [DataContract]
    public class Phrase
    { 
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public ConditionBeforePhrase ConditionBefore { get; set; }
        [DataMember]
        public ConditionAfterPhrase ConditionAfter { get; set; }
    }
}
