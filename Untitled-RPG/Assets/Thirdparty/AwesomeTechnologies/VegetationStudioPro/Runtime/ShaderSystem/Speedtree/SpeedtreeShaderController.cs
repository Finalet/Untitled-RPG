#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class SpeedtreeShaderController : IShaderController
    {
        public bool MatchShader(string shaderName)
        {
            return (shaderName == "Nature/SpeedTree");
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
                Heading = "SpeedTree settings",
                Description = "",
                LODFadePercentage = true,
                LODFadeCrossfade = true,
                SampleWind = true,
                DynamicHUE = true,
                BillboardHDWind = true                                               
            };

            Settings.AddBooleanProperty("ReplaceShader", "Replace shader", "This will replace the speedtree shader with a Vegetation Studio version that supports instanced indirect", true);

            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("FoliageHue", "Foliage HUE variation", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HueVariation"));
            Settings.AddColorProperty("FoliageTintColor", "Foliage tint color", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_Color"));

            Settings.AddLabelProperty("Bark settings");
            Settings.AddColorProperty("BarkHue", "Bark HUE variation", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HueVariation"));
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
            bool replaceShader = Settings.GetBooleanPropertyValue("ReplaceShader");


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
            
            if (replaceShader)
            {
                if (material.shader.name == "Nature/SpeedTree")
                {
                    material.shader = Shader.Find("AwesomeTechnologies/VS_SpeedTree");

                }
            }
            //else
            //{
            //    if (material.shader.name != "Nature/SpeedTree")
            //    {
            //        material.shader = Shader.Find("Nature/SpeedTree");
            //    }
            //}
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
