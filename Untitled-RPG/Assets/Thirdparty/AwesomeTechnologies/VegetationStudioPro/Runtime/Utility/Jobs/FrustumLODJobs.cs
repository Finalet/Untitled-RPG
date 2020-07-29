using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    [BurstCompile(CompileSynchronously = true)]
    public struct VegetationItemLODSplitAndFrustumCullingJob : IJob
    {
        [ReadOnly]
        public NativeList<MatrixInstance> VegetationItemMatrixList;
        [ReadOnly]
        public NativeArray<Plane> FrustumPlanes;

        public NativeList<Matrix4x4> VegetationItemLOD0MatrixList;
        public NativeList<Matrix4x4> VegetationItemLOD1MatrixList;
        public NativeList<Matrix4x4> VegetationItemLOD2MatrixList;
        public NativeList<Matrix4x4> VegetationItemLOD3MatrixList;

        public NativeList<Matrix4x4> VegetationItemLOD0ShadowMatrixList;
        public NativeList<Matrix4x4> VegetationItemLOD1ShadowMatrixList;
        public NativeList<Matrix4x4> VegetationItemLOD2ShadowMatrixList;
        public NativeList<Matrix4x4> VegetationItemLOD3ShadowMatrixList;

        public NativeList<Vector4> LOD0FadeList;
        public NativeList<Vector4> LOD1FadeList;
        public NativeList<Vector4> LOD2FadeList;
        public NativeList<Vector4> LOD3FadeList;

        public Vector3 LightDirection;
        public Vector3 PlaneOrigin;
        public Vector3 BoundsSize;
        public bool ShadowCulling;
        public bool NoFrustumCulling;

        public float CullDistance;
        public float3 CameraPosition;
        public float BoundingSphereRadius;

        public int VegetationItemDistanceBand;

        public float LODFactor;
        public float LODBias;
        public float LODFadeDistance;

        public float LOD1Distance;
        public float LOD2Distance;
        public float LOD3Distance;
        public int LODCount;

        public bool LODFadePercentage;
        public bool LODFadeCrossfade;

        public Vector3 FloatingOriginOffset;

        public void Execute()
        {       
            for (int i = VegetationItemMatrixList.Length - 1; i >= 0; i--)
            {
                MatrixInstance vegetationItemMatrixInstance = VegetationItemMatrixList[i];
                vegetationItemMatrixInstance.Matrix = TranslateMatrix(vegetationItemMatrixInstance.Matrix, FloatingOriginOffset);

                float distanceFactor = vegetationItemMatrixInstance.DistanceFalloff;
                float itemCullDistance = CullDistance * distanceFactor;
                float lod1Distance = math.clamp(LOD1Distance * LODFactor * LODBias, 0, itemCullDistance);
                float lod2Distance = math.clamp(LOD2Distance * LODFactor * LODBias, 0, itemCullDistance);
                float lod3Distance = math.clamp(LOD3Distance * LODFactor * LODBias, 0, itemCullDistance);

                switch (LODCount)
                {
                    case 1:
                        lod1Distance = math.max(lod1Distance, itemCullDistance);
                        break;
                    case 2:
                        lod2Distance = math.max(lod2Distance, itemCullDistance);
                        break;
                    case 3:
                        lod3Distance = math.max(lod3Distance, itemCullDistance);
                        break;
                }

                bool useLODFade = LODFadePercentage || LODFadeCrossfade;

                float3 position = ExtractTranslationFromMatrix(vegetationItemMatrixInstance.Matrix);
                float distance = math.distance(CameraPosition, position);

                if (distance > itemCullDistance) continue;

                if (NoFrustumCulling)
                {
                     if (distance <= lod1Distance || LODCount == 1)
                    {
                        VegetationItemLOD0MatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        if (useLODFade)
                        {
                            float lodFade = CalculateLODFade(distance, lod1Distance);
                            float lodFadeQuantified = 1 - Mathf.Clamp(Mathf.RoundToInt(lodFade * 16) / 16f, 0.0625f, 1f);
                            LOD0FadeList.Add(new Vector4(lodFade, lodFadeQuantified, 0, 0));
                        }
                    }
                    else if (distance <= lod2Distance || LODCount == 2)
                    {
                        VegetationItemLOD1MatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        if (useLODFade)
                        {
                            float lodFade = CalculateLODFade(distance, lod2Distance);
                            float lodFadeQuantified = 1 - Mathf.Clamp(Mathf.RoundToInt(lodFade * 16) / 16f, 0.0625f, 1f);
                            LOD1FadeList.Add(new Vector4(lodFade, lodFadeQuantified, 0, 0));
                        }
                    }
                    else if (distance <= lod3Distance || LODCount == 3)
                    {
                        VegetationItemLOD2MatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        if (useLODFade)
                        {
                            float lodFade = CalculateLODFade(distance, lod3Distance);
                            float lodFadeQuantified = 1 - Mathf.Clamp(Mathf.RoundToInt(lodFade * 16) / 16f, 0.0625f, 1f);
                            LOD2FadeList.Add(new Vector4(lodFade, lodFadeQuantified, 0, 0));
                        }
                    }
                    else
                    {
                        VegetationItemLOD3MatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        if (useLODFade)
                        {
                            float lodFade = CalculateLODFade(distance, itemCullDistance);
                            float lodFadeQuantified = 1 - Mathf.Clamp(Mathf.RoundToInt(lodFade * 16) / 16f, 0.0625f, 1f);
                            LOD3FadeList.Add(new Vector4(lodFade, lodFadeQuantified, 0, 0));
                        }
                    }
                    continue;
                }
                
                BoundingSphere boundingSphere = new BoundingSphere(position, BoundingSphereRadius);

                if (SphereInFrustum(boundingSphere) == -1)
                {
                    if (VegetationItemDistanceBand == 0 || !ShadowCulling) continue;

                    //TODO add LODFade for shadows
                    Bounds vegetationItemBounds = new Bounds(position, BoundsSize);
                    if (IsShadowVisible(vegetationItemBounds, LightDirection, PlaneOrigin, FrustumPlanes))
                    {
                        if (distance <= lod1Distance || LODCount == 1)
                        {
                            VegetationItemLOD0ShadowMatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        }
                        else if (distance <= lod2Distance || LODCount == 2)
                        {
                            VegetationItemLOD1ShadowMatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        }
                        else if (distance <= lod3Distance || LODCount == 3)
                        {
                            VegetationItemLOD2ShadowMatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        }
                        else
                        {
                            VegetationItemLOD3ShadowMatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        }
                    }
                }
                else
                {
                    if (distance <= lod1Distance || LODCount == 1)
                    {
                        VegetationItemLOD0MatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        if (useLODFade)
                        {
                            float lodFade = CalculateLODFade(distance, lod1Distance);
                            float lodFadeQuantified = 1 - Mathf.Clamp(Mathf.RoundToInt(lodFade * 16) / 16f, 0.0625f, 1f);
                            LOD0FadeList.Add(new Vector4(lodFade, lodFadeQuantified, 0, 0));
                        }
                    }
                    else if (distance <= lod2Distance || LODCount == 2)
                    {
                        VegetationItemLOD1MatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        if (useLODFade)
                        {
                            float lodFade = CalculateLODFade(distance, lod2Distance);
                            float lodFadeQuantified = 1 - Mathf.Clamp(Mathf.RoundToInt(lodFade * 16) / 16f, 0.0625f, 1f);
                            LOD1FadeList.Add(new Vector4(lodFade, lodFadeQuantified, 0, 0));
                        }
                    }
                    else if (distance <= lod3Distance || LODCount == 3)
                    {
                        VegetationItemLOD2MatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        if (useLODFade)
                        {
                            float lodFade = CalculateLODFade(distance, lod3Distance);
                            float lodFadeQuantified = 1 - Mathf.Clamp(Mathf.RoundToInt(lodFade * 16) / 16f, 0.0625f, 1f);
                            LOD2FadeList.Add(new Vector4(lodFade, lodFadeQuantified, 0, 0));
                        }
                    }
                    else
                    {
                        VegetationItemLOD3MatrixList.Add(vegetationItemMatrixInstance.Matrix);
                        if (useLODFade)
                        {
                            float lodFade = CalculateLODFade(distance, itemCullDistance);
                            float lodFadeQuantified = 1 - Mathf.Clamp(Mathf.RoundToInt(lodFade * 16) / 16f, 0.0625f, 1f);
                            LOD3FadeList.Add(new Vector4(lodFade, lodFadeQuantified, 0, 0));
                        }
                    }
                }
            }
        }

        int SphereInFrustum(BoundingSphere boundingSphere)
        {
            for (int i = 0; i <= FrustumPlanes.Length - 1; i++)
            {
                float dist = FrustumPlanes[i].normal.x * boundingSphere.position.x +
                             FrustumPlanes[i].normal.y * boundingSphere.position.y +
                             FrustumPlanes[i].normal.z * boundingSphere.position.z + FrustumPlanes[i].distance;
                if (dist < -boundingSphere.radius)
                {
                    return -1;
                }
            }

            return 1;
        }

        float3 ExtractTranslationFromMatrix(Matrix4x4 matrix)
        {
            float3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        Matrix4x4 TranslateMatrix(Matrix4x4 matrix, float3 offset)
        {
            Matrix4x4 translatedMatrix = matrix;
            translatedMatrix.m03 = matrix.m03 + offset.x;
            translatedMatrix.m13 = matrix.m13 + offset.y;
            translatedMatrix.m23 = matrix.m23 + offset.z;
            return translatedMatrix;
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

        public static bool IsShadowVisible(Bounds objectBounds, Vector3 lightDirection, Vector3 planeOrigin, NativeArray<Plane> frustumPlanes)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            bool hitPlane;
            Bounds shadowBounds = GetShadowBounds(objectBounds, lightDirection, planeOrigin, out hitPlane);
            return hitPlane && BoundsIntersectsFrustum(frustumPlanes, shadowBounds);
        }

        public static Bounds GetShadowBounds(Bounds objectBounds, Vector3 lightDirection, Vector3 planeOrigin, out bool hitPlane)
        {
            Ray p0 = new Ray(new Vector3(objectBounds.min.x, objectBounds.max.y, objectBounds.min.z), lightDirection);
            Ray p1 = new Ray(new Vector3(objectBounds.min.x, objectBounds.max.y, objectBounds.max.z), lightDirection);
            Ray p2 = new Ray(new Vector3(objectBounds.max.x, objectBounds.max.y, objectBounds.min.z), lightDirection);
            Ray p3 = new Ray(objectBounds.max, lightDirection);

            // ReSharper disable once InlineOutVariableDeclaration
            Vector3 hitPoint;
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

        public static bool IntersectPlane(Ray ray, Vector3 planeOrigin, out Vector3 hitPoint)
        {
            Vector3 planeNormal = -Vector3.up;
            float denominator = Vector3.Dot(ray.direction, planeNormal);
            if (denominator > 0.00001f)
            {
                float t = Vector3.Dot(planeOrigin - ray.origin, planeNormal) / denominator;
                hitPoint = ray.origin + ray.direction * t;
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
        }

        public static bool BoundsIntersectsFrustum(NativeArray<Plane> planes, Bounds bounds)
        {
            var center = bounds.center;
            var extents = bounds.extents;

            for (int i = 0; i <= planes.Length - 1; i++)
            {
                Vector3 planeNormal = planes[i].normal;
                float planeDistance = planes[i].distance;

                Vector3 abs = new Vector3(Mathf.Abs(planeNormal.x), Mathf.Abs(planeNormal.y), Mathf.Abs(planeNormal.z));
                float r = extents.x * abs.x + extents.y * abs.y + extents.z * abs.z;
                float s = planeNormal.x * center.x + planeNormal.y * center.y + planeNormal.z * center.z;
                if (s + r < -planeDistance)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
