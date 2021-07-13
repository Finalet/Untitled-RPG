using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Keys used to access feature status in the sky profile.
  public class ProfileFeatureKeys
  {
    // General.
    public const string MobileQualityFeature = "MobileQualityFeature"; 
    public const string GradientSkyFeature = "GradientSkyFeature";
    public const string FogFeature = "FogFeature";
    public const string VertexGradientSkyFeature = "VertexGradientSkyFeature";
    
    // Sun.
    public const string SunFeature = "SunFeature";
    public const string SunCustomTextureFeature = "SunCustomTextureFeature";
    public const string SunSpriteSheetFeature = "SunSpriteSheetFeature";
    public const string SunAlphaBlendFeature = "SunAlphaBlendFeature";
    public const string SunRotationFeature = "SunRotationFeature";

    // Moon.
    public const string MoonFeature = "MoonFeature";
    public const string MoonCustomTextureFeature = "MoonCustomTextureFeature";
    public const string MoonSpriteSheetFeature = "MoonSpriteSheetFeature";
    public const string MoonAlphaBlendFeature = "MoonAlphaBlendFeature";
    public const string MoonRotationFeature = "MoonRotationFeature";
    
    // Basic Stars.
    public const string StarBasicFeature = "StarBasicFeature";

    // Star Layer 1.
    public const string StarLayer1Feature = "StarLayer1Feature";
    public const string StarLayer1CustomTextureFeature = "StarLayer1CustomTextureFeature";
    public const string StarLayer1SpriteSheetFeature = "StarLayer1SpriteSheetFeature";
    public const string StarLayer1AlphaBlendFeature = "StarLayer1AlphaBlendFeature";

    // Star Layer 2.
    public const string StarLayer2Feature = "StarLayer2Feature";
    public const string StarLayer2CustomTextureFeature = "StarLayer2CustomTextureFeature";
    public const string StarLayer2SpriteSheetFeature = "StarLayer2SpriteSheetFeature";
    public const string StarLayer2AlphaBlendFeature = "StarLayer2AlphaBlendFeature";

    // Star Layer 3.
    public const string StarLayer3Feature = "StarLayer3Feature";
    public const string StarLayer3CustomTextureFeature = "StarLayer3CustomTextureFeature";
    public const string StarLayer3SpriteSheetFeature = "StarLayer3SpriteSheetFeature";
    public const string StarLayer3AlphaBlendFeature = "StarLayer3AlphaBlendFeature";

    // Rain.
    public const string RainFeature = "RainFeature";
    public const string RainSoundFeature = "RainSoundFeature";
    public const string RainSplashFeature = "RainSplashFeature";

    // Lightning & Thunder.
    public const string LightningFeature = "LightningFeature";
    public const string ThunderFeature = "ThunderFeature";

    // Clouds.
    public const string CloudFeature = "CloudFeature";
    public const string NoiseCloudFeature = "NoiseCloudFeature";
    public const string CubemapCloudFeature = "CubemapCloudFeature";
    public const string CubemapNormalCloudFeature = "CubemapNormalCloudFeature";
    public const string CubemapCloudTextureFormatRGBFeature = "CubemapCloudTextureFormatRGBFeature";
    public const string CubemapCloudTextureFormatRGBAFeature = "CubemapCloudTextureFormatRGBAFeature";
    public const string CubemapCloudDoubleLayerFeature = "CubemapCloudDoubleLayerFeature";
    public const string CubemapNormalCloudDoubleLayerFeature = "CubemapNormalCloudDoubleLayerFeature";
    public const string CubemapCloudDoubleLayerCubemapFeature = "CubemapCloudDoubleLayerCubemap";
    public const string CubemapNormalCloudDoubleLayerCubemapFeature = "CubemapNormalCloudDoubleLayerCubemap";
  }
}
