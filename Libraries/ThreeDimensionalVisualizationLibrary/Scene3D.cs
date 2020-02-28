using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using AuxiliaryLibrary;
using MathematicsLibrary.Geometry;
using ObjectSerializerLibrary;
using ThreeDimensionalVisualizationLibrary.Animations;
using ThreeDimensionalVisualizationLibrary.Formats;
using ThreeDimensionalVisualizationLibrary.Objects;

namespace ThreeDimensionalVisualizationLibrary
{
    [DataContract]
    [Serializable]
    public class Scene3D
    {
        protected const double DEFAULT_ANIMATION_STEP_DURATION = 0.05;

        protected double animationStepDuration = DEFAULT_ANIMATION_STEP_DURATION;

        private List<Object3D> objectList = new List<Object3D>(); // null;
        private List<Light> lightList = new List<Light>(); // null;
        private List<Animation> animationList = new List<Animation>(); // null;
     //   private List<AnimationStatus> animationStatusList = null;
    //    private List<int> animationQueue = null;

        private List<Object3D> expandedObjectList = null;
        private List<Tuple<string, Vector3D, Vector3D>> initialPoseTupleList = new List<Tuple<string, Vector3D, Vector3D>>();

        private object lockObject = null;

        [field: NonSerialized]
        public event EventHandler<StringEventArgs> AnimationDone;

        private Boolean synchronousAnimationOngoing = false;
        private List<int> animationQueue = new List<int>();

        private double cameraDistance = 2;
        private double cameraLatitude = 0.5;
        private double cameraLongitude = 0.785;
        private float[] cameraTarget = new float[] { 0f, 0f, 0f };

        public Scene3D()
        {
            objectList = new List<Object3D>();
            lightList = new List<Light>();
            animationList = new List<Animation>();
            animationQueue = new List<int>();
            initialPoseTupleList = new List<Tuple<string, Vector3D, Vector3D>>();
          //  animationQueue = new List<int>();
        }


        // NOTE: does not handle nested objects!
        public void SortForTranslucence()
        {
            objectList.Sort((a, b) => a.Material.Opacity.CompareTo(b.Material.Opacity));
            objectList.Reverse();
        }

        public Object3D GetObject(string name)
        {
            Object3D foundObject = null;
            foreach (Object3D object3D in objectList)
            {
                foundObject = object3D.FindObject(name);
                if (foundObject != null) { break; }
            }
            return foundObject;
        }

        public void HighlightObject(string name)
        {
            List<Object3D> allObjectsList = GetAllObjects(null, true);
            foreach (Object3D object3D in allObjectsList) { object3D.HighLight = false; }
            if (name != null)
            {
                Object3D highlightObject = GetObject(name);
                highlightObject.HighLight = true;
            }
        }

        // Gets the list of (top-level) objects, with child objects included.
        public List<Object3D> GetAllObjects(Object3D baseObject, Boolean expandObjects)
        {
            List<Object3D> retrievedObjectList = new List<Object3D>();
            if (baseObject == null)
            {
                foreach (Object3D object3D in objectList)
                {
                    retrievedObjectList.Add(object3D);   
                    if (object3D.Object3DList.Count > 0)
                    {
                        if (expandObjects)
                        {
                            foreach (Object3D childObject in object3D.Object3DList)
                            {
                                retrievedObjectList.Add(childObject);
                                List<Object3D> grandchildObjectList = GetAllObjects(childObject, expandObjects);
                                retrievedObjectList.AddRange(grandchildObjectList);
                            }
                        }
                    }
                }
            }
            else
            {
                if (expandObjects)
                {
                    if (baseObject.Object3DList.Count > 0)
                    {
                        retrievedObjectList.AddRange(baseObject.Object3DList);
                    }
                }
            }
            return retrievedObjectList;
        }

        public Boolean AnyLightOn()
        {
            Light firstLightOn = lightList.Find(l => l.IsOn == true);
            if (firstLightOn == null) { return false; }
            else { return true; }
        }

