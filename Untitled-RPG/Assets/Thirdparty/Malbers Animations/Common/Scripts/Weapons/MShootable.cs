using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;
using System.Collections;
using MalbersAnimations.Scriptables;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/Shootable")]
    public class MShootable : MWeapon, IShootableWeapon, IThrower
    {
        #region Variables
        public enum Release_Projectile { Never, OnAttackStart, OnAttackReleased, ByAnimation }
        public enum Cancel_Aim { ReleaseProjectile, ResetWeapon}
        ///<summary> Does not shoot projectile when is false, useful for other controllers like Invector and ootii to let them shoot the arrow themselves </summary>
        [Tooltip("When the Projectile is Release?")]
        public Release_Projectile releaseProjectile = Release_Projectile.OnAttackStart;

        ///<summary> When Aiming is Cancel what the Weapon should do? </summary>
        [Tooltip("When the Projectile is Release?")]
        public Cancel_Aim CancelAim = Cancel_Aim.ResetWeapon;

        [Tooltip("Projectile prefab the weapon fires")]
        public GameObjectReference m_Projectile = new GameObjectReference();                                //Arrow to Release Prefab
        [Tooltip("Parent of the Projectile")]
        public Transform m_ProjectileParent;
        public Vector3Reference gravity = new Vector3Reference(Physics.gravity);

        public BoolReference UseAimAngle = new BoolReference(false);
        public BoolReference HasReloadAnim = new BoolReference(false);
        public FloatReference m_AimAngle = new FloatReference(0);

        /// <summary>This Curve is for Limiting the Bow Animations while the Character is on weird/hard Positions</summary>
        public AnimationCurve AimLimit = new AnimationCurve(new Keyframe[] { new Keyframe(-1, 1), new Keyframe(-0.5f, 1), new Keyframe(0f, 1), new Keyframe(0.5f, 1), new Keyframe(1f, 1) });

        

        [SerializeField] private IntReference m_Ammo = new IntReference(30);                             //Total of Ammo for this weapon
        [SerializeField] private IntReference m_AmmoInChamber = new IntReference(1);                     //Total of Ammo in the Chamber
        [SerializeField] private IntReference m_ChamberSize = new IntReference(1);                       //Max Capacity of the Ammo in once hit
        [SerializeField] private BoolReference m_AutoReload = new BoolReference(false);                  //Press Fire one or continues 

        #endregion

        #region Events
        public GameObjectEvent OnLoadProjectile = new GameObjectEvent();
        public GameObjectEvent OnFireProjectile = new GameObjectEvent();
        public UnityEvent OnReload = new UnityEvent();
        #endregion

        #region Properties
        public GameObject Projectile { get => m_Projectile.Value; set => m_Projectile.Value = value; }

        /// <summary> Projectile Instance to launch from the weapon</summary>
        public GameObject ProjectileInstance { get; set; }
        public Transform ProjectileParent => m_ProjectileParent;

        public bool InstantiateProjectileOfFire = true;

        public Vector3 Gravity { get => gravity.Value; set => gravity.Value = value; }

        /// <summary> Adds a Throw Angle to the Aimer Direction </summary>
        public float AimAngle { get => m_AimAngle.Value; set => m_AimAngle.Value = value; }
        public Vector3 Velocity { get; set; }
        public Action<bool> Predict { get; set; }

        /// <summary> Total Ammo of the Weapon</summary>
        public int TotalAmmo { get => m_Ammo.Value; set => m_Ammo.Value = value; }

        public int AmmoInChamber { get => m_AmmoInChamber.Value; set => m_AmmoInChamber.Value = value; }

        /// <summary>When the Ammo in Chamber gets to Zero it will reload Automatically</summary>
        public bool AutoReload { get => m_AutoReload.Value; set => m_AutoReload.Value = value; }

        public int ChamberSize { get => m_ChamberSize.Value; set => m_ChamberSize.Value = value; }

        /// <summary>Does the weapon has Ammo in the Chamber?</summary>
        public bool HasAmmo => AmmoInChamber > 0;

        /// <summary>Does the weapon has Ammo in the Chamber?</summary>
        public float AimWeight { get; private set; }

        /// <summary>With Aim Limit?</summary>
        public bool CanShootWithLimit { get; private set; }

        
        public override bool IsEquiped
        {
            get => base.IsEquiped;
            set
            {
                base.IsEquiped = value;
                if (!value)
                    DestroyProjectileInstance(); //If by AnyChange the Projectile is live Destroy it!!
            }
        }

        public override bool IsAiming
        {
            get => base.IsAiming;
            set
            {
                base.IsAiming = value;

                if (!value)
                {
                    if (CancelAim == Cancel_Aim.ReleaseProjectile) //if the weapon is set to Cancel the Aim
                    {
                        MainAttack_Released();
                    }
                    else
                    {
                        ResetCharge(); //Reset the Charge of the wapon
                    }
                }
            }
        }


        #endregion

        /// <summary>  Set the total Ammo (Refill when you got some ammo)  </summary>
        public void SetTotalAmmo(int value)
        {
            TotalAmmo = value;
            if (AutoReload) Reload();
        }


        public override void WeaponReady(bool value)
        {
            IsReady = value;

            if (IsReady && IsAiming)
            {
                WeaponAction?.Invoke(WA.Preparing);
            }

            if (debug) Debug.Log($"{name}:<color=white> <b>[Ready : {IsReady}] </b>   </color>");  //Debug
        }



        void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (!m_Ammo.UseConstant && m_Ammo.Variable != null) //Listen the Total ammo in case it changes
                m_Ammo.Variable.OnValueChanged += SetTotalAmmo;
        }

        private void OnDisable()
        {
            if (!m_Ammo.UseConstant && m_Ammo.Variable != null)
                m_Ammo.Variable.OnValueChanged -= SetTotalAmmo;
        }

        internal override void OnAnimatorWeaponIK(IMWeaponOwner RC)
        {
            CalculateAimLimit(RC);                      //Get te Aim Limit from the Aimer of the Character.
            IKProfile?.OnAnimator_IK(RC);
            iKProfile?.ApplyOffsets(RC.Anim, RC.AimDirection, AimWeight);
        }

        internal override void MainAttack_Start(IMWeaponOwner RC)
        {
            base.MainAttack_Start(RC);

            Hack1ChamberAmmo();

            if (IsAiming && CanAttack && IsReady) //and the Rider is not on any reload animation
            {
                if (HasAmmo)                                                                  //If there's Ammo on the chamber
                {
                    if (!CanCharge) //Means the Weapon does not need to Charge  so Release the Projectile First!
                    {
                        CalculateAimLimit(RC);

                        if (CanShootWithLimit)
                        {
                            if (debug) Debug.Log($"{name}:<color=white> <b>[Fire Projectile No Charge] </b>   </color>");  //Debug
                            WeaponAction.Invoke(WA.Fire_Projectile);

                            if (releaseProjectile == Release_Projectile.OnAttackStart)
                                ReleaseProjectile();
                        }
                    }
                }
                else
                {
                    PlaySound(WSound.Empty);                   //Play Empty Sound Which is stored in the 4 Slot  
                    if (debug) Debug.Log($"{name}:<color=white> <b>[Empty Ammo] </b>   </color>");  //Debug
                }

                CanAttack = false;  //Calcualte the Rate Fire of the arm
            }
        }

        private void Hack1ChamberAmmo()
        {
            if (!HasAmmo && TotalAmmo > 0 && ChamberSize == 1 && AutoReload)
            {
                AmmoInChamber = 1; //HACK for 1 Chamber Size Weapon
                if (debug) Debug.Log($"{name}:<color=white> <b>[HACK for the BOW ARROWS] </b>   </color>");  //Debug
            }
        }

        internal override void MainAttack_Released()
        {
            MainInput = false;


            if (IsReady && HasAmmo && CanCharge && IsCharging && CanShootWithLimit)   //If we are not firing any arrow then try to Attack with the bow
            {
                WeaponAction?.Invoke(WA.Fire_Projectile);    //Play the Fire Animation on the CHaracter

                if (releaseProjectile == Release_Projectile.OnAttackReleased)
                    ReleaseProjectile();
            }
        }

        internal override void SecondaryAttack_Start(IMWeaponOwner RC)
        {/*NO WEAPON*/ }
        internal override void SecondaryAttack_Released()
        {/*NO WEAPON*/ }


        public virtual void ReduceAmmo(int amount)
        {
            AmmoInChamber -= amount;
            if (debug) Debug.Log($"{name}:<color=white> <b>[Ammo Reduced({amount})][Total Ammo ({TotalAmmo})][Ammo in Chamber({AmmoInChamber})]</b>   </color>");  //Debug
            if (AmmoInChamber <= 0 && AutoReload) Reload();
        }


        /// <summary> Charge the Weapon!! </summary>
        internal override void Attack_Charge(IMWeaponOwner RC, float time)
        {
            if (MainInput) //The Input For Charging is Down
            {
                if (Automatic && CanAttack && Rate > 0) //If is automatic then continue attacking
                {
                    MainAttack_Start(RC);
                    if (debug) Debug.Log($"{name}:<color=white> <b>[Charge Started] </b> [{ProjectileInstance}]  </color>");  //Debug
                }

                if (IsReady && HasAmmo && CanCharge)  //Is the Weapon ready?? we Have projectiles and we can Charge
                {
                    CalculateAimLimit(RC);

                    if (!CanShootWithLimit)
                    {
                        ResetCharge();
                        return;
                    }

                    if (!IsCharging && IsAiming)            //If Attack is pressed Start Bending for more Strength the Bow
                    {
                        //WeaponAction?.Invoke(WA.Preparing);
                        IsCharging = true;
                        ChargeCurrentTime = 0;
                        Predict?.Invoke(true);

                        PlaySound(WSound.Charge); //Play the Charge Sound

                        if (debug) Debug.Log($"{name}:<color=white> <b>[Charge Started] </b></color>");  //Debug
                    }
                    else             // //If Attack is pressed Continue Bending the Bow for more Strength the Bow
                    {
                        Charge(time);
                    }
                }
            }
        }

        private void CalculateAimLimit(IMWeaponOwner RC)
        {
            AimWeight =
            IsRightHanded ? AimLimit.Evaluate(RC.HorizontalAngle) : AimLimit.Evaluate(-RC.HorizontalAngle);       //The Weight evaluated on the AnimCurve
            CanShootWithLimit = (AimWeight == 1);     //Calculate the Imposible range to shoot
        }


        public override void ResetCharge()
        {
            base.ResetCharge();
            Predict?.Invoke(false);
            Velocity = Vector3.zero; //Reset Velocity
        }

        public override void Charge(float time)
        {
            base.Charge(time);
            CalculateVelocity();
            //Predict?.Invoke(true);
        }


        /// <summary> Create an arrow ready to shooot CALLED BY THE ANIMATOR </summary>
        public virtual void EquipProjectile()
        {
            if (!HasAmmo) return;                                           //means there's no Ammo

            if (ProjectileInstance == null)
            {
                var Pos = ProjectileParent ? ProjectileParent.position : AimPos;
                var Rot = ProjectileParent ? ProjectileParent.rotation : AimOrigin.rotation;
                ProjectileInstance = Instantiate(Projectile, Pos, Rot, ProjectileParent);                  //Instantiate the Arrow in the Knot of the Bow
            }

            var projectile = ProjectileInstance.GetComponent<IProjectile>();                               //Get the IArrow Component

            var ProjectileRB = ProjectileInstance.GetComponent<Rigidbody>(); //IMPORTANT

            if (ProjectileRB)
            {
                ProjectileRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                ProjectileRB.isKinematic = true;
            }

            var ProjectileCol = ProjectileInstance.GetComponent<Collider>(); //IMPORTANT
            if (ProjectileCol)
            {
                ProjectileCol.enabled = false;
            }

            if (projectile != null)
            {
                ProjectileInstance.transform.Translate(projectile.PosOffset, Space.Self);   //Translate in the offset of the arrow to put it on the hand
                ProjectileInstance.transform.Rotate(projectile.RotOffset, Space.Self);      //Rotate in the offset of the arrow to put it on the hand
               // ProjectileInstance.transform.localScale = (projectile.ScaleOffset);         //Scale in the offset of the arrow to put it on the hand
            }

            OnLoadProjectile.Invoke(ProjectileInstance);

            if (debug) Debug.Log($"{name}:<color=white> <b>[Projectile Equiped] </b> [{ProjectileInstance.name}]  </color>");  //Debug
        }

        public virtual void ReleaseProjectile()
        {
            if (!gameObject.activeInHierarchy) return; //Crazy bug ??
            Predict?.Invoke(false);


            if (releaseProjectile == Release_Projectile.Never)
            {
                DestroyProjectileInstance();
                return;
            }
            else if (InstantiateProjectileOfFire)
            {
                EquipProjectile();
            }

            ReduceAmmo(1); //Reduce the Ammo

            if (ProjectileInstance == null) return;

            ProjectileInstance.transform.parent = null;

            if (debug) Debug.Log($"{name}:<color=white> <b>[Projectile Released] </b> [{ProjectileInstance.name}]  </color>");  //Debug


            IProjectile projectile = ProjectileInstance.GetComponent<IProjectile>();

            if (projectile != null)
            {
                ProjectileInstance.transform.position = AimOrigin.position;                  //Put the Correct position to Throw the Arrow IMPORTANT!!!!!

                CalculateVelocity();

                ProjectileInstance.transform.forward = Velocity.normalized; //Align the Projectile to the velocity


                ProjectileInstance.transform.Translate(projectile.PosOffset, Space.Self);  //Translate in the offset of the arrow to put it on the hand

                projectile.Prepare(Owner, Gravity, Velocity, Layer, TriggerInteraction);

                var newDamage = new StatModifier(statModifier)
                { Value = Mathf.Lerp(MinDamage, MaxDamage, ChargedNormalized) };

                projectile.PrepareDamage(newDamage, CriticalChance, CriticalMultiplier);

                projectile.Fire();
            }

            OnFireProjectile.Invoke(ProjectileInstance);
            ProjectileInstance = null;

            // WeaponReady(false); //Tell the weapon cannot be Ready until Somebody set it ready again

            PlaySound(WSound.Fire); //Play the Release Projectile Sound

            ResetCharge();
        }

        private void CalculateVelocity()
        {
            var Direction = (WeaponOwner.Aimer.AimPoint - AimOrigin.position).normalized;
        
            if (UseAimAngle.Value)
            {
                var RightV = Vector3.Cross(Direction, -Gravity);
                Velocity = Quaternion.AngleAxis(AimAngle, RightV) * Direction * Power;
            }
            else
                Velocity = Direction * Power;
        }


        /// <summary> Destroy the Active Arrow , used when is Stored the Weapon again and it had an arrow ready</summary>
        public virtual void DestroyProjectileInstance()
        {
            if (ProjectileInstance != null)
            {
                Destroy(ProjectileInstance);
                if (debug) Debug.Log($"{name}:<color=white> <b>[Destroy Projectile Inst]</b> </color>");  //Debug

            }
            ProjectileInstance = null; //Clean the Arrow Instance
        }

        public override bool Reload()
        {
            if (TotalAmmo == 0) return false;                //Means the Weapon Cannot Reload
            if (ChamberSize == AmmoInChamber) return false;  //Means there's no need to Reload.. the Chamber is full!!

            if (HasReloadAnim.Value)
            {
                WeaponAction.Invoke(WA.Reload);                       //PLAY THE RELOAD ANIMATION if you have reload animations; (RELOAD WILL BE DONE VIA ANIMATION)
                PlaySound(WSound.Reload);
                IsReloading = true;
                return true;
            }
            else
            {
                return ReloadWeapon();
            }
        }

        /// <summary> This can be called also by the ANIMATOR </summary>
        /// <returns></returns>
        public bool ReloadWeapon()
        {
            int RefillChamber = ChamberSize - AmmoInChamber;                    //Ammo Needed to refill the Chamber

            int AmmoLeft = TotalAmmo - RefillChamber;                           //Ammo Remaining

            if (AmmoLeft >= 0)                                                  //If is there any Ammo 
            {
                AmmoInChamber += RefillChamber;
                TotalAmmo -= RefillChamber;
            }
            else
            {
                AmmoInChamber += TotalAmmo;                                     //Set in the Chamber the remaining ammo  
                TotalAmmo = 0;                                                  //Empty the Total Ammo
            }

            if (ChamberSize <= 1 && TotalAmmo == 0) AmmoInChamber = 0; //Hack to use the AmmoInChamber

            return true;
        }

        public bool Reload(int ReloadAmount)
        {
            if (TotalAmmo == 0) return false;                                   //Means the Weapon Cannot Reload, there's no more ammo
            if (ChamberSize == AmmoInChamber) return false;                     //Means there's no need to Reload.. the Chamber is full!!


            int RefillChamber = ChamberSize - AmmoInChamber;                    //Ammo Needed to refill the Chamber
            ReloadAmount = Mathf.Clamp(ReloadAmount, 0, RefillChamber);

            int AmmoLeft = TotalAmmo - ReloadAmount;                           //Ammo Remaining


            if (AmmoLeft >= 0)                                                  //If is there any Ammo 
            {
                AmmoInChamber += ReloadAmount;
                TotalAmmo -= ReloadAmount;
            }
            else
            {
                AmmoInChamber += TotalAmmo;                                     //Set in the Chamber the remaining ammo  
                TotalAmmo = 0;                                                  //Empty the Total Ammo
            }

            OnReload.Invoke();
            return true;
        }

        /// <summary> If finish reload but is still aiming go to the Aiming animation **CALLED BY THE ANIMATOR**</summary>
        public virtual void FinishReload()
        {
            WeaponAction?.Invoke(IsAiming && !IsReady ? WA.Aim : WA.Idle);
            IsReloading = false;
            if (debug) Debug.Log($"{name}:<color=yellow> <b>[Finish Reload]</b> </color>");  //Debug
        }
    }


    #region INSPECTOR

