using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;
using System.Collections;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Weapons
{
    public class MGun : MWeapon, IGun
    {
        [SerializeField] private IntReference Ammo = new IntReference(30);                             //Total of Ammo for this weapon
        [SerializeField] private IntReference ammoInChamber =  new IntReference(8);                    //Total of Ammo in the Chamber
        [SerializeField] public IntReference ClipSize = new IntReference(8);                         //Max Capacity of the Ammo in once hit
        [SerializeField] private bool isAutomatic = false;             //Press Fire one or continues 
        //public float fireRate = 0.5f;
        public GameObject bulletHole;
        public float BulletHoleTime = 10;
        public Vector3Event OnFire;
        public UnityEvent OnReload;
        public TransformEvent OnHit;
        public BoolEvent OnAiming;
        protected bool isAiming;

        public int TotalAmmo
        {
            get { return Ammo.Value; }
            set { Ammo.Value = value; }
        }

        public int AmmoInChamber
        {
            get { return ammoInChamber.Value; }
            set { ammoInChamber.Value = value; }
        }

        public bool IsAutomatic
        {
            get { return isAutomatic; }
            set { isAutomatic = value; }
        }

        public bool IsAiming
        {
            get { return isAiming; }
            set
            {
                if (isAiming != value)
                {
                    isAiming = value;
                    OnAiming.Invoke(IsAiming);
                }
            }
        }

        //public float FireRate
        //{
        //    get { return fireRate; }
        //    set { fireRate = value; }
        //}

        /// <summary>
        /// Set the total Ammo (Refill when you got some ammo)
        /// </summary>
        /// <param name="value"></param>
        public void SetTotalAmmo(int value)
        {
            Ammo = value;
        }

        void Awake()
        {
            InitializeWeapon();
        }

        public virtual void ReduceAmmo(int amount)
        {
            AmmoInChamber -= amount;
        }

        IEnumerator ReduceAmmoAfterShoot()
        {
            yield return null;
            yield return null;
            ReduceAmmo(1);
        }
        IEnumerator C_ReduceAmmoAfterShoot;


        public virtual void FireProyectile(RaycastHit AimRay)
        {
            float percent = Random.Range(0, 1);

            Vector3 AimDirection = (AimRay.point - transform.position).normalized;

            if (C_ReduceAmmoAfterShoot != null) StopCoroutine(C_ReduceAmmoAfterShoot);
            C_ReduceAmmoAfterShoot = ReduceAmmoAfterShoot();
            StartCoroutine(C_ReduceAmmoAfterShoot);

            OnFire.Invoke(AimDirection);

            if (AimRay.transform)                                                                                       //If the AIM Ray hit something 
            {
                var interactable = AimRay.transform.GetComponent<IInteractable>();
                interactable?.Interact();

                AffectStat.Value = Random.Range(MinDamage, MaxDamage);
                AffectStat.ModifyStat(AimRay.transform.GetComponentInParent<Stats>());

                Damager.SetDamage(AimRay.normal, AimRay.transform);               

                if (AimRay.rigidbody)                                                                                       //If the thing we hit has a rigidbody
                {
                    AimRay.rigidbody.AddForceAtPosition(AimDirection * Mathf.Lerp(MinForce, MaxForce, percent), AimRay.point);                                //Apply the force to it
                }

                BulletHole(AimRay);

                OnHit.Invoke(AimRay.transform);  //Invoke OnHitSomething Event
            }
        }

        public virtual bool Reload()
        {
            if (Ammo == 0) return false;
            int AmmoSpent = ClipSize - AmmoInChamber;                         //Ammo Spent from the Chamber
            if (AmmoSpent == 0) return false;                                   //If there's no ammo spent exit

            int AmmoLeft = TotalAmmo - AmmoSpent;                               //Ammo Remaining

            if (AmmoLeft >= 0)                                                  //If is there any Ammo 
            {
                AmmoInChamber += AmmoSpent;
                TotalAmmo = AmmoLeft;
            }
            else
            {
                AmmoInChamber += TotalAmmo;                                          //Set in the Chamber the remaining ammo  
                TotalAmmo = 0;                                                  //Empty the Total Ammo
            }
            OnReload.Invoke();
            return true;
        }

        public virtual void BulletHole(RaycastHit hit)
        {
            if (!bulletHole) return;

            //For Good Instantiation on the object (Scaled or not)
            Vector3 NewScale = hit.transform.lossyScale;

            NewScale.x = 1f / Mathf.Max(NewScale.x, 0.0001f);
            NewScale.y = 1f / Mathf.Max(NewScale.y, 0.0001f);
            NewScale.z = 1f / Mathf.Max(NewScale.z, 0.0001f);

            GameObject Hlper = new GameObject();
            Hlper.transform.parent = hit.collider.transform;
            Hlper.transform.localScale = NewScale;
            Hlper.transform.localPosition = hit.collider.transform.InverseTransformPoint(hit.point);
            Hlper.transform.localRotation = Quaternion.identity;


            GameObject hole = Instantiate(bulletHole);

            hole.transform.parent = Hlper.transform;
            hole.transform.localScale = Vector3.one;
            hole.transform.localPosition = Vector3.zero;
            hole.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            hole.transform.localRotation = hole.transform.localRotation * Quaternion.Euler(0, Random.Range(-90, 90), 0);


            Destroy(Hlper, BulletHoleTime);
        }

        public override void ResetWeapon()  { }
    }
}