using System;
using System.Collections.Generic;
using AwesomeTechnologies.BillboardSystem;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Culling;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem.Wind;
using Unity.Collections;
using Unity.Jobs;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.VegetationSystem
{
    [AwesomeTechnologiesScriptOrder(100)]
    [ExecuteInEditMode]
    public partial class VegetationSystemPro : MonoBehaviour
    {
        public QuadTree<VegetationCell> VegetationCellQuadTree;
        public QuadTree<BillboardCell> BillboardCellQuadTree;
        [NonSerialized]
        public readonly List<VegetationCell> VegetationCellList = new List<VegetationCell>();
        [NonSerialized]
        public readonly List<BillboardCell> BillboardCellList = new List<BillboardCell>();
        [NonSerialized]
        public readonly List<VegetationCell> LoadedVegetationCellList = new List<VegetationCell>();
        [NonSerialized]
        public readonly List<VegetationCell> ProcessInstancedIndirectCellList = new List<VegetationCell>();        
        [NonSerialized]
        public readonly List<VegetationCell> CompactMemoryCellList = new List<VegetationCell>();
        [NonSerialized]
        public readonly List<VegetationCell> PredictiveCellLoaderList = new List<VegetationCell>();

        public VegetationCellSpawner VegetationCellSpawner = new VegetationCellSpawner();

        public Bounds VegetationSystemBounds;
        public bool AutomaticBoundsCalculation = true;

        public PersistentVegetationStorage PersistentVegetationStorage;
        
        public PredictiveCellLoader PredictiveCellLoader;
        public int PredictiveCellLoaderCellsPerFrame = 1;
        public bool LoadPotentialVegetationCells = true;
        
        public int CurrentTabIndex;
        public int VegetationPackageIndex;        
        public float SeaLevel;
        public bool ExcludeSeaLevelCells;
        public float VegetationCellSize = 100;
        public float BillboardCellSize = 500;
        
        [NonSerialized]
        public float AdditionalBoundingSphereRadius;

        public int SelectedTextureMaskGroupTextureIndex;
        public int SelectedTextureMaskGroupIndex;

        public TextureMask DebugTextureMask;

        [NonSerialized]
        public bool InitDone;

        private JobHandle _prepareVegetationHandle;

        public VegetationSettings VegetationSettings = new VegetationSettings();
        public VegetationRenderSettings VegetationRenderSettings = new VegetationRenderSettings();
        public EnvironmentSettings EnvironmentSettings = new EnvironmentSettings();
        public List<VegetationStudioCamera> VegetationStudioCameraList = new List<VegetationStudioCamera>();

        public bool ShowVegetationCells;
        public bool ShowBillboardCells;
        public bool ShowVisibleBillboardCells;
        public bool ShowPotentialVisibleCells;
        public bool ShowVisibleCells;
        public bool ShowBiomeCells;
        public bool ShowVegetationMaskCells;
        public bool ShowHeatMap;
        public bool ShowTerrainTextures = true;

        public bool ShowVegetationPackageGeneralSettingsMenu = true;
        public bool ShowVegetationPackageNoiseMenu = true;
        public bool ShowTerrainTextureRulesMenu = true;
        public bool ShowTextureMaskRulesMenu = true;
        public bool ShowVegetationMaskRulesMenu = true;
        public bool ShowShaderSettingsMenu = true;
        public bool ShowPositionMenu = true;
        public bool ShowDistanceFalloffMenu = true;
        public bool ShowBiomeRulesMenu = true;
        public bool ShowConcaveLocationRulesMenu = true;
        public bool ShowColliderRulesMenu = true;
        public bool ShowBillboardsMenu = true;
        public bool ShowVegetationItemSettingsMenu = true;
        public bool ShowTerrainSourceSettingsMenu = true;
        public bool ShowAddVegetationItemMenu = true;
        public bool ShowLODMenu = true;

        //public bool ForceBillboardCellLoadinComplete = false;

        public List<IVegetationStudioTerrain> VegetationStudioTerrainList = new List<IVegetationStudioTerrain>();
        public List<GameObject> VegetationStudioTerrainObjectList = new List<GameObject>();
        public List<VegetationPackagePro> VegetationPackageProList = new List<VegetationPackagePro>();
        public List<VegetationPackageProModelInfo> VegetationPackageProModelsList = new List<VegetationPackageProModelInfo>();
        public List<WindControllerSettings> WindControllerSettingsList = new List<WindControllerSettings>();
        
        public delegate void MultiOnAddCameraDelegate(VegetationStudioCamera vegetationStudioCamera);
        public MultiOnAddCameraDelegate OnAddCameraDelegate;

        public delegate void MultiOnRemoveCameraDelegate(VegetationStudioCamera vegetationStudioCamera);
        public MultiOnRemoveCameraDelegate OnRemoveCameraDelegate;        
        
        public delegate void MultiOnVegetationStudioRefreshDelegate(VegetationSystemPro vegetationSystemPro);
        public MultiOnVegetationStudioRefreshDelegate OnRefreshVegetationSystemDelegate;
        public MultiOnVegetationStudioRefreshDelegate OnRefreshColliderSystemDelegate;
        public MultiOnVegetationStudioRefreshDelegate OnRefreshRuntimePrefabSpawnerDelegate;        
        
        public delegate void MultiOnClearCacheDelegate(VegetationSystemPro vegetationSystemPro);
        public delegate void MultiOnClearCacheVegetationCellDelegate(VegetationSystemPro vegetationSystemPro, VegetationCell vegetationCell);        
        public delegate void MultiOnClearCacheVegetationItemDelegate(VegetationSystemPro vegetationSystemPro, int vegetationPackageIndex, int vegetationItemIndex);
        public delegate void MultiOnClearCacheVegetationCellVegetationItemDelegate(VegetationSystemPro vegetationSystemPro, VegetationCell vegetationCell, int vegetationPackageIndex, int vegetationItemIndex);
        
        public delegate void MultiOnVegetationCellSpawnedDelegate(VegetationCell vegetationCell);
        public MultiOnVegetationCellSpawnedDelegate OnVegetationCellLoaded;  
        
        public MultiOnClearCacheDelegate OnClearCacheDelegate;
        public MultiOnClearCacheVegetationItemDelegate OnClearCacheVegetationItemDelegate;
        public MultiOnClearCacheVegetationCellDelegate OnClearCacheVegetationCellDelegate;    
        public MultiOnClearCacheVegetationCellVegetationItemDelegate OnClearCacheVegetationCellVegetatonItemDelegate;                  
        
        public delegate void MultOnRenderCompleteDelegate(VegetationSystemPro vegetationSystemPro);
        public MultOnRenderCompleteDelegate OnRenderCompleteDelegate;
        
        [NonSerialized]
        private readonly List<IWindController> _windControllerList = new List<IWindController>();

        public WindZone SelectedWindZone;
        public float WindSpeedFactor = 1f;
        public Light SunDirectionalLight;

        // ReSharper disable once UnusedMember.Local

        //private Texture _dummyMaskTexture;
        private ComputeBuffer _dummyComputeBuffer;

        public int FrustumKernelHandle;
        //public int DistanceKernelHandle;
        public ComputeShader FrusumMatrixShader;
        private int _cameraFrustumPlan0;
        private int _cameraFrustumPlan1;
        private int _cameraFrustumPlan2;
        private int _cameraFrustumPlan3;
        private int _cameraFrustumPlan4;
        private int _cameraFrustumPlan5;

        public int MergeBufferKernelHandle;
        public ComputeShader MergeBufferShader;

        private int _floatingOriginOffsetID = -1;

        private int _mergeBufferID = -1;
        private int _mergeSourceBuffer0ID = -1;
        private int _mergeSourceBuffer1ID = -1;
        private int _mergeSourceBuffer2ID = -1;
        private int _mergeSourceBuffer3ID = -1;
        private int _mergeSourceBuffer4ID = -1;
        private int _mergeSourceBuffer5ID = -1;
        private int _mergeSourceBuffer6ID = -1;
        private int _mergeSourceBuffer7ID = -1;
        private int _mergeSourceBuffer8ID = -1;
        private int _mergeSourceBuffer9ID = -1;
        private int _mergeSourceBuffer10ID = -1;
        private int _mergeSourceBuffer11ID = -1;
        private int _mergeSourceBuffer12ID = -1;
        private int _mergeSourceBuffer13ID = -1;
        private int _mergeSourceBuffer14ID = -1;

        private int _mergeInstanceCount0ID = -1;
        private int _mergeInstanceCount1ID = -1;
        private int _mergeInstanceCount2ID = -1;
        private int _mergeInstanceCount3ID = -1;
        private int _mergeInstanceCount4ID = -1;
        private int _mergeInstanceCount5ID = -1;
        private int _mergeInstanceCount6ID = -1;
        private int _mergeInstanceCount7ID = -1;
        private int _mergeInstanceCount8ID = -1;
        private int _mergeInstanceCount9ID = -1;
        private int _mergeInstanceCount10ID = -1;
        private int _mergeInstanceCount11ID = -1;
        private int _mergeInstanceCount12ID = -1;
        private int _mergeInstanceCount13ID = -1;
        private int _mergeInstanceCount14ID = -1;

        private int _visibleBufferLod0ID = -1;
        private int _visibleBufferLod1ID = -1;
        private int _visibleBufferLod2ID = -1;
        private int _visibleBufferLod3ID = -1;
        
        private int _shadowBufferLod0ID = -1;
        private int _shadowBufferLod1ID = -1;
        private int _shadowBufferLod2ID = -1;
        private int _shadowBufferLod3ID = -1;
        
        private int _sourceBufferID = -1;
        private int _instanceCountID = -1;

        private int _boundingSphereRadiusID = -1;
        private int _useLodsID = -1;
        private int _noFrustumCullingID = -1;
        private int _shadowCullingID = -1;

        private int _cullFarStartID;
        private int _visibleShaderDataBufferID;
        private int _indirectShaderDataBufferID;

        private int _cameraPositionID;
        private int _cullDistanceID;
        private int _farCullDistanceID;

        private int _unityLODFadeID;

        private int _lod1Distance = -1;
        private int _lod2Distance = -1;
        private int _lod3Distance = -1;
        
        private int _lightDirection = -1;
        private int _planeOrigin = -1;
        private int _boundsSize = -1;       
        
        private int _lodFactor = -1;
        private int _lodBias = -1;
        private int _lodFadeDistance = -1;
        private int _lodCount = -1;

        private readonly List<VegetationCell> _hasBufferList = new List<VegetationCell>();

        private readonly Matrix4x4[] _renderArray = new Matrix4x4[1000];
        private readonly Vector4[] _renderLodFadeArray = new Vector4[1000];
        //private Vector4[] _lodFadeSingleArray = new Vector4[1];

        //Floating origin variables
        public Transform FloatingOriginAnchor;
        public Vector3 FloatingOriginOffset;
        public Vector3 FloatingOriginStartPosition;

        public Vector3 VegetationSystemPosition
        {
            get
            {
                Vector3 position = VegetationSystemBounds.center - VegetationSystemBounds.extents;
                position.y = 0;
                return position;
            }           
        }

        public void DetectPersistentVegetationStorage()
        {
            if (!PersistentVegetationStorage)
            {
                PersistentVegetationStorage = GetComponent<PersistentVegetationStorage>();
            }
        }
        
        // ReSharper disable once UnusedMember.Local
        void Reset()
        {
            AutoSelectCamera();
            FindWindZone();
            FindDirectionalLight();
            DetectPersistentVegetationStorage();
        }

        void FindDirectionalLight()
        {
            Light selectedLight = null;
            float intensity = float.MinValue;

            Light[] lights = FindObjectsOfType<Light>();
            for (int i = 0; i <= lights.Length - 1; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    if (lights[i].intensity > intensity)
                    {
                        intensity = lights[i].intensity;
                        selectedLight = lights[i];
                    }
                }
            }

            SunDirectionalLight = selectedLight;
        }

        void AutoSelectCamera()
        {
            Camera selectedCamera = Camera.main;

            if (selectedCamera == null)
            {
                Camera[] cameras = FindObjectsOfType<Camera>();
                for (int i = 0; i <= cameras.Length - 1; i++)
                {
                    if (cameras[i].gameObject.name.Contains("Main Camera") ||
                        cameras[i].gameObject.name.Contains("MainCamera"))
                    {
                        selectedCamera = cameras[i];
                        break;
                    }
                }
            }
            AddCamera(selectedCamera);
        }

        public void RefreshVegetationSystem()
        {
            SetupVegetationSystem();
        }
        
        public void RefreshColliderSystem()
        {
            OnRefreshColliderSystemDelegate?.Invoke(this);
        }
        
        public void RefreshRuntimePrefabSpawner()
        {
            OnRefreshRuntimePrefabSpawnerDelegate?.Invoke(this);
        }

        void SetupVegetationSystem()
        {
            CompleteCellLoading();

            ProcessInstancedIndirectCellList.Clear();
            DisposeVegetationStudioCameras();
            DisposeVegetationCells();
            DisposeBillboardCells();

            if (VegetationSystemBounds.size.magnitude < 1) return;
            SetupWindSamplers();

            SetupVegetationItemModels();
            RefreshVegetationStudioTerrains();
            CreateVegetationCells();
            CreateBillboardCells();
            SetVegetationStudioCamerasDirty();

            OnRefreshVegetationSystemDelegate?.Invoke(this);
        }

        public void CompleteCellLoading()
        {
            _prepareVegetationHandle.Complete();
        }
      
        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            VegetationStudioManager.RegisterVegetationSystem(this);
            DetectPersistentVegetationStorage();
            
            EnableEditorApi();

            LoadSettingsFromQualityManager();

            SetupPredictiveCellLoader();
            SetupVegetationCellSpawner();

            SetupSceneviewCamera();
            SetupFloatingOrigin();

            SetupComputeShaders();
            SetupBillboardShaderIDs();
            SetupInstancedRenderMaterialPropertiesIDs();

            SetupVegetationSystem();
            VegetationCellSpawner.Init();
            SetupWind();
            InitDone = true;
        }
        
        void LoadSettingsFromQualityManager()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            QualityManager qualityManager = GetComponent<QualityManager>();
            if (qualityManager)
            {
                qualityManager.SetQualityLevel(false);
            }
        }


        void EnableEditorApi()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SceneViewDetector.OnSceneViewTransformChangeDelegate += OnSceneviewTransformChanged;
            }
