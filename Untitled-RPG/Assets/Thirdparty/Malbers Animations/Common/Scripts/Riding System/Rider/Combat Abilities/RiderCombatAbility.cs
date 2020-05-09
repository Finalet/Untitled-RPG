using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Weapons;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.HAP
{
    /// <summary>All the Setup of the Combat Abilities are scripted on the Children of this class</summary>
    /// 
    /// 
    public abstract class RiderCombatAbility : ScriptableObject
    {
        /// <summary>Type of Weapon this Ability can use </summary>
        [Tooltip("Type of Weapon this Ability can use ")]
        public WeaponID WeaponType;
        /// <summary>Does this Ability Require Aiming?</summary>
        [Tooltip("Does this Ability Require Aiming?")]
        public BoolReference CanAim;

        /// <summary>Temporal Rider Combat Reference used for Animator Sent Messages</summary>
        protected RiderCombat RC { get; set; }
        protected Animator Anim;
        protected IMWeapon weapon;



        public virtual void StartAbility(RiderCombat ridercombat)
        {
            RC = ridercombat;                                                               //Get the reference for the RiderCombat Script
            Anim = RC.Anim;
        }


      //  public abstract Transform SetAimOrigin();
        


        /// <summary>Called when the Weapon is Equiped </summary>
        public virtual void ActivateAbility() { }


        ///// <summary>Called on the FixedUpdate of the Rider Combat Script </summary>
        //public virtual void FixedUpdateAbility(RiderCombat RC) { }


        /// <summary>Set the Primary Attack </summary>
        public virtual void MainAttack() { }

        /// <summary>Set when the Primary Attack is Active and Holding</summary>
        public virtual void MainAttackHold() { }

        /// <summary>Set when the Primary Attack is Released (BOW) </summary>
        public virtual void MainAttackReleased() { }
        
        /// <summary>Set the Secondary Attack
        public virtual void SecondaryAttack() { }
        
        /// <summary>Set when the Secondary Attack is Active and Holding</summary>
        public virtual void SecondaryAttackHold() { }

        /// <summary>Set when the Secondary Attack is Released (BOW) </summary>
        public virtual void SecondaryAttackReleased() { }

        /// <summary>Called when the Weapon Start of finish Aiming </summary>
        public virtual void OnWeaponAim(bool aim) { }

        /// <summary> Reload Weapon </summary>
        public virtual void ReloadWeapon() { }


        /// <summary>Called on the Update of the Rider Combat Script </summary>
        public virtual void UpdateAbility() {}


        /// <summary>Called on the Late Update of the Rider Combat Script </summary>
        public virtual void LateUpdateAbility() { }


        /// <summary> Resets the Ability when there's no Active weapon </summary>
        public virtual void ResetAbility()
        {
            if (RC.ActiveWeapon == null) return;

            if (RC.debug)
            {
                Debug.Log("Ability Reseted: "+ name);
            }
        }

        public virtual void ListenAnimator(string Method, object value)
        {
            this.Invoke(Method, value);
        }

        /// <summary>If the Ability can change the Camera Side State for better Aiming and better looks </summary>
        public virtual bool ChangeAimCameraSide()    { return true; }
    

        /// <summary> Stuff Set in the OnAnimatorIK </summary>
        public virtual void OnAbilityIK()  {}
      
        /// <summary>Gets the Tranform for calculating the Aiming</summary>
        public virtual Transform AimRayOrigin()
        {
            return (RC.ActiveWeapon.RightHand ? RC.RightShoulder : RC.LeftShoulder);
        }

        ///// <summary>Not Implemented Yet </summary>
        //public virtual void OnActionChange() { }
    }
}