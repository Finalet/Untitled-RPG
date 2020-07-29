using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        public void AddVegetationPackage(VegetationPackagePro vegetationPackagePro)
        {
            if (!VegetationPackageProList.Contains(vegetationPackagePro))
                VegetationPackageProList.Add(vegetationPackagePro);
        }

        public void RemoveVegetationPackage(VegetationPackagePro vegetationPackagePro)
        {
            VegetationPackageProList.Remove(vegetationPackagePro);
        }


        public VegetationPackagePro GetVegetationPackageFromBiome(BiomeType biomeType)
        {
            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                if (VegetationPackageProList[i].BiomeType == biomeType) return VegetationPackageProList[i];
            }

            return null;
        }

        public int GetMaxVegetationPackageItemCount()
        {
            int itemCount = 0;
            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                itemCount = Mathf.Max(VegetationPackageProList[i].VegetationInfoList.Count, itemCount);              
            }
            return itemCount;
        }

        public List<BiomeType> GetAdditionalBiomeList()
        {
            List<BiomeType> allBiomeList = new List<BiomeType>();

            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                if (VegetationPackageProList[i].BiomeType != BiomeType.Default) allBiomeList.Add(VegetationPackageProList[i].BiomeType);
            }
            return allBiomeList.Distinct().ToList();
        }

        public int GetBiomeSortOrder(BiomeType biomeType)
        {
            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                if (VegetationPackageProList[i].BiomeType == biomeType) return VegetationPackageProList[i].BiomeSortOrder;
            }

            return 1;
        }

        public VegetationItemInfoPro GetVegetationItemInfo(string vegetationItemID)
        {
            VegetationItemIndexes indexes = GetVegetationItemIndexes(vegetationItemID);
            if (indexes.VegetationPackageIndex >= 0)
            {
                return VegetationPackageProList[indexes.VegetationPackageIndex]
                    .VegetationInfoList[indexes.VegetationItemIndex];
            }
            return null;
        }

        public void SetAllVegetationPackagesDirty()
        {
#if UNITY_EDITOR
            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                EditorUtility.SetDirty(VegetationPackageProList[i]);
            }
#endif
        }
    }
}
