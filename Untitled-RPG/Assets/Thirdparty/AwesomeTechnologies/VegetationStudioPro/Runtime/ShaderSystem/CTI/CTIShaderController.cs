using AwesomeTechnologies.VegetationSystem;
using UnityEngine;


namespace AwesomeTechnologies.Shaders
{
    // ReSharper disable once InconsistentNaming
    public class CTIShaderController : IShaderController
    {
        private static readonly string[] AllShaderNames =
        {
            "CTI/LOD Leaves", "CTI/LOD Bark","CTI/LOD Bark Array"
        };
        
        private static readonly string[] FoliageShaderNames =
        {
            "CTI/LOD Leaves"
        };
        
        private static readonly string[] BarkShaderNames =
        {
            "CTI/LOD Bark","CTI/LOD Bark Array"
        };

        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            for (int i = 0; i <= AllShaderNames.Length - 1; i++)
            {
                if (AllShaderNames[i] == shaderName) return true;
            }

            return false;
        }

        
        
        public ShaderControllerSettings Settings { get; set; }
        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "CTI Tree shader",
                Description = "",
                LODFadePercentage = true,
                LODFadeCrossfade = true,
                SampleWind = false,
                SupportsInstantIndirect = true,
                BillboardHDWind = true,
                BillboardSnow = true,
                OverrideBillboardAtlasNormalShader = "AwesomeTechnologies/Billboards/RenderNormalsAtlasCTI",
                OverrideBillboardAtlasShader = "AwesomeTechnologies/Billboards/RenderDiffuseAtlasCTI"
            };

            Settings.AddLabelProperty("Bark settings");

            Settings.AddColorProperty("BarkColorVariation", "Color variation", "", ShaderControllerSettings.GetColorFromMaterials(materials, "_HueVariation",BarkShaderNames));
            Settings.AddFloatProperty("BarkTranslucencyStrength", "Translucency strength", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TranslucencyStrength",BarkShaderNames), 0, 1);
            Settings.AddFloatProperty("BarkAlphaCutoff", "Alpha cutoff", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Cutoff",BarkShaderNames), 0, 1);           
            
            Settings.AddLabelProperty("Foliage settings");

            Settings.AddColorProperty("ColorVariation", "Color variation", "", ShaderControllerSettings.GetColorFromMaterials(materials, "_HueVariation",FoliageShaderNames));
            Settings.AddFloatProperty("TranslucencyStrength", "Translucency strength", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TranslucencyStrength",FoliageShaderNames), 0, 1);
            Settings.AddFloatProperty("AlphaCutoff", "Alpha cutoff", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Cutoff",FoliageShaderNames), 0, 1);

            Settings.AddLabelProperty("Wind settings");

            Settings.AddFloatProperty("TumbleStrength", "Tumble Strength", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TumbleStrength"), -1, 1);
            Settings.AddFloatProperty("TumbleFrequency", "Tumble Frequency", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TumbleFrequency"), 0, 4);
            Settings.AddFloatProperty("TimeOffset", "Time Offset", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TimeOffset"), 0, 2);
            Settings.AddFloatProperty("LeafTurbulence", "Leaf Turbulence", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_LeafTurbulence"), 0, 4);
            Settings.AddFloatProperty("EdgeFlutterInfluence", "Edge Flutter Influence", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_EdgeFlutterInfluence"), 0, 1);
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;

            Shader.SetGlobalFloat("_Lux_SnowAmount", environmentSettings.SnowAmount);
            Shader.SetGlobalColor("_Lux_SnowColor", environmentSettings.SnowColor);
            Shader.SetGlobalColor("_Lux_SnowSpecColor", environmentSettings.SnowSpecularColor);
            Shader.SetGlobalVector("_Lux_RainfallRainSnowIntensity",new Vector3(environmentSettings.RainAmount, environmentSettings.RainAmount, environmentSettings.SnowAmount));
            Shader.SetGlobalVector("_Lux_WaterFloodlevel",new Vector4(environmentSettings.RainAmount, environmentSettings.RainAmount, environmentSettings.RainAmount,environmentSettings.RainAmount));


            if (ShaderControllerSettings.HasShader(material, FoliageShaderNames))
            {
                material.SetColor("_HueVariation", Settings.GetColorPropertyValue("ColorVariation"));
                material.SetFloat("_TranslucencyStrength", Settings.GetFloatPropertyValue("TranslucencyStrength"));
                material.SetFloat("_Cutoff", Settings.GetFloatPropertyValue("AlphaCutoff"));  
            }
            else
            {
                material.SetColor("_HueVariation", Settings.GetColorPropertyValue("BarkColorVariation"));
                material.SetFloat("_TranslucencyStrength", Settings.GetFloatPropertyValue("BarkTranslucencyStrength"));
                material.SetFloat("_Cutoff", Settings.GetFloatPropertyValue("BarkAlphaCutoff"));
            }


            material.SetFloat("_TumbleStrength", Settings.GetFloatPropertyValue("TumbleStrength"));
            material.SetFloat("_TumbleFrequency", Settings.GetFloatPropertyValue("TumbleFrequency"));
            material.SetFloat("_TimeOffset", Settings.GetFloatPropertyValue("TimeOffset"));
            material.SetFloat("_LeafTurbulence", Settings.GetFloatPropertyValue("LeafTurbulence"));
            material.SetFloat("_EdgeFlutterInfluence", Settings.GetFloatPropertyValue("EdgeFlutterInfluence"));
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
