using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.Masks
{
//    [BurstCompile]
//    public struct SampleRgbaChannelIncludeMask : IJob
//    {
//        public NativeList<VegetationInstance> InstanceList;
//        [ReadOnly]
//        public NativeArray<RGBABytes> RgbaNativeArray;
//
//        public bool Inverse;
//        public int SelectedChannel;
//
//        public int Width;
//        public int Height;
//        public Rect TextureRect;
//
//        public float MinValue;
//        public float MaxValue;
//
//        public void Execute()
//        {
//            float2 cellSize = new float2(TextureRect.width/Width,TextureRect.height/Height);
//            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width/2f, 0, TextureRect.center.y - TextureRect.height / 2f);
//            
//            int minValue = Mathf.RoundToInt(MinValue * 256);
//            int maxValue = Mathf.RoundToInt(MaxValue * 256);
//
//            for (int i = 0; i <= InstanceList.Length - 1; i++)
//            {
//                VegetationInstance vegetationInstance = InstanceList[i];
//                float3 localPosition = vegetationInstance.Position - texturePosition;
//
//                int x = Mathf.RoundToInt(localPosition.x / cellSize.x);
//                int z = Mathf.RoundToInt(localPosition.z / cellSize.y);
//
//                if (x < 0 || x > Width - 1) continue;
//                if (z < 0 || z > Height -1) continue;
//
//                int value = 0;
//
//                
//
//                switch (SelectedChannel)
//                {
//                    case 0:
//                        value = RgbaNativeArray[x + (z * Width)].R;
//                        break;
//                    case 1:
//                        value = RgbaNativeArray[x + (z * Width)].G;
//                        break;
//                    case 2:
//                        value = RgbaNativeArray[x + (z * Width)].B;
//                        break;
//                    case 3:
//                        value = RgbaNativeArray[x + (z * Width)].A;
//                        break;
//                }
//
//                if (Inverse)
//                {
//                    value = 256 - value;
//                }               
//
//                if (value >= minValue && value <= maxValue)
//                {
//                    vegetationInstance.TextureMaskData = 1;
//                }
//                InstanceList[i] = vegetationInstance;
//            }
//        }
//    }

    [BurstCompile]
#if UNITY_2019_1_OR_NEWER
    public struct SampleRgbaChannelIncludeMaskJob : IJobParallelForDefer
#else
    public struct SampleRgbaChannelIncludeMaskJob : IJobParallelFor
#endif
    {
        public NativeArray<float3> Position;
        public NativeArray<byte> TextureMaskData;
        public NativeArray<byte> Excluded;
        [ReadOnly] public NativeArray<RGBABytes> RgbaNativeArray;

        public bool Inverse;
        public int SelectedChannel;

        public int Width;
        public int Height;
        public Rect TextureRect;

        public float MinValue;
        public float MaxValue;

        public float2 Repeat;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            float2 cellSize = new float2(TextureRect.width / Width, TextureRect.height / Height);
            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width / 2f, 0,
                TextureRect.center.y - TextureRect.height / 2f);

            int minValue = Mathf.RoundToInt(MinValue * 256);
            int maxValue = Mathf.RoundToInt(MaxValue * 256);
        
            float3 localPosition = Position[index] - texturePosition;
            
            
            localPosition = new float3(localPosition.x / cellSize.x ,0,localPosition.z / cellSize.y);            
            float3 normalizedPosition = new float3(localPosition.x/Width, 0, localPosition.z/Height);                        
            normalizedPosition = new float3(normalizedPosition.x*Repeat.x,0, normalizedPosition.z* Repeat.y);
            normalizedPosition = math.frac(normalizedPosition);

            int x = Mathf.RoundToInt(normalizedPosition.x * Width); 
            int z = Mathf.RoundToInt(normalizedPosition.z * Height);                                    
            
            //int x = Mathf.RoundToInt((localPosition.x / cellSize.x));
            //int z = Mathf.RoundToInt((localPosition.z / cellSize.y));

            if (x < 0 || x > Width - 1) return;
            if (z < 0 || z > Height - 1) return;

            int value = 0;


            switch (SelectedChannel)
            {
                case 0:
                    value = RgbaNativeArray[x + (z * Width)].R;
                    break;
                case 1:
                    value = RgbaNativeArray[x + (z * Width)].G;
                    break;
                case 2:
                    value = RgbaNativeArray[x + (z * Width)].B;
                    break;
                case 3:
                    value = RgbaNativeArray[x + (z * Width)].A;
                    break;
            }

            if (Inverse)
            {
                value = 256 - value;
            }

            if (value >= minValue && value <= maxValue)
            {
                TextureMaskData[index] = 1;
            }
        }
    }

    //    [BurstCompile]
    //    public struct SampleRgbaChannelExcludeMask : IJob
    //    {
    //        public NativeList<VegetationInstance> InstanceList;
    //        [ReadOnly] public NativeArray<RGBABytes> RgbaNativeArray;
    //
    //        public bool Inverse;
    //        public int SelectedChannel;
    //
    //        public int Width;
    //        public int Height;
    //        public Rect TextureRect;
    //
    //        public float MinValue;
    //        public float MaxValue;
    //
    //        public void Execute()
    //        {
    //            float2 cellSize = new float2(TextureRect.width / Width, TextureRect.height / Height);
    //            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width / 2f, 0,
    //                TextureRect.center.y - TextureRect.height / 2f);
    //
    //            int minValue = Mathf.RoundToInt(MinValue * 256);
    //            int maxValue = Mathf.RoundToInt(MaxValue * 256);
    //
    //            for (int i = InstanceList.Length - 1; i >= 0; i--)
    //            {
    //                VegetationInstance vegetationInstance = InstanceList[i];
    //                float3 localPosition = vegetationInstance.Position - texturePosition;
    //
    //                int x = Mathf.RoundToInt(localPosition.x / cellSize.x);
    //                int z = Mathf.RoundToInt(localPosition.z / cellSize.y);
    //
    //                if (x < 0 || x > Width - 1) continue;
    //                if (z < 0 || z > Height - 1) continue;
    //
    //                int value = 0;
    //
    //                switch (SelectedChannel)
    //                {
    //                    case 0:
    //                        value = RgbaNativeArray[x + (z * Width)].R;
    //                        break;
    //                    case 1:
    //                        value = RgbaNativeArray[x + (z * Width)].G;
    //                        break;
    //                    case 2:
    //                        value = RgbaNativeArray[x + (z * Width)].B;
    //                        break;
    //                    case 3:
    //                        value = RgbaNativeArray[x + (z * Width)].A;
    //                        break;
    //                }
    //
    //                if (Inverse)
    //                {
    //                    value = 256 - value;
    //                }
    //
    //                if (value >= minValue && value <= maxValue)
    //                {
    //                    InstanceList.RemoveAtSwapBack(i);
    //                }
    //            }
    //        }
    //    }


    [BurstCompile]
