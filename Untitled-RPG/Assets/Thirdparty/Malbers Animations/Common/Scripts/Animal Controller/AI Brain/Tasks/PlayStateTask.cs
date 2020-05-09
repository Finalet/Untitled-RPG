using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Play Animal State")]
    public class PlayStateTask : MTask
    {
        [Space, Tooltip("State to play")]
        public StateID StateID;
        [Tooltip("Play the State only when the animal has arrived to the target")]
        public bool PlayNearTarget = false;

        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self; 
        [Tooltip("What to do with the State")]
        public StateAction action =  StateAction.Activate;

        public ExecuteTask Play =  ExecuteTask.OnStart;
        [Tooltip("Time elapsed to Play the Mode again and Again")]
        public FloatReference CoolDown =  new FloatReference (2f);


        public override void StartTask(MAnimalBrain brain, int index)
        {
            if (Play == ExecuteTask.OnStart)
            {
                StateActivate(brain);
            }
            brain.TasksTimeElapsed[index] = CoolDown;       //Set this so the first attack does need to wait for the cooldown
        }


        public override void UpdateTask(MAnimalBrain brain, int index)
        {
            if (Play ==  ExecuteTask.OnUpdate) //If the animal is in range of the Target
            {
                if (brain.CheckIfTasksCountDownElapsed(CoolDown,index))
                {
                    StateActivate(brain);
                    brain.ResetTaskTimeElapsed(index);
                }
            }
        }

        private void StateActivate(MAnimalBrain brain)
        {
            if (PlayNearTarget && !brain.AIMovement.HasArrived) return; //Dont play if Play on target is true but we are not near the target.

            switch (affect)
            {
                case Affected.Self:
                    PlayState(brain.Animal); 
                    break;
                case Affected.Target:
                    if (brain.TargetAnimal) 
                        PlayState(brain.TargetAnimal);
                    break;
                default:
                    break;
            }
        }

        public void PlayState(MAnimal CurrentAnimal)
        {
            switch (action)
            {
                case StateAction.Activate:
                    CurrentAnimal.State_Activate(StateID);
                    break;
                case StateAction.AllowExit:
                    if (CurrentAnimal.ActiveStateID == StateID) CurrentAnimal.ActiveState.AllowExit();
                    break;
                case StateAction.ForceActivate:
                    CurrentAnimal.State_Force(StateID);
                    break;
                case StateAction.Enable:
                    CurrentAnimal.State_Enable(StateID);
                    break;
                case StateAction.Disable:
                    CurrentAnimal.State_Disable(StateID);
                    break;
                default:
                    break;
            }
        }
            

        public override void ExitTask(MAnimalBrain brain, int index)
        {
            if (Play == ExecuteTask.OnExit) //If the animal is in range of the Target
                StateActivate(brain);
        }

        void Reset()
        {
            Description = "Plays a State on the Animal(Self or the Target)";
        }
    }
}