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
//    public struct IncludeVegetationMaskLineJob : IJob
//    {
//        public NativeList<VegetationInstance> VegetationInstanceList;
//        public float Denisty;
//        public float Scale;
//        public LineSegment2D LineSegment2D;
//        public float Width;
//
//        public void Execute()
//        {
//            for (int i = VegetationInstanceList.Length - 1; i >= 0; i--)
//            {
//                VegetationInstance vegetationInstance = VegetationInstanceList[i];
//                Vector2 position = new Vector2(vegetationInstance.Position.x, vegetationInstance.Position.z);
//                if (LineSegment2D.DistanceToPoint(position) < Width / 2f)
//                {
//                    vegetationInstance.VegetationMaskScale = math.max(vegetationInstance.VegetationMaskScale, Scale);
//                    vegetationInstance.VegetationMaskDensity =
//                        math.max(vegetationInstance.VegetationMaskDensity, Denisty);
//                    VegetationInstanceList[i] = vegetationInstance;
//                }
//            }
//        }
//        
//        
//    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct IncludeVegetationMaskLineJob : IJobParallelForDefer
#else
    public struct IncludeVegetationMaskLineJob : IJobParallelFor          
#endif

    {
        public NativeArray<byte> Excluded;
        public NativeArray<float3> Position;
        public NativeArray<float> VegetationMaskScale;
        public NativeArray<float> VegetationMaskDensity;
        public float Denisty;
        public float Scale;
        public LineSegment2D LineSegment2D;
        public float Width;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;

            Vector2 position = new Vector2(Position[index].x, Position[index].z);
            if (LineSegment2D.DistanceToPoint(position) < Width / 2f)
            {
                VegetationMaskScale[index] = math.max(VegetationMaskScale[index], Scale);
                VegetationMaskDensity[index] =
                    math.max(VegetationMaskDensity[index], Denisty);
            }
        }
    }

//    [BurstCompile(CompileSynchronously = true)]
//    public struct SampleVegetatiomMaskLineJob : IJob
//    {
//        public NativeList<VegetationInstance> VegetationInstanceList;
//        public LineSegment2D LineSegment2D;
//        public float AdditionalWidth;
//        public float AdditionalWidthMax;
//        public float NoiseScale;
//        public float Width;
//
//        public void Execute()
//        {
//            for (int i = VegetationInstanceList.Length - 1; i >= 0; i--)
//            {
//                VegetationInstance vegetationInstance = VegetationInstanceList[i];
//                Vector2 position = new Vector2(vegetationInstance.Position.x, vegetationInstance.Position.z);
//
//                float perlin = noise.snoise(new float2(position.x / NoiseScale, position.y / NoiseScale));
//                perlin += 1f;
//                perlin /= 2f;
//                perlin = math.clamp(perlin, 0, 1);
//
//                float additionalWidth = math.lerp(AdditionalWidth, AdditionalWidthMax, perlin);
//
//                if (LineSegment2D.DistanceToPoint(position) < (additionalWidth + Width/2f))
//                {
//                    VegetationInstanceList.RemoveAtSwapBack(i);
//                }
//            }
//
//
//        }
//    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct SampleVegetatiomMaskLineJob : IJobParallelForDefer
#else
    public struct SampleVegetatiomMaskLineJob : IJobParallelFor               
