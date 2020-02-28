using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Memories;

namespace AgentLibrary.Cognition
{
    [DataContract]
    public class CognitiveActionTarget
    {

        [DataMember]
        public int NextIndex { get; set; }
        [DataMember]
        public string TargetItemID { get; set; }
        public ActionItem TargetItem { get; set; }

        public CognitiveActionTarget()
        {
            NextIndex = -1;                                           // Default value; see also CognitiveAction.
            TargetItem = null;
        }

        // 20190131
        public CognitiveActionTarget(int nextIndex, string targetItemID)
        {
            NextIndex = nextIndex;
            TargetItemID = targetItemID;
        }
        
        // 20190131
        public void Initialize(Agent ownerAgent)
        {
            TargetItem = (ActionItem)ownerAgent.LongTermMemory.ItemList.Find(t => t.ID == TargetItemID);
        }

    }
}


