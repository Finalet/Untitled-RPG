using System;
using System.Collections.Generic;
using AwesomeTechnologies.Utility.Culling;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using AwesomeTechnologies.BillboardSystem;
using AwesomeTechnologies.Utility;

namespace AwesomeTechnologies.VegetationSystem
{
    public class VegetationStudioCameraRenderList
    {
        [NonSerialized]
        public readonly List<NativeList<MatrixInstance>> VegetationItemMergeMatrixList = new List<NativeList<MatrixInstance>>();
        [NonSerialized]
        public readonly List<NativeList<Matrix4x4>> VegetationItemLOD0MatrixList = new List<NativeList<Matrix4x4>>();
        [NonSerialized]
        public readonly List<NativeList<Matrix4x4>> VegetationItemLOD1MatrixList = new List<NativeList<Matrix4x4>>();
        [NonSerialized]
        public readonly List<NativeList<Matrix4x4>> VegetationItemLOD2MatrixList = new List<NativeList<Matrix4x4>>();
        [NonSerialized]
        public readonly List<NativeList<Matrix4x4>> VegetationItemLOD3MatrixList = new List<NativeList<Matrix4x4>>();

        [NonSerialized]
        public readonly List<NativeList<Matrix4x4>> VegetationItemLOD0ShadowMatrixList = new List<NativeList<Matrix4x4>>();
        [NonSerialized]
        public readonly List<NativeList<Matrix4x4>> VegetationItemLOD1ShadowMatrixList = new List<NativeList<Matrix4x4>>();
        [NonSerialized]
        public readonly List<NativeList<Matrix4x4>> VegetationItemLOD2ShadowMatrixList = new List<NativeList<Matrix4x4>>();
        [NonSerialized]
        public readonly List<NativeList<Matrix4x4>> VegetationItemLOD3ShadowMatrixList = new List<NativeList<Matrix4x4>>();

        [NonSerialized]
        public readonly List<NativeList<Vector4>> VegetationItemLOD0LodFadeList = new List<NativeList<Vector4>>();
        [NonSerialized]
        public readonly List<NativeList<Vector4>> VegetationItemLOD1LodFadeList = new List<NativeList<Vector4>>();
        [NonSerialized]
        public readonly List<NativeList<Vector4>> VegetationItemLOD2LodFadeList = new List<NativeList<Vector4>>();
        [NonSerialized]
        public readonly List<NativeList<Vector4>> VegetationItemLOD3LodFadeList = new List<NativeList<Vector4>>();

        public VegetationStudioCameraRenderList(int vegetationItemCount)
        {
            for (int i = 0; i <= vegetationItemCount - 1; i++)
            {
                NativeList<MatrixInstance> newMatrixList =
                    new NativeList<MatrixInstance>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemMergeMatrixList.Add(newMatrixList);

                NativeList<Matrix4x4> newMatrixLOD0List =
                    new NativeList<Matrix4x4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD0MatrixList.Add(newMatrixLOD0List);

                NativeList<Matrix4x4> newMatrixLOD1List =
                    new NativeList<Matrix4x4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD1MatrixList.Add(newMatrixLOD1List);

                NativeList<Matrix4x4> newMatrixLOD2List =
                    new NativeList<Matrix4x4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD2MatrixList.Add(newMatrixLOD2List);

                NativeList<Matrix4x4> newMatrixLOD3List =
                    new NativeList<Matrix4x4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD3MatrixList.Add(newMatrixLOD3List);

                NativeList<Matrix4x4> newMatrixLOD0ShadowList =
                    new NativeList<Matrix4x4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD0ShadowMatrixList.Add(newMatrixLOD0ShadowList);

                NativeList<Matrix4x4> newMatrixLOD1ShadowList =
                    new NativeList<Matrix4x4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD1ShadowMatrixList.Add(newMatrixLOD1ShadowList);

                NativeList<Matrix4x4> newMatrixLOD2ShadowList =
                    new NativeList<Matrix4x4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD2ShadowMatrixList.Add(newMatrixLOD2ShadowList);

                NativeList<Matrix4x4> newMatrixLOD3ShadowList =
                    new NativeList<Matrix4x4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD3ShadowMatrixList.Add(newMatrixLOD3ShadowList);

                NativeList<Vector4> newLOD0LodFadeList =
                    new NativeList<Vector4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD0LodFadeList.Add(newLOD0LodFadeList);

                NativeList<Vector4> newLOD1LodFadeList =
                    new NativeList<Vector4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD1LodFadeList.Add(newLOD1LodFadeList);

                NativeList<Vector4> newLOD2LodFadeList =
                    new NativeList<Vector4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD2LodFadeList.Add(newLOD2LodFadeList);

                NativeList<Vector4> newLOD3LodFadeList =
                    new NativeList<Vector4>(1024, Allocator.Persistent) { Capacity = 1024 };
                VegetationItemLOD3LodFadeList.Add(newLOD3LodFadeList);
            }
        }

