using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MathematicsLibrary.Geometry
{
    [DataContract]
    [Serializable]
    public class Vector3D
    {
        [DataMember]
        public double X { get; set; }
        [DataMember]
        public double Y { get; set; }
        [DataMember]
        public double Z { get; set; }

        public Vector3D()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        public Vector3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3D(Vector3D vector)
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }

        public static Vector3D FromPoints(Point3D point1, Point3D point2)
        {
            Vector3D vector = new Vector3D(point2.X, point2.Y, point2.Z);
            vector.X -= point1.X;
            vector.Y -= point1.Y;
            vector.Z -= point1.Z;
            return vector;
        }

        public static Vector3D Cross(Vector3D vector1, Vector3D vector2)
        {
            Vector3D crossProduct = new Vector3D();
            crossProduct.X = vector1.Y * vector2.Z - vector2.Y * vector1.Z;
            crossProduct.Y = vector1.Z * vector2.X - vector2.Z * vector1.X;
            crossProduct.Z = vector1.X * vector2.Y - vector2.X * vector1.Y;
            return crossProduct;
        }

        public void Add(Vector3D addedVector)
        {
            this.X += addedVector.X;
            this.Y += addedVector.Y;
            this.Z += addedVector.Z;
        }

        public void Normalize()
        {
            double length = GetLength();
            if (length > 0)
            {
                X /= length;
                Y /= length;
                Z /= length;
            }
        }

        public double GetLength()
        {
            double length = Math.Sqrt(X * X + Y * Y + Z * Z);
            return length;
        }

        // 20191028
        public string AsString(string format)
        {
            string vector3DAsString = X.ToString(format) + "," + Y.ToString(format) + "," + Z.ToString(format);
            return vector3DAsString;
        }

        // 20191028
        public static Boolean TryParse(string vectorAsString, out Vector3D vector3D)
        {
            vector3D = null;
            List<string> vectorAsStringSplit = vectorAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (vectorAsStringSplit.Count != 3) { return false; }
            else
            {
                double x;
                double y;
                double z;
                Boolean xOK = double.TryParse(vectorAsStringSplit[0], out x);
                Boolean yOK = double.TryParse(vectorAsStringSplit[1], out y);
                Boolean zOK = double.TryParse(vectorAsStringSplit[2], out z);
                if (xOK && yOK && zOK)
                {
                    vector3D = new Vector3D(x, y, z);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // 20191028
        public static Vector3D Interpolate(Vector3D vector1, Vector3D vector2, double beta)
        {
            double x = vector1.X * (1 - beta) + vector2.X * beta;
            double y = vector1.Y * (1 - beta) + vector2.Y * beta;
            double z = vector1.Z * (1 - beta) + vector2.Z * beta;
            Vector3D interpolatedVector = new Vector3D(x, y, z);
            return interpolatedVector;
        }

    }
}
