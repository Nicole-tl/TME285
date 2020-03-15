using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Cognition;
using AgentLibrary.Memories;

namespace AgentApplication.AddedClasses
{

    [DataContract]
    public class GetPhoneNumber : CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.WMTag,
                CognitiveActionParameterType.WMTag,
                CognitiveActionParameterType.WMTag
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
            // Get the tag and the item name
            string inputValueTag = inputList[0].Identifier;
            string inputTag = inputList[1].Identifier;
            DataItem inputDataInformation = ownerAgent.WorkingMemory.FindLastByTag(inputValueTag);
            string inputValue = inputDataInformation.GetValueByTag(inputValueTag);
            TagValueUnit inputTagValueUnit = new TagValueUnit(inputTag, inputValue);
            List<DataItem> itemDataInfo = ownerAgent.LongTermMemory.FindAll(new List<TagValueUnit>() { inputTagValueUnit});

            // Generate the phone number string
            if (itemDataInfo != null && itemDataInfo.Count != 0)
            {
                string phoneTag = InputList[2].Identifier;
                string phoneNumber1 = itemDataInfo[0].GetStringValueByTag(phoneTag);
                string phoneNumber2 = phoneNumber1.Remove(0, 1);
                string phoneNumber3 = "0" + phoneNumber2;
                string outputTag = outputList[0].Identifier;
                ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, phoneNumber3));
                return successTarget;

            }
            else
            {
                return failureTarget;
            }

        }
    }
}
