using UnityEngine;
using System.Collections.Generic;

namespace AwesomeTechnologies.Utility.BVHTree
{
    public class BVH
    {
        public static int MaxPrimsCountPerNode = 1; // NOTE : do not change as BVH raycast will not work with more than 1 primitive per leaf node
        public static BVHNode Bvh;
        public static List<ObjectData> Objects;
        public static float Progress;

        /// <summary>
        /// Build the primitives to all selected objects to be used in the BVH
        /// </summary>
        public static void Build(ref List<ObjectData> allObjects, out List<BVHNode> nodes, out List<BVHTriangle> tris, out List<BVHTriangle> finalPrims)
        {
            // Current primitive id
            int pID = 0;

            // These buffers are disposed in the Main script from which this method is called
            nodes = new List<BVHNode>();
            tris = new List<BVHTriangle>();
            finalPrims = new List<BVHTriangle>();

            // Iterate thourgh all objects and create primitives using object's triangles
            foreach (ObjectData o in allObjects)
            {
                if (!o.IsValid)
                {
                    continue;
                }

                // Using the mesh verts and the tris from each submesh,
                // we can build the lumes separately for each submesh
                for (int i = 0; i < o.Indices.Length; i += 3)
                {

                    int trID0 = o.Indices[i + 0];
                    int trID1 = o.Indices[i + 1];
                    int trID2 = o.Indices[i + 2];

                    Vector3 v0 = o.VerticeList[trID0];
                    Vector3 v1 = o.VerticeList[trID1];
                    Vector3 v2 = o.VerticeList[trID2];

                    Vector3 n0 = o.HasNormals ? o.NormalList[trID0] : Vector3.zero;
                    Vector3 n1 = o.HasNormals ? o.NormalList[trID1] : Vector3.zero;
                    Vector3 n2 = o.HasNormals ? o.NormalList[trID2] : Vector3.zero;

                    // You have to add the check for excluding back face trinagles here

                    // Backfacing triangles skip
                    Vector3 n = Vector3.Cross((v1 - v0).normalized, (v2 - v0).normalized).normalized;

                    // Backfacing check
                    if (Vector3.Dot(Vector3.up, n) >= 0f)
                    {
                        tris.Add(new BVHTriangle(   v0, v1, v2,
                                                    n0, n1, n2,
                                                    pID++,o.TerrainSourceID));
                    }
                }
            }

            // Creating (Building) the bvh
            Bvh = new BVHNode(tris, nodes, ref finalPrims);
        }

        /// <summary>
        /// Stores the flattened data to lists of structs - nodes and triangles
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="prims"></param>
        /// <param name="lNodes"></param>
        /// <param name="lPrims"></param>
        public static void BuildLbvhData(List<BVHNode> nodes, List<BVHTriangle> prims, out List<LBVHNODE> lNodes, out List<LBVHTriangle> lPrims)
        {
            lPrims = new List<LBVHTriangle>();
            lNodes = new List<LBVHNODE>();

            for (int pID = 0; pID < prims.Count; pID++)
            {
                var tr = prims[pID];

                lPrims.Add(new LBVHTriangle(tr.V0, tr.V1, tr.V2, tr.N,tr.TerrainSourceID));
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                lNodes.Add(new LBVHNODE(nodes[i]));
            }
        }

        public static bool OverlapBbox(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
        {

            if (aMax.x < bMin.x || aMin.x > bMax.x) return false;
            if (aMax.y < bMin.y || aMin.y > bMax.y) return false;
            if (aMax.z < bMin.z || aMin.z > bMax.z) return false;

            // We have an overlap
            return true;
        }


        public static bool CalculateCellSize(int nodeID, List<LBVHNODE> nodes, ref Vector3 cellMinExtended, ref Vector3 cellMaxExtended, ref Vector3 cellMin, ref Vector3 cellMax)
        {
            if (nodes[nodeID].IsLeaf == 1)
            {
                Vector3 nodeMin = nodes[nodeID].BMin;
                Vector3 nodeMax = nodes[nodeID].BMax;

                if (OverlapBbox(cellMinExtended, cellMaxExtended, nodeMin, nodeMax))
                {
                    if (nodeMin.y < cellMin.y) cellMin.y = nodeMin.y;
                    if (nodeMax.y > cellMax.y) cellMax.y = nodeMax.y;
                }
            }
            else
            {
                CalculateCellSize(nodes[nodeID].LChildID, nodes, ref cellMinExtended, ref cellMaxExtended, ref cellMin, ref cellMax);
                CalculateCellSize(nodes[nodeID].RChildID, nodes, ref cellMinExtended, ref cellMaxExtended, ref cellMin, ref cellMax);
            }
            return true;
        }
    }
}