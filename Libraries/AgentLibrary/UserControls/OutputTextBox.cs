using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgentLibrary.UserControls
{
    public partial class OutputTextBox : TextBox
    {
        private const int DEFAULT_MILLISECOND_SLEEP_TIME = 200;
        private const double DEFAULT_DISPLAY_TIME = 2;

        private Thread clearThread = null;
        private Boolean running = false;
        private int millisecondSleepTime = DEFAULT_MILLISECOND_SLEEP_TIME;
        private int millisecondDisplayTime = (int)Math.Round(1000 * DEFAULT_DISPLAY_TIME);
        private int messageAge;

        public OutputTextBox()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            running = true;
            clearThread = new Thread(new ThreadStart(ClearLoop));
            clearThread.Start();
        }

        private void ClearLoop()
        {
            while (running)
            {
                Thread.Sleep(millisecondSleepTime);
                if (!running) { break; } // Needed, in case the user exists the program during the sleep interval.
                if (messageAge > millisecondDisplayTime)
                {
                    if (InvokeRequired) { this.Invoke(new MethodInvoker(() => this.Text = "")); }
                    else { this.Text = ""; }
                }
                messageAge += millisecondSleepTime;
            }
        }

        public void Stop()
        {
            running = false;
        }

        private string GenerateOutput(string currentOutput, string message)
        {
            if (currentOutput != "") { currentOutput += ". " + message; }
            else { currentOutput = message; }
            return currentOutput;
        }

        public void ShowOutput(string message)
        {
            messageAge = 0;
            if (!running) { Initialize();  }
            if (InvokeRequired) { this.Invoke(new MethodInvoker(() => this.Text = GenerateOutput(this.Text, message))); }
            else { this.Text = GenerateOutput(this.Text, message); }
        }

        public double DisplayTime
        {
            get { return (double)millisecondDisplayTime/ 1000; }
            set { millisecondDisplayTime = (int)Math.Round(1000 * value); }
        }
    }
}
