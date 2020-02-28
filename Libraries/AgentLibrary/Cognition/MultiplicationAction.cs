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
    public class MultiplicationAction: CognitiveAction
    {
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

        // ToDo: Change so that only two operands are allowed ...
        // The (two) entries in the inputList must all be of type STMTag
        // The entry in the outputList should also be of that type.
        public override CognitiveActionTarget Process()
        {
          //  CognitiveActionTarget cognitiveActionTarget = new CognitiveActionTarget();
            double multiplicationResult = 1;
            List<string> operandsTagList = new List<string>();
            foreach (CognitiveActionParameter inputParameter in inputList)
            {
                operandsTagList.Add(inputParameter.Identifier);
            }
            DataItem operandsItem = ownerAgent.WorkingMemory.FindLastByTagList(operandsTagList);
            foreach (string operandTag in operandsTagList)
            {
                string valueString = operandsItem.GetStringValueByTag(operandTag);
                double value;
                Boolean ok = double.TryParse(valueString, out value);
                if (ok)
                {
                    multiplicationResult *= value;
                }
                else // Non-numerical input => transition to failure target
                {
                    return failureTarget;
                  //  cognitiveActionTarget = failureTarget;
                  //  return cognitiveActionTarget;
                }
            }
            string outputTag = outputList[0].Identifier;
            TagValueUnit resultTagValueUnit = new TagValueUnit(outputTag, multiplicationResult);
            ownerAgent.LastHistoryDataItem.ContentList.Add(resultTagValueUnit);
            return successTarget;
        }
    }
}
