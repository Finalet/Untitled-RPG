using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AwesomeTechnologies.Utility
{
    public enum VegetationColorMaskQuality
    {
        Low1024 = 0,
        Normal2048 = 1,
        High4096 = 2,
        Ultra8192 =3
    }

    public enum VegetationColorMaskBackgroundSource
    {
        Color = 0,
        Image = 1,
        MicrosplatTerrain = 2
    }
    
    public class VegetationColorMaskCreator : MonoBehaviour
    {
        public VegetationColorMaskQuality VegetationColorMaskQuality = VegetationColorMaskQuality.High4096;
        public int InvisibleLayer = 30;

        public bool IncludeGrass = true;
        public bool IncludePlants = true;
        public bool IncludeTrees;
        public bool IncludeObjects;
        public bool IncludeLargeObjects;

        public float VegetationScale = 2f;
        public Color BackgroundColor = new Color(51f/255f, 126f / 255f, 8f / 255f, 0f / 255f);
        public bool RenderWithoutLight = true;
        public Texture2D BackgroundTexture;
        public Rect AreaRect;
        
        public VegetationColorMaskBackgroundSource BackgroundSource = VegetationColorMaskBackgroundSource.Color;

        public int GetVegetationColorMaskQualityPixelResolution(VegetationColorMaskQuality vegetationColorMaskQuality)
        {
            switch (vegetationColorMaskQuality)
            {
                case VegetationColorMaskQuality.Low1024:
                    return 1024;
                case VegetationColorMaskQuality.Normal2048:
                    return 2048;
                case VegetationColorMaskQuality.High4096:
                    return 4096;
                case VegetationColorMaskQuality.Ultra8192:
                    return 8192;
                default:
                    return 1024;
            }
        }
    }
}