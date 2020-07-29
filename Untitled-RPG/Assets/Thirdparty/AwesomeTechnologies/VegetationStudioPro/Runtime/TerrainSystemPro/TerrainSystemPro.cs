using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationStudio;
using UnityEngine;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Jobs;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_2018_3_OR_NEWER
      using System.IO;
      #endif


namespace AwesomeTechnologies.TerrainSystem
{
    public partial class TerrainSystemPro : MonoBehaviour
    {
        public VegetationSystemPro VegetationSystemPro;
        public int CurrentTabIndex;
        public int VegetationPackageIndex;
        public int VegetationPackageTextureIndex;

        public bool ShowCurvesMenu = true;
        public bool ShowNoiseMenu = true;

        // ReSharper disable once UnusedMember.Local
        void Reset()
        {
            VegetationSystemPro = GetComponent<VegetationSystemPro>();
        }

        List<IVegetationStudioTerrain> GetOverlapTerrainList(Bounds updateBounds)
        {
            List<IVegetationStudioTerrain> overlapTerrainList = new List<IVegetationStudioTerrain>();
            for (int i = 0; i <= VegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
            {
                if (VegetationSystemPro.VegetationStudioTerrainList[i]
                    .NeedsSplatMapUpdate(updateBounds))
                {
                    overlapTerrainList.Add(VegetationSystemPro.VegetationStudioTerrainList[i]);
                }
            }

            return overlapTerrainList;
        }

        void PrepareTextureCurves()
        {
            for (int i = 0; i <= VegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                VegetationSystemPro.VegetationPackageProList[i].PrepareNativeArrayTextureCurves();
            }
        }

        void DisposeTextureCurves()
        {
            for (int i = 0; i <= VegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                VegetationSystemPro.VegetationPackageProList[i].DisposeNativeArrayTextureCurves();
            }
        }

        public Texture2D GetTerrainTexture(int index)
        {
            for (int i = 0; i <= VegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
            {
                IVegetationStudioTerrain iVegetationStudioTerrain = VegetationSystemPro.VegetationStudioTerrainList[i];
                if (iVegetationStudioTerrain.HasTerrainTextures())
                {
                    return iVegetationStudioTerrain.GetTerrainTexture(index);
                }
            }
            return null;
        }

        public void GetSplatPrototypesFromTerrain(VegetationPackagePro vegetationPackage)
        {
            for (int i = 0; i <= VegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
            {
                IVegetationStudioTerrain iVegetationStudioTerrain = VegetationSystemPro.VegetationStudioTerrainList[i];
                if (iVegetationStudioTerrain.HasTerrainTextures())
                {
                    
#if UNITY_2018_3_OR_NEWER
                    TerrainLayer[] terrainLayers = iVegetationStudioTerrain.GetTerrainLayers();

                    for (int j = 0; j <= vegetationPackage.TerrainTextureList.Count - 1; j++)
                    {
                        if (j < terrainLayers.Length)
                        {
                            vegetationPackage.TerrainTextureList[j].Texture = terrainLayers[j].diffuseTexture;
                            vegetationPackage.TerrainTextureList[j].TextureNormals = terrainLayers[j].normalMapTexture;
                            vegetationPackage.TerrainTextureList[j].Offset = terrainLayers[j].tileOffset;
                            vegetationPackage.TerrainTextureList[j].TileSize = terrainLayers[j].tileSize;
                        }
                    }

                    break;
#else
  SplatPrototype[] splatPrototypes = iVegetationStudioTerrain.GetSplatPrototypes();

                    for (int j = 0; j <= vegetationPackage.TerrainTextureList.Count - 1; j++)
                    {
                        if (j < splatPrototypes.Length)
                        {
                            vegetationPackage.TerrainTextureList[j].Texture = splatPrototypes[j].texture;
                            vegetationPackage.TerrainTextureList[j].TextureNormals = splatPrototypes[j].normalMap;
                            vegetationPackage.TerrainTextureList[j].Offset = splatPrototypes[j].tileOffset;
                            vegetationPackage.TerrainTextureList[j].TileSize = splatPrototypes[j].tileSize;
                        }
                    }
                    break;
#endif                                     
                }
            }
        }

        public void SetSplatPrototypes(VegetationPackagePro vegetationPackage)
        {
#if UNITY_2018_3_OR_NEWER
                       TerrainLayer[] terrainLayers = new TerrainLayer[vegetationPackage.TerrainTextureList.Count];
            for (int i = 0; i <= vegetationPackage.TerrainTextureList.Count - 1; i++)
            {
                TerrainTextureInfo terrainTextureInfo = vegetationPackage.TerrainTextureList[i];
                TerrainLayer terrainLayer = terrainTextureInfo.TerrainLayer;

                if (terrainLayer == null)
                {
                    terrainLayer = new TerrainLayer
                    {
                        diffuseTexture = terrainTextureInfo.Texture,
                        normalMapTexture = terrainTextureInfo.TextureNormals,
                        tileSize = terrainTextureInfo.TileSize,
                        tileOffset = terrainTextureInfo.Offset                                                  
                    };     
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        terrainLayer = SaveTerrainLayer(terrainLayer,vegetationPackage);                  
                    }
                    EditorUtility.SetDirty(vegetationPackage);
#endif
                    terrainTextureInfo.TerrainLayer = terrainLayer;
                }
                else
                {
                    terrainLayer.diffuseTexture = terrainTextureInfo.Texture;
                    terrainLayer.normalMapTexture = terrainTextureInfo.TextureNormals;
                    terrainLayer.tileSize = terrainTextureInfo.TileSize;
                    terrainLayer.tileOffset = terrainTextureInfo.Offset;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(terrainLayer);
#endif
                }                                                       
                terrainLayers[i] = terrainLayer;                
            }

            for (int i = 0; i <= VegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
            {
                IVegetationStudioTerrain iVegetationStudioTerrain = VegetationSystemPro.VegetationStudioTerrainList[i];
                if (iVegetationStudioTerrain.HasTerrainTextures())
                {
                    iVegetationStudioTerrain.SetTerrainLayers(terrainLayers);
                }
            }              
#else                        
            SplatPrototype[] splatPrototypes = new SplatPrototype[vegetationPackage.TerrainTextureList.Count];
            for (int i = 0; i <= vegetationPackage.TerrainTextureList.Count - 1; i++)
            {
                TerrainTextureInfo terrainTextureInfo = vegetationPackage.TerrainTextureList[i];

                SplatPrototype splatPrototype = new SplatPrototype
                {
                    texture = terrainTextureInfo.Texture,
                    normalMap = terrainTextureInfo.TextureNormals,
                    tileSize = terrainTextureInfo.TileSize,
                    tileOffset = terrainTextureInfo.Offset                   
                };
                splatPrototypes[i] = splatPrototype;
            }

            for (int i = 0; i <= VegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
            {
                IVegetationStudioTerrain iVegetationStudioTerrain = VegetationSystemPro.VegetationStudioTerrainList[i];
                if (iVegetationStudioTerrain.HasTerrainTextures())
                {
                    iVegetationStudioTerrain.SetSplatPrototypes(splatPrototypes);
                }
            }  
#endif
        }

        
#if UNITY_2018_3_OR_NEWER
        private TerrainLayer SaveTerrainLayer(TerrainLayer terrainLayer, VegetationPackagePro vegetationPackagePro)
        {
#if UNITY_EDITOR
            if (!vegetationPackagePro) return terrainLayer;

            string terrainDataPath = AssetDatabase.GetAssetPath(vegetationPackagePro);
            var directory = Path.GetDirectoryName(terrainDataPath);
            
            var filename = Path.GetFileNameWithoutExtension(terrainDataPath);
            var folderName = filename + "_TerrainLayers";

            if (!AssetDatabase.IsValidFolder(directory + "/" + folderName))
                AssetDatabase.CreateFolder(directory, folderName);            
            
            terrainDataPath = terrainDataPath.Replace(".asset", "");
            string newTerrainLayerDataPath = directory + "/" + folderName + "/_TerrainLayer_" + Guid.NewGuid().ToString() + ".asset";
            AssetDatabase.CreateAsset(terrainLayer, newTerrainLayerDataPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<TerrainLayer>(newTerrainLayerDataPath);
#else
            return null;
#endif
        }
#endif

        public void GenerateSplatMap(bool clearLockedTextures, IVegetationStudioTerrain iVegetationStudioTerrain)
        {
            if (!VegetationSystemPro) return;

            VegetationSystemPro.ClearCache(iVegetationStudioTerrain.TerrainBounds);            
            PrepareTextureCurves();
            
            float worldspaceMinHeight = VegetationSystemPro.VegetationSystemBounds.center.y -
                                        VegetationSystemPro.VegetationSystemBounds.extents.y;
            float worldspaceSeaLevel = worldspaceMinHeight + VegetationSystemPro.SeaLevel;
            float worldspaceMaxHeight = VegetationSystemPro.VegetationSystemBounds.center.y +
                                        VegetationSystemPro.VegetationSystemBounds.extents.y;
            float heightCurveSampleHeight = worldspaceMaxHeight - worldspaceSeaLevel;
                                 
            VegetationPackagePro defaultBiomeVegetationPackagePro =
                VegetationSystemPro.GetVegetationPackageFromBiome(BiomeType.Default);

            if (defaultBiomeVegetationPackagePro == null)
            {
                Debug.LogWarning("You need a default biome in order to generate splatmaps. ");
                return;
            }

            iVegetationStudioTerrain.PrepareSplatmapGeneration(clearLockedTextures);                
            
            iVegetationStudioTerrain.GenerateSplatMapBiome(VegetationSystemPro.VegetationSystemBounds,
                    BiomeType.Default, null, defaultBiomeVegetationPackagePro.TerrainTextureSettingsList,
                    heightCurveSampleHeight, worldspaceSeaLevel, clearLockedTextures);
            
            List<BiomeType> additionalBiomeList = VegetationSystemPro.GetAdditionalBiomeList();            

            List<VegetationPackagePro> additionalVegetationPackageList = new List<VegetationPackagePro>();
            for (int i = 0; i <= additionalBiomeList.Count - 1; i++)
            {
                additionalVegetationPackageList.Add(
                    VegetationSystemPro.GetVegetationPackageFromBiome(additionalBiomeList[i]));
            }
            BiomeSortOrderComparer biomeSortOrderComparer = new BiomeSortOrderComparer();
            additionalVegetationPackageList.Sort(biomeSortOrderComparer);

            for (int i = 0; i <= additionalVegetationPackageList.Count - 1; i++)
            {
                VegetationPackagePro currentVegetationPackagePro = additionalVegetationPackageList[i];
                if (!currentVegetationPackagePro.GenerateBiomeSplamap) continue;                
                
                List<PolygonBiomeMask> biomeMaskList = VegetationStudioManager.GetBiomeMasks(currentVegetationPackagePro.BiomeType);
                iVegetationStudioTerrain.GenerateSplatMapBiome(VegetationSystemPro.VegetationSystemBounds,
                        currentVegetationPackagePro.BiomeType, biomeMaskList, currentVegetationPackagePro.TerrainTextureSettingsList, heightCurveSampleHeight, worldspaceSeaLevel,clearLockedTextures);
                                               
            }

            JobHandle.ScheduleBatchedJobs();
            iVegetationStudioTerrain.CompleteSplatmapGeneration();
            DisposeTextureCurves();
        }

        public void GenerateSplatMap(bool clearLockedTextures)
        {
            List<IVegetationStudioTerrain> overlapTerrainList = GetOverlapTerrainList(VegetationSystemPro.VegetationSystemBounds);
            int terrainCount = overlapTerrainList.Count;
            for (int i = 0; i <= overlapTerrainList.Count - 1; i++)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Generate splatmap",
                    "Terrain " + (i + 1) + "/" + terrainCount, (i + 1) / (float) terrainCount);
#endif
                GenerateSplatMap(clearLockedTextures, overlapTerrainList[i]);
                GC.Collect();
            }
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
            
        }

       public void GenerateSplatMapParallel(bool clearLockedTextures)
        {
            if (!VegetationSystemPro) return;
            
            VegetationSystemPro.ClearCache();
            
            List<IVegetationStudioTerrain> overlapTerrainList =
                GetOverlapTerrainList(VegetationSystemPro.VegetationSystemBounds);
            PrepareTextureCurves();

            float worldspaceMinHeight = VegetationSystemPro.VegetationSystemBounds.center.y -
                                        VegetationSystemPro.VegetationSystemBounds.extents.y;
            float worldspaceSeaLevel = worldspaceMinHeight + VegetationSystemPro.SeaLevel;
            float worldspaceMaxHeight = VegetationSystemPro.VegetationSystemBounds.center.y +
                                        VegetationSystemPro.VegetationSystemBounds.extents.y;
            float heightCurveSampleHeight = worldspaceMaxHeight - worldspaceSeaLevel;


            VegetationPackagePro defaultBiomeVegetationPackagePro =
                VegetationSystemPro.GetVegetationPackageFromBiome(BiomeType.Default);

            if (defaultBiomeVegetationPackagePro == null)
            {
                Debug.LogWarning("You need a default biome in order to generate splatmaps. ");
                return;
            }

            int terrainCount = overlapTerrainList.Count;
            for (int i = 0; i <= overlapTerrainList.Count - 1; i++)
            {
                                
                overlapTerrainList[i].PrepareSplatmapGeneration(clearLockedTextures);                
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Prepare generation",
                    "Terrain " + (i + 1) + "/" + terrainCount, (i + 1) / (float) terrainCount);
#endif                
            }
            
            for (int i = 0; i <= overlapTerrainList.Count - 1; i++)
            {
                overlapTerrainList[i].GenerateSplatMapBiome(VegetationSystemPro.VegetationSystemBounds,
                    BiomeType.Default, null, defaultBiomeVegetationPackagePro.TerrainTextureSettingsList, heightCurveSampleHeight, worldspaceSeaLevel,clearLockedTextures);
                
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Generating default biome",
                    "Terrain " + (i + 1) + "/" + terrainCount, (i + 1) / (float) terrainCount);
#endif
            }

            List<BiomeType> additionalBiomeList = VegetationSystemPro.GetAdditionalBiomeList();
            

            List<VegetationPackagePro> additionalVegetationPackageList = new List<VegetationPackagePro>();
            for (int i = 0; i <= additionalBiomeList.Count - 1; i++)
            {
                additionalVegetationPackageList.Add(
                    VegetationSystemPro.GetVegetationPackageFromBiome(additionalBiomeList[i]));
            }
            BiomeSortOrderComparer biomeSortOrderComparer = new BiomeSortOrderComparer();
            additionalVegetationPackageList.Sort(biomeSortOrderComparer);

            for (int i = 0; i <= additionalVegetationPackageList.Count - 1; i++)
            {
                VegetationPackagePro currentVegetationPackagePro = additionalVegetationPackageList[i];
                if (!currentVegetationPackagePro.GenerateBiomeSplamap) continue;                
                
                List<PolygonBiomeMask> biomeMaskList = VegetationStudioManager.GetBiomeMasks(currentVegetationPackagePro.BiomeType);
                for (int j = 0; j <= overlapTerrainList.Count - 1; j++)
                {
                    overlapTerrainList[j].GenerateSplatMapBiome(VegetationSystemPro.VegetationSystemBounds,
                        currentVegetationPackagePro.BiomeType, biomeMaskList, currentVegetationPackagePro.TerrainTextureSettingsList, heightCurveSampleHeight, worldspaceSeaLevel,clearLockedTextures);
                }                               
            }

            JobHandle.ScheduleBatchedJobs();

           
            for (int i = 0; i <= overlapTerrainList.Count - 1; i++)
            {
#if UNITY_EDITOR
                      EditorUtility.DisplayProgressBar("Updating terrain",
                          "Terrain " + (i + 1) + "/" + terrainCount, (i + 1) / (float) terrainCount);
      #endif
                overlapTerrainList[i].CompleteSplatmapGeneration();
            }
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
            DisposeTextureCurves();
        }

      
    }
}
