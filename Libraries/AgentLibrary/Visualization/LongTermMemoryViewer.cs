using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AgentLibrary;
using AgentLibrary.EventArgsClasses;
using AgentLibrary.Memories;
using AgentLibrary.Cognition;
using CustomUserControlsLibrary;


namespace AgentLibrary.Visualization
{
    public partial class LongTermMemoryViewer : ScrollableZoomControl
    {
        private const double DEFAULT_WIDTH_TO_HEIGHT_RATIO = 1.6180339887;
        private const double DEFAULT_MINIMUM_ACTION_LEFT = 0.05;
        private const double DEFAULT_MAXIMUM_ACTION_RIGHT = 0.45;
        private const double DEFAULT_MINIMUM_ACTION_BOTTOM = 0.05;
        private const double DEFAULT_MAXIMUM_ACTION_TOP = 0.95;
        private const double DEFAULT_MINIMUM_DATA_LEFT = 0.55;
        private const double DEFAULT_MAXIMUM_DATA_RIGHT = 0.95;
        private const double DEFAULT_MINIMUM_DATA_BOTTOM = 0.05;
        private const double DEFAULT_MAXIMUM_DATA_TOP = 0.95;
        private const double DEFAULT_MAXIMUM_BOX_HEIGHT = 0.20;
        private const double DEFAULT_MINIMUM_SPACING = 0.01;
        private const double DEFAULT_MAXIMUM_SPACING = 0.05;
        private const int DEFAULT_FRAME_PEN_WIDTH = 4;

        private Memory longTermMemory;
        private MemoryItem selectedItem = null;
        private MemoryItem grabbedItem = null;
        private Point itemGrabPoint;
        private List<ActionItem> connectedItemList = null;

        private Color DEFAULT_INPUT_ITEM_COLOR = Color.LightBlue;
        private Color DEFAULT_COGNITIVE_ITEM_COLOR = Color.LightGreen;
        private Color DEFAULT_OUTPUT_ITEM_COLOR = Color.LightYellow;
        private Color DEFAULT_DATA_ITEM_COLOR = Color.LightSalmon;
        private Color DEFAULT_HIGHLIGHT_OFF_COLOR = Color.Gray;
        private Color DEFAULT_SELECTION_FRAME_COLOR = Color.OrangeRed;

        private Color inputItemColor;
        private Color cognitiveItemColor;
        private Color outputItemColor;
        private Color dataItemColor;
        private Color highLightOffColor;
        private Color selectionFrameColor;

        private int framePenWidth = DEFAULT_FRAME_PEN_WIDTH;

        private double widthToHeightRatio = DEFAULT_WIDTH_TO_HEIGHT_RATIO;
        private double minimumActionLeft = DEFAULT_MINIMUM_ACTION_LEFT;
        private double maximumActionRight = DEFAULT_MAXIMUM_ACTION_RIGHT;
        private double minimumActionBottom = DEFAULT_MINIMUM_ACTION_BOTTOM;
        private double maximumActionTop = DEFAULT_MAXIMUM_ACTION_TOP;
        private double minimumDataLeft = DEFAULT_MINIMUM_DATA_LEFT;
        private double maximumDataRight = DEFAULT_MAXIMUM_DATA_RIGHT;
        private double minimumDataBottom = DEFAULT_MINIMUM_DATA_BOTTOM;
        private double maximumDataTop = DEFAULT_MAXIMUM_DATA_TOP;
        private double maximumBoxHeight = DEFAULT_MAXIMUM_BOX_HEIGHT;
        private double maximumBoxWidth = DEFAULT_MAXIMUM_BOX_HEIGHT * DEFAULT_WIDTH_TO_HEIGHT_RATIO;
        private double minimumSpacing = DEFAULT_MINIMUM_SPACING;
        private double maximumSpacing = DEFAULT_MAXIMUM_SPACING;

        public event EventHandler<MemoryItemEventArgs> ItemSelected = null;

