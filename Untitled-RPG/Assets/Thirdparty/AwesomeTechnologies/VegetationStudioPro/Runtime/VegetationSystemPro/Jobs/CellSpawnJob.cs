using System.Diagnostics.Contracts;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable RedundantNameQualifier

namespace AwesomeTechnologies.VegetationSystem
{
    [BurstCompile]
    public struct ClearInstanceMemoryJob : IJobParallelFor
    {
        public NativeArray<VegetationInstance> VegetationInstanceList;
        public void Execute(int index)
        {
            VegetationInstanceList[index] = new VegetationInstance();
        }
    }        
    
    [BurstCompile]
    public struct InitInstanceData : IJobParallelFor
    {
        public NativeArray<byte> HeightmapSampled;
        public NativeArray<byte> Excluded;
        public void Execute(int index)
        {
            HeightmapSampled[index] = 0;
            Excluded[index] = 1;
        }
    }   
    
    [BurstCompile]
#if UNITY_2019_1_OR_NEWER
    public struct FilterIncludeMaskJob : IJobParallelForDefer
#else
    public struct FilterIncludeMaskJob : IJobParallelFor 
#endif
    {
        public NativeArray<byte> Excluded;
        public NativeArray<byte> TextureMaskData;
        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            if (TextureMaskData[index] == 0)
            {
                Excluded[index] = 1;
            }
        }
    }

    public struct VegetationInstance
    {
        public float3 Position;             //12 bytes   ok
        public quaternion Rotation;         //16 bytes   ok
        public float3 Scale;                //12 bytes   ok
        public float3 TerrainNormal;        //12 bytes   ok
        public float BiomeDistance;         //4 bytes    ok
        public byte TerrainTextureData;     //1 bytes    ok
        public int RandomNumberIndex;       //4 bytes    ok
        public float DistanceFalloff;       //4 bytes    ok
        public float VegetationMaskDensity; //4 bytes    ok
        public float VegetationMaskScale;   //4 bytes    ok
        public byte TerrainSourceID;        //1 byte     ok
        public byte TextureMaskData;        //1 byte     ok        
        public byte Excluded;               //1 byte     ok
        public byte HeightmapSampled;       //1 byte     ok
    }

   
   

    public struct VegetationSpawnLocationInstance
    {
        public float3 Position;
        public float SpawnChance;
        public int RandomNumberIndex;
        public float BiomeDistance;
    }

//    public struct VegetationInstanceTerrainInfo
//    {
//        public float TerrainHeight;
//        public float3 TerrainNormal;
//    }

