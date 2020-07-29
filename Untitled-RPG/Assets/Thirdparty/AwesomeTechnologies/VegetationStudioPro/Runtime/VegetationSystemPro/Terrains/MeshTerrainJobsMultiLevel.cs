using AwesomeTechnologies.Utility.BVHTree;
using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
// ReSharper disable InlineOutVariableDeclaration

namespace AwesomeTechnologies.MeshTerrains
{

    [BurstCompile(CompileSynchronously = true)]
    public struct UpdateBVHInstanceListMultiLevelJob : IJob
    {
        [ReadOnly] public NativeArray<HitInfo> RaycastHits;
        //public NativeList<VegetationInstance> InstanceList;
        //[ReadOnly] public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;

        public NativeList<float3> Position;
        public NativeList<quaternion> Rotation;
        public NativeList<float3> Scale;
        public NativeList<float3> TerrainNormal;
        public NativeList<float> BiomeDistance;
        public NativeList<byte> TerrainTextureData;
        public NativeList<int> RandomNumberIndex;
        public NativeList<float> DistanceFalloff;
        public NativeList<float> VegetationMaskDensity;
        public NativeList<float> VegetationMaskScale;
        public NativeList<byte> TerrainSourceID;        
        public NativeList<byte> TextureMaskData;
        public NativeList<byte> Excluded;
        public NativeList<byte> HeightmapSampled;
        
        public void Execute()
        {
            for (int i = 0; i <= RaycastHits.Length - 1; i++)
            {
                HitInfo raycastHit = RaycastHits[i];
                if (raycastHit.HitDistance > 0)
                {
                    Position.Add(raycastHit.HitPoint);
                    Rotation.Add(UnityEngine.Quaternion.Euler(0, 45, 0));
                    Scale.Add(new float3(1, 1, 1));
                    TerrainNormal.Add(raycastHit.HitNormal);
                    BiomeDistance.Add(100000);
                    TerrainTextureData.Add(0);
                    RandomNumberIndex.Add(i);
                    DistanceFalloff.Add(1);
                    VegetationMaskDensity.Add(0);
                    VegetationMaskScale.Add(0);
                    TerrainSourceID.Add(raycastHit.TerrainSourceID);
                    TextureMaskData.Add(0);
                    Excluded.Add(0);
                    HeightmapSampled.Add(0);
                }
            }
        }
    }

