using System;
using MalbersAnimations.Weapons;
using UnityEngine;

namespace MalbersAnimations.HAP
{
    [CreateAssetMenu(menuName = "Malbers Animations/HAP/Gun Combat IK")]
    public class GunCombatIK : GunCombat
    {
        private static Keyframe[] KeyFrames = { new Keyframe(0, 0.61f), new Keyframe(1.25f, 0.61f), new Keyframe(2, 0.4f) };

        [Space]
        public Vector3 RightHandOffset;
        public Vector3 LeftHandOffset;
        public float  AimSmooth =2;
        private float IKWeight;
        private float IKWeightHead;
        public AnimationCurve HandIKDistance = new AnimationCurve(KeyFrames);


        public override void ResetAbility()
        {
            base.ResetAbility();
           IKWeight = IKWeightHead = 0;
        }
        public override void ActivateAbility()
        {
            base.ActivateAbility();
            IKWeight = IKWeightHead = 0;
        }

        public override void OnAbilityIK()
        {
            bool isRightHand = RC.Weapon_is_RightHand;
            Vector3 RayOrigin = isRightHand ? RC.RightShoulder.position : RC.LeftShoulder.position;
            Vector3 AimDirection = RC.AimDirection;

            Ray RayHand = new Ray(RayOrigin, AimDirection);

            IKWeight = Mathf.MoveTowards(IKWeight, RC.Aim ? 1 : 0, RC.DeltaTime * AimSmooth);
            IKWeightHead = Mathf.MoveTowards(IKWeightHead, RC.Aim ? 1 : 0, RC.DeltaTime * AimSmooth);

            if (RC.WeaponAction == WA.Fire_Proyectile || RC.WeaponAction == WA.ReloadLeft || RC.WeaponAction == WA.ReloadRight || isReloading)
            {
                IKWeight = 0;

                if (RC.WeaponAction != WA.Fire_Proyectile)
                {
                    IKWeightHead = 0;
                }
            }


            if (IKWeight != 0)
            {
                float Hand_Distance = isRightHand ? HandIKDistance.Evaluate(1 + RC.HorizontalAngle) : HandIKDistance.Evaluate(1 - RC.HorizontalAngle); //Values for the Distance of the Arm while rotating

               
                //Vector3 LookDirection = RC.MainCamera.transform.forward;
                Vector3 HandPosition = isRightHand ? RC.RightHand.position : RC.LeftHand.position;
                Vector3 HandOffset = isRightHand ? RightHandOffset : LeftHandOffset;

                Vector3 IKPoint = RayHand.GetPoint(Hand_Distance);
                Vector3 LookDirectionFromHand = (RC.Aimer.AimHit.point - HandPosition).normalized;

                var HandRotation =
                    Quaternion.LookRotation(AimDirection) * Quaternion.Euler(HandOffset); //Set the Aim Look Rotation for the  Right or Left Hand

                var ikGoal = isRightHand ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;  //Set the IK goal acording the Right or Left Hand

                //Arm IK
                Anim.SetIKPosition(ikGoal, IKPoint);
                Anim.SetIKPositionWeight(ikGoal, IKWeight);

                Anim.SetIKRotation(ikGoal, HandRotation);
                Anim.SetIKRotationWeight(ikGoal, IKWeight);
            }


            if (IKWeightHead != 0 && RC.Aim)
            {
                //HeadIK
                Anim.SetLookAtPosition(RayHand.GetPoint(10));
                Anim.SetLookAtWeight(1 * IKWeightHead, 0.1f * IKWeightHead, 1 * IKWeightHead);
            }
        }
    }
}