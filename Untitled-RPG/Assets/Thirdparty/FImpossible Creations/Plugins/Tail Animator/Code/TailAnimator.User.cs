using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    /// <summary>
    /// FC: In this parital class you will find methods useful for custom
    /// coding and dynamic tail hierarchy changes etc.
    /// </summary>
    public partial class TailAnimator2
    {

        /// <summary>
        /// Re-initialize tail with new transforms chain
        /// </summary>
        public void User_SetTailTransforms(List<Transform> list)
        {
            StartBone = list[0];
            EndBone = list[list.Count - 1];
            _TransformsGhostChain = list;

            StartAfterTPose = false;
            initialized = false;
            Init();
        }

        /// <summary>
        /// Putting additional tail transform in chain list (added to the end of tail)
        /// </summary>
        public TailSegment User_AddTailTransform(Transform transform)
        {
            TailSegment newSeg = new TailSegment(transform);
            TailSegment last = TailSegments[TailSegments.Count - 1];
            newSeg.ParamsFromAll(last);

            newSeg.RefreshFinalPos(newSeg.transform.position);
            newSeg.RefreshFinalRot(newSeg.transform.rotation);
            newSeg.ProceduralPosition = newSeg.transform.position;
            newSeg.PosRefRotation = newSeg.transform.rotation;

            _TransformsGhostChain.Add(transform);
            TailSegments.Add(newSeg);
            last.SetChildRef(newSeg);
            newSeg.SetParentRef(last);
            newSeg.SetChildRef(GhostChild);
            GhostChild.SetParentRef(newSeg);

            // Resetting indexes for curves
            for (int i = 0; i < TailSegments.Count; i++)
                TailSegments[i].SetIndex(i, TailSegments.Count);

            return newSeg;
        }


        /// <summary>
        /// Dynamically removing tail segments from chain
        /// </summary>
        /// <param name="fromLastTo"> Segment with this index will be removed too but used as ghosting child </param>
        public void User_CutEndSegmentsTo(int fromLastTo)
        {
            if (fromLastTo < TailSegments.Count)
            {
                GhostChild = TailSegments[fromLastTo];
                GhostChild.SetChildRef(null);

                for (int i = TailSegments.Count - 1; i >= fromLastTo; i--)
                {
                    TailSegments.RemoveAt(i);
                    _TransformsGhostChain.RemoveAt(i);
                }
            }
            else
            {
                Debug.Log("[Tail Animator Cutting] Wrong index, you want cut from end to " + fromLastTo + " segment but there are only " + TailSegments.Count + " segments!");
            }
        }


        /// <summary>
        /// Syncing tail with current transforms positions and rotations
        /// </summary>
        public void User_ReposeTail()
        {
            GhostParent.Reset();
            for (int i = 0; i < TailSegments.Count; i++)
                TailSegments[i].Reset();
            GhostChild.Reset();
        }


    }
}