using System;
using UnityEngine;

namespace MalbersAnimations.Weapons
{
    [CreateAssetMenu(menuName = "Malbers Animations/Weapons/Gun Combat FK")]
    public class GunCombatFK : OldIKProfile
    {
        public float AimHorizontalOffset = 20;                          //Adjusment for the Aim body Offet (to view better the hand)

        [Header("Right Offsets")]
        public Vector3 RightShoulderOffset = new Vector3(-90, 90, 0);
        public Vector3 RightHandOffset = new Vector3(-90, 90, 0);
        [Header("Left Offsets")]
        public Vector3 LeftShoulderOffset = new Vector3(90, 90, 0);
        public Vector3 LeftHandOffset = new Vector3(90, 90, 0);
        [Space]
        public Vector3 HeadOffset = new Vector3(0, -90, -90);
        [Range(0, 1)]
        public float headLookWeight = 0.7f;

        protected Quaternion Delta_Rotation;

        public override void LateUpdate_IK(IMWeaponOwner RC)
        {
            if (RC.Aim && RC.WeaponAction != WA.Reload)
            {
                var UpVector = Vector3.up;

                Quaternion AimRotation = Quaternion.LookRotation(RC.AimDirection, UpVector); //Get the Rotation ...

                Vector3 ShoulderRotationAxis = RC.Aimer.AimTarget ? Vector3.Cross(UpVector, RC.AimDirection).normalized : RC.Aimer.MainCamera.transform.right;

                float angle = (Vector3.Angle(Vector3.up, RC.AimDirection) - 90);


                Debug.DrawRay(RC.Weapon.IsRightHanded ? RC.RightShoulder.position : RC.LeftShoulder.position, ShoulderRotationAxis * 0.33f, Color.yellow);

                if (RC.Weapon.IsRightHanded)                                                                                                                //If the Weapon is RIGHT Handed  
                {
                    RC.RightShoulder.RotateAround(RC.RightShoulder.position, ShoulderRotationAxis, angle);          //Rotate Up/Down the Right Shoulder to AIM Up/Down
                    RC.RightShoulder.rotation *= Quaternion.Euler(RightShoulderOffset);

                    if (!RC.Aimer.AimTarget)
                    {
                        RC.RightShoulder.RotateAround(RC.RightShoulder.position, Vector3.up, (RC.AimingSide ? 0 : -AimHorizontalOffset));                   //Offset the RIGHT Arm for better view
                    }
                }
                else                                                                                                                                            //If the Weapon is LEFT Handed  
                {
                    RC.LeftShoulder.RotateAround(RC.LeftShoulder.position, ShoulderRotationAxis, angle);                                                        //Rotate Up/Down the Left Shoulder to AIM Up/Down

                    RC.LeftShoulder.rotation *= Quaternion.Euler(LeftShoulderOffset);

                    if (!RC.Aimer.AimTarget)
                    {
                        RC.LeftShoulder.RotateAround(RC.LeftShoulder.position, Vector3.up, (RC.AimingSide ? AimHorizontalOffset : 0));                      //Offset the LEFT Arm for better view  
                    }
                }


                RC.Head.rotation = Quaternion.Slerp(RC.Head.rotation, AimRotation * Quaternion.Euler(HeadOffset), headLookWeight);                             //Head Look Rotation

                if (RC.WeaponAction != WA.Fire_Projectile)                                                                                           //Activate the Hand AIM DIRECTION  when is not Firing or Reloading
                {
                    if (RC.Weapon.IsRightHanded)
                    {
                        RC.RightHand.rotation = Delta_Rotation * Quaternion.Euler(RightHandOffset);
                    }
                    else
                    {
                        RC.LeftHand.rotation = Delta_Rotation * Quaternion.Euler(LeftHandOffset);
                    }

                    Delta_Rotation = Quaternion.Lerp(Delta_Rotation, AimRotation, Time.deltaTime * 20);                                                                 //Smoothly AIM the Hand
                }
            }
        }
    }
}