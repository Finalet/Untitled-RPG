using System.Collections.Generic;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public interface IVegetationStudioTerrain
    {
        string TerrainType { get; }
        Bounds TerrainBounds { get; }


        void RefreshTerrainData();

        void RefreshTerrainData(Bounds bounds);

        JobHandle SampleCellHeight(NativeArray<Bounds> vegetationCellBoundsList, float worldspaceHeightCutoff, Rect cellBoundsRect,
            JobHandle dependsOn = default(JobHandle));

        JobHandle SampleTerrain(NativeList<VegetationSpawnLocationInstance> spawnLocationList, VegetationInstanceData instanceData, int sampleCount, Rect spawnRect,
            JobHandle dependsOn);

        bool NeedsSplatMapUpdate(Bounds updateBounds);
        void PrepareSplatmapGeneration(bool clearLockedTextures);
        void GenerateSplatMapBiome(Bounds updateBounds, BiomeType biomeType, List<PolygonBiomeMask> polygonBiomeMaskList, List<TerrainTextureSettings> terrainTextureSettingsList, float heightCurveSampleHeight, float worldSpaceSeaLevel,bool clearLockedTextures);
        void CompleteSplatmapGeneration();
        
        JobHandle SampleConcaveLocation(VegetationInstanceData instanceData,float minHeightDifference, float distance , bool inverse, bool average, Rect spawnRect, JobHandle dependsOn);

        void Init();

        void DisposeTemporaryMemory();
        
        void OverrideTerrainMaterial();
        void RestoreTerrainMaterial();

        void VerifySplatmapAccess();

        void UpdateTerrainMaterial(float worldspaceSeaLevel, float worldspaceMaxTerrainHeight, TerrainTextureSettings terrainTextureSettings);

        JobHandle ProcessSplatmapRules(List<TerrainTextureRule> terrainTextureRuleList,
            VegetationInstanceData instanceData, bool include, Rect cellRect,JobHandle dependsOn);

        bool HasTerrainTextures();
        Texture2D GetTerrainTexture(int index);

#if UNITY_2018_3_OR_NEWER
        TerrainLayer[] GetTerrainLayers();
        void SetTerrainLayers(TerrainLayer[] terrainLayers);
#else
        SplatPrototype[] GetSplatPrototypes();
        void SetSplatPrototypes(SplatPrototype[] splatPrototypes);
#endif

    }

    public class VegetationStudioTerrain
    {
        public static IVegetationStudioTerrain GetIVegetationStudioTerrain(GameObject go)
        {
            if (go == null) return null;

            MonoBehaviour[] list = go.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour mb in list)
            {
                if (mb is IVegetationStudioTerrain)
                {
                    return mb as IVegetationStudioTerrain;
                }
            }

            return null;
        }
    }
}



