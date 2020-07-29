using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeTechnologies.Extensions;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Culling;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    [CustomEditor(typeof(PersistentVegetationStorage))]
    public class PersistentVegetationStorageEditor : VegetationStudioProBaseEditor
    {
        private PersistentVegetationStorage _persistentVegetationStorage;
        private static readonly List<int> LayerNumbers = new List<int>();
        private int _changedCellIndex = -1;

        private VegetationBrush _vegetationBrush;

        private static Texture[] _brushTextures;

        private SceneMeshRaycaster _sceneMeshRaycaster;

        private bool _painting;


        private static readonly string[] TabNames =
        {
            "Settings", "Stored Vegetation", "Bake Vegetation", "Edit Vegetation", "Paint Vegetation",
            "Precision Painting"//, "Import"
        };

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            _persistentVegetationStorage = (PersistentVegetationStorage) target;
            LoadBrushIcons();
            LoadImporters();
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
            DisableBrush();
        }
        
        void SetSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(_persistentVegetationStorage.gameObject.scene);
                EditorUtility.SetDirty(_persistentVegetationStorage);
            }
        }

        public override void OnInspectorGUI()
        {
            HelpTopic = "persistent-vegetation-storage";           
            OverrideLogoTextureName = "Banner_PersistentVegetationStorage";
            LargeLogo = false;

            _persistentVegetationStorage = (PersistentVegetationStorage) target;
            ShowLogo = true;

            base.OnInspectorGUI();

            if (!_persistentVegetationStorage.VegetationSystemPro)
            {
                EditorGUILayout.HelpBox(
                    "The PersistentVegetationStorage Component needs to be added to a GameObject with a VegetationSystemPro Component.",
                    MessageType.Error);
                return;
            }

            if (!_persistentVegetationStorage.VegetationSystemPro.InitDone)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox(
                    "Vegetation system component has configuration errors. Fix to enable component.",
                    MessageType.Error);
                GUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();
            _persistentVegetationStorage.CurrentTabIndex =
                GUILayout.SelectionGrid(_persistentVegetationStorage.CurrentTabIndex, TabNames, 3,
                    EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            switch (_persistentVegetationStorage.CurrentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawStoredVegetationInspector();
                    break;
                case 2:
                    DrawBakeVegetationInspector();
                    break;
                case 3:
                    DrawEditVegetationInspector();
                    break;
                case 4:
                    DrawPaintVegetationInspector();
                    break;
                case 5:
                    DrawPrecisionPaintingInspector();
                    break;
                case 6:
                    DrawImportInspector();
                    break;
            }

            if (_persistentVegetationStorage.CurrentTabIndex != 4) DisableBrush();
        }

        void DrawPrecisionPaintingInspector()
        {
            if (!IsPersistentoragePackagePresent()) return;

            SelectVegetationPackage();
            
            if (_persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count == 0) return;

            if (_sceneMeshRaycaster == null)
            {
                _sceneMeshRaycaster = new SceneMeshRaycaster();
            }

            EditorGUILayout.HelpBox(
                "Precision Painting will allow you to fine place vegetation. Position is based on a screen ray and will even allow you to place vegetation upside down if the rotation settings is set to follow terrain.",
                MessageType.Info);

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select Vegetation Item", LabelStyle);

            VegetationPackagePro vegetationPackagePro =
                _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[
                    _persistentVegetationStorage.SelectedVegetationPackageIndex];

            VegetationPackageEditorTools.DrawVegetationItemSelector(
                vegetationPackagePro,
                VegetationPackageEditorTools.CreateVegetationInfoIdList(
                    vegetationPackagePro,
                    new[] {VegetationType.Grass, VegetationType.Plant}), 60,
                ref _persistentVegetationStorage.SelectedPrecisionPaintingVegetationID);

            GUILayout.EndVertical();

            SelectGroundLayers();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", LabelStyle);
            _persistentVegetationStorage.PrecisionPaintingMode =
                (PrecisionPaintingMode) EditorGUILayout.EnumPopup("Painting mode",
                    _persistentVegetationStorage.PrecisionPaintingMode);
            if (_persistentVegetationStorage.PrecisionPaintingMode == PrecisionPaintingMode.TerrainAndMeshes)
            {
                EditorGUILayout.HelpBox("This will raycast any enabled meshes in the scene for position.",
                    MessageType.Info);
            }

            _persistentVegetationStorage.UseSteepnessRules = EditorGUILayout.Toggle("Use steepness/angle rules",
                _persistentVegetationStorage.UseSteepnessRules);
            //_persistentVegetationStorage.UseScaleRules = EditorGUILayout.Toggle("Use scale rules",
            //    _persistentVegetationStorage.UseScaleRules);
            _persistentVegetationStorage.SampleDistance = EditorGUILayout.Slider("Sample distance",
                _persistentVegetationStorage.SampleDistance, 0.25f, 5f);
            GUILayout.EndVertical();
        }

        bool IsPersistentoragePackagePresent()
        {
            if (!_persistentVegetationStorage.PersistentVegetationStoragePackage)
            {
                EditorGUILayout.HelpBox("You need to add a persistent vegetation package to the component.",
                    MessageType.Error);
                return false;
            }

            if (_persistentVegetationStorage.PersistentVegetationStoragePackage.PersistentVegetationCellList
                    .Count != _persistentVegetationStorage.VegetationSystemPro.VegetationCellList.Count)
            {
                EditorGUILayout.HelpBox("The vegetation storage is not initialized or initialized for another world or cell size.",
                    MessageType.Error);
                return false;
            }
            
            return true;            
        }

        private void LoadImporters()
        {
            if (_persistentVegetationStorage.VegetationImporterList.Count != 0) return;

            var interfaceType = typeof(IVegetationImporter);
            var importerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetLoadableTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance);

            foreach (var importer in importerTypes)
            {
                IVegetationImporter importerInterface = importer as IVegetationImporter;
                if (importerInterface != null)
                {
                    _persistentVegetationStorage.VegetationImporterList.Add(importerInterface);
                }
            }
        }

        private static void LoadBrushIcons()
        {
            _brushTextures = new Texture[20];

            for (int i = 0; i <= _brushTextures.Length - 1; i++)
            {
                _brushTextures[i] = (Texture2D) Resources.Load("Brushes/Brush_" + i, typeof(Texture2D));
            }
        }

        void SelectVegetationPackage()
        {
            if (_persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count == 0)
            {
                EditorGUILayout.HelpBox("The vegetation system does not have any biomes/vegetation packages",
                    MessageType.Warning);
                return;
            }            
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select biome/vegetation package", LabelStyle);
            string[] packageNameList =
                new string[_persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count];
            for (int i = 0;
                i <= _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count - 1;
                i++)
            {
                if (_persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[i])
                {
                    packageNameList[i] = (i + 1).ToString() + " " + _persistentVegetationStorage.VegetationSystemPro
                                             .VegetationPackageProList[i].PackageName + " (" + _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[i].BiomeType.ToString() + ")";;
                }
                else
                {
                    packageNameList[i] = "Not found";
                }
            }

            EditorGUI.BeginChangeCheck();
            _persistentVegetationStorage.SelectedVegetationPackageIndex = EditorGUILayout.Popup(
                "Selected vegetation package", _persistentVegetationStorage.SelectedVegetationPackageIndex,
                packageNameList);
            if (EditorGUI.EndChangeCheck())
            {
            }

            GUILayout.EndVertical();
        }

        
        public void CreatePersistentVegetationStorage()
        {
            PersistentVegetationStoragePackage newPackage = CreateInstance<PersistentVegetationStoragePackage>();

            if (!AssetDatabase.IsValidFolder("Assets/PersistentVegetationStorageData"))
            {
                AssetDatabase.CreateFolder("Assets", "PersistentVegetationStorageData");
            }

            string filename = "PersistentVegetationStorage_" + Guid.NewGuid() + ".asset";
            AssetDatabase.CreateAsset(newPackage, "Assets/PersistentVegetationStorageData/" + filename);

            PersistentVegetationStoragePackage loadedPackage = AssetDatabase.LoadAssetAtPath<PersistentVegetationStoragePackage>("Assets/PersistentVegetationStorageData/" + filename);
            _persistentVegetationStorage.PersistentVegetationStoragePackage = loadedPackage;
            _persistentVegetationStorage.InitializePersistentStorage();
        }
        
        private void DrawStoredVegetationInspector()
        {
            if (!IsPersistentoragePackagePresent()) return;

            List<PersistentVegetationInstanceInfo> instanceList = _persistentVegetationStorage
                .PersistentVegetationStoragePackage.GetPersistentVegetationInstanceInfoList();

            if (instanceList.Count == 0)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox(
                    "There is no Vegetation Items stored in this storage. To add bake vegetation from the rules, paint with the tool or import from 3rd party systems.",
                    MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            int totalCount = 0;
            for (int i = 0; i <= instanceList.Count - 1; i++)
            {
                totalCount += instanceList[i].Count;
            }

            long fileSize = AssetUtility.GetAssetSize(_persistentVegetationStorage.PersistentVegetationStoragePackage);

            float storageSize = (float) fileSize / (1024 * 1024);
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Storage size: " + storageSize.ToString("F2") + " mbyte", LabelStyle);
            EditorGUILayout.LabelField("Total item count: " + totalCount.ToString("N0"), LabelStyle);

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Status", LabelStyle);
            EditorGUI.BeginChangeCheck();
            _persistentVegetationStorage.DisablePersistentStorage = EditorGUILayout.Toggle("Disable persistent storage",
                _persistentVegetationStorage.DisablePersistentStorage);
            if (EditorGUI.EndChangeCheck())
            {
                _persistentVegetationStorage.VegetationSystemPro.RefreshVegetationSystem();
                EditorUtility.SetDirty(_persistentVegetationStorage);
            }

            GUILayout.EndVertical();

            SelectVegetationPackage();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select Vegetation Item", LabelStyle);

            VegetationPackagePro vegetationPackagePro =
                _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[
                    _persistentVegetationStorage.SelectedVegetationPackageIndex];

            List<string> vegetationItemIdList = new List<string>();
            for (int i = 0; i <= instanceList.Count - 1; i++)
            {
                VegetationItemInfoPro vegetationItemInfoPro =
                    vegetationPackagePro.GetVegetationInfo(instanceList[i].VegetationItemID);
                if (vegetationItemInfoPro != null)
                {
                    vegetationItemIdList.Add(instanceList[i].VegetationItemID);
                }
            }

            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro, vegetationItemIdList, 60,
                ref _persistentVegetationStorage.SelectedStorageVegetationID);
            GUILayout.EndVertical();

            VegetationItemInfoPro vegetationItemInfo =
                _persistentVegetationStorage.VegetationSystemPro.GetVegetationItemInfo(_persistentVegetationStorage
                    .SelectedStorageVegetationID);
            GUILayout.BeginVertical("box");

            if (vegetationItemInfo != null)
            {
                EditorGUILayout.LabelField("Information : " + vegetationItemInfo.Name, LabelStyle);
            }

            if (vegetationItemInfo != null)
            {
                EditorGUI.BeginChangeCheck();
                vegetationItemInfo.UseVegetationMasksOnStorage = EditorGUILayout.Toggle("Use vegetation masks",
                    vegetationItemInfo.UseVegetationMasksOnStorage);
                if (EditorGUI.EndChangeCheck())
                {
                    _persistentVegetationStorage.VegetationSystemPro.ClearCache(vegetationItemInfo.VegetationItemID);
                    EditorUtility.SetDirty(vegetationPackagePro);
                }
            }

            int instanceCount = 0;
            PersistentVegetationInstanceInfo selectedPersistentVegetationInstanceInfo = null;
            for (int i = 0; i <= instanceList.Count - 1; i++)
            {
                if (instanceList[i].VegetationItemID == _persistentVegetationStorage.SelectedStorageVegetationID)
                {
                    selectedPersistentVegetationInstanceInfo = instanceList[i];
                    instanceCount = instanceList[i].Count;
                }
            }

            EditorGUILayout.LabelField("Instance count: " + instanceCount.ToString("N0"), LabelStyle);

            if (selectedPersistentVegetationInstanceInfo != null)
            {
                for (int i = 0; i <= selectedPersistentVegetationInstanceInfo.SourceCountList.Count - 1; i++)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        PersistentVegetationStorageTools.GetSourceName(selectedPersistentVegetationInstanceInfo
                            .SourceCountList[i].VegetationSourceID) + " : " + selectedPersistentVegetationInstanceInfo
                            .SourceCountList[i].Count.ToString("N0"), LabelStyle);
                    if (GUILayout.Button("Clear instances", GUILayout.Width(120)))
                    {
                        _persistentVegetationStorage.RemoveVegetationItemInstances(
                            _persistentVegetationStorage.SelectedStorageVegetationID,
                            selectedPersistentVegetationInstanceInfo.SourceCountList[i].VegetationSourceID);
                        _persistentVegetationStorage.VegetationSystemPro.ClearCache(_persistentVegetationStorage
                            .SelectedStorageVegetationID);
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                    }

                    GUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Clear selected Vegetation Item from storage"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear vegetation item",
                    "Are you sure you want to clear the item from storage", "Clear", "Clear/enable run-time spawn",
                    "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        _persistentVegetationStorage.RemoveVegetationItemInstances(_persistentVegetationStorage
                            .SelectedStorageVegetationID);
                        _persistentVegetationStorage.VegetationSystemPro.ClearCache(_persistentVegetationStorage
                            .SelectedStorageVegetationID);
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        break;                    
                    case 1:
                        _persistentVegetationStorage.RemoveVegetationItemInstances(_persistentVegetationStorage
                            .SelectedStorageVegetationID);
                        
                        VegetationItemInfoPro tempVegetationItemInfo =
                            _persistentVegetationStorage.VegetationSystemPro.GetVegetationItemInfo(_persistentVegetationStorage
                                .SelectedStorageVegetationID);
                        
                        if (tempVegetationItemInfo != null) tempVegetationItemInfo.EnableRuntimeSpawn = true;
                        
                        _persistentVegetationStorage.VegetationSystemPro.ClearCache(_persistentVegetationStorage
                            .SelectedStorageVegetationID);
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        _persistentVegetationStorage.VegetationSystemPro.SetAllVegetationPackagesDirty();
                        break;
                }
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("This will clear all Vegetation Items from the selected biome from storage.",
                MessageType.Info);
            if (GUILayout.Button("Clear ALL items from the selected biome from storage"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear storage",
                    "Are you sure you want to clear the entire vegetation package from the storage?", "Clear", "Clear/enable run-time spawn",
                    "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        for (int i = 0; i <= vegetationItemIdList.Count - 1; i++)
                        {
                            _persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i]);
                        }

                        _persistentVegetationStorage.VegetationSystemPro.ClearCache();
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        break;
                    case 1:
                        for (int i = 0; i <= vegetationItemIdList.Count - 1; i++)
                        {
                            _persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i]);
                            VegetationItemInfoPro tempVegetationItemInfo =
                                _persistentVegetationStorage.VegetationSystemPro.GetVegetationItemInfo(
                                    vegetationItemIdList[i]);

                            if (tempVegetationItemInfo != null) tempVegetationItemInfo.EnableRuntimeSpawn = true;
                        }

                        _persistentVegetationStorage.VegetationSystemPro.ClearCache();
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        _persistentVegetationStorage.VegetationSystemPro.SetAllVegetationPackagesDirty();
                        break;
                }
            }
            
            if (GUILayout.Button("Clear ALL BAKED items from the selected biome from storage"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear storage",
                    "Are you sure you want to clear baked items from the entire vegetation package from the storage?", "Clear", "Clear/enable run-time spawn",
                    "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        for (int i = 0; i <= vegetationItemIdList.Count - 1; i++)
                        {
                            _persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i],0);
                        }

                        _persistentVegetationStorage.VegetationSystemPro.ClearCache();
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        break;
                    case 1:
                        for (int i = 0; i <= vegetationItemIdList.Count - 1; i++)
                        {
                            _persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i],0);
                            VegetationItemInfoPro tempVegetationItemInfo =
                                _persistentVegetationStorage.VegetationSystemPro.GetVegetationItemInfo(
                                    vegetationItemIdList[i]);

                            if (tempVegetationItemInfo != null) tempVegetationItemInfo.EnableRuntimeSpawn = true;
                        }

                        _persistentVegetationStorage.VegetationSystemPro.ClearCache();
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        _persistentVegetationStorage.VegetationSystemPro.SetAllVegetationPackagesDirty();
                        break;
                }
            }

            GUILayout.EndVertical();
            
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Clear ALL items from all VegetationPackages/biomes"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear storage",
                    "Are you sure you want to clear the entire storage", "Clear", "Clear/enable run-time spawn",
                    "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        ClearAllItemsFromAllVegetationPackages(false, false);
                        break;
                    case 1:
                        ClearAllItemsFromAllVegetationPackages(false, true);
                        break;
                }
            }
            
            if (GUILayout.Button("Clear ALL BAKED items from all VegetationPackages/biomes"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear storage",
                    "Are you sure you want to clear baked items the entire storage", "Clear", "Clear/enable run-time spawn",
                    "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        ClearAllItemsFromAllVegetationPackages(true, false);
                        break;
                    case 1:
                        ClearAllItemsFromAllVegetationPackages(true, true);
                        break;
                }
            }
            GUILayout.EndVertical();
        }

        void ClearAllItemsFromAllVegetationPackages(bool bakedOnly, bool enableRuntimeSpawn)
        {
            for (int i = 0; i <= _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                VegetationPackagePro vegetationPackagePro =
                    _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[i];

                for (int j = 0; j <= vegetationPackagePro.VegetationInfoList.Count - 1; j++)
                {                    
                    VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[j];

                    if (bakedOnly)
                    {
                        _persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemInfoPro.VegetationItemID, 0);
                    }
                    else
                    {
                        _persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemInfoPro.VegetationItemID);
                    }

                    if (enableRuntimeSpawn)
                    {
                        vegetationItemInfoPro.EnableRuntimeSpawn = true;
                    }                    
                }
                EditorUtility.SetDirty(vegetationPackagePro);
            }

            EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
            _persistentVegetationStorage.VegetationSystemPro.ClearCache();
        }
        
        private void DisableBrush()
        {
            if (_vegetationBrush != null)
            {
                _vegetationBrush.Dispose();
                _vegetationBrush = null;
            }
        }

        private void DrawPaintVegetationInspector()
        {
            if (!IsPersistentoragePackagePresent()) return;

            SelectVegetationPackage();

            if (_persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count == 0) return;
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select Vegetation Item", LabelStyle);

            VegetationPackagePro vegetationPackagePro =
                _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[
                    _persistentVegetationStorage.SelectedVegetationPackageIndex];

            VegetationPackageEditorTools.DrawVegetationItemSelector(
                vegetationPackagePro,
                VegetationPackageEditorTools.CreateVegetationInfoIdList(
                    vegetationPackagePro,
                    new[]
                    {
                        VegetationType.Grass, VegetationType.Plant, VegetationType.Tree, VegetationType.Objects,
                        VegetationType.LargeObjects
                    }), 60,
                ref _persistentVegetationStorage.SelectedPaintVegetationID);

            GUILayout.EndVertical();

            SelectGroundLayers();

            // ReSharper disable once NotAccessedVariable
            bool flag;
            _persistentVegetationStorage.SelectedBrushIndex = AspectSelectionGrid(
                _persistentVegetationStorage.SelectedBrushIndex, _brushTextures, 32, "No brushes defined.", out flag);
            EditorGUILayout.LabelField("Delete Vegetation: Ctrl-Click", LabelStyle);
            EditorGUILayout.HelpBox("Delete Vegetation will only remove vegetation of the selected type.",
                MessageType.Info);

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", LabelStyle);
            _persistentVegetationStorage.IgnoreHeight = EditorGUILayout.Toggle("Ignore height",
                _persistentVegetationStorage.IgnoreHeight);
            EditorGUILayout.HelpBox("Ignore height will remove vegetation at any height when deleting with the brush.",
                MessageType.Info);
            _persistentVegetationStorage.RandomizePosition = EditorGUILayout.Toggle("Randomize Position",
                _persistentVegetationStorage.RandomizePosition);
            _persistentVegetationStorage.PaintOnColliders = EditorGUILayout.Toggle("Paint on colliders",
                _persistentVegetationStorage.PaintOnColliders);
            _persistentVegetationStorage.UseSteepnessRules = EditorGUILayout.Toggle("Use steepness/angle rules",
                _persistentVegetationStorage.UseSteepnessRules);
            //_persistentVegetationStorage.UseScaleRules = EditorGUILayout.Toggle("Use scale rules",
            //    _persistentVegetationStorage.UseScaleRules);

            _persistentVegetationStorage.SampleDistance = EditorGUILayout.Slider("Sample distance",
                _persistentVegetationStorage.SampleDistance, 0.25f, 25f);
            _persistentVegetationStorage.BrushSize =
                EditorGUILayout.Slider("Brush Size", _persistentVegetationStorage.BrushSize, 0.25f, 30);

            EditorGUILayout.HelpBox("Vegetation items will follow the rotation mode set in the VegetationPackage",
                MessageType.Info);

            GUILayout.EndVertical();
        }

        private void DrawEditVegetationInspector()
        {
            if (!IsPersistentoragePackagePresent()) return;

            SelectVegetationPackage();

            if (_persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count == 0) return;
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select Vegetation Item", LabelStyle);

            VegetationPackagePro vegetationPackagePro =
                _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[
                    _persistentVegetationStorage.SelectedVegetationPackageIndex];

            EditorGUI.BeginChangeCheck();

            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro,
                VegetationPackageEditorTools.CreateVegetationInfoIdList(
                    vegetationPackagePro,
                    new[] {VegetationType.Objects, VegetationType.Tree, VegetationType.LargeObjects}), 60,
                ref _persistentVegetationStorage.SelectedEditVegetationID);

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            GUILayout.EndVertical();

            SelectGroundLayers();

            EditorGUILayout.LabelField("Insert Vegetation Item: Ctrl-Click", LabelStyle);
            EditorGUILayout.LabelField("Delete Vegetation Item: Ctrl-Shift-Click", LabelStyle);

            EditorGUILayout.HelpBox(
                "Select the Vegetation item to edit. Move/scale and rotate handles will show up in the sceneview. ",
                MessageType.Info);
        }

        private void DrawImportInspector()
        {
            if (!IsPersistentoragePackagePresent()) return;

            SelectVegetationPackage();

            VegetationPackagePro vegetationPackagePro =
                _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[
                    _persistentVegetationStorage.SelectedVegetationPackageIndex];

            for (int i = 0; i <= _persistentVegetationStorage.VegetationImporterList.Count - 1; i++)
            {
                _persistentVegetationStorage.VegetationImporterList[i].PersistentVegetationStoragePackage =
                    _persistentVegetationStorage.PersistentVegetationStoragePackage;
                _persistentVegetationStorage.VegetationImporterList[i].VegetationPackagePro =
                    vegetationPackagePro;
                _persistentVegetationStorage.VegetationImporterList[i].PersistentVegetationStorage =
                    _persistentVegetationStorage;
            }

            string[] importerNames = GetImporterNameArray();
            GUILayout.BeginVertical("box");
            _persistentVegetationStorage.SelectedImporterIndex =
                EditorGUILayout.Popup(_persistentVegetationStorage.SelectedImporterIndex, importerNames);
            GUILayout.EndVertical();

            if (_persistentVegetationStorage.VegetationImporterList.Count == 0) return;

            GUILayout.BeginVertical("box");
            IVegetationImporter importer =
                _persistentVegetationStorage.VegetationImporterList[_persistentVegetationStorage.SelectedImporterIndex];

            EditorGUILayout.LabelField(importer.ImporterName, LabelStyle);
            GUILayout.EndVertical();

            importer.OnGUI();
        }

        private string[] GetImporterNameArray()
        {
            string[] resultArray = new string[_persistentVegetationStorage.VegetationImporterList.Count];
            for (int i = 0; i <= _persistentVegetationStorage.VegetationImporterList.Count - 1; i++)
            {
                resultArray[i] = _persistentVegetationStorage.VegetationImporterList[i].ImporterName;
            }

            return resultArray;
        }

        private void DrawBakeVegetationInspector()
        {
            if (!IsPersistentoragePackagePresent()) return;

            SelectVegetationPackage();

            if (_persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count == 0) return;
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select Vegetation Item", LabelStyle);

            VegetationPackagePro vegetationPackagePro =
                _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[
                    _persistentVegetationStorage.SelectedVegetationPackageIndex];

            List<string> vegetationItemIdList =
                VegetationPackageEditorTools.CreateVegetationInfoIdList(vegetationPackagePro);
            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro, vegetationItemIdList, 60,
                ref _persistentVegetationStorage.SelectedBakeVegetationID);

            GUILayout.EndVertical();

            if (vegetationItemIdList.Count == 0)
            {
                EditorGUILayout.HelpBox("There is no Vegetation Items configured in the Vegetation Package.",
                    MessageType.Info);
                return;
            }

            GUILayout.BeginVertical("box");

            if (GUILayout.Button("Bake Vegetation Item from ruleset"))
            {
                _persistentVegetationStorage.BakeVegetationItem(_persistentVegetationStorage.SelectedBakeVegetationID);
                _persistentVegetationStorage.VegetationSystemPro.ClearCache();
                EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
            }

            EditorGUILayout.HelpBox(
                "Bake vegetation item will calculate all instances of the vegetation item in the terrain and store this in the persistent storage. This will also disable 'Enable run-time spawn' on the vegetation item.",
                MessageType.Info);

            if (GUILayout.Button("Bake ALL Vegetation Items from ruleset"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Bake vegetation",
                    "this will bake all Vegetation Items from the selected VegetationPackage/Biome to the persistent storage. 'Enable run-time spawn' will be set to false after bake. Clear and bake will remove baked vegetation in the storage.",
                    "Bake", "Clear and bake", "Cancel");

                switch (buttonResult)
                {
                    case 0:

                        for (int i = 0; i <= vegetationItemIdList.Count - 1; i++)
                        {
                            _persistentVegetationStorage.BakeVegetationItem(vegetationItemIdList[i]);
                        }

                        _persistentVegetationStorage.VegetationSystemPro.ClearCache();
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        break;
                    case 1:
                        for (int i = 0; i <= vegetationItemIdList.Count - 1; i++)
                        {
                            _persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i], 0);
                            _persistentVegetationStorage.BakeVegetationItem(vegetationItemIdList[i]);
                        }

                        _persistentVegetationStorage.VegetationSystemPro.ClearCache();
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        //EditorUtility.SetDirty(_persistentVegetationStorage.VegetationSystem.CurrentVegetationPackage);
                        break;
                }
            }

            GUILayout.EndVertical();
            
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Bake ALL Vegetation Items from all VegetationPackages/Biomes"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Bake vegetation",
                    "this will bake all Vegetation Items from the selected VegetationPackage/Biome to the persistent storage. 'Enable run-time spawn' will be set to false after bake. Clear and bake will remove baked vegetation in the storage.",
                    "Bake", "Clear and bake", "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        BakeAllVegetationItemsFromAllBiomes(false);
                        break;
                    case 1:
                        BakeAllVegetationItemsFromAllBiomes(true);
                        break;
                }
            }
            GUILayout.EndVertical();
        }

        void BakeAllVegetationItemsFromAllBiomes(bool clearOld)
        {
            for (int i = 0; i <= _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                VegetationPackagePro vegetationPackagePro =
                    _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[i];

                for (int j = 0; j <= vegetationPackagePro.VegetationInfoList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[j];
                    if (clearOld)
                    {
                        _persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemInfoPro.VegetationItemID, 0);
                    }
                    _persistentVegetationStorage.BakeVegetationItem(vegetationItemInfoPro.VegetationItemID);                   
                }
                EditorUtility.SetDirty(vegetationPackagePro);
            }
            EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
            _persistentVegetationStorage.VegetationSystemPro.ClearCache();
        }
        
        private void DrawSettingsInspector()
        {
            EditorGUILayout.HelpBox(
                "The PersistentVegetationStorage Component is designed to store baked vegetation generated from the rules in the VegetationSystem Component or from 3rd party systems. The Vegetation Item locations are stored in a scriptable object.",
                MessageType.Info);

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation storage", LabelStyle);
            EditorGUI.BeginChangeCheck();


            GUILayout.BeginHorizontal();
            _persistentVegetationStorage.PersistentVegetationStoragePackage = EditorGUILayout.ObjectField("Storage",
                _persistentVegetationStorage.PersistentVegetationStoragePackage,
                typeof(PersistentVegetationStoragePackage), true) as PersistentVegetationStoragePackage;
            
            if (GUILayout.Button("Create Storage"))
            {
                CreatePersistentVegetationStorage();
            }

            GUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                if (_persistentVegetationStorage.PersistentVegetationStoragePackage != null &&
                    !_persistentVegetationStorage.PersistentVegetationStoragePackage.Initialized)
                {
                    if (EditorUtility.DisplayDialog("Initialize persistent storage",
                        "Do you want to initialize the storage for the current VegetationSystem?", "OK", "Cancel"))
                    {
                        _persistentVegetationStorage.InitializePersistentStorage();
                    }
                }

                EditorUtility.SetDirty(target);
                if (_persistentVegetationStorage.PersistentVegetationStoragePackage)
                {
                    EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);  
                }
                _persistentVegetationStorage.VegetationSystemPro.RefreshVegetationSystem();
            }

            EditorGUILayout.HelpBox(
                "Create a new PersistentVegetationStoragePackage object by right clicking in a project folder and select Create/AwesomeTechnologies/Persistent Vegetation Storage Package. Then drag and drop this here.",
                MessageType.Info);
            GUILayout.EndVertical();

            if (_persistentVegetationStorage.PersistentVegetationStoragePackage == null) return;

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Setup", LabelStyle);
            if (GUILayout.Button("Initialize persistent storrage"))
            {
                if (EditorUtility.DisplayDialog("Initialize persistent storage",
                    "Are you sure you want to initialize the storage for the current VegetationSystem? This will remove any existing vegetation in the storage.",
                    "OK", "Cancel"))
                {
                    _persistentVegetationStorage.InitializePersistentStorage();
                    EditorUtility.SetDirty(target);
                    EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                    _persistentVegetationStorage.VegetationSystemPro.RefreshVegetationSystem();
                }
            }

            EditorGUILayout.HelpBox(
                "Initialize persistent storrage will clear the current storrage and configure it to store vegetation items for the current configuration of the VegetationSystem component",
                MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Status", LabelStyle);
            EditorGUILayout.LabelField("Cell count: " + _persistentVegetationStorage.GetPersistentVegetationCellCount(),
                LabelStyle);
            GUILayout.EndVertical();
        }

        private static int AspectSelectionGrid(int selected, Texture[] textures, int approxSize, string emptyString,
            out bool doubleClick)
        {
            GUILayout.BeginVertical("box", GUILayout.MinHeight(10f));
            int result = 0;
            doubleClick = false;
            if (textures.Length != 0)
            {
                float num = (EditorGUIUtility.currentViewWidth - 20f) / approxSize;
                int num2 = (int) Mathf.Ceil(textures.Length / num);
                Rect aspectRect = GUILayoutUtility.GetAspectRect(num / num2);
                Event current = Event.current;
                if (current.type == EventType.MouseDown && current.clickCount == 2 &&
                    aspectRect.Contains(current.mousePosition))
                {
                    doubleClick = true;
                    current.Use();
                }

                result = GUI.SelectionGrid(aspectRect, Math.Min(selected, textures.Length - 1), textures,
                    Mathf.RoundToInt(EditorGUIUtility.currentViewWidth - 20f) / approxSize, "GridList");
            }
            else
            {
                GUILayout.Label(emptyString);
            }

            GUILayout.EndVertical();
            return result;
        }

        private void OnSceneGuiEditVegetation()
        {
            if (_persistentVegetationStorage.SelectedEditVegetationID == "") return;

            if (Event.current.type == EventType.MouseDown)
            {
                _changedCellIndex = -1;
            }

            VegetationPackagePro vegetationPackagePro =
                _persistentVegetationStorage.VegetationSystemPro.VegetationPackageProList[
                    _persistentVegetationStorage.SelectedVegetationPackageIndex];

            VegetationItemInfoPro vegetationItemInfo =
                vegetationPackagePro.GetVegetationInfo(_persistentVegetationStorage.SelectedEditVegetationID);

            if (Event.current.type == EventType.MouseUp)
            {
                if (_changedCellIndex != -1)
                {
                    _persistentVegetationStorage.RepositionCellItems(_changedCellIndex,
                        _persistentVegetationStorage.SelectedEditVegetationID);
                    EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                }
            }

            List<VegetationCell> closeCellList = new List<VegetationCell>();

            VegetationStudioCamera vegetationStudioCamera =
                _persistentVegetationStorage.VegetationSystemPro.GetSceneViewVegetationStudioCamera();

            if (vegetationStudioCamera == null) return;

            for (int i = 0; i <= vegetationStudioCamera.JobCullingGroup.VisibleCellIndexList.Length - 1; i++)
            {
                int index = vegetationStudioCamera.JobCullingGroup.VisibleCellIndexList[i];

                VegetationCell vegetationCell = vegetationStudioCamera.PotentialVisibleCellList[index];
                BoundingSphereInfo boundingSphereInfo =
                    vegetationStudioCamera.JobCullingGroup.BundingSphereInfoList[index];

                if (boundingSphereInfo.CurrentDistanceBand == 0)
                {
                    closeCellList.Add(vegetationCell);
                }
            }

            Event currentEvent = Event.current;

            if (currentEvent.shift || currentEvent.control)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.control &&
                !currentEvent.shift && !currentEvent.alt)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                var hits = Physics.RaycastAll(ray, 10000f).OrderBy(h => h.distance).ToArray();
                for (int i = 0; i <= hits.Length - 1; i++)
                {
                    if (hits[i].collider is TerrainCollider ||
                        _persistentVegetationStorage.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                    {
                        float scale =
                            UnityEngine.Random.Range(vegetationItemInfo.MinScale, vegetationItemInfo.MaxScale);
                        _persistentVegetationStorage.AddVegetationItemInstance(
                            _persistentVegetationStorage.SelectedEditVegetationID, hits[i].point,
                            new Vector3(scale, scale, scale), Quaternion.Euler(0, UnityEngine.Random.Range(0, 365), 0),
                            true, 1, 1, true);
                        EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                        break;
                    }
                }
            }

            if (currentEvent.shift && currentEvent.control)
            {
                for (int i = 0; i <= closeCellList.Count - 1; i++)
                {
                    PersistentVegetationCell persistentVegetationCell = _persistentVegetationStorage
                        .PersistentVegetationStoragePackage.PersistentVegetationCellList[closeCellList[i].Index];
                    PersistentVegetationInfo persistentVegetationInfo =
                        persistentVegetationCell.GetPersistentVegetationInfo(_persistentVegetationStorage
                            .SelectedEditVegetationID);

                    if (persistentVegetationInfo != null)
                    {
                        for (int j = persistentVegetationInfo.VegetationItemList.Count - 1; j >= 0; j--)
                        {
                            PersistentVegetationItem persistentVegetationItem =
                                persistentVegetationInfo.VegetationItemList[j];

                            Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
                            float distance = Vector3.Distance(cameraPosition,
                                persistentVegetationItem.Position + _persistentVegetationStorage.VegetationSystemPro
                                    .VegetationSystemPosition);

                            Handles.color = Color.red;
                            if (Handles.Button(
                                persistentVegetationItem.Position + _persistentVegetationStorage.VegetationSystemPro
                                    .VegetationSystemPosition,
                                Quaternion.LookRotation(persistentVegetationItem.Position - cameraPosition, Vector3.up),
                                0.025f * distance, 0.025f * distance, Handles.CircleHandleCap))
                            {
                                persistentVegetationInfo.RemovePersistentVegetationItemInstance(
                                    ref persistentVegetationItem);
                                _persistentVegetationStorage.PersistentVegetationStoragePackage.SetInstanceInfoDirty();

                                _persistentVegetationStorage.VegetationSystemPro.ClearCache(closeCellList[i],vegetationItemInfo.VegetationItemID);
                                EditorUtility.SetDirty(_persistentVegetationStorage.PersistentVegetationStoragePackage);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i <= closeCellList.Count - 1; i++)
                {
                    PersistentVegetationCell persistentVegetationCell = _persistentVegetationStorage
                        .PersistentVegetationStoragePackage.PersistentVegetationCellList[closeCellList[i].Index];

                    PersistentVegetationInfo persistentVegetationInfo =
                        persistentVegetationCell.GetPersistentVegetationInfo(_persistentVegetationStorage
                            .SelectedEditVegetationID);

                    if (persistentVegetationInfo != null)
                    {
                        Vector3 cameraPosition = vegetationStudioCamera.SelectedCamera.transform.position;
                        
                        for (int j = persistentVegetationInfo.VegetationItemList.Count - 1; j >= 0; j--)
                        {
                            PersistentVegetationItem persistentVegetationItem =
                                persistentVegetationInfo.VegetationItemList[j];

                            Vector3 worldspacePosition = persistentVegetationItem.Position +
                                                         _persistentVegetationStorage.VegetationSystemPro
                                                             .VegetationSystemPosition;


                           if (Vector3.Distance(cameraPosition, worldspacePosition) > 50f) continue;                            
                            
                            EditorGUI.BeginChangeCheck();

                            if (Tools.current == Tool.Move)
                            {
                                Vector3 newPosition = Handles.PositionHandle(worldspacePosition, Quaternion.identity);

                                float xAxisMovement = Mathf.Abs(worldspacePosition.x - newPosition.x);
                                float zAxisMovement = Mathf.Abs(worldspacePosition.z - newPosition.z);

                                if (EditorGUI.EndChangeCheck())
                                {
                                    if (xAxisMovement < 0.01f && zAxisMovement < 0.01f)
                                    {
                                        persistentVegetationItem.Position =
                                            newPosition - _persistentVegetationStorage.VegetationSystemPro
                                                .VegetationSystemPosition;
                                    }
                                    else
                                    {
                                        Vector3 newTerrainPosition = PositionVegetationItem(newPosition);
                                        persistentVegetationItem.Position =
                                            newTerrainPosition - _persistentVegetationStorage.VegetationSystemPro
                                                .VegetationSystemPosition;
                                    }

                                    persistentVegetationInfo.UpdatePersistentVegetationItemInstanceSourceId(
                                        ref persistentVegetationItem, 1);
                                    _persistentVegetationStorage.PersistentVegetationStoragePackage
                                        .SetInstanceInfoDirty();

                                    _changedCellIndex = closeCellList[i].Index;

                                    persistentVegetationInfo.VegetationItemList[j] = persistentVegetationItem;

                                    _persistentVegetationStorage.VegetationSystemPro.ClearCache(closeCellList[i],vegetationItemInfo.VegetationItemID);

                                    EditorUtility.SetDirty(_persistentVegetationStorage
                                        .PersistentVegetationStoragePackage);
                                }
                            }

                            if (Tools.current == Tool.Rotate)
                            {
                                float size = HandleUtility.GetHandleSize(worldspacePosition) * 1f;
                                const float snap = 0.1f;

                                Handles.color = Color.red;
                                Quaternion newRotation;

                                if (vegetationItemInfo.VegetationType == VegetationType.Tree)
                                {
                                    newRotation = Handles.Disc(persistentVegetationItem.Rotation, worldspacePosition,
                                        Vector3.up, size, true, snap);
                                }
                                else
                                {
                                    newRotation = Handles.RotationHandle(persistentVegetationItem.Rotation,
                                        worldspacePosition);
                                }

                                if (EditorGUI.EndChangeCheck())
                                {
                                    persistentVegetationItem.Rotation = newRotation;

                                    persistentVegetationInfo.UpdatePersistentVegetationItemInstanceSourceId(
                                        ref persistentVegetationItem, 1);
                                    _persistentVegetationStorage.PersistentVegetationStoragePackage
                                        .SetInstanceInfoDirty();

                                    persistentVegetationInfo.VegetationItemList[j] = persistentVegetationItem;
                                    EditorUtility.SetDirty(_persistentVegetationStorage
                                        .PersistentVegetationStoragePackage);

                                    _persistentVegetationStorage.VegetationSystemPro.ClearCache(closeCellList[i],vegetationItemInfo.VegetationItemID);
                                }
                            }

                            if (Tools.current == Tool.Scale)
                            {
                                Handles.color = Color.red;

                                float size = HandleUtility.GetHandleSize(worldspacePosition) * 1f;
                                const float snap = 0.1f;
                                float newScale = Handles.ScaleSlider(persistentVegetationItem.Scale.x,
                                    worldspacePosition, Vector3.right, persistentVegetationItem.Rotation, size, snap);

                                if (EditorGUI.EndChangeCheck())
                                {
                                    persistentVegetationItem.Scale = new Vector3(newScale, newScale, newScale);

                                    persistentVegetationInfo.UpdatePersistentVegetationItemInstanceSourceId(
                                        ref persistentVegetationItem, 1);
                                    _persistentVegetationStorage.PersistentVegetationStoragePackage
                                        .SetInstanceInfoDirty();

                                    persistentVegetationInfo.VegetationItemList[j] = persistentVegetationItem;
                                    EditorUtility.SetDirty(_persistentVegetationStorage
                                        .PersistentVegetationStoragePackage);


                                    _persistentVegetationStorage.VegetationSystemPro.ClearCache(closeCellList[i],vegetationItemInfo.VegetationItemID);
                                }
                            }
                        }
                    }
                }
            }
        }

        private Vector3 PositionVegetationItem(Vector3 position)
        {
            Ray ray = new Ray(position + new Vector3(0, 2000f, 0), Vector3.down);

            var hits = Physics.RaycastAll(ray).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i <= hits.Length - 1; i++)
            {
                if (hits[i].collider is TerrainCollider ||
                    _persistentVegetationStorage.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                {
                    return hits[i].point;
                }
            }

            return position;
        }

        // ReSharper disable once UnusedMember.Local
        void OnSceneGUI()
        {
            if (!_persistentVegetationStorage) return;
            if (_persistentVegetationStorage.VegetationSystemPro == null) return;
            if (!_persistentVegetationStorage.PersistentVegetationStoragePackage) return;


            if (_persistentVegetationStorage.CurrentTabIndex == 3)
            {
                OnSceneGuiEditVegetation();
            }

            if (_persistentVegetationStorage.CurrentTabIndex == 4)
            {
                OnSceneGUIPaintVegetation();
            }

            if (_persistentVegetationStorage.CurrentTabIndex == 5)
            {
                OnSceneGUIPrecisionPainting();
            }
        }

        void OnSceneGUIPrecisionPainting()
        {
            if (_persistentVegetationStorage.SelectedPrecisionPaintingVegetationID == "") return;
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.Repaint)
            {
                PrecisionPaintItem(false);
            }

            if (Event.current.type == EventType.MouseUp)
            {
                HandleUtility.Repaint();
                _painting = false;
            }

            if (Event.current.type == EventType.MouseMove)
            {
                HandleUtility.Repaint();
                PrecisionPaintItem(false);
            }

            if (Event.current.type == EventType.MouseDrag)
            {
                HandleUtility.Repaint();
                if (_painting)
                {
                    PrecisionPaintItem(true);
                }
            }

            if (Event.current.type == EventType.MouseDown)
            {
                HandleUtility.Repaint();
                if (Event.current.button == 0)
                {
                    _painting = true;
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    PrecisionPaintItem(true);
                }
                else
                {
                    GUIUtility.hotControl = 0;
                }
            }
        }

        void PrecisionPaintItem(bool addVegetationItem)
        {
            if (_sceneMeshRaycaster == null) return;

            bool includeMeshes = true;
            bool includeColliders = false;
            switch (_persistentVegetationStorage.PrecisionPaintingMode)
            {
                case PrecisionPaintingMode.Terrain:
                    includeMeshes = false;
                    // ReSharper disable once RedundantAssignment
                    includeColliders = false;
                    break;
                case PrecisionPaintingMode.TerrainAndColliders:
                    includeMeshes = false;
                    includeColliders = true;
                    break;
                case PrecisionPaintingMode.TerrainAndMeshes:
                    // ReSharper disable once RedundantAssignment
                    includeMeshes = true;
                    // ReSharper disable once RedundantAssignment
                    includeColliders = false;
                    break;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit raycastHit;
            if (_sceneMeshRaycaster.RaycastSceneMeshes(ray, out raycastHit, true, includeColliders, includeMeshes))
            {
                float size = HandleUtility.GetHandleSize(raycastHit.point) * 0.1f;

                Gizmos.color = Color.white;
                Handles.SphereHandleCap(0, raycastHit.point, Quaternion.identity, size, EventType.Repaint);

                Gizmos.color = Color.green;
                Vector3 normal = raycastHit.normal.normalized;
                Handles.DrawLine(raycastHit.point, raycastHit.point + normal * 2);

                if (!addVegetationItem) return;
                if (Event.current.control)
                {
                    EraseVegetationItem(raycastHit.point,
                        _persistentVegetationStorage.SelectedPrecisionPaintingVegetationID,
                        _persistentVegetationStorage.SampleDistance);
                }
                else
                {
                    AddVegetationItem(raycastHit.point, normal,
                        _persistentVegetationStorage.SelectedPrecisionPaintingVegetationID,
                        _persistentVegetationStorage.SampleDistance);
                }
            }
        }

        void OnSceneGUIPaintVegetation()
        {
            if (Event.current.alt) return;
            if (_persistentVegetationStorage.SelectedPaintVegetationID == "") return;

            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            bool raycastHit = false;


            if (Event.current.type == EventType.Repaint)
            {
                Vector3 hitPosition = Vector3.zero;
                UpdatePreviewBrush(ref raycastHit, ref hitPosition);
                if (raycastHit)
                {
                    PaintVegetationItems(hitPosition, false);
                }
            }


            if (Event.current.type == EventType.MouseUp)
            {
                HandleUtility.Repaint();
                _painting = false;
            }

            if (Event.current.type == EventType.MouseMove)
            {
                HandleUtility.Repaint();

                Vector3 hitPosition = Vector3.zero;
                UpdatePreviewBrush(ref raycastHit, ref hitPosition);
                if (raycastHit)
                {
                    PaintVegetationItems(hitPosition, false);
                }
            }

            if (Event.current.type == EventType.MouseDrag)
            {
                HandleUtility.Repaint();
                if (_painting)
                {
                    Vector3 hitPosition = Vector3.zero;
                    UpdatePreviewBrush(ref raycastHit, ref hitPosition);
                    if (raycastHit)
                    {
                        PaintVegetationItems(hitPosition, true);
                    }
                }
            }

            if (Event.current.type == EventType.MouseDown)
            {
                HandleUtility.Repaint();
                if (Event.current.button == 0)
                {
                    _painting = true;
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();

                    Vector3 hitPosition = Vector3.zero;
                    UpdatePreviewBrush(ref raycastHit, ref hitPosition);
                    if (raycastHit)
                    {
                        PaintVegetationItems(hitPosition, true);
                    }
                }
                else
                {
                    GUIUtility.hotControl = 0;
                }
            }
        }

        private void PaintVegetationItems(Vector3 hitPosition, bool addVegetationItems)
        {
            Vector3 corner = hitPosition + new Vector3(-_persistentVegetationStorage.BrushSize, 0f,
                                 -_persistentVegetationStorage.BrushSize);
            float currentSampleDistance = _persistentVegetationStorage.SampleDistance;

            int xCount = Mathf.RoundToInt(_persistentVegetationStorage.BrushSize * 2 / currentSampleDistance);
            int zCount = xCount;

            for (int x = 0; x <= xCount - 1; x++)
            {
                for (int z = 0; z <= zCount - 1; z++)
                {
                    Vector3 samplePosition =
                        corner + new Vector3(x * currentSampleDistance, 0, z * currentSampleDistance);
                    Vector3 normal;

                    var randomizedPosition = _persistentVegetationStorage.RandomizePosition
                        ? RandomizePosition(samplePosition, currentSampleDistance)
                        : samplePosition;

                    samplePosition = _persistentVegetationStorage.PaintOnColliders
                        ? AllignToCollider(samplePosition, out normal)
                        : AllignToTerrain(samplePosition, out normal);

                    if (addVegetationItems)
                    {
                        randomizedPosition = _persistentVegetationStorage.PaintOnColliders
                            ? AllignToCollider(randomizedPosition, out normal)
                            : AllignToTerrain(randomizedPosition, out normal);
                    }

                    if (!SampleBrushPosition(samplePosition, corner)) continue;

                    float size = HandleUtility.GetHandleSize(samplePosition) * 0.1f;
                    Handles.SphereHandleCap(0, samplePosition, Quaternion.identity, size, EventType.Repaint);
                    normal = normal.normalized;
                    Handles.DrawLine(samplePosition, samplePosition + normal);

                    if (!addVegetationItems) continue;
                    if (Event.current.control)
                    {
                        EraseVegetationItem(randomizedPosition, _persistentVegetationStorage.SelectedPaintVegetationID,
                            currentSampleDistance);
                    }
                    else
                    {
                        AddVegetationItem(randomizedPosition, normal,
                            _persistentVegetationStorage.SelectedPaintVegetationID, currentSampleDistance);
                    }
                }
            }
        }

        void SelectGroundLayers()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox(
                "Select ground layers that will be used for selection when adding and moving masks. These will be used in addition to unity terrains.",
                MessageType.Info);
            _persistentVegetationStorage.GroundLayerMask =
                LayerMaskField("Ground Layers", _persistentVegetationStorage.GroundLayerMask);
            GUILayout.EndVertical();
        }

        static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            var layers = InternalEditorUtility.layers;

            LayerNumbers.Clear();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i <= layers.Length -1 ; i++)
            {
                LayerNumbers.Add(LayerMask.NameToLayer(layers[i]));  
            }

            int maskWithoutEmpty = 0;
            for (int i = 0; i <= LayerNumbers.Count -1; i++)
            {
                if (((1 << LayerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

            int mask = 0;
            for (int i = 0; i <= LayerNumbers.Count -1; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << LayerNumbers[i]);
            }

            layerMask.value = mask;

            return layerMask;
        }

        void EraseVegetationItem(Vector3 worldPosition, string vegetationItemID, float sampleDistance)
        {
            if (_persistentVegetationStorage.IgnoreHeight)
            {
                VegetationStudioManager.RemoveVegetationItemInstance2D(vegetationItemID, worldPosition, sampleDistance);
            }
            else
            {
                VegetationStudioManager.RemoveVegetationItemInstance(vegetationItemID, worldPosition, sampleDistance);
            }

        }

        void AddVegetationItem(Vector3 worldPosition, Vector3 terrainNormal, string vegetationItemID,
            float sampleDistance)
        {
            VegetationItemInfoPro vegetationItemInfo = _persistentVegetationStorage.VegetationSystemPro.GetVegetationItemInfo(vegetationItemID);

            float randomScale = UnityEngine.Random.Range(vegetationItemInfo.MinScale, vegetationItemInfo.MaxScale);
            Quaternion rotation;
            Vector3 lookAt;
            var slopeCos = Vector3.Dot(terrainNormal, Vector3.up);
            float slopeAngle = Mathf.Acos(slopeCos) * Mathf.Rad2Deg;
            Vector3 angleScale = Vector3.zero;
            Vector3 scale = new Vector3(randomScale, randomScale, randomScale);

            switch (vegetationItemInfo.Rotation)
            {
                case VegetationRotationType.RotateY:
                    rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 365f), 0));
                    break;
                case VegetationRotationType.FollowTerrain:
                    lookAt = Vector3.Cross(-terrainNormal, Vector3.right);
                    // reverse it if it is down.
                    lookAt = lookAt.y < 0 ? -lookAt : lookAt;
                    // look at the hit's relative up, using the normal as the up vector
                    rotation = Quaternion.LookRotation(lookAt, terrainNormal);
                    //targetUp = Rotation * Vector3.up;
                    rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(0, 365f), new Vector3(0, 1, 0));
                    break;
                case VegetationRotationType.FollowTerrainScale:
                    lookAt = Vector3.Cross(-terrainNormal, Vector3.right);
                    // reverse it if it is down.
                    lookAt = lookAt.y < 0 ? -lookAt : lookAt;
                    // look at the hit's relative up, using the normal as the up vector
                    rotation = Quaternion.LookRotation(lookAt, terrainNormal);
                    //targetUp = Rotation * Vector3.up;
                    rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(0, 365f), new Vector3(0, 1, 0));

                    float newScale = Mathf.Clamp01(slopeAngle / 45f);
                    angleScale = new Vector3(newScale, 0, newScale);
                    break;
                default:
                    rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 365f), 0));
                    break;
            }


            if (vegetationItemInfo.UseSteepnessRule && _persistentVegetationStorage.UseSteepnessRules)
            {
                if (!vegetationItemInfo.UseAdvancedSteepnessRule)
                {
                    if (slopeAngle >= vegetationItemInfo.MaxSteepness || slopeAngle < vegetationItemInfo.MinSteepness) return;
                }
                else
                {
                    float steepnessSpawnChance = SampleCurveArray(vegetationItemInfo.SteepnessRuleCurve.GenerateCurveArray(4096), slopeAngle, 90);
                    if (RandomCutoff(steepnessSpawnChance)) return;
                }
            }

            VegetationStudioManager.AddVegetationItemInstanceEx(vegetationItemID, worldPosition, new Vector3(scale.x + angleScale.x, scale.y + angleScale.y, scale.z + angleScale.z),
                rotation, 5, sampleDistance,1);
        }

        private bool RandomCutoff(float value)
        {
            float randomNumber = UnityEngine.Random.Range(0, 1);
            return !(value > randomNumber);
        }

        private float SampleCurveArray(float[] curveArray, float value, float maxValue)
        {
            if (curveArray.Length == 0) return 0f;

            int index = Mathf.RoundToInt((value / maxValue) * curveArray.Length);
            index = Mathf.Clamp(index, 0, curveArray.Length - 1);
            return curveArray[index];
        }

        Vector3 RandomizePosition(Vector3 position, float sampleDistance)
        {
            float randomDistanceFactor = 4f;

            //UnityEngine.Random.InitState(Mathf.RoundToInt(position.x * 10) + Mathf.RoundToInt(position.z * 10));
            return position + new Vector3(
                       UnityEngine.Random.Range(-sampleDistance / randomDistanceFactor,
                           sampleDistance / randomDistanceFactor),
                       UnityEngine.Random.Range(-sampleDistance / randomDistanceFactor,
                           sampleDistance / randomDistanceFactor),
                       UnityEngine.Random.Range(-sampleDistance / randomDistanceFactor,
                           sampleDistance / randomDistanceFactor));
        }

        bool SampleBrushPosition(Vector3 worldPosition, Vector3 corner)
        {
            Vector3 position = worldPosition - corner;
            float xNormalized = position.x / _vegetationBrush.Size / 2f;
            float zNormalized = position.z / _vegetationBrush.Size / 2f;

            Texture2D currentBrushTexture =
                _brushTextures[_persistentVegetationStorage.SelectedBrushIndex] as Texture2D;
            if (currentBrushTexture == null) return false;

            int x = Mathf.Clamp(Mathf.RoundToInt(xNormalized * currentBrushTexture.width), 0,
                currentBrushTexture.width);
            int z = Mathf.Clamp(Mathf.RoundToInt(zNormalized * currentBrushTexture.height), 0,
                currentBrushTexture.height);

            Color color = currentBrushTexture.GetPixel(x, z);

            if (color.a > 0.1f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Vector3 AllignToTerrain(Vector3 position, out Vector3 normal)
        {
            Ray ray = new Ray(position + new Vector3(0, 1000, 0), Vector3.down);

            var hits = Physics.RaycastAll(ray).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i <= hits.Length - 1; i++)
            {
                if (hits[i].collider is TerrainCollider ||
                    _persistentVegetationStorage.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                {
                    normal = hits[i].normal;
                    return hits[i].point;
                }
            }

            normal = Vector3.up;
            return position;
        }

        Vector3 AllignToCollider(Vector3 position, out Vector3 normal)
        {
            Ray ray = new Ray(position + new Vector3(0, 1000, 0), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
            {
                normal = hit.normal;
                return hit.point;
            }
            else
            {
                normal = Vector3.up;
                return position;
            }
        }

        private VegetationBrush GetActiveBrush(int size)
        {
            if (_vegetationBrush == null)
            {
                _vegetationBrush = new VegetationBrush();
            }

            _vegetationBrush.Load(_brushTextures[_persistentVegetationStorage.SelectedBrushIndex] as Texture2D, size);
            return _vegetationBrush;
        }

        private void UpdatePreviewBrush(ref bool raycastHit, ref Vector3 hitPosition)
        {
            if (!_persistentVegetationStorage.VegetationSystemPro) return;

            Projector previewProjector = GetActiveBrush(Mathf.CeilToInt(_persistentVegetationStorage.BrushSize))
                .GetPreviewProjector();
            // ReSharper disable once NotAccessedVariable
            Vector2 vector;
            Vector3 vector2;

            var hitTarget = _persistentVegetationStorage.PaintOnColliders
                ? RaycastAllColliders(out vector, out vector2)
                : Raycast(out vector, out vector2);

            if (hitTarget)
            {
                previewProjector.material.mainTexture = _brushTextures[_persistentVegetationStorage.SelectedBrushIndex];
                var num = _persistentVegetationStorage.BrushSize;

                previewProjector.enabled = true;

                vector2.y = SampleTerrainHeight(vector2);
                previewProjector.transform.position = vector2 + new Vector3(0f, 50f, 0f);

                hitPosition = vector2;

                previewProjector.orthographicSize = num;
                raycastHit = true;
            }
            else
            {
                previewProjector.enabled = false;
            }
        }

        float SampleTerrainHeight(Vector3 position)
        {
            Ray ray = new Ray(position + new Vector3(0, 10000, 0), Vector3.down);

            var hits = Physics.RaycastAll(ray).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i <= hits.Length - 1; i++)
            {
                if (hits[i].collider is TerrainCollider ||
                    _persistentVegetationStorage.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                {
                    return hits[i].point.y;
                }
            }

            return position.y;
        }

        private bool Raycast(out Vector2 uv, out Vector3 pos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            var hits = Physics.RaycastAll(ray).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i <= hits.Length - 1; i++)
            {
                if (hits[i].collider is TerrainCollider ||
                    _persistentVegetationStorage.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                {
                    uv = hits[i].textureCoord;
                    pos = hits[i].point;
                    return true;
                }
            }

            uv = Vector2.zero;
            pos = Vector3.zero;
            return false;
        }

        private static bool RaycastAllColliders(out Vector2 uv, out Vector3 pos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit raycastHit;
            bool result;
            if (Physics.Raycast(ray, out raycastHit, float.PositiveInfinity))
            {
                uv = raycastHit.textureCoord;
                pos = raycastHit.point;
                result = true;
            }
            else
            {
                uv = Vector2.zero;
                pos = Vector3.zero;
                result = false;
            }

            return result;
        }
    }
}