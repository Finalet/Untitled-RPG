using System;
using System.Collections.Generic;
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEngine;


public class MeshTerrainToolWindow : EditorWindow
{

    private bool _addAsChild;

    private readonly List<MeshRenderer> _meshRendererList = new List<MeshRenderer>();
    [MenuItem("Window/Awesome Technologies/Mesh terrain setup tool")]
    // ReSharper disable once UnusedMember.Local
    static void Init()
    {
        MeshTerrainToolWindow window = (MeshTerrainToolWindow)GetWindow(typeof(MeshTerrainToolWindow));
        window.Show();
    }
    // ReSharper disable once UnusedMember.Local
    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Add Meshes", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        GameObject go = EditorGUILayout.ObjectField("Add mesh", null, typeof(GameObject), true) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            if (go != null)
            {
                for (int i = 0; i <= Selection.gameObjects.Length - 1; i++)
                {
                    MeshRenderer meshRenderer = Selection.gameObjects[i].GetComponent<MeshRenderer>();
                    if (meshRenderer)
                    {
                        _meshRendererList.Add(meshRenderer);
                    }
                }

                if (Selection.gameObjects.Length == 0)
                {
                    MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
                    if (meshRenderer)
                    {
                        _meshRendererList.Add(meshRenderer);
                        Debug.Log(meshRenderer);
                    }
                }
            }
        }

        EditorGUILayout.HelpBox("Add meshes with a MeshRenderer. Make sure you get a selection in the hierarchy then drag and drop this. You can drag and drop multiple at the same time or a root with children.", MessageType.Info);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Mesh renderers", EditorStyles.boldLabel);
        if (GUILayout.Button("Clear list"))
        {
            _meshRendererList.Clear();
        }

        for (int i = 0; i <= _meshRendererList.Count - 1; i++)
        {
            EditorGUI.BeginChangeCheck();
            _meshRendererList[i] =
                EditorGUILayout.ObjectField("Mesh Renderer", _meshRendererList[i], typeof(MeshRenderer), true) as
                    MeshRenderer;
            if (EditorGUI.EndChangeCheck())
            {
                if (_meshRendererList[i] == null)
                {
                    _meshRendererList.RemoveAt(i);
                    GUILayout.EndVertical();
                    return;
                }
            }
        }
        GUILayout.EndVertical();

        if (_meshRendererList.Count > 0)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Create", EditorStyles.boldLabel);

            _addAsChild = EditorGUILayout.Toggle("Add as child of mesh", _addAsChild);

            if (GUILayout.Button("Create mesh terrains"))
            {
                CreateMeshTerrains(false);
            }

            if (GUILayout.Button("Create mesh terrains and add to vegetation system"))
            {
                CreateMeshTerrains(true);
            }
            EditorGUILayout.HelpBox("This will create one mesh terrain per mesh, create the MeshTerrainData scriptable object and generate the BVH tree.", MessageType.Info);
            GUILayout.EndVertical();
        }
    }

    void CreateMeshTerrains(bool addToVegetationSystem)
    {
        for (int i = 0; i <= _meshRendererList.Count - 1; i++)
        {
            MeshRenderer meshRenderer = _meshRendererList[i];
            if (meshRenderer == null) continue;

            GameObject go = new GameObject("MeshTerrain_" + meshRenderer.name);
            if (_addAsChild)
            {
                go.transform.SetParent(meshRenderer.transform);
            }
            MeshTerrain meshTerrain = go.AddComponent<MeshTerrain>();
          

            MeshTerrainData meshTerrainData = CreateInstance<MeshTerrainData>();

            if (!AssetDatabase.IsValidFolder("Assets/MeshTerrainData"))
            {
                AssetDatabase.CreateFolder("Assets", "MeshTerrainData");
            }              

            string filename = "MeshTerrainData_" + Guid.NewGuid() + ".asset";
            AssetDatabase.CreateAsset(meshTerrainData,"Assets/MeshTerrainData/" + filename);

            MeshTerrainData loadedMeshTerrainData = AssetDatabase.LoadAssetAtPath<MeshTerrainData>("Assets/MeshTerrainData/" + filename);
            meshTerrain.MeshTerrainData = loadedMeshTerrainData;
            meshTerrain.AddMeshRenderer(meshRenderer.gameObject, TerrainSourceID.TerrainSourceID1);
            meshTerrain.GenerateMeshTerrain();

            if (addToVegetationSystem)
            {
                VegetationStudioManager.AddTerrain(go,true);
            }
        }
    }
}

