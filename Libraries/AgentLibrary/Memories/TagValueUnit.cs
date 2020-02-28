using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary.Memories
{
    [DataContract]
    /// This class is used for storing information (in the agent's memory, both STM and LTM).
    /// Every DataItem instance contains a list of tag value units.
    public class TagValueUnit
    {
        private const int DEFAULT_MAXIMUM_NUMBER_OF_LIST_ITEMS_DISPLAYED = 10;

        private int maximumNumberOfListItemsDisplayed = DEFAULT_MAXIMUM_NUMBER_OF_LIST_ITEMS_DISPLAYED;

        public TagValueUnit(string tag, dynamic value)
        {
            Tag = tag;
            Value = value;
        }

        public string AsString()
        {
            string tagValueUnitAsString = "Tag: " + Tag + " Value: ";

            if (Value is List<DataItem>)
            {
                List<DataItem> dataItemList = (List<DataItem>)Value;
                int lastIndexToShow = Math.Min(maximumNumberOfListItemsDisplayed, dataItemList.Count);
                tagValueUnitAsString += " {";
                for (int jj = 0; jj < lastIndexToShow; jj++)
                {
                    tagValueUnitAsString += dataItemList[jj].ID + ",";
                }
                tagValueUnitAsString = tagValueUnitAsString.TrimEnd(new char[] { ',' });
                tagValueUnitAsString += "}";
            }
            else
            {
                tagValueUnitAsString += Value.ToString();
            }
         /*   if (Value is List<DataItem>)
            {

            }  */

            return tagValueUnitAsString; 
        }

        [DataMember]
        /// The tag identifies the type of information stored.
        public string Tag { get; set; }
        [DataMember]
        /// The value contains the actual information (identifiable via the Tag).
        public dynamic Value { get; set; }
    }
}
