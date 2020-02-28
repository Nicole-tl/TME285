using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentLibrary.Cognition;
using AgentLibrary.IO;

namespace AgentLibrary.Memories
{
    [DataContract]
    public class CognitiveItem: ActionItem
    {
        protected List<CognitiveAction> cognitiveActionList = null;
        protected Boolean allowContextActivation = false;

        // Pointers
        protected ActionItem targetItem;

        protected Thread cognitionThread;
        public event EventHandler ProcessCompleted = null;

        public CognitiveItem(): base()
        {
            cognitiveActionList = new List<CognitiveAction>();
        }

        public CognitiveItem(string id): base()
        {
            cognitiveActionList = new List<CognitiveAction>();
            this.id = id;
            SplitID();
        }

        public override void Initialize(Agent ownerAgent)
        {
            base.Initialize(ownerAgent);
            SplitID();
            if (cognitiveActionList.Count == 0) { this.endsDialogue = true; } // Not really relevant, since the property is (so far) only used for output items.
            foreach (CognitiveAction cognitiveAction in cognitiveActionList)
            {
                cognitiveAction.Initialize(ownerAgent, this);
            }
        }

        private void OnProcessCompleted()
        {
            if (ProcessCompleted != null)
            {
                EventHandler handler = ProcessCompleted;
                handler(this, EventArgs.Empty);
            }
        }

        // Process cognitive actions until either (i) a non-null target is found.
        // or (ii) an error is detected.
        protected void CognitionLoop()
        {
            //   dynamicInformationList = new List<TagValueUnit>(); // 20190113 // Removed 20190128

            // 20190128: Add the item already here, so that cognitive actions can have direct access to
            // any information added by preceding cognitive actions (via ownerAgent.LastHistoryDataItem.ContentList).
            DataItem historyDataItem = GenerateHistoryItem("", Constants.COGNITIVE_ITEM_VALUE, this, null); // , dynamicInformationList);
            ownerAgent.WorkingMemory.AddItem(historyDataItem);
            ownerAgent.LastHistoryDataItem = historyDataItem;

            CognitiveActionTarget cognitiveActionOutput = null;
            if (cognitiveActionList != null)
            {
                int index = 0;   // Per construction, always start with the first action
                while (index >= 0)
                {
                    CognitiveAction cognitiveAction = cognitiveActionList[index];
                    cognitiveActionOutput = cognitiveAction.Process();
                    index = cognitiveActionOutput.NextIndex;
                }
            }
            targetItem = cognitiveActionOutput.TargetItem;

            OnProcessCompleted();
        }  

        public override void Process()
        {
            cognitionThread = new Thread(new ThreadStart(CognitionLoop));
            cognitionThread.Start();
        }

        [DataMember]
        public List<CognitiveAction> CognitiveActionList
        {
            get { return cognitiveActionList; }
            set { cognitiveActionList = value; }
        }

        public ActionItem TargetItem
        {
            get { return targetItem; }
        }
    }
}
