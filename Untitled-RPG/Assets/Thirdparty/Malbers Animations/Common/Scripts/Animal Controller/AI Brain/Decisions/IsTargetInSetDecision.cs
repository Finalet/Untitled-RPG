using MalbersAnimations.Scriptables;


namespace MalbersAnimations.Controller.AI
{
    public class IsTargetInSetDecision : MAIDecision
    {
        public override string DisplayName => "General/Is Target in RuntimeSet";

        public RuntimeGameObjects Set;

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            if (brain.AIMovement.Target != null)
            {
                return Set.Items.Exists(x => x.transform == brain.AIMovement.Target);
            }

            return false;
        }
    }
}