using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentLibrary.Cognition;
using AgentLibrary.IO;

namespace AgentLibrary.Memories
{
    [DataContract]
    public class Memory
    {
        private const int DEFAULT_LOCK_MILLISECOND_TIMEOUT = 50;

        private string name;
        private List<MemoryItem> itemList;

        private List<DataItem> dataItemList;  // Pointer list, set upon Initialize()

        public event EventHandler MemoryChanged = null;
        private static object lockObject = new object();

        public Memory()
        {
            itemList = new List<MemoryItem>();
            dataItemList = new List<DataItem>();
        }

        private void OnMemoryChanged()
        {
            if (MemoryChanged != null)
            {
                EventHandler handler = MemoryChanged;
                handler(this, EventArgs.Empty);
            }
        }

        public void Initialize()
        {
            name = Constants.LTM_NAME;
            dataItemList = new List<DataItem>();
            foreach (MemoryItem item in itemList)
            {
                if (item.GetType() == typeof(DataItem))
                {
                    dataItemList.Add((DataItem)item);
                }
            }
        }

        public void AddItem(MemoryItem item)
        {
            Monitor.Enter(lockObject);
            item.TimeStamp = DateTime.Now;
            itemList.Add(item);
            if (item.GetType() == typeof(DataItem))
            {
                dataItemList.Add((DataItem)item);
            }
            Monitor.Exit(lockObject);
            OnMemoryChanged();
        }

