using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AgentLibrary.Cognition;
using AgentLibrary.Memories; //Uses for the TagValueUnit


namespace AgentApplication.AddedClasses
{
    // Note to students:
    // You can use this file as a starting point when generating cognitive actions.
    // The three methods below must be implemented.

    [DataContract]
    public class GetCurrentTimeAction : CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>();
            //{     Does not need any input

            //};
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
            string hourOfToday = DateTime.Now.Hour.ToString("00");
            string minOfToday = DateTime.Now.Minute.ToString("00");
            string currentTime = hourOfToday + "." + minOfToday;

            string outputTag = outputList[0].Identifier;
            ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, currentTime));
            return successTarget;
        }
    }
}