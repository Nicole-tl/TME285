using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Memories;

namespace AgentLibrary.EventArgsClasses
{
    public class MemoryItemEventArgs
    {
        private MemoryItem item;

        public MemoryItemEventArgs(MemoryItem item)
        {
            this.item = item;
        }

        public MemoryItem Item
        {
            get { return item; }
        }
    }
}
