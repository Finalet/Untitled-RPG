using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller.AI
{

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Movement Task", fileName = "New Move Task")]
    public class MoveStopTask : MTask
    {
        public override string DisplayName => "Movement/Movement-Stop";


        public enum MoveType
        {
            MoveToCurrentTarget,
            MoveToNextTarget,
            StopAnimal,
            Stop,
            RotateInPlace,
            Flee,
            CircleAround,
            KeepDistance,
            MoveToLastKnownDestination
        };
        public enum CircleDirection { Left, Right };


        [Space, Tooltip("Type of the Movement task")]
        public MoveType task = MoveType.MoveToCurrentTarget;
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
        public bool UpdateFleeMovingTarget = true;
        public bool LookAtTarget = false;
        public bool Interact = true;


        /// <summary>Reduce the amount of calls of the Task... higher value: lest acurrancy but more performance</summary>
        public float interval = 0.2f;
        public Color debugColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);

        public override void StartTask(MAnimalBrain brain, int index)
        {
            switch (task)
            {
                case MoveType.MoveToCurrentTarget:
                    if (brain.AIMovement.Target)
                    {
                        if (!brain.AIMovement.CheckAirTarget())
                        {
                            brain.AIMovement.DefaultMoveToTarget();
                            brain.AIMovement.ResumeAgent();
                        }
                        brain.AIMovement.LookAtTargetOnArrival = LookAtTarget;
                    }
                    else
                    {
                        Debug.LogWarning("The Animal does not have a current Target");
                    }
                    break;
                case MoveType.MoveToNextTarget:
                    brain.AIMovement.SetNextTarget();
                    brain.AIMovement.LookAtTargetOnArrival = LookAtTarget;
                    break;
                case MoveType.Stop:
                    brain.AIMovement.Stop();
                    brain.AIMovement.UpdateTargetPosition = false;              //IMPORTANT or the animal will try to Move if the Target moves
                    brain.AIMovement.LookAtTargetOnArrival = LookAtTarget;      //IMPORTANT or the animal will try to Move if the Target moves
                    brain.TaskDone(index);
                    break;
                case MoveType.StopAnimal:
                    brain.Animal.LockMovement = true;
                    brain.TaskDone(index);
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
                case MoveType.MoveToLastKnownDestination:
                    var LastDestination = brain.AIMovement.DestinationPosition; //Store the Last Destination
                    brain.AIMovement.DestinationPosition = Vector3.zero;
                    brain.AIMovement.SetDestination(LastDestination, true); //Go to the last Destination position


                    break;
                default:
                    break;
            }
        }


        public override void UpdateTask(MAnimalBrain brain, int index)
        {
            if (interval > 0)
            {
                if (MTools.ElapsedTime(brain.TasksTime[index], interval))
                {
                    UpdateMovementTask(brain, index);
                    brain.SetElapsedTaskTime(index);
                }
            }
            else
                UpdateMovementTask(brain, index);
        }


        public override void OnTargetArrived(MAnimalBrain brain, Transform target, int index)
        {
            switch (task)
            {
                case MoveType.MoveToCurrentTarget:
                    brain.TaskDone(index);
                    break;
                case MoveType.MoveToNextTarget:
                    brain.TaskDone(index);
                    break;
                default:
                    break;
            }
        }

        private void UpdateMovementTask(MAnimalBrain brain, int index)
        {
            switch (task)
            {
                case MoveType.MoveToCurrentTarget:
                    //MoveToTarget(brain);
                    break;
                case MoveType.Flee:
                    {
                        if (UpdateFleeMovingTarget)
                            Flee(brain, index);
                        else
                            ArriveToFleePoint(brain, index);
                        break;
                    }
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

                // Debug.Log(brain.Target);

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
            brain.AIMovement.StoppingDistance = 100f;               //Set the Stopping Distance to almost nothing that way the animal keeps trying to go towards the target
            brain.AIMovement.LookAtTargetOnArrival = true;          //Set the Animal to look Forward to the Target
            brain.AIMovement.Stop();
        }

        private void KeepDistance(MAnimalBrain brain, int index)
        {
            if (brain.Target)
            {
                brain.AIMovement.StoppingDistance = stoppingDistance;
                brain.AIMovement.LookAtTargetOnArrival = false;

                Vector3 KeepDistPoint = brain.Animal.transform.position;

                var DirFromTarget = KeepDistPoint - brain.Target.position;

                float halThreshold = distanceThreshold * 0.5f;
                float TargetDist = DirFromTarget.magnitude;

                if ((TargetDist) < distance - distanceThreshold) //Flee 
                {
                    float DistanceDiff = distance - TargetDist;
                    KeepDistPoint = CalculateDistance(brain, index, DirFromTarget, DistanceDiff, halThreshold);
                }
                else if (TargetDist > distance + distanceThreshold) //Go to Target
                {
                    float DistanceDiff = TargetDist - distance;
                    KeepDistPoint = CalculateDistance(brain, index, -DirFromTarget, DistanceDiff, -halThreshold);
                }
                else
                {
                    brain.AIMovement.HasArrived = true;
                    brain.AIMovement.LookAtTargetOnArrival = true;
                    brain.AIMovement.StoppingDistance = distance + distanceThreshold; //Force to have a greater Stopping Distance so the animal can rotate around the target
                    brain.AIMovement.RemainingDistance = 0; //Force the remaining distance to be 0
                }

                if (brain.debug)
                    Debug.DrawRay(KeepDistPoint, brain.transform.up, Color.cyan, interval);
            }
        }

        private Vector3 CalculateDistance(MAnimalBrain brain, int index, Vector3 DirFromTarget, float DistanceDiff, float halThreshold)
        {
            Vector3 KeepDistPoint = brain.transform.position + DirFromTarget.normalized * (DistanceDiff + halThreshold);
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
                brain.AIMovement.LookAtTargetOnArrival = false;

                var CurrentPos = brain.Animal.transform.position;
                var TargetDirection = CurrentPos - brain.Target.transform.position;
                float TargetDistance = TargetDirection.magnitude;

                if (TargetDistance < distance)
                {
                    Vector3 fleePoint = CurrentPos + TargetDirection.normalized * distance * 0.5f;   //player is too close from us, pick a point diametrically oppossite at twice that distance and try to move there.

                    brain.AIMovement.StoppingDistance = stoppingDistance;

                    Debug.DrawRay(fleePoint, Vector3.up * 3, Color.blue, 2f);

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
                    brain.AIMovement.StoppingDistance = distance * 10;  //Force a big Stopping distance to ensure the animal can look at the Target
                    brain.AIMovement.LookAtTargetOnArrival = true;
                }
            }
        }

        private void ArriveToFleePoint(MAnimalBrain brain, int index)
        {
            if (brain.AIMovement.HasArrived) brain.TaskDone(index); //IF we arrived to the Point we set on the Start Task then set this task as done
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

#if UNITY_EDITOR
    [CustomEditor(typeof(MoveStopTask)), CanEditMultipleObjects]
    public class MoveTaskEditor : Editor
    {
        SerializedProperty
            Description, distance, debugColor, distanceThreshold, Interact, WaitForPreviousTask,
            stoppingDistance, task, Direction, UpdateFleeMovingTarget,
            MessageID, interval, arcsCount, LookAtTarget;
            MonoScript script;
        private void OnEnable()
        {
            script = MonoScript.FromScriptableObject((ScriptableObject)target);

            Description = serializedObject.FindProperty("Description");
            WaitForPreviousTask = serializedObject.FindProperty("WaitForPreviousTask");
            task = serializedObject.FindProperty("task");
            arcsCount = serializedObject.FindProperty("arcsCount");
            distance = serializedObject.FindProperty("distance");
            Direction = serializedObject.FindProperty("direction");
            distanceThreshold = serializedObject.FindProperty("distanceThreshold");
            UpdateFleeMovingTarget = serializedObject.FindProperty("UpdateFleeMovingTarget");
            MessageID = serializedObject.FindProperty("MessageID");
            interval = serializedObject.FindProperty("interval");
            debugColor = serializedObject.FindProperty("debugColor");
            stoppingDistance = serializedObject.FindProperty("stoppingDistance");
            LookAtTarget = serializedObject.FindProperty("LookAtTarget");
            Interact = serializedObject.FindProperty("Interact");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //MalbersEditor.DrawDescription("Movement Task for the AI Brain");

            EditorGUI.BeginChangeCheck();
            MalbersEditor.DrawScript(script);

            EditorGUILayout.PropertyField(Description);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(MessageID);
            EditorGUILayout.PropertyField(debugColor, GUIContent.none, GUILayout.MaxWidth(40));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(WaitForPreviousTask);
            EditorGUILayout.PropertyField(interval);

            MoveStopTask.MoveType taskk = (MoveStopTask.MoveType)task.intValue;

            string Help = GetTaskType(taskk);
            EditorGUILayout.PropertyField(task, new GUIContent("Task", Help));

            switch (taskk)
            {
                case MoveStopTask.MoveType.MoveToCurrentTarget:
                    EditorGUILayout.PropertyField(LookAtTarget, new GUIContent("Look at Target", "If we Arrived to the Target then Keep Looking At it"));
                    EditorGUILayout.PropertyField(Interact, new GUIContent("Interact", "If we Arrived to the Target and is Interactable, Interact!"));
                    break;
                case MoveStopTask.MoveType.MoveToNextTarget:
                    EditorGUILayout.PropertyField(LookAtTarget, new GUIContent("Look at Target", "If we Arrived to the Target then Keep Looking At it"));
                    EditorGUILayout.PropertyField(Interact, new GUIContent("Interact", "If we Arrived to the Target and is Interactable, Interact!"));
                    break;
                case MoveStopTask.MoveType.StopAnimal:
                    EditorGUILayout.PropertyField(LookAtTarget, new GUIContent("Look at Target", "If we Arrived to the Target then Keep Looking At it"));
                    break;
                case MoveStopTask.MoveType.Stop:
                    EditorGUILayout.PropertyField(LookAtTarget, new GUIContent("Look at Target", "If we Arrived to the Target then Keep Looking At it"));
                    break;
                case MoveStopTask.MoveType.RotateInPlace:
                    break;
                case MoveStopTask.MoveType.Flee:
                    EditorGUILayout.PropertyField(distance, new GUIContent("Distance", "Flee Safe Distance away from the Target"));
                    EditorGUILayout.PropertyField(stoppingDistance, new GUIContent("Stop Distance", "Stopping Distance of the Flee point"));
                    EditorGUILayout.PropertyField(UpdateFleeMovingTarget, new GUIContent("Moving Target", "If The target is moving: Update the Fleeing Point"));
                    break;
                case MoveStopTask.MoveType.CircleAround:
                    EditorGUILayout.PropertyField(distance, new GUIContent("Distance", "Flee Safe Distance away from the Target"));
                    EditorGUILayout.PropertyField(stoppingDistance, new GUIContent("Stop Distance", "Stopping Distance of the Circle Around Points"));
                    EditorGUILayout.PropertyField(Direction, new GUIContent("Direction", "Direction to Circle around the Target... left or right"));
                    EditorGUILayout.PropertyField(arcsCount, new GUIContent("Arc Count", "Amount of Point to Form a Circle around the Target"));
                    break;
                case MoveStopTask.MoveType.KeepDistance:
                    EditorGUILayout.PropertyField(distance);
                    EditorGUILayout.PropertyField(stoppingDistance);
                    EditorGUILayout.PropertyField(distanceThreshold);
                    break;
                default:
                    break;
            }




            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(MTools.StyleGreen);
            EditorGUILayout.HelpBox(taskk.ToString() + ":\n" + Help, MessageType.None);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Movement Task Inspector");
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private string GetTaskType(MoveStopTask.MoveType taskk)
        {
            switch (taskk)
            {
                case MoveStopTask.MoveType.MoveToCurrentTarget:
                    return "The Animal will move towards current assigned Target.";
                case MoveStopTask.MoveType.MoveToNextTarget:
                    return "The Animal will move towards the Next Target, if it has a Current Target that is a Waypoint and has a Next Target.";
                case MoveStopTask.MoveType.StopAnimal:
                    return "The Animal will Stop moving. [Animal.LockMovement] will be |True| at Start; and it will be |False| at the end of the Task.";
                case MoveStopTask.MoveType.Stop:
                    return "The Animal will Stop the Agent from moving. Calling AIAnimalControl.Stop(). \nIt will keep the Current Target Assigned.";
                case MoveStopTask.MoveType.RotateInPlace:
                    return "The Animal will not move but it will rotate on the Spot towards the current Target Direction.";
                case MoveStopTask.MoveType.Flee:
                    return "The Animal will move away from the current target until it reaches a the safe distance.";
                case MoveStopTask.MoveType.CircleAround:
                    return "The Animal will Circle around the current Target from a safe distance.";
                case MoveStopTask.MoveType.KeepDistance:
                    return "The Animal will Keep a safe distance from the target\nIf the distance is too close it will flee\nIf the distance is too far it will come near the Target.";
                case MoveStopTask.MoveType.MoveToLastKnownDestination:
                    return "The Animal will Move to the Last Know destination of the Previous Target";
                default:
                    return string.Empty;
            }
        }
    }
#endif
}