using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using MathematicsLibrary.Geometry;
using ObjectSerializerLibrary;
using ThreeDimensionalVisualizationLibrary.Animations; // For the KeyFrame class
using ThreeDimensionalVisualizationLibrary.Materials;

namespace ThreeDimensionalVisualizationLibrary.Objects
{
    [DataContract]
    [Serializable]
    public class Object3D
    {
        protected const double DEFAULT_NORMAL_LENGTH = 0.1;

        protected string name = null;
        protected List<Vertex3D> vertexList = null;
        protected List<TriangleIndices> triangleIndicesList = null;
        protected List<Vector3D> triangleNormalVectorList = null;
        protected List<Object3D> object3DList = null; // Allows nested objects (but note that transparency sorting will not work).
        protected List<KeyFrame> keyFrameList = null;
        protected Material material;
        protected int textureID = -1;

        protected System.Boolean visible = true;
        protected float vertexSize = 1f;
        protected float[] vertexColor = new float[] { 1f, 1f, 1f, 1f };
        protected float wireFrameWidth = 1f;
        protected ShadingModel shadingModel = ShadingModel.Smooth;
        protected System.Boolean useLight = true;
        protected System.Boolean showVertices = false;
        protected System.Boolean showWireFrame = false;
        protected System.Boolean showSurfaces = true;
        protected System.Boolean showNormals = false;

        // 20161011
        protected double[] position = new double[] { 0f, 0f, 0f };
        protected double[] rotation = new double[] { 0f, 0f, 0f };

        // 20191031
        protected System.Boolean highlight = false;
        protected Material highlightMaterial = null;

        // 20200104
        protected double[] positionOffset = new double[] { 0f, 0f, 0f };
        protected double[] initialPosition = new double[] { 0f, 0f, 0f };
        protected double[] initialRotation = new double[] { 0f, 0f, 0f };

        // 20200106
        protected int currentKeyFrameIndex = 0;

        public Object3D()
        {
            name = "";
            vertexList = new List<Vertex3D>();
            triangleIndicesList = new List<TriangleIndices>();
            textureID = -1;
            material = new Material();
            object3DList = new List<Object3D>(); // 20191024 - might as well add it here...
            keyFrameList = new List<KeyFrame>();
        }

        public virtual void Generate(List<double> parameterList)
        {
            vertexList = new List<Vertex3D>();
            triangleIndicesList = new List<TriangleIndices>();
            position = new double[] { 0f, 0f, 0f };
            rotation = new double[] { 0f, 0f, 0f };
        }

        // Generates a new key frame.
        public void GenerateKeyFrame(string name, List<Vertex3D> vertexList)
        {
            string keyFrameName = name;
            if (keyFrameName == null)
            {
                keyFrameName = "Frame" + keyFrameList.Count.ToString();
            }
            KeyFrame keyFrame = new KeyFrame(keyFrameName, vertexList);
            keyFrameList.Add(keyFrame);
        }

        // 20200104
        public void SetInitialPose()
        {
            initialPosition = new double[] { position[0], position[1], position[2] };
            InitialRotation = new double[] { rotation[0], rotation[1], rotation[2] };
        }

        public void GenerateTriangleConnectionLists()
        {
            for (int iTriangle = 0; iTriangle < triangleIndicesList.Count; iTriangle++)
            {
                TriangleIndices triangleIndices = triangleIndicesList[iTriangle];
                vertexList[triangleIndices.Index1].TriangleConnectionList.Add(iTriangle);
                vertexList[triangleIndices.Index2].TriangleConnectionList.Add(iTriangle);
                vertexList[triangleIndices.Index3].TriangleConnectionList.Add(iTriangle);
            }
        }

