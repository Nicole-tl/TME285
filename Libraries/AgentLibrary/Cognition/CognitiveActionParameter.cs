using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary.Cognition
{
    [DataContract]
    public class CognitiveActionParameter
    {
        [DataMember]
        public CognitiveActionParameterType Type { get; set; }
        [DataMember]
        public string Identifier { get; set; }

        public CognitiveActionParameter(CognitiveActionParameterType type, string identifier)
        {
            this.Type = type;
            this.Identifier = identifier;
        }
    }
}
