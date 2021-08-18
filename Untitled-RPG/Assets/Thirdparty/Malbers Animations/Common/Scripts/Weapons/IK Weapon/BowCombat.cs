using System;
using UnityEngine;

namespace MalbersAnimations.Weapons
{
    /// <summary>Ability that it will Manage the Bow Combat System while Riding</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Weapons/Bow Profile")]
    public class BowCombat : OldIKProfile
    {
        [Space, Header("Right Handed Bow Offsets")]
        public Vector3 ChestRight = new Vector3(25, 0, 0);
        public Vector3 ShoulderRight = new Vector3(5, 0, 0);
        public Vector3 HandRight;

        [Header("Left Handed Bow Offsets")]
        public Vector3 ChestLeft = new Vector3(-25, 0, 0);
        public Vector3 ShoulderLeft = new Vector3(-5, 0, 0);
        public Vector3 HandLeft;


        //public override void OnAnimator_IK(IMWeaponOwner RC)
        //{
        //    FixAimPoseBow2(RC);

        //    m_IKProfile?.ApplyOffsets(RC.Anim, RC.Aimer.AimDirection);
        //}

        public override void LateUpdate_IK(IMWeaponOwner RC)
        {
            FixAimPoseBow1(RC);
        }

        //protected virtual void FixAimPoseBow2(IMWeaponOwner RC)
        //{
        //    if (RC.Aim)
        //    {
        //        RC.Anim.SetLookAtWeight(1, 1, 1, 1, 0.1f);
        //        RC.Anim.SetLookAtPosition(RC.Aimer.AimPoint);
        //    }   
        //}

        /// <summary>This will rotate the bones of the character to match the AIM direction </summary>
        protected virtual void FixAimPoseBow1(IMWeaponOwner RC)
        {
            var Bow = RC.Weapon as MShootable;

            if (RC.Aim)
            {
                RC.Anim.Update(0);

                var UpVector = Vector3.up;

                float Weight =
                   Bow.IsRightHanded ? Bow.AimLimit.Evaluate(RC.HorizontalAngle) : Bow.AimLimit.Evaluate(-RC.HorizontalAngle); //The Weight evaluated on the AnimCurve

                Vector3 AimDirection = RC.AimDirection;
                 Quaternion AimLookAt = Quaternion.LookRotation(AimDirection, UpVector);


                var angle = RC.Aimer.VerticalAngle * Weight;

                Vector3 ShoulderRotationAxis = Vector3.Cross(UpVector, AimDirection).normalized;

                Debug.DrawRay(RC.Chest.position, ShoulderRotationAxis, Color.yellow);

                RC.Chest.RotateAround(RC.Chest.position, ShoulderRotationAxis, angle/2); //Nicely Done!! 

                if (Bow.IsRightHanded)
                {
                    RC.Chest.rotation *= Quaternion.Euler(ChestRight);
                    RC.RightHand.rotation *= Quaternion.Euler(HandRight);
                    RC.RightShoulder.rotation = Quaternion.Lerp(RC.RightShoulder.rotation, AimLookAt * Quaternion.Euler(ShoulderRight), Weight); // MakeDamage the boy always look to t
                }
                else
                {
                    RC.Chest.rotation *= Quaternion.Euler(ChestLeft);
                    RC.LeftHand.rotation *= Quaternion.Euler(HandLeft);
                    RC.LeftShoulder.rotation = Quaternion.Lerp(RC.LeftShoulder.rotation, AimLookAt * Quaternion.Euler(ShoulderLeft), Weight); // MakeDamage the boy always look to t
                }
            }
        }
    }
}