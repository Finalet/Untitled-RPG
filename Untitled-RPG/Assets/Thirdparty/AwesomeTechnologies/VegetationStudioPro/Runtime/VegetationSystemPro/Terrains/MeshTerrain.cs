using System.Collections.Generic;
using AwesomeTechnologies.Utility.BVHTree;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.MeshTerrains
{
    public class MeshSampleCell
    {
        public Bounds CellBounds;
        public MeshSampleCell(Rect rectangle)
        {
            CellBounds = RectExtension.CreateBoundsFromRect(rectangle, -100000);
        }
    }

    public struct BVHRay
    {
        public float3 Origin;
        public float3 Direction;
        public int DoRaycast;
    }

    public struct BVHRaycastContainer
    {
        public NativeArray<HitInfo> RaycastHits;
        public NativeList<HitInfo> RaycastHitList;
        public NativeArray<BVHRay> Rays;
        public NativeArray<HitInfo> TempHi;
    } 

    [System.Serializable]
    public struct MeshTerrainMeshSource
    {
        public MeshRenderer MeshRenderer;
        public TerrainSourceID TerrainSourceID;
        public MaterialPropertyBlock MaterialPropertyBlock;
    }

    [System.Serializable]
    public struct MeshTerrainTerrainSource
    {
        public Terrain Terrain;
        public TerrainSourceID TerrainSourceID;
        public MaterialPropertyBlock MaterialPropertyBlock;
    }

    [ExecuteInEditMode]
    public class MeshTerrain : MonoBehaviour, IVegetationStudioTerrain
    {
        List<ObjectData> _objects;
        public List<BVHTriangle> Tris;
        private List<BVHNode> _nodes;
        private List<BVHTriangle> _finalPrims;

        public int CurrentTabIndex;
        public MeshTerrainData MeshTerrainData;
        public List<MeshTerrainTerrainSource> MeshTerrainTerrainSourceList = new List<MeshTerrainTerrainSource>();
        public List<MeshTerrainMeshSource> MeshTerrainMeshSourceList = new List<MeshTerrainMeshSource>();
        public bool ShowDebugInfo;
        public bool NeedGeneration;
        private Material _debugMaterial;
        public bool MultiLevelRaycast;

        public bool AutoAddToVegegetationSystem;

        private NativeArray<LBVHNODE> _nativeNodes;
        private NativeArray<LBVHTriangle> _nativePrims;

        private bool _initDone;
        public bool Filterlods;

        public List<BVHRaycastContainer> RaycastContainerList = new List<BVHRaycastContainer>();

        public void GenerateMeshTerrain()
        {
            _objects = new List<ObjectData>();
            for (int i = 0; i <= MeshTerrainMeshSourceList.Count - 1; i++)
            {
                if (MeshTerrainMeshSourceList[i].MeshRenderer.GetComponent<MeshFilter>().sharedMesh == null) continue;
                ObjectData o = new ObjectData(MeshTerrainMeshSourceList[i].MeshRenderer, (int)MeshTerrainMeshSourceList[i].TerrainSourceID);
                if (o.IsValid) _objects.Add(o);
            }
            BVH.Build(ref _objects, out _nodes, out Tris, out _finalPrims);
            BVH.BuildLbvhData(_nodes, _finalPrims, out MeshTerrainData.lNodes, out MeshTerrainData.lPrims);
           
            MeshTerrainData.Bounds = CalculateTerrainBounds();

#if UNITY_EDITOR
            EditorUtility.SetDirty(MeshTerrainData);
#endif         
            //GenerateCellCoverage();
            CreateNativeArrays();

            VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
        }

        //void GenerateCellCoverage()
        //{

        //    List<MeshSampleCell> meshSampleCellList = new List<MeshSampleCell>();

        //    float sampleCellSize = 5;

        //    int cellXCount = Mathf.CeilToInt(TerrainBounds.size.x / sampleCellSize);
        //    int cellZCount = Mathf.CeilToInt(TerrainBounds.size.z / sampleCellSize);

        //    Vector2 corner = new Vector2(TerrainBounds.center.x - TerrainBounds.size.x / 2f,
        //        TerrainBounds.center.z - TerrainBounds.size.z / 2f);

        //    for (int x = 0; x <= cellXCount - 1; x++)
        //    {
        //        for (int z = 0; z <= cellZCount - 1; z++)
        //        {
        //            MeshSampleCell meshSampleCell = new MeshSampleCell(new Rect(
        //                new Vector2(sampleCellSize * x + corner.x, sampleCellSize * z + corner.y),
        //                new Vector2(sampleCellSize, sampleCellSize)));

        //            meshSampleCellList.Add(meshSampleCell);
        //        }
        //    }

        //    var sampleCellBoundsArray = new NativeArray<Bounds>(meshSampleCellList.Count,Allocator.Persistent);

        //    for (int i = 0; i <= meshSampleCellList.Count - 1; i++)
        //    {
        //        sampleCellBoundsArray[i] = meshSampleCellList[i].CellBounds;
        //    }

        //    NativeArray<LBVHNODE> tempNativeNodes = new NativeArray<LBVHNODE>(MeshTerrainData.lNodes.ToArray(), Allocator.Persistent);

        //    BVHTerrainCellSampleJob2 bvhTerranCellSampleJob = new BVHTerrainCellSampleJob2
        //    {
        //        VegetationCellBoundsList = sampleCellBoundsArray,
        //        Nodes = tempNativeNodes,
        //        TerrainRect = RectExtension.CreateRectFromBounds(TerrainBounds)
        //    };
        //    JobHandle sampleHandle = bvhTerranCellSampleJob.Schedule(meshSampleCellList.Count, 32);

        //    sampleHandle.Complete();

        //    MeshTerrainData.CoverageList.Clear();

        //    for (int i = 0; i <= sampleCellBoundsArray.Length - 1; i++)
        //    {
        //        if (float.IsNegativeInfinity(sampleCellBoundsArray[i].size.y))
        //        {
        //            MeshTerrainData.CoverageList.Add(0);
        //        }
        //        else
        //        {
        //            MeshTerrainData.CoverageList.Add(1);
        //        }
        //    }

        //    sampleCellBoundsArray.Dispose();
        //    tempNativeNodes.Dispose();
        //}

        Bounds CalculateTerrainBounds()
        {
            Bounds terrainBounds = new Bounds();
            for (int i = 0; i <= MeshTerrainMeshSourceList.Count - 1; i++)
            {
                if (i == 0)
                {
                    if (MeshTerrainMeshSourceList[i].MeshRenderer)
                        terrainBounds = MeshTerrainMeshSourceList[i].MeshRenderer.bounds;
                }
                else
                {
                    if (MeshTerrainMeshSourceList[i].MeshRenderer)
                        terrainBounds.Encapsulate(MeshTerrainMeshSourceList[i].MeshRenderer.bounds);
                }
            }
            return terrainBounds;
        }

        public void AddMeshRenderer(GameObject go, TerrainSourceID terrainSourceID)
        {
            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i <= meshRenderers.Length - 1; i++)
            {
                if (Filterlods)
                {
                    if (meshRenderers[i].name.ToUpper().Contains("LOD1")) continue;
                    if (meshRenderers[i].name.ToUpper().Contains("LOD2")) continue;
                    if (meshRenderers[i].name.ToUpper().Contains("LOD3")) continue;
                }

                var newMeshTerrainTerrainSource = new MeshTerrainMeshSource()
                {
                    MeshRenderer = meshRenderers[i],
                    TerrainSourceID = terrainSourceID,
                    MaterialPropertyBlock = new MaterialPropertyBlock()

                };
                MeshTerrainMeshSourceList.Add(newMeshTerrainTerrainSource);
            }
            NeedGeneration = true;
        }

        Color GetMeshTerrainSourceTypeColor(TerrainSourceID terrainSourceID)
        {
            switch (terrainSourceID)
            {
                case TerrainSourceID.TerrainSourceID1:
                    return Color.green;
                case TerrainSourceID.TerrainSourceID2:
                    return Color.red;
                case TerrainSourceID.TerrainSourceID3:
                    return Color.blue;
                case TerrainSourceID.TerrainSourceID4:
                    return Color.yellow;
                case TerrainSourceID.TerrainSourceID5:
                    return Color.grey;
                case TerrainSourceID.TerrainSourceID6:
                    return Color.magenta;
                case TerrainSourceID.TerrainSourceID7:
                    return Color.cyan;
                case TerrainSourceID.TerrainSourceID8:
                    return Color.white;
            }

            return Color.green;
        }

        public void AddTerrain(GameObject go, TerrainSourceID terrainSourceID)
        {
            Terrain[] terrains = go.GetComponentsInChildren<Terrain>();

            for (int i = 0; i <= terrains.Length - 1; i++)
            {
                var newMeshTerrainTerrainSource = new MeshTerrainTerrainSource
                {
                    Terrain = terrains[i],
                    TerrainSourceID = terrainSourceID,
                    MaterialPropertyBlock = new MaterialPropertyBlock()
                };

                MeshTerrainTerrainSourceList.Add(newMeshTerrainTerrainSource);
                NeedGeneration = true;
            }
        }

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            _debugMaterial = Resources.Load("MeshTerrainDebugMaterial", typeof(Material)) as Material;

            _initDone = true;


            if (AutoAddToVegegetationSystem && Application.isPlaying)
            {                
                VegetationStudioManager.AddTerrain(gameObject,false);
            }
            else
            {
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
            }
        }

        void CreateNativeArrays()
        {
            DisposeNativeArrays();

            if (MeshTerrainData == null) return;

            _nativeNodes = new NativeArray<LBVHNODE>(MeshTerrainData.lNodes.ToArray(), Allocator.Persistent);
            _nativePrims = new NativeArray<LBVHTriangle>(MeshTerrainData.lPrims.ToArray(), Allocator.Persistent);
        }

        void DisposeNativeArrays()
        {
            if (_nativeNodes.IsCreated) _nativeNodes.Dispose();
            if (_nativePrims.IsCreated) _nativePrims.Dispose();
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
            _initDone = false;

            if (AutoAddToVegegetationSystem && Application.isPlaying)
            {
                VegetationStudioManager.RemoveTerrain(gameObject);
            }
            else
            {
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
            }

            DisposeNativeArrays();
        }

        public bool HasMeshRenderer(MeshRenderer meshRenderer)
        {
            for (int i = 0; i <= MeshTerrainMeshSourceList.Count - 1; i++)
            {
                if (MeshTerrainMeshSourceList[i].MeshRenderer == meshRenderer) return true;
            }
            return false;
        }

        public bool HasTerrain(Terrain terrain)
        {
            for (int i = 0; i <= MeshTerrainTerrainSourceList.Count - 1; i++)
            {
                if (MeshTerrainTerrainSourceList[i].Terrain == terrain) return true;
            }
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        void Update()
        {
            DrawDebuginfo();
        }

        void DrawDebuginfo()
        {
            if (ShowDebugInfo)
            {
                for (int i = 0; i <= MeshTerrainMeshSourceList.Count - 1; i++)
                {
                    if (MeshTerrainMeshSourceList[i].MaterialPropertyBlock == null)
                    {
                        MeshTerrainMeshSource meshTerrainMeshSource = MeshTerrainMeshSourceList[i];
                        meshTerrainMeshSource.MaterialPropertyBlock = new MaterialPropertyBlock();
                        MeshTerrainMeshSourceList[i] = meshTerrainMeshSource;
                    }
                    DrawMeshRenderer(MeshTerrainMeshSourceList[i].MeshRenderer, MeshTerrainMeshSourceList[i].MaterialPropertyBlock, MeshTerrainMeshSourceList[i].TerrainSourceID);
                }
            }
        }

        void DrawMeshRenderer(MeshRenderer meshRenderer, MaterialPropertyBlock materialPropertyBlock, TerrainSourceID terrainSourceID)
        {
            if (!meshRenderer) return;
            MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
            if (!meshFilter) return;
            if (!meshFilter.sharedMesh) return;
            Matrix4x4 matrix = Matrix4x4.TRS(meshRenderer.transform.position, meshRenderer.transform.rotation, meshRenderer.transform.lossyScale);

            materialPropertyBlock.SetColor("_Color", GetMeshTerrainSourceTypeColor(terrainSourceID));

            for (int i = 0; i <= meshFilter.sharedMesh.subMeshCount - 1; i++)
            {
                Graphics.DrawMesh(meshFilter.sharedMesh, matrix, _debugMaterial, 0, null, i, materialPropertyBlock);
            }
        }

        public string TerrainType => "Mesh terrain";
        public Bounds TerrainBounds
        {
            get
            {
                if (MeshTerrainData)
                {
                    return MeshTerrainData.Bounds;
                }
                return new Bounds();
            }
        }

        public void RefreshTerrainData()
        {
            
        }

        public void RefreshTerrainData(Bounds bounds)
        {
            
        }

        public JobHandle SampleCellHeight(NativeArray<Bounds> vegetationCellBoundsList, float worldspaceHeightCutoff, Rect cellBoundsRect,
            JobHandle dependsOn = default(JobHandle))
        {
            if (!_initDone) return dependsOn;
            if (!_nativeNodes.IsCreated) CreateNativeArrays();
            if (!_nativeNodes.IsCreated) return dependsOn;


            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            if (!cellBoundsRect.Overlaps(terrainRect)) return dependsOn;

            //BVHTerrainCellSampleJob bvhTerranCellSampleJob = new BVHTerrainCellSampleJob
            //{
            //    VegetationCellBoundsList = vegetationCellBoundsList,
            //    TerrainMinHeight = TerrainBounds.center.y - TerrainBounds.extents.y,
            //    TerrainMaxHeight = TerrainBounds.center.y + TerrainBounds.extents.y,
            //    TerrainRect = RectExtension.CreateRectFromBounds(TerrainBounds)
            //};
            //dependsOn = bvhTerranCellSampleJob.Schedule(vegetationCellBoundsList.Length, 32, dependsOn);

            BVHTerrainCellSampleJob2 bvhTerranCellSampleJob = new BVHTerrainCellSampleJob2
            {
                VegetationCellBoundsList = vegetationCellBoundsList,
                Nodes = _nativeNodes,
                TerrainRect = RectExtension.CreateRectFromBounds(TerrainBounds)
            };
            dependsOn = bvhTerranCellSampleJob.Schedule(vegetationCellBoundsList.Length, 32, dependsOn);           

            return dependsOn;
        }

        public JobHandle SampleTerrain(NativeList<VegetationSpawnLocationInstance> spawnLocationList, VegetationInstanceData instanceData, int sampleCount, Rect spawnRect,
            JobHandle dependsOn)
        {
           
            if (!_initDone) return dependsOn;
            if (!_nativeNodes.IsCreated) CreateNativeArrays();

            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            if (!spawnRect.Overlaps(terrainRect)) return dependsOn;

            BVHRaycastContainer raycastContainer = new BVHRaycastContainer
            {
                Rays = new NativeArray<BVHRay>(sampleCount, Allocator.TempJob),
                RaycastHits = new NativeArray<HitInfo>(sampleCount, Allocator.TempJob),
                RaycastHitList = new NativeList<HitInfo>(sampleCount *2,Allocator.TempJob),
                TempHi = new NativeArray<HitInfo>(sampleCount, Allocator.TempJob)
            };
            RaycastContainerList.Add(raycastContainer);

            CreateBVHRaycastJob createBVHRaysJob =
                new CreateBVHRaycastJob
                {
                    
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    SpawnLocationList = spawnLocationList.AsDeferredJobArray(),
#else
                    SpawnLocationList = spawnLocationList.ToDeferredJobArray(),  
#endif
                    

                    Rays = raycastContainer.Rays,
                    TerrainRect = terrainRect
                };
            dependsOn = createBVHRaysJob.Schedule(dependsOn);


            if (MultiLevelRaycast)
            {
                SampleBVHTreeMultiLevelJob jobData = new SampleBVHTreeMultiLevelJob()
                {
                    Rays = raycastContainer.Rays,
                    HitInfos = raycastContainer.RaycastHitList,
                    NativeNodes = _nativeNodes,
                    NativePrims = _nativePrims,
                    TempHi = raycastContainer.TempHi
                };

                dependsOn = jobData.Schedule(dependsOn);

                UpdateBVHInstanceListMultiLevelJob updateInstanceListJob = new UpdateBVHInstanceListMultiLevelJob
                {
                    Position = instanceData.Position,
                    Rotation = instanceData.Rotation,
                    Scale = instanceData.Scale,
                    TerrainNormal= instanceData.TerrainNormal,
                    BiomeDistance= instanceData.BiomeDistance,
                    TerrainTextureData= instanceData.TerrainTextureData ,
                    RandomNumberIndex= instanceData.RandomNumberIndex,
                    DistanceFalloff= instanceData.DistanceFalloff,
                    VegetationMaskDensity= instanceData.VegetationMaskDensity,
                    VegetationMaskScale= instanceData.VegetationMaskScale,
                    TerrainSourceID= instanceData.TerrainSourceID,
                    TextureMaskData= instanceData.TextureMaskData,
                    Excluded= instanceData.Excluded,
                    HeightmapSampled = instanceData.HeightmapSampled,
                    
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    RaycastHits = raycastContainer.RaycastHitList.AsDeferredJobArray(),
#else
                    RaycastHits = raycastContainer.RaycastHitList.ToDeferredJobArray(),             
#endif
                    

                };
                dependsOn = updateInstanceListJob.Schedule(dependsOn);
            }
            else
            {
                SampleBVHTreeJob jobData = new SampleBVHTreeJob()
                {
                    Rays = raycastContainer.Rays,
                    HitInfos = raycastContainer.RaycastHits,
                    NativeNodes = _nativeNodes,
                    NativePrims = _nativePrims,
                    TempHi = raycastContainer.TempHi
                };
                              
                dependsOn = jobData.Schedule(sampleCount, 32, dependsOn);

                UpdateBVHInstanceListJob updateInstanceListJob = new UpdateBVHInstanceListJob
                {
                    Position = instanceData.Position,
                    Rotation = instanceData.Rotation,
                    Scale = instanceData.Scale,
                    TerrainNormal= instanceData.TerrainNormal,
                    BiomeDistance= instanceData.BiomeDistance,
                    TerrainTextureData= instanceData.TerrainTextureData ,
                    RandomNumberIndex= instanceData.RandomNumberIndex,
                    DistanceFalloff= instanceData.DistanceFalloff,
                    VegetationMaskDensity= instanceData.VegetationMaskDensity,
                    VegetationMaskScale= instanceData.VegetationMaskScale,
                    TerrainSourceID= instanceData.TerrainSourceID,
                    TextureMaskData= instanceData.TextureMaskData,
                    Excluded= instanceData.Excluded,
                    RaycastHits = raycastContainer.RaycastHits,
                    HeightmapSampled = instanceData.HeightmapSampled,
                    
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    SpawnLocationList = spawnLocationList.AsDeferredJobArray()
#else
                    SpawnLocationList = spawnLocationList.ToDeferredJobArray()          
#endif
                };

                dependsOn = updateInstanceListJob.Schedule(dependsOn);
            }           
            return dependsOn;
        }

        public bool NeedsSplatMapUpdate(Bounds updateBounds)
        {
            return false;
        }

        public void PrepareSplatmapGeneration(bool clearLockedTextures)
        {
            
        }

        public void GenerateSplatMapBiome(Bounds updateBounds, BiomeType biomeType, List<PolygonBiomeMask> polygonBiomeMaskList, List<TerrainTextureSettings> terrainTextureSettingsList, float heightCurveSampleHeight, float worldSpaceSeaLevel,bool clearLockedTextures)
        {
            
        }
        public void CompleteSplatmapGeneration()
        {
            
        }
        public JobHandle SampleConcaveLocation(VegetationInstanceData instanceData, float minHeightDifference, float distance, bool inverse, bool average,Rect spawnRect,
            JobHandle dependsOn)
        {
            if (!_initDone) return dependsOn;
            //TODO implement concave sampling for mesh terrain
            return dependsOn;
        }

        public void Init()
        {
            
        }

        public void DisposeTemporaryMemory()
        {
            for (int i = 0; i <= RaycastContainerList.Count - 1; i++)
            {
                RaycastContainerList[i].Rays.Dispose();
                RaycastContainerList[i].RaycastHits.Dispose();
                RaycastContainerList[i].RaycastHitList.Dispose();
                RaycastContainerList[i].TempHi.Dispose();
            }
            RaycastContainerList.Clear();
        }

        public void OverrideTerrainMaterial()
        {
           
        }

        public void RestoreTerrainMaterial()
        {
            
        }

        public void VerifySplatmapAccess()
        {
            
        }

        public void UpdateTerrainMaterial(float worldspaceSeaLevel, float worldspaceMaxTerrainHeight, TerrainTextureSettings terrainTextureSettings)
        {
            
        }

        public JobHandle ProcessSplatmapRules(List<TerrainTextureRule> terrainTextureRuleList, VegetationInstanceData instanceData,bool include, Rect cellRect,JobHandle dependsOn)
        {
            return dependsOn;
        }

        public bool HasTerrainTextures()
        {
            return false;
        }

        public Texture2D GetTerrainTexture(int index)
        {
            return null;
        }

#if UNITY_2018_3_OR_NEWER
        public TerrainLayer[] GetTerrainLayers()
        {
            return new TerrainLayer[0];
        }

        public void SetTerrainLayers(TerrainLayer[] terrainLayers)
        {
            
        }        
#else
        public SplatPrototype[] GetSplatPrototypes()
        {
            return new SplatPrototype[0];
        }

        public void SetSplatPrototypes(SplatPrototype[] splatPrototypes)
        {
            
        }
#endif

        // ReSharper disable once UnusedMember.Local
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(TerrainBounds.center, TerrainBounds.size);
        }
    }
}