        public void Dispose()
        {
            DisposeMatrixInstanceList(VegetationItemMergeMatrixList);

            DisposeMatrixList(VegetationItemLOD0MatrixList);
            DisposeMatrixList(VegetationItemLOD1MatrixList);
            DisposeMatrixList(VegetationItemLOD2MatrixList);
            DisposeMatrixList(VegetationItemLOD3MatrixList);

            DisposeMatrixList(VegetationItemLOD0ShadowMatrixList);
            DisposeMatrixList(VegetationItemLOD1ShadowMatrixList);
            DisposeMatrixList(VegetationItemLOD2ShadowMatrixList);
            DisposeMatrixList(VegetationItemLOD3ShadowMatrixList);
            
            DisposeVector4List(VegetationItemLOD0LodFadeList);
            DisposeVector4List(VegetationItemLOD1LodFadeList);
            DisposeVector4List(VegetationItemLOD2LodFadeList);
            DisposeVector4List(VegetationItemLOD3LodFadeList);
        }

        void DisposeMatrixList(List<NativeList<Matrix4x4>> list)
        {
            for (int i = 0; i <= list.Count - 1; i++)
            {
                list[i].Dispose();
            }
        }

        void DisposeMatrixInstanceList(List<NativeList<MatrixInstance>> list)
        {
            for (int i = 0; i <= list.Count - 1; i++)
            {
                list[i].Dispose();
            }
        }

        void DisposeVector4List(List<NativeList<Vector4>> list)
        {
            for (int i = 0; i <= list.Count - 1; i++)
            {
                list[i].Dispose();
            }
        }
    }

    [Serializable]
    public enum VegetationStudioCameraType
    {
        Normal,
        SceneView
    }
    
    [Serializable]
    public enum CameraCullingMode
    {
        Frustum = 0,
        Complete360 = 1        
    }

    [Serializable]
    public class VegetationStudioCamera
    {
        [SerializeField]
        public Camera SelectedCamera;

        public VegetationStudioCameraType VegetationStudioCameraType = VegetationStudioCameraType.Normal;
        public JobCullingGroup JobCullingGroup;
        public JobCullingGroup BillboardJobCullingGroup;

        public delegate void MultiOnVegetationCellVisibityChangedDelegate(VegetationStudioCamera vegetationStudioCamera, VegetationCell vegetationCell);
        public MultiOnVegetationCellVisibityChangedDelegate OnVegetationCellVisibleDelegate;       
        public MultiOnVegetationCellVisibityChangedDelegate OnVegetationCellInvisibleDelegate;
        public MultiOnVegetationCellVisibityChangedDelegate OnPotentialCellInvisibleDelegate;
        
        public delegate void MultiOnVegetationDistanceBandChangeDelegate(VegetationStudioCamera vegetationStudioCamera, VegetationCell vegetationCell, int distanceBand, int previousDistanceBand);
        public MultiOnVegetationDistanceBandChangeDelegate OnVegetationCellDistanceBandChangeDelegate;
        
        public bool RenderDirectToCamera;
        public bool RenderBillboardsOnly;
        

        public CameraCullingMode CameraCullingMode = CameraCullingMode.Frustum;
        public VegetationSystemPro VegetationSystemPro;
        private Vector3 _potentialCellsCenterPosition = new Vector3(0,-10000,0);
        private float _potentialCellPadding = 100;
        private float _lastVegetationDistance;
        private bool _dirty;

        private Vector3 _floatingOriginOffset = new Vector3(0,0,0);

        public GameObject WindSampler;

        private JobHandle _currentJobHandle;

        [NonSerialized]
        public List<VegetationStudioCameraRenderList> VegetationStudioCameraRenderList;

        [NonSerialized]
        public List<VegetationCell> PotentialVisibleCellList;

        //[NonSerialized] private List<VegetationCell> _lastVisibleVegetationCellList;
        //[NonSerialized] private List<VegetationCell> _tempVisibleVegetationCellList;

