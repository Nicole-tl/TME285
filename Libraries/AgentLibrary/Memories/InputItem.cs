using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AgentLibrary.Cognition;
using AgentLibrary.IO;
using AgentLibrary.Patterns;
using HumanLanguageLibrary.TextAnalysis;

namespace AgentLibrary.Memories
{
    [DataContract]
    public class InputItem: ActionItem
    {
        protected Boolean isEntryPoint = false;
        protected InputSource requiredInputSource;
        protected double minimumInputMatchScore = 1;
        protected List<InputAction> inputActionList;
        protected string requiredContext = null;
        protected string confusionHandlerOutputItemID = null;

        private OutputItem confusionHandlerOutputItem = null;

        private UserInput input;
        private double inputMatchScore = 0;
        private InputAction selectedAction = null;
        private ComplexityEstimator complexityEstimator;

        /// This is the default constructor, which initializes the InputActionList as an empty list,
        /// and sets the IsEntryPoint property to false. It also sets the MinimumMatchingScore to 1,
        /// the Context to null, and the ConfusionHandlerOutputItemID to null.
        public InputItem(): base()
        {
            inputActionList = new List<InputAction>();
            isEntryPoint = false;
            requiredInputSource = InputSource.Utterance;
            minimumInputMatchScore = 1;
            inputActionList = new List<InputAction>();
            confusionHandlerOutputItemID = null;
            confusionHandlerOutputItem = null;
            //     priority = 0;
         //   context = ""; // null;
        }

        /// This is the parameterized constructor that sets the id, the input action list
        /// (as a pointer to the corresponding input parameter, not a new instance), and the context.
        public InputItem(string id, List<InputAction> inputActionList, string contextAfter): base()
        {
            isEntryPoint = false;
         //   priority = 0;
            this.id = id;
            SplitID();
            this.inputActionList = inputActionList;
            this.contextAfter = contextAfter;
        }

        // ToDo: 20190401: Must rewrite this method: Should rank only the alternatives for input actions
        // that share a common targetItem (typically, there is only one such input action).
        private void RankPatternAlternatives()
        {
            List<PatternAlternative> completePatternAlternativeList = new List<PatternAlternative>();
            foreach (InputAction inputAction in this.InputActionList)
            {
                foreach (Pattern pattern in inputAction.PatternList)
                {
                    completePatternAlternativeList.AddRange(pattern.PatternAlternativeList);
                }
            }
            completePatternAlternativeList = completePatternAlternativeList.OrderBy(a => a.NonWildcardWordCount).ThenBy(b => b.NonWildcardSyllableCount).ToList();
            foreach (InputAction inputAction in this.InputActionList)
            {
                foreach (Pattern pattern in inputAction.PatternList)
                {
                    foreach (PatternAlternative patternAlternative in pattern.PatternAlternativeList)
                    {
                        int ranking = completePatternAlternativeList.FindIndex(a => a.ID == patternAlternative.ID);
                        patternAlternative.Ranking = ranking;  // Simple to complex
                    }
                }
            }
        }

        public override void Initialize(Agent ownerAgent)
        {
            base.Initialize(ownerAgent);
            SplitID();
            if (inputActionList.Count == 0) { this.endsDialogue = true; } // Not really relevant, since the property is (so far) only used for output items.
            foreach (InputAction inputAction in inputActionList)
            {
                inputAction.Initialize(ownerAgent);
            }
            int patternAlternativeID = 0;
            // 20190322: Set unique ID for each pattern alternative (needed for complexity matching)
            // 20190402: Perhaps change this 
            foreach (InputAction inputAction in inputActionList)
            {
                foreach (Pattern pattern in inputAction.PatternList)
                {
                    for (int ii = 0; ii < pattern.PatternAlternativeList.Count; ii++)
                    {
                        pattern.PatternAlternativeList[ii].ID = patternAlternativeID;
                        patternAlternativeID++;
                    }
                }
            }
            // 20190322: Rank the alternative in order of complexity:
            RankPatternAlternatives();
            // 20190322: Initialize complexity estimator
            complexityEstimator = new ComplexityEstimator();
            if (confusionHandlerOutputItemID != null)
            {
                confusionHandlerOutputItem = (OutputItem)ownerAgent.LongTermMemory.ItemList.Find(i => i.ID == confusionHandlerOutputItemID);
            }
        }

        /// This method simply sets the input, making it available for the Process method.
        public void SetInput(UserInput input)
        {
            this.input = input;
        }

