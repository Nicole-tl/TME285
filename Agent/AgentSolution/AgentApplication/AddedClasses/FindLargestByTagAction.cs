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
    public class FindLargestByTagAction: CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.WMTag,
                CognitiveActionParameterType.LTMTag,
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
            // Get all museum string
            string inputTag = inputList[0].Identifier;
            string largestCategoryTag = inputList[1].Identifier;
            DataItem inputDataInformation = ownerAgent.WorkingMemory.FindLastByTag(inputTag);
            string inputString = inputDataInformation.GetValueByTag(inputTag);
            string dataNameInfo = inputString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
            List<string> nameList = dataNameInfo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            int lenOfnameList = nameList.Count;
            int Largest = 100;
            string LargestMuseum = null;

            // compare the ranking for all museum and save the smallest one
            for (int i = 0; i <= lenOfnameList - 1; i = i+1 )
            {
                TagValueUnit itemTagValueUnit = new TagValueUnit("item", nameList[i]);
                List<DataItem> eachDataItemList = ownerAgent.LongTermMemory.FindAll(new List<TagValueUnit>() { itemTagValueUnit});
                DataItem eachDataItem = eachDataItemList[0];
                int itemLargestCategoryValue = Convert.ToInt32(eachDataItem.GetStringValueByTag(largestCategoryTag));
                if (itemLargestCategoryValue <= Largest)
                {
                    Largest = itemLargestCategoryValue;
                    LargestMuseum = nameList[i];
                }

            }
            if (LargestMuseum != null)
            {
                string outputTag = outputList[0].Identifier;
                ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, LargestMuseum));
                return successTarget;
            }
            else
            {
                return failureTarget;
            }
            
        }
    }
}
