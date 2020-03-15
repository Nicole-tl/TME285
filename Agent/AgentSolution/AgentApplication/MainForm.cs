using AgentApplication.AddedClasses;
using AgentLibrary;
using AgentLibrary.Cognition;
using AgentLibrary.EventArgsClasses;
using AgentLibrary.Internet;
using AgentLibrary.IO;
using AgentLibrary.Memories;
using AgentLibrary.Patterns;
using AudioLibrary;
using AuxiliaryLibrary;
using CommunicationLibrary;
using CustomUserControlsLibrary;
using Microsoft.Speech.Synthesis;
using ObjectSerializerLibrary;
using OpenTK;
using SpeechLibrary;
using SpeechLibrary.Modification.Resampling;
using SpeechLibrary.Visualization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Speech.Recognition;
using System.Threading;
using System.Windows.Forms;
using ThreeDimensionalVisualizationLibrary;
using ThreeDimensionalVisualizationLibrary.Animations;
using ThreeDimensionalVisualizationLibrary.Objects;

//Add vision application (?)

namespace AgentApplication
{
    public partial class MainForm : Form
    {
        protected const string DEFAULT_DATA_FOLDER_RELATIVE_PATH = "\\..\\..\\..\\Data\\";

        protected string dataFolderRelativePath = DEFAULT_DATA_FOLDER_RELATIVE_PATH;

        protected AgentConfiguration configuration;
        protected SpeechSynthesizer speechSynthesizer;
        protected VoiceModifier voiceModifier;
        protected Scene3D scene;
        protected SpeechRecognitionEngine speechRecognitionEngine = null;

        private Server server = null;

        private object lockObject = new object();
        private object speechLockObject = new object();

        private Agent agent;

        private Process visionProcess = null;

        private List<DataDownloader> dataDownloaderList;
        private List<InstalledVoice> voiceList;
        private List<string> recordingDeviceList;

        private List<string> speechQueue = null;
        private Thread speechThread = null;

        private List<Type> addedCognitiveActionTypeList = null;

        public MainForm()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            scene = new Scene3D();
            viewer3D.Scene = scene; // Just to get the black background.

            inputTextBox.Enabled = false;
            outputTextBox.ReadOnly = true;

            inputTextBox.InputReceived += new EventHandler<StringEventArgs>(HandleTextInputReceived);
        //    inputOutputControl.InputReceived += new EventHandler<StringEventArgs>(HandleTouchInputReceived);

            secondarySplitContainerLeft.Panel1Collapsed = true;
            speechRecognitionControl.ShowSoundStream = false;

            speechRecognitionControl.SoundRecognized += new EventHandler<StringEventArgs>(HandleSoundRecognized);
            speechRecognitionControl.DisplayDuration = 2; // Default value.

            // Generate list of voices
            SpeechSynthesizer tmpSynthesizer = new SpeechSynthesizer();
            voiceList = tmpSynthesizer.GetInstalledVoices().ToList();
            voiceListComboBox.Items.Clear();
            if (voiceList.Count > 0)
            {
                foreach (InstalledVoice voice in voiceList) { voiceListComboBox.Items.Add(voice.VoiceInfo.Name); }
                voiceListComboBox.SelectedIndex = 0;
            }

            // Generate list of microphones
            recordingDeviceList = SpeechRecognitionControl.GetDeviceNames();
            recordingDeviceComboBox.Items.Clear();
            if (recordingDeviceList.Count > 0)
            {
                foreach (string recordingDevice in recordingDeviceList) { recordingDeviceComboBox.Items.Add(recordingDevice); }
                recordingDeviceComboBox.SelectedIndex = 0;
            }

