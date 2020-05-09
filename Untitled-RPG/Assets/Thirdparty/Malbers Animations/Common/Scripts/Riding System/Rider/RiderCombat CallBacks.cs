using UnityEngine;
using System.Collections;
using System.Reflection;
using MalbersAnimations.Weapons;

namespace MalbersAnimations.HAP
{
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    /// CALLBACKS
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    public partial class RiderCombat
    {

        #region Attack Callbacks
        /// <summary> Start the Main Attack Logic </summary>
        public virtual void MainAttack()
        {
            if (IsWeaponActive && WeaponCanAttack)
            {
                ActiveAbility.MainAttack();
                ActiveWeapon.MainAttack = true; //Enable this because the Hold can be Activated (Some weapons Attack After the MainAttackIs released)
            }
        }

        /// <summary>Called to release the Main Attack (Ex release the Arrow on the Bow, the Melee Atack)</summary>
        public virtual void MainAttackReleased()
        {
            if (IsWeaponActive)
            {
                ActiveAbility.MainAttackReleased();
                ActiveWeapon.MainAttack = false;
            }
        }

        public virtual void MainAttack(bool value)
        {
            if (value)
                MainAttack();
            else
                MainAttackReleased();
        }

        public virtual void SecondAttack()
        {
            if (IsWeaponActive && WeaponCanAttack)
            {
                ActiveAbility.SecondaryAttack();
                ActiveWeapon.SecondAttack = true;
            }
        }


        public virtual void SecondAttackReleased()
        {
            if (IsWeaponActive)
            {
                ActiveAbility.SecondaryAttackReleased();
                ActiveWeapon.SecondAttack = false;
            }
        }

        public virtual void SecondAttack(bool value)
        {
            if (value)
                SecondAttack();
            else
                SecondAttackReleased();
        }


        /// <summary>If the Weapon can be Reload ... Reload it!</summary>
        public virtual void ReloadWeapon()
        {
            if (IsWeaponActive)
                ActiveAbility.ReloadWeapon();
        }
        #endregion

        /// <summary>is there an Ability Active and the Active Weapon is Active too</summary>
        bool IsWeaponActive => ActiveAbility && ActiveWeapon.Active && Active;


        /// <summary>Send by the Weapons when they are Enabled (CALLED BY SEND MESSAGE MWEAPON)</summary>
        public virtual void WeaponEnabled(MWeapon weapon)
        {
            if (CombatMode && ActiveWeapon.WeaponID == weapon.WeaponID) //means that the weapon that we just use has been disabled
            {
                if (debug) Debug.Log($"Active Weapon <B>{weapon.name}</B> Enabled");
            }
        }


        /// <summary>Send by the Weapons when they are Disable (CALLED BY SEND MESSAGE MWEAPON)</summary>
        public virtual void WeaponDisabled(MWeapon weapon)
        {
            if (CombatMode && ActiveWeapon.WeaponID == weapon.WeaponID) //means that the weapon that we just use has been disabled
            {
                if (debug) Debug.Log($"Active Weapon <B>{weapon.name}</B> Disabled");

                if (Aim)
                {
                    Aim = false;
                }
            }
        }



        #region Holders
        public virtual void Change_Weapon_Holder_Back()
        {
            if (!Rider.IsRiding) return;                                                                //Just work while is in the horse
            if (UseHolders)
            {
                if (WeaponAction == WA.None || WeaponAction == WA.Idle ||
                    WeaponAction == WA.AimLeft || WeaponAction == WA.AimRight)   //Toogle weapon only If we are on Action none,Idle or Aiming
                {
                    Change_Weapon_Holder_Inputs(WeaponHolder.Back);
                }
            }
        }

        public virtual void Change_Weapon_Holder_Left()
        {
            if (!Rider.IsRiding) return;                                                                //Just work while is in the horse
            if (UseHolders)
            {
                if (WeaponAction == WA.None || WeaponAction == WA.Idle ||
                    WeaponAction == WA.AimLeft || WeaponAction == WA.AimRight)   //Toogle weapon only If we are on Action none,Idle or Aiming
                {
                    Change_Weapon_Holder_Inputs(WeaponHolder.Left);
                }
            }
        }

        public virtual void Change_Weapon_Holder_Right()
        {
            if (!Rider.IsRiding) return;                                                                //Just work while is in the horse
            if (UseHolders)
            {
                if (WeaponAction == WA.None || WeaponAction == WA.Idle ||
                    WeaponAction == WA.AimLeft || WeaponAction == WA.AimRight)   //Toogle weapon only If we are on Action none,Idle or Aiming
                {
                    Change_Weapon_Holder_Inputs(WeaponHolder.Right);
                }
            }
        }
        #endregion

        public virtual void SetAim(bool value)
        {
            if (!CombatMode) return;
            if (!ActiveAbility?.CanAim) return;
            if (!ActiveWeapon.Active) return;

            if (ToggleAim)
            {
                if (value) Aim = !Aim;
            }
            else
            {
                Aim = value;
            }
        }

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

        #region IKREINS
        /// <summary>This will be call by the Animator Layer "Rider Arm Right"  to free the Reins from the Right Hand </summary>
        public virtual void FreeRightHand(bool value)
        {
            if (Rider.Montura != null)
            {
                IKReins IK_Reins = Rider.Montura.transform.GetComponent<IKReins>();

                if (IK_Reins)
                    IK_Reins.FreeRightHand(value);                                      //Send to the Reins Script that the  RIGHT Hand is free or not
            }
        }
        /// <summary>This will be call by the Animator Layer "Rider Arm Left"  to free the Reins from the Left Hand </summary>
        public virtual void FreeLeftHand(bool value)
        {
            if (Rider.Montura != null)
            {
                IKReins IK_Reins = Rider.Montura.transform.GetComponent<IKReins>();

                if (IK_Reins)
                    IK_Reins.FreeLeftHand(value); //Send to the Reins Script that the LEFT Hand is free or not
            }
        }
        #endregion


        /// <summary>Resets the Rider Combat Mode and **Reset** the Action to NONE, also ask for Store the Current Weapon</summary>
        /// <param name="storeWeapon">if the weapon needs to be stored when this method is called</param>
        public virtual void ResetRiderCombat(bool storeWeapon)
        {
            ResetRiderCombat();
            WeaponAction = WA.None;
            if (storeWeapon) Store_Weapon();  //Store when Dismounting
        }

        /// <summary>Sets IsinCombatMode=false, ActiveAbility=null,WeaponType=None and Resets the Aim Mode. DOES **NOT** RESET THE ACTION TO NONE
        /// This one is Used Internally... since the Action will be set by the Store and Unequip Weapons</summary>
        public virtual void ResetRiderCombat()
        {
            CombatMode = false;
            WeaponType = null;
            Aim = false;
            ResetActiveAbility();
        }


        /// <summary>
        /// Execute Draw Weapon Animation without the need of an Active Weapon
        /// This is used when is Called Externally for other script (Integrations) </summary>
        /// <param name="holder">Which Holder the weapon is going to be draw from</param>
        /// <param name="weaponType">What type of weapon</param>
        /// <param name="isRightHand">Is it going to be draw with the left or the right hand</param>
        public virtual void Draw_Weapon(WeaponHolder holder, WeaponID weaponType, bool isRightHand)
        {
            ResetRiderCombat();

            WeaponAction = isRightHand ? WA.DrawFromRight : WA.DrawFromLeft;

            SetWeaponIdleAnimState(weaponType, isRightHand);
            WeaponType = weaponType;

            //LinkAnimator();
            if (debug) Debug.Log("Draw with No Active Weapon");  //Debug
        }

        /// <summary>Execute Store Weapon Animation without the need of an Active Weapon
        /// This is used when is Called Externally for other script (Integrations) </summary>
        /// <param name="holder">The holder that the weapon is going to be Stored</param>
        /// <param name="isRightHand">is whe Weapon Right Handed?</param>
        public virtual void Store_Weapon(WeaponHolder holder, bool isRightHand)
        {
            WeaponType = null;                                                  //Set the weapon ID to None (For the correct Animations)

            ActiveHolderSide = holder;
            WeaponAction = isRightHand ? WA.StoreToRight : WA.StoreToLeft; //Set the  Weapon Action to -1 to Store Weapons to Right or -2 to left

            ResetRiderCombat();
            if (debug) Debug.Log("Store with No Active Weapon ");
        }

        /// <summary>
        /// Sets in the animator the Correct Idle Animation State for the Right/Left Hand Ex: "Melee Idle Right Hand" which is the exact name in the Animator
        /// </summary>
        public virtual void SetWeaponIdleAnimState(bool IsRightHand)
        {
            string WeaponIdle = " Idle " + (IsRightHand ? "Right" : "Left") + " Hand";

            WeaponIdle = ActiveWeapon.WeaponType.name + WeaponIdle;        //Melee Idle Right/Left Hand

            Anim.CrossFade(WeaponIdle, 0.25f, ActiveWeapon.RightHand ? Layer_RiderArmRight : Layer_RiderArmLeft); //Active the Layer Right/Left Arm for Idle Pose
        }

        /// <summary>
        /// Sets in the animator the Correct Idle Animation State for the Right/Left Hand 
        /// Ex: "Melee Idle Right Hand" which is the exact name in the Animator
        /// </summary>
        public virtual void SetWeaponIdleAnimState(WeaponID weapon, bool isRightHand)
        {
            WeaponType = weapon;
            SetWeaponIdleAnimState(isRightHand);
        }

        /// <summary>Returns the Weapon type of the Active Weapon</summary>
        public virtual WeaponID GetWeaponType() { return ActiveWeapon?.WeaponType; }

        /// <summary>Get a Callback From the RiderCombat Layer Weapons States</summary>
        public virtual void WeaponSound(int SoundID) { ActiveWeapon?.PlaySound(SoundID); }

        /// <summary>This will recieve the messages Animator Behaviors the moment the rider make an action on the weapon</summary>
        public virtual void Action(int value)
        {
            if ((WeaponAction == WA.AimLeft || WeaponAction == WA.AimRight) && !ActiveWeapon.Active) return; //Meaning the Action cannot Aim because the ActiveWeapon is not Active

            WeaponAction = (WA)value;
        }

        /// <summary>Enable an Input on the Mount Animal</summary>
        public virtual void EnableMountInput(string input) { Rider.Montura?.MountInput?.EnableInput(input); }

        /// <summary>Disable an Input on the Mount Animal</summary>
        public virtual void DisableMountInput(string input) { Rider.Montura?.MountInput?.DisableInput(input); }

        /// <summary>Messages Get from the Animator</summary>
        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);

            if (ActiveAbility)
            {
                ActiveAbility.ListenAnimator(message, value);
            }
        }
    }
}