using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentLibrary.Cognition;
using AgentLibrary.EventArgsClasses;
using AgentLibrary.IO;
using AgentLibrary.Memories;
using AgentLibrary.Patterns;
using AuxiliaryLibrary;

namespace AgentLibrary
{
    [DataContract]
    public sealed class Agent
    {
        private const double DEFAULT_COMPLEXITY_EMA_PARAMETER = 0.5;

        private string name = "UnnamedAgent";

        private Memory workingMemory;
        private Memory longTermMemory;

        private List<InputItem> inputItemList; // Pointer to all input items
        private InputItem nextExpectedInputItem; // Pointer to the target of the most recent output item (often null)
        private InputItem lastProcessedInputItem;
        private OutputItem defaultConfusionHandlerOutputItem;
        private OutputItem lastOutputItemInContext; // Pointer to the most recent output in the current context, if context != null
        private DataItem contextItem;
        private DataItem lastHistoryDataItem;
    //    private DataItem dialogueHistoryItem;
        private List<DataItem> dataItemList; // Pointer to all data items
        private Boolean useSpeech = true;

        private Boolean dialogueLocked = false;  // Find a way to use this variable, to avoid confusion between dialogues...
        private Random randomNumberGenerator;

        private Thread inputProcessingThread;

        public event EventHandler<StringEventArgs> TextOutputGenerated = null;
        public event EventHandler<StringEventArgs> SpeechOutputGenerated = null;
        public event EventHandler<StringEventArgs> MovementGenerated = null;  // 20191112
        public event EventHandler<StringEventArgs> DisplayOutputGenerated = null; // 20191114
        public event EventHandler MemoryViewUpdateNeeded = null;

        // 20190322
        private Boolean adaptComplexity = false;
        private double complexityEMAParameter = DEFAULT_COMPLEXITY_EMA_PARAMETER;

        // 20191112
        private string startItemID = null;

        public Agent()
        {
            longTermMemory = new Memory();
            longTermMemory.Name = Constants.LTM_NAME;
        }

        // Perhaps place this in its own thread, in case there are many items to process?
        public void Initialize()
        {
            randomNumberGenerator = new Random();
            workingMemory = new Memory();
            workingMemory.Name = Constants.STM_NAME;

         /*   DataItem contextItem = new DataItem();
            contextItem.ID = "s0000000";
            this.contextItem = contextItem;  
            shortTermMemory.AddItem(contextItem);  */
         /*   DataItem dialogueHistoryItem = new DataItem();
            dialogueHistoryItem.ID = "s0000001";
            this.dialogueHistoryItem = dialogueHistoryItem;
            shortTermMemory.AddItem(dialogueHistoryItem);  */

            // MW ToDo: Add ContextItem here.

          //  dynamicInformationItem = new StorageItem();
          //  dynamicInformationItem.ID = "s0000001";
          //  shortTermMemory.ItemList.Add(dynamicInformationItem);
            longTermMemory.Initialize(); 

            inputItemList = new List<InputItem>();
            dataItemList = new List<DataItem>();
            foreach (MemoryItem item in longTermMemory.ItemList)
            {
                item.Initialize(this);
                if ((item.GetType() == typeof(InputItem)) || item.GetType().IsSubclassOf(typeof(InputItem)))
                {
                    inputItemList.Add((InputItem)item);
                }
                else if ((item.GetType() == typeof(CognitiveItem)) || item.GetType().IsSubclassOf(typeof(CognitiveItem)))
                {
                    ((CognitiveItem)item).ProcessCompleted += new EventHandler(HandleCognitiveProcessingCompleted);
                }
                else if ((item.GetType() == typeof(OutputItem)))
                {
                    ((OutputItem)item).ProcessCompleted += new EventHandler<AgentOutputEventArgs>(HandleOutputProcessingCompleted);
                }
                else if (item.GetType() == typeof(DataItem))
                {
                    dataItemList.Add((DataItem)item);
                }
            }
        //    inputItemList.Sort((i1, i2) => i2.Priority.CompareTo(i1.Priority));

            // Set the default error handler, if any
            defaultConfusionHandlerOutputItem = null;
            DataItem defaultErrorHandlerDataItem = longTermMemory.FindLastByTag(Constants.DEFAULT_CONFUSION_HANDLER_TAG);
            if (defaultErrorHandlerDataItem != null)
            {
                string defaultErrorHandlerOutputItemID = defaultErrorHandlerDataItem.GetStringValueByTag(Constants.DEFAULT_CONFUSION_HANDLER_TAG);
                defaultConfusionHandlerOutputItem = (OutputItem)longTermMemory.ItemList.Find(i => i.ID == defaultErrorHandlerOutputItemID);
            }
        }

