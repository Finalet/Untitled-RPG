using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    // Positions Post Processing support calculations for effects like deflection
    // which needs baked position to correctly detect needed stuff for
    // every frame update
    public partial class TailAnimator2
    {
        /// <summary> Always return chain with coordinates without any POST processes as reference for POST processing algorithms. Separated list of tail bones operating on the same transforms but without post processed coords </summary>
        private List<TailSegment> _pp_reference;

        // Artificial bone points bakery
        private TailSegment _pp_ref_rootParent;
        private TailSegment _pp_ref_lastChild;

        private bool _pp_initialized = false;


        /// <summary>
        /// Return true if Tail Aniamtor is using feature which requires post processing support
        /// </summary>
        bool PostProcessingNeeded()
        {
            if (Deflection > Mathf.Epsilon) return true;
            else return false;
        }


        /// <summary>
        /// Post processing start frame calculations
        /// </summary>
        void PostProcessing_Begin()
        {
            TailSegments_UpdateCoordsForRootBone(_pp_reference[_tc_startI]);

            // Deflection support
            if (Deflection > Mathf.Epsilon) Deflection_BeginUpdate();
        }


        /// <summary>
        /// Computing reference coordinates for POST processing
        /// </summary>
        void PostProcessing_ReferenceUpdate()
        {
            TailSegment child = _pp_reference[_tc_startI];
            
            #region Prepare base positions calculation for tail segments to use in coords calculations and as reference


            while (child != _pp_ref_lastChild)
            {
                child.ParamsFrom(TailSegments[child.Index]); // Copying parameters settings from true bones
                TailSegment_PrepareVelocity(child);
                child = child.ChildBone;
            }

            // Udpate for artificial end bone
            TailSegment_PrepareMotionParameters(_pp_ref_lastChild);
            TailSegment_PrepareVelocity(_pp_ref_lastChild);

            #endregion


            #region Processing segments, calculating full target coords and apply to transforms

            child = _pp_reference[_tc_startII];
            while (child != _pp_ref_lastChild)
            {
                TailSegment_BaseSwingProcessing(child);
                TailCalculations_SegmentPreProcessingStack(child);
                TailSegment_PreRotationPositionBlend(child);
                child = child.ChildBone;
            }

            // Applying processing for artificial child bone without transform
            TailCalculations_UpdateArtificialChildBone(_pp_ref_lastChild);

            #endregion

            
            child = _pp_reference[_tc_startII];
            while (child != _pp_ref_lastChild)
            {
                // Calculate rotation
                TailCalculations_SegmentRotation(child, child.LastKeyframeLocalPosition);
                child = child.ChildBone;
            }

            // If ghost child has transform let's apply motion too (change rotation of last bone)
            TailCalculations_SegmentRotation(child, child.LastKeyframeLocalPosition);
            child.ParentBone.RefreshFinalRot(child.ParentBone.TrueTargetRotation);
        }

    }
}