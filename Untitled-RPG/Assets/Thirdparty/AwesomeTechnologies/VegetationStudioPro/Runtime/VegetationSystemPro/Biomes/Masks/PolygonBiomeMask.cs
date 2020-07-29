using System.Collections.Generic;
using UnityEngine;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace AwesomeTechnologies.VegetationSystem.Biomes
{
    public class BiomeMaskSortOrderComparer : IComparer<PolygonBiomeMask>
    {
        public int Compare(PolygonBiomeMask x, PolygonBiomeMask y)
        {
            if (x != null && y != null)
            {
                return x.BiomeSortOrder.CompareTo(y.BiomeSortOrder);
            }
            else
            {
                return 0;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct FilterBiomeSpawnLocationsJob : IJobParallelFor
    {
        public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;       
        [ReadOnly]
        public NativeArray<float> CurveArray;
        [ReadOnly]
        public NativeArray<float> InverseCurveArray;
        [ReadOnly]
        public NativeArray<Vector2> PolygonArray;
        [ReadOnly]
        public NativeArray<LineSegment2D> SegmentArray;

        public bool Include;
        public bool UseNoise;
        public float NoiseScale;
        public float BlendDistance;
        public Rect PolygonRect;

        public void Execute(int index)
        {
            VegetationSpawnLocationInstance vegetationSpawnLocationInstance = SpawnLocationList[index];
            
            float originalSpawnChance = vegetationSpawnLocationInstance.SpawnChance;
            Vector2 point = new Vector2(SpawnLocationList[index].Position.x, SpawnLocationList[index].Position.z);
            if (!PolygonRect.Contains(point)) return;
            
            if (IsInPolygon(point))
            {
                vegetationSpawnLocationInstance.SpawnChance = math.select(
                    vegetationSpawnLocationInstance.SpawnChance =
                        math.min(0, vegetationSpawnLocationInstance.SpawnChance),
                    vegetationSpawnLocationInstance.SpawnChance =
                        math.max(1, vegetationSpawnLocationInstance.SpawnChance), Include);

                float distanceToEdge = DistanceToEdge(point);
                
                vegetationSpawnLocationInstance.BiomeDistance = math.select(vegetationSpawnLocationInstance.BiomeDistance, math.min(distanceToEdge, vegetationSpawnLocationInstance.BiomeDistance),Include);

                if (distanceToEdge < BlendDistance)
                {
                    float perlinNoise = math.select(1, Mathf.PerlinNoise(point.x / NoiseScale, point.y / NoiseScale), UseNoise);
                    perlinNoise = math.select(perlinNoise, 0, !Include && !UseNoise);                  
                    vegetationSpawnLocationInstance.SpawnChance = math.select(math.max((SampleInverseCurveArray(distanceToEdge / BlendDistance)) * (1 - perlinNoise), vegetationSpawnLocationInstance.SpawnChance), math.min(SampleCurveArray(distanceToEdge / BlendDistance) * perlinNoise, vegetationSpawnLocationInstance.SpawnChance), Include);


                    vegetationSpawnLocationInstance.SpawnChance = math.select(
                        math.min(vegetationSpawnLocationInstance.SpawnChance, originalSpawnChance),
                        math.max(vegetationSpawnLocationInstance.SpawnChance, originalSpawnChance), Include);
                }
                SpawnLocationList[index] = vegetationSpawnLocationInstance;
            }
        }

        private float SampleCurveArray(float value)
        {
            if (CurveArray.Length == 0) return 0f;
            int index = Mathf.RoundToInt((value) * CurveArray.Length);
            index = Mathf.Clamp(index, 0, CurveArray.Length - 1);
            return CurveArray[index];
        }

        private float SampleInverseCurveArray(float value)
        {
            if (InverseCurveArray.Length == 0) return 0f;
            int index = Mathf.RoundToInt((value) * InverseCurveArray.Length);
            index = Mathf.Clamp(index, 0, InverseCurveArray.Length - 1);
            return InverseCurveArray[index];
        }

        private float DistanceToEdge(Vector2 point)
        {
            float distance = float.MaxValue;
            for (int i = 0; i < SegmentArray.Length; i++)
            {
                if (SegmentArray[i].DisableEdge == 0)
                {
                    distance = math.min(distance, SegmentArray[i].DistanceToPoint(point)); 
                }
            }

            return distance;
        }

        private bool IsInPolygon(Vector2 p)
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
                    && (p.y - (long)p1.y) * (p2.x - p1.x)
                    < (p2.y - (long)p1.y) * (p.x - p1.x))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }
            return inside;
        }       
    }

    public class PolygonBiomeMask
    {
        public Bounds MaskBounds;
        public BiomeType BiomeType;
        public float BlendDistance;
        public bool UseNoise;
        public float NoiseScale;
        public int BiomeSortOrder;
        private Rect _polygonRect;

        public delegate void MultionMaskDeleteDelegate(PolygonBiomeMask maskArea);
        public MultionMaskDeleteDelegate OnMaskDeleteDelegate;

        public void CallDeleteEvent()
        {
            OnMaskDeleteDelegate?.Invoke(this);
        }

        private Vector2[] _points2D;

        private LineSegment2D[] _segments;
        private Vector3[] _points3D;
        public NativeArray<Vector2> PolygonArray;
        public NativeArray<LineSegment2D> SegmentArray;
        public NativeArray<float> CurveArray;
        public NativeArray<float> InverseCurveArray;

        public NativeArray<float> TextureCurveArray;

        private bool[] _disableEdges;
        //public NativeArray<float> TextureInverseCurveArray;

        public void AddPolygon(List<Vector3> pointList, List<bool> disableEdgeList)
        {
            _disableEdges = disableEdgeList.ToArray();
            
            _points2D = new Vector2[pointList.Count];
            _points3D = new Vector3[pointList.Count];
            for (int i = 0; i <= pointList.Count - 1; i++)
            {
                _points2D[i] = new Vector2(pointList[i].x, pointList[i].z);
                _points3D[i] = pointList[i];
            }
            MaskBounds = GetMaskBounds();

            PolygonArray = new NativeArray<Vector2>(_points2D.Length,Allocator.Persistent);
            PolygonArray.CopyFrom(_points2D);
            CreateSegments();
            
            
//            Bounds bounds = new Bounds();
//            for (int i = 0; i <= pointList.Count - 1; i++)
//            {
//                if (i == 0)
//                {
//                    bounds = new Bounds(pointList[i],Vector3.zero);
//                }
//                else
//                {
//                    bounds.Encapsulate(pointList[i]);
//                }
//            }
            _polygonRect = RectExtension.CreateRectFromBounds(MaskBounds);           
        }

        public void SetCurve(float[] curveArray)
        {
            CurveArray = new NativeArray<float>(curveArray.Length, Allocator.Persistent);
            CurveArray.CopyFrom(curveArray);
        }

        public void SetInverseCurve(float[] curveArray)
        {
            InverseCurveArray = new NativeArray<float>(curveArray.Length, Allocator.Persistent);
            InverseCurveArray.CopyFrom(curveArray);
        }


        public void SetTextureCurve(float[] curveArray)
        {
            TextureCurveArray = new NativeArray<float>(curveArray.Length, Allocator.Persistent);
            TextureCurveArray.CopyFrom(curveArray);
        }

        //public void SetTextureInverseCurve(float[] curveArray)
        //{
        //    TextureInverseCurveArray = new NativeArray<float>(curveArray.Length, Allocator.Persistent);
        //    TextureInverseCurveArray.CopyFrom(curveArray);
        //}

        void CreateSegments()
        {
            _segments = new LineSegment2D[_points2D.Length];
            for (int i = 0; i <= _points2D.Length - 2; i++)
            {
                LineSegment2D lineSegment2D = new LineSegment2D(_points2D[i], _points2D[i+1]);
                _segments[i] = lineSegment2D;

                if (_disableEdges[i] && _disableEdges[i + 1])
                {
                    _segments[i].DisableEdge = 1;
                }               
            }
            if (_points2D.Length > 0)
            {
                LineSegment2D lineSegment2D = new LineSegment2D(_points2D[0], _points2D[_points2D.Length -1]);
                _segments[_points2D.Length - 1] = lineSegment2D;
                
                if (_disableEdges[0] && _disableEdges[_points2D.Length -1])
                {
                    _segments[_points2D.Length - 1].DisableEdge = 1;
                }       
            }
            SegmentArray = new NativeArray<LineSegment2D>(_segments.Length, Allocator.Persistent);
            SegmentArray.CopyFrom(_segments);           
        }

        public bool Contains(Vector3 point)
        {
            if (!PolygonArray.IsCreated) return false;
            Vector2 point2D = new Vector2(point.x,point.z);
            return IsInPolygon(point2D);
        }

        public JobHandle FilterSpawnLocations(NativeList<VegetationSpawnLocationInstance> spawnLocationList, BiomeType currentBiomeType, int sampleCount,JobHandle dependsOn)
        {
            FilterBiomeSpawnLocationsJob filterBiomeSpawnLocationsJob =
                new FilterBiomeSpawnLocationsJob
                {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    SpawnLocationList = spawnLocationList.AsDeferredJobArray(),
#else
                    SpawnLocationList = spawnLocationList.ToDeferredJobArray(),              
#endif

                    PolygonArray = PolygonArray,
                    SegmentArray = SegmentArray,                   
                    Include = currentBiomeType == BiomeType,
                    BlendDistance = BlendDistance,
                    UseNoise = UseNoise,
                    NoiseScale = NoiseScale,
                    CurveArray = CurveArray,
                    InverseCurveArray = InverseCurveArray,
                    PolygonRect = _polygonRect
                };

            dependsOn = filterBiomeSpawnLocationsJob.Schedule(sampleCount, 64, dependsOn);
            return dependsOn;
        }

        private Bounds GetMaskBounds()
        {
            var expandedBounds = _points3D.Length > 0 ? new Bounds(_points3D[0], new Vector3(1, 1, 1)) : new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));

            for (int i = 0; i <= _points3D.Length - 1; i++)
            {
                expandedBounds.Encapsulate(_points3D[i]);
            }
            return expandedBounds;
        }

        public void Dispose()
        {
            if (PolygonArray.IsCreated) PolygonArray.Dispose();
            if (SegmentArray.IsCreated) SegmentArray.Dispose();
            if (CurveArray.IsCreated) CurveArray.Dispose();
            if (InverseCurveArray.IsCreated) InverseCurveArray.Dispose();
            if (TextureCurveArray.IsCreated) TextureCurveArray.Dispose();
            //if (TextureInverseCurveArray.IsCreated) TextureInverseCurveArray.Dispose();
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
                    && (p.y - (long)p1.y) * (p2.x - p1.x)
                    < (p2.y - (long)p1.y) * (p.x - p1.x))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }
            return inside;
        }
    }
}