        public void AddObject(Object3D object3D)
        {
            if (objectList == null) { objectList = new List<Object3D>(); }
            objectList.Add(object3D);
        }

        protected void AnimationLoop(int animationIndex)
        {
            Animation animation = animationList[animationIndex];
            Object3D animatedObject = expandedObjectList.Find(o => o.Name == animation.ObjectName);
            int animationStepCount1 = (int)Math.Round(animation.Duration1 / animationStepDuration)+1;
            int animationStepCount2 = (int)Math.Round(animation.Duration2 / animationStepDuration) + 1;
            int millisecondStepDuration1 = (int)Math.Round(1000*animation.Duration1 / animationStepCount1);
            int millisecondStepDuration2 = (int)Math.Round(1000 * animation.Duration2 / animationStepCount2);
            int keyFrame1Index = animatedObject.KeyFrameList.FindIndex(k => k.Name == animation.FirstKeyFrameName);
            int keyFrame2Index = animatedObject.KeyFrameList.FindIndex(k => k.Name == animation.SecondKeyFrameName);

            Vector3D initialPosition = new Vector3D();
            initialPosition.X = animatedObject.Position[0];
            initialPosition.Y = animatedObject.Position[1];
            initialPosition.Z = animatedObject.Position[2];
            Vector3D initialRotation = new Vector3D();
            initialRotation.X = animatedObject.Rotation[0];
            initialRotation.Y = animatedObject.Rotation[1];
            initialRotation.Z = animatedObject.Rotation[2];
            Vector3D firstFinalPosition = new Vector3D();
            firstFinalPosition.X = animation.FirstDeltaPosition.X + initialPosition.X;
            firstFinalPosition.Y = animation.FirstDeltaPosition.Y + initialPosition.Y;
            firstFinalPosition.Z = animation.FirstDeltaPosition.Z + initialPosition.Z;
            Vector3D firstFinalRotation = new Vector3D();
            firstFinalRotation.X = animation.FirstDeltaRotation.X + initialRotation.X;
            firstFinalRotation.Y = animation.FirstDeltaRotation.Y + initialRotation.Y;
            firstFinalRotation.Z = animation.FirstDeltaRotation.Z + initialRotation.Z;

            Stopwatch animationStopWatch = new Stopwatch();
            Monitor.Enter(lockObject);
            animatedObject.InterpolatePose(initialPosition, initialPosition, initialRotation, initialRotation, 0);
            Monitor.Exit(lockObject);
            // First part of the animation:
            int initialKeyFrameIndex = animatedObject.CurrentKeyFrameIndex;
            if (animation.Duration1 > 0)
            {
                for (int ii = 0; ii <= animationStepCount1; ii++)
                {
                    animationStopWatch.Reset();
                    animationStopWatch.Start();
                    double beta = ii / (double)animationStepCount1;
                    Monitor.Enter(lockObject);
                    if (keyFrame1Index != initialKeyFrameIndex)
                    {
                        animatedObject.InterpolateKeyFrames(initialKeyFrameIndex, keyFrame1Index, beta);
                    }
                    animatedObject.InterpolatePose(initialPosition, firstFinalPosition, initialRotation, firstFinalRotation, beta);
                    Monitor.Exit(lockObject);
                    int elapsedMilliseconds = (int)Math.Round(animationStopWatch.ElapsedTicks / (double)Stopwatch.Frequency);
                    if (elapsedMilliseconds < millisecondStepDuration1)
                    {
                        int sleepDuration = millisecondStepDuration1 - elapsedMilliseconds;
                        Thread.Sleep(sleepDuration);
                    }
                }
                Monitor.Enter(lockObject);
                animatedObject.AssignKeyFrame(keyFrame1Index);
                Monitor.Exit(lockObject);
            }


            initialPosition = new Vector3D();
            initialPosition.X = animatedObject.Position[0];
            initialPosition.Y = animatedObject.Position[1];
            initialPosition.Z = animatedObject.Position[2];
            initialRotation = new Vector3D();
            initialRotation.X = animatedObject.Rotation[0];
            initialRotation.Y = animatedObject.Rotation[1];
            initialRotation.Z = animatedObject.Rotation[2];
            Vector3D secondFinalPosition = new Vector3D();
            secondFinalPosition.X = animation.SecondDeltaPosition.X + initialPosition.X;
            secondFinalPosition.Y = animation.SecondDeltaPosition.Y + initialPosition.Y;
            firstFinalPosition.Z = animation.SecondDeltaPosition.Z + initialPosition.Z;
            Vector3D secondFinalRotation = new Vector3D();
            secondFinalRotation.X = animation.SecondDeltaRotation.X + initialRotation.X;
            secondFinalRotation.Y = animation.SecondDeltaRotation.Y + initialRotation.Y;
            secondFinalRotation.Z = animation.SecondDeltaRotation.Z + initialRotation.Z;


            // Second part of the animation:
            initialKeyFrameIndex = animatedObject.CurrentKeyFrameIndex;
            if (animation.Duration2 > 0)
            {
                for (int ii = 0; ii <= animationStepCount2; ii++)
                {
                    animationStopWatch.Reset();
                    animationStopWatch.Start();
                    double beta = ii / (double)animationStepCount2;
                    Monitor.Enter(lockObject);
                    if (keyFrame2Index != initialKeyFrameIndex)
                    {
                        animatedObject.InterpolateKeyFrames(initialKeyFrameIndex, keyFrame2Index, beta);
                    }
                    animatedObject.InterpolatePose(initialPosition, secondFinalPosition, initialRotation, secondFinalRotation, beta);
                    Monitor.Exit(lockObject);
                    int elapsedMilliseconds = (int)Math.Round(animationStopWatch.ElapsedTicks / (double)Stopwatch.Frequency);
                    if (elapsedMilliseconds < millisecondStepDuration2)
                    {
                        int sleepDuration = millisecondStepDuration2 - elapsedMilliseconds;
                        Thread.Sleep(sleepDuration);
                    }
                }
                Monitor.Enter(lockObject);
                animatedObject.AssignKeyFrame(keyFrame2Index);
                Monitor.Exit(lockObject);
            }
            if (animation.IsSynchronous)
            {
                synchronousAnimationOngoing = false;
            }
            if (animationQueue.Count > 0)
            {
                RunAnimation(animationQueue[0]);
                animationQueue.RemoveAt(0);
            }
            else
            {
                OnAnimationDone(animatedObject.Name);
            }
        }

