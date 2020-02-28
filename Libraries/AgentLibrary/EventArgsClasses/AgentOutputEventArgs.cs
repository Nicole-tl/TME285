using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.IO;

namespace AgentLibrary.EventArgsClasses
{
    public class AgentOutputEventArgs
    {
        private AgentOutput agentOutput;

        public AgentOutputEventArgs(AgentOutput agentOutput)
        {
            this.agentOutput = new AgentOutput(agentOutput.Destination, agentOutput.Message);
        }

        public AgentOutput AgentOutput
        {
            get { return agentOutput; }
        }
    }
}
