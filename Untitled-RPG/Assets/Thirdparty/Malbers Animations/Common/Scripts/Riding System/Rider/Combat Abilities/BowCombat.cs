using System;
using MalbersAnimations.Weapons;
using UnityEngine;

namespace MalbersAnimations.HAP
{
    /// <summary>Ability that it will Manage the Bow Combat System while Riding</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/HAP/Bow Combat")]
    public class BowCombat : RiderCombatAbility
    {
        bool isHolding;             //for checking if the Rider is Holding/Tensing the String
        float HoldTime;             //Time pass since the Rider started tensing the string

        private static Keyframe[] KeyFrames =
            { new Keyframe(0, 1), new Keyframe(1.25f, 1), new Keyframe(1.5f, 0), new Keyframe(2f, 0) };


        [Space, Header("Bow Tension")]
        [Range(0, 1)]
        public float MaxArmTension = 1f;
        public AnimationCurve TensionCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });

        [Space, Header("Right Handed Bow Offsets")]
        public Vector3 ChestRight = new Vector3(25, 0, 0);
        public Vector3 ShoulderRight = new Vector3(5, 0, 0);
        public Vector3 HandRight;
        public Vector3 LeftArrowTail;

        [Header("Left Handed Bow Offsets")]
        public Vector3 ChestLeft = new Vector3(-25, 0, 0);
        public Vector3 ShoulderLeft = new Vector3(-5, 0, 0);
        public Vector3 HandLeft;
        public Vector3 RightArrowTail;


        [Space]
        [Tooltip("This Curve is for straightening the aiming Arm while is on the Aiming State")]
        public AnimationCurve AimWeight = new AnimationCurve(KeyFrames);

        protected bool KnotToHand;
        protected Quaternion Delta_Hand;
        protected IBow Bow;

        public override void StartAbility(RiderCombat RC)
        {
            base.StartAbility(RC);
            KnotToHand = false;
        }

        public override void ActivateAbility()
        {
            Bow = RC.ActiveWeapon as IBow;               //Store the Bow
        }

        public override void UpdateAbility()
        {
            BowKnotInHand(); //Update the BowKnot in the Hand if is not Firing
        }

        public override void OnWeaponAim(bool aim)
        {
            if (!aim && isHolding)
            {
                isHolding = false;
                HoldTime = 0;
                Anim.SetFloat(RC.Hash_WHold, 0);            //Reset Hold Animator Values
                (RC.ActiveWeapon as IBow).BendBow(0);                            //Reset the bending of the bow
            }
        }


        //public override void MainAttack()
        //{
        //   // BowAttack();       //Try Firing the Bow
        //}

        public override void MainAttackHold()
        {
            BowHold();
        }


        public override void MainAttackReleased()
        {
            ReleaseArrow();   //Try Releasing the Arrow
        }

        public override void LateUpdateAbility()
        {
            FixAimPoseBow();
        }

        /// <summary>Bow Attack Mode</summary>
        protected virtual void BowHold()
        {
            if (RC.Aim && RC.WeaponAction != WA.Fire_Proyectile)                                                        //Shoot arrows only when is aiming and If we are notalready firing any arrow
            {
                Bow = RC.ActiveWeapon as IBow;               //Store the Bow

                bool isInRange = RC.Weapon_is_RightHand ? RC.HorizontalAngle < 0.5f : RC.HorizontalAngle > -0.5f;       //Calculate the Imposible range to shoot

                if (!isInRange)
                {
                    isHolding = false;
                    HoldTime = 0;
                    return;
                }

                if (!isHolding)            //If Attack is pressed Start Bending for more Strength the Bow
                {
                    RC.WeaponAction = WA.Hold;
                    isHolding = true;
                    HoldTime = 0;
                }
                else             // //If Attack is pressed Continue Bending the Bow for more Strength the Bow
                {
                    HoldTime += Time.deltaTime;

                    var NormalizedTensionBow = TensionCurve.Evaluate(HoldTime / Bow.HoldTime);
                    var NormalizedTensionArm = Mathf.Clamp(NormalizedTensionBow, 0, MaxArmTension);

                    if (HoldTime <= Bow.HoldTime + Time.deltaTime)
                        Bow.BendBow(NormalizedTensionBow);    //Bend the Bow

                    Anim.SetFloat(RC.Hash_WHold, NormalizedTensionArm);
                }
            }
        }

        /// <summary> If Attack is Released Go to next Action and release the Proyectile</summary>
        private void ReleaseArrow()
        {
            if (RC.Aim && RC.WeaponAction != WA.Fire_Proyectile && isHolding)       //If we are not firing any arrow then try to Attack with the bow
            {
                Bow = RC.ActiveWeapon as IBow;               //Store the Bow

                var Knot = Bow.KNot;
                Knot.rotation = Quaternion.LookRotation(RC.AimDirection);                           //Aligns the Knot and Arrow to the AIM DIRECTION before Releasing the Arrow

                RC.WeaponAction = WA.Fire_Proyectile;              //Go to Action FireProyectile
                isHolding = false;
                HoldTime = 0;

                Bow.ReleaseArrow(RC.AimDirection);
                Bow.BendBow(0);
                Anim.SetFloat(RC.Hash_WHold, 0);            //Reset Hold Animator Values

                RC.OnAttack.Invoke(RC.ActiveWeapon);                 //Invoke the On Attack Event
            }
        }

        /// <summary>This is Called by the Animator </summary>
        public virtual void EquipArrow()
        {
            Bow = RC.ActiveWeapon as IBow;               //Store the Bow
            Bow.EquipArrow();
        }

        /// <summary>Keeps the Camera from changing side while aiming</summary>
        public override bool ChangeAimCameraSide() { return false; }


        public override void ResetAbility()
        {
            base.ResetAbility();
            KnotToHand = false;
            Bow = null;
            isHolding = false;
        }

        /// <summary>This will rotate the bones of the character to match the AIM direction </summary>
        protected virtual void FixAimPoseBow()
        {
            if (RC.Aim)
            {
                RC.Anim.Update(0);

                var UpVector = Vector3.up;

                float Weight =
                    RC.Weapon_is_RightHand ? AimWeight.Evaluate(1 + RC.HorizontalAngle) : AimWeight.Evaluate(1 - RC.HorizontalAngle); //The Weight evaluated on the AnimCurve

                Vector3 AimDirection = RC.AimDirection;

                Quaternion AimLookAt = Quaternion.LookRotation(AimDirection, UpVector);
                //Quaternion LookRotation = Quaternion.LookRotation(LookDirection, RC.Aimer.ForcedTarget ? UpVector : RC.Aimer.MainCamera.transform.up);

                Vector3 ShoulderRotationAxis = RC.Aimer.AimTarget ? Vector3.Cross(UpVector, AimDirection).normalized : RC.Aimer.MainCamera.transform.right;

                Debug.DrawRay(RC.Chest.position, ShoulderRotationAxis, Color.yellow);

                RC.Chest.RotateAround(RC.Chest.position, ShoulderRotationAxis, (Vector3.Angle(UpVector, AimDirection) - 90) * Weight); //Nicely Done!! 

                if (RC.Weapon_is_RightHand)
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



        /// <summary>Put the Bow Knot to the fingers Hand This is called for the Animator </summary>
        public virtual void BowKnotToHand(bool enabled)
        {
            Bow = RC.ActiveWeapon as IBow;               //Store the Bow

            KnotToHand = enabled;

            if (!KnotToHand && Bow != null)
            {
                Bow.RestoreKnot();
            }
        }

        /// <summary>Updates the BowKnot position in the center of the hand if is active</summary>
        protected void BowKnotInHand()
        {
            if (KnotToHand)
            {
                Bow = RC.ActiveWeapon as IBow;               //Store the Bow
                Bow.KNot.position = RC.Weapon_is_RightHand ?
                    RC.LeftHand.TransformPoint(LeftArrowTail) :
                    RC.RightHand.TransformPoint(RightArrowTail);
            }
        }

        public override Transform AimRayOrigin()
        {
            Bow = RC.ActiveWeapon as IBow;            
            return Bow.KNot;
        }
    }
}