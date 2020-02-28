using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomUserControlsLibrary;
using AgentLibrary.Memories;

namespace AgentLibrary.Visualization
{
    public partial class WorkingMemoryTextViewer : ColorListBox
    {
        private Memory memory;
        private DateTime timeOfLastUpdate;
        private const string DEFAULT_INDENTATION_STRING = "  ";
        private const string DEFAULT_SEPARATOR_STRING = "-----------------------------------------------------------------------------------";

        private Color newColor = Color.LightBlue;
        private Color oldColor = Color.LightBlue;
        private Color separatorColor = Color.Silver;

        public WorkingMemoryTextViewer()
        {
            InitializeComponent();
        }

        public void SetMemory(Memory memory)
        {
            timeOfLastUpdate = DateTime.MinValue;
            this.memory = memory;
            ShowMemory();
        }

        public void ShowMemory()
        {
            this.Items.Clear();
            if (memory == null) { return; }
            List<MemoryItem> itemList = memory.TryGetAllItems();
            if (itemList.Count > 0)
            {
                itemList.Reverse();
                foreach (MemoryItem memoryItem in itemList)
                {
                    Color foreColor = oldColor;
                    if (memoryItem.TimeStamp.Add(new TimeSpan(0,0,0,0,50)) > timeOfLastUpdate) { foreColor = newColor; }
                    DataItem dataItem = (DataItem)memoryItem;
                    string itemID = "ID: " + dataItem.ID;
                    ColorListBoxItem item = new ColorListBoxItem(itemID, this.BackColor, foreColor);
                    this.Items.Add(item);
                    string predecessorID = "Predecessor ID: ";
                    if (dataItem.PredecessorID == null) { predecessorID += "null"; }
                    else { predecessorID += dataItem.PredecessorID; }
                    item = new ColorListBoxItem(predecessorID, this.BackColor, foreColor);
                    this.Items.Add(item);
                    foreach (TagValueUnit tagValueUnit in dataItem.ContentList)
                    {
                        string tagValueUnitAsString = DEFAULT_INDENTATION_STRING + tagValueUnit.AsString();
                        item = new ColorListBoxItem(tagValueUnitAsString, this.BackColor, foreColor);
                        this.Items.Add(item);
                    }
                    item = new ColorListBoxItem(DEFAULT_SEPARATOR_STRING, this.BackColor, separatorColor);
                    this.Items.Add(item);
                }
                timeOfLastUpdate = DateTime.Now;
            }
        }

    }
}
