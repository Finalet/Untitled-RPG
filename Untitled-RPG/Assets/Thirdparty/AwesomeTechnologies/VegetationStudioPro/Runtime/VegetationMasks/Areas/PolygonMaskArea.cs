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
//    public struct IncludeVegetatiomMaskPolygonJob : IJob
//    {
//        public NativeList<VegetationInstance> VegetationInstanceList;
//        [ReadOnly] public NativeArray<Vector2> PolygonArray;
//        public float Denisty;
//        public float Scale;
//
//        public void Execute()
//        {
//            for (int i = VegetationInstanceList.Length - 1; i >= 0; i--)
//            {
//                VegetationInstance vegetationInstance = VegetationInstanceList[i];
//                Vector2 position = new Vector2(vegetationInstance.Position.x, vegetationInstance.Position.z);
//                if (IsInPolygon(position))
//                {
//                    vegetationInstance.VegetationMaskScale = math.max(vegetationInstance.VegetationMaskScale, Scale);
//                    vegetationInstance.VegetationMaskDensity =
//                        math.max(vegetationInstance.VegetationMaskDensity, Denisty);
//                    VegetationInstanceList[i] = vegetationInstance;
//                }
//            }
//        }
//
//        bool IsInPolygon(Vector2 p)
//        {
//            bool inside = false;
//
//            if (PolygonArray.Length < 3)
//            {
//                return false;
//            }
//
//            var oldPoint = new Vector2(
//                PolygonArray[PolygonArray.Length - 1].x, PolygonArray[PolygonArray.Length - 1].y);
//
//            for (int i = 0; i < PolygonArray.Length; i++)
//            {
//                var newPoint = new Vector2(PolygonArray[i].x, PolygonArray[i].y);
//
//                Vector2 p1;
//                Vector2 p2;
//                if (newPoint.x > oldPoint.x)
//                {
//                    p1 = oldPoint;
//                    p2 = newPoint;
//                }
//                else
//                {
//                    p1 = newPoint;
//                    p2 = oldPoint;
//                }
//
//                if ((newPoint.x < p.x) == (p.x <= oldPoint.x)
//                    && (p.y - (long) p1.y) * (p2.x - p1.x)
//                    < (p2.y - (long) p1.y) * (p.x - p1.x))
//                {
//                    inside = !inside;
//                }
//
//                oldPoint = newPoint;
//            }
//
//            return inside;
//        }
//    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct IncludeVegetatiomMaskPolygonJob : IJobParallelForDefer
#else
    public struct IncludeVegetatiomMaskPolygonJob : IJobParallelFor
#endif
    {
        public NativeArray<byte> Excluded;
        public NativeArray<float3> Position;
        public NativeArray<float> VegetationMaskScale;
        public NativeArray<float> VegetationMaskDensity;
        [ReadOnly] public NativeArray<Vector2> PolygonArray;
        public float Denisty;
        public float Scale;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            Vector2 position = new Vector2(Position[index].x, Position[index].z);
            if (IsInPolygon(position))
            {
                VegetationMaskScale[index] = math.max(VegetationMaskScale[index], Scale);
                VegetationMaskDensity[index] =
                    math.max(VegetationMaskDensity[index], Denisty);
            }
        }

        bool IsInPolygon(Vector2 p)
        {
            bool inside = false;

            if (PolygonArray.Length < 3)
            {
                return false;
            }

            var oldPoint = new Vector2(
                PolygonArray[PolygonArray.Length - 1].x, PolygonArray[PolygonArray.Length - 1].y);

            for (int i = 0; i < PolygonArray.Length; i++)
            {
                var newPoint = new Vector2(PolygonArray[i].x, PolygonArray[i].y);

                Vector2 p1;
                Vector2 p2;
                if (newPoint.x > oldPoint.x)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.x < p.x) == (p.x <= oldPoint.x)
                    && (p.y - (long) p1.y) * (p2.x - p1.x)
                    < (p2.y - (long) p1.y) * (p.x - p1.x))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct SampleVegetatiomMaskPolygonJob : IJobParallelForDefer
#else
    public struct SampleVegetatiomMaskPolygonJob : IJobParallelFor
