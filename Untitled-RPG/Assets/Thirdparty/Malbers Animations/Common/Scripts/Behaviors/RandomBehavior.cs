using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Is Used to execute random animations in a State Machine</summary>
    public class RandomBehavior : StateMachineBehaviour
    {
        public int Range;

        override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            IRandomizer holder = animator.GetComponent<IRandomizer>();
            holder?.SetRandom(Random.Range(1, Range + 1));
        }
    }

    public interface IRandomizer
    {
        void SetRandom(int value);
    }
}