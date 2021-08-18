using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Invoke Event")]
    public class InvokeEventTask : MTask
    {
        public override string DisplayName => "General/Invoke Event";

        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;
        public GameObjectEvent Raise = new GameObjectEvent();

        public override void StartTask(MAnimalBrain brain, int index)
        {
            switch (affect)
            {
                case Affected.Self:
                    Raise.Invoke(brain.Animal.gameObject);
                    break;
                case Affected.Target:
                    Raise.Invoke(brain.Target.gameObject);
                    break;
                default:
                    break;
            }
            brain.TaskDone(index);
        }

        void Reset() { Description = "Raise the Event when the Task start. Use this only for Scriptable Assets"; }
    }
}
