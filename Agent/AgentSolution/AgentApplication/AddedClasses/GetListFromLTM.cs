using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Cognition;
using AgentLibrary.Memories;
using AgentLibrary;


namespace AgentApplication.AddedClasses
{

    [DataContract]
    public class GetListFromLTM : CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList= new List<CognitiveActionParameterType>()
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
                CognitiveActionParameterType.WMTag,
            };
            return requiredParameterTypeOutputList;
        }

        public override CognitiveActionTarget Process()
        {
            Boolean searchSuccessful = false;
            CognitiveActionTarget cognitiveActionTarget = new CognitiveActionTarget();
            string wmTag = inputList[0].Identifier;
            DataItem inputDynamicInformationItem = ownerAgent.WorkingMemory.FindLastByTag(wmTag);

            if (inputDynamicInformationItem != null)
            {
                string nameValue = inputDynamicInformationItem.GetStringValueByTag(wmTag);
                TagValueUnit categoryTagValueUnit = new TagValueUnit(wmTag, nameValue);
                List<DataItem> ListOfItem = ownerAgent.LongTermMemory.FindAll(new List<TagValueUnit>() { categoryTagValueUnit });
                
                // If finding data item with specific category then find all and save them as a string
                if (ListOfItem != null && ListOfItem.Count != 0)
                {
                    string ListOfItemName = "";
                    foreach (DataItem item in ListOfItem)
                    { 
                        string itemName = item.GetStringValueByTag("item");
                        ListOfItemName = ListOfItemName + itemName + "\r\n";

                    }

                    string generatedFilePath = "Text" + Constants.DISPLAY_OUTPUT_SEPARATOR + ListOfItemName;

                    string outputTag = outputList[0].Identifier; 
                    ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, generatedFilePath));
                    cognitiveActionTarget = successTarget;
                    searchSuccessful = true;
                }
                else //If the category does not exist in LTM, then display all category in LTM 
                {
                    string inputListTag = inputList[0].Identifier;
                    string inputTag = "";
                    if (inputListTag == "cuisine")
                    {   
                        inputTag = "allCuisines";
                    }
                    else
                    {
                        inputTag = "allMuseum";
                    }
                    DataItem dataCategory = ownerAgent.LongTermMemory.FindLastByTag(inputTag);
                    string listOfCategory = dataCategory.GetValueByTag(inputTag);
                    string generatedListOfCategory = "Text" + Constants.DISPLAY_OUTPUT_SEPARATOR + listOfCategory;
                    string outputTag = outputList[0].Identifier;
                    ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, generatedListOfCategory));
                }
            }
            if (!searchSuccessful) {

                cognitiveActionTarget = failureTarget; 
            }
            return cognitiveActionTarget;
        }
    }
}
