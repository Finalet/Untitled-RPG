
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


namespace AwesomeTechnologies.VegetationSystem
{
    [System.Serializable]
    public class VegetationSettings
    {
        public float PlantDistance = 150;
        public float AdditionalTreeMeshDistance = 100;
        public float AdditionalBillboardDistance = 3000;
        public int Seed = 0;

        public float LODDistanceFactor = 1;
        public bool GrassShadows = false;
        public bool PlantShadows = false;
        public bool TreeShadows = true;
        public bool ObjectShadows = true;
        public bool LargeObjectShadows = true;
        public bool BillboardShadows = false;
        public bool DisableRenderDistanceFactor = false;

        public LayerMask GrassLayer = 0;
        public LayerMask PlantLayer = 0;
        public LayerMask TreeLayer = 0;
        public LayerMask ObjectLayer = 0;
        public LayerMask LargeObjectLayer = 0;
        public LayerMask BillboardLayer = 0;

        public float GrassDensity = 1;
        public float PlantDensity = 1;
        public float TreeDensity = 1;
        public float ObjectDensity = 1;
        public float LargeObjectDensity = 1;
        public float VegetationScale = 1f;       

        public ShadowCastingMode GetBillboardShadowCastingMode()
        {
#if UNITY_2018_3_OR_NEWER
            return BillboardShadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off;
#else
            return BillboardShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
#endif
        }

        public ShadowCastingMode GetShadowCastingMode(VegetationType vegetationType)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return GrassShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                case VegetationType.Plant:
                    return PlantShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                case VegetationType.Tree:
                    return TreeShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                case VegetationType.Objects:
                    return ObjectShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                case VegetationType.LargeObjects:
                    return LargeObjectShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }

            return ShadowCastingMode.Off;
        }

        public LayerMask GetLayer(VegetationType vegetationType)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return GrassLayer;
                case VegetationType.Plant:
                    return PlantLayer;
                case VegetationType.Tree:
                    return TreeLayer;
                case VegetationType.Objects:
                    return ObjectLayer;
                case VegetationType.LargeObjects:
                    return LargeObjectLayer;
            }
            return 0;
        }

        public LayerMask GetBillboardLayer()
        {
            return BillboardLayer;
        }

        public float GetVegetationItemDensity(VegetationType vegetationType)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return GrassDensity;
                case VegetationType.Plant:
                    return PlantDensity;
                case VegetationType.Tree:
                    return TreeDensity;
                case VegetationType.Objects:
                    return ObjectDensity;
                case VegetationType.LargeObjects:
                    return LargeObjectDensity;
                default:
                    return 1;
            }
        }

        public float GetVegetationDistance()
        {
            return PlantDistance;
        }

        public float GetBillboardDistance()
        {
            return PlantDistance + AdditionalTreeMeshDistance + AdditionalBillboardDistance;
        }

        public float GetTreeDistance()
        {
            return PlantDistance + AdditionalTreeMeshDistance;
        }
    }
}

