using System;
using System.Collections.Generic;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.VegetationSystem;
using Unity.Jobs;
using UnityEngine;

// ReSharper disable DelegateSubtraction
namespace AwesomeTechnologies.PrefabSpawner
{
    //[ExecuteInEditMode]
    public class RuntimePrefabSpawner : MonoBehaviour
    {
        public VegetationSystemPro VegetationSystemPro;
        public int CurrentTabIndex;
        public int VegetationPackageIndex;
        private Transform _runtimePrefabParent;
        public bool ShowDebugCells;
        public bool ShowRuntimePrefabs;

        private Vector3 _lastFloatingOriginOffset;

        [NonSerialized] public VisibleVegetationCellSelector VisibleVegetationCellSelector;

        [NonSerialized] public readonly List<VegetationPackageRuntimePrefabInfo> PackageRuntimePrefabInfoList =
            new List<VegetationPackageRuntimePrefabInfo>();

        private void Reset()
        {
            FindVegetationSystemPro();
        }

        private void FindVegetationSystemPro()
        {
            if (!VegetationSystemPro)
            {
                VegetationSystemPro = GetComponent<VegetationSystemPro>();
            }
        }

        public void RefreshRuntimePrefabs()
        {
            SetupRuntimePrefabSystem();
        }

        private void OnEnable()
        {
            FindVegetationSystemPro();
            SetFloatingOrigin();
            SetupDelegates();
            SetupRuntimePrefabSystem();
        }

        void SetFloatingOrigin()
        {
            if (!VegetationSystemPro) return;
            _lastFloatingOriginOffset = VegetationSystemPro.FloatingOriginOffset;
        }

        void TestFloatingOrigin()
        {
            if (!VegetationSystemPro) return;           
            if (_lastFloatingOriginOffset != VegetationSystemPro.FloatingOriginOffset)
            {
                UpdateFloatingOrigin(VegetationSystemPro.FloatingOriginOffset - _lastFloatingOriginOffset);
            }
            _lastFloatingOriginOffset = VegetationSystemPro.FloatingOriginOffset;
        }

