using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using MalbersAnimations.HAP;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Weapons
{
    [System.Serializable] public class WeaponEvent : UnityEvent<IMWeapon> { }
    [System.Serializable] public class WeaponActionEvent : UnityEvent<WA> { }

    public abstract class MWeapon : MonoBehaviour, IMWeapon 
    {
        [SerializeField] public int weaponID;                                                  //Weapon ID unique Number
        [SerializeField] private BoolReference active = new BoolReference( true);                          //is the animal
        [SerializeField] private FloatReference minDamage = new FloatReference (10);                       //Weapon minimum Damage
        [SerializeField] private FloatReference maxDamage = new FloatReference(20);                        //Weapon Max Damage
        [SerializeField] private FloatReference minForce = new FloatReference(500);                        //Weapon min Force to push rigid bodies;
        [SerializeField] private FloatReference maxForce = new FloatReference(1000);                       //Weapon max Force to push rigid bodies;
        [SerializeField] public StatModifier AffectStat;                                                   //Affect Stat
        public WeaponID weaponType;                      

        private bool isEquiped = false;

        public bool rightHand = true;                       // With which hand you will draw the Weapon;

        public WeaponHolder holder = WeaponHolder.None;     // From which Holder you will draw the Weapon

        public Vector3
               positionOffset,                              //Position Offset
               rotationOffset;                              //Rotation Offset

        //protected DamageValues DV;                          //Direction and DamageAmount for the Weapon

        /// <summary> Weapon Sounds</summary>
        public AudioClip[] Sounds;                          //Sounds for the weapon
        public AudioSource WeaponSound;                     //Reference for the audio Source;

        #region Properties

        /// <summary>Unique Weapon ID for each weapon</summary>
        public int WeaponID { get { return weaponID; } }

        /// <summary>Holder that the weapon can be draw from</summary>
        public WeaponHolder Holder
        {
            get { return holder; }
            set { holder = value; }
        }

        /// <summary> Is the Weapon Equiped </summary>
        public bool IsEquiped
        {
            get { return isEquiped; }
            set
            {
                isEquiped = value;

                if (isEquiped)
                {
                    OnEquiped.Invoke(this);
                }
                else
                {
                    Owner = null;
                    HitLayer = new LayerMask();      //Clean the Layer Mask
                    OnUnequiped.Invoke(this);
                }
            }
        }


        /// <summary>Enables the Main Attack</summary>
        public virtual bool MainAttack { get; set; }

        /// <summary>Enables the Secondary Attack or Aiming</summary>
        public virtual bool SecondAttack { get; set; }

        public float MinDamage
        {
            set { minDamage.Value = value; }
            get { return minDamage; }
        }

        public float MaxDamage
        {
            set { maxDamage.Value = value; }
            get { return maxDamage; }
        }

        /// <summary>Is the weapon used on the Right hand(True) or left hand (False)</summary>
        public bool RightHand
        {
            set { rightHand = value; }
            get { return rightHand; }
        }

        public Vector3 PositionOffset
        {
            get { return positionOffset; }
        }

        public Vector3 RotationOffset
        {
            get { return rotationOffset; }
        }

        public float MinForce
        {
            get { return minForce; }
            set { minForce.Value = value; }
        }

        public float MaxForce
        {
            get { return maxForce; }
            set { maxForce.Value = value; }
        }

        /// <summary>The rider using this weapon while this weapon is equiped</summary>
        public Transform Owner { get; set; }

        public LayerMask HitLayer { get; set; }

        /// <summary>Enable or Disable the weapon to "block it"</summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (active.Value != value)
                {
                    active.Value = value;
                    if (Owner) Owner.SendMessage(value ? "WeaponEnabled" : "WeaponDisabled", this, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public WeaponID WeaponType
        {
            get { return weaponType; }
        }

        public QueryTriggerInteraction TriggerInteraction { get; set; }
        #endregion

        public WeaponEvent OnEquiped = new WeaponEvent();
        public WeaponEvent OnUnequiped = new WeaponEvent();

        /// <summary>Returns True if the Weapons has the same ID</summary>
        public override bool Equals(object a)
        {
            if (a is IMWeapon)
            {
                if (weaponID == (a as IMWeapon).WeaponID)
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public abstract void ResetWeapon(); 
        

        public virtual void InitializeWeapon()
        {
            isEquiped = false;

            WeaponSound = GetComponent<AudioSource>();
            if (!WeaponSound)
                WeaponSound = gameObject.AddComponent<AudioSource>(); //Create an AudioSourse if theres no Audio Source on the weapon

            WeaponSound.spatialBlend = 1;
        }

        /// CallBack from the RiderCombat Layer in the Animator to reproduce a sound on the weapon
        public virtual void PlaySound(int ID)
        {
            if (ID > Sounds.Length - 1) return;

            if (Sounds[ID] != null && WeaponSound)
            {
                WeaponSound.PlayOneShot(Sounds[ID]);   //Play Draw/ Weapon Clip
            }
        }

        ///Editor
        [HideInInspector] public bool ShowEventEditor;
    }
}