using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Cognition;
using AgentLibrary.Memories;

namespace AgentApplication.AddedClasses
{
    public class TimeAction: CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            return new List<CognitiveActionParameterType>();
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
            string outputWMTag = outputList[0].Identifier;
            DateTime time = DateTime.Now;
            string hour = time.Hour.ToString();
            string minute = time.Minute.ToString();
            string timeString = hour + " " + minute;
            ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputWMTag, timeString));
            return successTarget;
        }
    }
}
