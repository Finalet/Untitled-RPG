using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(MPickUp)), CanEditMultipleObjects]
    public class MPickUpEditor : Editor
    {
        private MonoScript script;
        private SerializedProperty PickUpArea, PickUpLayer, FocusedItem, AutoPick, Holder, RotOffset, item, PosOffset, CanPickUp , OnDropping, OnPicking, ShowEvents, DebugRadius, DebugColor; 

        private MPickUp m;

        private void OnEnable()
        {
            m = (MPickUp)target;
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            PickUpArea = serializedObject.FindProperty("PickUpArea");
            PickUpLayer = serializedObject.FindProperty("PickUpLayer");

            Holder = serializedObject.FindProperty("Holder");
            PosOffset = serializedObject.FindProperty("PosOffset");
            RotOffset = serializedObject.FindProperty("RotOffset");

            FocusedItem = serializedObject.FindProperty("FocusedItem");
            item = serializedObject.FindProperty("item");

            CanPickUp = serializedObject.FindProperty("CanPickUp");
            OnPicking = serializedObject.FindProperty("OnPicking");
            OnDropping = serializedObject.FindProperty("OnDropping");
            ShowEvents = serializedObject.FindProperty("ShowEvents");
            AutoPick = serializedObject.FindProperty("AutoPick");
            DebugColor = serializedObject.FindProperty("DebugColor");
            DebugRadius = serializedObject.FindProperty("DebugRadius");
        }

        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Pick Up Logic for Pickable Items");
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);
                serializedObject.Update();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(PickUpArea);
                EditorGUILayout.PropertyField(PickUpLayer);
                EditorGUILayout.PropertyField(AutoPick);
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(Holder);
                    EditorGUILayout.LabelField("Offsets", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(PosOffset, new GUIContent("Position", "Position Local Offset to parent the item to the holder"));
                    EditorGUILayout.PropertyField(RotOffset, new GUIContent("Rotation", "Rotation Local Offset to parent the item to the holder"));
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(item);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(FocusedItem);
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndVertical();


                GUIStyle styles = new GUIStyle(EditorStyles.foldout);

                styles.fontStyle = FontStyle.Bold;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.indentLevel++;
                    ShowEvents.boolValue = EditorGUILayout.Foldout(ShowEvents.boolValue, "Events", styles);
                    EditorGUI.indentLevel--;

                    if (ShowEvents.boolValue)
                    {
                        EditorGUILayout.PropertyField(CanPickUp);
                        EditorGUILayout.PropertyField(OnPicking);
                        EditorGUILayout.PropertyField(OnDropping);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(DebugRadius);
                    EditorGUILayout.PropertyField(DebugColor, GUIContent.none, GUILayout.MaxWidth(40));
                }
                EditorGUILayout.EndHorizontal();

                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
            }
        }
    }
}