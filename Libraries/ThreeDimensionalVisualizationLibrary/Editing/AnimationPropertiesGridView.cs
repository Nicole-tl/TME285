using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ObjectSerializerLibrary;
using ThreeDimensionalVisualizationLibrary.Animations;
using ThreeDimensionalVisualizationLibrary.Objects;
using MathematicsLibrary.Geometry;

namespace ThreeDimensionalVisualizationLibrary.Editing
{
    public partial class AnimationPropertiesGridView : DataGridView
    {
        protected const int SCROLL_BAR_WIDTH = 30;

        protected Animation animation;
        protected Scene3D scene;
        protected string formatString = "0.000";
        protected List<Object3D> expandedObjectList; // Unnested.
        protected string oldAnimationName;
        protected Animation oldAnimation;

     //   public event EventHandler NameIsUnique = null;
        public event EventHandler AnimationNameChanged = null;

        public AnimationPropertiesGridView()
        {
            InitializeComponent();
        }

        private void Clear()
        {
            this.Rows.Clear();
            this.Columns.Clear();
            this.Columns.Add("propertyColumn", "Property");
            this.Columns.Add("valueColumn", "Value");
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AllowUserToOrderColumns = false;
            this.AllowUserToResizeColumns = true;
            this.AllowUserToResizeRows = false;
            this.Columns[0].ReadOnly = true;
            this.RowHeadersVisible = false;
            OnResize(null);
        }

     /*   protected void OnNameIsUnique()
        {
            if (NameIsUnique != null)
            {
                EventHandler handler = NameIsUnique;
                handler(this, EventArgs.Empty);
            }
        }  */

        protected void OnAnimationNameChanged()
        {
            if (AnimationNameChanged != null)
            {
                EventHandler handler = AnimationNameChanged;
                handler(this, EventArgs.Empty);
            }
        }

        protected Boolean CheckNameIsUnique(string name)
        {
            List<string> animationNameList = new List<string>();
            foreach (Animation animation in scene.AnimationList)
            {
                animationNameList.Add(animation.Name);
            }
            if (animationNameList.Contains(name)) { return false; }
            else { return true; }
        }

        public void SetScene(Scene3D scene)
        {
            this.scene = scene;
            if (scene == null) { return; }
            if (scene.ObjectList == null) { return; }
            if (scene.ObjectList.Count == 0) { return; }
            expandedObjectList = scene.GetAllObjects(null, true);
            InstantiateAnimation(0);
            ShowAnimation(-1);
        }

        protected void InstantiateAnimation(int objectIndex)
        {
            animation = new Animation();
            List<string> animationNameList = new List<string>();
            foreach (Animation animation in scene.AnimationList) { animationNameList.Add(animation.Name); }
            int index = scene.AnimationList.Count;
            string animationName = "Animation" + index.ToString();
            while (animationNameList.Contains(animationName))
            {
                index++;
                animationName = "Animation" + index.ToString();
            }
            animation.Name = animationName;
            Object3D object3D = expandedObjectList[objectIndex];
            animation.ObjectName = object3D.Name;
            animation.FirstKeyFrameName = object3D.KeyFrameList[0].Name;
            animation.SecondKeyFrameName = object3D.KeyFrameList[0].Name;
            animation.FirstDeltaPosition = new Vector3D(0,0,0);
            animation.FirstDeltaRotation = new Vector3D(0,0,0);
            animation.SecondDeltaPosition = new Vector3D(0, 0, 0);
            animation.SecondDeltaRotation = new Vector3D(0, 0, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Columns.Count < 2) { return; }
            int columnWidth = (int)(Math.Truncate((this.Width - SCROLL_BAR_WIDTH) / 2.0));
            this.Columns[0].Width = columnWidth;
            this.Columns[1].Width = columnWidth;
        }

