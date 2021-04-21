using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        /// <summary> Tail calculations start index for interations  </summary>
        int _tc_startI = 0;
        /// <summary> Indexes in front of root </summary>
        int _tc_startII = 1;
        /// <summary> Delta time multiplied one itme instead of i-times </summary>
        //float _tc_offsetDelta = 0.1f;
        /// <summary> Full length of tail computed at initialize in world space, used for unify animation feature </summary>
        public float _TC_TailLength { get; private set; }

        /// <summary> Tail calculations helper bone class instance reference </summary>
        TailSegment _tc_rootBone = null;

        /// <summary> Tail calculations helper rotation variable to avoid GC </summary>
        Quaternion _tc_lookRot = Quaternion.identity;
        /// <summary> Tail calculations helper rotation variable to avoid GC </summary>
        Quaternion _tc_targetParentRot = Quaternion.identity;

        /// <summary> Chain root offset rotation for shaping and auto-waving features </summary>
        Quaternion _tc_startBoneRotOffset = Quaternion.identity;
        float _tc_tangle = 1f;


        /// <summary>
        /// Define root of chain >> _tc_rootBone - root bone of tail chain >> TailBones[0] or GhostParent
        /// </summary>
        void TailCalculations_Begin()
        {
            if (IncludeParent) // Include first bone in chain to be modified
            {
                _tc_startI = 0;
                _tc_rootBone = TailSegments[0];
            }
            else // Leaving first bone in chain intact (exclude)
            {
                _tc_startI = 1;
                if (TailSegments.Count > 1) _tc_rootBone = TailSegments[1]; else { _tc_rootBone = TailSegments[0]; _tc_startI = -1; }
            }

            _tc_startII = _tc_startI + 1;
            if (_tc_startII > TailSegments.Count - 1) _tc_startII = -1;

            if (Deflection > Mathf.Epsilon) if (!_pp_initialized) InitializePostProcessing();


            if (Tangle < 0)
                _tc_tangle = Mathf.LerpUnclamped(1f, 1.5f, Tangle + 1f);
            else
                _tc_tangle = Mathf.LerpUnclamped(1f, -4f, Tangle);
        }



        /// <summary>
        /// Defining start bone root reference coordinates and initial processing
        /// </summary>
        void TailSegments_UpdateRootFeatures()
        {
            // Root bone rotation offset for auto-waving feature
            if (UseWaving)
            {
                Waving_AutoSwingUpdate();
                _tc_startBoneRotOffset = (WavingRotationOffset * RotationOffset);
            }
            else
                _tc_startBoneRotOffset = RotationOffset;

            // Sustain support
            if (Sustain > Mathf.Epsilon) Waving_SustainUpdate();

            // Post process advanced features
            if (PostProcessingNeeded()) PostProcessing_Begin();

        }



        /// <summary>
        /// Processing calculated simple segment position with special effects like limiting / collision / smoothing etc.
        /// Calling methods using bone's parent variables
        /// </summary>
        void TailCalculations_SegmentPreProcessingStack(TailSegment child)
        {
            if (!UseCollision) // Basic motion without collision
            // Different update order with enable/disable collisions to avoid jittering on angle limiting
            {
                // Limit segments angles
                if (AngleLimit < 181) child.ProceduralPosition = AngleLimiting(child, child.ProceduralPosition);

                // Smoothing motion
                if (child.PositionSpeed < 1f) child.ProceduralPosition = TailCalculations_SmoothPosition(child.PreviousPosition /*+ _limiting_influenceOffset*/, child.ProceduralPosition, child);
            }
            else
            {
                // Smoothing motion
                if (child.PositionSpeed < 1f) child.ProceduralPosition = TailCalculations_SmoothPosition(child.PreviousPosition /*+ _limiting_influenceOffset*/, child.ProceduralPosition, child);

                // Computing collision offset as first thing
                TailCalculations_ComputeSegmentCollisions(child, ref child.ProceduralPosition);

                // Limit segments angles
                if (AngleLimit < 181) child.ProceduralPosition = AngleLimiting(child, child.ProceduralPosition);
            }


            // Control stretching
            if (MaxStretching < 1f) StretchingLimiting(child);

            // Apply gravity
            if (!FEngineering.VIsZero(child.Gravity) || UseWind) CalculateGravityPositionOffsetForSegment(child);

            if (Axis2D > 0) Axis2DLimit(child);
        }


        /// <summary>
        /// Post processing for segment if used for example deflection
        /// </summary>
        void TailCalculations_SegmentPostProcessing(TailSegment bone)
        {
            // Applying deflection
            if (Deflection > Mathf.Epsilon) Deflection_SegmentOffsetSimple(bone, ref bone.ProceduralPosition);

            // Soon there may be more post processes
        }


        /// <summary>
        /// Second iteration for segments rotation calculations
        /// Rotating segments towards calculated procedural positions with blending if used
        /// </summary>
        void TailCalculations_SegmentRotation(TailSegment child, Vector3 localOffset)
        {

            // Calculating correct rotation towards calculated position in parent orientation
            _tc_lookRot = Quaternion.FromToRotation
                ( // Support negative scale with transform direction
                child.ParentBone.transform.TransformDirection(localOffset), // Child local pos offset - direction from parent true position towards default bone postion from keyframe animation
                child.ProceduralPositionWeightBlended - child.ParentBone.ProceduralPositionWeightBlended // Direction from parent calculated position towards child target calculated postion
                );

            // Rotating towards desired orientation
            _tc_targetParentRot = _tc_lookRot * child.ParentBone.transform.rotation;

            if ( AnimateRoll)
            {
                _tc_targetParentRot = Quaternion.Lerp(child.ParentBone.TrueTargetRotation, _tc_targetParentRot, deltaForLerps * Mathf.LerpUnclamped(10f, 60f, child.RotationSpeed));
            }

            // Notice than we setting parent rotation from child relation : parent rotation is dictating child position
            // Remember transform rotaiton before smoothing reference rotation !!!
            child.ParentBone.TrueTargetRotation = _tc_targetParentRot;

            // Remembering previous value before changing to new            
            child.ParentBone.PreviousPosReferenceRotation = child.ParentBone.PosRefRotation;

            // Setting positions reference rotation separately
            if ( !AnimateRoll) if (child.RotationSpeed < 1f) _tc_targetParentRot = TailCalculations_SmoothRotation(child.ParentBone.PosRefRotation, _tc_targetParentRot, child);

            child.ParentBone.PosRefRotation = _tc_targetParentRot;
        }



        /// <summary>
        /// Applying tail motion to transforms
        /// Accesing parent of bone to change it's rotation
        /// </summary>
        void TailCalculations_ApplySegmentMotion(TailSegment child)
        {
            child.ParentBone.transform.rotation = child.ParentBone.TrueTargetRotation;
            child.transform.position = child.ProceduralPositionWeightBlended;

            child.RefreshFinalPos(child.ProceduralPositionWeightBlended);
            child.ParentBone.RefreshFinalRot(child.ParentBone.TrueTargetRotation);
        }

    }
}