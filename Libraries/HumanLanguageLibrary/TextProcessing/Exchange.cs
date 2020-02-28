using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.TextProcessing
{
    [DataContract]
    public class Exchange
    {
        private string statement1;
        private string statement2;

        [DataMember]
        public string Statement1
        {
            get { return statement1; }
            set { statement1 = value; }
        }

        [DataMember]
        public string Statement2
        {
            get { return statement2; }
            set { statement2 = value; }
        }
    }
}
