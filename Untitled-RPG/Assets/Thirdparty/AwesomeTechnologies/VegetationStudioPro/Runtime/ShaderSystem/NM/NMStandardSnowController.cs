using System;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    // ReSharper disable once InconsistentNaming
    public class NMStandardSnowController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "NatureManufacture Shaders/Standard Shaders/Standard Metalic Snow", "NatureManufacture Shaders/Standard Shaders/Standard Specular Snow"
        };

        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            for (int i = 0; i <= ShaderNames.Length - 1; i++)
            {
                if (ShaderNames[i] == shaderName) return true;
            }

            return false;
        }

        public ShaderControllerSettings Settings { get; set; }
        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "Nature Manufacture Standard Snow",
                Description = "",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = false,
                SupportsInstantIndirect = true,
                BillboardHDWind = false

            };

            Settings.AddLabelProperty("Snow settings");
            Settings.AddBooleanProperty("GlobalSnow", "Use Global Snow Value", "", true);
            Settings.AddFloatProperty("SnowAmount", "Snow Amount", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Snow_Amount"), 0, 1);
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;
            if (material == null) return;
            try
            {
                bool globalSnow = Settings.GetBooleanPropertyValue("GlobalSnow");

                if (globalSnow)
                {
                    material.SetFloat("_Snow_Amount", environmentSettings.SnowAmount * 2f);
                }
                else
                {
                    float snowAmount = Settings.GetFloatPropertyValue("SnowAmount");
                    material.SetFloat("_Snow_Amount", snowAmount * 2f);
                }           
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void UpdateWind(Material material, WindSettings windSettings)
        {
            
        }

        public bool MatchBillboardShader(Material[] materials)
        {
            return true;
        }
    }
}
