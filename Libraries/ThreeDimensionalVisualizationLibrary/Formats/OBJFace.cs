using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDimensionalVisualizationLibrary.Formats
{
    public class OBJFace
    {
        private List<int> positionIndexList;
        private List<int> normalIndexList;  // Indices of the normal vectors
        private List<int> textureCoordinateIndexList;

        private List<int> vertexIndexList;

        public OBJFace()
        {
            positionIndexList = new List<int>();
            normalIndexList = new List<int>();
            textureCoordinateIndexList = new List<int>();
            vertexIndexList = new List<int>();
        }

        public List<int> PositionIndexList
        {
            get { return positionIndexList; }
            set { positionIndexList = value; }
        }

        public List<int> NormalIndexList
        {  
            get { return normalIndexList; }
            set { normalIndexList = value; }
        }

        public List<int> TextureCoordinateIndexList
        {
            get { return textureCoordinateIndexList; }
            set { textureCoordinateIndexList = value; }
        }

        public List<int> VertexIndexList
        {
            get { return vertexIndexList; }
            set { vertexIndexList = value; }
        }
    }
}
