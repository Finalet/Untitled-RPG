using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Mode", order = 2)]
    public class CheckModeDecision : MAIDecision
    {
        public override string DisplayName => "Animal/Check Mode";

        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target)")]
        public Affected checkOn = Affected.Self;
        [Tooltip("Check if the Mode is Entering or Exiting")]
        public EEnterExit ModeState = EEnterExit.Enter;

        public ModeID ModeID;
        [Tooltip("Which ability is playing in the Mode. If is set to less or equal to zero; then it will return true if the Mode Playing")]
        public IntReference Ability = new IntReference(0);

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
                if (Ability <= 0)
                    return true; //Means that Is playing a random mode does not mater which one
                else
                    return Ability == (animal.ModeAbility % 1000); //Return if the Ability is playing 
            }
            return false;
        }

        //Why????????????????
        private bool OnExitMode(MAnimal animal)
        {
            if (animal.LastModeID != 0)
            {
                animal.LastModeID = 0;
                animal.LastAbilityIndex = 0;
                return true;
            }
            return false;
        }
    }
}
