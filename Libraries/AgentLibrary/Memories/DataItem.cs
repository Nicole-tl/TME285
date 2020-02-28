using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary.Memories
{
    [DataContract]
    /// The DataItem class is used for storing data in the agent's memory. The
    /// data are stored in the form of a list of items of type TagValueUnit.
    public class DataItem: MemoryItem
    {
        private string predecessorID = null;
        private List<TagValueUnit> contentList;

        public DataItem(): base()
        {
            predecessorID = null;
            contentList = new List<TagValueUnit>();
        }

        // 20190111
        public DataItem(string id, List<TagValueUnit> tagValueUnitList, string predecessorID)
        {
            this.ID = id;
            this.contentList = tagValueUnitList;
            this.predecessorID = predecessorID;
        }

        public TagValueUnit GetTagValueUnit(string tag)
        {
            TagValueUnit tagValueUnit = contentList.Find(t => t.Tag.ToLower() == tag.ToLower());
            return tagValueUnit;
        }

        /// This method checks wheterh or not any of the TagValueUnit instances
        /// contain the specified tag.
        public Boolean HasTag(string tag)
        {
            foreach (TagValueUnit contentUnit in contentList)
            {
                if (contentUnit.Tag.ToLower() == tag.ToLower()) { return true; }
            }
            return false;
        }

        /// This method returns the content (value) of the TagValueUnit
        /// identified by the tag. Note that the value is typecase as a string.
        /// If the tag cannot be found, the method returns null.
        public string GetStringValueByTag(string tag)
        {
            foreach (TagValueUnit contentUnit in contentList)
            {
                if (contentUnit.Tag.ToLower() == tag.ToLower()) { return (string)contentUnit.Value; }
            }
            return null;
        }

        public dynamic GetValueByTag(string tag)
        {
            foreach (TagValueUnit contentUnit in contentList)
            {
                if (contentUnit.Tag.ToLower() == tag.ToLower()) { return contentUnit.Value; }
            }
            return null;
        }

        /// This method first checks whether or not a TagValueUnit is available
        /// with the specified tag. If so, it sets the value (as the corresponding
        /// input parameter). If not, the method adds a new TagValueUnit, with
        /// the specified tag and value.
        public void Set(string tag, dynamic value)
        {
            TagValueUnit tagValueUnit = contentList.Find(t => t.Tag.ToLower() == tag.ToLower());
            if (tagValueUnit != null)
            {
                tagValueUnit.Value = value;
            }
            else
            {
                tagValueUnit = new TagValueUnit(tag, value);
                contentList.Add(tagValueUnit);
            }
        }

        /// This method returns the content(value) of the TagValueUnit
        /// identified by the tag, without any typecasting.
        public dynamic Get(string tag)
        {
            TagValueUnit tagValueUnit = contentList.Find(t => t.Tag.ToLower() == tag.ToLower());
            if (tagValueUnit == null) { return null; }
            else
            {
                return tagValueUnit.Value;
            }
        }

        [DataMember]
        /// This parameter can be used (optionally) to connect different items in memory.
        /// It is useful in cases where one easily wants to connect a sequence of memory
        /// items. Example: The agent maintains (in STM) one memory item for each action
        /// item that is processed and each such STM memory item connects (via the PredecessorID)
        /// the information about Nth processed action item to the information about the
        /// (N-1)th processed action item.
        public string PredecessorID
        {
            get { return predecessorID; }
            set { predecessorID = value; }
        }

        [DataMember]
        /// This list holds the actual contents of the memory item. See also TagValueUnit.
        public List<TagValueUnit> ContentList
        {
            get { return contentList; }
            set { contentList = value; }
        }
    }
}
