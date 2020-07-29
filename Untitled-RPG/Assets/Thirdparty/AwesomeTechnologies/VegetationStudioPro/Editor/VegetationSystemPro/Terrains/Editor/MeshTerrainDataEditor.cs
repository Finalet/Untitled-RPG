using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.MeshTerrains
{
    [CustomEditor(typeof(MeshTerrainData))]
    public class MeshTerrainDataEditor : VegetationStudioProBaseEditor
    {
        [MenuItem("Assets/Create/Awesome Technologies/Mesh Terrain/MeshTerrainData")]
        public static void CreateMeshTerrainDataScriptableObject()
        {
            ScriptableObjectUtility.CreateAndReturnAsset<MeshTerrainData>();
        }
    }
}
