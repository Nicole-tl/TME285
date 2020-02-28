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
    public class CategoryNameSearchSingleAction: CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.WMTag,
                CognitiveActionParameterType.LTMCategory
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

        private const string SEARCH_TAG_1 = "category"; // <category>";
        private const string SEARCH_TAG_2 = "name"; // "<name>";

        public override CognitiveActionTarget Process()
        {
            Boolean searchSuccessful = false;
            CognitiveActionTarget cognitiveActionTarget = new CognitiveActionTarget();
            string queryTag = inputList[0].Identifier;
            DataItem inputDynamicInformationItem = ownerAgent.WorkingMemory.FindLastByTag(queryTag);
            if (inputDynamicInformationItem != null)
            {
                string queryValue = inputDynamicInformationItem.GetStringValueByTag(queryTag);
                if (queryValue != null)
                {
                    string categoryName = inputList[1].Identifier;
                    TagValueUnit categoryTagValueUnit = new TagValueUnit(SEARCH_TAG_1, categoryName);
                    TagValueUnit nameTagValueUnit = new TagValueUnit(SEARCH_TAG_2, queryValue);
                    DataItem ltmItem = ownerAgent.LongTermMemory.Find(new List<TagValueUnit>() { categoryTagValueUnit, nameTagValueUnit });
                    if (ltmItem != null)
                    {
                        string outputTag = outputList[0].Identifier;
                        TagValueUnit outputTagValueUnit = ltmItem.ContentList.Find(t => t.Tag == outputTag);
                        if (outputTagValueUnit != null)
                        {
                            TagValueUnit copiedOutputTagValueUnit = new TagValueUnit(outputTagValueUnit.Tag, outputTagValueUnit.Value);
                            ownerAgent.LastHistoryDataItem.ContentList.Add(copiedOutputTagValueUnit);
                            cognitiveActionTarget = successTarget;
                            searchSuccessful = true;
                        }
                    }
                }
            }
            if (!searchSuccessful) { cognitiveActionTarget = failureTarget; }
            return cognitiveActionTarget;
        }
    }
}
