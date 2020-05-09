using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(Pickable)), CanEditMultipleObjects]
    public class PickableEditor : Editor
    {
        private MonoScript script;
        private SerializedProperty PickAnimations, PickUpMode, PickUpAbility, DropMode, DropAbility, DropAnimations, Align, AlignTime, AlignDistance,
            OnFocused, OnPicked, OnDropped, ShowEvents, FloatID, IntID, m_collider;

        private Pickable m;

        private void OnEnable()
        {
            m = (Pickable)target;
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            PickAnimations = serializedObject.FindProperty("PickAnimations");
            PickUpMode = serializedObject.FindProperty("PickUpMode");
            PickUpAbility = serializedObject.FindProperty("PickUpAbility");
            DropAnimations = serializedObject.FindProperty("DropAnimations");
            DropMode = serializedObject.FindProperty("DropMode");
            DropAbility = serializedObject.FindProperty("DropAbility");
            Align = serializedObject.FindProperty("Align");
            AlignTime = serializedObject.FindProperty("AlignTime");
            AlignDistance = serializedObject.FindProperty("AlignDistance");
            OnFocused = serializedObject.FindProperty("OnFocused");
            OnPicked = serializedObject.FindProperty("OnPicked");
            OnDropped = serializedObject.FindProperty("OnDropped");
            ShowEvents = serializedObject.FindProperty("ShowEvents");
            FloatID = serializedObject.FindProperty("m_Value");
            IntID = serializedObject.FindProperty("m_ID");
            m_collider = serializedObject.FindProperty("m_collider");
        }

        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Pickable Item");
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);
                serializedObject.Update();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(m_collider);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ToggleLeft("Is Picked",m.IsPicked);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.PropertyField(IntID, new GUIContent("ID", "Int value the Pickable Item can store.. that it can be use for anything"));
                EditorGUILayout.PropertyField(FloatID, new GUIContent("Value", "Float value the Pickable Item can store.. that it can be use for anything"));
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(PickAnimations, new GUIContent("Pick Up Animations","Use Pick Up Animation to pick this item"));

                    if (PickAnimations.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(PickUpMode, new GUIContent("Mode","Mode that has the Pick Up animation"), GUILayout.MinWidth(170));
                        EditorGUIUtility.labelWidth = 40;
                        EditorGUILayout.PropertyField(PickUpAbility, new GUIContent("Ability", "Ability ID for the  Pick Up animation"), GUILayout.MinWidth(50));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();

                if (PickAnimations.boolValue)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(Align, new GUIContent("Align", "Align the Animal to the Item"));

                    if (Align.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(AlignDistance, new GUIContent("Distance", "Distance to move the Animal towards the Item"));
                        EditorGUIUtility.labelWidth = 40;
                        EditorGUILayout.PropertyField(AlignTime, new GUIContent("Time", "Time needed to do the Alignment"));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }



                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(DropAnimations, new GUIContent("Drop Animations ","Use Pick Up Animation to pick this item"));

                    if (PickAnimations.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(DropMode, new GUIContent("Mode", "Mode that has the Pick Up animation"), GUILayout.MinWidth(170));
                        EditorGUIUtility.labelWidth = 40;
                        EditorGUILayout.PropertyField(DropAbility, new GUIContent("Ability", "Ability ID for the  Pick Up animation"), GUILayout.MinWidth(20));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.indentLevel++;
                    ShowEvents.boolValue = EditorGUILayout.Foldout(ShowEvents.boolValue, "Events");
                    EditorGUI.indentLevel--;

                    if (ShowEvents.boolValue)
                    {
                        EditorGUILayout.PropertyField(OnFocused);
                        EditorGUILayout.PropertyField(OnPicked);
                        EditorGUILayout.PropertyField(OnDropped);
                    }
                }
                EditorGUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
            }
        }
    }
}