//    [BurstCompile(CompileSynchronously = true)]
//    public struct OffsetAndRotateScaleVegetationInstanceJob : IJobParallelFor
//    {
//        public NativeArray<VegetationInstance> VegetationInstanceList;
//        [ReadOnly] public NativeArray<float> RandomNumbers;
//        public VegetationRotationType VegetationRotationType;
//
//        public float MinScale;
//        public float MaxScale;
//
//        public Vector3 Offset;
//        public Vector3 RotationOffset;
//        public Vector3 ScaleMultiplier;
//
//        public void Execute(int index)
//        {                       
//            VegetationInstance vegetationInstance = VegetationInstanceList[index];
//            Vector3 lookAt;
//            switch (VegetationRotationType)
//            {
//                case VegetationRotationType.RotateY:
//                    vegetationInstance.Rotation = UnityEngine.Quaternion.Euler(new Vector3(0,
//                        RandomRange(vegetationInstance.RandomNumberIndex, 0, 365f), 0));
//                    vegetationInstance.RandomNumberIndex++;
//                    break;
//                case VegetationRotationType.RotateXYZ:
//                    vegetationInstance.Rotation = UnityEngine.Quaternion.Euler(new Vector3(
//                        RandomRange(vegetationInstance.RandomNumberIndex, 0, 365f),
//                        RandomRange(vegetationInstance.RandomNumberIndex + 1, 0, 365f),
//                        RandomRange(vegetationInstance.RandomNumberIndex + 2, 0, 365f)));
//                    vegetationInstance.RandomNumberIndex += 3;
//                    break;
//                case VegetationRotationType.FollowTerrain:
//                    lookAt = math.cross(-vegetationInstance.TerrainNormal, new Vector3(1, 0, 0));
//                    if (lookAt.y < 0) lookAt = -lookAt;
//                    vegetationInstance.Rotation =
//                        UnityEngine.Quaternion.LookRotation(lookAt, vegetationInstance.TerrainNormal);
//                    vegetationInstance.Rotation *= UnityEngine.Quaternion.AngleAxis(
//                        RandomRange(vegetationInstance.RandomNumberIndex, 0, 365f), new Vector3(0, 1, 0));
//                    vegetationInstance.RandomNumberIndex++;
//                    break;
//                case VegetationRotationType.FollowTerrainScale:
//                    lookAt = math.cross(-vegetationInstance.TerrainNormal, new Vector3(1, 0, 0));
//                    if (lookAt.y < 0) lookAt = -lookAt;
//                    vegetationInstance.Rotation =
//                        UnityEngine.Quaternion.LookRotation(lookAt, vegetationInstance.TerrainNormal);
//                    vegetationInstance.Rotation *= UnityEngine.Quaternion.AngleAxis(
//                        RandomRange(vegetationInstance.RandomNumberIndex, 0, 365f), new Vector3(0, 1, 0));
//                    vegetationInstance.RandomNumberIndex++;
//
//                    var slopeCos = math.dot(vegetationInstance.TerrainNormal, new float3(0, 1, 0));
//                    float slopeAngle = math.acos(slopeCos) * Mathf.Rad2Deg;
//
//                    float newScale = Mathf.Clamp01(slopeAngle / 45f);
//                    float3 angleScale = new Vector3(newScale, 0, newScale);
//                    vegetationInstance.Scale += angleScale;
//                    break;
//            }
//
//            float randomScale = RandomRange(vegetationInstance.RandomNumberIndex, MinScale, MaxScale);
//            vegetationInstance.RandomNumberIndex++;
//            float3 randomVectorScale = new float3(randomScale, randomScale, randomScale);
//            vegetationInstance.Scale *= randomVectorScale;
//            vegetationInstance.Scale *= ScaleMultiplier;
//
//            UnityEngine.Quaternion rotationOffset = UnityEngine.Quaternion.Euler(RotationOffset);
//            vegetationInstance.Rotation *= rotationOffset;
//
//            UnityEngine.Quaternion rotation = vegetationInstance.Rotation;
//            // ReSharper disable once RedundantCast
//            vegetationInstance.Position += (float3) (rotation * (Offset * vegetationInstance.Scale));
//            VegetationInstanceList[index] = vegetationInstance;                       
//        }
//       
//        public float RandomRange(int randomNumberIndex, float min, float max)
//        {
//            while (randomNumberIndex > 9999)
//                randomNumberIndex = randomNumberIndex - 10000;
//            return math.lerp(min, max, RandomNumbers[randomNumberIndex]);
//        }               
//    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct OffsetAndRotateScaleVegetationInstanceMathJob : IJobParallelForDefer
#else
    public struct OffsetAndRotateScaleVegetationInstanceMathJob : IJobParallelFor
