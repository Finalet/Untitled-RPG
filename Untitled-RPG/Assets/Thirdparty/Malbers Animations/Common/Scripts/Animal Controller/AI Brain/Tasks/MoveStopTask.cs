using MalbersAnimations.Scriptables;
using UnityEngine;
namespace MalbersAnimations.Controller.AI
{

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Movement Task", fileName = "New Move Task")]
    public class MoveStopTask : MTask
    {
        public enum MoveType
        { 
            MoveToTarget, 
            StopAnimal, 
            StopAgent, 
            RotateInPlace, 
            Flee, 
            CircleAround,
            KeepDistance 
        };
        public enum CircleDirection { Left, Right };


        [Space,Tooltip("Type of the Movement task")]
        public MoveType task = MoveType.MoveToTarget;
        /// <summary> Distance for the Flee, Circle Around and keep Distance Task</summary>
        public FloatReference distance = new FloatReference(10f);
        /// <summary> Distance Threshold for the Keep Distance Task</summary>
        public FloatReference distanceThreshold = new FloatReference(1f);
        /// <summary> Custom Stopping Distance to Override the AI Movement Stopping Distance</summary>
        public FloatReference stoppingDistance = new FloatReference(0.5f);

        /// <summary> Custom Stopping Distance to Override the AI Movement Stopping Distance</summary>
        public CircleDirection direction = CircleDirection.Left;

        /// <summary> Amount of Target Position around the Target</summary>
        public int arcsCount = 12;

        /// <summary> If the Targets Move, or you want to keep an eye of the Target this Option should be enable </summary>
        public bool UpdateFleeMovingTarget = false;
        public bool LookAtTarget = true;


        /// <summary>Reduce the amount of calls of the Task... higher value: lest acurrancy but more performance</summary>
        public float interval = 0.2f;
        public Color debugColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);

        public override void StartTask(MAnimalBrain brain, int index)
        {
            switch (task)
            {
                case MoveType.MoveToTarget:
                    brain.AIMovement.UpdateTargetPosition = true;           // Make Sure to update the Target position on the Animal
                    brain.AIMovement.MoveAgent = true;                      // If the Target is mooving then Move/Resume the Agent...
                    brain.AIMovement.MoveAgentOnMovingTarget = true;                      // If the Target is mooving then Move/Resume the Agent...
                    brain.AIMovement.LookAtTargetOnArrival = LookAtTarget;                      // If the Target is mooving then Move/Resume the Agent...
                   if (brain.AIMovement.IsAITarget!= null) 
                        brain.AIMovement.StoppingDistance = brain.AIMovement.IsAITarget.StopDistance();  //Restore the Stopping Distance
                    brain.Animal.LockMovement = false;
                    brain.AIMovement.ResumeAgent();
                    break;
                case MoveType.StopAgent:
                    brain.AIMovement.Stop();
                    break;
                case MoveType.StopAnimal:
                    brain.Animal.LockMovement = true;
                    break;
                case MoveType.RotateInPlace:
                    RotateInPlace(brain);
                    break;
                case MoveType.Flee:
                    Flee(brain, index);
                    break;
                case MoveType.KeepDistance:
                    KeepDistance(brain, index);
                    break;
                case MoveType.CircleAround:
                    CalculateClosestCirclePoint(brain, index);
                    break;
                default:
                    break;
            }
        }


        public override void UpdateTask(MAnimalBrain brain, int index)
        {
            if (interval > 0 )
            {
                if (brain.CheckIfTasksCountDownElapsed(interval, index))
                {
                    UpdateMovementTask(brain, index);
                    brain.ResetTaskTimeElapsed(index);
                }
            }
            else
                UpdateMovementTask(brain, index);
        }

        private void UpdateMovementTask(MAnimalBrain brain, int index)
        {
            switch (task)
            {
                case MoveType.MoveToTarget:
                    MoveToTarget(brain);
                    break;
                case MoveType.StopAgent:
                    break;
                //case MoveType.LockMovement:
                //    break;
                //case MoveType.RotateInPlace:
                //    break;
                case MoveType.Flee:
                    if (UpdateFleeMovingTarget)
                        Flee(brain, index);
                    else
                        ArriveToFleePoint(brain, index);
                    break;

                case MoveType.KeepDistance:
                    KeepDistance(brain, index);
                    break;
                case MoveType.CircleAround:
                    CircleAround(brain, index);
                    break;
                default:
                    break;
            }
        }

        private static void MoveToTarget(MAnimalBrain brain)
        {
            brain.AIMovement.UpdateTargetPosition = true;           // Make Sure to update the Target position on the Animal
            brain.AIMovement.MoveAgent = true;                      // If the Target is mooving then Move/Resume the Agent...
            brain.AIMovement.MoveAgentOnMovingTarget = true;        // If the Target is mooving then Move/Resume the Agent...

        }

