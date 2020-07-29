using System.Collections.Generic;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationStudio;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        private void DisposeVegetationCells()
        {
            _prepareVegetationHandle.Complete();

            for (int i = 0; i <= VegetationCellList.Count - 1; i++)
            {
                VegetationCellList[i].Dispose();
            }

            VegetationCellList.Clear();
        }

        private void CreateVegetationCells()
        {
            DisposeVegetationCells();
            Bounds expandedBounds = new Bounds(VegetationSystemBounds.center, VegetationSystemBounds.size);
            expandedBounds.Expand(new Vector3(VegetationCellSize * 2f, 0, VegetationCellSize * 2f));

            Rect expandedRect = RectExtension.CreateRectFromBounds(expandedBounds);

            VegetationCellQuadTree = new QuadTree<VegetationCell>(expandedRect);
            int cellXCount = Mathf.CeilToInt(VegetationSystemBounds.size.x / VegetationCellSize);
            int cellZCount = Mathf.CeilToInt(VegetationSystemBounds.size.z / VegetationCellSize);

            Vector2 corner = new Vector2(VegetationSystemBounds.center.x - VegetationSystemBounds.size.x / 2f,
                VegetationSystemBounds.center.z - VegetationSystemBounds.size.z / 2f);

            for (int x = 0; x <= cellXCount - 1; x++)
            {
                for (int z = 0; z <= cellZCount - 1; z++)
                {
                    VegetationCell vegetationCell = new VegetationCell(new Rect(
                        new Vector2(VegetationCellSize * x + corner.x, VegetationCellSize * z + corner.y),
                        new Vector2(VegetationCellSize, VegetationCellSize)));
                    VegetationCellList.Add(vegetationCell);
                    vegetationCell.Index = VegetationCellList.Count - 1;
                    VegetationCellQuadTree.Insert(vegetationCell);
                }
            }

            LoadedVegetationCellList.Clear();
            LoadedVegetationCellList.Capacity = VegetationCellList.Count;

            NativeArray<Bounds> vegetationCellBounds =
                new NativeArray<Bounds>(VegetationCellList.Count, Allocator.Persistent);
            for (int i = 0; i <= VegetationCellList.Count - 1; i++)
            {
                vegetationCellBounds[i] = VegetationCellList[i].VegetationCellBounds;
            }

            float minBoundsHeight = VegetationSystemBounds.center.y - VegetationSystemBounds.extents.y;
            float worldspaceSealevel = minBoundsHeight + SeaLevel;
            if (!ExcludeSeaLevelCells) worldspaceSealevel = minBoundsHeight;

            JobHandle jobHandle = default(JobHandle);
            for (int i = 0; i <= VegetationStudioTerrainList.Count - 1; i++)
            {
                jobHandle = VegetationStudioTerrainList[i]
                    .SampleCellHeight(vegetationCellBounds, worldspaceSealevel, expandedRect, jobHandle);
            }

            jobHandle.Complete();

            for (int i = 0; i <= VegetationCellList.Count - 1; i++)
            {
                VegetationCellList[i].VegetationCellBounds = vegetationCellBounds[i];
            }

            vegetationCellBounds.Dispose();

            PrepareVegetationCells();
            
            VegetationStudioManager.OnVegetationCellRefresh(this);
        }

        public void RefreshTerrainArea()
        {
            RefreshTerrainArea(VegetationSystemBounds);
        }

        public void RefreshTerrainArea(Bounds bounds)
        {
            if (VegetationCellQuadTree == null) return;

            List<VegetationCell> overlapVegetationCellList = new List<VegetationCell>();
            Rect updateRect = RectExtension.CreateRectFromBounds(bounds);
            VegetationCellQuadTree.Query(updateRect, overlapVegetationCellList);

            Bounds updateBounds = bounds;

            NativeArray<Bounds> vegetationCellBounds =
                new NativeArray<Bounds>(overlapVegetationCellList.Count, Allocator.Persistent);
            for (int i = 0; i <= overlapVegetationCellList.Count - 1; i++)
            {
                Bounds cellBounds = RectExtension.CreateBoundsFromRect(overlapVegetationCellList[i].Rectangle, -100000);
                vegetationCellBounds[i] = cellBounds;
                updateBounds.Encapsulate(cellBounds);
            }

            updateRect = RectExtension.CreateRectFromBounds(updateBounds);

            float minBoundsHeight = VegetationSystemBounds.center.y - VegetationSystemBounds.extents.y;
            float worldspaceSealevel = minBoundsHeight + SeaLevel;
            if (!ExcludeSeaLevelCells) worldspaceSealevel = minBoundsHeight;

            JobHandle jobHandle = default(JobHandle);
            for (int i = 0; i <= VegetationStudioTerrainList.Count - 1; i++)
            {
                jobHandle = VegetationStudioTerrainList[i]
                    .SampleCellHeight(vegetationCellBounds, worldspaceSealevel, updateRect, jobHandle);
            }

            jobHandle.Complete();

            for (int i = 0; i <= overlapVegetationCellList.Count - 1; i++)
            {
                overlapVegetationCellList[i].VegetationCellBounds = vegetationCellBounds[i];
            }

            vegetationCellBounds.Dispose();

            ForceCullingRefresh();
            ClearCache(bounds);
        }

        private void ForceCullingRefresh()
        {
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                VegetationStudioCameraList[i].PreCullVegetation(true);
            }
        }

        private void PrepareVegetationCells()
        {
            for (int i = 0; i <= VegetationCellList.Count - 1; i++)
            {
                VegetationCellSpawner.PrepareVegetationCell(VegetationCellList[i]);
            }
        }

        private void SetupVegetationCellSpawner()
        {
            VegetationCellSpawner.VegetationStudioTerrainList = VegetationStudioTerrainList;
            VegetationCellSpawner.VegetationPackageProList = VegetationPackageProList;
            VegetationCellSpawner.VegetationPackageProModelsList = VegetationPackageProModelsList;
            VegetationCellSpawner.VegetationSettings = VegetationSettings;
            VegetationCellSpawner.VegetationSystemPro = this;
            VegetationCellSpawner.PersistentVegetationStorage = PersistentVegetationStorage;
            VegetationCellSpawner.CompactMemoryCellList = CompactMemoryCellList;
        }

        void ReturnVegetationCellTemporaryMemory()
        {            
            for (int i = 0; i <= CompactMemoryCellList.Count - 1; i++)
            {
                VegetationCell vegetationCell = CompactMemoryCellList[i];
                for (int j = 0; j <= vegetationCell.VegetationInstanceDataList.Count - 1; j++)
                {
                    VegetationInstanceData vegetationInstanceData = vegetationCell.VegetationInstanceDataList[j];
                    VegetationCellSpawner.VegetationInstanceDataPool.ReturnObject(vegetationInstanceData);
                }
                vegetationCell.VegetationInstanceDataList.Clear();                
            }
            CompactMemoryCellList.Clear();
        }

        void ReturnVegetationCellTemporaryMemory(VegetationCell vegetationCell)
        {
            for (int i = 0; i <= vegetationCell.VegetationInstanceDataList.Count - 1; i++)
            {
                VegetationInstanceData vegetationInstanceData = vegetationCell.VegetationInstanceDataList[i];
                VegetationCellSpawner.VegetationInstanceDataPool.ReturnObject(vegetationInstanceData);
            }
            vegetationCell.VegetationInstanceDataList.Clear();            
        }

        public void SpawnVegetationCell(VegetationCell vegetationCell)
        {
            CompleteCellLoading();
            
            if (!vegetationCell.Prepared)
            {
                VegetationCellSpawner.PrepareVegetationCell(vegetationCell);
            }

            bool hasInstancedIndirect;
            
            JobHandle cellSpawnHandle =
                VegetationCellSpawner.SpawnVegetationCell(vegetationCell, out hasInstancedIndirect);
            cellSpawnHandle.Complete();

            if (!hasInstancedIndirect || !Application.isPlaying) return;
            
            ProcessInstancedIndirectCellList.Add(vegetationCell);
            JobHandle prepareInstancedIndirectHandle = PrepareInstancedIndirectSetupJobs();
            prepareInstancedIndirectHandle.Complete();
            SetupInstancedIndirectComputeBuffers();
            ReturnVegetationCellTemporaryMemory(vegetationCell);
        }
        
        public void SpawnVegetationCell(VegetationCell vegetationCell, string vegetationItemID)
        {
            CompleteCellLoading();
            
            if (!vegetationCell.Prepared)
            {
                VegetationCellSpawner.PrepareVegetationCell(vegetationCell);
            }

            bool hasInstancedIndirect;
            
            JobHandle cellSpawnHandle =
                VegetationCellSpawner.SpawnVegetationCell(vegetationCell,vegetationItemID, out hasInstancedIndirect);
            cellSpawnHandle.Complete();

            if (!hasInstancedIndirect || !Application.isPlaying) return;
            
            ProcessInstancedIndirectCellList.Add(vegetationCell);
            JobHandle prepareInstancedIndirectHandle = PrepareInstancedIndirectSetupJobs();
            prepareInstancedIndirectHandle.Complete();
            SetupInstancedIndirectComputeBuffers();
            ReturnVegetationCellTemporaryMemory(vegetationCell);
        }

        public NativeList<MatrixInstance> GetVegetationItemInstances(VegetationCell vegetationCell,
            string vegetationItemID)
        {
            CompleteCellLoading();
            
            VegetationItemIndexes vegetationItemIndexes = GetVegetationItemIndexes(vegetationItemID);

            if (vegetationCell.Prepared)
            {
                return vegetationCell.VegetationPackageInstancesList[vegetationItemIndexes.VegetationPackageIndex]
                    .VegetationItemMatrixList[vegetationItemIndexes.VegetationItemIndex];
            }
            return new NativeList<MatrixInstance>();
        }
        
        public VegetationItemIndexes GetVegetationItemIndexes(string vegetationItemID)
        {
            VegetationItemIndexes indexes = new VegetationItemIndexes
            {
                VegetationItemIndex = -1, VegetationPackageIndex = -1
            };

            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                for (int j = 0; j <= VegetationPackageProList[i].VegetationInfoList.Count - 1; j++)
                {
                    if (VegetationPackageProList[i].VegetationInfoList[j].VegetationItemID == vegetationItemID)
                    {
                        indexes.VegetationPackageIndex = i;
                        indexes.VegetationItemIndex = j;
                        return indexes;
                    }
                }
            }
            return indexes;
        }             
        
         public void ClearCache()
        {
            CompleteCellLoading();

            for (int i = 0; i <= LoadedVegetationCellList.Count - 1; i++)
            {
                LoadedVegetationCellList[i].ClearCache();
            }

            LoadedVegetationCellList.Clear();
            ClearBillboardCellsCache();
            OnClearCacheDelegate?.Invoke(this);
        }

        public void ClearCache(Bounds bounds)
        {
            Rect clearRect = RectExtension.CreateRectFromBounds(bounds);
            CompleteCellLoading();

            //TODO Use quadtree here
            for (int i = LoadedVegetationCellList.Count - 1; i >= 0; i--)
            {
                VegetationCell vegetationCell = LoadedVegetationCellList[i];
                if (!vegetationCell.Rectangle.Overlaps(clearRect)) continue;

                vegetationCell.ClearCache();
                LoadedVegetationCellList.RemoveAtSwapBack(i);

                OnClearCacheVegetationCellDelegate?.Invoke(this, vegetationCell);
            }

            ClearBillboardCellsCache(bounds);
        }

        public void ClearCache(VegetationCell vegetationCell)
        {
            CompleteCellLoading();
            vegetationCell.ClearCache();
            ClearBillboardCellsCache(vegetationCell.VegetationCellBounds);
            OnClearCacheVegetationCellDelegate?.Invoke(this, vegetationCell);
        }
        
        public void ClearCache(VegetationCell vegetationCell, string vegetationItemID)
        {
            VegetationItemIndexes indexes = GetVegetationItemIndexes(vegetationItemID);
            if (indexes.VegetationPackageIndex >= 0)
            {
                ClearCache(vegetationCell,indexes.VegetationPackageIndex, indexes.VegetationItemIndex);
            }
        }

        public void ClearCache(VegetationCell vegetationCell,int vegetationPackageIndex, int vegetationItemIndex)
        {
            CompleteCellLoading();            
            VegetationItemInfoPro vegetationItemInfo =
                VegetationPackageProList[vegetationPackageIndex].VegetationInfoList[vegetationItemIndex];
            bool tree = vegetationItemInfo.VegetationType == VegetationType.Tree ||
                        vegetationItemInfo.VegetationType == VegetationType.LargeObjects;
            
            vegetationCell.ClearCache(vegetationPackageIndex,vegetationItemIndex,tree);
            ClearBillboardCellsCache(vegetationCell.VegetationCellBounds,vegetationPackageIndex,vegetationItemIndex);           
            OnClearCacheVegetationCellVegetatonItemDelegate?.Invoke(this, vegetationCell,vegetationPackageIndex,vegetationItemIndex);
        }
        
        public void ClearCache(string vegetationItemID)
        { 
            CompleteCellLoading();                        
            VegetationItemIndexes indexes = GetVegetationItemIndexes(vegetationItemID);
            if (indexes.VegetationPackageIndex >= 0)
            {
                ClearCache(indexes.VegetationPackageIndex, indexes.VegetationItemIndex);
            }
        }

        public void ClearCache(int vegetationPackageIndex, int vegetationItemIndex)
        {
            CompleteCellLoading();            
            VegetationItemInfoPro vegetationItemInfo =
                VegetationPackageProList[vegetationPackageIndex].VegetationInfoList[vegetationItemIndex];
            bool tree = vegetationItemInfo.VegetationType == VegetationType.Tree ||
                        vegetationItemInfo.VegetationType == VegetationType.LargeObjects;

            for (int i = 0; i <= LoadedVegetationCellList.Count - 1; i++)
            {
                LoadedVegetationCellList[i].ClearCache(vegetationPackageIndex, vegetationItemIndex, tree);
            }

            ClearBillboardCellsCache(vegetationPackageIndex, vegetationItemIndex);
            OnClearCacheVegetationItemDelegate?.Invoke(this, vegetationPackageIndex, vegetationItemIndex);
        }
    }
}