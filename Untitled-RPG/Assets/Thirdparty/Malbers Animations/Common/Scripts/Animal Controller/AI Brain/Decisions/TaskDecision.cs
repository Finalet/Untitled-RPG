using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    /// <summary>This Decision Check if a Task is Done </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Task Done Decision", order = 200)]
    public class TaskDecision : MAIDecision
    {
        public override string DisplayName => "General/Task Finished?";



        [Space, Tooltip("What Task Index you want to check if is Done")]
        public int TaskIndex;

        public override bool Decide(MAnimalBrain brain, int index)
        {
            return brain.IsTaskDone(TaskIndex);
        }
    }
}