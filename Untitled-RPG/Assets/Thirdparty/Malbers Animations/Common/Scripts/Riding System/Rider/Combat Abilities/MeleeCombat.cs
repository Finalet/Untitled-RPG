using MalbersAnimations.Weapons;
using UnityEngine;

namespace MalbersAnimations.HAP
{
    [CreateAssetMenu(menuName = "Malbers Animations/HAP/MeleeCombat")]
    public class MeleeCombat : RiderCombatAbility
    {
        [Tooltip("Time before attacking again with melee")]
        public float meleeAttackDelay = 0.5f;
        [Tooltip("The Rider will Attack with the Melee Weapon depending on wheter the camera is on the Left or Right side of the Rider")]
        public bool UseCameraSide = true;
        [Tooltip("Inverts the Directions of the Melee Attacks")]
        public bool InvertCameraSide;

        bool isAttacking = false;
        float timeOnAttack = 0;


        public override void ActivateAbility()
        {
            base.ActivateAbility();
            timeOnAttack = 0;
            isAttacking = false;
        }


        public override void MainAttack()
        {
            if (!RC.Active) return;

            if (!isAttacking)
            {
                if (UseCameraSide)
                {
                    bool side = RC.CameraSide;
                    RiderMeleeAttack(InvertCameraSide ? side : !side);
                }
                else
                {
                    RiderMeleeAttack(false);                 //Attack with Left Hand
                }
            }
        }

        public override void MainAttackHold()
        {
            CheckAttacking();
        }

        public override void SecondaryAttack()
        {
            if (!RC.Active) return;
            if (UseCameraSide) return;

            if (!isAttacking)
                RiderMeleeAttack(true);                 //Attack with Left Hand
        }

        public override void SecondaryAttackHold()
        {
            CheckAttacking();
        }

        void CheckAttacking()
        {
            if (!RC.Active) return;

            if (isAttacking)
            {
                if (Time.time - timeOnAttack > meleeAttackDelay)
                {
                    isAttacking = false;
                    if (RC.ActiveWeapon.MainAttack) MainAttack();
                    else if (RC.ActiveWeapon.SecondAttack) SecondaryAttack();
                }
            }
        }

        /// <summary>Set all parameters for Melee Attack </summary>
        /// <param name="rightSide">true = Right Arm.. false = Left Arm</param>
        protected virtual void RiderMeleeAttack(bool rightSide)
        {
            Anim.SetInteger(Hash.IDInt, -99);                           //Avoid to execute the Lower Attack Animation clip for the rider ►?????◄

            int attackID;

            if (RC.Weapon_is_RightHand)                                 //If the Active Weapon Is Right Handed
            {
                if (rightSide) attackID = Random.Range(1, 3);           // Set the Attacks for the RIGHT Side with the 'Right Hand'
                else attackID = Random.Range(3, 5);                     // Set the Attacks for the LEFT Side with the 'Right Hand'
            }
            else                                                        //Else Active Weapon is Left Handed
            {
                if (rightSide) attackID = Random.Range(7, 9);           // Set the Attacks for the RIGHT Side with the 'Left Hand'
                else attackID = Random.Range(5, 7);                     // Set the Attacks for the LEFT Side with the 'Left Hand'
            }

            RC.WeaponAction = (WA)attackID;

            isAttacking = true;
            timeOnAttack = Time.time;

            RC.OnAttack.Invoke(RC.ActiveWeapon);                      //Invoke the OnAttack Event
        }

        /// <summary>Call From the Animator in the melee state that the weapon can cause damage</summary>
        public virtual void OnCauseDamage(bool value)
        {
            (RC.ActiveWeapon as IMelee).CanDoDamage(value);
        }
    }
}