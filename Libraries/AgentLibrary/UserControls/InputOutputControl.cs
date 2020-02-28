using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AuxiliaryLibrary;

namespace AgentLibrary.UserControls
{
    public partial class InputOutputControl : UserControl
    {
        public InputOutputControl()
        {
            InitializeComponent();
        }

        public event EventHandler<StringEventArgs> InputReceived = null;

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            string message = GetMessage();
            OnInputReceived(message);
        }

        private void OnInputReceived(string message)
        {
            if (InputReceived != null)
            {
                EventHandler<StringEventArgs> handler = InputReceived;
                StringEventArgs e = new StringEventArgs(message);
                handler(this, e);
            }
        }

        // Should handle information received by the control FROM the agent
        public virtual void ReceiveFromAgent(string input)
        {
            // To be overridden in derived classes.
        }

        // Should obtain information to be sent TO the agent.
        protected virtual string GetMessage()
        {
            // To be overridden in derived classes.
            return "";
        }
    }
}
