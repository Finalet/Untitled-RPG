using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        public void SetupVegetationItemModels()
        {
            List<GameObject> windSamplerList = new List<GameObject>();
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                windSamplerList.Add(VegetationStudioCameraList[i].WindSampler);
            }

            float additonalBoundingSphereRadius = 0;            
            ClearVegetationItemModels();
            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                VegetationPackageProModelInfo vegetationPackageProModelInfo =
                    new VegetationPackageProModelInfo(VegetationPackageProList[i], EnvironmentSettings, windSamplerList, VegetationStudioCameraList.Count);
                VegetationPackageProModelsList.Add(vegetationPackageProModelInfo);

                additonalBoundingSphereRadius = Mathf.Max(additonalBoundingSphereRadius,
                    vegetationPackageProModelInfo.GetAdditionalBoundingSphereRadius());
            }
            AdditionalBoundingSphereRadius = additonalBoundingSphereRadius;
        }

        public void SetupVegetationItemModelsPerCameraBuffers()
        {
            List<GameObject> windSamplerList = new List<GameObject>();
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                ClearWindSampler(VegetationStudioCameraList[i].WindSampler);
                windSamplerList.Add(VegetationStudioCameraList[i].WindSampler);
            }
            
            for (int i = 0; i <= VegetationPackageProModelsList.Count - 1; i++)
            {
                VegetationPackageProModelsList[i].CreateCameraBuffers(VegetationStudioCameraList.Count);
                VegetationPackageProModelsList[i].CreateCameraWindSamplerItems(windSamplerList);
            }                       
        }

        void ClearWindSampler(GameObject windSampler)
        {
            if (Application.isPlaying)
            {
                foreach (Transform child in windSampler.transform) {                
                    Destroy(child.gameObject);
                }
            }
            else
            {
                foreach (Transform child in windSampler.transform) {                
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        public VegetationItemModelInfo GetVegetationItemModelInfo(string vegetationItemID)
        {
            VegetationItemIndexes vegetationItemIndexes = GetVegetationItemIndexes(vegetationItemID);
            return GetVegetationItemModelInfo(vegetationItemIndexes.VegetationPackageIndex,
                vegetationItemIndexes.VegetationItemIndex);
        }
        
        public VegetationItemModelInfo GetVegetationItemModelInfo(int vegetationPackageIndex, int vegetationItemIndex)
        {
            if (vegetationPackageIndex < VegetationPackageProModelsList.Count)
            {
                if (vegetationItemIndex <
                    VegetationPackageProModelsList[vegetationPackageIndex].VegetationItemModelList.Count)
                {
                    return VegetationPackageProModelsList[vegetationPackageIndex]
                        .VegetationItemModelList[vegetationItemIndex];
                }
            }
            return null;
        }

        public void RefreshMaterials()
        {
            for (int i = 0; i <= VegetationPackageProModelsList.Count - 1; i++)
            {
                for (int j = 0; j <= VegetationPackageProModelsList[i].VegetationItemModelList.Count - 1; j++)
                {
                    VegetationPackageProModelsList[i].VegetationItemModelList[j].RefreshMaterials();
                }
            }
        }

        void ClearVegetationItemModels()
        {
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (VegetationStudioCameraList[i].WindSampler)
                {
                    foreach (Transform child in VegetationStudioCameraList[i].WindSampler.transform)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(child.gameObject);
                        }
                        else
                        {
                            DestroyImmediate(child.gameObject);
                        }
                    }
                }
            }

            for (int i = 0; i <= VegetationPackageProModelsList.Count - 1; i++)
            {
                VegetationPackageProModelsList[i].Dispose();
            }
            VegetationPackageProModelsList.Clear();
        }
    }
}
