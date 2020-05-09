using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check State", order = 0)]
    public class CheckStateDecision : MAIDecision
    {
        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target)")]
        public Affected check = Affected.Self;
        [Tooltip("Check if the State is Entering or Exiting")]
        public EEnterExit when = EEnterExit.Enter;
        public StateID StateID;

        public override bool Decide(MAnimalBrain brain,int index)
        {
            switch (check)
            {
                case Affected.Self:
                    return CheckState(brain.Animal);
                case Affected.Target:
                    return brain.TargetAnimal != null && CheckState(brain.TargetAnimal);
                default:
                    return false;
            }
        }

        private bool CheckState(MAnimal animal)
        {
            switch (when)
            {
                case EEnterExit.Enter:
                    return animal.ActiveStateID == StateID.ID;
                case EEnterExit.Exit:
                    return animal.LastState.ID == StateID.ID;
                default:
                    return false;
            }
        }
    }
}