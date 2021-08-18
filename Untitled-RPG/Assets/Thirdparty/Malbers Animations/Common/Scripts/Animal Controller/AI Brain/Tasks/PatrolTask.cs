using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller.AI
{

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Patrol")]
    public class PatrolTask : MTask
    {
        public override string DisplayName => "Movement/Patrol";


        public PatrolType patrolType = PatrolType.LastWaypoint;

        [Tooltip("Use a Runtime GameObjects Set to find the Next waypoint")]
        public RuntimeGameObjects RuntimeSet;
        public GetRuntimeGameObjects.RuntimeSetTypeGameObject rtype = GetRuntimeGameObjects.RuntimeSetTypeGameObject.Random;
        public IntReference RTIndex = new IntReference();
        public StringReference RTName = new StringReference();


        public override void StartTask(MAnimalBrain brain, int index)
        {
            switch (patrolType)
            {
                case PatrolType.LastWaypoint:
                    if (brain.LastWayPoint != null)                                    //If we had a last Waypoint then move to it
                    {
                        brain.TargetAnimal = null;                                     //Clean the Animal Target in case it was one
                        brain.AIMovement.SetTarget(brain.LastWayPoint.WPTransform);    //Move to the last waypoint the animal  used
                    }
                    break;
                case PatrolType.UseRuntimeSet:
                    if (RuntimeSet != null)                                             //If we had a last Waypoint then move to it
                    {
                        brain.TargetAnimal = null;                                                          //Clean the Animal Target in case it was one

                        switch (rtype)
                        {
                            case GetRuntimeGameObjects.RuntimeSetTypeGameObject.First:
                                brain.AIMovement.SetTarget(RuntimeSet.Item_GetFirst());
                                break;
                            case GetRuntimeGameObjects.RuntimeSetTypeGameObject.Random:
                                brain.AIMovement.SetTarget(RuntimeSet.Item_GetRandom());
                                break;
                            case GetRuntimeGameObjects.RuntimeSetTypeGameObject.Index:
                                brain.AIMovement.SetTarget(RuntimeSet.Item_Get(RTIndex));
                                break;
                            case GetRuntimeGameObjects.RuntimeSetTypeGameObject.ByName:
                                brain.AIMovement.SetTarget(RuntimeSet.Item_Get(RTName));
                                break;
                            case GetRuntimeGameObjects.RuntimeSetTypeGameObject.Closest:
                                brain.AIMovement.SetTarget(RuntimeSet.Item_GetClosest(brain.Animal.gameObject));
                                break;
                            default:
                                break;
                        }
                        break;
                    }

                    break;
                default:
                    break;
            }

            brain.TaskDone(index);
        }

        public override void ExitAIState(MAnimalBrain brain, int index)
        {
            brain.AIMovement.StopWaitCoroutine();
        }

        public override void OnTargetArrived(MAnimalBrain brain, Transform Target, int index)
        {
            if (Target)
            {
                brain.SetLastWayPoint(Target);                  //Set as Last waypoint the target you have arrived
                brain.AIMovement.SetNextTarget();               //Listen to the OnTarget Arrive to then Make the Next Target
            }
        }

        void Reset() { Description = "Simple Patrol Logic using the Default AiAnimal Control Movement System"; }
    }

    public enum PatrolType { LastWaypoint, UseRuntimeSet }



#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PatrolTask))]
    public class PatrolTaskEditor : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty Description, MessageID, patrolType, RuntimeSet, rtype, RTIndex, RTName, WaitForPreviousTask;

        private void OnEnable()
        {
            WaitForPreviousTask = serializedObject.FindProperty("WaitForPreviousTask");
            Description = serializedObject.FindProperty("Description");
            MessageID = serializedObject.FindProperty("MessageID");
            patrolType = serializedObject.FindProperty("patrolType");
            rtype = serializedObject.FindProperty("rtype");
            RTIndex = serializedObject.FindProperty("RTIndex");
            RTName = serializedObject.FindProperty("RTName");
            RuntimeSet = serializedObject.FindProperty("RuntimeSet");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(Description);
            UnityEditor.EditorGUILayout.PropertyField(MessageID);
            UnityEditor.EditorGUILayout.PropertyField(WaitForPreviousTask);
            UnityEditor.EditorGUILayout.Space();

            UnityEditor.EditorGUILayout.PropertyField(patrolType);

            var tt = (PatrolType)patrolType.intValue;

            switch (tt)
            {
                case PatrolType.LastWaypoint:

                    break;
                case PatrolType.UseRuntimeSet:

                    UnityEditor.EditorGUILayout.PropertyField(RuntimeSet);
                    UnityEditor.EditorGUILayout.PropertyField(rtype, new GUIContent("Get"));
                    var Sel = (GetRuntimeGameObjects.RuntimeSetTypeGameObject)rtype.intValue;
                    switch (Sel)
                    {
                        case GetRuntimeGameObjects.RuntimeSetTypeGameObject.Index:
                            UnityEditor.EditorGUILayout.PropertyField(RTIndex, new GUIContent("Element Index"));
                            break;
                        case GetRuntimeGameObjects.RuntimeSetTypeGameObject.ByName:
                            UnityEditor.EditorGUILayout.PropertyField(RTName, new GUIContent("Element Name"));
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}