        protected override void OnCellEnter(DataGridViewCellEventArgs e)
        {
            base.OnCellEnter(e);
            if (e.RowIndex == 0)
            {
                oldAnimation = (Animation)ObjectCopier.Copy(animation);
             //   oldAnimationName = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            }
        }

        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            base.OnCellEndEdit(e);
            if (e.RowIndex == 0)
            {
                string name = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                Boolean nameIsUnique = CheckNameIsUnique(name);
                if (nameIsUnique)
                {
                    animation.Name = name;
                    if (animation.Name != oldAnimation.Name)
                    {
                        if (nameIsUnique)
                        {
                            OnAnimationNameChanged();
                        }
                    }
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = animation.Name;
                }
            }
            else if (e.RowIndex == 3)
            {
                animation.Duration1 = double.Parse(Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            }
            else if (e.RowIndex == 5)
            {
                string positionString = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                Vector3D deltaPosition = null;
                Boolean ok = Vector3D.TryParse(positionString, out deltaPosition);
                if (ok)
                {
                    animation.FirstDeltaPosition = deltaPosition;
                }
                else
                {
                    animation.FirstDeltaPosition = oldAnimation.FirstDeltaPosition;
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = animation.FirstDeltaPosition.AsString(formatString);
                }
            }
            else if (e.RowIndex == 6)
            {
                string rotationString = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                Vector3D deltaRotation = null;
                Boolean ok = Vector3D.TryParse(rotationString, out deltaRotation);
                if (ok)
                {
                    animation.FirstDeltaRotation = deltaRotation;
                }
                else
                {
                    animation.FirstDeltaRotation = oldAnimation.FirstDeltaRotation;
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = animation.FirstDeltaRotation.AsString(formatString);
                }
            }
            else if (e.RowIndex == 7)
            {
                animation.Duration2 = double.Parse(Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            }
            else if (e.RowIndex == 9)
            {
                string positionString = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                Vector3D deltaPosition = null;
                Boolean ok = Vector3D.TryParse(positionString, out deltaPosition);
                if (ok)
                {
                    animation.SecondDeltaPosition = deltaPosition;
                }
                else
                {
                    animation.SecondDeltaPosition = oldAnimation.SecondDeltaPosition;
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = animation.SecondDeltaPosition.AsString(formatString);
                }
            }
            else if (e.RowIndex == 10)
            {
                string rotationString = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                Vector3D deltaRotation = null;
                Boolean ok = Vector3D.TryParse(rotationString, out deltaRotation);
                if (ok)
                {
                    animation.SecondDeltaRotation = deltaRotation;
                }
                else
                {
                    animation.SecondDeltaRotation = oldAnimation.SecondDeltaRotation;
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = animation.SecondDeltaRotation.AsString(formatString);
                }
            }
        }

        protected override void OnCurrentCellDirtyStateChanged(EventArgs e)
        {
            base.OnCurrentCellDirtyStateChanged(e);
            if (IsCurrentCellDirty)
            {
                CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
            if (CurrentCell.RowIndex == 1)
            {
                string isSynchronousString = CurrentCell.Value.ToString();
                animation.IsSynchronous = Boolean.Parse(isSynchronousString);
            }
            else if (CurrentCell.RowIndex == 2)
            {
                string objectName = CurrentCell.Value.ToString();
                animation.ObjectName = objectName;
                Object3D object3D = expandedObjectList.Find(o => o.Name == objectName);
                List<string> keyFrameNameList = new List<string>();
                foreach (KeyFrame keyFrame in object3D.KeyFrameList)
                {
                    keyFrameNameList.Add(keyFrame.Name);
                }
                ((DataGridViewComboBoxCell)Rows[4].Cells[1]).DataSource = keyFrameNameList;
                ((DataGridViewComboBoxCell)Rows[8].Cells[1]).DataSource = keyFrameNameList;
                Rows[4].Cells[1].Value = object3D.KeyFrameList[0].Name;
                Rows[8].Cells[1].Value = object3D.KeyFrameList[0].Name;
                animation.FirstDeltaPosition = new Vector3D(0,0,0);
                Rows[5].Cells[1].Value = animation.FirstDeltaPosition.AsString(formatString);
                animation.FirstDeltaRotation = new Vector3D(0,0,0);
                Rows[6].Cells[1].Value = animation.FirstDeltaRotation.AsString(formatString);
                animation.SecondDeltaPosition = new Vector3D(0, 0, 0);
                Rows[9].Cells[1].Value = animation.SecondDeltaPosition.AsString(formatString);
                animation.SecondDeltaRotation = new Vector3D(0, 0, 0);
                Rows[10].Cells[1].Value = animation.SecondDeltaRotation.AsString(formatString);
            }
            else if (CurrentCell.RowIndex == 4)
            {
                string firstKeyFrameName = CurrentCell.Value.ToString();
                animation.FirstKeyFrameName = firstKeyFrameName;
            }
            else if (CurrentCell.RowIndex == 8)
            {
                string secondKeyFrameName = CurrentCell.Value.ToString();
                animation.SecondKeyFrameName = secondKeyFrameName;
            }
        }

        public void ShowAnimation(int animationIndex)
        {
            Clear();
            if (animationIndex >= 0)
            {
                animation = scene.AnimationList[animationIndex];
            }
            else { return; }
            this.Rows.Add(new string[] { "Name", animation.Name });
            this.Rows.Add(new string[] { "IsSynchronous", animation.IsSynchronous.ToString() });
            DataGridViewComboBoxCell isSynchronousCell = new DataGridViewComboBoxCell();
            List<string> options = new List<string>() { "true", "false" };
            isSynchronousCell.DataSource = options;
            isSynchronousCell.Value = animation.IsSynchronous.ToString().ToLower();  // objectNameList[0];
            this.Rows[1].Cells[1] = isSynchronousCell;
            this.Rows.Add(new string[] { "Object", animation.ObjectName });
            DataGridViewComboBoxCell objectCell = new DataGridViewComboBoxCell();
            List<string> objectNameList = new List<string>();
            foreach (Object3D object3D in expandedObjectList) { objectNameList.Add(object3D.Name); }
            objectCell.DataSource = objectNameList;
            objectCell.Value = animation.ObjectName; // objectNameList[0];
            this.Rows[2].Cells[1] = objectCell;
            this.Rows.Add(new string[] { "Duration1", animation.Duration1.ToString(formatString) });
            this.Rows.Add(new string[] { "FirstKeyFrameName", animation.FirstKeyFrameName});
            DataGridViewComboBoxCell firstKeyFrameCell = new DataGridViewComboBoxCell();
            Object3D currentObject = expandedObjectList.Find(o => o.Name == animation.ObjectName);
            List<string> keyFrameNameList = new List<string>();
            foreach (KeyFrame keyFrame in currentObject.KeyFrameList)
            {
                keyFrameNameList.Add(keyFrame.Name);
            }
            firstKeyFrameCell.DataSource = keyFrameNameList;
            firstKeyFrameCell.Value = animation.FirstKeyFrameName;
            this.Rows[4].Cells[1] = firstKeyFrameCell;       
            this.Rows.Add(new string[] { "FirstDeltaPosition", animation.FirstDeltaPosition.AsString(formatString) });
            this.Rows.Add(new string[] { "FirstDeltaRotation", animation.FirstDeltaRotation.AsString(formatString) });
            this.Rows.Add(new string[] { "Duration2", animation.Duration2.ToString(formatString) });
            this.Rows.Add(new string[] { "SecondKeyFrameName", animation.SecondKeyFrameName });
            DataGridViewComboBoxCell secondKeyFrameCell = new DataGridViewComboBoxCell();
            secondKeyFrameCell.DataSource = keyFrameNameList;
            secondKeyFrameCell.Value = animation.SecondKeyFrameName;
            this.Rows[8].Cells[1] = secondKeyFrameCell;
            this.Rows.Add(new string[] { "SecondDeltaPosition", animation.SecondDeltaPosition.AsString(formatString) });
            this.Rows.Add(new string[] { "SecondDeltaRotation", animation.SecondDeltaRotation.AsString(formatString) });
        }

        public Animation Animation
        {
            get { return animation; }
        }
    }
}
