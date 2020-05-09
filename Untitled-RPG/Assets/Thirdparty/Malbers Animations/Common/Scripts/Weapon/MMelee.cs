using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using UnityEngine.Events;
using MalbersAnimations.HAP;

namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/Melee Weapon")]
    public class MMelee : MWeapon, IMelee
    {
        protected bool isOnAtackingState;                         //If the weapon is attacking
        protected bool canCauseDamage;                      //The moment in the Animation the weapon can cause Damage        

        public Collider meleeCollider;
        protected List<Transform> AlreadyHitted = new List<Transform>();

        public GameObjectEvent OnHit;
        public BoolEvent OnCauseDamage;
        public Color DebugColor = new Color(1, 0.25f, 0, 0.5f);


        public override bool MainAttack
        {
            get { return base.MainAttack; }
            set
            {
                base.MainAttack = value;
                CanCauseDamage = false;
            }
        }

        public bool CanCauseDamage
        {
            get { return canCauseDamage; }
            set
            {
                if (!IsEquiped) return;

                canCauseDamage = value;
                AlreadyHitted = new List<Transform>();  //Reset the list of transform that I already Hit

                meleeCollider.enabled = value;
            }
        }

        protected TriggerProxy meleeColliderProxy;

        //The Attack Momentum while the Animation is playing
        public virtual void CanDoDamage(bool value)
        {
            CanCauseDamage = value;
            OnCauseDamage.Invoke(value);
        }

        void Start()
        {
             InitializeWeapon();
        }

        public override void InitializeWeapon()
        {
            base.InitializeWeapon();
            CanCauseDamage = false;

            if (meleeCollider)
            {
                meleeColliderProxy = meleeCollider.gameObject.AddComponent<TriggerProxy>();            //Create a proxy to comunicate the Collision and trigger events in case the melee conllider is on another gameObject

                if (meleeCollider.isTrigger)
                {
                    meleeColliderProxy.OnTrigger_Enter.AddListener(WeaponOnTriggerEnter);
                }
                else
                {
                    meleeColliderProxy.OnCollision_Enter.AddListener(WeaponCollisionEnter);
                }
                meleeCollider.enabled = false;
            }
        }


        /// <summary>If the Collider is not a Trigger</summary>
        protected virtual void WeaponCollisionEnter(Collision other)
        {
            if (!IsEquiped) return;
            if (other.contacts.Length == 0) return;

            SetDamageStuff(other.contacts[0].point, other.transform);
        }

        /// <summary>Set Damage if you're using triggers </summary>
        protected virtual void WeaponOnTriggerEnter(Collider other)
        {
            if (!IsEquiped) return;
            if (other.isTrigger) return;

            SetDamageStuff(other.ClosestPointOnBounds(meleeCollider.bounds.center), other.transform);
        }


        internal void SetDamageStuff(Vector3 OtherHitPoint, Transform other)
        {
            var Root = other.root;

            if (Root == transform.root) return;                       //if Im hitting myself

            Mount montura = other.GetComponentInParent<Mount>();
            Mount OwnerMount = Owner.GetComponent<MRider>()?.Montura;
            if (OwnerMount != null && montura ==  OwnerMount) return;          //Do not Hit your Mount   
            
            //Do not Hit my Mount
            if (!MalbersTools.Layer_in_LayerMask(other.gameObject.layer, HitLayer)) return;          //Just hit what is on the HitMask Layer

            Debug.DrawLine(OtherHitPoint, meleeCollider.bounds.center, Color.red, 3f);

            if (canCauseDamage && !AlreadyHitted.Find(item => item == Root))                        //If can cause damage and If I didnt hit the same transform twice
            {
                AlreadyHitted.Add(Root);

                Rigidbody OtherRB = other.GetComponentInParent<Rigidbody>();

                var interactable = other.GetComponent<IInteractable>();
                interactable?.Interact();

                if (OtherRB)                                                             
                {
                    OtherRB.AddExplosionForce(MinForce * 50, OtherHitPoint, 5);
                }


                var mesh = Root.GetComponentInChildren<Renderer>();
                var TargetPos = Root.position;
                if (mesh == null) TargetPos = mesh.bounds.center;

                Vector3 direction = (OtherHitPoint - TargetPos).normalized;

                AffectStat.Value = Random.Range(MinDamage, MaxDamage);
                AffectStat.ModifyStat(Root.GetComponentInChildren<Stats>());                     //Affect Stats


                Damager.SetDamage(direction, Root);

                PlaySound(3);     //Play Hit Sound when get something                                                                                  

                OnHit.Invoke(other.gameObject);

                if (!meleeCollider.isTrigger)
                {
                    meleeCollider.enabled = false;
                }
            }
        }


        /// <summary>Disable Listeners </summary>
        void OnDisable()
        {
            if (meleeColliderProxy)
            {
                if (meleeCollider.isTrigger)

                    meleeColliderProxy.OnTrigger_Stay.RemoveListener(WeaponOnTriggerEnter);
                else
                    meleeColliderProxy.OnCollision_Enter.RemoveListener(WeaponCollisionEnter);
            }
        }

        public override void ResetWeapon()
        {
            meleeCollider.enabled = false;   
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (meleeCollider)
            {
                MalbersTools.DrawTriggers(meleeCollider.transform, meleeCollider, DebugColor);
            }
        }
#endif
    }
}