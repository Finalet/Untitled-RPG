using UnityEngine;

namespace AwesomeTechnologies.Utility.BVHTree
{
    public class BVHTriangle
    {
        public int PrimID;
        public Vector3 V0, V1, V2, N0, N1, N2, N;
        public int TerrainSourceID;

        public Vector3 Center;
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Centroid;

        public BVHTriangle(Vector3 v0, Vector3 v1, Vector3 v2,
                            Vector3 n0, Vector3 n1, Vector3 n2,
                            int primID, int terrainSourceID)
        {
            V0 = v0; V1 = v1; V2 = v2;
            N0 = n0; N1 = n1; N2 = n2;

            N = Vector3.Cross(Vector3.Normalize(V1 - V0), Vector3.Normalize(V2 - V0));

            PrimID = primID;
            TerrainSourceID = terrainSourceID;

            Min = Vector3.zero;
            Max = Vector3.zero;

            Center = (Min + Max) * 0.5f;
            Centroid = Center;

            CalculateBBox();
        }

        public void CalculateBBox()
        {
            Min = new Vector3(Mathf.Min(Min.x, V0.x), Mathf.Min(Min.y, V0.y), Mathf.Min(Min.z, V0.z));
            Max = new Vector3(Mathf.Max(Max.x, V0.x), Mathf.Max(Max.y, V0.y), Mathf.Max(Max.z, V0.z));

            Min = new Vector3(Mathf.Min(Min.x, V1.x), Mathf.Min(Min.y, V1.y), Mathf.Min(Min.z, V1.z));
            Max = new Vector3(Mathf.Max(Max.x, V1.x), Mathf.Max(Max.y, V1.y), Mathf.Max(Max.z, V1.z));

            Min = new Vector3(Mathf.Min(Min.x, V2.x), Mathf.Min(Min.y, V2.y), Mathf.Min(Min.z, V2.z));
            Max = new Vector3(Mathf.Max(Max.x, V2.x), Mathf.Max(Max.y, V2.y), Mathf.Max(Max.z, V2.z));

            Centroid = (Min + Max) * 0.5f;
        }  
    }
}