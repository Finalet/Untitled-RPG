using UnityEngine;
using System.Collections;
using MalbersAnimations.Weapons;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.HAP
{
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    /// LOGIC
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    public partial class MWeaponManager
    {
        #region Reset Awake Start Update LateUpdate

        protected virtual void Awake()
        {
            Anim = GetComponent<Animator>();                                       //Get the Animator 
            Aimer = GetComponent<Aim>();
            Rider = GetComponent<IRider>();
           // m_Aim = GetComponent<Aim>();

            DefaultAnimUpdateMode = Anim.updateMode;
           
            defaultAimSide = Aimer.AimSide;

            StoreAfterTime = new WaitForSeconds(StoreAfter.Value);

            GetHashIDs();

            Head = Anim.GetBoneTransform(HumanBodyBones.Head);                     //Get the Rider Head transform
            Chest = Anim.GetBoneTransform(HumanBodyBones.Chest);                   //Get the Rider Head transform

            RightHand = Anim.GetBoneTransform(HumanBodyBones.RightHand);           //Get the Rider Right Hand transform
            LeftHand = Anim.GetBoneTransform(HumanBodyBones.LeftHand);             //Get the Rider Left  Hand transform

            RightShoulder = Anim.GetBoneTransform(HumanBodyBones.RightUpperArm);   //Get the Rider Right Shoulder transform
            LeftShoulder = Anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);     //Get the Rider Left  Shoulder transform }
 

            PrepareHolsters();
        }

        private void OnEnable()
        {
            SetBoolParameter += SetAnimParameter;
            SetIntParameter += SetAnimParameter;
            SetFloatParameter += SetAnimParameter;
           // Aimer.OnAiming.AddListener(Aim_Set);
        }


        private void OnDisable()
        {
            SetBoolParameter -= SetAnimParameter;
            SetIntParameter -= SetAnimParameter;
            SetFloatParameter -= SetAnimParameter;
           // Aimer.OnAiming.RemoveListener(Aim_Set);
        }

        protected virtual void GetHashIDs()
        {
            Hash_WAction = Animator.StringToHash(m_WeaponAction);
            Hash_WAim = Animator.StringToHash(m_WeaponAim);
            Hash_WType = Animator.StringToHash(m_WeaponType);
            Hash_WCharge = Animator.StringToHash(m_WeaponCharge);
            Hash_WHand = Animator.StringToHash(m_WeaponHand);
        }

        void FixedUpdate()
        {
            SetHorizontalAngle();
            WeaponCharged(Time.fixedDeltaTime);
        }

        void LateUpdate()
        {
            if (Active && CombatMode && WeaponIsActive)
            {
                if (Aim) Weapon.LateUpdateWeaponIK(this);    //Do the IK logic to the Character using the weapon
                Weapon.LateWeaponModification(this);         //If there's an Active Ability do the Late Ability thingy
            }
        }


        void OnAnimatorIK()
        {
            if (Active && CombatMode && WeaponIsActive && WeaponIKW != 0)               //If there's a Weapon Active
            {
                Weapon.OnAnimatorWeaponIK(this);
                if (Weapon.TwoHandIK) Weapon.TwoWeaponIK(this);
            }
        }

        #endregion

        protected virtual void WeaponCharged(float time)
        {
            if (Active && CombatMode && WeaponIsActive)           //If there's a Weapon Active
            {
                Weapon.Attack_Charge(this, time);
            }
        }





        #region AIMSTUFFS
        ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        /// <summary> This method is used to activate the AIM mode when the right click is pressed</summary>
        public virtual void SetHorizontalAngle()
        {
            if (!Aimer.Active) Aimer.Calculate(); //Recalculate when the Aimer is not Active

            float NewHorizontalAngle = Aimer.HorizontalAngle / 180; //Get the Normalized value for the look direction
            HorizontalAngle = Mathf.Lerp(HorizontalAngle, NewHorizontalAngle, Time.fixedDeltaTime * 10f); //Smooth Swap between 1 and -1
            SetFloatParameter(Hash_WAim, HorizontalAngle);
        }


        /// <summary> Set the Value of the Weapon Charge on the Animator </summary>
        public void SetWeaponCharge(float Charge) => SetFloatParameter(Hash_WCharge, Charge * Weapon.ChargeCharMultiplier);
        public void SetWeaponHand() => SetBoolParameter(Hash_WHand, Weapon.IsRightHanded);

        #endregion

        #region Draw Store Equip Unequip Weapons

        /// <summary>If the Rider had a weapon before mounting.. equip it.
        /// The weapon need an |IMWeapon| Interface, if not it wont be equiped</summary>
        public virtual void SetWeaponBeforeMounting(GameObject weapon) => SetWeaponBeforeMounting(weapon.GetComponent<MWeapon>());

        /// <summary>If the Rider had a weapon before mounting.. equip it.
        /// The weapon need an |IMWeapon| Interface, if not it wont be equiped</summary>
        public virtual void SetWeaponBeforeMounting(MWeapon weapon)
        {
            if (weapon == null) return;
            Weapon = weapon;
            if (Weapon)                                        //If the weapon doesn't have IMweapon Interface do nothing
            {
                CombatMode = true;

                Weapon.PrepareWeapon(this);
                Holster_SetActive(Weapon.HolsterID);                                                   //Set the Active holster for the Active Weapon

                WeaponType = Weapon.WeaponType;                                                     //Set the Weapon Type
                WeaponAction = WA.Idle;
                OnEquipWeapon.Invoke(Weapon.gameObject);
                Weapon.GetComponent<ICollectable>()?.Pick();

                ExitAim();

                Weapon.gameObject.SetActive(true);                                      //Set the Game Object Instance Active    
                ParentWeapon();

                SendMessage(Weapon.IsRightHanded ? FreeRightHand : FreeLeftHand, true);  //IK REINS Message


                if (debug) Debug.Log($"<b>{name}:<color=cyan> [EQUIP BEFORE MOUNTING -> {Weapon.Holster.name} -> {Weapon.name}]</color> </b>");  //Debug


                Weapon.gameObject.SetActive(true);                                      //Set the Game Object Instance Active    
                ParentWeapon();

                Weapon.GetComponent<ICollectable>()?.Pick();
            }
        }



        /// <summary>If using Holsters this will toggle the Active weapon (Call this by an Input) </summary>
        public virtual void ToggleActiveHolster()
        {
            if (Weapon)
            {
                if (WeaponAction == WA.Idle) Store_Weapon();              //Draw a weapon if we are on Action Idle 
            }
            else
            {
                Draw_Weapon();
            }
        }

        /// <summary>If using Holsters this will toggle the Active weapon (Call this by an Input) </summary>
        public virtual void ToggleActiveHolster_FAST()
        {
            if (Weapon)
            {
                if (WeaponAction == WA.Idle) UnEquip_FAST();              //Draw a weapon if we are on Action Idle 
            }
            else
            {
                Equip_FAST();
            }
        }

        /// <summary>Equip Weapon from holster or from Inventory  (Called by the Animator)</summary>
        public virtual void Equip_Weapon()
        {
            if (!Active) return;
            //if (Weapon.gameObject.IsPrefab()) return;   //Means the Weapon is a prefab and is not instantiated yet (MAKE A WAIT COROYTINE)

            WeaponAction = WA.Idle;                                             //Set the Action to Equip
            CombatMode = true;

            if (Weapon == null) return;

            if (debug) Debug.Log($"<B>{name}:<color=yellow> [Equip -> {Weapon.name}]</color></b> T{Time.time:F3}");

            Weapon.PrepareWeapon(this);

            if (UseHolsters)                                                             //If Use holster Means that the weapons are on the holster
            {
                ParentWeapon();
                StartCoroutine(MTools.AlignTransformLocal(Weapon.transform, Weapon.PositionOffset, Weapon.RotationOffset, Vector3.one, HolsterTime)); //Smoothly put the weapon in the hand
            }
            else //if (UseInventory)                                                            //If Use Inventory means that the weapons are on the inventory
            {
                if (!AlreadyInstantiated)                                                     //Do this if the Instantiation is not handled Externally
                {
                    ParentWeapon();

                    Weapon.transform.localPosition = Weapon.PositionOffset;    //Set the Correct Position
                    Weapon.transform.localEulerAngles = Weapon.RotationOffset; //Set the Correct Rotation
                    Weapon.transform.localScale = Vector3.one; //Set the Correct Rotation
                }
                Weapon.gameObject.SetActive(true);                                      //Set the Game Object Instance Active    
            }

            OnEquipWeapon.Invoke(Weapon.gameObject);                                               //Let everybody know that the weapon is equipped

            Weapon.PlaySound(WSound.Equip); //Play Equip Sound
        }

        /// <summary>Unequip Weapon from holster or from Inventory (Called by the Animator)</summary>
        public virtual void Unequip_Weapon()
        {
         //   if (!Active) return;
            ResetCombat();

            if (Weapon == null) return;

            if (debug) Debug.Log($"<b>{name}:<color=cyan> [Unequip -> {Weapon.name}]</color> </b>");  //Debug

            OnUnequipWeapon.Invoke(Weapon.gameObject);            //Let the rider know that the weapon has been unequiped.

            if (UseHolsters)                                 //If Use holster Parent the ActiveMWeapon the the holster
            {
                Weapon.transform.parent = ActiveHolster.Transform;        //Parent the weapon to his original holster          
                StartCoroutine(MTools.AlignTransform(Weapon.transform, Weapon.HolsterOffset, HolsterTime));
            }
            else if (!AlreadyInstantiated)
            {
                Destroy(Weapon.gameObject);
            }

            Weapon = null;     //IMPORTANT
        }

        /// <summary> Parents the Weapon to the Correct Hand</summary>
        private void ParentWeapon() =>
            Weapon.transform.parent = Weapon.IsRightHanded ? RightHandEquipPoint : LeftHandEquipPoint;  //Parent the Active Weapon to the Right/Left Hand


        /// <summary> Draw (Set the Correct Parameters to play Draw Weapon Animation) </summary>
        public virtual void Draw_Weapon()
        {
            if (!Active) return;

            ExitAim();
 
            if (UseInventory)                                                    //If is using External Equip
            {
                if (Weapon != null) Holster_SetActive(Weapon.HolsterID);          //Set the Current holster to the weapon asigned holster (THE WEAPON IS ALREADY SET)
            }


            if (UseHolsters)
            {
                Weapon = ActiveHolster.Weapon;         //Set the new Weapon from the Holster Weapon
            }

            if (Weapon)
            {
                if (Weapon.IgnoreDraw)
                {
                    Equip_FAST();
                    return;
                }

                WeaponType = Weapon.WeaponType;              //Set the Weapon Type (For the correct Animations)
                Action(DrawWeaponID);                        //Set the  Weapon Action to +1 to Draw Weapons From Right or from Left -1

                SendMessage(Weapon.IsRightHanded ? FreeRightHand : FreeLeftHand, true);  //IK REINS Message

                if (debug) Debug.Log($"<b>{name}:<color=yellow> [Draw -> { (Weapon.IsRightHanded ? "Right Hand" : "Left Hand")} -> {Weapon.Holster.name} -> {Weapon.name}]</color> </b>");  //Debug
            }
        }

        /// <summary>Store (Set the Correct Parameters to play Store Weapon Animation) </summary>
        public virtual void Store_Weapon()
        {
            if (Weapon == null) return;                    //Skip if there's no Active Weapon or is not inCombatMode, meaning there's an active weapon
           
            if (Weapon.IgnoreDraw)
            {
                UnEquip_FAST();
                return;
            }

            WeaponAction = StoreWeaponID;                 //Set the  Weapon Action to Store Weapons 
            if (debug) Debug.Log($"<b>{name}:<color=cyan> [Store -> { (Weapon.IsRightHanded ? "Righ Hand" : "Left Hand")} -> {Weapon.Holster.name} -> {Weapon.name}]</color> </b>");  //Debug
        }

        public virtual void Equip_FAST()
        {
            if (UseHolsters) Weapon = ActiveHolster.Weapon;                //Check if the Child on the holster Has a IMWeapon on it
            if (Weapon == null || Weapon.gameObject.IsPrefab()) return;     //Means the Weapon is a prefab and is not instantiated yet (MAKE A WAIT COROUTINE)

            Equip_FAST(Weapon,true);
        }

        /// <summary> Performs a Fast equiping of a MWeapon</summary>
        /// <param name="weapon">Weapon to equip</param>
        /// <param name="doParent">Parent the weapon to the Hand, it also resets the scale to 1.1.1</param>
        public virtual void Equip_FAST(MWeapon weapon, bool doParent = true)
        {
            Weapon = weapon;

            if (debug) Debug.Log($"<b>{name}:<color=cyan> [FAST EQUIP -> {Weapon.Holster.name} -> {Weapon.name}]</color> </b>");  //Debug
           
            ExitAim();


            SendMessage(Weapon.IsRightHanded ? FreeRightHand : FreeLeftHand, true);  //IK REINS 

            WeaponType = Weapon.WeaponType;
            WeaponAction = WA.Idle;                                             //Set the Action to Equip
            CombatMode = true;
            Weapon.PrepareWeapon(this);
            Weapon.GetComponent<ICollectable>()?.Pick();

            Weapon.PlaySound(0); //Play Draw Sound

            if (doParent)
            {
                Weapon.gameObject.SetActive(true);                                      //Set the Game Object Instance Active    
                ParentWeapon();
                Weapon.transform.SetLocalTransform(Weapon.PositionOffset, Weapon.RotationOffset, Vector3.one); //Local position when is Parent to the weapon
            }

            OnEquipWeapon.Invoke(Weapon.gameObject);
        }


        public void UnEquip_FAST()
        {
            if (Weapon == null) return;

           if (debug) Debug.Log($"<b>{name}:<color=cyan> [FAST UNEQUIP -> {Weapon.Holster.name} -> {Weapon.name}]</color> </b>");  //Debug

            ResetCombat();

          //  LastStoredWeaponHand = Weapon.IsRightHanded;
            OnUnequipWeapon.Invoke(Weapon.gameObject);            //Let the rider know that the weapon has been unequiped.

            if (UseHolsters)                                 //If Use holster Parent the ActiveMWeapon the the holster
            {
                Weapon.transform.parent = ActiveHolster.Transform;        //Parent the weapon to his original holster          
                Weapon.transform.SetLocalTransform(Weapon.HolsterOffset); //Set the Holster Offset Option
            }
            else if (UseInventory && !AlreadyInstantiated)
            {
                Destroy(Weapon.gameObject);
            }
            OnUnequipWeapon.Invoke(Weapon.gameObject);
            Weapon = null;     //IMPORTANT
        }


     //   bool LastStoredWeaponHand;
        /// <summary>  Used By the Animator at the end of the Store Weapon animations  </summary>
        public virtual void Finish_StoreW()
        {
            if (Weapon == null) //Means that there's no next Weapon Trying to equip
            {
                Action(0);
                //if (LastStoredWeaponHand) FreeRightHand(false);
                //else FreeLeftHand(false);
            }
        }

        /// <summary>Enable Disable the Attack Trigger in case it has one... this is called by the animator Like the Attack Triggers!!! </summary>
        public void ActivateDamager(int value) => Weapon?.ActivateDamager(value);

        /// <summary>Update all Damagers on the Character</summary>
        public void UpdateDamagerSet() { }


        private void TryInstantiateWeapon(MWeapon Next_Weapon)
        {
            if (!AlreadyInstantiated)
            {
                var WeaponGO = Instantiate(Next_Weapon.gameObject, transform);      //Instanciate the Weapon GameObject
                WeaponGO.SetActive(false);                                          //Hide it to show it later
                Next_Weapon = WeaponGO.GetComponent<MWeapon>();                     //UPDATE THE REFERENCE
                if (debug) Debug.Log("<B>" + WeaponGO.name + "</B> Instantiated");
            }
            Weapon = Next_Weapon;
        }

        /// <summary> Is called to swap weapons</summary>
        private IEnumerator SwapWeaponsHolster(int HolstertoSwap)
        {
            Store_Weapon();
            while (WeaponAction != WA.None) yield return null;    // Wait for the weapon is Unequiped Before it can Draw Another
            Holster_SetActive(HolstertoSwap);
            Draw_Weapon();                                  //Set the parameters so draw a weapon
        }

        /// <summary>Is called to swap weapons</summary>
        private IEnumerator SwapWeaponsInventory(GameObject nextWeapon)
        {
            Store_Weapon();
            
            while (WeaponAction != WA.None) yield return null;        // Wait for the weapon is Unequiped Before it can Draw Another

            TryInstantiateWeapon(nextWeapon.GetComponent<MWeapon>());
           
            Draw_Weapon();                                                                  //Set the parameters so draw a weapon
        }
        #endregion
    }
}