#if UNITY_2019_1_OR_NEWER
    public struct SampleRgbaChannelDensityMaskJob : IJobParallelForDefer
#else
    public struct SampleRgbaChannelDensityMaskJob : IJobParallelFor
#endif
    {
        public NativeArray<VegetationSpawnLocationInstance> SpawnLocations;
        [ReadOnly] public NativeArray<RGBABytes> RgbaNativeArray;

        public bool Inverse;
        public int SelectedChannel;

        public int Width;
        public int Height;
        public Rect TextureRect;

        public float DensityMultiplier;

        public float2 Repeat;

        public void Execute(int index)
        {
            VegetationSpawnLocationInstance spawnLocation = SpawnLocations[index];

            float2 cellSize = new float2(TextureRect.width / Width, TextureRect.height / Height);
            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width / 2f, 0,
                TextureRect.center.y - TextureRect.height / 2f);

            float3 localPosition = spawnLocation.Position - texturePosition;

            localPosition = new float3(localPosition.x / cellSize.x, 0, localPosition.z / cellSize.y);
            float3 normalizedPosition = new float3(localPosition.x / Width, 0, localPosition.z / Height);
            normalizedPosition = new float3(normalizedPosition.x * Repeat.x, 0, normalizedPosition.z * Repeat.y);
            normalizedPosition = math.frac(normalizedPosition);

            int x = Mathf.RoundToInt(normalizedPosition.x * Width);
            int z = Mathf.RoundToInt(normalizedPosition.z * Height);

            if (x < 0 || x > Width - 1) return;
            if (z < 0 || z > Height - 1) return;

            int value = 0;

            switch (SelectedChannel)
            {
                case 0:
                    value = RgbaNativeArray[x + (z * Width)].R;
                    break;
                case 1:
                    value = RgbaNativeArray[x + (z * Width)].G;
                    break;
                case 2:
                    value = RgbaNativeArray[x + (z * Width)].B;
                    break;
                case 3:
                    value = RgbaNativeArray[x + (z * Width)].A;
                    break;
            }

            if (Inverse)
            {
                value = 256 - value;
            }
           
            float densityFactor = (value / 256f) * DensityMultiplier;
            spawnLocation.SpawnChance = spawnLocation.SpawnChance * densityFactor;
            SpawnLocations[index] = spawnLocation;
        }
    }

    [BurstCompile]
