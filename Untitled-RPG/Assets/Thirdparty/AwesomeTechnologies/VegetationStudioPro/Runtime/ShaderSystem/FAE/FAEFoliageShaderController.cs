#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{

    public class FAEFoliageShaderController : IShaderController
    {
        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            return (shaderName == "FAE/Foliage") ? true : false;
        }

        public bool MatchBillboardShader(Material[] materials)
        {
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }
        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "Fantasy Adventure Environment Foliage",
                Description = "Description text",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = true,
                SupportsInstantIndirect = true
            };

            Settings.AddLabelProperty("Color");
            Settings.AddFloatProperty("AmbientOcclusion", "Ambient Occlusion", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_AmbientOcclusion"), 0, 1);

            Settings.AddLabelProperty("Translucency");
            Settings.AddFloatProperty("TranslucencyAmount", "Amount", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TransmissionAmount"), 0, 10);
            Settings.AddFloatProperty("TranslucencySize", "Size", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TransmissionSize"), 1, 20);

            Settings.AddLabelProperty("Wind");
            Settings.AddFloatProperty("WindInfluence", "Influence", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_MaxWindStrength"), 0, 1);
            Settings.AddFloatProperty("GlobalWindMotion", "Global motion", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_GlobalWindMotion"), 0, 1);
            Settings.AddFloatProperty("LeafFlutter", "Leaf flutter", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_LeafFlutter"), 0, 1);
            Settings.AddFloatProperty("WindSwinging", "Swinging", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_WindSwinging"), 0, 1);
            Settings.AddFloatProperty("WindAmplitude", "Amplitude Multiplier", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_WindAmplitudeMultiplier"), 0, 10);
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;

            material.SetFloat("_AmbientOcclusion", Settings.GetFloatPropertyValue("AmbientOcclusion"));

            material.SetFloat("_TransmissionAmount", Settings.GetFloatPropertyValue("TranslucencyAmount"));
            material.SetFloat("_TransmissionSize", Settings.GetFloatPropertyValue("TranslucencySize"));

            material.SetFloat("_MaxWindStrength", Settings.GetFloatPropertyValue("WindInfluence"));
            material.SetFloat("_GlobalWindMotion", Settings.GetFloatPropertyValue("GlobalWindMotion"));
            material.SetFloat("_LeafFlutter", Settings.GetFloatPropertyValue("LeafFlutter"));
            material.SetFloat("_WindSwinging", Settings.GetFloatPropertyValue("WindSwinging"));
            material.SetFloat("_WindAmplitudeMultiplier", Settings.GetFloatPropertyValue("WindAmplitude"));
        }

        public void UpdateWind(Material material, WindSettings windSettings)
        {

        }
    }
}
#endif