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

                // List of DataItem with certain category
                List<DataItem> ListOfItem = ownerAgent.LongTermMemory.FindAll(new List<TagValueUnit>() { categoryTagValueUnit });
                if (ListOfItem != null)
                {  
                    
                    string outputTag = outputList[0].Identifier; // outputTagList[0];
                    ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, ListOfItem));

                    return successTarget;
                }
                else
                {
                    // NEED TO DEFINE SOMETHING??
                    return failureTarget;
                }
            }
            if (!searchSuccessful) { cognitiveActionTarget = failureTarget; }
            return cognitiveActionTarget;
        }
    }
}
