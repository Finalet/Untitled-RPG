using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public class VegetationPackageProModelInfo
    {
        public readonly List<VegetationItemModelInfo> VegetationItemModelList = new List<VegetationItemModelInfo>();
        public readonly EnvironmentSettings EnvironmentSettings;

        public VegetationPackageProModelInfo(VegetationPackagePro vegetationPackagePro, EnvironmentSettings environmentSettings, List<GameObject> windSamplerList, int cameraCount)
        {
            EnvironmentSettings = environmentSettings;

            for (int i = 0; i <= vegetationPackagePro.VegetationInfoList.Count - 1; i++)
            {
                VegetationItemModelInfo vegetationItemModelInfo =
                    new VegetationItemModelInfo(vegetationPackagePro.VegetationInfoList[i],EnvironmentSettings, windSamplerList,cameraCount);
                VegetationItemModelList.Add(vegetationItemModelInfo);
            }
        }

        public float GetAdditionalBoundingSphereRadius()
        {
            float additionalBoundingSphereRadius = 0;
            for (int i = 0; i <= VegetationItemModelList.Count -1; i++)
            {
                additionalBoundingSphereRadius = Mathf.Max(VegetationItemModelList[i].BoundingSphereRadius, additionalBoundingSphereRadius);
            }

            return additionalBoundingSphereRadius;
        }

        public void CreateCameraWindSamplerItems(List<GameObject> windSamplerList)
        {
            for (int i = 0; i <= VegetationItemModelList.Count -1; i++)
            {
                VegetationItemModelList[i].CreateCameraWindSamplerItems(windSamplerList);
            }
        }
              
        public void CreateCameraBuffers(int cameraCount)
        {
            for (int i = 0; i <= VegetationItemModelList.Count -1; i++)
            {
                VegetationItemModelList[i].CreateCameraBuffers(cameraCount);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i <= VegetationItemModelList.Count - 1; i++)
            {
                VegetationItemModelList[i].Dispose();
            }
            VegetationItemModelList.Clear();
        }    
    }
}
