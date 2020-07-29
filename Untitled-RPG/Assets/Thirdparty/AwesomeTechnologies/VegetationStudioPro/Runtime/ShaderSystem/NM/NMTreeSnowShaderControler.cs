#if VEGETATION_STUDIO_PRO
using System;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{

    public class NMTreeSnowShaderControler : IShaderController
    {
        private static readonly string[] FoliageShaderNames =
        {
            "NatureManufacture Shaders/Trees/Tree Leaves Metalic Snow", "NatureManufacture Shaders/Trees/Tree Leaves Specular Snow"
        };

        private static readonly string[] BarkShaderNames =
        {
            "NatureManufacture Shaders/Trees/Tree Bark Metalic Snow","NatureManufacture Shaders/Trees/Tree Bark Specular Snow"
        };

        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            for (int i = 0; i <= FoliageShaderNames.Length - 1; i++)
            {
                if (FoliageShaderNames[i] == shaderName) return true;
            }

            for (int i = 0; i <= BarkShaderNames.Length - 1; i++)
            {
                if (BarkShaderNames[i] == shaderName) return true;
            }

            return false;
        }

        public bool MatchBillboardShader(Material[] materials)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                if (materials[i].shader.name == "NatureManufacture Shaders/Trees/Cross Model Shader Snow") return true;
            }

            return false;
        }

        bool IsBarkShader(string shaderName)
        {
            for (int i = 0; i <= BarkShaderNames.Length - 1; i++)
            {
                if (BarkShaderNames[i].Contains(shaderName)) return true;
            }

            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "Nature Manufacture tree with snow",
                Description = "",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = false,
                SupportsInstantIndirect = true,
                BillboardSnow = true,
                BillboardHDWind = true                                            
            };

            Settings.AddLabelProperty("Snow settings");
            Settings.AddBooleanProperty("GlobalSnow", "Use Global Snow Value", "",true);
            Settings.AddFloatProperty("SnowAmount", "Snow Amount", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Snow_Amount"), 0,1);
            Settings.AddFloatProperty("SnowBrightnessReduction", "Brightness Reduction", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_SnowBrightnessReduction"), 0, 1);

            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("HealtyColor", "Healthy Color", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HealthyColor"));
            Settings.AddColorProperty("DryColor", "DryColor", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_DryColor"));

            Settings.AddLabelProperty("Bark settings");
            Settings.AddColorProperty("BarkColor", "Bark Color", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_Color"));

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
                bool barkShader = IsBarkShader(material.shader.name);
                bool globalSnow = Settings.GetBooleanPropertyValue("GlobalSnow");

                if (globalSnow)
                {
                    material.SetFloat("_Snow_Amount", environmentSettings.SnowAmount);
                }
                else
                {
                    float snowAmount = Settings.GetFloatPropertyValue("SnowAmount");
                    material.SetFloat("_Snow_Amount", snowAmount);
                }

                if (barkShader)
                {
                    Color barkColor = Settings.GetColorPropertyValue("BarkColor");
                    material.SetColor("_Color", barkColor);
                }
                else
                {
                    Color healtyColor = Settings.GetColorPropertyValue("HealtyColor");
                    Color dryColor = Settings.GetColorPropertyValue("DryColor");

                    material.SetColor("_HealthyColor", healtyColor);
                    material.SetColor("_DryColor", dryColor);

                    float shiverDrag = Settings.GetFloatPropertyValue("ShiverDrag");
                    float shiverDirectionality = Settings.GetFloatPropertyValue("ShiverDirectionality");

                    material.SetFloat("_ShiverDrag", shiverDrag);
                    material.SetFloat("_ShiverDirectionality", shiverDirectionality);


                    float snowBrightnessReduction = Settings.GetFloatPropertyValue("SnowBrightnessReduction");
                    material.SetFloat("_SnowBrightnessReduction", snowBrightnessReduction);
                }

                float initialBend = Settings.GetFloatPropertyValue("InitialBend");
                float stiffness = Settings.GetFloatPropertyValue("Stiffness");
                float drag = Settings.GetFloatPropertyValue("Drag");

                material.SetFloat("_InitialBend", initialBend);
                material.SetFloat("_Stiffness", stiffness);
                material.SetFloat("_Drag", drag);
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
    }
}
#endif
