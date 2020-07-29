using UnityEngine;
using System.Collections.Generic;
using AwesomeTechnologies.MeshTerrains;
using Unity.Collections;
using Unity.Mathematics;

namespace AwesomeTechnologies.Utility.BVHTree
{
    public struct HitInfo
    {
        public float3 HitPoint;
        public float3 HitNormal;
        public float HitDistance;
        public byte TerrainSourceID;

        public HitInfo(HitInfo hitInfo)
        {
            HitPoint = hitInfo.HitPoint;
            HitNormal = hitInfo.HitNormal;
            HitDistance = hitInfo.HitDistance;
            TerrainSourceID = hitInfo.TerrainSourceID;
        }

        public void Clear()
        {
            HitDistance = float.MaxValue;
        }
    }

    [System.Serializable]
    // ReSharper disable once InconsistentNaming
    public struct LBVHTriangle
    {
        public float3 V0;       // 12 bytes
        public float3 V1;       // 12 bytes
        public float3 V2;       // 12 bytes
        public float3 N;        // 12 bytes
        public int TerrainSourceID;

        public LBVHTriangle(float3 v0, float3 v1, float3 v2, float3 n, int terrainSourceID)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            N  = n;
            TerrainSourceID = terrainSourceID;
        }

        public bool IntersectRay(BVHRay ray, out HitInfo hitInfo)
        {
            bool intersect = false;

            hitInfo.HitPoint = new float3(0f, 0f, 0f);
            hitInfo.HitNormal = new float3(0f, 1f, 0f);
            hitInfo.HitDistance = float.MaxValue;
            hitInfo.TerrainSourceID = (byte)TerrainSourceID;

            float3 rayO = ray.Origin, rayD = ray.Direction;

            float3 edge0 = V0 - rayO;
            float3 edge1 = V1 - rayO;
            float3 edge2 = V2 - rayO;

            float3 cross0 = math.normalize(math.cross(edge0, edge1));
            float3 cross1 = math.normalize(math.cross(edge1, edge2));
            float3 cross2 = math.normalize(math.cross(edge2, edge0));

            float angle0 = math.dot(cross0, rayD);
            float angle1 = math.dot(cross1, rayD);
            float angle2 = math.dot(cross2, rayD);

            if (angle0 < 0f && angle1 < 0f && angle2 < 0f)
            {
                float3 w0 = rayO - V0;
                var a = -math.dot(N, w0);
                var b = math.dot(N, rayD);
                var r = a / b;
                float3 I = rayO + rayD * r;
                if (a < 0f)
                {
                    hitInfo.HitPoint = I;
                    hitInfo.HitDistance = r;
                    hitInfo.HitNormal = math.normalize(N);
                    intersect = true;
                }
            }
            return intersect;
        }