        void UpdateFloatingOrigin(Vector3 deltaFloatingOriginOffset)
        {
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        runtimePrefabManager?.RuntimePrefabStorage.UpdateFloatingOrigin(deltaFloatingOriginOffset);
                    }
                }
            }
        }

        private void SetupDelegates()
        {
            if (!VegetationSystemPro) return;

            VegetationSystemPro.OnRefreshVegetationSystemDelegate += OnRefreshVegetationSystem;
            VegetationSystemPro.OnRefreshRuntimePrefabSpawnerDelegate += OnRefreshVegetationSystem;

            VegetationSystemPro.OnClearCacheDelegate += OnClearCache;
            VegetationSystemPro.OnClearCacheVegetationCellDelegate += OnClearCacheVegetationCell;
            VegetationSystemPro.OnClearCacheVegetationItemDelegate += OnClearCacheVegetationItem;
            VegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate +=
                OnClearCacheVegetationCellVegetationItem;
            VegetationSystemPro.OnRenderCompleteDelegate += OnRenderComplete;
        }

        private void RemoveDelegates()
        {
            if (!VegetationSystemPro) return;

            VegetationSystemPro.OnRefreshVegetationSystemDelegate -= OnRefreshVegetationSystem;
            VegetationSystemPro.OnRefreshRuntimePrefabSpawnerDelegate -= OnRefreshVegetationSystem;


            VegetationSystemPro.OnClearCacheDelegate -= OnClearCache;
            VegetationSystemPro.OnClearCacheVegetationCellDelegate -= OnClearCacheVegetationCell;
            VegetationSystemPro.OnClearCacheVegetationItemDelegate -= OnClearCacheVegetationItem;
            VegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate -=
                OnClearCacheVegetationCellVegetationItem;
            VegetationSystemPro.OnRenderCompleteDelegate -= OnRenderComplete;
        }

        private void OnDisable()
        {
            DisposeRuntimePrefabSystem();
            RemoveDelegates();
        }

        public void SetRuntimePrefabVisibility(bool value)
        {
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        runtimePrefabManager?.SetRuntimePrefabVisibility(value);
                    }
                }
            }
        }
        
        

        private void OnClearCache(VegetationSystemPro vegetationSystemPro)
        {
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        runtimePrefabManager?.VegetationItemSelector.RefreshAllVegetationCells();
                    }
                }
            }
        }

        private void OnClearCacheVegetationCell(VegetationSystemPro vegetationSystemPro, VegetationCell vegetationCell)
        {
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        runtimePrefabManager?.VegetationItemSelector.RefreshVegetationCell(vegetationCell);
                    }
                }
            }
        }

        private void OnClearCacheVegetationItem(VegetationSystemPro vegetationSystemPro, int vegetationPackageIndex,
            int vegetationItemIndex)
        {
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    if (i == vegetationPackageIndex && j == vegetationItemIndex)
                    {
                        VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                            packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                        for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                        {
                            RuntimePrefabManager runtimePrefabManager =
                                vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                            runtimePrefabManager?.VegetationItemSelector.RefreshAllVegetationCells();
                        }
                    }
                }
            }
        }

        private void OnClearCacheVegetationCellVegetationItem(VegetationSystemPro vegetationSystemPro,
            VegetationCell vegetationCell, int vegetationPackageIndex, int vegetationItemIndex)
        {
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    if (i == vegetationPackageIndex && j == vegetationItemIndex)
                    {
                        VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                            packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                        for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                        {
                            RuntimePrefabManager runtimePrefabManager =
                                vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                            runtimePrefabManager?.VegetationItemSelector.RefreshVegetationCell(vegetationCell);
                        }
                    }
                }
            }
        }

        private void OnRefreshVegetationSystem(VegetationSystemPro vegetationSystemPro)
        {
            SetupRuntimePrefabSystem();
        }

        public void UpdateCullingDistance()
        {
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        runtimePrefabManager?.UpdateRuntimePrefabDistance();
                    }
                }
            }
        }

        public void SetupRuntimePrefabSystem()
        {
            if (!VegetationSystemPro) return;

            DisposeRuntimePrefabSystem();
            CreateRuntimePrefabParent();

            VisibleVegetationCellSelector = new VisibleVegetationCellSelector();

            for (int i = 0; i <= VegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                VegetationPackagePro vegetationPackagePro = VegetationSystemPro.VegetationPackageProList[i];
                VegetationPackageRuntimePrefabInfo vegetationPackageRuntimePrefabInfo =
                    new VegetationPackageRuntimePrefabInfo();

                for (int j = 0; j <= vegetationPackagePro.VegetationInfoList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[j];

                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        new VegetationItemRuntimePrefabInfo();

                    for (int k = 0; k <= vegetationItemInfoPro.RuntimePrefabRuleList.Count - 1; k++)
                    {
                        RuntimePrefabRule runtimePrefabRule = vegetationItemInfoPro.RuntimePrefabRuleList[k];
                        RuntimePrefabManager runtimePrefabManager =
                            new RuntimePrefabManager(VisibleVegetationCellSelector, VegetationSystemPro,
                                vegetationItemInfoPro, runtimePrefabRule, _runtimePrefabParent, ShowRuntimePrefabs);                        
                        vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Add(runtimePrefabManager);
                    }

                    vegetationPackageRuntimePrefabInfo.RuntimePrefabManagerList.Add(vegetationItemRuntimePrefabInfo);
                }

                PackageRuntimePrefabInfoList.Add(vegetationPackageRuntimePrefabInfo);
            }

            VisibleVegetationCellSelector.Init(VegetationSystemPro);
        }

        void CreateRuntimePrefabParent()
        {
            GameObject runtimePrefabParentObject = new GameObject("Run-time prefabs") {hideFlags = HideFlags.DontSave};
            runtimePrefabParentObject.transform.SetParent(transform);
            _runtimePrefabParent = runtimePrefabParentObject.transform;
        }

        void DestroyRuntimePrefabParent()
        {
            if (!_runtimePrefabParent) return;

            if (Application.isPlaying)
            {
                Destroy(_runtimePrefabParent.gameObject);
            }
            else
            {
                DestroyImmediate(_runtimePrefabParent.gameObject);
            }
        }

        private void OnRenderComplete(VegetationSystemPro vegetationSystemPro)
        {           
            if (PackageRuntimePrefabInfoList.Count == 0) return;

            TestFloatingOrigin();
            
            UnityEngine.Profiling.Profiler.BeginSample("Runtime prefab system processing");
            JobHandle cullingJobHandle = default(JobHandle);

            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        if (runtimePrefabManager == null) continue;
                        cullingJobHandle =
                            runtimePrefabManager.VegetationItemSelector.ProcessInvisibleCells(cullingJobHandle);
                        cullingJobHandle =
                            runtimePrefabManager.VegetationItemSelector.ProcessVisibleCells(cullingJobHandle);
                        cullingJobHandle = runtimePrefabManager.VegetationItemSelector.ProcessCulling(cullingJobHandle);
                    }
                }
            }

            cullingJobHandle.Complete();

            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        runtimePrefabManager?.VegetationItemSelector.ProcessEvents();
                    }
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void DisposeRuntimePrefabSystem()
        {
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        runtimePrefabManager?.Dispose();
                    }
                }

                packageRuntimePrefabInfo.RuntimePrefabManagerList.Clear();
            }

            PackageRuntimePrefabInfoList.Clear();
            VisibleVegetationCellSelector?.Dispose();
            VisibleVegetationCellSelector = null;
            DestroyRuntimePrefabParent();
            
            
            
        }
        
        

        public int GetLoadedInstanceCount()
        {
            int instanceCount = 0;
            
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        instanceCount += runtimePrefabManager.VegetationItemSelector.InstanceList.Length;
                    }
                }               
            }

            return instanceCount;
        }

        public int GetVisibleColliders()
        {
            
            int instanceCount = 0;
            
            for (int i = 0; i <= PackageRuntimePrefabInfoList.Count - 1; i++)
            {
                VegetationPackageRuntimePrefabInfo packageRuntimePrefabInfo = PackageRuntimePrefabInfoList[i];
                for (int j = 0; j <= packageRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; j++)
                {
                    VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo =
                        packageRuntimePrefabInfo.RuntimePrefabManagerList[j];

                    for (int k = 0; k <= vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList.Count - 1; k++)
                    {
                        RuntimePrefabManager runtimePrefabManager =
                            vegetationItemRuntimePrefabInfo.RuntimePrefabManagerList[k];
                        instanceCount += runtimePrefabManager.RuntimePrefabStorage.RuntimePrefabInfoList.Count;
                    }
                }               
            }

            return instanceCount;
        }        

        private void OnDrawGizmosSelected()
        {
            if (ShowDebugCells)
            {
                VisibleVegetationCellSelector?.DrawDebugGizmos();
            }
        }
    }
}