using System;
using System.Collections.Generic;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

// ReSharper disable DelegateSubtraction

namespace AwesomeTechnologies.ColliderSystem
{    
    [AwesomeTechnologiesScriptOrder(105)]
    [ExecuteInEditMode]
    public class ColliderSystemPro : MonoBehaviour
    {
        public VegetationSystemPro VegetationSystemPro;

        public bool SetBakedCollidersStatic = true;
        public bool ConvertBakedCollidersToMesh = false;
        
        [NonSerialized] public VisibleVegetationCellSelector VisibleVegetationCellSelector;  
        [NonSerialized] public readonly List<VegetationPackageColliderInfo> PackageColliderInfoList = new List<VegetationPackageColliderInfo>(); 
        public NativeList<JobHandle> JobHandleList;
        
        public delegate void MultiCreateColliderDelegate(GameObject colliderGameObject);
        public MultiCreateColliderDelegate OnCreateColliderDelegate;
        public delegate void MultiBeforeDestroyColliderDelegate(GameObject colliderGameObject);
        public MultiBeforeDestroyColliderDelegate OnBeforeDestroyColliderDelegate;
        
        public int CurrentTabIndex;
        public bool ShowDebugCells;
        private Transform _colliderParent;
        public bool ShowColliders;
        private Vector3 _lastFloatingOriginOffset;

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

