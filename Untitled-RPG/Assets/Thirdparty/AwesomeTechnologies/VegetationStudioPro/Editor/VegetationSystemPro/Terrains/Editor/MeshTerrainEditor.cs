using System;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.MeshTerrains
{
    [CustomEditor(typeof(MeshTerrain))]
    public class MeshTerrainEditor : VegetationStudioProBaseEditor
    {
        private MeshTerrain _meshTerrain;

        private static readonly string[] TabNames =
        {
            "Settings", "Mesh terrain sources", "Debug"
        };
        public override void OnInspectorGUI()
        {
            OverrideLogoTextureName = "Banner_MeshTerrain";
            LargeLogo = false;
            //HelpTopic = "home/vegetation-studio/components/vegetation-system";
            _meshTerrain = (MeshTerrain) target;

            base.OnInspectorGUI();

            _meshTerrain.CurrentTabIndex = GUILayout.SelectionGrid(_meshTerrain.CurrentTabIndex, TabNames, 3, EditorStyles.toolbarButton);
            switch (_meshTerrain.CurrentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawSourceInspector();
                    break;
                case 2:
                    DrawDebugInspector();
                    break;                                  
            }
        }

        void DrawSettingsInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", LabelStyle);

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            _meshTerrain.MeshTerrainData = EditorGUILayout.ObjectField("Mesh terrain data", _meshTerrain.MeshTerrainData, typeof(MeshTerrainData), true) as MeshTerrainData;
            if (GUILayout.Button("Create MeshTerrainData object"))
            {
                CreateMeshTerrainDataObject();
            }

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                if (_meshTerrain.MeshTerrainData)
                {
                    _meshTerrain.RefreshTerrainData();
                }
            }
           
            EditorGUILayout.HelpBox(
                "Create and drop a MeshTerrainData object here. This will store the generated mesh terrain data.",
                MessageType.Info);

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Spawning", LabelStyle);
            EditorGUI.BeginChangeCheck();
            _meshTerrain.MultiLevelRaycast =
                EditorGUILayout.Toggle("Multi level spawning", _meshTerrain.MultiLevelRaycast);
            EditorGUILayout.HelpBox(
                "Enabling multi level spawning will spawn vegetation on all intercecting levels in the mesh.",
                MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                VegetationStudioManager.ClearCache();
                EditorUtility.SetDirty(_meshTerrain);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            _meshTerrain.AutoAddToVegegetationSystem =
                EditorGUILayout.Toggle("Add/Remove at Enable/Disable", _meshTerrain.AutoAddToVegegetationSystem);
            EditorGUILayout.HelpBox("When set the terrain will add and remove itself when enabled/disabled. This will only work if automatic area calculation is disabled on the VegetationSystem. This is only done in playmode and builds", MessageType.Info);
            //_meshTerrain.ApplyFloatingOriginOffset = EditorGUILayout.Toggle("Apply floating origin at enable.", _meshTerrain.ApplyFloatingOriginOffset);
            //EditorGUILayout.HelpBox("Disable this if the loaded terrain is instantiated at original location and then moved to match floating origin.", MessageType.Info);
            GUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Info", LabelStyle);

            MeshTerrainData meshTerrainData = _meshTerrain.MeshTerrainData;
            if (meshTerrainData != null)
            {                                
                EditorGUILayout.LabelField("Nodes: " + meshTerrainData.lNodes.Count, LabelStyle);
                EditorGUILayout.LabelField("Triangles: " + meshTerrainData.lPrims.Count, LabelStyle);
            }
           
            GUILayout.EndVertical();
        }

        void DrawSourceInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add terrain sources", LabelStyle);

            bool addMesh = false;
            GUILayout.BeginHorizontal();
            DropZoneTools.DrawMeshTerrainDropZone(DropZoneType.MeshRenderer,_meshTerrain, ref addMesh);
            //DropZoneTools.DrawMeshTerrainDropZone(DropZoneType.Terrain, _meshTerrain, ref addMesh);           

            GUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(
                "Drop a mesh renderers or Unity terrains to add them to the terrain source data.",
                MessageType.Info);


            _meshTerrain.Filterlods = EditorGUILayout.Toggle("Skip LODs", _meshTerrain.Filterlods);
            EditorGUILayout.HelpBox("When checked import will skip all meshes with the name containing LOD1,LOD2 or LOD3.",
                MessageType.Info);

        

            GUILayout.EndVertical();

            if (addMesh)
            {
                EditorUtility.SetDirty(target);
                return;
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Included sources", LabelStyle);
            EditorGUILayout.Space();

            int removeIndex = -1;

            if (_meshTerrain.MeshTerrainMeshSourceList.Count > 0)
            {
                EditorGUILayout.LabelField("Meshes", LabelStyle);

                for (int i = 0; i <= _meshTerrain.MeshTerrainMeshSourceList.Count - 1; i++)
                {
                    GUILayout.BeginHorizontal();

                    MeshTerrainMeshSource meshTerrainMeshSource =_meshTerrain.MeshTerrainMeshSourceList[i];

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.LabelField("Mesh: ", LabelStyle, GUILayout.Width(50));
                    meshTerrainMeshSource.MeshRenderer = EditorGUILayout.ObjectField("", meshTerrainMeshSource.MeshRenderer, typeof(MeshRenderer), true, GUILayout.Width(150)) as MeshRenderer;
                    meshTerrainMeshSource.TerrainSourceID = (TerrainSourceID) EditorGUILayout.EnumPopup("", meshTerrainMeshSource.TerrainSourceID, GUILayout.Width(150));
                    if (GUILayout.Button("Remove"))
                    {
                        removeIndex = i;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        _meshTerrain.MeshTerrainMeshSourceList[i] = meshTerrainMeshSource;
                        _meshTerrain.NeedGeneration = true;
                        EditorUtility.SetDirty(target);
                    }

                    GUILayout.EndHorizontal();
                }
            }

            if (removeIndex > -1)
            {
                _meshTerrain.MeshTerrainMeshSourceList.RemoveAt(removeIndex);
                _meshTerrain.NeedGeneration = true;
                EditorUtility.SetDirty(target);

                //HandleUtility.Repaint();
            }

            if (_meshTerrain.MeshTerrainTerrainSourceList.Count > 0)
            {
                EditorGUILayout.LabelField("Terrains", LabelStyle);
            }

            GUILayout.EndVertical();

            if (_meshTerrain.MeshTerrainData)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Generate terrain data", LabelStyle);
                if (_meshTerrain.NeedGeneration)
                {
                    EditorGUILayout.HelpBox(
                        "The terrain sources have changed. Mesh terrain data needs to be regenerated.",
                        MessageType.Warning);
                }

                if (GUILayout.Button("Generate terrain data"))
                {
                    _meshTerrain.GenerateMeshTerrain();
                    _meshTerrain.NeedGeneration = false;
                    VegetationStudioManager.ClearCache();
                }
                EditorGUILayout.HelpBox(
                    "The generated data will be stored in the assigned MeshTerrainData object.",
                    MessageType.Info);
                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "You need to create and assign a mesh terrain data in order to generate the BVH tree.",
                    MessageType.Warning);
            }


           
        }

        public void CreateMeshTerrainDataObject()
        {
            MeshTerrainData meshTerrainData = CreateInstance<MeshTerrainData>();

            if (!AssetDatabase.IsValidFolder("Assets/MeshTerrainData"))
            {
                AssetDatabase.CreateFolder("Assets", "MeshTerrainData");
            }

            string filename = "MeshTerrainData_" + Guid.NewGuid() + ".asset";
            AssetDatabase.CreateAsset(meshTerrainData, "Assets/MeshTerrainData/" + filename);

            MeshTerrainData loadedMeshTerrainData = AssetDatabase.LoadAssetAtPath<MeshTerrainData>("Assets/MeshTerrainData/" + filename);
            _meshTerrain.MeshTerrainData = loadedMeshTerrainData;

            SetSceneDirty();
        }

        void SetSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(_meshTerrain.gameObject.scene);
                EditorUtility.SetDirty(_meshTerrain);
            }
        }

        void DrawDebugInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Debug", LabelStyle);
            
            EditorGUI.BeginChangeCheck();
            _meshTerrain.ShowDebugInfo = EditorGUILayout.Toggle("Show debug info", _meshTerrain.ShowDebugInfo);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
           
            GUILayout.EndVertical();
        }
    }
}
