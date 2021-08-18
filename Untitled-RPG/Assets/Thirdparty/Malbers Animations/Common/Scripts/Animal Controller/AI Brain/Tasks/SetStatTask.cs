using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Set Stat")]
    public class SetStatTask : MTask
    {

        public override string DisplayName => "General/Set Stat";



        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;
        public StatModifier stat;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            if (affect == Affected.Self)
            {
                if (brain.AnimalStats != null)
                {
                    if (brain.AnimalStats.TryGetValue(stat.ID, out Stat statS))
                    {
                        stat.ModifyStat(statS);
                    }
                }
            }
            else
            {
                if (brain.TargetStats != null)
                {
                    if (brain.TargetStats.TryGetValue(stat.ID, out Stat statS))
                    {
                        stat.ModifyStat(statS);
                    }
                }
            }

            brain.TaskDone(index);
        }
    }
}
