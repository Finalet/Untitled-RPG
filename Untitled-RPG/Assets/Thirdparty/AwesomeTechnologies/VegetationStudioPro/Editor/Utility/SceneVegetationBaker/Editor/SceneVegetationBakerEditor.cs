using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Baking
{
    [CustomEditor(typeof(SceneVegetationBaker))]
    public class SceneVegetationBakerEditor : VegetationStudioProBaseEditor
    {
        private SceneVegetationBaker _sceneVegetationBaker;
        
        private static readonly string[] VegetationTypeNames =
        {
            "All","Trees", "Large Objects", "Objects", "Plants", "Grass"
        };
        
        private int _vegIndex;
        private int _lastVegIndex;
        private int _selectedGridIndex;
        private int _selectedVegetationTypeIndex;
        
        public override void OnInspectorGUI()
        {
            HelpTopic = "scene-vegetation-baker";
            _sceneVegetationBaker = (SceneVegetationBaker)target;
                                   
            ShowLogo = false;

            base.OnInspectorGUI();

            if (!_sceneVegetationBaker.VegetationSystemPro)
            {
                EditorGUILayout.HelpBox(
                    "The SceneVegetationBaker Component needs to be added to a GameObject with a VegetationSystemPro Component.",
                    MessageType.Error);
                return;
            }


            VegetationSystemPro vegetationSystemPro = _sceneVegetationBaker.VegetationSystemPro;
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select biome/vegetation package", LabelStyle);
            string[] packageNameList = new string[vegetationSystemPro.VegetationPackageProList.Count];
            for (int i = 0; i <= vegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                if (vegetationSystemPro.VegetationPackageProList[i])
                {
                    packageNameList[i] = (i + 1).ToString() + " " +
                                         vegetationSystemPro.VegetationPackageProList[i].PackageName + " (" + vegetationSystemPro.VegetationPackageProList[i].BiomeType.ToString() + ")";;
                }
                else
                {
                    packageNameList[i] = "Not found";
                }
            }

            EditorGUI.BeginChangeCheck();
            _sceneVegetationBaker.VegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package",
                _sceneVegetationBaker.VegetationPackageIndex, packageNameList);
            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
            }
            
            GUILayout.EndVertical();    
            
            VegetationPackagePro vegetationPackagePro =
                vegetationSystemPro.VegetationPackageProList[_sceneVegetationBaker.VegetationPackageIndex];

            if (vegetationPackagePro == null) return;
            if (vegetationPackagePro.VegetationInfoList.Count == 0) return;
            
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select Vegetation Item", LabelStyle);

            EditorGUI.BeginChangeCheck();
            _selectedVegetationTypeIndex = GUILayout.SelectionGrid(_selectedVegetationTypeIndex, VegetationTypeNames, 3,
                EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                _selectedGridIndex = 0;
            }

            VegetationPackageEditorTools.VegetationItemTypeSelection vegetationItemTypeSelection =
                VegetationPackageEditorTools.GetVegetationItemTypeSelection(_selectedVegetationTypeIndex);

            int selectionCount = 0;

            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationSystemPro, vegetationPackagePro,
                ref _selectedGridIndex, ref _vegIndex, ref selectionCount, vegetationItemTypeSelection, 70);

            if (_lastVegIndex != _vegIndex) GUI.FocusControl(null);
            _lastVegIndex = _vegIndex;
                  
            GUILayout.EndVertical();
            
            VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[_vegIndex];
            if (vegetationItemInfoPro == null) return;                       

            GUILayout.BeginVertical("box");
            _sceneVegetationBaker.ExportStatic = EditorGUILayout.Toggle("Export as static objects", _sceneVegetationBaker.ExportStatic);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            if (GUILayout.Button("Bake VegetationItem to scene"))
            {
                BakeVegetationToScene(vegetationItemInfoPro);
            }

            GUILayout.EndVertical();
        }

        void BakeVegetationToScene( VegetationItemInfoPro vegetationItemInfoPro)
        {
            GameObject root = new GameObject
            {
                name = "BakedVegetationItem_" + vegetationItemInfoPro.VegetationItemID,
                isStatic = true
            };

            root.transform.position =  Vector3.zero;

            for (int i = 0; i <= _sceneVegetationBaker.VegetationSystemPro.VegetationCellList.Count - 1; i++)
            {
                if (i % 100 == 0)
                {
                    float progress = (float)i / _sceneVegetationBaker.VegetationSystemPro.VegetationCellList.Count;
                    EditorUtility.DisplayProgressBar("Bake to scene", "Spawn all vegetation item instances", progress);
                }
                AddVegetationItemsToScene(_sceneVegetationBaker.VegetationSystemPro.VegetationCellList[i], root, vegetationItemInfoPro);
            }

            EditorUtility.ClearProgressBar();            
            _sceneVegetationBaker.VegetationSystemPro.ClearCache(vegetationItemInfoPro.VegetationItemID);
        }

        void AddVegetationItemsToScene(VegetationCell vegetationCell, GameObject parent, VegetationItemInfoPro vegetationItemInfoPro)
        {
            if (!vegetationItemInfoPro.VegetationPrefab) return;            
            VegetationSystemPro vegetationSystemPro = _sceneVegetationBaker.VegetationSystemPro;
            
            vegetationSystemPro.SpawnVegetationCell(vegetationCell,vegetationItemInfoPro.VegetationItemID);
            NativeList<MatrixInstance> vegetationInstanceList =
                vegetationSystemPro.GetVegetationItemInstances(vegetationCell, vegetationItemInfoPro.VegetationItemID);
                
            for (int j = 0; j <= vegetationInstanceList.Length - 1; j++)
            {
                Matrix4x4 vegetationItemMatrix = vegetationInstanceList[j].Matrix;
                Vector3 position = MatrixTools.ExtractTranslationFromMatrix(vegetationItemMatrix);
                Vector3 scale = MatrixTools.ExtractScaleFromMatrix(vegetationItemMatrix);
                Quaternion rotation = MatrixTools.ExtractRotationFromMatrix(vegetationItemMatrix);

                GameObject vegetationItem = Instantiate(vegetationItemInfoPro.VegetationPrefab, parent.transform);
                vegetationItem.transform.position = position;
                vegetationItem.transform.localScale = scale;
                vegetationItem.transform.rotation = rotation;
                vegetationItem.isStatic = _sceneVegetationBaker.ExportStatic;                
            }                
            vegetationCell.ClearCache();                                             
        }
        
        private void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorSceneManager.MarkSceneDirty(_sceneVegetationBaker.gameObject.scene);
            EditorUtility.SetDirty(_sceneVegetationBaker);
        }
    }
}
