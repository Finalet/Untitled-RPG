using UnityEngine;
using MalbersAnimations.Controller;

namespace MalbersAnimations
{
    /// <summary>Is Used to execute random animations in a State Machine</summary>
    public class RandomBehavior : StateMachineBehaviour
    {
        public int Range;

        override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            MAnimal animal = animator.GetComponent<MAnimal>();
            animal?.SetRandomID(UnityEngine.Random.Range(1, Range + 1));
        }
    }
}