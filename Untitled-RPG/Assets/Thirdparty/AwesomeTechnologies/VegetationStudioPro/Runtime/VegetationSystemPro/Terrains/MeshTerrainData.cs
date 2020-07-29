using System.Collections.Generic;
using AwesomeTechnologies.Utility.BVHTree;
using UnityEngine;

namespace AwesomeTechnologies.MeshTerrains
{
    [PreferBinarySerialization]
    [System.Serializable]
    public class MeshTerrainData : ScriptableObject
    {
        public Bounds Bounds;
        public int TriangleCount;

        public List<LBVHNODE> lNodes = new List<LBVHNODE>();
        public List<LBVHTriangle> lPrims = new List<LBVHTriangle>();
        public List<byte> CoverageList = new List<byte>();
    }
}
