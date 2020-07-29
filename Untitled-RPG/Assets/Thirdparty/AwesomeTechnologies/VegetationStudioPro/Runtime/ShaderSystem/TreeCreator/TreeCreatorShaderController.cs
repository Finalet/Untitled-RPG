using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class TreeCreatorShaderController : IShaderController
    {
        private static readonly string[] FoliageShaderNames =
        {
            "Hidden/Nature/Tree Creator Albedo Rendertex",
            "Hidden/Nature/Tree Creator Leaves Rendertex",
            "Hidden/Nature/Tree Creator Leaves Optimized",
            "Hidden/Nature/Tree Creator Leaves Fast Optimized",
            "Nature/Tree Creator Leaves Fast",
            "Nature/Tree Creator Leaves",
            "Hidden/Nature/Tree Creator Bark Rendertex",
            "Hidden/Nature/Tree Creator Bark Optimized",
            "Nature/Tree Creator Bark"
        };

        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            for (int i = 0; i <= FoliageShaderNames.Length - 1; i++)
            {
                if (FoliageShaderNames[i] == shaderName) return true;
            }

            if (shaderName.Contains("Tree Creator")) return true;
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }
        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "Tree Creator",
                Description = "",
                LODFadePercentage = false,
                LODFadeCrossfade = false,
                SampleWind = false,
                UpdateWind = true,
                SupportsInstantIndirect = false,
                BillboardHDWind = false,
                OverrideBillboardAtlasNormalShader = "AwesomeTechnologies/Billboards/RenderNormalsAtlas",
                OverrideBillboardAtlasShader = "AwesomeTechnologies/Billboards/RenderDiffuseAtlasNormal",
                BillboardRenderMode = BillboardRenderMode.Standard
            };
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;
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