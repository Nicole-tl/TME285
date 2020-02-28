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
using ThreeDimensionalVisualizationLibrary;
using ThreeDimensionalVisualizationLibrary.Materials;
using ThreeDimensionalVisualizationLibrary.Objects;

namespace ThreeDimensionalVisualizationLibrary.Editing
{
    public partial class ObjectHierarchyViewer : TreeView
    {
        private Color unHighlightColor = Color.LightGray;
        private Color highlightColor = Color.LightBlue;
        private Color specularColor = Color.White;

        private float[] selectedObjectAmbientColor = new float[] { 1f, 1f, 1f, 1f };
        private float[] selectedObjectDiffuseColor = new float[] { 1f, 1f, 1f, 1f };
        private float[] selectedObjectSpecularColor = new float[] { 1f, 1f, 1f, 1f };

        // private Scene3D scene;
        private Viewer3D viewer3D; // Pointer required to make updates in the 3D scene (visualization).
        private List<Object3D> availableObjectsList;
        private List<Object3D> newObjectList; // After drag-drop in the treeview

        private Object3D previousSelectedObject = null;
        private Object3D selectedObject = null;

        private Material highlightMaterial;
        private Material storedMaterial;

        public event EventHandler<Object3DEventArgs> SelectionChanged = null;


        public ObjectHierarchyViewer()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            this.BackColor = Color.Black;
            this.ForeColor = Color.Lime;
            this.BorderStyle = BorderStyle.None;
            HideSelection = false; // Keeps selection even when focus is lost.
            highlightMaterial = new Material();
            highlightMaterial.SetAmbientColor(highlightColor);
            highlightMaterial.SetDiffuseColor(highlightColor);
            highlightMaterial.SetSpecularColor(specularColor);
            highlightMaterial.Shininess = 100; // To do: Parameterize
        }

        private void OnSelectionChanged(Object3D selectedObject)
        {
            if (SelectionChanged != null)
            {
                EventHandler<Object3DEventArgs> handler = SelectionChanged;
                Object3DEventArgs e = new Object3DEventArgs(selectedObject);
                handler(this, e);
            }
        }

        private void RegenerateObject(TreeNode parent, Object3D parentObject)
        {
            if (parent == null)
            {
                newObjectList = new List<Object3D>();
                foreach (TreeNode treeNode in this.Nodes[0].Nodes)
                {
                    Object3D object3D = availableObjectsList.Find(o => o.Name == treeNode.Text);
                    object3D.Object3DList = new List<Object3D>();
                    RegenerateObject(treeNode, object3D);
                  /*  foreach (TreeNode childNode in treeNode.Nodes)
                    {
                        RegenerateObject(treeNode, object3D);
                    }  */
                    newObjectList.Add(object3D);
                }
            }
            else
            {
                if (parent.Nodes != null)
                {
                    foreach (TreeNode childNode in parent.Nodes)
                    {
                        Object3D childObject3D = availableObjectsList.Find(o => o.Name == childNode.Text);
                        childObject3D.Object3DList = new List<Object3D>();
                        RegenerateObject(childNode, childObject3D);
                     /*   foreach (TreeNode grandChildNode in childNode.Nodes)
                        {
                            RegenerateObject(childNode, childObject3D);
                        }  */
                        parentObject.Object3DList.Add(childObject3D);
                    }
                }
            }
        }

        private void ShowObject(Object3D object3D, TreeNode parentNode)
        {
            TreeNode objectNode = new TreeNode(object3D.Name);
            objectNode.Name = object3D.Name;
            if (parentNode == Nodes[0]) // null)
            {
                Nodes[0].Nodes.Add(objectNode);
            }
            else
            {
                parentNode.Nodes.Add(objectNode);
            }
            foreach (Object3D childObject in object3D.Object3DList)
            {
                ShowObject(childObject, objectNode);
            }
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            drgevent.Effect = DragDropEffects.Move;
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            availableObjectsList = viewer3D.Scene.GetAllObjects(null, true);
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = PointToClient(new Point(drgevent.X, drgevent.Y));

            // Retrieve the node at the drop location.
            TreeNode targetNode = GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)drgevent.Data.GetData(typeof(TreeNode));

