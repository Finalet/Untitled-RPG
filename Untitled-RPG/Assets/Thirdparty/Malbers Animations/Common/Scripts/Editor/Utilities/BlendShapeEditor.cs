using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(BlendShape))/*,CanEditMultipleObjects*/]
    public class BlendShapeEditor : Editor
    {
        BlendShape M;
       // private MonoScript script;
        protected int index = 0;
        SerializedProperty blendShapes, preset, LODs, mesh, random, LoadPresetOnStart;

        private void OnEnable()
        {
            M = (BlendShape)target;
           // script = MonoScript.FromMonoBehaviour(M);
            blendShapes = serializedObject.FindProperty("blendShapes");
            preset = serializedObject.FindProperty("preset");
            LODs = serializedObject.FindProperty("LODs");
            mesh = serializedObject.FindProperty("mesh");
            random = serializedObject.FindProperty("random");
            LoadPresetOnStart = serializedObject.FindProperty("LoadPresetOnStart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Adjust the Blend Shapes on the Mesh");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                 //   MalbersEditor.DrawScript(script);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            EditorGUILayout.PropertyField(mesh);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                            M.SetShapesCount();
                            EditorUtility.SetDirty(target);
                        }

                        MalbersEditor.Arrays(LODs, new GUIContent("LODs", "Other meshes with Blend Shapes to change"));
                    }
                    EditorGUILayout.EndVertical();

                    int Length = 0;
                    if (mesh.objectReferenceValue != null)
                        Length = blendShapes.arraySize;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            if (Length > 0)
                            {
                                int pin = serializedObject.FindProperty("PinnedShape").intValue;
                                EditorGUILayout.LabelField(new GUIContent("Pin Shape:              (" + pin + ") |" + M.mesh.sharedMesh.GetBlendShapeName(pin) + "|", "Current Shape Store to modigy When accesing public methods from other scripts"));
                            }
                        }
                        EditorGUILayout.EndVertical();
                         
                        if (Length > 0)
                        {
                            if (M.blendShapes == null)
                            {
                                M.blendShapes = M.GetBlendShapeValues();
                                serializedObject.ApplyModifiedProperties();
                            }

                            for (int i = 0; i < Length; i++)
                            {
                                if (i >= M.mesh.sharedMesh.blendShapeCount) continue;

                                var bs = blendShapes.GetArrayElementAtIndex(i);
                                if (bs != null && M.mesh.sharedMesh != null)
                                {

                                    bs.floatValue = EditorGUILayout.Slider("(" + i.ToString("D2") + ") " + M.mesh.sharedMesh.GetBlendShapeName(i), bs.floatValue, 0, 100);
                                }
                                //EditorUtility.SetDirty(M.mesh);
                            }

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            {
                                EditorGUILayout.PropertyField(preset, new GUIContent("Preset", "Saves the Blend Shapes values to a scriptable Asset"));
                            }
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            {
                                EditorGUILayout.LabelField("On Start", EditorStyles.boldLabel);
                                EditorGUI.BeginDisabledGroup(preset.objectReferenceValue == null);
                                EditorGUILayout.PropertyField(LoadPresetOnStart, new GUIContent("Load Preset", "Load a  Blend Shape Preset on Start"));
                                EditorGUI.EndDisabledGroup();

                                EditorGUI.BeginDisabledGroup(preset.objectReferenceValue != null && LoadPresetOnStart.boolValue);
                                EditorGUILayout.PropertyField(random, new GUIContent("Random", "Make Randoms Blend Shapes at start"));
                                EditorGUI.EndDisabledGroup();

                            }
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("Reset"))
                                {
                                    //Undo.RecordObject(target, "Reset Blend Shape");
                                   // Undo.RecordObject(M.mesh, "Reset Blend Shape");

                                    for (int i = 0; i < Length; i++)
                                    {
                                        blendShapes.GetArrayElementAtIndex(i).floatValue = 0; 
                                    }
                                   // EditorUtility.SetDirty(M.mesh);
                                  //  M.UpdateBlendShapes();
                                }
                                if (GUILayout.Button("Randomize"))
                                {
                                  //  Undo.RecordObject(target, "Randomize Blend Shape");
                                  //  Undo.RecordObject(M.mesh, "Randomize Blend Shape");

                                    for (int i = 0; i < Length; i++)
                                    {
                                        blendShapes.GetArrayElementAtIndex(i).floatValue = Random.Range(0, 100);
                                    }

                                  //  M.UpdateBlendShapes();
                                    //  EditorUtility.SetDirty(M.mesh);
                                }
                                if (GUILayout.Button("Save"))
                                {
                                    if (preset.objectReferenceValue == null)
                                    {
                                        string newBonePath = EditorUtility.SaveFilePanelInProject("Create New Blend Preset", "BlendShape preset", "asset", "Message");

                                        BlendShapePreset bsPreset = CreateInstance<BlendShapePreset>();

                                        AssetDatabase.CreateAsset(bsPreset, newBonePath);

                                        preset.objectReferenceValue = bsPreset;
                                        serializedObject.ApplyModifiedProperties();

                                        Debug.Log("New Blend Shape Preset Created");
                                        M.SavePreset();

                                    }
                                    else
                                    {
                                        if (EditorUtility.DisplayDialog("Overwrite Blend Shape Preset", "Are you sure to overwrite the preset?", "Yes", "No"))
                                        {
                                            M.SavePreset();
                                        }
                                    }
                                    //EditorUtility.SetDirty(M);
                                    //EditorUtility.SetDirty(M.preset);
                                }

                                EditorGUI.BeginDisabledGroup(preset.objectReferenceValue == null);
                                {
                                    if (GUILayout.Button("Load"))
                                    {
                                        if (preset.objectReferenceValue != null)
                                        {
                                            if (M.preset.blendShapes == null || M.preset.blendShapes.Length == 0)
                                                Debug.LogWarning("The preset " + M.preset.name + " is empty, Please use a Valid Preset");
                                            else
                                            {
                                                M.LoadPreset();
                                                EditorUtility.SetDirty(target);
                                            }
                                        }
                                    }

                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Blend Shapes Changed");
                if (M.mesh)  Undo.RecordObject(M.mesh, "Blend Shapes Changed");

                M.UpdateBlendShapes() ;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
