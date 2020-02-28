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
    public class GetValueAction: CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
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
                CognitiveActionParameterType.WMTag
            };
            return requiredParameterTypeOutputList;
        }

        public override CognitiveActionTarget Process()
        {
            string stmTag = inputList[0].Identifier;
            DataItem stmItem = ownerAgent.WorkingMemory.FindLastByTag(stmTag);  // Finds the (last) item containing the tag
            DataItem ltmItem = ((List<DataItem>)stmItem.GetValueByTag(stmTag))[0]; // Finds the first (in this case the only) element from the list
            string stmGetValueTag = inputList[1].Identifier;
            string value = ltmItem.GetStringValueByTag(stmGetValueTag);
            string outputTag = outputList[0].Identifier;
            ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, value));
            return successTarget;
        }
    }
}
