using UnityEngine;
using AwesomeTechnologies.MeshTerrains;
using Unity.Mathematics;

namespace AwesomeTechnologies.Utility.BVHTree
{
    [System.Serializable]
    // ReSharper disable once InconsistentNaming
    public class BVHBBox
    {
        public Vector3 Center = Vector3.zero;
        public Vector3 Min = Vector3.one * float.MaxValue;
        public Vector3 Max = Vector3.one * -float.MaxValue;

        public static bool IntersectRay(BVHRay r, float3 min, float3 max, out float hitDist)
        {
            float tXmin, tXmax, tYmin, tYmax, tZmin, tZmax;
            float xA = 1f / r.Direction.x, yA = 1f / r.Direction.y, zA = 1f / r.Direction.z;
            float xE = r.Origin.x, yE = r.Origin.y, zE = r.Origin.z;

            // calculate t interval in x-axis
            if (xA >= 0)
            {
                tXmin = (min.x - xE) * xA;
                tXmax = (max.x - xE) * xA;
            }
            else
            {
                tXmin = (max.x - xE) * xA;
                tXmax = (min.x - xE) * xA;
            }

            // calculate t interval in y-axis
            if (yA >= 0)
            {
                tYmin = (min.y - yE) * yA;
                tYmax = (max.y - yE) * yA;
            }
            else
            {
                tYmin = (max.y - yE) * yA;
                tYmax = (min.y - yE) * yA;
            }

            // calculate t interval in z-axis
            if (zA >= 0)
            {
                tZmin = (min.z - zE) * zA;
                tZmax = (max.z - zE) * zA;
            }
            else
            {
                tZmin = (max.z - zE) * zA;
                tZmax = (min.z - zE) * zA;
            }

            var tMin = math.max(tXmin, math.max(tYmin, tZmin));
            var tMax = math.min(tXmax, math.min(tYmax, tZmax));

            hitDist = tMin;
            return (tMin <= tMax);
        }
    }
}