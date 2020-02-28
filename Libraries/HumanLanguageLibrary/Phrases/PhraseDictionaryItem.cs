using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.Phrases
{
    [DataContract]
    public class PhraseDictionaryItem
    {
        private string phrase1;
        private string phrase2;
        private ConditionBeforePhrase conditionBefore;
        private ConditionAfterPhrase conditionAfter;
        private Boolean oneSidedInterchangeability = false;

        public PhraseDictionaryItem()
        {
            oneSidedInterchangeability = false;
        }

        [DataMember]
        public string Phrase1
        {
            get { return phrase1; }
            set { phrase1 = value; }
        }

        [DataMember]
        public string Phrase2
        {
            get { return phrase2; }
            set { phrase2 = value; }
        }

        [DataMember]
        public ConditionBeforePhrase ConditionBefore
        {
            get { return conditionBefore; }
            set { conditionBefore = value; }
        }

        [DataMember]
        public ConditionAfterPhrase ConditionAfter
        {
            get { return conditionAfter; }
            set { conditionAfter = value; }
        }

        [DataMember]
        public Boolean OneSidedInterchangeability
        {
            get { return oneSidedInterchangeability; }
            set { oneSidedInterchangeability = value; }
        }
    }
}
