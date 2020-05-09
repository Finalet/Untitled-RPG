using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(MStrafe))]
    [CanEditMultipleObjects]
    public class MStrafeEd : Editor
    {
        private MStrafe M;

        MonoScript script;
        SerializedProperty updateAnimator;

        private void OnEnable()
        {
            M = ((MStrafe)target);
            script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
            updateAnimator = serializedObject.FindProperty("UpdateAnimator");

        }   


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Strafing Logic"); 
       
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);

            MalbersEditor.DrawScript(script);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("active"), new GUIContent("Active", "Enable Disable the Strafing Logic"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SType"), new GUIContent("Use", "Use Camera or a Target to Calculate the Strafing"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MainCamera"), new GUIContent("Main Camera", "Use the Main Camera for the Strafe Logic"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Target"), new GUIContent("Target", "Use a Target for the Strafe Logic"));
            EditorGUILayout.EndVertical();


            //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("Gravity"), new GUIContent("Gravity", "Gravity Direction"));
            //EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Rotate"), new GUIContent("Rotate", "Add extra rotation while on Strafe Mode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SmoothValue"), new GUIContent("SmoothValue", "Smooth Value to the Rotation"));
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(updateAnimator, new GUIContent("Update Animator", "Update Animator with the Parameter"));

            if (M.UpdateAnimator)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StrafeAngle"), new GUIContent("Param Name", "Name of the Parameter on The Animator for the Strafing"));
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Strafe Inspector");
               // EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}