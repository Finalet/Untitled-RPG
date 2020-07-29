using System;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationStudio;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace AwesomeTechnologies.VegetationSystem
{
    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct UnityTerrainSampleConcaveJob : IJobParallelForDefer
#else
    public struct UnityTerrainSampleConcaveJob : IJobParallelFor
#endif
    {
        public NativeArray<byte> Excluded;
        public NativeArray<float3> Position;
        [ReadOnly] public NativeArray<float> InputHeights;

        public float Distance;
        public float MinHeightDifference;
        public bool Inverse;
        public bool Average;

        public int HeightmapWidth;
        public int HeightmapHeight;
        public Vector3 HeightMapScale;
        public Vector3 Size;
        public Vector3 TerrainPosition;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;

            Vector3 worldPosition = Position[index];
            Vector3 terrainSpacePositon = worldPosition - TerrainPosition;
            float2 heightmapPosition = new float2(terrainSpacePositon.x / HeightMapScale.x,
                terrainSpacePositon.z / HeightMapScale.z);

            int x = Mathf.RoundToInt(heightmapPosition.x);
            int z = Mathf.RoundToInt(heightmapPosition.y);

            int sampleDistance = Mathf.RoundToInt(Distance / HeightMapScale.x);
            float centerHeight = GetHeight(x, z);

            float height1 = GetHeight(x - sampleDistance, z - sampleDistance);
            float height2 = GetHeight(x, z - sampleDistance);
            float height3 = GetHeight(x + sampleDistance, z - sampleDistance);

            float height4 = GetHeight(x - sampleDistance, z);
            float height5 = GetHeight(x + sampleDistance, z);

            float height6 = GetHeight(x - sampleDistance, z + sampleDistance);
            float height7 = GetHeight(x, z + sampleDistance);
            float height8 = GetHeight(x + sampleDistance, z + sampleDistance);

            float edgeHeight;

            if (Average)
            {
                edgeHeight = (height1 + height2 + height3 + height4 + height5 + height6 + height7 + height8) / 8f;
            }
            else
            {
                edgeHeight = GetMinimumHeight(height1, height2, height3, height4, height5, height6, height7, height8);
            }

            bool remove = edgeHeight < centerHeight + MinHeightDifference;
            if (Inverse) remove = !remove;
            if (!remove) return;

            Excluded[index] = 1;
        }

        float GetMinimumHeight(float height1, float height2, float height3, float height4, float height5, float height6,
            float height7, float height8)
        {
            float minHeight = math.min(height1, height2);
            minHeight = math.min(minHeight, height3);
            minHeight = math.min(minHeight, height4);
            minHeight = math.min(minHeight, height5);
            minHeight = math.min(minHeight, height6);
            minHeight = math.min(minHeight, height7);
            minHeight = math.min(minHeight, height8);
            return minHeight;
        }

        float GetHeight(int x, int y)
        {
            x = math.clamp(x, 0, HeightmapWidth - 1);
            y = math.clamp(y, 0, HeightmapHeight - 1);
            return InputHeights[y * HeightmapWidth + x] * HeightMapScale.y;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct UnityTerranCellSampleJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> InputHeights;
        public NativeArray<Bounds> VegetationCellBoundsList;
        public int HeightmapWidth;

        public int HeightmapHeight;

        //public Vector3 Scale;
        public Vector3 HeightMapScale;

        public Rect TerrainRect;
        public Vector3 TerrainPosition;
        public float WorldspaceHeightCutoff;


        public void Execute(int index)
        {
            Bounds vegetationCellBounds = VegetationCellBoundsList[index];
            Rect cellRect = RectExtension.CreateRectFromBounds(vegetationCellBounds);
            if (!TerrainRect.Overlaps(cellRect)) return;

            float2 worldspaceCellCorner = new float2(vegetationCellBounds.center.x - vegetationCellBounds.extents.x,
                vegetationCellBounds.center.z - vegetationCellBounds.extents.z);
            float2 terrainspaceCellCorner = new float2(worldspaceCellCorner.x - TerrainPosition.x,
                worldspaceCellCorner.y - TerrainPosition.z);
            float2 heightmapPosition = new float2(terrainspaceCellCorner.x / HeightMapScale.x,
                terrainspaceCellCorner.y / HeightMapScale.z);

            int xCount = Mathf.CeilToInt(cellRect.width / HeightMapScale.x);
            int zCount = Mathf.CeilToInt(cellRect.height / HeightMapScale.z);

            int xStart = Mathf.FloorToInt(heightmapPosition.x);
            int zStart = Mathf.FloorToInt(heightmapPosition.y);

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int x = xStart; x <= xStart + xCount; x++)
            {
                for (int z = zStart; z <= zStart + zCount; z++)
                {
                    float heightSample = GetHeight(x, z);
                    if (heightSample < minHeight) minHeight = heightSample;
                    if (heightSample > maxHeight) maxHeight = heightSample;
                }
            }


            if (maxHeight + TerrainPosition.y < WorldspaceHeightCutoff) return;

            float centerY = (maxHeight + minHeight) / 2f;
            float height = maxHeight - minHeight;
            vegetationCellBounds =
                new Bounds(
                    new Vector3(vegetationCellBounds.center.x, centerY + TerrainPosition.y,
                        vegetationCellBounds.center.z),
                    new Vector3(vegetationCellBounds.size.x, height, vegetationCellBounds.size.z));
            VegetationCellBoundsList[index] = vegetationCellBounds;
        }

        float GetHeight(int x, int y)
        {
            x = math.clamp(x, 0, HeightmapWidth - 1);
            y = math.clamp(y, 0, HeightmapHeight - 1);
            return InputHeights[y * HeightmapWidth + x] * HeightMapScale.y;
        }
    }

