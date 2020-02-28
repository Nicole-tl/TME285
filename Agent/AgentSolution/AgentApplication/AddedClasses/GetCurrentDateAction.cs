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
    public class GetCurrentDateAction : CognitiveAction
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
            string day = DateTime.Now.Day.ToString("00");
            string month = DateTime.Now.Month.ToString("00");
            string year = DateTime.Now.Year.ToString("00");
            string currentDate = year + "." + month + "." + day;
            string outputTag = outputList[0].Identifier;
            ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, currentDate));
            return successTarget;
        }
    }
}