        public void Start()
        {
            Initialize();
            workingMemory.ItemList = new List<MemoryItem>();
            workingMemory.DataItemList = new List<DataItem>();
            if (longTermMemory == null) { return; }
            if (startItemID != null)
            {
                MemoryItem item = longTermMemory.ItemList.Find(i => i.ID == startItemID);
                if (item != null)
                {
                    if (item.GetType() == typeof(OutputItem))
                    {
                        ((OutputItem)item).Process();
                    }
                }
            }
        }

        private void OnMemoryViewUpdateNeeded()
        {
            if (MemoryViewUpdateNeeded != null)
            {
                EventHandler handler = MemoryViewUpdateNeeded;
                handler(this, EventArgs.Empty);
            }
        }

        private void HandleOutputProcessingCompleted(object sender, AgentOutputEventArgs e)
        {
            OutputItem lastOutputItem = (OutputItem)sender;
            string message = e.AgentOutput.Message;
            OutputDestination destination = e.AgentOutput.Destination;
            if (destination == OutputDestination.Speech)
            {
                OnTextOutputGenerated(message);
                if (useSpeech)
                {
                    OnSpeechOutputGenerated(message);
                }
                OnMemoryViewUpdateNeeded();
            }
            else if (destination == OutputDestination.Animation)
            {
                OnMovementGenerated(message);
                OnMemoryViewUpdateNeeded();
            }
            else if (destination == OutputDestination.Display)
            {
                OnDisplayOutputGenerated(message);
                OnMemoryViewUpdateNeeded();
            }
            // Add support for face and gesture output here:

            // 20190814 test
            if (lastOutputItem.TargetItem is OutputItem)
            {
                ((OutputItem)lastOutputItem.TargetItem).Process();
            }  

            // 20190626 test
            if (lastOutputItem.EndsDialogue)
            {
                string context = GetContext();
                if (context == null)  // 20191128: Changed from != to ==   CORRECT?
                {
                    nextExpectedInputItem = null;
                }
            }

            // Next, check if the context of the last output item is null
            // AND the context of the previous output item was != null,
            // then repeat the previous output item, provided that
            // output repetition has not been suppressed.
            if (lastOutputItem.ContextAfter == null)
            {
                if (!lastOutputItem.SuppressOutputRepetition)
                {
                    string previousID;
                    if (lastHistoryDataItem.PredecessorID != null)
                    {
                        previousID = lastHistoryDataItem.PredecessorID;
                        DataItem historyDataItem = (DataItem)workingMemory.ItemList.Find(i => i.ID == previousID);
                        Boolean endReached = false;
                        while (!endReached)
                        {
                            string itemType = historyDataItem.GetStringValueByTag(Constants.ACTION_ITEM_TYPE_TAG);
                            if (itemType == Constants.OUTPUT_ITEM_VALUE)
                            {
                                string previousOutputItemID = historyDataItem.GetStringValueByTag(Constants.ACTION_ITEM_ID_TAG);
                                OutputItem previousOutputItem  = (OutputItem)longTermMemory.ItemList.Find(i => i.ID == previousOutputItemID);
                                if (previousOutputItem.ContextAfter != null)
                                {
                                    if (!previousOutputItem.EndsDialogue)
                                    {
                                        previousOutputItem.Process();
                                        break;
                                    }
                                    else { return; }
                                }
                                else { return; }
                            }
                            if (historyDataItem.PredecessorID == null) { endReached = true; }
                            else
                            {
                                previousID = historyDataItem.PredecessorID;
                                historyDataItem = (DataItem)workingMemory.ItemList.Find(i => i.ID == previousID);
                                if (historyDataItem == null) { endReached = true; }
                            }
                        }
                    }
                }
            }  
        }

        // MW ToDo: Handle the case where the TargetItem is null.
        private void HandleCognitiveProcessingCompleted(object sender, EventArgs e)
        {
            CognitiveItem processedItem = (CognitiveItem)sender;
            ActionItem item = processedItem.TargetItem;
            if (item != null)
            {
                item.Process();
            }
        }

        private Boolean IsContextEmpty()
        {
            if (contextItem.ContentList.Count == 0) { return true; }
            else { return false; }
        }

