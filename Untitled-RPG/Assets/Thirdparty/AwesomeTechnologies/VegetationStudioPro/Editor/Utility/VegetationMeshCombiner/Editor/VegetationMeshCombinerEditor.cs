using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using AwesomeTechnologies.TouchReact;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.MeshTools
{
    [CustomEditor(typeof(VegetationMeshCombiner))]
    public class VegetationMeshCombinerEditor : VegetationStudioProBaseEditor
    {
        private VegetationMeshCombiner _vegetationMeshCombiner;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            OverrideLogoTextureName = "Banner_VegetationMeshCombiner";
            LargeLogo = false;
            
            _vegetationMeshCombiner = (VegetationMeshCombiner) target;
            _vegetationMeshCombiner.TargetGameObject = EditorGUILayout.ObjectField("Root GameObject", _vegetationMeshCombiner.TargetGameObject, typeof(GameObject),true) as GameObject;
            EditorGUILayout.HelpBox("This tool will merge multiple meshes with a single mesh and materials into a single mesh with submeshes.", MessageType.Info);
            EditorGUILayout.HelpBox("Add all meshes as children to a GameObject and assign it as root gameobject. A dialog will ask you for a location to save the merged mesh. A new Gameobject will be created in the scene with the merged mesh and materials set up.", MessageType.Info);

            
            if (!_vegetationMeshCombiner.TargetGameObject)
            {
                EditorGUILayout.HelpBox("You need to assign a root GameObject", MessageType.Warning);
            }

            _vegetationMeshCombiner.MergeSubmeshesWitEquialMaterial = EditorGUILayout.Toggle("Merge submeshes",
                _vegetationMeshCombiner.MergeSubmeshesWitEquialMaterial);
            EditorGUILayout.HelpBox("When enabled this will merge submeshes using the same material.", MessageType.Info);
    
            if (GUILayout.Button("Generate merged mesh"))
            {
                if (_vegetationMeshCombiner.TargetGameObject)
                {
                    MergeMesh();
                }
                
                
                
            }
        }

        void MergeMesh()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save merged mesh", "", "asset",
                "Please enter a file name to save the merged mesh to");
            if (path.Length != 0)
            {
                GameObject mergedMesh =VegetationMeshCombiner.CombineMeshes(_vegetationMeshCombiner.TargetGameObject, _vegetationMeshCombiner.MergeSubmeshesWitEquialMaterial);
                MeshFilter meshFilter = mergedMesh.GetComponentInChildren<MeshFilter>();                 
                AssetDatabase.CreateAsset(meshFilter.sharedMesh, path);
            }           
        }
    }
}

