using UnityEngine;

namespace MalbersAnimations
{
    public class IsKinematicB : StateMachineBehaviour
    {
        public enum OnEnterOnExit { OnEnter, OnExit, OnEnterOnExit}
        public OnEnterOnExit SetKinematic = OnEnterOnExit.OnEnterOnExit;
        [Tooltip("Changes the Kinematic property of the RigidBody On Enter/OnExit")]
        public bool isKinematic = true;

        Rigidbody rb;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            rb = animator.GetComponent<Rigidbody>();
            if (SetKinematic == OnEnterOnExit.OnEnter)
            {
                rb.isKinematic = isKinematic;
            }
            else if (SetKinematic == OnEnterOnExit.OnEnterOnExit)
            {
                rb.isKinematic = true;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (SetKinematic == OnEnterOnExit.OnExit)
            {
                rb.isKinematic = isKinematic;
            }
            else if (SetKinematic == OnEnterOnExit.OnEnterOnExit)
            {
                rb.isKinematic = false;
            }
        }
    }
}