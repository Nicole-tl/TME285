using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThreeDimensionalVisualizationLibrary
{
    [DataContract]
    [Serializable]
    public class Light
    {
        private EnableCap enableCap; // Identifies the light index
    //    private LightName lightName; // Identifies the light name.
        private List<float> position; // x,y,z,w
        private System.Boolean isOn = false;

        public Light()
        {
            enableCap = EnableCap.Light0; // Default: Light source 0
            position = new List<float>() { 0f, -5f, 0f, 1f }; // Light at z = 10  (last index = 0 =-> infinity)
        }

        [DataMember]
        public List<float> Position
        {
            get { return position; }
            set { position = value; }
        }

        [DataMember]
        public EnableCap EnableCap
        {
            get { return enableCap; }
            set { enableCap = value; }
        }

     /*   [DataMember]
        public LightName LightName
        {
            get { return lightName; }
            set { lightName = value; }
        }  */

        [DataMember]
        public System.Boolean IsOn
        {
            get { return isOn; }
            set { isOn = value; }
        }
    }
}
