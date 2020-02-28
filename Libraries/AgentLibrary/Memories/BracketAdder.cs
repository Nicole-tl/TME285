using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary.Memories
{
    public class BracketAdder
    {
        public static string Process(string stringWithoutBrackets)
        {
            string stringWithBrackets = "<" + stringWithoutBrackets + ">";
            return stringWithBrackets;
        }
    }
}
