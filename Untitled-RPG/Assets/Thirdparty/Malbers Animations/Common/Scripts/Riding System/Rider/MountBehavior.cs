using UnityEngine;
using System;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.HAP
{
    public class MountBehavior : StateMachineBehaviour
    {
        public AnimationCurve MovetoMountPoint;

        protected MRider rider;
        protected Transform MountTrigger;
        protected bool AlingWithY;

        /// <summary>Smooth time to put the Rider in the right position for mount</summary>
        private float alignTime;
        /// <summary>Needed for Game Creator Characters</summary>
        public float AnimationMult = 1f;

        /// <summary>The animal current scale factor</summary>
        float AnimalScaleFactor = 1;

        /// <summary>Transform Animation to Add or Remove movement while mounting</summary>
        TransformAnimation Fix;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            rider = animator.FindComponent<MRider>();   //Get the Rider

            rider.SetMountSide(0);                                  //Remove the side of the mounted ****IMPORTANT

            alignTime = rider.AlingMountTrigger;                    //Get the Mount Trigger Time
            MountTrigger = rider.MountTrigger.transform;            //Get the MountTrigger for the Position   

            rider.MountRotation = rider.transform.rotation;
            rider.MountPosition = rider.transform.position; 

            AnimalScaleFactor = rider.Montura.Animal.ScaleFactor;   //Get the Scale Factor on the Mount

            MTools.ResetFloatParameters(animator);                         //Set All Float values to their defaut (For all the Float Values on the Controller  while is not riding)

            Fix = rider.MountTrigger.Adjustment;                    //Get the Fix for the Mount Trigger

            rider.Start_Mounting();                                 //Call Start Mounting
        }

        override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float DeltaTime = animator.updateMode == AnimatorUpdateMode.AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;
            var TargetRot = animator.rootRotation;
            var TargetPos = rider.transform.position += (animator.velocity * DeltaTime * AnimalScaleFactor * (Fix ? Fix.time : 1) * AnimationMult); 

            float norm_time = stateInfo.normalizedTime; //State Normalized time

            var Mount_Position = rider.Montura.MountPoint.position;
            var Mount_Rotation = rider.Montura.MountPoint.rotation;


            //Smootly move to the Mount Start Position && rotation
            if (norm_time < alignTime)
            {
                var lerp = norm_time / alignTime;

               // Vector3 NewPos = new Vector3(MountTrigger.position.x, TargetPos.y, MountTrigger.position.z);
                //TargetPos = Vector3.Lerp(TargetPos, NewPos, lerp);

                TargetPos = Vector3.Lerp(TargetPos, MountTrigger.position, lerp);
                TargetRot = Quaternion.Lerp(TargetRot,  MountTrigger.rotation, lerp);
            }

           
            if (Fix) //Smoothy adjust the rider to the Mount Position/Rotation
            {
                if (Fix.UsePosition)
                {
                    if (!Fix.SeparateAxisPos)
                    {
                        TargetPos = Vector3.LerpUnclamped(TargetPos, Mount_Position, Fix.PosCurve.Evaluate(norm_time));
                    }
                    else
                    {
                        float x = Mathf.LerpUnclamped(TargetPos.x, Mount_Position.x, Fix.PosXCurve.Evaluate(norm_time) * Fix.Position.x);
                        float y = Mathf.LerpUnclamped(TargetPos.y, Mount_Position.y, Fix.PosYCurve.Evaluate(norm_time) * Fix.Position.y);
                        float z = Mathf.LerpUnclamped(TargetPos.z, Mount_Position.z, Fix.PosZCurve.Evaluate(norm_time) * Fix.Position.z);

                        Vector3 newPos = new Vector3(x, y, z);

                        TargetPos = newPos;
                    }
                }
                else
                {
                    TargetPos = Vector3.Lerp(TargetPos, Mount_Position, MovetoMountPoint.Evaluate(norm_time));
                }


                if (Fix.UseRotation) TargetRot = Quaternion.Lerp(TargetRot, Mount_Rotation, Fix.RotCurve.Evaluate(norm_time));
                else
                    TargetRot = Quaternion.Lerp(TargetRot, Mount_Rotation, MovetoMountPoint.Evaluate(norm_time));
            }
            else  //if there's no Fix position then use the default
            {
                TargetPos = Vector3.Lerp(TargetPos, Mount_Position, MovetoMountPoint.Evaluate(norm_time));
                TargetRot = Quaternion.Lerp(TargetRot, Mount_Rotation, MovetoMountPoint.Evaluate(norm_time));
            }

            rider.MountRotation = TargetRot;
            rider.MountPosition = TargetPos;
            rider.Mount_TargetTransform();
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        { rider.End_Mounting(); }
    }
}

