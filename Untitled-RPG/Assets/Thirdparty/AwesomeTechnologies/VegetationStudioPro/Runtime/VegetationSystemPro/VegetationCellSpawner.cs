using System.Collections.Generic;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace AwesomeTechnologies.VegetationSystem
{
    [BurstCompile(CompileSynchronously = true)]
    public struct FilterSpawnLocationsChanceJob : IJobParallelFor
    {
        public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;
        [ReadOnly] public NativeArray<float> RandomNumbers;

        public float Density;

        public float RandomRange(int randomNumberIndex, float min, float max)
        {
            while (randomNumberIndex > 9999)
                randomNumberIndex = randomNumberIndex - 10000;

            return Mathf.Lerp(min, max, RandomNumbers[randomNumberIndex]);
        }

        private bool RandomCutoff(float value, int randomNumberIndex)
        {
            var randomNumber = RandomRange(randomNumberIndex, 0, 1);
            return !(value > randomNumber);
        }

        public void Execute(int index)
        {
            VegetationSpawnLocationInstance vegetationSpawnLocationInstance = SpawnLocationList[index];
            if (RandomCutoff(vegetationSpawnLocationInstance.SpawnChance * Density,
                vegetationSpawnLocationInstance.RandomNumberIndex))
            {
                vegetationSpawnLocationInstance.RandomNumberIndex++;
                vegetationSpawnLocationInstance.SpawnChance = -1;
                SpawnLocationList[index] = vegetationSpawnLocationInstance;
            }
            else
            {
                vegetationSpawnLocationInstance.RandomNumberIndex++;
                SpawnLocationList[index] = vegetationSpawnLocationInstance;
            }
        }
    }

    public class VegetationCellSpawner
    {
        public NativeList<JobHandle> JobHandleList;
        public NativeList<JobHandle> CellJobHandleList;
        public NativeArray<float> RandomNumbers;
        public List<IVegetationStudioTerrain> VegetationStudioTerrainList;
        public List<VegetationPackagePro> VegetationPackageProList;
        public List<VegetationPackageProModelInfo> VegetationPackageProModelsList;
        public VegetationSettings VegetationSettings;
        public VegetationSystemPro VegetationSystemPro;
        public PersistentVegetationStorage PersistentVegetationStorage;
        public List<VegetationCell> CompactMemoryCellList;


        public VegetationInstanceDataPool VegetationInstanceDataPool;
        public float WorldspaceSeaLevel = 0;

        public void Init()
        {
            JobHandleList = new NativeList<JobHandle>(64, Allocator.Persistent);
            CellJobHandleList = new NativeList<JobHandle>(64, Allocator.Persistent);
            GenerateRandomNumberList();
            VegetationInstanceDataPool = new VegetationInstanceDataPool();
        }

        void GenerateRandomNumberList()
        {
            //TODO add seed for vegetation package
            Random.InitState(0);
            RandomNumbers = new NativeArray<float>(10000, Allocator.Persistent);
            for (int i = 0; i <= RandomNumbers.Length - 1; i++)
            {
                RandomNumbers[i] = Random.Range(0f, 1f);
            }
        }

        private int GetFirstUnityTerrainIndex()
        {
            for (int i = 0; i <= VegetationStudioTerrainList.Count - 1; i++)
            {
                if (VegetationStudioTerrainList[i] is UnityTerrain)
                {
                    return i;
                }
            }
            return -1;
        }

        public void PrepareVegetationCell(VegetationCell vegetationCell)
        {
            int itemCount = 0;
            Profiler.BeginSample("PrepareVegetationCell");
            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                itemCount += VegetationPackageProList[i].VegetationInfoList.Count;
                VegetationPackageInstances vegetationPackageInstances =
                    new VegetationPackageInstances(VegetationPackageProList[i].VegetationInfoList.Count);
                vegetationCell.VegetationPackageInstancesList.Add(vegetationPackageInstances);
            }

            vegetationCell.VegetationInstanceDataList.Capacity = itemCount;
            vegetationCell.Prepared = true;
            Profiler.EndSample();
        }

        JobHandle ExecuteSpawnRules(VegetationCell vegetationCell, Rect vegetationCellRect, int vegetationPackageIndex, int vegetationItemIndex)
        {
            int firstUnityTerrainIndex = GetFirstUnityTerrainIndex();
            VegetationItemInfoPro vegetationItemInfoPro = VegetationPackageProList[vegetationPackageIndex].VegetationInfoList[vegetationItemIndex];
            VegetationPackagePro vegetationPackagePro = VegetationPackageProList[vegetationPackageIndex];
            VegetationItemModelInfo vegetationItemModelInfo =
                VegetationPackageProModelsList[vegetationPackageIndex].VegetationItemModelList[vegetationItemIndex];
            BiomeType currentBiome = VegetationPackageProList[vegetationPackageIndex].BiomeType;
            int currentBiomeSortOrder = VegetationPackageProList[vegetationPackageIndex].BiomeSortOrder;

            float globalDensity =
                VegetationSettings.GetVegetationItemDensity(vegetationItemInfoPro.VegetationType);

            if (vegetationCell.VegetationPackageInstancesList[vegetationPackageIndex].LoadStateList[vegetationItemIndex] == 1) return default(JobHandle);
            vegetationCell.VegetationPackageInstancesList[vegetationPackageIndex].LoadStateList[vegetationItemIndex] = 1;

            bool doRuntimeSpawn = !(currentBiome != BiomeType.Default && !vegetationCell.HasBiome(currentBiome));

            if (globalDensity < 0.05f) doRuntimeSpawn = false;
            
            NativeList<MatrixInstance> matrixList =
                vegetationCell.VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemMatrixList[vegetationItemIndex];
            
            matrixList.Clear();          

            if (!vegetationItemInfoPro.EnableRuntimeSpawn) doRuntimeSpawn = false;      
            
            if (vegetationItemInfoPro.UseVegetationMask)
            {
                bool hasVegetationTypeIndex = false;
                if (vegetationCell.VegetationMaskList != null)
                {
                    for (int k = 0; k <= vegetationCell.VegetationMaskList.Count - 1; k++)
                    {
                        if (vegetationCell.VegetationMaskList[k]
                            .HasVegetationTypeIndex(vegetationItemInfoPro.VegetationTypeIndex))
                        {
                            hasVegetationTypeIndex = true;
                            break;
                        }
                    }
                }

                if (!hasVegetationTypeIndex) doRuntimeSpawn = false;
            }

            JobHandle vegetationItemHandle = default(JobHandle);
            
            if (doRuntimeSpawn)
            {
                VegetationInstanceData instanceData = VegetationInstanceDataPool.GetObject(); 
                instanceData.Clear();
                vegetationCell.VegetationInstanceDataList.Add(instanceData);
                
                float sampleDistance = vegetationItemInfoPro.SampleDistance / globalDensity;
                float density = 1;

                float calculatedSampleDistance = Mathf.Clamp(sampleDistance / density, 0.1f,
                    vegetationCell.VegetationCellBounds.size.x / 2f);
                int xSamples = Mathf.CeilToInt(vegetationCell.VegetationCellBounds.size.x / calculatedSampleDistance);
                int zSamples = Mathf.CeilToInt(vegetationCell.VegetationCellBounds.size.z / calculatedSampleDistance);
                int sampleCount = xSamples * zSamples;

                matrixList.Capacity = sampleCount;
                instanceData.SpawnLocations.ResizeUninitialized(sampleCount);                
               
                if (firstUnityTerrainIndex > -1)
                { 
                    instanceData.ResizeUninitialized(sampleCount);
                    InitInstanceData initInstanceData = new InitInstanceData
                    {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER                        
                        HeightmapSampled = instanceData.HeightmapSampled.AsDeferredJobArray(),
                        Excluded = instanceData.Excluded.AsDeferredJobArray()
#else
                        HeightmapSampled = instanceData.HeightmapSampled.ToDeferredJobArray(),
                        Excluded = instanceData.Excluded.ToDeferredJobArray()
#endif
                    };
                    vegetationItemHandle = initInstanceData.Schedule(sampleCount, 256, vegetationItemHandle);
                }
                else
                {
                    instanceData.SetCapasity(sampleCount);
                }             
                    float defaultSpawnChance = 0;
                if (currentBiome == BiomeType.Default) defaultSpawnChance = 1;
                            
                CalculateCellSpawnLocationsWideJob calculateCellSpawnLocationsWideJob =
                    new CalculateCellSpawnLocationsWideJob
                    {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                        SpawnLocations = instanceData.SpawnLocations.AsDeferredJobArray(),
#else
                        SpawnLocations = instanceData.SpawnLocations.ToDeferredJobArray(),
#endif
                       
                        CellSize = vegetationCell.VegetationCellBounds.size,
                        CellCorner =
                            vegetationCell.VegetationCellBounds.center -
                            vegetationCell.VegetationCellBounds.extents,
                        SampleDistance = sampleDistance,
                        RandomizePosition = vegetationItemInfoPro.RandomizePosition,
                        Density = 1,
                        DefaultSpawnChance = defaultSpawnChance,
                        RandomNumbers = RandomNumbers,
                        CellRect = vegetationCellRect,
                        CellIndex = vegetationCell.Index,
                        Seed = vegetationItemInfoPro.Seed + VegetationSettings.Seed,
                        UseSamplePointOffset = vegetationItemInfoPro.UseSamplePointOffset,
                        SamplePointMinOffset = vegetationItemInfoPro.SamplePointMinOffset,
                        SamplePointMaxOffset = vegetationItemInfoPro.SamplePointMaxOffset,
                        XSamples = xSamples,
                        ZSamples = zSamples,
                        CalculatedSampleDistance = calculatedSampleDistance
                    };

                vegetationItemHandle = calculateCellSpawnLocationsWideJob.Schedule(sampleCount,64,vegetationItemHandle);             
                
                if (vegetationCell.BiomeMaskList != null)
                {
                    for (int k = 0; k <= vegetationCell.BiomeMaskList.Count - 1; k++)
                    {
                        if (vegetationCell.BiomeMaskList[k].BiomeSortOrder < currentBiomeSortOrder) continue;
                        vegetationItemHandle = vegetationCell.BiomeMaskList[k]
                            .FilterSpawnLocations(instanceData.SpawnLocations, currentBiome, sampleCount, vegetationItemHandle);
                    }
                }         
                
                if (vegetationItemInfoPro.UseNoiseCutoff)
                {                   
                    PerlinNoiseCutoffJob perlinNoiseCutoffJob =
                        new PerlinNoiseCutoffJob
                        {
                            InversePerlinMask = vegetationItemInfoPro.NoiseCutoffInverse,
                            PerlinCutoff = vegetationItemInfoPro.NoiseCutoffValue,
                            PerlinScale = vegetationItemInfoPro.NoiseCutoffScale,
                            Offset = vegetationItemInfoPro.NoiseCutoffOffset,
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            SpawnLocationList = instanceData.SpawnLocations.AsDeferredJobArray() 
#else
                            SpawnLocationList = instanceData.SpawnLocations.ToDeferredJobArray() 
#endif
                            
                            
                        };

                    vegetationItemHandle = perlinNoiseCutoffJob.Schedule(sampleCount,64, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseNoiseDensity)
                {
                    PerlinNoiseDensityJob perlinNoiseDensityJob =
                        new PerlinNoiseDensityJob
                        {
                            InversePerlinMask = vegetationItemInfoPro.NoiseDensityInverse,
                            PerlinScale = vegetationItemInfoPro.NoiseDensityScale,
                            Offset = vegetationItemInfoPro.NoiseDensityOffset,
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            SpawnLocationList = instanceData.SpawnLocations.AsDeferredJobArray()
#else
                            SpawnLocationList = instanceData.SpawnLocations.ToDeferredJobArray()
#endif

                            
                            
                        };
                    vegetationItemHandle = perlinNoiseDensityJob.Schedule(sampleCount,64,vegetationItemHandle);
                }


                if (vegetationItemInfoPro.UseTextureMaskDensityRules)
                {
                    for (int k = 0; k <= vegetationItemInfoPro.TextureMaskDensityRuleList.Count - 1; k++)
                    {
                        TextureMaskGroup textureMaskGroup =
                            vegetationPackagePro.GetTextureMaskGroup(vegetationItemInfoPro
                                .TextureMaskDensityRuleList[k].TextureMaskGroupID);
                        if (textureMaskGroup != null)
                        {
                            vegetationItemHandle = textureMaskGroup.SampleDensityMask(instanceData, vegetationCellRect,
                                vegetationItemInfoPro.TextureMaskDensityRuleList[k], vegetationItemHandle);
                        }
                    }
                }


                FilterSpawnLocationsChanceJob filterSpawnLocationsChanceJob =
                    new FilterSpawnLocationsChanceJob
                    {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                        SpawnLocationList = instanceData.SpawnLocations.AsDeferredJobArray(),
#else
                        SpawnLocationList = instanceData.SpawnLocations.ToDeferredJobArray(),
#endif
                        RandomNumbers = RandomNumbers,
                        Density = vegetationItemInfoPro.Density
                    };

                vegetationItemHandle = filterSpawnLocationsChanceJob.Schedule(sampleCount,64,vegetationItemHandle);

                for (int k = 0; k <= VegetationStudioTerrainList.Count - 1; k++)
                {
                    vegetationItemHandle = VegetationStudioTerrainList[k]
                        .SampleTerrain(instanceData.SpawnLocations, instanceData, sampleCount, vegetationCellRect,
                            vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTerrainSourceExcludeRule)
                {
                    TerrainSourceExcludeRuleJob terrainSourceExcludeRuleJob =
                        new TerrainSourceExcludeRuleJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                        Excluded = instanceData.Excluded.AsDeferredJobArray(),
                        TerrainSourceID = instanceData.TerrainSourceID.AsDeferredJobArray(),
#else
                        Excluded = instanceData.Excluded.ToDeferredJobArray(),
                        TerrainSourceID = instanceData.TerrainSourceID.ToDeferredJobArray(),
#endif
                        TerrainSourceRule = vegetationItemInfoPro.TerrainSourceExcludeRule
                        };
                    vegetationItemHandle = terrainSourceExcludeRuleJob.Schedule(instanceData.Excluded,64,vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTerrainSourceIncludeRule)
                {
                    TerrainSourceIncludeRuleJob terrainSourceIncludeRuleJob =
                        new TerrainSourceIncludeRuleJob
                        {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            TerrainSourceID = instanceData.TerrainSourceID.AsDeferredJobArray(),
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            TerrainSourceID = instanceData.TerrainSourceID.ToDeferredJobArray(),
#endif
                            TerrainSourceRule = vegetationItemInfoPro.TerrainSourceIncludeRule
                        };
                    vegetationItemHandle = terrainSourceIncludeRuleJob.Schedule(instanceData.Excluded,64, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseSteepnessRule)
                {
                    InstanceSteepnessRuleJob instanceSteepnessRuleJob =
                        new InstanceSteepnessRuleJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            TerrainNormal = instanceData.TerrainNormal.AsDeferredJobArray(),
                            RandomNumberIndex = instanceData.RandomNumberIndex.AsDeferredJobArray(),
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            TerrainNormal = instanceData.TerrainNormal.ToDeferredJobArray(),
                            RandomNumberIndex = instanceData.RandomNumberIndex.ToDeferredJobArray(),
#endif
                            MinSteepness = vegetationItemInfoPro.MinSteepness,
                            MaxSteepness = vegetationItemInfoPro.MaxSteepness,
                            Advanced = vegetationItemInfoPro.UseAdvancedSteepnessRule,
                            SteepnessRuleCurveArray = vegetationItemModelInfo.SteepnessRuleCurveArray,
                            RandomNumbers = RandomNumbers
                        };
                    vegetationItemHandle = instanceSteepnessRuleJob.Schedule(instanceData.Excluded,64,vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseHeightRule)
                {
                    InstanceHeightRuleJob instanceHeightRuleJob =
                        new InstanceHeightRuleJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            Position = instanceData.Position.AsDeferredJobArray(),
                            RandomNumberIndex = instanceData.RandomNumberIndex.AsDeferredJobArray(),
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            Position = instanceData.Position.ToDeferredJobArray(),
                            RandomNumberIndex = instanceData.RandomNumberIndex.ToDeferredJobArray(),
#endif
                            MinHeight = vegetationItemInfoPro.MinHeight + WorldspaceSeaLevel,
                            MaxHeight = vegetationItemInfoPro.MaxHeight + WorldspaceSeaLevel,
                            Advanced = vegetationItemInfoPro.UseAdvancedHeightRule,
                            HeightRuleCurveArray = vegetationItemModelInfo.HeightRuleCurveArray,
                            RandomNumbers = RandomNumbers,
                            MaxCurveHeight = vegetationItemInfoPro.MaxCurveHeight
                        };
                    vegetationItemHandle = instanceHeightRuleJob.Schedule(instanceData.Excluded,64,vegetationItemHandle);
                }

                if (!vegetationItemInfoPro.UseVegetationMask && vegetationCell.VegetationMaskList != null)
                {
                    for (int k = 0; k <= vegetationCell.VegetationMaskList.Count - 1; k++)
                    {
                        vegetationItemHandle = vegetationCell.VegetationMaskList[k].SampleMask(instanceData,
                            vegetationItemInfoPro.VegetationType, vegetationItemHandle);
                    }
                }
                else
                {
                    if (vegetationCell.VegetationMaskList != null)
                    {
                        for (int k = 0; k <= vegetationCell.VegetationMaskList.Count - 1; k++)
                        {
                            vegetationItemHandle = vegetationCell.VegetationMaskList[k].SampleIncludeVegetationMask(
                                instanceData,
                                vegetationItemInfoPro.VegetationTypeIndex, vegetationItemHandle);
                        }

                        if (vegetationCell.VegetationMaskList.Count > 0)
                        {
                            ProcessIncludeVegetationMaskJob processIncludeVegetationMaskJob =
                                new ProcessIncludeVegetationMaskJob
                                {
                                    
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                                    Excluded = instanceData.Excluded.AsDeferredJobArray(),
                                    Scale = instanceData.Scale.AsDeferredJobArray(),
                                    RandomNumberIndex = instanceData.RandomNumberIndex.AsDeferredJobArray(),
                                    VegetationMaskDensity = instanceData.VegetationMaskDensity.AsDeferredJobArray(),
                                    VegetationMaskScale = instanceData.VegetationMaskScale.AsDeferredJobArray(),
#else
                                    Excluded = instanceData.Excluded.ToDeferredJobArray(),
                                    Scale = instanceData.Scale.ToDeferredJobArray(),
                                    RandomNumberIndex = instanceData.RandomNumberIndex.ToDeferredJobArray(),
                                    VegetationMaskDensity = instanceData.VegetationMaskDensity.ToDeferredJobArray(),
                                    VegetationMaskScale = instanceData.VegetationMaskScale.ToDeferredJobArray(),
#endif
                                    RandomNumbers = RandomNumbers
                                };
                            vegetationItemHandle = processIncludeVegetationMaskJob.Schedule(instanceData.Excluded,64,vegetationItemHandle);
                        }
                    }
                }

                if (vegetationItemInfoPro.UseConcaveLocationRule)
                {
                    for (int k = 0; k <= VegetationStudioTerrainList.Count - 1; k++)
                    {
                        vegetationItemHandle = VegetationStudioTerrainList[k].SampleConcaveLocation(instanceData,
                            vegetationItemInfoPro.ConcaveLoactionMinHeightDifference,
                            vegetationItemInfoPro.ConcaveLoactionDistance, vegetationItemInfoPro.ConcaveLocationInverse,
                            vegetationItemInfoPro.ConcaveLoactionAverage, vegetationCellRect, vegetationItemHandle);
                    }
                }

                if (vegetationItemInfoPro.UseTextureMaskIncludeRules)
                {
                    for (int k = 0; k <= vegetationItemInfoPro.TextureMaskIncludeRuleList.Count - 1; k++)
                    {
                        TextureMaskGroup textureMaskGroup =
                            vegetationPackagePro.GetTextureMaskGroup(vegetationItemInfoPro
                                .TextureMaskIncludeRuleList[k].TextureMaskGroupID);
                        if (textureMaskGroup != null)
                        {
                            vegetationItemHandle = textureMaskGroup.SampleIncludeMask(instanceData, vegetationCellRect,
                                vegetationItemInfoPro.TextureMaskIncludeRuleList[k], vegetationItemHandle);
                        }
                    }

                    FilterIncludeMaskJob filterIncludeMaskJob =
                        new FilterIncludeMaskJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            TextureMaskData = instanceData.TextureMaskData.AsDeferredJobArray()
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            TextureMaskData = instanceData.TextureMaskData.ToDeferredJobArray()
#endif
                        };
                    vegetationItemHandle = filterIncludeMaskJob.Schedule(instanceData.Excluded,64,vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTextureMaskExcludeRules)
                {
                    for (int k = 0; k <= vegetationItemInfoPro.TextureMaskExcludeRuleList.Count - 1; k++)
                    {
                        TextureMaskGroup textureMaskGroup =
                            vegetationPackagePro.GetTextureMaskGroup(vegetationItemInfoPro
                                .TextureMaskExcludeRuleList[k].TextureMaskGroupID);
                        if (textureMaskGroup != null)
                        {
                            vegetationItemHandle = textureMaskGroup.SampleExcludeMask(instanceData, vegetationCellRect,
                                vegetationItemInfoPro.TextureMaskExcludeRuleList[k], vegetationItemHandle);
                        }
                    }
                }

                if (vegetationItemInfoPro.UseTextureMaskScaleRules)
                {
                    for (int k = 0; k <= vegetationItemInfoPro.TextureMaskScaleRuleList.Count - 1; k++)
                    {
                        TextureMaskGroup textureMaskGroup =
                            vegetationPackagePro.GetTextureMaskGroup(vegetationItemInfoPro
                                .TextureMaskScaleRuleList[k].TextureMaskGroupID);
                        if (textureMaskGroup != null)
                        {
                            vegetationItemHandle = textureMaskGroup.SampleScaleMask(instanceData, vegetationCellRect,
                                vegetationItemInfoPro.TextureMaskScaleRuleList[k], vegetationItemHandle);
                        }
                    }
                }

                OffsetAndRotateScaleVegetationInstanceMathJob offsetAndRotateScaleVegetationInstanceJob =
                    new OffsetAndRotateScaleVegetationInstanceMathJob
                    {
                        RandomNumbers = RandomNumbers,
                        
                        
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                        Excluded = instanceData.Excluded.AsDeferredJobArray(),
                        Scale = instanceData.Scale.AsDeferredJobArray(),
                        Position = instanceData.Position.AsDeferredJobArray(),
                        Rotation = instanceData.Rotation.AsDeferredJobArray(),
                        RandomNumberIndex = instanceData.RandomNumberIndex.AsDeferredJobArray(),
                        TerrainNormal = instanceData.TerrainNormal.AsDeferredJobArray(), 
#else
                        Excluded = instanceData.Excluded.ToDeferredJobArray(),
                        Scale = instanceData.Scale.ToDeferredJobArray(),
                        Position = instanceData.Position.ToDeferredJobArray(),
                        Rotation = instanceData.Rotation.ToDeferredJobArray(),
                        RandomNumberIndex = instanceData.RandomNumberIndex.ToDeferredJobArray(),
                        TerrainNormal = instanceData.TerrainNormal.ToDeferredJobArray(), 
#endif
                        VegetationRotationType = vegetationItemInfoPro.Rotation,
                        MinScale = vegetationItemInfoPro.MinScale,
                        MaxScale = vegetationItemInfoPro.MaxScale,
                        Offset = vegetationItemInfoPro.Offset,
                        RotationOffset = vegetationItemInfoPro.RotationOffset,
                        ScaleMultiplier = vegetationItemInfoPro.ScaleMultiplier,
                        MinUpOffset = vegetationItemInfoPro.MinUpOffset,
                        MaxUpOffset = vegetationItemInfoPro.MaxUpOffset 
                    };
                vegetationItemHandle = offsetAndRotateScaleVegetationInstanceJob.Schedule(instanceData.Excluded,64,vegetationItemHandle);

                
                if (vegetationItemInfoPro.UseNoiseScaleRule)
                {
                    PerlinNoiseScaleJob perlinNoiseScaleJob =
                        new PerlinNoiseScaleJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            Position = instanceData.Position.AsDeferredJobArray(),
                            Scale = instanceData.Scale.AsDeferredJobArray(),
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            Position = instanceData.Position.ToDeferredJobArray(),
                            Scale = instanceData.Scale.ToDeferredJobArray(),
#endif
                            PerlinScale = vegetationItemInfoPro.NoiseScaleScale,
                            MinScale = vegetationItemInfoPro.NoiseScaleMinScale,
                            MaxScale = vegetationItemInfoPro.NoiseScaleMaxScale,
                            InversePerlinMask = vegetationItemInfoPro.NoiseScaleInverse,
                            Offset = vegetationItemInfoPro.NoiseScaleOffset
                        };
                    vegetationItemHandle = perlinNoiseScaleJob.Schedule(instanceData.Excluded,64, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseBiomeEdgeScaleRule && currentBiome != BiomeType.Default)
                {
                    BiomeEdgeDistanceScaleRuleJob biomeEdgeDistanceScaleRuleJob =
                        new BiomeEdgeDistanceScaleRuleJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            Scale = instanceData.Scale.AsDeferredJobArray(),
                            BiomeDistance = instanceData.BiomeDistance.AsDeferredJobArray(),
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            Scale = instanceData.Scale.ToDeferredJobArray(),
                            BiomeDistance = instanceData.BiomeDistance.ToDeferredJobArray(),
#endif
                            MinScale = vegetationItemInfoPro.BiomeEdgeScaleMinScale,
                            MaxScale = vegetationItemInfoPro.BiomeEdgeScaleMaxScale,
                            MaxDistance = vegetationItemInfoPro.BiomeEdgeScaleDistance,
                            InverseScale = vegetationItemInfoPro.BiomeEdgeScaleInverse
                        };
                    vegetationItemHandle = biomeEdgeDistanceScaleRuleJob.Schedule(instanceData.Excluded,64,vegetationItemHandle);
                }
                
                if (vegetationItemInfoPro.UseBiomeEdgeIncludeRule && currentBiome != BiomeType.Default)
                {
                    BiomeEdgeDistanceIncludeRuleJob biomeEdgeDistanceIncludeRuleJob =
                        new BiomeEdgeDistanceIncludeRuleJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            BiomeDistance = instanceData.BiomeDistance.AsDeferredJobArray(),
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            BiomeDistance = instanceData.BiomeDistance.ToDeferredJobArray(),
#endif
                            MaxDistance = vegetationItemInfoPro.BiomeEdgeIncludeDistance,
                            Inverse = vegetationItemInfoPro.BiomeEdgeIncludeInverse
                        };
                    vegetationItemHandle = biomeEdgeDistanceIncludeRuleJob.Schedule(instanceData.Excluded,64, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTerrainTextureIncludeRules)
                {
                    for (int k = 0; k <= VegetationStudioTerrainList.Count - 1; k++)
                    {
                        vegetationItemHandle = VegetationStudioTerrainList[k]
                            .ProcessSplatmapRules(vegetationItemInfoPro.TerrainTextureIncludeRuleList, instanceData,
                                true,
                                vegetationCellRect,
                                vegetationItemHandle);
                    }
                }

                if (vegetationItemInfoPro.UseTerrainTextureExcludeRules)
                {
                    for (int k = 0; k <= VegetationStudioTerrainList.Count - 1; k++)
                    {
                        vegetationItemHandle = VegetationStudioTerrainList[k]
                            .ProcessSplatmapRules(vegetationItemInfoPro.TerrainTextureExcludeRuleList, instanceData,
                                false,
                                vegetationCellRect,
                                vegetationItemHandle);
                    }
                }

                if (vegetationItemInfoPro.UseDistanceFalloff)
                {
                    DistanceFalloffJob distanceFalloffJob =
                        new DistanceFalloffJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            RandomNumberIndex = instanceData.RandomNumberIndex.AsDeferredJobArray(),
                            DistanceFalloff = instanceData.DistanceFalloff.AsDeferredJobArray(),
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            RandomNumberIndex = instanceData.RandomNumberIndex.ToDeferredJobArray(),
                            DistanceFalloff = instanceData.DistanceFalloff.ToDeferredJobArray(),
#endif
                            RandomNumbers = RandomNumbers,
                            DistanceFalloffStartDistance = vegetationItemInfoPro.DistanceFalloffStartDistance
                        };
                    vegetationItemHandle = distanceFalloffJob.Schedule(instanceData.Excluded,64, vegetationItemHandle);
                }
                
                NewCreateInstanceMatrixJob createInstanceMatrixJob =
                    new NewCreateInstanceMatrixJob
                    {
                        Excluded = instanceData.Excluded,
                        Position = instanceData.Position,
                        Scale = instanceData.Scale,
                        Rotation = instanceData.Rotation,
                        DistanceFalloff = instanceData.DistanceFalloff,
                        VegetationInstanceMatrixList = matrixList
                    };

                vegetationItemHandle = createInstanceMatrixJob.Schedule(vegetationItemHandle);
            }

            if (!doRuntimeSpawn)
            {
                if (PersistentVegetationStorage && !PersistentVegetationStorage.DisablePersistentStorage)
                {
                    PersistentVegetationCell persistentVegetationCell = PersistentVegetationStorage.GetPersistentVegetationCell(vegetationCell.Index);
                    PersistentVegetationInfo persistentVegetationInfo = persistentVegetationCell?.GetPersistentVegetationInfo(vegetationItemInfoPro.VegetationItemID);
                    if (persistentVegetationInfo != null && persistentVegetationInfo.VegetationItemList.Count > 0)
                    {
                        persistentVegetationInfo.CopyToNativeArray();
                        matrixList.ResizeUninitialized(persistentVegetationInfo.NativeVegetationItemArray.Length);
                        
                        LoadPersistentStorageToMatrixWideJob loadPersistentStorageToMatrixJob =
                            new LoadPersistentStorageToMatrixWideJob
                            {
                                InstanceList = persistentVegetationInfo.NativeVegetationItemArray,
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER    
                                VegetationInstanceMatrixList = matrixList.AsDeferredJobArray(),
#else
                                VegetationInstanceMatrixList = matrixList.ToDeferredJobArray(),
#endif
                                VegetationSystemPosition = VegetationSystemPro.VegetationSystemPosition
                            };
                        vegetationItemHandle = loadPersistentStorageToMatrixJob.Schedule(matrixList,64, vegetationItemHandle);
                    }
                }
            }
            else
            {
                if (PersistentVegetationStorage && !PersistentVegetationStorage.DisablePersistentStorage)
                {
                    
                    PersistentVegetationCell persistentVegetationCell = PersistentVegetationStorage.GetPersistentVegetationCell(vegetationCell.Index);
                    PersistentVegetationInfo persistentVegetationInfo = persistentVegetationCell?.GetPersistentVegetationInfo(vegetationItemInfoPro.VegetationItemID);
                    if (persistentVegetationInfo != null && persistentVegetationInfo.VegetationItemList.Count > 0)
                    {
                        persistentVegetationInfo.CopyToNativeArray();
                        LoadPersistentStorageToMatrixJob loadPersistentStorageToMatrixJob =
                            new LoadPersistentStorageToMatrixJob
                            {
                                InstanceList = persistentVegetationInfo.NativeVegetationItemArray,
                                VegetationInstanceMatrixList = matrixList,
                                VegetationSystemPosition = VegetationSystemPro.VegetationSystemPosition
                            };

                        vegetationItemHandle = loadPersistentStorageToMatrixJob.Schedule(vegetationItemHandle);
                    }
                }
            }
            
            Profiler.BeginSample("Schedule batched jobs");
            JobHandle.ScheduleBatchedJobs();
            Profiler.EndSample();
            
            return vegetationItemHandle;
        }

        public JobHandle SpawnVegetationCell(VegetationCell vegetationCell,out bool hasInstancedIndirect)
        {
            hasInstancedIndirect = false;

            JobHandleList.Clear();
            Rect vegetationCellRect = vegetationCell.Rectangle;

            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                for (int j = 0; j <= VegetationPackageProList[i].VegetationInfoList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro = VegetationPackageProList[i].VegetationInfoList[j];                                        
                    JobHandleList.Add(ExecuteSpawnRules(vegetationCell, vegetationCellRect, i, j));

                    if (vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.InstancedIndirect)
                    {
                        hasInstancedIndirect = true;
                    }
                }
            }

            if (JobHandleList.Length > 0)
            {
                CompactMemoryCellList.Add(vegetationCell);
                return JobHandle.CombineDependencies(JobHandleList);
            }

            return default(JobHandle);
        }

        public JobHandle SpawnVegetationCell(VegetationCell vegetationCell, string vegetationItemID,
            out bool hasInstancedIndirect)
        {
            hasInstancedIndirect = false;
            Rect vegetationCellRect = vegetationCell.Rectangle;

            VegetationItemIndexes vegetationItemIndexes = VegetationSystemPro.GetVegetationItemIndexes(vegetationItemID);

            if (vegetationItemIndexes.VegetationPackageIndex >= 0)
            {
                VegetationItemInfoPro vegetationItemInfoPro = VegetationPackageProList[vegetationItemIndexes.VegetationPackageIndex].VegetationInfoList[vegetationItemIndexes.VegetationItemIndex];
                
                if (vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.InstancedIndirect)
                {
                    hasInstancedIndirect = true;
                }
                
                CompactMemoryCellList.Add(vegetationCell);
                return ExecuteSpawnRules(vegetationCell, vegetationCellRect,
                    vegetationItemIndexes.VegetationPackageIndex, vegetationItemIndexes.VegetationItemIndex);
            }
            
            return default(JobHandle);    
        }          
        
        public JobHandle SpawnVegetationCell(VegetationCell vegetationCell, int currentDistanceBand,
            out bool hasInstancedIndirect, bool billboardsOnly)
        {
            hasInstancedIndirect = false;

            if (billboardsOnly)
            {
                vegetationCell.LoadedBillboards = true;
            }
            else
            {
                vegetationCell.LoadedDistanceBand = currentDistanceBand;
            }

            JobHandleList.Clear();
            Rect vegetationCellRect = vegetationCell.Rectangle;

            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                for (int j = 0; j <= VegetationPackageProList[i].VegetationInfoList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro = VegetationPackageProList[i].VegetationInfoList[j];
                   
                    if (billboardsOnly)
                    {
                        if (!vegetationItemInfoPro.UseBillboards ||
                            vegetationItemInfoPro.VegetationType != VegetationType.Tree)
                        {
                            continue;
                        }
                    }

                    int vegetationItemDistanceBand = vegetationItemInfoPro.GetDistanceBand();
                    if (currentDistanceBand > vegetationItemDistanceBand) continue;

                    JobHandleList.Add(ExecuteSpawnRules(vegetationCell, vegetationCellRect, i, j));

                    if (vegetationItemInfoPro.VegetationRenderMode == VegetationRenderMode.InstancedIndirect)
                    {
                        hasInstancedIndirect = true;
                    }
                }
            }

            if (JobHandleList.Length > 0)
            {
                CompactMemoryCellList.Add(vegetationCell);
                return JobHandle.CombineDependencies(JobHandleList);
            }

            return default(JobHandle);
        }      
        
        public void Dispose()
        {
            if (JobHandleList.IsCreated) JobHandleList.Dispose();
            if (CellJobHandleList.IsCreated) CellJobHandleList.Dispose();
            if (RandomNumbers.IsCreated) RandomNumbers.Dispose();
            VegetationInstanceDataPool.Dispose();
        }
    }
}