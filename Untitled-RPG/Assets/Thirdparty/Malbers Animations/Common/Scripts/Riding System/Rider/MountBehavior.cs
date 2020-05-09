using UnityEngine;
using System;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.HAP
{
    public class MountBehavior : StateMachineBehaviour
    {
        public enum DismountType { Movement, CameraDirection, Camera}

        public AnimationCurve MovetoMountPoint;

        protected MRider rider;
        protected Transform MountTrigger;
        protected Transform transform;
        protected Transform hip;

        /// <summary>Smooth time to put the Rider in the right position for mount</summary>
        protected float alignTime;

        /// <summary>The animal current scale factor</summary>
        float AnimalScaleFactor = 1;

        /// <summary>Transform Animation to Add or Remove movement while mounting</summary>
        TransformAnimation Fix;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetInteger(Hash.MountSide, 0);                 //Remove the side of the mounted ****IMPORTANT
            rider = animator.GetComponent<MRider>();                //Get the Rider

            alignTime = rider.AlingMountTrigger;                 //Get the Mount Trigger Time
            transform = animator.transform;                         //Get the transform
            MountTrigger = rider.MountTrigger.transform;            //Get the MountTrigger for the Position     

            AnimalScaleFactor = rider.Montura.Animal.ScaleFactor;   //Get the Scale Factor on the Mount

            MalbersTools.ResetFloatParameters(animator);                         //Set All Float values to their defaut (For all the Float Values on the Controller  while is not riding)

            Fix = rider.MountTrigger.Adjustment;                    //Get the Fix for the Mount Trigger

            rider.Start_Mounting();                                 //Call Start Mounting
        }

        override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            transform.position += (animator.velocity * Time.deltaTime * AnimalScaleFactor * (Fix ? Fix.time : 1)); //Scale the animations Root Position   
            //transform.position = animator.rootPosition;
            transform.rotation = animator.rootRotation;

            var Mount_Position = rider.Montura.MountPoint.position;
            var Mount_Rotation = rider.Montura.MountPoint.rotation;


            //Smootly move to the Mount Start Position && rotation
            if (stateInfo.normalizedTime < alignTime)
            {
                var lerp = stateInfo.normalizedTime / alignTime;

                Vector3 NewPos = new Vector3(MountTrigger.position.x, transform.position.y, MountTrigger.position.z);
                transform.position = Vector3.Lerp(transform.position, NewPos, lerp);

                //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(MountTrigger.forward), lerp);
                transform.rotation = Quaternion.Lerp(transform.rotation,  MountTrigger.rotation, lerp);
            }

            //Smoothy adjust the rider to the Mount Position/Rotation

            if (Fix)
            {
                if (Fix.UsePosition)
                {
                    if (!Fix.SeparateAxisPos)
                    {
                        transform.position = Vector3.LerpUnclamped(transform.position, Mount_Position, Fix.PosCurve.Evaluate(stateInfo.normalizedTime));
                    }
                    else
                    {
                        float x = Mathf.LerpUnclamped(transform.position.x, Mount_Position.x, Fix.PosXCurve.Evaluate(stateInfo.normalizedTime) * Fix.Position.x);
                        float y = Mathf.LerpUnclamped(transform.position.y, Mount_Position.y, Fix.PosYCurve.Evaluate(stateInfo.normalizedTime) * Fix.Position.y);
                        float z = Mathf.LerpUnclamped(transform.position.z, Mount_Position.z, Fix.PosZCurve.Evaluate(stateInfo.normalizedTime) * Fix.Position.z);

                        Vector3 newPos = new Vector3(x, y, z);

                        transform.position = newPos;
                    }
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, Mount_Position, MovetoMountPoint.Evaluate(stateInfo.normalizedTime));
                }


                if (Fix.UseRotation) transform.rotation = Quaternion.Lerp(transform.rotation, Mount_Rotation, Fix.RotCurve.Evaluate(stateInfo.normalizedTime));
                else
                    transform.rotation = Quaternion.Lerp(transform.rotation, Mount_Rotation, MovetoMountPoint.Evaluate(stateInfo.normalizedTime));
            }
            else  //if there's no Fix position then use the default
            {
                transform.position = Vector3.Lerp(transform.position, Mount_Position, MovetoMountPoint.Evaluate(stateInfo.normalizedTime));
                transform.rotation = Quaternion.Lerp(transform.rotation, Mount_Rotation, MovetoMountPoint.Evaluate(stateInfo.normalizedTime));
            }
        }
       
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            rider.End_Mounting();                           //Call EndMounting
        }

       
    }
}

