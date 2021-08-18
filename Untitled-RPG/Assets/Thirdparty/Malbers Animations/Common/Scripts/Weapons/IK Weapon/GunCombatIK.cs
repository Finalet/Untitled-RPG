using System;
using UnityEngine;

namespace MalbersAnimations.Weapons
{
    [CreateAssetMenu(menuName = "Malbers Animations/Weapons/Gun Combat IK")]
    public class GunCombatIK : OldIKProfile
    {
        private static readonly Keyframe[] KeyFrames = { new Keyframe(0, 0.61f), new Keyframe(1.25f, 0.61f), new Keyframe(2, 0.4f) };

        [Space]
        public Vector3 RightHandOffset;
        public Vector3 LeftHandOffset;
        public float Smoothness = 2;
        public float HandDistance = 1;
        public AnimationCurve HandIKDistance = new AnimationCurve(KeyFrames);


        public override void OnAnimator_IK(IMWeaponOwner RC)
        {
            float DeltaTime = RC.Anim.updateMode == AnimatorUpdateMode.AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;

            bool isRightHand = RC.Weapon.IsRightHanded;
            Vector3 RayOrigin = isRightHand ? RC.RightShoulder.position : RC.LeftShoulder.position;
            Vector3 AimDirection = RC.AimDirection;

            Ray RayHand = new Ray(RayOrigin, AimDirection);

            Debug.DrawRay(RayHand.origin, RayHand.direction * 20, Color.clear);

            RC.WeaponIKW = Mathf.MoveTowards(RC.WeaponIKW, RC.Aim ? 1 : 0, DeltaTime * Smoothness);
           // RC.IKWeight = RC.Aim ? 1:0 ;

            var Action = Mathf.Abs(RC.WeaponAction);
            if (Action  == WA.Reload)  RC.WeaponIKW = 0;
            var HandIK = 1;
            if (Action == WA.Fire_Projectile) HandIK = 0;


            if (HandIK != 0)
            {
                float Hand_Distance = isRightHand ? HandIKDistance.Evaluate(RC.HorizontalAngle) : HandIKDistance.Evaluate(-RC.HorizontalAngle); //Values for the Distance of the Arm while rotating

                Hand_Distance *= HandDistance;

                 //Vector3 LookDirection = RC.MainCamera.transform.forward;
                 //Vector3 HandPosition = isRightHand ? RC.RightHand.position : RC.LeftHand.position;
                 Vector3 HandOffset = isRightHand ? RightHandOffset : LeftHandOffset;

                Vector3 IKPoint = RayHand.GetPoint(Hand_Distance);
                //Vector3 LookDirectionFromHand = (RC.Aimer.AimHit.point - HandPosition).normalized;

                var HandRotation =
                    Quaternion.LookRotation(AimDirection) * Quaternion.Euler(HandOffset); //Set the Aim Look Rotation for the  Right or Left Hand

                var ikGoal = isRightHand ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;  //Set the IK goal acording the Right or Left Hand

                //Arm IK
                RC.Anim.SetIKPosition(ikGoal, IKPoint);
                RC.Anim.SetIKPositionWeight(ikGoal, HandIK);

                RC.Anim.SetIKRotation(ikGoal, HandRotation);
                RC.Anim.SetIKRotationWeight(ikGoal, HandIK);
            }


            if (RC.WeaponIKW != 0 && RC.Aim)
            {
                //HeadIK
                RC.Anim.SetLookAtPosition(RayHand.GetPoint(10));
                RC.Anim.SetLookAtWeight(1 * RC.WeaponIKW, 0.1f * RC.WeaponIKW , 1 * RC.WeaponIKW);
            }
        }
    }
}