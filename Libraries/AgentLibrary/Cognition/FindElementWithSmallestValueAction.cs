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
    public class FindElementWithSmallestValueAction : CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.WMTag,
                CognitiveActionParameterType.LTMTag,
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
            string searchTag = inputList[1].Identifier;
            DataItem stmItem = ownerAgent.WorkingMemory.FindLastByTag(stmTag);  // Finds the (last) item containing the tag
            List<DataItem> searchResultList = (List<DataItem>)stmItem.GetValueByTag(stmTag); // Finds the value (i.e. the list of items)
            DataItem smallestValueItem = null;
            if (searchResultList == null)
            {
                return failureTarget;
            }
            else
            {
                double smallestValue = double.MaxValue;
                int itemIndex = -1;
                for (int ii = 0; ii < searchResultList.Count; ii++)
                {
                    DataItem dataItem = searchResultList[ii];
                    string value = dataItem.GetStringValueByTag(searchTag);
                    double valueAsNumber = double.Parse(value);
                    if (valueAsNumber < smallestValue)
                    {
                        smallestValueItem = dataItem;
                        smallestValue = valueAsNumber;
                        itemIndex = ii;
                    }
                }
                string outputTag = outputList[0].Identifier;
                ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, new List<DataItem>() { smallestValueItem }));
                return successTarget;
            }
        }
    }
}
