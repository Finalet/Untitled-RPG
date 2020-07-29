using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{   
 public class VegetationInstanceData
    {
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
        public NativeList<byte> TerrainSourceID;        
        public NativeList<byte> TextureMaskData;
        public NativeList<byte> Excluded;
        public NativeList<byte> HeightmapSampled;

        public NativeList<VegetationSpawnLocationInstance> SpawnLocations;
        
        public VegetationInstanceData()
        {
            Position = new NativeList<float3>(Allocator.Persistent);
            Rotation = new NativeList<quaternion>(Allocator.Persistent);
            Scale = new NativeList<float3>(Allocator.Persistent);
            TerrainNormal = new NativeList<float3>(Allocator.Persistent);
            BiomeDistance = new NativeList<float>(Allocator.Persistent);
            TerrainTextureData = new NativeList<byte>(Allocator.Persistent);
            RandomNumberIndex = new NativeList<int>(Allocator.Persistent);
            DistanceFalloff = new NativeList<float>(Allocator.Persistent);
            VegetationMaskDensity = new NativeList<float>(Allocator.Persistent);
            VegetationMaskScale = new NativeList<float>(Allocator.Persistent);
            TerrainSourceID = new NativeList<byte>(Allocator.Persistent);
            TextureMaskData = new NativeList<byte>(Allocator.Persistent);
            Excluded = new NativeList<byte>(Allocator.Persistent);
            HeightmapSampled = new NativeList<byte>(Allocator.Persistent);
            
            
            SpawnLocations = new NativeList<VegetationSpawnLocationInstance>(Allocator.Persistent);
        }

        public void ResizeUninitialized(int length)
        {
            Position.ResizeUninitialized(length);
            Rotation.ResizeUninitialized(length);
            Scale.ResizeUninitialized(length);
            TerrainNormal.ResizeUninitialized(length);
            BiomeDistance.ResizeUninitialized(length);
            TerrainTextureData.ResizeUninitialized(length);
            RandomNumberIndex.ResizeUninitialized(length);
            DistanceFalloff.ResizeUninitialized(length);
            VegetationMaskDensity.ResizeUninitialized(length);
            VegetationMaskScale.ResizeUninitialized(length);            
            TerrainSourceID.ResizeUninitialized(length);
            TextureMaskData.ResizeUninitialized(length);
            Excluded.ResizeUninitialized(length);            
            HeightmapSampled.ResizeUninitialized(length);
        }
        
        public void Dispose()
        {
            Position.Dispose();
            Rotation.Dispose();
            Scale.Dispose();
            TerrainNormal.Dispose();
            BiomeDistance.Dispose();
            TerrainTextureData.Dispose();
            RandomNumberIndex.Dispose();
            DistanceFalloff.Dispose();
            VegetationMaskDensity.Dispose();
            VegetationMaskScale.Dispose();
            TerrainSourceID.Dispose();
            TextureMaskData.Dispose();
            Excluded.Dispose();
            HeightmapSampled.Dispose();
            SpawnLocations.Dispose();
        }
        
        public void Clear()
        {
            Position.Clear();
            Rotation.Clear();
            Scale.Clear();
            TerrainNormal.Clear();
            BiomeDistance.Clear();
            TerrainTextureData.Clear();
            RandomNumberIndex.Clear();
            DistanceFalloff.Clear();
            VegetationMaskDensity.Clear();
            VegetationMaskScale.Clear();
            TerrainSourceID.Clear();
            TextureMaskData.Clear();
            Excluded.Clear();
            HeightmapSampled.Clear();
            SpawnLocations.Clear();
        }

        public void CompactMemory()
        {
            Position.CompactMemory();
            Rotation.CompactMemory();
            Scale.CompactMemory();
            TerrainNormal.CompactMemory();
            BiomeDistance.CompactMemory();
            TerrainTextureData.CompactMemory();
            RandomNumberIndex.CompactMemory();
            DistanceFalloff.CompactMemory();
            VegetationMaskDensity.CompactMemory();
            VegetationMaskScale.CompactMemory();
            TerrainSourceID.CompactMemory();
            TextureMaskData.CompactMemory();
            Excluded.CompactMemory();
            HeightmapSampled.CompactMemory();
            SpawnLocations.CompactMemory();            
        }

        public void SetCapasity(int capasity)
        {
            Position.Capacity = capasity;
            Rotation.Capacity = capasity;
            Scale.Capacity = capasity;
            TerrainNormal.Capacity = capasity;
            BiomeDistance.Capacity = capasity;
            TerrainTextureData.Capacity = capasity;
            RandomNumberIndex.Capacity = capasity;
            DistanceFalloff.Capacity = capasity;
            VegetationMaskDensity.Capacity = capasity;
            VegetationMaskScale.Capacity = capasity;
            TerrainSourceID.Capacity = capasity;
            TextureMaskData.Capacity = capasity;
            Excluded.Capacity = capasity;
            HeightmapSampled.Capacity = capasity;
            SpawnLocations.Capacity = capasity;
        }
    }
}
