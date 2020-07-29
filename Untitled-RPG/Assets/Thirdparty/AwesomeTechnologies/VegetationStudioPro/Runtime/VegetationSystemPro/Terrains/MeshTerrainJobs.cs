using AwesomeTechnologies.Utility.BVHTree;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.MeshTerrains
{
    [BurstCompile(CompileSynchronously = true)]
    public struct CreateBVHRaycastJob : IJob
    {
        [ReadOnly]
        public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;
        public NativeArray<BVHRay> Rays;
        public Rect TerrainRect;

        public int LayerMask;
        public int MaxHits;
        public void Execute()
        {
            for (int i = 0; i <= SpawnLocationList.Length - 1; i++)
            {
                float3 position = SpawnLocationList[i].Position;
                Vector2 point2D = new Vector2(position.x, position.z);
                if (!TerrainRect.Contains(point2D))
                {
                    BVHRay ray = new BVHRay
                    {
                        Origin = position + new float3(0, 10000, 0),
                        Direction = new float3(0, -1, 0),
                        DoRaycast = 0
                    };
                    Rays[i] = ray;
                }
                else
                {
                    BVHRay ray = new BVHRay
                    {
                        Origin = position + new float3(0, 10000, 0),
                        Direction = new float3(0, -1, 0),
                        DoRaycast = math.@select(1, 0, SpawnLocationList[i].SpawnChance < 0)
                    };
                    Rays[i] = ray;
                }
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct UpdateBVHInstanceListJob : IJob
    {
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
        
        [ReadOnly]
        public NativeArray<HitInfo> RaycastHits;       
        [ReadOnly]
        public NativeArray<VegetationSpawnLocationInstance> SpawnLocationList;

        public void Execute()
        {
            for (int i = 0; i <= RaycastHits.Length - 1; i++)
            {
                HitInfo raycastHit = RaycastHits[i];
                if (raycastHit.HitDistance > 0)
                {
                    Position.Add(raycastHit.HitPoint);
                    Rotation.Add(Quaternion.Euler(0, 0, 0));
                    Scale.Add(new float3(1, 1, 1));
                    TerrainNormal.Add(raycastHit.HitNormal);
                    BiomeDistance.Add(100000);
                    TerrainTextureData.Add(0);
                    RandomNumberIndex.Add(SpawnLocationList[i].RandomNumberIndex);
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

    [BurstCompile(CompileSynchronously = true)]
    public struct BVHTerrainCellSampleJob : IJobParallelFor
    {
        public NativeArray<Bounds> VegetationCellBoundsList;
        public Rect TerrainRect;
        public float TerrainMinHeight;
        public float TerrainMaxHeight;

       
        public void Execute(int index)
        {
            Bounds vegetationCellBounds = VegetationCellBoundsList[index];
            Rect cellRect = RectExtension.CreateRectFromBounds(vegetationCellBounds);
            if (!TerrainRect.Overlaps(cellRect)) return;

            float minHeight;
            float maxHeight = vegetationCellBounds.center.y + vegetationCellBounds.extents.y;

            if (vegetationCellBounds.center.y < 99999)
            {
                minHeight = TerrainMinHeight;
            }
            else
            {
                minHeight = vegetationCellBounds.center.y - vegetationCellBounds.extents.y;
            }

            if (TerrainMinHeight < minHeight) minHeight = TerrainMinHeight;
            if (TerrainMaxHeight > maxHeight) maxHeight = TerrainMaxHeight;

            float centerY = (maxHeight + minHeight) / 2f;
            float height = maxHeight - minHeight;
            vegetationCellBounds = new Bounds(new Vector3(vegetationCellBounds.center.x, centerY, vegetationCellBounds.center.z), new Vector3(vegetationCellBounds.size.x, height, vegetationCellBounds.size.z));
            VegetationCellBoundsList[index] = vegetationCellBounds;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct BVHTerrainCellSampleJob2 : IJobParallelFor
    {
        public NativeArray<Bounds> VegetationCellBoundsList;
        [ReadOnly]
        public NativeArray<LBVHNODE> Nodes;
        public Rect TerrainRect; 

        public void Execute(int index)
        {
            Bounds vegetationCellBounds = VegetationCellBoundsList[index];
            Rect cellRect = RectExtension.CreateRectFromBounds(vegetationCellBounds);
            if (!TerrainRect.Overlaps(cellRect)) return;
            
            Vector3 cellMin = vegetationCellBounds.center - vegetationCellBounds.extents;
            Vector3 cellMax = vegetationCellBounds.center + vegetationCellBounds.extents;

            Vector3 orgCellMin = cellMin;
            Vector3 orgCellMax = cellMax;

            cellMin.y = float.MaxValue;
            cellMax.y = float.MinValue;

            Vector3 cellMinExtended = new Vector3(cellMin.x, float.MinValue, cellMin.z);
            Vector3 cellMaxExtended = new Vector3(cellMax.x, float.MaxValue, cellMax.z);

            if (CalculateCellSize(0, ref cellMinExtended, ref cellMaxExtended, ref cellMin, ref cellMax))
            {
                if (vegetationCellBounds.center.y > -99999)
                {
                    cellMax = math.max(cellMax, orgCellMax);
                    cellMin = math.min(cellMin, orgCellMin);
                }

                float centerY = (cellMin.y + cellMax.y) / 2f;
                float height = cellMax.y - cellMin.y;
                vegetationCellBounds = new Bounds(new Vector3(vegetationCellBounds.center.x, centerY, vegetationCellBounds.center.z), new Vector3(vegetationCellBounds.size.x, height, vegetationCellBounds.size.z));
                if (float.IsNegativeInfinity(vegetationCellBounds.size.y))
                {
                    vegetationCellBounds.center = new Vector3(vegetationCellBounds.center.x,-100000, vegetationCellBounds.center.z);
                }
                VegetationCellBoundsList[index] = vegetationCellBounds;
            }
        }

        public bool CalculateCellSize(int nodeID, ref Vector3 cellMinExtended,
            ref Vector3 cellMaxExtended, ref Vector3 cellMin, ref Vector3 cellMax)
        {
            if (Nodes[nodeID].IsLeaf == 1)
            {
                Vector3 nodeMin = Nodes[nodeID].BMin;
                Vector3 nodeMax = Nodes[nodeID].BMax;

                if (OverlapBbox(cellMinExtended, cellMaxExtended, nodeMin, nodeMax))
                {
                    if (nodeMin.y < cellMin.y) cellMin.y = nodeMin.y;
                    if (nodeMax.y > cellMax.y) cellMax.y = nodeMax.y;
                }
            }
            else
            {
                CalculateCellSize(Nodes[nodeID].LChildID, ref cellMinExtended, ref cellMaxExtended, ref cellMin,
                    ref cellMax);
                CalculateCellSize(Nodes[nodeID].RChildID, ref cellMinExtended, ref cellMaxExtended, ref cellMin,
                    ref cellMax);
            }
            return true;
        }

        public static bool OverlapBbox(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
        {
            if (aMax.x < bMin.x || aMin.x > bMax.x) return false;
            if (aMax.y < bMin.y || aMin.y > bMax.y) return false;
            if (aMax.z < bMin.z || aMin.z > bMax.z) return false;

            // We have an overlap
            return true;
        }      
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct SampleBVHTreeJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<BVHRay> Rays;
        public NativeArray<HitInfo> HitInfos;
        [ReadOnly]
        public NativeArray<LBVHNODE> NativeNodes;
        [ReadOnly]
        public NativeArray<LBVHTriangle> NativePrims;
        public NativeArray<HitInfo> TempHi;
        //public Rect TerrainRect;

        //[ReadOnly] public NativeArray<byte> CoverageMapArray;

        public void Execute(int index)
        {
            BVHRay ray = Rays[index];
            if (ray.DoRaycast == 0)
            {
                HitInfo hitInfo = new HitInfo
                {
                    HitDistance = -1
                };
                HitInfos[index] = hitInfo;
                return;
            }

            //float3 worldPosition = Rays[index].origin;
            //if (HasMesh(worldPosition))
            //{
            RayCastStackless(index);
            //}
        }

        //bool HasMesh(float3 worldPosition)
        //{
        //    float terrainXPositon = worldPosition.x - TerrainRect.center.x - TerrainRect.width /2f;
        //    float terrainZPositon = worldPosition.z - TerrainRect.center.y - TerrainRect.height / 2f;
        //    return false;
        //}
       
        public enum TraverseSstate { FromParent, FromSibling, FromChild }

        public bool RayCastStackless(int index)
        {

            LBVHNODE current = NativeNodes[0];

            // Without using stack
            float3 rayDirection = Rays[index].Direction;

            // ----------------------------------------------------------------------------------------------------            
            float sA = rayDirection[current.SplitAxis];
            current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
            current.FarNodeID  = math.@select(current.RChildID, current.LChildID, sA < 0f);
            // ----------------------------------------------------------------------------------------------------

            int rootNearID = current.NearNodeID;
            int rootNodeID = current.NodeID;
            current = NativeNodes[rootNearID];

            TraverseSstate state = TraverseSstate.FromParent;

            bool intersect = false;
            float bestDist = float.MaxValue;

            while (current.NodeID != rootNodeID)
            {
                switch (state)
                {
                    case TraverseSstate.FromChild:
                        int cID = current.NodeID;

                        current = NativeNodes[current.ParentID];

                        // ----------------------------------------------------------------------------------------------------                        
                        sA = rayDirection[current.SplitAxis];

                        current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
                        current.FarNodeID  = math.@select(current.RChildID, current.LChildID, sA < 0f);

                        // ----------------------------------------------------------------------------------------------------

                        if (cID == current.NearNodeID)
                        {
                            current = NativeNodes[current.FarNodeID];
                            state = TraverseSstate.FromSibling;
                        }
                        else
                        {
                            state = TraverseSstate.FromChild;
                        }

                        break;

                    case TraverseSstate.FromSibling:
                        if (!current.IntersectRay(Rays[index]))
                        {
                            current = NativeNodes[current.ParentID];
                            state = TraverseSstate.FromChild;
                        }
                        else if (current.IsLeaf == 1)
                        {
                            // No need to iterate as we can only have 1 triangle in a leaf node
                            
                            if (NativePrims[current.PrimitivesOffset].IntersectRay(Rays[index], ref TempHi, index))
                            {
                                if (TempHi[index].HitDistance < bestDist)
                                {
                                    bestDist = TempHi[index].HitDistance;
                                    HitInfos[index] = TempHi[index];
                                    intersect = true;
                                }
                            }
                            current = NativeNodes[current.ParentID];
                            state = TraverseSstate.FromChild;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------------------------------
                            sA = rayDirection[current.SplitAxis];                          

                            current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID  = math.@select(current.RChildID, current.LChildID, sA < 0f);

                            // ----------------------------------------------------------------------------------------------------

                            current = NativeNodes[current.NearNodeID];
                            state = TraverseSstate.FromParent;
                        }

                        break;

                    case TraverseSstate.FromParent:
                        if (!current.IntersectRay(Rays[index]))
                        {
                            cID = current.NodeID;
                            current = NativeNodes[current.ParentID];

                            // ----------------------------------------------------------------------------------------------------                
                            sA = rayDirection[current.SplitAxis];

                            current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID  = math.@select(current.RChildID, current.LChildID, sA < 0f);                   

                            current = cID == current.NearNodeID ? NativeNodes[current.FarNodeID] : NativeNodes[current.NearNodeID];
                            state = TraverseSstate.FromSibling;
                        }
                        else if (current.IsLeaf == 1)
                        {
                            // No need to iterate as we can only have 1 triangle in a leaf node

                            if (NativePrims[current.PrimitivesOffset].IntersectRay(Rays[index], ref TempHi, index))
                            {
                                if (TempHi[index].HitDistance < bestDist)
                                {
                                    bestDist = TempHi[index].HitDistance;
                                    HitInfos[index] = TempHi[index];
                                    intersect = true;                                       
                                }
                            }

                            // ----------------------------------------------------------------------------------------------------

                            // ReSharper disable InlineOutVariableDeclaration
                            int lChild, rChild, splitAxis;
                            NativeNodes[current.ParentID].GetChildrenIDsAndSplitAxis(out lChild, out rChild, out splitAxis);

                            sA = rayDirection[splitAxis];
                            int farNodeID = math.@select(rChild, lChild, sA < 0f);
                            current = NativeNodes[farNodeID];
                            state = TraverseSstate.FromSibling;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------------------------------        
                            
                            sA = rayDirection[current.SplitAxis];

                            current.NearNodeID = math.@select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.@select(current.RChildID, current.LChildID, sA < 0f);
                            current = NativeNodes[current.NearNodeID];
                            state = TraverseSstate.FromParent;
                        }
                        break;
                }
            }
            return intersect;
        }
    }
}

