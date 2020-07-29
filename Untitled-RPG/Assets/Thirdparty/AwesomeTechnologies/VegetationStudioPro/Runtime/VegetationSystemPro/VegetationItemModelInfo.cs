using System;
using AwesomeTechnologies.Shaders;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace AwesomeTechnologies.VegetationSystem
{
    public class CameraComputeBuffers
    {
        public ComputeBuffer MergeBuffer;
        public ComputeBuffer VisibleBufferLOD0;
        public ComputeBuffer VisibleBufferLOD1;
        public ComputeBuffer VisibleBufferLOD2;
        public ComputeBuffer VisibleBufferLOD3;
        
        public ComputeBuffer ShadowBufferLOD0;
        public ComputeBuffer ShadowBufferLOD1;
        public ComputeBuffer ShadowBufferLOD2;
        public ComputeBuffer ShadowBufferLOD3;
                
        private readonly uint[] _args = { 0, 0, 0, 0, 0 };
        public readonly List<ComputeBuffer> ArgsBufferMergedLOD0List = new List<ComputeBuffer>();
        public readonly List<ComputeBuffer> ArgsBufferMergedLOD1List = new List<ComputeBuffer>();
        public readonly List<ComputeBuffer> ArgsBufferMergedLOD2List = new List<ComputeBuffer>();
        public readonly List<ComputeBuffer> ArgsBufferMergedLOD3List = new List<ComputeBuffer>();
        
        public readonly List<ComputeBuffer> ShadowArgsBufferMergedLOD0List = new List<ComputeBuffer>();
        public readonly List<ComputeBuffer> ShadowArgsBufferMergedLOD1List = new List<ComputeBuffer>();
        public readonly List<ComputeBuffer> ShadowArgsBufferMergedLOD2List = new List<ComputeBuffer>();
        public readonly List<ComputeBuffer> ShadowArgsBufferMergedLOD3List = new List<ComputeBuffer>();

        public CameraComputeBuffers(Mesh vegetationMeshLod0, Mesh vegetationMeshLod1, Mesh vegetationMeshLod2, Mesh vegetationMeshLod3)
        {
            MergeBuffer = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            MergeBuffer.SetCounterValue(0);

            VisibleBufferLOD0 = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            VisibleBufferLOD0.SetCounterValue(0);

            VisibleBufferLOD1 = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            VisibleBufferLOD1.SetCounterValue(0);

            VisibleBufferLOD2 = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            VisibleBufferLOD2.SetCounterValue(0);

            VisibleBufferLOD3 = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            VisibleBufferLOD3.SetCounterValue(0);
            
            ShadowBufferLOD0 = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            ShadowBufferLOD0.SetCounterValue(0);

            ShadowBufferLOD1 = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            ShadowBufferLOD1.SetCounterValue(0);

            ShadowBufferLOD2 = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            ShadowBufferLOD2.SetCounterValue(0);

            ShadowBufferLOD3 = new ComputeBuffer(5000, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            ShadowBufferLOD3.SetCounterValue(0);


            for (int i = 0; i <= vegetationMeshLod0.subMeshCount - 1; i++)
            {
                _args[0] = vegetationMeshLod0.GetIndexCount(i);
                _args[2] = vegetationMeshLod0.GetIndexStart(i);
                
                ComputeBuffer argsBufferMergedLod0 =
                    new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);                             
                argsBufferMergedLod0.SetData(_args);
                ArgsBufferMergedLOD0List.Add(argsBufferMergedLod0);
                
                ComputeBuffer shadowArgsBufferMergedLod0 =
                    new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);                             
                shadowArgsBufferMergedLod0.SetData(_args);
                ShadowArgsBufferMergedLOD0List.Add(shadowArgsBufferMergedLod0);
                
            }

            for (int i = 0; i <= vegetationMeshLod1.subMeshCount - 1; i++)
            {   
                _args[0] = vegetationMeshLod1.GetIndexCount(i);
                _args[2] = vegetationMeshLod1.GetIndexStart(i);
                
                ComputeBuffer argsBufferMergedLod1 =
                    new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);              
                argsBufferMergedLod1.SetData(_args);
                ArgsBufferMergedLOD1List.Add(argsBufferMergedLod1);
                
                ComputeBuffer shadowArgsBufferMergedLod1 =
                    new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);              
                shadowArgsBufferMergedLod1.SetData(_args);
                ShadowArgsBufferMergedLOD1List.Add(shadowArgsBufferMergedLod1);
            }

            for (int i = 0; i <= vegetationMeshLod2.subMeshCount - 1; i++)
            {
                _args[0] = vegetationMeshLod2.GetIndexCount(i);
                _args[2] = vegetationMeshLod2.GetIndexStart(i);
                
                ComputeBuffer argsBufferMergedLod2 =
                    new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);              
                argsBufferMergedLod2.SetData(_args);
                ArgsBufferMergedLOD2List.Add(argsBufferMergedLod2);
                
                ComputeBuffer shadowArgsBufferMergedLod2 =
                    new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);              
                shadowArgsBufferMergedLod2.SetData(_args);
                ShadowArgsBufferMergedLOD2List.Add(shadowArgsBufferMergedLod2);
            }

            for (int i = 0; i <= vegetationMeshLod3.subMeshCount - 1; i++)
            {
                _args[0] = vegetationMeshLod3.GetIndexCount(i);
                _args[2] = vegetationMeshLod3.GetIndexStart(i);
                
                ComputeBuffer argsBufferMergedLod3 =
                    new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);               
                argsBufferMergedLod3.SetData(_args);
                ArgsBufferMergedLOD3List.Add(argsBufferMergedLod3);
                
                ComputeBuffer shadowArgsBufferMergedLod3 =
                    new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);               
                shadowArgsBufferMergedLod3.SetData(_args);
                ShadowArgsBufferMergedLOD3List.Add(shadowArgsBufferMergedLod3);
            }
        }

        public void UpdateComputeBufferSize(int newInstanceCount)
        {
            MergeBuffer?.Release();
            MergeBuffer = null;

            VisibleBufferLOD0?.Release();
            VisibleBufferLOD0 = null;

            VisibleBufferLOD1?.Release();
            VisibleBufferLOD1 = null;

            VisibleBufferLOD2?.Release();
            VisibleBufferLOD2 = null;

            VisibleBufferLOD3?.Release();
            VisibleBufferLOD3 = null;
            
            ShadowBufferLOD0?.Release();
            ShadowBufferLOD0 = null;

            ShadowBufferLOD1?.Release();
            ShadowBufferLOD1 = null;

            ShadowBufferLOD2?.Release();
            ShadowBufferLOD2 = null;

            ShadowBufferLOD3?.Release();
            ShadowBufferLOD3 = null;

            MergeBuffer = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            MergeBuffer.SetCounterValue(0);

            VisibleBufferLOD0 = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            VisibleBufferLOD0.SetCounterValue(0);

            VisibleBufferLOD1 = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            VisibleBufferLOD1.SetCounterValue(0);

            VisibleBufferLOD2 = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            VisibleBufferLOD2.SetCounterValue(0);

            VisibleBufferLOD3 = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            VisibleBufferLOD3.SetCounterValue(0);
            
            ShadowBufferLOD0 = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            ShadowBufferLOD0.SetCounterValue(0);

            ShadowBufferLOD1 = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            ShadowBufferLOD1.SetCounterValue(0);

            ShadowBufferLOD2 = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            ShadowBufferLOD2.SetCounterValue(0);

            ShadowBufferLOD3 = new ComputeBuffer(newInstanceCount, (16 * 4 * 2) + 16, ComputeBufferType.Append);
            ShadowBufferLOD3.SetCounterValue(0);
        }

        public void DestroyComputeBuffers()
        {
            MergeBuffer?.Release();
            MergeBuffer = null;

            VisibleBufferLOD0?.Release();
            VisibleBufferLOD0 = null;

            VisibleBufferLOD1?.Release();
            VisibleBufferLOD1 = null;

            VisibleBufferLOD2?.Release();
            VisibleBufferLOD2 = null;

            VisibleBufferLOD3?.Release();
            VisibleBufferLOD3 = null;
            
            ShadowBufferLOD0?.Release();
            ShadowBufferLOD0 = null;

            ShadowBufferLOD1?.Release();
            ShadowBufferLOD1 = null;

            ShadowBufferLOD2?.Release();
            ShadowBufferLOD2 = null;

            ShadowBufferLOD3?.Release();
            ShadowBufferLOD3 = null;

            ReleaseArgsBuffers();
        }

        void ReleaseArgsBuffers()
        {
            for (var i = 0; i <= ArgsBufferMergedLOD0List.Count - 1; i++)
            {
                if (ArgsBufferMergedLOD0List[i] != null) ArgsBufferMergedLOD0List[i].Release();
            }

            for (var i = 0; i <= ArgsBufferMergedLOD1List.Count - 1; i++)
            {
                if (ArgsBufferMergedLOD1List[i] != null) ArgsBufferMergedLOD1List[i].Release();
            }

            for (var i = 0; i <= ArgsBufferMergedLOD2List.Count - 1; i++)
            {
                if (ArgsBufferMergedLOD2List[i] != null) ArgsBufferMergedLOD2List[i].Release();
            }

            for (var i = 0; i <= ArgsBufferMergedLOD3List.Count - 1; i++)
            {
                if (ArgsBufferMergedLOD3List[i] != null) ArgsBufferMergedLOD3List[i].Release();
            }                       
            
            for (var i = 0; i <= ShadowArgsBufferMergedLOD0List.Count - 1; i++)
            {
                if (ShadowArgsBufferMergedLOD0List[i] != null) ShadowArgsBufferMergedLOD0List[i].Release();
            }

            for (var i = 0; i <= ShadowArgsBufferMergedLOD1List.Count - 1; i++)
            {
                if (ShadowArgsBufferMergedLOD1List[i] != null) ShadowArgsBufferMergedLOD1List[i].Release();
            }

            for (var i = 0; i <= ShadowArgsBufferMergedLOD2List.Count - 1; i++)
            {
                if (ShadowArgsBufferMergedLOD2List[i] != null) ShadowArgsBufferMergedLOD2List[i].Release();
            }

            for (var i = 0; i <= ShadowArgsBufferMergedLOD3List.Count - 1; i++)
            {
                if (ShadowArgsBufferMergedLOD3List[i] != null) ShadowArgsBufferMergedLOD3List[i].Release();
            }

            ArgsBufferMergedLOD0List.Clear();
            ArgsBufferMergedLOD1List.Clear();
            ArgsBufferMergedLOD2List.Clear();
            ArgsBufferMergedLOD3List.Clear();
                        
            ShadowArgsBufferMergedLOD0List.Clear();
            ShadowArgsBufferMergedLOD1List.Clear();
            ShadowArgsBufferMergedLOD2List.Clear();
            ShadowArgsBufferMergedLOD3List.Clear();
        }

    }

    public class VegetationItemModelInfo
    {
        public GameObject VegetationModel;

        public Mesh VegetationMeshLod0;
        public Mesh VegetationMeshLod1;
        public Mesh VegetationMeshLod2;
        public Mesh VegetationMeshLod3;

        public float LOD1Distance;
        public float LOD2Distance;
        public float LOD3Distance;
        public int LODCount;

        public bool LODFadePercentage;
        public bool LODFadeCrossfade;

        public int DistanceBand;

        public Material[] VegetationMaterialsLOD0;
        public Material[] VegetationMaterialsLOD1;
        public Material[] VegetationMaterialsLOD2;
        public Material[] VegetationMaterialsLOD3;
        public MeshRenderer VegetationRendererLOD0;
        public MeshRenderer VegetationRendererLOD1;
        public MeshRenderer VegetationRendererLOD2;
        public MeshRenderer VegetationRendererLOD3;
        public MaterialPropertyBlock VegetationMaterialPropertyBlockLOD0;
        public MaterialPropertyBlock VegetationMaterialPropertyBlockLOD1;
        public MaterialPropertyBlock VegetationMaterialPropertyBlockLOD2;
        public MaterialPropertyBlock VegetationMaterialPropertyBlockLOD3;
        public MaterialPropertyBlock VegetationMaterialPropertyBlockShadowsLOD0;
        public MaterialPropertyBlock VegetationMaterialPropertyBlockShadowsLOD1;
        public MaterialPropertyBlock VegetationMaterialPropertyBlockShadowsLOD2;
        public MaterialPropertyBlock VegetationMaterialPropertyBlockShadowsLOD3;
        public VegetationItemInfoPro VegetationItemInfo;
        public EnvironmentSettings EnvironmentSettings;
        public float BoundingSphereRadius;

        public GameObject SelectedVegetationModelLOD0;
        public GameObject SelectedVegetationModelLOD1;
        public GameObject SelectedVegetationModelLOD2;
        public GameObject SelectedVegetationModelLOD3;
        
        public Material BillboardMaterial;


        //TODO update these automatic when new cameras is added or removed
        public List<MeshRenderer> WindSamplerMeshRendererList = new List<MeshRenderer>();
        public readonly List<CameraComputeBuffers> CameraComputeBufferList = new List<CameraComputeBuffers>();
        public readonly List<MaterialPropertyBlock> CameraBillboardMaterialPropertyBlockList = new List<MaterialPropertyBlock>();

        //public NativeArray<float> DistanceFalloffCurveArray;

        public NativeArray<float> HeightRuleCurveArray;
        public NativeArray<float> SteepnessRuleCurveArray;

        private float _maxVegetationSize;

        [NonSerialized]
        public IShaderController ShaderControler;

        public VegetationItemModelInfo(VegetationItemInfoPro vegetationItemInfo, EnvironmentSettings environmentSettings, List<GameObject> windSamplerList, int cameraCount)
        {
            EnvironmentSettings = environmentSettings;
            VegetationItemInfo = vegetationItemInfo;
            VegetationModel = vegetationItemInfo.VegetationPrefab;

            if (vegetationItemInfo.PrefabType == VegetationPrefabType.Texture)
            {
                VegetationModel = Resources.Load("DefaultGrassPatch") as GameObject;
            }

            if (VegetationModel == null)
            {
                VegetationModel = Resources.Load("MissingVegetationItemCube") as GameObject;
                Debug.LogError("The vegetation prefab of item: " + vegetationItemInfo.Name + " is missing. Please replace or delete VegetationItem.");
            }

            DistanceBand = vegetationItemInfo.GetDistanceBand();

#if UNITY_EDITOR
            MaterialUtility.EnableMaterialInstancing(VegetationModel);
#endif
            SelectedVegetationModelLOD0 = MeshUtils.SelectMeshObject(VegetationModel, LODLevel.LOD0);
            SelectedVegetationModelLOD1 = MeshUtils.SelectMeshObject(VegetationModel, LODLevel.LOD1);
            SelectedVegetationModelLOD2 = MeshUtils.SelectMeshObject(VegetationModel, LODLevel.LOD2);
            SelectedVegetationModelLOD3 = MeshUtils.SelectMeshObject(VegetationModel, LODLevel.LOD3);

            ShaderControler = ShaderSelector.GetShaderControler(vegetationItemInfo.ShaderName);
            if (ShaderControler != null) ShaderControler.Settings = vegetationItemInfo.ShaderControllerSettings;
            LODCount = MeshUtils.GetLODCount(VegetationModel, ShaderControler);

            CreateCameraWindSamplerItems(windSamplerList);

            if (ShaderControler != null)
            {
                LODFadePercentage = ShaderControler.Settings.LODFadePercentage;
                LODFadeCrossfade = ShaderControler.Settings.LODFadeCrossfade;
            }

            VegetationMeshLod0 = GetVegetationMesh(VegetationModel, LODLevel.LOD0);
            VegetationMeshLod1 = GetVegetationMesh(VegetationModel, LODLevel.LOD1);
            VegetationMeshLod2 = GetVegetationMesh(VegetationModel, LODLevel.LOD2);
            VegetationMeshLod3 = GetVegetationMesh(VegetationModel, LODLevel.LOD3);

            VegetationRendererLOD0 = SelectedVegetationModelLOD0.GetComponentInChildren<MeshRenderer>();
            VegetationMaterialsLOD0 = CreateMaterials(VegetationRendererLOD0.sharedMaterials, 0);

            VegetationRendererLOD1 = SelectedVegetationModelLOD1.GetComponentInChildren<MeshRenderer>();
            VegetationMaterialsLOD1 = CreateMaterials(VegetationRendererLOD1.sharedMaterials, 1);

            VegetationRendererLOD2 = SelectedVegetationModelLOD2.GetComponentInChildren<MeshRenderer>();
            VegetationMaterialsLOD2 = CreateMaterials(VegetationRendererLOD2.sharedMaterials, 2);

            VegetationRendererLOD3 = SelectedVegetationModelLOD3.GetComponentInChildren<MeshRenderer>();
            VegetationMaterialsLOD3 = CreateMaterials(VegetationRendererLOD3.sharedMaterials, 3);

            if (vegetationItemInfo.PrefabType == VegetationPrefabType.Texture)
            {
                SetGrassTexture(VegetationMaterialsLOD0, vegetationItemInfo.VegetationTexture);
                SetGrassTexture(VegetationMaterialsLOD1, vegetationItemInfo.VegetationTexture);
                SetGrassTexture(VegetationMaterialsLOD2, vegetationItemInfo.VegetationTexture);
                SetGrassTexture(VegetationMaterialsLOD3, vegetationItemInfo.VegetationTexture);
            }

            VegetationMaterialPropertyBlockLOD0 = new MaterialPropertyBlock();
            VegetationRendererLOD0.GetPropertyBlock(VegetationMaterialPropertyBlockLOD0);
            if (VegetationMaterialPropertyBlockLOD0 == null) VegetationMaterialPropertyBlockLOD0 = new MaterialPropertyBlock();

            VegetationMaterialPropertyBlockLOD1 = new MaterialPropertyBlock();
            VegetationRendererLOD1.GetPropertyBlock(VegetationMaterialPropertyBlockLOD1);
            if (VegetationMaterialPropertyBlockLOD1 == null) VegetationMaterialPropertyBlockLOD1 = new MaterialPropertyBlock();

            VegetationMaterialPropertyBlockLOD2 = new MaterialPropertyBlock();
            VegetationRendererLOD2.GetPropertyBlock(VegetationMaterialPropertyBlockLOD2);
            if (VegetationMaterialPropertyBlockLOD2 == null) VegetationMaterialPropertyBlockLOD2 = new MaterialPropertyBlock();

            VegetationMaterialPropertyBlockLOD3 = new MaterialPropertyBlock();
            VegetationRendererLOD3.GetPropertyBlock(VegetationMaterialPropertyBlockLOD3);
            if (VegetationMaterialPropertyBlockLOD3 == null) VegetationMaterialPropertyBlockLOD3 = new MaterialPropertyBlock();

            VegetationMaterialPropertyBlockShadowsLOD0 = new MaterialPropertyBlock();
            VegetationRendererLOD0.GetPropertyBlock(VegetationMaterialPropertyBlockShadowsLOD0);
            if (VegetationMaterialPropertyBlockShadowsLOD0 == null) VegetationMaterialPropertyBlockShadowsLOD0 = new MaterialPropertyBlock();

            VegetationMaterialPropertyBlockShadowsLOD1 = new MaterialPropertyBlock();
            VegetationRendererLOD1.GetPropertyBlock(VegetationMaterialPropertyBlockShadowsLOD1);
            if (VegetationMaterialPropertyBlockShadowsLOD1 == null) VegetationMaterialPropertyBlockShadowsLOD1 = new MaterialPropertyBlock();

            VegetationMaterialPropertyBlockShadowsLOD2 = new MaterialPropertyBlock();
            VegetationRendererLOD2.GetPropertyBlock(VegetationMaterialPropertyBlockShadowsLOD2);
            if (VegetationMaterialPropertyBlockShadowsLOD2 == null) VegetationMaterialPropertyBlockShadowsLOD2 = new MaterialPropertyBlock();

            VegetationMaterialPropertyBlockShadowsLOD3 = new MaterialPropertyBlock();
            VegetationRendererLOD3.GetPropertyBlock(VegetationMaterialPropertyBlockShadowsLOD3);
            if (VegetationMaterialPropertyBlockShadowsLOD3 == null) VegetationMaterialPropertyBlockShadowsLOD3 = new MaterialPropertyBlock();

            LOD1Distance = GetLODDistance(VegetationModel, 0);
            LOD2Distance = GetLODDistance(VegetationModel, 1);
            LOD3Distance = GetLODDistance(VegetationModel, 2);

            vegetationItemInfo.Bounds = MeshUtils.CalculateBoundsInstantiate(VegetationModel);
            
            float maxScaleMultiplier = Mathf.Max(new float[] {vegetationItemInfo.ScaleMultiplier.x,vegetationItemInfo.ScaleMultiplier.y,vegetationItemInfo.ScaleMultiplier.z});             
            BoundingSphereRadius = (vegetationItemInfo.Bounds.extents.magnitude * VegetationItemInfo.MaxScale * VegetationItemInfo.YScale * maxScaleMultiplier) + 5;
            
            

            CreateCameraBuffers(cameraCount);           

            HeightRuleCurveArray = new NativeArray<float>(4096, Allocator.Persistent);
            UpdateHeightRuleCurve();


            SteepnessRuleCurveArray = new NativeArray<float>(4096, Allocator.Persistent);
            UpdateSteepnessRuleCurve();

            //DistanceFalloffCurveArray = new NativeArray<float>(256, Allocator.Persistent);
            //UpdateDistanceFalloutCurve();

            if (vegetationItemInfo.VegetationType == VegetationType.Tree)
            {
                CreateBillboardMaterial();
            }
        }

        public void CreateCameraWindSamplerItems(List<GameObject> windSamplerList)
        {
            if (ShaderControler != null && ShaderControler.Settings.SampleWind)
            {
                for (int i = 0; i <= windSamplerList.Count - 1; i++)
                {
                    GameObject windSampleItem = Object.Instantiate(SelectedVegetationModelLOD0);
                    windSampleItem.hideFlags = HideFlags.HideAndDontSave;
                    windSampleItem.name = "VegetationSystemRenderer";
                    windSampleItem.transform.SetParent(windSamplerList[i].transform);
                    windSampleItem.transform.localPosition = new Vector3(0, 0, 3);
                    windSampleItem.transform.localRotation = Quaternion.identity;
                    CleanVegetationObject(windSampleItem);
                    MeshRenderer meshRenderer = windSampleItem.GetComponentInChildren<MeshRenderer>();
                    WindSamplerMeshRendererList.Add(meshRenderer);
                }
            }
        }
              
        public void CreateCameraBuffers(int cameraCount)
        {
            DisposeCameraBuffers();
            CameraBillboardMaterialPropertyBlockList.Clear();
            
            for (int i = 0; i <= cameraCount - 1; i++)
            {
                CameraComputeBuffers cameraComputeBuffers = new CameraComputeBuffers(VegetationMeshLod0, VegetationMeshLod1, VegetationMeshLod2, VegetationMeshLod3);
                CameraComputeBufferList.Add(cameraComputeBuffers);
                
                CameraBillboardMaterialPropertyBlockList.Add(new MaterialPropertyBlock());               
            }
        }
        
        public bool BillboardLODFadeCrossfade;

        void SetGrassTexture(Material[] materials, Texture2D texture)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                materials[i].SetTexture("_MainTex", texture);
            }
        }

        void UpdateBillboardMaterial()
        {
            if (!BillboardMaterial) return;

            BillboardMaterial.SetFloat("_Cutoff", VegetationItemInfo.BillboardCutoff);

            if (ShaderControler != null)
            {
                BillboardLODFadeCrossfade = ShaderControler.Settings.LODFadeCrossfade;
            }

            if (ShaderControler != null && ShaderControler.Settings.DynamicHUE)
            {
                Color hueColor = ShaderControler.Settings.GetColorPropertyValue("FoliageHue");
                BillboardMaterial.SetColor("_HueVariation", hueColor);
                BillboardMaterial.EnableKeyword("AT_HUE_VARIATION_ON");
            }
            else
            {
                BillboardMaterial.SetColor("_HueVariation", new Color(1f, 0.5f, 0f, 25f / 256f));
                BillboardMaterial.DisableKeyword("AT_HUE_VARIATION_ON");
            }
           
            BillboardMaterial.SetColor("_Color", VegetationItemInfo.BillboardTintColor);
            BillboardMaterial.SetFloat("_Brightness", VegetationItemInfo.BillboardBrightness);
            BillboardMaterial.SetFloat("_SnowAmount", Mathf.Clamp01(EnvironmentSettings.SnowAmount));
            BillboardMaterial.SetColor("_SnowColor", EnvironmentSettings.BillboardSnowColor);
            BillboardMaterial.SetFloat("_SnowBlendFactor", EnvironmentSettings.SnowBlendFactor);
            BillboardMaterial.SetFloat("_SnowBrightness", EnvironmentSettings.SnowBrightness);
            BillboardMaterial.SetFloat("_BillboardWindSpeed", VegetationItemInfo.BillboardWindSpeed);
            BillboardMaterial.SetFloat("_Smoothness", VegetationItemInfo.BillboardSmoothness);
            BillboardMaterial.SetFloat("_NormalStrength", VegetationItemInfo.BillboardNormalStrength);            

            if (VegetationItemInfo.BillboardRenderMode == BillboardRenderMode.Standard)
            {
                BillboardMaterial.SetFloat("_Metallic", VegetationItemInfo.BillboardMetallic);       
            }
            else
            {
                BillboardMaterial.SetFloat("_Specular", VegetationItemInfo.BillboardSpecular);       
            }
    
            BillboardMaterial.SetFloat("_Occlusion", VegetationItemInfo.BillboardOcclusion);       
        }

        void CreateBillboardMaterial()
        {
            if (VegetationItemInfo.BillboardRenderMode == BillboardRenderMode.Standard)
            {
                BillboardMaterial = new Material(Shader.Find("AwesomeTechnologies/Billboards/GroupBillboards")) {
                    enableInstancing = true, hideFlags = HideFlags.DontSave
                };
            }
            else
            {
                BillboardMaterial = new Material(Shader.Find("AwesomeTechnologies/Billboards/GroupBillboardsSpecular")) {
                    enableInstancing = true, hideFlags = HideFlags.DontSave
                };
            }
            
            BillboardMaterial.SetTexture("_MainTex", VegetationItemInfo.BillboardTexture);
            //BillboardMaterial.SetTexture("_AOTex", VegetationItemInfo.BillboardAoTexture);
            BillboardMaterial.SetTexture("_Bump", VegetationItemInfo.BillboardNormalTexture);
            BillboardMaterial.SetInt("_InRow",
                BillboardAtlasRenderer.GetBillboardQualityColumnCount(VegetationItemInfo
                    .BillboardQuality));
            BillboardMaterial.SetInt("_InCol",
                BillboardAtlasRenderer.GetBillboardQualityRowCount(VegetationItemInfo
                    .BillboardQuality));
            BillboardMaterial.SetInt("_CullDistance", 340);
            BillboardMaterial.SetInt("_FarCullDistance", 5000);

            if (Application.isPlaying)
            {
                BillboardMaterial.EnableKeyword("AT_CAMERA_SHADER");
                BillboardMaterial.DisableKeyword("AT_CAMERA_MATERIAL");
            }
            else
            {
                BillboardMaterial.DisableKeyword("AT_CAMERA_SHADER");
                BillboardMaterial.EnableKeyword("AT_CAMERA_MATERIAL");
            }

            if (ShaderControler != null && ShaderControler.Settings.DynamicHUE)
            {
                BillboardMaterial.EnableKeyword("AT_HUE_VARIATION_ON");
                BillboardMaterial.DisableKeyword("AT_HUE_VARIATION_OFF");
            }
            else
            {
                BillboardMaterial.DisableKeyword("AT_HUE_VARIATION_ON");
                BillboardMaterial.EnableKeyword("AT_HUE_VARIATION_OFF");
            }


            if (ShaderControler != null)
            {
                if (ShaderControler.Settings.BillboardSnow)
                {
                    BillboardMaterial.EnableKeyword("USE_SNOW");
                }
                else
                {
                    BillboardMaterial.DisableKeyword("USE_SNOW");
                }

                if (ShaderControler.Settings.BillboardHDWind)
                {
                    BillboardMaterial.EnableKeyword("USE_HDWIND");
                }
                else
                {
                    BillboardMaterial.DisableKeyword("USE_HDWIND");
                }
            }


            UpdateBillboardMaterial();
        }


        void DisposeCameraBuffers()
        {
            for (int i = 0; i <= CameraComputeBufferList.Count - 1; i++)
            {
                CameraComputeBufferList[i].DestroyComputeBuffers();
            }

            CameraComputeBufferList.Clear();
        }

        public void Dispose()
        {
            DestroyMaterials(VegetationMaterialsLOD0);
            DestroyMaterials(VegetationMaterialsLOD1);
            DestroyMaterials(VegetationMaterialsLOD2);
            DestroyMaterials(VegetationMaterialsLOD3);

            DisposeCameraBuffers();

            //if (DistanceFalloffCurveArray.IsCreated) DistanceFalloffCurveArray.Dispose();

            if (HeightRuleCurveArray.IsCreated) HeightRuleCurveArray.Dispose();
            if (SteepnessRuleCurveArray.IsCreated) SteepnessRuleCurveArray.Dispose();
        }

        //public void UpdateDistanceFalloutCurve()
        //{
        //    float[] curveArray = VegetationItemInfo.DistanceFallofffAnimationCurve.GenerateCurveArray();
        //    DistanceFalloffCurveArray.CopyFrom(curveArray);
        //}

        public void UpdateHeightRuleCurve()
        {
            float[] curveArray = VegetationItemInfo.HeightRuleCurve.GenerateCurveArray(4096);
            HeightRuleCurveArray.CopyFrom(curveArray);
        }

        public void UpdateSteepnessRuleCurve()
        {
            float[] curveArray = VegetationItemInfo.SteepnessRuleCurve.GenerateCurveArray(4096);
            SteepnessRuleCurveArray.CopyFrom(curveArray);
        }

        private static void DestroyMaterials(Material[] materials)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(materials[i]);
                }
                else
                {
                    Object.DestroyImmediate(materials[i]);
                }
            }
        }

        private static Mesh GetVegetationMesh(GameObject rootVegetationModel, LODLevel lodLevel)
        {
            GameObject selectedVegetationModel = MeshUtils.SelectMeshObject(rootVegetationModel, lodLevel);
            MeshFilter vegetationMeshFilter = selectedVegetationModel.GetComponentInChildren<MeshFilter>();

            if (vegetationMeshFilter.sharedMesh)
            {
                return vegetationMeshFilter.sharedMesh;
            }
            else
            {
                return new Mesh();
            }                       
        }

        private static float GetLODDistance(GameObject rootVegetationModel, int lodIndex)
        {
            LODGroup lodGroup = rootVegetationModel.GetComponentInChildren<LODGroup>();
            if (lodGroup)
            {
                LOD[] lods = lodGroup.GetLODs();
                if (lodIndex >= 0 && lodIndex < lods.Length)
                {
                    return (lodGroup.size / lods[lodIndex].screenRelativeTransitionHeight);
                }
            }
            return -1;
        }

        private Material[] CreateMaterials(Material[] sharedMaterials, int lodIndex)
        {
            Material[] materials = new Material[sharedMaterials.Length];
            for (int i = 0; i <= sharedMaterials.Length - 1; i++)
            {
                if (sharedMaterials[i])
                {
                    materials[i] = new Material(sharedMaterials[i]);
                    if (materials[i].shader.name == "Hidden/Nature/Tree Creator Leaves Optimized")
                    {
                        materials[i].shader = Shader.Find("Nature/Tree Creator Leaves");
                    }
                }
                else
                {
                    materials[i] = new Material(Shader.Find("Standard")) { enableInstancing = true };
                }

                RefreshMaterial(materials[i], lodIndex);
            }
            return materials;
        }

        private void RefreshMaterial(Material material, int lodIndex)
        {
            
            if (material.HasProperty("_CullFarStart"))
            {
                material.SetFloat("_CullFarStart", 100000f);
            }
            
            material.enableInstancing = true;
            if (LODFadePercentage)
            {
                if (lodIndex < LODCount - 1)
                {
                    material.DisableKeyword("LOD_FADE_CROSSFADE");
                    material.EnableKeyword("LOD_FADE_PERCENTAGE");
                }
            }

            if (LODFadeCrossfade)
            {
                if (lodIndex == LODCount - 1)
                {
                    material.EnableKeyword("LOD_FADE_CROSSFADE");
                    material.DisableKeyword("LOD_FADE_PERCENTAGE");
                }
            }

            if (!LODFadePercentage && !LODFadeCrossfade)
            {
                material.DisableKeyword("LOD_FADE_CROSSFADE");
                material.DisableKeyword("LOD_FADE_PERCENTAGE");
            }
           
            if (VegetationItemInfo.VegetationRenderMode == VegetationRenderMode.Normal)
            {
                material.DisableKeyword("LOD_FADE_CROSSFADE");
                material.DisableKeyword("LOD_FADE_PERCENTAGE");
            }                        

            
           // material.EnableKeyword("LOD_FADE_CROSSFADE");
           // material.DisableKeyword("LOD_FADE_PERCENTAGE");
            
            
            ShaderControler?.UpdateMaterial(material, EnvironmentSettings);
        }

        public void RefreshMaterials()
        {
            for (int i = 0; i <= VegetationMaterialsLOD0.Length - 1; i++)
            {
                RefreshMaterial(VegetationMaterialsLOD0[i], 0);
            }

            for (int i = 0; i <= VegetationMaterialsLOD1.Length - 1; i++)
            {
                RefreshMaterial(VegetationMaterialsLOD1[i], 1);
            }

            for (int i = 0; i <= VegetationMaterialsLOD2.Length - 1; i++)
            {
                RefreshMaterial(VegetationMaterialsLOD2[i], 2);
            }

            for (int i = 0; i <= VegetationMaterialsLOD3.Length - 1; i++)
            {
                RefreshMaterial(VegetationMaterialsLOD3[i], 3);
            }

            UpdateBillboardMaterial();
        }

        //public ComputeBuffer MergeBuffer;
        //public ComputeBuffer VisibleBufferLOD0;
        //public ComputeBuffer VisibleBufferLOD1;
        //public ComputeBuffer VisibleBufferLOD2;
        //public ComputeBuffer VisibleBufferLOD3;
        //private readonly uint[] _args = { 0, 0, 0, 0, 0 };
        //public List<ComputeBuffer> ArgsBufferMergedLOD0List = new List<ComputeBuffer>();
        //public List<ComputeBuffer> ArgsBufferMergedLOD1List = new List<ComputeBuffer>();
        //public List<ComputeBuffer> ArgsBufferMergedLOD2List = new List<ComputeBuffer>();
        //public List<ComputeBuffer> ArgsBufferMergedLOD3List = new List<ComputeBuffer>();

        public Mesh GetLODMesh(int lodIndex)
        {
            switch (lodIndex)
            {
                case 0:
                    return VegetationMeshLod0;
                case 1:
                    return VegetationMeshLod1;
                case 2:
                    return VegetationMeshLod2;
                case 3:
                    return VegetationMeshLod3;
            }

            return null;
        }

        public Material[] GetLODMaterials(int lodIndex)
        {
            switch (lodIndex)
            {
                case 0:
                    return VegetationMaterialsLOD0;
                case 1:
                    return VegetationMaterialsLOD1;
                case 2:
                    return VegetationMaterialsLOD2;
                case 3:
                    return VegetationMaterialsLOD3;
            }
            return null;
        }

        public MaterialPropertyBlock GetLODMaterialPropertyBlock(int lodIndex)
        {
            switch (lodIndex)
            {
                case 0:
                    return VegetationMaterialPropertyBlockLOD0;
                case 1:
                    return VegetationMaterialPropertyBlockLOD1;
                case 2:
                    return VegetationMaterialPropertyBlockLOD2;
                case 3:
                    return VegetationMaterialPropertyBlockLOD3;
            }
            return null;
        }

        public ComputeBuffer GetLODVisibleBuffer(int lodIndex, int cameraIndex, bool shadows)
        {

            if (shadows)
            {
                switch (lodIndex)
                {
                    case 0:
                        return CameraComputeBufferList[cameraIndex].ShadowBufferLOD0;
                    case 1:
                        return CameraComputeBufferList[cameraIndex].ShadowBufferLOD1;
                    case 2:
                        return CameraComputeBufferList[cameraIndex].ShadowBufferLOD2;
                    case 3:
                        return CameraComputeBufferList[cameraIndex].ShadowBufferLOD3;
                }
                return null;
            }
            else
            {
                switch (lodIndex)
                {
                    case 0:
                        return CameraComputeBufferList[cameraIndex].VisibleBufferLOD0;
                    case 1:
                        return CameraComputeBufferList[cameraIndex].VisibleBufferLOD1;
                    case 2:
                        return CameraComputeBufferList[cameraIndex].VisibleBufferLOD2;
                    case 3:
                        return CameraComputeBufferList[cameraIndex].VisibleBufferLOD3;
                }
                return null; 
            }                      
        }

        public List<ComputeBuffer> GetLODArgsBufferList(int lodIndex,int cameraIndex, bool shadows)
        {
            if (shadows)
            {
                switch (lodIndex)
                {
                    case 0:
                        return CameraComputeBufferList[cameraIndex].ShadowArgsBufferMergedLOD0List;
                    case 1:
                        return CameraComputeBufferList[cameraIndex].ShadowArgsBufferMergedLOD1List;
                    case 2:
                        return CameraComputeBufferList[cameraIndex].ShadowArgsBufferMergedLOD2List;
                    case 3:
                        return CameraComputeBufferList[cameraIndex].ShadowArgsBufferMergedLOD3List;
                }
                return null; 
            }
            else
            {
                switch (lodIndex)
                {
                    case 0:
                        return CameraComputeBufferList[cameraIndex].ArgsBufferMergedLOD0List;
                    case 1:
                        return CameraComputeBufferList[cameraIndex].ArgsBufferMergedLOD1List;
                    case 2:
                        return CameraComputeBufferList[cameraIndex].ArgsBufferMergedLOD2List;
                    case 3:
                        return CameraComputeBufferList[cameraIndex].ArgsBufferMergedLOD3List;
                }
                return null;
            }
        }

        void CleanVegetationObject(GameObject go)
        {
            Mesh emptyMesh = new Mesh { bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(2, 2, 2)) };

            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i <= meshFilters.Length - 1; i++)
            {
                meshFilters[i].sharedMesh = emptyMesh;
            }

            Rigidbody[] rigidbodies = go.GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i <= rigidbodies.Length - 1; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(rigidbodies[i]);
                }
                else
                {
                    Object.DestroyImmediate(rigidbodies[i]);
                }
            }

            Collider[] colliders = go.GetComponentsInChildren<Collider>();
            for (int i = 0; i <= colliders.Length - 1; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(colliders[i]);
                }
                else
                {
                    Object.DestroyImmediate(colliders[i]);
                }
            }


            BillboardRenderer[] billboardAtlasRenderers = go.GetComponentsInChildren<BillboardRenderer>();
            for (int i = 0; i <= billboardAtlasRenderers.Length - 1; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(billboardAtlasRenderers[i]);
                }
                else
                {
                    Object.DestroyImmediate(billboardAtlasRenderers[i]);
                }
            }

            NavMeshObstacle[] navMeshObstacles = go.GetComponentsInChildren<NavMeshObstacle>();
            for (int i = 0; i <= navMeshObstacles.Length - 1; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(navMeshObstacles[i]);
                }
                else
                {
                    Object.DestroyImmediate(navMeshObstacles[i]);
                }
            }

            Transform[] transforms = go.GetComponentsInChildren<Transform>();
            for (int i = 0; i <= transforms.Length - 1; i++)
            {
                if (transforms[i].name.Contains("Billboard"))
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(transforms[i].gameObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(transforms[i].gameObject);
                    }
                }
            }

            transforms = go.GetComponentsInChildren<Transform>();
            for (int i = 0; i <= transforms.Length - 1; i++)
            {
                if (transforms[i].name.Contains("CollisionObject"))
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(transforms[i].gameObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(transforms[i].gameObject);
                    }
                }
            }

            LODGroup[] lodgroups = go.GetComponentsInChildren<LODGroup>();
            for (int i = 0; i <= lodgroups.Length - 1; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(lodgroups[i]);
                }
                else
                {
                    Object.DestroyImmediate(lodgroups[i]);
                }
            }

            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i <= meshRenderers.Length - 1; i++)
            {
                meshRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
                meshRenderers[i].receiveShadows = false;
                meshRenderers[i].lightProbeUsage = LightProbeUsage.Off;

                if (meshRenderers[i].sharedMaterials.Length > 1)
                {
                    Material[] materials = new Material[1];
                    materials[0] = Resources.Load("WindSampler", typeof(Material)) as Material;//_windSamplerMaterial;//meshRenderers[i].sharedMaterials[0]; //_windSamplerMaterial;//
                    meshRenderers[i].sharedMaterials = materials;
                }
            }
        }
    }
}
