using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace ThreeDimensionalVisualizationLibrary.Materials
{
    [DataContract]
    [Serializable]
    public class Material
    {
        public const int NO_TEXTURE_AVAILABLE = -1;
        public const int TEXTURE_NOT_LOADED = -2;

        private string name;
        private float[] ambientColor = new float[] { 1f, 1f, 1f, 1f };
        private float[] diffuseColor = new float[] { 1f, 1f, 1f, 1f };
        private float[] specularColor = new float[] { 1f, 1f, 1f, 1f };
        private float shininess;
        private float opacity = 1f; // 1 = opaque, 0 = transparent

        private Bitmap diffuseMap = null;
        private int diffuseMapHandle = NO_TEXTURE_AVAILABLE; // = the id of the texture (once it has been loaded).
        private string diffuseMapFileName = null;

        public int LoadDiffuseMap()
        {
            if (diffuseMapHandle == TEXTURE_NOT_LOADED)
            {
                GL.Enable(EnableCap.Texture2D);

                int id = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, id);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                //Describe to gl what we want the bound texture to look like
                GL.TexImage2D(
                    TextureTarget.Texture2D, 0,
                    PixelInternalFormat.Rgba,
                    diffuseMap.Width, diffuseMap.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte, IntPtr.Zero);

                //Lock pixel data to memory and prepare for pass through
                var bitmapData = diffuseMap.LockBits(new Rectangle(
                    0, 0, diffuseMap.Width, diffuseMap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //Tell gl to write the data from  bitmap image to the bound texture
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0,
                    diffuseMap.Width, diffuseMap.Height,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte, bitmapData.Scan0);

                // Release lock and unbind texture
                diffuseMap.UnlockBits(bitmapData);
                //GL.BindTexture(TextureTarget.Texture2D, 0);

                diffuseMapHandle = id;
            }

            return diffuseMapHandle;
        }

        public void SetAmbientColor(Color color)
        {
            if (ambientColor == null) { ambientColor = new float[] { 1f, 1f, 1f, 1f }; }
            ambientColor[0] = color.R / (float)255;
            ambientColor[1] = color.G / (float)255;
            ambientColor[2] = color.B / (float)255;
            ambientColor[3] = color.A / (float)255;
        }

        public void SetDiffuseColor(Color color)
        {
            if (diffuseColor == null) { diffuseColor = new float[] { 1f, 1f, 1f, 1f }; }
            diffuseColor[0] = color.R / (float)255;
            diffuseColor[1] = color.G / (float)255;
            diffuseColor[2] = color.B / (float)255;
            diffuseColor[3] = color.A / (float)255;
        }

        public void SetSpecularColor(Color color)
        {
            if (specularColor == null) { specularColor = new float[] { 1f, 1f, 1f, 1f }; }
            specularColor[0] = color.R / (float)255;
            specularColor[1] = color.G / (float)255;
            specularColor[2] = color.B / (float)255;
            specularColor[3] = color.A / (float)255;
        }

        public Bitmap DiffuseMap
        {
            get { return diffuseMap; }
            set
            {
                diffuseMap = value;
                diffuseMapHandle = TEXTURE_NOT_LOADED;  // Must run LoadTextures() to provide the texture to OpenGL.
            }
        }

        public string AmbientColorAsString(char splitCharacter)
        {
            string ambientColorAsString = "";
            for (int ii = 0; ii <= 2; ii++)
            {
                ambientColorAsString += ambientColor[ii].ToString() + splitCharacter.ToString(); // " ";  // NOTE: The alpha channel (index 3) is specified in the opacity field.
            }
            ambientColorAsString = ambientColorAsString.TrimEnd(new char[] { splitCharacter });
            return ambientColorAsString;
        }

        public string DiffuseColorAsString(char splitCharacter)
        {
            string diffuseColorAsString = "";
            for (int ii = 0; ii <= 2; ii++)
            {
                diffuseColorAsString += diffuseColor[ii].ToString() + splitCharacter.ToString();   // NOTE: The alpha channel (index 3) is specified in the opacity field.
            }
            diffuseColorAsString = diffuseColorAsString.TrimEnd(new char[] { splitCharacter });
            return diffuseColorAsString;
        }

        public string SpecularColorAsString(char splitCharacter)
        {
            string specularColorAsString = "";
            for (int ii = 0; ii <= 2; ii++)
            {
                specularColorAsString += specularColor[ii].ToString() + splitCharacter.ToString();  // NOTE: The alpha channel (index 3) is specified in the opacity field.
            }
            specularColorAsString = specularColorAsString.TrimEnd(new char[] { splitCharacter });
            return specularColorAsString;
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public float[] AmbientColor
        {
            get { return ambientColor; }
            set { ambientColor = value; }
        }

        [DataMember]
        public float[] DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }

        [DataMember]
        public float[] SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        [DataMember]
        public float Shininess
        {
            get { return shininess; }
            set { shininess = value; }
        }

        [DataMember]
        public float Opacity
        {
            get { return opacity; }
            set
            {
                opacity = value;
                ambientColor[3] = opacity; // Alpha channel
                diffuseColor[3] = opacity;
                if (specularColor == null) { specularColor = new float[] { 1f, 1f, 1f, 1f }; }  // Required for deserialization (fields in alphabetical order).
                specularColor[3] = opacity;
            }
        }

        [DataMember]
        public string DiffuseMapFileName
        {
            get { return diffuseMapFileName; }
            set { diffuseMapFileName = value; }
        }
    }
}
