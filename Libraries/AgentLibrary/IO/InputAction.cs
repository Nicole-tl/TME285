using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Memories;
using AgentLibrary.Patterns;

namespace AgentLibrary.IO
{
    [DataContract]
    public class InputAction
    {
        private List<Pattern> patternList;
        private string targetItemID;  // the target item ID
        private ActionItem targetItem = null; // The target item (generated upon startup)

        public InputAction()
        {
            patternList = new List<Pattern>();
        }

        public InputAction(List<Pattern> patternList, string targetItemID)
        {
            this.patternList = patternList;
            this.targetItemID = targetItemID;
        }

        public void Initialize(Agent ownerAgent)
        {
            targetItem = (ActionItem)ownerAgent.LongTermMemory.ItemList.Find(i => i.ID == targetItemID);
            foreach (Pattern pattern in patternList)
            {
                pattern.Initialize(); // (ownerAgent.RandomNumberGenerator);  // Changed 20190402: random numbers needed only for output items
            }
        }

        [DataMember]
        public List<Pattern> PatternList
        {
            get { return patternList; }
            set { patternList = value; }
        }

        [DataMember]
        public string TargetItemID
        {
            get { return targetItemID; }
            set { targetItemID = value; }
        }

        public ActionItem TargetItem
        {
            get { return targetItem; }
        }
    }
}
