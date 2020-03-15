using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary;
using AgentLibrary.Memories;

namespace AgentLibrary.Cognition
{
    [DataContract]
    public class MyFindLastOutputAction : CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            return null;
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
            CognitiveActionTarget cognitiveActionTarget = new CognitiveActionTarget();
            cognitiveActionTarget = failureTarget;
            if (ownerAgent.LastHistoryDataItem.PredecessorID != null)
            {
                string previousID = ownerAgent.LastHistoryDataItem.PredecessorID;
                DataItem historyDataItem = (DataItem)ownerAgent.WorkingMemory.ItemList.Find(i => i.ID == previousID);
                Boolean endReached = false;
                while (!endReached)
                {
                    string itemType = historyDataItem.GetStringValueByTag(Constants.ACTION_ITEM_TYPE_TAG);
                    if (itemType == Constants.OUTPUT_ITEM_VALUE)
                    {
                        string previousMessage = historyDataItem.GetStringValueByTag(Constants.HISTORY_ITEM_MESSAGE_TAG);
                        string outputTag = outputList[0].Identifier;
                        TagValueUnit previousMessageTagValueUnit = new TagValueUnit(outputTag, previousMessage);
                        ownerAgent.LastHistoryDataItem.ContentList.Add(previousMessageTagValueUnit);
                        cognitiveActionTarget = successTarget;
                        break;
                    }
                    else
                    {
                        if (historyDataItem.PredecessorID == null) { endReached = true; }
                        else
                        {
                            previousID = historyDataItem.PredecessorID;
                            historyDataItem = (DataItem)ownerAgent.WorkingMemory.ItemList.Find(i => i.ID == previousID);
                            if (historyDataItem == null) { endReached = true; }
                        }
                    }
                }
            }
            return cognitiveActionTarget;
        }
    }
}