        private void OnEnable()
        {
            FindVegetationSystemPro();
            SetFloatingOrigin();
            SetupDelegates();
            SetupColliderSystem();
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
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.RuntimePrefabStorage.UpdateFloatingOrigin(deltaFloatingOriginOffset);
                }
            }    
        }

        private void OnCreateCollider(GameObject colliderObject)
        {
            OnCreateColliderDelegate?.Invoke(colliderObject);
        }
        
        private void OnBeforeDestroyCollider(GameObject colliderObject)
        {
            OnBeforeDestroyColliderDelegate?.Invoke(colliderObject);
        }

        private void SetupDelegates()
        {
            if (!VegetationSystemPro) return;
            
            VegetationSystemPro.OnRefreshVegetationSystemDelegate += OnRefreshVegetationSystem;
            VegetationSystemPro.OnRefreshColliderSystemDelegate += OnRefreshVegetationSystem;

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
            VegetationSystemPro.OnRefreshColliderSystemDelegate -= OnRefreshVegetationSystem;

            VegetationSystemPro.OnClearCacheDelegate -= OnClearCache;
            VegetationSystemPro.OnClearCacheVegetationCellDelegate -= OnClearCacheVegetationCell;
            VegetationSystemPro.OnClearCacheVegetationItemDelegate -= OnClearCacheVegetationItem;
            VegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate -=
                OnClearCacheVegetationCellVegetationItem;
            VegetationSystemPro.OnRenderCompleteDelegate -= OnRenderComplete;
        }

        private void OnDisable()
        {
            DisposeColliderSystem();
            RemoveDelegates();
        }
        
        public void SetColliderVisibility(bool value)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.SetColliderVisibility(value);
                }
            }  
        }

        private void OnClearCache(VegetationSystemPro vegetationSystemPro)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.VegetationItemSelector.RefreshAllVegetationCells();
                }
            }   
        }

        private void OnClearCacheVegetationCell(VegetationSystemPro vegetationSystemPro, VegetationCell vegetationCell)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.VegetationItemSelector.RefreshVegetationCell(vegetationCell);
                }
            }    
        }

        private void OnClearCacheVegetationItem(VegetationSystemPro vegetationSystemPro, int vegetationPackageIndex,
            int vegetationItemIndex)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    if (i == vegetationPackageIndex && j == vegetationItemIndex)
                    {
                        ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                        colliderManager?.VegetationItemSelector.RefreshAllVegetationCells(); 
                    }
                }
            }    
        }

        private void OnClearCacheVegetationCellVegetationItem(VegetationSystemPro vegetationSystemPro,
            VegetationCell vegetationCell, int vegetationPackageIndex, int vegetationItemIndex)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    if (i == vegetationPackageIndex && j == vegetationItemIndex)
                    {
                        ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                        colliderManager?.VegetationItemSelector.RefreshVegetationCell(vegetationCell);
                    }
                }
            }    
        }

        private void OnRefreshVegetationSystem(VegetationSystemPro vegetationSystemPro)
        {
            SetupColliderSystem();
        }

        public void UpdateCullingDistance()
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.UpdateColliderDistance();
                }
            }    
        }

        public void SetupColliderSystem()
        {
            if (!VegetationSystemPro) return;
                        
            DisposeColliderSystem();
            
            JobHandleList = new NativeList<JobHandle>(64,Allocator.Persistent);
            
            CreateColliderParent();

            VisibleVegetationCellSelector = new VisibleVegetationCellSelector();

            for (int i = 0; i <= VegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                VegetationPackagePro vegetationPackagePro = VegetationSystemPro.VegetationPackageProList[i];
                VegetationPackageColliderInfo vegetationPackageColliderInfo = new VegetationPackageColliderInfo();

                for (int j = 0; j <= vegetationPackagePro.VegetationInfoList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[j];
                    if (vegetationItemInfoPro.ColliderType != ColliderType.Disabled)
                    {                       
                        ColliderManager tmpColliderManager = new ColliderManager(VisibleVegetationCellSelector,VegetationSystemPro,vegetationItemInfoPro,_colliderParent,ShowColliders);
                        tmpColliderManager.OnCreateColliderDelegate += OnCreateCollider;
                        tmpColliderManager.OnBeforeDestroyColliderDelegate += OnBeforeDestroyCollider;
                        
                        vegetationPackageColliderInfo.ColliderManagerList.Add(tmpColliderManager);
                    }
                    else
                    {
                        vegetationPackageColliderInfo.ColliderManagerList.Add(null);
                    }
                }
                
                PackageColliderInfoList.Add(vegetationPackageColliderInfo);
            }        
            VisibleVegetationCellSelector.Init(VegetationSystemPro);            
        }

        void CreateColliderParent()
        {
            GameObject colliderParentObject = new GameObject("Run-time colliders") {hideFlags = HideFlags.DontSave};
            colliderParentObject.transform.SetParent(transform);
            _colliderParent = colliderParentObject.transform;
        }

        void DestroyColliderParent()
        {
            if (!_colliderParent) return;

            if (Application.isPlaying)
            {
                Destroy(_colliderParent.gameObject);
            }
            else
            {
                DestroyImmediate(_colliderParent.gameObject);
            }
        }

        private void OnRenderComplete(VegetationSystemPro vegetationSystemPro)
        {
            if (PackageColliderInfoList.Count == 0) return;

            TestFloatingOrigin();
            
            Profiler.BeginSample("Collider system processing");
            JobHandleList.Clear();
            JobHandle cullingJobHandle = default(JobHandle);
                       
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    if (colliderManager == null) continue;

                    JobHandle itemCullingHandle = cullingJobHandle;
                    
                    itemCullingHandle =  colliderManager.VegetationItemSelector.ProcessInvisibleCells(itemCullingHandle);
                    itemCullingHandle =  colliderManager.VegetationItemSelector.ProcessVisibleCells(itemCullingHandle);
                    itemCullingHandle =  colliderManager.VegetationItemSelector.ProcessCulling(itemCullingHandle);
                    JobHandleList.Add(itemCullingHandle);
                }
            }
            
            JobHandle mergedHandle = JobHandle.CombineDependencies(JobHandleList);            
            mergedHandle.Complete();
            
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                  ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                  colliderManager?.VegetationItemSelector.ProcessEvents();
                }
            }           
            Profiler.EndSample();
        }

        public void DisposeColliderSystem()
        {
            if (JobHandleList.IsCreated)
            {
                JobHandleList.Dispose();
            }
            
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    if (colliderManager != null)
                    {
                        colliderManager.OnCreateColliderDelegate -= OnCreateCollider;
                        colliderManager.OnBeforeDestroyColliderDelegate -= OnBeforeDestroyCollider; 
                    }
                    colliderManager?.Dispose();
                }

                vegetationPackageColliderInfo.ColliderManagerList.Clear();
            }
            PackageColliderInfoList.Clear();
            VisibleVegetationCellSelector?.Dispose();
            VisibleVegetationCellSelector = null;
            DestroyColliderParent();
        }

        public int GetLoadedInstanceCount()
        {
            int instanceCount = 0;
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    if (colliderManager != null)
                    {
                        instanceCount += colliderManager.VegetationItemSelector.InstanceList.Length;
                    }
                }
            }
            return instanceCount;
        }

        public int GetVisibleColliders()
        {
            int instanceCount = 0;
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    if (colliderManager != null)
                    {
                        instanceCount += colliderManager.RuntimePrefabStorage.RuntimePrefabInfoList.Count;
                    }
                }
            }
            return instanceCount;
        }

        public void BakeCollidersToScene()
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                VegetationPackagePro vegetationPackagePro = VegetationSystemPro.VegetationPackageProList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[j];
                    if (!vegetationItemInfoPro.ColliderUseForBake)
                    {
                        continue;
                    }
                    
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    if (colliderManager != null)
                    {
                        BakeVegetationItemColliders(colliderManager, vegetationItemInfoPro);
                    }
                }
            }
        }

        void BakeVegetationItemColliders(ColliderManager colliderManager, VegetationItemInfoPro vegetationItemInfoPro)
        {

            GC.Collect();
                      
            GameObject rootItem = new GameObject("Baked colliders_" + vegetationItemInfoPro.Name + "_" + vegetationItemInfoPro.VegetationItemID);

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.DisplayProgressBar("Bake Collider: " + vegetationItemInfoPro.Name, "Bake cell", 0);
#endif                       
            int colliderCount = 0;
            
            for (int i = 0; i <= VegetationSystemPro.VegetationCellList.Count - 1; i++)
            {
                VegetationCell vegetationCell = VegetationSystemPro.VegetationCellList[i];
           
#if UNITY_EDITOR
                if (i % 10 == 0)
                {

                    if (!Application.isPlaying) EditorUtility.DisplayProgressBar("Bake Collider: " + vegetationItemInfoPro.Name, "Bake cell " + i + "/" + (VegetationSystemPro.VegetationCellList.Count - 1), i/((float)VegetationSystemPro.VegetationCellList.Count - 1));
                }
#endif
                
                VegetationSystemPro.SpawnVegetationCell(vegetationCell,vegetationItemInfoPro.VegetationItemID);
                NativeList<MatrixInstance> vegetationInstanceList =
                    VegetationSystemPro.GetVegetationItemInstances(vegetationCell, vegetationItemInfoPro.VegetationItemID);
                
                for (int j = 0; j <= vegetationInstanceList.Length - 1; j++)
                {
                    Matrix4x4 vegetationItemMatrix = vegetationInstanceList[j].Matrix;
                    Vector3 position = MatrixTools.ExtractTranslationFromMatrix(vegetationItemMatrix);
                    Vector3 scale = MatrixTools.ExtractScaleFromMatrix(vegetationItemMatrix);
                    Quaternion rotation = MatrixTools.ExtractRotationFromMatrix(vegetationItemMatrix);

                    ItemSelectorInstanceInfo itemSelectorInstanceInfo = new ItemSelectorInstanceInfo
                    {
                        Position = position, Scale = scale, Rotation = rotation
                    };

                    GameObject newCollider = colliderManager.ColliderPool.GetObject(itemSelectorInstanceInfo);
                    newCollider.hideFlags = HideFlags.None;
                    newCollider.transform.SetParent(rootItem.transform, true);
                    SetNavmeshArea(newCollider, vegetationItemInfoPro.NavMeshArea);     
                    
                    if (SetBakedCollidersStatic)
                    {
                        SetStatic(newCollider);   
                    }

                    if (ConvertBakedCollidersToMesh)
                    {
                        CreateNavMeshColliderMeshes(newCollider);
                    }
                    
                    
                    colliderCount++;
                }
                
                vegetationCell.ClearCache();              
            }
            
            VegetationSystemPro.ClearCache(vegetationItemInfoPro.VegetationItemID);

#if UNITY_EDITOR
            if (!Application.isPlaying)  EditorUtility.ClearProgressBar();
#endif
            
            if (colliderCount == 0)
            {
                DestroyImmediate(rootItem);
            }
        }

        static void SetNavmeshArea(GameObject go, int navmeshArea)
        {
#if UNITY_EDITOR          
            GameObjectUtility.SetNavMeshArea(go, navmeshArea);   
#endif            
            foreach(Transform child in go.transform)
            {
                SetNavmeshArea(child.gameObject,navmeshArea);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (ShowDebugCells)
            {
                VisibleVegetationCellSelector?.DrawDebugGizmos();
            }
        }
        
         private static void CreateNavMeshColliderMeshes(GameObject go)
        {
            Material colliderMaterial = new Material(Shader.Find("Standard"));
            colliderMaterial.SetColor("_Color", Color.gray);

            Collider[] colliders = go.GetComponentsInChildren<Collider>();
            for (int i = 0; i <= colliders.Length - 1; i++)
            {
                var collider1 = colliders[i] as CapsuleCollider;
                if (collider1 != null)
                {
                    CapsuleCollider capsuleCollider = collider1;
                    MeshFilter meshFilter = capsuleCollider.gameObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh =
                        MeshUtils.CreateCapsuleMesh(capsuleCollider.radius, capsuleCollider.height);
                    
                    MeshRenderer meshrenderer = capsuleCollider.gameObject.AddComponent<MeshRenderer>();
                    meshrenderer.sharedMaterial = colliderMaterial;

                    switch (collider1.direction)
                    {
                        case 0:
                            collider1.transform.rotation = Quaternion.Euler(collider1.transform.rotation.eulerAngles.x,collider1.transform.rotation.eulerAngles.y,collider1.transform.rotation.eulerAngles.z -90);
                            break; 
                        case 2:
                            collider1.transform.rotation = Quaternion.Euler(collider1.transform.rotation.eulerAngles.x -90,collider1.transform.rotation.eulerAngles.y,collider1.transform.rotation.eulerAngles.z);
                            break; 
                    }

                    collider1.transform.localPosition += new Vector3(
                        capsuleCollider.center.x * capsuleCollider.transform.localScale.x,
                        capsuleCollider.center.y * capsuleCollider.transform.localScale.y,
                        capsuleCollider.center.z * capsuleCollider.transform.localScale.z);                        
                    
                    DestroyImmediate(capsuleCollider);
                }

                var meshCollider1 = colliders[i] as MeshCollider;
                if (meshCollider1 != null)
                {
                    MeshCollider meshCollider = meshCollider1;
                    MeshFilter meshFilter = meshCollider.gameObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = meshCollider.sharedMesh;
                    MeshRenderer meshrenderer = meshCollider.gameObject.AddComponent<MeshRenderer>();
                    meshrenderer.sharedMaterial = colliderMaterial;
                    DestroyImmediate(meshCollider);
                }

                var boxCollider1 = colliders[i] as BoxCollider;
                if (boxCollider1 != null)
                {
                    BoxCollider boxCollider = boxCollider1;
                    MeshFilter meshFilter = boxCollider.gameObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = MeshUtils.CreateBoxMesh(boxCollider.size.z, boxCollider.size.x, boxCollider.size.y);
                    MeshRenderer meshrenderer = boxCollider.gameObject.AddComponent<MeshRenderer>();
                    meshrenderer.sharedMaterial = colliderMaterial;
                    
                    boxCollider.transform.localPosition += new Vector3(
                        boxCollider.center.x * boxCollider.transform.localScale.x,
                        boxCollider.center.y * boxCollider.transform.localScale.y,
                        boxCollider.center.z * boxCollider.transform.localScale.z);      
                    
                    DestroyImmediate(boxCollider);
                }

                var sphereCollider1 = colliders[i] as SphereCollider;
                if (sphereCollider1 != null)
                {
                    SphereCollider sphereCollider = sphereCollider1;
                    MeshFilter meshFilter = sphereCollider.gameObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = MeshUtils.CreateSphereMesh(sphereCollider.radius);
                    MeshRenderer meshrenderer = sphereCollider.gameObject.AddComponent<MeshRenderer>();
                    meshrenderer.sharedMaterial = colliderMaterial;
                    
                    sphereCollider.transform.localPosition += new Vector3(
                        sphereCollider.center.x * sphereCollider.transform.localScale.x,
                        sphereCollider.center.y * sphereCollider.transform.localScale.y,
                        sphereCollider.center.z * sphereCollider.transform.localScale.z);      
                    
                    DestroyImmediate(sphereCollider);
                }
            }
        }

        private static void SetStatic(GameObject go)
        {

#if UNITY_EDITOR
            Collider[] colliders = go.GetComponentsInChildren<Collider>();
            for (int i = 0; i <= colliders.Length - 1; i++)
            {
                colliders[i].gameObject.isStatic = true;
            }
#endif
        }
    }
}