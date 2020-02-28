using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentLibrary.IO;
using AgentLibrary.EventArgsClasses;
using AgentLibrary.Patterns;
using HumanLanguageLibrary.TextAnalysis;
//using TextProcessingLibrary;

namespace AgentLibrary.Memories
{
    [DataContract]
    public class OutputItem: ActionItem
    {
        private List<string> dynamicInformationList;
        private List<Pattern> outputPatternList;
        private string targetItemID;
        private MemoryItem targetItem;
        private Boolean suppressOutputRepetition = false;
        private OutputDestination outputDestination = OutputDestination.Speech;

        private ComplexityEstimator complexityEstimator;

        public event EventHandler<AgentOutputEventArgs> ProcessCompleted = null;

        protected void OnProcessCompleted(AgentOutput agentOutput)
        {
            if (ProcessCompleted != null)
            {
                EventHandler<AgentOutputEventArgs> handler = ProcessCompleted;
                AgentOutputEventArgs e = new AgentOutputEventArgs(agentOutput);
                handler(this, e);
            }
        }

        private Thread processThread;

        public OutputItem(): base()
        {
            outputPatternList = new List<Pattern>();
            outputDestination = OutputDestination.Speech;
        }

        public OutputItem(string id, List<Pattern> outputPatternList, string targetItemID, string context): base()
        {
            this.ID = id;
            SplitID();
            this.outputPatternList = outputPatternList;
            this.targetItemID = targetItemID;
            this.contextAfter = context;
            this.endsDialogue = true;
        }

        public void SetDynamicInformation(List<string> dynamicInformationList)
        {
            this.dynamicInformationList = dynamicInformationList;
        }

        public override void Initialize(Agent ownerAgent)
        {
            base.Initialize(ownerAgent);
            SplitID();
            targetItem = ownerAgent.LongTermMemory.ItemList.Find(i => i.ID == targetItemID);
            foreach (Pattern pattern in outputPatternList)
            {
                pattern.Initialize(ownerAgent.RandomNumberGenerator);
            }

            // 20190626, test:
            this.endsDialogue = true;

            if (targetItemID != null)
            {
                this.endsDialogue = false;
            }
            complexityEstimator = new ComplexityEstimator(); // 20190322
        }

        public override void Process()
        {
            processThread = new Thread(new ThreadStart(ProcessLoop));
            processThread.Start();
        }  

        // 20190322
        private void SelectComplexityMatchingAlternative(double userSentenceComplexity, out int patternIndex, out string outputSentence, out double agentSentenceComplexity)
        {
            List<Tuple<int, string, double>> patternComplexityTupleList = new List<Tuple<int, string, double>>();  // first index: pattern index, second index: sentence, third index: complexity
            List<TagValueUnit> dynamicInformationList = new List<TagValueUnit>();
            List<string> completeTagList = new List<string>();
            foreach (Pattern pattern in outputPatternList)
            {
                List<string> outputTagList = pattern.GetTagList().Except(completeTagList).ToList();
                foreach (string outputTag in outputTagList)
                {
                    completeTagList.Add(outputTag);
                    string processedOutputTag = BracketRemover.Process(outputTag);   // 20190201: Remove < >
                    DataItem dynamicInformationItem = ownerAgent.WorkingMemory.FindLastByTag(processedOutputTag);
                    TagValueUnit tagValueUnit = dynamicInformationItem.GetTagValueUnit(processedOutputTag);
                    dynamicInformationList.Add(tagValueUnit);
                }
            }
            for (int ii = 0; ii < outputPatternList.Count; ii++)
            {
                Pattern pattern = outputPatternList[ii];
                List<string> alternativeList = pattern.GenerateAllOutputAlternatives(dynamicInformationList);
                for (int jj = 0; jj < alternativeList.Count; jj++)
                {
                    string alternative = alternativeList[jj];
                    double complexity = complexityEstimator.ComputeFleschKincaidScore(alternative);
                    patternComplexityTupleList.Add(new Tuple<int, string, double>(ii, alternative, complexity));
                }
            }
            Tuple<int, string, double> patternComplexityTuple = patternComplexityTupleList.OrderBy(c => Math.Abs(c.Item3 - userSentenceComplexity)).First();
            outputSentence = patternComplexityTuple.Item2;
            patternIndex = patternComplexityTuple.Item1;
            agentSentenceComplexity = patternComplexityTuple.Item3;
        }