#if UNITY_2019_1_OR_NEWER
    public struct SampleRgbaChannelScaleMaskJob : IJobParallelForDefer
#else
    public struct SampleRgbaChannelScaleMaskJob : IJobParallelFor
#endif
    {
        public NativeArray<float3> Position;
        public NativeArray<byte> Excluded;
        public NativeArray<float3> Scale;
        [ReadOnly] public NativeArray<RGBABytes> RgbaNativeArray;

        public bool Inverse;
        public int SelectedChannel;

        public int Width;
        public int Height;
        public Rect TextureRect;

        public float ScaleMultiplier;

        public float2 Repeat;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;

            float2 cellSize = new float2(TextureRect.width / Width, TextureRect.height / Height);
            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width / 2f, 0,
                TextureRect.center.y - TextureRect.height / 2f);

            float3 localPosition = Position[index] - texturePosition;

            localPosition = new float3(localPosition.x / cellSize.x, 0, localPosition.z / cellSize.y);
            float3 normalizedPosition = new float3(localPosition.x / Width, 0, localPosition.z / Height);
            normalizedPosition = new float3(normalizedPosition.x * Repeat.x, 0, normalizedPosition.z * Repeat.y);
            normalizedPosition = math.frac(normalizedPosition);

            int x = Mathf.RoundToInt(normalizedPosition.x * Width);
            int z = Mathf.RoundToInt(normalizedPosition.z * Height);

            if (x < 0 || x > Width - 1) return;
            if (z < 0 || z > Height - 1) return;

            int value = 0;

            switch (SelectedChannel)
            {
                case 0:
                    value = RgbaNativeArray[x + (z * Width)].R;
                    break;
                case 1:
                    value = RgbaNativeArray[x + (z * Width)].G;
                    break;
                case 2:
                    value = RgbaNativeArray[x + (z * Width)].B;
                    break;
                case 3:
                    value = RgbaNativeArray[x + (z * Width)].A;
                    break;
            }

            if (Inverse)
            {
                value = 256 - value;
            }

            float3 originalScale = Scale[index];
            float scaleFactor = (value / 256f) * ScaleMultiplier;

            Scale[index] = new float3(originalScale.x * scaleFactor, originalScale.y* scaleFactor, originalScale.z * scaleFactor);
        }
    }
    
    [BurstCompile]
#if UNITY_2019_1_OR_NEWER
    public struct SampleRgbaChannelExcludeMaskJob : IJobParallelForDefer
#else
    public struct SampleRgbaChannelExcludeMaskJob : IJobParallelFor
