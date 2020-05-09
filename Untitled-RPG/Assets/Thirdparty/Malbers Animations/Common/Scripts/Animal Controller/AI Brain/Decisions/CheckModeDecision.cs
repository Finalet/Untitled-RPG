using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Mode", order = 1)]
    public class CheckModeDecision : MAIDecision
    {
        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target)")]
        public Affected checkOn = Affected.Self;
        [Tooltip("Check if the Mode is Entering or Exiting")]
        public EEnterExit ModeState = EEnterExit.Enter;

        public ModeID ModeID;
        [Tooltip("Which Ability of the Mode should I search for. If is set to -1 then I'll search for all of them")]
        public IntReference Ability = new IntReference(-1);

        public override bool Decide(MAnimalBrain brain,int Index)
        {
            switch (checkOn)
            {
                case Affected.Self:
                    return AnimalMode(brain.Animal);
                case Affected.Target:
                    return AnimalMode(brain.TargetAnimal);
                default:
                    return false;
            }
        }

        private bool AnimalMode(MAnimal animal)
        {
            if (animal == null) return false;

            switch (ModeState)
            {
                case EEnterExit.Enter:
                    return OnEnterMode(animal);
                case EEnterExit.Exit:
                    return OnExitMode(animal);
                default:
                    return false;
            }
        }

        private bool OnEnterMode(MAnimal animal)
        {
            if (animal.ActiveModeID == ModeID)
            {
                if (Ability == -1)
                    return true; //Means that Is playing a random mode does not mater which one
                else
                    return Ability == (animal.ModeAbility % 1000); //Return if the Ability is playing 
            }
            return false;
        }

        private bool OnExitMode(MAnimal animal)
        {
            if (animal.LastMode != 0 && animal.LastModeStatus == MStatus.Completed || animal.LastModeStatus == MStatus.Interrupted)
            {
                //Forget Last Mode (IMPORTANT)
                animal.LastMode = 0;
                animal.LastAbility = 0;
                animal.LastModeStatus = MStatus.None;
                return true;
            }
            return false;
        }
    }
}
