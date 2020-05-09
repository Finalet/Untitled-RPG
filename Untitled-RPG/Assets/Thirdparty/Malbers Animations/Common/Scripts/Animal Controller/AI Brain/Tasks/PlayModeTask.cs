using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Play Mode")]
    public class PlayModeTask : MTask
    {
        public enum PlayWhen { Once, Forever }

        [Space, Tooltip("Mode you want to activate when the brain is using this task")]
        public ModeID modeID;
        [Tooltip("Ability ID for the Mode... if is set to -1 it will play a random Ability")]
        public IntReference AbilityID = new IntReference(-1);
        [Tooltip("Play the mode only when the animal has arrived to the target")]
        public bool near = false;

        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;

        [Tooltip("Play Once: it will play only at the start of the Task. Play Forever: will play forever using the Cooldown property")]
        public PlayWhen Play = PlayWhen.Forever;
        [Tooltip("Time elapsed to Play the Mode again and Again")]
        public FloatReference CoolDown = new FloatReference(2f);

        [Space, Tooltip("Align with a Look At towards the Target when Playing a mode")]
        public bool lookAtAlign = false;
        [ConditionalHide("lookAtAlign", true), Tooltip("Align time to rotate towards the Target")]
        public float alignTime = 0.3f;


        public override void StartTask(MAnimalBrain brain, int index)
        {
            if (Play == PlayWhen.Once)
            {
                if (near && !brain.AIMovement.HasArrived) return; //Dont play if Play on target is true but we are not near the target.
                PlayMode(brain);
            }
            //   brain.TasksTimeElapsed[index] = 0;       //Set this so the first attack does need to wait for the cooldown
        }


        public override void UpdateTask(MAnimalBrain brain, int index)
        {
            if (Play == PlayWhen.Forever) //If the animal is in range of the Target
            {
                if (near && !brain.ArrivedToTarget) return; //Dont play if Play on target is true but we are not near the target.

                if (brain.CheckIfTasksCountDownElapsed(CoolDown, index))
                {
                    PlayMode(brain);
                    brain.ResetTaskTimeElapsed(index);
                }
            }
        }

        private void PlayMode(MAnimalBrain brain)
        {
            switch (affect)
            {
                case Affected.Self:

                    if (brain.Animal.Mode_TryActivate(modeID, AbilityID))
                    {
                        if (lookAtAlign && brain.Target)
                            brain.StartCoroutine(MalbersTools.AlignLookAtTransform(brain.transform, brain.Target, alignTime));
                    }
                    break;
                case Affected.Target:
                    if (brain.TargetAnimal && brain.TargetAnimal.Mode_TryActivate(modeID, AbilityID))
                    {
                        if (lookAtAlign && brain.Target)
                            brain.StartCoroutine(MalbersTools.AlignLookAtTransform(brain.TargetAnimal.transform, brain.transform, alignTime));
                    }
                    break;
                default:
                    break;
            }
        }

        public void PlayMode(MAnimal animal)
        {
            animal.Mode_TryActivate(modeID, AbilityID);
        }

        void Reset() { Description = "Plays a mode on the Animal(Self or the Target)"; }
    }
}