#endif
        }

        void DisableEditorApi()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // ReSharper disable once DelegateSubtraction
                SceneViewDetector.OnSceneViewTransformChangeDelegate -= OnSceneviewTransformChanged;
            }
#endif
        }

        void OnSceneviewTransformChanged(Camera currentCamera)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        void SetupSceneviewCamera()
        {
            for (int i = VegetationStudioCameraList.Count - 1; i >= 0; i--)
            {
                if (VegetationStudioCameraList[i].VegetationStudioCameraType == VegetationStudioCameraType.SceneView || VegetationStudioCameraList[i].SelectedCamera == null)
                {
                    RemoveVegetationStudioCamera(VegetationStudioCameraList[i]);
                }
            }

            if (!Application.isPlaying)
            {
                VegetationStudioCamera sceneviewCamera =
                    new VegetationStudioCamera(VegetationStudioCameraType.SceneView)
                    {
                        CameraCullingMode  = CameraCullingMode.Frustum,
                        RenderDirectToCamera = false,
                        VegetationSystemPro = this
                    };

                AddVegetationStudioCamera(sceneviewCamera);
                //VegetationStudioCameraList.Add(sceneviewCamera);
            }

            if (Application.isPlaying && VegetationStudioCameraList.Count == 0)
            {
                AutoSelectCamera();
            }
        }

        void SetupFloatingOrigin()
        {
            Transform anchor = GetFloatingOriginAnchor();
            FloatingOriginStartPosition = anchor.position;
        }
        void UpdateFloatingOrigin()
        {
            if (Application.isPlaying)
            {
                Transform anchor = GetFloatingOriginAnchor();
                FloatingOriginOffset = anchor.transform.position - FloatingOriginStartPosition;
            }
            else
            {
                FloatingOriginOffset = Vector3.zero;
            }
        }

        Transform GetFloatingOriginAnchor()
        {
            if (FloatingOriginAnchor) return FloatingOriginAnchor;
            return transform;
        }