#endif
    {
        public NativeArray<float3> Position;
        public NativeArray<byte> Excluded;
        [ReadOnly] public NativeArray<RGBABytes> RgbaNativeArray;

        public bool Inverse;
        public int SelectedChannel;

        public int Width;
        public int Height;
        public Rect TextureRect;

        public float MinValue;
        public float MaxValue;

        public float2 Repeat;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            float2 cellSize = new float2(TextureRect.width / Width, TextureRect.height / Height);
            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width / 2f, 0,
                TextureRect.center.y - TextureRect.height / 2f);

            int minValue = Mathf.RoundToInt(MinValue * 256);
            int maxValue = Mathf.RoundToInt(MaxValue * 256);

            float3 localPosition = Position[index] - texturePosition;

            localPosition = new float3(localPosition.x / cellSize.x ,0,localPosition.z / cellSize.y);            
            float3 normalizedPosition = new float3(localPosition.x/Width, 0, localPosition.z/Height);                        
            normalizedPosition = new float3(normalizedPosition.x*Repeat.x,0, normalizedPosition.z* Repeat.y);
            normalizedPosition = math.frac(normalizedPosition);

            int x = Mathf.RoundToInt(normalizedPosition.x * Width); 
            int z = Mathf.RoundToInt(normalizedPosition.z * Height); 
            
            //int x = Mathf.RoundToInt((localPosition.x / Repeat.x) / cellSize.x);
            //int z = Mathf.RoundToInt((localPosition.z / Repeat.y) / cellSize.y);

            if (x < 0 || x > Width - 1) return;
            if (z < 0 || z > Height - 1) return;

            int value = 0;

            switch (SelectedChannel)
            {
                case 0:
                    value = RgbaNativeArray[x + (z * Width)].R;
                    break;
                case 1:
                    value = RgbaNativeArray[x + (z * Width)].G;
                    break;
                case 2:
                    value = RgbaNativeArray[x + (z * Width)].B;
                    break;
                case 3:
                    value = RgbaNativeArray[x + (z * Width)].A;
                    break;
            }

            if (Inverse)
            {
                value = 256 - value;
            }

            if (value >= minValue && value <= maxValue)
            {
                Excluded[index] = 1;
            }
        }
    }

    [System.Serializable]
    public class TextureMask
    {
        public Rect TextureRect;
        public Vector2 Repeat = Vector2.one;
        public Texture2D MaskTexture;

        private NativeArray<RGBABytes> _rgbaNativeArray;

        public JobHandle SampleIncludeMask(VegetationInstanceData instanceData, Rect spawnRect,
            TextureMaskType textureMaskType, TextureMaskRule textureMaskRule, JobHandle dependsOn)
        {
            if (!spawnRect.Overlaps(TextureRect)) return dependsOn;
            if (MaskTexture == null) return dependsOn;

            switch (textureMaskType)
            {
                case TextureMaskType.RGBAChannel:
                     _rgbaNativeArray = MaskTexture.GetRawTextureData<RGBABytes>();

                    bool inverse = textureMaskRule.GetBooleanPropertyValue("Inverse");
                    int selectedChannel = textureMaskRule.GetIntPropertyValue("ChannelSelector");

                    if (MaskTexture.format == TextureFormat.RGBA32)
                    {
                        selectedChannel--;
                        if (selectedChannel == -1) selectedChannel = 3;
                    }

                    SampleRgbaChannelIncludeMaskJob sampleRgbaChannelIncludeMaskJob =
                        new SampleRgbaChannelIncludeMaskJob();
                    sampleRgbaChannelIncludeMaskJob.Width = MaskTexture.width;
                    sampleRgbaChannelIncludeMaskJob.Height = MaskTexture.height;
                    sampleRgbaChannelIncludeMaskJob.Repeat = Repeat;
                     
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    sampleRgbaChannelIncludeMaskJob.Excluded = instanceData.Excluded.AsDeferredJobArray();
                    sampleRgbaChannelIncludeMaskJob.Position = instanceData.Position.AsDeferredJobArray();
                    sampleRgbaChannelIncludeMaskJob.TextureMaskData = instanceData.TextureMaskData.AsDeferredJobArray();
#else
                     sampleRgbaChannelIncludeMaskJob.Excluded = instanceData.Excluded.ToDeferredJobArray();
                     sampleRgbaChannelIncludeMaskJob.Position = instanceData.Position.ToDeferredJobArray();
                     sampleRgbaChannelIncludeMaskJob.TextureMaskData = instanceData.TextureMaskData.ToDeferredJobArray();    
#endif
                    sampleRgbaChannelIncludeMaskJob.RgbaNativeArray = _rgbaNativeArray;
                    sampleRgbaChannelIncludeMaskJob.SelectedChannel = selectedChannel;
                    sampleRgbaChannelIncludeMaskJob.Inverse = inverse;
                    sampleRgbaChannelIncludeMaskJob.TextureRect = TextureRect;
                    sampleRgbaChannelIncludeMaskJob.MinValue = textureMaskRule.MinDensity;
                    sampleRgbaChannelIncludeMaskJob.MaxValue = textureMaskRule.MaxDensity;
                    return sampleRgbaChannelIncludeMaskJob.Schedule(instanceData.Excluded, 32, dependsOn);
            }

            return dependsOn;
        }

        public JobHandle SampleExcludeMask(VegetationInstanceData instanceData, Rect spawnRect,
            TextureMaskType textureMaskType, TextureMaskRule textureMaskRule, JobHandle dependsOn)
        {
            if (!spawnRect.Overlaps(TextureRect)) return dependsOn;
            if (MaskTexture == null) return dependsOn;
            switch (textureMaskType)
            {
                case TextureMaskType.RGBAChannel:
                    _rgbaNativeArray = MaskTexture.GetRawTextureData<RGBABytes>();

                    bool inverse = textureMaskRule.GetBooleanPropertyValue("Inverse");
                    int selectedChannel = textureMaskRule.GetIntPropertyValue("ChannelSelector");

                    if (MaskTexture.format == TextureFormat.RGBA32)
                    {
                        selectedChannel--;
                        if (selectedChannel == -1) selectedChannel = 3;
                    }

                    SampleRgbaChannelExcludeMaskJob sampleRgbaChannelExcludeMaskJob = new SampleRgbaChannelExcludeMaskJob();
                    sampleRgbaChannelExcludeMaskJob.Width = MaskTexture.width;
                    sampleRgbaChannelExcludeMaskJob.Height = MaskTexture.height;
                    sampleRgbaChannelExcludeMaskJob.Repeat = Repeat;
                    
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    sampleRgbaChannelExcludeMaskJob.Excluded = instanceData.Excluded.AsDeferredJobArray();
                    sampleRgbaChannelExcludeMaskJob.Position = instanceData.Position.AsDeferredJobArray();  
#else
                    sampleRgbaChannelExcludeMaskJob.Excluded = instanceData.Excluded.ToDeferredJobArray();
                    sampleRgbaChannelExcludeMaskJob.Position = instanceData.Position.ToDeferredJobArray();      
#endif
                    sampleRgbaChannelExcludeMaskJob.RgbaNativeArray = _rgbaNativeArray;
                    sampleRgbaChannelExcludeMaskJob.SelectedChannel = selectedChannel;
                    sampleRgbaChannelExcludeMaskJob.Inverse = inverse;
                    sampleRgbaChannelExcludeMaskJob.TextureRect = TextureRect;
                    sampleRgbaChannelExcludeMaskJob.MinValue = textureMaskRule.MinDensity;
                    sampleRgbaChannelExcludeMaskJob.MaxValue = textureMaskRule.MaxDensity;
                    return sampleRgbaChannelExcludeMaskJob.Schedule(instanceData.Excluded,32,dependsOn);
            }

            return dependsOn;
        }


        public JobHandle SampleScaleMask(VegetationInstanceData instanceData, Rect spawnRect,
          TextureMaskType textureMaskType, TextureMaskRule textureMaskRule, JobHandle dependsOn)
        {
            if (!spawnRect.Overlaps(TextureRect)) return dependsOn;
            if (MaskTexture == null) return dependsOn;
            switch (textureMaskType)
            {
                case TextureMaskType.RGBAChannel:
                    _rgbaNativeArray = MaskTexture.GetRawTextureData<RGBABytes>();

                    bool inverse = textureMaskRule.GetBooleanPropertyValue("Inverse");
                    int selectedChannel = textureMaskRule.GetIntPropertyValue("ChannelSelector");

                    if (MaskTexture.format == TextureFormat.RGBA32)
                    {
                        selectedChannel--;
                        if (selectedChannel == -1) selectedChannel = 3;
                    }

                    SampleRgbaChannelScaleMaskJob sampleRgbaChannelScaleMaskJob = new SampleRgbaChannelScaleMaskJob();
                    sampleRgbaChannelScaleMaskJob.Width = MaskTexture.width;
                    sampleRgbaChannelScaleMaskJob.Height = MaskTexture.height;
                    sampleRgbaChannelScaleMaskJob.Repeat = Repeat;

#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    sampleRgbaChannelScaleMaskJob.Excluded = instanceData.Excluded.AsDeferredJobArray();
                    sampleRgbaChannelScaleMaskJob.Position = instanceData.Position.AsDeferredJobArray(); 
                    sampleRgbaChannelScaleMaskJob.Scale = instanceData.Scale.AsDeferredJobArray();
#else
                    sampleRgbaChannelScaleMaskJob.Scale = instanceData.Scale.ToDeferredJobArray();
                    sampleRgbaChannelScaleMaskJob.Excluded = instanceData.Excluded.ToDeferredJobArray();
                    sampleRgbaChannelScaleMaskJob.Position = instanceData.Position.ToDeferredJobArray();
#endif
                    sampleRgbaChannelScaleMaskJob.RgbaNativeArray = _rgbaNativeArray;
                    sampleRgbaChannelScaleMaskJob.SelectedChannel = selectedChannel;
                    sampleRgbaChannelScaleMaskJob.Inverse = inverse;
                    sampleRgbaChannelScaleMaskJob.TextureRect = TextureRect;
                    sampleRgbaChannelScaleMaskJob.ScaleMultiplier = textureMaskRule.ScaleMultiplier;
                    return sampleRgbaChannelScaleMaskJob.Schedule(instanceData.Excluded, 32, dependsOn);
            }

            return dependsOn;
        }

        public JobHandle SampleDensityMask(VegetationInstanceData instanceData, Rect spawnRect,
        TextureMaskType textureMaskType, TextureMaskRule textureMaskRule, JobHandle dependsOn)
        {
            if (!spawnRect.Overlaps(TextureRect)) return dependsOn;
            if (MaskTexture == null) return dependsOn;
            switch (textureMaskType)
            {
                case TextureMaskType.RGBAChannel:
                    _rgbaNativeArray = MaskTexture.GetRawTextureData<RGBABytes>();

                    bool inverse = textureMaskRule.GetBooleanPropertyValue("Inverse");
                    int selectedChannel = textureMaskRule.GetIntPropertyValue("ChannelSelector");

                    if (MaskTexture.format == TextureFormat.RGBA32)
                    {
                        selectedChannel--;
                        if (selectedChannel == -1) selectedChannel = 3;
                    }

                    SampleRgbaChannelDensityMaskJob sampleRgbaChannelDensityMaskJob = new SampleRgbaChannelDensityMaskJob();
                    sampleRgbaChannelDensityMaskJob.Width = MaskTexture.width;
                    sampleRgbaChannelDensityMaskJob.Height = MaskTexture.height;
                    sampleRgbaChannelDensityMaskJob.Repeat = Repeat;

#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    sampleRgbaChannelDensityMaskJob.SpawnLocations = instanceData.SpawnLocations.AsDeferredJobArray();
#else
                    sampleRgbaChannelDensityMaskJob.SpawnLocations = instanceData.SpawnLocations.ToDeferredJobArray();
#endif
                    sampleRgbaChannelDensityMaskJob.RgbaNativeArray = _rgbaNativeArray;
                    sampleRgbaChannelDensityMaskJob.SelectedChannel = selectedChannel;
                    sampleRgbaChannelDensityMaskJob.Inverse = inverse;
                    sampleRgbaChannelDensityMaskJob.TextureRect = TextureRect;
                    sampleRgbaChannelDensityMaskJob.DensityMultiplier = textureMaskRule.DensityMultiplier;
                    return sampleRgbaChannelDensityMaskJob.Schedule(instanceData.SpawnLocations, 32, dependsOn);
            }

            return dependsOn;
        }

        public void Dispose()
        {
        }
    }
}