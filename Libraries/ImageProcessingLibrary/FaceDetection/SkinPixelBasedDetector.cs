using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingLibrary.FaceDetection
{
    public class SkinPixelBasedDetector: FaceDetector
    {
        private const int DEFAULT_NUMBER_OF_CENTER_POSITIONS = 5;

        private double faceCenterHorizontalPosition;
        private int numberOfCenterPositions = DEFAULT_NUMBER_OF_CENTER_POSITIONS;
        private List<int> centerHorizontalPositionList = new List<int>();

        protected override void ProcessBitmap(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                ImageProcessor skinPixelDetectionProcessor = new ImageProcessor(bitmap);
                skinPixelDetectionProcessor.FindSkinPixelsRGB();
                skinPixelDetectionProcessor.Release();
                processedBitmap = skinPixelDetectionProcessor.Bitmap;
                ImageProcessor faceDetectionProcessor = new ImageProcessor(processedBitmap);
                faceDetectionProcessor.Binarize(1);
                double[,] imageMatrix = faceDetectionProcessor.AsGrayMatrix();
                List<Tuple<int, int>> horizontalPositionSkinPixelTupleList = new List<Tuple<int, int>>();
                List<int> horizontalSkinPixelList = new List<int>();
                for (int ii = 0; ii < bitmap.Width; ii++)
                {
                    int skinPixels = 0;
                    for (int jj = 0; jj < bitmap.Height; jj++)
                    {
                        if (imageMatrix[ii, jj] > 0) { skinPixels++; }
                    }
                    Tuple<int, int> horizontalPositionSkinPixelTuple = new Tuple<int, int>(ii, skinPixels);
                    horizontalPositionSkinPixelTupleList.Add(horizontalPositionSkinPixelTuple);
                    horizontalSkinPixelList.Add(skinPixels);
                }
                List<int> verticalSkinPixelList = new List<int>();
                for (int jj = 0; jj < bitmap.Height; jj++)
                {
                    int skinPixels = 0;
                    for (int ii = 0; ii < bitmap.Width; ii++)
                    {
                        if (imageMatrix[ii, jj] > 0) { skinPixels++; }
                    }
                    verticalSkinPixelList.Add(skinPixels);
                }
                if (horizontalPositionSkinPixelTupleList.Count > 0)
                {
                    horizontalPositionSkinPixelTupleList.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                    horizontalPositionSkinPixelTupleList.Reverse();
                    faceCenterHorizontalPosition = 0;
                    double centerFraction = 0.8;
                    double maxValue = horizontalPositionSkinPixelTupleList[0].Item2;
                    int index = 0;
                    double thresholdValue = (int)(centerFraction * maxValue);
                    while (horizontalPositionSkinPixelTupleList[index].Item2 >= thresholdValue)
                    {
                        faceCenterHorizontalPosition += horizontalPositionSkinPixelTupleList[index].Item1;
                        index++;
                        if (index >= horizontalPositionSkinPixelTupleList.Count) { break; }
                    }
                    faceCenterHorizontalPosition /= index;
                    int faceCenterIntHorizontalPosition = (int)Math.Round(faceCenterHorizontalPosition);
                    double edgeFraction = 0.25;
                    double edgeThreshold = (int)(edgeFraction * maxValue);
                    int horizontalScanLineIndex = faceCenterIntHorizontalPosition;
                    double scanLineSkinPixels = horizontalSkinPixelList[faceCenterIntHorizontalPosition];
                    while (scanLineSkinPixels > edgeThreshold)
                    {
                        horizontalScanLineIndex++;
                        if (horizontalScanLineIndex >= horizontalSkinPixelList.Count) { break; }
                        scanLineSkinPixels = horizontalSkinPixelList[horizontalScanLineIndex];
                    }
                    int rightEdge = horizontalScanLineIndex;
                    if (rightEdge >= bitmap.Width) { rightEdge = bitmap.Width - 1; }
                    horizontalScanLineIndex = faceCenterIntHorizontalPosition;
                    scanLineSkinPixels = horizontalSkinPixelList[faceCenterIntHorizontalPosition];
                    while (scanLineSkinPixels > edgeThreshold)
                    {
                        horizontalScanLineIndex--;
                        if (horizontalScanLineIndex < 0) { break; }
                        scanLineSkinPixels = horizontalSkinPixelList[horizontalScanLineIndex];
                    }
                    int leftEdge = horizontalScanLineIndex;
                    if (leftEdge < 0) { leftEdge = 0; }

                    int maximumVerticalSkinPixels = verticalSkinPixelList.Max();
                    int maximumVerticalIndex = verticalSkinPixelList.IndexOf(maximumVerticalSkinPixels);
                    edgeThreshold = (int)(edgeFraction * maximumVerticalSkinPixels);
                    int verticalScanLineIndex = maximumVerticalIndex;
                    int verticalSkinPixels = verticalSkinPixelList[verticalScanLineIndex];
                    while (verticalSkinPixels > edgeThreshold)
                    {
                        verticalScanLineIndex--;
                        if (verticalScanLineIndex <= 0) { break; }
                        verticalSkinPixels = verticalSkinPixelList[verticalScanLineIndex];
                    }
                    int topEdge = verticalScanLineIndex;
                    int width = rightEdge - leftEdge;
                    int heightEstimate = (int)Math.Round(1.3 * width);
                    int bottomEdge = topEdge + heightEstimate;
                    if (bottomEdge >= bitmap.Height) { bottomEdge = bitmap.Height - 1; }
                    OnFaceCenterPositionAvailable(faceCenterIntHorizontalPosition);

                    if (centerHorizontalPositionList.Count > numberOfCenterPositions)
                    {
                        centerHorizontalPositionList.RemoveAt(0);
                    }
                    centerHorizontalPositionList.Add(faceCenterIntHorizontalPosition);
                    double min = centerHorizontalPositionList.Min();
                    double max = centerHorizontalPositionList.Max();
                    Boolean lockAcquired = false;
                    if (((faceCenterIntHorizontalPosition - min) < 10) && ((max - faceCenterIntHorizontalPosition) < 10))
                    {
                        lockAcquired = true;
                    }
                    OnFaceBoundingBoxAvailable(leftEdge, rightEdge, topEdge, bottomEdge, lockAcquired);
                }
                faceDetectionProcessor.Release();
            }
        }

        public int NumberOfCenterPositions
        {
            get { return numberOfCenterPositions; }
            set { numberOfCenterPositions = value; }
        }
    }
}
