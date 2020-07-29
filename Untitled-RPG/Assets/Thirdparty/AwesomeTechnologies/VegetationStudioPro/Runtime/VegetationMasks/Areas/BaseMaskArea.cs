using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies
{
//    [BurstCompile(CompileSynchronously = true)]
       //    public struct ProcessIncludeVegetationMaskJob : IJob
       //    {
       //        public NativeList<VegetationInstance> VegetationInstanceList;
       //        [ReadOnly]
       //        public NativeArray<float> RandomNumbers;
       //
       //        public void Execute()
       //        {
       //            for (int i = VegetationInstanceList.Length - 1; i >= 0; i--)
       //            {
       //                VegetationInstance vegetationInstance = VegetationInstanceList[i];
       //                if (RandomCutoff(vegetationInstance.VegetationMaskDensity, vegetationInstance.RandomNumberIndex))
       //                {
       //                    VegetationInstanceList.RemoveAtSwapBack(i);
       //                }
       //                else
       //                {
       //                    vegetationInstance.Scale *= vegetationInstance.VegetationMaskScale;
       //                    VegetationInstanceList[i] = vegetationInstance;
       //                }
       //            }
       //        }
       //
       //        public float RandomRange(int randomNumberIndex, float min, float max)
       //        {
       //            while (randomNumberIndex > 9999)
       //                randomNumberIndex = randomNumberIndex - 10000;
       //
       //            return Mathf.Lerp(min, max, RandomNumbers[randomNumberIndex]);
       //        }
       //
       //        private bool RandomCutoff(float value, int randomNumberIndex)
       //        {
       //            var randomNumber = RandomRange(randomNumberIndex, 0, 1);
       //            return !(value > randomNumber);
       //        }
       //    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct ProcessIncludeVegetationMaskJob : IJobParallelForDefer
#else
    public struct ProcessIncludeVegetationMaskJob : IJobParallelFor    
#endif
    {

        public NativeArray<byte> Excluded;
        public NativeArray<float> VegetationMaskDensity;
        public NativeArray<float> VegetationMaskScale;
        public NativeArray<float3> Scale;
        public NativeArray<int> RandomNumberIndex;
        [ReadOnly]
        public NativeArray<float> RandomNumbers;

        public void Execute(int index)
        {
    
                //VegetationInstance vegetationInstance = VegetationInstanceList[index];
                if (Excluded[index] == 1) return;

                if (RandomCutoff(VegetationMaskDensity[index], RandomNumberIndex[index]))
                {
                    Excluded[index] = 1;                    
                }
                else
                {
                    Scale[index] *= VegetationMaskScale[index];
                }
            
        }

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
    }
    
    public class BaseMaskArea
    {
        public Bounds MaskBounds;

        public bool RemoveGrass = true;
        public bool RemovePlants = true;
        public bool RemoveTrees = true;
        public bool RemoveObjects = true;
        public bool RemoveLargeObjects = true;
        public float AdditionalGrassWidth = 0;
        public float AdditionalPlantWidth = 0;
        public float AdditionalTreeWidth = 0;
        public float AdditionalObjectWidth = 0;
        public float AdditionalLargeObjectWidth = 0;

        public float AdditionalGrassWidthMax = 0;
        public float AdditionalPlantWidthMax = 0;
        public float AdditionalTreeWidthMax = 0;
        public float AdditionalObjectWidthMax = 0;
        public float AdditionalLargeObjectWidthMax = 0;

        public float NoiseScaleGrass = 5;
        public float NoiseScalePlant = 5;
        public float NoiseScaleTree = 5;
        public float NoiseScaleObject = 5;
        public float NoiseScaleLargeObject = 5;

        public string VegetationItemID = "";

        public List<VegetationTypeSettings> VegetationTypeList = new List<VegetationTypeSettings>();

        public delegate void MultionMaskDeleteDelegate(BaseMaskArea baseMaskArea);

        public MultionMaskDeleteDelegate OnMaskDeleteDelegate;

        //public virtual bool Contains(Vector3 point, VegetationType vegetationType, bool useAdditionalDistance, bool useExcludeFilter)
        //{
        //    return false;
        //}


        public virtual JobHandle SampleMask(VegetationInstanceData instanceData,
            VegetationType vegetationType,
            JobHandle dependsOn)
        {
            return dependsOn;
        }

        public virtual JobHandle SampleIncludeVegetationMask(VegetationInstanceData instanceData,
            VegetationTypeIndex vegetationTypeIndex,
            JobHandle dependsOn)
        {
            return dependsOn;
        }

        public virtual bool HasVegetationTypeIndex(VegetationTypeIndex vegetationTypeIndex)
        {
            return false;
        }

        public float GetAdditionalWidth(VegetationType vegetationType)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return AdditionalGrassWidth;
                case VegetationType.Plant:
                    return AdditionalPlantWidth;
                case VegetationType.Tree:
                    return AdditionalTreeWidth;
                case VegetationType.Objects:
                    return AdditionalObjectWidth;
                case VegetationType.LargeObjects:
                    return AdditionalLargeObjectWidth;
            }

            return 0;
        }

        public VegetationTypeSettings GetVegetationTypeSettings(VegetationTypeIndex vegetationTypeIndex)
        {
            for (int i = 0; i <= VegetationTypeList.Count - 1; i++)
            {
                if (VegetationTypeList[i].Index == vegetationTypeIndex) return VegetationTypeList[i];
            }

            return null;
        }

        public bool ExcludeVegetationType(VegetationType vegetationType)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return RemoveGrass;
                case VegetationType.Plant:
                    return RemovePlants;
                case VegetationType.Tree:
                    return RemoveTrees;
                case VegetationType.Objects:
                    return RemoveObjects;
                case VegetationType.LargeObjects:
                    return RemoveLargeObjects;
            }

            return false;
        }

        public float GetAdditionalWidthMax(VegetationType vegetationType)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return AdditionalGrassWidthMax;
                case VegetationType.Plant:
                    return AdditionalPlantWidthMax;
                case VegetationType.Tree:
                    return AdditionalTreeWidthMax;
                case VegetationType.Objects:
                    return AdditionalObjectWidthMax;
                case VegetationType.LargeObjects:
                    return AdditionalLargeObjectWidthMax;
            }

            return 0;
        }

        public float GetPerlinScale(VegetationType vegetationType)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return NoiseScaleGrass;
                case VegetationType.Plant:
                    return NoiseScalePlant;
                case VegetationType.Tree:
                    return NoiseScaleTree;
                case VegetationType.Objects:
                    return NoiseScaleObject;
                case VegetationType.LargeObjects:
                    return NoiseScaleLargeObject;
            }

            return 0;
        }

        //public virtual bool ContainsMask(Vector3 point, VegetationType vegetationType, VegetationTypeIndex vegetationTypeIndex, ref float size, ref float density)
        //{
        //    bool hasVegetationType = HasVegetationType(vegetationTypeIndex,ref size,ref density);
        //    if (!hasVegetationType) return false;
        //    return Contains(point, vegetationType, false, false);
        //}

        //public bool HasVegetationType(VegetationTypeIndex vegetationTypeIndex, ref float size, ref float density)
        //{
        //    for (int i = 0; i <= VegetationTypeList.Count - 1; i++)
        //    {
        //        if (VegetationTypeList[i].Index == vegetationTypeIndex)
        //        {
        //            size = VegetationTypeList[i].Size;
        //            density = VegetationTypeList[i].Density;
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        public void CallDeleteEvent()
        {
            if (OnMaskDeleteDelegate != null) OnMaskDeleteDelegate(this);
        }

        public float GetMaxAdditionalDistance()
        {
            float[] values =
            {
                    AdditionalGrassWidth, AdditionalPlantWidth, AdditionalTreeWidth, AdditionalObjectWidth,
                    AdditionalLargeObjectWidth, AdditionalGrassWidthMax, AdditionalPlantWidthMax,
                    AdditionalTreeWidthMax, AdditionalObjectWidthMax, AdditionalLargeObjectWidthMax
                };
            return Mathf.Max(values) * 1.5f;
        }

        public float SamplePerlinNoise(Vector3 point, float perlinNoiceScale)
        {
            return Mathf.PerlinNoise((point.x) / perlinNoiceScale, (point.z) / perlinNoiceScale);
        }

        public virtual void Dispose()
        {

        }
    }
}
