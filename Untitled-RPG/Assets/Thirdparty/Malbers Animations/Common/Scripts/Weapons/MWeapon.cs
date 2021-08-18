using UnityEngine;
using UnityEngine.Events; 
using MalbersAnimations.Scriptables;
using MalbersAnimations.Controller;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using System.Collections;
using System; 

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    [System.Serializable] public class WeaponEvent : UnityEvent<IMWeapon> { }

    public abstract class MWeapon : MDamager, IMWeapon,  IMDamager
    {
        [SerializeField] private FloatReference minDamage = new FloatReference(10);                       //Weapon minimum Damage
        [SerializeField] private FloatReference maxDamage = new FloatReference(20);                        //Weapon Max Damage

        [SerializeField] private Sprite m_UI;

        [SerializeField] private FloatReference minForce = new FloatReference(500);                        //Weapon min Force to push rigid bodies;

        #region Weapon Charge
        [SerializeField] private FloatReference chargeTime = new FloatReference(0);
        [SerializeField] private float chargeCharMultiplier = 1;
        public AnimationCurve ChargeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
        #endregion

        [SerializeField] private StringReference description = new StringReference(string.Empty);

        [SerializeField] private FloatReference m_rate = new FloatReference(0);                        //Weapon Rate
        
        [SerializeField] private BoolReference m_Automatic = new BoolReference(false);                   //Press Fire to Contiue Attacking
        [SerializeField] private BoolReference m_IgnoreDraw = new BoolReference(false);                   //Press Fire to Contiue Attacking


        /// <summary>Continue Attacking using the Rate of the Weapon </summary>
        public bool Automatic { get => m_Automatic.Value; set => m_Automatic.Value = value; }
        public bool IgnoreDraw { get => m_IgnoreDraw.Value; set => m_IgnoreDraw.Value = value; }
        public Sprite UISprite { get => m_UI; set => m_UI = value; }

        #region Aiming
        [SerializeField] private Transform m_AimOrigin;
        public virtual Transform AimOrigin { get => m_AimOrigin; set => m_AimOrigin = value; }
        public Vector3 AimPos => AimOrigin.position;

        [SerializeField] private AimSide m_AimSide;
        #endregion

        [SerializeField] protected WeaponID weaponType;
        [SerializeField] private HolsterID holster;          // From which Holder you will draw the Weapon
        [SerializeField] private HolsterID holsterAnim;      // From which Holder you will draw the Weapon

        public OldIKProfile IKProfile; 
        public IKProfile iKProfile; 

        public BoolReference rightHand = new BoolReference( true);                        // With which hand you will draw the Weapon;

        public Vector3 positionOffsetR;                       // Position Offset Right Hand
        public Vector3 rotationOffsetR;                       // Rotation Offset Right Hand

        public Vector3 positionOffsetL;                       // Position Offset Left hand
        public Vector3 rotationOffsetL;                       // Rotation Offset Left Hand

        public TransformOffset HolsterOffset =  new TransformOffset(1);


        //Two Handed Weapon IKProperties
        public Vector3 positionOffsetIKHand;                // Position Offset Left hand
        public Vector3 rotationOffsetIKHand;                // Rotation Offset Left Hand
        public bool TwoHandIK;                              // Makes the IK for the 2Hands
        public Transform IKHandPoint;                       // Rotation Offset Left Hand


        /// <summary> Weapon Sounds</summary>
        public AudioClip[] Sounds;                          //Sounds for the weapon
        public AudioSource WeaponSound;                     //Reference for the audio Source;

        private Rigidbody WeaponRB;                     //Reference for the audio Source;
        private Collider WeaponCol;                     //Reference for the audio Source;




        #region Properties

        /// <summary> Extra Transform to Ignore Damage</summary>
        public virtual Transform IgnoreTransform{ get ; set; }
        //{
        //    get => ignoreTransform;
        //    set
        //    {
        //        ignoreTransform = value;
        //        Debug.Log(ignoreTransform);
        //    }
        //}
        //private Transform ignoreTransform;



        /// <summary>Unique Weapon ID for each weapon</summary>
        public virtual int WeaponID => index;

        /// <summary>ID of the Damager (To be activated by the Animator)</summary>
        public override int Index => weaponType.ID;

        /// <summary>Holster the weapon can be draw from</summary> 
        public virtual int HolsterID => (Holster != null) ? Holster.ID : 0;

        public HolsterID Holster { get => holster; set => holster = value; }
        public int HolsterAnim => holsterAnim != null ? holsterAnim.ID : holster.ID;//  { get => holsterAnim; set => holsterAnim = value; }

        /// <summary>Send to the Weapon Owner that the weapon Action Changed</summary>
        public Action<int> WeaponAction { get; set; }


        private bool isEquiped = false;

        /// <summary> Is the Weapon Equiped </summary>
        public virtual bool IsEquiped
        {
            get => isEquiped;
            set
            {
                isEquiped = value;
                if (debug) Debug.Log($"{name}:<color=white> <b>[IsEquiped : {value}] </b> </color>");  //Debug

                if (isEquiped && Owner)
                {
                    OnEquiped.Invoke(Owner.transform);
                }
                else
                {
                    Owner = null;                       //Clean the Owner
                    OnUnequiped.Invoke(null);
                }
            }
        }

        /// <summary>Is the Weapon Charging?</summary>

        public virtual bool IsCharging { get; set; }

        /// <summary>Is the Weapon Reloading?</summary>
        public virtual bool IsReloading{ get; set; }

        private bool canAttack;

        /// <summary>Can the Weapon Attack?  Uses the Weapon Rate to evaluate if the weapon can Attack Again (Works for Melee and Shotable weapons)</summary>
        public virtual bool CanAttack
        {
            get => canAttack;
            set
            {
                canAttack = value;

                if (!canAttack)
                {
                    if (Rate > 0) StartCoroutine(DoAttackRate());
                    else canAttack = true; //Restore if the weapon has no Rate
                }
                //Debug.Log("Can Attack: " + canAttack);
            }
        }

        /// <summary>Is the Weapon Ready to Attack?? (Set by the Animations)</summary>
        public virtual bool IsReady { get; set; }


        protected void Debugging(string value)
        {
            if (debug) Debug.Log($"{name}:<color=white> <b>[{value}] </b></color>");  //Debug
        }

        public virtual void WeaponReady(bool value)
        {
            IsReady = value;
            
            if (IsReady)
            { WeaponAction?.Invoke(WA.Preparing); }

            Debugging($"Ready : {IsReady}"); 
            
        }

        public IEnumerator DoAttackRate()
        {
            yield return new WaitForSeconds(Rate);
            canAttack = true;
        }

        /// <summary>Is the Weapon Attacking... the Opposite of CanAttack</summary>
        public virtual bool IsAttacking { get => !CanAttack; }

        /// <summary>Main Attack Input Value. Also Means the Main Attack has Started</summary>
        public virtual bool MainInput  { get; set; }
        //{
        //    get => m_MainInput;
        //    set
        //    {
        //        m_MainInput = value;
        //        Debug.Log("MainInput: " + value);
        //    }
        //}
        //bool m_MainInput;

        /// <summary>Second Attack Input Value. Also Means the Secondary Attack has Started</summary>
        public virtual bool SecondInput { get; set; }

        public string Description { get => description.Value; set => description.Value = value; }


        private bool isAiming;
        /// <summary>Is the Weapon Aiming?</summary>
        public virtual bool IsAiming
        {
            get => isAiming;
            set
            {
                isAiming = value;
                OnAiming.Invoke(isAiming);
            }
        }


        /// <summary>Side of the Camera to use when using the Weapon</summary>
        public AimSide AimSide { get => m_AimSide; set => m_AimSide = value; }

        public float MinDamage { get => minDamage.Value; set => minDamage.Value = value; }
        public float MaxDamage { get => maxDamage.Value; set => maxDamage.Value = value; }

        /// <summary>Time needed to fully charge the weapon</summary>
        public float ChargeTime { get => chargeTime.Value; set => chargeTime.Value = value; }

        /// <summary>Can the Weapon be Charged?</summary>
        public bool CanCharge => ChargeTime > 0;

        /// <summary> Charge multiplier to Apply to the Character Charge Value (For the Animator Parameter)  </summary>
        public float ChargeCharMultiplier { get => chargeCharMultiplier; set => chargeCharMultiplier = value; }

        /// <summary>Elapsed Time since the Charge Weapon Started</summary>
        public float ChargeCurrentTime { get; set; }
        //{
        //    get => m_ChargeCurrentTime;
        //    set
        //    {
        //        m_ChargeCurrentTime = value;
        //        Debug.Log("m_ChargeCurrentTime: "+value);
        //    }
        //}
        //float m_ChargeCurrentTime;

         

        /// <summary>Is the weapon used on the Right hand(True) or left hand (False)</summary>
        public bool IsRightHanded => rightHand.Value;
        public bool IsLefttHanded => !IsRightHanded;

        public Vector3 PositionOffset => IsRightHanded ? positionOffsetR : positionOffsetL;

        public Vector3 RotationOffset => IsRightHanded ? rotationOffsetR : rotationOffsetL;

        /// <summary>Minimun Force the Weapon can do to a Rigid Body</summary>
        public float MinForce { get => minForce.Value; set => minForce.Value = value; }

        /// <summary>Maximun Force the Weapon can do to a Rigid Body</summary>
        public float MaxForce { get => m_Force.Value; set => m_Force.Value = value; }

        /// <summary>Weapon Rate</summary>
        public float Rate { get => m_rate.Value; set => m_rate.Value = value; }

        ///// <summary>Is the weapon fully charged?</summary>
        //public bool IsCharged => ChargeCurrentTime >= ChargeTime;

        /// <summary>Normalized Value for the Charge. if ChargeTime == 0 then Random Value between [0-1]</summary>
        public float ChargedNormalized => CanCharge ? ChargeCurve.Evaluate(Mathf.Clamp01(ChargeCurrentTime / ChargeTime)) : UnityEngine.Random.Range(0f, 1f);
        
        /// <summary> Normalized Value of the Charge </summary>
        public float Power => Mathf.Lerp(MinForce, MaxForce, ChargedNormalized);

        /// <summary>Enable or Disable the weapon to "block it"</summary>
        public override bool Active
        {
            get => enabled;
            set
            {
                m_Active.Value = enabled = value;
                if (!value && IsEquiped) WeaponAction?.Invoke(WA.Idle); //If the weapon is Disabled change the Weapon to Idle (if it Was Aiming or Shooting or Something like that
            }
        }

        public IMWeaponOwner WeaponOwner { get; set; }

        public WeaponID WeaponType => weaponType;

        #endregion

        #region Events
        public TransformEvent OnEquiped = new TransformEvent();
        public TransformEvent OnUnequiped = new TransformEvent();
        /// <summary>Invoked when the weapon is Charging  (Returns a Normalized Value) </summary>
        public FloatEvent OnCharged = new FloatEvent();
        public FloatEvent OnChargedFinished = new FloatEvent();
        public BoolEvent OnAiming = new BoolEvent();
       // public UnityEvent OnPlaced = new UnityEvent();
        #endregion


        /// <summary>Returns True if the Weapons has the same ID</summary>
        public override bool Equals(object a)
        {
            if (a is IMWeapon)
                return WeaponID == (a as IMWeapon).WeaponID;

            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        #region WeaponActions
        /// <summary>Set the Primary Attack </summary>
        internal virtual void MainAttack_Start(IMWeaponOwner RC)
        {
            MainInput = true;
            SecondInput = false;
            ResetCharge();
        }

        /// <summary>Set when the Current Attack is Active and Holding ... So reset the Attack</summary>
        internal virtual void Attack_Charge(IMWeaponOwner RC, float time)
        {
            if (Automatic && CanAttack && Rate > 0)
            {
                if (MainInput) MainAttack_Start(RC);  
                else if (SecondInput) SecondaryAttack_Start(RC);
            }
        }
           
        /// <summary>Set when the Primary Attack is Released (BOW) </summary>
        internal virtual void MainAttack_Released()
        {
            //Debugging($"Main Attack Released");
            MainInput = false;
            ResetCharge();
        }

        /// <summary>Set the Secondary Attack
        internal virtual void SecondaryAttack_Start(IMWeaponOwner RC)
        {
           // Debugging($"2nd Attack Started");
            SecondInput = true;
            MainInput = false;
            ResetCharge();
        }

        /// <summary>Set when the Secondary Attack is Released (BOW) </summary>
        internal virtual void SecondaryAttack_Released()
        {
           // Debugging($"2nd Attack Released");

            SecondInput = false;
            ResetCharge();
        }

        /// <summary> Reload Weapon </summary>
        internal virtual void Reload(IMWeaponOwner RC) { }

        /// <summary>Called on the Late Update of the Rider Combat Script </summary>
        internal virtual void LateUpdateWeaponIK(IMWeaponOwner RC)   { IKProfile?.LateUpdate_IK(RC); }

        /// <summary>Called on the Late Update of the Rider Combat Script </summary>
        internal virtual void LateWeaponModification(IMWeaponOwner RC) { }

        internal virtual void OnAnimatorWeaponIK(IMWeaponOwner RC) 
        {
            IKProfile?.OnAnimator_IK(RC);
            iKProfile?.ApplyOffsets(RC.Anim, RC.AimDirection, 1);
        }


        internal virtual void TwoWeaponIK(IMWeaponOwner RC)
        {
            if (TwoHandIK)
            {
                var ikGoal = !IsRightHanded ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;  //Set the IK goal acording the Right or Left Hand

                var Weight = 1;  

                RC.Anim.SetIKPosition(ikGoal, IKHandPoint.position);
                RC.Anim.SetIKPositionWeight(ikGoal, Weight);

                RC.Anim.SetIKRotation(ikGoal, IKHandPoint.rotation);
                RC.Anim.SetIKRotationWeight(ikGoal, Weight);
            }
        }


        /// <summary> Reload Weapon </summary>
        public virtual bool Reload() { return false; }

        #endregion


        #region ABILITY SYSTEM 
        /// <summary>Prepare weapon with all the necesary component to activate on the Weapons Owner (SAME AS START ABILITY)</summary>
        public virtual bool PrepareWeapon(IMWeaponOwner _char)
        {
            if (gameObject.IsPrefab()) return false; //Means is still a prefab

            WeaponOwner = _char;
            Owner = _char.Owner;
            IsEquiped = true;
            CanAttack = true;
            ChargeCurrentTime = 0;

            DisablePhysics();

            Debugging($"Prepared");

            return true;
        }


        /// <summary> Attack Trigger Behaviour </summary>
        public virtual void ActivateDamager(int value) { }

        /// <summary>Charge the Weapon using time.deltatime</summary>
        public virtual void Charge(float time)
        {
            if (CanCharge)
            {
                ChargeCurrentTime += time;
                IsCharging = true;
                OnCharged.Invoke(ChargedNormalized); //Charge Normalized
            }
            else
            {
                ReleaseCharge(); //Means the weapon does not need charging
            }
        }

        /// <summary>Reset the Charge of the weapon</summary>
        public virtual void ResetCharge()
        {
            if (CanCharge)
            {
                ChargeCurrentTime = 0;
                IsCharging = false;
                OnCharged.Invoke(0);

                Debugging($"Charge Reseted");
            }
        }

        /// <summary>Set when the Primary Attack is Released (BOW) </summary>
        public virtual void ReleaseCharge()
        {
            WeaponAction?.Invoke(WA.Release);
            ResetCharge();
        }
        #endregion

        /// <summary>Resets all the Weapon Properties</summary>
        public virtual void ResetWeapon()
        {
            Owner = null;
            WeaponOwner = null;
            IsEquiped = false;
            IsAiming = false;
            ResetCharge();

            Debugging($"Weapon Reseted");

        }

        public virtual void Initialize()
        {
            IsEquiped = false;

            if (!WeaponSound) WeaponSound = gameObject.FindComponent<AudioSource>(); //Gets the Weapon Source
            if (!WeaponSound) WeaponSound = gameObject.AddComponent<AudioSource>(); //Create an AudioSourse if theres no Audio Source on the weapon

            WeaponSound.spatialBlend = 1;

            WeaponRB = GetComponent<Rigidbody>(); //Gets the Weapon Rigid Body
            WeaponCol = GetComponent<Collider>(); //Gets the Weapon Collider
            
            if (holsterAnim == null) holsterAnim = holster;

            Find_Owner();
        }

        public void DisablePhysics()
        {
            if (WeaponRB)
            {
                WeaponRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                WeaponRB.isKinematic = true; //IMPORTANT
            }
                if (WeaponCol) WeaponCol.enabled = false;
        }

        /// CallBack from the RiderCombat Layer in the Animator to reproduce a sound on the weapon
        public virtual void PlaySound(int ID)
        {
            if (ID < Sounds.Length && Sounds[ID] != null)
            {
                var newSound = Sounds[ID];
                if (WeaponSound && !playingSound && gameObject.activeInHierarchy)
                {
                    StartCoroutine(DoubleShoot(newSound)); //HACK FOR THE SOUND
                }
            }
        }

        protected bool playingSound;
        protected IEnumerator DoubleShoot(AudioClip newSound)
        {
            playingSound = true;
            yield return null;
            yield return null;
            yield return null;
            WeaponSound.PlayOneShot(newSound);
            playingSound = false;
        }

        /// <summary>This is used to listen the Animator asociated to this gameObject </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            index = UnityEngine.Random.Range(100000, 999999);

            WeaponSound = GetComponent<AudioSource>(); //Gets the Weapon Source

            if (!WeaponSound) WeaponSound = gameObject.AddComponent<AudioSource>(); //Create an AudioSourse if theres no Audio Source on the weapon

            WeaponSound.spatialBlend = 1;

            holster = MTools.GetInstance<HolsterID>("Back Holster");
        }
#endif


        [HideInInspector] public int Editor_Tabs1;
        [HideInInspector] public int Editor_Tabs2;
    }


    #region INSPECTOR
