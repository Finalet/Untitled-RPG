using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Keys for accessing timeline keyframe groups.
  public abstract class ProfilePropertyKeys
  {
    // Sky.
    public const string SkyCubemapKey = "SkyCubemapKey";
    public const string SkyUpperColorKey = "SkyUpperColorKey";
    public const string SkyMiddleColorKey = "SkyMiddleColorKey";
    public const string SkyLowerColorKey = "SkyLowerColorKey";
    public const string SkyMiddleColorPositionKey = "SkyMiddleColorPosition";
    public const string HorizonTrasitionStartKey = "HorizonTransitionStartKey";
    public const string HorizonTransitionLengthKey = "HorizonTransitionLengthKey";
    public const string HorizonStarScaleKey = "HorizonStarScaleKey";
    public const string StarTransitionStartKey = "StarTransitionStartKey";
    public const string StarTransitionLengthKey = "StarTransitionLengthKey";

    // Sun.
    public const string SunColorKey = "SunColorKey";
    public const string SunTextureKey = "SunTextureKey";
    public const string SunSizeKey = "SunSizeKey";
    public const string SunRotationSpeedKey = "SunRotationSpeedKey";
    public const string SunEdgeFeatheringKey = "SunEdgeFeatheringKey";
    public const string SunColorIntensityKey = "SunColorIntensityKey";
    public const string SunLightColorKey = "SunLightColorKey";
    public const string SunLightIntensityKey = "SunLightIntensityKey";
    public const string SunPositionKey = "SunPositionKey";
    public const string SunSpriteRowCountKey = "SunSpriteRowCountKey";
    public const string SunSpriteColumnCountKey = "SunSpriteColumnCountKey";
    public const string SunSpriteItemCountKey = "SunSpriteItemCount";
    public const string SunSpriteAnimationSpeedKey = "SunSpriteAnimationSpeed";

    // Moon.
    public const string MoonColorKey = "MoonColorKey";
    public const string MoonTextureKey = "MoonTextureKey";
    public const string MoonSizeKey = "MoonSizeKey";
    public const string MoonRotationSpeedKey = "MoonRotationSpeedKey";
    public const string MoonEdgeFeatheringKey = "MoonEdgeFeatheringKey";
    public const string MoonColorIntensityKey = "MoonColorIntensityKey";
    public const string MoonLightColorKey = "MoonLightColorKey";
    public const string MoonLightIntensityKey = "MoonLightIntensityKey";
    public const string MoonPositionKey = "MoonPositionKey";
    public const string MoonOrbitSpeedKey = "MoonOrbitSpeed";
    public const string MoonSpriteRowCountKey = "MoonSpriteRowCountKey";
    public const string MoonSpriteColumnCountKey = "MoonSpriteColumnCountKey";
    public const string MoonSpriteItemCountKey = "MoonSpriteItemCount";
    public const string MoonSpriteAnimationSpeedKey = "MoonSpriteAnimationSpeed";

    // Star Basic.
    public const string StarBasicCubemapKey = "StarBasicCubemapKey";
    public const string StarBasicTwinkleSpeedKey = "StarBasicTwinkleSpeedKey";
    public const string StarBasicTwinkleAmountKey = "StarBasicTwinkleAmountKey";
    public const string StarBasicOpacityKey = "StarBasicOpacityKey";
    public const string StarBasicTintColorKey = "StarBasicTintColorKey";
    public const string StarBasicIntensityKey = "StarBasicIntensityKey";
    public const string StarBasicExponentKey = "StarBasicExponentKey";
    

    // Star layer 1.
    public const string Star1SizeKey = "Star1SizeKey";
    public const string Star1DensityKey = "Star1DensityKey";
    public const string Star1TextureKey = "Star1TextureKey";
    public const string Star1ColorKey = "Star1ColorKey";
    public const string Star1TwinkleAmountKey = "Star1TwinkleAmountKey";
    public const string Star1TwinkleSpeedKey = "Star1TwinkleSpeedKey";
    public const string Star1RotationSpeedKey = "Star1RotationSpeed";
    public const string Star1EdgeFeatheringKey = "Star1EdgeFeathering";
    public const string Star1ColorIntensityKey = "Star1ColorIntensityKey";
    public const string Star1SpriteRowCountKey = "Star1SpriteRowCountKey";
    public const string Star1SpriteColumnCountKey = "Star1SpriteColumnCountKey";
    public const string Star1SpriteItemCountKey = "Star1SpriteItemCount";
    public const string Star1SpriteAnimationSpeedKey = "Star1SpriteAnimationSpeed";

    // Star layer 2.
    public const string Star2SizeKey = "Star2SizeKey";
    public const string Star2DensityKey = "Star2DensityKey";
    public const string Star2TextureKey = "Star2TextureKey";
    public const string Star2ColorKey = "Star2ColorKey";
    public const string Star2TwinkleAmountKey = "Star2TwinkleAmountKey";
    public const string Star2TwinkleSpeedKey = "Star2TwinkleSpeedKey";
    public const string Star2RotationSpeedKey = "Star2RotationSpeed";
    public const string Star2EdgeFeatheringKey = "Star2EdgeFeathering";
    public const string Star2ColorIntensityKey = "Star2ColorIntensityKey";
    public const string Star2SpriteRowCountKey = "Star2SpriteRowCountKey";
    public const string Star2SpriteColumnCountKey = "Star2SpriteColumnCountKey";
    public const string Star2SpriteItemCountKey = "Star2SpriteItemCount";
    public const string Star2SpriteAnimationSpeedKey = "Star2SpriteAnimationSpeed";

    // Star layer 3.
    public const string Star3SizeKey = "Star3SizeKey";
    public const string Star3DensityKey = "Star3DensityKey";
    public const string Star3TextureKey = "Star3TextureKey";
    public const string Star3ColorKey = "Star3ColorKey";
    public const string Star3TwinkleAmountKey = "Star3TwinkleAmountKey";
    public const string Star3TwinkleSpeedKey = "Star3TwinkleSpeedKey";
    public const string Star3RotationSpeedKey = "Star3RotationSpeed";
    public const string Star3EdgeFeatheringKey = "Star3EdgeFeathering";
    public const string Star3ColorIntensityKey = "Star3ColorIntensityKey";
    public const string Star3SpriteRowCountKey = "Star3SpriteRowCountKey";
    public const string Star3SpriteColumnCountKey = "Star3SpriteColumnCountKey";
    public const string Star3SpriteItemCountKey = "Star3SpriteItemCount";
    public const string Star3SpriteAnimationSpeedKey = "Star3SpriteAnimationSpeed";

    // Clouds Noise Generated.
    public const string CloudNoiseTextureKey = "CloudNoiseTextureKey";
    public const string CloudDensityKey = "CloudDensityKey";
    public const string CloudSpeedKey = "CloudSpeedKey";
    public const string CloudDirectionKey = "CloudDirectionKey";
    public const string CloudHeightKey = "CloudHeightKey";
    public const string CloudColor1Key = "CloudColor1Key";
    public const string CloudColor2Key = "CloudColor2Key";
    public const string CloudFadePositionKey = "CloudFadePositionKey";
    public const string CloudFadeAmountKey = "CloudFadeAmountKey";
    public const string CloudTextureTiling = "CloudTextureTiling";

    // Cubemap Normal Clouds.
    public const string CloudCubemapNormalTextureKey = "CloudCubemapNormalTextureKey";
    public const string CloudCubemapNormalLitColorKey = "CloudCubemapNormalLitColorKey";
    public const string CloudCubemapNormalShadowKey = "CloudCubemapNormalShadowColorKey";
    public const string CloudCubemapNormalRotationSpeedKey = "CloudCubemapNormalRotationSpeedKey";
    public const string CloudCubemapNormalAmbientIntensity = "CloudCubemapNormalAmbientIntensityKey";
    public const string CloudCubemapNormalHeightKey = "CloudCubemapNormalHeightKey";
    public const string CloudCubemapNormalDoubleLayerRotationSpeedKey = "CloudCubemapNormalDoubleLayerRotationSpeedKey";
    public const string CloudCubemapNormalDoubleLayerHeightKey = "CloudCubemapNormalDoubleLayerHeightKey";
    public const string CloudCubemapNormalDoubleLayerCustomTextureKey = "CloudCubemapNormalDoubleLayerCustomTextureKey";
    public const string CloudCubemapNormalDoubleLayerLitColorKey = "CloudCubemapNormalDoubleLayerLitColorKey";
    public const string CloudCubemapNormalDoubleLayerShadowKey = "CloudCubemapNormalDoubleLayerShadowKey";

    // Cubemap Clouds.
    public const string CloudCubemapTextureKey = "CloudCubemapTextureKey";
    public const string CloudCubemapRotationSpeedKey = "CloudCubemapRotationSpeedKey";
    public const string CloudCubemapTintColorKey = "CloudCubemapTintColorKey";
    public const string CloudCubemapHeightKey = "CloudCubemapHeightKey";
    public const string CloudCubemapDoubleLayerRotationSpeedKey = "CloudCubemapDoubleLayerRotationSpeedKey";
    public const string CloudCubemapDoubleLayerHeightKey = "CloudCubemapDoubleLayerHeightKey";
    public const string CloudCubemapDoubleLayerCustomTextureKey = "CloudCubemapDoubleLayerCustomTextureKey";
    public const string CloudCubemapDoubleLayerTintColorKey = "CloudCubemapDoubleLayerTintColorKey";

    // Fog.
    public const string FogDensityKey = "FogDensityKey";
    public const string FogColorKey = "FogColorKey";
    public const string FogLengthKey = "FogLengthKey";
    public const string FogSyncWithGlobal = "FogSyncWithGlobal";

    // Rain Drizzle.
    public const string RainNearIntensityKey = "RainNearIntensityKey";
    public const string RainFarIntensityKey = "RainFarIntensityKey";
    public const string RainNearSpeedKey = "RainNearSpeedKey";
    public const string RainFarSpeedKey = "RainFarSpeedKey";
    public const string RainSoundVolumeKey = "RainSoundVolume";
    public const string RainSoundKey = "RainSoundKey";
    public const string RainTintColorKey = "RainTintColorKey";
    public const string RainWindTurbulence = "RainWindTurbulenceKey";
    public const string RainWindTurbulenceSpeed = "RainWindTurbulenceSpeedKey";
    public const string RainNearTextureKey = "RainNearTextureKey";
    public const string RainFarTextureKey = "RainFarTextureKey";
    public const string RainNearTextureTiling = "RainNearTextureTiling";
    public const string RainFarTextureTiling = "RainFarTextureTiling";

    // Rain Ground Splashes.
    public const string RainSplashMaxConcurrentKey = "RainSplashMaxConcurrentKey";
    public const string RainSplashAreaStartKey = "RainSplashAreaStartKey";
    public const string RainSplashAreaLengthKey = "RainSplashAreaLengthKey";
    public const string RainSplashScaleKey = "RainSplashScaleKey";
    public const string RainSplashScaleVarienceKey = "RainSplashScaleVarienceKey";
    public const string RainSplashIntensityKey = "RainSplashIntensityKey";
    public const string RainSplashSurfaceOffsetKey = "RainSplashSurfaceOffsetKey";
    public const string RainSplashTintColorKey = "RainSplashTintColorKey";

    // Lightning and Thunder.
    public const string LightningProbabilityKey = "LightningProbabilityKey";
    public const string LightningStrikeCoolDown = "LightningStrikeCoolDown";
    public const string LightningIntensityKey = "LightningIntensityKey";
    public const string LightningTintColorKey = "LightningTintColorKey";
    public const string ThunderSoundVolumeKey = "ThunderSoundVolumeKey";
    public const string ThunderSoundDelayKey = "ThunderSoundDelayKey";

    public static HashSet<string> GetPropertyKeysSet()
    {
      FieldInfo[] fields = typeof(ProfilePropertyKeys).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      HashSet<string> validPropertyKeys = new HashSet<string>();

      foreach (FieldInfo field in fields) {
        if (!field.IsLiteral) {
          continue;
        }

        string propKey = field.GetValue(null) as string;
        validPropertyKeys.Add(propKey);
      }

      return validPropertyKeys;
    }
  }
}