        public bool IntersectRay(BVHRay ray, ref NativeArray<HitInfo> hitInfos, int hitInfoID)
        {
            float3 rayO = ray.Origin, rayD = ray.Direction;

            float3 edge0 = V0 - rayO;
            float3 edge1 = V1 - rayO;
            float3 edge2 = V2 - rayO;

            float3 cross0 = math.normalize(math.cross(edge0, edge1));
            float3 cross1 = math.normalize(math.cross(edge1, edge2));
            float3 cross2 = math.normalize(math.cross(edge2, edge0));

            float angle0 = math.dot(cross0, rayD);
            float angle1 = math.dot(cross1, rayD);
            float angle2 = math.dot(cross2, rayD);

            if (angle0 < 0f && angle1 < 0f && angle2 < 0f)
            {
                float3 w0 = rayO - V0;
                var a = -math.dot(N, w0);
                var b = math.dot(N, rayD);
                var r = a / b;
                float3 I = rayO + rayD * r;
                if (a < 0f)
                {
                    HitInfo hI = new HitInfo
                    {
                        HitNormal = math.normalize(N),
                        HitPoint = I,
                        HitDistance = r,
                        TerrainSourceID = (byte)TerrainSourceID
                    };
                    hitInfos[hitInfoID] = hI;
                    return true;
                }
            }
            return false;
        }
    }

    [System.Serializable]
    // ReSharper disable once InconsistentNaming
    public struct LBVHNODE              // .. bytes stride
    {
        public float3 BMin;             // 12 bytes
        public float3 BMax;             // 12 bytes
        public int NodeID;              // 4 bytes

        public int PrimitivesCount;     // 4 bytes
        public int PrimitivesOffset;    // 4 bytes

        public int ParentID;            // 4 bytes

        public int LChildID;            // 4 bytes
        public int RChildID;            // 4 bytes

        public int IsLeaf;              // 4 byte

        public int NearNodeID;          // 4 byte
        public int FarNodeID;           // 4 byte
        public int SplitAxis;           // 4 byte

        public LBVHNODE(BVHNode node)
        {
            BMin = node.Min;
            BMax = node.Max;
            NodeID = node.NodeID;
            PrimitivesCount = node.PrimitivesCount;
            PrimitivesOffset = node.PrimitivesOffset;
            ParentID = node.ParentID;
            LChildID = node.LChildID;
            RChildID = node.RChildID;
            IsLeaf = node.IsLeaf;
            SplitAxis = node.SplitAxis;
            NearNodeID = -1;
            FarNodeID = -1;
        }

        public void GetChildrenIDsAndSplitAxis(out int lChildID, out int rChildID, out int splitAxis)
        {
            lChildID = LChildID;
            rChildID = RChildID;
            splitAxis = SplitAxis;
        }

        public bool IntersectRay(BVHRay r)//, out float hitDist)
        {
            float tXmin, tXmax, tYmin, tYmax, tZmin, tZmax;
            float xA = 1f / r.Direction.x, yA = 1f / r.Direction.y, zA = 1f / r.Direction.z;
            float xE = r.Origin.x, yE = r.Origin.y, zE = r.Origin.z;

            // calculate t interval in x-axis
            if (xA >= 0)
            {
                tXmin = (BMin.x - xE) * xA;
                tXmax = (BMax.x - xE) * xA;
            }
            else
            {
                tXmin = (BMax.x - xE) * xA;
                tXmax = (BMin.x - xE) * xA;
            }

            // calculate t interval in y-axis
            if (yA >= 0)
            {
                tYmin = (BMin.y - yE) * yA;
                tYmax = (BMax.y - yE) * yA;
            }
            else
            {
                tYmin = (BMax.y - yE) * yA;
                tYmax = (BMin.y - yE) * yA;
            }

            // calculate t interval in z-axis
            if (zA >= 0)
            {
                tZmin = (BMin.z - zE) * zA;
                tZmax = (BMax.z - zE) * zA;
            }
            else
            {
                tZmin = (BMax.z - zE) * zA;
                tZmax = (BMin.z - zE) * zA;
            }

            // find if there an intersection among three t intervals

            // float3
            var tMin = math.max(tXmin, math.max(tYmin, tZmin));
            var tMax = math.min(tXmax, math.min(tYmax, tZmax));

            // Vector3
            //t_min = Mathf.Max(t_xmin, Mathf.Max(t_ymin, t_zmin));
            //t_max = Mathf.Min(t_xmax, Mathf.Min(t_ymax, t_zmax));

            //hitDist = t_min;
            return (tMin <= tMax);
        }
    }

    [System.Serializable]
    public struct ObjectData
    {
        public MeshRenderer Renderer;
        public Mesh Mesh;
        public int SubMesheCount;
        public List<Vector3> VerticeList;
        public List<Vector3> NormalList;
        public int[] Indices;
        public bool HasNormals;
        public BVHNode BVH;
        public bool IsValid;
        public List<BVHNode> Nodes;
        public List<BVHTriangle> Prims;
        public int TerrainSourceID;

        public ObjectData(MeshRenderer r, int terrainSourceID)
        {
            Renderer = r;
            Mesh = r.GetComponent<MeshFilter>().sharedMesh;
            IsValid = Mesh != null;
            
            SubMesheCount = 0;
            VerticeList = null;
            NormalList = null;
            Indices = null;
            HasNormals = false;
            Prims = null;
            Nodes = null;
            TerrainSourceID = terrainSourceID;

            BVH = new BVHNode();

            if (IsValid)
            {
                SubMesheCount = Mesh.subMeshCount;
                VerticeList = new List<Vector3>();
                Mesh.GetVertices(VerticeList);
                NormalList = new List<Vector3>();
                Mesh.GetNormals(NormalList);
                Indices = new int[Mesh.triangles.Length];
                Indices = Mesh.triangles;
                HasNormals = NormalList.Count > 0;

                Matrix4x4 mtrx = Renderer.localToWorldMatrix;
                for (int v = 0; v < VerticeList.Count; v++)
                {
                    VerticeList[v] = mtrx.MultiplyPoint3x4(VerticeList[v]);
                    NormalList[v] = mtrx.MultiplyVector(NormalList[v]);
                }
            }
        }
    }
}



//using UnityEngine;
//using System.Collections.Generic;
//using AwesomeTechnologies.MeshTerrains;
//using Unity.Mathematics;

