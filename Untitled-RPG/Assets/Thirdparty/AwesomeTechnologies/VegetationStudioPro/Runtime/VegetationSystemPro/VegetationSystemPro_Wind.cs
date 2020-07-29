using System;
using System.Linq;
using AwesomeTechnologies.Extensions;
using AwesomeTechnologies.VegetationSystem.Wind;
using UnityEngine;
using UnityEngine.Profiling;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {      
        void SetupWind()
        {
            _windControllerList.Clear();
            var interfaceType = typeof(IWindController);
            var windControlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetLoadableTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance);

            foreach (var windControler in windControlerTypes)
            {
                IWindController windControllerInterface = (IWindController)windControler;
                if (windControllerInterface == null) continue;

                string windControllerID = windControllerInterface.WindControlerID;
                WindControllerSettings windControllerSettings = GetWindControllerSettings(windControllerID);
                if (windControllerSettings == null)
                {
                    windControllerSettings = windControllerInterface.CreateDefaultSettings();
                    WindControllerSettingsList.Add(windControllerSettings);
                }
                else
                {
                    windControllerInterface.Settings = windControllerSettings;
                }

                _windControllerList.Add(windControllerInterface);
            }           
        }

        private void FindWindZone()
        {
            if (!SelectedWindZone)
            {
                SelectedWindZone = (WindZone)FindObjectOfType(typeof(WindZone));

                if (!SelectedWindZone)
                {
                    GameObject windZoneObject = new GameObject("WindZone");
                    SelectedWindZone = windZoneObject.AddComponent<WindZone>();
                }
            }
        }

        public void UpdateWindSettings()
        {
            for (int i = 0; i <= _windControllerList.Count - 1; i++)
            {
                _windControllerList[i].RefreshSettings();
            }
        }

        void SetupWindSamplers()
        {
            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (VegetationStudioCameraList[i].WindSampler) continue;

                var windSamplerTransform = transform.Find("WindSampler_" + i);

                if (!windSamplerTransform)
                {
                    GameObject windSamplerObject =
                        new GameObject("WindSampler_" + i) {hideFlags = HideFlags.HideInHierarchy};
                    windSamplerObject.transform.SetParent(transform, false);
                    windSamplerObject.transform.position = Vector3.zero;
                    VegetationStudioCameraList[i].WindSampler = windSamplerObject;
                }
                else
                {
                    VegetationStudioCameraList[i].WindSampler = windSamplerTransform.gameObject;
                }
            }
        }

        public void UpdateWind()
        {
            Profiler.BeginSample("Update wind");
            for (int i = 0; i <= _windControllerList.Count - 1; i++)
            {
                _windControllerList[i].UpdateWind(SelectedWindZone, WindSpeedFactor);
            }

            for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            {
                if (!VegetationStudioCameraList[i].Enabled) continue;

                GameObject windSampler = VegetationStudioCameraList[i].WindSampler;
                Camera tempCamera = VegetationStudioCameraList[i].SelectedCamera;
                if (tempCamera)
                {
                    windSampler.transform.position = tempCamera.transform.position;
                    windSampler.transform.rotation = tempCamera.transform.rotation;                  
                }
            }
            Profiler.EndSample();
        }

        WindControllerSettings GetWindControllerSettings(string windControllerID)
        {
            for (int i = 0; i <= WindControllerSettingsList.Count - 1; i++)
            {
                if (WindControllerSettingsList[i] == null) continue;
                if (WindControllerSettingsList[i].WindControlerID == windControllerID)
                    return WindControllerSettingsList[i];
            }
            return null;
        }
    }
}
