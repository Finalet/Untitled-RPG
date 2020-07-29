using System;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    // ReSharper disable once InconsistentNaming
    public class NMAdvancedGrassController : IShaderController
    {
        private static readonly string[] FoliageShaderNames =
        {
            "NatureManufacture Shaders/Grass/Advanced Grass Light", "NatureManufacture Shaders/Grass/Advanced Grass Specular","NatureManufacture Shaders/Grass/Advanced Grass Standard"
        };

        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            for (int i = 0; i <= FoliageShaderNames.Length - 1; i++)
            {
                if (FoliageShaderNames[i] == shaderName) return true;
            }

            return false;
        }

        public ShaderControllerSettings Settings { get; set; }
        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "Nature Manufacture Advanced Grass",
                Description = "",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = false,
                SupportsInstantIndirect = true,
                BillboardHDWind = false

            };

            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("HealthyColorTint", "Healthy color tint", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HealthyColor"));
            Settings.AddColorProperty("DryColorTint", "Dry color tint", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_DryColor"));
            Settings.AddFloatProperty("ColorNoiseSpread", "Color noise spread", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_ColorNoiseSpread"), 1, 150);
            Settings.AddFloatProperty("AlphaCutoff", "Alpha cutoff", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Cutoff"), 0, 1);

            Settings.AddLabelProperty("Wind settings");
            Settings.AddFloatProperty("InitialBend", "Initial Bend", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_InitialBend"), 0, 10);
            Settings.AddFloatProperty("Stiffness", "Stiffness", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Stiffness"), 0, 10);
            Settings.AddFloatProperty("Drag", "Drag", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Drag"), 0, 10);
            Settings.AddFloatProperty("ShiverDrag", "Shiver Drag", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_ShiverDrag"), 0, 10);
            Settings.AddFloatProperty("ShiverDirectionality", "Shiver Directionality", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_ShiverDirectionality"), 0, 1);
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;
            if (material == null) return;
            try
            {              
                material.SetFloat("_CullFarStart", 10000);
                material.SetColor("_HealthyColor", Settings.GetColorPropertyValue("HealthyColorTint"));
                material.SetColor("_DryColor", Settings.GetColorPropertyValue("DryColorTint"));

                material.SetFloat("_ColorNoiseSpread", Settings.GetFloatPropertyValue("ColorNoiseSpread"));
                material.SetFloat("_Cutoff", Settings.GetFloatPropertyValue("AlphaCutoff"));

                material.SetFloat("_InitialBend", Settings.GetFloatPropertyValue("InitialBend"));
                material.SetFloat("_Stiffness", Settings.GetFloatPropertyValue("Stiffness"));
                material.SetFloat("_Drag", Settings.GetFloatPropertyValue("Drag"));
                material.SetFloat("_ShiverDrag", Settings.GetFloatPropertyValue("ShiverDrag"));
                material.SetFloat("_ShiverDirectionality", Settings.GetFloatPropertyValue("ShiverDirectionality"));
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
            return false;
        }
    }
}
