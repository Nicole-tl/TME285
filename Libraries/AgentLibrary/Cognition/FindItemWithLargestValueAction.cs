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
    public class FindItemWithLargestValueAction: CognitiveAction
    {
        public FindItemWithLargestValueAction()
        {
        }

        public override CognitiveActionTarget Process()
        {
            string stmTag = inputList[0].Identifier;
            string searchTag = inputList[1].Identifier;
            DataItem stmItem = ownerAgent.WorkingMemory.FindLastByTag(stmTag);  // Finds the (last) item containing the tag
            List<DataItem> searchResultList = (List<DataItem>)stmItem.GetValueByTag(stmTag); // Finds the value (i.e. the list of items)
            if (searchResultList == null)
            {
                return failureTarget;
            }
            else
            {
                double largestValue = double.MinValue;
                int itemIndex = -1;
                for (int ii = 0; ii < searchResultList.Count; ii++)
                {
                    DataItem dataItem = searchResultList[ii];
                    string value = dataItem.GetStringValueByTag(searchTag);
                    double valueAsNumber = double.Parse(value);
                    if (valueAsNumber >largestValue)
                    {
                        largestValue = valueAsNumber;
                        itemIndex = ii;
                    }
                }
                string name = searchResultList[itemIndex].GetStringValueByTag(Constants.NAME_TAG);
                string outputTag = outputList[0].Identifier;
                ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, name));
                return successTarget;
            }
        }

        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
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

        /*   public CognitiveActionTarget OLDProcess()
           {
               CognitiveActionTarget cognitiveActionOutput = new CognitiveActionTarget();
               string categoryName = inputTagList[0];
               string valueIdentifier = inputTagList[1];
               if (parameterList != null)
               {
                   if (parameterList.Count == 2)
                   {
                       categorySearchTag = parameterList[0];
                       outputTag = parameterList[1];
                   }
               }
               List<DataItem> matchingItemList = ownerAgent.LongTermMemory.
                   FindAll(new List<TagValueUnit>() { new TagValueUnit(categorySearchTag, categoryName) });

               // Test
            //   TagValueUnit searchResultTagValueUnit = new TagValueUnit("SearchResult", matchingItemList);
            //   ownerAgent.LastHistoryDataItem.ContentList.Add(searchResultTagValueUnit);

               double largestValue = 0;
               DataItem selectedItem = null;
               foreach (DataItem matchingItem in matchingItemList)
               {
                   string valueAsString = matchingItem.GetStringValueByTag(valueIdentifier);
                   double valueAsNumber = double.Parse(valueAsString);
                   if (valueAsNumber > largestValue)
                   {
                       largestValue = valueAsNumber;
                       selectedItem = matchingItem;
                   }
               }
               TagValueUnit outputTagValueUnit = new TagValueUnit(outputTagList[0], selectedItem.GetStringValueByTag(outputTag));
               ownerAgent.LastHistoryDataItem.ContentList.Add(outputTagValueUnit);
             //  ownerItem.DynamicInformationList.Add(outputTagValueUnit);
               cognitiveActionOutput.NextIndex = -1;
               cognitiveActionOutput.TargetItem = targetItemList[0];
               return cognitiveActionOutput;
           }  */
    }
}
