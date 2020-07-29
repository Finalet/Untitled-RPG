using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace AwesomeTechnologies.VegetationSystem
{

    [BurstCompile(CompileSynchronously = true)]
    public class CalculateVisibilityAndLODJob : IJob
    {
        [ReadOnly]
        public NativeList<Matrix4x4> InstanceList;
        [ReadOnly]
        public NativeArray<Plane> FrustumPlanes;

        public NativeList<Matrix4x4> LOD0InstanceList;
        public NativeList<Matrix4x4> LOD1InstanceList;
        public NativeList<Matrix4x4> LOD2InstanceList;
        public NativeList<Matrix4x4> LOD0ShadowInstanceList;
        public NativeList<Matrix4x4> LOD1ShadowInstanceList;
        public NativeList<Matrix4x4> LOD2ShadowInstanceList;

        public NativeList<float> LOD0LODFadeList;
        public NativeList<float> LOD1LODFadeList;
        public NativeList<float> LOD2LODFadeList;
        public NativeList<float> LOD0ShadowLODFadeList;
        public NativeList<float> LOD1ShadowLODFadeList;
        public NativeList<float> LOD2ShadowLODFadeList;

        public Vector3 LightDirection;
        public Vector3 PlaneOrigin;
        public Vector3 CameraPosition;
        public Vector3 ItemBoundSize;

        public float LOD1Distance;
        public float LOD2Distance;
        public float CullDistance;
        public float LODFadeDistance;
        public bool DisableLOD = false;

        void ClearLists()
        {
            LOD0InstanceList.Clear();
            LOD1InstanceList.Clear();
            LOD2InstanceList.Clear();
            LOD0ShadowInstanceList.Clear();
            LOD1ShadowInstanceList.Clear();
            LOD2ShadowInstanceList.Clear();

            LOD0LODFadeList.Clear();
            LOD1LODFadeList.Clear();
            LOD2LODFadeList.Clear();
            LOD0ShadowLODFadeList.Clear();
            LOD1ShadowLODFadeList.Clear();
            LOD2ShadowLODFadeList.Clear();
        }

        public void Execute()
        {
            ClearLists();

            for (int i = 0; i <= InstanceList.Length - 1; i++)
            {
                float3 position = ExtractTranslationFromMatrix(InstanceList[i]);
                float distance = math.distance(position, CameraPosition);
                if (distance <= CullDistance) continue;                

                int visibility = CheckItemVisibility(position, ItemBoundSize);

                if (visibility == 2)
                {
                    if (LOD1Distance < 0 || DisableLOD)
                    {
                        LOD0ShadowInstanceList.Add(InstanceList[i]);
                    }
                    else
                    {
                        if (distance > LOD2Distance)
                        {
                            LOD2ShadowInstanceList.Add(InstanceList[i]);
                        }
                        else if (distance > LOD1Distance)
                        {
                            LOD1ShadowInstanceList.Add(InstanceList[i]);
                            LOD1ShadowLODFadeList.Add(CalculateLODFade(distance, LOD2Distance));
                        }
                        else
                        {
                            LOD0ShadowInstanceList.Add(InstanceList[i]);
                            LOD0ShadowLODFadeList.Add(CalculateLODFade(distance, LOD1Distance));
                        }
                    }
                }
                else if (visibility == 1)
                {
                    if (LOD1Distance < 0 || DisableLOD)
                    {
                        LOD0InstanceList.Add(InstanceList[i]);
                    }
                    else
                    {
                        if (distance > LOD2Distance)
                        {
                            LOD2InstanceList.Add(InstanceList[i]);
                            
                        }
                        else if (distance > LOD1Distance)
                        {
                            LOD1InstanceList.Add(InstanceList[i]);
                            LOD1LODFadeList.Add(CalculateLODFade(distance, LOD2Distance));
                        }
                        else
                        {
                            LOD0InstanceList.Add(InstanceList[i]);
                            LOD0LODFadeList.Add(CalculateLODFade(distance, LOD1Distance));
                        }
                    }
                }
            }
        }

        int CheckItemVisibility(float3 position, float3 boundSize)
        {
            float3 boundsCenter = position + new float3(0, boundSize.y / 2f, 0);
            Bounds itemBounds = new Bounds(boundsCenter, boundSize);
            if (BoundsIntersectsFrustum(itemBounds))
            {
                return 1;
            }
            else
            {
                bool visibleShadow = IsShadowVisible(itemBounds, LightDirection, PlaneOrigin, FrustumPlanes);
                if (visibleShadow) return 2;
            }

            return 0;
        }

        float3 ExtractTranslationFromMatrix(Matrix4x4 matrix)
        {
            float3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        public bool BoundsIntersectsFrustum(Bounds bounds)
        {
            var center = bounds.center;
            var extents = bounds.extents;

            for (int i = 0; i <= FrustumPlanes.Length - 1; i++)
            {
                float3 planeNormal = FrustumPlanes[i].normal;
                float planeDistance = FrustumPlanes[i].distance;

                float3 abs = math.abs(FrustumPlanes[i].normal);
                float r = extents.x * abs.x + extents.y * abs.y + extents.z * abs.z;
                float s = planeNormal.x * center.x + planeNormal.y * center.y + planeNormal.z * center.z;
                if (s + r < -planeDistance)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsShadowVisible(Bounds objectBounds, Vector3 lightDirection, Vector3 planeOrigin, NativeArray<Plane> frustumPlanes)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            bool hitPlane;
            Bounds shadowBounds = GetShadowBounds(objectBounds, lightDirection, planeOrigin, out hitPlane);
            return hitPlane && BoundsIntersectsFrustum(shadowBounds);
        }

        public Bounds GetShadowBounds(Bounds objectBounds, float3 lightDirection, float3 planeOrigin, out bool hitPlane)
        {
            Ray p0 = new Ray(new float3(objectBounds.min.x, objectBounds.max.y, objectBounds.min.z), lightDirection);
            Ray p1 = new Ray(new float3(objectBounds.min.x, objectBounds.max.y, objectBounds.max.z), lightDirection);
            Ray p2 = new Ray(new float3(objectBounds.max.x, objectBounds.max.y, objectBounds.min.z), lightDirection);
            Ray p3 = new Ray(objectBounds.max, lightDirection);

            // ReSharper disable once InlineOutVariableDeclaration
            float3 hitPoint;
            hitPlane = false;

            if (IntersectPlane(p0, planeOrigin, out hitPoint))
            {
                objectBounds.Encapsulate(hitPoint);
                hitPlane = true;
            }

            if (IntersectPlane(p1, planeOrigin, out hitPoint))
            {
                objectBounds.Encapsulate(hitPoint);
                hitPlane = true;
            }

            if (IntersectPlane(p2, planeOrigin, out hitPoint))
            {
                objectBounds.Encapsulate(hitPoint);
                hitPlane = true;
            }

            if (IntersectPlane(p3, planeOrigin, out hitPoint))
            {
                objectBounds.Encapsulate(hitPoint);
                hitPlane = true;
            }
            return objectBounds;
        }

        public bool IntersectPlane(Ray ray, float3 planeOrigin, out float3 hitPoint)
        {
            float3 planeNormal = -Vector3.up;
            float3 rayOrigin = ray.origin;
            float denominator = math.dot(ray.direction, planeNormal);
            if (denominator > 0.00001f)
            {
                float t = math.dot(planeOrigin - rayOrigin, planeNormal) / denominator;
                hitPoint = ray.origin + ray.direction * t;
                return true;
            }
            hitPoint = Vector3.zero;
            return false;
        }

        float CalculateLODFade(float cameraDistance, float nextLODDistance)
        {
            float distance = nextLODDistance - cameraDistance;
            if (distance <= LODFadeDistance)
            {
                return Mathf.Clamp01(1 - distance / LODFadeDistance);
            }
            return 0;
        }
    }
}
