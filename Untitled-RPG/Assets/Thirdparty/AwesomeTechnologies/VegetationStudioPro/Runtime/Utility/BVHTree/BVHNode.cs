using UnityEngine;
using System.Collections.Generic;
namespace AwesomeTechnologies.Utility.BVHTree
{
    public struct BVHNode
    {
        public int IsLeaf;                     // 0 = false, 1 = true        
        public int NodeType;                   // o is root, 1 is left , 2 is right
        public Vector3 Centroid;                   //  = new Vector3(0f);
        public Vector3 Min;                        //  = new Vector3(1f) *  float.MaxValue;
        public Vector3 Max;                        //  = new Vector3(1f) * -float.MaxValue;

        // Move from a single primitive in a leaf to multiple
        public int PrimID;                     //  = -1;
        public int PrimitivesCount;            //  How many primitives there are in the leaf
        public int PrimitivesOffset;           //  Where in the flatten buffer the primitives are starting from
        public int NodeID;                     //  = 0;
        public int ParentID;                   //  = 0;
        public int LChildID;                   //  = 0;
        public int RChildID;                   //  = 0;

        // Used in the intersection methods
        public int SplitAxis;
        public float SplitValue;
        public int NearNodeID;
        public int FarNodeID;
        public int SplitMethod;

        public static BVHNode CreateBVHNode()
        {
            BVHNode b = new BVHNode
            {
                NodeType = 0,
                IsLeaf = 0,
                Centroid = Vector3.zero,
                Min = Vector3.one * float.MaxValue,
                Max = Vector3.one * -float.MaxValue,
                PrimID = -1,
                PrimitivesCount = 0,
                PrimitivesOffset = 0,
                NodeID = 0,
                ParentID = -1,
                LChildID = 0,
                RChildID = 0,
                NearNodeID = 0,
                FarNodeID = 0,
                SplitAxis = 0,
                SplitMethod = 1
            };
            return b;
        }

        public BVHNode(List<BVHTriangle> tris, List<BVHNode> nodes, ref List<BVHTriangle> finalPrims)
        {
            this = CreateBVHNode();

            Centroid = Vector3.zero;
            Min = Vector3.zero;
            Max = Vector3.zero;
            NodeID = 0;
            CalculateBBox(tris);
            nodes.Add(this);

            if (tris.Count > 0)
            {
                Build(0, tris, ref nodes, ref finalPrims);
            }
        }

        public void Build(int nodeID, List<BVHTriangle> tris, ref List<BVHNode> nodes, ref List<BVHTriangle> finalPrims)
        {
            BVHNode node = nodes[nodeID];

            // Leaf
            if (tris.Count <= BVH.MaxPrimsCountPerNode)
            {
                // NOTE : the prims offset is calculated in the " Flattening BVH " method
                node.IsLeaf = 1;

                node.PrimitivesCount = 1;
                node.PrimitivesOffset = finalPrims.Count;

                finalPrims.Add(tris[0]);
                nodes[nodeID] = node;

            }
            else
            {

                List<BVHTriangle> lTris = new List<BVHTriangle>();
                List<BVHTriangle> rTris = new List<BVHTriangle>();

                switch (node.SplitMethod)
                {

                    // -------------------- Equal count split ! --------------------
                    case 0: //  (int)BVHSplitMethod.SPLIT_EQUAL_COUNTS:

                        node.GetLongestAxisAndValue();
                        int splitAxis = node.SplitAxis;

                        // Equal split
                        if (lTris.Count == 0 || rTris.Count == 0)
                        {
                            // In this case the incomming tris list should be sorted
                            switch (splitAxis)
                            {
                                case 0: tris.Sort(CompareX); break;
                                case 1: tris.Sort(CompareY); break;
                                case 2: tris.Sort(CompareZ); break;
                            }
                            int trisHalf = tris.Count / 2;

                            lTris = tris.GetRange(0, trisHalf);
                            rTris = tris.GetRange(trisHalf, tris.Count - trisHalf);
                        }

                        break;

                    // -------------------- Median axis split ! --------------------
                    case 1: // (int)BVHSplitMethod.SPLIT_MIDDLE:

                        node.GetLongestAxisAndValue();

                        float splitValue = node.SplitValue;
                        splitAxis = node.SplitAxis;


                        // Median split triangle buffer
                        switch (splitAxis)
                        {
                            case 0:
                                lTris = tris.FindAll(n => n.Centroid.x < splitValue);
                                rTris = tris.FindAll(n => n.Centroid.x >= splitValue);
                                break;
                            case 1:
                                lTris = tris.FindAll(n => n.Centroid.y < splitValue);
                                rTris = tris.FindAll(n => n.Centroid.y >= splitValue);
                                break;
                            case 2:
                                lTris = tris.FindAll(n => n.Centroid.z < splitValue);
                                rTris = tris.FindAll(n => n.Centroid.z >= splitValue);
                                break;
                        }

                        // If median split was not good enough
                        // Switch to equal split
                        if (lTris.Count == 0 || rTris.Count == 0)
                        {
                            // In this case the incomming tris list should be sorted
                            switch (splitAxis)
                            {
                                case 0: tris.Sort(CompareX); break;
                                case 1: tris.Sort(CompareY); break;
                                case 2: tris.Sort(CompareZ); break;
                            }

                            int trisHalf = tris.Count / 2;
                            lTris = tris.GetRange(0, trisHalf);
                            rTris = tris.GetRange(trisHalf, tris.Count - trisHalf);
                        }
                        //Debug.Log("Sliptting primitives using median split");
                        break;

                    // -------------------- Split using surface area heuristic ! --------------------
                    case 2: // BVHSplitMethod.SPLIT_SAH:

                        break;
                }
                BVHNode lChild = CreateBVHNode();
                BVHNode rChild = CreateBVHNode();

                lChild.NodeID = nodes.Count + 0;
                rChild.NodeID = nodes.Count + 1;

                lChild.ParentID = node.NodeID;
                rChild.ParentID = node.NodeID;

                lChild.NodeType = 1;
                rChild.NodeType = 2;

                node.LChildID = lChild.NodeID;
                node.RChildID = rChild.NodeID;

                lChild.CalculateBBox(lTris);
                rChild.CalculateBBox(rTris);

                // ----------------------------------------------------------------------------
                // Adding both children into the nodes list
                // ----------------------------------------------------------------------------

                nodes.Add(lChild);
                nodes.Add(rChild);

                // ----------------------------------------------------------------------------
                // Building the children nodes
                // ----------------------------------------------------------------------------

                // Use only when dealing with structs
                nodes[nodeID] = node;

                Build(lChild.NodeID, lTris, ref nodes, ref finalPrims);
                Build(rChild.NodeID, rTris, ref nodes, ref finalPrims);
            }
        }

