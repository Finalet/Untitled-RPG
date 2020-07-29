using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    // ReSharper disable once InconsistentNaming
    public class VSGrassShaderController : IShaderController
    {
        private static readonly string[] FoliageShaderNames =
        {
            "AwesomeTechnologies/Grass/Grass"
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
                Heading = "Vegetation Studio Grass",
                Description = "",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = false,
                SupportsInstantIndirect = true,
                BillboardHDWind = false

            };

            Settings.AddLabelProperty("Foliage settings");

            Settings.AddColorProperty("TintColor1", "Dry color tint", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_Color"));
            Settings.AddColorProperty("TintColor2", "Healthy color tint", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_ColorB"));

            Vector4 colorScale = ShaderControllerSettings.GetVector4FromMaterials(materials, "_AG_ColorNoiseArea");

            Settings.AddFloatProperty("TintAreaScale", "Tint area scale", "", colorScale.y, 10f, 150f);
            Settings.AddFloatProperty("RandomDarkening", "Random darkening", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_RandomDarkening"), 0, 1);
            Settings.AddFloatProperty("RootAmbient", "Root ambient", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_RootAmbient"), 0, 1);
            Settings.AddFloatProperty("AlphaCutoff", "Alpha cutoff", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Cutoff"), 0, 1);
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;
            material.SetColor("_Color", Settings.GetColorPropertyValue("TintColor1"));
            material.SetColor("_ColorB", Settings.GetColorPropertyValue("TintColor2"));
            material.SetFloat("_Cutoff", Settings.GetFloatPropertyValue("AlphaCutoff"));

            material.SetFloat("_RandomDarkening", Settings.GetFloatPropertyValue("RandomDarkening"));
            material.SetFloat("_RootAmbient", Settings.GetFloatPropertyValue("RootAmbient"));

            Vector4 colorScale = material.GetVector("_AG_ColorNoiseArea");
            colorScale = new Vector4(colorScale.x, Settings.GetFloatPropertyValue("TintAreaScale"), colorScale.z, colorScale.w);
            material.SetVector("_AG_ColorNoiseArea", colorScale);
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
