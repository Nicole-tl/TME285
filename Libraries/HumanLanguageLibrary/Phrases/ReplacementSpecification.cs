using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.Phrases
{
    [DataContract]
    public class ReplacementSpecification
    {
     //   public static string ALL_VERBS_INFINITIVE_TO_ING_FORM = "$V0ToIng";
        public static string ALL_POSITIVE_INTEGERS = "$Z+";
        public static string ALL_INTEGERS = "$Z";
        public static string ALL_INTEGERS_LARGER_THAN_1 = "$Z1+";
    
        private string identifier;
        private string description;
     //   private string relativeFilePath;  // 20191112: Unused
        private List<string> replacementList;
        private Boolean displayable = true; // True for most replacements, except (some of) those involving numbers
        private Boolean editable = true; // True except for default replacements.

        public ReplacementSpecification()
        {
            replacementList = null;
         //   relativeFilePath = null;
        }

        public static List<string> GetDefaultReplacementSpecificationIdentifierList()
        {
          //  return new List<string>() { ALL_VERBS_INFINITIVE_TO_ING_FORM, ALL_INTEGERS, ALL_POSITIVE_INTEGERS, ALL_INTEGERS_LARGER_THAN_1 };
            return new List<string>() { ALL_INTEGERS, ALL_POSITIVE_INTEGERS, ALL_INTEGERS_LARGER_THAN_1 };
        }

        [DataMember]
        public string Identifier
        {
            get { return identifier; }
            set { identifier = value; }
        }

        [DataMember]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

     /*   [DataMember]
        public string RelativeFilePath
        {
            get { return relativeFilePath; }
            set { relativeFilePath = value; }
        }  */

        [DataMember]
        public List<string> ReplacementList
        {
            get { return replacementList; }
            set { replacementList = value; }
        }

        [DataMember]
        public Boolean Displayable
        {
            get { return displayable; }
            set { displayable = value; }
        }

        [DataMember]
        public Boolean Editable
        {
            get { return editable; }
            set { editable = value; }
        }
    }
}
