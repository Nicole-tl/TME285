using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathematicsLibrary.Geometry;
using ThreeDimensionalVisualizationLibrary.Materials;
using ThreeDimensionalVisualizationLibrary.Objects;

namespace ThreeDimensionalVisualizationLibrary.Formats
{
    public class OBJFormatHandler
    {
        protected const string OBJECT_PREFIX = "o ";
        protected const string POSITION_PREFIX = "v ";
        protected const string NORMAL_PREFIX = "vn ";
        protected const string TEXTURE_COORDINATES_PREFIX = "vt ";
        protected const string FACE_PREFIX = "f ";
        protected const string MATERIAL_LIBRARY_PREFIX = "mtllib ";
        protected const string MATERIAL_USAGE_PREFIX = "usemtl ";
        protected const string MATERIAL_DEFINITION_PREFIX = "newmtl ";
        protected const string AMBIENT_COLOR_PREFIX = "Ka ";
        protected const string DIFFUSE_COLOR_PREFIX = "Kd ";
        protected const string SPECULAR_COLOR_PREFIX = "Ks ";
        protected const string SPECULAR_EXPONENT_PREFIX = "Ns ";
        protected const string OPACITY_PREFIX = "d ";
        protected const string TRANSPARENCY_PREFIX = "Tr ";
        protected const string DIFFUSE_TEXTURE_MAP_PREFIX = "map_Kd ";

        protected List<Point3D> positionList = null;
        protected List<Point2D> textureCoordinatesList = null;
        protected List<Vector3D> normalList = null;
        protected List<Material> materialList = null;
        //              protected List<OBJFace> faceList = new List<OBJFace>();

        public List<Object3D> Load(string fileName)
        {
            StreamReader fileReader = new StreamReader(fileName);
            string directory = Path.GetDirectoryName(fileName);
            List<string> dataStringList = new List<string>();
            while (!fileReader.EndOfStream)
            {
                string dataString = fileReader.ReadLine();
                dataStringList.Add(dataString);
            }
            Boolean ok;
            List<string> errorList;
            List<Object3D> object3DList = GenerateObjects(dataStringList, directory, out ok, out errorList);
            return object3DList;
        }

        public void SaveMaterialList(List<Material> materialList, string materialFilePath)
        {
            StreamWriter materialWriter = new StreamWriter(materialFilePath);
            string directory = Path.GetDirectoryName(materialFilePath);
            foreach (Material material in materialList)
            {
                materialWriter.WriteLine(MATERIAL_DEFINITION_PREFIX + material.Name);
                materialWriter.WriteLine(AMBIENT_COLOR_PREFIX + material.AmbientColorAsString(' '));
                materialWriter.WriteLine(DIFFUSE_COLOR_PREFIX + material.DiffuseColorAsString(' '));
                materialWriter.WriteLine(SPECULAR_COLOR_PREFIX + material.SpecularColorAsString(' '));
                materialWriter.WriteLine(SPECULAR_EXPONENT_PREFIX + material.Shininess.ToString());
                materialWriter.WriteLine(OPACITY_PREFIX + material.Opacity.ToString()); // No need to write transparency (= 1 - opacity)
                if (material.DiffuseMap != null)
                {
                    materialWriter.WriteLine(DIFFUSE_TEXTURE_MAP_PREFIX + material.DiffuseMapFileName);
                    string diffuseMapFilePath = directory + "\\" + material.DiffuseMapFileName;
                    material.DiffuseMap.Save(diffuseMapFilePath);
                }
                materialWriter.WriteLine("");
            }
            materialWriter.Close();
        }

