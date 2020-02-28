using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ObjectSerializerLibrary; // For copying

namespace ThreeDimensionalVisualizationLibrary.Animations
{
    [DataContract]
    [Serializable]
    public class KeyFrame
    {
        private string name;
        private List<Vertex3D> vertexList;

        public KeyFrame(string name, List<Vertex3D> vertexList)
        {
            this.name = name;
            this.vertexList = new List<Vertex3D>();
            foreach (Vertex3D vertex in vertexList)
            {
                Vertex3D copiedVertex = (Vertex3D)ObjectCopier.Copy(vertex);
                this.vertexList.Add(vertex);
            }
        }

        public double GetXCenter()
        {
            double xCenter = 0;
            foreach (Vertex3D vertex in vertexList)
            {
                xCenter += vertex.Position.X;
            }
            xCenter /= vertexList.Count;
            return xCenter;
        }

        public void SetXCenter(double xCenter)
        {
            double oldXCenter = GetXCenter();
            foreach (Vertex3D vertex in vertexList)
            {
                vertex.Position.X += (xCenter - oldXCenter);
            }
        }

        public double GetYCenter()
        {
            double yCenter = 0;
            foreach (Vertex3D vertex in vertexList)
            {
                yCenter += vertex.Position.Y;
            }
            yCenter /= vertexList.Count;
            return yCenter;
        }

        public void SetYCenter(double yCenter)
        {
            double oldYCenter = GetYCenter();
            foreach (Vertex3D vertex in vertexList)
            {
                vertex.Position.Y += (yCenter - oldYCenter);
            }
        }

        public double GetZCenter()
        {
            double zCenter = 0;
            foreach (Vertex3D vertex in vertexList)
            {
                zCenter += vertex.Position.Z;
            }
            zCenter /= vertexList.Count;
            return zCenter;
        }

        public void SetZCenter(double zCenter)
        {
            double oldZCenter = GetZCenter();
            foreach (Vertex3D vertex in vertexList)
            {
                vertex.Position.Z += (zCenter - oldZCenter);
            }
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public List<Vertex3D> VertexList
        {
            get { return vertexList; }
            set { vertexList = value; }
        }
    }
}
