using System;
using MalbersAnimations.Weapons;
using UnityEngine;

namespace MalbersAnimations.HAP
{
    /// <summary>
    /// This Class holds the common methods for GunCombatIK and GunCombatFK
    /// </summary>
    public abstract class GunCombat : RiderCombatAbility
    {
        private IGun Gun;
        protected bool isReloading;                  //If the gun is currently Reloading
        protected float currentFireRateTime = 0;
        protected InputButton DefaultInputType;

        public override void ActivateAbility()
        {
            Gun = RC.ActiveWeapon as IGun;  //Store the Current Gun
            isReloading = false;

            if (Gun == null) return;
            currentFireRateTime = 0;
        }


        public override void UpdateAbility()
        {
            if (Gun != null && Gun.IsAiming != RC.Aim)
            {
                Gun.IsAiming = RC.Aim;         //This  is used to send to the Gun that was Aiming and also invokes the Event On Aiming
            }
        }


        public override void ReloadWeapon()
        {
            PistolReload();
        }

        public override void MainAttack()
        {
            if (RC.Aim && RC.WeaponAction != WA.Fire_Proyectile)
            {
                PistolAttack();                                                          //Fire Pistol
            }
        }

        /// <summary> Pistol Attack Mode</summary>
        protected virtual void PistolAttack()
        {
            if (isReloading) return;

            if (Gun.AmmoInChamber > 0)                                                           //If there's Ammo on the chamber
            { 
                RC.WeaponAction = WA.Fire_Proyectile;                                            //Set the Weapon Action to Fire Proyectile   Let the Animator Know!!!

                Gun.FireProyectile(RC.Aimer.AimHit);

                RC.OnAttack.Invoke(RC.ActiveWeapon);                                                        //Invoke the On Attack Event
                currentFireRateTime = Time.time;
            }
            else
            {
                if (Time.time - currentFireRateTime > 0.5f)
                {
                    Gun.PlaySound(4);                                                                       //Play Empty Sound Which is stored in the 4 Slot     
                    currentFireRateTime = Time.time;
                }
            }
        }

        /// <summary>If the Weapon is a Gun Reload </summary>
        public virtual void PistolReload()
        {
            if (Gun.Reload())                                                                                       //If the Gun can Reload Activate the Reload Animation and Reload
            {
                RC.WeaponAction = RC.Weapon_is_RightHand ? WA.ReloadRight : WA.ReloadLeft;     //Set the Animator to the Reload Animations
            }
            else
            {
                RC.WeaponAction = WA.Idle;
            }
        }


        public override void ResetAbility()
        {
            base.ResetAbility();
            if (Gun == null) return;

            isReloading = false;
            Gun = null;

        }

        /// <summary> If finish reload but is still aiming go to the Aiming animation **CALLED BY THE ANIMATOR**</summary>
        public virtual void FinishReload()
        {
            RC.WeaponAction = RC.Aim ? (RC.Weapon_is_RightHand ? WA.AimRight : WA.AimLeft) : WA.Idle;
        }

        ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        /// <summary> Set if is in the reloading state  **CALLED BY THE ANIMATOR**</summary>
        public void IsReloading(bool value)
        {
            isReloading = value;
        }


        /// <summary> Invoked from the Animator</summary>
        public void ResetDoubleShoot()
        {
            RC.Anim.SetInteger(Hash.IDInt, 0);
        }

        //protected void EnableAimIKBehaviour(bool value)
        //{
        //    AimIKBehaviour[] aimIK = RC.Anim.GetBehaviours<AimIKBehaviour>();

        //    foreach (var item in aimIK)
        //    {
        //        item.active = value;
        //    }
        //}
    }
}