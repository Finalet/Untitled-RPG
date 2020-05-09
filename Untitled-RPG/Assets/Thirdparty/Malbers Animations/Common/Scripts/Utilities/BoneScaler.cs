using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    /// <summary>Uses presets to create/load/save bones scale variations of a character.</summary>
    [HelpURL("https://docs.google.com/document/d/1QBLQVWcDSyyWBDrrcS2PthhsToWkOayU0HUtiiSWTF8/edit#heading=h.lszqo9w9utck")]
    public class BoneScaler : MonoBehaviour
    {
        public BonePreset preset;

        //[Header("Auto find bones")]
        public Transform Root;
        public string[] Filter = new string[8] { "Pivot", "Attack", "Track", "Trigger", "Camera", "Target" , "Fire", "Debug" };

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
                    preset.Bones.Add(new MiniTransform(Bones[i].name, Bones[i].localPosition,  Bones[i].localScale));
                }

                if (transform.name == Bones[0].name)
                {
                    preset.Bones[0].name = "Root";
                }

                Debug.Log("Preset: " + preset.name + " Saved from "+ name);
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


                Debug.Log("Preset: " + preset.name + " Loaded on "+name);

            }
            else
            {
                Debug.LogWarning("There's no Preset to Load from");
            }
        }
    }
}