//namespace AwesomeTechnologies.Utility.BVHTree
//{
//    public struct HitInfo
//    {
//        public float3 hitPoint;
//        public float3 hitNormal;
//        public float hitDistance;
//        public float3 barycentricCoordinate;
//        public float2 textureCoordinates;
//        public int triangleIndex;

//        public void CopyFrom(HitInfo hitInfo)
//        {
//            hitPoint               = hitInfo.hitPoint;
//            hitNormal              = hitInfo.hitNormal;
//            hitDistance            = hitInfo.hitDistance;
//            barycentricCoordinate  = hitInfo.barycentricCoordinate;
//            textureCoordinates     = hitInfo.textureCoordinates;
//            triangleIndex          = hitInfo.triangleIndex;
//        }

//        public HitInfo(HitInfo hitInfo)
//        {
//            hitPoint               = hitInfo.hitPoint;
//            hitNormal              = hitInfo.hitNormal;
//            hitDistance            = hitInfo.hitDistance;
//            barycentricCoordinate  = hitInfo.barycentricCoordinate;
//            textureCoordinates     = hitInfo.textureCoordinates;
//            triangleIndex          = hitInfo.triangleIndex;
//        }

//        public void Clear()
//        {
//            hitDistance = float.MaxValue;
//        }

//        public HitInfo( float3 hitPoint,
//                        float3 hitNormal,
//                        float hitDistance,
//                        float3 barycentricCoordinate,
//                        float2 textureCoordinates,
//                        int triangleIndex)
//        {
//            this.hitPoint = hitPoint;
//            this.hitNormal = hitNormal;
//            this.hitDistance = hitDistance;
//            this.textureCoordinates = textureCoordinates;
//            this.barycentricCoordinate = barycentricCoordinate;
//            this.triangleIndex = triangleIndex;
//        }
//    }

//    public struct BVHTriangleIJob
//    {
//        public float3 v0, v1, v2, n0, n1, n2, n;

//        public int mtlID;

//        public BVHTriangleIJob(BVHTriangle t)
//        {
//            v0 = t.v0;
//            v1 = t.v1;
//            v2 = t.v2;
//            n0 = t.n0;
//            n1 = t.n1;
//            n2 = t.n2;
//            n = t.n;
//            mtlID = t.mtlID;
//        }
//    }

//    [System.Serializable]
//    public struct LBVHTriangle
//    {
//        public float3  v0;        // 12 bytes
//        public float3 v1;         // 12 bytes
//        public float3 v2;         // 12 bytes
//        public float3 n0;         // 12 bytes
//        public float3 n1;         // 12 bytes
//        public float3 n2;         // 12 bytes
//        public float3 normal;     // 12 bytes
//        public int    primID;     //  4 bytes
//        public int    mtlID;      //  4 bytes ----------
//        public float3 empty;      // 12 bytes ----------

//        public LBVHTriangle(float3 _v0, float3 _v1, float3 _v2,
//            float3 _n0, float3 _n1, float3 _n2,
//                            int _primID,
//                            int _mtlID)
//        {
//            v0      = _v0;
//            v1      = _v1;
//            v2      = _v2;
//            n0      = _n0;
//            n1      = _n1;
//            n2      = _n2;
//            primID  = _primID;
//            mtlID   = _mtlID;

//            normal = math.cross(math.normalize(v1 - v0), math.normalize(v2 - v0));
//            empty   = new float3(0f, 0f, 0f);
//        }

//        public bool IntersectRay(BVHRay ray, ref HitInfo hitInfo)
//        {
//            float3 rayO = ray.origin, rayD = ray.direction;
//            double edge0x = v0.x - rayO.x, edge0y = v0.y - rayO.y, edge0z = v0.z - rayO.z;
//            double edge1x = v1.x - rayO.x, edge1y = v1.y - rayO.y, edge1z = v1.z - rayO.z;
//            double edge2x = v2.x - rayO.x, edge2y = v2.y - rayO.y, edge2z = v2.z - rayO.z;
//            double cross11 = edge0y * edge1z - edge0z * edge1y;
//            double cross12 = edge0z * edge1x - edge0x * edge1z;
//            double cross13 = edge0x * edge1y - edge0y * edge1x;
//            double cross21 = edge1y * edge2z - edge1z * edge2y;
//            double cross22 = edge1z * edge2x - edge1x * edge2z;
//            double cross23 = edge1x * edge2y - edge1y * edge2x;
//            double cross31 = edge2y * edge0z - edge2z * edge0y;
//            double cross32 = edge2z * edge0x - edge2x * edge0z;
//            double cross33 = edge2x * edge0y - edge2y * edge0x;
//            double angle1 = rayD.x * cross11 + rayD.y * cross12 + rayD.z * cross13;
//            double angle2 = rayD.x * cross21 + rayD.y * cross22 + rayD.z * cross23;
//            double angle3 = rayD.x * cross31 + rayD.y * cross32 + rayD.z * cross33;