            // These types are needed when serializing and de-serializing agents (if any cognitive actions have been added under AddedClasses/).
            addedCognitiveActionTypeList = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(CognitiveAction))).ToList();
        }

        // Currently unused.
        #region External methods
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetForegroundWindow();

        internal struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        #endregion

        private void HandleTextInputReceived(object sender, StringEventArgs e)
        {
            if (agent != null)
            {
                string message = e.StringValue;
                if (message != "")
                {
                    UserInput userInput = new UserInput(InputSource.Utterance, message);
                    agent.ProcessInput(userInput);
                }
            }
        }

        // Not used (yet)
     /*   private void HandleTouchInputReceived(object sender, StringEventArgs e)
        {
            if (agent != null)
            {
                string message = e.StringValue;
                if (message != "")
                {
                    UserInput userInput = new UserInput(InputSource.Touch, message);
                    agent.ProcessInput(userInput);
                }
            }
        }  */

        private void HandleSoundRecognized(object sender, StringEventArgs e)
        {
            string message = e.StringValue;
            if (message != "")
            {
            //    if (InvokeRequired) { this.Invoke(new MethodInvoker(() => inputTextBox.Text = message)); }
           //     else { inputTextBox.Text = message; }
                UserInput userInput = new UserInput(InputSource.Utterance, message);
                agent.ProcessInput(userInput);
            }
        }

        private void HandleServerReceived(object sender, DataPacketEventArgs e)
        {
            string message = e.DataPacket.Message;
            if (message != "")
            {
                UserInput userInput = new UserInput(InputSource.Vision, message);
                agent.ProcessInput(userInput);
            }
        }

        private System.Boolean StartServer(out string error)
        {
            error = "";
            server = new Server();
            server.Received -= new EventHandler<DataPacketEventArgs>(HandleServerReceived);
            server.Received += new EventHandler<DataPacketEventArgs>(HandleServerReceived);
            server.Connect(configuration.IpAddress, configuration.Port);
            if (server.Connected)
            {
                server.AcceptClients();
                string processFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + configuration.VisionRelativeFilePath;
                if (!File.Exists(processFilePath))
                {
                    error += "Incorrect file path for the Vision application";
                    return false;
                }
                else
                {
                    try
                    {
                        visionProcess = Process.Start(processFilePath);
                        visionProcess.WaitForInputIdle(2000);
                        IntPtr p = visionProcess.MainWindowHandle;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        error += Path.GetFileName(processFilePath) + " Failed to start:" + ex.Message + "\r\n";
                        return false;
                    }

                }
            }
            else
            {
                error += "Failed to connect server";
                return false;
            }
        }

        private void LoadGrammar(string fileName)
        {
            List<string> grammarItemList = new List<string>();
            using (StreamReader grammarReader = new StreamReader(fileName))
            {
                while (!grammarReader.EndOfStream)
                {
                    string grammarItem = grammarReader.ReadLine();
                    grammarItemList.Add(grammarItem);
                }
                grammarReader.Close();
            }
            SetGrammar(grammarItemList);
        }

        private void SetGrammar(List<string> grammarItemList)
        {
            List<Tuple<string, Choices>> choicesList = new List<Tuple<string, Choices>>();
            foreach (string specification in grammarItemList)
            {
                if (specification.StartsWith("Grammar"))
                {
                    List<string> specificationSplit = specification.Split(new char[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (specificationSplit.Count == 3)
                    {
                        GrammarBuilder grammarBuilder = new GrammarBuilder();
                        string grammarName = specificationSplit[1];
                        string rule = specificationSplit[2];
                        Choices ruleChoices = new Choices();
                        if (!rule.Contains('<'))
                        {
                            string[] ruleItemList = rule.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            ruleChoices.Add(ruleItemList);
                            grammarBuilder.Append(ruleChoices);
                        }
                        else
                        {
                            List<string> ruleItemList = rule.Split(new char[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            for (int ii = 0; ii < ruleItemList.Count(); ii++)
                            {
                                string ruleItem = ruleItemList[ii];
                                if (!ruleItem.Contains("#"))
                                {
                                    grammarBuilder.Append(ruleItem);
                                }
                                else
                                {
                                    string choiceName = ruleItem.Remove(0, 1);
                                    Choices choices = choicesList.Find(c => c.Item1 == choiceName).Item2;
                                    grammarBuilder.Append(choices);
                                }
                            }
                        }
                        Grammar grammar = new Grammar(grammarBuilder);
                        grammar.Name = grammarName;
                        speechRecognitionEngine.LoadGrammar(grammar);
                    }
                }
                else if (specification.StartsWith("Choice"))
                {
                    List<string> specificationSplit = specification.Split(new char[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (specificationSplit.Count == 3)
                    {
                        Choices choices = new Choices();
                        string choicesName = specificationSplit[1].TrimStart(new char[] { ' ' }).TrimEnd(new char[] { ' ' });
                        string options = specificationSplit[2];
                        string[] optionsList = options.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        choices.Add(optionsList);
                        choicesList.Add(new Tuple<string, Choices>(choicesName, choices));
                    }
                }
            }
        }

        private System.Boolean SetUpAgent(out string error)
        {
            System.Boolean success = true;
            error = "";
            // Set up the server, for handling communication with the Vision application:
            if (configuration.UseVision)
            {
                string serverError = "";
                System.Boolean serverOK = StartServer(out serverError);
                if (!serverOK)
                {
                    success = false;
                    error += serverError + "\r\n";
                }
            }

            // Set up internet downloader(s)
            dataDownloaderList = new List<DataDownloader>();

            internetDataDownload dataDownloader = new internetDataDownload();  
            dataDownloader.DownloadComplete += new EventHandler<DataItemListEventArgs>(HandleDownloadComplete);
            dataDownloaderList.Add(dataDownloader);  // Uncomment to use! Note that more data downloaders can be added.

            // Set event handlers for agent: (first remove old event handlers, just in case ...)
            agent.MovementGenerated -= new EventHandler<StringEventArgs>(HandleMovementGenerated);
            agent.TextOutputGenerated -= new EventHandler<StringEventArgs>(HandleTextOutputGenerated);
            agent.SpeechOutputGenerated -= new EventHandler<StringEventArgs>(HandleSpeechOutputGenerated);
            agent.DisplayOutputGenerated -= new EventHandler<StringEventArgs>(HandleDisplayOutputGenerated);
            agent.MovementGenerated += new EventHandler<StringEventArgs>(HandleMovementGenerated);
            agent.TextOutputGenerated += new EventHandler<StringEventArgs>(HandleTextOutputGenerated);
            agent.SpeechOutputGenerated += new EventHandler<StringEventArgs>(HandleSpeechOutputGenerated);
            agent.DisplayOutputGenerated += new EventHandler<StringEventArgs>(HandleDisplayOutputGenerated);

            // Set up voice:
            speechSynthesizer = new SpeechSynthesizer();
            List<InstalledVoice> voiceList = speechSynthesizer.GetInstalledVoices().ToList();
            InstalledVoice voice = voiceList.Find(v => v.VoiceInfo.Name == configuration.BaseVoiceName);
            if (voice == null)
            {
                error += "Voice " + configuration.BaseVoiceName + " not available" + "\r\n";
                success = false;
            }
            else
            {
                speechSynthesizer.SelectVoice(voice.VoiceInfo.Name);
            }

            // Set up speech recognition engine:
            if (configuration.UseSpeechInput)
            {
                speechRecognitionEngine = new SpeechRecognitionEngine();
                string grammarDirectory = Path.GetDirectoryName(Application.ExecutablePath) + dataFolderRelativePath;
                if (Directory.Exists(grammarDirectory))
                {
                    string speechRecognitionGrammarFilePath = grammarDirectory + configuration.SpeechRecognitionGrammarFileName; ;
                    if (File.Exists(speechRecognitionGrammarFilePath))
                    {
                        LoadGrammar(speechRecognitionGrammarFilePath);
                        speechRecognitionControl.SetSpeechRecognitionEngine(speechRecognitionEngine);
                    }
                    else
                    {
                        success = false;
                        error += "Grammar file " + configuration.SpeechRecognitionGrammarFileName + " not available.";
                    }
                }
                else
                {
                    success = false;
                    error += "Directory " + grammarDirectory + " not found." + "\r\n";
                }


                // Set up microphone
                string recordingDeviceName = configuration.RecordingDeviceName;
                int recordingDeviceID = recordingDeviceList.FindIndex(r => r.StartsWith(recordingDeviceName)); // "StartsWith" required here. Name (sometimes) ends with \0.
                if (recordingDeviceID >= 0)
                {
                    speechRecognitionControl.DeviceID = recordingDeviceID;
                    speechRecognitionControl.ShowSoundStream = true;
                    if (configuration.RecordOnDemandOnly)
                    {
                        toggleRecordingButton.Enabled = true;
                    }
                    else
                    {
                        speechRecognitionControl.StartContinuousRecording();
                        toggleRecordingButton.Enabled = false;
                    }
                }
                else
                {
                    toggleRecordingButton.Enabled = false;
                    error += "Recording device " + recordingDeviceName + " not available" + "\r\n";
                    success = false;
                }
            }

            // Set up voice modifier and visualizer:
            string directory = Path.GetDirectoryName(Application.ExecutablePath) + dataFolderRelativePath;
            if (Directory.Exists(directory))
            {
                string voiceModifierFilePath = directory + configuration.VoiceModifierFileName;
                if (File.Exists(voiceModifierFilePath))
                {
                    voiceModifier = (VoiceModifier)ObjectXmlSerializer.ObtainSerializedObject(voiceModifierFilePath, typeof(VoiceModifier));
                }
                else
                {
                    success = false;
                    error += "Voice modifier " + configuration.VoiceModifierFileName + " not found" + "\r\n";
                }
                string agentVisualizerFilePath = directory + configuration.AgentVisualizerFileName;
                if (File.Exists(agentVisualizerFilePath))
                {
                    // XML-formatted files are usually larger than binary-formatted (.DAT) files.
                    // On the other hand, XML de-serialization is typically faster...
                    if (agentVisualizerFilePath.EndsWith(".xml"))
                    {
                        scene = (Scene3D)ObjectXmlSerializer.ObtainSerializedObject(agentVisualizerFilePath, typeof(Scene3D));
                    }
                    else if (agentVisualizerFilePath.EndsWith(".dat"))
                    {
                        using (Stream stream = File.Open(agentVisualizerFilePath, FileMode.Open))
                        {
                            var binaryFormatter = new BinaryFormatter();
                            scene = (Scene3D)binaryFormatter.Deserialize(stream);
                            stream.Close();
                        }
                    }
                    else
                    {
                        success = false;
                        error += "Unknown format for the agent visualizer" + "\r\n";
                    }
                    if (success)
                    {
                        scene.SetLockObject(lockObject);
                        foreach (Object3D object3D in scene.ObjectList)
                        {
                            object3D.AssignKeyFrame(0);
                        }
                        viewer3D.Scene = scene;
                        viewer3D.CameraDistance = scene.CameraDistance;
                        viewer3D.CameraLatitude = scene.CameraLatitude;
                        viewer3D.CameraLongitude = scene.CameraLongitude;
                        viewer3D.CameraTarget = new Vector3(scene.CameraTarget[0], scene.CameraTarget[1], scene.CameraTarget[2]);
                        viewer3D.StartAnimation();
                    }
                }
                else
                {
                    success = false;
                    error += "Agent visualizer " + configuration.AgentVisualizerFileName + " not found" + "\r\n";
                }
            }

            // Set up an empty speech queue
            speechQueue = new List<string>();

            return success;
        }

        private void loadAgentConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = DialogFilters.XML;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    configuration = (AgentConfiguration)ObjectXmlSerializer.ObtainSerializedObject(openFileDialog.FileName, typeof(AgentConfiguration));
                    configurationPropertyPanel.SetObject(configuration);
                    saveAgentConfigurationToolStripMenuItem.Enabled = true;
                    if (agent != null) { startButton.Enabled = true; }
                }
            }
        }

        private void saveAgentConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = DialogFilters.XML;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ObjectXmlSerializer.SerializeObject(saveFileDialog.FileName, configuration);
                }
            }
        }

        private void HandleMovementGenerated(object sender, StringEventArgs e)
        {
            string message = e.StringValue;
            scene.RunAnimation(AnimationConstants.GENERAL_ANIMATION_PREFIX + message);
        }

        private void SpeechLoop()
        {
            while (speechQueue.Count > 0)
            {
                Monitor.Enter(speechLockObject);
                string utterance = speechQueue[0];
                speechQueue.RemoveAt(0);
                WAVSound originalSound = Speaker.Speak(speechSynthesizer, utterance);
                double startTime = originalSound.GetFirstTimeAboveThreshold(0, 10, 300);  // To do: Parameterize
                double endTime = originalSound.GetLastTimeAboveThreshold(0, 10, 300);    // To do: Parameterize
                WAVSound shortenedOriginalSound = originalSound.Extract(startTime, endTime);  // Remove initial and final silence.
                WAVSound modifiedSound = voiceModifier.Modify(shortenedOriginalSound);
                WAVSound.PlaySoundSync(modifiedSound);
                Monitor.Exit(speechLockObject);
            }
        }

        private void Speak(string utterance)
        {
            scene.RunAnimation(AnimationConstants.SPEECH_ANIMATION_PREFIX + utterance);
            speechThread = new Thread(new ThreadStart(SpeechLoop));
            speechThread.Start();
          /*  WAVSound originalSound = Speaker.Speak(speechSynthesizer, utterance);
            double startTime = originalSound.GetFirstTimeAboveThreshold(0, 10, 300);
            double endTime = originalSound.GetLastTimeAboveThreshold(0, 10, 300);
            WAVSound shortenedOriginalSound = originalSound.Extract(startTime, endTime);  // Remove initial and final silence.
            WAVSound modifiedSound = voiceModifier.Modify(shortenedOriginalSound);
            WAVSound.PlaySoundAsync(modifiedSound);  */
        }

        private void HandleSpeechOutputGenerated(object sender, StringEventArgs e)
        {
            string utterance = e.StringValue;
            Monitor.Enter(speechLockObject);  // This will stop execution until the previous output statement has been completed
            speechQueue.Add(utterance);
            Monitor.Exit(speechLockObject);
            Speak(utterance);

            inputTextBox.Text = "";  // Need to clear the input textbox here IF the most recent input was in the form of speech
        }

        private void HandleTextOutputGenerated(object sender, StringEventArgs e)
        {
            string utterance = e.StringValue;
            outputTextBox.ShowOutput(utterance);
        }

        private void ThreadSafeRefresh()
        {
            if (InvokeRequired) { this.Invoke(new MethodInvoker(() => Refresh())); }
            else { Refresh(); }
        }

        //
        // Note to students: This is a rather rudimentary application of Display output:
        //                   For example, it can only show one image at a time. However,
        //                   note that, in the case of text, the code can at least
        //                   show multiple lines of text (separated by "\r\n"). 
        //                   To clear the display view, send any message with DisplaySource set to "None".
        //
        //                   You are of course welcome to modify the HandleDisplayOutputGenerated() method below.
        //
        private void HandleDisplayOutputGenerated(object sender, StringEventArgs e)
        {
            string message = e.StringValue; // This should be of the form <DisplaySource>:<path> (see the DisplayControl user control)
            if (message != "")
            {
                List<string> messageSplit = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (messageSplit.Count == 2)
                {
                    try
                    {
                        DisplaySource displaySource = (DisplaySource)Enum.Parse(typeof(DisplaySource), messageSplit[0]);
                        string relativeFilePath = messageSplit[1];
                        if (displaySource == DisplaySource.File)  // Image from file: Find the absolute path
                        {
                            string filePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + configuration.DisplayDataPath + relativeFilePath;
                            if (File.Exists(filePath))
                            {
                                Bitmap image = new Bitmap(filePath);
                                displayControl.FillImage(image);
                                ThreadSafeRefresh();
                            }
                        }
                        else if (displaySource == DisplaySource.Internet)
                        {
                            // Note to students: There are many images for which the code below does NOT work - it
                            // depends on the settings specified for the web page (e.g. whether or not
                            // direct image download is allowed etc.). Often better to obtain 
                            // images from file (see above).
                            using (WebClient wc = new WebClient())
                            {
                                byte[] byteArray = wc.DownloadData(messageSplit[1]);
                                MemoryStream ms = new MemoryStream(byteArray);
                                Bitmap image = (Bitmap)Image.FromStream(ms);
                                displayControl.FillImage(image);
                                ThreadSafeRefresh();
                            }
                        }
                        else if (displaySource == DisplaySource.Text)
                        {
                            TextToImageConverter textToImageConverter = new TextToImageConverter();
                            Bitmap textAsImage = textToImageConverter.Convert(messageSplit[1]);
                            displayControl.AssignImage(textAsImage);
                            ThreadSafeRefresh();
                        }
                        else if (displaySource == DisplaySource.None) { displayControl.Clear(); }
                    }
                    catch { } // Do nothing here. (Perhaps add some specific error handling later...)
                }
                else if (messageSplit.Count == 1) // Empty message (can only be used for clearing the display)
                {
                    try
                    {
                        DisplaySource displaySource = (DisplaySource)Enum.Parse(typeof(DisplaySource), messageSplit[0]);
                        if (displaySource == DisplaySource.None)
                        {
                            displayControl.Clear();
                            ThreadSafeRefresh();
                        }
                    }
                    catch { }  // Do nothing here. (Perhaps add some specific error handling later...)
                }
            }
        }

        // Note to students: This event handler is executed when a
        // downloader has completed its download (which is either
        // carried out once and for all, or repeatedly).
        //
        // It is up to the user to write the specific data downloader class(es),
        // which must be derived from the DataDownloader class under
        // AgentLibrary.Internet.
        //
        // A very simple, specific example can be found under AddedClasses
        // in the AgentApplication.

        private void HandleDownloadComplete(object sender, DataItemListEventArgs e)
        {
            List<DataItem> dataItemList = e.DataItemList;
            if (dataItemList != null)
            {
                foreach (DataItem dataItem in dataItemList)
                {
                    agent.LongTermMemory.AddLTMDataItem(dataItem);
                }
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            string error;
            System.Boolean success = SetUpAgent(out error);
            if (success)
            {
                inputTextBox.Enabled = true;
                startButton.Enabled = false;
                agent.UseSpeech = configuration.UseSpeechOutput;
                agent.Start();
                workingMemoryTextViewer.SetMemory(agent.WorkingMemory);
                agent.MemoryViewUpdateNeeded -= new EventHandler(UpdateWorkingMemoryView); // First remove event handler from earlier run, if any.
                agent.MemoryViewUpdateNeeded += new EventHandler(UpdateWorkingMemoryView);
                foreach (DataDownloader dataDownloader in dataDownloaderList) { dataDownloader.Start(); }
                stopButton.Enabled = true;
            }
            else
            {
                MessageBox.Show(error, "Error: Unable to set up agent");
            }
        }

        // This method stop the agent and the Vision application (if available)
        //
        // Note: This method is not _guaranteed_ to always stop all parts
        // of the agent _immediately_. Use at your own risk (often better
        // just to restart the program).
        //
        // Note also that the startButton is not re-activated until a
        // new agent is selected in the menu (or loaded from file).
        private void stopButton_Click(object sender, EventArgs e)
        {
            inputTextBox.Enabled = false;
            if (server != null)
            {
                server.Disconnect();
                server = null;
            }
            stopButton.Enabled = false;
            if (visionProcess != null)
            {
                visionProcess.Kill();  // A bit brutal, but it works...
                visionProcess = null;
            }
            if (dataDownloaderList != null)
            {
                foreach (DataDownloader dataDownloader in dataDownloaderList)
                {
                    dataDownloader.Stop();
                }
            }
            speechRecognitionControl.StopContinuousRecording();
            viewer3D.Stop();
        }

        /*   private void testButton_Click(object sender, EventArgs e)
           {
               Server server = new Server();
               server.Connect("127.0.0.1", 7);
               server.AcceptClients();
           }  */

        private void loadAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = DialogFilters.XML;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        agent = (Agent)ObjectXmlSerializer.ObtainSerializedObject(openFileDialog.FileName, typeof(Agent), addedCognitiveActionTypeList);
                        if (configuration != null)
                        {
                            startButton.Enabled = true;
                        }
                    }
                    catch
                    {
                        agent = null;
                        saveAgentToolStripMenuItem.Enabled = true;
                        startButton.Enabled = false;
                        MessageBox.Show("Error loading agent");
                    }
                }
            }
        }

        private void saveAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = DialogFilters.XML;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ObjectXmlSerializer.SerializeObject(saveFileDialog.FileName, agent, addedCognitiveActionTypeList);
                }
            }
        }

        private void newAgentConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configuration = new AgentConfiguration();

            // Override, to make sure that the default voice actually exists on the system in question:
            if (speechSynthesizer == null) { speechSynthesizer = new SpeechSynthesizer(); }
            List<InstalledVoice> voiceList = speechSynthesizer.GetInstalledVoices().ToList();
            if (voiceList.Count > 0)
            {
                InstalledVoice defaultVoice = voiceList.Find(t => t.VoiceInfo.Name == configuration.BaseVoiceName);
                if (defaultVoice == null)
                {
                    configuration.BaseVoiceName = voiceList[0].VoiceInfo.Name;
                }
            }
            else
            {
                configuration.BaseVoiceName = "";
            }

            if (recordingDeviceComboBox.Items.Count > 0) { configuration.RecordingDeviceName = recordingDeviceComboBox.Items[0].ToString(); }
            configurationPropertyPanel.SetObject(configuration);
            saveAgentConfigurationToolStripMenuItem.Enabled = true;
            if (agent != null) { startButton.Enabled = true; }
        }

        private void speechInputVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            secondarySplitContainerLeft.Panel1Collapsed = !speechInputVisibleToolStripMenuItem.Checked;
            speechRecognitionControl.ShowSoundStream = speechInputVisibleToolStripMenuItem.Checked;
        }

        private void recordingDeviceListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int recordingDeviceIndex = recordingDeviceComboBox.SelectedIndex;
            string recordingDeviceName = recordingDeviceComboBox.Items[recordingDeviceIndex].ToString();
            if (configuration != null)
            {
                configuration.RecordingDeviceName = recordingDeviceName;
                configurationPropertyPanel.SetObject(configuration); // Needed in order to refresh the property panel.
            }
        }

        private void availableVoicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int voiceIndex = voiceListComboBox.SelectedIndex;
            string voiceName = voiceListComboBox.Items[voiceIndex].ToString();
            if (configuration != null)
            {
                configuration.BaseVoiceName = voiceName;
                configurationPropertyPanel.SetObject(configuration); // Needed in order to refresh the property panel.
            }
        }

        private void toggleRecordingButton_Click(object sender, EventArgs e)
        {
            if (toggleRecordingButton.Text == "Record")
            {
                toggleRecordingButton.Text = "Stop recording";
                speechRecognitionControl.StartContinuousRecording();
            }
            else
            {
                toggleRecordingButton.Text = "Record";
                speechRecognitionControl.StopContinuousRecording();
                speechRecognitionControl.RunRecognizer(speechRecognitionControl.Sound);
            }
        }

        private void UpdateWorkingMemoryView(object sender, EventArgs e)
        {
            if (InvokeRequired) { this.BeginInvoke(new MethodInvoker(() => workingMemoryTextViewer.ShowMemory())); }
            else { workingMemoryTextViewer.ShowMemory(); }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            outputTextBox.Stop();
            speechRecognitionControl.StopContinuousRecording();
            viewer3D.Stop();
            Application.Exit();
        }

        private void importLTMDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (agent != null)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = DialogFilters.Text;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            agent.ImportDataItems(openFileDialog.FileName);
                        }
                        catch
                        {
                            MessageBox.Show("Data format error.");
                        }
                    }
                }
            }
        }

        #region Sample agents
        private void agent1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            agent = new Agent();
            agent.Name = "Agent1";

            #region // DataItem - Geolocation of Chalmers Unversity of Technology
            DataItem Chalmers = new DataItem();
            Chalmers.ID = "L0000001";
            Chalmers.ContentList.Add(new TagValueUnit("myLocation", "Chalmers"));
            Chalmers.ContentList.Add(new TagValueUnit("latitude", "57.6896523"));
            Chalmers.ContentList.Add(new TagValueUnit("longitude", "11.9766811"));
            agent.LongTermMemory.ItemList.Add(Chalmers);
            #endregion

            #region Face detection 
            InputItem inputItem0 = new InputItem();
            inputItem0.RequiredInputSource = InputSource.Vision;
            inputItem0.IsEntryPoint = true;
            inputItem0.ID = "A015.00001";

            InputAction inputAction0 = new InputAction();
            Pattern pattern0 = new Pattern();
            pattern0.PatternSpecification = "FaceDetected";
            inputAction0.PatternList = new List<Pattern>() { pattern0 };
            inputAction0.TargetItemID = "A015.00002";
            inputItem0.InputActionList.Add(inputAction0);
            agent.LongTermMemory.ItemList.Add(inputItem0);

            List<Pattern> outputPatternList1P = new List<Pattern>() { new Pattern("Good morning.") };
            OutputItem outputItem0 = new OutputItem("A015.00002", outputPatternList1P, null, null);
            agent.LongTermMemory.ItemList.Add(outputItem0);


            #endregion

            // MINOR DIALOGUE
            #region Politness - 1
            InputAction inputAction1P = new InputAction(new List<Pattern>() { new Pattern("[Good morning, Hello, Hi]") }, "A001.10001");
            InputItem inputItem1P = new InputItem("A001.00001", new List<InputAction> { inputAction1P }, null);
            inputItem1P.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem1P);

            OutputItem outputItem100 =
            new OutputItem("A001.10001", new List<Pattern>() { new Pattern("waveL") }, "A001.10003", null);
            outputItem100.OutputDestination = OutputDestination.Animation; // Will be sent to 3D visualizer (if any)
            agent.LongTermMemory.ItemList.Add(outputItem100);

            OutputItem outputItem101 =
            new OutputItem("A001.10003", new List<Pattern>() { new Pattern("waveR") }, "A001.10004", null);
            outputItem101.OutputDestination = OutputDestination.Animation; // Will be sent to 3D visualizer (if any)
            agent.LongTermMemory.ItemList.Add(outputItem101);

            OutputItem outputItem102 =
            new OutputItem("A001.10004", new List<Pattern>() { new Pattern("downL") }, "A001.10005", null);
            outputItem102.OutputDestination = OutputDestination.Animation; // Will be sent to 3D visualizer (if any)
            agent.LongTermMemory.ItemList.Add(outputItem102);

            OutputItem outputItem103 =
            new OutputItem("A001.10005", new List<Pattern>() { new Pattern("downR") }, "A001.10002", null);
            outputItem103.OutputDestination = OutputDestination.Animation; // Will be sent to 3D visualizer (if any)
            agent.LongTermMemory.ItemList.Add(outputItem103);

            OutputItem outputItem26 =
            new OutputItem("A001.10002", new List<Pattern>() { new Pattern("Speak") }, "A001.00002", null);
            outputItem26.OutputDestination = OutputDestination.Animation; // Will be sent to 3D visualizer (if any)
            agent.LongTermMemory.ItemList.Add(outputItem26);

            OutputItem outputItem1P = new OutputItem("A001.00002", outputPatternList1P, "A001.00003", null);
            agent.LongTermMemory.AddItem(outputItem1P);

            List<Pattern> outputPatternList4P = new List<Pattern>() { new Pattern("How are you?") };
            OutputItem outputItem4P = new OutputItem("A001.00003", outputPatternList4P, "A001.00004", null);
            agent.LongTermMemory.AddItem(outputItem4P);
            

            InputAction inputAction21P = new InputAction(new List<Pattern>() { new Pattern("[I'm,{}] [fine, very good, good, feeling very good] [thanks, thank you,{}]") }, "A001.00005");
            InputAction inputAction22P = new InputAction(new List<Pattern>() { new Pattern("[I'm,{}] not [so good, feeling well, good]") }, "A001.00006");
            List<InputAction> inputActionList2P = new List<InputAction>() { inputAction21P, inputAction22P };
            InputItem inputItem2P = new InputItem("A001.00004", inputActionList2P, null);
            agent.LongTermMemory.AddItem(inputItem2P);

            List<Pattern> outputPatternList2P = new List<Pattern>() { new Pattern("That's good to hear!") };
            OutputItem outputItem2P = new OutputItem("A001.00005", outputPatternList2P, "A016.00002", null);
            agent.LongTermMemory.AddItem(outputItem2P);
            List<Pattern> outputPatternList3P = new List<Pattern>() { new Pattern("I'm sorry to hear that. Hope you get [well,better] soon") };
            OutputItem outputItem3P = new OutputItem("A001.00006", outputPatternList3P, null, null);
            agent.LongTermMemory.AddItem(outputItem3P);

            OutputItem outputItem25 =
            new OutputItem("A016.00002", new List<Pattern>() { new Pattern("Smile") }, "A016.00003", null);
            outputItem25.OutputDestination = OutputDestination.Animation; // Will be sent to 3D visualizer (if any)
            agent.LongTermMemory.ItemList.Add(outputItem25);
            #endregion

            #region Name - 1
            InputAction inputAction1N = new InputAction(new List<Pattern>() { new Pattern("[By the way,{}] what is your name?") }, "A001.00008");
            InputItem inputItem1N = new InputItem("A001.00007", new List<InputAction> { inputAction1N }, null);
            inputItem1N.IsEntryPoint = true;
            inputItem1N.RequiredInputSource = InputSource.Utterance;
            inputItem1N.InputActionList.Add(inputAction1N);
            agent.LongTermMemory.ItemList.Add(inputItem1N);

            Pattern outputPattern11N = new Pattern("My name is Bob, what is [yours, your name]?");
            OutputItem outputItem11N = new OutputItem("A001.00008", new List<Pattern> { outputPattern11N }, "A001.00009", null);
            agent.LongTermMemory.ItemList.Add(outputItem11N);


            Pattern pattern2N = new Pattern();           
            pattern2N.PatternSpecification = "[My name is, {}] <name1>";
            InputAction inputAction2N = new InputAction(new List<Pattern>() { pattern2N }, "A001.000010");
            InputItem inputItem2N = new InputItem("A001.00009", new List<InputAction> { inputAction2N }, null);
            agent.LongTermMemory.ItemList.Add(inputItem2N);


            Pattern outputPattern12N = new Pattern("[What a beautiful name, {}]. Nice to meet you <name1>.");
            OutputItem outputItem12N = new OutputItem("A001.000010", new List<Pattern> { outputPattern12N }, "A001.00011", null);
            outputItem12N.SuppressOutputRepetition = false;  
            agent.LongTermMemory.ItemList.Add(outputItem12N);

            // Years old
            InputAction inputAction13N = new InputAction(new List<Pattern>() { new Pattern("[By the way,{}] how old are you?") }, "A001.00013");
            InputItem inputItem13N = new InputItem("A001.00012", new List<InputAction> { inputAction13N }, null);
            inputItem13N.IsEntryPoint = true;
            inputItem13N.InputActionList.Add(inputAction13N);
            agent.LongTermMemory.ItemList.Add(inputItem13N);

            Pattern outputPattern13N = new Pattern("I am 7 years old"); //Depends on animation
            OutputItem outputItem13N = new OutputItem("A001.00013", new List<Pattern> { outputPattern13N }, "A001.00014", null);
            outputItem13N.SuppressOutputRepetition = false;   // NO NEED TO REPITITION
            agent.LongTermMemory.ItemList.Add(outputItem13N);

            #endregion

            #region - Time and date
            // Time - 2
            InputAction inputAction2T = new InputAction(new List<Pattern>() { new Pattern("What time is it?") }, "A001.00016");
            InputItem inputItem2T = new InputItem("A001.00015", new List<InputAction> { inputAction2T }, null);
            inputItem2T.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem2T);


            CognitiveItem cognitiveItem2T = new CognitiveItem();
            cognitiveItem2T.ID = "A001.00016";
            AddedClasses.GetCurrentTimeAction cognitiveAction2T = new AddedClasses.GetCurrentTimeAction();
            cognitiveAction2T.OutputList = new List<CognitiveActionParameter>()
            { new CognitiveActionParameter(CognitiveActionParameterType.WMTag,"currentTime")};
            cognitiveAction2T.SuccessTarget = new CognitiveActionTarget(-1, "A001.00017");
            cognitiveItem2T.CognitiveActionList.Add(cognitiveAction2T);
            agent.LongTermMemory.ItemList.Add(cognitiveItem2T);

            Pattern outputPattern2T = new Pattern("It is <currentTime>");
            OutputItem outputItem2T = new OutputItem("A001.00017", new List<Pattern> { outputPattern2T}, null, null);
            agent.LongTermMemory.AddItem(outputItem2T);

            // Date - 2
            InputAction inputAction2D = new InputAction(new List<Pattern>() { new Pattern("What date is it today?") }, "A001.00024");
            InputItem inputItem2D = new InputItem("A001.00023", new List<InputAction> { inputAction2D }, null);
            inputItem2D.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem2D);

            CognitiveItem cognitiveItem2D = new CognitiveItem();
            cognitiveItem2D.ID = "A001.00024";
            GetCurrentDateAction cognitiveAction2D = new GetCurrentDateAction();
            cognitiveAction2D.OutputList = new List<CognitiveActionParameter>()
            { new CognitiveActionParameter(CognitiveActionParameterType.WMTag,"dateOfToday")};
            cognitiveAction2D.SuccessTarget = new CognitiveActionTarget(-1, "A001.00025");
            cognitiveItem2D.CognitiveActionList.Add(cognitiveAction2D);
            agent.LongTermMemory.ItemList.Add(cognitiveItem2D);

            Pattern outputPattern2D = new Pattern("It is <dateOfToday> Today");
            OutputItem outputItem2D = new OutputItem("A001.00025", new List<Pattern> { outputPattern2D }, null, null);
            agent.LongTermMemory.AddItem(outputItem2D);

            // Weekday - 3
            InputAction inputAction21D = new InputAction(new List<Pattern>() { new Pattern("What weekday is it today?") }, "A001.00021");
            InputItem inputItem21D = new InputItem("A001.00020", new List<InputAction> { inputAction21D }, null);
            inputItem21D.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem21D);

            CognitiveItem cognitiveItem21D = new CognitiveItem();
            cognitiveItem21D.ID = "A001.00021";
            GetCurrentWeekdayAction cognitiveAction21D = new GetCurrentWeekdayAction();
            cognitiveAction21D.InputList = new List<CognitiveActionParameter>();
            cognitiveAction21D.OutputList = new List<CognitiveActionParameter>()
            { new CognitiveActionParameter(CognitiveActionParameterType.WMTag,"weekdayOfToday")};
            cognitiveAction21D.SuccessTarget = new CognitiveActionTarget(-1, "A001.00022");
            cognitiveItem21D.CognitiveActionList.Add(cognitiveAction21D);
            agent.LongTermMemory.ItemList.Add(cognitiveItem21D);

            Pattern outputPattern21D = new Pattern("It is <weekdayOfToday> Today");
            OutputItem outputItem21D = new OutputItem("A001.00022", new List<Pattern> { outputPattern21D}, null, null);
            agent.LongTermMemory.AddItem(outputItem21D);

            #endregion

            #region Repetition
            InputAction inputAction1R = new InputAction(new List<Pattern>()
              { new Pattern("[Can, Could] you repeat [that, what you said] [please,{}]"),
                new Pattern("[Please,{}] repeat") }, "A001.00031");
            InputItem inputItem1R = new InputItem("A001.00030", new List<InputAction> { inputAction1R }, null);
            inputItem1R.IsEntryPoint = true;
            agent.LongTermMemory.ItemList.Add(inputItem1R);

            CognitiveItem cognitiveItem1R = new CognitiveItem("A001.00031");
            MyFindLastOutputAction cognitiveAction1R = new MyFindLastOutputAction();
            cognitiveAction1R.SetOutputList(CognitiveActionParameterType.WMTag, new List<string>() { "lastOutput" });
            cognitiveAction1R.SuccessTarget = new CognitiveActionTarget(-1, "A001.00032");
            cognitiveItem1R.CognitiveActionList = new List<CognitiveAction>() { cognitiveAction1R };
            agent.LongTermMemory.ItemList.Add(cognitiveItem1R);

            Pattern outputPattern1R = new Pattern("[Absolutely, Of cause, Sure, I am sorry I spoke too fast.] I said: <lastOutput>");
            OutputItem outputItem1R = new OutputItem("A001.00032", new List<Pattern>() { outputPattern1R }, null, null);
            outputItem1R.SuppressOutputRepetition = true;
            agent.LongTermMemory.ItemList.Add(outputItem1R);
            #endregion

            #region Major dialougue - city guide
            // First part - recommend a resturant
            InputAction inputAction1 = new InputAction(new List<Pattern>() { new Pattern("Can you recommend me a restaurant?") }, "A002.00002");
            InputItem inputItem1 = new InputItem("A002.00001", new List<InputAction> { inputAction1 }, null);
            inputItem1.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem1);

            Pattern outputPattern1 = new Pattern("[Absolutley, Sure, of cause, Certainly]. What food are you interested in?");
            OutputItem outputItem1 = new OutputItem("A002.00002", new List<Pattern> { outputPattern1 }, "A002.00003", null);
            agent.LongTermMemory.AddItem(outputItem1);

            InputAction inputAction2 = new InputAction(new List<Pattern>() { new Pattern("[I am interested in, { }] <cuisine>") }, "A002.00004");
            InputItem inputItem2 = new InputItem("A002.00003", new List<InputAction> { inputAction2 }, null);
            agent.LongTermMemory.ItemList.Add(inputItem2);

            // Find all restaurant with specific cuisin
            CognitiveItem cognitiveItem2 = new CognitiveItem();
            cognitiveItem2.ID = "A002.00004";
            GetListFromLTM cognitiveAction21 = new GetListFromLTM();
            cognitiveAction21.InputList = new List<CognitiveActionParameter>()
                {
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "cuisine"),
                       new CognitiveActionParameter(CognitiveActionParameterType.LTMCategory, "cuisine")
                };
            cognitiveAction21.OutputList =
                new List<CognitiveActionParameter>() { new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "ListOfItem") };
            cognitiveAction21.SuccessTarget = new CognitiveActionTarget(-1, "A002.00005");
            cognitiveAction21.FailureTarget = new CognitiveActionTarget(-1, "A000.00001");
            cognitiveItem2.CognitiveActionList.Add(cognitiveAction21);

            agent.LongTermMemory.ItemList.Add(cognitiveItem2);

            Pattern displayPattern21 = new Pattern(" Here is a List of the <cuisine> restaurants.");
            OutputItem outputItem21 = new OutputItem("A002.00005", new List<Pattern>() { displayPattern21 }, "A002.00006", null);
            agent.LongTermMemory.ItemList.Add(outputItem21);
            
            // Display the list of restauraunt with specific cuisine
            Pattern displayPattern2 = new Pattern("<ListOfItem>");
            OutputItem outputItem2 = new OutputItem("A002.00006", new List<Pattern>() { displayPattern2 }, "A002.00007", null);
            outputItem2.OutputDestination = OutputDestination.Display;  
            agent.LongTermMemory.ItemList.Add(outputItem2);

            // When not able to find the mentioned cuisine
            Pattern outputPattern22 = new Pattern("[I am sorry, I am not able to understand] [Here is a list of cuisine {}], you can choose between.");
            OutputItem outputItem22 = new OutputItem("A000.00001", new List<Pattern> { outputPattern22 }, "A000.00002", null);
            agent.LongTermMemory.ItemList.Add(outputItem22);

            // Display all cuisine
            OutputItem outputItem23 = new OutputItem("A000.00002", new List<Pattern>() { displayPattern2 }, "A002.00003", null);
            outputItem23.OutputDestination = OutputDestination.Display;  // To display
            agent.LongTermMemory.ItemList.Add(outputItem23);

            // Remove list from display
            Pattern displayPattern31 = new Pattern(DisplaySource.None + "|" + "Paris.jpg");
            displayPattern31.UseVerbatim = true;  // Required to aviod processing the pattern
            OutputItem outputItem31 = new OutputItem("A002.00007", new List<Pattern>() { displayPattern31 }, "A002.00008", null);
            outputItem31.OutputDestination = OutputDestination.Display;  // To display
            agent.LongTermMemory.ItemList.Add(outputItem31);

            InputAction inputAction3 = new InputAction(new List<Pattern>() { new Pattern("[Which one is closest, which one is the closest one, which is closest]") }, "A002.00009");
            InputAction inputAcion31 = new InputAction(new List<Pattern>() { new Pattern("[I want to make a reservation at, I like, I want to go to, {}] <restaurantChoice>") }, "A004.10008");
            InputItem inputItem3 = new InputItem("A002.00008", new List<InputAction> { inputAction3, inputAcion31 }, null);
            agent.LongTermMemory.AddItem(inputItem3);

            // Calculate the closest one
            CognitiveItem cognitiveItem3 = new CognitiveItem();
            cognitiveItem3.ID = "A002.00009";
            ShortestDistanceToChalmersAction cognitiveAction3 = new ShortestDistanceToChalmersAction();
            cognitiveAction3.InputList = new List<CognitiveActionParameter>()
                {
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "ListOfItem"),
                       new CognitiveActionParameter(CognitiveActionParameterType.LTMTag, "geolocation"),
                       new CognitiveActionParameter(CognitiveActionParameterType.LTMTag,"myLocation")
                };
            cognitiveAction3.OutputList =
                new List<CognitiveActionParameter>() { new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "ClosestRestaurant") };
            cognitiveAction3.SuccessTarget = new CognitiveActionTarget(-1, "A002.00010");
            cognitiveItem3.CognitiveActionList.Add(cognitiveAction3);
            agent.LongTermMemory.ItemList.Add(cognitiveItem3);

            Pattern outputPattern3 = new Pattern("<ClosestRestaurant> is the closest one.");
            OutputItem outputItem3 = new OutputItem("A002.00010", new List<Pattern> { outputPattern3 }, null, null);
            agent.LongTermMemory.AddItem(outputItem3);
            #endregion

            #region - Second part of Major - Museum

            InputAction inputAction4 = new InputAction(new List<Pattern>() { new Pattern("[Can you recommend me a museum, do you have any good recommendation of museum]?") }, "A003.00002");
            InputItem inputItem4 = new InputItem("A003.00001", new List<InputAction> { inputAction4 }, null);
            inputItem4.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem4);

            Pattern outputPattern4 = new Pattern("[Certainly, Absolutely, Of cause].What [type, kind] of museum are you interested in?");
            OutputItem outputItem4 = new OutputItem("A003.00002", new List<Pattern> { outputPattern4 }, "A003.00003", null);
            agent.LongTermMemory.AddItem(outputItem4);

            InputAction inputAction5 = new InputAction(new List<Pattern>() { new Pattern("[I am interested in, { }] <museumCategory+>") }, "A003.00004");
            InputItem inputItem5 = new InputItem("A003.00003", new List<InputAction> { inputAction5 }, null);
            agent.LongTermMemory.ItemList.Add(inputItem5);

            // Action to find all museum within specific category
            CognitiveItem cognitiveItem5 = new CognitiveItem();
            cognitiveItem5.ID = "A003.00004";
            GetListFromLTM cognitiveAction51 = new GetListFromLTM();
            cognitiveAction51.InputList = new List<CognitiveActionParameter>()
                {
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "museumCategory"),
                       new CognitiveActionParameter(CognitiveActionParameterType.LTMCategory, "museumCategory")
                };
            cognitiveAction51.OutputList =
                new List<CognitiveActionParameter>() { new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "ListOfItem") };
            cognitiveAction51.SuccessTarget = new CognitiveActionTarget(-1, "A003.00005");
            cognitiveAction51.FailureTarget = new CognitiveActionTarget(-1, "A010.00001");
            cognitiveItem5.CognitiveActionList.Add(cognitiveAction51);
            agent.LongTermMemory.ItemList.Add(cognitiveItem5);

            Pattern displayPattern61 = new Pattern(" Here is a List of the <museumCategory> museum.");
            OutputItem outputItem61 = new OutputItem("A003.00005", new List<Pattern>() { displayPattern61 }, "A003.00006", null);
            agent.LongTermMemory.ItemList.Add(outputItem61);

            // Display list of museum in specific topic
            Pattern displayPattern6 = new Pattern("<ListOfItem>");
            OutputItem outputItem6 = new OutputItem("A003.00006", new List<Pattern>() { displayPattern6 }, "A003.00007", null);
            outputItem6.OutputDestination = OutputDestination.Display;  
            agent.LongTermMemory.ItemList.Add(outputItem6);

            Pattern outputPattern62 = new Pattern("[I am sorry, I am afraid I cant find any museum within this category. I am not able to find those museum.]");
            OutputItem outputItem62 = new OutputItem("A010.00001", new List<Pattern> { outputPattern62 }, "A010.00002", null);
            agent.LongTermMemory.ItemList.Add(outputItem62);

            OutputItem outputItem64 = new OutputItem("A010.00002", new List<Pattern> {new Pattern(" [Here is a list of the category, {}], you can choose between.") }, "A010.00003", null);
            agent.LongTermMemory.ItemList.Add(outputItem64);
            
            OutputItem outputItem63 = new OutputItem("A010.00003", new List<Pattern>() { displayPattern6 }, "A003.00003", null);
            outputItem63.OutputDestination = OutputDestination.Display;  
            agent.LongTermMemory.ItemList.Add(outputItem63);

            // Removing the list
            Pattern displayPattern71 = new Pattern(DisplaySource.None + "|" + "Paris.jpg");
            displayPattern71.UseVerbatim = true;  /
            OutputItem outputItem71 = new OutputItem("A003.00007", new List<Pattern>() { displayPattern71 }, "A003.00008", null);
            outputItem71.OutputDestination = OutputDestination.Display;  
            agent.LongTermMemory.ItemList.Add(outputItem71);

            InputAction inputAction8 = new InputAction(new List<Pattern>() { new Pattern("[Which one is largest, which is the largest one]") }, "A003.00009");
            InputItem inputItem8 = new InputItem("A003.00008", new List<InputAction> { inputAction8 }, null);
            agent.LongTermMemory.AddItem(inputItem8);

            // Action for calculate the largest one by ranknig
            CognitiveItem cognitiveItem8 = new CognitiveItem();
            cognitiveItem8.ID = "A003.00009";
            FindLargestByTagAction cognitiveAction8 = new FindLargestByTagAction();
            cognitiveAction8.InputList = new List<CognitiveActionParameter>()
                {
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "ListOfItem"),
                       new CognitiveActionParameter(CognitiveActionParameterType.LTMTag, "ranking"),
                       new CognitiveActionParameter(CognitiveActionParameterType.LTMTag,"myLocation")
                };
            cognitiveAction8.OutputList =
                new List<CognitiveActionParameter>() { new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "ItemWithLargestCategory") };
            cognitiveAction8.SuccessTarget = new CognitiveActionTarget(-1, "A003.00010");
            cognitiveAction8.FailureTarget = new CognitiveActionTarget(-1, "A010.0004");
            cognitiveItem8.CognitiveActionList.Add(cognitiveAction8);
            agent.LongTermMemory.ItemList.Add(cognitiveItem8);

            Pattern outputPattern8 = new Pattern("<ItemWithLargestCategory> is the Largest one.");
            OutputItem outputItem8 = new OutputItem("A003.00010", new List<Pattern> { outputPattern8 }, null, null);
            agent.LongTermMemory.AddItem(outputItem8);
            #endregion

            #region - Major dialouge, third part - book a table
            Pattern inputPattern9 = new Pattern("[Could you do me a favor, can you help me, by the way could you help me] to [book a table, make a reservation]");
            InputAction inputAction9 = new InputAction(new List<Pattern>() { inputPattern9}, "A004.00002");
            InputItem inputItem9 = new InputItem("A004.00001", new List<InputAction> { inputAction9 }, null);
            inputItem9.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem9);

            Pattern outputPattern10 = new Pattern("[Absolutely, Certainly, Sure, of cause, my pleasure to help you, {}]. is it for today or tomorrow");
            OutputItem outputItem10 = new OutputItem("A004.00002", new List<Pattern> { outputPattern10 }, "A004.00003", null);
            agent.LongTermMemory.AddItem(outputItem10);
            InputAction inputAction10 = new InputAction(new List<Pattern>() { new Pattern("<dateReservation>") }, "A004.00004");
            InputItem inputItem10 = new InputItem("A004.00003", new List<InputAction> { inputAction10 }, null);
            agent.LongTermMemory.AddItem(inputItem10);

            Pattern outputPattern11 = new Pattern("For how many [people, person]");
            OutputItem outputItem11 = new OutputItem("A004.00004", new List<Pattern> { outputPattern11 }, "A004.00005", null);
            agent.LongTermMemory.AddItem(outputItem11);
            InputAction inputAction11 = new InputAction(new List<Pattern>() { new Pattern("[I want to make a reservation for, for {}] <peopleReservation> [people,person] ") }, "A004.00006");
            InputItem inputItem11 = new InputItem("A004.00005", new List<InputAction> { inputAction11 }, null);
            agent.LongTermMemory.AddItem(inputItem11);


            Pattern outputPattern12 = new Pattern("which restaurant[would you like to, do you want to] make the reservation?");
            OutputItem outputItem12 = new OutputItem("A004.00006", new List<Pattern> { outputPattern12 }, "A004.00007", null);
            agent.LongTermMemory.AddItem(outputItem12);

            // Alt 1. When knowing the restaurant (saved as restaurantChoice
            // Alt 2. Need recommendation of restaurants
            InputAction inputAction121 = new InputAction(new List<Pattern>() { new Pattern("[I want to make a reservation, {}] at <restaurantChoice>") }, "A004.00008");
            Pattern pattern122 = new Pattern("[Do you have any recommendation, what do you recommend, I don't know, I don't know yet, I am not sure]");
            InputAction inputAction122 = new InputAction(new List<Pattern>() { pattern122 }, "A004.10004");
            InputItem inputItem12 = new InputItem("A004.00007", new List<InputAction> { inputAction121, inputAction122 }, null);
            agent.LongTermMemory.ItemList.Add(inputItem12);

            Pattern outputPattern131 = new Pattern("[Let me help you, Let me give you some recommendation, Let's see, I will help you to find one]. What food are you interested in?");
            OutputItem outputItem131 = new OutputItem("A004.10004", new List<Pattern> { outputPattern131 }, "A002.00003", null);
            agent.LongTermMemory.AddItem(outputItem131);

            // Continue for alt.1
            Pattern outputPattern132 = new Pattern("[Let me, i will] help you to make a reservation at <restaurantChoice>.");
            OutputItem outputItem132 = new OutputItem("A004.00008", new List<Pattern> { outputPattern132 }, "A004.00009", null);
            agent.LongTermMemory.AddItem(outputItem132);


            InputAction inputAction13 = new InputAction(new List<Pattern>() { new Pattern("[Thank you, thanks]") }, "A004.10009");
            InputItem inputItem13 = new InputItem("A004.00009", new List<InputAction> { inputAction13 }, null);
            agent.LongTermMemory.AddItem(inputItem13);

            // Action to find the phone number
            CognitiveItem cognitiveItem13 = new CognitiveItem();
            cognitiveItem13.ID = "A004.10009";
            GetPhoneNumber cognitiveAction13 = new GetPhoneNumber();
            cognitiveAction13.InputList = new List<CognitiveActionParameter>()
                {
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "restaurantChoice"),
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "item"),
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "phoneNumber")
                };
            cognitiveAction13.OutputList =
            new List<CognitiveActionParameter>() { new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "phoneNumberOfItem") };
            cognitiveAction13.SuccessTarget = new CognitiveActionTarget(-1, "A004.00012");
            cognitiveAction13.FailureTarget = new CognitiveActionTarget(-1, "A004.00011");
            cognitiveItem13.CognitiveActionList.Add(cognitiveAction13);
            agent.LongTermMemory.ItemList.Add(cognitiveItem13);

            Pattern outputPattern14 = new Pattern("[I am sorry, {}] I am not able to find this restaurant.");
            OutputItem outputItem14 = new OutputItem("A004.00011", new List<Pattern> { outputPattern14 }, "A004.10004", null);
            agent.LongTermMemory.AddItem(outputItem14);

            Pattern outputPattern15 = new Pattern("Ok, please wait a minute. I will call the restaurant for a reservation.");
            OutputItem outputItem15 = new OutputItem("A004.00012", new List<Pattern> { outputPattern15 }, "A004.00013", null);
            agent.LongTermMemory.AddItem(outputItem15);

            Pattern outputPattern16 = new Pattern("<phoneNumberOfItem>");
            OutputItem outputItem16 = new OutputItem("A004.00013", new List<Pattern> { outputPattern16 }, "A004.00014", null);
            agent.LongTermMemory.AddItem(outputItem16);

            OutputItem outputItem109 =
            new OutputItem("A004.00014", new List<Pattern>() { new Pattern("phoneup") }, "A004.00015", null);
            outputItem109.OutputDestination = OutputDestination.Animation; 
            agent.LongTermMemory.ItemList.Add(outputItem109);

            Pattern outputPattern17 = new Pattern("[Hello, hi] I would like to make a reservation <dateReservation> for <peopleReservation> people");
            OutputItem outputItem17 = new OutputItem("A004.00015", new List<Pattern> { outputPattern17 }, "A004.00016", null);
            agent.LongTermMemory.AddItem(outputItem17);
            
            OutputItem outputItem110 =
           new OutputItem("A004.00016", new List<Pattern>() { new Pattern("phonedown") }, "A004.00017", null);
            outputItem110.OutputDestination = OutputDestination.Animation; 
            agent.LongTermMemory.ItemList.Add(outputItem110);

            // Continue for alt 2. When returning from recommendation of restaurant
            Pattern outputPattern133 = new Pattern("[Let me, i will] help you to make a reservation at <restaurantChoice>.");
            OutputItem outputItem133= new OutputItem("A004.10008", new List<Pattern> { outputPattern133 }, "A004.10009", null); 
            agent.LongTermMemory.AddItem(outputItem133);

            // Action for finding the phone number
            CognitiveItem cognitiveItem133 = new CognitiveItem();
            cognitiveItem133.ID = "A004.10009";
            GetPhoneNumber cognitiveAction133 = new GetPhoneNumber();
            cognitiveAction133.InputList = new List<CognitiveActionParameter>()
                {
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "restaurantChoice"),
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "item"),
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "phoneNumber")
                };
            cognitiveAction133.OutputList =
            new List<CognitiveActionParameter>() { new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "phoneNumberOfItem") };
            cognitiveAction133.SuccessTarget = new CognitiveActionTarget(-1, "A004.10012");
            cognitiveAction133.FailureTarget = new CognitiveActionTarget(-1, "A004.10011");
            cognitiveItem133.CognitiveActionList.Add(cognitiveAction133);
            agent.LongTermMemory.ItemList.Add(cognitiveItem133);

            Pattern outputPattern143 = new Pattern("[I am sorry, {}] I am not able to find this restaurant.");
            OutputItem outputItem143 = new OutputItem("A004.10011", new List<Pattern> { outputPattern143 }, "A004.10004", null);
            agent.LongTermMemory.AddItem(outputItem143);

            Pattern outputPattern153 = new Pattern("Ok, please wait a minute. I will call the restaurant for a reservation.");
            OutputItem outputItem153 = new OutputItem("A004.10012", new List<Pattern> { outputPattern153 }, "A004.10013", null);
            agent.LongTermMemory.AddItem(outputItem153);

            Pattern outputPattern163 = new Pattern("<phoneNumberOfItem>");
            OutputItem outputItem163 = new OutputItem("A004.10013", new List<Pattern> { outputPattern163 }, "A004.10014", null);
            agent.LongTermMemory.AddItem(outputItem163);

            OutputItem outputItem171 =
            new OutputItem("A004.10014", new List<Pattern>() { new Pattern("phoneup") }, "A004.10015", null);
            outputItem171.OutputDestination = OutputDestination.Animation; // Will be sent to 3D visualizer (if any)
            agent.LongTermMemory.ItemList.Add(outputItem171);

            Pattern outputPattern173 = new Pattern("[Hello, hi] I would like to make a reservation <dateReservation> for <peopleReservation> people");
            OutputItem outputItem173 = new OutputItem("A004.10015", new List<Pattern> { outputPattern173 }, "A004.10016", null);
            agent.LongTermMemory.AddItem(outputItem173);

            OutputItem outputItem172 =
           new OutputItem("A004.10016", new List<Pattern>() { new Pattern("phonedown") }, "A004.10017", null);
            outputItem172.OutputDestination = OutputDestination.Animation; // Will be sent to 3D visualizer (if any)
            agent.LongTermMemory.ItemList.Add(outputItem172);

            #endregion

            #endregion




            if (configuration != null) { startButton.Enabled = true; }
            saveAgentToolStripMenuItem.Enabled = true;
        }


        private void runConsistencyCheckButton_Click(object sender, EventArgs e)
        {
            consistencyCheckColorListBox.Items.Clear();
            List<string> reportList;
            System.Boolean ok = agent.CheckConsistency(out reportList);
            if (ok)
            {
                agentStatusLabel.Text = "OK";
                agentStatusStrip.BackColor = Color.Lime;
                agentStatusStrip.ForeColor = Color.Black;
            }
            else
            {
                agentStatusLabel.Text = "Not OK";
                agentStatusStrip.BackColor = Color.Red;
                agentStatusStrip.ForeColor = Color.White;
            }
            if (reportList.Count > 0)
            {
                foreach (string reportFragment in reportList)
                {
                    Color foreColor = Color.Empty;
                    Color backColor = Color.Empty;
                    if (reportFragment.StartsWith("[Error]"))
                    {
                        backColor = Color.White;
                        foreColor = Color.Red;
                    }
                    else if (reportFragment.StartsWith("[Warning]"))
                    {
                        backColor = Color.Black;
                        foreColor = Color.Yellow;
                    }
                    ColorListBoxItem reportItem = new ColorListBoxItem(reportFragment, backColor, foreColor);
                    consistencyCheckColorListBox.Items.Add(reportItem);
                }
            }
            else
            {
                ColorListBoxItem reportItem = new ColorListBoxItem("No errors or warnings detected", consistencyCheckColorListBox.BackColor, Color.Lime);
                consistencyCheckColorListBox.Items.Add(reportItem);
            }
        }
    }
}