        public void SaveObjectList(List<Object3D> object3DList, string objectFilePath, string materialFileName)
        {
            StreamWriter objectWriter = new StreamWriter(objectFilePath);
            objectWriter.WriteLine(MATERIAL_LIBRARY_PREFIX + materialFileName);
            positionList = new List<Point3D>();
            normalList = new List<Vector3D>();
            textureCoordinatesList = new List<Point2D>();
            int positionCount = 0;
            int normalCount = 0;
            int textureCoordinatesCount = 0;
            int previousPositionCount = 0;
            int previousNormalCount = 0;
            int previousTextureCoordinatesCount = 0;
            foreach (Object3D object3D in object3DList)
            {
                // First add elements to the positionList, normalList and textureCoordinatesList.
                foreach (Vertex3D vertex3D in object3D.VertexList)
                {
                    positionList.Add(vertex3D.Position);
                    positionCount++;
                    if (vertex3D.NormalVector != null)
                    {
                        normalList.Add(vertex3D.NormalVector);
                        normalCount++;
                    }
                    if (vertex3D.TextureCoordinates != null)
                    {
                        textureCoordinatesList.Add(vertex3D.TextureCoordinates);
                        textureCoordinatesCount++;
                    }
                }
                objectWriter.WriteLine(OBJECT_PREFIX + object3D.Name);
                for (int ii = previousPositionCount; ii < positionCount; ii++)
                {
                    Point3D position = positionList[ii];
                    objectWriter.WriteLine(POSITION_PREFIX + " " + position.X.ToString() + " " + position.Y.ToString() + " " + position.Z.ToString());
                }
                for (int ii = previousTextureCoordinatesCount; ii < textureCoordinatesCount; ii++)
                {
                    Point2D textureCoordinates = textureCoordinatesList[ii];
                    objectWriter.WriteLine(TEXTURE_COORDINATES_PREFIX + " " + textureCoordinates.X.ToString() + " " + textureCoordinates.Y.ToString());
                }
                for (int ii = previousNormalCount; ii < normalCount; ii++)
                {
                    Vector3D normal = normalList[ii];
                    objectWriter.WriteLine(NORMAL_PREFIX + " " + normal.X.ToString() + " " + normal.Y.ToString() + " " + normal.Z.ToString());
                }
                // Next, generate OBJFace information, based on the triangle indices:
                foreach (TriangleIndices triangleIndices in object3D.TriangleIndicesList)
                {
                    Vertex3D vertex1 = object3D.VertexList[triangleIndices.Index1];
                    Vertex3D vertex2 = object3D.VertexList[triangleIndices.Index2];
                    Vertex3D vertex3 = object3D.VertexList[triangleIndices.Index3];
                    if ((vertex1.NormalVector != null) && (vertex2.NormalVector != null) &&
                        (vertex3.NormalVector != null))
                    {
                        if ((vertex1.TextureCoordinates != null) && (vertex2.TextureCoordinates != null)
                             && (vertex3.TextureCoordinates != null))
                        {
                            // Both normals and textures available
                            int index1 = triangleIndices.Index1 + previousPositionCount + 1;
                            int index2 = triangleIndices.Index2 + previousPositionCount + 1;
                            int index3 = triangleIndices.Index3 + previousPositionCount + 1;
                            string objFaceString = FACE_PREFIX +
                               index1.ToString() + "/" + index1.ToString() + "/" + index1.ToString() + " " +
                                index2.ToString() + "/" + index2.ToString() + "/" + index2.ToString() + " " +
                                index3.ToString() + "/" + index3.ToString() + "/" + index3.ToString();
                            objectWriter.WriteLine(objFaceString);
                        }
                    }

                    // Add code here, writing f <v> or f <v>/<vt>/<vn> etc., as required
                }
                if (object3D.Material != null)
                {
                    objectWriter.WriteLine(MATERIAL_USAGE_PREFIX + object3D.Material.Name);
                }
                objectWriter.WriteLine("");
                previousPositionCount = positionCount;
                previousNormalCount = normalCount;
                previousTextureCoordinatesCount = textureCoordinatesCount;
            }
            objectWriter.Close();
        }

