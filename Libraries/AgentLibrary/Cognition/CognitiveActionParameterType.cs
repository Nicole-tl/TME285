using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary.Cognition
{
    [DataContract]
    public enum CognitiveActionParameterType { [EnumMember]WMTag, [EnumMember]LTMTag, [EnumMember]LTMCategory, [EnumMember]Other };
}
