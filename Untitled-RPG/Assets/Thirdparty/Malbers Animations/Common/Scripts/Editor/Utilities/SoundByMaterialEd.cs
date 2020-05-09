using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(SoundByMaterial))]
    public class SoundByMaterialEd : Editor
    {
        private ReorderableList list;
        SerializedProperty soundbymaterial;
        private SoundByMaterial M;
        private MonoScript script;

        private void OnEnable()
        {
            M = (SoundByMaterial)target;
            script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

            soundbymaterial = serializedObject.FindProperty("materialSounds");

            list = new ReorderableList(serializedObject, soundbymaterial, true, true, true, true);

            list.drawElementCallback = DrawElementCallback;
            list.drawHeaderCallback = HeaderCallbackDelegate;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Plays the sound matching the physic material on the hit object\nInvoke the method 'PlayMaterialSound'");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                    MalbersEditor.DrawScript(script);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("audioSource"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultSound"));
                    EditorGUILayout.EndVertical();

                    list.DoLayoutList();

                    if (list.index != -1)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            SerializedProperty Element = soundbymaterial.GetArrayElementAtIndex(list.index);
                            SerializedProperty SoundElement = Element.FindPropertyRelative("Sounds");

                            MalbersEditor.Arrays(SoundElement);
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
               
                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "SoundByMat Inspector");
            }
            serializedObject.ApplyModifiedProperties();

        }
        void HeaderCallbackDelegate(Rect rect)
        {
            //Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 14, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Sound by Material List ", EditorStyles.miniLabel);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = M.materialSounds[index];
            rect.y += 2;

            Rect R_1 = new Rect(rect.x, rect.y, (rect.width ) , EditorGUIUtility.singleLineHeight);
            //Rect R_2 = new Rect(rect.x + 25 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 14, EditorGUIUtility.singleLineHeight);

            element.material = (PhysicMaterial)EditorGUI.ObjectField(R_1, element.material, typeof(PhysicMaterial), false);

        }

       
    }
}