        public void ProcessInput(UserInput input)
        {
            inputProcessingThread = new Thread(new ThreadStart(() => ProcessInputThread(input)));
            inputProcessingThread.Start();
        }

        public void ProcessInputThread(UserInput input)
        {
        //    Boolean runConfusionItem = false;
            if (nextExpectedInputItem != null)
            {
                if (nextExpectedInputItem.RequiredInputSource == input.Source)
                {
                    nextExpectedInputItem.SetInput(input);
                    nextExpectedInputItem.Process();
                    if (nextExpectedInputItem.SelectedAction != null)
                    {
                        ActionItem targetItem = nextExpectedInputItem.SelectedAction.TargetItem;
                        targetItem.Process();
                        return;
                    }
                } 
            }
            foreach (InputItem item in inputItemList)
            {
                if (item.IsEntryPoint)
                {
                    if (item.RequiredInputSource == input.Source)
                    {
                        item.SetInput(input);
                        item.Process();
                        if (item.SelectedAction != null)
                        {
                            OnMemoryViewUpdateNeeded();
                            ActionItem targetItem = item.SelectedAction.TargetItem;
                            targetItem.Process();
                            return;
                        }
                    }
                }
            }
            // Run the confusion item (if true) here instead, so that the agent
            // has a chance to match other inputs (e.g. a repetition request).
            if (nextExpectedInputItem != null)
            {
                if (nextExpectedInputItem.RequiredInputSource == input.Source)
                {
                    if (nextExpectedInputItem.ConfusionHandlerOutputItem != null)
                    {
                        OnMemoryViewUpdateNeeded();
                        nextExpectedInputItem.ConfusionHandlerOutputItem.Process();
                        return;
                    }
                }
            }
            if (defaultConfusionHandlerOutputItem != null)
            {
                if (input.Source == InputSource.Utterance) // Do not run default confusion handling for vision input.
                {
                    defaultConfusionHandlerOutputItem.Process();
                }
            }
        }

