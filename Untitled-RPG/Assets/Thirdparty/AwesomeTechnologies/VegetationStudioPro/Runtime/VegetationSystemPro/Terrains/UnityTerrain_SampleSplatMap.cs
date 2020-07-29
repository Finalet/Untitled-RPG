using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AwesomeTechnologies.Vegetation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


namespace AwesomeTechnologies.VegetationSystem
{
    // ReSharper disable once InconsistentNaming
    public struct ARGBBytes
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;
    }

    // ReSharper disable once InconsistentNaming
    public struct RGBABytes
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;
    }

//    [BurstCompile(CompileSynchronously = true)]
//    public struct SplatMapRuleJob : IJob
//    {
//        public NativeList<VegetationInstance> InstanceList;
//        [ReadOnly]
//        public NativeArray<ARGBBytes> SplatMapArray;
//        public float MinValue;
//        public float MaxValue;
//        public int SplatmapIndex;
//        public int Width;
//        public int Height;
//        public float3 TerrainPosition;
//        public float2 SplatCellSize;
//
//        public void Execute()
//        {
//            int minValue = Mathf.RoundToInt(MinValue * 256);
//            int maxValue = Mathf.RoundToInt(MaxValue * 256);
//
//            for (int i = 0; i <= InstanceList.Length - 1; i++)
//            {
//                VegetationInstance vegetationInstance = InstanceList[i];
//                float3 localPosition = vegetationInstance.Position - TerrainPosition;
//                int x = math.clamp(Mathf.RoundToInt(localPosition.x / SplatCellSize.x), 0, Width -1);
//                int z = math.clamp(Mathf.RoundToInt(localPosition.z / SplatCellSize.y), 0, Height -1);
//
//                int value = 0;
//                switch (SplatmapIndex)
//                {
//                    case 0:
//                        value = SplatMapArray[x + (z * Width)].R;
//                        break;
//                    case 1:
//                        value = SplatMapArray[x + (z * Width)].G;
//                        break;
//                    case 2:
//                        value = SplatMapArray[x + (z * Width)].B;
//                        break;
//                    case 3:
//                        value = SplatMapArray[x + (z * Width)].A;
//                        break;
//                }
//                if (value >= minValue && value <= maxValue)
//                {
//                    vegetationInstance.TerrainTextureData = 1;
//                }
//                InstanceList[i] = vegetationInstance;
//            }
//        }
//    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct SplatMapRuleJob : IJobParallelForDefer
#else
    public struct SplatMapRuleJob : IJobParallelFor
#endif
    {
       // public NativeArray<VegetationInstance> InstanceList;
        public NativeArray<byte> Excluded;
        public NativeArray<byte> TerrainTextureData;
        public NativeArray<float3> Position;

        [ReadOnly] public NativeArray<ARGBBytes> SplatMapArray;
        public float MinValue;
        public float MaxValue;
        public int SplatmapIndex;
        public int Width;
        public int Height;
        public float3 TerrainPosition;        
        public float2 SplatCellSize;
        public bool Include;

        public void Execute(int index)       
        {
            if (Excluded[index] == 1) return;
            
            int minValue = Mathf.RoundToInt(MinValue * 256);
            int maxValue = Mathf.RoundToInt(MaxValue * 256);

            float3 localPosition = Position[index] - TerrainPosition;
            //int x = math.clamp(Mathf.RoundToInt(localPosition.x / SplatCellSize.x), 0, Width - 1);
            //int z = math.clamp(Mathf.RoundToInt(localPosition.z / SplatCellSize.y), 0, Height - 1);
            
            
            int x = Mathf.RoundToInt(localPosition.x / SplatCellSize.x);
            int z = Mathf.RoundToInt(localPosition.z / SplatCellSize.y);
            if (x < 0 || z < 0 || x > (Width - 1) || z > (Height - 1))
            {
                if (Include)
                {
                    TerrainTextureData[index] = 1;
                }
                return;
            }
            

            int value = 0;
            switch (SplatmapIndex)
            {
                case 0:
                    value = SplatMapArray[x + (z * Width)].R;
                    break;
                case 1:
                    value = SplatMapArray[x + (z * Width)].G;
                    break;
                case 2:
                    value = SplatMapArray[x + (z * Width)].B;
                    break;
                case 3:
                    value = SplatMapArray[x + (z * Width)].A;
                    break;
            }

            if (value >= minValue && value <= maxValue)
            {
                TerrainTextureData[index] = 1;
            }
        }
    }


//    [BurstCompile]
//    public struct SplatMapRuleCompleteJob : IJob
//    {
//        public NativeList<VegetationInstance> InstanceList;
//        public bool Include;
//
//        public void Execute()
//        {
//            for (int i = InstanceList.Length - 1; i >= 0; i--)
//            {
//                if (Include)
//                {
//                    if (InstanceList[i].TerrainTextureData != 1)
//                    {
//                        InstanceList.RemoveAtSwapBack(i);
//                    }
//                    else
//                    {
//                        VegetationInstance vegetationInstance = InstanceList[i];
//                        vegetationInstance.TerrainTextureData = 0;
//                        InstanceList[i] = vegetationInstance;
//                    }
//                }
//                else
//                {
//                    if (InstanceList[i].TerrainTextureData == 1)
//                    {
//                        InstanceList.RemoveAtSwapBack(i);
//                    }
//                    else
//                    {
//                        VegetationInstance vegetationInstance = InstanceList[i];
//                        vegetationInstance.TerrainTextureData = 0;
//                        InstanceList[i] = vegetationInstance;
//                    }
//                }
//            }
//        }
//    }
//    
    
    [BurstCompile]