        public void ProcessLoop()
        {
            string output;
            List<TagValueUnit> dynamicInformationList = new List<TagValueUnit>();
            DataItem userComplexityItem = null;
            TagValueUnit complexityUnit = null;
            if (ownerAgent.AdaptComplexity)
            {
                userComplexityItem = ownerAgent.WorkingMemory.FindLastByTag(Constants.USER_SENTENCE_COMPLEXITY_TAG);
            }
            if ((!ownerAgent.AdaptComplexity) || (ownerAgent.AdaptComplexity && (userComplexityItem == null)))
            {
                int selectedPatternIndex = ownerAgent.RandomNumberGenerator.Next(0, outputPatternList.Count);
                Pattern selectedPattern = outputPatternList[selectedPatternIndex];
                if (!selectedPattern.UseVerbatim)
                {
                    List<string> outputTagList = selectedPattern.GetTagList();
                    dynamicInformationList = new List<TagValueUnit>();
                    foreach (string outputTag in outputTagList)
                    {
                        string processedOutputTag = BracketRemover.Process(outputTag);   // 20190201: Remove < >
                        DataItem dynamicInformationItem = ownerAgent.WorkingMemory.FindLastByTag(processedOutputTag);
                        TagValueUnit tagValueUnit = dynamicInformationItem.GetTagValueUnit(processedOutputTag);
                        dynamicInformationList.Add(tagValueUnit);
                    }
                    output = selectedPattern.GenerateOutput(dynamicInformationList);
                }
                else { output = selectedPattern.PatternSpecification; }  // 20191203
            }
            else
            {
                // 20190322
                int selectedPatternIndex;
                TagValueUnit userComplexityTagValueUnit = userComplexityItem.GetTagValueUnit(Constants.USER_SENTENCE_COMPLEXITY_TAG);
                double userSentenceComplexity = userComplexityTagValueUnit.Value;
                double agentSentenceComplexity;

                // 20191203: Note: Might crash here for verbatim patterns (useVerbatim = true), but
                // it does not matter, since complexity matching is not currently used.

                SelectComplexityMatchingAlternative(userSentenceComplexity, out selectedPatternIndex, out output, out agentSentenceComplexity);
                complexityUnit = new TagValueUnit(Constants.AGENT_SENTENCE_COMPLEXITY_TAG, agentSentenceComplexity);
                // Now compute the dynamic information list:
                Pattern selectedPattern = outputPatternList[selectedPatternIndex];
                if (!selectedPattern.UseVerbatim)
                {
                    List<string> outputTagList = selectedPattern.GetTagList();
                    dynamicInformationList = new List<TagValueUnit>();
                    foreach (string outputTag in outputTagList)
                    {
                        string processedOutputTag = BracketRemover.Process(outputTag);   // 20190201: Remove < >
                        DataItem dynamicInformationItem = ownerAgent.WorkingMemory.FindLastByTag(processedOutputTag);
                        TagValueUnit tagValueUnit = dynamicInformationItem.GetTagValueUnit(processedOutputTag);
                        dynamicInformationList.Add(tagValueUnit);
                    }
                }
            }

            // 20190322
            if (complexityUnit != null)
            {
                dynamicInformationList.Add(complexityUnit);
            }
            DataItem historyDataItem = GenerateHistoryItem(output, Constants.OUTPUT_ITEM_VALUE, this, dynamicInformationList);
            ownerAgent.WorkingMemory.AddItem(historyDataItem);
            ownerAgent.LastHistoryDataItem = historyDataItem;

            if (this.ContextAfter != null)  // 20190103: change this only of the context is != null; see TestAgent10
            {
                ownerAgent.NextExpectedInputItem = null;
            }
            if (this.targetItem is InputItem) { ownerAgent.NextExpectedInputItem = (InputItem)this.targetItem; }    
            AgentOutput agentOutput = new AgentOutput(outputDestination, output);  
            OnProcessCompleted(agentOutput);
        }

        [DataMember]
        public List<Pattern> OutputPatternList
        {
            get { return outputPatternList; }
            set { outputPatternList = value; }
        }

        [DataMember]
        public string TargetItemID
        {
            get { return targetItemID; }
            set { targetItemID = value; }
        }

        [DataMember]
        public Boolean SuppressOutputRepetition
        {
            get { return suppressOutputRepetition; }
            set { suppressOutputRepetition = value; }
        }

        [DataMember]
        public OutputDestination OutputDestination
        {
            get { return outputDestination; }
            set { outputDestination = value; }
        }

        public MemoryItem TargetItem
        {
            get { return targetItem; }
        }
    }
}
