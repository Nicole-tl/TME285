using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentLibrary.IO;

namespace AgentLibrary.Memories
{
    [DataContract]
    /// The ActionItem class is the abstract base class for the classes that handle
    /// agent actions, i.e. InputItem, CognitiveItem, and OutputItem. See also DataItem
    public abstract class ActionItem: MemoryItem
    {
        protected Agent ownerAgent;
        protected string groupID;
        protected string localID;
        protected string contextAfter = null;
        protected Boolean endsDialogue = false;
        //  protected DataItem dynamicInformationItem;


        /// This method sets a pointer to the agent.
        public override void Initialize(Agent ownerAgent)
        {
            this.ownerAgent = ownerAgent;
         //   this.dynamicInformationItem = ownerAgent.DynamicInformationItem; 
        }

        protected DataItem GenerateHistoryItem(string message, string itemType, ActionItem item, List<TagValueUnit> tagValueUnitList)
        {
            DataItem historyDataItem = new DataItem();
            long lastNumberedID = ownerAgent.WorkingMemory.GetLastDataItemID();
            long nextID = lastNumberedID + 1;
            historyDataItem.ID = Constants.STM_ITEM_PREFIX + nextID.ToString("0000000");
            if (ownerAgent.LastHistoryDataItem == null)
            {
                historyDataItem.PredecessorID = null;
            }
            else
            {
                historyDataItem.PredecessorID = ownerAgent.LastHistoryDataItem.ID;
            }
            historyDataItem.ContentList.Add(new TagValueUnit("Category", "History"));
            historyDataItem.ContentList.Add(new TagValueUnit(Constants.ACTION_ITEM_ID_TAG, this.ID));
            historyDataItem.ContentList.Add(new TagValueUnit(Constants.ACTION_ITEM_TYPE_TAG, itemType));
            historyDataItem.ContentList.Add(new TagValueUnit(Constants.ITEM_POINTER_TAG, item));
            historyDataItem.ContentList.Add(new TagValueUnit(Constants.HISTORY_ITEM_MESSAGE_TAG, message));
            if (tagValueUnitList != null)
            {
                foreach (TagValueUnit tagValueUnit in tagValueUnitList)
                {
                    historyDataItem.ContentList.Add(tagValueUnit);
                }
            }
            return historyDataItem;
        }

        public void SplitID()
        {
            List<string> groupLocalID = id.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            this.groupID = groupLocalID[0];
            this.localID = groupLocalID[1];
        }

        public abstract void Process();

        [DataMember]
        public string ContextAfter
        {
            get { return contextAfter; }
            set { contextAfter = value; }
        }

     //   [DataMember]
        public string GroupID
        {
            get { return groupID; }
       //     private set { groupID = value; }
        }

     //   [DataMember]
        public string LocalID
        {
            get { return localID; }
        //    private set { localID = value; }
        }

    //    [DataMember]
        public Boolean EndsDialogue
        {
            get { return endsDialogue; }
      //      private set { endsDialogue = value; }
        }

    }
}
