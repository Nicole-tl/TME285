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
    public class SortDescendingOnValueAction: CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.WMTag,
                CognitiveActionParameterType.LTMTag
            };
            return requiredParameterTypeInputList;
        }

        public override List<CognitiveActionParameterType> GetRequiredParameterTypeOutputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeOutputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.WMTag
            };
            return requiredParameterTypeOutputList;
        }

        public override CognitiveActionTarget Process()
        {
            string stmTag = inputList[0].Identifier;
            string sortTag = inputList[1].Identifier;
            DataItem stmItem = ownerAgent.WorkingMemory.FindLastByTag(stmTag);  // Finds the (last) item containing the tag
            List<DataItem> searchResultList = (List<DataItem>)stmItem.GetValueByTag(stmTag); // Finds the value (i.e. the list of items)
            if (searchResultList == null)
            {
                return failureTarget;
            }
            else
            {
                List<DataItem> sortedList = new List<DataItem>(searchResultList);  // Make new list instance, so that the original can be kept as well.
                sortedList.Sort((a, b) => (double.Parse(a.GetValueByTag(sortTag))).CompareTo(double.Parse(b.GetValueByTag(sortTag))));
                sortedList.Reverse(); // Sort in descending order.
                string outputSTMTag = outputList[0].Identifier;
                ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputSTMTag, sortedList));
                return successTarget;
            }
        }
    }
}
