using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.PrefabSpawner
{
    [CustomEditor(typeof(RuntimePrefabSpawner))]
    public class RuntimePrefabSpawnerEditor : VegetationStudioProBaseEditor
    {
        private RuntimePrefabSpawner _runtimePrefabSpawner;
        private int _vegIndex;
        private int _lastVegIndex;
        private int _selectedGridIndex;
        private int _selectedVegetationTypeIndex;
        
        private static readonly string[] TabNames =
        {
            "Settings","Editor", "Debug"
        };
        
        private static readonly string[] VegetationTypeNames =
        {
           "All","Trees", "Large Objects", "Objects", "Plants", "Grass"
        };
        
        public override void OnInspectorGUI()
        {
            _runtimePrefabSpawner = (RuntimePrefabSpawner) target;
            OverrideLogoTextureName = "Banner_RuntimePrefabSpawner";
            LargeLogo = false;
            base.OnInspectorGUI();
			
            VegetationSystemPro vegetationSystemPro = _runtimePrefabSpawner.VegetationSystemPro;
            if (!vegetationSystemPro)
            {
                EditorGUILayout.HelpBox("This component needs to be added to a GameObject with a VegetationSystemPro component.", MessageType.Error);
                return;
            }
            
            if (!_runtimePrefabSpawner.enabled)
            {
                EditorGUILayout.HelpBox("Component is disabled. Enable to use run-time prefab spawning.", MessageType.Warning);
            }
            
            
            EditorGUI.BeginChangeCheck();
            _runtimePrefabSpawner.CurrentTabIndex = GUILayout.SelectionGrid(_runtimePrefabSpawner.CurrentTabIndex, TabNames, 3, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
                SetSceneDirty();
            }
            switch (_runtimePrefabSpawner.CurrentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawEditorInspector();
                    break;        
                case 2:
                    DrawDebugInspector();
                    break;               
            }         			
        }

        private void DrawDebugInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Debug info", LabelStyle);
            EditorGUI.BeginChangeCheck();
            _runtimePrefabSpawner.ShowDebugCells = EditorGUILayout.Toggle("Show visible vegetation cells", _runtimePrefabSpawner.ShowDebugCells);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
                SetSceneDirty();
            }
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Runtime info", LabelStyle);

            if (_runtimePrefabSpawner.VisibleVegetationCellSelector != null)
            {					
                EditorGUILayout.LabelField("Visible cells: " + _runtimePrefabSpawner.VisibleVegetationCellSelector.VisibleSelectorVegetationCellList.Count.ToString(), LabelStyle);
				
                EditorGUILayout.LabelField("Loaded instances: " + _runtimePrefabSpawner.GetLoadedInstanceCount(), LabelStyle);
                EditorGUILayout.LabelField("Visible colliders: " + _runtimePrefabSpawner.GetVisibleColliders(), LabelStyle);	
                if (GUILayout.Button("Refresh"))
                {

                }
            }
            else
            {
                EditorGUILayout.HelpBox("Colliders run-time info only show in playmode.", MessageType.Info);
            }			
            GUILayout.EndVertical();			
        }

        private void DrawEditorInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Visibility", LabelStyle);
            EditorGUI.BeginChangeCheck();
            _runtimePrefabSpawner.ShowRuntimePrefabs = EditorGUILayout.Toggle("Show run-time prefabs", _runtimePrefabSpawner.ShowRuntimePrefabs);
            EditorGUILayout.HelpBox("This will show the run-time spawned prefabs in the editor hierarchy.", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
            {
                _runtimePrefabSpawner.SetRuntimePrefabVisibility(_runtimePrefabSpawner.ShowRuntimePrefabs);
                SceneView.RepaintAll();
                SetSceneDirty();
                EditorApplication.RepaintHierarchyWindow();
            }
			
            GUILayout.EndVertical();	
        }

        private void DrawSettingsInspector()
        {

            VegetationSystemPro vegetationSystemPro = _runtimePrefabSpawner.VegetationSystemPro;                
            
            if (vegetationSystemPro.VegetationPackageProList.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "There is no vegetation package/biome in the vegetation system",
                    MessageType.Warning);
                return;
            }

            
                
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
            _runtimePrefabSpawner.VegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package",
                _runtimePrefabSpawner.VegetationPackageIndex, packageNameList);
            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
            }
            
            GUILayout.EndVertical();    
            
            VegetationPackagePro vegetationPackagePro =
                vegetationSystemPro.VegetationPackageProList[_runtimePrefabSpawner.VegetationPackageIndex];

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
             EditorGUILayout.LabelField("Selected item", vegetationItemInfoPro.Name);
          
            
            if (GUILayout.Button("Add run-time prefab rule"))
            {
                RuntimePrefabRule newRuntimePrefabRule = new RuntimePrefabRule ();
                newRuntimePrefabRule.SetSeed();
                
                vegetationItemInfoPro.RuntimePrefabRuleList.Add(newRuntimePrefabRule);
                _runtimePrefabSpawner.RefreshRuntimePrefabs();
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }               
            GUILayout.EndVertical();
            
             for (int i = 0; i <= vegetationItemInfoPro.RuntimePrefabRuleList.Count - 1; i++)
            {
                RuntimePrefabRule runtimePrefabRule = vegetationItemInfoPro.RuntimePrefabRuleList[i];
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
               
                runtimePrefabRule.RuntimePrefab = EditorGUILayout.ObjectField("Runtime prefab", runtimePrefabRule.RuntimePrefab, typeof(GameObject), true) as GameObject;

                var prefabTexture = AssetPreview.GetAssetPreview(runtimePrefabRule.RuntimePrefab);
                Texture2D convertedPrefabTexture = new Texture2D(2, 2, TextureFormat.ARGB32, true, true);
                if (Application.isPlaying)
                {
                    convertedPrefabTexture = prefabTexture;
                }
                else
                {
                    if (prefabTexture)
                    {
                        convertedPrefabTexture.LoadImage(prefabTexture.EncodeToPNG());
                    }
                }

                if (convertedPrefabTexture)
                {
                    Rect space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(convertedPrefabTexture.height));
                    float width = space.width;

                    space.xMin = (width - convertedPrefabTexture.width);
                    if (space.xMin < 0) space.xMin = 0;

                    space.width = convertedPrefabTexture.width;
                    space.height = convertedPrefabTexture.height;
                    EditorGUI.DrawPreviewTexture(space, convertedPrefabTexture);
                }
                runtimePrefabRule.SpawnFrequency = EditorGUILayout.Slider("Spawn frequency", runtimePrefabRule.SpawnFrequency, 0, 1f);
                runtimePrefabRule.PrefabScale = EditorGUILayout.Vector3Field("Scale", runtimePrefabRule.PrefabScale);
                runtimePrefabRule.UseVegetationItemScale = EditorGUILayout.Toggle("Add vegetation item scale", runtimePrefabRule.UseVegetationItemScale);
                runtimePrefabRule.PrefabRotation = EditorGUILayout.Vector3Field("Rotation", runtimePrefabRule.PrefabRotation);
                runtimePrefabRule.PrefabOffset = EditorGUILayout.Vector3Field("Offset", runtimePrefabRule.PrefabOffset);
                runtimePrefabRule.PrefabLayer = EditorGUILayout.LayerField("Prefab layer", runtimePrefabRule.PrefabLayer);
                runtimePrefabRule.Seed = EditorGUILayout.IntSlider("Seed", runtimePrefabRule.Seed,0,99);                
                runtimePrefabRule.UsePool = EditorGUILayout.Toggle("Use pooling system", runtimePrefabRule.UsePool);
                
                runtimePrefabRule.DistanceFactor = EditorGUILayout.Slider("Distance factor", runtimePrefabRule.DistanceFactor, 0, 1f);
                float currentDistance = vegetationSystemPro.VegetationSettings.GetVegetationDistance() * runtimePrefabRule.DistanceFactor;                
                EditorGUILayout.LabelField("Current distance: " + currentDistance.ToString("F2") + " meters", LabelStyle);

                EditorGUILayout.HelpBox(
                    "The distance from the camera where prefabs are instantiated. Distance is a factor of the current vegetation item draw distance.",
                    MessageType.Info);
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                    _runtimePrefabSpawner.RefreshRuntimePrefabs();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Remove run-time prefab rule"))
                {
                    vegetationItemInfoPro.RuntimePrefabRuleList.Remove(runtimePrefabRule);
                    _runtimePrefabSpawner.RefreshRuntimePrefabs();
                    GUILayout.EndVertical();
                    return;
                }

                GUILayout.EndVertical();
            }                        
        }

        private void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorSceneManager.MarkSceneDirty(_runtimePrefabSpawner.gameObject.scene);
            EditorUtility.SetDirty(_runtimePrefabSpawner);
        }
    }
}