        public bool Enabled => IsEnabled();
        bool IsEnabled()
        {
            bool isPlaying = Application.isPlaying;

            if (VegetationStudioCameraRenderList == null) return false;
            if (JobCullingGroup == null) return false;
            if (BillboardJobCullingGroup == null) return false;

            if (!isPlaying && VegetationStudioCameraType == VegetationStudioCameraType.SceneView)
            {
                return true;
            }

            if (VegetationStudioCameraType == VegetationStudioCameraType.Normal && !isPlaying)
            {
                return false;
            }

            if (SelectedCamera == null)
            {
                Debug.Log("no camera");
                return false;
            }

            return (SelectedCamera && SelectedCamera.enabled && SelectedCamera.gameObject.activeInHierarchy);
        }
        public VegetationStudioCamera(Camera selectedCamera)
        {
            SelectedCamera = selectedCamera;
        }

        Vector3 GetCameraPosition()
        {
            return SelectedCamera.transform.position - _floatingOriginOffset;
        }

        public void SetFloatingOriginOffset(Vector3 floatingOriginOffset)
        {
            _floatingOriginOffset = floatingOriginOffset;

            JobCullingGroup?.SetFloatingOriginOffset(floatingOriginOffset);
            BillboardJobCullingGroup?.SetFloatingOriginOffset(floatingOriginOffset);
        }

        public VegetationStudioCamera(VegetationStudioCameraType vegetationStudioCameraType)
        {
#if UNITY_EDITOR
            if (vegetationStudioCameraType == VegetationStudioCameraType.SceneView)
            {
              
                VegetationStudioCameraType = vegetationStudioCameraType;
                SelectedCamera = SceneViewDetector.GetCurrentSceneViewCamera();
                SceneViewDetector.OnChangedSceneViewCameraDelegate += OnChangedSceneViewCameraDelegate;
            }
#endif
        }

        void OnChangedSceneViewCameraDelegate(Camera camera)
        {          
            SelectedCamera = camera;
            Dispose();
        }

        public void SetDirty()
        {
            _dirty = true;
        }

        public void PreCullVegetation(bool forceUpdate)
        {
            if (!SelectedCamera) return;

            if (JobCullingGroup == null) CreateCullingGroup();
            if (BillboardJobCullingGroup == null) CreateBillboardCullingGroup();

            UpdatePotentialVisibleCells(forceUpdate);
            JobCullingGroup.CameraCullingMode = CameraCullingMode;
            BillboardJobCullingGroup.CameraCullingMode = CameraCullingMode;
            BillboardJobCullingGroup.AddShadowCells = false;
        }

        public JobHandle ScheduleCullVegetationJob(JobHandle dependsOn)
        {
            if (JobCullingGroup == null) return default(JobHandle);
            _currentJobHandle = JobCullingGroup.Cull(dependsOn);
            _currentJobHandle = BillboardJobCullingGroup.Cull(_currentJobHandle);
            return _currentJobHandle;
        }

        public void ProcessEvents()
        {
            //ProcessPotentialVegetationCellsVisibilityEvents();
            JobCullingGroup?.ProcessEvents();
            JobCullingGroup?.ProcessDistanceBandEvents();
        }

        public void PrepareRenderLists(List<VegetationPackagePro> vegetationSystemProList)
        {

            if (!ValidateVegetationStudioCameraRenderList(vegetationSystemProList))
            {
                DisposeVegetationStudioCameraRenderList();
            }

            if (VegetationStudioCameraRenderList == null)
            {
                VegetationStudioCameraRenderList =
                    new List<VegetationStudioCameraRenderList>(vegetationSystemProList.Count);
                for (int i = 0; i <= vegetationSystemProList.Count - 1; i++)
                {
                    VegetationStudioCameraRenderList.Add(new VegetationStudioCameraRenderList(vegetationSystemProList[i].VegetationInfoList.Count));
                }
            }
        }

