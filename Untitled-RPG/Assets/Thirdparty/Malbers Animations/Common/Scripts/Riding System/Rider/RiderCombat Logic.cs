using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using MalbersAnimations.Weapons;
using MalbersAnimations.Events;
using System;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.HAP
{
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    /// LOGIC
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    public partial class RiderCombat
    {
        #region Reset Awake Start Update LateUpdate

        void Awake() { InitRiderCombat(); }


        protected virtual void GetHashIDs()
        {
            Hash_WAction = Animator.StringToHash(m_WeaponAction);
            Hash_WAim = Animator.StringToHash(m_WeaponAim);
            Hash_WType = Animator.StringToHash(m_WeaponType);
            Hash_WHolder = Animator.StringToHash(m_WeaponHolder);
            Hash_WHold = Animator.StringToHash(m_WeaponHold);
        }


        void Update()
        {
            CombatLogicUpdate();
        }


        void ForceAnimatorUpdateMode(bool force)
        {
            Anim.updateMode = force ? AnimatorUpdateMode.Normal : DefaultAnimUpdateMode;
            if (ForceNormalUpdate)
            {
                if (ForceMountNormalUpdate)
                {
                    Rider.Montura.Anim.updateMode = force ? AnimatorUpdateMode.Normal : Rider.Montura.DefaultAnimUpdateMode;
                }
                Rider.ForceLateUpdateLink = force;
            }
        }

        void LateUpdate()
        {
            if (!Active) return;                     //Skip if Lock Combat is On

            if (IsWeaponActive)
                ActiveAbility.LateUpdateAbility();      //If there's an Active Ability do the Late Ability thingy
        }
        #endregion

        /// <summary>Initialize all variables for the Rider</summary>
        protected virtual void InitRiderCombat()
        {
            _t = transform;                                                //Get this Transform   
            Anim = GetComponent<Animator>();                                       //Get the Animator
            Rider = GetComponent<MRider>();


            DefaultAnimUpdateMode = Anim.updateMode;
            GetHashIDs();

            Head = Anim.GetBoneTransform(HumanBodyBones.Head);                     //Get the Rider Head transform
            Chest = Anim.GetBoneTransform(HumanBodyBones.Chest);                   //Get the Rider Head transform

            RightHand = Anim.GetBoneTransform(HumanBodyBones.RightHand);           //Get the Rider Right Hand transform
            LeftHand = Anim.GetBoneTransform(HumanBodyBones.LeftHand);             //Get the Rider Left  Hand transform

            RightShoulder = Anim.GetBoneTransform(HumanBodyBones.RightUpperArm);   //Get the Rider Right Shoulder transform
            LeftShoulder = Anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);     //Get the Rider Left  Shoulder transform

            SetActiveHolder(ActiveHolderSide);                                       //Set one Holder to Draw Weapons
            CombatMode = false;
            ActiveAbility = null;

            Layer_RiderArmLeft = Anim.GetLayerIndex("Rider Arm Left");              //Gets the Left Arm Layer Index
            Layer_RiderArmRight = Anim.GetLayerIndex("Rider Arm Right");            //Gets the Right Arm Layer Index   
            Layer_RiderCombat = Anim.GetLayerIndex(CombatLayerName);                 //Gets the Combat Later Index


            Aimer = GetComponent<IAim>();
            Aimer.Active = true;

            if (CloneAbilities)
            {
                for (int i = 0; i < CombatAbilities.Count; i++)
                {
                    RiderCombatAbility instance = (RiderCombatAbility)ScriptableObject.CreateInstance(CombatAbilities[i].GetType());
                    instance = ScriptableObject.Instantiate(CombatAbilities[i]);                                 //Create a clone from the Original Scriptable Objects! IMPORTANT
                    instance.name = instance.name.Replace("(Clone)", "(C)");
                    CombatAbilities[i] = instance;
                }
            }

            foreach (var ability in CombatAbilities)
            {
                ability.StartAbility(this); //Initialize the Abilities
            }


            //Event Listeners
            Rider.OnStartMounting.AddListener(OnStartMounting);
            Rider.OnStartDismounting.AddListener(OnStartDismounting);
            //rider.OnEndMounting.AddListener(OnEndMounting);
            //rider.OnEndDismounting.AddListener(OnEndDismounting);

            OnAimSide.AddListener(InAimMode);
        }


        #region Events Listeners
        public virtual void OnStartMounting()
        {
            DefaultMonturaCamBase = Rider.Montura.Animal.UseCameraInput;  //Store The Default Animal Type of Input
        }

        public virtual void OnStartDismounting()
        {
            if (ExitCombatOnDismount) Store_Weapon();
        }
        #endregion

        protected virtual void CombatLogicUpdate()
        {
            if (!Rider.IsRiding) return;                                                        //Just work while is in the horse
            if (!Active) return;                                                                //Skip if is Disable

            if (CombatMode)                                                                             //If there's a Weapon Active
            {
                if (IsWeaponActive)
                {
                     CalculateCameraTargetSide();
                    if (ActiveAbility.CanAim) AimMode();

                    ActiveAbility.UpdateAbility();                                                    //Update The Active Ability

                    if (ActiveWeapon.MainAttack && WeaponCanAttack)
                        ActiveAbility.MainAttackHold();

                    if (ActiveWeapon.SecondAttack && WeaponCanAttack)
                        ActiveAbility.SecondaryAttackHold();
                }
            }
        }


        void OnAnimatorIK()
        {
            bool AnimatePhysics = Anim.updateMode == AnimatorUpdateMode.AnimatePhysics;
            DeltaTime = AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;

            if (ActiveAbility)
            {
                ActiveAbility.OnAbilityIK();
            }
        }

        /// <summary> Gets the Camera and Target Side </summary>
        private void CalculateCameraTargetSide()
        {
            TargetSide = Aimer.TargetSide;                                                //Get the Target Side Left/Right
            CameraSide = Aimer.CameraSide;                                                //Get the Camera Side Left/Right
        }



        #region AIMSTUFFS
        ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        /// <summary> This method is used to activate the AIM mode when the right click is pressed</summary>
        public virtual void AimMode()
        {
            LookDirectionHorizontal();

            if (StrafeOnTarget)
            {
                Rider.Montura.Animal.UseCameraInput = Aimer.AimTarget ? true : DefaultMonturaCamBase;
            }
        }

        /// <summary> Set the Animator Aim Side to look to the Camera or Target Forward Direction </summary>
        protected virtual void LookDirectionHorizontal()
        {
            Vector3 dir = Vector3.ProjectOnPlane(AimDirection, Vector3.up);

            float NewHorizontalAngle = (Vector3.Angle(dir, _t.root.forward) * ((Aimer.AimTarget ? TargetSide : CameraSide) ? 1 : -1)) / 180; //Get the Normalized value for the look direction

            horizontalAngle = Mathf.Lerp(HorizontalAngle, NewHorizontalAngle, DeltaTime * 15f); //Smooth Swap between 1 and -1

            Anim.SetFloat(Hash_WAim, HorizontalAngle);
        }

        public virtual void UpdateCameraAimSide()
        {
            if (CameraSide)
            {
                if (!(!ActiveAbility.ChangeAimCameraSide() && Weapon_is_RightHand))   //Prevent the Camera Swap if is Using A Bow
                    OnAimSide.Invoke(1);
                else
                    OnAimSide.Invoke(-1);
            }
            else
            {
                if (!(!ActiveAbility.ChangeAimCameraSide() && Weapon_is_LeftHand))  //Prevent the Camera Swap if is Using A Bow
                    OnAimSide.Invoke(-1);
                else
                    OnAimSide.Invoke(1);
            }
        }


        private void InAimMode(int AimID)
        {
            if (AimID != 0) //means is Aiming
            {
                if (WeaponAction != WA.Hold && WeaponAction != WA.ReloadLeft && WeaponAction != WA.ReloadRight)
                {
                    WeaponAction = ActiveWeapon.RightHand ? WA.AimRight : WA.AimLeft;   //If the weapon is Right Handed set the action to AimRight else AimLeft
                }
            }
        }


      
        #endregion

        #region Draw Store Equip Unequip Weapons

        /// <summary>Set the Active Holder Transform for the Active Holder Side </summary>
        protected virtual void SetActiveHolder(WeaponHolder holder)
        {
            ActiveHolderSide = holder;
            switch (ActiveHolderSide)
            {
                case WeaponHolder.None:
                    ActiveHolderTransform = HolderBack; // Set BACK As default the HolderBack
                    break;
                case WeaponHolder.Left:
                    ActiveHolderTransform = HolderLeft ? HolderLeft : HolderBack;
                    break;
                case WeaponHolder.Right:
                    ActiveHolderTransform = HolderRight ? HolderRight : HolderBack;
                    break;
                case WeaponHolder.Back:
                    ActiveHolderTransform = HolderBack;
                    break;
                default:
                    break;
            }
        }

        /// <summary>If the Rider had a weapon before mounting.. equip it.
        /// The weapon need an |IMWeapon| Interface, if not it wont be equiped</summary>
        public virtual void SetWeaponBeforeMounting(GameObject weapon)
        {
            if (weapon == null) return;
            if (weapon.GetComponent<IMWeapon>() == null) return;                                        //If the weapon doesn't have IMweapon Interface do nothing

            /// Try the set to false the weapon if is not a Prefab ///

            ActiveWeaponGameObject = weapon;

            CombatMode = true;

            ActiveWeapon.Owner = transform;
            ActiveWeapon.IsEquiped = true;                                                            //Let the weapon know that it has been Equiped
            ActiveWeapon.HitLayer = Aimer.AimLayer;                                                   //Link the Hit Mask
            ActiveWeapon.TriggerInteraction = Aimer.TriggerInteraction;                               //Link the Trigger Interatction 

            SetActiveHolder(ActiveWeapon.Holder);                                                     //Set the Active Holder for the Active Weapon

            WeaponType = GetWeaponType();                                                              //Set the Weapon Type

            WeaponAction = WA.Idle;

            SetWeaponIdleAnimState(ActiveWeapon.RightHand);                                           //Set the Correct Idle Hands Animations

            SetActiveAbility();

            if (ActiveAbility)
            {
                ActiveAbility.ActivateAbility();
            }
            else
            {
                Debug.LogError("The Weapon is combatible but there's no Combat Ability available for it, please Add the matching ability it on the list of Combat Abilities");
            }

            OnEquipWeapon.Invoke(ActiveWeaponGameObject);
        }

        /// <summary>If using Holders this will toggle the Active weapon (Call this </summary>
        public virtual void ToggleActiveHolderWeapon()
        {
            if (ActiveWeaponGameObject)
            {
                if (WeaponAction == WA.Idle)
                    Store_Weapon();                                         //Draw a weapon if we are on Action Idle 
            }
            else
            {
                Draw_Weapon();
            }
        }

        /// <summary>Is called to swap weapons </summary>
        public virtual void Change_Weapon_Holder_Inputs(WeaponHolder holder)
        {
            if (debug) Debug.Log($"Change Holder to: <b>{holder}</b>");

            if (ActiveHolderSide != holder && WeaponAction == WA.Idle)        //if there's a weapon on hand, Store it and draw the other weapon from the next holder
            {
                StartCoroutine(SwapWeaponsHolder(holder));
            }
            else if (ActiveHolderSide != holder && WeaponAction == WA.None)   //if there's no weapon draw the weapon from the next holder
            {
                SetActiveHolder(holder);
                Draw_Weapon();
            }
            else
            {
                if (!CombatMode)
                {
                    if (WeaponAction == WA.None)
                        Draw_Weapon();                                                      //Draw a weapon if we are on Action None
                }
                else
                {
                    if (WeaponAction == WA.Idle)
                        Store_Weapon();                                                    //Store a weapon if we are on Action Idle 
                }
            }
        }

        /// <summary>Equip Weapon from Holders or from Inventory  (Called by the Animator)</summary>
        public virtual void Equip_Weapon()
        {
            //WeaponAction = WA.Equip;                                             //Set the Action to Equip
            WeaponAction = WA.Idle;                                             //Set the Action to Equip
            CombatMode = true;

            if (ActiveWeapon == null) return;

            if (debug) Debug.Log($"Equip Weapon Type: <b> {ActiveWeapon.WeaponType.name}</b>");

            ActiveWeapon.HitLayer = Aimer.AimLayer;                                                   //Update the Hit Mask on the Weapon
            ActiveWeapon.TriggerInteraction = Aimer.TriggerInteraction;                               //Update the Trigger Interatction on the Weapon

            if (UseHolders)                                                             //If Use Holders Means that the weapons are on the Holders
            {
                if (ActiveHolderTransform.transform.childCount > 0)                     //If there's a Weapon on the Holder
                {
                    ActiveWeaponGameObject = ActiveHolderTransform.GetChild(0).gameObject;      //Set the Active Weapon as the First Child Inside the Holder

                    ActiveWeaponGameObject.transform.parent =
                        ActiveWeapon.RightHand ? RightHandEquipPoint : LeftHandEquipPoint; //Parent the Active Weapon to the Right/Left Hand
                    ActiveWeapon.Holder = ActiveHolderSide;

                    StartCoroutine(SmoothWeaponTransition
                        (ActiveWeaponGameObject.transform, ActiveWeapon.PositionOffset, ActiveWeapon.RotationOffset, 0.3f)); //Smoothly put the weapon in the hand
                }
            }
            else if (UseInventory)                                                            //If Use Inventory means that the weapons are on the inventory
            {
                if (!AlreadyInstantiated)                                                     //Do this if the Instantiation is not handled Externally
                {
                    ActiveWeaponGameObject.transform.parent =
                        ActiveWeapon.RightHand ? RightHandEquipPoint : LeftHandEquipPoint;  //Parent the Active Weapon to the Right/Left Hand

                    ActiveWeaponGameObject.transform.localPosition = ActiveWeapon.PositionOffset;    //Set the Correct Position
                    ActiveWeaponGameObject.transform.localEulerAngles = ActiveWeapon.RotationOffset; //Set the Correct Rotation
                }
                ActiveWeaponGameObject.gameObject.SetActive(true);                                      //Set the Game Object Instance Active    
            }

            ActiveWeapon.Owner = transform;
            ActiveWeapon.IsEquiped = true;                                                     //Inform the weapon that it has been equipped

            OnEquipWeapon.Invoke(ActiveWeaponGameObject);                                               //Let everybody know that the weapon is equipped

            if (ActiveAbility) ActiveAbility.ActivateAbility();                                //Call For the first activation of the weapon when first Equipped
        }

        /// <summary>Unequip Weapon from Holders or from Inventory (Called by the Animator)</summary>
        public virtual void Unequip_Weapon()
        {
            WeaponType = null;
            WeaponAction = WA.None;

            if (ActiveWeapon == null) return;
            if (debug) Debug.Log($"Unequip Weapon Type: <b>{ActiveWeapon.WeaponType.name}</b>");


            ActiveWeapon.IsEquiped = false;                   //Let the weapon know that it has been unequiped
            OnUnequipWeapon.Invoke(ActiveWeaponGameObject);            //Let the rider know that the weapon has been unequiped.

            if (UseHolders)                                 //If Use Holders Parent the ActiveMWeapon the the Holder
            {
                ActiveWeaponGameObject.transform.parent = ActiveHolderTransform.transform;        //Parent the weapon to his original holder          
                StartCoroutine(SmoothWeaponTransition(ActiveWeaponGameObject.transform, Vector3.zero, Vector3.zero, 0.3f));
            }
            else if (UseInventory && !AlreadyInstantiated && ActiveWeaponGameObject)
            {
                Destroy(ActiveWeaponGameObject);
            }

            ActiveWeaponGameObject = null; //IMPORTANT
        }

        /// <summary> Draw (Set the Correct Parameters to play Draw Weapon Animation) </summary>
        public virtual void Draw_Weapon()
        {
            ResetRiderCombat();

            if (UseInventory)                                                                           //If is using inventory
            {
                if (ActiveWeapon != null)
                {
                    SetActiveHolder(ActiveWeapon.Holder);                                             //Set the Current Holder to the weapon asigned holder
                }
            }
            else //if Use Holders
            {
                if (ActiveHolderTransform.childCount == 0) return;
                IMWeapon isCombatible = ActiveHolderTransform.GetChild(0).GetComponent<IMWeapon>();     //Check if the Child on the holder Has a IMWeapon on it

                if (isCombatible == null) return;

                ActiveWeaponGameObject = (ActiveHolderTransform.GetChild(0).gameObject);                          //Set Active Weapon to the Active Holder Child 
            }

            WeaponType = GetWeaponType();                                                              //Set the Weapon Type (For the correct Animations)

            WeaponAction = ActiveWeapon.RightHand ? WA.DrawFromRight : WA.DrawFromLeft; //Set the  Weapon Action to -1 to Draw Weapons From Right or from left -2

            SetWeaponIdleAnimState(ActiveWeapon.RightHand);

            SetActiveAbility();

            if (debug) Debug.Log($"Play Draw Animation with: <b>{ActiveWeaponGameObject?.name}</b>");  //Debug
        }

        private void SetActiveAbility()
        {
            ActiveAbility = CombatAbilities.Find(ability => ability.WeaponType == ActiveWeapon.WeaponType); //Find the Ability for the IMWeapon 
         
        }

        /// <summary>Store (Set the Correct Parameters to play Store Weapon Animation) </summary>
        public virtual void Store_Weapon()
        {
            if (ActiveWeapon == null || !CombatMode) return;                          //Skip if there's no Active Weapon or is not inCombatMode

            ResetRiderCombat();
            ActiveWeapon.ResetWeapon();                                         //IMPORTANT
            WeaponType = null;                                                  //Set the weapon ID to None (For the correct Animations)
            SetActiveHolder(ActiveWeapon.Holder);
            WeaponAction = ActiveWeapon.RightHand ? WA.StoreToRight : WA.StoreToLeft;   //Set the  Weapon Action to -1 to Store Weapons to Right or -2 to left

            if (debug) Debug.Log($"Store: <b>{ActiveWeaponGameObject?.name}</b>");
        }

        /// <summary>Reset and Remove the Active Ability </summary>
        protected virtual void ResetActiveAbility()
        {
            ActiveAbility?.ResetAbility();
            ActiveAbility = null;
        }


        /// <summary>Sets the weapon that are in the Inventory</summary>
        /// <param name="Next_Weapon">Game object that it should have an IMWeapon Interface</param>
        public virtual void SetWeaponByInventory(GameObject Next_Weapon)
        {
            if (!Rider.IsRiding) return;                                         //Work Only when is riding else Skip
            if (UseHolders) return;                                             //Work Only when is riding else Skip

            if (WeaponAction != WA.Idle && WeaponAction != WA.None &&
                 WeaponAction != WA.AimLeft && WeaponAction != WA.AimRight && WeaponAction != WA.Hold) return;     //If is not on any of these states then Dont Equip..

            StopAllCoroutines();

            if (Next_Weapon == null)                                    //That means Store the weapon
            {
                if (ActiveWeaponGameObject)
                    Store_Weapon();
                if (debug) Debug.Log("Active Weapon NOT NULL Store the Active Weapon");
                return;
            }

            IMWeapon Next_IMWeapon = Next_Weapon.GetComponent<IMWeapon>();

            if (Next_IMWeapon == null)
            {
                if (ActiveWeaponGameObject)
                    Store_Weapon();
                if (debug) Debug.Log("Active Weapon NOT NULL and Store because  Next Weapon is not Compatible");
                return;                                                 //If the Next Weapon doesnot have the IMWeapon Interface dismiss... the next weapon is not compatible
            }

            if (ActiveWeapon == null)                               //Means there's no weapon active so draw it
            {
                if (!AlreadyInstantiated)
                {
                    Next_Weapon = Instantiate(Next_Weapon, Rider.transform);    //Instanciate the Weapon GameObject
                    Next_Weapon.SetActive(false);                               //Hide it to show it later
                    if (debug) Debug.Log("<B>" + Next_Weapon.name + "</B> Instantiated");
                }
                ActiveWeaponGameObject = Next_Weapon;

                Draw_Weapon();                                                  //Debug.Log("Active weapon is NULL so DRAW");
            }
            else if (ActiveWeapon.Equals(Next_IMWeapon))                         //You are trying to draw the same weapon
            {
                if (!CombatMode)
                {
                    Draw_Weapon();
                    Debug.Log("Active weapon is the same as the NEXT Weapon and we are NOT in Combat so DRAW");
                }
                else
                {
                    Store_Weapon();
                    if (debug) Debug.Log("Active weapon is the same as the NEXT Weapon and we ARE  in Combat so STORE");
                }

            }
            else                                                                //If the weapons are different Swap it
            {
                StartCoroutine(SwapWeaponsInventory(Next_Weapon));
                if (debug) Debug.Log("Active weapon is DIFFERENT to the NEXT weapon so Switch" + Next_Weapon);
            }
        }


        /// <summary> Is called to swap weapons</summary>
        private IEnumerator SwapWeaponsHolder(WeaponHolder HoldertoSwap)
        {
            Store_Weapon();

            while (WeaponAction == WA.StoreToLeft || WeaponAction == WA.StoreToRight) // Wait for the weapon is Unequiped Before it can Draw Another
            {
                yield return null;
            }

            SetActiveHolder(HoldertoSwap);
            Draw_Weapon();                                  //Set the parameters so draw a weapon
        }

        /// <summary>Is called to swap weapons</summary>
        private IEnumerator SwapWeaponsInventory(GameObject nextWeapon)
        {
            Store_Weapon();

            while (WeaponAction == WA.StoreToLeft || WeaponAction == WA.StoreToRight) // Wait for the weapon is Unequiped Before it can Draw Another
            {
                yield return null;
            }

            if (!AlreadyInstantiated)
            {
                nextWeapon = Instantiate(nextWeapon, Rider.transform);        //instanciate the Weapon GameObject
                nextWeapon.SetActive(false);
            }

            ActiveWeaponGameObject = nextWeapon;                    //Set the New Weapon
            SetActiveHolder(ActiveWeapon.Holder);

            Draw_Weapon();                                  //Set the parameters so draw a weapon
        }

        /// <summary>This Coroutine will smoothly move the weapon from the holder and viceversa in a time if we are using the holders </summary>
        private IEnumerator SmoothWeaponTransition(Transform obj, Vector3 posOfsset, Vector3 rotOffset, float time)
        {
            float elapsedtime = 0;
            Vector3 startPos = obj.localPosition;
            Quaternion startRot = obj.localRotation;

            while (elapsedtime < time)
            {
                obj.localPosition = Vector3.Slerp(startPos, posOfsset, Mathf.SmoothStep(0, 1, elapsedtime / time));
                obj.localRotation = Quaternion.Slerp(startRot, Quaternion.Euler(rotOffset), elapsedtime / time);
                elapsedtime += Time.deltaTime;
                yield return null;
            }
            obj.localPosition = posOfsset;
            obj.localEulerAngles = rotOffset;
        }
        #endregion

    }
}