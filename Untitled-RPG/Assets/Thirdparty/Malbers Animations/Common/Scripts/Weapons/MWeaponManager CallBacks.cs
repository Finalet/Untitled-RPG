using MalbersAnimations.Weapons;
using UnityEngine;

namespace MalbersAnimations.HAP
{
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    /// CALLBACKS
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    public partial class MWeaponManager
    {

        #region Holsters

        private void PrepareHolsters()
        {
            if (holsters != null && holsters.Count == 0) return;

            for (int i = 0; i < holsters.Count; i++) holsters[i].Index = i; //Set the Index on each Holster
            Holster_SetActive(DefaultHolster);

            foreach (var h in holsters)
                h.PrepareWeapon();
        }


        public void Holster_SetActive(int ID)
        {
            ActiveHolster = holsters.Find(x => x.GetID == ID);
            ActiveHolsterIndex = ActiveHolster != null ? ActiveHolster.Index : 0;
            if (debug) Debug.Log($"<B>{name}: [Active Holster -> {ActiveHolsterIndex}]</b>");
        }

        public void Holster_SetNext()
        {
            ActiveHolsterIndex = (ActiveHolsterIndex + 1) % holsters.Count;
            ActiveHolster = holsters[ActiveHolsterIndex];
        }

        public void Holster_SetPrevius()
        {
            ActiveHolsterIndex = (ActiveHolsterIndex - 1) % holsters.Count;
            ActiveHolster = holsters[ActiveHolsterIndex];
        }

        public virtual void Holster_Equip(int HolsterID)
        {
            if (UseHolsters && Active && !Paused && CheckRidingOnly)
            {
                if (MTools.CompareOR(Mathf.Abs(WeaponAction), WA.None, WA.Idle))   //Change weapons from holsters only If we are on none,Idle or Aiming
                {
                    if (ActiveHolster != HolsterID)        //if there's a weapon on hand, Store it and draw the other weapon from the next holster
                    {
                        StartCoroutine(SwapWeaponsHolster(HolsterID));
                    }
                    else
                    {
                        if (!CombatMode) Draw_Weapon();             //Draw a weapon if we are on Action None
                        else Store_Weapon();                         //Store a weapon if we are on Action Idle 
                    }
                }
            }
        }

        public virtual void Holster_Equip_Fast(int HolsterID)
        {
            if (UseHolsters && Active && !Paused && CheckRidingOnly)
            {
                if (MTools.CompareOR(Mathf.Abs(WeaponAction), WA.None, WA.Idle))   //Change weapons from holsters only If we are on none,Idle or Aiming
                {
                    if (ActiveHolster != HolsterID)   //if there's a weapon on hand, Store it and draw the other weapon from the next holster
                    {
                        UnEquip_FAST();
                        Holster_SetActive(HolsterID);
                        Equip_FAST();
                    }
                    else
                    {
                        if (!CombatMode) Equip_FAST();              // Draw a weapon if we are on Action None
                                                                    // else UnEquip_FAST();                         //Store a weapon if we are on Action Idle 
                    }
                }
            }
        }

        public virtual void Holster_Equip(HolsterID HolsterID) => Holster_Equip(HolsterID.ID);
        public virtual void Holster_Equip_Fast(HolsterID HolsterID) => Holster_Equip_Fast(HolsterID.ID);

        public virtual void Holster_SetWeapon(GameObject WeaponGO)
        {
            var Next_Weapon = WeaponGO != null ?  WeaponGO.GetComponent<MWeapon>() : null;

            if (Next_Weapon != null)
            {
                var holster = holsters.Find(x => x.ID == Next_Weapon.HolsterID);

                if (holster != null)
                {
                    if (debug) Debug.Log($"<B>{name}: <color=green>[Set Weapon on Holster] -> [{holster.ID.name}] [{Next_Weapon.name}]</color></B>");
 
                    var WasEquipped = false;  

                    if (holster.Weapon != null)
                    {
                        WasEquipped = holster.Weapon.IsEquiped; //Check if the weapon that you are replacing was Equipped

                        if (WasEquipped)  UnEquip_FAST();

                        holster.Weapon.GetComponent<ICollectable>()?.Drop();  //if is a collecctable then Drop it
                        if (holster.Weapon) holster.Weapon = null; //Reset the Holster Weapon to nul
                    }

                    if (WeaponGO.IsPrefab()) WeaponGO = Instantiate(WeaponGO);        //if is a prefab instantiate on the scene

                    WeaponGO.transform.parent = holster.Transform;                    //Parent the weapon to his original holster          
                   
                    WeaponGO.transform.SetLocalTransform(Next_Weapon.HolsterOffset);

                    holster.Weapon = Next_Weapon;

                    holster.Weapon.DisablePhysics();

                    if (WasEquipped) Equip_FAST();
                     
                }
                else
                {
                    if (debug) Debug.Log($"<B>{name}: <color=orange>[Set Weapon on Holster Failed]</B> -> There's no Holster <B>[{Next_Weapon.Holster.name}]</B> on the Holster List for the Weapon <B>[{Next_Weapon.name}]</B></color>");
                }
            }
        }
        #endregion


        /// <summary>Sets the weapon equipped by an External Source</summary>
        public virtual void Equip_External(GameObject WeaponGo)
        {
            if (Active && UseInventory && !Paused && CheckRidingOnly)
            {
                var Next_Weapon = WeaponGo?.GetComponent<MWeapon>();
                if (!MTools.CompareOR(Mathf.Abs(WeaponAction), WA.Idle, WA.None)) return; //If is not on any of these states then Dont Equip..

                StopAllCoroutines();

                if (Next_Weapon == null)                                    //That means Store the weapon
                {
                    Store_Weapon();
                    if (debug) Debug.Log("Active Weapon is Empty or is not Compatible Store the Active Weapon");
                }
                else if (Weapon == null)                               //Means there's no weapon active so draw it
                {
                    TryInstantiateWeapon(Next_Weapon);
                    Holster_SetActive(Weapon.HolsterID);
                    Draw_Weapon();

                }
                else if (Weapon.Equals(Next_Weapon))                         //You are trying to draw the same weapon
                {
                    if (!CombatMode)
                    {
                        Draw_Weapon();
                        if (debug) Debug.Log("Active weapon is the same as the NEXT Weapon and we are NOT in Combat so DRAW");
                    }
                    else
                    {
                        Store_Weapon();
                        if (debug) Debug.Log("Active weapon is the same as the NEXT Weapon and we ARE  in Combat so STORE");
                    }
                }
                else                                                                //If the weapons are different Swap it
                {
                    StartCoroutine(SwapWeaponsInventory(WeaponGo));
                    if (debug) Debug.Log("Active weapon is DIFFERENT to the NEXT weapon so Switch" + WeaponGo);
                }
            }
        }

        /// <summary>Sets the weapon equipped by an External Source</summary>
        public virtual void Equip_External_Fast(GameObject WeaponGo)
        {
            if (Active && UseInventory && CheckRidingOnly)
            {
                if (!MTools.CompareOR(Mathf.Abs(WeaponAction), WA.Idle, WA.None)) return; //If is not on any of these states then Dont Equip..

                StopAllCoroutines();

                var Next_Weapon = WeaponGo != null ? WeaponGo.GetComponent<MWeapon>() : null;

                if (Next_Weapon == null)                               //That means Store the weapon
                {
                    UnEquip_FAST();
                }
                else if (Weapon == null)                               //Means there's no ACTIVE weapon Equip the newone
                {
                    TryInstantiateWeapon(Next_Weapon);
                    Holster_SetActive(Weapon.HolsterID);
                    Equip_FAST();
                }
                else if (!Weapon.Equals(Next_Weapon))                         //You are trying to draw the same weapon
                {
                    UnEquip_FAST();
                    TryInstantiateWeapon(Next_Weapon);
                    Equip_FAST();
                    if (debug) Debug.Log("Active weapon is DIFFERENT to the NEXT weapon so Switch" + WeaponGo);
                }
            }
        }

        #region Attack Callbacks
        /// <summary> Start the Main Attack Logic </summary>
        public virtual void MainAttack()
        {
            if (WeaponIsActive)
            {
                if (!Aimer.Active) Aimer.Calculate(); //Quick Aim Calculation in case the Aimer is Disabled


                Weapon.MainAttack_Start(this);
                OnMainAttackStart.Invoke(Weapon.gameObject);
            }
        }

        /// <summary>Called to release the Main Attack (Ex release the Arrow on the Bow, the Melee Atack)</summary>
        public virtual void MainAttackReleased()
        {
            if (WeaponIsActive) Weapon.MainAttack_Released();
        }

        public virtual void MainAttack(bool value)
        {
            if (value) MainAttack(); else MainAttackReleased();
        }

        public virtual void SecondAttack()
        {
            if (WeaponIsActive)  Weapon.SecondaryAttack_Start(this);
        }

        public virtual void SecondAttackReleased()
        {
            if (WeaponIsActive) Weapon.SecondaryAttack_Released();
        }

        public virtual void SecondAttack(bool value)
        {
            if (value) SecondAttack(); else SecondAttackReleased();
        }


        /// <summary>If the Weapon can be Reload ... Reload it!</summary>
        public virtual void ReloadWeapon()
        {
            if (WeaponIsActive && Weapon.Reload())
            {
                ExitAim();
            }
        }

        public void ExitAim()
        {
            if (DisableAim) Aimer.Active = false;
            else Aimer.ExitAim();
        }

        public void WeaponIK(bool value) => WeaponIKW = value ? 1 : 0;

        #endregion

        #region Inputs

        protected void GetAttack1Input(bool inputValue)
        {
            if (inputValue) MainAttack();
            else MainAttackReleased();
        }

        protected void GetAttack2Input(bool inputValue)
        {
            if (inputValue) SecondAttack();
            else SecondAttackReleased();
        }

        protected void GetReloadInput(bool inputValue)
        {
            if (inputValue) ReloadWeapon();
        }

        #endregion
     
        /// <summary>Sets IsinCombatMode=false, ActiveAbility=null,WeaponType=None and Resets the Aim Mode. DOES **NOT** RESET THE ACTION TO NONE
        /// This one is Used Internally... since the Action will be set by the Store and Unequip Weapons</summary>
        public virtual void ResetCombat()
        {
            WeaponType = WeaponAction = WA.None;
            Weapon?.ResetWeapon();
            CombatMode = Aim = false;
            ExitAim();
            if (debug) Debug.Log($"<B>{name}: <color=red>[Reset Combat]</color></B>");
        }


        /// <summary>
        /// Execute Draw Weapon Animation without the need of an Active Weapon
        /// This is used when is Called Externally for other script (Integrations) </summary>
        /// <param name="holster">Which holster the weapon is going to be draw from</param>
        /// <param name="weaponType">What type of weapon</param>
        /// <param name="isRightHand">Is it going to be draw with the left or the right hand</param>
        public virtual void Draw_Weapon(int holster, int weaponType, bool isRightHand)
        {
            ExitAim();

            ResetCombat();
            Holster_SetActive(holster);
            WeaponType = weaponType;
            WeaponAction = (WA.Draw * 100 + holster);

            SendMessage(isRightHand ? FreeRightHand : FreeLeftHand, true);  //IK REINS Message

            if (debug) Debug.Log($"<B>{name}: <b>  Draw with No Active Weapon");  //Debug
        }

        /// <summary>Execute Store Weapon Animation without the need of an Active Weapon
        /// This is used when is Called Externally for other script (Integrations) </summary>
        /// <param name="holster">The holster that the weapon is going to be Stored</param>
        /// <param name="isRightHand">is whe Weapon Right Handed?</param>
        public virtual void Store_Weapon(int holster, bool isRightHand)
        {
            WeaponType = 0;                                                  //Set the weapon ID to None (For the correct Animations)
            Holster_SetActive(holster);

            WeaponAction = StoreWeaponID;    //Set the  Weapon Action  to Store Weapons 

            ResetCombat();
            if (debug) Debug.Log($"<B>{name}: <b> Store with No Active Weapon ");
        }

        protected int StoreWeaponID => (WA.Store * 100 + Weapon.HolsterAnim);
        protected int DrawWeaponID => (WA.Draw * 100 + Weapon.HolsterAnim);

        /// <summary>Get a Callback From the RiderCombat Layer Weapons States</summary>
        public virtual void WeaponSound(int SoundID) => Weapon?.PlaySound(SoundID);

        #region Animator Methods
        /// <summary>Messages Get from the Animator</summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value)
        {
            bool w = Weapon ? Weapon.OnAnimatorBehaviourMessage(message, value) : false;
            return this.InvokeWithParams(message, value) || w;
        }

        public void SetAnimParameter(int hash, int value) => Anim.SetInteger(hash, value);

        /// <summary>Set a float on the Animator</summary>
        public void SetAnimParameter(int hash, float value) => Anim.SetFloat(hash, value);

        /// <summary>Set a Bool on the Animator</summary>
        public void SetAnimParameter(int hash, bool value) => Anim?.SetBool(hash, value);
        #endregion
    }
}