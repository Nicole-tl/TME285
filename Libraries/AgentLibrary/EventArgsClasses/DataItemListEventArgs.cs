using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentLibrary.Memories;
using ObjectSerializerLibrary;

namespace AgentLibrary.EventArgsClasses
{
    public class DataItemListEventArgs : EventArgs
    {
        private List<DataItem> dataItemList;

        public DataItemListEventArgs(List<DataItem> dataItemList)
        {
            this.dataItemList = new List<DataItem>();
            foreach (DataItem dataItem in dataItemList)
            {
                DataItem addedItem = (DataItem)ObjectCopier.Copy(dataItem);
                this.dataItemList.Add(addedItem);
            }
        }

        public List<DataItem> DataItemList
        {
            get { return dataItemList; }
        }
    }
}