        void UpdatePotentialVisibleCells(bool forceUpdate)
        {
            Vector3 selectedCameraPosition = GetCameraPosition();

            _potentialCellPadding = VegetationSystemPro.VegetationCellSize * 2;

            bool needsUpdate = forceUpdate;
            if (PotentialVisibleCellList == null)
            {
                PotentialVisibleCellList = new List<VegetationCell>();
                needsUpdate = true;
            }

            float distance = Vector3.Distance(_potentialCellsCenterPosition, selectedCameraPosition);
            if (distance > VegetationSystemPro.VegetationCellSize || Math.Abs(_lastVegetationDistance - VegetationSystemPro.VegetationSettings.GetTreeDistance()) > 0.1f || _dirty)
            {
                needsUpdate = true;
                _potentialCellsCenterPosition = selectedCameraPosition;
                _lastVegetationDistance = VegetationSystemPro.VegetationSettings.GetTreeDistance();
            }

            if (needsUpdate)
            {
                _dirty = false;
                
//                if (_lastVisibleVegetationCellList == null) _lastVisibleVegetationCellList = new List<VegetationCell>();
//                for (int i = 0; i <= JobCullingGroup.VisibleCellIndexList.Length - 1; i++)
//                {
//                    _lastVisibleVegetationCellList.Add(PotentialVisibleCellList[JobCullingGroup.VisibleCellIndexList[i]]);
//                }                
                
                JobCullingGroup.VisibleCellIndexList.Clear();

                Profiler.BeginSample("Potential Cell Selection");

                float areaSize = VegetationSystemPro.VegetationSettings.GetTreeDistance() * 2 + _potentialCellPadding * 2;
                Vector2 position = new Vector2(selectedCameraPosition.x - areaSize / 2f, selectedCameraPosition.z - areaSize / 2f);
                Rect selectedAreaRect = new Rect(position, new Vector2(areaSize, areaSize));

                if (OnPotentialCellInvisibleDelegate != null)
                {
                    for (int i = 0; i <= PotentialVisibleCellList.Count - 1; i++)
                    {
                        VegetationCell vegetationCell = PotentialVisibleCellList[i];
                        if (!vegetationCell.Rectangle.Overlaps(selectedAreaRect))
                        {
                            OnPotentialCellInvisibleDelegate(this, vegetationCell);
                        }
                    }
                }                               
                
                PotentialVisibleCellList.Clear();
                VegetationSystemPro.VegetationCellQuadTree.Query(selectedAreaRect, PotentialVisibleCellList);

                Profiler.EndSample();
                UpdateCullingGroup();

                if (VegetationSystemPro.LoadPotentialVegetationCells)
                {
                    VegetationSystemPro.PredictiveCellLoader.ClearNonImportant();
                    VegetationSystemPro.PredictiveCellLoader.PreloadArea(PotentialVisibleCellList, false);
                }
            }
        }

        void CreateCullingGroup()
        {
            JobCullingGroup?.Dispose();
            JobCullingGroup = new JobCullingGroup { TargetCamera = SelectedCamera };
            JobCullingGroup.OnStateChanged += OnStateChanged;
            JobCullingGroup.OnDistanceBandStateChanged += OnDistanceBandStateChanged;
        }

        void CreateBillboardCullingGroup()
        {
            BillboardJobCullingGroup?.Dispose();
            BillboardJobCullingGroup = new JobCullingGroup { TargetCamera = SelectedCamera };
            //BillboardJobCullingGroup.OnStateChanged += OnStateChanged;

            UpdateBillboardCullingGroup();
        }

        public void UpdateBillboardCullingGroup()
        {
            if (BillboardJobCullingGroup == null) return;

            var currentBillboardCellSize = VegetationSystemPro.BillboardCellSize;
            if (!Application.isPlaying)
            {
                currentBillboardCellSize = 200;
            }

            BillboardJobCullingGroup.DistanceBandList.Clear();
            BillboardJobCullingGroup.DistanceBandList.Add(VegetationSystemPro.VegetationSettings.GetBillboardDistance() + currentBillboardCellSize / 2f);

            BillboardJobCullingGroup.BundingSphereInfoList.Clear();
            if (BillboardJobCullingGroup.BundingSphereInfoList.Capacity < VegetationSystemPro.BillboardCellList.Count)
            {
                BillboardJobCullingGroup.BundingSphereInfoList.Capacity = VegetationSystemPro.BillboardCellList.Count;
            }

            for (int i = 0; i <= VegetationSystemPro.BillboardCellList.Count - 1; i++)
            {
                BoundingSphereInfo boundingSphereInfo = new BoundingSphereInfo
                {
                    BoundingSphere = VegetationSystemPro.BillboardCellList[i].GetBoundingSphere(),
                    LastVisisbility = (int)BoundingSphereVisibility.Invisible,
                    CurrentDistanceBand = -1,
                    //PreviousDistance = -1,
                    Enabled = 1
                };
                BillboardJobCullingGroup.BundingSphereInfoList.Add(boundingSphereInfo);
            }
        }

