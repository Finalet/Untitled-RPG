using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.VegetationStudio
{
    [Serializable]
    public class VegetationSystemProQualityLevel
    {
        public int QualityLevelIndex;
        public string Name;
        
        public float GrassDensity = 1;
        public float PlantDensity = 1;
        public float TreeDensity = 1;
        public float ObjectDensity = 1;
        public float LargeObjectDensity = 1;

        public float PlantDistance = 150;
        public float AdditionalTreeMeshDistance = 150;
        public float AdditionalBillboardDistance = 1000;
        
        public bool GrassShadows;
        public bool PlantShadows;
        public bool TreeShadows = true;
        public bool ObjectShadows = true;
        public bool LargeObjectShadows = true;
        public bool BillboardShadows ;
        
        public List<VegetationPackagePro> VegetationPackageProList = new List<VegetationPackagePro>();
    }
    
    public class QualityManager : MonoBehaviour
    {
        public List<VegetationSystemProQualityLevel> QualityLevelList = new List<VegetationSystemProQualityLevel>();
        public VegetationSystemPro VegetationSystemPro;
        public int QualityLevelIndex;

        private void Reset()
        {
            VegetationSystemPro = GetComponent<VegetationSystemPro>();
        }

        public void SetQualityLevel(bool forceRefresh = true)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            int index = QualitySettings.GetQualityLevel();
            SetQualityLevel(index,forceRefresh);
        }

        public void SetQualityLevel(int index,bool forceRefresh = true)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (index < QualityLevelList.Count)
            {
                VegetationSystemProQualityLevel vegetationSystemProQualityLevel = QualityLevelList[index];
                if (vegetationSystemProQualityLevel == null || !VegetationSystemPro)
                {
                    return;
                }

                VegetationSystemPro.VegetationSettings.GrassDensity = vegetationSystemProQualityLevel.GrassDensity;
                VegetationSystemPro.VegetationSettings.PlantDensity = vegetationSystemProQualityLevel.PlantDensity;

                VegetationSystemPro.VegetationSettings.TreeDensity = vegetationSystemProQualityLevel.TreeDensity;
                VegetationSystemPro.VegetationSettings.ObjectDensity = vegetationSystemProQualityLevel.ObjectDensity;
                VegetationSystemPro.VegetationSettings.LargeObjectDensity = vegetationSystemProQualityLevel.LargeObjectDensity;

                VegetationSystemPro.VegetationSettings.GrassShadows = vegetationSystemProQualityLevel.GrassShadows;
                VegetationSystemPro.VegetationSettings.PlantShadows = vegetationSystemProQualityLevel.PlantShadows;
                VegetationSystemPro.VegetationSettings.TreeShadows = vegetationSystemProQualityLevel.TreeShadows;
                VegetationSystemPro.VegetationSettings.ObjectShadows = vegetationSystemProQualityLevel.ObjectShadows;
                VegetationSystemPro.VegetationSettings.LargeObjectShadows= vegetationSystemProQualityLevel.LargeObjectShadows;
                VegetationSystemPro.VegetationSettings.BillboardShadows = vegetationSystemProQualityLevel.BillboardShadows;

                VegetationSystemPro.VegetationSettings.PlantDistance = vegetationSystemProQualityLevel.PlantDistance;
                VegetationSystemPro.VegetationSettings.AdditionalTreeMeshDistance = vegetationSystemProQualityLevel.AdditionalTreeMeshDistance;
                VegetationSystemPro.VegetationSettings.AdditionalBillboardDistance = vegetationSystemProQualityLevel.AdditionalBillboardDistance;

                VegetationSystemPro.VegetationPackageProList.Clear();

                for (int i = 0; i <= vegetationSystemProQualityLevel.VegetationPackageProList.Count - 1; i++)
                {
                    VegetationPackagePro vegetationPackagePro =
                        vegetationSystemProQualityLevel.VegetationPackageProList[i];
                    if (vegetationPackagePro)
                    {
                        VegetationSystemPro.VegetationPackageProList.Add(vegetationPackagePro);
                    }
                }

                if (!forceRefresh)
                {
                    return;
                }

                VegetationSystemPro.ClearCache();
                VegetationSystemPro.RefreshVegetationSystem();
            }
        }
    }
}