#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(MShootable))]
    public class MShootableEditor : MWeaponEditor
    {
        SerializedProperty  m_AmmoInChamber, m_Ammo, m_ChamberSize, releaseProjectile, m_Projectile, AimLimit, 
            m_AutoReload, InstantiateProjectileOfFire, ProjectileParent, CancelAim, 
            OnReload, OnLoadProjectile, OnFireProjectile, gravity, UseAimAngle, m_AimAngle, HasReloadAnim;

        protected MShootable mShoot;

        private void OnEnable()
        {
            SetOnEnable();
            mShoot = (MShootable)target;
        }


        protected override void SetOnEnable()
        {
            WeaponTab = "Shootable";
            base.SetOnEnable();
            AimLimit = serializedObject.FindProperty("AimLimit");
            UseAimAngle = serializedObject.FindProperty("UseAimAngle");
            m_AimAngle = serializedObject.FindProperty("m_AimAngle");
            CancelAim = serializedObject.FindProperty("CancelAim");


            m_AutoReload = serializedObject.FindProperty("m_AutoReload");
            HasReloadAnim = serializedObject.FindProperty("HasReloadAnim");
            InstantiateProjectileOfFire = serializedObject.FindProperty("InstantiateProjectileOfFire");
            
            releaseProjectile = serializedObject.FindProperty("releaseProjectile");
            m_Projectile = serializedObject.FindProperty("m_Projectile");
            ProjectileParent = serializedObject.FindProperty("m_ProjectileParent");
            

            m_AmmoInChamber = serializedObject.FindProperty("m_AmmoInChamber");
            m_Ammo = serializedObject.FindProperty("m_Ammo");
            m_ChamberSize = serializedObject.FindProperty("m_ChamberSize");

            OnReload = serializedObject.FindProperty("OnReload");
            OnLoadProjectile = serializedObject.FindProperty("OnLoadProjectile");
            OnFireProjectile = serializedObject.FindProperty("OnFireProjectile");
            gravity = serializedObject.FindProperty("gravity");
        }
         

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Projectile Weapons Properties");
            WeaponInspector(false);
            serializedObject.ApplyModifiedProperties();
        }

        protected override void UpdateSoundHelp()
        {
            SoundHelp = "0:Draw   1:Store   2:Shoot   3:Reload   4:Empty  5:Charge";
        }

        protected override string CustomEventsHelp()
        {
            return "\n\n On Fire Gun: Invoked when the weapon is fired \n(Vector3: the Aim direction of the rider), \n\n On Hit: Invoked when the Weapon Fired and hit something \n(Transform: the gameobject that was hitted) \n\n On Aiming: Invoked when the Rider is Aiming or not \n\n On Reload: Invoked when Reload";
        }

        protected override void DrawExtras()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Physics", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 50;
                    EditorGUILayout.PropertyField(minForce, new GUIContent("Min", "Minimun Force"));
                    EditorGUILayout.PropertyField(Force, new GUIContent("Max", "Maximun Force"));
                    EditorGUIUtility.labelWidth = 0;

                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(forceMode);
                EditorGUILayout.PropertyField(gravity);
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
        }

        protected override void DrawAdvancedWeapon()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Aim Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_AimOrigin);
            EditorGUILayout.PropertyField(m_AimSide);
            EditorGUILayout.PropertyField(CancelAim);
            EditorGUILayout.PropertyField(AimLimit, new GUIContent("Aim Limit", "This Curve is for Limiting the Bow Animations while the Character is on weird/hard Positions"));

            EditorGUILayout.PropertyField(UseAimAngle, new GUIContent("Use Aim Angle", " Adds a Throw Angle to the Aimer Direction?"));
            if (mShoot.UseAimAngle.Value)
            {
                EditorGUILayout.PropertyField(m_AimAngle, new GUIContent("Aim Angle", " Adds a Throw Angle to the Aimer Direction"));
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Projectile", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(releaseProjectile);

            if (releaseProjectile.intValue != 0)
            {
                EditorGUILayout.PropertyField(InstantiateProjectileOfFire, new GUIContent("Inst Projectile on Fire", "Instanciate the Projectile when Firing the weapon.\n E.g The Pistol Instantiate the projectile on Firing. The bow Instantiate the Arrow Before Firing"));
                EditorGUILayout.PropertyField(m_Projectile);
                EditorGUILayout.PropertyField(ProjectileParent);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Ammunition", EditorStyles.boldLabel);
            //EditorGUILayout.PropertyField(m_Automatic, new GUIContent("Automatic", "one shot at the time or Automatic"));
            EditorGUILayout.PropertyField(m_AutoReload, new GUIContent("Auto Reload", "The weapon will reload automatically when the Ammo in chamber is zero"));
            EditorGUILayout.PropertyField(HasReloadAnim, new GUIContent("Has Reload Anim", "If the Weapon have reload animation then Play it"));
            EditorGUILayout.PropertyField(m_ChamberSize, new GUIContent("Chamber Size", "Total of Ammo that can be shoot before reloading"));

            if (mShoot.ChamberSize > 1)
            {
                EditorGUILayout.PropertyField(m_AmmoInChamber, new GUIContent("Ammo in Chamber", "Current ammo in the chamber"));
            }
            EditorGUILayout.PropertyField(m_Ammo, new GUIContent("Total Ammo", "Total ammo for the wapon"));
            EditorGUILayout.EndVertical();
        }


        protected override void ChildWeaponEvents()
        {
            EditorGUILayout.PropertyField(OnLoadProjectile);
            EditorGUILayout.PropertyField(OnFireProjectile);
            //EditorGUILayout.PropertyField(OnAiming);
            EditorGUILayout.PropertyField(OnReload);
        }
    }
#endif
    #endregion
}