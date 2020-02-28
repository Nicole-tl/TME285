using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MathematicsLibrary.Geometry;

namespace ThreeDimensionalVisualizationLibrary
{
    [DataContract]
    [Serializable]
    public class Vertex3D
    {
        private Point3D position;
        private Color color;
        private Vector3D normalVector = null;
        private List<int> triangleConnectionList; // Indices of the triangles that include this vertex
        private Point2D textureCoordinates = null;

        public Vertex3D(double x, double y, double z)
        {
            position = new Point3D(x, y, z);
            color = Color.White;
            triangleConnectionList = new List<int>();
        }

        public static Vertex3D Interpolate(Vertex3D vertex1, Vertex3D vertex2, double beta)
        {
            double x = vertex1.Position.X * (1 - beta) + vertex2.Position.X * beta;
            double y = vertex1.Position.Y * (1 - beta) + vertex2.Position.Y * beta;
            double z = vertex1.Position.Z * (1 - beta) + vertex2.Position.Z * beta;
            double nx = vertex1.NormalVector.X * (1 - beta) + vertex2.NormalVector.X * beta;
            double ny = vertex1.NormalVector.Y * (1 - beta) + vertex2.NormalVector.Y * beta;
            double nz = vertex1.NormalVector.Z * (1 - beta) + vertex2.NormalVector.Z * beta;
            Vertex3D vertex = new Vertex3D(x, y, z);
            vertex.NormalVector = new Vector3D(nx, ny, nz);
            vertex.Color = vertex1.Color;
            return vertex;
        }

        [DataMember]
        public Point3D Position
        {
            get { return position; }
            set { position = value; }
        }

        [DataMember]
        public Point2D TextureCoordinates
        {
            get { return textureCoordinates; }
            set { textureCoordinates = value; }
        }

        [DataMember]
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        [DataMember]
        public Vector3D NormalVector
        {
            get { return normalVector; }
            set { normalVector = value; }
        }

        [DataMember]
        public List<int> TriangleConnectionList
        {
            get { return triangleConnectionList; }
            set { triangleConnectionList = value; }
        }
    }
}
