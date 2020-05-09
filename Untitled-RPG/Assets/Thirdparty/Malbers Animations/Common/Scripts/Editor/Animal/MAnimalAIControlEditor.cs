using UnityEngine;
using UnityEditor;
using MalbersAnimations.Utilities;
using System;

namespace MalbersAnimations.Controller
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MAnimalAIControl), true)]
    public class AnimalAIControlEd : Editor
    {
        MAnimalAIControl M;
        MonoScript script;

        SerializedProperty stoppingDistance, walkDistance, targett, AutoNextTarget, AutoInteract,
            OnTargetPositionArrived, OnTargetArrived, OnTargetSet, debugGizmos, debug, Editor_Tabs1,
            UpdateTargetPosition, MoveAgentOnMovingTarget, MovingTargetInterval, LookAtTargetOnArrival;

        private void OnEnable()
        {
            M = (MAnimalAIControl)target;
            script = MonoScript.FromMonoBehaviour(M);

            OnTargetSet = serializedObject.FindProperty("OnTargetSet");
            OnTargetArrived = serializedObject.FindProperty("OnTargetArrived");
            OnTargetPositionArrived = serializedObject.FindProperty("OnTargetPositionArrived");
            stoppingDistance = serializedObject.FindProperty("stoppingDistance");
            walkDistance = serializedObject.FindProperty("walkDistance");
            targett = serializedObject.FindProperty("target");
            AutoNextTarget = serializedObject.FindProperty("AutoNextTarget");
            AutoInteract = serializedObject.FindProperty("AutoInteract");
            debugGizmos = serializedObject.FindProperty("debugGizmos");
            debug = serializedObject.FindProperty("debug");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            UpdateTargetPosition = serializedObject.FindProperty("updateTargetPosition");
            MoveAgentOnMovingTarget = serializedObject.FindProperty("MoveAgentOnMovingTarget");
            MovingTargetInterval = serializedObject.FindProperty("MovingTargetInterval");
            LookAtTargetOnArrival = serializedObject.FindProperty("LookAtTargetOnArrival");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("AI Movement Logic for the Animal");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);

                MalbersEditor.DrawScript(script);

                Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Events", "Debug" });

                int Selection = Editor_Tabs1.intValue;

                if (Selection == 0) ShowGeneral();
                else if (Selection == 1) ShowEvents();
                else if (Selection == 2) ShowDebug();


                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Animal AI Control Changed");
                }

            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
        private void ShowGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(targett, new GUIContent("Target", "Target to follow"));
                EditorGUILayout.PropertyField(AutoNextTarget, new GUIContent("Auto Next Target", "if True, the Animal will set the Next Target when he arrives to the Current Target. Animal Brain will disable this option "));
                EditorGUILayout.PropertyField(AutoInteract, new GUIContent("Auto Interact", "Means the Animal will interact to any Waypoint(Zone) automatically when he arrived to it"));
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Moving Target Options", EditorStyles.boldLabel);

                // UpdateTargetPosition.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Update Target Destination", "If the Target moves, the Destination Position will be updated with the Target's new Position"), UpdateTargetPosition.boolValue);
                // MoveAgentOnMovingTarget.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Move on Moving Target", "Move Agent On Moving Target:\nIf the Target Moves, the Agent will move the Animal towards the Target when is not inside the Stopping Distance Radius"), MoveAgentOnMovingTarget.boolValue);
                // LookAtTargetOnArrival.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("LookAt Moving Target on Arrival", "If we have arrived to the Target then Try to Keep Lookin Directly to it (Forward Direction Aims to the Target)"), LookAtTargetOnArrival.boolValue);
                EditorGUILayout.PropertyField(UpdateTargetPosition, new GUIContent("Update Destination", "If the Target moves, the Destination Position will be updated with the Target's Position"));
                EditorGUILayout.PropertyField(MoveAgentOnMovingTarget, new GUIContent("Move on moving Target", "Move Agent On Moving Target:\nIf the Target Moves, the Agent will move the Animal towards the Target. Set this to false if your Destination Position is diferent from the Target Position"));
                EditorGUILayout.PropertyField(LookAtTargetOnArrival, new GUIContent("Look Target on arrival", "Look At Moving Target on Arrival\nIf we have arrived to the Target then Try to Keep Lookin Directly to it (Forward Direction Aims to the Target)"));
                EditorGUILayout.PropertyField(MovingTargetInterval, new GUIContent("Check Interval", "Check if the Target moved every x Seconds"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(stoppingDistance, new GUIContent("Stopping Distance", "Agent Stopping Distance"));
                    EditorGUILayout.PropertyField(walkDistance, new GUIContent("Walk Distance", "Distance to stop Runing and Start Walking"));

                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (M.Agent)
                    {
                        M.Agent.stoppingDistance = stoppingDistance.floatValue;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            if (!M.Agent)
            {
                EditorGUILayout.HelpBox("There's no Agent found on the hierarchy on this gameobject\nPlease add a NavMesh Agent Component", MessageType.Error);
            }
        }

        private void ShowEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(OnTargetPositionArrived, new GUIContent("On Position Arrived"));
                EditorGUILayout.PropertyField(OnTargetArrived, new GUIContent("On Target Arrived"));
                EditorGUILayout.PropertyField(OnTargetSet, new GUIContent("On New Target Set"));
            }
            EditorGUILayout.EndVertical();
        }
        private void ShowDebug()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 90f;
                EditorGUILayout.PropertyField(debug, new GUIContent("Debug Log"));
                EditorGUILayout.PropertyField(debugGizmos);
                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndHorizontal();
                if (Application.isPlaying)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(targett);
                    EditorGUILayout.ObjectField("Next Target", M.NextTarget, typeof(Transform), false);
                    EditorGUILayout.Vector3Field("Destination Pos", M.DestinationPosition);
                    EditorGUILayout.Space();
                    EditorGUILayout.FloatField("Remaining Distance", M.RemainingDistance);
                    EditorGUILayout.ToggleLeft("Target is Moving", M.TargetIsMoving);
                    EditorGUILayout.ToggleLeft("Move Agent", M.MoveAgent);
                    EditorGUILayout.Space();
                    EditorGUILayout.ToggleLeft("Is Target a WayPoint", M.IsWayPoint != null);
                    EditorGUILayout.ToggleLeft("Is AI Target", M.IsAITarget != null);
                    EditorGUILayout.ToggleLeft("Is On Mode", M.IsOnMode);
                    //EditorGUILayout.ToggleLeft("Enter OffMesh", M.EnterOFFMESH);
                    EditorGUILayout.ToggleLeft("Free Move", M.FreeMove);
                    EditorGUILayout.ToggleLeft("Flying OffMesh", M.IsMovingOffMesh);
                    EditorGUILayout.ToggleLeft("Agent Active", M.Agent.enabled);
                    EditorGUILayout.ToggleLeft("Agent in NavMesh", M.Agent.isOnNavMesh);
                    EditorGUILayout.ToggleLeft("Is Waiting", M.IsWaiting);
                    EditorGUILayout.ToggleLeft("Has Arrived to Destination", M.HasArrived);
                    EditorGUILayout.ToggleLeft("Is Grounded?", M.IsGrounded);
                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}