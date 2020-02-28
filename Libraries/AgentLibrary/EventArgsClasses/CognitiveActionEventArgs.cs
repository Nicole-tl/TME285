using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Cognition;
using AgentLibrary.Memories;

namespace AgentLibrary.EventArgsClasses
{
    public class CognitiveActionEventArgs
    {
        private CognitiveAction cognitiveAction;

        public CognitiveActionEventArgs(CognitiveAction cognitiveAction)
        {
            this.cognitiveAction = cognitiveAction;
        }

        public CognitiveAction CognitiveAction
        {
            get { return cognitiveAction; }
        }
    }
}
