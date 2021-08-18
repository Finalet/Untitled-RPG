using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Wait", order = 201)]
    public class WaitDecision : MAIDecision
    {
        public override string DisplayName => "General/Wait";

        [Space]
        /// <summary>Range for Looking forward and Finding something</summary>
        public FloatReference WaitMinTime = new FloatReference(5);
        public FloatReference WaitMaxTime = new FloatReference(5);

        public override void PrepareDecision(MAnimalBrain brain, int Index)
        {
            //Store the time we want to wait on the Local Decision Float var
            brain.DecisionsVars[Index].floatValue = UnityEngine.Random.Range(WaitMinTime, WaitMaxTime);
        }

        public override bool Decide(MAnimalBrain brain,int Index)
        {
            var WaitTime = brain.DecisionsVars[Index].floatValue;

            bool timepassed = MTools.ElapsedTime(brain.StateLastTime, WaitTime);
 
            return timepassed;
        }
    }
}