        // This method computes the normal vector for each triangle, and assigns the
        // same normal vector to each vertex in the triangle. 
        // Suitable for flat shading, but not for smooth shading ZZZ
        public void ComputeTriangleNormalVectors()
        {
            triangleNormalVectorList = new List<Vector3D>();
            for (int iTriangle = 0; iTriangle < triangleIndicesList.Count; iTriangle++)
            {
                TriangleIndices triangleIndices = triangleIndicesList[iTriangle];
                Vector3D firstVector = Vector3D.FromPoints(vertexList[triangleIndices.Index2].Position, vertexList[triangleIndices.Index1].Position);
                Vector3D secondVector = Vector3D.FromPoints(vertexList[triangleIndices.Index3].Position, vertexList[triangleIndices.Index1].Position);
                Vector3D normalVector = Vector3D.Cross(firstVector, secondVector);
                normalVector.Normalize();
                triangleNormalVectorList.Add(normalVector);
            //    vertexList[triangleIndices.Index1].NormalVector = normalVector;
            //    vertexList[triangleIndices.Index2].NormalVector = normalVector;
            //    vertexList[triangleIndices.Index3].NormalVector = normalVector;
            }
        }

        // This method computes the normal vector for each vertex, by averaging over
        // the normal vectors of the triangles to which the vertex belongs.
        // Suitable for smooth shading.
        public void ComputeVertexNormalVectors()
        {
            for (int iVertex = 0; iVertex < vertexList.Count; iVertex++)
            {
                Vertex3D vertex = vertexList[iVertex]; 
                Vector3D vertexNormalVector = new Vector3D();
                for (int ii = 0; ii < vertex.TriangleConnectionList.Count; ii++)
                {
                    int triangleIndex = vertex.TriangleConnectionList[ii];
                    vertexNormalVector.Add(triangleNormalVectorList[triangleIndex]);
                }
                vertexNormalVector.Normalize();
                vertex.NormalVector = vertexNormalVector;
            }
        }

        public void SetUniformColor(Color color)
        {
            foreach (Vertex3D vertex in vertexList)
            {
                vertex.Color = color;
            }
        }

        // Do not use (superseded by Move(...); see below)
        // Translate(...) retained for back compatibility only.
    /*    public void Translate(double deltaX, double deltaY, double deltaZ)
        {
            foreach (Vertex3D vertex in vertexList)
            {
                vertex.Position.X += deltaX;
                vertex.Position.Y += deltaY;
                vertex.Position.Z += deltaZ;
            }
            if (object3DList != null)
            {
                foreach (Object3D object3D in object3DList) { object3D.Translate(deltaX, deltaY, deltaZ); }
            }  
        }    */  

        private double GetXCenter()
        {
            double xCenter = 0;
            foreach (Vertex3D vertex in vertexList)
            {
                xCenter += vertex.Position.X;
            }
            xCenter /= vertexList.Count;
            return xCenter;
        }

        private void SetXCenter(double xCenter)
        {
            double oldXCenter = GetXCenter();
            foreach (Vertex3D vertex in vertexList)
            {
                vertex.Position.X += (xCenter - oldXCenter);
            }
            foreach (KeyFrame keyFrame in keyFrameList)
            {
                keyFrame.SetXCenter(xCenter);
            }
        }

        private double GetYCenter()
        {
            double yCenter = 0;
            foreach (Vertex3D vertex in vertexList)
            {
                yCenter += vertex.Position.Y;
            }
            yCenter /= vertexList.Count;
            return yCenter;
        }

        private void SetYCenter(double yCenter)
        {
            double oldYCenter = GetYCenter();
            foreach (Vertex3D vertex in vertexList)
            {
                vertex.Position.Y += (yCenter - oldYCenter);
            }
            foreach (KeyFrame keyFrame in keyFrameList)
            {
                keyFrame.SetYCenter(yCenter);
            }
        }

        private double GetZCenter()
        {
            double zCenter = 0;
            foreach (Vertex3D vertex in vertexList)
            {
                zCenter += vertex.Position.Z;
            }
            zCenter /= vertexList.Count;
            return zCenter;
        }