#endif
    {
        public NativeArray<float3> Position;
        public NativeArray<byte> Excluded;
        public LineSegment2D LineSegment2D;
        public float AdditionalWidth;
        public float AdditionalWidthMax;
        public float NoiseScale;
        public float Width;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;

            Vector2 position = new Vector2(Position[index].x, Position[index].z);

            float perlin = noise.snoise(new float2(position.x / NoiseScale, position.y / NoiseScale));
            perlin += 1f;
            perlin /= 2f;
            perlin = math.clamp(perlin, 0, 1);

            float additionalWidth = math.lerp(AdditionalWidth, AdditionalWidthMax, perlin);

            if (LineSegment2D.DistanceToPoint(position) < (additionalWidth + Width / 2f))
            {
                Excluded[index] = 1;
            }
        }
    }

    public class LineMaskArea : BaseMaskArea
    {
        LineSegment2D _line2D;

        private Vector3 _point1;
        private Vector3 _point2;
        private Vector3 _centerPoint;
        private float _width;

        public void SetLineData(Vector3 point1, Vector3 point2, float width)
        {
            _centerPoint = Vector3.Lerp(point1, point2, 0.5f);
            _point1 = point1;
            _point2 = point2;
            _width = width;
            _line2D = new LineSegment2D(new Vector3(point1.x, point1.z), new Vector3(point2.x, point2.z));

            MaskBounds = GetMaskBounds();
        }

        public override bool HasVegetationTypeIndex(VegetationTypeIndex vegetationTypeIndex)
        {
            for (int i = 0; i <= VegetationTypeList.Count - 1; i++)
            {
                if (VegetationTypeList[i].Index == vegetationTypeIndex) return true;
            }

            return false;
        }

        public override JobHandle SampleMask(VegetationInstanceData instanceData, VegetationType vegetationType,
            JobHandle dependsOn)
        {
            if (!ExcludeVegetationType(vegetationType)) return dependsOn;

            SampleVegetatiomMaskLineJob sampleVegetatiomMaskLineJob =
                new SampleVegetatiomMaskLineJob
                {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    Position = instanceData.Position.AsDeferredJobArray(),
                    Excluded = instanceData.Excluded.AsDeferredJobArray(),
#else
                    Position = instanceData.Position.ToDeferredJobArray(),
                    Excluded = instanceData.Excluded.ToDeferredJobArray(),                
#endif

                    LineSegment2D = _line2D,
                    Width = _width,
                    AdditionalWidth = GetAdditionalWidth(vegetationType),
                    AdditionalWidthMax = GetAdditionalWidthMax(vegetationType),
                    NoiseScale = GetPerlinScale(vegetationType)
                };

            dependsOn = sampleVegetatiomMaskLineJob.Schedule(instanceData.Excluded, 32, dependsOn);
            return dependsOn;
        }

        public override JobHandle SampleIncludeVegetationMask(VegetationInstanceData instanceData,
            VegetationTypeIndex vegetationTypeIndex,
            JobHandle dependsOn)
        {
            VegetationTypeSettings vegetationTypeSettings = GetVegetationTypeSettings(vegetationTypeIndex);

            if (vegetationTypeSettings != null)
            {
                IncludeVegetationMaskLineJob includeVegetationMaskLineJob =
                    new IncludeVegetationMaskLineJob
                    {
                        
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                        Excluded = instanceData.Excluded.AsDeferredJobArray(),
                        Position = instanceData.Position.AsDeferredJobArray(),
                        VegetationMaskDensity = instanceData.VegetationMaskDensity.AsDeferredJobArray(),
                        VegetationMaskScale = instanceData.VegetationMaskScale.AsDeferredJobArray(),
#else
                        Excluded = instanceData.Excluded.ToDeferredJobArray(),
                        Position = instanceData.Position.ToDeferredJobArray(),   
                        VegetationMaskDensity = instanceData.VegetationMaskDensity.ToDeferredJobArray(),
                        VegetationMaskScale = instanceData.VegetationMaskScale.ToDeferredJobArray(),
#endif
                        


                        Denisty = vegetationTypeSettings.Density,
                        Scale = vegetationTypeSettings.Size,
                        LineSegment2D = _line2D,
                        Width = _width
                    };
                dependsOn = includeVegetationMaskLineJob.Schedule(instanceData.Excluded, 32, dependsOn);
            }

            return dependsOn;
        }

        //public override bool Contains(Vector3 point, VegetationType vegetationType, bool useAdditionalDistance, bool useExcludeFilter)
        //{
        //    float additionalWidth = 0f;

        //    if (useExcludeFilter)
        //    {
        //        switch (vegetationType)
        //        {
        //            case VegetationType.Grass:
        //                if (!RemoveGrass) return false;
        //                additionalWidth = Mathf.Lerp(AdditionalGrassWidth, AdditionalGrassWidthMax, SamplePerlinNoise(point,NoiseScaleGrass));
        //                break;
        //            case VegetationType.Objects:
        //                if (!RemoveObjects) return false;
        //                //_additionalWidth = additionalObjectWidth;
        //                additionalWidth = Mathf.Lerp(AdditionalObjectWidth, AdditionalObjectWidthMax, SamplePerlinNoise(point, NoiseScaleGrass));
        //                break;
        //            case VegetationType.Plant:
        //                if (!RemovePlants) return false;
        //                //_additionalWidth = additionalPlantWidth;
        //                additionalWidth = Mathf.Lerp(AdditionalPlantWidth, AdditionalPlantWidthMax, SamplePerlinNoise(point, NoiseScaleGrass));

        //                break;
        //            case VegetationType.LargeObjects:
        //                if (!RemoveLargeObjects) return false;
        //                //_additionalWidth = additionalLargeObjectWidth;
        //                additionalWidth = Mathf.Lerp(AdditionalLargeObjectWidth, AdditionalLargeObjectWidthMax, SamplePerlinNoise(point, NoiseScaleGrass));

        //                break;
        //            case VegetationType.Tree:
        //                if (!RemoveTrees) return false;
        //                //_additionalWidth = additionalTreeWidth;
        //                additionalWidth = Mathf.Lerp(AdditionalTreeWidth, AdditionalTreeWidthMax, SamplePerlinNoise(point, NoiseScaleGrass));

        //                break;
        //        }
        //    }
        //    else
        //    {
        //        switch (vegetationType)
        //        {
        //            case VegetationType.Grass:
        //                additionalWidth = Mathf.Lerp(AdditionalGrassWidth, AdditionalGrassWidthMax, SamplePerlinNoise(point,NoiseScaleGrass));
        //                break;
        //            case VegetationType.Objects:
        //                additionalWidth = Mathf.Lerp(AdditionalObjectWidth, AdditionalObjectWidthMax, SamplePerlinNoise(point, NoiseScaleGrass));
        //                break;
        //            case VegetationType.Plant:
        //                additionalWidth = Mathf.Lerp(AdditionalPlantWidth, AdditionalPlantWidthMax, SamplePerlinNoise(point, NoiseScaleGrass));
        //                break;
        //            case VegetationType.LargeObjects:
        //                additionalWidth = Mathf.Lerp(AdditionalLargeObjectWidth, AdditionalLargeObjectWidthMax, SamplePerlinNoise(point, NoiseScaleGrass));
        //                break;
        //            case VegetationType.Tree:
        //                additionalWidth = Mathf.Lerp(AdditionalTreeWidth, AdditionalTreeWidthMax, SamplePerlinNoise(point, NoiseScaleGrass));
        //                break;
        //        }
        //    }
        //    UnityEngine.Vector2 point2D = new UnityEngine.Vector2(point.x, point.z);

        //    if (!useAdditionalDistance) additionalWidth = 0;

        //    float distance = _line2D.DistanceTo(point2D);
        //    if (distance < _width/2f + additionalWidth/2f)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public Bounds GetMaskBounds()
        {
            Bounds expandedBounds = new Bounds(_centerPoint, new Vector3(1, 1, 1));
            expandedBounds.Encapsulate(_point1);
            expandedBounds.Encapsulate(_point2);
            expandedBounds.Expand(_width);

            expandedBounds.Expand(GetMaxAdditionalDistance());
            return expandedBounds;
        }
    }
}