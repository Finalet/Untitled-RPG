using UnityEngine;
using MalbersAnimations.Weapons;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;

namespace MalbersAnimations.HAP
{
    /// <summary>Variables and Properties</summary>
    public partial class RiderCombat
    {
        #region Public Entries
        public bool UseInventory = true;                                //Use this if the weapons are on an Inventory
        public bool AlreadyInstantiated = true;                         //If the weapons comes from an inventory check if they are already intantiate
        public bool UseHolders = false;                                 //Use this if the weapons are on the Holders


        public bool StrafeOnTarget;                                     //If there's a target Strafe if this is set to true
        private bool DefaultMonturaCamBase;                                      //For storing the Montura Input Type


        public bool ForceNormalUpdate = true;                           //If there's a target Strafe if this is set to true
        public bool ForceMountNormalUpdate = true;                           //If there's a target Strafe if this is set to true
        public bool ExitCombatOnDismount = true;                        //Exit the Combat Mode on Dismount

        [SerializeField] private WeaponHolder activeHolderSide = WeaponHolder.Back;       //Start with Back Holder as default  

        public Transform
                LeftHandEquipPoint,                                      //Equip Point for the Left Hand    
                RightHandEquipPoint,                                     //Equip Point for the Right Hand    
                HolderLeft,                                              //Transform where the Left weapons are going to be
                HolderRight,                                             //Transform where the Left weapons are going to be
                HolderBack,                                              //Transform where the Left weapons are going to be
                ActiveHolderTransform;                                   //Active Transform holder to draw weapons from

        
        /// <summary>Path of the Combat Layer on the Resource Folder </summary>
        public string CombatLayerPath = "Layers/Combat";
        /// <summary>Name of the Combat Layer </summary>
        public string CombatLayerName = "Rider Combat";

        /// <summary>Combat Abilities availables for the rider </summary>
        public List<RiderCombatAbility> CombatAbilities;

        /// <summary>Clone the Scriptable Assets of theCombat Abilities on start... (use this if you have more than one Rider</summary>
        public bool CloneAbilities = true;

        public bool debug;                                              
        #endregion

        /// <summary>Active Ability when a weapon is Equiped</summary>
        public RiderCombatAbility ActiveAbility { get; set; }

        /// <summary>Reference for the Animator component</summary>
        public Animator Anim { get; set; }
        /// <summary>Reference for the Animator Update Mode</summary>
        public AnimatorUpdateMode DefaultAnimUpdateMode { get; set; }


        private GameObject activeWeaponGO;
        private WeaponID weaponType;                        //Which Type of weapon is in the active weapon
        private WA weaponAction = WA.None;             //Which type of action is making the active weapon

        protected int Layer_RiderArmRight;                               //Right Arm Layer    
        protected int Layer_RiderArmLeft;                                //Left  Arm Layer
        protected int Layer_RiderCombat;                                 //Combat Layer

     
        /// <summary>Catche the Transforms</summary>
        internal Transform _t;

        #region AIM Variables
        public IAim Aimer;
        private float horizontalAngle;                                  //Value for the Aim Sides for the animator

       // protected RaycastHit aimRayHit;                                 //RayCastHit to Store all the information of the Aim Ray
        #endregion

        #region Animator Hashs

        public string m_WeaponAim = "WeaponAim";
        public string m_WeaponType = "WeaponType";
        public string m_WeaponHolder = "WeaponHolder";
        public string m_WeaponAction = "WeaponAction";
        public string m_WeaponHold = "WeaponHold";


        internal int Hash_WAim;
        internal int Hash_WType;
        internal int Hash_WHolder;
        internal int Hash_WAction;
        internal int Hash_WHold;
        #endregion

        #region Events
        public BoolEvent OnCombatMode = new BoolEvent();
        public GameObjectEvent OnEquipWeapon = new GameObjectEvent();
        public GameObjectEvent OnUnequipWeapon = new GameObjectEvent();
        public WeaponActionEvent OnWeaponAction = new WeaponActionEvent();
        public WeaponEvent OnAttack = new WeaponEvent();
        public BoolEvent OnAiming = new BoolEvent();
        public IntEvent OnAimSide = new IntEvent();
      //  public TransformEvent OnTarget = new TransformEvent();
        #endregion

        protected Transform
                 mountPoint;                                             //Reference for the Moint Point;  



        #region Properties

        /// <summary> If is false everything will be ignored </summary>
        public BoolReference active = new BoolReference(true);

        /// <summary> If is false everything will be ignored </summary>
        /// <summary> If is false everything will be ignored </summary>
        public bool Active
        {
            get { return active.Value; }
            set
            {
                active.Value = value;

                if (ActiveWeapon != null && !value) //If the Rider Combat was deactivated
                {
                    ActiveWeapon.MainAttack = false;
                    ActiveWeapon.SecondAttack = false;
                    Aim = false; //Reset the Aiming
                }
            }
        }

        public MRider Rider { get; set; }
        public float DeltaTime { get; set; }


        

