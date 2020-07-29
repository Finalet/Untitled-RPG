#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    // ReSharper disable once InconsistentNaming
    public class FAETreeShaderController : IShaderController
    {
        private static readonly string[] BranchShaderNames =
        {
            "FAE/Tree Branch"
        };

        private static readonly string[] TrunkShaderNames =
        {
            "FAE/Tree Trunk"
        };

        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            for (int i = 0; i <= BranchShaderNames.Length - 1; i++)
            {
                if (BranchShaderNames[i].Contains(shaderName)) return true;
            }

            for (int i = 0; i <= TrunkShaderNames.Length - 1; i++)
            {
                if (TrunkShaderNames[i].Contains(shaderName)) return true;
            }
            return false;
        }

        public bool MatchBillboardShader(Material[] materials)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                if (materials[i].shader.name == "FAE/Tree Billboard") return true;
            }
            return false;
        }

        bool IsTrunkShader(string shaderName)
        {
            for (int i = 0; i <= TrunkShaderNames.Length - 1; i++)
            {
                if (TrunkShaderNames[i].Contains(shaderName)) return true;
            }

            return false;
        }

        public ShaderControllerSettings Settings { get; set; }
        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "Fantasy Adventure Environment Tree",
                Description = "",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = true,
                SupportsInstantIndirect = false
            };

            Settings.AddLabelProperty("Branch");
            Settings.AddColorProperty("HueVariation", "Hue Variation", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HueVariation"));
            Settings.AddColorProperty("TransmissionColor", "Transmission Color", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_TransmissionColor"));

            Settings.AddLabelProperty("Trunk");
            Settings.AddFloatProperty("GradientBrightness", "Gradient Brightness", "",
                ShaderControllerSettings.GetFloatFromMaterials(materials, "_GradientBrightness"), 0, 2);
            Settings.AddFloatProperty("AmbientOcclusion", "Ambient Occlusion", "",
                ShaderControllerSettings.GetFloatFromMaterials(materials, "_AmbientOcclusion"), 0, 1);

            Settings.AddLabelProperty("Wind");
            Settings.AddFloatProperty("WindInfluence", "Influence", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_MaxWindStrength"), 0, 1);
            Settings.AddFloatProperty("WindAmplitude", "Amplitude Multiplier", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_WindAmplitudeMultiplier"), 0, 10);

        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;

            bool isTrunk = IsTrunkShader(material.shader.name);

            if (isTrunk)
            {
                float ambientOcclusion = Settings.GetFloatPropertyValue("AmbientOcclusion");
                float gradientBrightness = Settings.GetFloatPropertyValue("GradientBrightness");


                material.SetFloat("_AmbientOcclusion", ambientOcclusion);
                material.SetFloat("_GradientBrightness", gradientBrightness);
            }
            else
            {
                material.SetColor("_HueVariation", Settings.GetColorPropertyValue("HueVariation"));
                material.SetColor("_TransmissionColor", Settings.GetColorPropertyValue("TransmissionColor"));
                material.SetFloat("_MaxWindStrength", Settings.GetFloatPropertyValue("WindInfluence"));
                material.SetFloat("_WindAmplitudeMultiplier", Settings.GetFloatPropertyValue("WindAmplitude"));
            }
        }

        public void UpdateWind(Material material, WindSettings windSettings)
        {
            
        }
    }
}
#endif
