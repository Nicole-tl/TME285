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
    public class CategoryIdentificationAction: CognitiveAction
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

        // This item finds the contents of the input tag, then tries to
        // find a matching LTM item whose category equal parameterList[0],
        // and then adds the corresponding name property to the dynamicInformationList.
        public override CognitiveActionTarget Process()
        {
            //CognitiveActionTarget cognitiveActionOutput = new CognitiveActionTarget();
            string wmTag = inputList[0].Identifier;
            DataItem inputDynamicInformationItem = ownerAgent.WorkingMemory.FindLastByTag(wmTag);
            if (inputDynamicInformationItem != null)
            {
                string nameValue = inputDynamicInformationItem.GetStringValueByTag(wmTag);
                TagValueUnit nameTagValueUnit = new TagValueUnit(Constants.NAME_TAG, nameValue);
                string category = inputList[1].Identifier;
                TagValueUnit categoryTagValueUnit = new TagValueUnit(Constants.CATEGORY_TAG, category);
                DataItem categoryItem = ownerAgent.LongTermMemory.Find(new List<TagValueUnit>() { categoryTagValueUnit, nameTagValueUnit });
                if (categoryItem != null)
                {
                    string outputTag = outputList[0].Identifier; // outputTagList[0];
                    string outputValue = categoryItem.GetStringValueByTag(Constants.NAME_TAG); // Get the Name value for the categoryItem
                    TagValueUnit outputTagValueUnit = new TagValueUnit(outputTag, outputValue);
                    ownerAgent.LastHistoryDataItem.ContentList.Add(outputTagValueUnit);
                    return successTarget;
                }
                else
                {
                    return failureTarget;
                }
            }
            else { return failureTarget; }
        }
    }
}
