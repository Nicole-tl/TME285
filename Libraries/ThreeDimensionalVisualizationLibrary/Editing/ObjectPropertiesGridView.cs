using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreeDimensionalVisualizationLibrary.Objects;

namespace ThreeDimensionalVisualizationLibrary.Editing
{
    public partial class ObjectPropertiesGridView : DataGridView
    {
        protected const int SCROLL_BAR_WIDTH = 30;

        private Object3D object3D;
        private string oldValue;

        public ObjectPropertiesGridView()
        {
            InitializeComponent();
        }

        public event EventHandler ObjectChanged = null;

        private void OnObjectChanged()
        {
            if (ObjectChanged != null)
            {
                EventHandler handler = ObjectChanged;
                handler(this, EventArgs.Empty);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Columns.Count < 2) { return; }
            int columnWidth = (int)(Math.Truncate((this.Width - SCROLL_BAR_WIDTH) / 2.0));
            this.Columns[0].Width = columnWidth;
            this.Columns[1].Width = columnWidth;
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

        // allObjectsNameList required if one wants to change the parent of the object
        public void SetObject(Object3D object3D) 
        {
            this.object3D = object3D;
            Clear();
            if (this.object3D != null)
            {
                this.Rows.Add(new string[] { "Name", object3D.Name });
                this.Rows.Add(new string[] { "RotationCenterX", object3D.RotationCenterX.ToString("0.000") });
                this.Rows.Add(new string[] { "RotationCenterY", object3D.RotationCenterY.ToString("0.000") });
                this.Rows.Add(new string[] { "RotationCenterZ", object3D.RotationCenterZ.ToString("0.000") });
                this.Rows.Add(new string[] { "CenterX", object3D.CenterX.ToString("0.000") });
                this.Rows.Add(new string[] { "CenterY", object3D.CenterY.ToString("0.000") });
                this.Rows.Add(new string[] { "CenterZ", object3D.CenterZ.ToString("0.000") });
                this.Rows.Add(new string[] { "Material name", object3D.Material.Name });
                this.Rows.Add(new string[] { "Material ambient color", object3D.Material.AmbientColorAsString(',') });
                this.Rows.Add(new string[] { "Material diffuse color", object3D.Material.DiffuseColorAsString(',') });
                this.Rows.Add(new string[] { "Material specular color", object3D.Material.SpecularColorAsString(',') });
                this.Rows.Add(new string[] { "Material shininess", object3D.Material.Shininess.ToString("0.0000") });
                this.Rows.Add(new string[] { "Material opacity", object3D.Material.Opacity.ToString("0.0000") });
                this.Rows.Add(new string[] { "Material diffuse map", object3D.Material.DiffuseMapFileName });
            }
        }

        protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
        {
            base.OnCellBeginEdit(e);
            oldValue = null;
            if (Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                oldValue = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            }
        }

        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            if (Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
            {
                Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                return;
            }
            string cellValueAsString = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            base.OnCellEndEdit(e);
            if (e.RowIndex == 0)
            {
                this.object3D.Name = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                OnObjectChanged();
            }
            else if (e.RowIndex == 1)
            {
                double x;
                Boolean ok = double.TryParse(cellValueAsString, out x);
                if (ok)
                {
                    this.object3D.RotationCenterX = x;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 2)
            {
                double y;
                Boolean ok = double.TryParse(cellValueAsString, out y);
                if (ok)
                {
                    this.object3D.RotationCenterY = y;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 3)
            {
                double z;
                Boolean ok = double.TryParse(cellValueAsString, out z);
                if (ok)
                {
                    this.object3D.RotationCenterZ = z;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
       /*     else if (e.RowIndex == 4)
            {
                double rotationX;
                Boolean ok = double.TryParse(cellValueAsString, out rotationX);
                if (ok)
                {
                    this.object3D.RotationX = rotationX;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 5)
            {
                double rotationY;
                Boolean ok = double.TryParse(cellValueAsString, out rotationY);
                if (ok)
                {
                    this.object3D.RotationY = rotationY;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 6)
            {
                double rotationZ;
                Boolean ok = double.TryParse(cellValueAsString, out rotationZ);
                if (ok)
                {
                    this.object3D.RotationZ = rotationZ;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }  */
            else if (e.RowIndex == 4)
            {
                double xCenter;
                Boolean ok = double.TryParse(cellValueAsString, out xCenter);
                if (ok)
                {
                    this.object3D.CenterX = xCenter;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 5)
            {
                double yCenter;
                Boolean ok = double.TryParse(cellValueAsString, out yCenter);
                if (ok)
                {
                    this.object3D.CenterY = yCenter;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 6)
            {
                double centerZ;
                Boolean ok = double.TryParse(cellValueAsString, out centerZ);
                if (ok)
                {
                    this.object3D.CenterZ = centerZ;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 7)
            {
                if (cellValueAsString != null)
                {
                    if (cellValueAsString.Replace(" ", "").Length > 0)
                    {
                        this.object3D.Material.Name = cellValueAsString;
                    }
                    else
                    {
                        Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                    }
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 8)
            {
                List<string> cellValueAsStringSplit = cellValueAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (cellValueAsStringSplit.Count == 3)
                {
                    float r;
                    float g;
                    float b;
                    Boolean rOK = float.TryParse(cellValueAsStringSplit[0], out r);
                    Boolean gOK = float.TryParse(cellValueAsStringSplit[1], out g);
                    Boolean bOK = float.TryParse(cellValueAsStringSplit[2], out b);
                    if ((rOK) && (gOK) && (bOK))
                    {
                        this.object3D.Material.AmbientColor = new float[] { r, g, b, object3D.Material.Opacity };
                    }
                    else
                    {
                        Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                    }
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 9)
            {
                List<string> cellValueAsStringSplit = cellValueAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (cellValueAsStringSplit.Count == 3)
                {
                    float r;
                    float g;
                    float b;
                    Boolean rOK = float.TryParse(cellValueAsStringSplit[0], out r);
                    Boolean gOK = float.TryParse(cellValueAsStringSplit[1], out g);
                    Boolean bOK = float.TryParse(cellValueAsStringSplit[2], out b);
                    if ((rOK) && (gOK) && (bOK))
                    {
                        this.object3D.Material.DiffuseColor = new float[] { r, g, b, object3D.Material.Opacity };
                    }
                    else
                    {
                        Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                    }
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 10)
            {
                List<string> cellValueAsStringSplit = cellValueAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (cellValueAsStringSplit.Count == 3)
                {
                    float r;
                    float g;
                    float b;
                    Boolean rOK = float.TryParse(cellValueAsStringSplit[0], out r);
                    Boolean gOK = float.TryParse(cellValueAsStringSplit[1], out g);
                    Boolean bOK = float.TryParse(cellValueAsStringSplit[2], out b);
                    if ((rOK) && (gOK) && (bOK))
                    {
                        this.object3D.Material.SpecularColor = new float[] { r, g, b, object3D.Material.Opacity };
                    }
                    else
                    {
                        Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                    }
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 11)
            {
                float shininess;
                Boolean ok = float.TryParse(cellValueAsString, out shininess);
                if (ok)
                {
                    this.object3D.Material.Shininess = shininess;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 12)
            {
                float opacity;
                Boolean ok = float.TryParse(cellValueAsString, out opacity);
                if (ok)
                {
                    this.object3D.Material.Opacity = opacity;
                    OnObjectChanged();
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
            else if (e.RowIndex == 13)
            {
                if (cellValueAsString != null)
                {
                    if (cellValueAsString.Replace(" ", "").Length > 0)
                    {
                        this.object3D.Material.DiffuseMapFileName = cellValueAsString;
                    }
                    else
                    {
                        Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                    }
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                }
            }
        }
    }
}
