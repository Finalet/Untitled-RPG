using UnityEngine;
namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Chance Task")]
    public class ChanceTask : MTask
    {
        [Range(0,1), Tooltip("Change the this Task can execute another Task when the AI State start")]
        public float Chance = 1;

        [RequiredField,Tooltip("Task to execute if the chance succeded")]
        public MTask Task;

        public override void StartTask(MAnimalBrain brain, int index )
        {
            var Random = UnityEngine.Random.Range(0f, 1f);

            if (brain.debug)
                Debug.Log($"Change to Try : <B>{Task.name} </B>  Chance:<B> {Chance}/{Random.ToString("F2")}</B> is <B>{Chance >= Random} </B>");

            if (Chance >= Random)
            {
                Task.StartTask(brain,0);
                brain.TaskDone(index); //Set Done to this task
            }
        }

        void Reset()  { Description = "Gives a Percent Chance to execute another task"; }
    }
}