        private void SetZCenter(double zCenter)
        {
            double oldZCenter = GetZCenter();
            foreach (Vertex3D vertex in vertexList)
            {
                vertex.Position.Z += (zCenter - oldZCenter);
            }
            foreach (KeyFrame keyFrame in keyFrameList)
            {
                keyFrame.SetZCenter(zCenter);
            }
        }

        // 20161010 NEW
        public void Move(double deltaX, double deltaY, double deltaZ)
        {
            if (position == null) { position = new double[] { 0f, 0f, 0f }; } // Needed in case of deserialization.
            position[0] += deltaX;
            position[1] += deltaY;
            position[2] += deltaZ;
        }

        // 20161010 NEW
        public void RotateX(double deltaRotationX)
        {
            rotation[0] += deltaRotationX;
        }

        // 20161010 NEW
        public void RotateY(double deltaRotationY)
        {
            rotation[1] += deltaRotationY;
        }

        // 20161010 NEW
        public void RotateZ(double deltaRotationZ)
        {
            rotation[2] += deltaRotationZ;
        }

        public Object3D FindObject(string name)
        {
            Object3D foundObject = null;
            if (this.name == name) { return this; }
            else
            {
                if (object3DList != null)
                {
                    foreach (Object3D object3D in this.object3DList)
                    {
                        foundObject = object3D.FindObject(name);
                        if (foundObject != null)
                        {
                            break;
                        }
                    }
                }
                return foundObject;
            }
        }

        private void RenderVertices()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
            System.Boolean lightEnabled = GL.IsEnabled(EnableCap.Lighting);
            if (lightEnabled) { GL.Disable(EnableCap.Lighting); }
            float previousPointSize = GL.GetFloat(GetPName.PointSize);
            GL.PointSize(vertexSize);
            GL.Enable(EnableCap.PointSmooth);
            GL.Begin(PrimitiveType.Points);
            for (int ii = 0; ii < vertexList.Count; ii++)
            {
                GL.Color3(vertexList[ii].Color.R, vertexList[ii].Color.G, vertexList[ii].Color.B);  // 20161118
                GL.Vertex3(vertexList[ii].Position.X, vertexList[ii].Position.Z, -vertexList[ii].Position.Y);
            }
            GL.End();
            GL.PointSize(previousPointSize);
            if (lightEnabled) { GL.Enable(EnableCap.Lighting); }
        }

