#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class Speedtree8ShaderController : IShaderController
    {
        public bool MatchShader(string shaderName)
        {
            return (shaderName == "Nature/SpeedTree8");
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
                Heading = "SpeedTree 8 settings",
                Description = "",
                LODFadePercentage = true,
                LODFadeCrossfade = true,
                SampleWind = true,
                DynamicHUE = true,
                BillboardHDWind = true,
#if UNITY_2018_3_OR_NEWER
                SupportsInstantIndirect = true
#else
                SupportsInstantIndirect = false
#endif
            };
#if UNITY_2018_3_OR_NEWER
            Settings.AddBooleanProperty("ReplaceShader", "Replace shader", "This will replace the speedtree shader with a Vegetation Studio version that supports instanced indirect", true);
#endif
            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("FoliageHue", "Foliage HUE variation", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HueVariationColor"));
            Settings.AddColorProperty("FoliageTintColor", "Foliage tint color", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_Color"));

            Settings.AddLabelProperty("Bark settings");
            Settings.AddColorProperty("BarkHue", "Bark HUE variation", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HueVariationColor"));
            Settings.AddColorProperty("BarkTintColor", "Bark tint color", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_Color"));
        }
       
        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;
            Color foliageHueVariation = Settings.GetColorPropertyValue("FoliageHue");
            Color barkHueVariation = Settings.GetColorPropertyValue("BarkHue");
            Color foliageTintColor = Settings.GetColorPropertyValue("FoliageTintColor");
            Color barkTintColor = Settings.GetColorPropertyValue("BarkTintColor");
            
#if UNITY_2018_3_OR_NEWER
            bool replaceShader = Settings.GetBooleanPropertyValue("ReplaceShader");
#endif
            if (material.HasProperty("_Cutoff"))
            {
                material.SetFloat("_Cutoff", material.GetFloat("_Cutoff"));
            }
          
            if (HasKeyword(material,"GEOM_TYPE_BRANCH"))
            {
                material.SetColor("_HueVariation", barkHueVariation);
                material.SetColor("_Color", barkTintColor);
            }
            else
            {
                material.SetColor("_HueVariation", foliageHueVariation);
                material.SetColor("_Color", foliageTintColor);
            }
            
#if UNITY_2018_3_OR_NEWER            
            if (replaceShader)
            {
                if (material.shader.name == "Nature/SpeedTree8")
                {
                   material.shader = Shader.Find("AwesomeTechnologies/VS_SpeedTree8");
                }
            }
#endif
        }

        bool HasKeyword(Material material, string keyword)
        {
            for (int i = 0; i <= material.shaderKeywords.Length - 1; i++)
            {
                if (material.shaderKeywords[i].Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateWind(Material material, WindSettings windSettings)
        {
            
        }
    }
}
#endif
