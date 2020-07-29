using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public static class LineSegment2Dextention
    {
        public static float DistanceToPoint(this LineSegment2D lineSegment, Vector2 point)
        {
            return Mathf.Sqrt(SqrDistanceToPoint(point, lineSegment));
        }
        public static float SqrDistanceToPoint(Vector2 point, LineSegment2D segment)
        {
            Vector2 diff = point - segment.Center;
            float param = math.dot(segment.Direction, diff);
            Vector2 closestPoint;
            if (-segment.Extent < param)
            {
                if (param < segment.Extent)
                {
                    closestPoint = segment.Center + param * segment.Direction;
                }
                else
                {
                    closestPoint = segment.Point1;
                }
            }
            else
            {
                closestPoint = segment.Point0;
            }
            diff = closestPoint - point;
            return diff.sqrMagnitude;
        }
    }

    public struct LineSegment2D
    {
        public Vector2 Point0;
        public Vector2 Point1;
        public Vector2 Center;
        public Vector2 Direction;
        public readonly float Extent;
        public int DisableEdge;

        public LineSegment2D(Vector2 point0, Vector2 point1)
        {
            Point0 = point0;
            Point1 = point1;           
            Center = 0.5f * (Point0 + Point1);
            Direction = Point1 - Point0;
            float directionLength = Direction.magnitude;
            float inverseDirectionLength = 1f / directionLength;
            Direction *= inverseDirectionLength;
            Extent = 0.5f * directionLength;
            DisableEdge = 0;
        }
    }
    
    public struct LineSegment3D
    {
        public Vector3 Point0;
        public Vector3 Point1;
        public Vector3 Center;
        public Vector3 Direction;
        public float Extent;
       
        public LineSegment3D(Vector3 point0, Vector3 point1)
        {
            Point0 = point0;
            Point1 = point1;
            Center = Direction = Vector3.zero;
            Extent = 0f;
            CalcDir();
        }
        
        public void CalcDir()
        {
            Center = 0.5f * (Point0 + Point1);
            Direction = Point1 - Point0;
            var directionLength = Direction.magnitude;
            var invDirectionLength = 1f / directionLength;
            Direction *= invDirectionLength;
            Extent = 0.5f * directionLength;
        }
        
        public float DistanceTo(Vector3 point)
        {
            return Mathf.Sqrt(SqrPoint3Segment3(ref point, ref this));
        }
               
        public static float SqrPoint3Segment3(ref Vector3 point, ref LineSegment3D segment)
        {
            var diff = point - segment.Center;
            var param = Vector3.Dot(segment.Direction,diff);
            Vector3 closestPoint;
            if (-segment.Extent < param)
            {
                if (param < segment.Extent)
                {
                    closestPoint = segment.Center + param * segment.Direction;
                }
                else
                {
                    closestPoint = segment.Point1;
                }
            }
            else
            {
                closestPoint = segment.Point0;
            }
            diff = closestPoint - point;
            return diff.sqrMagnitude;
        }          
    }  

    // ReSharper disable once ClassNeverInstantiated.Global
    public class PolygonUtility{

        public static void AlignPointsWithTerrain(List<Vector3> pointList, bool closePolygon, LayerMask groundLayerMask)
        {
            for (int i = 0; i <= pointList.Count - 1; i++)
            {
                Ray ray = new Ray(pointList[i] + new Vector3(0, 10000f, 0), Vector3.down);

                var hits = Physics.RaycastAll(ray, 20000f).OrderBy(h => h.distance).ToArray(); 
                for (int j = 0; j <= hits.Length - 1; j++)
                {
                    if (!(hits[j].collider is TerrainCollider || groundLayerMask.Contains(hits[j].collider.gameObject.layer))) continue;
                    pointList[i] = hits[j].point;
                    break;
                }
            }

            if (closePolygon && pointList.Count > 0)
            {
                pointList.Add(pointList[0]);
            }
        }

        public static List<Vector3> InflatePolygon(List<Vector3> pointList, double offset, bool closedPolygon)
        {
            List<Vector3> offsetPointList = new List<Vector3>();

            List<External.ClipperLib.IntPoint> polygon = new List<External.ClipperLib.IntPoint>();
            foreach (var point in pointList)
            {
                polygon.Add(new External.ClipperLib.IntPoint(point.x, point.z));
            }

            External.ClipperLib.ClipperOffset co = new External.ClipperLib.ClipperOffset();
            co.AddPath(polygon, External.ClipperLib.JoinType.jtRound,
                closedPolygon ? External.ClipperLib.EndType.etClosedPolygon : External.ClipperLib.EndType.etOpenRound);


            List<List<External.ClipperLib.IntPoint>> solution = new List<List<External.ClipperLib.IntPoint>>();
            co.Execute(ref solution, offset);

            foreach (var offsetPath in solution)
            {
                foreach (var offsetPathPoint in offsetPath)
                {
                    offsetPointList.Add(new Vector3(Convert.ToInt32(offsetPathPoint.X), 0, Convert.ToInt32(offsetPathPoint.Y)));
                }
            }
            return offsetPointList;
        }

        public static List<Vector2> DouglasPeucker(List<Vector2> points, int startIndex, int lastIndex, float epsilon)
        {
            float dmax = 0f;
            int index = startIndex;

            for (int i = index + 1; i < lastIndex; ++i)
            {
                float d = PointLineDistance(points[i], points[startIndex], points[lastIndex]);
                if (d > dmax)
                {
                    index = i;
                    dmax = d;
                }
            }

            if (dmax > epsilon)
            {
                var res1 = DouglasPeucker(points, startIndex, index, epsilon);
                var res2 = DouglasPeucker(points, index, lastIndex, epsilon);

                var finalRes = new List<Vector2>();
                for (int i = 0; i < res1.Count - 1; ++i)
                {
                    finalRes.Add(res1[i]);
                }

                foreach (Vector2 t in res2)
                {
                    finalRes.Add(t);
                }

                return finalRes;
            }
            else
            {
                return new List<Vector2>(new[] { points[startIndex], points[lastIndex] });
            }
        }

        public static float PointLineDistance(Vector2 point, Vector2 start, Vector2 end)
        {
            if (start == end)
            {
                return Vector2.Distance(point, start);
            }

            float n = Mathf.Abs((end.x - start.x) * (start.y - point.y) - (start.x - point.x) * (end.y - start.y));
            float d = Mathf.Sqrt((end.x - start.x) * (end.x - start.x) + (end.y - start.y) * (end.y - start.y));

            return n / d;
        }

        public static double Cross(Vector2 o, Vector2 a, Vector2 b)
        {
            return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
        }

        public static List<Vector2> GetConvexHull(List<Vector2> points)
        {
            if (points == null)
                return null;

            if (points.Count <= 1)
                return points;

            int n = points.Count, k = 0;
            List<Vector2> h = new List<Vector2>(new Vector2[2 * n]);

            points.Sort((a, b) =>
                a.x.Equals(b.x) ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && Cross(h[k - 2], h[k - 1], points[i]) <= 0)
                    k--;
                h[k++] = points[i];
            }

            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && Cross(h[k - 2], h[k - 1], points[i]) <= 0)
                    k--;
                h[k++] = points[i];
            }

            return h.Take(k - 1).ToList();
        }

        public static List<Vector2> DouglasPeuckerReduction
            (List<Vector2> pointList, float tolerance)
        {
            if (pointList == null || pointList.Count < 3)
                return pointList;

            int firstPoint = 0;
            int lastPoint = pointList.Count - 1;
            List<int> pointIndexsToKeep = new List<int> { firstPoint, lastPoint };

            while (pointList[firstPoint].Equals(pointList[lastPoint]))
            {
                lastPoint--;
            }

            DouglasPeuckerReduction(pointList, firstPoint, lastPoint,
                tolerance, ref pointIndexsToKeep);

            pointIndexsToKeep.Sort();

            return pointIndexsToKeep.Select(index => pointList[index]).ToList();
        }

        private static void DouglasPeuckerReduction(List<Vector2>
                points, int firstPoint, int lastPoint, float tolerance,
            ref List<int> pointIndexsToKeep)
        {
            float maxDistance = 0;
            int indexFarthest = 0;

            for (int index = firstPoint; index < lastPoint; index++)
            {
                float distance = PerpendicularDistance
                    (points[firstPoint], points[lastPoint], points[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (!(maxDistance > tolerance) || indexFarthest == 0) return;
            pointIndexsToKeep.Add(indexFarthest);

            DouglasPeuckerReduction(points, firstPoint,
                indexFarthest, tolerance, ref pointIndexsToKeep);
            DouglasPeuckerReduction(points, indexFarthest,
                lastPoint, tolerance, ref pointIndexsToKeep);
        }

        public static float PerpendicularDistance
            (Vector2 p1, Vector2 p2, Vector2 p)
        {
            float area = Mathf.Abs(.5f * (p1.x * p2.y + p2.x *
                                          p.y + p.x * p1.y - p2.x * p1.y - p.x *
                                          p2.y - p1.x * p.y));
            float bottom = Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) +
                                      Mathf.Pow(p1.y - p2.y, 2));
            float height = area / bottom * 2;
            return height;
        }

    }
}