#endif
    {
        public NativeArray<float3> Position;
        public NativeArray<byte> Excluded;
        [ReadOnly] public NativeArray<Vector2> PolygonArray;
        [ReadOnly] public NativeArray<LineSegment2D> SegmentArray;
        public float AdditionalWidth;
        public float AdditionalWidthMax;
        public float NoiseScale;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            Vector2 position = new Vector2(Position[index].x, Position[index].z);

            float perlin = noise.snoise(new float2(position.x / NoiseScale, position.y / NoiseScale));
            perlin += 1f;
            perlin /= 2f;
            perlin = math.clamp(perlin, 0, 1);

            float additionalWidth = math.lerp(AdditionalWidth, AdditionalWidthMax, perlin);

            if (IsInPolygon(position) || (DistanceToEdge(position) < additionalWidth))
            {
                Excluded[index] = 1;
            }
        }

        float DistanceToEdge(Vector2 point)
        {
            float distance = float.MaxValue;
            for (int i = 0; i < SegmentArray.Length; i++)
            {
                distance = math.min(distance, SegmentArray[i].DistanceToPoint(point));
            }

            return distance;
        }

        bool IsInPolygon(Vector2 p)
        {
            bool inside = false;

            if (PolygonArray.Length < 3)
            {
                return false;
            }

            var oldPoint = new Vector2(
                PolygonArray[PolygonArray.Length - 1].x, PolygonArray[PolygonArray.Length - 1].y);

            for (int i = 0; i < PolygonArray.Length; i++)
            {
                var newPoint = new Vector2(PolygonArray[i].x, PolygonArray[i].y);

                Vector2 p1;
                Vector2 p2;
                if (newPoint.x > oldPoint.x)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.x < p.x) == (p.x <= oldPoint.x)
                    && (p.y - (long) p1.y) * (p2.x - p1.x)
                    < (p2.y - (long) p1.y) * (p.x - p1.x))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }
    }