        private void RenderWireFrame()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Disable(EnableCap.Lighting);
            //   GL.Disable(EnableCap.DepthTest);
            //  GL.Enable(EnableCap.CullFace);
            System.Boolean useLightState = useLight;
            useLight = false;
            float previousWireFrameWidth = GL.GetFloat(GetPName.LineWidth);
            GL.LineWidth(wireFrameWidth);
            RenderTriangles();
            GL.LineWidth(previousWireFrameWidth);
            useLight = useLightState;
            //   GL.Disable(EnableCap.CullFace);
            //    GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
        }

        private void RenderSurfaces()
        {
          /*  if (textureID > 0)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, Convert.ToInt32(TextureWrapMode.Repeat));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, Convert.ToInt32(TextureWrapMode.Repeat));
            }  */
            if (useLight)
            {
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, material.DiffuseColor);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, material.AmbientColor);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, material.SpecularColor);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, material.Shininess);
            }
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            if (useLight) { GL.Enable(EnableCap.Lighting); }
            else { GL.Disable(EnableCap.Lighting); }
            RenderTriangles();
        }

        private void TestRenderTriangles()
        {
            GL.Begin(PrimitiveType.Triangles);
            if (textureID > 0)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
            }

            foreach (var tri in triangleIndicesList)
            {
                var vertex1 = vertexList[tri.Index1];
                GL.Normal3(vertex1.NormalVector.X, vertex1.NormalVector.Z, -vertex1.NormalVector.Y);
                if (textureID > 0) GL.TexCoord2(vertex1.TextureCoordinates.X, vertex1.TextureCoordinates.Y);
                if (!useLight) GL.Color4(vertex1.Color.R, vertex1.Color.G, vertex1.Color.B, vertex1.Color.A);
                GL.Vertex3(vertex1.Position.X, vertex1.Position.Z, -vertex1.Position.Y);

                var vertex2 = vertexList[tri.Index2];
                GL.Normal3(vertex2.NormalVector.X, vertex2.NormalVector.Z, -vertex2.NormalVector.Y);
                if (textureID > 0) GL.TexCoord2(vertex2.TextureCoordinates.X, vertex2.TextureCoordinates.Y);
                if (!useLight) GL.Color4(vertex2.Color.R, vertex2.Color.G, vertex2.Color.B, vertex2.Color.A);
                GL.Vertex3(vertex2.Position.X, vertex2.Position.Z, -vertex2.Position.Y);

                var vertex3 = vertexList[tri.Index3];
                GL.Normal3(vertex3.NormalVector.X, vertex3.NormalVector.Z, -vertex3.NormalVector.Y);
                if (textureID > 0) GL.TexCoord2(vertex3.TextureCoordinates.X, vertex3.TextureCoordinates.Y);
                if (!useLight) GL.Color4(vertex3.Color.R, vertex3.Color.G, vertex3.Color.B, vertex3.Color.A);
                GL.Vertex3(vertex3.Position.X, vertex3.Position.Z, -vertex3.Position.Y);
            }

            GL.End();
            GL.Disable(EnableCap.Texture2D);

            //for (int iTriangle = 0; iTriangle < triangleIndicesList.Count; iTriangle++)
            //{
            //    TriangleIndices triangleIndices = triangleIndicesList[iTriangle];
            //    Vertex3D vertex1 = vertexList[triangleIndices.Index1];
            //    Vertex3D vertex2 = vertexList[triangleIndices.Index2];
            //    Vertex3D vertex3 = vertexList[triangleIndices.Index3];
            //    Vector3D normal1 = vertex1.NormalVector;
            //    Vector3D normal2 = vertex2.NormalVector;
            //    Vector3D normal3 = vertex3.NormalVector;
            //    Vector3D triangeNormal = triangleNormalVectorList[iTriangle];
            //    GL.Begin(PrimitiveType.Triangles);
            //    if (textureID > 0) { GL.BindTexture(TextureTarget.Texture2D, textureID); }
            //    if (shadingModel == ShadingModel.Flat) { GL.Normal3(triangeNormal.X, triangeNormal.Z, -triangeNormal.Y); }
            //    if (shadingModel == ShadingModel.Smooth) { GL.Normal3(normal1.X, normal1.Z, -normal1.Y); }
            //    if (!useLight) { GL.Color4(vertex1.Color.R, vertex1.Color.G, vertex1.Color.B, vertex1.Color.A); }
            //    if (textureID > 0) { GL.TexCoord2(vertex1.TextureCoordinates.X, vertex1.TextureCoordinates.Y); }
            //    GL.Vertex3(vertex1.Position.X, vertex1.Position.Z, -vertex1.Position.Y);
            //    if (shadingModel == ShadingModel.Smooth) { GL.Normal3(normal2.X, normal2.Z, -normal2.Y); }
            //    if (!useLight) { GL.Color4(vertex2.Color.R, vertex2.Color.G, vertex2.Color.B, vertex2.Color.A); }
            //    if (textureID > 0)
            //    {
            //        if (vertex2.TextureCoordinates.X < vertex1.TextureCoordinates.X)
            //        {
            //            GL.TexCoord2(vertex2.TextureCoordinates.X + 1, vertex2.TextureCoordinates.Y);
            //        }
            //        else { GL.TexCoord2(vertex2.TextureCoordinates.X, vertex2.TextureCoordinates.Y); }
            //    }
            //    GL.Vertex3(vertex2.Position.X, vertex2.Position.Z, -vertex2.Position.Y);
            //    if (shadingModel == ShadingModel.Smooth) { GL.Normal3(normal3.X, normal3.Z, -normal3.Y); }
            //    if (!useLight) { GL.Color4(vertex3.Color.R, vertex3.Color.G, vertex3.Color.B, vertex3.Color.A); }
            //    if (textureID > 0)
            //    {
            //        if (vertex3.TextureCoordinates.X < vertex1.TextureCoordinates.X)
            //        {
            //            GL.TexCoord2(vertex3.TextureCoordinates.X + 1, vertex3.TextureCoordinates.Y);
            //        }
            //        else { GL.TexCoord2(vertex3.TextureCoordinates.X, vertex3.TextureCoordinates.Y); }
            //    }
            //    GL.Vertex3(vertex3.Position.X, vertex3.Position.Z, -vertex3.Position.Y);
            //    GL.End();
            //}
        }

        private void RenderTriangles()
        {
            if (textureID > 0)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
            }
            for (int iTriangle = 0; iTriangle < triangleIndicesList.Count; iTriangle++)
            {
                TriangleIndices triangleIndices = triangleIndicesList[iTriangle];
                Vertex3D vertex1 = vertexList[triangleIndices.Index1];
                Vertex3D vertex2 = vertexList[triangleIndices.Index2];
                Vertex3D vertex3 = vertexList[triangleIndices.Index3];
                Vector3D normal1 = vertex1.NormalVector;
                Vector3D normal2 = vertex2.NormalVector;
                Vector3D normal3 = vertex3.NormalVector;
                Vector3D triangeNormal = triangleNormalVectorList[iTriangle];
                GL.Begin(PrimitiveType.Triangles);
                if (shadingModel == ShadingModel.Flat) { GL.Normal3(triangeNormal.X, triangeNormal.Z, -triangeNormal.Y); }
                if (shadingModel == ShadingModel.Smooth) { GL.Normal3(normal1.X, normal1.Z, -normal1.Y); }
                if (!useLight) { GL.Color4(vertex1.Color.R, vertex1.Color.G, vertex1.Color.B, vertex1.Color.A); }
                if (textureID > 0) { GL.TexCoord2(vertex1.TextureCoordinates.X, vertex1.TextureCoordinates.Y); }
                GL.Vertex3(vertex1.Position.X, vertex1.Position.Z, -vertex1.Position.Y);
                if (shadingModel == ShadingModel.Smooth) { GL.Normal3(normal2.X, normal2.Z, -normal2.Y); }
                if (!useLight) { GL.Color4(vertex2.Color.R, vertex2.Color.G, vertex2.Color.B, vertex2.Color.A); }
                if (textureID > 0)
                {
                    GL.TexCoord2(vertex2.TextureCoordinates.X, vertex2.TextureCoordinates.Y);
                  /*  if (vertex2.TextureCoordinates.X < vertex1.TextureCoordinates.X)
                    {
                        GL.TexCoord2(vertex2.TextureCoordinates.X + 1, vertex2.TextureCoordinates.Y);
                    }
                    else { GL.TexCoord2(vertex2.TextureCoordinates.X, vertex2.TextureCoordinates.Y); }  */
                }
                GL.Vertex3(vertex2.Position.X, vertex2.Position.Z, -vertex2.Position.Y);
                if (shadingModel == ShadingModel.Smooth) { GL.Normal3(normal3.X, normal3.Z, -normal3.Y); }
                if (!useLight) { GL.Color4(vertex3.Color.R, vertex3.Color.G, vertex3.Color.B, vertex3.Color.A); }
                if (textureID > 0)
                {
                    GL.TexCoord2(vertex3.TextureCoordinates.X, vertex3.TextureCoordinates.Y);
                 /*   if (vertex3.TextureCoordinates.X < vertex1.TextureCoordinates.X)
                    {
                        GL.TexCoord2(vertex3.TextureCoordinates.X + 1, vertex3.TextureCoordinates.Y);
                    }
                    else { GL.TexCoord2(vertex3.TextureCoordinates.X, vertex3.TextureCoordinates.Y); }   */
                }
                GL.Vertex3(vertex3.Position.X, vertex3.Position.Z, -vertex3.Position.Y);
                GL.End();
            }
            if (textureID > 0)
            {
                GL.Disable(EnableCap.Texture2D);
            }
        }

        public void RenderNormals()
        {
            GL.Disable(EnableCap.Lighting);
            foreach (Vertex3D vertex in vertexList)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color4(1f, 0f, 0f,1f);
                GL.Vertex3(vertex.Position.X, vertex.Position.Z, -vertex.Position.Y);
                GL.Color4(1f, 0f, 0f, 1f);
                GL.Vertex3(vertex.Position.X + DEFAULT_NORMAL_LENGTH * vertex.NormalVector.X, 
                           vertex.Position.Z + DEFAULT_NORMAL_LENGTH * vertex.NormalVector.Z, 
                           -vertex.Position.Y- DEFAULT_NORMAL_LENGTH * vertex.NormalVector.Y);
                GL.End();
            }
            GL.Enable(EnableCap.Lighting);
        }

        public void Render()
        {
            if (!visible) { return; }

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.Translate(position[0], position[2], -position[1]);
            GL.Translate(positionOffset[0], positionOffset[2], -positionOffset[1]);  // 20191101 - allows rotation around an arbitrary point
            GL.Rotate(rotation[2], new Vector3d(0f, 1f, 0f)); 
            GL.Rotate(rotation[1], new Vector3d(0f, 0f, -1f)); 
            GL.Rotate(rotation[0], new Vector3d(1f, 0f, 0f));
            GL.Translate(-positionOffset[0], -positionOffset[2], positionOffset[1]); // 20191101 - restore after rotating.

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            if (showNormals) { RenderNormals(); }
            Material storedMaterial = null;
            if (highlight && (highlightMaterial != null))
            {
                storedMaterial = (Material)ObjectCopier.Copy(material);
                material = highlightMaterial;
            }
            if (material.Opacity < 1) GL.Enable(EnableCap.Blend);
            if (triangleNormalVectorList == null) { ComputeTriangleNormalVectors(); }
            if (showSurfaces) { RenderSurfaces(); }
            if (showWireFrame) { RenderWireFrame(); }
            if (showVertices) { RenderVertices(); }
            if (material.Opacity < 1) GL.Disable(EnableCap.Blend);
            if (highlight && (highlightMaterial != null))
            { 
                material = storedMaterial;
            }
            if (object3DList != null)
            {
                foreach (Object3D object3D in object3DList)
                {
                    object3D.Render();
                }
            }
            GL.PopMatrix();
        }

        public void LoadTextures()
        {
            if (material == null) { return; }
            var diffuseTexture = material.LoadDiffuseMap();
            if (diffuseTexture != Material.NO_TEXTURE_AVAILABLE)
            {
                SetTexture(diffuseTexture);
            }
        }

        public void AddVertex(Vertex3D vertex)
        {
            vertexList.Add(vertex);
        }

        // 20160617  (Not really used, but makes it possible to set a texture directly)
        public void SetTexture(int textureID)
        {
            this.textureID = textureID;
        }

        // 20191008
        public void AddTriangle(TriangleIndices triangleIndices)
        {
            triangleIndicesList.Add(triangleIndices);
        }

        // 20191024
        public void AssignKeyFrame(int keyFrameIndex)
        {
            currentKeyFrameIndex = keyFrameIndex;
            KeyFrame keyFrame = keyFrameList[keyFrameIndex];
            this.vertexList = new List<Vertex3D>();
            foreach (Vertex3D vertex in keyFrame.VertexList)
            {
                this.vertexList.Add(vertex);
            }
        }

        // 20190124
        public void InterpolateKeyFrames(int keyFrameIndex1, int keyFrameIndex2, double beta)
        {
            KeyFrame keyFrame1 = keyFrameList[keyFrameIndex1];
            KeyFrame keyFrame2 = keyFrameList[keyFrameIndex2];
      //      this.vertexList = new List<Vertex3D>();
            for (int ii = 0; ii < keyFrame1.VertexList.Count; ii++)
            {
                Vertex3D vertex1 = keyFrame1.VertexList[ii];
                Vertex3D vertex2 = keyFrame2.VertexList[ii];
                Vertex3D interpolatedVertex = Vertex3D.Interpolate(vertex1, vertex2, beta);
                this.vertexList[ii] = interpolatedVertex;
            }
            GenerateTriangleConnectionLists();
            ComputeTriangleNormalVectors();
          //  ComputeVertexNormalVectors();
        }

        public void InterpolatePose(Vector3D initialPosition, Vector3D finalPosition, Vector3D initialRotation, Vector3D finalRotation, double beta)
        {
            Vector3D position = Vector3D.Interpolate(initialPosition, finalPosition, beta);
            Vector3D rotation = Vector3D.Interpolate(initialRotation, finalRotation, beta);
            this.position = new double[] { position.X, position.Y, position.Z };
            this.rotation = new double[] { rotation.X, rotation.Y, rotation.Z };
        }

        // 20191031
        public void SetHighlightMaterial(Material highlightMaterial)
        {
            this.highlightMaterial = highlightMaterial;
            foreach (Object3D object3D in this.object3DList)
            {
                object3D.SetHighlightMaterial(highlightMaterial);
            }
        }


        /*   [DataMember]
           public float Alpha
           {
               get { return alpha; }
               set { alpha = value; }
           }

           [DataMember]
           public Color AmbientColor
           {
               get 
               {
                   Color color = Color.FromArgb((int)(Math.Round(255 * ambientColor[3])), (int)(Math.Round(255 * ambientColor[0])),
                                                 (int)(Math.Round(255 * ambientColor[1])), (int)(Math.Round(255 * ambientColor[2])));
                   return color;
               }
               set
               {
                   if (ambientColor == null) { ambientColor = new float[] { 1f, 1f, 1f, 1f }; }
                   ambientColor[0] = value.R/ (float)255;
                   ambientColor[1] = value.G / (float)255;
                   ambientColor[2] = value.B/ (float)255;
                   ambientColor[3] = value.A / (float)255;
               }
           }

           [DataMember]
           public Color DiffuseColor
           {
               get
               {
                   Color color = Color.FromArgb((int)(Math.Round(255 * diffuseColor[3])), (int)(Math.Round(255 * diffuseColor[0])),
                                                 (int)(Math.Round(255 * diffuseColor[1])), (int)(Math.Round(255 * diffuseColor[2])));
                   return color;
               }
               set
               {
                   if (diffuseColor == null) { diffuseColor = new float[] { 1f, 1f, 1f, 1f }; }
                   diffuseColor[0] = value.R / (float)255;
                   diffuseColor[1] = value.G / (float)255;
                   diffuseColor[2] = value.B / (float)255;
                   diffuseColor[3] = value.A / (float)255;
               }
           }

           [DataMember]
           public Color SpecularColor
           {
               get
               {
                   Color color = Color.FromArgb((int)(Math.Round(255 * specularColor[3])), (int)(Math.Round(255 * specularColor[0])),
                                                 (int)(Math.Round(255 * specularColor[1])), (int)(Math.Round(255 * specularColor[2])));
                   return color;
               }
               set
               {
                   if (specularColor == null) { specularColor = new float[] { 1f, 1f, 1f, 1f }; }
                   specularColor[0] = value.R / (float)255;
                   specularColor[1] = value.G / (float)255;
                   specularColor[2] = value.B / (float)255;
                   specularColor[3] = value.A / (float)255;
               }
           }  

           [DataMember]
           public int Shininess
           {
               get { return shininess; }
               set { shininess = value; }
           }  */

        public double RotationCenterX
        {
            get { return positionOffset[0]; }
            set
            {
                positionOffset[0] = value;
           //     double oldX = position[0];
           //     Move(value - oldX, 0, 0);
            }
        }

        public double RotationCenterY
        {
            get { return positionOffset[1]; }
            set
            {
                positionOffset[1] = value;
         //       double oldY = position[1];
         //       Move(0, value - oldY, 0);
            }
        }

        public double RotationCenterZ
        {
            get { return positionOffset[2]; }
            set
            {
                positionOffset[2] = value;
           //     double oldZ = position[2];
           //     Move(0, 0, value - oldZ);
            }
        }

        public double RotationX
        {
            get { return rotation[0]; }
            set
            {
                double oldRotationX = rotation[0];
                RotateX(value - oldRotationX);
            }
        }

        public double RotationY
        {
            get { return rotation[1]; }
            set
            {
                double oldRotationY = rotation[1];
                RotateY(value - oldRotationY);
            }
        }

        public double RotationZ
        {
            get { return rotation[2]; }
            set
            {
                double oldRotationZ = rotation[2];
                RotateZ(value - oldRotationZ);
            }
        }

        public double CenterX
        {
            get
            {
                double centerX = GetXCenter();
                return centerX;
            }
            set
            {
                SetXCenter(value);
            }
        }

        public double CenterY
        {
            get
            {
                double centerY = GetYCenter();
                return centerY;
            }
            set
            {
                SetYCenter(value);
            }
        }

        public double CenterZ
        {
            get
            {
                double centerZ = GetZCenter();
                return centerZ;
            }
            set
            {
                SetZCenter(value);
            }
        }

        public System.Boolean HighLight
        {
            get { return highlight; }
            set { highlight = value; }
        }

        public double[] InitialPosition
        {
            get { return initialPosition; }
            set { initialPosition = value; }
        }

        public double[] InitialRotation
        {
            get { return initialRotation; }
            set { initialRotation = value; }
        }

        public int CurrentKeyFrameIndex
        {
            get { return currentKeyFrameIndex; }
        }

        [DataMember]
        public double[] Position
        {
            get { return position; }
            set { position = value; }
        }

        [DataMember]
        public double[] PositionOffset
        {
            get { return positionOffset; }
            set { positionOffset = value; }
        }

        [DataMember]
        public double[] Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        [DataMember]
        public List<Vertex3D> VertexList
        {
            get { return vertexList; }
            set { vertexList = value; }
        }

        [DataMember]
        public List<KeyFrame> KeyFrameList
        {
            get { return keyFrameList; }
            set { keyFrameList = value; }
        }

        [DataMember]
        public List<TriangleIndices> TriangleIndicesList
        {
            get { return triangleIndicesList; }
            set { triangleIndicesList = value; }
        }
          
        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public Material Material
        {
            get { return material; }
            set { material = value; }
        }

        [DataMember]
        public float VertexSize
        {
            get { return vertexSize; }
            set { vertexSize = value; }
        }

        [DataMember]
        public float WireFrameWidth
        {
            get { return wireFrameWidth; }
            set { wireFrameWidth = value; }
        }

        [DataMember]
        public ShadingModel ShadingModel
        {
            get { return shadingModel; }
            set { shadingModel = value; }
        }

        [DataMember]
        public System.Boolean UseLight
        {
            get { return useLight; }
            set { useLight = value; }
        }

        [DataMember]
        public System.Boolean ShowVertices
        {
            get { return showVertices; }
            set { showVertices = value; }
        }

        [DataMember]
        public System.Boolean ShowWireFrame
        {
            get { return showWireFrame; }
            set { showWireFrame = value; }
        }

        [DataMember]
        public System.Boolean ShowSurfaces
        {
            get { return showSurfaces; }
            set { showSurfaces = value; }
        }

        [DataMember]
        public System.Boolean ShowNormals
        {
            get { return showNormals; }
            set { showNormals = value; }
        }

        [DataMember]
        public List<Object3D> Object3DList
        {
            get { return object3DList; }
            set { object3DList = value; }
        }

        [DataMember]
        public System.Boolean Visible
        {
            get { return visible; }
            set { visible = value; }
        }
    }
}
