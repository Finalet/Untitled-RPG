#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{

    public class FAEGrassShaderController : IShaderController
    {
        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            return (shaderName == "FAE/Grass") ? true : false;
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
                Heading = "Fantasy Adventure Environment Grass",
                Description = "Description text",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = true,
                SupportsInstantIndirect = true
            };

            bool hasPigmentMap = Shader.GetGlobalTexture("_PigmentMap");


            Settings.AddLabelProperty("Color");
            Settings.AddBooleanProperty("EnablePigmentMap", "Use pigment map", "", hasPigmentMap);
            Settings.AddColorProperty("TopColor", "Top", "", ShaderControllerSettings.GetColorFromMaterials(materials, "_ColorTop"));
            Settings.AddColorProperty("BottomColor", "Bottom", "", ShaderControllerSettings.GetColorFromMaterials(materials, "_ColorBottom"));
            Settings.AddFloatProperty("WindTint", "Wind tint", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_ColorVariation"), 0, 1);
            Settings.AddFloatProperty("AmbientOcclusion", "Ambient Occlusion", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_AmbientOcclusion"), 0, 1);

            Settings.AddLabelProperty("Translucency");
            Settings.AddFloatProperty("TranslucencyAmount", "Amount", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TransmissionAmount"), 0, 10);
            Settings.AddFloatProperty("TranslucencySize", "Size", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TransmissionSize"), 1, 20);

            Settings.AddLabelProperty("Wind");
            Settings.AddFloatProperty("WindInfluence", "Influence", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_MaxWindStrength"), 0, 1);
            Settings.AddFloatProperty("WindSwinging", "Swinging", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_WindSwinging"), 0, 1);
            Settings.AddFloatProperty("WindAmplitude", "Amplitude Multiplier", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_WindAmplitudeMultiplier"), 0, 10);

#if TOUCH_REACT
            Settings.AddLabelProperty("Touch React");
#else
            Settings.AddLabelProperty("Player bending");

#endif
            Settings.AddFloatProperty("BendingInfluence", "Influence", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_BendingInfluence"), 0, 1);


        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;

            //Force enable touch react usage
#if TOUCH_REACT
            material.SetFloat("_VS_TOUCHBEND", 0);
#endif
            material.SetFloat("_PigmentMapInfluence", Settings.GetBooleanPropertyValue("EnablePigmentMap") ? 1 : 0);

            //Allow VS heightmaps to control the height
            material.SetFloat("_MaxHeight", 0.5f);

            material.SetColor("_ColorTop", Settings.GetColorPropertyValue("TopColor"));
            material.SetColor("_ColorBottom", Settings.GetColorPropertyValue("BottomColor"));
            material.SetFloat("_ColorVariation", Settings.GetFloatPropertyValue("WindTint"));
            material.SetFloat("_AmbientOcclusion", Settings.GetFloatPropertyValue("AmbientOcclusion"));

            material.SetFloat("_TransmissionAmount", Settings.GetFloatPropertyValue("TranslucencyAmount"));
            material.SetFloat("_TransmissionSize", Settings.GetFloatPropertyValue("TranslucencySize"));

            material.SetFloat("_MaxWindStrength", Settings.GetFloatPropertyValue("WindInfluence"));
            material.SetFloat("_WindSwinging", Settings.GetFloatPropertyValue("WindSwinging"));
            material.SetFloat("_WindAmplitudeMultiplier", Settings.GetFloatPropertyValue("WindAmplitude"));

            material.SetFloat("_BendingInfluence", Settings.GetFloatPropertyValue("BendingInfluence"));




        }

        public void UpdateWind(Material material, WindSettings windSettings)
        {

        }
    }
}
#endif