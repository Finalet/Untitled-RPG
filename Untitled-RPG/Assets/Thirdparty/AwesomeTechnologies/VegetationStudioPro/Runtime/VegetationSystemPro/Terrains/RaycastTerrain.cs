using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace AwesomeTechnologies.VegetationSystem
{
    public class RaycastContainers
    {
        public NativeArray<RaycastHit> RaycastHits;
        public NativeArray<RaycastCommand> RaycastCommands;
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct CreateRaycastCommandsJob : IJob
    {
        [ReadOnly] public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;
        public NativeArray<RaycastCommand> RaycastCommands;

        public int LayerMask;
        public int MaxHits;

        public float3 FloatingOriginOffset;

        public void Execute()
        {
            for (int i = 0; i <= SpawnLocationList.Length - 1; i++)
            {
                RaycastCommand raycastCommand = new RaycastCommand
                {
                    distance = 20000,
                    from = SpawnLocationList[i].Position + new float3(0, 10000, 0) + FloatingOriginOffset,
                    direction = new Vector3(0, -1, 0),
                    layerMask = LayerMask,
                    maxHits = MaxHits
                };
                RaycastCommands[i] = raycastCommand;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct CreateRaycastCommandsListJob : IJob
    {
        [ReadOnly] public NativeList<VegetationSpawnLocationInstance> SpawnLocationList;
        public NativeList<RaycastCommand> RaycastCommands;

        public int LayerMask;
        public int MaxHits;

        public void Execute()
        {
            for (int i = 0; i <= SpawnLocationList.Length - 1; i++)
            {
                RaycastCommand raycastCommand = new RaycastCommand
                {
                    distance = 20000,
                    from = SpawnLocationList[i].Position + new float3(0, 10000, 0),
                    direction = new Vector3(0, -1, 0),
                    layerMask = LayerMask,
                    maxHits = MaxHits
                };
                RaycastCommands.Add(raycastCommand);
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct UpdateInstanceListJob : IJob
    {
        [ReadOnly] public NativeArray<RaycastHit> RaycastHits;
        [ReadOnly] public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;
        public Rect TerrainRect;
        public float3 FloatingOriginOffset;
        public byte TerrainSourceID;

        public NativeList<float3> Position;
        public NativeList<quaternion> Rotation;
        public NativeList<float3> Scale;
        public NativeList<float3> TerrainNormal;
        public NativeList<float> BiomeDistance;
        public NativeList<byte> TerrainTextureData;
        public NativeList<int> RandomNumberIndex;
        public NativeList<float> DistanceFalloff;
        public NativeList<float> VegetationMaskDensity;
        public NativeList<float> VegetationMaskScale;
        public NativeList<byte> TerrainSourceIDs;
        public NativeList<byte> TextureMaskData;
        public NativeList<byte> Excluded;
        public NativeList<byte> HeightmapSampled;

        public void Execute()
        {
            for (int i = 0; i <= RaycastHits.Length - 1; i++)
            {
                if (SpawnLocationList[i].SpawnChance < 0) continue;

                RaycastHit raycastHit = RaycastHits[i];
                if (raycastHit.distance > 0)
                {
                    float3 position = raycastHit.point;
                    position -= FloatingOriginOffset;

                    Vector2 point2D = new Vector2(position.x, position.z);
                    if (TerrainRect.Contains(point2D))
                    {
                        Position.Add(position);
                        Rotation.Add(Quaternion.Euler(0, 45, 0));
                        Scale.Add(new float3(1, 1, 1));
                        TerrainNormal.Add(raycastHit.normal);
                        BiomeDistance.Add(SpawnLocationList[i].BiomeDistance);
                        TerrainTextureData.Add(0);
                        RandomNumberIndex.Add(SpawnLocationList[i].RandomNumberIndex);
                        DistanceFalloff.Add(1);
                        VegetationMaskDensity.Add(1);
                        VegetationMaskScale.Add(1);
                        TerrainSourceIDs.Add(TerrainSourceID);
                        TextureMaskData.Add(0);
                        Excluded.Add(0);
                        HeightmapSampled.Add(0);
                    }
                }
            }
        }
    }


    [BurstCompile(CompileSynchronously = true)]
    public struct RaycastTerranCellSampleJob : IJobParallelFor
    {
        public NativeArray<Bounds> VegetationCellBoundsList;
        public Rect TerrainRect;
        public float TerrainMinHeight;
        public float TerrainMaxHeight;

        public void Execute(int index)
        {
            Bounds vegetationCellBounds = VegetationCellBoundsList[index];
            Rect cellRect = RectExtension.CreateRectFromBounds(vegetationCellBounds);
            if (!TerrainRect.Overlaps(cellRect)) return;

            float minHeight;
            float maxHeight = vegetationCellBounds.center.y + vegetationCellBounds.extents.y;

            if (vegetationCellBounds.center.y < 99999)
            {
                minHeight = TerrainMinHeight;
            }
            else
            {
                minHeight = vegetationCellBounds.center.y - vegetationCellBounds.extents.y;
            }

            if (TerrainMinHeight < minHeight) minHeight = TerrainMinHeight;
            if (TerrainMaxHeight > maxHeight) maxHeight = TerrainMaxHeight;

            float centerY = (maxHeight + minHeight) / 2f;
            float height = maxHeight - minHeight;
            vegetationCellBounds =
                new Bounds(new Vector3(vegetationCellBounds.center.x, centerY, vegetationCellBounds.center.z),
                    new Vector3(vegetationCellBounds.size.x, height, vegetationCellBounds.size.z));
            VegetationCellBoundsList[index] = vegetationCellBounds;
        }
    }

    [ExecuteInEditMode]
    public class RaycastTerrain : MonoBehaviour, IVegetationStudioTerrain
    {
        public string TerrainType => "Raycast terrain";
        public Bounds RaycastTerrainBounds = new Bounds(Vector3.zero, new Vector3(100, 20, 100));

        public Bounds TerrainBounds =>
            new Bounds(RaycastTerrainBounds.center + TerrainPosition, RaycastTerrainBounds.size);

        public LayerMask RaycastLayerMask;
        public int MaxHits = 4;

        public List<RaycastContainers> RaycastContainerList = new List<RaycastContainers>();
        public ObjectPool<RaycastContainers> RaycastContainerPool = new ObjectPool<RaycastContainers>();

        public TerrainSourceID TerrainSourceID;

        public bool AutoAddToVegegetationSystem;

        //public bool ApplyFloatingOriginOffset = true;
        private bool _initDone;

        //private Vector3 _terrainPosition;
        public Vector3 TerrainPosition = Vector3.zero;

        // ReSharper disable once UnusedMember.Local
        void Reset()
        {
            RaycastTerrainBounds = new Bounds(Vector3.zero, new Vector3(100, 20, 100));
            TerrainPosition = transform.position;
        }

        public void RefreshTerrainData()
        {
        }

        public void RefreshTerrainData(Bounds bounds)
        {
        }

        public JobHandle SampleCellHeight(NativeArray<Bounds> vegetationCellBoundsList, float worldspaceHeightCutoff,
            Rect cellBoundsRect,
            JobHandle dependsOn = default(JobHandle))
        {
            if (!_initDone) return dependsOn;

            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            if (!cellBoundsRect.Overlaps(terrainRect)) return dependsOn;

            RaycastTerranCellSampleJob raycastTerranCellSampleJob = new RaycastTerranCellSampleJob
            {
                VegetationCellBoundsList = vegetationCellBoundsList,
                TerrainMinHeight = TerrainBounds.center.y - TerrainBounds.extents.y,
                TerrainMaxHeight = TerrainBounds.center.y + TerrainBounds.extents.y,
                TerrainRect = terrainRect
            };

            JobHandle handle = raycastTerranCellSampleJob.Schedule(vegetationCellBoundsList.Length, 32, dependsOn);
            return handle;
        }

        public JobHandle SampleTerrain(NativeList<VegetationSpawnLocationInstance> spawnLocationList,
            VegetationInstanceData instanceData, int sampleCount,
            Rect spawnRect, JobHandle dependsOn)
        {
            if (!_initDone) return dependsOn;

            Vector3 floatingOriginOffset = VegetationStudioManager.GetFloatingOriginOffset();
            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            if (!spawnRect.Overlaps(terrainRect)) return dependsOn;

            MaxHits = 1;

            RaycastContainers raycastContainers = RaycastContainerPool.Get();
            raycastContainers.RaycastCommands = new NativeArray<RaycastCommand>(sampleCount, Allocator.TempJob);
            raycastContainers.RaycastHits = new NativeArray<RaycastHit>(sampleCount * MaxHits, Allocator.TempJob);
            RaycastContainerList.Add(raycastContainers);

            CreateRaycastCommandsJob createRaycastCommandsJob =
                new CreateRaycastCommandsJob
                {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    SpawnLocationList = spawnLocationList.AsDeferredJobArray(),
#else
                    SpawnLocationList = spawnLocationList.ToDeferredJobArray(),        
#endif
                    

                    LayerMask = RaycastLayerMask,
                    MaxHits = MaxHits,
                    RaycastCommands = raycastContainers.RaycastCommands,
                    FloatingOriginOffset = floatingOriginOffset
                };
            dependsOn = createRaycastCommandsJob.Schedule(dependsOn);
            dependsOn = RaycastCommand.ScheduleBatch(raycastContainers.RaycastCommands, raycastContainers.RaycastHits,
                32, dependsOn);

            UpdateInstanceListJob updateInstanceListJob = new UpdateInstanceListJob
            {
                Position = instanceData.Position,
                Rotation = instanceData.Rotation,
                Scale = instanceData.Scale,
                TerrainNormal = instanceData.TerrainNormal,
                BiomeDistance = instanceData.BiomeDistance,
                TerrainTextureData = instanceData.TerrainTextureData,
                RandomNumberIndex = instanceData.RandomNumberIndex,
                DistanceFalloff = instanceData.DistanceFalloff,
                VegetationMaskDensity = instanceData.VegetationMaskDensity,
                VegetationMaskScale = instanceData.VegetationMaskScale,
                TerrainSourceIDs = instanceData.TerrainSourceID,
                TextureMaskData = instanceData.TextureMaskData,
                Excluded = instanceData.Excluded,
                RaycastHits = raycastContainers.RaycastHits,
                HeightmapSampled = instanceData.HeightmapSampled,
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                SpawnLocationList = spawnLocationList.AsDeferredJobArray(),
#else
                SpawnLocationList = spawnLocationList.ToDeferredJobArray(),     
#endif
                

                TerrainRect = terrainRect,
                FloatingOriginOffset = floatingOriginOffset,
                TerrainSourceID = (byte) TerrainSourceID
            };
            dependsOn = updateInstanceListJob.Schedule(dependsOn);
            return dependsOn;
        }

        public bool NeedsSplatMapUpdate(Bounds updateBounds)
        {
            return false;
        }

        public void PrepareSplatmapGeneration(bool clearLockedTextures)
        {
        }

        public void GenerateSplatMapBiome(Bounds updateBounds, BiomeType biomeType,
            List<PolygonBiomeMask> polygonBiomeMaskList, List<TerrainTextureSettings> terrainTextureSettingsList,
            float heightCurveSampleHeight, float worldSpaceSeaLevel, bool clearLockedTextures)
        {
        }

        public void CompleteSplatmapGeneration()
        {
        }

        public JobHandle SampleConcaveLocation(VegetationInstanceData instanceData, float minHeightDifference,
            float distance, bool inverse, bool average, Rect spawnRect,
            JobHandle dependsOn)
        {
            if (!_initDone) return dependsOn;
            //TODO implement concave sampling for raycast terrain
            return dependsOn;
        }

        public void Init()
        {
        }

        public void DisposeTemporaryMemory()
        {
            for (int i = 0; i <= RaycastContainerList.Count - 1; i++)
            {
                if (RaycastContainerList[i].RaycastCommands.IsCreated)
                {
                    RaycastContainerList[i].RaycastCommands.Dispose();
                }

                RaycastContainerList[i].RaycastHits.Dispose();
                RaycastContainerPool.Release(RaycastContainerList[i]);
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

        public void UpdateTerrainMaterial(float worldspaceSeaLevel, float worldspaceMaxTerrainHeight,
            TerrainTextureSettings terrainTextureSettings)
        {
        }

        public JobHandle ProcessSplatmapRules(List<TerrainTextureRule> terrainTextureRuleList,
            VegetationInstanceData instanceData, bool include, Rect cellRect, JobHandle dependsOn)
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
            Gizmos.DrawWireCube(
                RaycastTerrainBounds.center + TerrainPosition + VegetationStudioManager.GetFloatingOriginOffset(),
                RaycastTerrainBounds.size);
        }

        // ReSharper disable once UnusedMember.Local
        void Update()
        {
            if (!Application.isPlaying)
            {
                TerrainPosition = transform.position;
            }

            //if (!Application.isPlaying && _terrainPosition != transform.position)
            //{
            //    SetTerrainPosition();
            //}
        }


        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            _initDone = true;

            //SetTerrainPosition();

            if (AutoAddToVegegetationSystem && Application.isPlaying)
            {
                VegetationStudioManager.AddTerrain(gameObject, false);
            }
            else
            {
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
            }
        }

        //void SetTerrainPosition()
        //{
        //    if (ApplyFloatingOriginOffset)
        //    {
        //        _terrainPosition = transform.position - VegetationStudioManager.GetFloatingOriginOffset();
        //    }
        //    else
        //    {
        //        _terrainPosition = transform.position;
        //    }
        //}

        public void RefreshTerrain()
        {
            VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
            VegetationStudioManager.ClearCache(TerrainBounds);
        }

        public void RefreshTerrain(Bounds bounds)
        {
            VegetationStudioManager.RefreshTerrainArea(bounds);
            VegetationStudioManager.ClearCache(bounds);
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
        }
    }
}