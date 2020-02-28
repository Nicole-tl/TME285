using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Visualization;

namespace AgentLibrary.Memories
{
    [DataContract]
    /// MemoryItem is the base class for all memory items.
    /// See also ActionItem, DataItem, and the classes derived from these two classes.
    public class MemoryItem  // MW ToDo: Make abstract
    {
        protected string id;
        protected DateTime timeStamp;
        protected MemoryItemVisualizationData visualizationData;

        public MemoryItem()
        {
            visualizationData = new MemoryItemVisualizationData();
        }

        public virtual void Initialize(Agent ownerAgent) { }  // MW ToDo: Make abstract

        [DataMember]
        /// This property is the id of the memory item. 
        /// 
        /// TO DO: Change this text, after the introduction of the groupID an the localID for action items.
        /// 
        /// Since the id is a string, one
        /// can in principle specify it in many different ways. However, the suggested
        /// usage is to prefix action items with "A", long-term memory items with "D" and
        /// short-term memory item with "S". Note that each memory item must have a unique id.
        public string ID
        {
            get { return id; }
            set
            {
                id = value;
                if (this.GetType().IsSubclassOf(typeof(ActionItem)))
                {
                    ((ActionItem)this).SplitID();
                }
            }
        }

        [DataMember]
        /// The time stamp determines the time at which the memory item was added
        /// (either in STM or LTM).
        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }

        [DataMember]
        public MemoryItemVisualizationData VisualizationData
        {
            get { return visualizationData; }
            set { visualizationData = value; }
        }
    }
}