#endif
    {
        public NativeArray<float3> Position;
        public NativeArray<int> RandomNumberIndex;
        public NativeArray<quaternion> Rotation;
        public NativeArray<float3> Scale;
        public NativeArray<float3> TerrainNormal;    
        public NativeArray<byte> Excluded;       
        
        [ReadOnly] public NativeArray<float> RandomNumbers;
        public VegetationRotationType VegetationRotationType;

        public float MinScale;
        public float MaxScale;

        public float3 Offset;
        public float3 RotationOffset;
        public float3 ScaleMultiplier;

        public float MinUpOffset;
        public float MaxUpOffset;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            Vector3 lookAt;
            float3 up = new float3(0,1,0);
            
            switch (VegetationRotationType)
            {
                case VegetationRotationType.RotateY:
                    Rotation[index] = quaternion.Euler(new float3(0,
                        RandomRange(RandomNumberIndex[index], 0, 6.28f), 0));
                    RandomNumberIndex[index]++;
                    break;
                case VegetationRotationType.RotateXYZ:
                    Rotation[index] = quaternion.Euler(new float3(
                        RandomRange(RandomNumberIndex[index], 0, 6.28f),
                        RandomRange(RandomNumberIndex[index] + 1, 0, 6.28f),
                        RandomRange(RandomNumberIndex[index] + 2, 0, 6.28f)));
                    RandomNumberIndex[index] += 3;
                    up = TerrainNormal[index];
                    break;
                case VegetationRotationType.FollowTerrain:
                    lookAt = math.cross(-TerrainNormal[index], new float3(1, 0, 0));
                    if (lookAt.y < 0) lookAt = -lookAt;
                    Rotation[index] = UnityEngine.Quaternion.LookRotation(lookAt, TerrainNormal[index]);
                    //vegetationInstance.Rotation = quaternion.LookRotation(lookAt, vegetationInstance.TerrainNormal);
                    Rotation[index] = math.mul(Rotation[index],  quaternion.AxisAngle(new float3(0, 1, 0),
                        RandomRange(RandomNumberIndex[index], 0, 365f)));
                    RandomNumberIndex[index]++;
                    up = TerrainNormal[index];
                    break;
                case VegetationRotationType.FollowTerrainScale:
                    lookAt = math.cross(-TerrainNormal[index], new float3(1, 0, 0));
                    if (lookAt.y < 0) lookAt = -lookAt;
                    Rotation[index] = UnityEngine.Quaternion.LookRotation(lookAt, TerrainNormal[index]);
                    //vegetationInstance.Rotation = quaternion.LookRotation(lookAt, vegetationInstance.TerrainNormal);
                    Rotation[index] = math.mul(Rotation[index] ,quaternion.AxisAngle(new float3(0, 1, 0),
                        RandomRange(RandomNumberIndex[index], 0, 365f)));
                    RandomNumberIndex[index]++;

                    var slopeCos = math.dot(TerrainNormal[index], new float3(0, 1, 0));
                    float slopeAngle = math.degrees(math.acos(slopeCos));

                    float newScale = math.clamp(slopeAngle / 45f, 0, 1);
                    float3 angleScale = new float3(newScale, 0, newScale);
                    Scale[index] += angleScale;
                    up = TerrainNormal[index];
                    break;
            }

            float randomScale = RandomRange(RandomNumberIndex[index], MinScale, MaxScale);
            RandomNumberIndex[index]++;
            float3 randomVectorScale = new float3(randomScale, randomScale, randomScale);
            Scale[index] *= randomVectorScale;
            Scale[index] *= ScaleMultiplier;

            quaternion rotationOffset = quaternion.Euler(math.radians(RotationOffset));
            Rotation[index] = math.mul(Rotation[index], rotationOffset);

            quaternion rotation = Rotation[index];

            float3 scaledOffset = Offset * Scale[index];
            Position[index] += math.mul(rotation, scaledOffset);

            float yScale = Scale[index].y;
            float upOffset = RandomRange(RandomNumberIndex[index], MinUpOffset * yScale , MaxUpOffset * yScale);
            RandomNumberIndex[index]++;
            Position[index] += up * upOffset;
        }

        public float RandomRange(int randomNumberIndex, float min, float max)
        {
            while (randomNumberIndex > 9999)
                randomNumberIndex = randomNumberIndex - 10000;
            return math.lerp(min, max, RandomNumbers[randomNumberIndex]);
        }               
    }

    [BurstCompile(CompileSynchronously = true)]
         public struct CreateInstanceMatrixJob : IJob
         {
             [ReadOnly] public NativeList<VegetationInstance> InstanceList;
             public NativeList<MatrixInstance> VegetationInstanceMatrixList;
     
             public void Execute()
             {
                 for (int i = 0; i < InstanceList.Length; i++)
                 {
                     VegetationInstance vegetationInstance = InstanceList[i];
                     if (vegetationInstance.Excluded == 1) continue;
     
                     MatrixInstance matrixInstance = new MatrixInstance
                     {
                         Matrix = Matrix4x4.TRS(InstanceList[i].Position,
                             InstanceList[i].Rotation, InstanceList[i].Scale),
                         DistanceFalloff = InstanceList[i].DistanceFalloff
                     };
                     VegetationInstanceMatrixList.Add(matrixInstance);
                 }
             }
         }

    [BurstCompile(CompileSynchronously = true)]
    public struct NewCreateInstanceMatrixJob : IJob
    {
        [ReadOnly]public NativeList<byte> Excluded;
        [ReadOnly]public NativeList<float3> Position;
        [ReadOnly]public NativeList<quaternion> Rotation;
        [ReadOnly]public NativeList<float3> Scale;
        [ReadOnly]public NativeList<float> DistanceFalloff;
        public NativeList<MatrixInstance> VegetationInstanceMatrixList;

        public void Execute()
        {
            for (int i = 0; i < Excluded.Length; i++)
            {
                if (Excluded[i] == 1) continue;

                MatrixInstance matrixInstance = new MatrixInstance
                {
                    Matrix = Matrix4x4.TRS(Position[i],
                        Rotation[i], Scale[i]),
                    DistanceFalloff = DistanceFalloff[i]
                };
                VegetationInstanceMatrixList.Add(matrixInstance);
            }
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct LoadPersistentStorageToMatrixJob : IJob
    {
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<PersistentVegetationItem> InstanceList;
        public NativeList<MatrixInstance> VegetationInstanceMatrixList;
        public Vector3 VegetationSystemPosition;

        public void Execute()
        {
            for (int i = 0; i < InstanceList.Length; i++)
            {
                MatrixInstance matrixInstance = new MatrixInstance
                {
                    Matrix = Matrix4x4.TRS(InstanceList[i].Position + VegetationSystemPosition,
                        InstanceList[i].Rotation, InstanceList[i].Scale),
                    DistanceFalloff = InstanceList[i].DistanceFalloff
                };

                matrixInstance.DistanceFalloff = InstanceList[i].DistanceFalloff;

                VegetationInstanceMatrixList.Add(matrixInstance);
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct LoadPersistentStorageToMatrixWideJob : IJobParallelForDefer
#else
    public struct LoadPersistentStorageToMatrixWideJob : IJobParallelFor
#endif
    {
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<PersistentVegetationItem> InstanceList;
        public NativeArray<MatrixInstance> VegetationInstanceMatrixList;
        public Vector3 VegetationSystemPosition;

        public void Execute(int index)
        {
            MatrixInstance matrixInstance = new MatrixInstance
            {
                Matrix = Matrix4x4.TRS(InstanceList[index].Position + VegetationSystemPosition,
                    InstanceList[index].Rotation, InstanceList[index].Scale),
                DistanceFalloff = InstanceList[index].DistanceFalloff
            };

            matrixInstance.DistanceFalloff = InstanceList[index].DistanceFalloff;
            VegetationInstanceMatrixList[index] = matrixInstance;
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct DistanceFalloffJob : IJobParallelForDefer
#else
    public struct DistanceFalloffJob : IJobParallelFor
#endif
    {
        public NativeArray<int> RandomNumberIndex;
        public NativeArray<float> DistanceFalloff;
        public NativeArray<byte> Excluded;
        [ReadOnly] public NativeArray<float> RandomNumbers;
        [ReadOnly] public float DistanceFalloffStartDistance;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            DistanceFalloff[index] =
                math.clamp(
                    DistanceFalloffStartDistance + RandomRange(RandomNumberIndex[index], 0,
                        1 - DistanceFalloffStartDistance), 0, 1);
            RandomNumberIndex[index]++;
        }

        public float RandomRange(int randomNumberIndex, float min, float max)
        {
            while (randomNumberIndex > 9999)
                randomNumberIndex = randomNumberIndex - 10000;

            return Mathf.Lerp(min, max, RandomNumbers[randomNumberIndex]);
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct PerlinNoiseCutoffJob : IJobParallelFor
    {
        public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;
        public float PerlinCutoff;
        public float PerlinScale;
        public bool InversePerlinMask;
        public float2 Offset;

        public void Execute(int index)
        {
            VegetationSpawnLocationInstance vegetationSpawnLocationInstance = SpawnLocationList[index];
            if (vegetationSpawnLocationInstance.SpawnChance < float.Epsilon) return;

            float perlin = noise.cnoise(new float2(
                (vegetationSpawnLocationInstance.Position.x + Offset.x) / PerlinScale,
                (vegetationSpawnLocationInstance.Position.z + Offset.y) / PerlinScale));
            perlin += 1f;
            perlin /= 2f;
            perlin = math.clamp(perlin, 0, 1);
            perlin = math.@select(perlin, 1 - perlin, InversePerlinMask);
            if (perlin <= PerlinCutoff)
            {
                vegetationSpawnLocationInstance.SpawnChance = 0;
            }

            SpawnLocationList[index] = vegetationSpawnLocationInstance;
        }
    }

//    [BurstCompile(CompileSynchronously = true)]
//    public struct FillInstanceListJob : IJob
//    {
//        public NativeList<VegetationInstance> InstanceList;
//        public int SampleCount;
//
//        public void Execute()
//        {
//            VegetationInstance vegetationInstance = new VegetationInstance {Excluded = 1};
//
//            for (int i = 0; i <= SampleCount - 1; i++)
//            {
//                InstanceList.Add(vegetationInstance);
//            }
//        }
//    }

    [BurstCompile(CompileSynchronously = true)]
    public struct PerlinNoiseDensityJob : IJobParallelFor
    {
        public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;
        public float PerlinScale;
        public bool InversePerlinMask;
        public float2 Offset;

        public void Execute(int index)
        {
            VegetationSpawnLocationInstance vegetationSpawnLocationInstance = SpawnLocationList[index];
            if (vegetationSpawnLocationInstance.SpawnChance > float.Epsilon)
            {
                float perlin = noise.cnoise(new float2(
                    (vegetationSpawnLocationInstance.Position.x + Offset.x) / PerlinScale,
                    (vegetationSpawnLocationInstance.Position.z + Offset.y) / PerlinScale));
                perlin += 1f;
                perlin /= 2f;
                perlin = math.clamp(perlin, 0, 1);
                perlin = math.@select(perlin, 1 - perlin, InversePerlinMask);
                vegetationSpawnLocationInstance.SpawnChance *= perlin;
                SpawnLocationList[index] = vegetationSpawnLocationInstance;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct PerlinNoiseScaleJob : IJobParallelForDefer
#else
    public struct PerlinNoiseScaleJob : IJobParallelFor
#endif
    {
        public NativeArray<byte> Excluded;
        public NativeArray<float3> Position;
        public NativeArray<float3> Scale;       
        public float PerlinScale;
        public bool InversePerlinMask;
        public float2 Offset;
        public float MinScale;
        public float MaxScale;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            float perlin = noise.cnoise(new float2(
                (Position[index].x + Offset.x) / PerlinScale,
                (Position[index].z + Offset.y) / PerlinScale));
            perlin += 1f;
            perlin /= 2f;
            perlin = math.clamp(perlin, 0, 1);
            perlin = math.@select(perlin, 1 - perlin, InversePerlinMask);

            float scaleMultiplier = math.lerp(MinScale, MaxScale, Mathf.Clamp(perlin, 0, 1));
            float3 scaleVector = new float3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
            Scale[index] *= scaleVector;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct TerrainSourceExcludeRuleJob : IJobParallelForDefer
#else
    public struct TerrainSourceExcludeRuleJob : IJobParallelFor
#endif
    {
        public NativeArray<byte> TerrainSourceID;        
        public NativeArray<byte> Excluded;
        
        public TerrainSourceRule TerrainSourceRule;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            if (TerrainSourceRule[TerrainSourceID[index]])
            {
                Excluded[index] = 1;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct TerrainSourceIncludeRuleJob : IJobParallelForDefer
#else
    public struct TerrainSourceIncludeRuleJob : IJobParallelFor
#endif
    {
        public NativeArray<byte> TerrainSourceID;        
        public NativeArray<byte> Excluded;
        public TerrainSourceRule TerrainSourceRule;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            if (!TerrainSourceRule[TerrainSourceID[index]])
            {
                Excluded[index] = 1;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct InstanceHeightRuleJob : IJobParallelForDefer
#else
    public struct InstanceHeightRuleJob : IJobParallelFor
#endif
    {
        public NativeArray<byte> Excluded;
        public NativeArray<float3> Position;
        public NativeArray<int> RandomNumberIndex;
        
        public float MinHeight;
        public float MaxHeight;
        [ReadOnly] public NativeArray<float> HeightRuleCurveArray;
        [ReadOnly] public NativeArray<float> RandomNumbers;
        public bool Advanced;
        public float MaxCurveHeight;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;

            if (Advanced)
            {
                float relativeHeight = Position[index].y - MinHeight;
                float heightNormalized = relativeHeight / MaxCurveHeight;
                float spawnChance = SampleCurveArray(heightNormalized);
                if (RandomCutoff(spawnChance, RandomNumberIndex[index]))
                {
                    Excluded[index] = 1;
                }

                RandomNumberIndex[index]++;
            }
            else
            {
                if (Position[index].y < MinHeight || Position[index].y > MaxHeight)
                {
                    Excluded[index] = 1;                    
                }
            }
        }

        private bool RandomCutoff(float value, int randomNumberIndex)
        {
            float randomNumber = RandomRange(randomNumberIndex, 0, 1);
            return !(value > randomNumber);
        }

        public float RandomRange(int randomNumberIndex, float min, float max)
        {
            while (randomNumberIndex > 9999)
                randomNumberIndex = randomNumberIndex - 10000;

            return Mathf.Lerp(min, max, RandomNumbers[randomNumberIndex]);
        }

        private float SampleCurveArray(float value)
        {
            if (HeightRuleCurveArray.Length == 0) return 0f;
            int index = Mathf.RoundToInt((value) * HeightRuleCurveArray.Length);
            index = Mathf.Clamp(index, 0, HeightRuleCurveArray.Length - 1);
            return HeightRuleCurveArray[index];
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct InstanceSteepnessRuleJob : IJobParallelForDefer
#else
    public struct InstanceSteepnessRuleJob : IJobParallelFor
#endif
    {       
        public NativeArray<byte> Excluded;
        public NativeArray<float3> TerrainNormal;
        public NativeArray<int> RandomNumberIndex;
        
        [ReadOnly] public NativeArray<float> SteepnessRuleCurveArray;
        [ReadOnly] public NativeArray<float> RandomNumbers;
        public bool Advanced;

        public float MinSteepness;
        public float MaxSteepness;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            var slopeCos = math.dot(TerrainNormal[index], new float3(0, 1, 0));
            float slopeAngle = math.acos(slopeCos) * Mathf.Rad2Deg;
            //TODO replace with math degrees

            if (Advanced)
            {
                float slopeNormalized = slopeAngle / 90;
                float spawnChance = SampleCurveArray(slopeNormalized);
                if (RandomCutoff(spawnChance, RandomNumberIndex[index]))
                {
                    Excluded[index] = 1;
                }

               RandomNumberIndex[index]++;
            }
            else
            {
                if (slopeAngle < MinSteepness || slopeAngle > MaxSteepness)
                {
                    Excluded[index] = 1;
                }
            }
        }

        private bool RandomCutoff(float value, int randomNumberIndex)
        {
            float randomNumber = RandomRange(randomNumberIndex, 0, 1);
            return !(value > randomNumber);
        }

        public float RandomRange(int randomNumberIndex, float min, float max)
        {
            while (randomNumberIndex > 9999)
                randomNumberIndex = randomNumberIndex - 10000;

            return Mathf.Lerp(min, max, RandomNumbers[randomNumberIndex]);
        }

        private float SampleCurveArray(float value)
        {
            if (SteepnessRuleCurveArray.Length == 0) return 0f;
            int index = Mathf.RoundToInt((value) * SteepnessRuleCurveArray.Length);
            index = Mathf.Clamp(index, 0, SteepnessRuleCurveArray.Length - 1);
            return SteepnessRuleCurveArray[index];
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct BiomeEdgeDistanceScaleRuleJob : IJobParallelForDefer
#else
    public struct BiomeEdgeDistanceScaleRuleJob : IJobParallelFor
#endif
    {
        public NativeArray<float3> Scale;
        public NativeArray<byte> Excluded;
        public NativeArray<float> BiomeDistance;
        
        public float MaxDistance;
        public float MinScale;
        public float MaxScale;
        public bool InverseScale;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            if (BiomeDistance[index] < MaxDistance)
            {
                float scaleMultiplier =
                    math.@select(math.lerp(MinScale, MaxScale, BiomeDistance[index] / MaxDistance),
                        math.lerp(MaxScale, MinScale, BiomeDistance[index] / MaxDistance),
                        InverseScale);
                Scale[index] *= new float3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct BiomeEdgeDistanceIncludeRuleJob : IJobParallelForDefer
#else
    public struct BiomeEdgeDistanceIncludeRuleJob : IJobParallelFor
#endif
    {
        public NativeArray<byte> Excluded;
        public NativeArray<float> BiomeDistance;
        public float MaxDistance;
        public bool Inverse;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            if (Inverse)
            {
                if (BiomeDistance[index] < MaxDistance)
                {
                    Excluded[index] = 1;
                }
            }
            else
            {
                if (BiomeDistance[index] > MaxDistance)
                {
                    Excluded[index] = 1;
                }
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct AddSpawnLocations : IJob
    {
        [WriteOnly] public NativeList<VegetationSpawnLocationInstance> SpawnLocations;
        public int SampleCount;

        public void Execute()
        {
            for (int i = 0; i <= SampleCount - 1; i++)
            {
                SpawnLocations.Add(new VegetationSpawnLocationInstance());
            }
        }
    }


    [BurstCompile(CompileSynchronously = true)]
    public struct CalculateCellSpawnLocationsWideJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<VegetationSpawnLocationInstance> SpawnLocations;
        [ReadOnly] public NativeArray<float> RandomNumbers;
        public Vector3 CellCorner;
        public Vector3 CellSize;
        public Rect CellRect;
        public int CellIndex;
        public float SampleDistance;
        public float Density;
        public float DefaultSpawnChance;
        public int Seed;
        public bool UseSamplePointOffset;
        public float SamplePointMinOffset;
        public float SamplePointMaxOffset;
        public bool RandomizePosition;
        public float CalculatedSampleDistance;
        public int XSamples;
        public int ZSamples;

        public void Execute(int index)
        {
            // ReSharper disable once RedundantCast
            int z = Mathf.FloorToInt((float) index / XSamples);
            int x = index - (z * XSamples);

            Vector3 samplePosition = new Vector3(CellCorner.x + x * CalculatedSampleDistance, CellCorner.y + 10,
                CellCorner.z + z * CalculatedSampleDistance);
            VegetationSpawnLocationInstance vegetationSpawnLocationInstance =
                new VegetationSpawnLocationInstance
                {
                    Position = samplePosition,
                    SpawnChance = DefaultSpawnChance,
                    BiomeDistance = 1000000f
                };

            int randomNumberIndex = x + z * ZSamples + CellIndex + Seed;
            while (randomNumberIndex > 9999)
                randomNumberIndex = randomNumberIndex - 10000;
            vegetationSpawnLocationInstance.RandomNumberIndex = randomNumberIndex;

            if (RandomizePosition)
            {
                float3 offset = GetRandomOffset(CalculatedSampleDistance / 2f,
                    vegetationSpawnLocationInstance.RandomNumberIndex);
                vegetationSpawnLocationInstance.RandomNumberIndex =
                    vegetationSpawnLocationInstance.RandomNumberIndex + 2;
                vegetationSpawnLocationInstance.Position += offset;
            }

            if (UseSamplePointOffset)
            {
                float randomOffset = RandomRange(vegetationSpawnLocationInstance.RandomNumberIndex,
                    SamplePointMinOffset, SamplePointMaxOffset);
                vegetationSpawnLocationInstance.RandomNumberIndex += 1;
                float randomRotation =
                    math.frac(SamplePointMinOffset) *
                    365;
                UnityEngine.Quaternion samplePointOffsetRotation = UnityEngine.Quaternion.Euler(0, randomRotation, 0);
                float3 samplePointOffset = samplePointOffsetRotation * new Vector3(randomOffset, 0, 0);
                vegetationSpawnLocationInstance.Position += samplePointOffset;
            }

            if (!CellRect.Contains(new Vector2(vegetationSpawnLocationInstance.Position.x,
                vegetationSpawnLocationInstance.Position.z)))
            {
                vegetationSpawnLocationInstance.SpawnChance = 0;
            }

            SpawnLocations[index] = vegetationSpawnLocationInstance;
        }

        float3 GetRandomOffset(float distance, int randomNumberIndex)
        {
            return new float3(RandomRange(randomNumberIndex, -distance, distance), 0,
                RandomRange(randomNumberIndex + 1, -distance, distance));
        }

        public float RandomRange(int randomNumberIndex, float min, float max)
        {
            while (randomNumberIndex > 9999)
                randomNumberIndex = randomNumberIndex - 10000;

            return Mathf.Lerp(min, max, RandomNumbers[randomNumberIndex]);
        }
    }
}