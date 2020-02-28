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
    public class FindElementByIndexAction: CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.WMTag,
                CognitiveActionParameterType.Other,
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
            string wmTag = inputList[0].Identifier;
            int elementIndex = int.Parse(inputList[1].Identifier);
            DataItem dataItem = ownerAgent.WorkingMemory.FindLastByTag(wmTag);
            List<DataItem> dataItemList = dataItem.GetValueByTag(wmTag);
            if (elementIndex < dataItemList.Count)
            { 
                string outputWMTag = outputList[0].Identifier;
                ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputWMTag, new List<DataItem>() { dataItemList[elementIndex] }));
                return successTarget;
            }
            else { return failureTarget; }
        }
    }
}
