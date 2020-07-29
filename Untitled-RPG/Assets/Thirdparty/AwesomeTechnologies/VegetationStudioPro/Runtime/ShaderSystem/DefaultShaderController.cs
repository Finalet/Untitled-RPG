using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class DefaultShaderController : IShaderController
    {
        public bool MatchShader(string shaderName)
        {
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings();
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {

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