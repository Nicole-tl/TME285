using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary
{
    [DataContract]
    public enum WordClass { [EnumMember]Noun, [EnumMember]Verb, [EnumMember]Adjective, [EnumMember]Adverb, [EnumMember]Pronoun, [EnumMember]Other,
    [EnumMember]Unknown};
}
