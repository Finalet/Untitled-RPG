using UnityEngine;
namespace MalbersAnimations.Controller.AI
{

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Patrol")]
    public class PatrolTask : MTask
    {
        public override void StartTask(MAnimalBrain brain, int index)
        {
            if (brain.LastWayPoint != null)                                    //If we had a last Waypoint then move to it
            {
                brain.TargetAnimal = null;                                     //Clean the Animal Target in case it was one
                brain.AIMovement.SetTarget(brain.LastWayPoint.WPTransform);    //Move to the last waypoint the animal  used
            }
        }

        public override void OnTargetArrived(MAnimalBrain brain, Transform Target, int index)
        {
            brain.SetLastWayPoint(Target);                  //Set as Last waypoint the target you have arrived
           // brain.AIMovement.MoveAgent = true;
            brain.AIMovement.SetNextTarget();               //Listen to the OnTarget Arrive to then Make the Next Target
        }
        
        void Reset()        { Description = "Simple Patrol Logic using the Default AiAnimal Control Movement System"; }
    }
}