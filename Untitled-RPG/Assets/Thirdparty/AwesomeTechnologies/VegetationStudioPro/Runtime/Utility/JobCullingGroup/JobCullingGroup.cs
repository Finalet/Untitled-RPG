using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace AwesomeTechnologies.Utility.Culling
{
    public struct JobCullingGroupEvent
    {
        public int CurrentDistanceBand;
        public int Index;
        public bool IsVisible;
        public int PreviousDistanceBand;
    }

    public struct BoundingSphereInfo
    {
        public BoundingSphere BoundingSphere;
        public int CurrentDistanceBand;
        public int PreviousDistanceBand;
        public int Visibility;
        public int LastVisisbility;
        public int Enabled;
    }

    enum BoundingSphereVisibility
    {
        Invisible = -1,
        Visible = 1
    }

    [BurstCompile(CompileSynchronously = true)]
    struct BoundingSphereEventJob : IJobParallelForFilter
    {
        [ReadOnly]
        public NativeArray<BoundingSphereInfo> BoundingSphereInfoList;

        public bool Execute(int index)
        {
            //if (BoundingSphereInfoList[index].CurrentDistanceBand != BoundingSphereInfoList[index].PreviousDistance)
            //{
            //    return true;
            //}

            if (BoundingSphereInfoList[index].LastVisisbility != BoundingSphereInfoList[index].Visibility)
            {
                return true;
            }            
            return false;
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    struct BoundingSphereDistanceBandEventJob : IJobParallelForFilter
    {
        [ReadOnly]
        public NativeArray<BoundingSphereInfo> BoundingSphereInfoList;

        public bool Execute(int index)
        {
            if (BoundingSphereInfoList[index].CurrentDistanceBand != BoundingSphereInfoList[index].PreviousDistanceBand)
            {
                return true;
            }

            return false;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    struct BoundingSphereVisibleJob : IJobParallelForFilter
    {
        [ReadOnly]
        public NativeArray<BoundingSphereInfo> BoundingSphereInfoList;

        public bool Execute(int index)
        {
            if (BoundingSphereInfoList[index].Visibility == 1 && BoundingSphereInfoList[index].CurrentDistanceBand != -1)
            {
                return true;
            }

            return false;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    struct BoundingSphereCullJob : IJobParallelFor
    {
        public NativeArray<BoundingSphereInfo> BoundingSphereInfoList;
        [ReadOnly]
        public NativeList<float> DistancesList;
        [ReadOnly]
        public NativeArray<Plane> FrustumPlanes;
        public Vector3 DistanceReferencePoint;
        public bool NoFrustumCulling;
        public bool AddShadowCells;
        public Vector3 FloatingOriginOffset;
        public void Execute(int index)
        {
            BoundingSphereInfo boundingSphereInfo = BoundingSphereInfoList[index];
            boundingSphereInfo.BoundingSphere.position += FloatingOriginOffset;

            if (boundingSphereInfo.Enabled == 0) return;

            boundingSphereInfo.Visibility = NoFrustumCulling ? 1 : SphereInFrustum(boundingSphereInfo.BoundingSphere);

            
            float distance = math.distance(boundingSphereInfo.BoundingSphere.position, DistanceReferencePoint);
            boundingSphereInfo.CurrentDistanceBand = -1;
            for (int i = 0; i <= DistancesList.Length - 1; i++)
            {
                if (distance < DistancesList[i])
                {
                    boundingSphereInfo.CurrentDistanceBand = i;
                    break;
                }
            }

            AddShadowCells = true;
            
            if (AddShadowCells && !NoFrustumCulling)
            {
                if (boundingSphereInfo.Visibility == -1 && boundingSphereInfo.CurrentDistanceBand == 0)
                {
                    boundingSphereInfo.Visibility = 1;
                    boundingSphereInfo.CurrentDistanceBand = 1;
                }
            }

            //changes for collider system
            if (boundingSphereInfo.CurrentDistanceBand == -1)
            {
                boundingSphereInfo.Visibility = -1;
            }
            
            boundingSphereInfo.BoundingSphere.position -= FloatingOriginOffset;
            BoundingSphereInfoList[index] = boundingSphereInfo;
        }

        int SphereInFrustum(BoundingSphere boundingSphere)
        {
            for (int i = 0; i <= FrustumPlanes.Length - 1; i++)
            {
                float dist = FrustumPlanes[i].normal.x * boundingSphere.position.x + FrustumPlanes[i].normal.y * boundingSphere.position.y + FrustumPlanes[i].normal.z * boundingSphere.position.z + FrustumPlanes[i].distance;
                if (dist < -boundingSphere.radius)
                {
                    return -1;
                }
            }
            return 1;
        }
    }

    public class JobCullingGroup
    {
        public NativeList<float> DistanceBandList;
        public NativeList<BoundingSphereInfo> BundingSphereInfoList;
        public NativeList<int> VisibleCellIndexList;

        public CameraCullingMode CameraCullingMode = CameraCullingMode.Frustum;
        public bool AddShadowCells;
        public NativeArray<Plane> FrustumPlanes;
        private NativeList<int> _eventList;
        private NativeList<int> _distanceBandEventList;
        private static readonly Plane[] FrustumPlaneArray = new Plane[6];

        public Camera TargetCamera { set; get; }
        public StateChanged OnStateChanged { get; set; }
        public StateChanged OnDistanceBandStateChanged { get; set; }
        public delegate void StateChanged(JobCullingGroupEvent sphere);

        private Vector3 _floatingOriginOffset = new Vector3(0,0,0);

        public JobCullingGroup()
        {
            DistanceBandList = new NativeList<float>(10, Allocator.Persistent);
            BundingSphereInfoList = new NativeList<BoundingSphereInfo>(Allocator.Persistent);
            FrustumPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
            _eventList = new NativeList<int>(Allocator.Persistent);
            _distanceBandEventList = new NativeList<int>(Allocator.Persistent);
            VisibleCellIndexList = new NativeList<int>(Allocator.Persistent);
        }

        public void SetFloatingOriginOffset(Vector3 floatingOriginOffset)
        {
            _floatingOriginOffset = floatingOriginOffset;
        }

        Vector3 GetTargetCameraPosition()
        {
            return TargetCamera.transform.position;
        }

        public void Dispose()
        {
            if (DistanceBandList.IsCreated) DistanceBandList.Dispose();
            if (BundingSphereInfoList.IsCreated) BundingSphereInfoList.Dispose();
            if (FrustumPlanes.IsCreated) FrustumPlanes.Dispose();
            if (_eventList.IsCreated) _eventList.Dispose();
            if (_distanceBandEventList.IsCreated) _distanceBandEventList.Dispose();
            if (VisibleCellIndexList.IsCreated) VisibleCellIndexList.Dispose();
        }

        public JobHandle Cull(JobHandle dependsOn)
        {
            _eventList.Clear();
            _distanceBandEventList.Clear();
            
            
            if (TargetCamera == null)
            {
               // Debug.LogWarning("Job Culling Group is missing camera");
                return dependsOn;
            }

            if (BundingSphereInfoList.Length == 0)
            {
                return dependsOn;
            }

            Profiler.BeginSample("Prepare VegetationCell culling jobs");

            GeometryUtility.CalculateFrustumPlanes(TargetCamera, FrustumPlaneArray);
            for (int i = 0; i <= 5; i++)
            {
                FrustumPlanes[i] = FrustumPlaneArray[i];
            }
            Vector3 targetCameraPosition = GetTargetCameraPosition();

            BoundingSphereCullJob boundingSphereCullJob =
                new BoundingSphereCullJob
                {
                    BoundingSphereInfoList = BundingSphereInfoList,
                    DistanceReferencePoint = targetCameraPosition,
                    DistancesList = DistanceBandList,
                    FrustumPlanes = FrustumPlanes,
                    NoFrustumCulling = CameraCullingMode == CameraCullingMode.Complete360,
                    AddShadowCells = AddShadowCells,
                    FloatingOriginOffset = _floatingOriginOffset
                };
           
            int length = BundingSphereInfoList.Length;
            
            VisibleCellIndexList.Clear();
            JobHandle handle = boundingSphereCullJob.Schedule(length, 32, dependsOn);

            BoundingSphereVisibleJob boundingSphereVisibleJob = new BoundingSphereVisibleJob { BoundingSphereInfoList = BundingSphereInfoList };
            JobHandle visibleHandle = boundingSphereVisibleJob.ScheduleAppend(VisibleCellIndexList, length, 100, handle);
                        
            BoundingSphereEventJob boundingSphereEventJob =
                new BoundingSphereEventJob { BoundingSphereInfoList = BundingSphereInfoList };
            visibleHandle = boundingSphereEventJob.ScheduleAppend(_eventList, length, 100, visibleHandle);
            
            BoundingSphereDistanceBandEventJob boundingSphereDistanceBandEventJob =
                new BoundingSphereDistanceBandEventJob
                {
                    BoundingSphereInfoList = BundingSphereInfoList
                };
            visibleHandle = boundingSphereDistanceBandEventJob.ScheduleAppend(_distanceBandEventList, length, 100, visibleHandle);           

            Profiler.EndSample();
            return visibleHandle;
        }

        public void ProcessEvents()
        {
            for (int i = 0; i <= _eventList.Length - 1; i++)
            {
                int index = _eventList[i];
                BoundingSphereInfo boundingSphereInfo = BundingSphereInfoList[index];

                if (OnStateChanged != null)
                {
                    JobCullingGroupEvent evt = new JobCullingGroupEvent
                    {
                        IsVisible = boundingSphereInfo.Visibility == (int)BoundingSphereVisibility.Visible,
                        Index = index,
                        CurrentDistanceBand = boundingSphereInfo.CurrentDistanceBand,
                        PreviousDistanceBand = boundingSphereInfo.PreviousDistanceBand
                    };
                    OnStateChanged(evt);
                }
                boundingSphereInfo.LastVisisbility = boundingSphereInfo.Visibility;
                BundingSphereInfoList[index] = boundingSphereInfo;
            }
        }
        
        public void ProcessDistanceBandEvents()
        {
            for (int i = 0; i <= _distanceBandEventList.Length - 1; i++)
            {
                int index = _distanceBandEventList[i];
                BoundingSphereInfo boundingSphereInfo = BundingSphereInfoList[index];

                if (OnDistanceBandStateChanged != null)
                {
                    JobCullingGroupEvent evt = new JobCullingGroupEvent
                    {
                        IsVisible = boundingSphereInfo.Visibility == (int)BoundingSphereVisibility.Visible,
                        Index = index,
                        CurrentDistanceBand = boundingSphereInfo.CurrentDistanceBand,
                        PreviousDistanceBand = boundingSphereInfo.PreviousDistanceBand
                    };
                    OnDistanceBandStateChanged(evt);
                }
                boundingSphereInfo.PreviousDistanceBand = boundingSphereInfo.CurrentDistanceBand;
                BundingSphereInfoList[index] = boundingSphereInfo;
            }
        }
    }
}
