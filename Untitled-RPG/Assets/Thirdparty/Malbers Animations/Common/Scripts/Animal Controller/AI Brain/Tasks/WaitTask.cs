using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Wait")]
    public class WaitTask : MTask
    {
        public override string DisplayName => "General/Wait";



        [Space]
        /// <summary>Range for Looking forward and Finding something</summary>
        public FloatReference WaitMinTime = new FloatReference(5);
        public FloatReference WaitMaxTime = new FloatReference(5);


        public override void StartTask(MAnimalBrain brain, int index)
        {
            brain.TasksVars[index].floatValue = UnityEngine.Random.Range(WaitMinTime, WaitMaxTime);
        }

        public override void UpdateTask(MAnimalBrain brain, int index)
        {
            if (MTools.ElapsedTime(brain.TasksTime[index], brain.TasksVars[index].floatValue))
            { 
                brain.TaskDone(index);
            }
        }
    }
}
