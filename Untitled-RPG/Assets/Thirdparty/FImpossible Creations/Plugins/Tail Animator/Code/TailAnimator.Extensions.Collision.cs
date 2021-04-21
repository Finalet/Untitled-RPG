using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        public enum ECollisionSpace { World_Slow, Selective_Fast }
        public enum ECollisionMode { m_3DCollision, m_2DCollision }

        [Tooltip("Using some simple calculations to make tail bend on colliders")]
        public bool UseCollision = false;

        [Tooltip("How collision should be detected, world gives you collision on all world colliders but with more use of cpu (using unity's rigidbodies), 'Selective' gives you possibility to detect collision on selected colliders without using Rigidbodies, it also gives smoother motion (deactivated colliders will still detect collision, unless its game object is disabled)")]
        public ECollisionSpace CollisionSpace = ECollisionSpace.Selective_Fast;
        public ECollisionMode CollisionMode = ECollisionMode.m_3DCollision;

        #region Selective Variables


        [Tooltip("If you want to stop checking collision if segment collides with one collider\n\nSegment collision with two or more colliders in the same time with this option enabled can result in stuttery motion")]
        public bool CheapCollision = false;

        [Tooltip("Using trigger collider to include encountered colliders into collide with list")]
        public bool DynamicWorldCollidersInclusion = false;

        [Tooltip("Radius of trigger collider for dynamic inclusion of colliders")]
        public float InclusionRadius = 1f;
        public bool IgnoreMeshColliders = true;

        public List<Collider> IncludedColliders;
        public List<Collider2D> IncludedColliders2D;
        /// <summary>Colliders always included with 'Dynamic World Colliders Inclusion' mode</summary>
        public List<Component> DynamicAlwaysInclude { get; private set; }
        protected List<FImp_ColliderData_Base> IncludedCollidersData;

        /// <summary> List of collider datas to be checked by every tail segment</summary>
        protected List<FImp_ColliderData_Base> CollidersDataToCheck;


        #endregion


        #region World Collision Variables


        [Tooltip("Capsules can give much more precise collision detection")]
        public int CollidersType = 0;
        public bool CollideWithOtherTails = false;
        [Tooltip("Collision with colliders even if they're disabled (but game object must be enabled)\nHelpful to setup character limbs collisions without need to create new Layer")]
        public bool CollideWithDisabledColliders = true;


        [Range(0f, 1f)]
        public float CollisionSlippery = 1f;
        [Tooltip("If tail colliding objects should fit to colliders (0) or be reflect from them (Reflecting Only with 'Slithery' parameter greater than ~0.2)")]
        [Range(0f, 1f)]
        public float ReflectCollision = 0f;

        public AnimationCurve CollidersScaleCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public float CollidersScaleMul = 6.5f;
        [Range(0f, 1f)]
        public float CollisionsAutoCurve = 0.5f;

        public List<Collider> IgnoredColliders;
        public List<Collider2D> IgnoredColliders2D;
        public bool CollidersSameLayer = true;
        [Tooltip("If you add rigidbodies to each tail segment's collider, collision will work on everything but it will be less optimal, you don't have to add here rigidbodies but then you must have not kinematic rigidbodies on objects segments can collide")]
        public bool CollidersAddRigidbody = true;
        public float RigidbodyMass = 1f;

        public LayerMask CollidersLayer = 0;


        #endregion

        void RefreshSegmentsColliders()
        {
            if (CollisionSpace == ECollisionSpace.Selective_Fast)
            {
                if (TailSegments != null)
                    if (TailSegments.Count > 1)
                        for (int i = 0; i < TailSegments.Count; i++)
                            TailSegments[i].ColliderRadius = GetColliderSphereRadiusFor(i);
            }
        }


        void BeginCollisionsUpdate()
        {
            if (CollisionSpace == ECollisionSpace.Selective_Fast)
            {
                RefreshIncludedCollidersDataList();

                // Letting every tail segment check only enabled colliders by game object
                CollidersDataToCheck.Clear();

                for (int i = 0; i < IncludedCollidersData.Count; i++)
                {
                    if (IncludedCollidersData[i].Transform == null) { forceRefreshCollidersData = true; break; }
                    
                    if (IncludedCollidersData[i].Transform.gameObject.activeInHierarchy)
                    {

                        // Collisions with all even disabled colliders
                        if (CollideWithDisabledColliders)
                        {
                            IncludedCollidersData[i].RefreshColliderData();
                            CollidersDataToCheck.Add(IncludedCollidersData[i]);
                        }
                        else
                        {
                            if (CollisionMode == ECollisionMode.m_3DCollision)
                            {
                                // If we want to collide even with disabled colliders
                                if (IncludedCollidersData[i].Collider.enabled)
                                {
                                    IncludedCollidersData[i].RefreshColliderData();
                                    CollidersDataToCheck.Add(IncludedCollidersData[i]);
                                }
                            }
                            else
                            {
                                if (IncludedCollidersData[i].Collider2D.enabled)
                                {
                                    IncludedCollidersData[i].RefreshColliderData();
                                    CollidersDataToCheck.Add(IncludedCollidersData[i]);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Generating colliders on tail with provided settings
        /// If collision space is world then rigidbody colliders are added, if selective no additional components are needed
        /// </summary>
        void SetupSphereColliders()
        {
            if (CollisionSpace == ECollisionSpace.World_Slow)
            {
                for (int i = 1; i < _TransformsGhostChain.Count; i++)
                {
                    if (CollidersSameLayer) _TransformsGhostChain[i].gameObject.layer = gameObject.layer; else _TransformsGhostChain[i].gameObject.layer = CollidersLayer;
                }

                if (CollidersType != 0)
                {
                    for (int i = 1; i < _TransformsGhostChain.Count - 1; i++)
                    {
                        CapsuleCollider caps = _TransformsGhostChain[i].gameObject.AddComponent<CapsuleCollider>();

                        TailCollisionHelper tcol = _TransformsGhostChain[i].gameObject.AddComponent<TailCollisionHelper>().Init(CollidersAddRigidbody, RigidbodyMass);
                        tcol.TailCollider = caps;
                        tcol.Index = i;
                        tcol.ParentTail = this;

                        caps.radius = GetColliderSphereRadiusFor(_TransformsGhostChain, i);
                        caps.direction = 2;
                        caps.height = (_TransformsGhostChain[i].position - _TransformsGhostChain[i + 1].position).magnitude * 2f - caps.radius;
                        caps.center = _TransformsGhostChain[i].InverseTransformPoint(Vector3.Lerp(_TransformsGhostChain[i].position, _TransformsGhostChain[i + 1].position, 0.5f));

                        TailSegments[i].ColliderRadius = caps.radius;
                        TailSegments[i].CollisionHelper = tcol;
                    }
                }
                else
                {
                    for (int i = 1; i < _TransformsGhostChain.Count; i++)
                    {
                        SphereCollider s = _TransformsGhostChain[i].gameObject.AddComponent<SphereCollider>();
                        TailCollisionHelper tcol = _TransformsGhostChain[i].gameObject.AddComponent<TailCollisionHelper>().Init(CollidersAddRigidbody, RigidbodyMass);
                        tcol.TailCollider = s;
                        tcol.Index = i;
                        tcol.ParentTail = this;
                        s.radius = GetColliderSphereRadiusFor(_TransformsGhostChain, i);
                        TailSegments[i].ColliderRadius = s.radius;
                        TailSegments[i].CollisionHelper = tcol;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _TransformsGhostChain.Count; i++)
                    TailSegments[i].ColliderRadius = GetColliderSphereRadiusFor(i);

                IncludedCollidersData = new List<FImp_ColliderData_Base>();
                CollidersDataToCheck = new List<FImp_ColliderData_Base>();

                #region Dynamic inclusion preparations

                if (DynamicWorldCollidersInclusion)
                {
                    if (CollisionMode == ECollisionMode.m_3DCollision)
                        for (int i = 0; i < IncludedColliders.Count; i++) DynamicAlwaysInclude.Add(IncludedColliders[i]);
                    else
                        for (int i = 0; i < IncludedColliders2D.Count; i++) DynamicAlwaysInclude.Add(IncludedColliders2D[i]);

                    Transform middleSegm = TailSegments[TailSegments.Count / 2].transform;
                    float scaleRef = Vector3.Distance(_TransformsGhostChain[0].position, _TransformsGhostChain[_TransformsGhostChain.Count - 1].position);

                    TailCollisionHelper cHelper = middleSegm.gameObject.AddComponent<TailCollisionHelper>();
                    cHelper.ParentTail = this;
                    SphereCollider triggerC = null;
                    CircleCollider2D triggerC2D = null;

                    if (CollisionMode == ECollisionMode.m_3DCollision)
                    {
                        triggerC = middleSegm.gameObject.AddComponent<SphereCollider>();
                        triggerC.isTrigger = true;
                        cHelper.TailCollider = triggerC;
                    }
                    else
                    {
                        triggerC2D = middleSegm.gameObject.AddComponent<CircleCollider2D>();
                        triggerC2D.isTrigger = true;
                        cHelper.TailCollider2D = triggerC2D;
                    }

                    cHelper.Init(true, 1f, true);

                    float scale = Mathf.Abs(middleSegm.transform.lossyScale.z);
                    if (scale == 0f) scale = 1f;
                    if (triggerC != null) triggerC.radius = scaleRef / scale;
                    else triggerC2D.radius = scaleRef / scale;

                    if (CollidersSameLayer)
                        middleSegm.gameObject.layer = gameObject.layer;
                    else
                        middleSegm.gameObject.layer = CollidersLayer;
                }

                #endregion

                RefreshIncludedCollidersDataList();
            }

            collisionInitialized = true;
        }


        /// <summary>
        /// Collision data sent by single tail segment
        /// </summary>
        internal void CollisionDetection(int index, Collision collision)
        {
            TailSegments[index].collisionContacts = collision;
        }


        /// <summary>
        /// Exitting collision
        /// </summary>
        internal void ExitCollision(int index)
        {
            TailSegments[index].collisionContacts = null;
        }


        /// <summary>
        /// Use saved collision contact in right moment when uxecuting update methods
        /// </summary>
        protected bool UseCollisionContact(int index, ref Vector3 pos)
        {
            if (TailSegments[index].collisionContacts == null) return false;
            if (TailSegments[index].collisionContacts.contacts.Length == 0) return false; // In newest Unity 2018 versions 'Collision' class is generated even there are no collision contacts

            Collision collision = TailSegments[index].collisionContacts;
            float thisCollRadius = FImp_ColliderData_Sphere.CalculateTrueRadiusOfSphereCollider(TailSegments[index].transform, TailSegments[index].ColliderRadius) * 0.95f;

            if (collision.collider)
            {
                SphereCollider collidedSphere = collision.collider as SphereCollider;

                // If we collide sphere we can calculate precise segment offset for it
                if (collidedSphere)
                {
                    FImp_ColliderData_Sphere.PushOutFromSphereCollider(collidedSphere, thisCollRadius, ref pos, Vector3.zero);
                }
                else
                {
                    CapsuleCollider collidedCapsule = collision.collider as CapsuleCollider;

                    // If we collide capsule we can calculate precise segment offset for it
                    if (collidedCapsule)
                    {
                        FImp_ColliderData_Capsule.PushOutFromCapsuleCollider(collidedCapsule, thisCollRadius, ref pos, Vector3.zero);
                    }
                    else
                    {
                        BoxCollider collidedBox = collision.collider as BoxCollider;

                        // If we collide box we can calculate precise segment offset for it
                        if (collidedBox)
                        {
                            if (TailSegments[index].CollisionHelper.RigBody)
                            {
                                if (collidedBox.attachedRigidbody)
                                {
                                    if (TailSegments[index].CollisionHelper.RigBody.mass > 1f)
                                    {
                                        FImp_ColliderData_Box.PushOutFromBoxCollider(collidedBox, collision, thisCollRadius, ref pos);
                                        Vector3 pusherPos = pos;
                                        FImp_ColliderData_Box.PushOutFromBoxCollider(collidedBox, thisCollRadius, ref pos);

                                        pos = Vector3.Lerp(pos, pusherPos, TailSegments[index].CollisionHelper.RigBody.mass / 5f);
                                    }
                                    else
                                        FImp_ColliderData_Box.PushOutFromBoxCollider(collidedBox, thisCollRadius, ref pos);
                                }
                                else
                                    FImp_ColliderData_Box.PushOutFromBoxCollider(collidedBox, thisCollRadius, ref pos);
                            }
                            else
                                FImp_ColliderData_Box.PushOutFromBoxCollider(collidedBox, thisCollRadius, ref pos);
                        }
                        else // If we collide mesh we can't calculate very precise segment offset but we can support it in some way
                        {
                            MeshCollider collidedMesh = collision.collider as MeshCollider;
                            if (collidedMesh)
                            {
                                FImp_ColliderData_Mesh.PushOutFromMeshCollider(collidedMesh, collision, thisCollRadius, ref pos);
                            }
                            else // If we collide terrain we can calculate very precise segment offset because terrain not rotates
                            {
                                TerrainCollider terrain = collision.collider as TerrainCollider;
                                FImp_ColliderData_Terrain.PushOutFromTerrain(terrain, thisCollRadius, ref pos);
                            }
                        }
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Refreshing colliders data for included colliders when it's needed
        /// </summary>
        public void RefreshIncludedCollidersDataList()
        {
            bool refr = false;

            if (CollisionMode == ECollisionMode.m_3DCollision)
            {
                if (IncludedColliders.Count != IncludedCollidersData.Count || forceRefreshCollidersData)
                {
                    IncludedCollidersData.Clear();

                    for (int i = IncludedColliders.Count - 1; i >= 0; i--)
                    {
                        if (IncludedColliders[i] == null) { IncludedColliders.RemoveAt(i); continue; }
                        FImp_ColliderData_Base colData = FImp_ColliderData_Base.GetColliderDataFor(IncludedColliders[i]);
                        IncludedCollidersData.Add(colData);
                    }

                    refr = true;
                }
            }
            else
            {
                if (IncludedColliders2D.Count != IncludedCollidersData.Count || forceRefreshCollidersData)
                {
                    IncludedCollidersData.Clear();

                    for (int i = IncludedColliders2D.Count - 1; i >= 0; i--)
                    {
                        if (IncludedColliders2D[i] == null) { IncludedColliders2D.RemoveAt(i); continue; }
                        FImp_ColliderData_Base colData = FImp_ColliderData_Base.GetColliderDataFor(IncludedColliders2D[i]);
                        IncludedCollidersData.Add(colData);
                    }

                    refr = true;
                }
            }

            if (refr) forceRefreshCollidersData = false;
        }


        /// <summary>
        /// Pushing tail segment from detected collider
        /// </summary>
        public bool PushIfSegmentInsideCollider(TailSegment bone, ref Vector3 targetPoint)
        {
            bool pushed = false;

            if (!CheapCollision) // Detailed Collision
            {
                for (int i = 0; i < CollidersDataToCheck.Count; i++)
                {
                    bool push = CollidersDataToCheck[i].PushIfInside(ref targetPoint, bone.GetRadiusScaled(), Vector3.zero);
                    if (!pushed) if (push) { pushed = true; }
                }
            }
            else
            {
                for (int i = 0; i < CollidersDataToCheck.Count; i++)
                {
                    if (CollidersDataToCheck[i].PushIfInside(ref targetPoint, bone.GetRadiusScaled(), Vector3.zero)) { return true; }
                }
            }

            return pushed;
        }


        /// <summary>
        /// Calculating automatically scale for colliders on tail, which will be automatically assigned after initialization
        /// </summary>
        protected float GetColliderSphereRadiusFor(int i)
        {
            TailSegment tailPoint = TailSegments[i];
            float refDistance = 1f;

            if (i >= _TransformsGhostChain.Count) return refDistance;

            if (_TransformsGhostChain.Count > 1)
            {
                refDistance = Vector3.Distance(_TransformsGhostChain[1].position, _TransformsGhostChain[0].position);
            }

            float singleScale = refDistance;
            if (i != 0) singleScale = Mathf.Lerp(refDistance, Vector3.Distance(_TransformsGhostChain[i - 1].position, _TransformsGhostChain[i].position) * 0.5f, CollisionsAutoCurve);

            float div = _TransformsGhostChain.Count - 1;
            if (div <= 0f) div = 1f;
            float step = 1f / div;

            return 0.5f * singleScale * CollidersScaleMul * CollidersScaleCurve.Evaluate(step * (float)i);
        }


        /// <summary>
        /// Calculating automatically scale for colliders on tail, which will be automatically assigned after initialization
        /// </summary>
        protected float GetColliderSphereRadiusFor(List<Transform> transforms, int i)
        {
            float refDistance = 1f;
            if (transforms.Count > 1) refDistance = Vector3.Distance(_TransformsGhostChain[1].position, _TransformsGhostChain[0].position);

            float nextDistance = refDistance;
            if (i != 0) nextDistance = Vector3.Distance(_TransformsGhostChain[i - 1].position, _TransformsGhostChain[i].position);

            float singleScale = Mathf.Lerp(refDistance, nextDistance * 0.5f, CollisionsAutoCurve);
            float step = 1f / (float)(transforms.Count - 1);
            return 0.5f * singleScale * CollidersScaleMul * CollidersScaleCurve.Evaluate(step * (float)i);
        }


        /// <summary>
        /// Adding collider to included colliders list
        /// </summary>
        public void AddCollider(Collider collider)
        {
            if (IncludedColliders.Contains(collider)) return;
            IncludedColliders.Add(collider);
        }

        /// <summary>
        /// Adding collider to included colliders list
        /// </summary>
        public void AddCollider(Collider2D collider)
        {
            if (IncludedColliders2D.Contains(collider)) return;
            IncludedColliders2D.Add(collider);
        }

        /// <summary>
        /// Checking if colliders list don't have duplicates
        /// </summary>
        public void CheckForColliderDuplicatesAndNulls()
        {
            for (int i = 0; i < IncludedColliders.Count; i++)
            {
                Collider col = IncludedColliders[i];
                int count = IncludedColliders.Count(o => o == col);

                if (count > 1)
                {
                    IncludedColliders.RemoveAll(o => o == col);
                    IncludedColliders.Add(col);
                }
            }


            for (int i = IncludedColliders.Count - 1; i >= 0; i--)
            {
                if (IncludedColliders[i] == null) IncludedColliders.RemoveAt(i);
            }
        }

        public void CheckForColliderDuplicatesAndNulls2D()
        {
            for (int i = 0; i < IncludedColliders2D.Count; i++)
            {
                Collider2D col = IncludedColliders2D[i];
                int count = IncludedColliders2D.Count(o => o == col);

                if (count > 1)
                {
                    IncludedColliders2D.RemoveAll(o => o == col);
                    IncludedColliders2D.Add(col);
                }
            }
        }


        void TailCalculations_ComputeSegmentCollisions(TailSegment bone, ref Vector3 position)
        {
            // Computing collision contact timer
            if (bone.CollisionContactFlag) bone.CollisionContactFlag = false;
            else if (bone.CollisionContactRelevancy > 0f) bone.CollisionContactRelevancy -= justDelta;

            if (CollisionSpace == ECollisionSpace.Selective_Fast)
            {
                // Setting collision contact flag
                if (PushIfSegmentInsideCollider(bone, ref position))
                {
                    bone.CollisionContactFlag = true;
                    bone.CollisionContactRelevancy = justDelta * 7f;
                    bone.ChildBone.CollisionContactRelevancy = Mathf.Max(bone.ChildBone.CollisionContactRelevancy, justDelta * 3.5f);
                    if (bone.ChildBone.ChildBone != null) bone.ChildBone.ChildBone.CollisionContactRelevancy = Mathf.Max(bone.ChildBone.CollisionContactRelevancy, justDelta * 3f);
                    //bone.ParentBone.CollisionContactRelevancy = Mathf.Max(bone.ParentBone.CollisionContactRelevancy, delta * 3f);
                }
            }
            else
            {
                // Setting collision contact flag
                if (UseCollisionContact(bone.Index, ref position))
                {
                    bone.CollisionContactFlag = true;
                    bone.CollisionContactRelevancy = justDelta * 7f;
                    bone.ChildBone.CollisionContactRelevancy = Mathf.Max(bone.ChildBone.CollisionContactRelevancy, justDelta * 3.5f);
                    if (bone.ChildBone.ChildBone != null) bone.ChildBone.ChildBone.CollisionContactRelevancy = Mathf.Max(bone.ChildBone.CollisionContactRelevancy, justDelta * 3f);
                    //bone.ParentBone.CollisionContactRelevancy = Mathf.Max(bone.ParentBone.CollisionContactRelevancy, delta * 2f);
                }
            }
        }


    }
}