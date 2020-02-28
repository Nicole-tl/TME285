using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathematicsLibrary.Geometry;

namespace ThreeDimensionalVisualizationLibrary.Animations
{
    [DataContract]
    [Serializable]
    public class Animation
    {
        protected string name;
        protected Boolean isSynchronous;
        protected string objectName;
        protected double duration1;
        protected double duration2;
        protected string firstKeyFrameName;
        protected string secondKeyFrameName;
        protected Vector3D firstDeltaPosition;
        protected Vector3D firstDeltaRotation;
        protected Vector3D secondDeltaPosition;
        protected Vector3D secondDeltaRotation;

        public Animation()
        {
            name = "UnnamedAnimation";
            isSynchronous = true;
            objectName = null;
            duration1 = 1;
            duration2 = 0;
            firstKeyFrameName = null;
            secondKeyFrameName = null;
            firstDeltaPosition = new Vector3D(0, 0, 0);
            firstDeltaRotation = new Vector3D(0, 0, 0);
            secondDeltaPosition = new Vector3D(0, 0, 0);
            secondDeltaRotation = new Vector3D(0, 0, 0);
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public Boolean IsSynchronous
        {
            get { return isSynchronous; }
            set { isSynchronous = value; }
        }

        [DataMember]
        public string ObjectName
        {
            get { return objectName; }
            set { objectName = value; }
        }

        [DataMember]
        public double Duration1
        {
            get { return duration1; }
            set { duration1 = value; }
        }

        [DataMember]
        public double Duration2
        {
            get { return duration2; }
            set { duration2 = value; }
        }

        [DataMember]
        public string FirstKeyFrameName
        {
            get { return firstKeyFrameName; }
            set { firstKeyFrameName = value; }
        }

        [DataMember]
        public string SecondKeyFrameName
        {
            get { return secondKeyFrameName; }
            set { secondKeyFrameName = value; }
        }

        [DataMember]
        public Vector3D FirstDeltaPosition
        {
            get { return firstDeltaPosition; }
            set { firstDeltaPosition = value; }
        }

        [DataMember]
        public Vector3D FirstDeltaRotation
        {
            get { return firstDeltaRotation; }
            set { firstDeltaRotation = value; }
        }

        [DataMember]
        public Vector3D SecondDeltaPosition
        {
            get { return secondDeltaPosition; }
            set { secondDeltaPosition = value; }
        }

        [DataMember]
        public Vector3D SecondDeltaRotation
        {
            get { return secondDeltaRotation; }
            set { secondDeltaRotation = value; }
        }
    }
}