//     [BurstCompile(CompileSynchronously = true)]
//    public struct SampleVegetatiomMaskPolygonJob : IJob
//    {
//        public NativeList<VegetationInstance> VegetationInstanceList;
//        [ReadOnly]
//        public NativeArray<Vector2> PolygonArray;
//        [ReadOnly]
//        public NativeArray<LineSegment2D> SegmentArray;
//        public float AdditionalWidth;
//        public float AdditionalWidthMax;
//        public float NoiseScale;
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
//                if (IsInPolygon(position) || (DistanceToEdge(position) < additionalWidth))
//                {
//                    VegetationInstanceList.RemoveAtSwapBack(i);
//                }               
//            }
//        }
//
//        float DistanceToEdge(Vector2 point)
//        {
//            float distance = float.MaxValue;
//            for (int i = 0; i < SegmentArray.Length; i++)
//            {
//                distance = math.min(distance, SegmentArray[i].DistanceToPoint(point));
//            }
//
//            return distance;
//        }
//
//        bool IsInPolygon(Vector2 p)
//        {
//            bool inside = false;
//
//            if (PolygonArray.Length < 3)
//            {
//                return false;
//            }
//
//            var oldPoint = new Vector2(
//                PolygonArray[PolygonArray.Length - 1].x, PolygonArray[PolygonArray.Length - 1].y);
//
//            for (int i = 0; i < PolygonArray.Length; i++)
//            {
//                var newPoint = new Vector2(PolygonArray[i].x, PolygonArray[i].y);
//
//                Vector2 p1;
//                Vector2 p2;
//                if (newPoint.x > oldPoint.x)
//                {
//                    p1 = oldPoint;
//                    p2 = newPoint;
//                }
//                else
//                {
//                    p1 = newPoint;
//                    p2 = oldPoint;
//                }
//
//                if ((newPoint.x < p.x) == (p.x <= oldPoint.x)
//                    && (p.y - (long)p1.y) * (p2.x - p1.x)
//                    < (p2.y - (long)p1.y) * (p.x - p1.x))
//                {
//                    inside = !inside;
//                }
//
//                oldPoint = newPoint;
//            }
//            return inside;
//        }
//    }

    public class PolygonMaskArea : BaseMaskArea
    {
        private Vector2[] _points2D;
        private Vector3[] _points3D;
        private LineSegment2D[] _segments;

        public NativeArray<Vector2> PolygonArray;
        public NativeArray<LineSegment2D> SegmentArray;

        public void AddPolygon(List<Vector3> pointList)
        {
            _points2D = new Vector2[pointList.Count];
            _points3D = new Vector3[pointList.Count];
            for (int i = 0; i <= pointList.Count - 1; i++)
            {
                _points2D[i] = new Vector2(pointList[i].x, pointList[i].z);
                _points3D[i] = pointList[i];
            }

            MaskBounds = GetMaskBounds();
            if (PolygonArray.IsCreated) PolygonArray.Dispose();
            PolygonArray = new NativeArray<Vector2>(_points2D.Length, Allocator.Persistent);
            PolygonArray.CopyFromFast(_points2D);
            CreateSegments();
        }

        void CreateSegments()
        {
            _segments = new LineSegment2D[_points2D.Length];
            for (int i = 0; i <= _points2D.Length - 2; i++)
            {
                LineSegment2D lineSegment2D = new LineSegment2D(_points2D[i], _points2D[i + 1]);
                _segments[i] = lineSegment2D;
            }

            if (_points2D.Length > 0)
            {
                LineSegment2D lineSegment2D = new LineSegment2D(_points2D[0], _points2D[_points2D.Length - 1]);
                _segments[_points2D.Length - 1] = lineSegment2D;
            }

            if (SegmentArray.IsCreated) SegmentArray.Dispose();
            SegmentArray = new NativeArray<LineSegment2D>(_segments.Length, Allocator.Persistent);
            SegmentArray.CopyFromFast(_segments);
        }

        public override JobHandle SampleMask(VegetationInstanceData instanceData, VegetationType vegetationType,
            JobHandle dependsOn)
        {
            if (!ExcludeVegetationType(vegetationType)) return dependsOn;

            SampleVegetatiomMaskPolygonJob sampleVegetatiomMaskPolygonJob =
                new SampleVegetatiomMaskPolygonJob
                {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    Position = instanceData.Position.AsDeferredJobArray(),
                    Excluded = instanceData.Excluded.AsDeferredJobArray(),
#else
                    Position = instanceData.Position.ToDeferredJobArray(),
                    Excluded = instanceData.Excluded.ToDeferredJobArray(),             
#endif
                    PolygonArray = PolygonArray,
                    SegmentArray = SegmentArray,
                    AdditionalWidth = GetAdditionalWidth(vegetationType),
                    AdditionalWidthMax = GetAdditionalWidthMax(vegetationType),
                    NoiseScale = GetPerlinScale(vegetationType)
                };

            dependsOn = sampleVegetatiomMaskPolygonJob.Schedule(instanceData.Excluded, 32, dependsOn);
            return dependsOn;
        }

        public override JobHandle SampleIncludeVegetationMask(VegetationInstanceData instanceData,
            VegetationTypeIndex vegetationTypeIndex,
            JobHandle dependsOn)
        {
            VegetationTypeSettings vegetationTypeSettings = GetVegetationTypeSettings(vegetationTypeIndex);

            if (vegetationTypeSettings != null)
            {
                IncludeVegetatiomMaskPolygonJob includeVegetatiomMaskPolygonJob =
                    new IncludeVegetatiomMaskPolygonJob
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
                        PolygonArray = PolygonArray
                    };

               dependsOn = includeVegetatiomMaskPolygonJob.Schedule(instanceData.Excluded,32,dependsOn);
            }
            return dependsOn;
        }

        public override bool HasVegetationTypeIndex(VegetationTypeIndex vegetationTypeIndex)
        {
            for (int i = 0; i <= VegetationTypeList.Count - 1; i++)
            {
                if (VegetationTypeList[i].Index == vegetationTypeIndex) return true;
            }

            return false;
        }

        private Bounds GetMaskBounds()
        {
            var expandedBounds = _points3D.Length > 0
                ? new Bounds(_points3D[0], new Vector3(1, 1, 1))
                : new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));

            for (int i = 0; i <= _points3D.Length - 1; i++)
            {
                expandedBounds.Encapsulate(_points3D[i]);
            }

            expandedBounds.Expand(GetMaxAdditionalDistance());
            return expandedBounds;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (PolygonArray.IsCreated) PolygonArray.Dispose();
            if (SegmentArray.IsCreated) SegmentArray.Dispose();
        }
    }
}