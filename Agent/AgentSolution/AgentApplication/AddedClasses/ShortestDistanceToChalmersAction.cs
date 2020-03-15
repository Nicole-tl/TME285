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
    public class ShortestDistanceToChalmersAction: CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            List<CognitiveActionParameterType> requiredParameterTypeInputList = new List<CognitiveActionParameterType>()
            {
                CognitiveActionParameterType.WMTag,
                CognitiveActionParameterType.LTMTag,
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
            // Get the input string
            string wmTag = inputList[0].Identifier;
            string LTMTag = inputList[1].Identifier;
            DataItem dataFromWorkingMemory = ownerAgent.WorkingMemory.FindLastByTag(wmTag);
            string nameListWorkingMemory = dataFromWorkingMemory.GetStringValueByTag(wmTag);
            string dataNameInfo = nameListWorkingMemory.Split(new string[] { "|"}, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
            List<string> nameList = dataNameInfo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Get the geografic location of Chalmers 
            string myLTMTag = inputList[2].Identifier;
            DataItem dataOfChalmers = ownerAgent.LongTermMemory.FindLastByTag(myLTMTag);
            double myLatitude = Convert.ToDouble(dataOfChalmers.GetStringValueByTag("latitude"));
            double myLongitude = Convert.ToDouble(dataOfChalmers.GetStringValueByTag("longitude"));
            List<double> distances = new List<double>();
            double minDistance = 100;
            int positionOfMin = 0;
            int lenOfArray = nameList.Count;

            // Calculate the hypoteneus and save the shortest to positionOfMin
            for (int i = 0; i <= lenOfArray-1; i = i + 1) 
            {
                TagValueUnit nameTagValueUnit = new TagValueUnit("item", nameList[i]);
                List<DataItem> itemInfomation = ownerAgent.LongTermMemory.FindAll(new List<TagValueUnit>() { nameTagValueUnit });
                DataItem itemInfo = itemInfomation[0];
                string GeolocationOfItem = itemInfo.GetStringValueByTag(LTMTag);
                double LatitudeOfItem = Convert.ToDouble(GeolocationOfItem.Split(new string[] { "latitude\":", "," }, StringSplitOptions.RemoveEmptyEntries).ToList()[0]);
                double LongitudeOfItem = Convert.ToDouble(GeolocationOfItem.Split(new string[] { "longitude\":" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1]);
                double deltaLatitude = Math.Abs(LatitudeOfItem - myLatitude);
                double deltaLongitude = Math.Abs(LongitudeOfItem - myLongitude);
                double distanceForItem = Math.Sqrt(Math.Pow(deltaLatitude, 2) + Math.Pow(deltaLongitude, 2));
                distances.Add(distanceForItem);
                if (distanceForItem <= minDistance)
                {
                    minDistance = distanceForItem;
                    positionOfMin = i;
                }
                
            }
           
            string restaurantWithShortestDistance = nameList[positionOfMin];
            string outputTag = outputList[0].Identifier; 
            ownerAgent.LastHistoryDataItem.ContentList.Add(new TagValueUnit(outputTag, restaurantWithShortestDistance));
            return successTarget;
        }
    }
}