#if UNITY_2019_1_OR_NEWER
        public struct SplatMapRuleCompleteJob : IJobParallelForDefer
#else
    public struct SplatMapRuleCompleteJob : IJobParallelFor
#endif
    {
        public NativeArray<byte> Excluded;
        public NativeArray<byte> TerrainTextureData;
        public bool Include;

        public void Execute(int index)
        {      
            if (Excluded[index] == 1) return;
            
                if (Include)
                {
                    if (TerrainTextureData[index] != 1)
                    {
                        Excluded[index] = 1;  
                    }
                    else
                    {
                        TerrainTextureData[index] = 0;
                    }
                }
                else
                {
                    if (TerrainTextureData[index] == 1)
                    {
                        Excluded[index] = 1;
                    }
                    else
                    {
                        TerrainTextureData[index] = 0;
                    }
                }
            
        }
    }

    public partial class UnityTerrain
    {
        public Texture2D GetTerrainPreviewTexture(int textureIndex)
        {
#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
            if (textureIndex < Terrain.terrainData.terrainLayers.Length)
            {
                return AssetPreview.GetAssetPreview(Terrain.terrainData.terrainLayers[textureIndex].diffuseTexture);
            }
            else
            {
                return null;
            }           
#else
            if (textureIndex < Terrain.terrainData.splatPrototypes.Length)
            {
                return AssetPreview.GetAssetPreview(Terrain.terrainData.splatPrototypes[textureIndex].texture);
            }
            else
            {
                return null;
            }
#endif
#else
            return null;
#endif
        }

        public void RefreshSplatMaps()
        {
            if (!Terrain || !Terrain.terrainData) return;

            _splatMapArrayList.Clear();
            _splatMapFormatList.Clear();
            for (int i = 0; i <= Terrain.terrainData.alphamapTextures.Length - 1; i++)
            {
                NativeArray<ARGBBytes> splatmapArray =
                    Terrain.terrainData.alphamapTextures[i].GetRawTextureData<ARGBBytes>();
                _splatMapArrayList.Add(splatmapArray);

                if (Terrain.terrainData.alphamapTextures[i].format == TextureFormat.RGBA32)
                {
                    _splatMapFormatList.Add(1);
                }
                else
                {
                    _splatMapFormatList.Add(0);
                }

            }
        }

        bool IsSplatmapArraysValid()
        {
            for (int i = 0; i <= _splatMapArrayList.Count - 1; i++)
            {
                if (!_splatMapArrayList[i].IsCreated) return false;
            }

            return true;
        }

        public void VerifySplatmapAccess()
        {
            RefreshSplatMaps();
        }

        public JobHandle ProcessSplatmapRules(List<TerrainTextureRule> terrainTextureRuleList,
            VegetationInstanceData instanceData, bool include, Rect cellRect, JobHandle dependsOn)
        {
            if (cellRect.Overlaps(_terrainRect))
            {
                if (!IsSplatmapArraysValid()) return dependsOn;

                int width = Terrain.terrainData.alphamapWidth;
                int height = Terrain.terrainData.alphamapHeight;

                Vector2 splatCellSize = new Vector2(Terrain.terrainData.size.x / (width - 1),
                    Terrain.terrainData.size.z / (height - 1));
                for (int i = 0; i <= terrainTextureRuleList.Count - 1; i++)
                {
                    int splatmapIndex = terrainTextureRuleList[i].TextureIndex / 4;
                    int localIndex = terrainTextureRuleList[i].TextureIndex - (4 * splatmapIndex);

                    if (splatmapIndex < _splatMapArrayList.Count)
                    {
                        if (_splatMapFormatList[splatmapIndex] == 1)
                        {
                            localIndex--;
                            if (localIndex == -1) localIndex = 3;
                        }

                        SplatMapRuleJob splatMapRuleJob = new SplatMapRuleJob
                        {
                            
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                            Excluded = instanceData.Excluded.AsDeferredJobArray(),
                            TerrainTextureData = instanceData.TerrainTextureData.AsDeferredJobArray(),
                            Position = instanceData.Position.AsDeferredJobArray(),       
#else
                            Excluded = instanceData.Excluded.ToDeferredJobArray(),
                            TerrainTextureData = instanceData.TerrainTextureData.ToDeferredJobArray(),
                            Position = instanceData.Position.ToDeferredJobArray(),             
#endif
                            
                     
                            SplatMapArray = _splatMapArrayList[splatmapIndex],
                            MinValue = terrainTextureRuleList[i].MinimumValue,
                            MaxValue = terrainTextureRuleList[i].MaximumValue,
                            SplatmapIndex = localIndex,
                            Width = width,
                            Height = height,
                            TerrainPosition = TerrainPosition,
                            SplatCellSize = splatCellSize,
                            Include = include                                
                        };
                        dependsOn = splatMapRuleJob.Schedule(instanceData.Excluded,32,dependsOn);
                    }
                }

                SplatMapRuleCompleteJob splatMapRuleCompleteJob =
                    new SplatMapRuleCompleteJob
                    {
                        
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                        Excluded = instanceData.Excluded.AsDeferredJobArray(),
                        TerrainTextureData = instanceData.TerrainTextureData.AsDeferredJobArray(),   
#else
                        Excluded = instanceData.Excluded.ToDeferredJobArray(),
                        TerrainTextureData = instanceData.TerrainTextureData.ToDeferredJobArray(),           
#endif
                        Include = include
                    };
                return splatMapRuleCompleteJob.Schedule(instanceData.Excluded,32, dependsOn);
            }
            return dependsOn;
        }
    }
}