//            if (angle1 < 0f && angle2 < 0f && angle3 < 0f)
//            {
//                float r, a, b;
//                float3 w0 = rayO - v0;
//                a = -math.dot(normal, w0);
//                b = math.dot(normal, rayD);
//                r = a / b;
//                float3 I = rayO + rayD * r;
//                if (a < 0f)
//                {
//                        hitInfo.hitPoint = I;
//                        hitInfo.hitDistance = math.distance(rayO, I);
//                        hitInfo.hitNormal = normal;
//                        hitInfo.triangleIndex = 0;
//                        hitInfo.barycentricCoordinate = new float3(0f, 0f, 0f); 
//                        hitInfo.triangleIndex = primID;
//                        return true;
//                }
//            }
//            return false;
//        }
//    }

//    [System.Serializable]
//    public struct LBVHNODE              // .. bytes stride
//    {
//        public float3 bMin;            // 12 bytes
//        public float3 bMax;            // 12 bytes
//        public int nodeID;              // 4 bytes

//        public int primitivesCount;     // 4 bytes
//        public int primitivesOffset;    // 4 bytes

//        public int parentID;            // 4 bytes

//        public int LChildID;            // 4 bytes
//        public int RChildID;            // 4 bytes

//        public int isLeaf;              // 4 byte

//        public int nearNodeID;
//        public int farNodeID;
//        public int splitAxis;

//        public LBVHNODE(BVHNode node)
//        {
//            bMin                = node.min;
//            bMax                = node.max;
//            nodeID              = node.nodeID;
//            primitivesCount     = node.primitivesCount;
//            primitivesOffset    = node.primitivesOffset;
//            parentID            = node.parentID;
//            LChildID            = node.LChildID;
//            RChildID            = node.RChildID;
//            isLeaf              = node.isLeaf;
//            splitAxis           = node.splitAxis;

//            nearNodeID  = -1;
//            farNodeID   = -1;
//        }

//        public LBVHNODE(float3 bMin, float3 bMax, int parentID, int LChildID, int RChildID, int nodeID, int splitAxis, int primitivesCount, int primitivesOffset, int isLeaf)
//        {
//            this.bMin               = bMin;
//            this.bMax               = bMax;
//            this.nodeID             = nodeID;
//            this.primitivesCount    = primitivesCount;
//            this.primitivesOffset   = primitivesOffset;
//            this.parentID           = parentID;
//            this.LChildID           = LChildID;
//            this.RChildID           = RChildID;
//            this.isLeaf             = isLeaf;
//            this.splitAxis          = splitAxis;
//            nearNodeID  = -1;
//            farNodeID   = -1;
//        }
//    }

//    [System.Serializable]
//    public struct ObjectData
//    {
//        public MeshRenderer     rend;
//        public Mesh             mesh;
//        public int              subMeshes;
//        public List<Vector3>    verts;
//        public List<Vector3>    norms;
//        public int[]            tris;
//        public bool             hasNormals;
//        public BVHNode          bvh;
//        public bool             isValid;
//        public List<BVHNode>     nodes;
//        public List<BVHTriangle> prims;

//        public ObjectData(MeshRenderer r)
//        {
//            rend        = r;
//            mesh        = r.GetComponent<MeshFilter>().sharedMesh;
//            isValid     = mesh != null;

//            subMeshes   = mesh.subMeshCount;
//            verts       = null;
//            norms       = null;
//            tris        = null;
//            hasNormals  = false;
//            prims       = null;
//            nodes       = null;

//            bvh = new BVHNode();

//            if (isValid)
//            {
//                subMeshes = mesh.subMeshCount;
//                verts = new List<Vector3>();
//                mesh.GetVertices(verts);
//                norms = new List<Vector3>();
//                mesh.GetNormals(norms);
//                tris = new int[mesh.triangles.Length];
//                tris = mesh.triangles;
//                hasNormals = norms != null && norms.Count > 0;

//                Matrix4x4 mtrx = rend.localToWorldMatrix;
//                for (int v = 0; v < verts.Count; v++)
//                {
//                    verts[v] = mtrx.MultiplyPoint3x4(verts[v]);
//                    norms[v] = mtrx.MultiplyVector(norms[v]);
//                }
//            }
//        }
//    }
//}

//﻿using UnityEngine;
//using System.Collections.Generic;
//using AwesomeTechnologies.MeshTerrains;
//using Unity.Mathematics;