            if (!draggedNode.Equals(targetNode) && targetNode != null)
            {
                // Remove the node from its current 
                // location and add it to the node at the drop location.
                draggedNode.Remove();
                targetNode.Nodes.Add(draggedNode);

                // Expand the node at the location 
                // to show the dropped node.
                targetNode.Expand();
                RegenerateObject(null, null);
                this.viewer3D.Scene = new Scene3D();
                foreach (Object3D newObject in newObjectList)
                {
                    this.viewer3D.Scene.AddObject(newObject);
                }
                /*  viewer3D.Scene = new Scene3D();
                  foreach (Object3D object3D in newObjectList)
                  {
                      viewer3D.Scene.AddObject(object3D);
                  }  */
                SelectedNode = draggedNode;
            }
        }

        private void UnHighlightObject(Object3D object3D)
        {
            object3D.HighLight = false;
            /*  object3D.Material.AmbientColor = selectedObjectAmbientColor;
              object3D.Material.DiffuseColor = selectedObjectDiffuseColor;
              object3D.Material.SpecularColor = selectedObjectSpecularColor;  
              object3D.Material.SetDiffuseColor(unHighlightColor);
              object3D.Material.SetAmbientColor(unHighlightColor);
              object3D.Material.SetSpecularColor(specularColor);  */
          //  object3D.ShowWireFrame = false;
          /*  foreach (Object3D childObject in object3D.Object3DList)
            {
                UnHighlightObject(childObject);
            }  */
        }

        private void HighlightObject(Object3D object3D)
        {
            object3D.HighLight = true;
         //   object3D.ShowWireFrame = true;
         /*   selectedObjectAmbientColor = 
                new float[] { object3D.Material.AmbientColor[0], object3D.Material.AmbientColor[1], object3D.Material.AmbientColor[2], object3D.Material.AmbientColor[3] };
            selectedObjectDiffuseColor =
                new float[] { object3D.Material.DiffuseColor[0], object3D.Material.DiffuseColor[1], object3D.Material.DiffuseColor[2], object3D.Material.DiffuseColor[3] };
            selectedObjectSpecularColor =
                new float[] { object3D.Material.SpecularColor[0], object3D.Material.SpecularColor[1], object3D.Material.SpecularColor[2], object3D.Material.SpecularColor[3] };  
            object3D.Material.SetDiffuseColor(highlightColor); 
            object3D.Material.SetAmbientColor(highlightColor);
            object3D.Material.SetSpecularColor(specularColor);  */
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);
            if (previousSelectedObject != null)
            {
                UnHighlightObject(previousSelectedObject);
            }
        //    if (availableObjectsList == null) { availableObjectsList = viewer3D.Scene.GetAllObjects(null, true); }
            availableObjectsList = viewer3D.Scene.GetAllObjects(null, true);
            foreach (Object3D object3D in availableObjectsList)
            {
                UnHighlightObject(object3D);
                object3D.ShowNormals = false;
            }
            if (e.Node != Nodes[0]) // null)
            {
                selectedObject = viewer3D.Scene.GetObject(e.Node.Text);
                HighlightObject(selectedObject);
            }
            else
            {
                selectedObject = null;
            }
            previousSelectedObject = selectedObject;
            viewer3D.Invalidate();
            OnSelectionChanged(selectedObject);
        }

        public void ShowScene(Object3D selectedObject)
        {
            this.Nodes.Clear();
            this.Nodes.Add("Scene");
            foreach (Object3D object3D in viewer3D.Scene.ObjectList)
            {
                ShowObject(object3D, this.Nodes[0]); // null);
            }
            if (selectedObject != null)
            {
                this.selectedObject = selectedObject;
                TreeNode[] nodeList = Nodes.Find(selectedObject.Name, true);
                if (nodeList.Length > 0)
                {
                    SelectedNode = nodeList[0];
                }
            }
            ExpandAll();
        }

    /*    public void SetScene(Scene3D scene)
        {
            this.scene = scene;
            ShowScene();
        }  */

        public void SetViewer3D(Viewer3D viewer3D)
        {
            this.viewer3D = viewer3D;
            AllowDrop = true;
            availableObjectsList = null;
            ShowScene(null);
        }

        public void ClearSelection()
        {
            SelectedNode = null;
            selectedObject = null;
            if (viewer3D == null) { return; } // Can happen just after startup.
            if (availableObjectsList == null) { availableObjectsList = viewer3D.Scene.GetAllObjects(null, true); }
            foreach (Object3D object3D in availableObjectsList)
            {
                UnHighlightObject(object3D);
                object3D.ShowNormals = false;
            }
            OnSelectionChanged(null);  
        }

        public Object3D SelectedObject
        {
            get { return selectedObject; }
            set { selectedObject = value; }
        }
    }
}
