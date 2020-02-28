using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.IO;

namespace AgentLibrary.IO
{
    public class UserInput
    {
        private InputSource source;
        private string message;

        public UserInput(InputSource source, string message)
        {
            this.source = source;
            this.message = message;
        }

        public InputSource Source
        {
            get { return source; }
            set { source = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