        private void CalculateClosestCirclePoint(MAnimalBrain brain, int index)
        {
            float arcDegree = 360.0f / arcsCount;
            int Dir = direction == CircleDirection.Right ? 1 : -1;
            Quaternion rotation = Quaternion.Euler(0, Dir * arcDegree, 0);

            Vector3 currentDirection = Vector3.forward;
            Vector3 MinPoint = Vector3.zero;
            float minDist = float.MaxValue;
            brain.AIMovement.StoppingDistance = stoppingDistance;

            int MinIndex = 0;

            for (int i = 0; i < arcsCount; ++i)
            {
              var CurrentPoint = brain.Target.position + (currentDirection.normalized * distance);

                float DistCurrentPoint = Vector3.Distance(CurrentPoint, brain.transform.position);

                if (minDist > DistCurrentPoint)
                {
                    minDist = DistCurrentPoint;
                    MinIndex = i;
                    MinPoint = CurrentPoint;
                }

                currentDirection = rotation * currentDirection;
            }

            brain.AIMovement.UpdateTargetPosition = false;
            brain.AIMovement.StoppingDistance = stoppingDistance;

            brain.TasksVars[index].intValue = MinIndex;        //Store the Point index on the vars of this Task
            brain.TasksVars[index].boolValue = true; //Set this so we can seak for the next point

            brain.AIMovement.UpdateTargetPosition = false; //Means the Animal Wont Update the Destination Position with the Target position.
            brain.AIMovement.SetDestination(MinPoint);
            brain.AIMovement.HasArrived = false;
            brain.AIMovement.MoveAgentOnMovingTarget = false; 
        }

        private void CircleAround(MAnimalBrain brain, int index)
        {
            if (brain.AIMovement.HasArrived) //Means that we have arrived to the point so set the next point
            {
                brain.TasksVars[index].intValue++;
                brain.TasksVars[index].intValue = brain.TasksVars[index].intValue % arcsCount;
                brain.TasksVars[index].boolValue = true; //Set this so we can seek for the next point
            }

            if (brain.TasksVars[index].boolValue || brain.AIMovement.TargetIsMoving)
            {
                int pointIndex = brain.TasksVars[index].intValue;

                float arcDegree = 360.0f / arcsCount;
                int Dir = direction == CircleDirection.Right ? 1 : -1;
                Quaternion rotation = Quaternion.Euler(0, Dir * arcDegree * pointIndex, 0);


                Vector3 currentDirection = Vector3.forward;
                currentDirection = rotation * currentDirection;
                Vector3 CurrentPoint = brain.Target.position + (currentDirection.normalized * distance);

                Debug.DrawRay(CurrentPoint, Vector3.up, Color.green, interval);


                brain.AIMovement.UpdateTargetPosition = false; //Means the Animal Wont Update the Destination Position with the Target position.
                brain.AIMovement.SetDestination(CurrentPoint);

                brain.TasksVars[index].boolValue = false; //Set this so we can seak for the next point
            }
        }

        /// <summary>Locks the Movement forward but it still tries to go forward. Only Rotates</summary>
        private void RotateInPlace(MAnimalBrain brain)
        {
          //  brain.Animal.LockForwardMovement = true;
            brain.AIMovement.StoppingDistance = 100f;               //Set the Stopping Distance to almost nothing that way the animal keeps trying to go towards the target
            brain.AIMovement.LookAtTargetOnArrival = true;           //Set the Animal to look Forward to the Target
            brain.AIMovement.Stop();
        }

        private void KeepDistance(MAnimalBrain brain, int index)
        {
            if (brain.Target)
            {
                brain.AIMovement.StoppingDistance = stoppingDistance;

                Vector3 KeepDistPoint = brain.transform.position;

                var DirFromTarget = KeepDistPoint - brain.Target.position;

                float halThreshold = distanceThreshold * 0.5f;
                float TargetDist = DirFromTarget.magnitude;

                if ((TargetDist /*- halThreshold*/) < distance - distanceThreshold) //Flee 
                {
                    float DistanceDiff = distance - TargetDist;
                    KeepDistPoint = CalculateDistance(brain, index, DirFromTarget, DistanceDiff, halThreshold);
                    brain.TaskDone(index, false);
                }
                else if (TargetDist > distance + distanceThreshold) //Go to Target
                {
                    float DistanceDiff = TargetDist - distance;
                    KeepDistPoint = CalculateDistance(brain, index, -DirFromTarget, DistanceDiff, -halThreshold);
                    brain.TaskDone(index, false);
                }
                else
                {
                    brain.AIMovement.HasArrived = true;
                    brain.AIMovement.StoppingDistance = distance + distanceThreshold; //Force to have a greater Stopping Distance so the animal can rotate around the target
                    brain.TaskDone(index);
                }

                if (brain.debug)
                    Debug.DrawRay(KeepDistPoint, brain.transform.up, Color.cyan, interval);
            }
        }