        public void Save(List<Object3D> object3DList, string fileName)
        {
            // First, save the materials in the material file name
            string directory = Path.GetDirectoryName(fileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string materialFileName = fileNameWithoutExtension + "_Materials" + ".mtl";
            materialList = new List<Material>();
            foreach (Object3D object3D in object3DList)
            {
                string materialName = object3D.Material.Name;
                Material existingMaterial = materialList.Find(m => m.Name == materialName);
                if (existingMaterial == null) { materialList.Add(object3D.Material); }
            }
            string materialFilePath = directory + "\\" + materialFileName;
            SaveMaterialList(materialList, materialFilePath);
            // Next, save the object information
            string objectFileName = fileNameWithoutExtension + ".obj";
            string objectFilePath = directory + "\\" + objectFileName;
            SaveObjectList(object3DList, objectFilePath, materialFileName);
        }

        protected Point2D ParsePoint2D(string dataString, out Boolean ok)
        {
            ok = true;
            Point2D point2D = null;
            List<string> dataStringSplit = dataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (dataStringSplit.Count < 3) { ok = false; } // first index = the prefix. Following indices = the coordinates.
            else
            {
                float u;
                float v;
                Boolean okU = float.TryParse(dataStringSplit[1], out u);
                Boolean okV = float.TryParse(dataStringSplit[2], out v);
                ok = (okU) && (okV);
                if (ok) { point2D = new Point2D(u, v); }
            }
            return point2D;
        }

        protected Point3D ParsePoint3D(string dataString, out Boolean ok)
        {
            ok = true;
            Point3D point3D = null;
            List<string> dataStringSplit = dataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (dataStringSplit.Count < 4) { ok = false; }  // first index = the prefix. Following indices = the coordinates.
            else
            {
                float x;
                float y;
                float z;
                Boolean okX = float.TryParse(dataStringSplit[1], out x);
                Boolean okY = float.TryParse(dataStringSplit[2], out y);
                Boolean okZ = float.TryParse(dataStringSplit[3], out z);
                ok = (okX) && (okY) && (okZ);
                if (ok) { point3D = new Point3D(x, y, z); }
            }
            return point3D;
        }

        protected Vector3D ParseVector3D(string dataString, out Boolean ok)
        {
            ok = true;
            Vector3D vector3D = null;
            List<string> dataStringSplit = dataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (dataStringSplit.Count < 4) { ok = false; }  // first index = the prefix. Following indices = the coordinates.
            else
            {
                float x;
                float y;
                float z;
                Boolean okX = float.TryParse(dataStringSplit[1], out x);
                Boolean okY = float.TryParse(dataStringSplit[2], out y);
                Boolean okZ = float.TryParse(dataStringSplit[3], out z);
                ok = (okX) && (okY) && (okZ);
                if (ok) { vector3D = new Vector3D(x, y, z); }
            }
            return vector3D;
        }

        protected float ParseFloat(string dataString, out Boolean ok)
        {
            ok = true;
            float shininess = 0f;
            List<string> dataStringSplit = dataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (dataStringSplit.Count < 2) { ok = false; }  // first index = the prefix. Following indices = the double value.
            else
            {
                ok = float.TryParse(dataStringSplit[1], out shininess);
            }
            return shininess;
        }

        protected string ParseString(string dataString, out Boolean ok)
        {
            ok = true;
            string extractedString = "";
            List<string> dataStringSplit = dataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (dataStringSplit.Count < 2) { ok = false; }  // first index = the prefix. Following indices = the double value.
            else
            {
                extractedString = dataStringSplit[1];
            }
            return extractedString;
        }

        protected OBJFace ParseFace(string dataString, out Boolean ok)
        {
            ok = true;
            OBJFace objFace = null;
            string reducedDataString = dataString.Replace(FACE_PREFIX, "");
            if (!reducedDataString.Contains("/"))  // Simple specification of the kind "f 1 2 3 4"
            {
                List<string> dataStringSplit = reducedDataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (dataStringSplit.Count < 3) { ok = false; }
                else
                {
                    objFace = new OBJFace();
                    for (int ii = 0; ii < dataStringSplit.Count; ii++)
                    {
                        int vertexIndex;
                        Boolean parseOK = int.TryParse(dataStringSplit[ii], out vertexIndex);
                        if ((parseOK) && ((vertexIndex-1) < positionList.Count))
                        {
                            objFace.PositionIndexList.Add(vertexIndex-1);  // -1 since the OBJ format start indexing at 1.
                        }
                        else
                        {
                            ok = false;
                            return null;
                        }
                        objFace.PositionIndexList.Add(vertexIndex-1);
                    }
                }
            }
            else if (!reducedDataString.Contains("//"))  // Standard specification with single "/"
            {
                List<string> dataStringSplit = reducedDataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (dataStringSplit.Count < 3) { ok = false; }
                else
                {
                    int elementsPerItem = 0;
                    objFace = new OBJFace();
                    for (int ii = 0; ii < dataStringSplit.Count; ii++)
                    {
                        if (!dataStringSplit[ii].Contains("/"))
                        {
                            ok = false;
                            return null;
                        }
                        else
                        {
                            List<string> itemSplit = dataStringSplit[ii].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            if (ii == 0) { elementsPerItem = itemSplit.Count; }
                            else
                            {
                                if (itemSplit.Count != elementsPerItem)
                                {
                                    ok = false;
                                    return null;
                                }
                            }
                            if (elementsPerItem == 2) // Specification of the form "f 1/1 3/7 ..." (vertex index and normal index)
                            {
                                int vertexIndex;
                                int vertexNormalIndex;
                                Boolean ok1 = int.TryParse(itemSplit[0], out vertexIndex);
                                Boolean ok2 = int.TryParse(itemSplit[1], out vertexNormalIndex);
                                if ((ok1) && ((vertexIndex-1) < positionList.Count) && (ok2) && ((vertexNormalIndex-1) < normalList.Count))
                                {
                                    objFace.PositionIndexList.Add(vertexIndex-1);
                                    objFace.NormalIndexList.Add(vertexNormalIndex-1);
                                }
                                else
                                {
                                    ok = false;
                                    return null;
                                }
                            }
                            else if (elementsPerItem == 3) // Specification of the form "f 1/1/3 3/7/5 ..." (vertex index, texture coordinate index, and normal index)
                            {
                                int vertexIndex;
                                int textureCoordinateIndex;
                                int vertexNormalIndex;
                                Boolean ok1 = int.TryParse(itemSplit[0], out vertexIndex);
                                Boolean ok2 = int.TryParse(itemSplit[1], out textureCoordinateIndex);
                                Boolean ok3 = int.TryParse(itemSplit[2], out vertexNormalIndex);
                                if ((ok1) && ((vertexIndex - 1) < positionList.Count) && (ok2) && ((textureCoordinateIndex-1) < textureCoordinatesList.Count) &&
                                    (ok3) && ((vertexNormalIndex - 1) < normalList.Count))
                                {
                                    objFace.PositionIndexList.Add(vertexIndex - 1);
                                    objFace.TextureCoordinateIndexList.Add(textureCoordinateIndex - 1);
                                    objFace.NormalIndexList.Add(vertexNormalIndex - 1);
                                }
                                else
                                {
                                    ok = false;
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                List<string> dataStringSplit = reducedDataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (dataStringSplit.Count < 3) { ok = false; }
                else
                {
                    int elementsPerItem = 0;
                    objFace = new OBJFace();
                    for (int ii = 0; ii < dataStringSplit.Count; ii++)
                    {
                        if (!dataStringSplit[ii].Contains("//"))
                        {
                            ok = false;
                            return null;
                        }
                        else
                        {
                            List<string> itemSplit = dataStringSplit[ii].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList(); // Removes "//".
                            if (ii == 0) { elementsPerItem = itemSplit.Count; }
                            else
                            {
                                if (itemSplit.Count != elementsPerItem)
                                {
                                    ok = false;
                                    return null;
                                }
                            }
                            if (elementsPerItem == 2) // Specification of the form "f 1/1 3/7 ..." (vertex index and normal index)
                            {
                                int vertexIndex;
                                int vertexNormalIndex;
                                Boolean ok1 = int.TryParse(itemSplit[0], out vertexIndex);
                                Boolean ok2 = int.TryParse(itemSplit[1], out vertexNormalIndex);
                                if ((ok1) && ((vertexIndex - 1) < positionList.Count) && (ok2) && ((vertexNormalIndex - 1) < normalList.Count))
                                {
                                    objFace.PositionIndexList.Add(vertexIndex - 1);
                                    objFace.NormalIndexList.Add(vertexNormalIndex - 1);
                                }
                                else
                                {
                                    ok = false;
                                    return null;
                                }
                            }
                            else
                            {
                                ok = false;
                                return null;
                            }
                        }
                    }
                }
            }
            return objFace;
        }

        protected void GenerateTriangles(Object3D object3D, List<OBJFace> faceList)
        {
            int vertexIndex = 0;
            foreach (OBJFace face in faceList)
            {
                for (int ii = 0; ii < face.PositionIndexList.Count; ii++)
                {
                    int positionIndex = face.PositionIndexList[ii];
                    double x = positionList[positionIndex].X;
                    double y = positionList[positionIndex].Y;
                    double z = positionList[positionIndex].Z;
                    Vertex3D vertex = new Vertex3D(x, y, z);
                    if (ii < face.NormalIndexList.Count)
                    {
                        int normalIndex = face.NormalIndexList[ii];
                        vertex.NormalVector = new Vector3D();
                        vertex.NormalVector.X = normalList[normalIndex].X;
                        vertex.NormalVector.Y = normalList[normalIndex].Y;
                        vertex.NormalVector.Z = normalList[normalIndex].Z;
                    }
                    if (ii < face.TextureCoordinateIndexList.Count)
                    {
                        int textureCoordinateIndex = face.TextureCoordinateIndexList[ii];
                        vertex.TextureCoordinates = new Point2D(textureCoordinatesList[textureCoordinateIndex].X,
                            textureCoordinatesList[textureCoordinateIndex].Y);
                    }
                    object3D.AddVertex(vertex);
                    face.VertexIndexList.Add(vertexIndex);
                    vertexIndex++;
                }
            }
            foreach (OBJFace face in faceList)
            {
                for (int ii = 2; ii < face.PositionIndexList.Count; ii++)  // NOTE: Each face contains at least 3 vertices - see also ParseFace() above.
                {
                    int index1 = face.VertexIndexList[0];
                    int index2 = face.VertexIndexList[ii - 1];
                    int index3 = face.VertexIndexList[ii];

                    /*    int index1 = face.PositionIndexList[0];  // FIX ERROR HERE: CANNOT USE positionIndex after adding vertices above!
                        int index2 = face.PositionIndexList[ii - 1];
                        int index3 = face.PositionIndexList[ii];  */
                    TriangleIndices triangleIndices = new TriangleIndices(index1, index2, index3);
                    object3D.AddTriangle(triangleIndices);
                }
            }
            object3D.GenerateTriangleConnectionLists();
            object3D.ComputeTriangleNormalVectors(); // Needed only for flat shading.
        }

     /*   protected Object3D GenerateObject(List<OBJFace> faceList)
        {
            Object3D object3D = new Object3D();
            int vertexIndex = 0;
            foreach (OBJFace face in faceList)
            {
                for (int ii = 0; ii < face.PositionIndexList.Count; ii++)
                {
                    int positionIndex = face.PositionIndexList[ii];
                    double x = positionList[positionIndex].X;
                    double y = positionList[positionIndex].Y;
                    double z = positionList[positionIndex].Z;
                    Vertex3D vertex = new Vertex3D(x, y, z);
                    if (ii < face.NormalIndexList.Count)
                    {
                        int normalIndex = face.NormalIndexList[ii];
                        vertex.NormalVector = new Vector3D();
                        vertex.NormalVector.X = normalList[normalIndex].X;
                        vertex.NormalVector.Y = normalList[normalIndex].Y;
                        vertex.NormalVector.Z = normalList[normalIndex].Z;
                    }
                    object3D.AddVertex(vertex);
                    face.VertexIndexList.Add(vertexIndex);
                    vertexIndex++;
                }
            }
            foreach (OBJFace face in faceList)
            {
                for (int ii = 2; ii < face.PositionIndexList.Count; ii++)  // NOTE: Each face contains at least 3 vertices - see also ParseFace() above.
                {
                    int index1 = face.VertexIndexList[0];
                    int index2 = face.VertexIndexList[ii - 1];
                    int index3 = face.VertexIndexList[ii];
                    TriangleIndices triangleIndices = new TriangleIndices(index1, index2, index3);
                    object3D.AddTriangle(triangleIndices);
                }
            }
            object3D.GenerateTriangleConnectionLists();
            object3D.ComputeTriangleNormalVectors(); // Needed only for flat shading.
            return object3D;
        }  */

        protected List<Material> LoadMaterials(string filePath, out Boolean ok, out List<string> materialErrorList)
        {
            ok = true;
            materialErrorList = new List<string>();
            List<Material> addedMaterialList = new List<Material>();
            StreamReader materialReader = new StreamReader(filePath);
            List<string> dataStringList = new List<string>();
            while (!materialReader.EndOfStream)
            {
                string dataString = materialReader.ReadLine();
                dataStringList.Add(dataString);
            }
            Material addedMaterial = null;
            for (int ii = 0; ii < dataStringList.Count; ii++)
            {
                string dataString = dataStringList[ii];
                if (dataString.StartsWith(MATERIAL_DEFINITION_PREFIX))
                {
                    if (addedMaterial != null)
                    {
                        materialList.Add(addedMaterial);
                    }
                    addedMaterial = new Material();
                    string materialName = dataString.Replace(MATERIAL_DEFINITION_PREFIX, "").Trim();
                    if (materialName == "")
                    {
                        ok = false;
                        materialErrorList.Add("A material must have a name: " + dataString);
                        return null;
                    }
                    else
                    {
                        addedMaterial.Name = materialName;
                    }
                }
                else if (dataString.StartsWith(AMBIENT_COLOR_PREFIX))
                {
                    Boolean parseOK;
                    Point3D ambientColorInformation = ParsePoint3D(dataString, out parseOK);
                    //   Vector3D ambientColorInformation = ParseVector3D(dataString, out parseOK);
                    if (parseOK)
                    {
                        addedMaterial.AmbientColor[0] = (float)ambientColorInformation.X;
                        addedMaterial.AmbientColor[1] = (float)ambientColorInformation.Y;
                        addedMaterial.AmbientColor[2] = (float)ambientColorInformation.Z;
                    }
                    else
                    {
                        ok = false;
                        materialErrorList.Add("Parsing error in ambient color definition: " + dataString);
                        return null;
                    }
                }
                else if (dataString.StartsWith(DIFFUSE_COLOR_PREFIX))
                {
                    Boolean parseOK;
                    Point3D diffuseColorInformation = ParsePoint3D(dataString, out parseOK);
                //    Vector3D diffuseColorInformation = ParseVector3D(dataString, out parseOK);
                    if (parseOK)
                    {
                        addedMaterial.DiffuseColor[0] = (float)diffuseColorInformation.X;
                        addedMaterial.DiffuseColor[1] = (float)diffuseColorInformation.Y;
                        addedMaterial.DiffuseColor[2] = (float)diffuseColorInformation.Z;
                    }
                    else
                    {
                        ok = false;
                        materialErrorList.Add("Parsing error in diffuse color definition: " + dataString);
                        return null;
                    }
                }
                else if (dataString.StartsWith(SPECULAR_COLOR_PREFIX))
                {
                    Boolean parseOK;
                    Point3D specularColorInformation = ParsePoint3D(dataString, out parseOK);
                //    Vector3D specularColorInformation = ParseVector3D(dataString, out parseOK);
                    if (parseOK)
                    {
                        addedMaterial.SpecularColor[0] = (float)specularColorInformation.X;
                        addedMaterial.SpecularColor[1] = (float)specularColorInformation.Y;
                        addedMaterial.SpecularColor[2] = (float)specularColorInformation.Z;
                    }
                    else
                    {
                        ok = false;
                        materialErrorList.Add("Parsing error in specular color definition: " + dataString);
                        return null;
                    }
                }
                else if (dataString.StartsWith(SPECULAR_EXPONENT_PREFIX))
                {
                    Boolean parseOK;
                    float shininess = ParseFloat(dataString, out parseOK);
                    if (parseOK)
                    {
                        addedMaterial.Shininess = shininess;
                    }
                    else
                    {
                        ok = false;
                        materialErrorList.Add("Parsing error in specular exponent definition: " + dataString);
                        return null;
                    }
                }
                else if (dataString.StartsWith(OPACITY_PREFIX))
                {
                    Boolean parseOK;
                    float opacity = ParseFloat(dataString, out parseOK);
                    if (parseOK)
                    {
                        addedMaterial.Opacity = opacity;
                    }
                    else
                    {
                        ok = false;
                        materialErrorList.Add("Parsing error in opacity definition: " + dataString);
                        return null;
                    }
                }
                else if (dataString.StartsWith(TRANSPARENCY_PREFIX))
                {
                    Boolean parseOK;
                    float transparency = ParseFloat(dataString, out parseOK);
                    if (parseOK)
                    {
                        addedMaterial.Opacity = 1-transparency;
                    }
                    else
                    {
                        ok = false;
                        materialErrorList.Add("Parsing error in opacity definition: " + dataString);
                        return null;
                    }
                }
                else if (dataString.StartsWith(DIFFUSE_TEXTURE_MAP_PREFIX))
                {
                    Boolean diffuseMapNameOK;
                    string fileName = ParseString(dataString, out diffuseMapNameOK);
                    if (diffuseMapNameOK)
                    {
                        string diffuseMapFilePath = Path.GetDirectoryName(filePath) + "\\" + fileName;
                        if (File.Exists(diffuseMapFilePath))
                        {
                            Bitmap diffuseMap = (Bitmap)Image.FromFile(diffuseMapFilePath);
                            addedMaterial.DiffuseMapFileName = fileName;
                            addedMaterial.DiffuseMap = diffuseMap;
                        }
                        else
                        {
                            ok = false;
                            materialErrorList.Add("Error: Could not find texture named " + fileName);
                            return null;
                        }
                    }
                    else
                    {
                        ok = false;
                        materialErrorList.Add("Error: Diffuse map name missing: " + dataString);
                        return null;
                    }
                }

                // Add more here: textures.

            }
            if (addedMaterial != null) { addedMaterialList.Add(addedMaterial); }
            materialReader.Close();
            return addedMaterialList;
        }

        // NOTE: the directory is required for reading material files (mtllib)
        protected List<Object3D> GenerateObjects(List<string> dataStringList, string directory, out Boolean ok, out List<string> errorList)
        {
            ok = true;
            errorList = new List<string>();
            List<OBJFace> faceList = new List<OBJFace>();
            List<Object3D> object3DList = new List<Object3D>();
            Object3D object3D = null;
            Boolean inObject = false;
            positionList = new List<Point3D>();
            textureCoordinatesList = new List<Point2D>();
            normalList = new List<Vector3D>();
            materialList = new List<Material>();
            // First pass: Define positions, texture coordinates, and normals:
            foreach (string dataString in dataStringList)
            {
                if (!dataString.StartsWith("#"))
                {

                    if (dataString.StartsWith(POSITION_PREFIX))
                    {
                        Boolean parseOK;
                        Point3D position = ParsePoint3D(dataString, out parseOK);
                    //    Vector3D position = ParseVector3D(dataString, out parseOK);
                    /*    position.Z += 2900;
                        position.Z /= 10.0;
                        position.X /= 10.0;
                        position.Y /= 10.0;    */
                        if (parseOK) { positionList.Add(position); }
                        else
                        {
                            ok = false;
                            errorList.Add("Parsing error in position vector: " + dataString);
                            return null;
                        }
                    }
                    else if (dataString.StartsWith(NORMAL_PREFIX))
                    {
                        Boolean parseOK;
                        Vector3D vertexNormal = ParseVector3D(dataString, out parseOK);
                        if (parseOK) { normalList.Add(vertexNormal); }
                        else
                        {
                            ok = false;
                            errorList.Add("Parsing error in vertex normal vector: " + dataString);
                            return null;
                        }
                    }
                    else if (dataString.StartsWith(TEXTURE_COORDINATES_PREFIX))
                    {
                        Boolean parseOK;
                        Point2D uv = ParsePoint2D(dataString, out parseOK);
                        if (parseOK) { textureCoordinatesList.Add(uv); }
                        else
                        {
                            ok = false;
                            errorList.Add("Parsing error in texture coordinates specification: " + dataString);
                            return null;
                        }
                    }
                    else if (dataString.StartsWith(MATERIAL_LIBRARY_PREFIX))
                    {
                        string relativeFilePath = dataString.Replace(MATERIAL_LIBRARY_PREFIX, "").Trim();
                        string filePath = directory + "\\" + relativeFilePath;
                        if (!File.Exists(filePath))
                        {
                            ok = false;
                            errorList.Add("Could not find material library: " + relativeFilePath);
                            return null;
                        }
                        else
                        {
                            Boolean materialOK;
                            List<string> materialErrorList;
                            List<Material> addedMaterialList = LoadMaterials(filePath, out materialOK, out materialErrorList);
                            if (materialOK)
                            {
                                materialList.AddRange(addedMaterialList);
                            }
                            else
                            {
                                ok = false;
                                errorList.AddRange(materialErrorList);
                                return null;
                            }
                        }
                    }
                }
            }

            // Second pass: Define objects as sets of faces, with material property specifications
            foreach (string dataString in dataStringList)
            {
                if (dataString.StartsWith(OBJECT_PREFIX)) 
                {
                    if (inObject)
                    {
                        GenerateTriangles(object3D, faceList);
                        //    object3D = GenerateObject(faceList);
                        object3DList.Add(object3D);
                    }
                    object3D = new Object3D();
                    Boolean nameOK;
                    object3D.Name = ParseString(dataString, out nameOK);
                    if (!nameOK)
                    {
                        ok = false;
                        errorList.Add("Each object must have a name!");
                        return null;
                    }
                    faceList = new List<OBJFace>();
                    inObject = true;
                }
                else if (dataString.StartsWith(FACE_PREFIX))
                {
                    Boolean parseOK;
                    OBJFace face = ParseFace(dataString, out parseOK);
                    if (parseOK) { faceList.Add(face); }
                    else
                    {
                        ok = false;
                        errorList.Add("Parsing error in face specification: " + dataString);
                        return null;
                    }
                }
                else if (dataString.StartsWith(MATERIAL_USAGE_PREFIX))
                {
                    Boolean materialUsageOK;
                    string materialName = ParseString(dataString, out materialUsageOK);
                    if (materialUsageOK)
                    {
                        Material material = materialList.Find(m => m.Name == materialName);
                        if (material == null)
                        {
                            ok = false;
                            errorList.Add("Error: Material " + materialName + "not available.");
                            return null;
                        }
                        else
                        {
                            //    if (object3D == null) { return object3DList; }
                            object3D.Material = material;
                        }
                    }
                    else
                    {
                        ok = false;
                        errorList.Add("Error: Material name missing: " + dataString);
                        return null;
                    }
                }
            }

            if (inObject)
            {
                GenerateTriangles(object3D, faceList);
                //  object3D = GenerateObject(faceList);
                object3DList.Add(object3D);
            }
            // Next, build actual vertices from the specifications in the face list,
            // making sure not to duplicate vertices
            //     List<Boolean> definedList = new List<Boolean>();
            //    foreach (Vector3D position in positionList) { definedList.Add(false); }
            /*    int vertexIndex = 0;
                foreach (OBJFace face in faceList)
                {
                    for (int ii = 0; ii < face.PositionIndexList.Count; ii++)
                    {
                        int positionIndex = face.PositionIndexList[ii];
                        //    if (definedList[positionIndex] == false)
                        //   {
                        //      definedList[positionIndex] = true;
                        double x = positionList[positionIndex].X;
                        double y = positionList[positionIndex].Y;
                        double z = positionList[positionIndex].Z;
                        Vertex3D vertex = new Vertex3D(x, y, z);
                        if (ii < face.NormalIndexList.Count)
                        {
                            int normalIndex = face.NormalIndexList[ii];
                            vertex.NormalVector = new Vector3D();
                            vertex.NormalVector.X = normalList[normalIndex].X;
                            vertex.NormalVector.Y = normalList[normalIndex].Y;
                            vertex.NormalVector.Z = normalList[normalIndex].Z;
                        }
                        object3D.AddVertex(vertex);
                        face.VertexIndexList.Add(vertexIndex);
                        vertexIndex++;
                        //    }
                    }
                }

                // Now generate the object by triangulating the polygons defined in the faceList.
                // Use a simple triangulation, in which the first corner of each polygon appears
                // in every triangle:
                foreach (OBJFace face in faceList)
                {
                    for (int ii = 2; ii < face.PositionIndexList.Count; ii++)  // NOTE: Each face contains at least 3 vertices - see also ParseFace() above.
                    {
                        int index1 = face.VertexIndexList[0];  
                        int index2 = face.VertexIndexList[ii - 1];
                        int index3 = face.VertexIndexList[ii];
                        TriangleIndices triangleIndices = new TriangleIndices(index1, index2, index3);
                        object3D.AddTriangle(triangleIndices);
                    }
                }

                object3D.GenerateTriangleConnectionLists();
                object3D.ComputeTriangleNormalVectors();  */

            return object3DList;
        }
    }
}