//        void PrepareRenderLists()
//        {
//            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
//            {              
//                VegetationStudioCameraList[i].PrepareRenderLists(VegetationPackageProList);
//            }
//        }
        
        // ReSharper disable once UnusedMember.Local
        void Update()
        {
            if (!InitDone) return;

            UpdateFloatingOrigin();

            if (VegetationCellList.Count <= 0) return;

            JobHandle cullingHandle = default(JobHandle);
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                VegetationStudioCameraList[i].SetFloatingOriginOffset(FloatingOriginOffset);                  
                VegetationStudioCameraList[i].PreCullVegetation(false);
                VegetationStudioCameraList[i].PrepareRenderLists(VegetationPackageProList);
            }

            //PrepareRenderLists();

            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;
                cullingHandle = VegetationStudioCameraList[i].ScheduleCullVegetationJob(cullingHandle);
            }           

            Profiler.BeginSample("VegetationCell culling");
            cullingHandle.Complete();
            Profiler.EndSample();
            
            Profiler.BeginSample("Process culling events");
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;
                VegetationStudioCameraList[i].ProcessEvents();
            }             
            Profiler.EndSample();

            VerifySplatmapAccess();

            Profiler.BeginSample("Prepare cell loading jobs");
            VegetationCellSpawner.CellJobHandleList.Clear();

            float worldspaceMinHeight = VegetationSystemBounds.center.y -
                                        VegetationSystemBounds.extents.y;
            float worldspaceSeaLevel = worldspaceMinHeight + SeaLevel;
            VegetationCellSpawner.WorldspaceSeaLevel = worldspaceSeaLevel;

            PredictiveCellLoaderList.Clear();
            PredictiveCellLoader.GetCellsToLoad(PredictiveCellLoaderList);
            for (int i = 0; i <= PredictiveCellLoaderList.Count - 1; i++)
            {
                VegetationCell vegetationCell = PredictiveCellLoaderList[i];

                bool hasData = vegetationCell.LoadedDistanceBand != 99; 
                
                bool hasInstancedIndirect;
                JobHandle vegetationCellHandle = VegetationCellSpawner.SpawnVegetationCell(vegetationCell, 0, out hasInstancedIndirect,false);
                OnVegetationCellLoaded?.Invoke(vegetationCell);

                if (!hasData)
                {
                    if (!LoadedVegetationCellList.Contains(vegetationCell))
                    {
                        LoadedVegetationCellList.Add(vegetationCell);
                    }
                }
                    
                if (hasInstancedIndirect)
                {
                    ProcessInstancedIndirectCellList.Add(vegetationCell);
                }
                VegetationCellSpawner.CellJobHandleList.Add(vegetationCellHandle);
            }

            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;

                for (int j = 0; j <= VegetationStudioCameraList[i].JobCullingGroup.VisibleCellIndexList.Length - 1; j++)
                {
                    int potentialVisibleCellIndex = VegetationStudioCameraList[i].JobCullingGroup.VisibleCellIndexList[j];
                    VegetationCell vegetationCell = VegetationStudioCameraList[i].PotentialVisibleCellList[potentialVisibleCellIndex];

                    BoundingSphereInfo boundingSphereInfo = VegetationStudioCameraList[i].GetBoundingSphereInfo(potentialVisibleCellIndex);
                    if (vegetationCell.LoadedDistanceBand <= boundingSphereInfo.CurrentDistanceBand) continue;

//                    if (!Application.isPlaying && !vegetationCell.Prepared)
//                    {
//                        VegetationCellSpawner.PrepareVegetationCell(vegetationCell);
//                    }

                    // ReSharper disable once InlineOutVariableDeclaration
                    bool hasInstancedIndirect;
                    JobHandle vegetationCellHandle = VegetationCellSpawner.SpawnVegetationCell(vegetationCell, boundingSphereInfo.CurrentDistanceBand, out hasInstancedIndirect,false);
                    OnVegetationCellLoaded?.Invoke(vegetationCell);
                    LoadedVegetationCellList.Add(vegetationCell);
                    
                    if (hasInstancedIndirect)
                    {
                        ProcessInstancedIndirectCellList.Add(vegetationCell);
                    }

                    VegetationCellSpawner.CellJobHandleList.Add(vegetationCellHandle);
                }
            }

            _prepareVegetationHandle = JobHandle.CombineDependencies(VegetationCellSpawner.CellJobHandleList);
            Profiler.EndSample();

            JobHandle.ScheduleBatchedJobs();

            Profiler.BeginSample("Prepare render list jobs");
            float lodBias = QualitySettings.lodBias * VegetationSettings.LODDistanceFactor;
            Vector3 sunLightDirection = SunDirectionalLight ? SunDirectionalLight.transform.forward : new Vector3(0, 0, 0);
            float minBoundsHeight = VegetationSystemBounds.center.y - VegetationSystemBounds.extents.y;
            Vector3 planeOrigin = new Vector3(0, minBoundsHeight, 0);
            bool shadowCulling = (SunDirectionalLight != null);
            bool isPlaying = Application.isPlaying;

            VegetationCellSpawner.CellJobHandleList.Clear();
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;
                if (VegetationStudioCameraList[i].RenderBillboardsOnly) continue;

                for (int j = 0; j <= VegetationPackageProList.Count - 1; j++)
                {
                    for (int k = 0; k <= VegetationPackageProList[j].VegetationInfoList.Count - 1; k++)
                    {
                        NativeList<MatrixInstance> vegetationItemMatrixList =
                            VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j]
                                .VegetationItemMergeMatrixList[k];
                        vegetationItemMatrixList.Clear();

                        VegetationItemModelInfo vegetationItemModelInfo =
                            VegetationPackageProModelsList[j].VegetationItemModelList[k];

                        bool useInstancedIndirect = VegetationRenderSettings.UseInstancedIndirect();
                        if (useInstancedIndirect && vegetationItemModelInfo.VegetationItemInfo.VegetationRenderMode == VegetationRenderMode.InstancedIndirect) continue;
                        
                        //if (isPlaying && vegetationItemModelInfo.VegetationItemInfo.VegetationRenderMode ==
                        //    VegetationRenderMode.InstancedIndirect) continue;

                        //if (!vegetationItemModelInfo.VegetationItemInfo.EnableRuntimeSpawn) continue;

                        JobHandle vegetationItemMergeJobHandle = _prepareVegetationHandle;
                        for (int l = 0;
                            l <= VegetationStudioCameraList[i].JobCullingGroup.VisibleCellIndexList.Length - 1;
                            l++)
                        {
                            int potentialVisibleCellIndex = VegetationStudioCameraList[i].JobCullingGroup.VisibleCellIndexList[l];
                            VegetationCell vegetationCell = VegetationStudioCameraList[i]
                                .PotentialVisibleCellList[potentialVisibleCellIndex];
                            BoundingSphereInfo boundingSphereInfo = VegetationStudioCameraList[i]
                                .GetBoundingSphereInfo(potentialVisibleCellIndex);

                            int vegetationItemDistanceBand = vegetationItemModelInfo.DistanceBand;
//                            if (vegetationItemModelInfo.VegetationItemInfo.VegetationType == VegetationType.Objects)
//                            {
//                                vegetationItemDistanceBand = 0;
//                            }
                            
                            if (boundingSphereInfo.CurrentDistanceBand > vegetationItemDistanceBand) continue;

                            MergeCellInstancesJob mergeCellInstancesJob =
                                new MergeCellInstancesJob
                                {
                                    OutputNativeList = vegetationItemMatrixList,
                                    InputNativeList = vegetationCell.VegetationPackageInstancesList[j]
                                        .VegetationItemMatrixList[k]
                                };
                            Profiler.BeginSample("Schedule");
                            vegetationItemMergeJobHandle = mergeCellInstancesJob.Schedule(vegetationItemMergeJobHandle);
                            Profiler.EndSample();
                        }

                        NativeList<Matrix4x4> lod0MatrixList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD0MatrixList[k];
                        NativeList<Matrix4x4> lod1MatrixList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD1MatrixList[k];
                        NativeList<Matrix4x4> lod2MatrixList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD2MatrixList[k];
                        NativeList<Matrix4x4> lod3MatrixList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD3MatrixList[k];

                        NativeList<Matrix4x4> lod0ShadowMatrixList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD0ShadowMatrixList[k];
                        NativeList<Matrix4x4> lod1ShadowMatrixList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD1ShadowMatrixList[k];
                        NativeList<Matrix4x4> lod2ShadowMatrixList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD2ShadowMatrixList[k];
                        NativeList<Matrix4x4> lod3ShadowMatrixList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD3ShadowMatrixList[k];

                        NativeList<Vector4> lod0FadeList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD0LodFadeList[k];
                        NativeList<Vector4> lod1FadeList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD1LodFadeList[k];
                        NativeList<Vector4> lod2FadeList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD2LodFadeList[k];
                        NativeList<Vector4> lod3FadeList = VegetationStudioCameraList[i].VegetationStudioCameraRenderList[j].VegetationItemLOD3LodFadeList[k];

                        lod0MatrixList.Clear();
                        lod1MatrixList.Clear();
                        lod2MatrixList.Clear();
                        lod3MatrixList.Clear();

                        lod0ShadowMatrixList.Clear();
                        lod1ShadowMatrixList.Clear();
                        lod2ShadowMatrixList.Clear();
                        lod3ShadowMatrixList.Clear();

                        lod0FadeList.Clear();
                        lod1FadeList.Clear();
                        lod2FadeList.Clear();
                        lod3FadeList.Clear();

                        float vegetationItemCullDistance;

                        float renderDistanceFactor = vegetationItemModelInfo.VegetationItemInfo.RenderDistanceFactor;
                        if (VegetationSettings.DisableRenderDistanceFactor) renderDistanceFactor = 1;

                        //if (vegetationItemModelInfo.DistanceBand == 0)
                        if (vegetationItemModelInfo.VegetationItemInfo.VegetationType == VegetationType.Tree || vegetationItemModelInfo.VegetationItemInfo.VegetationType == VegetationType.LargeObjects)
                        {
                            vegetationItemCullDistance = VegetationSettings.GetTreeDistance() * renderDistanceFactor;
                        }
                        else
                        {
                            vegetationItemCullDistance = VegetationSettings.GetVegetationDistance() * renderDistanceFactor;
                        }

                        ShadowCastingMode shadowCastingMode =
                            VegetationSettings.GetShadowCastingMode(vegetationItemModelInfo.VegetationItemInfo
                                .VegetationType);
                        bool useShadowCulling = shadowCulling && shadowCastingMode == ShadowCastingMode.On;
                        useShadowCulling = useShadowCulling && vegetationItemModelInfo.DistanceBand == 1;

                        if (VegetationStudioCameraList[i].SelectedCamera == null)
                        {
                            VegetationCellSpawner.CellJobHandleList.Add(vegetationItemMergeJobHandle);
                            continue;
                        } 
                        
                        VegetationItemLODSplitAndFrustumCullingJob lodJob =
                            new VegetationItemLODSplitAndFrustumCullingJob
                            {
                                BoundingSphereRadius = vegetationItemModelInfo.BoundingSphereRadius,
                                BoundsSize = vegetationItemModelInfo.VegetationItemInfo.Bounds.size,
                                VegetationItemDistanceBand = vegetationItemModelInfo.DistanceBand,
                                VegetationItemMatrixList = vegetationItemMatrixList,
                                VegetationItemLOD0MatrixList = lod0MatrixList,
                                VegetationItemLOD1MatrixList = lod1MatrixList,
                                VegetationItemLOD2MatrixList = lod2MatrixList,
                                VegetationItemLOD3MatrixList = lod3MatrixList,
                                VegetationItemLOD0ShadowMatrixList = lod0ShadowMatrixList,
                                VegetationItemLOD1ShadowMatrixList = lod1ShadowMatrixList,
                                VegetationItemLOD2ShadowMatrixList = lod2ShadowMatrixList,
                                VegetationItemLOD3ShadowMatrixList = lod3ShadowMatrixList,
                                LOD0FadeList = lod0FadeList,
                                LOD1FadeList = lod1FadeList,
                                LOD2FadeList = lod2FadeList,
                                LOD3FadeList = lod3FadeList,
                                LightDirection = sunLightDirection,
                                ShadowCulling = useShadowCulling,
                                PlaneOrigin = planeOrigin,
                                FrustumPlanes = VegetationStudioCameraList[i].JobCullingGroup.FrustumPlanes,
                                CameraPosition = VegetationStudioCameraList[i].SelectedCamera.transform.position,
                                NoFrustumCulling = VegetationStudioCameraList[i].CameraCullingMode == CameraCullingMode.Complete360,
                                CullDistance = vegetationItemCullDistance,
                                LOD1Distance = vegetationItemModelInfo.LOD1Distance,
                                LOD2Distance = vegetationItemModelInfo.LOD2Distance,
                                LOD3Distance = vegetationItemModelInfo.LOD3Distance,
                                LODFactor = vegetationItemModelInfo.VegetationItemInfo.LODFactor,
                                LODBias = lodBias,
                                LODCount = vegetationItemModelInfo.LODCount,
                                LODFadeDistance = 10,
                                LODFadePercentage = vegetationItemModelInfo.LODFadePercentage,
                                LODFadeCrossfade = vegetationItemModelInfo.LODFadeCrossfade,
                                FloatingOriginOffset = FloatingOriginOffset                                
                            };

                        vegetationItemMergeJobHandle = lodJob.Schedule(vegetationItemMergeJobHandle);
                        VegetationCellSpawner.CellJobHandleList.Add(vegetationItemMergeJobHandle);
                        
//#if UNITY_EDITOR                        
//                        if (JobsUtility.JobDebuggerEnabled) vegetationItemMergeJobHandle.Complete();
//#endif
                    }
                }
            }

            Profiler.EndSample();

            if (VegetationCellSpawner.CellJobHandleList.Length > 0)
            {
                _prepareVegetationHandle = JobHandle.CombineDependencies(VegetationCellSpawner.CellJobHandleList);
            }

            JobHandle.ScheduleBatchedJobs();

            //TODO add merge vegetation instances for buillboards here
        }

        void SetShadowMapVariables()
        {

            if (SunDirectionalLight)
            {
                Vector3 sundirection = -SunDirectionalLight.transform.forward * 2.5f;
                Vector4 gVsSunDirection = new Vector4(sundirection.x, sundirection.y, sundirection.z, SunDirectionalLight.intensity);
                Shader.SetGlobalVector("gVSSunDirection", gVsSunDirection);
                Shader.SetGlobalVector("gVSSunSettings", new Vector4(SunDirectionalLight.shadowStrength, SunDirectionalLight.shadowBias, 0, 0));

            }
            else
            {
                Shader.SetGlobalVector("gVSSunDirection", Vector4.zero);
                Shader.SetGlobalVector("gVSSunSettings", new Vector4(0, 0, 0, 0));
            }
        }
        
        void InitGlobalShaderProperties()
        {
            float minVegetationDistance = Mathf.Clamp(VegetationSettings.GetVegetationDistance(), 20,
                VegetationSettings.GetVegetationDistance() - 20);
            Shader.SetGlobalVector("_VSGrassFade", new Vector4(minVegetationDistance, 20, 0, 0));
            Shader.SetGlobalVector("_VSShadowMapFadeScale", new Vector4(QualitySettings.shadowDistance - 30, 20, 1, 1));
        }
        
        
        // ReSharper disable once UnusedMember.Local
        void LateUpdate()
        {
            if (!InitDone) return;
            UpdateWind();
            SetShadowMapVariables();
            InitGlobalShaderProperties();

            Profiler.BeginSample("Complete spawning jobs");
            _prepareVegetationHandle.Complete();
            Profiler.EndSample();

            VerifySplatmapAccess();
            
            LoadBillboardCells();
            RenderBillboardCells();

            JobHandle prepareInstancedIndirectHandle = default(JobHandle);
            
            bool useInstancedIndirect = VegetationRenderSettings.UseInstancedIndirect();
            if (Application.isPlaying && useInstancedIndirect)
            {
                prepareInstancedIndirectHandle = PrepareInstancedIndirectSetupJobs();
            }
            RenderInstancedVegetation();
           
            if (Application.isPlaying && useInstancedIndirect)
            {
                prepareInstancedIndirectHandle.Complete();
                SetupInstancedIndirectComputeBuffers();
                RenderInstancedIndirectVegetation();
            }

            DisposeTemporaryTerrainMemory();
            ReturnVegetationCellTemporaryMemory();
            
            OnRenderCompleteDelegate?.Invoke(this);
        }
        
        JobHandle PrepareInstancedIndirectSetupJobs()
        {
            VegetationCellSpawner.CellJobHandleList.Clear();

            for (int i = 0; i <= ProcessInstancedIndirectCellList.Count - 1; i++)
            {
                VegetationCell vegetationCell = ProcessInstancedIndirectCellList[i];
                for (int j = 0; j <= vegetationCell.VegetationPackageInstancesList.Count - 1; j++)
                {
                    for (int k = 0; k <= vegetationCell.VegetationPackageInstancesList[j].VegetationItemMatrixList.Count - 1; k++)
                    {
                        VegetationItemInfoPro vegetationItemInfoPro = VegetationPackageProList[j].VegetationInfoList[k];
                        IndirectInstanceInfo indirectInstanceInfo = vegetationCell.VegetationPackageInstancesList[j]
                            .VegetationItemInstancedIndirectInstanceList[k];

                        VegetationItemModelInfo vegetationItemModelInfo = VegetationPackageProModelsList[j].VegetationItemModelList[k];

                        bool hasTreesLoaded = vegetationCell.LoadedBillboards && vegetationItemInfoPro.UseBillboards &&
                                              vegetationItemInfoPro.VegetationType == VegetationType.Tree;
                        if (vegetationItemModelInfo.DistanceBand < vegetationCell.LoadedDistanceBand && !hasTreesLoaded) continue;

                        if (vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.InstancedIndirect && !indirectInstanceInfo.Created)
                        {
                            NativeArray<MatrixInstance> vegetationItemMatrixList =
                                vegetationCell.VegetationPackageInstancesList[j].VegetationItemMatrixList[k];

                            indirectInstanceInfo.InstancedIndirectInstanceList = new NativeArray<InstancedIndirectInstance>(vegetationItemMatrixList.Length, Allocator.Persistent);
                            indirectInstanceInfo.Created = true;

                            CreateInstancedIndirectInstancesJob createInstancedIndirectInstancesJob =
                                new CreateInstancedIndirectInstancesJob
                                {
                                    InstanceList = vegetationItemMatrixList,
                                    IndirectInstanceList = indirectInstanceInfo.InstancedIndirectInstanceList
                                };
                            JobHandle handle = createInstancedIndirectInstancesJob.Schedule(
                                indirectInstanceInfo.InstancedIndirectInstanceList.Length, 32);
                            VegetationCellSpawner.CellJobHandleList.Add(handle);
                        }
                    }
                }
            }

            JobHandle processHandle = JobHandle.CombineDependencies(VegetationCellSpawner.CellJobHandleList);
            VegetationCellSpawner.CellJobHandleList.Clear();
            JobHandle.ScheduleBatchedJobs();
            return processHandle;
        }

        void SetupInstancedIndirectComputeBuffers()
        {
            bool useInstancedIndirect = VegetationRenderSettings.UseInstancedIndirect();
            if (!useInstancedIndirect) return;
            
            Profiler.BeginSample("Update compute buffers");
            for (int i = 0; i <= ProcessInstancedIndirectCellList.Count - 1; i++)
            {
                VegetationCell vegetationCell = ProcessInstancedIndirectCellList[i];
                for (int j = 0; j <= vegetationCell.VegetationPackageInstancesList.Count - 1; j++)
                {
                    for (int k = 0; k <= vegetationCell.VegetationPackageInstancesList[j].VegetationItemMatrixList.Count - 1; k++)
                    {
                        VegetationItemInfoPro vegetationItemInfoPro = VegetationPackageProList[j].VegetationInfoList[k];
                        IndirectInstanceInfo indirectInstanceInfo = vegetationCell.VegetationPackageInstancesList[j]
                            .VegetationItemInstancedIndirectInstanceList[k];
                        ComputeBufferInfo computeBufferInfo = vegetationCell.VegetationPackageInstancesList[j]
                            .VegetationItemComputeBufferList[k];

                        VegetationItemModelInfo vegetationItemModelInfo = VegetationPackageProModelsList[j].VegetationItemModelList[k];

                        //if (vegetationItemModelInfo.DistanceBand < vegetationCell.LoadedDistanceBand) continue;
                        bool hasTreesLoaded = vegetationCell.LoadedBillboards && vegetationItemInfoPro.UseBillboards &&
                                              vegetationItemInfoPro.VegetationType == VegetationType.Tree;
                        if (vegetationItemModelInfo.DistanceBand < vegetationCell.LoadedDistanceBand && !hasTreesLoaded) continue;
                        
                        if (vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.InstancedIndirect && !computeBufferInfo.Created)
                        {
                            int length = indirectInstanceInfo.InstancedIndirectInstanceList.Length;                         
                            if (length == 0) length = 1;

                            //TODO handle 0 length cells better

                            computeBufferInfo.ComputeBuffer = new ComputeBuffer(length, 16 * 4 + 4 * 4); //*2
                            computeBufferInfo.ComputeBuffer.SetData(indirectInstanceInfo.InstancedIndirectInstanceList);
                            computeBufferInfo.Created = true;
                            //TODO clear indirectInstanceInfo.InstancedIndirectInstanceList; //might be needed if unity clears buffers
                        }
                    }
                }
            }
            ProcessInstancedIndirectCellList.Clear();
            Profiler.EndSample();
        }

        void RenderInstancedIndirectVegetation()
        {
            DrawCellsIndirectComputeShader();
        }

        void DisposeTemporaryTerrainMemory()
        {
            for (int i = 0; i <= VegetationStudioTerrainList.Count - 1; i++)
            {
                VegetationStudioTerrainList[i].DisposeTemporaryMemory();
            }
        }

        //public List<Matrix4x4> RenderList = new List<Matrix4x4>();


       // private readonly byte[] _renderbyteArray = new byte[1000 * 16 * 4];
       // private readonly byte[] _renderbyteLodFadeArray = new byte[1000 * 4 * 4];

        void RenderInstancedVegetation()
        {
            Profiler.BeginSample("Draw instanced vegetation");
            bool isPlaying = Application.isPlaying;

            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;
                if (VegetationStudioCameraList[i].RenderBillboardsOnly) continue;

                var targetCamera = VegetationStudioCameraList[i].RenderDirectToCamera
                    ? VegetationStudioCameraList[i].SelectedCamera
                    : null;

                if (!Application.isPlaying) targetCamera = null;



                for (int j = 0; j <= VegetationPackageProList.Count - 1; j++)
                {
                    for (int k = 0; k <= VegetationPackageProList[j].VegetationInfoList.Count - 1; k++)
                    {
                        VegetationItemInfoPro vegetationItemInfoPro = VegetationPackageProList[j].VegetationInfoList[k];
                        bool useInstancedIndirect = VegetationRenderSettings.UseInstancedIndirect();
                        if (useInstancedIndirect && vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.InstancedIndirect) continue;
                        //if (isPlaying && vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.InstancedIndirect) continue;
                        //if (!vegetationItemInfoPro.EnableRuntimeSpawn) continue;

                        ShadowCastingMode shadowCastingMode =
                            VegetationSettings.GetShadowCastingMode(vegetationItemInfoPro.VegetationType);

                        if (vegetationItemInfoPro.DisableShadows)
                        {
                            shadowCastingMode = ShadowCastingMode.Off;
                        }

                        LayerMask layer = VegetationSettings.GetLayer(vegetationItemInfoPro.VegetationType);

                        VegetationItemModelInfo vegetationItemModelInfo =
                            VegetationPackageProModelsList[j].VegetationItemModelList[k];

                        NativeList<Matrix4x4> lod0MatrixList = VegetationStudioCameraList[i]
                            .VegetationStudioCameraRenderList[j].VegetationItemLOD0MatrixList[k];
                        NativeList<Matrix4x4> lod1MatrixList = VegetationStudioCameraList[i]
                            .VegetationStudioCameraRenderList[j].VegetationItemLOD1MatrixList[k];
                        NativeList<Matrix4x4> lod2MatrixList = VegetationStudioCameraList[i]
                            .VegetationStudioCameraRenderList[j].VegetationItemLOD2MatrixList[k];
                        NativeList<Matrix4x4> lod3MatrixList = VegetationStudioCameraList[i]
                            .VegetationStudioCameraRenderList[j].VegetationItemLOD3MatrixList[k];

                        NativeList<Vector4> lod0Fadeist = VegetationStudioCameraList[i]
                            .VegetationStudioCameraRenderList[j].VegetationItemLOD0LodFadeList[k];
                        NativeList<Vector4> lod1Fadeist = VegetationStudioCameraList[i]
                            .VegetationStudioCameraRenderList[j].VegetationItemLOD1LodFadeList[k];
                        NativeList<Vector4> lod2Fadeist = VegetationStudioCameraList[i]
                            .VegetationStudioCameraRenderList[j].VegetationItemLOD2LodFadeList[k];
                        NativeList<Vector4> lod3Fadeist = VegetationStudioCameraList[i]
                            .VegetationStudioCameraRenderList[j].VegetationItemLOD3LodFadeList[k];

                        if (vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.Normal)
                        {
                            RenderVegetationItemLODDrawMesh(lod0MatrixList, lod0Fadeist, vegetationItemModelInfo, 0,
                                targetCamera, i, shadowCastingMode, layer);
                            RenderVegetationItemLODDrawMesh(lod1MatrixList, lod1Fadeist, vegetationItemModelInfo, 1,
                                targetCamera, i, shadowCastingMode, layer);
                            RenderVegetationItemLODDrawMesh(lod2MatrixList, lod2Fadeist, vegetationItemModelInfo, 2,
                                targetCamera, i, shadowCastingMode, layer);
                            RenderVegetationItemLODDrawMesh(lod3MatrixList, lod3Fadeist, vegetationItemModelInfo, 3,
                                targetCamera, i, shadowCastingMode, layer);                           
                        }
                        else
                        {
                            RenderVegetationItemLOD(lod0MatrixList, lod0Fadeist, vegetationItemModelInfo, 0,
                                targetCamera, i, shadowCastingMode, layer);
                            RenderVegetationItemLOD(lod1MatrixList, lod1Fadeist, vegetationItemModelInfo, 1,
                                targetCamera, i, shadowCastingMode, layer);
                            RenderVegetationItemLOD(lod2MatrixList, lod2Fadeist, vegetationItemModelInfo, 2,
                                targetCamera, i, shadowCastingMode, layer);
                            RenderVegetationItemLOD(lod3MatrixList, lod3Fadeist, vegetationItemModelInfo, 3,
                                targetCamera, i, shadowCastingMode, layer);
                        }

                        if (shadowCastingMode == ShadowCastingMode.On)
                        {
                            NativeList<Matrix4x4> lod0ShadowMatrixList = VegetationStudioCameraList[i]
                                .VegetationStudioCameraRenderList[j].VegetationItemLOD0ShadowMatrixList[k];
                            NativeList<Matrix4x4> lod1ShadowMatrixList = VegetationStudioCameraList[i]
                                .VegetationStudioCameraRenderList[j].VegetationItemLOD1ShadowMatrixList[k];
                            NativeList<Matrix4x4> lod2ShadowMatrixList = VegetationStudioCameraList[i]
                                .VegetationStudioCameraRenderList[j].VegetationItemLOD2ShadowMatrixList[k];
                            NativeList<Matrix4x4> lod3ShadowMatrixList = VegetationStudioCameraList[i]
                                .VegetationStudioCameraRenderList[j].VegetationItemLOD3ShadowMatrixList[k];

                            if (vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.Normal)
                            {       
                                RenderVegetationItemLODDrawMesh(lod0ShadowMatrixList, lod0Fadeist, vegetationItemModelInfo, 0, targetCamera, i, ShadowCastingMode.ShadowsOnly, layer);
                                RenderVegetationItemLODDrawMesh(lod1ShadowMatrixList, lod1Fadeist, vegetationItemModelInfo, 1, targetCamera, i, ShadowCastingMode.ShadowsOnly, layer);
                                RenderVegetationItemLODDrawMesh(lod2ShadowMatrixList, lod2Fadeist, vegetationItemModelInfo, 2, targetCamera, i, ShadowCastingMode.ShadowsOnly, layer);
                                RenderVegetationItemLODDrawMesh(lod3ShadowMatrixList, lod3Fadeist, vegetationItemModelInfo, 3, targetCamera, i, ShadowCastingMode.ShadowsOnly, layer);

                            }
                            else
                            {
                                RenderVegetationItemLOD(lod0ShadowMatrixList, lod0Fadeist, vegetationItemModelInfo, 0, targetCamera, i, ShadowCastingMode.ShadowsOnly, layer);
                                RenderVegetationItemLOD(lod1ShadowMatrixList, lod1Fadeist, vegetationItemModelInfo, 1, targetCamera, i, ShadowCastingMode.ShadowsOnly, layer);
                                RenderVegetationItemLOD(lod2ShadowMatrixList, lod2Fadeist, vegetationItemModelInfo, 2, targetCamera, i, ShadowCastingMode.ShadowsOnly, layer);
                                RenderVegetationItemLOD(lod3ShadowMatrixList, lod3Fadeist, vegetationItemModelInfo, 3, targetCamera, i, ShadowCastingMode.ShadowsOnly, layer);                            
                            }
                        }
                    }
                }
            }

            Profiler.EndSample();
        }


        void SetupInstancedRenderMaterialPropertiesIDs()
        {
            _unityLODFadeID = Shader.PropertyToID("unity_LODFade");
        }

        
        //float[] _singleFloatArray = new float[1];
        void RenderVegetationItemLODDrawMesh(NativeList<Matrix4x4> matrixList, NativeList<Vector4> lodFadeList,
            VegetationItemModelInfo vegetationItemModelInfo, int lodIndex, Camera targetCamera, int cameraIndex,
            ShadowCastingMode shadowCastingMode, LayerMask layer)
        {            
            if (matrixList.Length == 0) return;
            
            Mesh mesh = vegetationItemModelInfo.GetLODMesh(lodIndex);
            Material[] materials = vegetationItemModelInfo.GetLODMaterials(lodIndex);
            MaterialPropertyBlock materialPropertyBlock = vegetationItemModelInfo.GetLODMaterialPropertyBlock(lodIndex);
            materialPropertyBlock.Clear();

            if (vegetationItemModelInfo.ShaderControler != null &&
                vegetationItemModelInfo.ShaderControler.Settings.SampleWind)
            {
                MeshRenderer meshRenderer = vegetationItemModelInfo.WindSamplerMeshRendererList[cameraIndex];
                if (meshRenderer)
                {
                    meshRenderer.GetPropertyBlock(materialPropertyBlock);
                }
            }
           
            for (int i = 0; i <= matrixList.Length - 1; i++)
            {                                                            
                int submeshesToRender = Mathf.Min(mesh.subMeshCount, materials.Length);                
                for (int m = 0; m <= submeshesToRender - 1; m++)
                {            
                    Graphics.DrawMesh(mesh, matrixList[i], materials[m], layer, targetCamera, m, materialPropertyBlock,
                        shadowCastingMode, true, null, LightProbeUsage.Off);                       
                }
            }
        }

        void RenderVegetationItemLOD(NativeList<Matrix4x4> matrixList, NativeList<Vector4> lodFadeList, VegetationItemModelInfo vegetationItemModelInfo, int lodIndex, Camera targetCamera, int cameraIndex, ShadowCastingMode shadowCastingMode, LayerMask layer)
        {          
            if (matrixList.Length == 0) return;

            int count = Mathf.CeilToInt(matrixList.Length / 1000f);
            int totalCount = matrixList.Length;
            for (int l = 0; l <= count - 1; l++)
            {
                int copyCount = 1000;
                if (totalCount < 1000) copyCount = totalCount;

                NativeSlice<Matrix4x4> matrixSlice = new NativeSlice<Matrix4x4>(matrixList, l * 1000, copyCount);          
                matrixSlice.CopyToFast(_renderArray);

                Mesh mesh = vegetationItemModelInfo.GetLODMesh(lodIndex);
                Material[] materials = vegetationItemModelInfo.GetLODMaterials(lodIndex);
                MaterialPropertyBlock materialPropertyBlock = vegetationItemModelInfo.GetLODMaterialPropertyBlock(lodIndex);
                materialPropertyBlock.Clear();

                if (vegetationItemModelInfo.ShaderControler != null &&
                    vegetationItemModelInfo.ShaderControler.Settings.SampleWind)
                {
                    MeshRenderer meshRenderer = vegetationItemModelInfo.WindSamplerMeshRendererList[cameraIndex];
                    if (meshRenderer)
                    {
                        meshRenderer.GetPropertyBlock(materialPropertyBlock);
                    }
                }

                //TODO get material property block from windsampler

                if (shadowCastingMode != ShadowCastingMode.ShadowsOnly)
                {
                    if ((vegetationItemModelInfo.LODFadePercentage || vegetationItemModelInfo.LODFadeCrossfade) && lodFadeList.Length == matrixList.Length)
                    {
                        NativeSlice<Vector4> lodFadeSlice = new NativeSlice<Vector4>(lodFadeList, l * 1000, copyCount);
                        lodFadeSlice.CopyToFast(_renderLodFadeArray);
                        materialPropertyBlock.SetVectorArray(_unityLODFadeID, _renderLodFadeArray);
                    }
                }

                int submeshesToRender = Mathf.Min(mesh.subMeshCount, materials.Length);                
                for (int m = 0; m <= submeshesToRender - 1; m++)
                {
                    Graphics.DrawMeshInstanced(mesh, m,
                        materials[m], _renderArray, copyCount,
                        materialPropertyBlock, shadowCastingMode, true, layer,
                        targetCamera,
                        LightProbeUsage.Off);
                }
                totalCount -= 1000;
            }
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
            VegetationStudioManager.UnregisterVegetationSystem(this);
            DisableEditorApi();
            DisposeVegetationStudioCameras();
            RemoveVegetationStudioCameraDelegates();
            DisposeVegetationCells();
            DisposeBillboardCells();
            ClearVegetationItemModels();
            DisposeComputeShaders();
            VegetationCellSpawner.Dispose();
            InitDone = false;
        }
    }
}