        private Vector3 CalculateDistance(MAnimalBrain brain, int index, Vector3 DirFromTarget, float DistanceDiff, float halThreshold)
        {
            Vector3 KeepDistPoint = brain.transform.position + DirFromTarget.normalized * (DistanceDiff + halThreshold);

            brain.TaskDone(index, false);
            brain.AIMovement.UpdateTargetPosition = false; //Means the Animal Wont Update the Destination Position with the Target position.
            brain.AIMovement.StoppingDistance = stoppingDistance;
            brain.AIMovement.SetDestination(KeepDistPoint, true);
            return KeepDistPoint;
        }


        private void Flee(MAnimalBrain brain, int index)
        {
            if (brain.Target)
            {
                brain.AIMovement.UpdateTargetPosition = false;                      //Means the Animal Wont Update the Destination Position with the Target position.
                var CurrentPos = brain.transform.position;
                var TargetDirection = CurrentPos - brain.Target.transform.position;
                float TargetDistance = TargetDirection.magnitude;

                if (TargetDistance < distance)
                {
                    float DistanceDiff = distance - TargetDistance;

                    Vector3 fleePoint = CurrentPos + TargetDirection.normalized * distance*0.5f;   //player is too close from us, pick a point diametrically oppossite at twice that distance and try to move there.


                    brain.TaskDone(index, false);

                    brain.AIMovement.StoppingDistance = stoppingDistance;

                    Debug.DrawRay(fleePoint, Vector3.up*15, Color.blue, 2f);

                    if (Vector3.Distance(CurrentPos, fleePoint) > stoppingDistance) //If the New flee Point is not in the Stopping distance radius then set a new Flee Point
                    {
                        brain.AIMovement.UpdateTargetPosition = false; //Means the Animal Wont Update the Destination Position with the Target position.
                        brain.AIMovement.SetDestination(fleePoint, true);

                        if (brain.debug)
                            Debug.DrawRay(fleePoint, brain.transform.up, Color.blue, 2f);
                    }
                }
                else
                {
                   // if (brain.AIMovement.HasArrived)
                    {
                        brain.AIMovement.StoppingDistance = distance * 10;  //Force a big Stopping distance to ensure the animal can look at the Target
                        brain.TaskDone(index); //Means this taks is Done ... it has Reachedn to the Fleeing Position
                    }
                }
            }
        }

        private void ArriveToFleePoint(MAnimalBrain brain, int index)
        {
            if (brain.AIMovement.HasArrived) brain.TaskDone(index); //IF we arrived to the Point we set on the Start Task then set this task as done
        }

        public override void ExitTask(MAnimalBrain brain, int index)
        {
            brain.AIMovement.MoveAgent = true;
            brain.AIMovement.UpdateTargetPosition = true;

            switch (task)
            {
                case MoveType.MoveToTarget:
                    brain.AIMovement.UpdateTargetPosition = false;           // Make Sure to update the Target position on the Animal
                    brain.AIMovement.MoveAgentOnMovingTarget = false;                      // If the Target is mooving then Move/Resume the Agent...
                    break;
                case MoveType.StopAgent:
                    brain.AIMovement.MoveAgent = true;
                    break;
                case MoveType.Flee:
                    brain.AIMovement.UpdateTargetPosition = true;
                    break;
                case MoveType.RotateInPlace:
                    brain.Animal.LockForwardMovement = false;
                    break;
                case MoveType.StopAnimal:
                    brain.Animal.LockMovement = false;
                    break;
                default:
                    break;
            }
        }


#if UNITY_EDITOR
        public override void DrawGizmos(MAnimalBrain brain)
        {
            if (task == MoveType.Flee)
            {
                UnityEditor.Handles.color = debugColor;
                UnityEditor.Handles.DrawSolidDisc(brain.AIMovement.Agent.transform.position, Vector3.up, distance);
            }
            else if (task == MoveType.KeepDistance)
            {
                UnityEditor.Handles.color = debugColor;
                UnityEditor.Handles.color = new Color(debugColor.r, debugColor.g, debugColor.b, 1f);
                UnityEditor.Handles.DrawWireDisc(brain.transform.position, Vector3.up, distance - distanceThreshold);
                UnityEditor.Handles.DrawWireDisc(brain.transform.position, Vector3.up, distance + distanceThreshold);
                //UnityEditor.Handles.DrawWireDisc(brain.AIMovement.Agent.transform.position, Vector3.up, distance);
            }
            else if (task == MoveType.CircleAround && brain.Target)
            {
                UnityEditor.Handles.color = new Color(debugColor.r, debugColor.g, debugColor.b, 1f);
                UnityEditor.Handles.DrawWireDisc(brain.AIMovement.Agent.transform.position, Vector3.up, distance);
                
                float arcDegree = 360.0f / arcsCount;
                Quaternion rotation = Quaternion.Euler(0, -arcDegree, 0);

                Vector3 currentDirection = Vector3.forward;

                for (int i = 0; i < arcsCount; ++i)
                {
                   var CurrentPoint = brain.Target.position + (currentDirection.normalized * distance);
                    Gizmos.DrawWireSphere(CurrentPoint, 0.1f);
                    currentDirection = rotation * currentDirection;
                }
            }
        }
#endif
    }
}