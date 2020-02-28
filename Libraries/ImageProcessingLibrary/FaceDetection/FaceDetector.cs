using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuxiliaryLibrary;
using ImageProcessingLibrary.Cameras;

namespace ImageProcessingLibrary.FaceDetection
{
    public abstract class FaceDetector
    {
        protected const double DEFAULT_TIME_STEP = 0.050;
        protected const int DEFAULT_MILLISECOND_TIMEOUT = 25;

        protected Camera camera = null;
        protected Boolean running = false;
        protected Thread processingThread = null;
        protected double timeStep = DEFAULT_TIME_STEP;
        protected int millisecondTimeout = DEFAULT_MILLISECOND_TIMEOUT;
        protected int millisecondTimeStep;
        protected Stopwatch stopwatch;

        protected static object lockObject = new object();

        public event EventHandler<IntEventArgs> FaceCenterPositionAvailable = null;
        public event EventHandler<FaceDetectionEventArgs> FaceBoundingBoxAvailable = null;

        protected Bitmap processedBitmap;

        public void SetCamera(Camera camera)
        {
            this.camera = camera;
        }

        protected abstract void ProcessBitmap(Bitmap bitmap);

        private void ProcessLoop()
        {
            while (running)
            {
                if (!camera.IsRunning()) { running = false; }
                stopwatch.Start();
                Bitmap bitmap = camera.GetBitmap();
                Monitor.Enter(lockObject);
                ProcessBitmap(bitmap);
                Monitor.Exit(lockObject);
           /*     if (Monitor.TryEnter(lockObject, millisecondTimeout))
                {
                    ProcessBitmap(bitmap);
                    Monitor.Exit(lockObject);
                }  */
                stopwatch.Stop();
                double elapsedSeconds = stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
                if (elapsedSeconds < timeStep)
                {
                    int sleepTime = (int)Math.Round(1000 * (timeStep - elapsedSeconds));
                    Thread.Sleep(sleepTime);
                }
                stopwatch.Reset();
            }
        }

        public void Start()
        {
            if (!running)
            {
                stopwatch = new Stopwatch();
                millisecondTimeStep = (int)Math.Round(1000 * timeStep);
                running = true;
                processingThread = new Thread(new ThreadStart(ProcessLoop));
                processingThread.Start();
            }
        }

        public void Stop()
        {
            running = false;
        }

        protected void OnFaceCenterPositionAvailable(int position)
        {
            if (FaceCenterPositionAvailable != null)
            {
                EventHandler<IntEventArgs> handler = FaceCenterPositionAvailable;
                IntEventArgs e = new IntEventArgs(position);
                handler(this, e);
            }
        }

        protected void OnFaceBoundingBoxAvailable(int left, int right, int top, int bottom, Boolean lockAcquired)
        {
            if (FaceBoundingBoxAvailable != null)
            {
                EventHandler<FaceDetectionEventArgs> handler = FaceBoundingBoxAvailable;
                FaceDetectionEventArgs e = new FaceDetectionEventArgs(left, right, top, bottom, lockAcquired);
                handler(this, e);
            }
        }

        public Bitmap GetProcessedBitmap()
        {
            Monitor.Enter(lockObject);
            Bitmap accessedBitmap = new Bitmap(processedBitmap);
            Monitor.Exit(lockObject);
            return accessedBitmap;
        }
    }
}