        // MW ToDo: if context != empty, check only the alwaysAvailable items. 
        public void OLDProcessInput(UserInput input)
        {
            if (!dialogueLocked)
            {
                if (inputItemList != null)
                {
                    Boolean contextEmpty = IsContextEmpty();
                    if (contextEmpty)
                    {
                        foreach (InputItem item in inputItemList)
                        {
                      //      if (item.IsEntryPoint)
                            {
                                item.SetInput(input);
                                item.Process();
                                if (item.SelectedAction != null)
                                {
                                    ActionItem targetItem = item.SelectedAction.TargetItem;
                                    targetItem.Process();
                                    break;
                                }
                            }
                        }
                    }
                    else 
                    {
                        // Ongoing dialogue: First check for a match with the ongoing dialogue...
                        string currentContext = contextItem.GetStringValueByTag("<context>");
                   
                        // OutputItem lastOutputItem = GetLastOutputItemBy
                        
                        //     OutputItem lastOutputItem = dialogueHistoryItem.
                       
                            
                            
                        if (nextExpectedInputItem != null)
                        {
                            nextExpectedInputItem.SetInput(input);
                            nextExpectedInputItem.Process();
                            if (nextExpectedInputItem.SelectedAction != null)
                            {
                                ActionItem targetItem = nextExpectedInputItem.SelectedAction.TargetItem;
                                targetItem.Process();
                                return;
                            }
                        }

                        foreach (InputItem item in inputItemList)
                        {
                            if (item.IsEntryPoint)
                            {
                                if (item.ContextAfter == null)  // Only check context-free dialogues until the current dialogue has been abandoned.
                                {
                                    item.SetInput(input);
                                    item.Process();
                                    if (item.SelectedAction != null)
                                    {
                                        ActionItem targetItem = item.SelectedAction.TargetItem;
                                        targetItem.Process();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnTextOutputGenerated(string message)
        {
            if (TextOutputGenerated != null)
            {
                EventHandler<StringEventArgs> handler = TextOutputGenerated;
                StringEventArgs e = new StringEventArgs(message);
                handler(this, e);
            }
        }

        private void OnSpeechOutputGenerated(string message)
        {
            if (SpeechOutputGenerated != null)
            {
                EventHandler<StringEventArgs> handler = SpeechOutputGenerated;
                StringEventArgs e = new StringEventArgs(message);
                handler(this, e);
            }
        }

        private void OnMovementGenerated(string message)
        {
            if (MovementGenerated != null)
            {
                EventHandler<StringEventArgs> handler = MovementGenerated;
                StringEventArgs e = new StringEventArgs(message);
                handler(this, e);
            }
        }

        private void OnDisplayOutputGenerated(string message)
        {
            if (DisplayOutputGenerated != null)
            {
                EventHandler<StringEventArgs> handler = DisplayOutputGenerated;
                StringEventArgs e = new StringEventArgs(message);
                handler(this, e);
            }
        }

        /*  public void ProcessOutput(AgentOutput output, OutputItem processedItem)
          {
              OutputDestination destination = output.Destination;
              string message = output.Message;
              if (destination == OutputDestination.Speech)
              {
                  OnTextOutputGenerated(message); // Text is always sent. (perhaps make it possible to turn off?
              }
              else if (destination == OutputDestination.Speech)
              {
                  OnSpeechOutputGenerated(message);
              }
              if (lastProcessedInputItem.Context == null)
              {
                  Boolean contextEmpty = IsContextEmpty();
                  if (!contextEmpty)
                  {
                      if (lastOutputItemInContext != null)
                      {
                          lastOutputItemInContext.Process();
                      }
                  }
              }
          }  */

        /// Adds an LTM item, pointing to the ID of the 
        /// default confusion handler (action) item, which must
        /// be an output item.
        public void SetDefaultConfusionHandler(string id)
        {
            long lastNumberedID = longTermMemory.GetLastDataItemID();
            long nextNumberedID = lastNumberedID + 1;
            DataItem defaultConfusionHandlerItem = new DataItem();
            defaultConfusionHandlerItem.ID = Constants.LTM_ITEM_PREFIX + nextNumberedID.ToString(Constants.LTM_ID_FORMAT);
            TagValueUnit tagValueUnit = new TagValueUnit(Constants.DEFAULT_CONFUSION_HANDLER_TAG, id);
            defaultConfusionHandlerItem.ContentList.Add(tagValueUnit);
            longTermMemory.AddItem(defaultConfusionHandlerItem);
        }

        public Boolean CheckConsistency(out List<string> report)
        {
            report = new List<string>();
            Boolean consistent = true;
            List<string> idList = new List<string>();
            List<ActionItem> actionItemList = new List<ActionItem>();
            List<string> targetIDList = new List<string>();
            List<string> dynamicInformationList = new List<string>();
            foreach (MemoryItem item in longTermMemory.ItemList)
            {
                idList.Add(item.ID);
                if (item.GetType().IsSubclassOf(typeof(ActionItem)))
                {
                    actionItemList.Add((ActionItem)item);
                    if (item is InputItem)
                    {
                        InputItem inputItem = (InputItem)item;
                        foreach (InputAction inputAction in inputItem.InputActionList)
                        {
                            if (!targetIDList.Contains(inputAction.TargetItemID))
                            {
                                if (inputAction.TargetItemID != null)
                                {
                                    targetIDList.Add(inputAction.TargetItemID);
                                }
                                foreach (Pattern pattern in inputAction.PatternList)
                                {
                                    List<string> patternDynamicInformationList = pattern.GetDynamicInformation();
                                    dynamicInformationList.AddRange(patternDynamicInformationList);
                                }
                            }
                        }
                        if (inputItem.ConfusionHandlerOutputItemID != null)
                        {
                            MemoryItem confusionHandlerItem = longTermMemory.ItemList.Find(t => t.ID == inputItem.ConfusionHandlerOutputItemID);
                            if (confusionHandlerItem == null)
                            {
                                report.Add("[Error]   Item " + inputItem.ConfusionHandlerOutputItemID + " used in input item " + inputItem.ID + " is not defined.");
                                consistent = false;
                            }
                            else
                            {
                                targetIDList.Add(inputItem.ConfusionHandlerOutputItemID);
                            }
                        }
                    }
                    else if (item is CognitiveItem)
                    {
                        CognitiveItem cognitiveItem = (CognitiveItem)item;
                        int index = 0;
                        foreach (CognitiveAction cognitiveAction in cognitiveItem.CognitiveActionList)
                        {
                            if (cognitiveAction.SuccessTarget == null)
                            {
                                report.Add("[Error]   Cognitive action " + index.ToString() + " in cognitive item " + cognitiveItem.ID + " lacks success target");
                                consistent = false;
                            }
                            else
                            {
                                string successTargetID = cognitiveAction.SuccessTarget.TargetItemID;
                                if (!targetIDList.Contains(successTargetID)) { targetIDList.Add(successTargetID); }
                            }
                            if (cognitiveAction.FailureTarget == null)
                            {
                                report.Add("[Warning] Cognitive action " + index.ToString() + " in cognitive item " + cognitiveItem.ID + " lacks failure target");
                            }
                            else
                            {
                                string failureTargetID = cognitiveAction.FailureTarget.TargetItemID;
                                if (!targetIDList.Contains(failureTargetID)) { targetIDList.Add(failureTargetID); }
                            }
                            foreach (CognitiveActionParameter parameter in cognitiveAction.OutputList)
                            {
                                if (parameter.Type == CognitiveActionParameterType.WMTag)
                                {
                                    dynamicInformationList.Add(parameter.Identifier);
                                }
                            }
                            index++;
                        }
                    }
                    else  // Must be OutputItem
                    {
                        OutputItem outputItem = (OutputItem)item;
                        if (!targetIDList.Contains(outputItem.TargetItemID))
                        {
                            if (outputItem.TargetItemID != null)
                            {
                                targetIDList.Add(outputItem.TargetItemID);
                            }
                        }
                    }
                }
            }
            // Is there a default confusion handler:
            string defaultConfusionItemID = null;
            DataItem confusionHandlerIdentifierItem = longTermMemory.FindLastByTag(Constants.DEFAULT_CONFUSION_HANDLER_TAG);
            if (confusionHandlerIdentifierItem != null)
            {
                defaultConfusionItemID = confusionHandlerIdentifierItem.GetStringValueByTag(Constants.DEFAULT_CONFUSION_HANDLER_TAG);
                targetIDList.Add(defaultConfusionItemID);
            }

            List<string> duplicateIDList = idList.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateIDList.Count > 0)
            {
                consistent = false;
                foreach (string duplicateID in duplicateIDList)
                {
                    report.Add("[Error]   Multiple copies of ID = " + duplicateID + " Found. ");
                }
            }
            foreach (ActionItem actionItem in actionItemList)
            {
                if (actionItem is InputItem)
                {
                    InputItem inputItem = (InputItem)actionItem;
                    if (!inputItem.IsEntryPoint)
                    {
                        int index = targetIDList.IndexOf(inputItem.ID);
                        if (index < 0)
                        {
                            consistent = false;
                            report.Add("[Error]   Item with ID = " + inputItem.ID + " is unreachable.");
                        }
                    }
                }
                else
                {
                    if (actionItem is CognitiveItem)
                    {
                        CognitiveItem cognitiveItem = (CognitiveItem)actionItem;
                        foreach (CognitiveAction cognitiveAction in cognitiveItem.CognitiveActionList)
                        {
                            foreach (CognitiveActionParameter parameter in cognitiveAction.InputList)
                            {
                                if (parameter.Type == CognitiveActionParameterType.WMTag)
                                {
                                    if (!dynamicInformationList.Contains(parameter.Identifier))
                                    {
                                        if (!dynamicInformationList.Contains(parameter.Identifier + "+"))
                                        {
                                            consistent = false;
                                            report.Add("[Error]   dynamic information = " + parameter.Identifier + " in " + cognitiveItem.ID + " will not be available in WM");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (actionItem is OutputItem)
                    {
                        OutputItem outputItem = (OutputItem)actionItem;
                        foreach (Pattern outputPattern in outputItem.OutputPatternList)
                        {
                            List<string> outputDynamicInformationList = outputPattern.GetDynamicInformation();
                            foreach (string outputDynamicInformation in outputDynamicInformationList)
                            {
                                if (!dynamicInformationList.Contains(outputDynamicInformation))
                                {
                                    consistent = false;
                                    report.Add("[Error]   dynamic information = " + outputDynamicInformation + " in " + outputItem.ID + " will not be available in WM");
                                }
                            }
                        }
                    }
                    if (startItemID != actionItem.ID)
                    {
                        int index = targetIDList.IndexOf(actionItem.ID);
                        if (index < 0)
                        {
                            consistent = false;
                            report.Add("[Error]   Item with ID = " + actionItem.ID + " is unreachable.");
                        }
                    }
                }
            }
            return consistent;
        }

        public Boolean ImportDataItems(string fileName)
        {
            long lastNumberedID = longTermMemory.GetLastDataItemID();
            long nextNumberedID = lastNumberedID + 1;
            StreamReader dataItemReader = new StreamReader(fileName);
            Boolean inItem = false;
            DataItem dataItem = null;
            while (!dataItemReader.EndOfStream)
            {
                string information = dataItemReader.ReadLine();
                if (information != "")
                {
                    if (!information.StartsWith("%"))
                    {
                        if (!inItem)
                        {
                            if (String.Equals(information, "DataItem"))
                            {
                                inItem = true;
                                dataItem = new DataItem();
                                dataItem.TimeStamp = DateTime.Now;
                            }
                        }
                        else
                        {
                            if (!String.Equals(information, "DataItem"))
                            {
                                List<string> informationSplit = information.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                if (informationSplit.Count >= 2)
                                {
                                    string tag = informationSplit[0].Trim(new char[] { ' ', '\t' });
                                    string content = information.Substring(tag.Length, information.Length - tag.Length).Trim(new char[] { ' ', '\t' });
                                    TagValueUnit tagValueUnit = new TagValueUnit(tag, content);
                                    dataItem.ContentList.Add(tagValueUnit);
                                }
                            }
                            else
                            {
                                dataItem.ID = Constants.LTM_ITEM_PREFIX + nextNumberedID.ToString(Constants.LTM_ID_FORMAT);
                                nextNumberedID++;
                                longTermMemory.AddItem(dataItem);
                                dataItem = new DataItem();
                                dataItem.TimeStamp = DateTime.Now;
                            }
                        }
                    }
                }
            }
            if (inItem)
            {
                dataItem.ID = Constants.LTM_ITEM_PREFIX + nextNumberedID.ToString(Constants.LTM_ID_FORMAT);
                longTermMemory.AddItem(dataItem);
            }
            return true; //
        }

        // 20190121-20190123
        // Finds the most recent non-null context, if any.
        // Note that even an ended dialogue can be re-initialized by a
        // context-dependent user input.
        public string GetContext()
        {
            string context = null;
            DataItem historyItem = lastHistoryDataItem;
            if (historyItem != null)
            {
                ActionItem historyActionItem = (ActionItem)historyItem.GetValueByTag(Constants.ITEM_POINTER_TAG);
                context = historyActionItem.ContextAfter;
                if (context == null)  // The context of the last history action item is null: Check for non-null predecessor
                {
                    while (historyItem.PredecessorID != null)
                    {
                        historyItem = (DataItem)workingMemory.ItemList.Find(i => i.ID == historyItem.PredecessorID);
                        
                        historyActionItem = (ActionItem)historyItem.GetValueByTag(Constants.ITEM_POINTER_TAG);
                        context = historyActionItem.ContextAfter;
                        if (context != null)
                        {
                            break;
                          //  if (!historyActionItem.EndsDialogue) { break; }  // The dialogue has not ended: Restore context
                          //  else { currentContext = null; } // The context was != null, but the dialogue has ended.
                        }
                    }
                }
            }
            return context;
        }

        public Boolean UseSpeech
        {
            get { return useSpeech; }
            set { useSpeech = value; }
        }

        public Boolean DialogueLocked
        {
            get { return dialogueLocked; }
            set { dialogueLocked = value; }
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public string StartItemID
        {
            get { return startItemID; }
            set { startItemID = value; }
        }

        [DataMember]
        public Memory LongTermMemory
        {
            get { return longTermMemory; }
            private set { longTermMemory = value; }
        }

        public Memory WorkingMemory
        {
            get { return workingMemory; }
            set { workingMemory = value; }
        }

        public DataItem ContextItem
        {
            get { return contextItem; }
        }

        public DataItem LastHistoryDataItem
        {
            get { return lastHistoryDataItem; }
            set { lastHistoryDataItem = value; }
        }

     /*   public DataItem DialogueHistoryItem
        {
            get { return dialogueHistoryItem; }
        }  */

        public InputItem NextExpectedInputItem
        {
            get { return nextExpectedInputItem; }
            set { nextExpectedInputItem = value; }
        }

        public InputItem LastProcessedInputItem
        {
            get { return lastProcessedInputItem; }
            set { lastProcessedInputItem = value; }
        }

        public OutputItem LastOutputItemInContext
        {
            get { return lastOutputItemInContext; }
            set { lastOutputItemInContext = value; }
        }

     /*   public DataItem DynamicInformationItem
        {
            get { return dynamicInformationItem; }
        }  */

        public Random RandomNumberGenerator
        {
            get { return randomNumberGenerator; }
        }

        // 20190322
        public Boolean AdaptComplexity
        {
            get { return adaptComplexity; }
            set { adaptComplexity = value; }
        }

        // 20190322
        public double ComplexityEMAParameter
        {
            get { return complexityEMAParameter; }
            set { complexityEMAParameter = value; }
        }
    }
}