//    [BurstCompile(CompileSynchronously = true)]
//    public struct UnityTerrainSampleJob : IJobParallelFor
//    {
//        [ReadOnly] public NativeArray<float> InputHeights;
//        [ReadOnly] public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;
//        public NativeArray<VegetationInstance> InstanceList;
//        public int HeightmapWidth;
//        public int HeightmapHeight;
//        public Vector3 Scale;
//        public Vector3 Size;
//        public Vector3 HeightMapScale;
//        public Vector3 TerrainPosition;
//        public byte TerrainSourceID;
//
//        public void Execute(int index)
//        {
//            VegetationInstance vegetationInstance = InstanceList[index];
//            if (vegetationInstance.HeightmapSampled == 1) return;     
//
//            VegetationSpawnLocationInstance spawnLocation = SpawnLocationList[index];
//            
//            if (spawnLocation.SpawnChance < 0)
//            {
//                vegetationInstance.Excluded = 1;
//                vegetationInstance.HeightmapSampled = 1;
//                InstanceList[index] = vegetationInstance;
//                return;
//            }
//
//            Vector3 worldPosition = spawnLocation.Position;
//            Vector3 terrainSpacePositon = worldPosition - TerrainPosition;
//            float2 interpolatedPosition = new float2(terrainSpacePositon.x / Size.x, terrainSpacePositon.z / Size.z);
//
//            if (interpolatedPosition.x < 0 || interpolatedPosition.x > 1 || interpolatedPosition.y < 0 ||
//                interpolatedPosition.y > 1)
//            {
//                {
//                    vegetationInstance.Excluded = 1;
//                    InstanceList[index] = vegetationInstance;
//                    return;
//                }
//            }
//
//            float height = GetTriangleInterpolatedHeight(interpolatedPosition.x, interpolatedPosition.y);
//            vegetationInstance.Position = new float3(spawnLocation.Position.x, height + TerrainPosition.y,
//                spawnLocation.Position.z);
//            vegetationInstance.TerrainNormal =
//                GetInterpolatedNormal(interpolatedPosition.x, interpolatedPosition.y);
//            vegetationInstance.Scale = new float3(1, 1, 1);
//            vegetationInstance.Rotation = Quaternion.Euler(0, 0, 0);
//            vegetationInstance.RandomNumberIndex = spawnLocation.RandomNumberIndex;
//            vegetationInstance.BiomeDistance = spawnLocation.BiomeDistance;
//            vegetationInstance.DistanceFalloff = 1;
//            vegetationInstance.TerrainSourceID = TerrainSourceID;
//            vegetationInstance.Excluded = 0;
//            vegetationInstance.HeightmapSampled = 1;
//            InstanceList[index] = vegetationInstance;
//        }
//
//        float GetTriangleInterpolatedHeight(float x, float y)
//        {
//            float fx = x * (HeightmapWidth - 1);
//            float fy = y * (HeightmapHeight - 1);
//            int lx = (int) fx;
//            int ly = (int) fy;
//
//            float u = fx - lx;
//            float v = fy - ly;
//            if (u > v)
//            {
//                float z00 = GetHeight(lx + 0, ly + 0);
//                float z01 = GetHeight(lx + 1, ly + 0);
//                float z11 = GetHeight(lx + 1, ly + 1);
//                return z00 + (z01 - z00) * u + (z11 - z01) * v;
//            }
//            else
//            {
//                float z00 = GetHeight(lx + 0, ly + 0);
//                float z10 = GetHeight(lx + 0, ly + 1);
//                float z11 = GetHeight(lx + 1, ly + 1);
//                return z00 + (z11 - z10) * u + (z10 - z00) * v;
//            }
//        }
//
//        float GetHeight(int x, int y)
//        {
//            x = math.clamp(x, 0, HeightmapWidth - 1);
//            y = math.clamp(y, 0, HeightmapHeight - 1);
//            return InputHeights[y * HeightmapWidth + x] * HeightMapScale.y;
//        }
//
//        public float3 GetInterpolatedNormal(float x, float y)
//        {
//            float fx = x * (HeightmapWidth - 1);
//            float fy = y * (HeightmapHeight - 1);
//            int lx = (int) fx;
//            int ly = (int) fy;
//
//            float3 n00 = CalculateNormalSobel(lx + 0, ly + 0);
//            float3 n10 = CalculateNormalSobel(lx + 1, ly + 0);
//            float3 n01 = CalculateNormalSobel(lx + 0, ly + 1);
//            float3 n11 = CalculateNormalSobel(lx + 1, ly + 1);
//
//            float u = fx - lx;
//            float v = fy - ly;
//
//            float3 s = math.lerp(n00, n10, u);
//            float3 t = math.lerp(n01, n11, u);
//            float3 value = math.lerp(s, t, v);
//            return math.normalize(value);
//        }
//
//        float3 CalculateNormalSobel(int x, int y)
//        {
//            float3 normal;
//            var dX = GetHeight(x - 1, y - 1) * -1.0F;
//            dX += GetHeight(x - 1, y) * -2.0F;
//            dX += GetHeight(x - 1, y + 1) * -1.0F;
//            dX += GetHeight(x + 1, y - 1) * 1.0F;
//            dX += GetHeight(x + 1, y) * 2.0F;
//            dX += GetHeight(x + 1, y + 1) * 1.0F;
//
//            dX /= Scale.x;
//
//            var dY = GetHeight(x - 1, y - 1) * -1.0F;
//            dY += GetHeight(x, y - 1) * -2.0F;
//            dY += GetHeight(x + 1, y - 1) * -1.0F;
//            dY += GetHeight(x - 1, y + 1) * 1.0F;
//            dY += GetHeight(x, y + 1) * 2.0F;
//            dY += GetHeight(x + 1, y + 1) * 1.0F;
//            dY /= Scale.z;
//
//            normal.x = -dX;
//            normal.y = 8;
//            normal.z = -dY;
//            return math.normalize(normal);
//        }
//    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct UnityTerrainSampleJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> InputHeights;
        [ReadOnly] public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;
        //public NativeArray<VegetationInstance> InstanceList;
        
        public NativeArray<float3> Position;
        public NativeArray<quaternion> Rotation;
        public NativeArray<float3> Scales;
        public NativeArray<float3> TerrainNormal;
        public NativeArray<float> BiomeDistance;
        public NativeArray<byte> TerrainTextureData;
        public NativeArray<int> RandomNumberIndex;
        public NativeArray<float> DistanceFalloff;
        public NativeArray<float> VegetationMaskDensity;
        public NativeArray<float> VegetationMaskScale;
        public NativeArray<byte> TerrainSourceIDs;        
        public NativeArray<byte> TextureMaskData;
        public NativeArray<byte> Excluded;
        public NativeArray<byte> HeightmapSampled;
        
        public int HeightmapWidth;
        public int HeightmapHeight;
        public Vector3 Scale;
        public Vector3 Size;
        public Vector3 HeightMapScale;
        public Vector3 TerrainPosition;
        public byte TerrainSourceID;

        public void Execute(int index)
        {
            if (HeightmapSampled[index] == 1) return;     

            VegetationSpawnLocationInstance spawnLocation = SpawnLocationList[index];
            
            if (spawnLocation.SpawnChance < 0)
            {
                Excluded[index] = 1;
                HeightmapSampled[index] = 1;
                return;
            }

            Vector3 worldPosition = spawnLocation.Position;
            Vector3 terrainSpacePositon = worldPosition - TerrainPosition;
            float2 interpolatedPosition = new float2(terrainSpacePositon.x / Size.x, terrainSpacePositon.z / Size.z);

            if (interpolatedPosition.x < 0 || interpolatedPosition.x > 1 || interpolatedPosition.y < 0 ||
                interpolatedPosition.y > 1)
            {
                {
                    Excluded[index] = 1;
                    return;
                }
            }

            float height = GetTriangleInterpolatedHeight(interpolatedPosition.x, interpolatedPosition.y);
            
            
            Position[index] = new float3(spawnLocation.Position.x, height + TerrainPosition.y,spawnLocation.Position.z);
            TerrainNormal[index] = GetInterpolatedNormal(interpolatedPosition.x, interpolatedPosition.y);
            Scales[index] = new float3(1, 1, 1);
            Rotation[index] = Quaternion.Euler(0, 0, 0);
            RandomNumberIndex[index] = spawnLocation.RandomNumberIndex;
            BiomeDistance[index] = spawnLocation.BiomeDistance;
            DistanceFalloff[index] = 1;
            TerrainSourceIDs[index] = TerrainSourceID;
            Excluded[index] = 0;
            HeightmapSampled[index] = 1;            
            TerrainTextureData[index] = 0;
            //TODO make sure we do not need to init these 2 here
            VegetationMaskDensity[index] = 0;
            VegetationMaskScale[index] = 0;
            TextureMaskData[index] = 0;    
        }

        float GetTriangleInterpolatedHeight(float x, float y)
        {
            float fx = x * (HeightmapWidth - 1);
            float fy = y * (HeightmapHeight - 1);
            int lx = (int) fx;
            int ly = (int) fy;

            float u = fx - lx;
            float v = fy - ly;
            if (u > v)
            {
                float z00 = GetHeight(lx + 0, ly + 0);
                float z01 = GetHeight(lx + 1, ly + 0);
                float z11 = GetHeight(lx + 1, ly + 1);
                return z00 + (z01 - z00) * u + (z11 - z01) * v;
            }
            else
            {
                float z00 = GetHeight(lx + 0, ly + 0);
                float z10 = GetHeight(lx + 0, ly + 1);
                float z11 = GetHeight(lx + 1, ly + 1);
                return z00 + (z11 - z10) * u + (z10 - z00) * v;
            }
        }

        float GetHeight(int x, int y)
        {
            x = math.clamp(x, 0, HeightmapWidth - 1);
            y = math.clamp(y, 0, HeightmapHeight - 1);
            return InputHeights[y * HeightmapWidth + x] * HeightMapScale.y;
        }

        public float3 GetInterpolatedNormal(float x, float y)
        {
            float fx = x * (HeightmapWidth - 1);
            float fy = y * (HeightmapHeight - 1);
            int lx = (int) fx;
            int ly = (int) fy;

            float3 n00 = CalculateNormalSobel(lx + 0, ly + 0);
            float3 n10 = CalculateNormalSobel(lx + 1, ly + 0);
            float3 n01 = CalculateNormalSobel(lx + 0, ly + 1);
            float3 n11 = CalculateNormalSobel(lx + 1, ly + 1);

            float u = fx - lx;
            float v = fy - ly;

            float3 s = math.lerp(n00, n10, u);
            float3 t = math.lerp(n01, n11, u);
            float3 value = math.lerp(s, t, v);
            return math.normalize(value);
        }

        float3 CalculateNormalSobel(int x, int y)
        {
            float3 normal;
            var dX = GetHeight(x - 1, y - 1) * -1.0F;
            dX += GetHeight(x - 1, y) * -2.0F;
            dX += GetHeight(x - 1, y + 1) * -1.0F;
            dX += GetHeight(x + 1, y - 1) * 1.0F;
            dX += GetHeight(x + 1, y) * 2.0F;
            dX += GetHeight(x + 1, y + 1) * 1.0F;

            dX /= Scale.x;

            var dY = GetHeight(x - 1, y - 1) * -1.0F;
            dY += GetHeight(x, y - 1) * -2.0F;
            dY += GetHeight(x + 1, y - 1) * -1.0F;
            dY += GetHeight(x - 1, y + 1) * 1.0F;
            dY += GetHeight(x, y + 1) * 2.0F;
            dY += GetHeight(x + 1, y + 1) * 1.0F;
            dY /= Scale.z;

            normal.x = -dX;
            normal.y = 8;
            normal.z = -dY;
            return math.normalize(normal);
        }
    }
    
    [BurstCompile]
    public struct AddToInstanceListJob : IJob
    {
        [DeallocateOnJobCompletion] public NativeArray<VegetationInstance> SourceInstanceArray;
        public NativeList<VegetationInstance> TargetInstanceList;

        public void Execute()
        {
            for (int i = 0; i <= SourceInstanceArray.Length - 1; i++)
            {
                VegetationInstance vegetationInstance = SourceInstanceArray[i];
                if (vegetationInstance.Excluded == 1) continue;
                TargetInstanceList.Add(vegetationInstance);
            }
        }
    }

    [AwesomeTechnologiesScriptOrder(-100)]
    [ExecuteInEditMode]
    public partial class UnityTerrain : MonoBehaviour, IVegetationStudioTerrain
    {
        public NativeArray<float> Heights;
        [FormerlySerializedAs("_terrain")] public Terrain Terrain;

        private int _heightmapHeight;
        private int _heightmapWidth;
        private Vector3 _size;
        private Vector3 _scale;
        private Vector3 _heightmapScale;

        private Rect _terrainRect;

        private readonly List<NativeArray<ARGBBytes>> _splatMapArrayList = new List<NativeArray<ARGBBytes>>();
        private readonly List<int> _splatMapFormatList = new List<int>();

#if !UNITY_2019_2_OR_NEWER    
        private Terrain.MaterialType _originalTerrainMaterialType = Terrain.MaterialType.BuiltInStandard;
#endif        
        private Material _originalTerrainMaterial;
        private float _originalTerrainheightmapPixelError;
        public bool TerrainMaterialOverridden;
        private bool _originalTerrainInstanced;
        private float _originalBasemapDistance;
        [NonSerialized] public Material TerrainHeatmapMaterial;

        public bool DisableTerrainTreesAndDetails = true;

        public bool AutoAddToVegegetationSystem;

        private bool _initDone;
        public TerrainSourceID TerrainSourceID;

        public Vector3 TerrainPosition = Vector3.zero;

        // ReSharper disable once UnusedMember.Local
        void Reset()
        {
            FindTerrain();

            TerrainPosition = transform.position;
        }

        void FindTerrain()
        {
            if (!Terrain)
            {
                Terrain = gameObject.GetComponent<Terrain>();
            }
        }

        // ReSharper disable once UnusedMember.Local
        void Awake()
        {
            FindTerrain();
        }

        void Start()
        {
            if (Terrain && DisableTerrainTreesAndDetails)
            {
                Terrain.drawTreesAndFoliage = false;
            }
        }

        //private ManagedNativeFloatArray _nativeManagedFloatArray;

        void LoadHeightData()
        {
            var terrainData = Terrain.terrainData;
            _heightmapScale = terrainData.heightmapScale;
            _heightmapHeight = terrainData.heightmapResolution;
            _heightmapWidth = terrainData.heightmapResolution;

            _size = terrainData.size;
            _scale.x = _size.x / (_heightmapWidth - 1);
            _scale.y = _size.y;
            _scale.z = _size.z / (_heightmapHeight - 1);

            Vector2 terrainCenter = new Vector2(TerrainPosition.x, TerrainPosition.z);
            Vector2 terrainSize = new Vector2(_size.x, _size.z);
            _terrainRect = new Rect(terrainCenter, terrainSize);

            float[,] hs = Terrain.terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);

            if (Heights.IsCreated) Heights.Dispose();

            Heights = new NativeArray<float>(_heightmapWidth * _heightmapHeight, Allocator.Persistent);
            Heights.CopyFromFast(hs);
        }

        public JobHandle SampleTerrain(NativeList<VegetationSpawnLocationInstance> spawnLocationList,
            VegetationInstanceData  instanceData, int sampleCount, Rect spawnRect, JobHandle dependsOn)
        {
            if (!_initDone) return dependsOn;
            
            
            if (spawnRect.Overlaps(_terrainRect))
            {
                UnityTerrainSampleJob unityTerrainSampleJob = new UnityTerrainSampleJob
                {
                    InputHeights = Heights,
                    
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    SpawnLocationList = spawnLocationList.AsDeferredJobArray(),
                    Position = instanceData.Position.AsDeferredJobArray(),
                    Rotation = instanceData.Rotation.AsDeferredJobArray(),
                    Scales = instanceData.Scale.AsDeferredJobArray(),
                    TerrainNormal= instanceData.TerrainNormal.AsDeferredJobArray(),
                    BiomeDistance= instanceData.BiomeDistance.AsDeferredJobArray(),
                    TerrainTextureData= instanceData.TerrainTextureData.AsDeferredJobArray(),
                    RandomNumberIndex= instanceData.RandomNumberIndex.AsDeferredJobArray(),
                    DistanceFalloff= instanceData.DistanceFalloff.AsDeferredJobArray(),
                    VegetationMaskDensity= instanceData.VegetationMaskDensity.AsDeferredJobArray(),
                    VegetationMaskScale= instanceData.VegetationMaskScale.AsDeferredJobArray(),
                    TerrainSourceIDs= instanceData.TerrainSourceID.AsDeferredJobArray(),
                    TextureMaskData= instanceData.TextureMaskData.AsDeferredJobArray(),
                    Excluded= instanceData.Excluded.AsDeferredJobArray(),
                    HeightmapSampled= instanceData.HeightmapSampled.AsDeferredJobArray(),  
#else
                    SpawnLocationList = spawnLocationList.ToDeferredJobArray(),
                    Position = instanceData.Position.ToDeferredJobArray(),
                    Rotation = instanceData.Rotation.ToDeferredJobArray(),
                    Scales = instanceData.Scale.ToDeferredJobArray(),
                    TerrainNormal= instanceData.TerrainNormal.ToDeferredJobArray(),
                    BiomeDistance= instanceData.BiomeDistance.ToDeferredJobArray(),
                    TerrainTextureData= instanceData.TerrainTextureData.ToDeferredJobArray(),
                    RandomNumberIndex= instanceData.RandomNumberIndex.ToDeferredJobArray(),
                    DistanceFalloff= instanceData.DistanceFalloff.ToDeferredJobArray(),
                    VegetationMaskDensity= instanceData.VegetationMaskDensity.ToDeferredJobArray(),
                    VegetationMaskScale= instanceData.VegetationMaskScale.ToDeferredJobArray(),
                    TerrainSourceIDs= instanceData.TerrainSourceID.ToDeferredJobArray(),
                    TextureMaskData= instanceData.TextureMaskData.ToDeferredJobArray(),
                    Excluded= instanceData.Excluded.ToDeferredJobArray(),
                    HeightmapSampled= instanceData.HeightmapSampled.ToDeferredJobArray(),        
#endif
                    

                    HeightMapScale = _heightmapScale,
                    HeightmapHeight = _heightmapHeight,
                    HeightmapWidth = _heightmapWidth,
                    TerrainPosition = TerrainPosition,
                    Scale = _scale,
                    Size = _size,
                    TerrainSourceID = (byte) TerrainSourceID
                };
                JobHandle handle = unityTerrainSampleJob.Schedule(sampleCount,64,dependsOn);
                return handle;
            }
            return dependsOn;
        }

        public void RefreshTerrainData()
        {
            LoadHeightData();
        }

        public void RefreshTerrainData(Bounds bounds)
        {
            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            Rect updateRect = RectExtension.CreateRectFromBounds(bounds);
            if (!updateRect.Overlaps(terrainRect))
            {
                LoadHeightData();
            }
        }

        public JobHandle SampleCellHeight(NativeArray<Bounds> vegetationCellBoundsList, float worldspaceHeightCutoff,
            Rect cellBoundsRect, JobHandle dependsOn = default(JobHandle))
        {
            if (!_initDone) return dependsOn;
            if (!Heights.IsCreated) LoadHeightData();

            if (cellBoundsRect.Overlaps(_terrainRect))
            {
                UnityTerranCellSampleJob unityTerranCellSampleJob = new UnityTerranCellSampleJob
                {
                    InputHeights = Heights,
                    VegetationCellBoundsList = vegetationCellBoundsList,
                    HeightMapScale = _heightmapScale,
                    HeightmapHeight = _heightmapHeight,
                    HeightmapWidth = _heightmapWidth,
                    TerrainPosition = TerrainPosition,
                    WorldspaceHeightCutoff = worldspaceHeightCutoff,
                    TerrainRect = RectExtension.CreateRectFromBounds(TerrainBounds)
                };

                JobHandle handle = unityTerranCellSampleJob.Schedule(vegetationCellBoundsList.Length, 32, dependsOn);
                return handle;
            }

            return dependsOn;
        }

        public JobHandle SampleConcaveLocation(VegetationInstanceData instanceData, float minHeightDifference,
            float distance, bool inverse, bool average, Rect spawnRect,
            JobHandle dependsOn)
        {
            if (!_initDone) return dependsOn;

            if (spawnRect.Overlaps(_terrainRect))
            {
                UnityTerrainSampleConcaveJob unityTerrainSampleConcaveLocationJob = new UnityTerrainSampleConcaveJob
                {
                    InputHeights = Heights,
                    
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    Excluded  = instanceData.Excluded.AsDeferredJobArray(),
                    Position = instanceData.Position.AsDeferredJobArray(),
#else
                    Excluded  = instanceData.Excluded.ToDeferredJobArray(),
                    Position = instanceData.Position.ToDeferredJobArray(),      
#endif
                    HeightMapScale = _heightmapScale,
                    HeightmapHeight = _heightmapHeight,
                    HeightmapWidth = _heightmapWidth,
                    TerrainPosition = TerrainPosition,
                    Size = _size,
                    Distance = distance,
                    MinHeightDifference = minHeightDifference,
                    Inverse = inverse,
                    Average = average
                };

                JobHandle handle = unityTerrainSampleConcaveLocationJob.Schedule(instanceData.Excluded, 64, dependsOn);
                return handle;
            }

            return dependsOn;
        }

        public void Init()
        {
            if (!Heights.IsCreated) LoadHeightData();
            //if (_nativeManagedFloatArray == null) LoadHeightData();
        }

        public void DisposeTemporaryMemory()
        {
        }

        public bool HasTerrainTextures()
        {
            return true;
        }

        public Texture2D GetTerrainTexture(int index)
        {
            if (!Terrain) return null;
            if (!Terrain.terrainData) return null;

#if UNITY_2018_3_OR_NEWER
            if (Terrain.terrainData.terrainLayers.Length > index)
            {
                if (Terrain.terrainData.terrainLayers[index])
                {
                    return Terrain.terrainData.terrainLayers[index].diffuseTexture;
                }
                else
                {
                    return null;
                }               
            }
 
#else
            if (Terrain.terrainData.splatPrototypes.Length > index)
            {
                return Terrain.terrainData.splatPrototypes[index].texture;
            }
#endif

            return null;
        }

