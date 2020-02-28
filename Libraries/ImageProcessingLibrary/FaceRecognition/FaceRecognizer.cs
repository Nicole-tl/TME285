using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingLibrary.FaceRecognition
{
    public abstract class FaceRecognizer
    {
        public abstract string Recognize(Bitmap bitmap);
    }
}