//namespace AwesomeTechnologies.Utility.BVHTree
//{
//    public struct HitInfo
//    {
//        public float3 hitPoint;
//        public float3 hitNormal;
//        public float hitDistance;
//        public float3 barycentricCoordinate;
//        public float2 textureCoordinates;
//        public int triangleIndex;
//        public int intersection;

//        public void CopyFrom(HitInfo hitInfo)
//        {
//            hitPoint = hitInfo.hitPoint;
//            hitNormal = hitInfo.hitNormal;
//            hitDistance = hitInfo.hitDistance;
//            barycentricCoordinate = hitInfo.barycentricCoordinate;
//            textureCoordinates = hitInfo.textureCoordinates;
//            triangleIndex = hitInfo.triangleIndex;
//            intersection = hitInfo.intersection;
//        }

//        public HitInfo(HitInfo hitInfo)
//        {
//            hitPoint = hitInfo.hitPoint;
//            hitNormal = hitInfo.hitNormal;
//            hitDistance = hitInfo.hitDistance;
//            barycentricCoordinate = hitInfo.barycentricCoordinate;
//            textureCoordinates = hitInfo.textureCoordinates;
//            triangleIndex = hitInfo.triangleIndex;
//            intersection = hitInfo.intersection;
//        }

//        public void Clear()
//        {
//            hitDistance = float.MaxValue;
//        }

//        public HitInfo(float3 hitPoint,
//                        float3 hitNormal,
//                        float hitDistance,
//                        float3 barycentricCoordinate,
//                        float2 textureCoordinates,
//                        int triangleIndex,
//                        int intersection)
//        {
//            this.hitPoint = hitPoint;
//            this.hitNormal = hitNormal;
//            this.hitDistance = hitDistance;
//            this.textureCoordinates = textureCoordinates;
//            this.barycentricCoordinate = barycentricCoordinate;
//            this.triangleIndex = triangleIndex;
//            this.intersection = intersection;
//        }
//    }

//    public struct BVHTriangleIJob
//    {
//        public float3 v0, v1, v2, n0, n1, n2, n;

//        public int mtlID;

//        public BVHTriangleIJob(BVHTriangle t)
//        {
//            v0 = t.v0;
//            v1 = t.v1;
//            v2 = t.v2;
//            n0 = t.n0;
//            n1 = t.n1;
//            n2 = t.n2;
//            n = t.n;
//            mtlID = t.mtlID;
//        }
//    }

//    [System.Serializable]
//    public struct LBVHTriangle
//    {
//        public float3 v0;         // 12 bytes
//        public float3 v1;         // 12 bytes
//        public float3 v2;         // 12 bytes
//        public float3 n0;         // 12 bytes
//        public float3 n1;         // 12 bytes
//        public float3 n2;         // 12 bytes
//        public float3 normal;     // 12 bytes
//        public int primID;     //  4 bytes
//        public int mtlID;      //  4 bytes ----------

//        public LBVHTriangle(float3 _v0, float3 _v1, float3 _v2,
//                            float3 _n0, float3 _n1, float3 _n2,
//                            int _primID,
//                            int _mtlID)
//        {
//            v0 = _v0;
//            v1 = _v1;
//            v2 = _v2;
//            n0 = _n0;
//            n1 = _n1;
//            n2 = _n2;
//            primID = _primID;
//            mtlID = _mtlID;

//            normal = math.cross(math.normalize(v1 - v0), math.normalize(v2 - v0));
//        }

//        public bool IntersectRay(BVHRay ray, ref HitInfo hitInfo)
//        {
//            float3 rayO = ray.origin, rayD = ray.direction;
//            double edge0x = v0.x - rayO.x, edge0y = v0.y - rayO.y, edge0z = v0.z - rayO.z;
//            double edge1x = v1.x - rayO.x, edge1y = v1.y - rayO.y, edge1z = v1.z - rayO.z;
//            double edge2x = v2.x - rayO.x, edge2y = v2.y - rayO.y, edge2z = v2.z - rayO.z;
//            double cross11 = edge0y * edge1z - edge0z * edge1y;
//            double cross12 = edge0z * edge1x - edge0x * edge1z;
//            double cross13 = edge0x * edge1y - edge0y * edge1x;
//            double cross21 = edge1y * edge2z - edge1z * edge2y;
//            double cross22 = edge1z * edge2x - edge1x * edge2z;
//            double cross23 = edge1x * edge2y - edge1y * edge2x;
//            double cross31 = edge2y * edge0z - edge2z * edge0y;
//            double cross32 = edge2z * edge0x - edge2x * edge0z;
//            double cross33 = edge2x * edge0y - edge2y * edge0x;
//            double angle1 = rayD.x * cross11 + rayD.y * cross12 + rayD.z * cross13;
//            double angle2 = rayD.x * cross21 + rayD.y * cross22 + rayD.z * cross23;
//            double angle3 = rayD.x * cross31 + rayD.y * cross32 + rayD.z * cross33;

