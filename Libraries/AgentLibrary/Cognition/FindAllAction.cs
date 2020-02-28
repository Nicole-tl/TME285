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
    public class FindAllAction: CognitiveAction
    {
        // This action finds all LTM items in a given category.
        // Thus, it finds all items containing a
        // tagValueUnit with tag = category and value = categoryValue,
        // where category value is the Identifier of inputList[0]
        // (which must be of type LTMCategory).
        //
        // Note: Items that have several category tags are selected
        // as well, provided that at least one of the corresponding
        // values match the categoryValue.
        //
        // A list of (pointers to) the items found is then added to
        // STM, with the tag specified as the Identifier of outputList[0].
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.LTMCategory
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
            string categoryValue = inputList[0].Identifier;
            List<TagValueUnit> tagValueUnitList = new List<TagValueUnit>() { new TagValueUnit(Constants.CATEGORY_TAG, categoryValue) };
            List<DataItem> searchResult = ownerAgent.LongTermMemory.FindAll(tagValueUnitList);
            string outputWMTag = outputList[0].Identifier;
            ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputWMTag, searchResult));
            return successTarget;
        }
    }
}