#if UNITY_EDITOR
    public abstract class MWeaponEditor : MDamagerEd
    {
        protected string SoundHelp;

        protected SerializedProperty
            Sounds, WeaponSound, weaponType, rightHand, StatID, mod, ChargeTime, chargeCharMultiplier, MaxChargeDamage, m_AimOrigin, m_UI,
            m_AimSide, OnCharged, OnUnequiped, OnEquiped, /*OnPlaced, */minDamage, maxDamage, minForce, holster, holsterAnim, IKProfile, iKProfile, Rate, TwoHandIK, IKHandPoint,
            rotationOffsetIKHand, positionOffsetIKHand, OnAiming, m_Automatic, ChargeCurve, HolsterOffset,
            description, Editor_Tabs2, Editor_Tabs1, rotationOffsetR, positionOffsetR, rotationOffsetL, positionOffsetL, m_IgnoreDraw; 
       
        bool offsets = true;

        protected string WeaponTab = "Weapon";

        protected string[] Tabs1 = new string[] { "General", "Damage", "IK", "Extras" };
        protected string[] Tabs2 = new string[] { "Weapon", "Sounds", "Events" };
        protected MWeapon mWeapon;

        protected virtual void SetOnEnable()
        {
            mWeapon = (MWeapon)target;
            FindBaseProperties();

            Tabs2[0] = WeaponTab;

            Sounds = serializedObject.FindProperty("Sounds");
            m_UI = serializedObject.FindProperty("m_UI");
            WeaponSound = serializedObject.FindProperty("WeaponSound");
            weaponType = serializedObject.FindProperty("weaponType");
            HolsterOffset = serializedObject.FindProperty("HolsterOffset");


            description = serializedObject.FindProperty("description");
            m_Automatic = serializedObject.FindProperty("m_Automatic");
            m_AimOrigin = serializedObject.FindProperty("m_AimOrigin");
            chargeCharMultiplier = serializedObject.FindProperty("chargeCharMultiplier");

            rightHand = serializedObject.FindProperty("rightHand");
            minDamage = serializedObject.FindProperty("minDamage");
            maxDamage = serializedObject.FindProperty("maxDamage");
            minForce = serializedObject.FindProperty("minForce");
            m_IgnoreDraw = serializedObject.FindProperty("m_IgnoreDraw");

            IKProfile = serializedObject.FindProperty("IKProfile");
            iKProfile = serializedObject.FindProperty("iKProfile");


            OnCharged = serializedObject.FindProperty("OnCharged");
            OnUnequiped = serializedObject.FindProperty("OnUnequiped");
            OnEquiped = serializedObject.FindProperty("OnEquiped");
            OnAiming = serializedObject.FindProperty("OnAiming");
           // OnPlaced = serializedObject.FindProperty("OnPlaced");

            holster = serializedObject.FindProperty("holster");
            holsterAnim = serializedObject.FindProperty("holsterAnim");

            rotationOffsetR = serializedObject.FindProperty("rotationOffsetR");
            rotationOffsetL = serializedObject.FindProperty("rotationOffsetL");
            positionOffsetR = serializedObject.FindProperty("positionOffsetR");
            positionOffsetL = serializedObject.FindProperty("positionOffsetL");

            ChargeTime = serializedObject.FindProperty("chargeTime");
            MaxChargeDamage = serializedObject.FindProperty("MaxChargeDamage");
            ChargeCurve = serializedObject.FindProperty("ChargeCurve");


            m_AimSide = serializedObject.FindProperty("m_AimSide");
            Rate = serializedObject.FindProperty("m_rate");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");

            StatID = statModifier.FindPropertyRelative("ID");
            mod = statModifier.FindPropertyRelative("modify");


            TwoHandIK = serializedObject.FindProperty("TwoHandIK");
            IKHandPoint = serializedObject.FindProperty("IKHandPoint");
            rotationOffsetIKHand = serializedObject.FindProperty("rotationOffsetIKHand");
            positionOffsetIKHand = serializedObject.FindProperty("positionOffsetIKHand");
        }

        protected  virtual void WeaponInspector(bool showAim = true)
        {

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
            if (Editor_Tabs1.intValue != Tabs1.Length) Editor_Tabs2.intValue = Tabs2.Length;

            Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, Tabs2);
            if (Editor_Tabs2.intValue != Tabs2.Length) Editor_Tabs1.intValue = Tabs1.Length;


            //First Tabs
            int Selection = Editor_Tabs1.intValue;
            if (Selection == 0) DrawWeapon(showAim);
            else if (Selection == 1) DrawDamage();
            else if (Selection == 2) DrawIKWeapon();
            else if (Selection == 3) DrawExtras();


            //2nd Tabs
            Selection = Editor_Tabs2.intValue;
            if (Selection == 0) DrawAdvancedWeapon();
            else if (Selection == 1) DrawSound();
            else if (Selection == 2) DrawEvents();  

        }

        protected virtual void DrawExtras()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Physics Force", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 50;
                    EditorGUILayout.PropertyField(minForce, new GUIContent("Min", "Minimun Force"));
                    EditorGUILayout.PropertyField(Force, new GUIContent("Max", "Maximun Force"));
                    EditorGUIUtility.labelWidth = 0;

                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(forceMode);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Misc", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(interact);
                EditorGUILayout.PropertyField(react);
                EditorGUILayout.PropertyField(pureDamage);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.EndVertical();

        }

        protected virtual void DrawDamage()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Layer Interaction", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(hitLayer);
                EditorGUILayout.PropertyField(triggerInteraction);
             
                EditorGUILayout.PropertyField(dontHitOwner, new GUIContent("Don't hit Owner"));
                if (dontHitOwner.FindPropertyRelative("ConstantValue").boolValue) EditorGUILayout.PropertyField(owner);
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(Rate, new GUIContent("Rate", "Time(Delay) between attacks"));
                EditorGUILayout.PropertyField(m_Automatic, new GUIContent("Automatic", "Continues Attacking if the Main Attack Input is pressed"));
                EditorGUILayout.PropertyField(ChargeTime, new GUIContent("Charge Time", "Weapons can be Charged|Hold before releasing the Attack."));

                if (mWeapon.ChargeTime > 0)
                {
                    EditorGUILayout.PropertyField(chargeCharMultiplier, new GUIContent("Charge Char Mult", "Charge multiplier to Apply to the Character Charge Value (For the Animator Parameter) "));
                    EditorGUILayout.PropertyField(ChargeCurve, new GUIContent("Curve", "Evaluation of the Charge in a Curve"));
                }
                else 
                    EditorGUILayout.HelpBox("When [Charge Time] is 0 the 'Charge Weapon' logic will be ignored", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();

            DrawCriticalDamage();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Modify Stat", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 50;
                    EditorGUILayout.PropertyField(StatID, new GUIContent("Stat"));
                    EditorGUILayout.PropertyField(mod, GUIContent.none, GUILayout.MinWidth(50), GUILayout.MaxWidth(120));
                    EditorGUIUtility.labelWidth = 0;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 50;
                    EditorGUILayout.PropertyField(minDamage, new GUIContent("Min", "Minimun Damage"));
                    EditorGUILayout.PropertyField(maxDamage, new GUIContent("Max", "Minimun Damage"));
                    EditorGUIUtility.labelWidth = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawWeapon(bool showAim = true)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_Active);
                MalbersEditor.DrawDebugIcon(debug);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(index, new GUIContent("Index", "Unique Weapon ID for each weapon"), GUILayout.MinWidth(1));

                    if (GUILayout.Button("Generate", EditorStyles.miniButton, GUILayout.MaxWidth(70)))
                        index.intValue = UnityEngine.Random.Range(100000, 999999);

                }
                EditorGUILayout.EndHorizontal();
                CheckWeaponID();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(weaponType, new GUIContent("Type", "Gets the Weapon Type ID, Used on the Animator to Play the Matching animation for the weapon"));
                EditorGUILayout.PropertyField(holster, new GUIContent("Holster", "The Side where the weapon is Draw/Store from"));
                EditorGUILayout.PropertyField(holsterAnim, new GUIContent("Holster Anim?", "Use Diferent Animation for Draw/Store"));
                //EditorGUILayout.PropertyField(m_UI, new GUIContent("UI", "Sprite to be represented on the UI"));
                EditorGUILayout.PropertyField(m_IgnoreDraw, new GUIContent("Ignore Draw", "Ignores Draw and Store Animations when equipping a weapon"));
            }
            EditorGUILayout.EndVertical();

            if (showAim)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(m_AimSide, new GUIContent("Aim Side", "Side of the Character the Weapon will aim when Aim is true"));
                    EditorGUILayout.PropertyField(m_AimOrigin, new GUIContent("Aim Origin", "Point where the Aiming will be Calculated.\nAlso for Shootable weapons the point where the Projectiles will come out"));
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(rightHand);
                EditorGUILayout.HelpBox("The Weapon is " + (mWeapon.IsRightHanded ? "Right Handed" : "Left Handed"), MessageType.Info);
            }
            EditorGUILayout.EndVertical();



            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUI.indentLevel++;
                offsets = EditorGUILayout.Foldout(offsets, "Offset " + (mWeapon.IsRightHanded? "Right Hand" : "Left Hand"));
                EditorGUI.indentLevel--;

                if (offsets)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(mWeapon.IsRightHanded ? positionOffsetR : positionOffsetL, new GUIContent("Position"));
                    EditorGUILayout.PropertyField(mWeapon.IsRightHanded ? rotationOffsetR : rotationOffsetL, new GUIContent("Rotation"));
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;

                }
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(HolsterOffset, true);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            //EditorGUILayout.PropertyField(debug);
        }


        protected virtual void DrawIKWeapon()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("On Animator IK", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(IKProfile, new GUIContent("IK Profile (Old)", "IK Modification to the Character Body to Aim Properly"));
                EditorGUILayout.PropertyField(iKProfile, new GUIContent("IK Profile", "IK Modification to the Character Body to Aim Properly"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Two Handed Weapon", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(TwoHandIK);

                if (TwoHandIK.boolValue)
                {
                    EditorGUILayout.LabelField("(The " + (mWeapon.IsRightHanded ? "Left Hand" : "Right Hand") + " is the auxiliar Hand)");

                    EditorGUILayout.PropertyField(IKHandPoint);
                    EditorGUILayout.PropertyField(positionOffsetIKHand);
                    EditorGUILayout.PropertyField(rotationOffsetIKHand);
                }
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawAdvancedWeapon()  { }

        protected virtual void DrawSound()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
           // EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(WeaponSound, new GUIContent("Weapon Source", "Audio Source for the wapons"));
            EditorGUI.indentLevel++;
            UpdateSoundHelp();
            EditorGUILayout.PropertyField(Sounds, new GUIContent("Sounds", "Sounds Played by the weapon"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.HelpBox(SoundHelp, MessageType.None);
            EditorGUILayout.EndVertical();
        }

        protected override void DrawCustomEvents()
        {
           // EditorGUILayout.PropertyField(OnPlaced, new GUIContent("On Placed [In Holster or Invectory]    "));
            EditorGUILayout.PropertyField(OnEquiped, new GUIContent("On Equiped"));
            EditorGUILayout.PropertyField(OnUnequiped, new GUIContent("On Unequiped"));
            EditorGUILayout.PropertyField(OnCharged, new GUIContent("On Charged Weapon"));
            EditorGUILayout.PropertyField(OnAiming, new GUIContent("On Aiming"));
            ChildWeaponEvents();
        }

        protected virtual string CustomEventsHelp() { return ""; }
        protected virtual void ChildWeaponEvents() { }
        protected virtual void UpdateSoundHelp() { }

        protected void CheckWeaponID()
        {
            if (index.intValue == 0)
                EditorGUILayout.HelpBox("Weapon ID needs cant be Zero, Please Set an ID number ", MessageType.Warning);
        }
    }
#endif

    #endregion
}