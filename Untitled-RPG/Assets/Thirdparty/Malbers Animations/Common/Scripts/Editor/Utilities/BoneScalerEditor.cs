using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(BoneScaler))]
    public class BoneScalerEditor : Editor
    {
        BoneScaler M;
        private MonoScript script;

        SerializedProperty /*positions, scales,*/ preset, Root;
        protected int index = 0;

        private void OnEnable()
        {
            M = (BoneScaler)target;
            script = MonoScript.FromMonoBehaviour(M);

            preset = serializedObject.FindProperty("preset");
            Root = serializedObject.FindProperty("Root");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Save/Load Bones Transormations into a Preset");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(preset);

                    bool disable_ = preset.objectReferenceValue == null;

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("New Preset"))
                        {
                            string newBonePath = EditorUtility.SaveFilePanelInProject("Create New Bone Preset", "new bone preset", "asset", "Message");

                            BonePreset bonePreset = CreateInstance<BonePreset>();

                            AssetDatabase.CreateAsset(bonePreset, newBonePath);

                            preset.objectReferenceValue = bonePreset;

                            EditorUtility.SetDirty(target);
                            Debug.Log("New Bone Preset Created");
                        }

                        EditorGUI.BeginDisabledGroup(disable_);
                        {
                            if (GUILayout.Button("Save"))
                            {
                                M.SavePreset();
                                EditorUtility.SetDirty(M.preset);
                            }

                            if (GUILayout.Button("Load"))
                            {
                                foreach (var bn in M.Bones)
                                {
                                    Undo.RecordObject(bn, "Bones Loaded"); // Save the bones loaded
                                }
                                
                                M.LoadPreset(); 
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Bones (" + M.Bones.Count.ToString() + ")");
                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(Root);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Root Changed");
                        EditorUtility.SetDirty(M);
                        serializedObject.ApplyModifiedProperties();
                        M.SetBones();
                    }

                    MalbersEditor.Arrays(serializedObject.FindProperty("Filter"), new GUIContent("Filter|Not Include bones with names|"));
                }
                EditorGUILayout.EndVertical(); 

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    MalbersEditor.Arrays(serializedObject.FindProperty("Bones"));
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