    public enum TraverseState
    {
        FromParent,
        FromSibling,
        FromChild
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct SampleBVHTreeMultiLevelJob : IJob
    {
        [ReadOnly] public NativeArray<BVHRay> Rays;
        public NativeList<HitInfo> HitInfos;
        [ReadOnly] public NativeArray<LBVHNODE> NativeNodes;
        [ReadOnly] public NativeArray<LBVHTriangle> NativePrims;
        public NativeArray<HitInfo> TempHi;

        public void Execute()
        {
            for (int i = 0; i <= Rays.Length - 1; i++)
            {
                BVHRay ray = Rays[i];
                if (ray.DoRaycast == 0)
                {
                    continue;
                }

                RayCastStackless(i);
            }
        }

        public bool RayCastStackless(int index)
        {
            LBVHNODE current = NativeNodes[0];
            float3 rayDirection = Rays[index].Direction;

            // ----------------------------------------------------------------------------------------------------            
            float sA = rayDirection[current.SplitAxis];
            current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
            current.FarNodeID = math.@select(current.RChildID, current.LChildID, sA < 0f);
            // ----------------------------------------------------------------------------------------------------

            int rootNearID = current.NearNodeID;
            int rootNodeID = current.NodeID;
            current = NativeNodes[rootNearID];

            TraverseState state = TraverseState.FromParent;
            bool intersect = false;

            while (current.NodeID != rootNodeID)
            {
                switch (state)
                {
                    case TraverseState.FromChild:

                        int cID = current.NodeID;

                        current = NativeNodes[current.ParentID];

                        // ----------------------------------------------------------------------------------------------------                        
                        sA = rayDirection[current.SplitAxis];
                        current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
                        current.FarNodeID = math.@select(current.RChildID, current.LChildID, sA < 0f);

                        // ----------------------------------------------------------------------------------------------------


                        if (cID == current.NearNodeID)
                        {
                            current = NativeNodes[current.FarNodeID];
                            state = TraverseState.FromSibling;
                        }
                        else
                        {
                            state = TraverseState.FromChild;
                        }

                        break;

                    case TraverseState.FromSibling:

                        // ReSharper disable once NotAccessedVariable
                        float dist;
                        if (!BVHBBox.IntersectRay(Rays[index], current.BMin, current.BMax, out dist))
                        {
                            current = NativeNodes[current.ParentID];
                            state = TraverseState.FromChild;
                        }
                        else if (current.IsLeaf == 1)
                        {

                            if (NativePrims[current.PrimitivesOffset].IntersectRay(Rays[index], ref TempHi, index))
                            {
                                HitInfos.Add(TempHi[index]);
                                intersect = true;
                            }

                            current = NativeNodes[current.ParentID];
                            state = TraverseState.FromChild;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------------------------------
                            sA = rayDirection[current.SplitAxis];
                            current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.@select(current.RChildID, current.LChildID, sA < 0f);
                            // ----------------------------------------------------------------------------------------------------

                            current = NativeNodes[current.NearNodeID];
                            state = TraverseState.FromParent;
                        }

                        break;

                    case TraverseState.FromParent:
                        if (!BVHBBox.IntersectRay(Rays[index], current.BMin, current.BMax, out dist))
                        {
                            cID = current.NodeID;
                            current = NativeNodes[current.ParentID];

                            // ----------------------------------------------------------------------------------------------------                        
                            sA = rayDirection[current.SplitAxis];
                            current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.@select(current.RChildID, current.LChildID, sA < 0f);
                            // ----------------------------------------------------------------------------------------------------

                            if (cID == current.NearNodeID)
                            {
                                current = NativeNodes[current.FarNodeID];
                                state = TraverseState.FromSibling;
                            }
                            else
                            {
                                current = NativeNodes[current.NearNodeID];
                                state = TraverseState.FromSibling;
                            }
                        }
                        else if (current.IsLeaf == 1)
                        {
                            // Test triangle for intersection

                            if (NativePrims[current.PrimitivesOffset].IntersectRay(Rays[index], ref TempHi, index))
                            {
                                HitInfos.Add(TempHi[index]);
                                intersect = true;
                            }

                            // ----------------------------------------------------------------------------------------------------

                            int lChild, rChild, splitAxis;
                            NativeNodes[current.ParentID]
                                .GetChildrenIDsAndSplitAxis(out lChild, out rChild, out splitAxis);
                            sA = rayDirection[splitAxis];

                            //int nearNodeID = math.@select(lChild, rChild, sA < 0f);
                            int farNodeID = math.@select(rChild, lChild, sA < 0f);

                            // ----------------------------------------------------------------------------------------------------

                            current = NativeNodes[farNodeID];
                            state = TraverseState.FromSibling;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------------------------------                            
                            sA = rayDirection[current.SplitAxis];
                            current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.@select(current.RChildID, current.LChildID, sA < 0f);
                            // ----------------------------------------------------------------------------------------------------

                            current = NativeNodes[current.NearNodeID];
                            state = TraverseState.FromParent;
                        }

                        break;
                }
            }

            return intersect;
        }

        public bool RayCast(int index, int nodeID)
        {
            if (NativeNodes[nodeID].IsLeaf == 1)
            {
                float bestDist = float.MaxValue;

                HitInfo hitInfo = new HitInfo();
                for (int i = 0; i < NativeNodes[nodeID].PrimitivesCount; i++)
                {
                    hitInfo.Clear();
                    if (NativePrims[NativeNodes[nodeID].PrimitivesOffset + i].IntersectRay(Rays[index], out hitInfo))
                    {
                        if (hitInfo.HitDistance < bestDist)
                        {
                            bestDist = hitInfo.HitDistance;
                            HitInfos.Add(new HitInfo(hitInfo));
                        }
                    }
                }
            }
            else
            {
                // ReSharper disable once NotAccessedVariable
                float hitDist;

                if (BVHBBox.IntersectRay(Rays[index], NativeNodes[nodeID].BMin, NativeNodes[nodeID].BMax, out hitDist))
                {
                    float d1;
                    float d2;

                    int lNodeID = NativeNodes[nodeID].LChildID;
                    int rNodeID = NativeNodes[nodeID].RChildID;

                    bool bHitL = BVHBBox.IntersectRay(Rays[index], NativeNodes[lNodeID].BMin, NativeNodes[lNodeID].BMax,
                        out d1);
                    bool bHitR = BVHBBox.IntersectRay(Rays[index], NativeNodes[rNodeID].BMin, NativeNodes[rNodeID].BMax,
                        out d2);

                    if (bHitL && bHitR)
                    {
                        if (d1 > d2)
                        {
                            RayCast(index, lNodeID);
                            RayCast(index, rNodeID);
                        }
                        else
                        {
                            RayCast(index, rNodeID);
                            RayCast(index, lNodeID);
                        }
                    }
                    else if (bHitL)
                    {
                        RayCast(index, lNodeID);
                    }
                    else if (bHitR)
                    {
                        RayCast(index, rNodeID);
                    }
                }
            }

            return false;
        }

    }
}