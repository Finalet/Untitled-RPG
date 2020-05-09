using UnityEngine;
using UnityEditor;

namespace MalbersAnimations
{
    //[CustomEditor(typeof(RigidConstraintsB))]
    public class RigidConstraintsBEd : Editor
    {
        SerializedProperty OnEnter, OnExit, PosX, PosY, PosZ, RotX, RotY, RotZ;

        private void OnEnable()
        {
            OnEnter = serializedObject.FindProperty("OnEnter");
            OnExit = serializedObject.FindProperty("OnExit");

            PosX = serializedObject.FindProperty("PosX");
            PosY = serializedObject.FindProperty("PosY");
            PosZ = serializedObject.FindProperty("PosZ");

            RotX = serializedObject.FindProperty("RotX");
            RotY = serializedObject.FindProperty("RotY");
            RotZ = serializedObject.FindProperty("RotZ");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Modify the Rigidbody Constraints attached to this Animator");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnterDrag"));
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    OnEnter.boolValue = EditorGUILayout.Toggle("On Enter", OnEnter.boolValue, EditorStyles.radioButton);
                    OnExit.boolValue = !OnEnter.boolValue;

                    OnExit.boolValue = EditorGUILayout.Toggle("On Exit", OnExit.boolValue, EditorStyles.radioButton);
                    OnEnter.boolValue = !OnExit.boolValue;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Constraints  ", EditorStyles.boldLabel, GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("X", EditorStyles.boldLabel, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("Y", EditorStyles.boldLabel, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("     Z", EditorStyles.boldLabel, GUILayout.MaxWidth(35));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Position ", GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        PosX.boolValue = EditorGUILayout.Toggle(PosX.boolValue, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        PosY.boolValue = EditorGUILayout.Toggle(PosY.boolValue, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        PosZ.boolValue = EditorGUILayout.Toggle(PosZ.boolValue, GUILayout.MaxWidth(15));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Rotation ", GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        RotX.boolValue = EditorGUILayout.Toggle(RotX.boolValue, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        RotY.boolValue = EditorGUILayout.Toggle(RotY.boolValue, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        RotZ.boolValue = EditorGUILayout.Toggle(RotZ.boolValue, GUILayout.MaxWidth(15));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}