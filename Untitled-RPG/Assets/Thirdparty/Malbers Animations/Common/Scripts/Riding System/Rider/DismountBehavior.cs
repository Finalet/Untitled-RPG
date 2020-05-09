using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations.HAP
{
    public class DismountBehavior : StateMachineBehaviour
    {
        private MRider rider;
        private Transform transform;
        private Transform MountPoint;
        private TransformAnimation Fix;
        private Vector3 LastRelativeRiderPosition;
        private float ScaleFactor;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetInteger(Hash.MountSide, 0);                 //Remove the side of the mounted **IMPORTANT*** otherwise it will keep trying to dismount

            rider = animator.GetComponent<MRider>();
            ScaleFactor = rider.Montura.Animal.ScaleFactor;         //Get the scale Factor from the Montura

            MountPoint = rider.Montura.MountPoint;

            Fix = rider.MountTrigger.Adjustment;                    //Get the Fix fo the Dismount

            transform = animator.transform;
            rider.Start_Dismounting();

            transform.position = rider.Montura.MountPoint.position;
            transform.rotation = rider.Montura.MountPoint.rotation;

            LastRelativeRiderPosition = MountPoint.InverseTransformPoint(transform.position);

            MalbersTools.ResetFloatParameters(animator);
        }


        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        { rider.End_Dismounting(); }

        override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            #region OldWay
            var transition = animator.GetAnimatorTransitionInfo(layerIndex);
           
            float time = animator.updateMode == AnimatorUpdateMode.AnimatePhysics? Time.fixedDeltaTime : Time.deltaTime;

            transform.rotation = animator.rootRotation;
            transform.position = MountPoint.TransformPoint(LastRelativeRiderPosition);              //Parent Position without Parenting
            
            transform.position += (animator.velocity * time * ScaleFactor * (Fix ? Fix.delay : 1));
           
            if (rider.Montura)  //Stop the Mountura from walking forward when Dismounting
            {
             
                if (rider.MountTrigger)   //Don't go under the floor
                {
                    if (transform.position.y < rider.MountTrigger.transform.position.y)
                        transform.position = new Vector3(transform.position.x, rider.MountTrigger.transform.position.y, transform.position.z);
                }

                rider.transform.rotation *= rider.Montura.Animal.AdditiveRotation;

                if (stateInfo.normalizedTime > 0.5f && animator.IsInTransition(layerIndex))
                {
                    float transitionTime = transition.normalizedTime;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation, transitionTime);
                    transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, rider.MountTrigger.transform.position.y, transform.position.z), time * 5f);
                }
            }

            animator.rootPosition = transform.position;

            LastRelativeRiderPosition = MountPoint.InverseTransformPoint(transform.position);
            #endregion
        }
    }
}