        void UpdateCullingGroup()
        {
            JobCullingGroup.DistanceBandList.Clear();
            JobCullingGroup.DistanceBandList.Add(VegetationSystemPro.VegetationSettings.GetVegetationDistance() + VegetationSystemPro.VegetationCellSize);
            JobCullingGroup.DistanceBandList.Add(VegetationSystemPro.VegetationSettings.GetTreeDistance() + VegetationSystemPro.VegetationCellSize);
            
            JobCullingGroup.BundingSphereInfoList.Clear();
            if (JobCullingGroup.BundingSphereInfoList.Capacity < PotentialVisibleCellList.Count)
            {
                JobCullingGroup.BundingSphereInfoList.Capacity = PotentialVisibleCellList.Count;
            }

            float additionalBoundingSphereRadius = VegetationSystemPro.AdditionalBoundingSphereRadius;           
            for (int i = 0; i <= PotentialVisibleCellList.Count - 1; i++)
            {
                BoundingSphere boundingSphere = PotentialVisibleCellList[i].GetBoundingSphere();
                boundingSphere.radius += additionalBoundingSphereRadius;
                
                BoundingSphereInfo boundingSphereInfo = new BoundingSphereInfo
                {
                    BoundingSphere = boundingSphere,
                    LastVisisbility = 0,//(int)BoundingSphereVisibility.Invisible,
                    CurrentDistanceBand = -1,
                    PreviousDistanceBand = -1,
                    Enabled = PotentialVisibleCellList[i].EnabledInt
                };
                JobCullingGroup.BundingSphereInfoList.Add(boundingSphereInfo);
            }
        }

//        private void ProcessPotentialVegetationCellsVisibilityEvents()
//        {
//            if (_lastVisibleVegetationCellList.Count == 0) return;                                                                     
//            if (_tempVisibleVegetationCellList == null) _tempVisibleVegetationCellList = new List<VegetationCell>();
//
//            for (int i = 0; i <= JobCullingGroup.VisibleCellIndexList.Length - 1; i++)
//            {
//                _tempVisibleVegetationCellList.Add(PotentialVisibleCellList[JobCullingGroup.VisibleCellIndexList[i]]);
//            }                       
//            
//            for (int i = 0; i <= _lastVisibleVegetationCellList.Count - 1; i++)
//            {
//                if (!_tempVisibleVegetationCellList.Contains(_lastVisibleVegetationCellList[i])) ;
//                {
//                    OnVegetationCellInvisibleDelegate?.Invoke(this, _lastVisibleVegetationCellList[i]);
//                }
//            }
//            _lastVisibleVegetationCellList.Clear();
//            _tempVisibleVegetationCellList.Clear();
//        }

        public BoundingSphereInfo GetBoundingSphereInfo(int potentialVisibleVegetationCellIndex)
        {
            return JobCullingGroup.BundingSphereInfoList[potentialVisibleVegetationCellIndex];
        }

        void OnStateChanged(JobCullingGroupEvent sphere)
        {           
            if (sphere.IsVisible )
            {
                OnVegetationCellVisibleDelegate?.Invoke(this, PotentialVisibleCellList[sphere.Index]);                
            }
            else
            {
                OnVegetationCellInvisibleDelegate?.Invoke(this, PotentialVisibleCellList[sphere.Index]);
            }
        }

        void OnDistanceBandStateChanged(JobCullingGroupEvent sphere)
        {
            OnVegetationCellDistanceBandChangeDelegate?.Invoke(this, PotentialVisibleCellList[sphere.Index],
                sphere.CurrentDistanceBand, sphere.PreviousDistanceBand);
        }

        void DisposeVegetationStudioCameraRenderList()
        {
            if (VegetationStudioCameraRenderList != null)
            {
                for (int i = 0; i <= VegetationStudioCameraRenderList.Count - 1; i++)
                {
                    VegetationStudioCameraRenderList[i].Dispose();
                }

                VegetationStudioCameraRenderList.Clear();
            }

            VegetationStudioCameraRenderList = null;
        }

        bool ValidateVegetationStudioCameraRenderList(List<VegetationPackagePro> vegetationPackageProList)
        {
            if (VegetationStudioCameraRenderList?.Count != vegetationPackageProList.Count) return false;
            for (int i = 0; i <= VegetationStudioCameraRenderList.Count - 1; i++)
            {
                if (VegetationStudioCameraRenderList[i].VegetationItemMergeMatrixList.Count !=
                    vegetationPackageProList[i].VegetationInfoList.Count)
                {
                    return false;
                }
            }        
            return true;
        }

