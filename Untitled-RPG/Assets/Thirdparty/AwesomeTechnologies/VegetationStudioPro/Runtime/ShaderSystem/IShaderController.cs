using System;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public interface IShaderController
    {
        bool MatchShader(string shaderName);
        ShaderControllerSettings Settings
        {
            get;
            set;
        }

        //Called once when a new vegetation item is added or if refreshing prefab
        void CreateDefaultSettings(Material[] materials);

        //called at Vegetation System refresh or if settings changed
        void UpdateMaterial(Material material, EnvironmentSettings environmentSettings);

        //called every frame for custom wind settings
        void UpdateWind(Material material, WindSettings windSettings);

        bool MatchBillboardShader(Material[] materials);
    }

    [Serializable]
    public struct WindSettings
    {
        public Vector3 Direction;
        public float Strength;
    }

    [Serializable]
    public class ShaderControllerSettings : BaseControllerSettings
    {
        public string Heading;
        public string Description;
        public bool SupportsInstantIndirect;
        public bool LODFadeCrossfade = false;
        public bool LODFadePercentage = false;
        public bool SampleWind = false;
        public bool UpdateWind = false;
        public bool DynamicHUE = false;
        public bool BillboardSnow = false;
        public bool BillboardHDWind = false;
        public string OverrideBillboardAtlasShader = "";
        public string OverrideBillboardAtlasNormalShader = "";
        public BillboardRenderMode BillboardRenderMode = BillboardRenderMode.Specular;

        public ShaderControllerSettings()
        {

        }

        public ShaderControllerSettings(ShaderControllerSettings source)
        {
            Heading = source.Heading;
            Description = source.Description;
            SupportsInstantIndirect = source.SupportsInstantIndirect;
            LODFadeCrossfade = source.LODFadeCrossfade;
            LODFadePercentage = source.LODFadePercentage;
            SampleWind = source.SampleWind;
            UpdateWind = source.UpdateWind;
            BillboardSnow = source.BillboardSnow;
            DynamicHUE = source.DynamicHUE;

            for (int i = 0; i <= source.ControlerPropertyList.Count - 1; i++)
            {
                ControlerPropertyList.Add(new SerializedControllerProperty(source.ControlerPropertyList[i]));
            }
        }

        public static bool HasShader(Material material, string[] shaderNames)
        {
            string shaderName = material.shader.name;
            for (int i = 0; i <= shaderNames.Length - 1; i++)
            {
                if (shaderName.Equals(shaderNames[i])) return true;
            }

            return false;
        }
        public static float GetFloatFromMaterials(Material[] materials, string propertyName)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                if (materials[i].HasProperty(propertyName))
                {
                    return materials[i].GetFloat(propertyName);
                }
            }

            return 1;
        }

        public static float GetFloatFromMaterials(Material[] materials, string propertyName, string[] shaderNames)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                if (materials[i].HasProperty(propertyName) && HasShader(materials[i],shaderNames))
                {
                    return materials[i].GetFloat(propertyName);
                }
            }

            return 1;
        }
        
        
        
        public static Vector4 GetVector4FromMaterials(Material[] materials, string propertyName)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                if (materials[i].HasProperty(propertyName))
                {
                    return materials[i].GetVector(propertyName);
                }
            }

            return Vector4.zero;
        }

        public static Color GetColorFromMaterials(Material[] materials, string propertyName, string[] shaderNames)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                if (materials[i].HasProperty(propertyName) && HasShader(materials[i],shaderNames))
                {
                    return materials[i].GetColor(propertyName);
                }
            }

            return Color.white;
        }
        
        public static Color GetColorFromMaterials(Material[] materials, string propertyName)
        {
            for (int i = 0; i <= materials.Length - 1; i++)
            {
                if (materials[i].HasProperty(propertyName))
                {
                    return materials[i].GetColor(propertyName);
                }
            }

            return Color.white;
        }
    }
}