        public LongTermMemoryViewer()
        {
            InitializeComponent();
            inputItemColor = DEFAULT_INPUT_ITEM_COLOR;
            cognitiveItemColor = DEFAULT_COGNITIVE_ITEM_COLOR;
            outputItemColor = DEFAULT_OUTPUT_ITEM_COLOR;
            dataItemColor = DEFAULT_DATA_ITEM_COLOR;
            highLightOffColor = DEFAULT_HIGHLIGHT_OFF_COLOR;
            selectionFrameColor = DEFAULT_SELECTION_FRAME_COLOR;
        }

        private void OnItemSelected()
        {
            if (ItemSelected != null)
            {
                MemoryItemEventArgs e = new MemoryItemEventArgs(selectedItem);
                EventHandler<MemoryItemEventArgs> handler = ItemSelected;
                handler(this, e);
            }
        }

        /*   protected override void OnPaintBackground(PaintEventArgs e)
           {
               base.OnPaintBackground(e);
               using (SolidBrush backgroundBrush = new SolidBrush(Color.Black))
               {
                   e.Graphics.FillRectangle(backgroundBrush, 0, 0, Width, Height);
               }
           }  */

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (longTermMemory == null) { return; }
            Color itemColor = Color.Empty;
            using (SolidBrush itemBrush = new SolidBrush(Color.Empty))
            {
                foreach (MemoryItem item in longTermMemory.ItemList)
                {
                    if (item.VisualizationData.HighLightOn)
                    {
                        if (item is InputItem) { itemColor = inputItemColor; }
                        else if (item is CognitiveItem) { itemColor = cognitiveItemColor; }
                        else if (item is OutputItem) { itemColor = outputItemColor; }
                        else if (item is DataItem) { itemColor = dataItemColor; }
                    }
                    else { itemColor = highLightOffColor; }
                    itemBrush.Color = itemColor;
                    float xPlot = GetPlotXAtX(item.VisualizationData.RelativeX);
                    float yPlot = GetPlotYAtY(item.VisualizationData.RelativeY);
                    float width = GetPlotXAtX(item.VisualizationData.RelativeX + item.VisualizationData.RelativeWidth) - xPlot;
                    float height = yPlot - GetPlotYAtY(item.VisualizationData.RelativeY + item.VisualizationData.RelativeHeight);
                    e.Graphics.FillRectangle(itemBrush, xPlot, yPlot, width, height);
                    if (selectedItem != null)
                    {
                        if (item.ID == selectedItem.ID)
                        {
                            using (Pen framePen = new Pen(selectionFrameColor))
                            {
                                framePen.Width = framePenWidth;
                                e.Graphics.DrawRectangle(framePen, xPlot, yPlot, width, height);
                            }
                        }
                    }
                    string id = item.ID;
                    float idLength = e.Graphics.MeasureString(id, this.Font).Width;
                    float idHeight = e.Graphics.MeasureString(id, this.Font).Height;
                    if ((idLength < width) && (idHeight < height))
                    {
                        itemBrush.Color = Color.Black;
                        e.Graphics.DrawString(id, this.Font, itemBrush, new PointF(xPlot + (width - idLength) / 2, yPlot + (height - idHeight) / 2));
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            double relativeX = GetXAtPlotX(e.X);
            double relativeY = GetYAtPlotY(e.Y);
            if (longTermMemory == null) { return; }
            if (connectedItemList != null)
            {
                connectedItemList = null;
                grabbedItem = null;
                selectedItem = null;
                ToggleHighlight(true);
                Invalidate();
                return;
            }
            Boolean selectionMade = false;
            if (e.Button == MouseButtons.Left)
            {
                foreach (MemoryItem item in longTermMemory.ItemList)
                {
                    if (relativeX >= item.VisualizationData.RelativeX)
                    {
                        if (relativeX <= (item.VisualizationData.RelativeX + item.VisualizationData.RelativeWidth))
                        {
                            if (relativeY <= item.VisualizationData.RelativeY)
                            {
                                if (relativeY >= (item.VisualizationData.RelativeY - item.VisualizationData.RelativeHeight))
                                {
                                    selectedItem = item;
                                    selectionMade = true;
                                    grabbedItem = item;
                                    itemGrabPoint = new Point(e.X, e.Y);
                                    OnItemSelected();
                                    Invalidate();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (!selectionMade)
            {
                selectedItem = null;
                grabbedItem = null;
                OnItemSelected();
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (connectedItemList != null)
            {
                float deltaX = e.X - itemGrabPoint.X;
                float deltaY = e.Y - itemGrabPoint.Y;
                foreach (ActionItem actionItem in connectedItemList)
                {
                    float newX = GetPlotXAtX(actionItem.VisualizationData.RelativeX) + deltaX;
                    float newY = GetPlotYAtY(actionItem.VisualizationData.RelativeY) + deltaY;
                    actionItem.VisualizationData.RelativeX = GetXAtPlotX(newX);
                    actionItem.VisualizationData.RelativeY = GetYAtPlotY(newY);
                }
                itemGrabPoint = new Point(e.X, e.Y);
                Invalidate();
            }
            else if (grabbedItem != null)
            {
                float deltaX = e.X - itemGrabPoint.X;
                float deltaY = e.Y - itemGrabPoint.Y;
                float newX = GetPlotXAtX(grabbedItem.VisualizationData.RelativeX) + deltaX;
                float newY = GetPlotYAtY(grabbedItem.VisualizationData.RelativeY) + deltaY;
                grabbedItem.VisualizationData.RelativeX = GetXAtPlotX(newX);
                grabbedItem.VisualizationData.RelativeY = GetYAtPlotY(newY);
                itemGrabPoint = new Point(e.X, e.Y);
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (connectedItemList == null)
            {
                if (selectedItem != null)
                {
                    if (selectedItem is ActionItem)
                    {
                        ToggleHighlight(false);
                        connectedItemList = longTermMemory.GetItemsInGroup((ActionItem)selectedItem);
                        foreach (ActionItem item in connectedItemList)
                        {
                            item.VisualizationData.HighLightOn = true;
                        }
                        selectedItem = null;
                    }
                }
            }
            else
            {
                ToggleHighlight(true);
                connectedItemList = null;
            }
            Invalidate();
          /*  if (selectedItem != null)
            {
                if (selectedItem is ActionItem)
                {
                    if (connectedItemList == null)
                    {
                        ToggleHighlight(false);
                        connectedItemList = longTermMemory.GetItemsInDialogue((ActionItem)selectedItem);
                        foreach (ActionItem item in connectedItemList)
                        {
                            item.VisualizationData.HighLightOn = true;
                        }
                    }
                    else
                    {
                        ToggleHighlight(true);
                        connectedItemList = null;
                    }
                }
                Invalidate();
            }  */
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (connectedItemList == null) { grabbedItem = null; }
            base.OnMouseUp(e);
        }

        private void HandleMemoryChanged(object sender, EventArgs e)
        {
            ToggleHighlight(true);
            SetItemSize(false);
            Refresh();
        }

        private void SetItemSize(Boolean distribute = true)
        {
            int numberOfItems = longTermMemory.ItemList.Count;
            int numberOfDataItems = longTermMemory.ItemList.FindAll(i => (i is DataItem)).Count;
            int numberOfActionItems = numberOfItems - numberOfDataItems;

            double boxWidth = maximumBoxWidth;
            double boxHeight = maximumBoxHeight;
            double spacing = maximumSpacing;
            int nActionY = (int)Math.Truncate((maximumActionTop - minimumActionBottom + maximumSpacing) / (boxHeight + maximumSpacing));
            int nActionX = (int)Math.Truncate((maximumActionRight - minimumActionLeft + maximumSpacing) / (boxWidth + maximumSpacing));
            int nAction = nActionX * nActionY;
            while (nAction < numberOfActionItems)
            {
                boxWidth *= 0.9;
                boxHeight *= 0.9;
                spacing *= 0.9;
                nActionY = (int)Math.Truncate((maximumActionTop - minimumActionBottom + spacing) / (boxHeight + spacing));
                nActionX = (int)Math.Truncate((maximumActionRight - minimumActionLeft + spacing) / (boxWidth + spacing));
                nAction = nActionX * nActionY;
            }

            int actionItemCount = 0;
            int yPositionIndex = 0;
            int xPositionIndex = 0;
            for (int ii = 0; ii < longTermMemory.ItemList.Count; ii++)
            {
                MemoryItem item = longTermMemory.ItemList[ii];
                if (item is ActionItem)
                {
                    if (distribute)
                    {
                        item.VisualizationData.RelativeX = minimumActionLeft + xPositionIndex * (boxWidth + spacing);
                        item.VisualizationData.RelativeY = maximumActionTop - yPositionIndex * (boxHeight + spacing);
                    }
                    item.VisualizationData.RelativeWidth = boxWidth;
                    item.VisualizationData.RelativeHeight = boxHeight;
                    actionItemCount++;
                    yPositionIndex++;
                    if ((yPositionIndex % nActionY) == 0)
                    {
                        yPositionIndex = 0;
                        xPositionIndex++;
                    }
                }
            }


            boxWidth = maximumBoxWidth;
            boxHeight = maximumBoxHeight;
            spacing = maximumSpacing;
            int nDataY = (int)Math.Truncate((maximumDataTop - minimumDataBottom + maximumSpacing) / (boxHeight + maximumSpacing));
            int nDataX = (int)Math.Truncate((maximumDataRight - minimumDataLeft + maximumSpacing) / (boxWidth + maximumSpacing));
            int nData = nDataX * nDataY;
            while (nData < numberOfDataItems)
            {
                boxWidth *= 0.9;
                boxHeight *= 0.9;
                spacing *= 0.9;
                nDataY = (int)Math.Truncate((maximumDataTop - minimumDataBottom + spacing) / (boxHeight + spacing));
                nDataX = (int)Math.Truncate((maximumDataRight - minimumDataLeft + spacing) / (boxWidth + spacing));
                nData = nDataX * nDataY;
            }


            int dataItemCount = 0;
            yPositionIndex = 0;
            xPositionIndex = 0;
            for (int ii = 0; ii < longTermMemory.ItemList.Count; ii++)
            {
                MemoryItem item = longTermMemory.ItemList[ii];
                if (item is DataItem)
                {
                    if (distribute)
                    {
                        item.VisualizationData.RelativeX = minimumDataLeft + xPositionIndex * (boxWidth + spacing);
                        item.VisualizationData.RelativeY = maximumDataTop - yPositionIndex * (boxHeight + spacing);
                    }
                    item.VisualizationData.RelativeWidth = boxWidth;
                    item.VisualizationData.RelativeHeight = boxHeight;
                    dataItemCount++;
                    yPositionIndex++;
                    if ((yPositionIndex % nDataY) == 0)
                    {
                        yPositionIndex = 0;
                        xPositionIndex++;
                    }
                }
            }
        }

        public void ToggleHighlight(Boolean highLightOn)
        {
            foreach (MemoryItem item in longTermMemory.ItemList)
            {
                item.VisualizationData.HighLightOn = highLightOn;
            }
        }

        public void SetMemory(Memory longTermMemory, Boolean autoArrange)
        {
            this.SetRange(0, 1, 0, 1);
            connectedItemList = null;
            selectedItem = null;
            this.longTermMemory = longTermMemory;
            this.longTermMemory.MemoryChanged += new EventHandler(HandleMemoryChanged);
            ToggleHighlight(true);
            if (autoArrange)
            {
                SetItemSize();
            }
            Refresh();
        }

        public void Arrange()
        {
            SetItemSize();
            Refresh();
        }

        // 20190409
        public void ClearSelection()
        {
            selectedItem = null;
            Refresh();
        }
    }
}
