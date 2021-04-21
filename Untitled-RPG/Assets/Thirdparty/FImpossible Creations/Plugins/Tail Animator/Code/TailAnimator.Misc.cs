using System.Collections;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        /// <summary> For resetting tail procedural data when just enabling animator </summary>
        bool wasDisabled = true;
        /// <summary> Just selected delta value from Time.delta unmodified </summary>
        float justDelta = 0.016f;
        /// <summary> Delta used for component logics -> it's value is near full 1f </summary>
        float secPeriodDelta = 0.5f;
        /// <summary> Delta for pos = Lerp(pos, target) operations -> It's value is desired to be around 0.016f but much higher in higher fps domain </summary>
        float deltaForLerps = 0.016f;
        /// <summary> Helper delta for target update rate usage -> -> It's value is desired to be 1f / targetRate </summary>
        float rateDelta = 0.016f;

        /// <summary> Helper for calculating stable delta calculations </summary>
        protected float collectedDelta = 0f;
        /// <summary> How many udpate loops should be done according to stable update rate </summary>
        protected int framesToSimulate = 1;
        protected int previousframesToSimulate = 1;

        bool updateTailAnimator = false;

        /// <summary>
        /// Conditions to do any calculations within Tail Animator
        /// </summary>
        void CheckIfTailAnimatorShouldBeUpdated()
        {
            if (!initialized)
            {
                if (StartAfterTPose)
                {
                    startAfterTPoseCounter++;
                    if (startAfterTPoseCounter > 6) Init();
                }

                updateTailAnimator = false;
                return;
            }

            #region Debug "`" disable key for editor only

#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.BackQuote))
            {
                updateTailAnimator = false;
                return;
            }
#endif

            #endregion


            if (UseMaxDistance)
            {
                MaxDistanceCalculations();
                conditionalWeight = OverrideWeight * distanceWeight;
            }
            else
                conditionalWeight = OverrideWeight;


            if (OptimizeWithMesh != null) if (OptimizeWithMesh.isVisible == false) { updateTailAnimator = false; return; }


            //#region Triggering Animate Physics Support

            //if (AnimatePhysics)
            //{
            //    if (!animatePhysicsWorking) StartCoroutine(AnimatePhysicsClock());
            //    if (!triggerAnimatePhysics) { updateTailAnimator = false; return; } else triggerAnimatePhysics = false; 
            //}

            //#endregion

            if (UseCollision) if (!collisionInitialized) SetupSphereColliders();

            if (TailSegments.Count == 0)
            {
                Debug.LogError("[TAIL ANIMATOR] No tail bones defined in " + name + " !");
                initialized = false;
                updateTailAnimator = false;
                return;
            }


            // Enabling / disabling with blending value
            if (TailAnimatorAmount * conditionalWeight <= Mathf.Epsilon)
            {
                wasDisabled = true;
                updateTailAnimator = false;
                return;
            }
            else
            {
                if (wasDisabled)
                {
                    User_ReposeTail();
                    previousWorldPosition = transform.position;
                    wasDisabled = false;
                }
            }

            if (IncludeParent) if (TailSegments.Count > 0) if (!TailSegments[0].transform.parent) IncludeParent = false;
            
            if (TailSegments.Count < 1)
            {
                updateTailAnimator = false;
                return;
            }

            updateTailAnimator = true;
        }


        /// <summary>
        /// Just defining delta time for component operations
        /// </summary>
        void DeltaTimeCalculations()
        {
            if (UpdateRate > 0) // If we setted target update rate
            {
                switch (DeltaType)
                {
                    case EFDeltaType.SafeDelta: case EFDeltaType.DeltaTime: justDelta = Time.deltaTime / Mathf.Clamp(Time.timeScale, 0.01f, 1f); break;
                    case EFDeltaType.SmoothDeltaTime: justDelta = Time.smoothDeltaTime; break;
                    case EFDeltaType.UnscaledDeltaTime: justDelta = Time.unscaledDeltaTime; break;
                    case EFDeltaType.FixedDeltaTime: justDelta = Time.fixedDeltaTime; break;
                }

                secPeriodDelta = 1f;
                deltaForLerps = secPeriodDelta;
                rateDelta = 1f / (float)UpdateRate;

                StableUpdateRateCalculations();
            }
            else // Unlimited update rate
            {
                switch (DeltaType)
                {
                    case EFDeltaType.SafeDelta: justDelta = Mathf.Lerp(justDelta, GetClampedSmoothDelta(), 0.075f); break;
                    case EFDeltaType.DeltaTime: justDelta = Time.deltaTime; break;
                    case EFDeltaType.SmoothDeltaTime: justDelta = Time.smoothDeltaTime; break;
                    case EFDeltaType.UnscaledDeltaTime: justDelta = Time.unscaledDeltaTime; break;
                    case EFDeltaType.FixedDeltaTime: justDelta = Time.fixedDeltaTime; break;
                }

                deltaForLerps = Mathf.Pow(secPeriodDelta, 0.1f) * 0.02f;
                rateDelta = justDelta;

                // Helper parameter to not calculate "*60" i-times
                secPeriodDelta = Mathf.Min(1f, justDelta * 60f);

                framesToSimulate = 1;
                previousframesToSimulate = 1;
            }
        }


        /// <summary>
        /// Calculating how many update loops should be done in this frame according to target update rate and elapsed deltaTime
        /// </summary>
        void StableUpdateRateCalculations()
        {
            previousframesToSimulate = framesToSimulate; // Remembering how many loops used in this frame for bones animation calibration in next frame
            collectedDelta += justDelta; // Collecting delta time from game frames
            framesToSimulate = 0; // Collecting delta for [one second] div by [UpdateRate] update so for one frame in static defined time rate

            while (collectedDelta >= rateDelta) // Collected delta is big enough to do tail motion frame
            {
                collectedDelta -= rateDelta;
                framesToSimulate += 1;
                if (framesToSimulate >= 3) { collectedDelta = 0; break; } // Simulating up to 3 frames update in one unity game frame
            }
        }


        /// <summary>
        /// Preparing not animated bones, if animated they will be changed after Update() and before LateUpdate() by Unity Animator
        /// </summary>
        void PreCalibrateBones()
        {
            TailSegment child = TailSegments[0];

            while (child != GhostChild)
            {
                child.transform.localPosition = child.InitialLocalPosition;
                child.transform.localRotation = child.InitialLocalRotation;
                child = child.ChildBone;
            }

        }


        /// <summary>
        /// Preparing bones for animation synchronized with keyframe animations
        /// </summary>
        void CalibrateBones()
        {
            if (UseIK)
                if (IKBlend > 0f)
                    UpdateIK();

            _limiting_stretchingHelperTooLong = Mathf.Lerp(0.4f, 0.0f, MaxStretching);
            _limiting_stretchingHelperTooShort = _limiting_stretchingHelperTooLong * 1.5f;
        }


        /// <summary>
        /// Checking for null referencees in ghost chain list
        /// </summary>
        public void CheckForNullsInGhostChain()
        {
            if (_TransformsGhostChain == null) _TransformsGhostChain = new System.Collections.Generic.List<Transform>();
            for (int i = _TransformsGhostChain.Count - 1; i >= 0; i--)
            {
                if (_TransformsGhostChain[i] == null) _TransformsGhostChain.RemoveAt(i);
            }
        }



        /// <summary> Helper variable for start after t-pose feature </summary>
        int startAfterTPoseCounter;


        /// <summary>
        /// Limiting smooth delta in certain ranges to prevent jittery
        /// </summary>
        float GetClampedSmoothDelta()
        {
            return Mathf.Clamp(Time.smoothDeltaTime, 0f, 0.25f);
        }


        /// <summary>
        /// Helper quaternion multiply method for non-slithery motion
        /// </summary>
        Quaternion MultiplyQ(Quaternion rotation, float times)
        {
            return Quaternion.AngleAxis(rotation.x * Mathf.Rad2Deg * times, Vector3.right) *
                   Quaternion.AngleAxis(rotation.z * Mathf.Rad2Deg * times, Vector3.forward) *
                   Quaternion.AngleAxis(rotation.y * Mathf.Rad2Deg * times, Vector3.up);
        }


        #region Curves Operations



        /// <summary>
        /// Evaluating value for tail segment from given clamped curve
        /// </summary>
        public float GetValueFromCurve(int i, AnimationCurve c)
        {
            if (!initialized) return c.Evaluate((float)i / (float)_TransformsGhostChain.Count);
            else
                return c.Evaluate(TailSegments[i].IndexOverlLength);
        }


        /// <summary>
        /// Clamping curve keys to fit in given bounds
        /// </summary>
        public AnimationCurve ClampCurve(AnimationCurve a, float timeStart, float timeEnd, float lowest, float highest)
        {
            Keyframe[] keys = a.keys;

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].time < timeStart) keys[i].time = timeStart; else if (keys[i].time > timeEnd) keys[i].time = timeEnd;
                if (keys[i].value < lowest) keys[i].value = lowest; else if (keys[i].value > highest) keys[i].value = highest;
            }

            a.keys = keys;
            return a;
        }


        /// <summary>
        /// Checking if curve keyframes are not equal
        /// </summary>
        //public bool KeysChanged(Keyframe[] a, Keyframe[] b)
        //{
        //    if (a == null || b == null) return true;
        //    if (a.Length != b.Length) return true;

        //    for (int i = 0; i < a.Length; i++)
        //    {
        //        if (a[i].time != b[i].time) return true;
        //        if (a[i].value != b[i].value) return true;
        //        if (a[i].inTangent != b[i].inTangent) return true;
        //        if (a[i].outTangent != b[i].outTangent) return true;
        //    }

        //    return false;
        //}

        #endregion


        /// <summary>
        /// Making sure ghost transform chain list is valid
        /// </summary>
        public void RefreshTransformsList()
        {
            if (_TransformsGhostChain == null) _TransformsGhostChain = new System.Collections.Generic.List<Transform>();
            else
                for (int i = _TransformsGhostChain.Count - 1; i >= 0; i--)
                    if (_TransformsGhostChain[0] == null) _TransformsGhostChain.RemoveAt(i);
        }

    }
}