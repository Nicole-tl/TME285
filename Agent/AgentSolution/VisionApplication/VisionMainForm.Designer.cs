namespace VisionApplication
{
    partial class VisionMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisionMainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToAgentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showProcessedBitmapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBoundingBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCenterLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.deviceNameComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.startCameraButton = new System.Windows.Forms.ToolStripButton();
            this.stopCameraButton = new System.Windows.Forms.ToolStripButton();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.cameraViewTabPage = new System.Windows.Forms.TabPage();
            this.faceDetectionControl = new ImageProcessingLibrary.FaceDetection.FaceDetectionControl();
            this.cameraSetupTabPage = new System.Windows.Forms.TabPage();
            this.cameraSetupControl = new ImageProcessingLibrary.Cameras.CameraSetupControl();
            this.communicationLogTabPage = new System.Windows.Forms.TabPage();
            this.communicationLogColorListBox = new CustomUserControlsLibrary.ColorListBox();
            this.connectionStatusStrip = new System.Windows.Forms.StatusStrip();
            this.connectionStatusPrefixLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.connectionStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.testMessageTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.sendToAgentButton = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            this.cameraViewTabPage.SuspendLayout();
            this.cameraSetupTabPage.SuspendLayout();
            this.communicationLogTabPage.SuspendLayout();
            this.connectionStatusStrip.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.actionsToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(651, 35);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(141, 34);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToAgentToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(87, 29);
            this.actionsToolStripMenuItem.Text = "Actions";
            // 
            // connectToAgentToolStripMenuItem
            // 
            this.connectToAgentToolStripMenuItem.Name = "connectToAgentToolStripMenuItem";
            this.connectToAgentToolStripMenuItem.Size = new System.Drawing.Size(251, 34);
            this.connectToAgentToolStripMenuItem.Text = "Connect to agent";
            this.connectToAgentToolStripMenuItem.Click += new System.EventHandler(this.connectToAgentToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showProcessedBitmapToolStripMenuItem,
            this.showBoundingBoxToolStripMenuItem,
            this.showCenterLineToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(92, 29);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // showProcessedBitmapToolStripMenuItem
            // 
            this.showProcessedBitmapToolStripMenuItem.CheckOnClick = true;
            this.showProcessedBitmapToolStripMenuItem.Name = "showProcessedBitmapToolStripMenuItem";
            this.showProcessedBitmapToolStripMenuItem.Size = new System.Drawing.Size(306, 34);
            this.showProcessedBitmapToolStripMenuItem.Text = "Show processed bitmap";
            this.showProcessedBitmapToolStripMenuItem.Click += new System.EventHandler(this.showProcessedBitmapToolStripMenuItem_Click);
            // 
            // showBoundingBoxToolStripMenuItem
            // 
            this.showBoundingBoxToolStripMenuItem.Checked = true;
            this.showBoundingBoxToolStripMenuItem.CheckOnClick = true;
            this.showBoundingBoxToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBoundingBoxToolStripMenuItem.Name = "showBoundingBoxToolStripMenuItem";
            this.showBoundingBoxToolStripMenuItem.Size = new System.Drawing.Size(306, 34);
            this.showBoundingBoxToolStripMenuItem.Text = "Show bounding box";
            this.showBoundingBoxToolStripMenuItem.Click += new System.EventHandler(this.showBoundingBoxToolStripMenuItem_Click);
            // 
            // showCenterLineToolStripMenuItem
            // 
            this.showCenterLineToolStripMenuItem.CheckOnClick = true;
            this.showCenterLineToolStripMenuItem.Name = "showCenterLineToolStripMenuItem";
            this.showCenterLineToolStripMenuItem.Size = new System.Drawing.Size(306, 34);
            this.showCenterLineToolStripMenuItem.Text = "Show center line";
            this.showCenterLineToolStripMenuItem.Click += new System.EventHandler(this.showCenterLineToolStripMenuItem_Click);
            // 
            // toolStrip2
            // 
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.deviceNameComboBox,
            this.startCameraButton,
            this.stopCameraButton});
            this.toolStrip2.Location = new System.Drawing.Point(0, 35);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStrip2.Size = new System.Drawing.Size(651, 34);
            this.toolStrip2.TabIndex = 2;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(117, 29);
            this.toolStripLabel1.Text = "Device name:";
            // 
            // deviceNameComboBox
            // 
            this.deviceNameComboBox.Name = "deviceNameComboBox";
            this.deviceNameComboBox.Size = new System.Drawing.Size(270, 34);
            // 
            // startCameraButton
            // 
            this.startCameraButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.startCameraButton.Image = ((System.Drawing.Image)(resources.GetObject("startCameraButton.Image")));
            this.startCameraButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.startCameraButton.Name = "startCameraButton";
            this.startCameraButton.Size = new System.Drawing.Size(52, 29);
            this.startCameraButton.Text = "Start";
            this.startCameraButton.Click += new System.EventHandler(this.startCameraButton_Click);
            // 
            // stopCameraButton
            // 
            this.stopCameraButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stopCameraButton.Enabled = false;
            this.stopCameraButton.Image = ((System.Drawing.Image)(resources.GetObject("stopCameraButton.Image")));
            this.stopCameraButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopCameraButton.Name = "stopCameraButton";
            this.stopCameraButton.Size = new System.Drawing.Size(53, 29);
            this.stopCameraButton.Text = "Stop";
            this.stopCameraButton.Click += new System.EventHandler(this.stopCameraButton_Click);
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.cameraViewTabPage);
            this.mainTabControl.Controls.Add(this.cameraSetupTabPage);
            this.mainTabControl.Controls.Add(this.communicationLogTabPage);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 69);
            this.mainTabControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(651, 656);
            this.mainTabControl.TabIndex = 3;
            this.mainTabControl.SelectedIndexChanged += new System.EventHandler(this.mainTabControl_SelectedIndexChanged);
            // 
            // cameraViewTabPage
            // 
            this.cameraViewTabPage.Controls.Add(this.faceDetectionControl);
            this.cameraViewTabPage.Location = new System.Drawing.Point(4, 29);
            this.cameraViewTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cameraViewTabPage.Name = "cameraViewTabPage";
            this.cameraViewTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cameraViewTabPage.Size = new System.Drawing.Size(643, 623);
            this.cameraViewTabPage.TabIndex = 0;
            this.cameraViewTabPage.Text = "Camera view";
            this.cameraViewTabPage.UseVisualStyleBackColor = true;
            // 
            // faceDetectionControl
            // 
            this.faceDetectionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.faceDetectionControl.DrawBoundingBox = true;
            this.faceDetectionControl.Location = new System.Drawing.Point(4, 5);
            this.faceDetectionControl.Name = "faceDetectionControl";
            this.faceDetectionControl.ShowBoundingBox = true;
            this.faceDetectionControl.ShowCenterLine = true;
            this.faceDetectionControl.ShowProcessedBitmap = false;
            this.faceDetectionControl.Size = new System.Drawing.Size(635, 613);
            this.faceDetectionControl.TabIndex = 0;
            // 
            // cameraSetupTabPage
            // 
            this.cameraSetupTabPage.Controls.Add(this.cameraSetupControl);
            this.cameraSetupTabPage.Location = new System.Drawing.Point(4, 29);
            this.cameraSetupTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cameraSetupTabPage.Name = "cameraSetupTabPage";
            this.cameraSetupTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cameraSetupTabPage.Size = new System.Drawing.Size(643, 623);
            this.cameraSetupTabPage.TabIndex = 1;
            this.cameraSetupTabPage.Text = "Camera setup";
            this.cameraSetupTabPage.UseVisualStyleBackColor = true;
            // 
            // cameraSetupControl
            // 
            this.cameraSetupControl.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cameraSetupControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraSetupControl.Location = new System.Drawing.Point(4, 5);
            this.cameraSetupControl.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cameraSetupControl.Name = "cameraSetupControl";
            this.cameraSetupControl.Size = new System.Drawing.Size(635, 613);
            this.cameraSetupControl.TabIndex = 0;
            // 
            // communicationLogTabPage
            // 
            this.communicationLogTabPage.Controls.Add(this.communicationLogColorListBox);
            this.communicationLogTabPage.Controls.Add(this.connectionStatusStrip);
            this.communicationLogTabPage.Controls.Add(this.toolStrip1);
            this.communicationLogTabPage.Location = new System.Drawing.Point(4, 29);
            this.communicationLogTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.communicationLogTabPage.Name = "communicationLogTabPage";
            this.communicationLogTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.communicationLogTabPage.Size = new System.Drawing.Size(643, 616);
            this.communicationLogTabPage.TabIndex = 2;
            this.communicationLogTabPage.Text = "Communication log";
            this.communicationLogTabPage.UseVisualStyleBackColor = true;
            // 
            // communicationLogColorListBox
            // 
            this.communicationLogColorListBox.BackColor = System.Drawing.Color.Black;
            this.communicationLogColorListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.communicationLogColorListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.communicationLogColorListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.communicationLogColorListBox.ForeColor = System.Drawing.Color.Lime;
            this.communicationLogColorListBox.FormattingEnabled = true;
            this.communicationLogColorListBox.IntegralHeight = false;
            this.communicationLogColorListBox.Location = new System.Drawing.Point(4, 39);
            this.communicationLogColorListBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.communicationLogColorListBox.Name = "communicationLogColorListBox";
            this.communicationLogColorListBox.SelectedItemBackColor = System.Drawing.Color.Empty;
            this.communicationLogColorListBox.SelectedItemForeColor = System.Drawing.Color.Empty;
            this.communicationLogColorListBox.Size = new System.Drawing.Size(635, 540);
            this.communicationLogColorListBox.TabIndex = 2;
            // 
            // connectionStatusStrip
            // 
            this.connectionStatusStrip.BackColor = System.Drawing.Color.Red;
            this.connectionStatusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.connectionStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionStatusPrefixLabel,
            this.connectionStatusLabel});
            this.connectionStatusStrip.Location = new System.Drawing.Point(4, 579);
            this.connectionStatusStrip.Name = "connectionStatusStrip";
            this.connectionStatusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.connectionStatusStrip.Size = new System.Drawing.Size(635, 32);
            this.connectionStatusStrip.TabIndex = 1;
            this.connectionStatusStrip.Text = "statusStrip1";
            // 
            // connectionStatusPrefixLabel
            // 
            this.connectionStatusPrefixLabel.ForeColor = System.Drawing.Color.White;
            this.connectionStatusPrefixLabel.Name = "connectionStatusPrefixLabel";
            this.connectionStatusPrefixLabel.Size = new System.Drawing.Size(106, 25);
            this.connectionStatusPrefixLabel.Text = "Connected: ";
            // 
            // connectionStatusLabel
            // 
            this.connectionStatusLabel.ForeColor = System.Drawing.Color.White;
            this.connectionStatusLabel.Name = "connectionStatusLabel";
            this.connectionStatusLabel.Size = new System.Drawing.Size(36, 25);
            this.connectionStatusLabel.Text = "No";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.testMessageTextBox,
            this.sendToAgentButton});
            this.toolStrip1.Location = new System.Drawing.Point(4, 5);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStrip1.Size = new System.Drawing.Size(635, 34);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(121, 29);
            this.toolStripLabel2.Text = "Test message:";
            // 
            // testMessageTextBox
            // 
            this.testMessageTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.testMessageTextBox.Name = "testMessageTextBox";
            this.testMessageTextBox.Size = new System.Drawing.Size(298, 34);
            // 
            // sendToAgentButton
            // 
            this.sendToAgentButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.sendToAgentButton.Enabled = false;
            this.sendToAgentButton.Image = ((System.Drawing.Image)(resources.GetObject("sendToAgentButton.Image")));
            this.sendToAgentButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.sendToAgentButton.Name = "sendToAgentButton";
            this.sendToAgentButton.Size = new System.Drawing.Size(128, 29);
            this.sendToAgentButton.Text = "Send to agent";
            this.sendToAgentButton.Click += new System.EventHandler(this.sendToAgentButton_Click);
            // 
            // VisionMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 725);
            this.Controls.Add(this.mainTabControl);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "VisionMainForm";
            this.Text = "Vision application";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VisionMainForm_FormClosing);
            this.Load += new System.EventHandler(this.VisionMainForm_Load);
            this.ResizeEnd += new System.EventHandler(this.VisionMainForm_ResizeEnd);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.mainTabControl.ResumeLayout(false);
            this.cameraViewTabPage.ResumeLayout(false);
            this.cameraSetupTabPage.ResumeLayout(false);
            this.communicationLogTabPage.ResumeLayout(false);
            this.communicationLogTabPage.PerformLayout();
            this.connectionStatusStrip.ResumeLayout(false);
            this.connectionStatusStrip.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox deviceNameComboBox;
        private System.Windows.Forms.ToolStripButton startCameraButton;
        private System.Windows.Forms.ToolStripButton stopCameraButton;
        private System.Windows.Forms.ToolStripMenuItem showProcessedBitmapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showBoundingBoxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCenterLineToolStripMenuItem;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage cameraViewTabPage;
        private System.Windows.Forms.TabPage cameraSetupTabPage;
        private ImageProcessingLibrary.Cameras.CameraSetupControl cameraSetupControl;
        private ImageProcessingLibrary.FaceDetection.FaceDetectionControl faceDetectionControl;
        private System.Windows.Forms.TabPage communicationLogTabPage;
        private System.Windows.Forms.StatusStrip connectionStatusStrip;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox testMessageTextBox;
        private System.Windows.Forms.ToolStripButton sendToAgentButton;
        private CustomUserControlsLibrary.ColorListBox communicationLogColorListBox;
        private System.Windows.Forms.ToolStripStatusLabel connectionStatusPrefixLabel;
        private System.Windows.Forms.ToolStripStatusLabel connectionStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToAgentToolStripMenuItem;
    }
}

