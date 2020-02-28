using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeDimensionalVisualizationLibrary.Objects;

namespace ThreeDimensionalVisualizationLibrary.Editing
{
    public class Object3DEventArgs
    {
        private Object3D object3D;

        public Object3DEventArgs(Object3D object3D)
        {
            this.object3D = object3D;
        }

        public Object3D Object3D
        {
            get { return object3D; }
        }
    }
}
