using System;
using System.Collections.Generic;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;
// ReSharper disable DelegateSubtraction

namespace AwesomeTechnologies.PrefabSpawner
{
    public class VegetationPackageRuntimePrefabInfo
    {
        [NonSerialized] public readonly List<VegetationItemRuntimePrefabInfo> RuntimePrefabManagerList = new List<VegetationItemRuntimePrefabInfo>();                
    }
    
    public class VegetationItemRuntimePrefabInfo
    {
        [NonSerialized] public readonly List<RuntimePrefabManager> RuntimePrefabManagerList = new List<RuntimePrefabManager>();        
    }
    
    public class RuntimePrefabManager
    {
        [NonSerialized] public readonly VegetationItemSelector VegetationItemSelector;

        [NonSerialized] public readonly RuntimePrefabPool RuntimePrefabPool;
        [NonSerialized] public readonly RuntimePrefabStorage RuntimePrefabStorage;
        private readonly VegetationSystemPro _vegetationSystemPro;
        private readonly RuntimePrefabRule _runtimePrefabRule;
        private bool _showPrefabsInHierarchy;
        
        public RuntimePrefabManager(VisibleVegetationCellSelector visibleVegetationCellSelector,
            VegetationSystemPro vegetationSystemPro, VegetationItemInfoPro vegetationItemInfoPro,RuntimePrefabRule runtimePrefabRule,
            Transform prefabParent, bool showPrefabsInHierarchy)
        {
            _showPrefabsInHierarchy = showPrefabsInHierarchy;
            _vegetationSystemPro = vegetationSystemPro;
            _runtimePrefabRule = runtimePrefabRule;
            
            float cullingDistance = vegetationSystemPro.VegetationSettings.GetVegetationDistance() * runtimePrefabRule.DistanceFactor;            
            VegetationItemSelector = new VegetationItemSelector(visibleVegetationCellSelector, vegetationSystemPro,
                vegetationItemInfoPro,true,_runtimePrefabRule.SpawnFrequency,_runtimePrefabRule.Seed)
            {
                CullingDistance = cullingDistance
            };
            
            VegetationItemSelector.OnVegetationItemVisibleDelegate += OnVegetationItemVisible;
            VegetationItemSelector.OnVegetationItemInvisibleDelegate += OnVegetationItemInvisible;
            VegetationItemSelector.OnVegetationCellInvisibleDelegate += OnVegetationCellInvisible;    
            
            RuntimePrefabPool = new RuntimePrefabPool(_runtimePrefabRule,vegetationItemInfoPro, prefabParent,_showPrefabsInHierarchy,_vegetationSystemPro);
            RuntimePrefabStorage = new RuntimePrefabStorage(RuntimePrefabPool);            
        }
        
        public void SetRuntimePrefabVisibility(bool value)
        {
            _showPrefabsInHierarchy = value;
            RuntimePrefabStorage.SetPrefabVisibility(value);
        }
        
        public void UpdateRuntimePrefabDistance()
        {
            float cullingDistance= _vegetationSystemPro.VegetationSettings.GetVegetationDistance() * _runtimePrefabRule.DistanceFactor;
            VegetationItemSelector.CullingDistance = cullingDistance;
        }
        
        private void OnVegetationItemVisible(ItemSelectorInstanceInfo itemSelectorInstanceInfo,
            VegetationItemIndexes vegetationItemIndexes, string vegetationItemID)
        {
            GameObject colliderObject = RuntimePrefabPool.GetObject(itemSelectorInstanceInfo);
            RuntimePrefabStorage.AddRuntimePrefab(colliderObject, itemSelectorInstanceInfo.VegetationCellIndex,
                itemSelectorInstanceInfo.VegetationCellItemIndex);
        }

        private void OnVegetationItemInvisible(ItemSelectorInstanceInfo itemSelectorInstanceInfo,
            VegetationItemIndexes vegetationItemIndexes, string vegetationItemID)
        {
            RuntimePrefabStorage.RemoveRuntimePrefab(itemSelectorInstanceInfo.VegetationCellIndex,itemSelectorInstanceInfo.VegetationCellItemIndex, RuntimePrefabPool);
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
            RuntimePrefabPool?.Dispose();
        }
    }
}