        private int GetNextIndexInGroup(string groupName, out int indexOfLastItemInGroup)
        {
            int lastIndexInGroup = 0;
            indexOfLastItemInGroup = 0;
            int itemIndex = 0;
            foreach (MemoryItem item in itemList)
            {
                if (item is ActionItem)
                {
                    if (item.ID.StartsWith(groupName))
                    {
                        int index = int.Parse(item.ID.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                        if (index > lastIndexInGroup)
                        {
                            lastIndexInGroup = index;
                            indexOfLastItemInGroup = itemIndex;
                        }
                    }
                }
                itemIndex++;
            }
            return lastIndexInGroup + 1;
        }

        public void AddActionItem(MemoryItem item, string groupName)
        {
            Monitor.Enter(lockObject);
            int indexOfLastItemInGroup = -1;
            int nextIndexInGroup = GetNextIndexInGroup(groupName, out indexOfLastItemInGroup);
            item.ID = groupName + "." + nextIndexInGroup.ToString(Constants.LOCAL_ID_FORMAT); // 20190409 // ("00000");
            item.TimeStamp = DateTime.Now;
            if (indexOfLastItemInGroup >= 0) { itemList.Insert(indexOfLastItemInGroup + 1, item); }
            else { itemList.Add(item); }
            Monitor.Exit(lockObject);
            OnMemoryChanged();
        }

        // 20190409
        // ToDo: Write one for WM as well?
        // NOTE, important: This method generates the ID of the item, unlike AddItem(), where the
        // ID must be set externally. For automatic addition of data items (e.g. as a result
        // of internet downloads) always use the AddLTMDataItem method:
        public void AddLTMDataItem(DataItem dataItem)
        {
            Monitor.Enter(lockObject);
            long lastDataItemID = GetLastDataItemID();
            dataItem.ID = Constants.LTM_ITEM_PREFIX + (lastDataItemID + 1).ToString(Constants.LTM_ID_FORMAT);
            itemList.Add(dataItem);
            dataItemList.Add(dataItem);  // 20191227
            Monitor.Exit(lockObject);
            OnMemoryChanged();
        }

        // 20191227
        public void RemoveLTMDataItem(string id)
        {
            Monitor.Enter(lockObject);
            int itemIndex = itemList.FindIndex(i => i.ID == id);
            int dataItemIndex = dataItemList.FindIndex(i => i.ID == id);
            if ((itemIndex >= 0) && (dataItemIndex >= 0))
            {
                itemList.RemoveAt(itemIndex);
                dataItemList.RemoveAt(dataItemIndex);
            }
            Monitor.Exit(lockObject);
            OnMemoryChanged();
        }

        public List<MemoryItem> TryGetAllItems()
        {
            List<MemoryItem> allItemsList = new List<MemoryItem>();
            if (Monitor.TryEnter(lockObject, DEFAULT_LOCK_MILLISECOND_TIMEOUT))
            {
                foreach (MemoryItem memoryItem in this.itemList)
                {
                    allItemsList.Add(memoryItem);
                }
                Monitor.Exit(lockObject);
            }
            return allItemsList;
        }

        public DataItem FindLastByTag(string tag)
        {
            int index = dataItemList.Count - 1;
            while (index >= 0)
            {
                DataItem item = dataItemList[index];
                if (item.HasTag(tag))
                {
                    return item;
                }
                index--;
            }
            return null; 
        }

        public DataItem FindLastByTagList(List<string> tagList)
        {
            int index = dataItemList.Count - 1;
            while (index >= 0)
            {
                DataItem item = dataItemList[index];
                Boolean matchFound = true;
                foreach (string tag in tagList)
                if (!item.HasTag(tag))
                {
                        matchFound = false;
                        break;
                }
                if (matchFound) { return item; }
                index--;
            }
            return null;
        }

        // MW ToDo: Make the search more efficient, needed if the
        // memory becomes very large
        public DataItem Find(List<TagValueUnit> tagValueUnitList)
        {
            foreach (DataItem item in dataItemList)
            {
                Boolean tagMatchFound = true;
                foreach (TagValueUnit tagValueUnit in tagValueUnitList)
                {
                    string tag = tagValueUnit.Tag;
                    if (!item.HasTag(tag))
                    {
                        tagMatchFound = false;
                        break;
                    }
                }
                if (tagMatchFound)
                {
                    Boolean allFound = true;
                    foreach (TagValueUnit tagValueUnit in tagValueUnitList)
                    {
                        // 20190125: Allow multiple Category tags (for example)
                        List<TagValueUnit> itemTagValueUnitList = item.ContentList.FindAll(t => t.Tag == tagValueUnit.Tag);
                        Boolean valueFound = false;
                        foreach (TagValueUnit itemTagValueUnit in itemTagValueUnitList)
                        {
                            if (String.Equals(itemTagValueUnit.Value, tagValueUnit.Value, StringComparison.OrdinalIgnoreCase))
                            {
                                valueFound = true;
                                break;
                            }
                        }
                        if (!valueFound)
                        {
                            allFound = false;
                        }
                    }
                    if (allFound)
                    {
                        return item;
                    }
                }
                else { return null; }
            }
            return null; // In case the LTM is empty ...
        }

        public DataItem FindFirst(TagValueUnit tagValueUnit)
        {
            Boolean tagMatchFound = true;
            foreach (DataItem item in dataItemList)
            { 
                string tag = tagValueUnit.Tag;
                if (!item.HasTag(tag))
                {
                    tagMatchFound = false;
                    break;
                }
                if (tagMatchFound)
                {
                    TagValueUnit itemTagValueUnit = item.ContentList.Find(t => t.Tag == tagValueUnit.Tag);
                    if (itemTagValueUnit.Value == tagValueUnit.Value) { return item; }
                }
            }
            return null; 
        }

        public DataItem FindLast(TagValueUnit tagValueUnit)
        {
            Boolean tagMatchFound = true;
            int index = dataItemList.Count - 1;
            while (index >= 0)
            {
                DataItem item = dataItemList[index];
                string tag = tagValueUnit.Tag;
                if (!item.HasTag(tag))
                {
                    tagMatchFound = false;
                    break;
                }
                if (tagMatchFound)
                {
                    TagValueUnit itemTagValueUnit = item.ContentList.Find(t => t.Tag == tagValueUnit.Tag);
                    if (itemTagValueUnit.Value == tagValueUnit.Value) { return item; }
                }
                index--;
            }
            return null; 
        }

        public List<DataItem> FindAll(List<TagValueUnit> tagValueUnitList)
        {
            List<DataItem> itemsFoundList = new List<DataItem>();
            foreach (DataItem item in dataItemList)
            {
                Boolean tagMatchFound = true;
                foreach (TagValueUnit tagValueUnit in tagValueUnitList)
                {
                    string tag = tagValueUnit.Tag;
                    if (!item.HasTag(tag))
                    {
                        tagMatchFound = false;
                        break;
                    }
                }
                if (tagMatchFound)
                {
                    Boolean allFound = true;
                    foreach (TagValueUnit tagValueUnit in tagValueUnitList)
                    {
                        // 20190125: Allow multiple Category tags (for example)
                        List<TagValueUnit> itemTagValueUnitList = item.ContentList.FindAll(t => t.Tag == tagValueUnit.Tag);
                        Boolean valueFound = false;
                        foreach (TagValueUnit itemTagValueUnit in itemTagValueUnitList)
                        {
                            if (String.Equals(itemTagValueUnit.Value, tagValueUnit.Value, StringComparison.OrdinalIgnoreCase))
                            {
                                valueFound = true;
                                break;
                            }
                        }
                        if (!valueFound)
                        {
                            allFound = false;
                        }
                    }
                    if (allFound)
                    {
                        itemsFoundList.Add(item);
                    }
                }
            }
            return itemsFoundList; 
        }

        // Note: This method might be computationally costly if the agent contains
        // many memory items. It should be applied mostly offline, before running
        // the agent, e.g. when importing data items (see the ImportDataItems() method).
        public long GetLastDataItemID()
        {
            long lastNumberedID = 0;
            List<string> dataItemIDList = new List<string>();
            foreach (MemoryItem item in itemList)
            {
                string id = item.ID;
                string prefix;
                if (this.name == Constants.LTM_NAME) { prefix = Constants.LTM_ITEM_PREFIX; }
                else { prefix = Constants.STM_ITEM_PREFIX; }
                if (id.StartsWith(prefix))
                {
                    Char prefixAsChar = prefix[0];
                    id = id.TrimStart(new char[] { prefixAsChar });
                    long integerID;
                    Boolean ok = long.TryParse(id, out integerID);
                    if (ok)
                    {
                        if (integerID > lastNumberedID)
                        {
                            lastNumberedID = integerID;
                        }
                    }
                }
            }
            return lastNumberedID;
        }

        // 20190116 To be modified!
        // 20190123 Modified to use groupID and localID
        public List<ActionItem> GetItemsInGroup(ActionItem selectedItem)
        {
            string groupID = selectedItem.GroupID;
          //  string idPrefix = selectedItem.ID.Split(new char[] { '.' })[0];
            List<ActionItem> connectedActionItemList = new List<ActionItem>();
            foreach (MemoryItem item in itemList)
            {
                if (item is ActionItem)
                {
                    if (((ActionItem)item).GroupID == groupID)
                    {
                        connectedActionItemList.Add((ActionItem)item);
                    }
                }
            }
            return connectedActionItemList;
        }

        // 20190117
        // 20190123 Modified to use groupID and localID
        public List<string> GetActionItemGroupIDList()
        {
            List<string> actionItemGroupNameList = new List<string>();
            if (itemList != null)
            {
                foreach (MemoryItem item in itemList)
                {
                    if (item is ActionItem)
                    {
                        string groupName = ((ActionItem)item).GroupID; // item.ID.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
                        if (!actionItemGroupNameList.Contains(groupName))
                        {
                            actionItemGroupNameList.Add(groupName);
                        }
                    }
                }
            }
            return actionItemGroupNameList;
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public List<MemoryItem> ItemList
        {
            get { return itemList; }
            set { itemList = value; }
        }

        public List<DataItem> DataItemList
        {
            get { return dataItemList; }
            set { dataItemList = value; }
        }
    }
}
