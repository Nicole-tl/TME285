using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Cognition;

namespace AgentApplication.AddedClasses
{
    // Note to students:
    // You can use this file as a starting point when generating cognitive actions.
    // The three methods below must be implemented.

    [DataContract]
    public class CognitiveActionTemplate: CognitiveAction
    {
        public override List<CognitiveActionParameterType> GetRequiredParameterTypeInputList()
        {
            throw new NotImplementedException();
        }

        public override List<CognitiveActionParameterType> GetRequiredParameterTypeOutputList()
        {
            throw new NotImplementedException();
        }

        public override CognitiveActionTarget Process()
        {
            throw new NotImplementedException();
        }
    }
}
