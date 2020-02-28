using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AuxiliaryLibrary;
using ImageProcessingLibrary.Cameras;

namespace ImageProcessingLibrary.FaceDetection
{
    public partial class FaceDetectionControl : CameraViewControl
    {
        private FaceDetector faceDetector;
        private int faceLeft = -1;
        private int faceRight = -1;
        private int faceTop = -1;
        private int faceBottom = -1;
        private int faceCenterPosition = -1;
        private Boolean faceLockAcquired = false;
        private Bitmap faceBitmap = null;
        private Boolean drawBoundingBox = true;
        private Boolean showProcessedBitmap = false;
        private Boolean showCenterLine = true;
        private Boolean showBoundingBox = true;

        public FaceDetectionControl()
        {
            InitializeComponent();
        }

        public void SetFacedetector(FaceDetector faceDetector)
        {
            this.faceDetector = faceDetector;
            this.faceDetector.FaceCenterPositionAvailable += new EventHandler<IntEventArgs>(HandleFaceCenterHorizontalPositionAvailable);
            this.faceDetector.FaceBoundingBoxAvailable += new EventHandler<FaceDetectionEventArgs>(HandleFaceBoundingBoxAvailable);
        }

        public override void SetCamera(Camera camera)
        {
            base.SetCamera(camera);
            camera.CaptureDevice.SetVideoProperty(DirectShowLib.VideoProcAmpProperty.WhiteBalance, 2500);
            camera.CaptureDevice.SetVideoProperty(DirectShowLib.VideoProcAmpProperty.Brightness, 0);
            faceCenterPosition = -1;
        }

        protected Bitmap DrawFaceDetectionInformation(Bitmap bitmap)
        {
            ImageProcessor centerBoundingBoxProcessor = new ImageProcessor(bitmap);
            if (showCenterLine)
            {
                if ((faceCenterPosition >= 0) && (faceCenterPosition < bitmap.Width))
                {
                    centerBoundingBoxProcessor.DrawVerticalLine(faceCenterPosition, Color.Red);
                }
            }
            if (showBoundingBox)
            {
                if ((faceLeft >= 0) && (faceRight < bitmap.Width) && (faceTop >= 0) && (faceBottom < bitmap.Height))
                {
                    if (!faceLockAcquired)
                    {
                        centerBoundingBoxProcessor.DrawBox(faceLeft, faceRight, faceTop, faceBottom, Color.Red);
                    }
                    else
                    {
                        centerBoundingBoxProcessor.DrawBox(faceLeft, faceRight, faceTop, faceBottom, Color.Lime);
                    }
                }
            }
            centerBoundingBoxProcessor.Release();
            return centerBoundingBoxProcessor.Bitmap;
        }

        protected override Bitmap ProcessImage(Bitmap bitmap)
        {
            if (showProcessedBitmap)
            {
                Bitmap processedBitmap = faceDetector.GetProcessedBitmap();
                if (processedBitmap != null)
                {
                    if ((showBoundingBox) || (showCenterLine))
                    {
                        Bitmap finalBitmap = DrawFaceDetectionInformation(processedBitmap);
                        return finalBitmap;
                    }
                    else
                    {
                        return processedBitmap;
                    }
                }
                else
                {
                    Bitmap finalBitmap = DrawFaceDetectionInformation(bitmap);
                    return finalBitmap;  // Fallback: Show at least the unprocessed image.
                } 
            }
            else
            {
                Bitmap finalBitmap = DrawFaceDetectionInformation(bitmap);
                return finalBitmap;
            }
        }

        private void HandleFaceCenterHorizontalPositionAvailable(object sender, IntEventArgs e)
        {
            faceCenterPosition = e.IntValue;
        }

        private void HandleFaceBoundingBoxAvailable(object sender, FaceDetectionEventArgs e)
        {
            faceLeft = e.Left;
            faceRight = e.Right;
            faceTop = e.Top;
            faceBottom = e.Bottom;
            faceLockAcquired = e.LockAcquired;
        }

        public Boolean IsRunning()
        {
            if (running) { return true; }
            else { return false; }
        }

        public override void Stop()
        {
            base.Stop();
            if (faceDetector != null)
            {
                faceDetector.Stop();
            }
        }

        public FaceDetector FaceDetector
        {
            get { return faceDetector; }
        }

        public Boolean DrawBoundingBox
        {
            get { return drawBoundingBox; }
            set { drawBoundingBox = value; }
        }

        public Boolean ShowProcessedBitmap
        {
            get { return showProcessedBitmap; }
            set { showProcessedBitmap = value; }
        }

        public Boolean ShowBoundingBox
        {
            get { return showBoundingBox; }
            set { showBoundingBox = value; }
        }

        public Boolean ShowCenterLine
        {
            get { return showCenterLine; }
            set { showCenterLine = value; }
        }
    }
}
