using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Memories;

namespace AgentLibrary.Cognition
{
    [DataContract]
    public abstract class CognitiveAction
    {
        // 20190131
        protected List<CognitiveActionParameter> inputList;
        protected List<CognitiveActionParameter> outputList;
        protected CognitiveActionTarget successTarget;
        protected CognitiveActionTarget failureTarget;
         
        // Pointers
    //    protected List<ActionItem> targetItemList;
        protected Agent ownerAgent;
        protected CognitiveItem ownerItem;


        public CognitiveAction()
        {
            inputList = new List<CognitiveActionParameter>();
            outputList = new List<CognitiveActionParameter>();
            successTarget = null;
            failureTarget = null;
            List<CognitiveActionParameterType> requiredInputTypeList = GetRequiredParameterTypeInputList();
            if (requiredInputTypeList != null)
            {
                foreach (CognitiveActionParameterType type in requiredInputTypeList)
                {
                    inputList.Add(new CognitiveActionParameter(type, ""));
                }
            }
            List<CognitiveActionParameterType> requiredOutputTypeList = GetRequiredParameterTypeOutputList();
            if (requiredOutputTypeList != null)
            {
                foreach (CognitiveActionParameterType type in requiredOutputTypeList)
                {
                    outputList.Add(new CognitiveActionParameter(type, ""));
                }
            }
        }

        public abstract List<CognitiveActionParameterType> GetRequiredParameterTypeInputList();

        public abstract List<CognitiveActionParameterType> GetRequiredParameterTypeOutputList();

        public static List<Type> GetAllDerivedClasses()
        {
            List<Type> typeList = new List<Type>();
            Assembly assembly = Assembly.GetAssembly(typeof(CognitiveAction));
            List<Type> typeListInAssembly = assembly.GetTypes().Where(t => (t.IsSubclassOf(typeof(CognitiveAction)) && !t.IsAbstract)).ToList();
            if (typeListInAssembly.Count > 0)
            {
                typeList.AddRange(typeListInAssembly);
            }
            return typeList;
        }

        public void SetInputList(CognitiveActionParameterType type, List<string> tagList)
        {
            inputList = new List<CognitiveActionParameter>();
            foreach (string tag in tagList)
            {
                CognitiveActionParameter parameter = new CognitiveActionParameter(type, tag);
                inputList.Add(parameter);
            }
        }

        public void SetOutputList(CognitiveActionParameterType type, List<string> tagList)
        {
            outputList = new List<CognitiveActionParameter>();
            foreach (string tag in tagList)
            {
                CognitiveActionParameter parameter = new CognitiveActionParameter(type, tag);
                outputList.Add(parameter);
            }
        }

        public void Initialize(Agent ownerAgent, CognitiveItem ownerItem)
        {
            this.ownerAgent = ownerAgent;
            this.ownerItem = ownerItem;
            //    targetItemList = new List<ActionItem>();
            successTarget.Initialize(ownerAgent); // 20190131
            if (failureTarget != null)
            {
                failureTarget.Initialize(ownerAgent); // 20190131
            }
        }

        public void AddToShortTermMemory(List<TagValueUnit> tagValueUnitList, string predecessorID)
        {
            DataItem stmItem = new DataItem();
            if (predecessorID != null) { stmItem.PredecessorID = predecessorID; }
            long lastNumberedID = ownerAgent.WorkingMemory.GetLastDataItemID();
            long nextID = lastNumberedID + 1;
            stmItem.ID = Constants.STM_ITEM_PREFIX + nextID.ToString(Constants.STM_ID_FORMAT);
            TagValueUnit idTagValueUnit = new TagValueUnit(Constants.ACTION_ITEM_ID_TAG, this.ownerItem.ID);
            TagValueUnit sourceItemTypeTagValueUnit = new TagValueUnit(Constants.ACTION_ITEM_TYPE_TAG, Constants.COGNITIVE_ITEM_VALUE);
            tagValueUnitList.Insert(0, sourceItemTypeTagValueUnit);
            tagValueUnitList.Insert(0, idTagValueUnit);
            foreach (TagValueUnit tagValueUnit in tagValueUnitList)
            {
                stmItem.ContentList.Add(tagValueUnit);
            }
            ownerAgent.WorkingMemory.AddItem(stmItem);
        }

        public abstract CognitiveActionTarget Process();

        [DataMember]
        public List<CognitiveActionParameter> InputList
        {
            get { return inputList; }
            set { inputList = value; }
        }

        [DataMember]
        public List<CognitiveActionParameter> OutputList
        {
            get { return outputList; }
            set { outputList = value; }
        }

        [DataMember]
        public CognitiveActionTarget SuccessTarget
        {
            get { return successTarget; }
            set { successTarget = value; }
        }

        [DataMember]
        public CognitiveActionTarget FailureTarget
        {
            get { return failureTarget; }
            set { failureTarget = value; }
        }
    }
}
