using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Quick Align")]
    public class QuickAlignTask : MTask
    {
        public override string DisplayName => "General/Quick Align";


        [RequiredField] public TransformVar AlignTarget;
        [Tooltip("Align time to rotate towards the Target")]
        public float alignTime = 0.3f;


        public override void StartTask(MAnimalBrain brain, int index)
        {
            if (AlignTarget != null || AlignTarget.Value == null)
                brain.StartCoroutine(MTools.AlignLookAtTransform(brain.Animal.transform, AlignTarget.Value, alignTime));
            else
                Debug.LogWarning($"The Align Target is empty or Null");

            brain.TaskDone(index);
        }

        void Reset() { Description = "Makes the Animal to look at a Tranform Target"; }
    }
}