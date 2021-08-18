using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Arrived to Target",order = -100)]
    public class ArriveDecision : MAIDecision
    {
        public override string DisplayName => "Movement/Has Arrived?";

        [Space,Tooltip("(OPTIONAL)Use it if you want to know if we have arrived to a specific Target")]
        public string TargetName = string.Empty;
        public float MinTargetHeight = 1f;
        public bool debug = false;

        public override bool Decide(MAnimalBrain brain, int index)
        {
            bool Result = false;

            if (string.IsNullOrEmpty(TargetName))
            {
                Result = brain.AIMovement.HasArrived && brain.AIMovement.TargetHeight <= MinTargetHeight;
            }
            else
            {
                Result = brain.AIMovement.HasArrived && brain.AIMovement.TargetHeight <= MinTargetHeight && brain.Target.root.name == TargetName; //If we are looking for an specific Target
            }

            if (debug)
                Debug.Log($"{brain.Animal.name}: <B>[{name}]</B> = <B>[{Result}]</B>");


            return Result;
        }
    }
}