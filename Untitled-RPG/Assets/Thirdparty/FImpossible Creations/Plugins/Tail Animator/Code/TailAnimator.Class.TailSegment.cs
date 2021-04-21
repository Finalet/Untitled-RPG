using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        /// <summary>
        /// FM: Helper class to animate tail bones freely
        /// </summary>
        [System.Serializable]
        public class TailSegment
        {
            public TailSegment ParentBone { get; private set; }
            public TailSegment ChildBone { get; private set; }

            /// <summary> All references must have transform reference except GhostChild bone, GhostParent can have same transform as first bone in chain </summary>
            public Transform transform { get; private set; }

            /// <summary> Index of bone in Tail Animator list </summary>
            public int Index { get; private set; }

            /// <summary> For quicker value getting from curves </summary>
            public float IndexOverlLength { get; private set; }

            /// <summary> Procedural position for tail motion without weight blending etc. </summary>
            public Vector3 ProceduralPosition = Vector3.zero;
            /// <summary> Final procedural position for tail transforms so weight blended </summary>
            public Vector3 ProceduralPositionWeightBlended = Vector3.zero;

            /// <summary> Final rotation for tail transform so rotated towards blended position </summary>
            public Quaternion TrueTargetRotation = Quaternion.identity;
            /// <summary> Reference rotation for position offset in parent orientation used in rotation smoothing </summary>
            public Quaternion PosRefRotation = Quaternion.identity;
            /// <summary> Memory for slithery motion </summary>
            public Quaternion PreviousPosReferenceRotation = Quaternion.identity;

            /// <summary> Memory for velocity (PreviousProceduralPosition) </summary>
            public Vector3 PreviousPosition;

            /// <summary> Blend with tail animator motion value used in partial blending </summary>
            public float BlendValue = 1f;

            /// <summary> Length of the bone in initial world space - distance to next bone transform </summary>
            public float BoneLength { get; private set; }
            /// <summary> Length of bone basing on intial positions / keyframe positions scaled with transform scale and length multiplier </summary>
            public Vector3 BoneDimensionsScaled;
            public float BoneLengthScaled;

            public Vector3 InitialLocalPosition = Vector3.zero;
            public Vector3 LocalOffset = Vector3.zero;
            public Quaternion InitialLocalRotation = Quaternion.identity;

            // Collision related variables
            public float ColliderRadius = 1f;


            public bool CollisionContactFlag = false;
            public float CollisionContactRelevancy = -1f;
            public Collision collisionContacts = null;

            // Animation styles helper variables
            public Vector3 VelocityHelper = Vector3.zero;
            public Quaternion QVelocityHelper = Quaternion.identity;

            /// <summary> Helper variable for 'Sustain' parameter </summary>
            public Vector3 PreviousPush = Vector3.zero;

            // Shaping
            public Quaternion Curving = Quaternion.identity;
            public Vector3 Gravity = Vector3.zero;
            public Vector3 GravityLookOffset = Vector3.zero;
            public float LengthMultiplier = 1f;

            // Expert
            public float PositionSpeed = 1f;
            public float RotationSpeed = 1f;
            public float Springiness = 0f;
            public float Slithery = 1f;
            public float Curling = 0.5f;
            public float Slippery = 1f;

            public TailCollisionHelper CollisionHelper { get; internal set; }


            #region Constructors

            public TailSegment() { Index = -1; Curving = Quaternion.identity; Gravity = Vector3.zero; LengthMultiplier = 1f; Deflection = Vector3.zero; DeflectionFactor = 0f; DeflectionRelevancy = -1; deflectionSmoothVelo = 0f; }
            public TailSegment(Transform transform) : this()
            {
                if (transform == null) return;

                this.transform = transform;

                // Init position setup
                ProceduralPosition = transform.position;
                PreviousPosition = transform.position;
                PosRefRotation = transform.rotation;
                PreviousPosReferenceRotation = PosRefRotation;
                TrueTargetRotation = PosRefRotation;
                ReInitializeLocalPosRot(transform.localPosition, transform.localRotation);
                BoneLength = 0.1f;
                //ReInitializeLocalPosRot(transform.localPosition, transform.localRotation);
            }

            public TailSegment(TailSegment copyFrom) : this(copyFrom.transform)
            {
                this.transform = copyFrom.transform;

                // Init position setup
                Index = copyFrom.Index;
                IndexOverlLength = copyFrom.IndexOverlLength;

                ProceduralPosition = copyFrom.ProceduralPosition;
                PreviousPosition = copyFrom.PreviousPosition;
                ProceduralPositionWeightBlended = copyFrom.ProceduralPosition;
                PosRefRotation = copyFrom.PosRefRotation;
                PreviousPosReferenceRotation = PosRefRotation;
                TrueTargetRotation = copyFrom.TrueTargetRotation;

                ReInitializeLocalPosRot(copyFrom.InitialLocalPosition, copyFrom.InitialLocalRotation);
            }


            public void ReInitializeLocalPosRot(Vector3 initLocalPos, Quaternion initLocalRot)
            {
                InitialLocalPosition = initLocalPos;
                InitialLocalRotation = initLocalRot;
            }


            #endregion


            public void SetIndex(int i, int tailSegments)
            {
                Index = i;
                if (i < 0) IndexOverlLength = 0f;
                else IndexOverlLength = (float)i / (float)tailSegments;
            }

            public void SetParentRef(TailSegment parent)
            { ParentBone = parent; BoneLength = (ProceduralPosition - ParentBone.ProceduralPosition).magnitude; }

            public void SetChildRef(TailSegment child)
            { ChildBone = child; }

            public float GetRadiusScaled()
            { return ColliderRadius * transform.lossyScale.x; }

            //public static Vector3 CalculateLocalLook(Transform parent, Transform child)
            //{ return -(parent.InverseTransformPoint(child.position) - parent.InverseTransformPoint(parent.position)).normalized; }


            /// <summary>
            /// Blend toward target position
            /// </summary>
            internal Vector3 BlendMotionWeight(Vector3 newPosition)
            {
                return Vector3.LerpUnclamped
                    (
                    // Blending from default pose to new position
                    ParentBone.ProceduralPosition + FEngineering.TransformVector(ParentBone.LastKeyframeLocalRotation, ParentBone.transform.lossyScale, LastKeyframeLocalPosition),
                    newPosition,
                    BlendValue
                    );
            }

            internal void Validate()
            {
                if (BoneLength == 0f) BoneLength = 0.001f;
            }


            #region Zero keyframe detection


            /// <summary> Initial local rotation or keyframe local rotation </summary>
            public Quaternion LastKeyframeLocalRotation { get { if (transform) return transform.localRotation; else return InitialLocalRotation; } }
            /// <summary> Initial local position or keyframe local position </summary>
            public Vector3 LastKeyframeLocalPosition { get { if (transform) return transform.localPosition; else return InitialLocalPosition; } }

            public Vector3 LastFinalPosition { get; private set; }
            public Quaternion LastFinalRotation { get; private set; }


            /// <summary>
            /// Offsset to default segment position from parent relation
            /// </summary>
            internal Vector3 ParentToFrontOffset()
            {
                return FEngineering.TransformVector
                    (
                    ParentBone.LastKeyframeLocalRotation,
                    ParentBone.transform.lossyScale,
                    LastKeyframeLocalPosition
                    );
            }


            public void RefreshFinalPos(Vector3 pos)
            { LastFinalPosition = pos; }

            public void RefreshFinalRot(Quaternion rot)
            { LastFinalRotation = rot; }

            #endregion


            #region Deflection feature support

            /// <summary> Dot product of deflection angle for segment </summary>
            public float DeflectionFactor { get; private set; }
            /// <summary> Deflection direction </summary>
            public Vector3 Deflection { get; private set; }
            public float DeflectionSmooth { get; private set; }
            private float deflectionSmoothVelo; // Helper variable for smooth damp

            /// <summary> Deflection world position </summary>
            public Vector3 DeflectionWorldPosition { get; private set; }
            /// <summary> Relevancy of deflection in list used for optimization </summary>
            public int DeflectionRelevancy { get; private set; }


            /// <summary>
            /// Checking relation between reference position to define deflection
            /// </summary>
            public bool CheckDeflectionState(float zeroWhenLower, float smoothTime, float delta)
            {
                Vector3 deflection = LastKeyframeLocalPosition - ParentBone.transform.InverseTransformVector(ProceduralPosition - ParentBone.ProceduralPosition);

                #region Debugging
                //deflection = LastKeyframeLocalPosition - FEngineering.TransformVector(ParentBone.ParentBone.ProceduralRotation, ParentBone.transform.lossyScale, ProceduralPosition - ParentBone.ProceduralPosition);
                //Debug.DrawRay(ParentBone.transform.position + Vector3.up * 1.1f, ParentBone.transform.TransformVector(ProceduralPosition - ParentBone.ProceduralPosition), Color.red);
                //Debug.DrawRay(ParentBone.ProceduralPosition + Vector3.up * 1.1f, FEngineering.TransformVector(ParentBone.ParentBone.ProceduralRotation, ParentBone.transform.lossyScale, ProceduralPosition - ParentBone.ProceduralPosition), new Color(1f, 0.5f, 0f, 1f) );
                #endregion

                DeflectionFactor = Vector3.Dot(LastKeyframeLocalPosition.normalized, deflection.normalized); // Calculating factor in local space

#if UNITY_EDITOR // Debugging
                deflection = ChildBone.ParentBone.transform.TransformVector(deflection); // Transforming into world space offset for segments calculations
#endif

                // Reseting when too small angle
                if (DeflectionFactor < zeroWhenLower)
                {
                    if (smoothTime <= Mathf.Epsilon)
                    {
#if UNITY_EDITOR
                        Deflection = Vector3.zero;
#endif
                        DeflectionSmooth = 0f;
                    }
                    else
                    {
#if UNITY_EDITOR
                        Deflection = Vector3.zero;
#endif

                        DeflectionSmooth = Mathf.SmoothDamp(DeflectionSmooth, -Mathf.Epsilon, ref deflectionSmoothVelo, smoothTime / 1.5f, Mathf.Infinity, delta);
                    }
                }
                else // Enabling or translating with smooth time
                {
                    if (smoothTime <= Mathf.Epsilon)
                    {
#if UNITY_EDITOR
                        Deflection = deflection;
#endif
                        DeflectionSmooth = 1f;
                    }
                    else
                    {
#if UNITY_EDITOR
                        Deflection = deflection;
#endif

                        DeflectionSmooth = Mathf.SmoothDamp(DeflectionSmooth, 1f, ref deflectionSmoothVelo, smoothTime, Mathf.Infinity, delta);
                    }
                }

                if (DeflectionSmooth <= Mathf.Epsilon) return true;
                else
                {
                    if (ChildBone.ChildBone != null)
                        DeflectionWorldPosition = ChildBone.ChildBone.ProceduralPosition;
                    else
                        DeflectionWorldPosition = ChildBone.ProceduralPosition;

                    return false;
                }
            }



            /// <summary>
            /// Signing bone as deflection point relevant
            /// </summary>
            /// <returns> True when deflection just detrected </returns>
            public bool DeflectionRelevant()
            {
                if (DeflectionRelevancy == -1)
                {
                    DeflectionRelevancy = 3;
                    return true;
                }

                DeflectionRelevancy = 3;
                return false;
            }


            /// <returns> True when relevancy is high   null when it's remove point relevancy   false when it's unrelevant </returns>
            public bool? DeflectionRestoreState()
            {
                if (DeflectionRelevancy > 0)
                {
                    DeflectionRelevancy--;

                    if (DeflectionRelevancy == 0) // When relevancy hit 0 then remove callback and set to -1
                    {
                        DeflectionRelevancy = -1;
                        return null;
                    }
                    else return true; // True when relevancy > 0
                }
                else
                    return false; // Unrelevant deflection state
            }



            #endregion


            #region Post Processing and others

            /// <summary>
            /// Copying animation parameters from other segment
            /// </summary>
            internal void ParamsFrom(TailSegment other)
            {
                BlendValue = other.BlendValue;
                ColliderRadius = other.ColliderRadius;
                Gravity = other.Gravity;
                LengthMultiplier = other.LengthMultiplier;
                BoneLength = other.BoneLength;
                BoneLengthScaled = other.BoneLengthScaled;
                BoneDimensionsScaled = other.BoneDimensionsScaled;
                collisionContacts = other.collisionContacts;
                CollisionHelper = other.CollisionHelper;

                PositionSpeed = other.PositionSpeed;
                RotationSpeed = other.RotationSpeed;
                Springiness = other.Springiness;
                Slithery = other.Slithery;
                Curling = other.Curling;
                Slippery = other.Slippery;
            }

            /// <summary>
            /// Copying all available parameters from other segment
            /// </summary>
            internal void ParamsFromAll(TailSegment other)
            {
                ParamsFrom(other);
                InitialLocalPosition = other.InitialLocalPosition;
                InitialLocalRotation = other.InitialLocalRotation;
                LastFinalPosition = other.LastFinalPosition;
                LastFinalRotation = other.LastFinalRotation;
                ProceduralPosition = other.ProceduralPosition;
                ProceduralPositionWeightBlended = other.ProceduralPositionWeightBlended;
                TrueTargetRotation = other.TrueTargetRotation;
                PosRefRotation = other.PosRefRotation;
                PreviousPosReferenceRotation = other.PreviousPosReferenceRotation;
                PreviousPosition = other.PreviousPosition;
                BoneLength = other.BoneLength;
                BoneDimensionsScaled = other.BoneDimensionsScaled;
                BoneLengthScaled = other.BoneLengthScaled;
                LocalOffset = other.LocalOffset;
                ColliderRadius = other.ColliderRadius;
                VelocityHelper = other.VelocityHelper;
                QVelocityHelper = other.QVelocityHelper;
                PreviousPush = other.PreviousPush;
            }

            internal void User_ReassignTransform(Transform t)
            {
                transform = t;
            }

            #endregion


            #region Resetting

            public void Reset()
            {
                PreviousPush = Vector3.zero;
                VelocityHelper = Vector3.zero;
                QVelocityHelper = Quaternion.identity;

                if (transform)
                {
                    ProceduralPosition = transform.position;
                    PosRefRotation = transform.rotation;
                    PreviousPosReferenceRotation = transform.rotation;
                }
                else
                {
                    if (ParentBone.transform)
                        ProceduralPosition = ParentBone.transform.position + ParentToFrontOffset();
                }

                PreviousPosition = ProceduralPosition;
                ProceduralPositionWeightBlended = ProceduralPosition;
            }

            #endregion
        }

    }
}