        public void Dispose()
        {    
            PotentialVisibleCellList?.Clear();
            JobCullingGroup?.Dispose();
            JobCullingGroup = null;

            BillboardJobCullingGroup?.Dispose();
            BillboardJobCullingGroup = null;

            DisposeVegetationStudioCameraRenderList();

            _potentialCellsCenterPosition = new Vector3(0,-10000,0);
        }

        public void RemoveDelegates()
        {
#if UNITY_EDITOR
            if (VegetationStudioCameraType == VegetationStudioCameraType.SceneView)
            {
                // ReSharper disable once DelegateSubtraction
                SceneViewDetector.OnChangedSceneViewCameraDelegate -= OnChangedSceneViewCameraDelegate;
            }
#endif
        }

        public void DrawVisibleCellGizmos()
        {
            if (JobCullingGroup == null) return;

            Gizmos.color = Color.white;
            for (int i = 0; i <= JobCullingGroup.VisibleCellIndexList.Length - 1; i++)
            {
                int index = JobCullingGroup.VisibleCellIndexList[i];
                if (PotentialVisibleCellList[index].Enabled)
                {

                    Gizmos.color =
                        GetDistanceBandColor(JobCullingGroup.BundingSphereInfoList[index].CurrentDistanceBand);
                    Gizmos.DrawWireCube(PotentialVisibleCellList[index].VegetationCellBounds.center,
                        PotentialVisibleCellList[index].VegetationCellBounds.size);
                }
            }

            //for (int i = 0; i <= _boundingsphereList.Count - 1; i++)
            //{
            //    Gizmos.DrawWireSphere(_boundingsphereList[i].position, _boundingsphereList[i].radius);
            //}
        }
        
        private Color GetDistanceBandColor(int distanceBand)
        {
            switch (distanceBand)
            {
                case 0:
                    return Color.yellow;
                case 1:
                    return Color.red;                   
            }
            return Color.white;
        }

        public void DrawVisibleBillboardCellGizmos()
        {
            if (BillboardJobCullingGroup == null) return;

            Gizmos.color = Color.green;           

            for (int i = 0; i <= BillboardJobCullingGroup.VisibleCellIndexList.Length - 1; i++)
            {
                int index = BillboardJobCullingGroup.VisibleCellIndexList[i];
                BillboardCell billboardCell = VegetationSystemPro.BillboardCellList[index];

                    Gizmos.DrawWireCube(billboardCell.BilllboardCellBounds.center,
                        billboardCell.BilllboardCellBounds.size);
            }
        }

        public void DrawPotentialCellGizmos()
        {
            if (PotentialVisibleCellList == null) return;

            Gizmos.color = Color.green;

            //for (int i = 0; i <= JobCullingGroup.VisibleCellIndexList.Length - 1; i++)
            //{
            //    int index = JobCullingGroup.VisibleCellIndexList[i];
            //    BoundingSphereInfo boundingSphereInfo = JobCullingGroup.BundingSphereInfoList[index];
            //    if (boundingSphereInfo.CurrentDistanceBand == -1)
            //    {
            //        Gizmos.color = Color.red;
            //        Gizmos.DrawWireCube(PotentialVisibleCellList[index].VegetationCellBounds.center,
            //            PotentialVisibleCellList[index].VegetationCellBounds.size);
            //    }
            //}

            //return;

            for (int i = 0; i <= PotentialVisibleCellList.Count - 1; i++)
            {
                //BoundingSphereInfo boundingSphereInfo = JobCullingGroup.BundingSphereInfoList[i];
                //if (boundingSphereInfo.CurrentDistanceBand == -1)
                //{
                //    Gizmos.color = Color.red;
                //    Gizmos.DrawWireCube(PotentialVisibleCellList[i].VegetationCellBounds.center,
                //            PotentialVisibleCellList[i].VegetationCellBounds.size);
                //}
               if (PotentialVisibleCellList[i].Enabled)
               {
                    //if (PotentialVisibleCellList[i].Loaded)
                    if (PotentialVisibleCellList[i].LoadedDistanceBand == 0)
                    {
                        Gizmos.color = Color.red;
                    }
                    else if (PotentialVisibleCellList[i].LoadedDistanceBand == 1)
                    {
                        Gizmos.color = Color.white;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }

                    Gizmos.DrawWireCube(PotentialVisibleCellList[i].VegetationCellBounds.center,
                        PotentialVisibleCellList[i].VegetationCellBounds.size);
                }
            }
        }
    }
}