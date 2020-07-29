using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public class ShadowUtility
    {
        /// <summary>
        /// Check for shadow visibility, send minimum terrain height in worldspace as planeOrigin
        /// </summary>
        /// <param name="objectBounds"></param>
        /// <param name="lightDirection"></param>
        /// <param name="planeOrigin"></param>
        /// <param name="frustumPlanes"></param>
        /// <returns></returns>
        public static bool IsShadowVisible(Bounds objectBounds, Vector3 lightDirection, Vector3 planeOrigin, Plane[] frustumPlanes)
        {
            bool hitPlane;
            Bounds shadowBounds = GetShadowBounds(objectBounds, lightDirection, planeOrigin, out hitPlane);
            //return hitPlane && GeometryUtility.TestPlanesAABB(frustumPlanes, shadowBounds);
            return hitPlane && BoundsIntersectsFrustum(frustumPlanes, shadowBounds);
        }

        /// <summary>
        /// Test for bounds visible in frustum
        /// </summary>
        /// <param name="planes"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static bool BoundsIntersectsFrustum(Plane[] planes, Bounds bounds)
        {
            var center = bounds.center;
            var extents = bounds.extents;

            for (int i = 0; i <= planes.Length -1; i++)
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

        /// <summary>
        /// Get the projected bounds of the shadow of an object
        /// </summary>
        /// <param name="objectBounds"></param>
        /// <param name="lightDirection"></param>
        /// <param name="planeOrigin"></param>
        /// <param name="hitPlane"></param>
        /// <returns></returns>
        public static Bounds GetShadowBounds(Bounds objectBounds, Vector3 lightDirection, Vector3 planeOrigin, out bool hitPlane)
        {
            Ray p0 = new Ray(new Vector3(objectBounds.min.x, objectBounds.max.y, objectBounds.min.z), lightDirection);
            Ray p1 = new Ray(new Vector3(objectBounds.min.x, objectBounds.max.y, objectBounds.max.z), lightDirection);
            Ray p2 = new Ray(new Vector3(objectBounds.max.x, objectBounds.max.y, objectBounds.min.z), lightDirection);
            Ray p3 = new Ray(objectBounds.max, lightDirection);

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
    }
}

