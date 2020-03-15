using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AgentLibrary.Cognition;
using AgentLibrary.Memories;

namespace AgentApplication.AddedClasses
{

    [DataContract]
    public class GetCurrentWeekdayAction : CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>();
            return requiredParameterTypeInputList;

        }
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeOutputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeOutputList = new List<CognitiveActionParameterType>()
            { CognitiveActionParameterType.WMTag
            };
            return requiredParameterTypeOutputList;

        }

        public override CognitiveActionTarget Process()
        {
            string weekday = DateTime.Now.DayOfWeek.ToString();
            string outputTag = outputList[0].Identifier;
            ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, weekday));
            return successTarget;
           
        }
    }
}

