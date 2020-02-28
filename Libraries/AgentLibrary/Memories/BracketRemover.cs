using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary.Memories
{
    public class BracketRemover
    {
        public static string Process(string stringWithBrackets)
        {
            int length = stringWithBrackets.Length;
            string stringWithoutBrackets = stringWithBrackets.Remove(stringWithBrackets.Length - 1, 1).Remove(0, 1);
            return stringWithoutBrackets;
        }
    }
}
