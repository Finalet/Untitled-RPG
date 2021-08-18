using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Mesh/Rebone Mesh")]
    public class ReboneMesh : MonoBehaviour
    {

        //[ContextMenuItem("Transfer Bones From Skin", "DuplicateBones")]
        //public GameObject _sourceSkinMesh;

        [ContextMenuItem("Transfer Bones From Root", "TransferRootBone")]
        public Transform RootBone;



        //[ContextMenu("Transfer Bones From Skin")]
        //void DuplicateBones()
        //{
        //    if (_sourceSkinMesh != null)
        //    {
        //        CopyFromSkinToSkin();
        //        Debug.Log("Trasfer Ready");
        //    }
        //}

        [ContextMenu("Transfer Bones From Root")]
       public void TransferRootBone()
        {
            if (RootBone != null)
            {
                CopyBonesSameBones();
              
            }
        }



        //private void CopyFromSkinToSkin()
        //{
        //    SkinnedMeshRenderer targetRenderer = _sourceSkinMesh.GetComponent<SkinnedMeshRenderer>();

        //    Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        //    foreach (Transform bone in targetRenderer.bones)
        //    {
        //        boneMap[bone.name] = bone;
        //    }

        //    SkinnedMeshRenderer thisRenderer = GetComponent<SkinnedMeshRenderer>();
        //    Transform[] boneArray = thisRenderer.bones;
        //    for (int idx = 0; idx < boneArray.Length; ++idx)
        //    {
        //        string boneName = boneArray[idx].name;
        //        if (false == boneMap.TryGetValue(boneName, out boneArray[idx]))
        //        {
        //            Debug.LogError("failed to get bone: " + boneName);
        //            Debug.Break();
        //        }
        //    }
        //    thisRenderer.bones = boneArray;
        //    thisRenderer.rootBone = targetRenderer.rootBone;
        //}

        private void CopyBonesSameBones()
        {
            SkinnedMeshRenderer thisRenderer = GetComponent<SkinnedMeshRenderer>();
            if (thisRenderer == null) return;

            var OldRootBone = thisRenderer.rootBone;

            Transform[] rootBone = RootBone.GetComponentsInChildren<Transform>();

            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

            foreach (Transform bone in rootBone)
            {
                boneMap[bone.name] = bone;
            }

            Transform[] boneArray = thisRenderer.bones;


            for (int idx = 0; idx < boneArray.Length; ++idx)
            {
                string boneName = boneArray[idx].name;

                if (false == boneMap.TryGetValue(boneName, out boneArray[idx]))
                {
                    Debug.LogError("failed to get bone: " + boneName);
                }
            }
            thisRenderer.bones = boneArray;

            if (boneMap.TryGetValue(OldRootBone.name, out Transform ro))
            {
                thisRenderer.rootBone = ro; //Remap the rootbone
            }

            Debug.Log($"Bone Trasfer Completed: {name}");
 
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ReboneMesh)),CanEditMultipleObjects]
    public class ReboneMeshEd : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Retarget Bones"))
            {
                foreach (var targ in targets)
                {
                    (targ as ReboneMesh).TransferRootBone();
                    EditorUtility.SetDirty(targ);
                }
            }
        }
    }
#endif
}