namespace AgentApplication
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newAgentConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAgentConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAgentConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.loadAgentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAgentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.importLTMDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sampleAgentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.agent1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.speechInputVisibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.startButton = new System.Windows.Forms.ToolStripButton();
            this.stopButton = new System.Windows.Forms.ToolStripButton();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.displayTabPage = new System.Windows.Forms.TabPage();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.secondarySplitContainerLeft = new System.Windows.Forms.SplitContainer();
            this.speechRecognitionControl = new SpeechLibrary.Visualization.SpeechRecognitionControl();
            this.recordingToolStrip = new System.Windows.Forms.ToolStrip();
            this.toggleRecordingButton = new System.Windows.Forms.ToolStripButton();
            this.viewer3D = new ThreeDimensionalVisualizationLibrary.Viewer3D();
            this.secondarySplitContainerRight = new System.Windows.Forms.SplitContainer();
            this.outputTextBox = new AgentLibrary.UserControls.OutputTextBox();
            this.inputTextBox = new AgentLibrary.UserControls.InputTextBox();
            this.displayControl = new AgentLibrary.UserControls.DisplayControl();
            this.workingMemoryTabPage = new System.Windows.Forms.TabPage();
            this.workingMemoryTextViewer = new AgentLibrary.Visualization.WorkingMemoryTextViewer();
            this.configurationTabPage = new System.Windows.Forms.TabPage();
            this.configurationPropertyPanel = new CustomUserControlsLibrary.PropertyPanels.PropertyPanel();
            this.toolStrip4 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.recordingDeviceComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.voiceListComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.checkTabPage = new System.Windows.Forms.TabPage();
            this.consistencyCheckColorListBox = new CustomUserControlsLibrary.ColorListBox();
            this.agentStatusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.agentStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip5 = new System.Windows.Forms.ToolStrip();
            this.runConsistencyCheckButton = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            this.displayTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.secondarySplitContainerLeft)).BeginInit();
            this.secondarySplitContainerLeft.Panel1.SuspendLayout();
            this.secondarySplitContainerLeft.Panel2.SuspendLayout();
            this.secondarySplitContainerLeft.SuspendLayout();
            this.recordingToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.secondarySplitContainerRight)).BeginInit();
            this.secondarySplitContainerRight.Panel1.SuspendLayout();
            this.secondarySplitContainerRight.Panel2.SuspendLayout();
            this.secondarySplitContainerRight.SuspendLayout();
            this.workingMemoryTabPage.SuspendLayout();
            this.configurationTabPage.SuspendLayout();
            this.toolStrip4.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.checkTabPage.SuspendLayout();
            this.agentStatusStrip.SuspendLayout();
            this.toolStrip5.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.sampleAgentsToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1887, 30);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newAgentConfigurationToolStripMenuItem,
            this.loadAgentConfigurationToolStripMenuItem,
            this.saveAgentConfigurationToolStripMenuItem,
            this.toolStripSeparator1,
            this.loadAgentToolStripMenuItem,
            this.saveAgentToolStripMenuItem,
            this.toolStripSeparator3,
            this.importLTMDataToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 26);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newAgentConfigurationToolStripMenuItem
            // 
            this.newAgentConfigurationToolStripMenuItem.Name = "newAgentConfigurationToolStripMenuItem";
            this.newAgentConfigurationToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
            this.newAgentConfigurationToolStripMenuItem.Text = "New agent configuration";
            this.newAgentConfigurationToolStripMenuItem.Click += new System.EventHandler(this.newAgentConfigurationToolStripMenuItem_Click);
            // 
            // loadAgentConfigurationToolStripMenuItem
            // 
            this.loadAgentConfigurationToolStripMenuItem.Name = "loadAgentConfigurationToolStripMenuItem";
            this.loadAgentConfigurationToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
            this.loadAgentConfigurationToolStripMenuItem.Text = "Load agent configuration";
            this.loadAgentConfigurationToolStripMenuItem.Click += new System.EventHandler(this.loadAgentConfigurationToolStripMenuItem_Click);
            // 
            // saveAgentConfigurationToolStripMenuItem
            // 
            this.saveAgentConfigurationToolStripMenuItem.Enabled = false;
            this.saveAgentConfigurationToolStripMenuItem.Name = "saveAgentConfigurationToolStripMenuItem";
            this.saveAgentConfigurationToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
            this.saveAgentConfigurationToolStripMenuItem.Text = "Save agent configuration";
            this.saveAgentConfigurationToolStripMenuItem.Click += new System.EventHandler(this.saveAgentConfigurationToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(257, 6);
            // 
            // loadAgentToolStripMenuItem
            // 
            this.loadAgentToolStripMenuItem.Name = "loadAgentToolStripMenuItem";
            this.loadAgentToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
            this.loadAgentToolStripMenuItem.Text = "Load agent";
            this.loadAgentToolStripMenuItem.Click += new System.EventHandler(this.loadAgentToolStripMenuItem_Click);
            // 
            // saveAgentToolStripMenuItem
            // 
            this.saveAgentToolStripMenuItem.Enabled = false;
            this.saveAgentToolStripMenuItem.Name = "saveAgentToolStripMenuItem";
            this.saveAgentToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
            this.saveAgentToolStripMenuItem.Text = "Save agent";
            this.saveAgentToolStripMenuItem.Click += new System.EventHandler(this.saveAgentToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(257, 6);
            // 
            // importLTMDataToolStripMenuItem
            // 
            this.importLTMDataToolStripMenuItem.Name = "importLTMDataToolStripMenuItem";
            this.importLTMDataToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
            this.importLTMDataToolStripMenuItem.Text = "Import LTM data";
            this.importLTMDataToolStripMenuItem.Click += new System.EventHandler(this.importLTMDataToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(257, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // sampleAgentsToolStripMenuItem
            // 
            this.sampleAgentsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.agent1ToolStripMenuItem});
            this.sampleAgentsToolStripMenuItem.Name = "sampleAgentsToolStripMenuItem";
            this.sampleAgentsToolStripMenuItem.Size = new System.Drawing.Size(121, 26);
            this.sampleAgentsToolStripMenuItem.Text = "Sample agents";
            // 
            // agent1ToolStripMenuItem
            // 
            this.agent1ToolStripMenuItem.Name = "agent1ToolStripMenuItem";
            this.agent1ToolStripMenuItem.Size = new System.Drawing.Size(144, 26);
            this.agent1ToolStripMenuItem.Text = "Agent 1";
            this.agent1ToolStripMenuItem.Click += new System.EventHandler(this.agent1ToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.speechInputVisibleToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(76, 26);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // speechInputVisibleToolStripMenuItem
            // 
            this.speechInputVisibleToolStripMenuItem.CheckOnClick = true;
            this.speechInputVisibleToolStripMenuItem.Name = "speechInputVisibleToolStripMenuItem";
            this.speechInputVisibleToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.speechInputVisibleToolStripMenuItem.Text = "Speech input visible";
            this.speechInputVisibleToolStripMenuItem.Click += new System.EventHandler(this.speechInputVisibleToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startButton,
            this.stopButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 30);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1887, 31);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // startButton
            // 
            this.startButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.startButton.Enabled = false;
            this.startButton.Image = ((System.Drawing.Image)(resources.GetObject("startButton.Image")));
            this.startButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(44, 28);
            this.startButton.Text = "Start";
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stopButton.Enabled = false;
            this.stopButton.Image = ((System.Drawing.Image)(resources.GetObject("stopButton.Image")));
            this.stopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(44, 28);
            this.stopButton.Text = "Stop";
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.displayTabPage);
            this.mainTabControl.Controls.Add(this.workingMemoryTabPage);
            this.mainTabControl.Controls.Add(this.configurationTabPage);
            this.mainTabControl.Controls.Add(this.checkTabPage);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 61);
            this.mainTabControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(1887, 942);
            this.mainTabControl.TabIndex = 3;
            // 
            // displayTabPage
            // 
            this.displayTabPage.Controls.Add(this.mainSplitContainer);
            this.displayTabPage.Location = new System.Drawing.Point(4, 29);
            this.displayTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.displayTabPage.Name = "displayTabPage";
            this.displayTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.displayTabPage.Size = new System.Drawing.Size(1879, 909);
            this.displayTabPage.TabIndex = 0;
            this.displayTabPage.Text = "Display";
            this.displayTabPage.UseVisualStyleBackColor = true;
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(4, 5);
            this.mainSplitContainer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.secondarySplitContainerLeft);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.secondarySplitContainerRight);
            this.mainSplitContainer.Size = new System.Drawing.Size(1871, 899);
            this.mainSplitContainer.SplitterDistance = 776;
            this.mainSplitContainer.SplitterWidth = 6;
            this.mainSplitContainer.TabIndex = 0;
            // 
            // secondarySplitContainerLeft
            // 
            this.secondarySplitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.secondarySplitContainerLeft.Location = new System.Drawing.Point(0, 0);
            this.secondarySplitContainerLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.secondarySplitContainerLeft.Name = "secondarySplitContainerLeft";
            this.secondarySplitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // secondarySplitContainerLeft.Panel1
            // 
            this.secondarySplitContainerLeft.Panel1.Controls.Add(this.speechRecognitionControl);
            this.secondarySplitContainerLeft.Panel1.Controls.Add(this.recordingToolStrip);
            // 
            // secondarySplitContainerLeft.Panel2
            // 
            this.secondarySplitContainerLeft.Panel2.Controls.Add(this.viewer3D);
            this.secondarySplitContainerLeft.Size = new System.Drawing.Size(776, 899);
            this.secondarySplitContainerLeft.SplitterDistance = 109;
            this.secondarySplitContainerLeft.SplitterWidth = 6;
            this.secondarySplitContainerLeft.TabIndex = 0;
            // 
            // speechRecognitionControl
            // 
            this.speechRecognitionControl.BackColor = System.Drawing.Color.Black;
            this.speechRecognitionControl.DetectionThreshold = 500;
            this.speechRecognitionControl.DeviceID = 0;
            this.speechRecognitionControl.DisplayDuration = 1D;
            this.speechRecognitionControl.DisplayMillisecondSleepTime = 200;
            this.speechRecognitionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.speechRecognitionControl.HighLightList = null;
            this.speechRecognitionControl.Location = new System.Drawing.Point(0, 31);
            this.speechRecognitionControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.speechRecognitionControl.MarkerList = null;
            this.speechRecognitionControl.MovingAverageLength = 10;
            this.speechRecognitionControl.Name = "speechRecognitionControl";
            this.speechRecognitionControl.RecognizeAutomatically = true;
            this.speechRecognitionControl.SampleRate = 44100;
            this.speechRecognitionControl.ShowSoundStream = false;
            this.speechRecognitionControl.SilenceTimeMargin = 0.5D;
            this.speechRecognitionControl.Size = new System.Drawing.Size(776, 78);
            this.speechRecognitionControl.StorageDuration = 3D;
            this.speechRecognitionControl.TabIndex = 1;
            // 
            // recordingToolStrip
            // 
            this.recordingToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.recordingToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleRecordingButton});
            this.recordingToolStrip.Location = new System.Drawing.Point(0, 0);
            this.recordingToolStrip.Name = "recordingToolStrip";
            this.recordingToolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.recordingToolStrip.Size = new System.Drawing.Size(776, 31);
            this.recordingToolStrip.TabIndex = 0;
            this.recordingToolStrip.Text = "toolStrip5";
            // 
            // toggleRecordingButton
            // 
            this.toggleRecordingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toggleRecordingButton.Enabled = false;
            this.toggleRecordingButton.Image = ((System.Drawing.Image)(resources.GetObject("toggleRecordingButton.Image")));
            this.toggleRecordingButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toggleRecordingButton.Name = "toggleRecordingButton";
            this.toggleRecordingButton.Size = new System.Drawing.Size(60, 28);
            this.toggleRecordingButton.Text = "Record";
            this.toggleRecordingButton.Click += new System.EventHandler(this.toggleRecordingButton_Click);
            // 
            // viewer3D
            // 
            this.viewer3D.BackColor = System.Drawing.Color.Black;
            this.viewer3D.CameraDistance = 4D;
            this.viewer3D.CameraLatitude = 0.39269908169872414D;
            this.viewer3D.CameraLongitude = 0.78539816339744828D;
            this.viewer3D.CameraTarget = ((OpenTK.Vector3)(resources.GetObject("viewer3D.CameraTarget")));
            this.viewer3D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewer3D.Location = new System.Drawing.Point(0, 0);
            this.viewer3D.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.viewer3D.Name = "viewer3D";
            this.viewer3D.Scene = null;
            this.viewer3D.ShowSurfaces = true;
            this.viewer3D.ShowVertices = false;
            this.viewer3D.ShowWireframe = false;
            this.viewer3D.ShowWorldAxes = false;
            this.viewer3D.Size = new System.Drawing.Size(776, 784);
            this.viewer3D.TabIndex = 1;
            this.viewer3D.UseSmoothShading = false;
            this.viewer3D.VSync = false;
            // 
            // secondarySplitContainerRight
            // 
            this.secondarySplitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.secondarySplitContainerRight.Location = new System.Drawing.Point(0, 0);
            this.secondarySplitContainerRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.secondarySplitContainerRight.Name = "secondarySplitContainerRight";
            this.secondarySplitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // secondarySplitContainerRight.Panel1
            // 
            this.secondarySplitContainerRight.Panel1.Controls.Add(this.outputTextBox);
            this.secondarySplitContainerRight.Panel1.Controls.Add(this.inputTextBox);
            // 
            // secondarySplitContainerRight.Panel2
            // 
            this.secondarySplitContainerRight.Panel2.Controls.Add(this.displayControl);
            this.secondarySplitContainerRight.Size = new System.Drawing.Size(1089, 899);
            this.secondarySplitContainerRight.SplitterDistance = 101;
            this.secondarySplitContainerRight.SplitterWidth = 6;
            this.secondarySplitContainerRight.TabIndex = 0;
            // 
            // outputTextBox
            // 
            this.outputTextBox.BackColor = System.Drawing.Color.Black;
            this.outputTextBox.DisplayTime = 2D;
            this.outputTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.outputTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputTextBox.ForeColor = System.Drawing.Color.Lime;
            this.outputTextBox.Location = new System.Drawing.Point(0, 64);
            this.outputTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.Size = new System.Drawing.Size(1089, 37);
            this.outputTextBox.TabIndex = 1;
            // 
            // inputTextBox
            // 
            this.inputTextBox.BackColor = System.Drawing.Color.Black;
            this.inputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inputTextBox.ForeColor = System.Drawing.Color.Lime;
            this.inputTextBox.Location = new System.Drawing.Point(0, 0);
            this.inputTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(1089, 37);
            this.inputTextBox.TabIndex = 0;
            // 
            // displayControl
            // 
            this.displayControl.BackColor = System.Drawing.Color.Black;
            this.displayControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displayControl.Location = new System.Drawing.Point(0, 0);
            this.displayControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.displayControl.Name = "displayControl";
            this.displayControl.Size = new System.Drawing.Size(1089, 792);
            this.displayControl.TabIndex = 0;
            // 
            // workingMemoryTabPage
            // 
            this.workingMemoryTabPage.Controls.Add(this.workingMemoryTextViewer);
            this.workingMemoryTabPage.Location = new System.Drawing.Point(4, 29);
            this.workingMemoryTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.workingMemoryTabPage.Name = "workingMemoryTabPage";
            this.workingMemoryTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.workingMemoryTabPage.Size = new System.Drawing.Size(1879, 896);
            this.workingMemoryTabPage.TabIndex = 4;
            this.workingMemoryTabPage.Text = "Working memory";
            this.workingMemoryTabPage.UseVisualStyleBackColor = true;
            // 
            // workingMemoryTextViewer
            // 
            this.workingMemoryTextViewer.BackColor = System.Drawing.Color.Black;
            this.workingMemoryTextViewer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.workingMemoryTextViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workingMemoryTextViewer.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.workingMemoryTextViewer.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.workingMemoryTextViewer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.workingMemoryTextViewer.FormattingEnabled = true;
            this.workingMemoryTextViewer.IntegralHeight = false;
            this.workingMemoryTextViewer.Location = new System.Drawing.Point(4, 5);
            this.workingMemoryTextViewer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.workingMemoryTextViewer.Name = "workingMemoryTextViewer";
            this.workingMemoryTextViewer.SelectedItemBackColor = System.Drawing.Color.Empty;
            this.workingMemoryTextViewer.SelectedItemForeColor = System.Drawing.Color.Empty;
            this.workingMemoryTextViewer.Size = new System.Drawing.Size(1871, 886);
            this.workingMemoryTextViewer.TabIndex = 0;
            // 
            // configurationTabPage
            // 
            this.configurationTabPage.Controls.Add(this.configurationPropertyPanel);
            this.configurationTabPage.Controls.Add(this.toolStrip4);
            this.configurationTabPage.Controls.Add(this.toolStrip3);
            this.configurationTabPage.Location = new System.Drawing.Point(4, 29);
            this.configurationTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.configurationTabPage.Name = "configurationTabPage";
            this.configurationTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.configurationTabPage.Size = new System.Drawing.Size(1879, 896);
            this.configurationTabPage.TabIndex = 2;
            this.configurationTabPage.Text = "Configuration";
            this.configurationTabPage.UseVisualStyleBackColor = true;
            // 
            // configurationPropertyPanel
            // 
            this.configurationPropertyPanel.BackColor = System.Drawing.Color.DimGray;
            this.configurationPropertyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationPropertyPanel.GenericListSizeFixed = false;
            this.configurationPropertyPanel.Location = new System.Drawing.Point(4, 61);
            this.configurationPropertyPanel.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.configurationPropertyPanel.Name = "configurationPropertyPanel";
            this.configurationPropertyPanel.Size = new System.Drawing.Size(1871, 830);
            this.configurationPropertyPanel.TabIndex = 4;
            // 
            // toolStrip4
            // 
            this.toolStrip4.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.recordingDeviceComboBox});
            this.toolStrip4.Location = new System.Drawing.Point(4, 33);
            this.toolStrip4.Name = "toolStrip4";
            this.toolStrip4.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStrip4.Size = new System.Drawing.Size(1871, 28);
            this.toolStrip4.TabIndex = 2;
            this.toolStrip4.Text = "toolStrip4";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(195, 25);
            this.toolStripLabel2.Text = "Available recording devices:";
            // 
            // recordingDeviceComboBox
            // 
            this.recordingDeviceComboBox.Name = "recordingDeviceComboBox";
            this.recordingDeviceComboBox.Size = new System.Drawing.Size(598, 28);
            this.recordingDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.recordingDeviceListBox_SelectedIndexChanged);
            // 
            // toolStrip3
            // 
            this.toolStrip3.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.voiceListComboBox});
            this.toolStrip3.Location = new System.Drawing.Point(4, 5);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStrip3.Size = new System.Drawing.Size(1871, 28);
            this.toolStrip3.TabIndex = 1;
            this.toolStrip3.Text = "toolStrip3";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(116, 25);
            this.toolStripLabel1.Text = "Available voices";
            // 
            // voiceListComboBox
            // 
            this.voiceListComboBox.Name = "voiceListComboBox";
            this.voiceListComboBox.Size = new System.Drawing.Size(598, 28);
            this.voiceListComboBox.SelectedIndexChanged += new System.EventHandler(this.availableVoicesComboBox_SelectedIndexChanged);
            // 
            // checkTabPage
            // 
            this.checkTabPage.Controls.Add(this.consistencyCheckColorListBox);
            this.checkTabPage.Controls.Add(this.agentStatusStrip);
            this.checkTabPage.Controls.Add(this.toolStrip5);
            this.checkTabPage.Location = new System.Drawing.Point(4, 29);
            this.checkTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkTabPage.Name = "checkTabPage";
            this.checkTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkTabPage.Size = new System.Drawing.Size(1879, 896);
            this.checkTabPage.TabIndex = 5;
            this.checkTabPage.Text = "Check consistency";
            this.checkTabPage.UseVisualStyleBackColor = true;
            // 
            // consistencyCheckColorListBox
            // 
            this.consistencyCheckColorListBox.BackColor = System.Drawing.Color.Black;
            this.consistencyCheckColorListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.consistencyCheckColorListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consistencyCheckColorListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.consistencyCheckColorListBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.consistencyCheckColorListBox.FormattingEnabled = true;
            this.consistencyCheckColorListBox.Location = new System.Drawing.Point(4, 32);
            this.consistencyCheckColorListBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.consistencyCheckColorListBox.Name = "consistencyCheckColorListBox";
            this.consistencyCheckColorListBox.SelectedItemBackColor = System.Drawing.Color.Empty;
            this.consistencyCheckColorListBox.SelectedItemForeColor = System.Drawing.Color.Empty;
            this.consistencyCheckColorListBox.Size = new System.Drawing.Size(1871, 833);
            this.consistencyCheckColorListBox.TabIndex = 3;
            // 
            // agentStatusStrip
            // 
            this.agentStatusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.agentStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.agentStatusLabel});
            this.agentStatusStrip.Location = new System.Drawing.Point(4, 865);
            this.agentStatusStrip.Name = "agentStatusStrip";
            this.agentStatusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.agentStatusStrip.Size = new System.Drawing.Size(1871, 26);
            this.agentStatusStrip.TabIndex = 2;
            this.agentStatusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(52, 20);
            this.toolStripStatusLabel1.Text = "Status:";
            // 
            // agentStatusLabel
            // 
            this.agentStatusLabel.Name = "agentStatusLabel";
            this.agentStatusLabel.Size = new System.Drawing.Size(36, 20);
            this.agentStatusLabel.Text = "N/A";
            // 
            // toolStrip5
            // 
            this.toolStrip5.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip5.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runConsistencyCheckButton});
            this.toolStrip5.Location = new System.Drawing.Point(4, 5);
            this.toolStrip5.Name = "toolStrip5";
            this.toolStrip5.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStrip5.Size = new System.Drawing.Size(1871, 27);
            this.toolStrip5.TabIndex = 1;
            this.toolStrip5.Text = "toolStrip5";
            // 
            // runConsistencyCheckButton
            // 
            this.runConsistencyCheckButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.runConsistencyCheckButton.Image = ((System.Drawing.Image)(resources.GetObject("runConsistencyCheckButton.Image")));
            this.runConsistencyCheckButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runConsistencyCheckButton.Name = "runConsistencyCheckButton";
            this.runConsistencyCheckButton.Size = new System.Drawing.Size(158, 24);
            this.runConsistencyCheckButton.Text = "Run consistency check";
            this.runConsistencyCheckButton.Click += new System.EventHandler(this.runConsistencyCheckButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1887, 1003);
            this.Controls.Add(this.mainTabControl);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Agent application";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.mainTabControl.ResumeLayout(false);
            this.displayTabPage.ResumeLayout(false);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.secondarySplitContainerLeft.Panel1.ResumeLayout(false);
            this.secondarySplitContainerLeft.Panel1.PerformLayout();
            this.secondarySplitContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.secondarySplitContainerLeft)).EndInit();
            this.secondarySplitContainerLeft.ResumeLayout(false);
            this.recordingToolStrip.ResumeLayout(false);
            this.recordingToolStrip.PerformLayout();
            this.secondarySplitContainerRight.Panel1.ResumeLayout(false);
            this.secondarySplitContainerRight.Panel1.PerformLayout();
            this.secondarySplitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.secondarySplitContainerRight)).EndInit();
            this.secondarySplitContainerRight.ResumeLayout(false);
            this.workingMemoryTabPage.ResumeLayout(false);
            this.configurationTabPage.ResumeLayout(false);
            this.configurationTabPage.PerformLayout();
            this.toolStrip4.ResumeLayout(false);
            this.toolStrip4.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.checkTabPage.ResumeLayout(false);
            this.checkTabPage.PerformLayout();
            this.agentStatusStrip.ResumeLayout(false);
            this.agentStatusStrip.PerformLayout();
            this.toolStrip5.ResumeLayout(false);
            this.toolStrip5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage displayTabPage;
        private System.Windows.Forms.TabPage configurationTabPage;
        private System.Windows.Forms.ToolStripMenuItem loadAgentConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAgentConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sampleAgentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton startButton;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.SplitContainer secondarySplitContainerRight;
        private AgentLibrary.UserControls.InputTextBox inputTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem loadAgentToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveAgentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newAgentConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem speechInputVisibleToolStripMenuItem;
        private System.Windows.Forms.SplitContainer secondarySplitContainerLeft;
        private ThreeDimensionalVisualizationLibrary.Viewer3D viewer3D;
        private AgentLibrary.UserControls.OutputTextBox outputTextBox;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox voiceListComboBox;
        private System.Windows.Forms.ToolStrip toolStrip4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox recordingDeviceComboBox;
        private CustomUserControlsLibrary.PropertyPanels.PropertyPanel configurationPropertyPanel;
        private System.Windows.Forms.ToolStrip recordingToolStrip;
        private System.Windows.Forms.ToolStripButton toggleRecordingButton;
        private SpeechLibrary.Visualization.SpeechRecognitionControl speechRecognitionControl;
        private System.Windows.Forms.TabPage workingMemoryTabPage;
        private AgentLibrary.Visualization.WorkingMemoryTextViewer workingMemoryTextViewer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem importLTMDataToolStripMenuItem;
        private AgentLibrary.UserControls.DisplayControl displayControl;
        private System.Windows.Forms.TabPage checkTabPage;
        private System.Windows.Forms.ToolStrip toolStrip5;
        private System.Windows.Forms.ToolStripButton runConsistencyCheckButton;
        private CustomUserControlsLibrary.ColorListBox consistencyCheckColorListBox;
        private System.Windows.Forms.StatusStrip agentStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel agentStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem agent1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton stopButton;
    }
}

