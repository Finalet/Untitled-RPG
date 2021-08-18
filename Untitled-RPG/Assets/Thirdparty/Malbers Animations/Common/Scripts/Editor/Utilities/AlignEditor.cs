using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(Aligner)), CanEditMultipleObjects]
    public class AlignEditor : Editor
    {

        SerializedProperty
            AlignPos, AlignRot, AlignLookAt, AlingPoint1, AlingPoint2, AlignTime, AlignCurve, DoubleSided, LookAtRadius, DebugColor, LookAtRadiusTime, AngleOffset;

       // MonoScript script;
        private void OnEnable()
        {
            //script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

            AlignPos = serializedObject.FindProperty("AlignPos");
            AngleOffset = serializedObject.FindProperty("AngleOffset");
            AlignRot = serializedObject.FindProperty("AlignRot");
            AlignLookAt = serializedObject.FindProperty("AlignLookAt");
            AlingPoint1 = serializedObject.FindProperty("mainPoint");
            AlingPoint2 = serializedObject.FindProperty("secondPoint");
            AlignTime = serializedObject.FindProperty("AlignTime");
            AlignCurve = serializedObject.FindProperty("AlignCurve");
            DoubleSided = serializedObject.FindProperty("DoubleSided");
            LookAtRadius = serializedObject.FindProperty("LookAtRadius");
            DebugColor = serializedObject.FindProperty("DebugColor");
            LookAtRadiusTime = serializedObject.FindProperty("LookAtRadiusTime");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Utility to align the Position and Rotation of an Target Object relative this one");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                //MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();

                    var currentGUIColor = GUI.color;
                    var selected = (GUI.color + Color.green) / 2;
                   
                    
                    GUI.color = AlignPos.boolValue ? selected : currentGUIColor;
                    AlignPos.boolValue = GUILayout.Toggle(AlignPos.boolValue, new GUIContent("Position", "Align Position"), EditorStyles.miniButton);

                    GUI.color = AlignRot.boolValue ? selected : currentGUIColor;
                    AlignRot.boolValue = GUILayout.Toggle(AlignRot.boolValue, new GUIContent("Rotation", "Align Rotation"), EditorStyles.miniButton);
                    if (AlignPos.boolValue || AlignRot.boolValue) AlignLookAt.boolValue = false;

                    GUI.color = AlignLookAt.boolValue ? selected : currentGUIColor;
                    AlignLookAt.boolValue = GUILayout.Toggle(AlignLookAt.boolValue, new GUIContent("Look At", "Align a gameObject Looking at the Aligner"), EditorStyles.miniButton);
                  
                    GUI.color = currentGUIColor;

                    if (AlignLookAt.boolValue) AlignPos.boolValue = AlignRot.boolValue = false;
                    EditorGUILayout.EndHorizontal();



                    if (AlignRot.boolValue || AlignPos.boolValue)
                        EditorGUILayout.PropertyField(DoubleSided, new GUIContent("Double Sided", "When Rotation is Enabled then It will find the closest Rotation"));

                    if (AlignLookAt.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(LookAtRadius, new GUIContent("Radius", "The Target will move close to the Aligner equals to the Radius"));
                     
                        EditorGUILayout.PropertyField(DebugColor, GUIContent.none, GUILayout.MaxWidth(40));
                        EditorGUILayout.EndHorizontal();

                        if (LookAtRadius.floatValue > 0)
                            EditorGUILayout.PropertyField(LookAtRadiusTime, new GUIContent("Look At Align Time", "Time to move The Target to the Aligner "));
                    }
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(AlingPoint1, new GUIContent("Main Point", "The Target GameObject will move to the Position of the Align Point"));
                    if (AlignPos.boolValue)
                    EditorGUILayout.PropertyField(AlingPoint2, new GUIContent("2nd Point", "If Point End is Active then the Animal will align to the closed position from the 2 align points line"));
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(AlignTime, new GUIContent("Align Time", "Time needed to make the Aligment"));
                    EditorGUILayout.PropertyField(AlignCurve, GUIContent.none, GUILayout.MaxWidth(75));
                    EditorGUILayout.EndHorizontal();
                }

                if (AlignRot.boolValue || AlignLookAt.boolValue)
                    EditorGUILayout.PropertyField(AngleOffset);
               
                EditorGUILayout.EndVertical();


            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Aligner Inspector");
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}