        //
        // Note to students: Whenever the agent makes an utterance, it is sent to the
        // visualizer, with the prefix "Speech", making it possible to animate the
        // agent's speech. Note, however, that animations run sequentially so that,
        // if another animation (e.g. a movement) is running when the speech animation
        // request is received, the latter will run only after the completion of the
        // ongoing animation.
        //
        public void RunAnimation(string animationSpecification)
        {
            if (animationSpecification.StartsWith(AnimationConstants.GENERAL_ANIMATION_PREFIX))  // General animation
            {
                string animationName = animationSpecification.Replace(AnimationConstants.GENERAL_ANIMATION_PREFIX, "");
                int animationIndex = animationList.FindIndex(a => a.Name.ToLower() == animationName.ToLower());
                if (animationIndex >= 0)
                {
                    RunAnimation(animationIndex);
                }
            }
            else // Speech animation
            {
                string animatedSpeech = animationSpecification.Replace(AnimationConstants.SPEECH_ANIMATION_PREFIX, "");
                int animationIndex = animationList.FindIndex(a => a.Name.ToLower() == animatedSpeech.ToLower());
                if (animationIndex >= 0)
                {
                    RunAnimation(animationIndex);
                }
            }
        }

        public void RunAnimation(int animationIndex)
        {
            Animation animation = animationList[animationIndex];
            if (animationQueue == null) { animationQueue = new List<int>(); }
            if ((!synchronousAnimationOngoing) || (!animation.IsSynchronous))
            {
                if (animation.IsSynchronous)
                {
                    synchronousAnimationOngoing = true; // Set the Boolean here, since it takes a while to start the thread.
                }
                expandedObjectList = GetAllObjects(null, true);
           //     Animation animation = animationList[animationIndex];
                int animatedObjectIndex = expandedObjectList.FindIndex(o => o.Name == animation.ObjectName);
                Thread animationThread = new Thread(new ThreadStart(() => AnimationLoop(animationIndex)));
                animationThread.Start();
            }
            else { animationQueue.Add(animationIndex); }
        }

