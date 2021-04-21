using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        Vector3 _limiting_limitPosition = Vector3.zero;
        Vector3 _limiting_influenceOffset = Vector3.zero;

        /// <summary> Helping stretching limiting be more responsible (lerp value help) </summary>
        float _limiting_stretchingHelperTooLong = 0f;
        float _limiting_stretchingHelperTooShort = 0f;

        /// <summary>
        /// If tail is too long we making it shorter if too short - back to default scale
        /// </summary>
        protected void StretchingLimiting(TailSegment bone)
        {
            Vector3 backDir = (bone.ParentBone.ProceduralPosition) - (bone.ProceduralPosition);
            float dist = backDir.magnitude;

            if (dist > 0f)
            {
                float maxDist = bone.BoneLengthScaled + bone.BoneLengthScaled * 2.5f * MaxStretching;

                if (dist > maxDist) // If tail too long
                {
                    _limiting_limitPosition = bone.ProceduralPosition + backDir * ((dist - bone.BoneLengthScaled) / dist);

                    if (MaxStretching == 0f)
                        bone.ProceduralPosition = _limiting_limitPosition;
                    else
                    {
                        float limValue = Mathf.InverseLerp(dist, 0f, maxDist) + _limiting_stretchingHelperTooLong; if (limValue > 0.999f) limValue = 0.99f;
                        if (ReactionSpeed < 0.5f) limValue *= deltaForLerps * (10f + ReactionSpeed * 30f);
                        bone.ProceduralPosition = Vector3.LerpUnclamped(bone.ProceduralPosition, _limiting_limitPosition, limValue);
                    }
                }
                else // If tail too short
                {
                    maxDist = bone.BoneLengthScaled + bone.BoneLengthScaled * 1.1f * MaxStretching;

                    if (dist < maxDist)
                    {
                        _limiting_limitPosition = bone.ProceduralPosition + backDir * ((dist - bone.BoneLengthScaled) / dist);

                        if (MaxStretching == 0f)
                            bone.ProceduralPosition = _limiting_limitPosition;
                        else
                            bone.ProceduralPosition = Vector3.LerpUnclamped(bone.ProceduralPosition, _limiting_limitPosition, Mathf.InverseLerp(dist, 0f, maxDist) + _limiting_stretchingHelperTooShort);
                    }
                }

            }

        }

        Quaternion _limiting_angle_ToTargetRot;
        Quaternion _limiting_angle_targetInLocal;
        Quaternion _limiting_angle_newLocal;

        /// <summary>
        /// If segment rotation is in too big angle we straighten it
        /// </summary>
        protected Vector3 AngleLimiting(TailSegment child, Vector3 targetPos)
        {
            float angleFactor = 0f;
            _limiting_limitPosition = targetPos;


            _limiting_angle_ToTargetRot = (
             Quaternion.FromToRotation
             (
                 child.ParentBone.transform.TransformDirection(child.LastKeyframeLocalPosition),
                 targetPos - child.ParentBone.ProceduralPosition)
             )
             * child.ParentBone.transform.rotation;

            _limiting_angle_targetInLocal = FEngineering.QToLocal(child.ParentBone.transform.rotation, _limiting_angle_ToTargetRot); // Quaternion.Inverse(child.ParentBone.PreviousRotation) * _limiting_angle_ToTargetRot;


            // Limiting all axis or one
            float angleDiffToInitPose = 0f;

            if (AngleLimitAxis.sqrMagnitude == 0f) // All axis limit angle
                angleDiffToInitPose = Quaternion.Angle(_limiting_angle_targetInLocal, child.LastKeyframeLocalRotation);
            else // Selective axis
            {
                #region Selective axis limits

                AngleLimitAxis.Normalize();

                if (LimitAxisRange.x == LimitAxisRange.y)
                {
                    angleDiffToInitPose = Mathf.DeltaAngle(
                        Vector3.Scale(child.InitialLocalRotation.eulerAngles, AngleLimitAxis).magnitude,
                        Vector3.Scale(_limiting_angle_targetInLocal.eulerAngles, AngleLimitAxis).magnitude);

                    if (angleDiffToInitPose < 0f) angleDiffToInitPose = -angleDiffToInitPose;
                }
                else
                {
                    angleDiffToInitPose = Mathf.DeltaAngle(
                        Vector3.Scale(child.InitialLocalRotation.eulerAngles, AngleLimitAxis).magnitude,
                        Vector3.Scale(_limiting_angle_targetInLocal.eulerAngles, AngleLimitAxis).magnitude);

                    if (angleDiffToInitPose > LimitAxisRange.x && angleDiffToInitPose < LimitAxisRange.y) angleDiffToInitPose = 0f;
                    if (angleDiffToInitPose < 0) angleDiffToInitPose = -angleDiffToInitPose;
                }

                #endregion
            }


            #region Debug
            //Debug.Log("Atarget in local = " +
            //    FEngineering.WrapVector(_limiting_angle_targetInLocal.eulerAngles) + " last key local = " +
            //    FEngineering.WrapVector(child.lastKeyframeLocalRotation.eulerAngles) + " angle = " + angleDiffToInitPose);
            #endregion

            // Finding rotate back to limited angle coordinates
            if (angleDiffToInitPose > AngleLimit)
            {

                float exceededAngle = Mathf.Abs(Mathf.DeltaAngle(angleDiffToInitPose, AngleLimit));
                angleFactor = Mathf.InverseLerp(0f, AngleLimit, exceededAngle); // percentage value (0-1) from target rotation to limit

                #region Debug

                //Debug.DrawLine(child.ParentBone.ParentBone.transform.position + child.ParentBone.ParentBone.ProceduralRotation * child.ParentBone.transform.localPosition,
                //child.ProceduralPosition, Color.red, 1f);

                //Debug.Log("[" + child.Index + "] diff = " 
                //    + angleDiffToInitPose + " exc =  " 
                //    + exceededAngle + " fact = " 
                //    + angleFactor);

                #endregion


                if (LimitSmoothing > Mathf.Epsilon)
                {
                    float smooth = Mathf.Lerp(55f, 15f, LimitSmoothing);
                    _limiting_angle_newLocal = Quaternion.SlerpUnclamped(_limiting_angle_targetInLocal, child.LastKeyframeLocalRotation, deltaForLerps * smooth * angleFactor);
                }
                else
                    _limiting_angle_newLocal = Quaternion.SlerpUnclamped(_limiting_angle_targetInLocal, child.LastKeyframeLocalRotation, angleFactor);


                _limiting_angle_ToTargetRot = FEngineering.QToWorld(child.ParentBone.transform.rotation, _limiting_angle_newLocal);
                _limiting_limitPosition = child.ParentBone.ProceduralPosition + _limiting_angle_ToTargetRot * Vector3.Scale(child.transform.lossyScale, child.LastKeyframeLocalPosition);

            }

            if (angleFactor > Mathf.Epsilon) return _limiting_limitPosition; else return targetPos;
        }


        /// <summary>
        /// Limiting tail motion in world space position movement
        /// </summary>
        void MotionInfluenceLimiting()
        {
            if (MotionInfluence != 1f)
            {   // one - param: param = 1 -> 0  param = 0 -> 1
                _limiting_influenceOffset = (BaseTransform.position - previousWorldPosition) * (1f - MotionInfluence);

                for (int i = 0; i < TailSegments.Count; i++)
                {
                    TailSegments[i].ProceduralPosition += _limiting_influenceOffset;
                    TailSegments[i].PreviousPosition += _limiting_influenceOffset;
                }

                GhostChild.ProceduralPosition += _limiting_influenceOffset;
                GhostChild.PreviousPosition += _limiting_influenceOffset;
            }
        }


        /// <summary> Helper gravity calculations variable to avoid GC </summary>
        Vector3 _tc_segmentGravityOffset = Vector3.zero;
        /// <summary> Helper gravity calculations variable to avoid GC </summary>
        Vector3 _tc_segmentGravityToParentDir = Vector3.zero;
        Vector3 _tc_preGravOff = Vector3.zero;

        /// <summary>
        /// Calculating gravity parameter position offset for tail segment with some vector direction operations
        /// </summary>
        void CalculateGravityPositionOffsetForSegment(TailSegment bone)
        {
            //if (updateLoops > 0)
            {
                _tc_segmentGravityOffset = (bone.Gravity + WindEffect) * bone.BoneLengthScaled;
                _tc_segmentGravityToParentDir = bone.ProceduralPosition - bone.ParentBone.ProceduralPosition;

                _tc_preGravOff = (_tc_segmentGravityToParentDir + _tc_segmentGravityOffset).normalized * _tc_segmentGravityToParentDir.magnitude;

                // Keeping same length of the bone to prevent gravity effect from stretching bones but offsetting in computed direction
                bone.ProceduralPosition = bone.ParentBone.ProceduralPosition + _tc_preGravOff;
            }
            //else
            //{
            //    bone.ProceduralPosition = bone.ParentBone.ProceduralPosition + _tc_preGravOff;
            //}
        }


        /// <summary>
        /// Limiting movement of tail bones in selected axis
        /// </summary>
        void Axis2DLimit(TailSegment child)
        {
            child.ProceduralPosition -=
                FEngineering.VAxis2DLimit(
                    child.ParentBone.transform,
                    child.ParentBone.ProceduralPosition,
                    child.ProceduralPosition, Axis2D);

        }


        #region Distance Limiting Calculations

        [Tooltip("If you want to use max distance fade option to smoothly disable tail animator when object is going far away from camera")]
        public bool UseMaxDistance = false;

        [Tooltip("(By default camera transform) Measuring distance from this object to define if object is too far and not need to update tail animator")]
        public Transform DistanceFrom;
        [HideInInspector]
        public Transform _distanceFrom_Auto;

        [Tooltip("Max distance to main camera / target object to smoothly turn off tail animator.")]
        public float MaximumDistance = 35f;

        [Tooltip("If object in range should be detected only when is nearer than 'MaxDistance' to avoid stuttery enabled - disable switching")]
        [Range(0.0f, 1f)]
        public float MaxOutDistanceFactor = 0f;

        [Tooltip("If distance should be measured not using Up (y) axis")]
        public bool DistanceWithoutY = false;

        [Tooltip("Offsetting point from which we want to measure distance to target")]
        public Vector3 DistanceMeasurePoint;

        [Tooltip("Disable fade duration in seconds")]
        [Range(0.25f, 2f)]
        public float FadeDuration = 0.75f;

        private bool maxDistanceExceed = false;
        private Transform finalDistanceFrom;
        private bool wasCameraSearch = false;

        /// <summary> Multiplier for blend weight when tail animator is far from camera or provided object </summary>
        private float distanceWeight = 1f;


        /// <summary>
        /// Getting distance value from distance measure point to target position
        /// </summary>
        public float GetDistanceMeasure(Vector3 targetPosition)
        {
            if (DistanceWithoutY)
            {
                Vector3 p = BaseTransform.position + BaseTransform.TransformVector(DistanceMeasurePoint);
                Vector2 p2 = new Vector2(p.x, p.z);
                return Vector2.Distance(p2, new Vector2(targetPosition.x, targetPosition.z));
            }
            else
                return Vector3.Distance(BaseTransform.position + BaseTransform.TransformVector(DistanceMeasurePoint), targetPosition);
        }


        /// <summary>
        /// Handling max distance feature
        /// </summary>
        private void MaxDistanceCalculations()
        {

            if (DistanceFrom != null)
                finalDistanceFrom = DistanceFrom;
            #region Defining distance measure reference if not found
            else
            {
                if (finalDistanceFrom == null)
                {
                    if (_distanceFrom_Auto == null)
                    {
                        Camera c = Camera.main;
                        if (c) _distanceFrom_Auto = c.transform;
                        else
                        {
                            if (!wasCameraSearch)
                            {
                                c = FindObjectOfType<Camera>();
                                if (c) _distanceFrom_Auto = c.transform;
                                wasCameraSearch = true;
                            }
                        }
                    }

                    finalDistanceFrom = _distanceFrom_Auto;
                }
            }
            #endregion


            // If we are using distance limitation
            if (MaximumDistance > 0f && finalDistanceFrom != null)
            {
                if (!maxDistanceExceed) // If look motion is not out of look range etc.
                {
                    float distance = GetDistanceMeasure(finalDistanceFrom.position);

                    if (distance > MaximumDistance + MaximumDistance * MaxOutDistanceFactor)
                        maxDistanceExceed = true;

                    distanceWeight += Time.unscaledDeltaTime * (1f / FadeDuration);
                    if (distanceWeight > 1f) distanceWeight = 1f;
                }
                else // When disabling tail animator
                {
                    // Entering back distance range
                    float distance = GetDistanceMeasure(finalDistanceFrom.position);
                    if (distance <= MaximumDistance) maxDistanceExceed = false;

                    distanceWeight -= Time.unscaledDeltaTime * (1f / FadeDuration);
                    if (distanceWeight < 0f) distanceWeight = 0f;
                }
            }
            else // If we don't use max of distance feature
            {
                maxDistanceExceed = false;
                distanceWeight = 1f;
            }
        }


        #endregion


    }

}