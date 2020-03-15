using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using AgentLibrary;
using AgentLibrary.Memories;
using AgentLibrary.Cognition;


namespace AgentApplication.AddedClasses
{
    [DataContract]
    public class GenerateDisplayTextPathAction : CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
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
            string fileNameTag = inputList[0].Identifier;
            string fileNameValue = ownerAgent.WorkingMemory.FindLastByTag(fileNameTag).GetStringValueByTag(fileNameTag);

            string generatedFilePath = "";
            if (fileNameValue != null)
            {
                // CREATE NEW AND CHANGE THIS LINE 
                generatedFilePath = "Text" + Constants.DISPLAY_OUTPUT_SEPARATOR + fileNameValue;
                string outputFilePathTag = outputList[0].Identifier;
                ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputFilePathTag, generatedFilePath));
                return successTarget;
            }
            return failureTarget;
        }
    }
}
