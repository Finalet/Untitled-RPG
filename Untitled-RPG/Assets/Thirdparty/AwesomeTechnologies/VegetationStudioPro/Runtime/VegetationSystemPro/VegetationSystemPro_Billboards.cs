using System;
using System.Collections.Generic;
using AwesomeTechnologies.Billboards;
using AwesomeTechnologies.BillboardSystem;
using AwesomeTechnologies.Utility.Quadtree;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        [NonSerialized]
        private readonly List<VegetationCell> _billboardTempVegetationCellList = new List<VegetationCell>();

        [NonSerialized] private readonly List<BillboardCell> _loadBillboardCellList = new List<BillboardCell>();

        private void DisposeBillboardCells()
        {
            _prepareVegetationHandle.Complete();

            for (var i = 0; i <= BillboardCellList.Count - 1; i++) BillboardCellList[i].Dispose();

            BillboardCellList.Clear();
        }

        public void UpdateBillboardCulling()
        {
            for (var i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
                VegetationStudioCameraList[i].UpdateBillboardCullingGroup();
        }

        public void RefreshBillboards()
        {
        }

        private void LoadBillboardCells()
        {
            _loadBillboardCellList.Clear();
            Profiler.BeginSample("Load billboard cells");
            _billboardTempVegetationCellList.Clear();
            VegetationCellSpawner.CellJobHandleList.Clear();

            Profiler.BeginSample("Find billboard cells with quadtree");
            for (var i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;

                for (var j = 0;
                    j <= VegetationStudioCameraList[i].BillboardJobCullingGroup.VisibleCellIndexList.Length - 1;
                    j++)
                {
                    var index = VegetationStudioCameraList[i].BillboardJobCullingGroup.VisibleCellIndexList[j];
                    var billboardCell = BillboardCellList[index];
                    if (billboardCell.Loaded) continue;

                    billboardCell.OverlapVegetationCells.Clear();
                    VegetationCellQuadTree.Query(billboardCell.Rectangle, billboardCell.OverlapVegetationCells);

                    _billboardTempVegetationCellList.AddRange(billboardCell.OverlapVegetationCells);

                    if (!_loadBillboardCellList.Contains(billboardCell)) _loadBillboardCellList.Add(billboardCell);
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("Load needed cells");
            for (var i = 0; i <= _billboardTempVegetationCellList.Count - 1; i++)
            {
                var vegetationCell = _billboardTempVegetationCellList[i];
                if (vegetationCell.LoadedDistanceBand <= 1) continue;
                if (vegetationCell.LoadedBillboards) continue;

                if (!Application.isPlaying && !vegetationCell.Prepared)
                    VegetationCellSpawner.PrepareVegetationCell(vegetationCell);

                // ReSharper disable once InlineOutVariableDeclaration
                bool hasInstancedIndirect;

                var spawnVegetationCellHandle = VegetationCellSpawner.SpawnVegetationCell(vegetationCell, 1, out hasInstancedIndirect, true);
                CompactMemoryCellList.Add(vegetationCell);
                LoadedVegetationCellList.Add(vegetationCell);
#if UNITY_EDITOR
                if (JobsUtility.JobDebuggerEnabled)
                {
                    spawnVegetationCellHandle.Complete();
                    ReturnVegetationCellTemporaryMemory();
                }
#endif
                VegetationCellSpawner.CellJobHandleList.Add(spawnVegetationCellHandle);                             
                if (hasInstancedIndirect) ProcessInstancedIndirectCellList.Add(vegetationCell);
            }                       

            var loadAllCellsHandle = JobHandle.CombineDependencies(VegetationCellSpawner.CellJobHandleList);
            VegetationCellSpawner.CellJobHandleList.Clear();

            Profiler.EndSample();

            Profiler.BeginSample("Merge cells and create mesh data");

            for (var i = 0; i <= _loadBillboardCellList.Count - 1; i++)
            {
                var billboardCell = _loadBillboardCellList[i];
                if (billboardCell.Loaded) continue;

                for (var j = 0; j <= billboardCell.VegetationPackageBillboardInstancesList.Count - 1; j++)
                for (var k = 0;
                    k <= billboardCell.VegetationPackageBillboardInstancesList[j].BillboardInstanceList.Count - 1;
                    k++)
                {
                    var vegetationItemInfoPro = VegetationPackageProList[j].VegetationInfoList[k];
                    if (vegetationItemInfoPro.VegetationType != VegetationType.Tree) continue;

                    var billboardInstance = billboardCell.VegetationPackageBillboardInstancesList[j]
                        .BillboardInstanceList[k];
                    if (billboardInstance.Loaded) continue;

                    var billboardPrepareMeshDataJobHandle = loadAllCellsHandle;
                    for (var l = 0; l <= billboardCell.OverlapVegetationCells.Count - 1; l++)
                    {
                        var vegetationCell = billboardCell.OverlapVegetationCells[l];
                        var cellInstanceList =
                            vegetationCell.VegetationPackageInstancesList[j].VegetationItemMatrixList[k];

                        var mergeCellInstancesJob = new MergeCellInstancesJob
                        {
                            OutputNativeList = billboardInstance.InstanceList, InputNativeList = cellInstanceList
                        };

                        billboardPrepareMeshDataJobHandle =
                            mergeCellInstancesJob.Schedule(billboardPrepareMeshDataJobHandle);
                    }

                    var vegetationItemSize = Mathf.Max(vegetationItemInfoPro.Bounds.extents.x,
                                                 vegetationItemInfoPro.Bounds.extents.y,
                                                 vegetationItemInfoPro.Bounds.extents.z) * 2f;

                    var createBillboardMeshJob = new BillboardGenerator.CreateBillboardMeshJob
                    {
                        InstanceList = billboardInstance.InstanceList,
                        VerticeList = billboardInstance.VerticeList,
                        NormalList = billboardInstance.NormalList,
                        UvList = billboardInstance.UvList,
                        Uv2List = billboardInstance.Uv2List,
                        Uv3List = billboardInstance.Uv3List,
                        IndexList = billboardInstance.IndexList,
                        BoundsYExtent = vegetationItemInfoPro.Bounds.extents.y,
                        VegetationItemSize = vegetationItemSize
                    };

                    billboardPrepareMeshDataJobHandle =
                        createBillboardMeshJob.Schedule(billboardPrepareMeshDataJobHandle);
                    VegetationCellSpawner.CellJobHandleList.Add(billboardPrepareMeshDataJobHandle);
                }
            }

            var combinedMergeJobHandle = JobHandle.CombineDependencies(VegetationCellSpawner.CellJobHandleList);
            VegetationCellSpawner.CellJobHandleList.Clear();
            loadAllCellsHandle.Complete();
            combinedMergeJobHandle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Create Mesh Objects");
            for (var i = 0; i <= _loadBillboardCellList.Count - 1; i++)
            {
                var billboardCell = _loadBillboardCellList[i];
                if (billboardCell.Loaded) continue;
                for (var j = 0; j <= billboardCell.VegetationPackageBillboardInstancesList.Count - 1; j++)
                for (var k = 0;
                    k <= billboardCell.VegetationPackageBillboardInstancesList[j].BillboardInstanceList.Count - 1;
                    k++)
                {
                    var billboardInstance = billboardCell.VegetationPackageBillboardInstancesList[j]
                        .BillboardInstanceList[k];
                    if (billboardInstance.Loaded) continue;
                    billboardInstance.InstanceCount = billboardInstance.InstanceList.Length;
                    if (billboardInstance.InstanceCount > 0)
                        billboardInstance.Mesh = BillboardGenerator.CreateMeshFromBillboardInstance(billboardInstance);
                    billboardInstance.Loaded = true;
                }

                billboardCell.Loaded = true;
            }

            _loadBillboardCellList.Clear();
            Profiler.EndSample();

            Profiler.EndSample();
        }

        private void ClearBillboardCellsCache()
        {
            for (var i = 0; i <= BillboardCellList.Count - 1; i++)
            {
                var billboardCell = BillboardCellList[i];
                billboardCell.ClearCache();
            }
        }

        private void ClearBillboardCellsCache(int vegetationPackageIndex, int vegetationItemIndex)
        {
            for (var i = 0; i <= BillboardCellList.Count - 1; i++)
            {
                var billboardCell = BillboardCellList[i];
                billboardCell.ClearCache(vegetationPackageIndex, vegetationItemIndex);
            }
        }

        private void ClearBillboardCellsCache(Bounds bounds)
        {           
            _prepareVegetationHandle.Complete();

            if (BillboardCellQuadTree == null) return;         
            var clearRect = RectExtension.CreateRectFromBounds(bounds);
            var overlapBillboardCellList = new List<BillboardCell>();
            BillboardCellQuadTree.Query(clearRect, overlapBillboardCellList);
            for (var i = 0; i <= overlapBillboardCellList.Count - 1; i++)
            {
                var billboardCell = overlapBillboardCellList[i];
                billboardCell.ClearCache();
            }
        }
        
        private void ClearBillboardCellsCache(Bounds bounds,int vegetationPackageIndex, int vegetationItemIndex)
        {
            var clearRect = RectExtension.CreateRectFromBounds(bounds);
            _prepareVegetationHandle.Complete();

            var overlapBillboardCellList = new List<BillboardCell>();
            BillboardCellQuadTree.Query(clearRect, overlapBillboardCellList);
            for (var i = 0; i <= overlapBillboardCellList.Count - 1; i++)
            {
                var billboardCell = overlapBillboardCellList[i];
                billboardCell.ClearCache(vegetationPackageIndex, vegetationItemIndex);
            }
        }

        private void SetupBillboardShaderIDs()
        {
            _cameraPositionID = Shader.PropertyToID("_CameraPosition");
            _cullDistanceID = Shader.PropertyToID("_CullDistance");
            _farCullDistanceID = Shader.PropertyToID("_FarCullDistance");
        }

        public void RenderBillboardCells()
        {
            Profiler.BeginSample("Draw billboards");
            var farCullDistance = Mathf.RoundToInt(VegetationSettings.GetBillboardDistance());
            var isPlaying = Application.isPlaying;

            var shadowCastingMode = VegetationSettings.GetBillboardShadowCastingMode();
            var layer = VegetationSettings.GetBillboardLayer();

            var positionMatrix = Matrix4x4.TRS(FloatingOriginOffset, Quaternion.identity, Vector3.one);

            for (var i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;

                var targetCamera = VegetationStudioCameraList[i].RenderDirectToCamera
                    ? VegetationStudioCameraList[i].SelectedCamera
                    : null;

                if (!isPlaying) targetCamera = null;

                for (var j = 0;
                    j <= VegetationStudioCameraList[i].BillboardJobCullingGroup.VisibleCellIndexList.Length - 1;
                    j++)
                {
                    var index = VegetationStudioCameraList[i].BillboardJobCullingGroup.VisibleCellIndexList[j];
                    var billboardCell = BillboardCellList[index];
                    for (var k = 0; k <= billboardCell.VegetationPackageBillboardInstancesList.Count - 1; k++)
                    for (var l = 0;
                        l <= billboardCell.VegetationPackageBillboardInstancesList[k].BillboardInstanceList.Count - 1;
                        l++)
                    {
                        var billboardInstance = billboardCell.VegetationPackageBillboardInstancesList[k]
                            .BillboardInstanceList[l];
                        if (billboardInstance.Loaded && billboardInstance.InstanceCount > 0)
                        {
                            if (VegetationStudioCameraList[i].SelectedCamera == null) continue;
                            var vegetationItemModelInfo = VegetationPackageProModelsList[k].VegetationItemModelList[l];

                            var vegetationItemInfoPro = VegetationPackageProList[k].VegetationInfoList[l];
                            if (!vegetationItemInfoPro.UseBillboards) continue;
                          
                            var camPos = VegetationStudioCameraList[i].SelectedCamera.transform.position;

                            var renderDistanceFactor = vegetationItemInfoPro.RenderDistanceFactor;
                            if (VegetationSettings.DisableRenderDistanceFactor) renderDistanceFactor = 1;

                            var cullDistance =
                                Mathf.RoundToInt(VegetationSettings.GetTreeDistance() * renderDistanceFactor);
                            if (vegetationItemModelInfo.BillboardLODFadeCrossfade) cullDistance -= 10;

                            //vegetationItemModelInfo.BillboardMaterial.SetVector(_cameraPositionID, camPos);

                            MaterialPropertyBlock materialPropertyBlock =
                                vegetationItemModelInfo.CameraBillboardMaterialPropertyBlockList[i];
                            
                            materialPropertyBlock.SetVector(_cameraPositionID, camPos);

                            materialPropertyBlock.SetInt(_cullDistanceID,
                                VegetationStudioCameraList[i].RenderBillboardsOnly ? 0 : cullDistance);

                            materialPropertyBlock.SetInt(_farCullDistanceID, farCullDistance);

                            Graphics.DrawMesh(billboardInstance.Mesh, positionMatrix,
                                vegetationItemModelInfo.BillboardMaterial, layer, targetCamera, 0, materialPropertyBlock,
                                shadowCastingMode, true);
                        }
                    }
                }
            }

            Profiler.EndSample();
        }

        private void PrepareAllBillboardCells()
        {
            for (var i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            for (var j = 0; j <= BillboardCellList.Count - 1; j++)
            {
                var billboardCell = BillboardCellList[j];
                if (!billboardCell.Prepared) billboardCell.PrepareBillboardCell(VegetationPackageProList);
            }
        }

        private void CreateBillboardCells()
        {
            DisposeBillboardCells();

            var expandedBounds = new Bounds(VegetationSystemBounds.center, VegetationSystemBounds.size);

            var currentBillboardCellSize = BillboardCellSize;
            if (!Application.isPlaying) currentBillboardCellSize = 400;
            expandedBounds.Expand(new Vector3(currentBillboardCellSize * 2f, 0, currentBillboardCellSize * 2f));

            BillboardCellQuadTree = new QuadTree<BillboardCell>(RectExtension.CreateRectFromBounds(expandedBounds));
            var cellXCount = Mathf.CeilToInt(VegetationSystemBounds.size.x / currentBillboardCellSize);
            var cellZCount = Mathf.CeilToInt(VegetationSystemBounds.size.z / currentBillboardCellSize);

            var corner = new Vector2(VegetationSystemBounds.center.x - VegetationSystemBounds.size.x / 2f,
                VegetationSystemBounds.center.z - VegetationSystemBounds.size.z / 2f);

            for (var x = 0; x <= cellXCount - 1; x++)
            for (var z = 0; z <= cellZCount - 1; z++)
            {
                var billboardCell = new BillboardCell(
                    new Rect(
                        new Vector2(currentBillboardCellSize * x + corner.x, currentBillboardCellSize * z + corner.y),
                        new Vector2(currentBillboardCellSize, currentBillboardCellSize)),
                    VegetationSystemBounds.center.y, VegetationSystemBounds.size.y);
                BillboardCellList.Add(billboardCell);
                billboardCell.Index = BillboardCellList.Count - 1;
                BillboardCellQuadTree.Insert(billboardCell);
            }

            PrepareAllBillboardCells();
        }
    }
}

