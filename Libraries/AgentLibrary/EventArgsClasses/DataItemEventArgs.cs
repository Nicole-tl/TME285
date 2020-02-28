using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Memories;
using ObjectSerializerLibrary;

namespace AgentLibrary.EventArgsClasses
{
    public class DataItemEventArgs: EventArgs
    {
        private DataItem dataItem;

        public DataItemEventArgs(DataItem dataItem)
        {
            this.dataItem = (DataItem)ObjectCopier.Copy(dataItem);
        }

        public DataItem DataItem
        {
            get { return dataItem; }
        }
    }
}
