using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace MalbersAnimations
{
    [CustomEditor(typeof(Tag))]
    public class TagEd : Editor
    {
        private void OnEnable()
        {
            SerializedProperty id = serializedObject.FindProperty("id");
            id.intValue = target.name.GetHashCode();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
    }
}