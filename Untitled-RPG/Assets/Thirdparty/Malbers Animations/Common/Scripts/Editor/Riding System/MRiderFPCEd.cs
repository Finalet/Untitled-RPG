using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace MalbersAnimations.HAP
{
     [CustomEditor(typeof(RiderFPC),true)] 
    public class MRiderFPCEd : MRiderEd
    {

        private SerializedProperty MountOffset, FollowRotation; 
        protected override void OnEnable()
        {
            base.OnEnable();
            MountOffset = serializedObject.FindProperty("MountOffset");
            FollowRotation = serializedObject.FindProperty("FollowRotation");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(MountOffset);
                EditorGUILayout.PropertyField(FollowRotation);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
