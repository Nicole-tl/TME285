using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommunicationLibrary;
using CustomUserControlsLibrary;
using ImageProcessingLibrary.Cameras;
using ImageProcessingLibrary.FaceDetection;


namespace VisionApplication
{
    public partial class VisionMainForm : Form
    {
        private const int DEFAULT_CAMERA_WIDTH = 640;
        private const int DEFAULT_CAMERA_HEIGHT = 480;
        private const int DEFAULT_FRAME_RATE = 25;
        private const int CAMERA_SETUP_REQUIRED_WIDTH = 1100;
        private const int CAMERA_SETUP_REQUIRED_HEIGHT = 550;
        private const int DEFAULT_DEVICE_INDEX = 0;
        private const string CLIENT_NAME = "Vision";
        private const string DEFAULT_IP_ADDRESS = "127.0.0.1";
        private const int DEFAULT_PORT = 7;

        private const string DEFAULT_START_MESSAGE = "StartVision";
        private const string DEFAULT_STOP_MESSAGE = "StopVision";

        private Camera camera;
        private int cameraWidth = DEFAULT_CAMERA_WIDTH;
        private int cameraHeight = DEFAULT_CAMERA_HEIGHT;
        private int frameRate = DEFAULT_FRAME_RATE;
        private int deviceIndex = DEFAULT_DEVICE_INDEX;
        private int formWidth;
        private int formHeight;
        private Boolean formSizeSet = false;
        private Boolean exitRequested = false;

        private string ipAddress = DEFAULT_IP_ADDRESS;
        private int port = DEFAULT_PORT;
        private Client client = null;

        private Boolean running = false;
        private string startMessage = DEFAULT_START_MESSAGE;
        private string stopMessage = DEFAULT_STOP_MESSAGE;

        public Boolean faceDe = true;
        public string message = "FaceLost";
        //
        // Note to students: This is just an example - you
        // may certainly wish to improve upon this simple
        // detector based on skin pixels.You may also wish
        // to add other features, e.g. face recognition (which
        // are not included in the IPASrc, except for the
        // abstract base class FaceRecognizer.
        //
        // In order to write your own FaceDetector, define
        // a new class under AddedClasses (in the VisionApplication)
        // that inherits from the abstract base class FaceDetector
        // i.e. public class <SOME_SUITABLE_NAME>: FaceDetector etc.
        // and implement the ProcessBitmap() method as required.
        //
        // You can of course also add motion detection - see the
        // VideoProcessingApplication under Demonstrations/, as 
        // well as the classes in ImageProcessingLibrary.MotionDetection.
        //
        private MyFaceDetector faceDetector;

        public VisionMainForm()
        {
            InitializeComponent();
            Boolean initializationOK = Initialize();
            if (initializationOK)
            {
                // Starting the camera with 40 seconds delay 
                System.Threading.Thread.Sleep(40000);
                Start();
                Connect();
            }
            

        }


        private Boolean Initialize()
        {
            formWidth = this.Width;
            formHeight = this.Height;
            camera = new Camera();
            camera.CameraStopped += new EventHandler(HandleCameraStopped);
            if (CaptureDevice.GetDeviceNames().Count == 0)
            {
                MessageBox.Show("Please connect a camera!");
                exitToolStripMenuItem.Enabled = true;
                return false;
            }
            else
            {
                deviceNameComboBox.Items.Clear();
                List<string> deviceNameList = Camera.GetDeviceNames();
                foreach (string deviceName in deviceNameList)
                {
                    deviceNameComboBox.Items.Add(deviceName);
                }
                deviceNameComboBox.SelectedIndex = deviceIndex;
                return true;
            }
        }

