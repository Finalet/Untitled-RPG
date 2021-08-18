using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    /// <summary>Uses presets to create/load/save bones scale variations of a character.</summary>
    [AddComponentMenu("Malbers/Utilities/Mesh/Bone Scaler")]

    public class BoneScaler : MonoBehaviour
    {
        public BonePreset preset;

        //[Header("Auto find bones")]
        public Transform Root;
        public string[] Filter = new string[14] 
        { "Pivot", "Attack", "Track", "Trigger", "Camera", "Target", "Fire", "Debug","AI","Look","Appearance","Interaction","Internal","Mesh" };

        // public bool rotations;
        public List<Transform> Bones = new List<Transform>();

        /// <summary>Called when the Root bone Changes </summary>
        public void SetBones()
        {
            if (Root)
                Bones = Root.GetComponentsInChildren<Transform>().ToList();

            List<Transform> newbones = new List<Transform>();
            foreach (var b in Bones)
            {
                bool foundOne = false;

                if (b.GetComponent<SkinnedMeshRenderer>()) continue; //Means is a Mesh so skip it!

                for (int i = 0; i < Filter.Length; i++)
                {
                    if (b.name.ToLower().Contains(Filter[i].ToLower()))
                    {
                        foundOne = true;
                        break;
                    }
                }
                if (!foundOne)
                    newbones.Add(b);
            }

            Bones = newbones;
        }

        public void SavePreset()
        {
            if (preset)
            {
                preset.Bones = new List<MiniTransform>();

                for (int i = 0; i < Bones.Count; i++)
                {
                    preset.Bones.Add(new MiniTransform(Bones[i].name, Bones[i].localPosition, Bones[i].localScale));
                }

                if (transform.name == Bones[0].name)
                {
                    preset.Bones[0].name = "Root";
                }

                Debug.Log("Preset: " + preset.name + " Saved from " + name);
            }
            else
            {
                Debug.LogWarning("There's no Preset Asset to save the bones");
            }
        }

        void Reset()
        {
            Root = transform;
            SetBones();
        }

        public void LoadPreset()
        {
            if (preset)
            {
                Bones = transform.GetComponentsInChildren<Transform>().ToList(); ;

                List<Transform> newbones = new List<Transform>();

                if (preset.Bones[0].name == "Root")
                {
                    if (preset.scales) transform.localScale = preset.Bones[0].Scale;
                    Root = transform;
                    newbones.Add(transform);

                    //#if UNITY_EDITOR
                    //                    UnityEditor.EditorUtility.SetDirty(Root);
                    //#endif
                }

                foreach (var bone in preset.Bones)
                {
                    var Bone_Found = Bones.Find(item => item.name == bone.name);

                    if (Bone_Found)
                    {
                        if (preset.positions) Bone_Found.localPosition = bone.Position;
                        //if (rotations) Bone_Found.rotation = bone.rotation;
                        if (preset.scales) Bone_Found.localScale = bone.Scale;

                        newbones.Add(Bone_Found);

                        //#if UNITY_EDITOR
                        //                        UnityEditor.EditorUtility.SetDirty(Bone_Found);
                        //#endif
                    }
                }

                Bones = newbones;


                Debug.Log("Preset: " + preset.name + " Loaded on " + name);

            }
            else
            {
                Debug.LogWarning("There's no Preset to Load from");
            }
        }
    }



    //INSPECTOR!
#if UNITY_EDITOR
    [CustomEditor(typeof(BoneScaler))]
    public class BoneScalerEditor : Editor
    {
        BoneScaler M;
       // private MonoScript script;

        SerializedProperty /*positions, scales,*/ preset, Root;
        protected int index = 0;

        private void OnEnable()
        {
            M = (BoneScaler)target;
            //script = MonoScript.FromMonoBehaviour(M);

            preset = serializedObject.FindProperty("preset");
            Root = serializedObject.FindProperty("Root");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Save/Load Bones Transormations into a Preset");

            EditorGUILayout.BeginVertical(MTools.StyleGray);
            {
               // MalbersEditor.DrawScript(script);

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

                    MalbersEditor.Arrays(serializedObject.FindProperty("Filter"), new GUIContent("Filter |Skip bones with these names|"));
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
#endif

}