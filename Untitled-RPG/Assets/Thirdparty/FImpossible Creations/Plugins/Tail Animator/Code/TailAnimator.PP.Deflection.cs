using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    /// <summary>
    /// POST POSITION PROCESSING - Deflection
    /// </summary>
    public partial class TailAnimator2
    {
        [Tooltip("Making tail segment deflection influence back segments")]
        [Range(0f, 1f)]
        public float Deflection = 0.0f;
        [FPD_Suffix(1f, 89f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float DeflectionStartAngle = 10f;
        [Range(0f, 1f)]
        public float DeflectionSmooth = 0f;
        [FPD_FixedCurveWindow(0, 0f, 1f, 1f, .65f, 0.4f, 1f, 0.9f)]
        public AnimationCurve DeflectionFalloff = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("Deflection can be triggered every time tail is waving but you not always would want this feature be enabled (different behaviour of tail motion)")]
        public bool DeflectOnlyCollisions = true;

        // Deflection feature variables 
        private List<TailSegment> _defl_source;
        private float _defl_treshold = 0.01f;


        /// <summary>
        /// Searching for deflection points on stored segments position 
        /// before post processing with deflection to avoid jitter
        /// </summary>
        void Deflection_BeginUpdate()
        {
            // Defining constants for segments
            _defl_treshold = DeflectionStartAngle / 90f;
            float smoothTime = DeflectionSmooth / 9f;

            //for (int i = TailBones.Count-1; i >=_tc_startII; --i)
            for (int i = _tc_startII; i < TailSegments.Count; i++)
            {
                TailSegment ppChild = _pp_reference[i];

                // Checking deflection state to detect bend amount
                bool cleared = ppChild.CheckDeflectionState(_defl_treshold, smoothTime, rateDelta);

                // Detecting if deflection occured and adding as deflection source point
                if (!cleared)
                {
                    bool dependenciesMeet = true;

                    if (DeflectOnlyCollisions)
                        if (ppChild.CollisionContactRelevancy <= 0f)
                            dependenciesMeet = false; // No collision - no deflection

                    // Adding to deflection points list
                    if (dependenciesMeet)
                    {
                        Deflection_AddDeflectionSource(ppChild);
                    }
                    else
                        Deflection_RemoveDeflectionSource(ppChild);
                }
                else
                {
                    Deflection_RemoveDeflectionSource(ppChild);
                }
            }
        }


        /// <summary>
        /// Checking conditions to remove deflection source point
        /// </summary>
        void Deflection_RemoveDeflectionSource(TailSegment child)
        {
            if (child.DeflectionRestoreState() == null)
                if (_defl_source.Contains(child)) _defl_source.Remove(child);
        }


        /// <summary>
        /// Checking conditions to add deflection source point and sorting list by index
        /// </summary>
        void Deflection_AddDeflectionSource(TailSegment child)
        {
            if (child.DeflectionRelevant())
                if (!_defl_source.Contains(child)) _defl_source.Add(child);
        }

        /// <summary>
        /// Changing position of segment for deflection pose if deflection points are detected
        /// </summary>
        void Deflection_SegmentOffsetSimple(TailSegment child, ref Vector3 position)
        {
            if (child.Index == _tc_startI) return; // We not affecting first bone

            // We using greatest deflection so we must remember last one used in loop
            float lastDeflectionPower = 0f;

            // Defining influence of further children deflection on this segment
            // Going through all detected deflection points in this loop
            for (int i = 0; i < _defl_source.Count; i++)
            {
                // When there is one deflection and another one in next index then deflection is cancelling
                if (child.Index > _defl_source[i].Index) continue; // When segment is further on tail than deflection point then don't do anything here
                if (child.Index == _defl_source[i].Index) continue; // Not deflecting deflection source point

                // If we already used deflection with greater power
                if (_defl_source[i].DeflectionFactor < lastDeflectionPower) continue;
                lastDeflectionPower = _defl_source[i].DeflectionFactor;

                // Using curve over segments from zero or previous deflection source towards current one
                float preI = 0; if (i > 0) preI = _defl_source[i].Index; // Index for falloff curve
                float timeOnCurve = Mathf.InverseLerp(preI, _defl_source[i].Index, child.Index); // Falloff from previous deflection point to next one over curve

                // Direction from procedural position towards deflective position
                Vector3 towardDeflection = _defl_source[i].DeflectionWorldPosition - child.ParentBone.ProceduralPosition;

                // Calculating position of segment directed towards deflection compensation
                Vector3 deflectedSegmentPos = child.ParentBone.ProceduralPosition;
                deflectedSegmentPos += towardDeflection.normalized * child.BoneLengthScaled; // Scale support

                // Applying certain amount of deflection on segment position
                child.ProceduralPosition = Vector3.LerpUnclamped(child.ProceduralPosition, deflectedSegmentPos, Deflection * DeflectionFalloff.Evaluate(timeOnCurve) * _defl_source[i].DeflectionSmooth);
            }
        }


    }
}