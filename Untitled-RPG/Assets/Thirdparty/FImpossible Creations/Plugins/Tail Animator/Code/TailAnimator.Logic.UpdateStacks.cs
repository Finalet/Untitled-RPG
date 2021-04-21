namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {

        void SimulateTailMotionFrame(bool pp)
        {
            #region Prepare base positions calculation for tail segments to use in coords calculations and as reference

            TailSegments_UpdateRootFeatures();

            TailSegments_UpdateCoordsForRootBone(_tc_rootBone);

            if (pp) PostProcessing_ReferenceUpdate();


            if (_tc_startI > -1)
            {
                TailSegment child = TailSegments[_tc_startI]; // Used in while() loops below
                                                              // Going in while is 2x faster than for(i;i;i) loop
                while (child != GhostChild)
                {
                    // Preparing parameter values adapted to stiff and slithery character and blended
                    TailSegment_PrepareMotionParameters(child);

                    // Velocity changes detection
                    TailSegment_PrepareVelocity(child);

                    child = child.ChildBone;
                }
            }

            // Udpate for artificial end bone
            TailSegment_PrepareMotionParameters(GhostChild);
            TailSegment_PrepareVelocity(GhostChild);

            #endregion

            #region Processing segments, calculating full target coords and apply to transforms

            if (_tc_startII > -1)
            {
                // Ignoring root related calculations
                TailSegment child = TailSegments[_tc_startII];

                while (child != GhostChild)
                {
                    TailSegment_BaseSwingProcessing(child);

                    // Pre processing with limiting, gravity etc.
                    TailCalculations_SegmentPreProcessingStack(child);

                    if (pp) TailCalculations_SegmentPostProcessing(child);

                    // Blending animation weight
                    TailSegment_PreRotationPositionBlend(child);

                    child = child.ChildBone;
                }
            }

            // Applying processing for artificial child bone without transform
            TailCalculations_UpdateArtificialChildBone(GhostChild);

            #endregion

        }

        void UpdateTailAlgorithm()
        {
            TailCalculations_Begin(); // Root definition

            if (framesToSimulate != 0) // If framerate not defined then framesToSimulate is always == 1
            {

                if (UseCollision) BeginCollisionsUpdate(); // Updating colliders to collide with

                // If post processing is needed we computing reference coordinates
                bool postProcesses = PostProcessingNeeded();

                MotionInfluenceLimiting();

                for (int i = 0; i < framesToSimulate; i++) // Simulating update frames
                    SimulateTailMotionFrame(postProcesses);

                // Updating root bone position
                TailSegments[_tc_startI].transform.position = TailSegments[_tc_startI].ProceduralPositionWeightBlended;
                TailSegments[_tc_startI].RefreshFinalPos(TailSegments[_tc_startI].ProceduralPositionWeightBlended);
                //TailSegments[_tc_startI].RefreshFinalLocalPos(TailSegments[_tc_startI].transform.localPosition);

                // Applying calculated coords to transforms
                if (_tc_startII > -1)
                {
                    TailSegment child = TailSegments[_tc_startII]; // Used in while() loops below
                    while (child != GhostChild)
                    {
                        // Calculate rotation
                        TailCalculations_SegmentRotation(child, child.LastKeyframeLocalPosition);

                        // Apply coords to segments
                        TailCalculations_ApplySegmentMotion(child);
                        child = child.ChildBone;
                    }
                }

                // If ghost child has transform let's apply motion too (change rotation of last bone)
                TailCalculations_SegmentRotation(GhostChild, GhostChild.LastKeyframeLocalPosition);
                GhostChild.ParentBone.transform.rotation = GhostChild.ParentBone.TrueTargetRotation;
                GhostChild.ParentBone.RefreshFinalRot(GhostChild.ParentBone.TrueTargetRotation);
                
                if (GhostChild.transform)
                {
                    GhostChild.RefreshFinalPos(GhostChild.transform.position);
                    GhostChild.RefreshFinalRot(GhostChild.transform.rotation);
                }
            }
            else // Skipping tail motion simulation and just applying coords computed lately
            // Executed only when using target UpdateRate
            {

                if (InterpolateRate)
                {
                    secPeriodDelta = rateDelta / 24f;
                    deltaForLerps = secPeriodDelta; // Unify delta value not amplified -> 1f / rate

                    SimulateTailMotionFrame(PostProcessingNeeded());

                    if (_tc_startII > -1)
                    {
                        TailSegment child = TailSegments[_tc_startII];
                        while (child != GhostChild)
                        {
                            TailCalculations_SegmentRotation(child, child.LastKeyframeLocalPosition);
                            TailCalculations_ApplySegmentMotion(child);
                            child = child.ChildBone;
                        }
                    }

                    TailCalculations_SegmentRotation(GhostChild, GhostChild.LastKeyframeLocalPosition);
                    GhostChild.ParentBone.transform.rotation = GhostChild.ParentBone.TrueTargetRotation;
                    GhostChild.ParentBone.RefreshFinalRot(GhostChild.ParentBone.TrueTargetRotation);
                }
                else
                {
                    if (_tc_startI > -1)
                    {
                        TailSegment segment = TailSegments[_tc_startI];

                        while (segment != null)
                        {
                            if (segment.transform)
                            {
                                segment.transform.position = segment.LastFinalPosition;
                                segment.transform.rotation = segment.LastFinalRotation;
                            }
                            else break;

                            segment = segment.ChildBone;
                        }
                    }
                    else
                    {

                    }
                }
            }

        }


    }
}