        private void HandleClientProgress(object sender, CommunicationProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => ShowProgress(e)));
            }
            else { ShowProgress(e); }
        }

        private void ShowProgress(CommunicationProgressEventArgs e)
        {
            ColorListBoxItem item;
            item = new ColorListBoxItem(e.Message, communicationLogColorListBox.BackColor, communicationLogColorListBox.ForeColor);
            communicationLogColorListBox.Items.Insert(0, item);
        }

        private void HandleClientError(object sender, CommunicationErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => ShowError(e)));
            }
            else { ShowError(e); }
        }

        private void ShowError(CommunicationErrorEventArgs e)
        {
            ColorListBoxItem item;
            item = new ColorListBoxItem(e.Message, communicationLogColorListBox.BackColor, communicationLogColorListBox.ForeColor);
            communicationLogColorListBox.Items.Insert(0, item);
        }

        private void HandleCameraStopped(object sender, EventArgs e)
        {
            if (exitRequested)
            {
                Application.Exit();
            }
            else
            {
                if (InvokeRequired) { this.Invoke(new MethodInvoker(() => startCameraButton.Enabled = true)); }
                else { startCameraButton.Enabled = true; }
                running = false;
            }
        }

        private void Start()
        {
            startCameraButton.Enabled = false;
            deviceNameComboBox.Enabled = false;
            camera.DeviceName = Camera.GetDeviceNames()[deviceIndex];
            camera.FrameRate = frameRate;
            camera.ImageWidth = cameraWidth;
            camera.ImageHeight = cameraHeight;
            camera.Start();
            faceDetector = new MyFaceDetector();
            faceDetector.SetCamera(camera);
            faceDetector.Start();
            faceDetectionControl.SetCamera(camera);
            faceDetectionControl.SetFacedetector(faceDetector);
            faceDetectionControl.Start();
            faceDetectionControl.ShowProcessedBitmap = showProcessedBitmapToolStripMenuItem.Checked;
            faceDetectionControl.ShowBoundingBox = showBoundingBoxToolStripMenuItem.Checked;
            faceDetectionControl.ShowCenterLine = showCenterLineToolStripMenuItem.Checked;
            faceDetector.FaceBoundingBoxAvailable += new EventHandler<FaceDetectionEventArgs>(HandleFaceDetected);             //Added line
            stopCameraButton.Enabled = true;
            running = true;
        }

        private void HandleFaceDetected(object sender, FaceDetectionEventArgs e)
        {
            if (faceDe == true)
            {
                message = "FaceDetected";
                faceDe = false;
            }
            else
            {
                message = "FaceLost";
            }
            client.Send(message); // sends the message to the agent
            faceDe = false;

        }

        private void HandleClientReceived(object sender, DataPacketEventArgs e)
        {
            string message = e.DataPacket.Message;
            if (message == startMessage)
            {
                if (!running)
                {
                    Start();
                }
            }
            else if (message == stopMessage)
            {
                if (running)
                {
                    Stop();
                }
            }
        }

        private void Stop()
        {
            if (InvokeRequired) { this.Invoke(new MethodInvoker(() => stopCameraButton.Enabled = false)); }
            else { stopCameraButton.Enabled = false; }
            faceDetectionControl.Stop(); // Stops also the face detector.
            camera.Stop();
        }


        #region GUI action methods
        private void stopCameraButton_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void startCameraButton_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void showProcessedBitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            faceDetectionControl.ShowProcessedBitmap = showProcessedBitmapToolStripMenuItem.Checked;
        }

        private void showBoundingBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            faceDetectionControl.ShowBoundingBox = showBoundingBoxToolStripMenuItem.Checked;
        }

        private void showCenterLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            faceDetectionControl.ShowCenterLine = showCenterLineToolStripMenuItem.Checked;
        }

        private void sendToAgentButton_Click(object sender, EventArgs e)
        {
            string testMessage = testMessageTextBox.Text;
            client.Send(testMessage);
        }

        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!formSizeSet)
            {
                formWidth = this.Width;
                formHeight = this.Height;
                formSizeSet = true;
            }
            if (mainTabControl.SelectedTab == cameraSetupTabPage)
            {
                this.Width = CAMERA_SETUP_REQUIRED_WIDTH;
                this.Height = CAMERA_SETUP_REQUIRED_HEIGHT;
                cameraSetupControl.SetCamera(camera);
            }
            else
            {
                this.Width = formWidth;
                this.Height = formHeight;
            }
        }

        private void VisionMainForm_ResizeEnd(object sender, EventArgs e)
        {
            if (mainTabControl.SelectedTab != cameraSetupTabPage)
            {
                cameraSetupControl.Stop();
                formWidth = this.Width;
                formHeight = this.Height;
            }
            else
            {
                cameraSetupControl.SetCamera(camera);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (camera.Connected)
            {
                exitRequested = true;
                faceDetectionControl.Stop();
                cameraSetupControl.Stop();
                camera.Stop();
            }
            else
            {
                Application.Exit();
            }
        }

        private void VisionMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (camera.Connected)
            {
                exitRequested = true;
                faceDetectionControl.Stop();
                cameraSetupControl.Stop();
                camera.Stop();
            }
            else
            {
                Application.Exit();
            }
        }
        #endregion

        #region Communication-related methods
        private void Connect()
        {
            client = new Client();
            client.Error += new EventHandler<CommunicationErrorEventArgs>(HandleClientError); 
            client.Progress += new EventHandler<CommunicationProgressEventArgs>(HandleClientProgress);
            client.Received += new EventHandler<DataPacketEventArgs>(HandleClientReceived);
            client.ConnectionEstablished += new EventHandler(HandleConnectionEstablished);
            client.ConnectionClosed += new EventHandler(HandleConnectionClosed);
            client.Name = CLIENT_NAME;
            client.Connect(ipAddress, port);
        }

        private void ShowConnectionStatus(Boolean connected)
        {
            if (connected)
            {
                connectToAgentToolStripMenuItem.Enabled = false;
                sendToAgentButton.Enabled = true;
                connectionStatusStrip.BackColor = Color.Lime;
                connectionStatusPrefixLabel.ForeColor = Color.Black;
                connectionStatusLabel.ForeColor = Color.Black;
                connectionStatusLabel.Text = "Yes";
            }
            else
            {
                connectToAgentToolStripMenuItem.Enabled = true;
                sendToAgentButton.Enabled = false;
                connectionStatusStrip.BackColor = Color.Red;
                connectionStatusPrefixLabel.ForeColor = Color.White;
                connectionStatusLabel.ForeColor = Color.White;
                connectionStatusLabel.Text = "No";
            }
        }

        private void HandleConnectionEstablished(object sender, EventArgs e)
        {
            if (InvokeRequired) { this.Invoke(new MethodInvoker(() => ShowConnectionStatus(true))); }
            else { ShowConnectionStatus(true); }
        }

        private void HandleConnectionClosed(object sender, EventArgs e)
        {
            if (InvokeRequired) { this.Invoke(new MethodInvoker(() => ShowConnectionStatus(false))); }
            else { ShowConnectionStatus(false); }
        }
        #endregion

        private void connectToAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void VisionMainForm_Load(object sender, EventArgs e)
        {
            this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            this.Top = 0;
            Invalidate();
        }
    }
}