//            if (angle1 < 0f && angle2 < 0f && angle3 < 0f)
//            {
//                float r, a, b;
//                float3 w0 = rayO - v0;
//                a = -math.dot(normal, w0);
//                b = math.dot(normal, rayD);
//                r = a / b;
//                float3 I = rayO + rayD * r;
//                if (a < 0f)
//                {
//                    hitInfo.hitPoint = I;
//                    hitInfo.hitDistance = math.distance(rayO, I);
//                    hitInfo.hitNormal = normal;
//                    hitInfo.triangleIndex = 0;
//                    hitInfo.barycentricCoordinate = new float3(0f, 0f, 0f);
//                    hitInfo.triangleIndex = primID;
//                    return true;
//                }
//            }
//            return false;
//        }
//    }

//    [System.Serializable]
//    public struct LBVHNODE              // .. bytes stride
//    {
//        public float3 bMin;             // 12 bytes
//        public float3 bMax;             // 12 bytes
//        public int nodeID;              // 4 bytes

//        public int primitivesCount;     // 4 bytes
//        public int primitivesOffset;    // 4 bytes

//        public int parentID;            // 4 bytes

//        public int LChildID;            // 4 bytes
//        public int RChildID;            // 4 bytes

//        public int isLeaf;              // 4 byte

//        public int nearNodeID;          // 4 byte
//        public int farNodeID;           // 4 byte
//        public int splitAxis;           // 4 byte

//        public LBVHNODE(BVHNode node)
//        {
//            bMin = node.min;
//            bMax = node.max;
//            nodeID = node.nodeID;
//            primitivesCount = node.primitivesCount;
//            primitivesOffset = node.primitivesOffset;
//            parentID = node.parentID;
//            LChildID = node.LChildID;
//            RChildID = node.RChildID;
//            isLeaf = node.isLeaf;
//            splitAxis = node.splitAxis;
//            nearNodeID = -1;
//            farNodeID = -1;
//        }

//        public LBVHNODE(float3 bMin, float3 bMax, int parentID, int LChildID, int RChildID, int nodeID, int splitAxis, int primitivesCount, int primitivesOffset, int isLeaf)
//        {
//            this.bMin = bMin;
//            this.bMax = bMax;
//            this.nodeID = nodeID;
//            this.primitivesCount = primitivesCount;
//            this.primitivesOffset = primitivesOffset;
//            this.parentID = parentID;
//            this.LChildID = LChildID;
//            this.RChildID = RChildID;
//            this.isLeaf = isLeaf;
//            this.splitAxis = splitAxis;
//            nearNodeID = -1;
//            farNodeID = -1;
//        }

//        public void SetNearFarNodes(int nearNodeID, int farNodeID)
//        {
//            this.nearNodeID = nearNodeID;
//            this.farNodeID = farNodeID;
//        }
//    }

//    [System.Serializable]
//    public struct ObjectData
//    {
//        public MeshRenderer rend;
//        public Mesh mesh;
//        public int subMeshes;
//        public List<Vector3> verts;
//        public List<Vector3> norms;
//        public int[] tris;
//        public bool hasNormals;
//        public BVHNode bvh;
//        public bool isValid;
//        public List<BVHNode> nodes;
//        public List<BVHTriangle> prims;

//        public ObjectData(MeshRenderer r)
//        {
//            rend = r;
//            mesh = r.GetComponent<MeshFilter>().sharedMesh;
//            isValid = mesh != null;

//            subMeshes = mesh.subMeshCount;
//            verts = null;
//            norms = null;
//            tris = null;
//            hasNormals = false;
//            prims = null;
//            nodes = null;

//            bvh = new BVHNode();

//            if (isValid)
//            {
//                subMeshes = mesh.subMeshCount;
//                verts = new List<Vector3>();
//                mesh.GetVertices(verts);
//                norms = new List<Vector3>();
//                mesh.GetNormals(norms);
//                tris = new int[mesh.triangles.Length];
//                tris = mesh.triangles;
//                hasNormals = norms != null && norms.Count > 0;

//                Matrix4x4 mtrx = rend.localToWorldMatrix;
//                for (int v = 0; v < verts.Count; v++)
//                {
//                    verts[v] = mtrx.MultiplyPoint3x4(verts[v]);
//                    norms[v] = mtrx.MultiplyVector(norms[v]);
//                }
//            }
//        }
//    }
//}



