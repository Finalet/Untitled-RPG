using UnityEngine;
using UnityEditor;
using System;

namespace MalbersAnimations.Controller.AI
{
    [CustomEditor(typeof(MoveStopTask))]
    [CanEditMultipleObjects]
    public class MoveTaskEditor : Editor
    {
        SerializedProperty
            Description, distance, debugColor, distanceThreshold, stoppingDistance, task, Direction, UpdateFleeMovingTarget, MessageID, interval, arcsCount, LookAtTarget;
        MonoScript script;
        private void OnEnable()
        {
            script = MonoScript.FromScriptableObject((ScriptableObject)target);

            Description = serializedObject.FindProperty("Description");
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
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Movement Task for the AI Brain");

            EditorGUI.BeginChangeCheck();
            MalbersEditor.DrawScript(script);

            EditorGUILayout.PropertyField(Description);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(MessageID);
            EditorGUILayout.PropertyField(debugColor,GUIContent.none,GUILayout.MaxWidth(40));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(interval);

            MoveStopTask.MoveType taskk = (MoveStopTask.MoveType)task.intValue;

            string Help = GetTaskType(taskk);
            EditorGUILayout.PropertyField(task, new GUIContent("Task", Help));

            switch (taskk)
            {
                case MoveStopTask.MoveType.MoveToTarget: 
                    EditorGUILayout.PropertyField(LookAtTarget, new GUIContent("Look at Target", "If we Arrived to the Target then Keep Looking At it"));
                    break;
                case MoveStopTask.MoveType.StopAnimal:
                    break;
                case MoveStopTask.MoveType.StopAgent:
                    break;
                case MoveStopTask.MoveType.RotateInPlace:
                    break;
                case MoveStopTask.MoveType.Flee:
                    EditorGUILayout.PropertyField(distance, new GUIContent("Distance","Flee Safe Distance away from the Target"));
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
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGreen);
            EditorGUILayout.HelpBox(taskk.ToString()+":\n\n"+Help, MessageType.None);
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
                case MoveStopTask.MoveType.MoveToTarget:
                    return "The Animal will move towards the Target";
                case MoveStopTask.MoveType.StopAnimal:
                    return "The Animal will Stop moving. [Animal.LockMovement] will be |True| at Start; and it will be |False| at the end of the Task";
                case MoveStopTask.MoveType.StopAgent:
                    return "The Animal will Stop the Agent from moving. Calling AIAnimalControl.Stop(). It will keep the Target";
                case MoveStopTask.MoveType.RotateInPlace:
                    return "The Animal will not move but it will rotate on the Spot towards the current Target Direction";
                case MoveStopTask.MoveType.Flee:
                    return "The Animal will move away from the current target until it reaches a the safe distance";
                case MoveStopTask.MoveType.CircleAround:
                    return "The Animal will Circle around the Target from a safe distance";
                case MoveStopTask.MoveType.KeepDistance:
                    return "The Animal will Keep a safe distance from the target\nIf the distance is too close it will flee\nIf the distance is too far it will come near the Target";
                default:
                    return string.Empty;
            }
        }
    }
}