        // The input must be processed so that, at this stage, it is in the form of
        // a string. However, this string can originate from text, voice, gesture etc.
        /// This method processes the input, but running through the list of input actions
        /// (of type InputAction) in order, checking for a match between the
        /// input (see SetInput) and the pattern specified in the InputAction.
        /// If a match is found, the method (i) adds an item in STM specifying the
        /// id and the type name (i.e. InputItem), as well as a list of tag value 
        /// units, one for each piece of dynamic information obtained from the
        /// Pattern.Match() method (see the Pattern class); (ii) sets the matching InputAction as the SelectedAction;
        /// (iii) adds another item in STM, specifying the id, the item type (InputItem),
        /// and the input (verbatim). The method then returns (i.e. it does not check
        /// input actions beyond the matching one. If no match is found, the SelectedAction is set to null.
        public override void Process()
        {
            selectedAction = null;

            Boolean contextMismatch = false;
            string context = ownerAgent.GetContext();

            if (requiredContext != null)
            {
                List<string> requiredContextList = requiredContext.Split(new char[] { Constants.CONTEXT_SPLIT_MARKER }, StringSplitOptions.RemoveEmptyEntries).ToList();
                contextMismatch = true;
                if (context != null)
                {
                    List<string> contextList = context.Split(new char[] { Constants.CONTEXT_SPLIT_MARKER }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (contextList.Count >= requiredContextList.Count)
                    {
                        contextMismatch = false;
                        for (int ii = 0; ii < requiredContextList.Count; ii++)
                        {
                            string requiredSubContext = requiredContextList[ii];
                            string subContext = contextList[ii];
                            if (subContext != requiredSubContext)
                            {
                                contextMismatch = true;
                                break;
                            }
                        }
                    }
                }
            }


            if (!contextMismatch)
            {
                foreach (InputAction inputAction in inputActionList)
                {
                    foreach (Pattern pattern in inputAction.PatternList)
                    {
                        //   Pattern pattern = inputAction.Pattern;
                        string message = input.Message;
                        double complexity = complexityEstimator.ComputeFleschKincaidScore(message);  // 20190322
                        List<TagValueUnit> dynamicInformationList;
                        double matchingScore = pattern.Match(message, out dynamicInformationList);
                        if (matchingScore == 1) // >= minimumInputMatchScore)
                        {
                            ownerAgent.LastProcessedInputItem = this;
                            if (ownerAgent.AdaptComplexity) // 20190322
                            {
                                TagValueUnit complexityUnit = new TagValueUnit(Constants.USER_SENTENCE_COMPLEXITY_TAG, complexity);
                                dynamicInformationList.Add(complexityUnit);
                                double emaComplexity = complexity;
                                DataItem emaComplexityItem = ownerAgent.WorkingMemory.FindLastByTag(Constants.USER_EMA_SENTENCE_COMPLEXITY_TAG);
                                if (emaComplexityItem != null)
                                {
                                    emaComplexity = (1 - ownerAgent.ComplexityEMAParameter) * emaComplexityItem.GetTagValueUnit(Constants.USER_EMA_SENTENCE_COMPLEXITY_TAG).Value +
                                        ownerAgent.ComplexityEMAParameter * complexity;
                                }
                                TagValueUnit emaComplexityUnit = new TagValueUnit(Constants.USER_EMA_SENTENCE_COMPLEXITY_TAG, emaComplexity);
                                dynamicInformationList.Add(emaComplexityUnit);
                            }
                            DataItem historyDataItem = GenerateHistoryItem(message, Constants.INPUT_ITEM_VALUE, this, dynamicInformationList);
                            ownerAgent.WorkingMemory.AddItem(historyDataItem);
                            ownerAgent.LastHistoryDataItem = historyDataItem;
                            selectedAction = inputAction;
                            return;
                            // break;   // Changed 20190402 (break => return, since break would only exit the inner loop).
                        }
                    }
                }
            }
        }

        [DataMember]
        public Boolean IsEntryPoint
        {
            get { return isEntryPoint; }
            set { isEntryPoint = value; }
        }

        [DataMember]
        public List<InputAction> InputActionList
        {
            get { return inputActionList; }
            set { inputActionList = value; }
        }

        [DataMember]
        public InputSource RequiredInputSource
        {
            get { return requiredInputSource; }
            set { requiredInputSource = value; }
        }

        [DataMember]
        public string RequiredContext
        {
            get { return requiredContext; }
            set { requiredContext = value; }
        }   

        [DataMember]
        public string ConfusionHandlerOutputItemID
        {
            get { return confusionHandlerOutputItemID; }
            set { confusionHandlerOutputItemID = value; }
        }

        public InputAction SelectedAction
        {
            get { return selectedAction; }
        }

        public OutputItem ConfusionHandlerOutputItem
        {
            get { return confusionHandlerOutputItem; }
        }
    }
}