////using UnityEngine;
////using System.Collections.Generic;
////using AwesomeTechnologies.MeshTerrains;
////using Unity.Mathematics;

////namespace AwesomeTechnologies.Utility.BVHTree
////{
////    public struct HitInfo
////    {
////        public float3 hitPoint;
////        public float3 hitNormal;
////        public float hitDistance;
////        public float3 barycentricCoordinate;
////        public float2 textureCoordinates;
////        public int triangleIndex;

////        public void CopyFrom(HitInfo hitInfo)
////        {
////            hitPoint               = hitInfo.hitPoint;
////            hitNormal              = hitInfo.hitNormal;
////            hitDistance            = hitInfo.hitDistance;
////            barycentricCoordinate  = hitInfo.barycentricCoordinate;
////            textureCoordinates     = hitInfo.textureCoordinates;
////            triangleIndex          = hitInfo.triangleIndex;
////        }

////        public HitInfo(HitInfo hitInfo)
////        {
////            hitPoint               = hitInfo.hitPoint;
////            hitNormal              = hitInfo.hitNormal;
////            hitDistance            = hitInfo.hitDistance;
////            barycentricCoordinate  = hitInfo.barycentricCoordinate;
////            textureCoordinates     = hitInfo.textureCoordinates;
////            triangleIndex          = hitInfo.triangleIndex;
////        }

////        public void Clear()
////        {
////            hitDistance = float.MaxValue;
////        }

////        public HitInfo( float3 hitPoint,
////                        float3 hitNormal,
////                        float hitDistance,
////                        float3 barycentricCoordinate,
////                        float2 textureCoordinates,
////                        int triangleIndex)
////        {
////            this.hitPoint = hitPoint;
////            this.hitNormal = hitNormal;
////            this.hitDistance = hitDistance;
////            this.textureCoordinates = textureCoordinates;
////            this.barycentricCoordinate = barycentricCoordinate;
////            this.triangleIndex = triangleIndex;
////        }
////    }

////    public struct BVHTriangleIJob
////    {
////        public float3 v0, v1, v2, n0, n1, n2, n;

////        public int mtlID;

////        public BVHTriangleIJob(BVHTriangle t)
////        {
////            v0 = t.v0;
////            v1 = t.v1;
////            v2 = t.v2;
////            n0 = t.n0;
////            n1 = t.n1;
////            n2 = t.n2;
////            n = t.n;
////            mtlID = t.mtlID;
////        }
////    }

////    [System.Serializable]
////    public struct LBVHTriangle
////    {
////        public float3  v0;        // 12 bytes
////        public float3 v1;         // 12 bytes
////        public float3 v2;         // 12 bytes
////        public float3 n0;         // 12 bytes
////        public float3 n1;         // 12 bytes
////        public float3 n2;         // 12 bytes
////        public float3 normal;     // 12 bytes
////        public int    primID;     //  4 bytes
////        public int    mtlID;      //  4 bytes ----------
////        public float3 empty;      // 12 bytes ----------

////        public LBVHTriangle(float3 _v0, float3 _v1, float3 _v2,
////            float3 _n0, float3 _n1, float3 _n2,
////                            int _primID,
////                            int _mtlID)
////        {
////            v0      = _v0;
////            v1      = _v1;
////            v2      = _v2;
////            n0      = _n0;
////            n1      = _n1;
////            n2      = _n2;
////            primID  = _primID;
////            mtlID   = _mtlID;

////            normal = math.cross(math.normalize(v1 - v0), math.normalize(v2 - v0));
////            empty   = new float3(0f, 0f, 0f);
////        }

////        public bool IntersectRay(BVHRay ray, ref HitInfo hitInfo)
////        {
////            float3 rayO = ray.origin, rayD = ray.direction;
////            double edge0x = v0.x - rayO.x, edge0y = v0.y - rayO.y, edge0z = v0.z - rayO.z;
////            double edge1x = v1.x - rayO.x, edge1y = v1.y - rayO.y, edge1z = v1.z - rayO.z;
////            double edge2x = v2.x - rayO.x, edge2y = v2.y - rayO.y, edge2z = v2.z - rayO.z;
////            double cross11 = edge0y * edge1z - edge0z * edge1y;
////            double cross12 = edge0z * edge1x - edge0x * edge1z;
////            double cross13 = edge0x * edge1y - edge0y * edge1x;
////            double cross21 = edge1y * edge2z - edge1z * edge2y;
////            double cross22 = edge1z * edge2x - edge1x * edge2z;
////            double cross23 = edge1x * edge2y - edge1y * edge2x;
////            double cross31 = edge2y * edge0z - edge2z * edge0y;
////            double cross32 = edge2z * edge0x - edge2x * edge0z;
////            double cross33 = edge2x * edge0y - edge2y * edge0x;
////            double angle1 = rayD.x * cross11 + rayD.y * cross12 + rayD.z * cross13;
////            double angle2 = rayD.x * cross21 + rayD.y * cross22 + rayD.z * cross23;
////            double angle3 = rayD.x * cross31 + rayD.y * cross32 + rayD.z * cross33;

