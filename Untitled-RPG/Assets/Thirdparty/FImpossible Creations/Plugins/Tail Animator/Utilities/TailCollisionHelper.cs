using UnityEngine;
#if UNITY_2019_4_OR_NEWER
using UnityEngine.Tilemaps;
#endif

namespace FIMSpace.FTail
{
    /// <summary>
    /// FM: Simple class sending collision events to main script
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Hidden/Tail Collision Helper")]
    public class TailCollisionHelper : MonoBehaviour
    {
        public TailAnimator2 ParentTail;
        public Collider TailCollider;
        public Collider2D TailCollider2D;
        public int Index;

        internal Rigidbody RigBody { get; private set; }
        internal Rigidbody2D RigBody2D { get; private set; }
        Transform previousCollision;

        internal TailCollisionHelper Init(bool addRigidbody = true, float mass = 1f, bool kinematic = false)
        {
            if (TailCollider2D == null)
            {
                if (addRigidbody)
                {
                    Rigidbody rig = GetComponent<Rigidbody>();
                    if (!rig) rig = gameObject.AddComponent<Rigidbody>();
                    rig.interpolation = RigidbodyInterpolation.Interpolate;
                    rig.useGravity = false;
                    rig.isKinematic = kinematic;
                    rig.constraints = RigidbodyConstraints.FreezeAll;
                    rig.mass = mass;
                    RigBody = rig;
                }
                else
                {
                    RigBody = GetComponent<Rigidbody>();
                    if (RigBody) RigBody.mass = mass;
                }
            }
            else
            {
                if (addRigidbody)
                {
                    Rigidbody2D rig = GetComponent<Rigidbody2D>();
                    if (!rig) rig = gameObject.AddComponent<Rigidbody2D>();
                    rig.interpolation = RigidbodyInterpolation2D.Interpolate;
                    rig.gravityScale = 0f;
                    rig.isKinematic = kinematic;
                    rig.constraints = RigidbodyConstraints2D.FreezeAll;
                    rig.mass = mass;
                    RigBody2D = rig;
                }
                else
                {
                    RigBody2D = GetComponent<Rigidbody2D>();
                    if (RigBody2D) RigBody2D.mass = mass;
                }
            }

            return this;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (ParentTail == null)
            {
                GameObject.Destroy(this);
                return;
            }

            TailCollisionHelper helper = collision.transform.GetComponent<TailCollisionHelper>();

            if (helper)
            {
                if (ParentTail.CollideWithOtherTails == false) return;
                if (helper.ParentTail == ParentTail) return;
            }

            if (ParentTail._TransformsGhostChain.Contains(collision.transform)) return;
            if (ParentTail.IgnoredColliders.Contains(collision.collider)) return;

            ParentTail.CollisionDetection(Index, collision);
            previousCollision = collision.transform;
        }


        void OnCollisionExit(Collision collision)
        {
            if (collision.transform == previousCollision)
            {
                ParentTail.ExitCollision(Index);
                previousCollision = null;
            }
        }


        void OnTriggerEnter(Collider other)
        {
            TailCollisionHelper helper = other.transform.GetComponent<TailCollisionHelper>();

            if (other.isTrigger) return;

            if (helper)
            {
                if (ParentTail.CollideWithOtherTails == false) return;
                if (helper.ParentTail == ParentTail) return;
            }

            if (ParentTail._TransformsGhostChain.Contains(other.transform)) return;
            if (ParentTail.IgnoredColliders.Contains(other)) return;

            if (ParentTail.IgnoreMeshColliders)
                if (other is MeshCollider) return;

            ParentTail.AddCollider(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (ParentTail.IncludedColliders.Contains(other))
            {
                if (!ParentTail.DynamicAlwaysInclude.Contains(other))
                    ParentTail.IncludedColliders.Remove(other);
            }
        }


        void OnTriggerEnter2D(Collider2D other)
        {
            TailCollisionHelper helper = other.transform.GetComponent<TailCollisionHelper>();

            if (other.isTrigger) return;

            if (helper)
            {
                if (ParentTail.CollideWithOtherTails == false) return;
                if (helper.ParentTail == ParentTail) return;
            }

            if (ParentTail._TransformsGhostChain.Contains(other.transform)) return;
            if (ParentTail.IgnoredColliders2D.Contains(other)) return;

            if (other is CompositeCollider2D) return;
#if UNITY_2019_4_OR_NEWER
            if (other is TilemapCollider2D) return;
#endif
            if (other is EdgeCollider2D) return;

            ParentTail.AddCollider(other);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (ParentTail.IncludedColliders2D.Contains(other))
            {
                if (!ParentTail.DynamicAlwaysInclude.Contains(other))
                    ParentTail.IncludedColliders2D.Remove(other);
            }
        }
    }
}