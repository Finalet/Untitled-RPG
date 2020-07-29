using System;
using System.Collections.Generic;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;
// ReSharper disable DelegateSubtraction

namespace AwesomeTechnologies.ColliderSystem
{
    public class VegetationPackageColliderInfo
    {
        [NonSerialized] public readonly List<ColliderManager> ColliderManagerList = new List<ColliderManager>();        
    }
    
    public class ColliderManager
    {
        [NonSerialized] public readonly VegetationItemSelector VegetationItemSelector;
        [NonSerialized] public readonly ColliderPool ColliderPool;
        [NonSerialized] public readonly RuntimePrefabStorage RuntimePrefabStorage;
        private readonly VegetationSystemPro _vegetationSystemPro;
        private readonly VegetationItemInfoPro _vegetationItemInfoPro;
        private bool _showColliders;
        
        public delegate void MultiCreateColliderDelegate(GameObject colliderGameObject);
        public MultiCreateColliderDelegate OnCreateColliderDelegate;
        public delegate void MultiBeforeDestroyColliderDelegate(GameObject colliderGameObject);
        public MultiBeforeDestroyColliderDelegate OnBeforeDestroyColliderDelegate;
        
        public ColliderManager(VisibleVegetationCellSelector visibleVegetationCellSelector,
            VegetationSystemPro vegetationSystemPro, VegetationItemInfoPro vegetationItemInfoPro,
            Transform colliderParent, bool showColliders)
        {
            _showColliders = showColliders;
            _vegetationSystemPro = vegetationSystemPro;
            _vegetationItemInfoPro = vegetationItemInfoPro;

            float cullingDistance = vegetationSystemPro.VegetationSettings.GetVegetationDistance() *
                                  vegetationItemInfoPro.ColliderDistanceFactor;
            
            VegetationItemSelector = new VegetationItemSelector(visibleVegetationCellSelector, vegetationSystemPro,
                vegetationItemInfoPro,false,1,0)
            {
                CullingDistance = cullingDistance
            };

            VegetationItemSelector.OnVegetationItemVisibleDelegate += OnVegetationItemVisible;
            VegetationItemSelector.OnVegetationItemInvisibleDelegate += OnVegetationItemInvisible;
            VegetationItemSelector.OnVegetationCellInvisibleDelegate += OnVegetationCellInvisible;

            VegetationItemModelInfo vegetationItemModelInfo =
                vegetationSystemPro.GetVegetationItemModelInfo(vegetationItemInfoPro.VegetationItemID);
            
            ColliderPool = new ColliderPool(vegetationItemInfoPro, vegetationItemModelInfo, vegetationSystemPro, colliderParent, _showColliders);
            RuntimePrefabStorage = new RuntimePrefabStorage(ColliderPool);
        }

        public void SetColliderVisibility(bool value)
        {
            _showColliders = value;
            RuntimePrefabStorage.SetPrefabVisibility(value);
            ColliderPool.SetColliderVisibility(value);
        }
        
        public void UpdateColliderDistance()
        {
            float cullingDistance = _vegetationSystemPro.VegetationSettings.GetVegetationDistance() *
                                  _vegetationItemInfoPro.ColliderDistanceFactor;            

            VegetationItemSelector.CullingDistance = cullingDistance;
        }

        private void OnVegetationItemVisible(ItemSelectorInstanceInfo itemSelectorInstanceInfo,
            VegetationItemIndexes vegetationItemIndexes, string vegetationItemID)
        {
            GameObject colliderObject = ColliderPool.GetObject(itemSelectorInstanceInfo);
            RuntimePrefabStorage.AddRuntimePrefab(colliderObject, itemSelectorInstanceInfo.VegetationCellIndex,
                itemSelectorInstanceInfo.VegetationCellItemIndex);

            OnCreateColliderDelegate?.Invoke(colliderObject);
        }

        private void OnVegetationItemInvisible(ItemSelectorInstanceInfo itemSelectorInstanceInfo,
            VegetationItemIndexes vegetationItemIndexes, string vegetationItemID)
        {

            if (OnBeforeDestroyColliderDelegate != null)
            {
                GameObject colliderObject = RuntimePrefabStorage.GetRuntimePrefab(
                    itemSelectorInstanceInfo.VegetationCellIndex,
                    itemSelectorInstanceInfo.VegetationCellItemIndex);
                OnBeforeDestroyColliderDelegate(colliderObject);
            }                      
            
            RuntimePrefabStorage.RemoveRuntimePrefab(itemSelectorInstanceInfo.VegetationCellIndex,
                itemSelectorInstanceInfo.VegetationCellItemIndex, ColliderPool);
        }

        private void OnVegetationCellInvisible(int vegetationCellIndex)
        {
            RuntimePrefabStorage.RemoveRuntimePrefab(vegetationCellIndex);
        }

        public void Dispose()
        {
            VegetationItemSelector.OnVegetationItemVisibleDelegate -= OnVegetationItemVisible;
            VegetationItemSelector.OnVegetationItemInvisibleDelegate -= OnVegetationItemInvisible;
            VegetationItemSelector.OnVegetationCellInvisibleDelegate -= OnVegetationCellInvisible;

            VegetationItemSelector?.Dispose();
            RuntimePrefabStorage?.Dispose();
            ColliderPool?.Dispose();
        }
    }
}