        // 20191028
        public void ResetAnimations()
        {
            if (expandedObjectList == null)
            {
                expandedObjectList = GetAllObjects(null, true);
            }
            foreach (Object3D object3D in expandedObjectList)
            {
                object3D.AssignKeyFrame(0);
                Animation animation = animationList.Find(a => a.ObjectName == object3D.Name);
                if (animation != null)
                {
                    Monitor.Enter(lockObject);
                    Vector3D initialPosition = new Vector3D(object3D.InitialPosition[0], object3D.InitialPosition[1], object3D.InitialPosition[2]);
                    Vector3D initialRotation = new Vector3D(object3D.InitialRotation[0], object3D.InitialRotation[1], object3D.InitialRotation[2]);
                    object3D.InterpolatePose(initialPosition, initialPosition, initialRotation, initialRotation, 0);
            //        object3D.InterpolatePose(animation.InitialPosition, animation.InitialPosition, animation.InitialRotation, animation.InitialRotation, 0);
                    Monitor.Exit(lockObject);
                }
            }
        }

        // 20191028
        private void OnAnimationDone(string animatedObjectName)
        {
            if (AnimationDone != null)
            {
                EventHandler<StringEventArgs> handler = AnimationDone;
                StringEventArgs e = new StringEventArgs(animatedObjectName);
                handler(this, e);
            }
        }

        // 20191028
        public void SetLockObject(object lockObject)
        {
            this.lockObject = lockObject;
        }

        // 20200104 (assumes that all objects have unique names)
        public Object3D GetParent(Object3D object3D)
        {
            List<Object3D> allObjectsList = GetAllObjects(null, true);
            foreach (Object3D potentialParentObject in allObjectsList)
            {
                if (potentialParentObject.Object3DList != null)
                {
                    int objectIndex = potentialParentObject.Object3DList.FindIndex(o => o.Name == object3D.Name);
                    if (objectIndex >= 0) { return potentialParentObject; }
                }
            }
            return null;
        }

        [DataMember]
        public List<Object3D> ObjectList
        {
            get { return objectList; }
            set { objectList = value; }
        }

        [DataMember]
        public List<Light> LightList
        {
            get { return lightList; }
            set { lightList = value; }
        }

        [DataMember]
        public List<Animation> AnimationList
        {
            get { return animationList; }
            set { animationList = value; }
        }

        [DataMember]
        public double AnimationStepDuration
        {
            get { return animationStepDuration; }
            set { animationStepDuration = value; }
        }

        [DataMember]
        public double CameraDistance
        {
            get { return cameraDistance; }
            set { cameraDistance = value; }
        }

        [DataMember]
        public double CameraLatitude
        {
            get { return cameraLatitude; }
            set { cameraLatitude = value; }
        }

        [DataMember]
        public double CameraLongitude
        {
            get { return cameraLongitude; }
            set { cameraLongitude = value; }
        }

        [DataMember]
        public float[] CameraTarget
        {
            get { return cameraTarget; }
            set { cameraTarget = value; }
        }
    }
}