        public void GetLongestAxisAndValue()
        {
            float xLength = Mathf.Abs(Min.x - Max.x);
            if (xLength < 0.000001f) xLength = 0f;
            float yLength = Mathf.Abs(Min.y - Max.y);
            if (yLength < 0.000001f) yLength = 0f;
            float zLength = Mathf.Abs(Min.z - Max.z);
            if (zLength < 0.000001f) zLength = 0f;

            float[] sides = new float[] { xLength, yLength, zLength };
            float l = Mathf.Max(sides);

            for (int i = 0; i < sides.Length; i++)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (l == sides[i])
                {
                    // Return a new array with the first element the axis ID 
                    // and the second element is the value which is the half
                    // of the length of the longest axis

                    SplitAxis = i;
                    SplitValue = Centroid[i];
                    return;
                }
            }

            SplitAxis = 0;
            SplitValue = 0f;

            Debug.LogError("NOTE:BBox longest side is not calculated properly!");
        }

        public GameObject CalculateBBox(List<BVHTriangle> tris) 
        {
            for (int tr = 0; tr < tris.Count; tr++)
            {
                Min = new Vector3(Mathf.Min(Min.x, tris[tr].V0.x), Mathf.Min(Min.y, tris[tr].V0.y), Mathf.Min(Min.z, tris[tr].V0.z));
                Max = new Vector3(Mathf.Max(Max.x, tris[tr].V0.x), Mathf.Max(Max.y, tris[tr].V0.y), Mathf.Max(Max.z, tris[tr].V0.z));

                Min = new Vector3(Mathf.Min(Min.x, tris[tr].V1.x), Mathf.Min(Min.y, tris[tr].V1.y), Mathf.Min(Min.z, tris[tr].V1.z));
                Max = new Vector3(Mathf.Max(Max.x, tris[tr].V1.x), Mathf.Max(Max.y, tris[tr].V1.y), Mathf.Max(Max.z, tris[tr].V1.z));

                Min = new Vector3(Mathf.Min(Min.x, tris[tr].V2.x), Mathf.Min(Min.y, tris[tr].V2.y), Mathf.Min(Min.z, tris[tr].V2.z));
                Max = new Vector3(Mathf.Max(Max.x, tris[tr].V2.x), Mathf.Max(Max.y, tris[tr].V2.y), Mathf.Max(Max.z, tris[tr].V2.z));
            }

            Centroid = (Min + Max) / 2f;

            return null;
        }

        // ==============================================================================================================
        // Comparers
        // ==============================================================================================================

        private static int CompareX(BVHTriangle h1, BVHTriangle h2)
        {
            if (h1.Centroid.x - h2.Centroid.x < 0f)
            {
                return -1;
            }
            return 1;
        }

        private static int CompareY(BVHTriangle h1, BVHTriangle h2)
        {
            if (h1.Centroid.y - h2.Centroid.y < 0f)
            {
                return -1;
            }
            return 1;
        }

        private static int CompareZ(BVHTriangle h1, BVHTriangle h2)
        {
            if (h1.Centroid.z - h2.Centroid.z < 0f)
            {
                return -1;
            }
            return 1;
        }
    }
}