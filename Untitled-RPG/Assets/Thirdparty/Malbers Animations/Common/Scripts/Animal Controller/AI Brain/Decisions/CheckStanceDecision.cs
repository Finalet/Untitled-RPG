using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Stance", order = 2)]
    public class CheckStanceDecision : MAIDecision
    {
        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target)")]
        public Affected check = Affected.Self;
        [Tooltip("Check if the State is Entering or Exiting")]
        public EEnterExit when = EEnterExit.Enter;
        public StanceID stanceID;

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
                    return animal.Stance == stanceID.ID;
                case EEnterExit.Exit:
                    return animal.LastStance == stanceID.ID;
                default:
                    return false;
            }
        }
    }
}