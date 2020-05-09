using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using MalbersAnimations.Events;


namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/Arrow")]
    public class MArrow : MonoBehaviour, IArrow
    {
        #region Variables
        [Space]
        [Tooltip("Damage this Arrow Causes")]
        public float damage = 1;                                   //Damage this Arrow Causes       
        [Tooltip("Penetration to the hitted mesh")]
        public float Penetration = 0.2f;                            //Penetration to the next mesh
        [Tooltip("How long the arrow is stay alive after hit Something")]
        public float AgeAfterImpact = 10f;                          //How long the arrow is stay alive after hit Something
        [Tooltip("How long the arrow is alive flying (Used for removing arrows shooted off the map)")]
        public float AgeFlying = 30f;                               //How long the arrow is alive flying (Used for shooting arrows off the map)
        [Tooltip("Damage this Arrow Causes")]
        public float tailOffset = 1;                                //Offset from the Tip to the End of the arrow 
        public bool useGravity = true;
        public bool AffectRigidBodies = true;

        public StatModifier AffectStat;

        [Space]
        [Header("Events")]

        public UnityEvent OnFireArrow;                       //Send the transform to the event
        public RayCastHitEvent OnHitTarget;                  //Send the transform to the event
        public float timeAfterImpact;
        public UnityEvent AfterImpact;

        protected LayerMask hitmask;                        //What to Hit
        protected float force;
        private Vector3 direction;
        protected Rigidbody _rigidbody;
        protected Vector3 DeltaPos;
        protected bool isflying;


        [HideInInspector] public RaycastHit HitPoint;

        WaitForEndOfFrame WfeoF = new WaitForEndOfFrame();

        #endregion
        Rigidbody _Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }
                return _rigidbody;
            }
        }

        public LayerMask HitMask
        {
            get { return hitmask; }
            set { hitmask = value; }
        }

        public float TailOffset
        {
            get { return tailOffset; }
            set { tailOffset = value; }
        }

        public QueryTriggerInteraction TriggerInteraction { get; set; }

        public float Damage
        {
            get { return damage; }
            set
            {
                damage = value;
                AffectStat.Value = value;
            }
        }


        public bool IsFlying
        {
            get { return isflying; }
        }

        /// <summary>Shoots the Arrow in a given direction</summary>
        public virtual void ShootArrow(float force, Vector3 direction)
        {
            this.force = force;
            this.direction = direction;
            if (_Rigidbody)
            {
                _Rigidbody.constraints = RigidbodyConstraints.None;
                _Rigidbody.isKinematic = false;

                if (useGravity)
                    _Rigidbody.useGravity = true;
                else
                    _Rigidbody.useGravity = false;

                _Rigidbody.AddForce(direction * force);

                // mRigidbody.velocity = direction * force;
                isflying = true;
                StartCoroutine(FlyingArrow());
            }

            Invoke("FlyingAge", AgeFlying);

            OnFireArrow.Invoke();
        }

        /// <summary>To avoid that the arrow flyies for all eternity</summary>
        private void FlyingAge()
        {
            if (isflying)
            {
                isflying = false;
                Destroy(gameObject);
                StopAllCoroutines();
            }
        }


        /// <summary>Call This if some else Shoot the Arrow, if the arrow is already flying check for collision to invoke ONHIT</summary>
        public virtual void TestFlyingArrow()
        {
            StopAllCoroutines();
            StartCoroutine(TestFly());
            Invoke("FlyingAge", AgeFlying);
        }


        IEnumerator TestFly()
        {
            isflying = true;
            DeltaPos = transform.position;


            RaycastHit hit = new RaycastHit();

            while (isflying)
            {
                float Prediction = Mathf.Abs((transform.position - DeltaPos).magnitude) + 0.3f; //Prediction to Hit something in the way

                if (Physics.Raycast(transform.position, _Rigidbody.velocity.normalized, out hit, Prediction, hitmask, TriggerInteraction))
                {
                    isflying = false;
                }
                else
                {
                    Debug.DrawRay(transform.position, _Rigidbody.velocity.normalized * Prediction, Color.red);

                    DeltaPos = transform.position;
                }

                yield return null;
            }

            TestHit(hit);
        }

        IEnumerator FlyingArrow()
        {
            RaycastHit hit;
            float Prediction = (force / _rigidbody.mass) * Time.fixedDeltaTime * Time.fixedDeltaTime;

            Debug.DrawRay(transform.position, direction * Prediction, Color.red);

            if (Physics.Raycast(transform.position, direction, out hit, Prediction, hitmask ,TriggerInteraction))
            {
                OnHit(hit);
                isflying = false;
            }

            yield return WfeoF;


            while (isflying)
            {
                Prediction = Mathf.Abs((transform.position - DeltaPos).magnitude) + 0.3f; //Prediction to Hit something in the way

                if (Physics.Raycast(transform.position, _Rigidbody.velocity, out hit, Prediction, hitmask, TriggerInteraction))
                {
                    OnHit(hit);
                    isflying = false;

                }
                else
                {
                    Debug.DrawRay(transform.position, _Rigidbody.velocity.normalized * Prediction, Color.red);
                    if (_Rigidbody.velocity.magnitude > 0)
                    {
                        transform.rotation = Quaternion.LookRotation(_Rigidbody.velocity.normalized, transform.up);
                    }

                    yield return WfeoF;
                    DeltaPos = transform.position;
                    yield return null;
                }
            }
        }


        /// <summary>Will Send Damage to the Hit</summary>
        public virtual void TestHit(RaycastHit other)
        {
            if (other.transform)
            {
                if (other.rigidbody && AffectRigidBodies)
                {
                    other.rigidbody.AddForceAtPosition(transform.forward * force, other.point);
                }
            }
            OnHitTarget.Invoke(other);
        }

        public virtual void OnHit(RaycastHit other)
        {
            if (other.transform)
            {
                if (AffectRigidBodies)
                {
                    other.rigidbody?.AddForceAtPosition(transform.forward * force, other.point);
                }

                AffectStat.Value = Damage;

                AffectStat.ModifyStat(other.transform.GetComponentInParent<Stats>());

                var interactable = other.transform.GetComponent<IInteractable>();
                interactable?.Interact();

                Damager.SetDamage(DeltaPos.normalized, other.transform);
            }


          //  _Rigidbody.isKinematic = true;
            _Rigidbody.constraints = RigidbodyConstraints.FreezeAll;


            //FIX FOR SCALED OBJECTS
            Vector3 NewScale = other.transform.lossyScale;
            NewScale.x = 1f / Mathf.Max(NewScale.x, 0.0001f);
            NewScale.y = 1f / Mathf.Max(NewScale.y, 0.0001f);
            NewScale.z = 1f / Mathf.Max(NewScale.z, 0.0001f);

            GameObject Hlper = new GameObject();
            Hlper.name = name + "Link";

            Hlper.transform.parent = other.collider.transform;
            Hlper.transform.localScale = NewScale;
            Hlper.transform.position = other.point;
            //Hlper.transform.localPosition = other.transform.InverseTransformPoint(other.point);
            Hlper.transform.localRotation = Quaternion.identity;

            transform.parent = Hlper.transform;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            transform.position += transform.forward * Penetration; //Put the arrow a bit deeper in the collider

            Destroy(Hlper, AgeAfterImpact);
            Destroy(gameObject, AgeAfterImpact);

            OnHitTarget.Invoke(other);

            if (timeAfterImpact > 0)
            {
                Invoke("InvokeAfterImpact", timeAfterImpact);
            }
            else
            {
                AfterImpact.Invoke();
            }
        }

        void InvokeAfterImpact()
        {
            AfterImpact.Invoke();
        }
    }
}