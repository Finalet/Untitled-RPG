using UnityEngine;
using System.Collections;
using UnityEditor;

namespace MalbersAnimations.Controller
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DragonEgg))]
    public class DragonEggEditor : Editor
    {
        MonoScript script;

        DragonEgg DE;
        private SerializedProperty input, time, hatchtype;

        private void OnEnable()
        {
            DE = (DragonEgg)target;

            script = MonoScript.FromMonoBehaviour(DE);

            hatchtype = serializedObject.FindProperty("hatchtype");
            input = serializedObject.FindProperty("input");
            time = serializedObject.FindProperty("seconds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Egg Logic");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                    MalbersEditor.DrawScript(script);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.HelpBox("Use Baby Dragons or Scale Up the egg", MessageType.None, true);


                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Dragon"), new GUIContent("Dragon", "the Prefab or Gameobject that contains the little dragon"));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("preHatchOffset"), new GUIContent("Pre-Hatch Offset"));

                        EditorGUILayout.PropertyField(hatchtype, new GUIContent("Hatch Type"));

                        DragonEgg.HatchType ht = (DragonEgg.HatchType)hatchtype.enumValueIndex;

                        switch (ht)
                        {
                            case DragonEgg.HatchType.None:
                                EditorGUILayout.HelpBox("Just Call the Method CrackEgg() to activate it", MessageType.Info, true);
                                break;
                            case DragonEgg.HatchType.Time:
                                EditorGUILayout.PropertyField(time, new GUIContent("Time", "ammount of Seconds to Hatch"));
                                break;
                            case DragonEgg.HatchType.Input:
                                EditorGUILayout.PropertyField(input, new GUIContent("Input", "Input assigned in the InputManager to Hatch"));
                                break;
                            default:
                                break;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEggCrack"), new GUIContent("On Egg Crack", "Invoked When the Egg Crack"));
                }
                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Dragon Egg Values Changed");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}