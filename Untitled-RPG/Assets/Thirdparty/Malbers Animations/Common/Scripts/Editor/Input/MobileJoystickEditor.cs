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
        SerializedProperty Jbutton, bg, invertX, invertY, sensitivityX, sensitivityY, axisValue, OnJoystickDown, OnJoystickUp, Drag, Dynamic,
            OnAxisChange, OnXAxisChange, pressed, OnYAxisChange, OnJoystickPressed, AxisEditor, EventsEditor, ReferencesEditor, deathpoint;

        private void OnEnable()
        {
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            // CameraInput = serializedObject.FindProperty("CameraInput");
            bg = serializedObject.FindProperty("bg");
            Jbutton = serializedObject.FindProperty("Jbutton");
            Dynamic = serializedObject.FindProperty("Dynamic");
            invertX = serializedObject.FindProperty("invertX");
            invertY = serializedObject.FindProperty("invertY");
            sensitivityX = serializedObject.FindProperty("sensitivityX");
            sensitivityY = serializedObject.FindProperty("sensitivityY");
            axisValue = serializedObject.FindProperty("axisValue");
            pressed = serializedObject.FindProperty("pressed");
            Drag = serializedObject.FindProperty("m_Drag");


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
                    EditorGUILayout.PropertyField(Jbutton, new GUIContent("Button Image"));
                    EditorGUILayout.PropertyField(bg, new GUIContent("Button Background"));
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {



                    if (MalbersEditor.Foldout(AxisEditor, "Axis Properties"))
                    {
                      

                        EditorGUILayout.BeginHorizontal();
                        {
                            MalbersEditor.BoolButton(invertX, new GUIContent("Invert X"));
                            MalbersEditor.BoolButton(invertY, new GUIContent("Invert Y"));
                            MalbersEditor.BoolButton(Drag, new GUIContent("Drag"));
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(deathpoint); 
                        EditorGUILayout.PropertyField(sensitivityX);
                        EditorGUILayout.PropertyField(sensitivityY);

                        if (!Drag.boolValue) EditorGUILayout.PropertyField(Dynamic);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                { 
                    if (MalbersEditor.Foldout(ReferencesEditor, "Exposed Values"))
                    {
                        EditorGUILayout.PropertyField(axisValue);
                        EditorGUILayout.PropertyField(pressed);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EventsEditor.boolValue = MalbersEditor.Foldout(EventsEditor, "Events");

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