#if UNITY_2018_3_OR_NEWER
        public TerrainLayer[] GetTerrainLayers()
        {
            if (!Terrain) return new TerrainLayer[0];
            return Terrain.terrainData.terrainLayers;
        }

        public void SetTerrainLayers(TerrainLayer[] terrainLayers)
        {
            if (Terrain)
            {                            
                Terrain.terrainData.terrainLayers = terrainLayers;
            }
        }
     
#else
        public SplatPrototype[] GetSplatPrototypes()
        {
            if (!Terrain) return new SplatPrototype[0];
            return Terrain.terrainData.splatPrototypes;
        }

        public void SetSplatPrototypes(SplatPrototype[] splatPrototypes)
        {
            if (Terrain)
            {
                Terrain.terrainData.splatPrototypes = splatPrototypes;
            }
        }
#endif

        //// ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            RefreshSplatMaps();

            _initDone = true;

            if (AutoAddToVegegetationSystem && Application.isPlaying)
            {
                AddTerrainToVegetationSystem();
            }
            else
            {
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
            }
        }

        public void AddTerrainToVegetationSystem()
        {
            VegetationStudioManager.AddTerrain(gameObject, false);
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

            Dispose();
        }

        public void RefreshTerrainArea()
        {
            VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
        }

        public void Dispose()
        {
            //_nativeManagedFloatArray?.Dispose();
            if (Heights.IsCreated)
            {
                Heights.Dispose();
            }

        }

        public string TerrainType => "Unity terrain";

        public Bounds TerrainBounds
        {
            get
            {
                if (Terrain)
                {
                    var terrainData = Terrain.terrainData;
                    return new Bounds(terrainData.bounds.center + TerrainPosition,
                        terrainData.bounds.size);
                }

                return new Bounds(Vector3.zero, Vector3.zero);
            }
        }

        // ReSharper disable once UnusedMember.Local
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(TerrainBounds.center, TerrainBounds.size);
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                TerrainPosition = transform.position;
            }
        }
    }
}