using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AgentLibrary;
using AgentLibrary.Cognition;
using AgentLibrary.EventArgsClasses;
using AgentLibrary.Internet;
using AgentLibrary.IO;
using AgentLibrary.Memories;
using AgentLibrary.Patterns;
using AgentLibrary.UserControls;
using AudioLibrary;
using AuxiliaryLibrary;
using CommunicationLibrary;
using CustomUserControlsLibrary;
using ObjectSerializerLibrary;
using SpeechLibrary;
using SpeechLibrary.Visualization;
using SpeechLibrary.Modification.Resampling;
using ThreeDimensionalVisualizationLibrary;
using ThreeDimensionalVisualizationLibrary.Animations;
using ThreeDimensionalVisualizationLibrary.Objects;
using Microsoft.Speech.Synthesis;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using AgentApplication.AddedClasses;

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

            // NOTE TO STUDENTS: this is just an example: You must write your own
            // downloader class (derived from the base class DataDownloader) and
            // place it in the AddedClasses/ folder.

            yelpDataDownloader dataDownloader = new yelpDataDownloader();  
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


            // MINOR DIALOGUE
            #region
            // Politness - 1
            InputAction inputAction1P = new InputAction(new List<Pattern>() { new Pattern("Good morning") }, "A001.00002");
            InputItem inputItem1P = new InputItem("A001.00001", new List<InputAction> { inputAction1P }, null);
            inputItem1P.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem1P);

            List<Pattern> outputPatternList1P = new List<Pattern>() { new Pattern("Good morning.") };
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
            OutputItem outputItem2P = new OutputItem("A001.00005", outputPatternList2P, null, null);
            agent.LongTermMemory.AddItem(outputItem2P);
            List<Pattern> outputPatternList3P = new List<Pattern>() { new Pattern("I'm sorry to hear that.") };
            OutputItem outputItem3P = new OutputItem("A001.00006", outputPatternList3P, null, null);
            agent.LongTermMemory.AddItem(outputItem3P);
            #endregion

            #region
            // Name region - 1
            InputAction inputAction1N = new InputAction(new List<Pattern>() { new Pattern("[By the way,{}] what is your name?") }, "A001.00008");
            InputItem inputItem1N = new InputItem("A001.00007", new List<InputAction> { inputAction1N }, null);
            inputItem1N.IsEntryPoint = true;
            inputItem1N.InputActionList.Add(inputAction1N);
            agent.LongTermMemory.ItemList.Add(inputItem1N);

            Pattern outputPattern11N = new Pattern("My name is Bob, what is [yours, your name]?");
            OutputItem outputItem11N = new OutputItem("A001.00008", new List<Pattern> { outputPattern11N }, "A001.00009", null);
            outputItem11N.SuppressOutputRepetition = false;   // NO NEED TO REPITITION (?)
            agent.LongTermMemory.ItemList.Add(outputItem11N);


            Pattern pattern2N = new Pattern();           
            pattern2N.PatternSpecification = "[My name is, {}] <name1>";
            InputAction inputAction2N = new InputAction(new List<Pattern>() { pattern2N }, "A001.000010");
            InputItem inputItem2N = new InputItem("A001.00009", new List<InputAction> { inputAction2N }, null);
            agent.LongTermMemory.ItemList.Add(inputItem2N);


            Pattern outputPattern12N = new Pattern("Nice to meet you <name1>.");
            OutputItem outputItem12N = new OutputItem("A001.000010", new List<Pattern> { outputPattern12N }, "A001.00011", null);
            outputItem12N.SuppressOutputRepetition = false;  
            agent.LongTermMemory.ItemList.Add(outputItem12N);

            // Years old
            InputAction inputAction13N = new InputAction(new List<Pattern>() { new Pattern("[By the way,{}] how old are you?") }, "A001.00013");
            InputItem inputItem13N = new InputItem("A001.00012", new List<InputAction> { inputAction13N }, null);
            inputItem13N.IsEntryPoint = true;
            inputItem13N.InputActionList.Add(inputAction13N);
            agent.LongTermMemory.ItemList.Add(inputItem13N);

            //Pattern outputPattern13N = new Pattern("I am 10 years old, [what about you, and you, how about you, how old are you, {}] ?"); //Depends on animation
            Pattern outputPattern13N = new Pattern("I am 10 years old"); //Depends on animation
            OutputItem outputItem13N = new OutputItem("A001.00013", new List<Pattern> { outputPattern13N }, "A001.00014", null);
            outputItem13N.SuppressOutputRepetition = false;   // NO NEED TO REPITITION
            agent.LongTermMemory.ItemList.Add(outputItem13N);

            #endregion

            #region
            // Time - 2
            InputAction inputAction2T = new InputAction(new List<Pattern>() { new Pattern("What time is it?") }, "A001.00016");
            InputItem inputItem2T = new InputItem("A001.00015", new List<InputAction> { inputAction2T }, null);
            inputItem2T.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem2T);

            // If defining like this, the program will run and store this when staring the program. So I need an action to repeatly "calculate/find" the current time.
            //string hourOfToday = DateTime.Now.Hour.ToString();
            //string minOfToday = DateTime.Now.Minute.ToString();
            //Pattern outputPattern2T = new Pattern(hourOfToday + ":" + minOfToday);

            CognitiveItem cognitiveItem2T = new CognitiveItem();
            cognitiveItem2T.ID = "A001.00016";
            AddedClasses.GetCurrentTimeAction cognitiveAction2T = new AddedClasses.GetCurrentTimeAction();
            //cognitiveAction1.InputList = new List<CognitiveActionParameter>()
            //{ new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "imageName") };
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
            InputAction inputAction21D = new InputAction(new List<Pattern>() { new Pattern("What <weekday> is it today?") }, "A001.00021");
            InputItem inputItem21D = new InputItem("A001.00020", new List<InputAction> { inputAction21D }, null);
            inputItem21D.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem21D);

            CognitiveItem cognitiveItem21D = new CognitiveItem();
            cognitiveItem21D.ID = "A001.00021";
            GetCurrentWeekdayAction cognitiveAction21D = new GetCurrentWeekdayAction();
            cognitiveAction21D.InputList = new List<CognitiveActionParameter>()
            { new CognitiveActionParameter(CognitiveActionParameterType.WMTag,"weekday")};
            cognitiveAction21D.OutputList = new List<CognitiveActionParameter>()
            { new CognitiveActionParameter(CognitiveActionParameterType.WMTag,"weekdayOfToday")};
            cognitiveAction21D.SuccessTarget = new CognitiveActionTarget(-1, "A001.00022");
            cognitiveItem21D.CognitiveActionList.Add(cognitiveAction21D);
            agent.LongTermMemory.ItemList.Add(cognitiveItem21D);

            Pattern outputPattern21D = new Pattern("It is <weekdayOfToday> Today");
            OutputItem outputItem21D = new OutputItem("A001.00022", new List<Pattern> { outputPattern21D}, null, null);
            agent.LongTermMemory.AddItem(outputItem21D);



            #endregion


            // Major dialougue - city guide
            #region
            // First part - recommend a resturant
            InputAction inputAction1 = new InputAction(new List<Pattern>() { new Pattern("Can you recommend me a resturant?") }, "A002.00002");
            InputItem inputItem1 = new InputItem("A002.00001", new List<InputAction> { inputAction1 }, null);
            inputItem1.IsEntryPoint = true;    // Entrypoint
            agent.LongTermMemory.AddItem(inputItem1);

            Pattern outputPattern1 = new Pattern("Certainly. What food are you interested in?");
            OutputItem outputItem1 = new OutputItem("A002.00002", new List<Pattern> { outputPattern1 }, "A002.00003", null);
            agent.LongTermMemory.AddItem(outputItem1);

            InputAction inputAction2 = new InputAction(new List<Pattern>() { new Pattern("[I am interested in, { }] <cuisine>") }, "A002.00004");
            InputItem inputItem2 = new InputItem("A002.00003", new List<InputAction> { inputAction2 }, "CuisinQuery");
            agent.LongTermMemory.ItemList.Add(inputItem2);

            CognitiveItem cognitiveItem2 = new CognitiveItem();
            cognitiveItem2.ID = "A002.00004";
            GetListFromLTM cognitiveAction21 = new GetListFromLTM();
            cognitiveAction21.InputList = new List<CognitiveActionParameter>()
                {
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "cuisine"),
                       new CognitiveActionParameter(CognitiveActionParameterType.LTMCategory, "cuisine")
                };
            cognitiveAction21.OutputList =
                new List<CognitiveActionParameter>() { new CognitiveActionParameter(CognitiveActionParameterType.LTMTag, "ListOfItem") };
            cognitiveAction21.SuccessTarget = new CognitiveActionTarget(-1, "A002.00005");
            cognitiveAction21.FailureTarget = new CognitiveActionTarget(1, null);
            cognitiveItem2.CognitiveActionList.Add(cognitiveAction21);


            // If cuisine not found
            GetListFromLTM cognitiveAction22 = new GetListFromLTM();
            cognitiveAction21.InputList = new List<CognitiveActionParameter>()
                {
                       new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "cuisine"),
                       new CognitiveActionParameter(CognitiveActionParameterType.LTMCategory, "cuisine")
                };
            cognitiveAction22.OutputList =
                new List<CognitiveActionParameter>() { new CognitiveActionParameter(CognitiveActionParameterType.WMTag, "ListOfItem") };
            cognitiveAction22.SuccessTarget = new CognitiveActionTarget(2, null);
            cognitiveAction22.FailureTarget = new CognitiveActionTarget(1, "A000.0001");
            // Add to Cognitive Item 2
            cognitiveItem2.CognitiveActionList.Add(cognitiveAction22);
            agent.LongTermMemory.ItemList.Add(cognitiveItem2);


            Pattern displayPattern2 = new Pattern("ListOfItem");
            displayPattern2.UseVerbatim = true;  // Required to aviod processing the pattern
            OutputItem outputItem2 = new OutputItem("A002.00005", new List<Pattern>() { displayPattern2 }, null, null);
            outputItem2.OutputDestination = OutputDestination.Display;  // To display
            agent.LongTermMemory.ItemList.Add(outputItem2);

            // PRINT LIST OF LTM
            /*Pattern outputPattern2 = new Pattern("Here is a list of <cuisine> resturants. <ListOfItem>");
            OutputItem outputItem2 = new OutputItem("A002.00005", new List<Pattern> { outputPattern2}, null, null);
            outputItem2.OutputDestination = OutputDestination.Display;  // To display
            agent.LongTermMemory.ItemList.Add(outputItem2);*/

            // GET LIST OF CUISINES FROM LTM DOES NOT WORK...
            Pattern outputPattern3 = new Pattern("Can you rephrase that please? We only got ****");
            OutputItem outputItem3 = new OutputItem("A000.00001", new List<Pattern> { outputPattern3 }, null, null);
            outputItem3.SuppressOutputRepetition = false;
            agent.LongTermMemory.ItemList.Add(outputItem3);

            #endregion




            if (configuration != null) { startButton.Enabled = true; }
            saveAgentToolStripMenuItem.Enabled = true;
        }

        #endregion

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