////            if (angle1 < 0f && angle2 < 0f && angle3 < 0f)
////            {
////                float r, a, b;
////                float3 w0 = rayO - v0;
////                a = -math.dot(normal, w0);
////                b = math.dot(normal, rayD);
////                r = a / b;
////                float3 I = rayO + rayD * r;
////                if (a < 0f)
////                {
////                        hitInfo.hitPoint = I;
////                        hitInfo.hitDistance = math.distance(rayO, I);
////                        hitInfo.hitNormal = normal;
////                        hitInfo.triangleIndex = 0;
////                        hitInfo.barycentricCoordinate = new float3(0f, 0f, 0f); 
////                        hitInfo.triangleIndex = primID;
////                        return true;
////                }
////            }
////            return false;
////        }
////    }

////    [System.Serializable]
////    public struct LBVHNODE              // .. bytes stride
////    {
////        public float3 bMin;            // 12 bytes
////        public float3 bMax;            // 12 bytes
////        public int nodeID;              // 4 bytes

////        public int primitivesCount;     // 4 bytes
////        public int primitivesOffset;    // 4 bytes

////        public int parentID;            // 4 bytes

////        public int LChildID;            // 4 bytes
////        public int RChildID;            // 4 bytes

////        public int isLeaf;              // 4 byte

////        public int nearNodeID;
////        public int farNodeID;
////        public int splitAxis;

////        public LBVHNODE(BVHNode node)
////        {
////            bMin                = node.min;
////            bMax                = node.max;
////            nodeID              = node.nodeID;
////            primitivesCount     = node.primitivesCount;
////            primitivesOffset    = node.primitivesOffset;
////            parentID            = node.parentID;
////            LChildID            = node.LChildID;
////            RChildID            = node.RChildID;
////            isLeaf              = node.isLeaf;
////            splitAxis           = node.splitAxis;

////            nearNodeID  = -1;
////            farNodeID   = -1;
////        }

////        public LBVHNODE(float3 bMin, float3 bMax, int parentID, int LChildID, int RChildID, int nodeID, int splitAxis, int primitivesCount, int primitivesOffset, int isLeaf)
////        {
////            this.bMin               = bMin;
////            this.bMax               = bMax;
////            this.nodeID             = nodeID;
////            this.primitivesCount    = primitivesCount;
////            this.primitivesOffset   = primitivesOffset;
////            this.parentID           = parentID;
////            this.LChildID           = LChildID;
////            this.RChildID           = RChildID;
////            this.isLeaf             = isLeaf;
////            this.splitAxis          = splitAxis;
////            nearNodeID  = -1;
////            farNodeID   = -1;
////        }
////    }

////    [System.Serializable]
////    public struct ObjectData
////    {
////        public MeshRenderer     rend;
////        public Mesh             mesh;
////        public int              subMeshes;
////        public List<Vector3>    verts;
////        public List<Vector3>    norms;
////        public int[]            tris;
////        public bool             hasNormals;
////        public BVHNode          bvh;
////        public bool             isValid;
////        public List<BVHNode>     nodes;
////        public List<BVHTriangle> prims;

////        public ObjectData(MeshRenderer r)
////        {
////            rend        = r;
////            mesh        = r.GetComponent<MeshFilter>().sharedMesh;
////            isValid     = mesh != null;

////            subMeshes   = mesh.subMeshCount;
////            verts       = null;
////            norms       = null;
////            tris        = null;
////            hasNormals  = false;
////            prims       = null;
////            nodes       = null;

////            bvh = new BVHNode();

////            if (isValid)
////            {
////                subMeshes = mesh.subMeshCount;
////                verts = new List<Vector3>();
////                mesh.GetVertices(verts);
////                norms = new List<Vector3>();
////                mesh.GetNormals(norms);
////                tris = new int[mesh.triangles.Length];
////                tris = mesh.triangles;
////                hasNormals = norms != null && norms.Count > 0;

////                Matrix4x4 mtrx = rend.localToWorldMatrix;
////                for (int v = 0; v < verts.Count; v++)
////                {
////                    verts[v] = mtrx.MultiplyPoint3x4(verts[v]);
////                    norms[v] = mtrx.MultiplyVector(norms[v]);
////                }
////            }
////        }
////    }
////}