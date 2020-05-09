using System.Collections.Generic;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/OR Decision")]
    public class ORDecision : MAIDecision
    {
       public List<MAIDecision> decisions;

        public override void PrepareDecision(MAnimalBrain brain, int Index)
        {
            foreach (var d in decisions) d.PrepareDecision(brain, Index);
        }

        public override bool Decide(MAnimalBrain brain,int Index)
        {
            foreach (var d in decisions)
            {
                if (d.Decide(brain, Index)) 
                    return true;
            }

            return false;
        }
        public override void FinishDecision(MAnimalBrain brain, int Index)
        {
            foreach (var d in decisions) d.FinishDecision(brain, Index);
        }

        public override void DrawGizmos(MAnimalBrain brain)
        {
            foreach (var d in decisions) d.DrawGizmos(brain);
        }

        void Reset() { Description = "At least ONE decisions on the list  must be TRUE in order to sent a True Decision"; }
    }
}
