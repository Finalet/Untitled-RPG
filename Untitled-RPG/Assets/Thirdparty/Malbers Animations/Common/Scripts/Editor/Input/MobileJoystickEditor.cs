using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations
{
    [CustomEditor(typeof(MobileJoystick))]
    public class MobileJoystickEditor : Editor
    {
        private MonoScript script;
        SerializedProperty /*CameraInput,*/ invertX, invertY, sensitivityX, sensitivityY, axisValue, OnJoystickDown, OnJoystickUp, 
            OnAxisChange, OnXAxisChange, pressed, OnYAxisChange, OnJoystickPressed, AxisEditor, EventsEditor, ReferencesEditor, deathpoint;

        private void OnEnable()
        {
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

           // CameraInput = serializedObject.FindProperty("CameraInput");
            invertX = serializedObject.FindProperty("invertX");
            invertY = serializedObject.FindProperty("invertY");
            sensitivityX = serializedObject.FindProperty("sensitivityX");
            sensitivityY = serializedObject.FindProperty("sensitivityY");
            axisValue = serializedObject.FindProperty("axisValue");
            pressed = serializedObject.FindProperty("pressed");


            OnJoystickDown = serializedObject.FindProperty("OnJoystickDown");
            OnJoystickUp = serializedObject.FindProperty("OnJoystickUp");
            OnAxisChange = serializedObject.FindProperty("OnAxisChange");
            OnXAxisChange = serializedObject.FindProperty("OnXAxisChange");
            OnYAxisChange = serializedObject.FindProperty("OnYAxisChange");
            OnJoystickPressed = serializedObject.FindProperty("OnJoystickPressed");

            AxisEditor = serializedObject.FindProperty("AxisEditor");
            EventsEditor = serializedObject.FindProperty("EventsEditor");
            ReferencesEditor = serializedObject.FindProperty("ReferencesEditor");
            deathpoint = serializedObject.FindProperty("deathpoint");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Mobile Joystick Logic");
          

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    AxisEditor.boolValue = MalbersEditor.Foldout(AxisEditor, "Axis Properties", true);  

                    if (AxisEditor.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            MalbersEditor.BoolButton(invertX, new GUIContent("Invert X"));
                            MalbersEditor.BoolButton(invertY, new GUIContent("Invert Y"));
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(deathpoint, new GUIContent("Death Point", "If the Axis Magnitude is lower than this value then the Axis will zero out"));
                        EditorGUILayout.PropertyField(sensitivityX);
                        EditorGUILayout.PropertyField(sensitivityY);

                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    ReferencesEditor.boolValue = MalbersEditor.Foldout(ReferencesEditor, "References", true);

                    if (ReferencesEditor.boolValue)
                    {
                        EditorGUILayout.PropertyField(axisValue);
                        EditorGUILayout.PropertyField(pressed);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EventsEditor.boolValue = MalbersEditor.Foldout(EventsEditor, "Events", true);

                    if (EventsEditor.boolValue)
                    {
                        EditorGUILayout.PropertyField(OnJoystickDown);
                        EditorGUILayout.PropertyField(OnJoystickUp);
                        EditorGUILayout.PropertyField(OnJoystickPressed);
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(OnAxisChange);
                        EditorGUILayout.PropertyField(OnXAxisChange);
                        EditorGUILayout.PropertyField(OnYAxisChange);
                    }
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
    }
}