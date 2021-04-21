#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        [FPD_Header("Debug Switch")]
        [SerializeField]
        private bool fDebug = false;

        public Transform _gizmosEditorStartPreview;
        public Transform _gizmospesp;
        public Transform _gizmosEditorEndPreview;
        public Transform _gizmospeep;

        public string _editor_Title = "Tail Animator 2";
        public bool _editor_IsInspectorViewingColliders = true;
        public bool _editor_IsInspectorViewingIncludedColliders = false;
        public int _editor_animatorViewedCounter = -1;

        void OnDrawGizmosSelected()
        {
            RefreshTransformsList();
            if (_TransformsGhostChain.Count == 0) return;
            if (!DrawGizmos) return;

            if (UseIK) Gizmos_DrawIK();

            if (UseMaxDistance) Gizmos_DrawMaxDistance();

            if (Application.isPlaying)
            {
                if (!initialized) return;

                if (!IncludeParent) Editor_TailCalculations_RefreshArtificialParentBone();

                if (_Editor_Category == ETailCategory.Shaping || fDebug)
                {
                    Handles.color = new Color(0.2f, .2f, 0.85f, 0.5f);
                    Handles.SphereHandleCap(0, TailSegments[0].transform.position, Quaternion.identity, HandleUtility.GetHandleSize(TailSegments[0].transform.position) * 0.05f, EventType.Repaint);

                    Handles.color = new Color(0.5f, 1f, 0.35f, 0.3f);

                    for (int i = 1; i < TailSegments.Count; i++)
                    {
                        Handles.SphereHandleCap(0, TailSegments[i].ProceduralPosition, Quaternion.identity, HandleUtility.GetHandleSize(TailSegments[i].ProceduralPosition) * 0.09f, EventType.Repaint);
                        Handles.DrawDottedLine(TailSegments[i].ProceduralPosition, TailSegments[i].ParentBone.ProceduralPosition, 2f);
                    }

                    if (IncludeParent) Handles.color = new Color(1f, .2f, 0.6f, 1f);
                    Handles.DrawDottedLine(TailSegments[0].ProceduralPosition, TailSegments[0].ParentBone.ProceduralPosition, 2f);

                    Handles.color = new Color(1f, .2f, 0.6f, 1f);
                    Handles.DrawDottedLine(GhostChild.ProceduralPosition, GhostChild.ParentBone.ProceduralPosition, 2f);

                    Handles.SphereHandleCap(0, GhostParent.ProceduralPosition, Quaternion.identity, HandleUtility.GetHandleSize(GhostParent.ProceduralPosition) * 0.09f, EventType.Repaint);
                    Handles.SphereHandleCap(0, GhostChild.ProceduralPosition, Quaternion.identity, HandleUtility.GetHandleSize(GhostParent.ProceduralPosition) * 0.09f, EventType.Repaint);
                }


                if (fDebug)
                {
                    Vector3 uoff;

                    if (PostProcessingNeeded() && _pp_initialized)
                    {
                        uoff = Vector3.up * _TC_TailLength * 0.065f;
                        Gizmos.color = new Color(0.8f, 0.2f, 0.5f, 0.4f); // Post processing reference line (transparent dark pinnk)
                        for (int i = 0; i < _pp_reference.Count; i++)
                        {
                            Gizmos.DrawSphere(_pp_reference[i].ProceduralPosition, .3f);

                            Gizmos.DrawLine(_pp_reference[i].ParentBone.ProceduralPosition + uoff, _pp_reference[i].ProceduralPosition + uoff);
                        }

                        Handles.color = new Color(0.8f, 0.2f, 0.5f, 0.4f); // Deflection source segment sphere 
                        for (int i = 0; i < _defl_source.Count; i++)
                            Handles.SphereHandleCap(0, _defl_source[i].ParentBone.ProceduralPosition + uoff, Quaternion.identity, HandleUtility.GetHandleSize(_defl_source[i].ProceduralPosition) * 0.09f, EventType.Repaint);

                        Handles.color = new Color(0.55f, .2f, 0.8f, 0.6f); // Deflection source segment sphere 
                        for (int i = 0; i < _defl_source.Count; i++)
                            Handles.SphereHandleCap(0, Vector3.LerpUnclamped(TailSegments[_defl_source[i].Index].ParentBone.ProceduralPosition, TailSegments[_defl_source[i].Index].ProceduralPosition, 0.5f) + uoff, Quaternion.identity, HandleUtility.GetHandleSize(_defl_source[i].ProceduralPosition) * 0.09f, EventType.Repaint);


                        #region Debug

                        //Handles.color = new Color(0.55f, .2f, 0.8f, 0.8f);
                        //for (int i = 0; i < _defl_source.Count; i++)
                        //    Handles.DrawDottedLine(_defl_source[i].ProceduralPosition + uoff, _defl_source[i].DeflectionWorldPosition + uoff, 2f);

                        //Handles.color = new Color(0.0f, 1f, 1f, 0.8f);
                        //for (int i = 0; i < _defl_source.Count; i++)
                        //{
                        //    int backRange = _tc_startI; if (i > 0) backRange = _defl_source[i - 1].Index;

                        //    for (int k = _defl_source[i].Index; k >= backRange; k--)
                        //        Handles.DrawDottedLine(TailBones[k].ProceduralPosition + uoff, _defl_source[i].DeflectionWorldPosition + uoff, 2f);
                        //}

                        #endregion

                        Gizmos.color = new Color(1f, 0.1f, 0.35f, 0.6f); // Deflection push focus (magenta) 
                        for (int i = 0; i < _defl_source.Count; i++)
                        {
                            Gizmos.DrawLine(_defl_source[i].DeflectionWorldPosition + uoff, _defl_source[i].ParentBone.ProceduralPosition + uoff);
                            //Handles.Label(_pp_reference[_defl_source[i].Index].ParentBone.ProceduralPosition + uoff + transform.right * _tc_tailLength * 0.032f + uoff, "b.par=[" + _defl_source[i].Index + "]");
                            Handles.DrawDottedLine(_pp_reference[_defl_source[i].Index].ParentBone.ProceduralPosition + uoff + transform.right * _TC_TailLength * 0.032f + uoff, TailSegments[_defl_source[i].Index].ProceduralPosition + uoff, 3f);
                        }


                        uoff = Vector3.up * _TC_TailLength * 0.044f;
                        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.5f); // True position line
                        for (int i = 0; i < _pp_reference.Count; i++)
                            Gizmos.DrawLine(TailSegments[i].ParentBone.ProceduralPosition + uoff, TailSegments[i].ProceduralPosition + uoff);

                        Gizmos.DrawLine(_pp_ref_lastChild.ParentBone.ProceduralPosition + uoff, _pp_ref_lastChild.ProceduralPosition + uoff);


                        // All segments debug draw
                        uoff = Vector3.up * _TC_TailLength * 0.065f;
                        Handles.color = new Color(1f, 1f, 1f, 0.4f);
                        for (int i = 0; i < TailSegments.Count; i++)
                        {
                            Handles.SphereHandleCap(0, TailSegments[i].ProceduralPosition, Quaternion.identity, HandleUtility.GetHandleSize(TailSegments[i].ProceduralPosition) * 0.09f, EventType.Repaint);
                            Handles.Label(TailSegments[i].ProceduralPosition - transform.right * _TC_TailLength * 0.02f + uoff, "[" + i + "]");
                        }
                    }
                }
            }

            if (_Editor_Category == ETailCategory.Setup)
            {
                if (_TransformsGhostChain != null)
                {
                    if (_TransformsGhostChain.Count > 1)
                    {

                        if (!IncludeParent) Handles.color = new Color(.9f, .4f, .7f, .9f); else Handles.color = new Color(0.5f, 1f, 0.35f, 0.8f);

                        if (_TransformsGhostChain.Count > 1)
                        {
                            FGUI_Handles.DrawBoneHandle(_TransformsGhostChain[0].position, _TransformsGhostChain[1].position, BaseTransform.forward, 1f);
                            Handles.SphereHandleCap(0, _TransformsGhostChain[0].position, Quaternion.identity, HandleUtility.GetHandleSize(_TransformsGhostChain[0].position) * 0.09f, EventType.Repaint);
                        }


                        for (int i = 1; i < _TransformsGhostChain.Count - 1; i++) // -1 because we painting bones from i to i+1
                        {
                            Handles.color = new Color(0.5f, 1f, 0.35f, 0.8f);
                            FGUI_Handles.DrawBoneHandle(_TransformsGhostChain[i].position, _TransformsGhostChain[i + 1].position, BaseTransform.forward, 1f);

                            Handles.color = new Color(0.5f, 1f, 0.35f, 0.3f);
                            Handles.SphereHandleCap(0, _TransformsGhostChain[i].position, Quaternion.identity, HandleUtility.GetHandleSize(_TransformsGhostChain[i].position) * 0.09f, EventType.Repaint);
                        }

                        if (_TransformsGhostChain.Count > 1)
                            if (IncludeParent)
                            {
                                if (_TransformsGhostChain[0].parent)
                                {
                                    Handles.color = new Color(1f, .2f, 0.6f, 0.3f);
                                    FGUI_Handles.DrawBoneHandle(_TransformsGhostChain[0].parent.position, _TransformsGhostChain[0].position, BaseTransform.forward, 1f);
                                }
                            }
                    }
                    else
                    {
                        if (_TransformsGhostChain.Count > 0)
                        {
                            if (_TransformsGhostChain[0].parent)
                            {
                                Transform t = _TransformsGhostChain[0]; Transform p = _TransformsGhostChain[0].parent;

                                Handles.color = new Color(0.8f, .8f, 0.2f, 0.8f);
                                FGUI_Handles.DrawBoneHandle(_TransformsGhostChain[0].parent.position, _TransformsGhostChain[0].position, BaseTransform.forward, 1f);
                                Handles.color = new Color(0.8f, .8f, 0.2f, 0.3f);
                                Handles.SphereHandleCap(0, _TransformsGhostChain[0].position, Quaternion.identity, HandleUtility.GetHandleSize(_TransformsGhostChain[0].position) * 0.135f, EventType.Repaint);
                                Handles.Label(t.position + Vector3.Cross(t.forward, p.forward).normalized * (t.position - p.position).magnitude / 2f, new GUIContent("[i]", "Tail chain with one bone setup - try using End Bone Offset in 'Setup' tab"));
                            }

                        }
                    }

                }
            }


            if (_TransformsGhostChain.Count < 2)
            {
                Handles.color = new Color(1f, 1f, 1f, 0.5f);
                Handles.SphereHandleCap(0, _TransformsGhostChain[0].position, Quaternion.identity, HandleUtility.GetHandleSize(_TransformsGhostChain[0].position) * 0.135f, EventType.Repaint);

                if (_TransformsGhostChain.Count > 0)
                {
                    if (!_TransformsGhostChain[0].parent)
                    {
                        Handles.color = new Color(1f, .2f, 0.2f, 0.8f);
                        Vector3 infoPos = _TransformsGhostChain[0].position + (_TransformsGhostChain[0].up + _TransformsGhostChain[0].right) * 0.2f;
                        Handles.DrawDottedLine(_TransformsGhostChain[0].position, infoPos, 2f);
                        Handles.DrawDottedLine(_TransformsGhostChain[0].position, transform.position, 2f);
                        Handles.Label(infoPos, new GUIContent(FGUI_Resources.Tex_Warning, "No parent in this bone, tail chain can't be created"));
                    }
                }
            }


            if (_Editor_Category == ETailCategory.Setup)
            {
                Handles.color = new Color(1f, .2f, 0.2f, 0.8f);
                Gizmos_DrawEndBoneJoint();
            }


            if (_editor_IsInspectorViewingColliders && _Editor_Category == ETailCategory.Features) Gizmos_DrawColliders();
        }


        void Gizmos_DrawEndBoneJoint()
        {
            // Drawing auto end joint offset
            if (FEngineering.VIsZero(EndBoneJointOffset))
            {
                Transform t = _TransformsGhostChain[_TransformsGhostChain.Count - 1];
                Transform p = _TransformsGhostChain[_TransformsGhostChain.Count - 1].parent;

                if (p) // Reference from parent
                {
                    Vector3 worldOffset = t.position - p.position;

                    Handles.color = new Color(0.3f, .3f, 1f, 0.8f);
                    FGUI_Handles.DrawBoneHandle(t.position, t.position + worldOffset, BaseTransform.forward, 1f);
                }
                else // Reference to child
                {
                    if (t.childCount > 0)
                    {
                        Transform ch = _TransformsGhostChain[0].GetChild(0);
                        Vector3 worldOffset = ch.position - t.position;
                        FGUI_Handles.DrawBoneHandle(t.position, t.position + worldOffset, BaseTransform.forward, 1f);
                    }
                }
            }
            // Drawing custom joint offset
            else
            {
                Transform t = _TransformsGhostChain[_TransformsGhostChain.Count - 1];
                Handles.color = new Color(0.3f, .3f, 1f, 0.8f);
                FGUI_Handles.DrawBoneHandle(t.position, t.position + t.TransformVector(EndBoneJointOffset), BaseTransform.forward, 1f);
            }
        }


        void Gizmos_DrawColliders()
        {
            if (UseCollision)
            {
                Color preCol = Gizmos.color;
                Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);

                for (int i = 1; i < _TransformsGhostChain.Count - 1; i++)
                {
                    if (_TransformsGhostChain[i] == null) continue;
                    Matrix4x4 curr = Matrix4x4.TRS(_TransformsGhostChain[i].position, _TransformsGhostChain[i].rotation, _TransformsGhostChain[i].lossyScale);
                    float radius = 1f;
                    if (Application.isPlaying) radius = TailSegments[i].ColliderRadius; else radius = GetColliderSphereRadiusFor(_TransformsGhostChain, i);

                    if (CollidersType != 0)
                    {
                        if (CollisionSpace == ECollisionSpace.World_Slow)
                        {
                            float preRadius = 1f;
                            if (Application.isPlaying) preRadius = TailSegments[i - 1].ColliderRadius; else preRadius = GetColliderSphereRadiusFor(_TransformsGhostChain, i - 1);

                            Gizmos.color = Color.HSVToRGB((float)i / (float)_TransformsGhostChain.Count, 0.9f, 0.9f) * new Color(1f, 1f, 1f, 0.55f);

                            Matrix4x4 pre = Matrix4x4.TRS(_TransformsGhostChain[i - 1].position, _TransformsGhostChain[i - 1].rotation, _TransformsGhostChain[i - 1].lossyScale);

                            Gizmos.DrawLine(pre.MultiplyPoint(Vector3.up * preRadius), curr.MultiplyPoint(Vector3.up * radius));
                            Gizmos.DrawLine(pre.MultiplyPoint(-Vector3.up * preRadius), curr.MultiplyPoint(-Vector3.up * radius));
                            Gizmos.DrawLine(pre.MultiplyPoint(Vector3.right * preRadius), curr.MultiplyPoint(Vector3.right * radius));
                            Gizmos.DrawLine(pre.MultiplyPoint(Vector3.left * preRadius), curr.MultiplyPoint(Vector3.left * radius));

                            if (i % 2 == 0)
                            {
                                Gizmos.matrix = curr;
                                Gizmos.DrawWireSphere(Vector3.zero, radius);
                                Gizmos.matrix = Matrix4x4.identity;
                            }
                        }
                        else
                        {
                            Gizmos.matrix = curr;
                            Gizmos.DrawWireSphere(Vector3.zero, radius);
                            Gizmos.matrix = Matrix4x4.identity;
                        }
                    }
                    else
                    {
                        Gizmos.matrix = curr;
                        Gizmos.DrawWireSphere(Vector3.zero, radius);
                        Gizmos.matrix = Matrix4x4.identity;
                    }
                }


                if (CollisionSpace == ECollisionSpace.Selective_Fast)
                {
                    if (_editor_IsInspectorViewingIncludedColliders)
                    {
                        Handles.color = new Color(0.4f, 1f, 0.25f, 0.22f);
                        int c = Mathf.Min(IncludedColliders.Count, 10); // Drawing max 10 lines toward included colliders
                        for (int i = 0; i < c; i++)
                        {
                            if (IncludedColliders[i] != null)
                            {
                                Handles.DrawDottedLine(_TransformsGhostChain[0].position, IncludedColliders[i].transform.position, 2f);
                                Handles.SphereHandleCap(0, IncludedColliders[i].transform.position, Quaternion.identity, HandleUtility.GetHandleSize(IncludedColliders[i].transform.position) * 0.09f, EventType.Repaint);
                            }
                        }
                    }

                    if (!Application.isPlaying)
                        if (DynamicWorldCollidersInclusion)
                        {
                            float scaleRef = Vector3.Distance(_TransformsGhostChain[0].position, _TransformsGhostChain[_TransformsGhostChain.Count - 1].position);
                            Transform collSource = _TransformsGhostChain[_TransformsGhostChain.Count / 2];
                            Gizmos.DrawWireSphere(collSource.position, scaleRef * InclusionRadius);
                        }
                }


                Gizmos.color = preCol;
            }
        }


        void Gizmos_DrawIK()
        {
            if (_TransformsGhostChain.Count < 2) return;
            if (_Editor_Category != ETailCategory.Features) return;
            if (_Editor_FeaturesCategory != ETailFeaturesCategory.IK) return;

            Vector3 start = _TransformsGhostChain[0].position;
            Vector3 end = _TransformsGhostChain[_TransformsGhostChain.Count - 1].position;
            Vector3 targetIK;

            if (IKTarget)
            {
                Handles.color = new Color(0f, 0.7f, 0.3f, 0.7f);
                targetIK = IKTarget.position;
            }
            else
            {
                Handles.color = new Color(0.7f, 0.0f, 0.3f, 0.7f);
                targetIK = _TransformsGhostChain[_TransformsGhostChain.Count - 1].position;
            }

            float d = (start - end).magnitude * 0.06f;

            Handles.DrawDottedLine(start, targetIK, 3f);
            Handles.DrawDottedLine(end, targetIK, 3f);

            Handles.DrawLine(targetIK - BaseTransform.forward * d, targetIK + BaseTransform.forward * d);
            Handles.DrawLine(targetIK - BaseTransform.right * d, targetIK + BaseTransform.right * d);
            Handles.DrawLine(targetIK - BaseTransform.up * d, targetIK + BaseTransform.up * d);
            Handles.SphereHandleCap(0, targetIK, Quaternion.identity, d * 0.25f, EventType.Repaint);

        }


        void Gizmos_DrawMaxDistance()
        {
            if (MaximumDistance <= 0f) return;

            float a = 0.525f;
            Vector3 startPos = BaseTransform.position + BaseTransform.TransformVector(DistanceMeasurePoint);

            if (DistanceWithoutY)
            {
                if (maxDistanceExceed)
                    Handles.color = new Color(1f, .1f, .1f, a);
                else Handles.color = new Color(0.02f, .65f, 0.2f, a);

                Handles.DrawWireDisc(startPos, Vector3.up, MaximumDistance);

                if (DistanceMeasurePoint != Vector3.zero)
                {
                    Gizmos.color = new Color(0.02f, .65f, 0.2f, a);
                    Gizmos.DrawLine(startPos - Vector3.right * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor), startPos + Vector3.right * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor));
                    Gizmos.DrawLine(startPos - Vector3.forward * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor), startPos + Vector3.forward * (MaximumDistance + MaximumDistance * MaxOutDistanceFactor));
                }

                if (MaxOutDistanceFactor > 0f)
                {
                    Handles.color = new Color(.835f, .135f, .08f, a);
                    Handles.DrawWireDisc(startPos, Vector3.up, MaximumDistance + MaximumDistance * MaxOutDistanceFactor);
                }
            }
            else
            {
                if (maxDistanceExceed)
                    Gizmos.color = new Color(1f, .1f, .1f, a);
                else
                    Gizmos.color = new Color(0.02f, .65f, 0.2f, a);

                Gizmos.DrawWireSphere(startPos, MaximumDistance);

                if (MaxOutDistanceFactor > 0f)
                {
                    Gizmos.color = new Color(.835f, .135f, .08f, a);
                    Gizmos.DrawWireSphere(startPos, MaximumDistance + MaximumDistance * MaxOutDistanceFactor);
                }
            }
        }
    }
}

#endif