        private bool combatMode;
        /// <summary>Enable or Disable the Combat Mode (True When the rider has equipped a weapon)</summary>
        public bool CombatMode
        {
            get { return combatMode; }
            set
            {
                combatMode = value;

                OnCombatMode.Invoke(value);
            }
        }
        /// <summary>Which Action is currently using the RiderCombat. See WeaponActions Enum for more detail</summary>
        public WA WeaponAction
        {
            get { return weaponAction; }
            set
            {
                weaponAction = value;
                Anim.SetInteger(Hash_WAction, (int)weaponAction);     //Set the WeaponAction in the Animator
                OnWeaponAction.Invoke(weaponAction);
            }
        }

        /// <summary>Returns the IMWeapon Interface of the Active Weapon GameObject</summary>
        public IMWeapon ActiveWeapon { get; set; }

        /// <summary> True if the Weapon can attack when on an Action like Idle, Hold, Aim Left or Aim Right</summary>
        public bool WeaponCanAttack => (WeaponAction == WA.Idle || WeaponAction == WA.Hold || WeaponAction == WA.AimLeft || WeaponAction == WA.AimRight);


        public bool TargetSide { get; private set; }
        private bool cameraSide;
        /// <summary>Which side is the rider Regarding the camera or the target Location (Right:true, Left:False)</summary>
        public bool CameraSide
        {
            get { return cameraSide; }
            private set
            {
                if (value != cameraSide)
                {
                    cameraSide = value;

                    if (Aim) UpdateCameraAimSide();
                }
            }
        }

        public BoolReference toggleAim;
        /// <summary>Set the Aim to Toggle</summary>
        public bool ToggleAim
        {
            get { return toggleAim; }
            set { toggleAim.Value = value; }
        }

        private bool aim;
        /// <summary>Is the Rider Aiming?</summary>
        public bool Aim
        {
           private set
            {
                if (aim != value)
                {
                    aim = value;
                    Rider.CombatAim = value; //IMPORTANT
                  //  Aimer.Active = value;

                    OnAiming.Invoke(value);             //Invoke on Aiming

                    if (ActiveAbility)  ActiveAbility.OnWeaponAim(aim); //Send to the Ability that the Rider is/isn't aiming

                    Aimer.AimOrigin = aim ?  ActiveAbility.AimRayOrigin() : null; //IMPORTANT Set the Aim Origin

                    if (aim)
                    {
                        UpdateCameraAimSide();
                        Rider.Montura.PauseStraightSpine(true);                 //Pause Straight spine in case is was activate it
                       
                        Aimer.IgnoreTransform = Rider.Montura.transform;        //Set the Horse to be ignored by the Aim Logic
                    }
                    else
                    {
                        OnAimSide.Invoke(0);                                    //Send that is not aiming anymore
                        WeaponAction = CombatMode ? WA.Idle : WA.None;
                        Rider.Montura.PauseStraightSpine(false);                //UnPause Straight spine in case is was activate it
                        Aimer.IgnoreTransform = null;
                    }

                    ForceAnimatorUpdateMode(value);
                }
            }
            get { return aim; }
        }


        /// <summary>Which side is the rider regarding the camera or the target Location (Right:true, Left:False)</summary>
        public bool IsCamRightSide { get { return CameraSide; } }
        


        /// <summary> Returns the Normalized Angle Around the Y Axis (from -1 to 1) regarding the Target position</summary>
        public float HorizontalAngle { get { return horizontalAngle; } }
       

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

        ///// <summary>Position of where the Aiming Starts</summary>
        //public Transform AimOrigin { get; set; }
         
        ///// <summary>Direct Point the Rider is Aiming</summary>
        //public Vector3 AimPoint { get; set; }

        ///// <summary>Information stored by the Aim Mode </summary>
        //public RaycastHit AimRay
        //{ get { return aimRayHit; } }

        /// <summary>Reference for the Active Weapon Game Object</summary>
        public GameObject ActiveWeaponGameObject  
        {
            get {return activeWeaponGO;}
            set
            {
                activeWeaponGO = value;
                ActiveWeapon = value ? value.GetComponent<IMWeapon>() : null; //Set the Interface reference for the Active Weapon
            }
        }

        public WeaponHolder ActiveHolderSide
        {
            get { return activeHolderSide; }

            set
            {
                activeHolderSide = value;
                Anim.SetInteger(Hash_WHolder, (int)activeHolderSide);  //Set the ActiveHolder in the Animator
            }
        }

        public bool Weapon_is_RightHand
        { get { return ActiveWeapon.RightHand; } }

        public bool Weapon_is_LeftHand
        { get { return !ActiveWeapon.RightHand; } }

        /// <summary>Which Type of Weapon is in the Active Weapon, this value is sent to the animator</summary>
        protected WeaponID WeaponType
        {
            get { return weaponType; }
            set
            {
                weaponType = value;
                Anim.SetInteger(Hash_WType, weaponType ?? 0);          //Set the WeaponType in the Animator
            }
        }
        #endregion

        ///Editor Variables
        [HideInInspector] public bool Editor_ShowAbilities = false;
        [HideInInspector] public int Editor_Tabs1;
        [HideInInspector] public int Editor_Tabs2;
    }
}