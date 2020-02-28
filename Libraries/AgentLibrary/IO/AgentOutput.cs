using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary.IO
{
    public class AgentOutput
    {
        private OutputDestination destination;
        private string message;

        public AgentOutput(OutputDestination destination, string message)
        {
            this.destination = destination;
            this.message = message;
        }

        public OutputDestination Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
