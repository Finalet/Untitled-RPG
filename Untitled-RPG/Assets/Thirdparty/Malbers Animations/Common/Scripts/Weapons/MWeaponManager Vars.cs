using UnityEngine;
using MalbersAnimations.Weapons;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System;
using MalbersAnimations.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace MalbersAnimations.HAP
{
    /// <summary>Variables and Properties</summary>
    public partial class MWeaponManager
    {
        private const string FreeBothHands = "FreeBothHands";
        private const string FreeRightHand = "FreeRightHand";
        private const string FreeLeftHand = "FreeLeftHand";

        #region Riding Only
        public bool UseWeaponsOnlyWhileRiding = true;
        public IRider Rider { get; private set; }
        #endregion


        /// <summary>Sets a Bool Parameter on the Animator using the parameter Hash</summary>
        public Action<int, bool> SetBoolParameter { get; set; }
        /// <summary>Sets a float Parameter on the Animator using the parameter Hash</summary>
        public Action<int, float> SetFloatParameter { get; set; }
        /// <summary>Sets a Integer Parameter on the Animator using the parameter Hash</summary> 
        public Action<int, int> SetIntParameter { get; set; }

        #region Public Entries
        public bool UseInventory = true;                                //Use this if the weapons are on an Inventory
        public bool AlreadyInstantiated = true;                         //If the weapons comes from an inventory check if they are already intantiate
        public bool UseHolsters = false;                                 //Use this if the weapons are on the Holsters


        public HolsterID DefaultHolster;
        public List<Holster> holsters = new List<Holster>();
        public float HolsterTime = 0.3f;

        /// <summary> Used to change to the Next/Previus Holster</summary>
        public int ActiveHolsterIndex { get; set; }

        /// <summary> ID Value of the Active Holster</summary>
        public Holster ActiveHolster { get; set; }

        public bool UseDefaultIK = true;                        //Exit the Combat Mode on Dismount


        [Tooltip("Disable the Aim Component when is not Needed")]
        public bool DisableAim = false;                        //Exit the Combat Mode on Dismount

        public Transform LeftHandEquipPoint, RightHandEquipPoint;  //Equip Point for the Right Hand    


        /// <summary>Path of the Combat Layer on the Resource Folder </summary>
        public string CombatLayerPath = "Layers/Combat";
        /// <summary>Name of the Combat Layer </summary>
        public string CombatLayerName = "Rider Combat";

        public bool debug;
        #endregion

        /// <summary>Reference for the Animator component</summary>
        public Animator Anim { get; set; }
        /// <summary>Reference for the Animator Update Mode</summary>
        public AnimatorUpdateMode DefaultAnimUpdateMode { get; set; }
        private int weaponAction = 0;             //Which type of action is making the active weapon

        [Tooltip("If the weapon is on the Idle Action it will be stored after X seconds")]
        public FloatReference StoreAfter = new FloatReference(0);

        #region Animator Hashs
        public string m_WeaponAim = "WeaponAim";
        public string m_WeaponType = "WeaponType";
        public string m_WeaponAction = "WeaponAction";
        public string m_WeaponCharge = "WeaponPower";
        public string m_WeaponHand = "WeaponHand";


        internal int Hash_WAim;
        internal int Hash_WType;
        internal int Hash_WAction;
        internal int Hash_WCharge;
        internal int Hash_WHand;
        #endregion

        #region Events
        public BoolEvent OnCombatMode = new BoolEvent();
        public GameObjectEvent OnEquipWeapon = new GameObjectEvent();
        public GameObjectEvent OnUnequipWeapon = new GameObjectEvent();
        public IntEvent OnWeaponAction = new IntEvent();
        public GameObjectEvent OnMainAttackStart = new GameObjectEvent();
        #endregion

        #region Properties

        /// <summary> If is false everything will be ignored </summary>
        public bool Active
        {
            get => enabled;
            set
            {
                if (!value)
                {
                    if (CombatMode) //Means it has a weapon equipped!!
                    {
                        Store_Weapon();
                    }
                    else
                        ResetCombat(); //If the Rider Combat was deactivated
                }
                enabled = value;
            }
        }

        /// <summary>  Same As Active  </summary>
        public void SetActive(bool value) => Active = value;

        /// <summary>is there an Ability Active and the Active Weapon is Active too</summary>
        public bool WeaponIsActive => (Weapon && Weapon.Active) && Active && !Paused && CheckRidingOnly;

        public bool CheckRidingOnly => !UseWeaponsOnlyWhileRiding || (Rider != null && Rider.IsRiding);

        public bool Paused => Time.timeScale == 0;

        //public MRider Rider { get; set; }

        public IAim Aimer { get; set; }

        /// <summary>Store the Default aiming Side</summary>
        public AimSide defaultAimSide { get; set; }

        //public Aim m_Aim { get; set; }

        public float DeltaTime { get; set; }

        private bool combatMode;
        /// <summary>Enable or Disable the Combat Mode (True When the rider has equipped a weapon)</summary>
        public bool CombatMode
        {
            get => combatMode;
            set
            {
                combatMode = value;
                OnCombatMode.Invoke(value);
            }
        }
        /// <summary>Which Action is currently using the RiderCombat. See WeaponActions Enum for more detail</summary>
        public int WeaponAction
        {
            get => weaponAction;
            set
            {
                weaponAction = value;
                if (debug) Debug.Log($"<B>{name}:<color=green> [Weapon Action] -> [<{WA.WValue(value)}><{value}>]</color></b>");
                SetIntParameter?.Invoke(Hash_WAction, (int)weaponAction);     //Set the WeaponAction in the Animator
                OnWeaponAction.Invoke(weaponAction);

                if (StoreAfter.Value > 0)
                {
                    if (IStoreAfter != null) StopCoroutine(IStoreAfter);
                    if (weaponAction == WA.Idle) IStoreAfter = StartCoroutine(C_StoreAfter());
                }

                if (weaponAction == WA.None) SendMessage(FreeBothHands);
            }
        }

         

        protected IEnumerator C_StoreAfter()
        {
            yield return StoreAfterTime;
            Store_Weapon();
        }


        private WaitForSeconds StoreAfterTime;
        Coroutine IStoreAfter;


        /// <summary> Weights for the IK Two Handed Weapon </summary>
        public float WeaponIKW { get; set; }

        public GameObject Owner => gameObject;

        private MWeapon m_weapon;
        /// <summary>Returns the IMWeapon Interface of the Active Weapon GameObject</summary>
        public MWeapon Weapon
        {
            get => m_weapon;
            set
            {
                if (value == null)          //No Weapon
                {
                    if (m_weapon != null) //If there was a weapon before then remove the Weapon 
                        SetWeapon(false);

                    m_weapon = value;
                }
                else                        //NEW WEAPON
                {
                    if (m_weapon != null) SetWeapon(false);

                    m_weapon = value;
                    SetWeapon(true);
                    SetWeaponHand();
                }
            }
        }

        /// <summary>Prepare a new and Old Weapon </summary>
        private void SetWeapon(bool new_Weapon)
        {
            if (new_Weapon)
            {
                m_weapon.WeaponAction += Action;
                m_weapon.OnCharged.AddListener(SetWeaponCharge);
            }
            else
            {
                m_weapon.WeaponAction -= Action;
                m_weapon.OnCharged.RemoveListener(SetWeaponCharge);
                m_weapon.IgnoreTransform = null;
            }
        }

        /// <summary>This will recieve the messages Animator Behaviors the moment the rider make an action on the weapon</summary>
        public virtual void Action(int value)
        {
            WeaponAction = value;
            if (Weapon && !Weapon.Active) Aim = (false); //Reset Aiming  if the weapon is Disable IMPORTANT
        }


        /// <summary>This will recieve the messages Animator Behaviors the moment the rider make an action on the weapon</summary>
        public virtual void CheckAim()
        {
            WeaponAction = Aim ? WA.Aim : CombatMode ? WA.Idle : WA.None;
        }

        public bool AimingSide => Aimer.AimingSide;

        private bool aim;
        /// <summary>Is the Rider Aiming?</summary>
        public bool Aim
        {
            private set
            {
                if (!WeaponIsActive) return; //Do nothing if the weapon is not active

                if (aim != value)
                {
                    aim = value;

                   if (Rider!= null) Rider.IsAiming = value; //Let know the Rider is Aiming. So if is using Straigth Spine, it stops.

                    if (debug) Debug.Log($"<B>{name}:<color=gray> [Aim -> {aim}]</color></b>");


                    if (aim)
                    {
                        Aimer.AimSide = Weapon.AimSide; //Send to the Aimer the Corret Side.
                        Aimer.Active = true; //Activate the Aimer Just In Case!!

                        if (Weapon is MShootable && !(Weapon as MShootable).HasAmmo) //If I have no Ammo Do not Aim
                        { return; }

                        if (!MTools.CompareOR(Mathf.Abs(WeaponAction), WA.Preparing, WA.Reload))
                            WeaponAction = WA.Aim;                    //If the weapon is Right Handed set the action to AimRight else AimLeft
                    }
                    else
                    {
                       ExitAim();

                       if (!MTools.CompareOR(Mathf.Abs(WeaponAction), WA.Fire_Projectile, WA.Reload))
                            WeaponAction = CombatMode ? WA.Idle : WA.None;
                    }

                    WeaponIKW = value ? 1 : 0;
                    
                    Weapon.IsAiming = value;    //Send to the active weapon  that the Rider is/isn't aiming
                }
            }
            get => aim;
        }

        public virtual void Aim_Set(bool value) => Aim = value;

        /// <summary> Returns the Normalized Angle Around the Y Axis (from -1 to 1) regarding the Target position</summary>
        public float HorizontalAngle { get; private set; }


        #region Bones References


        public Transform RightShoulder { get; set; }
        public Transform LeftShoulder { get; set; }
        public Transform RightHand { get; set; }
        public Transform LeftHand { get; set; }
        public Transform Head { get; set; }
        public Transform Chest { get; set; }
        #endregion

        /// <summary>Direction the Rider is Aiming</summary>
        public Vector3 AimDirection => Aimer.AimDirection;



        public LayerMask Layer { get => Aimer.Layer; set => Aimer.Layer = value; }
        public QueryTriggerInteraction TriggerInteraction { get => Aimer.TriggerInteraction; set => Aimer.TriggerInteraction = value; }



        public bool Weapon_is_RightHand => Weapon.IsRightHanded;
        public bool Weapon_is_LeftHand => !Weapon.IsRightHanded;


        private int weaponType;             //Which Type of weapon is in the active weapon
        /// <summary>Which Type of Weapon is in the Active Weapon, this value is sent to the animator</summary>
        public int WeaponType
        {
            get => weaponType;
            set
            {
                weaponType = value;
                SetIntParameter?.Invoke(Hash_WType, weaponType);          //Set the WeaponType in the Animator
            }
        }
        #endregion

        [HideInInspector] public int Editor_Tabs1;
        [HideInInspector] public int Editor_Tabs2;
    }
}