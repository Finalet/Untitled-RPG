using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public class Standard3dShaderDefinition : BaseShaderDefinition
  {
    public const float MaxStarSize = .2f;
    public const float MaxStarDensity = 1.0f;
    public const float MinEdgeFeathering = 0.0001f;
    public const float MinStarFadeBegin = -.999f;
    public const float MaxSpriteItems = 100000;
    public const float MinRotationSpeed = -10.0f;
    public const float MaxRotationSpeed = 10.0f;
    public const float MaxCloudRotationSpeed = .5f;
    public const float MaxHDRValue = 25.0f;

    public Standard3dShaderDefinition()
    {
      shaderName = "Funly/Sky Studio/Skybox/3D Standard";
    }

    protected override ProfileFeatureSection[] ProfileFeatureSection()
    {
      return new[]
      {
        new ProfileFeatureSection("Features", ProfileSectionKeys.FeaturesSectionKey, new []
        {
          ProfileFeatureDefinition.CreateBooleanFeature(ProfileFeatureKeys.MobileQualityFeature, false, "Use Mobile Features (Faster)", null, false,
            "Enables Sky Studio to render in mobile mode, which has simplified options and settings for low-end devices"),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.GradientSkyFeature, ShaderKeywords.Gradient, true, "Gradient Background", null, false,
            "Enables gradient background feature in shader as an alternative to a cubemap background."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.SunFeature, ShaderKeywords.Sun, false, "Sun", null, false,
            "Enables sun feature in skybox shader."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.MoonFeature, ShaderKeywords.Moon, false, "Moon", null, false,
            "Enables moon feature in skybox shader."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.CloudFeature, ShaderKeywords.Clouds, false, "Clouds", null, false,
            "Enables cloud feature in the skybox shader."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.FogFeature, ShaderKeywords.Fog, false, "Fog", null, false,
            "Enables fog feature in the skybox shader."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarBasicFeature, ShaderKeywords.StarsBasic, false, "Stars (Mobile)", ProfileFeatureKeys.MobileQualityFeature, true,
            "Enable fast simple cubemap stars designed for mobile devices for faster rendering."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer1Feature, ShaderKeywords.StarLayer1, false, "Star Layer 1", ProfileFeatureKeys.MobileQualityFeature, false,
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer2Feature, ShaderKeywords.StarLayer2, false, "Star Layer 2", ProfileFeatureKeys.MobileQualityFeature, false,
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer3Feature, ShaderKeywords.StarLayer3, false, "Star Layer 3", ProfileFeatureKeys.MobileQualityFeature, false,
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          ProfileFeatureDefinition.CreateBooleanFeature(ProfileFeatureKeys.RainFeature, false, "Rain", null, false,
            "Enables animated rain in the scene."),
          ProfileFeatureDefinition.CreateBooleanFeature(ProfileFeatureKeys.RainSplashFeature, false, "Rain Surface Splashes", null, false,
            "Enables surface splashes to simulate raindrops hitting the ground and other scene objects."),
          ProfileFeatureDefinition.CreateBooleanFeature(ProfileFeatureKeys.LightningFeature, false, "Lightning", null, false,
            "Enables lighting bolts in the scene at user staged spawn areas.")
        }),
        new ProfileFeatureSection("Sky", ProfileSectionKeys.SkySectionKey, new[] {
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.VertexGradientSkyFeature, ShaderKeywords.VertexGradient, false, "Use Vertex Gradient (Faster)", null, false,
            "If enabled the background gradient colors will be calculated per vertex (which is very fast on low-end hardware) as opposed to per pixel fragment." )
        }),
        new ProfileFeatureSection("Sun", ProfileSectionKeys.SunSectionKey, new []
        {
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.SunCustomTextureFeature, ShaderKeywords.SunCustomTexture, false, "Use Custom Texture", null, false,
            "Enables a custom texture to be used for the sun."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.SunSpriteSheetFeature, ShaderKeywords.SunSpriteSheet, false, "Texture Is Sprite Sheet Animation", ProfileFeatureKeys.SunCustomTextureFeature, true,
            "If enabled the sun texture will be used as a sprite sheet animation."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.SunAlphaBlendFeature, ShaderKeywords.SunAlphaBlend, false, "Use Alpha Blending", ProfileFeatureKeys.SunCustomTextureFeature, true,
            "Enables alpha blending of the sun texture into the background. If disabled additive blending will be used."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.SunRotationFeature, ShaderKeywords.SunRotation, false, "Animate Sun Rotation", ProfileFeatureKeys.SunCustomTextureFeature, true,
            "If enabled the sun texture will rotate using the rotation speed property"),
        }),
        new ProfileFeatureSection("Moon", ProfileSectionKeys.MoonSectionKey, new []
        {
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.MoonCustomTextureFeature, ShaderKeywords.MoonCustomTexture, false, "Use Custom Texture", null, false,
            "Enables a custom texture to be used for the moon."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.MoonSpriteSheetFeature, ShaderKeywords.MoonSpriteSheet, false, "Texture Is Sprite Sheet Animation", ProfileFeatureKeys.MoonCustomTextureFeature, true,
            "If enabled the moon texture will be used as a sprite sheet animation."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.MoonAlphaBlendFeature, ShaderKeywords.MoonAlphaBlend, false, "Use Alpha Blending", ProfileFeatureKeys.MoonCustomTextureFeature, true,
            "Enables alpha blending of the moon texture into the background. If disabled additive blending will be used."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.MoonRotationFeature, ShaderKeywords.MoonRotation, false, "Animate Moon Rotation", ProfileFeatureKeys.MoonCustomTextureFeature, true,
            "If enabled the moon texture will rotate using the rotation speed property"),
        }),
        new ProfileFeatureSection("Star Layer 1", ProfileSectionKeys.Star1SectionKey, new []
        {
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer1CustomTextureFeature, ShaderKeywords.StarLayer1CustomTexture, false, "Use Custom Texture", null, false,
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer1SpriteSheetFeature, ShaderKeywords.StarLayer1SpriteSheet, false, "Texture Is Sprite Sheet Animation", ProfileFeatureKeys.StarLayer1CustomTextureFeature, true,
            "If enabled star texture will be used as a sprite sheet animation."),
        }),
        new ProfileFeatureSection("Star Layer 2", ProfileSectionKeys.Star2SectionKey, new []
        {
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer2CustomTextureFeature, ShaderKeywords.StarLayer2CustomTexture, false, "Use Custom Texture", null, false,
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer2SpriteSheetFeature, ShaderKeywords.StarLayer2SpriteSheet, false, "Texture Is Sprite Sheet Animation", ProfileFeatureKeys.StarLayer2CustomTextureFeature, true,
            "If enabled star texture will be used as a sprite sheet animation."),
        }),
        new ProfileFeatureSection("Clouds", ProfileSectionKeys.CloudSectionKey, new [] {
          ProfileFeatureDefinition.CreateShaderFeatureDropdown(
            new [] { ProfileFeatureKeys.NoiseCloudFeature, ProfileFeatureKeys.CubemapCloudFeature, ProfileFeatureKeys.CubemapNormalCloudFeature },
            new [] { ShaderKeywords.NoiseClouds, ShaderKeywords.CubemapClouds, ShaderKeywords.CubemapNormalClouds },
            new [] { "Standard Clouds", "Cubemap Clouds", "Cubemap Lit Clouds" },
            0, "Cloud Type", ProfileFeatureKeys.CloudFeature, true,
            "Use the standard soft clouds, or use a fully art customizable cubemap for clouds."),

          // Texture formats for cubemap clouds.
          ProfileFeatureDefinition.CreateShaderFeatureDropdown(
            new [] { ProfileFeatureKeys.CubemapCloudTextureFormatRGBFeature, ProfileFeatureKeys.CubemapCloudTextureFormatRGBAFeature },
            new [] { ShaderKeywords.CubemapCloudTextureFormatRGB, ShaderKeywords.CubemapCloudTextureFormatRGBA },
            new [] { "RGB - Additive texture", "RGBA - Alpha texture" },
            1, "Cloud Texture Format", ProfileFeatureKeys.CubemapCloudFeature, true,
            "Texture format so the shader knows how to load and blend the clouds into the background."),
          
          // Double layer Cubemap.
          ProfileFeatureDefinition.CreateShaderFeature(
            ProfileFeatureKeys.CubemapCloudDoubleLayerFeature, ShaderKeywords.CubemapCloudDoubleLayer, false, "Double Layer Clouds", ProfileFeatureKeys.CubemapCloudFeature, true, 
            "If enabled, the skybox will render 2 layers of clouds which gives a sense of relative cloud motion in the sky."),

          ProfileFeatureDefinition.CreateShaderFeature(
            ProfileFeatureKeys.CubemapCloudDoubleLayerCubemapFeature, ShaderKeywords.CubemapCloudDoubleLayerCustomTexture, false, "Double Layer Uses Custom Cubemap", 
            ProfileFeatureKeys.CubemapCloudDoubleLayerFeature, true,
            "If enabled, you can specify a custom texture to be used as the double layer. If disabled the same cloud texture will be used at a different rotation offset"
          ),

          // Double layer Cubemap Normals.
          ProfileFeatureDefinition.CreateShaderFeature(
            ProfileFeatureKeys.CubemapNormalCloudDoubleLayerFeature, ShaderKeywords.CubemapNormalCloudDoubleLayer, false, "Double Layer Clouds", ProfileFeatureKeys.CubemapNormalCloudFeature, true,
            "If enabled, the skybox will render 2 layers of clouds which gives a sense of relative cloud motion in the sky."),
        }),
        new ProfileFeatureSection("Star Layer 3", ProfileSectionKeys.Star3SectionKey, new []
        {
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer3CustomTextureFeature, ShaderKeywords.StarLayer3CustomTexture, false, "Use Custom Texture", null, false,
            "Enables a layer of stars in the shader. Use less star layers for better performance."),
          ProfileFeatureDefinition.CreateShaderFeature(ProfileFeatureKeys.StarLayer3SpriteSheetFeature, ShaderKeywords.StarLayer3SpriteSheet, false, "Texture Is Sprite Sheet Animation", ProfileFeatureKeys.StarLayer3CustomTextureFeature, true,
            "If enabled star texture will be used as a sprite sheet animation."),
        }),
        new ProfileFeatureSection("Rain", ProfileSectionKeys.RainSectionKey, new [] {
          ProfileFeatureDefinition.CreateBooleanFeature(ProfileFeatureKeys.RainSoundFeature, true, "Rain Sound",  null, false,
          "Plays sound clip of rain in a loop.")
        }),
        new ProfileFeatureSection("Lightning", ProfileSectionKeys.LightningSectionKey, new [] {
          ProfileFeatureDefinition.CreateBooleanFeature(ProfileFeatureKeys.ThunderFeature, true, "Thunder Sounds", ProfileFeatureKeys.LightningFeature, true,
          "If enabled thunder sound effects will happen when lightning bolts strike.")
        })
      };
    }

    // Override this to return a different set of shader options.
    protected override ProfileGroupSection[] ProfileDefinitionTable()
    {
      return new[] {
        // Sky Section.
        new ProfileGroupSection("Sky", ProfileSectionKeys.SkySectionKey, "SkySectionIcon", null, false, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Sky Cubemap", ProfilePropertyKeys.SkyCubemapKey, null, ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.GradientSkyFeature, false,
            "Image used as background for the skybox."),

          ProfileGroupDefinition.ColorGroupDefinition(
            "Sky Upper Color", ProfilePropertyKeys.SkyUpperColorKey, ColorHelper.ColorWithHex(0x2C2260),
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.GradientSkyFeature, true,
            "Top color of the sky when using a gradient background."),

          ProfileGroupDefinition.ColorGroupDefinition(
            "Sky Middle Color", ProfilePropertyKeys.SkyMiddleColorKey, Color.white,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.GradientSkyFeature, true,
            "Middle color of the sky when using the gradient background."),

          ProfileGroupDefinition.ColorGroupDefinition(
            "Sky Lower Color", ProfilePropertyKeys.SkyLowerColorKey, ColorHelper.ColorWithHex(0xE3C882),
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.GradientSkyFeature, true,
            "Bottom color of the sky when using a gradient background."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Sky Middle Color Balance", ProfilePropertyKeys.SkyMiddleColorPositionKey, 0, 1, .5f,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.GradientSkyFeature, true,
            "Shift the middle color closer to lower color or closer upper color to alter the gradient balance."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Horizon Position", ProfilePropertyKeys.HorizonTrasitionStartKey, -1, 1, -.3f,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.GradientSkyFeature, true,
            "This vertical position controls where the gradient background will begin."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Sky Gradient Length", ProfilePropertyKeys.HorizonTransitionLengthKey, 0, 2, 1,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.GradientSkyFeature, true,
            "The length of the background gradient fade from the bottom color to the top color."),

          ProfileGroupDefinition.NumberGroupDefinition("Star Start", ProfilePropertyKeys.StarTransitionStartKey, -1, 1, .2f,
            "Vertical position where the stars will begin fading in from. Typically this is just above the horizon."),

          ProfileGroupDefinition.NumberGroupDefinition("Star Transition Length", ProfilePropertyKeys.StarTransitionLengthKey, 0, 2, .5f,
            "The length of the fade-in where stars go from invisible to visible."),

          ProfileGroupDefinition.NumberGroupDefinition("Star Distance Scale", ProfilePropertyKeys.HorizonStarScaleKey, .01f, 1, .7f,
            "Scale value applied to stars closers to the horizon for distance effect."),
        }),

        // Sun section.
        new ProfileGroupSection("Sun", ProfileSectionKeys.SunSectionKey, "SunSectionIcon", ProfileFeatureKeys.SunFeature, true, new[]
        {
          ProfileGroupDefinition.ColorGroupDefinition("Sun Color", ProfilePropertyKeys.SunColorKey, ColorHelper.ColorWithHex(0xFFE000),
            "Color of the sun."),

          ProfileGroupDefinition.TextureGroupDefinition("Sun Texture", ProfilePropertyKeys.SunTextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.SunCustomTextureFeature, true,
            "Texture used for the sun."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Sprite Columns", ProfilePropertyKeys.SunSpriteColumnCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.SunSpriteSheetFeature, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Sprite Rows", ProfilePropertyKeys.SunSpriteRowCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.SunSpriteSheetFeature, true,
            "Number of rows in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Sprite Item Count", ProfilePropertyKeys.SunSpriteItemCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.SunSpriteSheetFeature, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Sprite Animation Speed", ProfilePropertyKeys.SunSpriteAnimationSpeedKey,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.SunSpriteSheetFeature, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Rotation Speed", ProfilePropertyKeys.SunRotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 1,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.SunRotationFeature, true,
            "Speed value for sun texture rotation animation."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Size", ProfilePropertyKeys.SunSizeKey, 0, 1, .1f,
            "Size of the sun."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Edge Feathering", ProfilePropertyKeys.SunEdgeFeatheringKey, MinEdgeFeathering, 1, .8f,
            "Percent amount of gradient fade-in from the sun edges to it's center point."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Bloom Intensity", ProfilePropertyKeys.SunColorIntensityKey, 1, MaxHDRValue, 1,
            "Value that's multiplied against the suns color to intensify bloom effects."),

          ProfileGroupDefinition.ColorGroupDefinition("Sun Light Color", ProfilePropertyKeys.SunLightColorKey, Color.white,
            "Color of the directional light coming from the sun."),

          ProfileGroupDefinition.NumberGroupDefinition("Sun Light Intensity", ProfilePropertyKeys.SunLightIntensityKey, 0, 5, 1,
            "Intensity of the directional light coming from the sun."),

          ProfileGroupDefinition.SpherePointGroupDefinition("Sun Position", ProfilePropertyKeys.SunPositionKey, 0, 0,
            "Position of the sun in the skybox expressed as a horizontal and vertical rotation.")
        }),

        // Moon section.
        new ProfileGroupSection("Moon", ProfileSectionKeys.MoonSectionKey, "MoonSectionIcon", ProfileFeatureKeys.MoonFeature, true, new[]
        {
          ProfileGroupDefinition.ColorGroupDefinition("Moon Color", ProfilePropertyKeys.MoonColorKey, ColorHelper.ColorWithHex(0x989898),
            "Color of the moon."),

          ProfileGroupDefinition.TextureGroupDefinition("Moon Texture", ProfilePropertyKeys.MoonTextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.MoonCustomTextureFeature, true,
            "Texture used for the moon"),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Sprite Columns", ProfilePropertyKeys.MoonSpriteColumnCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.MoonSpriteSheetFeature, true,
            "Number of columns in the moon sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Sprite Rows", ProfilePropertyKeys.MoonSpriteRowCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.MoonSpriteSheetFeature, true,
            "Number of rows in the moon sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Sprite Item Count", ProfilePropertyKeys.MoonSpriteItemCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.MoonSpriteSheetFeature, true,
            "Number of columns in the moon sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Sprite Animation Speed", ProfilePropertyKeys.MoonSpriteAnimationSpeedKey,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.MoonSpriteSheetFeature, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Rotation Speed", ProfilePropertyKeys.MoonRotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 1,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.MoonRotationFeature, true,
            "Speed value for moon texture rotation animation."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Size", ProfilePropertyKeys.MoonSizeKey, 0, 1, .08f,
            "Size of the moon."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Edge Feathering", ProfilePropertyKeys.MoonEdgeFeatheringKey, MinEdgeFeathering, 1, .1f,
            "Percentage of fade-in from edge to the center of the moon."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Bloom Intensity", ProfilePropertyKeys.MoonColorIntensityKey, 1, MaxHDRValue, 1,
            "Value multiplied with the moon color to help intensify bloom filters."),

          ProfileGroupDefinition.ColorGroupDefinition("Moon Light Color", ProfilePropertyKeys.MoonLightColorKey, Color.white,
            "Color of the directional light coming from the moon."),

          ProfileGroupDefinition.NumberGroupDefinition("Moon Light Intensity", ProfilePropertyKeys.MoonLightIntensityKey, 0, 5, 1,
            "Intensity of the directional light coming from the moon."),

          ProfileGroupDefinition.SpherePointGroupDefinition("Moon Position", ProfilePropertyKeys.MoonPositionKey, 0, 0,
            "Position of the moon in the skybox expressed as a horizontal and vertical rotation.")
        }),

        // Clouds.
        new ProfileGroupSection("Clouds", ProfileSectionKeys.CloudSectionKey, "CloudSectionIcon", ProfileFeatureKeys.CloudFeature, true, new[]
        {
          // Cubemap clouds.
          ProfileGroupDefinition.TextureGroupDefinition("Cloud Cubemap", ProfilePropertyKeys.CloudCubemapTextureKey, null, ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.CubemapCloudFeature, true,
            "Cubemap texture with clouds to render"),
          
          ProfileGroupDefinition.NumberGroupDefinition("Cloud Cubemap Rotation Speed", ProfilePropertyKeys.CloudCubemapRotationSpeedKey, -MaxCloudRotationSpeed, MaxCloudRotationSpeed, .1f, ProfileFeatureKeys.CubemapCloudFeature, true,
            "Speed and direction to rotate the cloud cubemap in."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Cubemap Height", ProfilePropertyKeys.CloudCubemapHeightKey, -1.0f, 1.0f, 0, ProfileFeatureKeys.CubemapCloudFeature, true,
            "Adjust the horizon height of the cloud cubemap."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Cubemap Tint Color", ProfilePropertyKeys.CloudCubemapTintColorKey, Color.white, ProfileFeatureKeys.CubemapCloudFeature, true,
            "Tint color that will be multiplied against the cubemap texture color."),

          ProfileGroupDefinition.TextureGroupDefinition("Cloud Cubemap Double Layer Cubemap", ProfilePropertyKeys.CloudCubemapDoubleLayerCustomTextureKey, null, ProfileFeatureKeys.CubemapCloudDoubleLayerCubemapFeature, true,
            "Cubemap texture to use as the atmosphere layer for the clouds."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Cubemap Double Layer Rotation Speed", ProfilePropertyKeys.CloudCubemapDoubleLayerRotationSpeedKey, -MaxCloudRotationSpeed, MaxCloudRotationSpeed, .15f, ProfileFeatureKeys.CubemapCloudDoubleLayerFeature, true,
            "Speed and direction to rotate the second layer of clouds."),
 
          ProfileGroupDefinition.NumberGroupDefinition("Cloud Cubemap Double Layer Height", ProfilePropertyKeys.CloudCubemapDoubleLayerHeightKey, -1.0f, 1.0f, 0, ProfileFeatureKeys.CubemapCloudDoubleLayerFeature, true,
            "Adjust the horizon height of the second cloud cubemap layer."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Cubemap Double Layer Tint Color", ProfilePropertyKeys.CloudCubemapDoubleLayerTintColorKey, Color.white, ProfileFeatureKeys.CubemapCloudDoubleLayerFeature, true,
            "Tint color that will be multiplied against the double texture cubemap."),

          // Cubemap normal clouds.
          ProfileGroupDefinition.TextureGroupDefinition("Cloud Lit Cubemap", ProfilePropertyKeys.CloudCubemapNormalTextureKey, null, ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.CubemapNormalCloudFeature, true,
            "Cubemap texture with clouds to render"),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Lit - Light Color", ProfilePropertyKeys.CloudCubemapNormalLitColorKey, Color.white, ProfileFeatureKeys.CubemapNormalCloudFeature, true, 
            "Color of the clouds when directly illuminated by a lighting. Typically the top side of the cloud."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Lit - Shadow Color", ProfilePropertyKeys.CloudCubemapNormalShadowKey, Color.gray, ProfileFeatureKeys.CubemapNormalCloudFeature, true,
            "Color of the clouds when they are in a shadow. Typically the bottom side of the cloud."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Lit - Rotation Speed", ProfilePropertyKeys.CloudCubemapNormalRotationSpeedKey, -MaxCloudRotationSpeed, MaxCloudRotationSpeed, .1f, ProfileFeatureKeys.CubemapNormalCloudFeature, true,
            "Speed and direction to rotate the cloud cubemap in."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Lit - Height", ProfilePropertyKeys.CloudCubemapNormalHeightKey, -1.0f, 1.0f, 0, ProfileFeatureKeys.CubemapNormalCloudFeature, true,
            "Adjust the horizon height of the cloud cubemap."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Lit - Ambient Intensity", ProfilePropertyKeys.CloudCubemapNormalAmbientIntensity, 0.0f, 1.0f, .1f, ProfileFeatureKeys.CubemapNormalCloudFeature, true,
            "Ambient light intensity used for minimum brightness in cloud lighting calculations."),

          ProfileGroupDefinition.TextureGroupDefinition("Cloud Lit - Double Layer Cubemap", ProfilePropertyKeys.CloudCubemapNormalDoubleLayerCustomTextureKey, null, ProfileFeatureKeys.CubemapNormalCloudDoubleLayerCubemapFeature, true,
            "Cubemap texture to use as the atmosphere layer for the clouds."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Lit - Double Layer Lit Color", ProfilePropertyKeys.CloudCubemapNormalDoubleLayerLitColorKey, Color.white, ProfileFeatureKeys.CubemapNormalCloudDoubleLayerFeature, true, 
            "Color of the clouds when directly illuminated by a lighting. Typically the top side of the cloud."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Lit - Double Layer Shadow Color", ProfilePropertyKeys.CloudCubemapNormalDoubleLayerShadowKey, Color.gray, ProfileFeatureKeys.CubemapNormalCloudDoubleLayerFeature, true,
            "Color of the clouds when they are in a shadow. Typically the bottom side of the cloud."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Lit - Double Layer Rotation Speed", ProfilePropertyKeys.CloudCubemapNormalDoubleLayerRotationSpeedKey, -MaxCloudRotationSpeed, MaxCloudRotationSpeed, .15f, ProfileFeatureKeys.CubemapNormalCloudDoubleLayerFeature, true,
            "Speed and direction to rotate the second layer of clouds."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Lit - Double Layer Height", ProfilePropertyKeys.CloudCubemapNormalDoubleLayerHeightKey, -1.0f, 1.0f, 0, ProfileFeatureKeys.CubemapNormalCloudDoubleLayerFeature, true,
            "Adjust the horizon height of the second cloud cubemap layer."),

          // Standard Cloud Options.
          ProfileGroupDefinition.NumberGroupDefinition("Cloud Density", ProfilePropertyKeys.CloudDensityKey, 0, 1, .25f, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Density controls the amount of clouds in the scene."),

          ProfileGroupDefinition.TextureGroupDefinition("Cloud Noise Texture", ProfilePropertyKeys.CloudNoiseTextureKey, null, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Noise pattern used to generate the standard procedural clouds from. You can generate your own RGB noise textures with patterns to customize the visual style using photoshop and filters."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Texture Tiling", ProfilePropertyKeys.CloudTextureTiling, .1f, 20.0f, .55f, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Tiling changes the scale of the texture and how many times it will repeat in the sky. A higher number will increase visible resolution."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Speed", ProfilePropertyKeys.CloudSpeedKey, 0, 1, .1f, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Speed that the clouds move at as a percent from 0 to 1."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Direction", ProfilePropertyKeys.CloudDirectionKey, 0, Mathf.PI * 2, 1, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Direction that the clouds move in as an angle in radians between 0 and 2PI."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Height", ProfilePropertyKeys.CloudHeightKey, 0, 1, .7f, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Height (or altitude) of the clouds in the scene."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Color 1", ProfilePropertyKeys.CloudColor1Key, Color.white, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Primary color of the cloud features."),

          ProfileGroupDefinition.ColorGroupDefinition("Cloud Color 2", ProfilePropertyKeys.CloudColor2Key, Color.gray, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Secondary color of the clouds between the primary features."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Fade-Out Distance", ProfilePropertyKeys.CloudFadePositionKey, 0, 1, .7f, ProfileFeatureKeys.NoiseCloudFeature, true,
            "Distance at which the clouds will begin to fade away towards the horizon."),

          ProfileGroupDefinition.NumberGroupDefinition("Cloud Fade-Out Amount", ProfilePropertyKeys.CloudFadeAmountKey, 0, 1, .75f, ProfileFeatureKeys.NoiseCloudFeature, true,
            "This is the amount of fade out that will happen to clouds as they reach the horizon."),
        }),

        // Fog.
        new ProfileGroupSection("Fog", ProfileSectionKeys.FogSectionKey, "FogSectionIcon", ProfileFeatureKeys.FogFeature, true, new []
        {
          ProfileGroupDefinition.ColorGroupDefinition("Fog Color", ProfilePropertyKeys.FogColorKey, Color.white,
            "Color of the fog at the horizon."),

          ProfileGroupDefinition.NumberGroupDefinition("Fog Density", ProfilePropertyKeys.FogDensityKey, 0, 1, .12f,
            "Density, or thickness, of the fog to display at the horizon."),

          ProfileGroupDefinition.NumberGroupDefinition("Fog Height", ProfilePropertyKeys.FogLengthKey, .03f, 1, .1f,
            "The height of the fog as it extends from the horizon upwards into the sky"),

          ProfileGroupDefinition.BoolGroupDefinition("Update Global Fog Color", ProfilePropertyKeys.FogSyncWithGlobal, true, null, false,
            "Enable this to update the color of global fog, used by the rest of Unity, to match the current sky fog color from Sky Studio.")
        }),

        // Stars Basic.
        new ProfileGroupSection("Stars (Mobile)", ProfileSectionKeys.StarsBasicSectionKey, "StarSectionIcon", ProfileFeatureKeys.StarBasicFeature, true, new[] 
        {
            ProfileGroupDefinition.TextureGroupDefinition("Star Cubemap", ProfilePropertyKeys.StarBasicCubemapKey, null,
              ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.StarBasicFeature, true,
              "Cubemap image for basic stars on mobile."),
            
            ProfileGroupDefinition.NumberGroupDefinition("Star Twinkle Speed", ProfilePropertyKeys.StarBasicTwinkleSpeedKey, 0.0f, 10.0f, 1.0f,
              ProfileFeatureKeys.StarBasicFeature, true,
              "Speed at which the stars will twinkle at."),

            ProfileGroupDefinition.NumberGroupDefinition("Star Twinkle Amount", ProfilePropertyKeys.StarBasicTwinkleAmountKey, 0.0f, 1.0f, .75f,
              ProfileFeatureKeys.StarBasicFeature, true,
              "The amount of fade out that happens when stars twinkle."),
            
            ProfileGroupDefinition.NumberGroupDefinition("Star Opacity", ProfilePropertyKeys.StarBasicOpacityKey, 0.0f, 1.0f, 1.0f,
              ProfileFeatureKeys.StarBasicFeature, true,
              "Determines if stars are visible or not. Animate this property on the Sky Timeline to fade stars in/out in day-night cycles."),

            ProfileGroupDefinition.ColorGroupDefinition("Star Tint Color", ProfilePropertyKeys.StarBasicTintColorKey, Color.white, 
              ProfileFeatureKeys.StarBasicFeature, true,
              "Tint color of the stars."),

            ProfileGroupDefinition.NumberGroupDefinition("Star Smoothing Exponent", ProfilePropertyKeys.StarBasicExponentKey, .5f, 5.0f, 1.1f,
              "Exponent used to smooth out the edges of the star texture."),

            ProfileGroupDefinition.NumberGroupDefinition("Star Bloom Intensity (HDR)", ProfilePropertyKeys.StarBasicIntensityKey, 1.0f, MaxHDRValue, 1.0f,
              "Intensity boost that's multiplied against the stars to help them glow with HDR bloom filters.")
        }),

        // Star 1 section.
        new ProfileGroupSection("Star Layer 1", ProfileSectionKeys.Star1SectionKey, "StarSectionIcon", ProfileFeatureKeys.StarLayer1Feature, true, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Star 1 Texture", ProfilePropertyKeys.Star1TextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.StarLayer1CustomTextureFeature, true,
            "Texture used for the star image."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Sprite Columns", ProfilePropertyKeys.Star1SpriteColumnCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer1SpriteSheetFeature, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Sprite Rows", ProfilePropertyKeys.Star1SpriteRowCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer1SpriteSheetFeature, true,
            "Number of rows in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Sprite Item Count", ProfilePropertyKeys.Star1SpriteItemCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer1SpriteSheetFeature, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Sprite Animation Speed", ProfilePropertyKeys.Star1SpriteAnimationSpeedKey,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer1SpriteSheetFeature, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Star 1 Rotation Speed", ProfilePropertyKeys.Star1RotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 0,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.StarLayer1CustomTextureFeature, true,
            "Speed and direction the star rotates at."),

          ProfileGroupDefinition.ColorGroupDefinition("Star 1 Color", ProfilePropertyKeys.Star1ColorKey, Color.white,
            "Color of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Size", ProfilePropertyKeys.Star1SizeKey, 0, MaxStarSize, .005f,
            "Size of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Density", ProfilePropertyKeys.Star1DensityKey, 0, MaxStarDensity, .02f,
            ProfileGroupDefinition.RebuildType.Stars, null, false,
            "Density of the stars in this layer."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Twinkle Amount", ProfilePropertyKeys.Star1TwinkleAmountKey, 0, 1, .7f,
            "Percentage amount of twinkle animation."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Twinkle Speed", ProfilePropertyKeys.Star1TwinkleSpeedKey, 0, 10, 5,
            "Speed at which the star twinkles at."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Edge Feathering", ProfilePropertyKeys.Star1EdgeFeatheringKey, 0.0001f, 1, .4f,
            "Percentage of fade-in from the stars edges to the center."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 1 Bloom Intensity", ProfilePropertyKeys.Star1ColorIntensityKey, 1, MaxHDRValue, 1,
            "Value multiplied with the star color to intensify bloom filters."),
        }),

        // Star 2 section.
        new ProfileGroupSection("Star Layer 2", ProfileSectionKeys.Star2SectionKey, "StarSectionIcon", ProfileFeatureKeys.StarLayer2Feature, true, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Star 2 Texture", ProfilePropertyKeys.Star2TextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.StarLayer2CustomTextureFeature, true,
            "Texture used for the star image."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Sprite Columns", ProfilePropertyKeys.Star2SpriteColumnCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer2SpriteSheetFeature, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Sprite Rows", ProfilePropertyKeys.Star2SpriteRowCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer2SpriteSheetFeature, true,
            "Number of rows in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Sprite Item Count", ProfilePropertyKeys.Star2SpriteItemCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer2SpriteSheetFeature, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Sprite Animation Speed", ProfilePropertyKeys.Star2SpriteAnimationSpeedKey,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer2SpriteSheetFeature, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Star 2 Rotation Speed", ProfilePropertyKeys.Star2RotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 0,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.StarLayer2CustomTextureFeature, true,
            "Speed and direction the star rotates at."),

          ProfileGroupDefinition.ColorGroupDefinition("Star 2 Color", ProfilePropertyKeys.Star2ColorKey, Color.white,
            "Color of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Size", ProfilePropertyKeys.Star2SizeKey, 0, MaxStarSize, .005f,
            "Size of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Density", ProfilePropertyKeys.Star2DensityKey, 0, MaxStarDensity, .01f,
            ProfileGroupDefinition.RebuildType.Stars, null, false,
            "Density of the stars in this layer."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Twinkle Amount", ProfilePropertyKeys.Star2TwinkleAmountKey, 0, 1, .7f,
            "Texture used for the star image."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Twinkle Speed", ProfilePropertyKeys.Star2TwinkleSpeedKey, 0, 10, 5,
            "Speed at which the star twinkles at."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Edge Feathering", ProfilePropertyKeys.Star2EdgeFeatheringKey, MinEdgeFeathering, 1, .4f,
            "Percentage of fade-in from the stars edges to the center."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 2 Bloom Intensity", ProfilePropertyKeys.Star2ColorIntensityKey, 1, MaxHDRValue, 1,
            "Value multiplied with the star color to intensify bloom filters."),
        }),

        // Star 3 section.
        new ProfileGroupSection("Star Layer 3", ProfileSectionKeys.Star3SectionKey, "StarSectionIcon", ProfileFeatureKeys.StarLayer3Feature, true, new[]
        {
          ProfileGroupDefinition.TextureGroupDefinition(
            "Star 3 Texture", ProfilePropertyKeys.Star3TextureKey, null,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.StarLayer3CustomTextureFeature, true,
            "Texture used for the star image."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Sprite Columns", ProfilePropertyKeys.Star3SpriteColumnCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer3SpriteSheetFeature, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Sprite Rows", ProfilePropertyKeys.Star3SpriteRowCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer3SpriteSheetFeature, true,
            "Number of rows in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Sprite Item Count", ProfilePropertyKeys.Star3SpriteItemCountKey,
            1, MaxSpriteItems, 1, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer3SpriteSheetFeature, true,
            "Number of columns in the sprite sheet."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Sprite Animation Speed", ProfilePropertyKeys.Star3SpriteAnimationSpeedKey,
            0.0f, 90.0f, 15.0f, ProfileGroupDefinition.RebuildType.None, ProfileGroupDefinition.FormatStyle.Integer, ProfileFeatureKeys.StarLayer3SpriteSheetFeature, true,
            "Frames per second to flip through the sprite images."),

          ProfileGroupDefinition.NumberGroupDefinition(
            "Star 3 Rotation Speed", ProfilePropertyKeys.Star3RotationSpeedKey, MinRotationSpeed, MaxRotationSpeed, 0,
            ProfileGroupDefinition.RebuildType.None, ProfileFeatureKeys.StarLayer3CustomTextureFeature, true,
            "Speed and direction the star rotates at."),

          ProfileGroupDefinition.ColorGroupDefinition("Star 3 Color", ProfilePropertyKeys.Star3ColorKey, Color.white,
            "Color of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Size", ProfilePropertyKeys.Star3SizeKey, 0, MaxStarSize, .005f,
            "Size of the star."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Density", ProfilePropertyKeys.Star3DensityKey, 0, MaxStarDensity, .01f,
            ProfileGroupDefinition.RebuildType.Stars, null, false,
            "Density of the stars in this layer."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Twinkle Amount", ProfilePropertyKeys.Star3TwinkleAmountKey, 0, 1, .7f,
            "Speed at which the star twinkles at."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Twinkle Speed", ProfilePropertyKeys.Star3TwinkleSpeedKey, 0, 10, 5,
            "Speed at which the star twinkles at."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Edge Feathering", ProfilePropertyKeys.Star3EdgeFeatheringKey, MinEdgeFeathering, 1, .4f,
            "Percentage of fade-in from the stars edges to the center."),

          ProfileGroupDefinition.NumberGroupDefinition("Star 3 Bloom Intensity", ProfilePropertyKeys.Star3ColorIntensityKey, 1, MaxHDRValue, 1,
            "Value multiplied with the star color to intensify bloom filters."),
        }),
        
        // Rain section.
        new ProfileGroupSection("Rain", ProfileSectionKeys.RainSectionKey, "RainSectionIcon", ProfileFeatureKeys.RainFeature, true, new ProfileGroupDefinition[] {
          ProfileGroupDefinition.NumberGroupDefinition("Rain Sound Volume", ProfilePropertyKeys.RainSoundVolumeKey, 0.0f, 1.0f, .5f, ProfileFeatureKeys.RainSoundFeature, true,
            "Set the volume of the rain effects."),
          ProfileGroupDefinition.TextureGroupDefinition("Rain Near Texture", ProfilePropertyKeys.RainNearTextureKey, null,
            "The rain texture used for animating rain drizzle in the near foreground."),
          ProfileGroupDefinition.NumberGroupDefinition("Rain Near Texture Tiling", ProfilePropertyKeys.RainNearTextureTiling, .01f, 20.0f, 1.0f,
            "Set the tiling amount for the near texture"),
          ProfileGroupDefinition.NumberGroupDefinition("Rain Near Visibility", ProfilePropertyKeys.RainNearIntensityKey, 0.0f, 1.0f, .3f,
            "Set the alpha visibility of the rain downfall effect of the near texture."),
          ProfileGroupDefinition.NumberGroupDefinition("Rain Near Speed", ProfilePropertyKeys.RainNearSpeedKey, 0.0f, 5.0f, 2.5f,
            "Set the speed of the near rain texture."),

          ProfileGroupDefinition.TextureGroupDefinition("Rain Far Texture", ProfilePropertyKeys.RainFarTextureKey, null,
            "The rain texture used for animating rain drizzle in the far background."),
          ProfileGroupDefinition.NumberGroupDefinition("Rain Far Texture Tiling", ProfilePropertyKeys.RainFarTextureTiling, .01f, 20.0f, 1.0f,
            "Set the tiling amount for the far texture"),
          ProfileGroupDefinition.NumberGroupDefinition("Rain Far Visibility", ProfilePropertyKeys.RainFarIntensityKey, 0.0f, 1.0f, .15f,
            "Set the alpha visibility of the rain downfall effect of the far texture."),
          ProfileGroupDefinition.NumberGroupDefinition("Rain Far Speed", ProfilePropertyKeys.RainFarSpeedKey, 0.0f, 5.0f, 2.0f,
            "Set the speed of the far rain texture."),

          ProfileGroupDefinition.ColorGroupDefinition("Rain Tint Color", ProfilePropertyKeys.RainTintColorKey, Color.white,
            "Tint color that will be multilied against the rain texture color."),
          ProfileGroupDefinition.NumberGroupDefinition("Rain Turbulence", ProfilePropertyKeys.RainWindTurbulence, 0.0f, 1.0f, .2f,
            "Set the amount of jitter in the rain texture animation."),
          ProfileGroupDefinition.NumberGroupDefinition("Rain Turbulence Speed", ProfilePropertyKeys.RainWindTurbulenceSpeed, 0.0f, 2.0f, .5f,
            "Set the speed at which the jitter animation is applied to the rain texture"),
        }),

        // Rain surface splashes.
        new ProfileGroupSection("Rain Surface Splashes", ProfileSectionKeys.RainSplashSectionKey, "RainSplashSectionIcon", ProfileFeatureKeys.RainSplashFeature, true, new ProfileGroupDefinition[] {
          ProfileGroupDefinition.NumberGroupDefinition("Splash Count", ProfilePropertyKeys.RainSplashMaxConcurrentKey, 0, 1000, 400,
            "Number of raindrop splashes that will be placed into the scene."),
          ProfileGroupDefinition.NumberGroupDefinition("Splash Area Start", ProfilePropertyKeys.RainSplashAreaStartKey, 0, 50.0f, 1.5f,
            "The closest distance from the main camera where rain splashes can be spawned at."),
          ProfileGroupDefinition.NumberGroupDefinition("Splash Area Length", ProfilePropertyKeys.RainSplashAreaLengthKey, 0, 40.0f, 5.5f,
            "The length of the area that rain splashes can happen at, which starts from the Splash Area Offset."),
          ProfileGroupDefinition.NumberGroupDefinition("Splash Scale", ProfilePropertyKeys.RainSplashScaleKey, 0.001f, 60.0f, 2.50f,
            "The size of randrop splashes."),
          ProfileGroupDefinition.NumberGroupDefinition("Splash Scale Varience", ProfilePropertyKeys.RainSplashScaleVarienceKey, 0, 1, .25f,
            "The amount of variety in the initial size of rain splashes so they all don't look exactly the same."),
          ProfileGroupDefinition.NumberGroupDefinition("Splash Intensity", ProfilePropertyKeys.RainSplashIntensityKey, 0.0f, 1.0f, .75f,
            "Lower value makes rain splashes less visible (more transparent), and higher values make splashes more visible."),
          ProfileGroupDefinition.NumberGroupDefinition("Splash Surfce Offset", ProfilePropertyKeys.RainSplashSurfaceOffsetKey, 0.0f, 1.0f, .15f,
            "Offset from the collision surface normal to keep rain splashes above the object they hit."),
          ProfileGroupDefinition.ColorGroupDefinition("Splash Tint Color", ProfilePropertyKeys.RainSplashTintColorKey, Color.white,
            "Tint color that will be multilied against rain splash texture."),
        }),

        // Lightning section.
        new ProfileGroupSection("Lightning", ProfileSectionKeys.LightningSectionKey, "LightningSectionIcon", ProfileFeatureKeys.LightningFeature, true, new ProfileGroupDefinition[] {
          ProfileGroupDefinition.NumberGroupDefinition("Thunder Sound Volume", ProfilePropertyKeys.ThunderSoundVolumeKey, 0.0f, 1.0f, .35f, ProfileFeatureKeys.ThunderFeature, true,
            "Set the volume of the rain effects."),
          ProfileGroupDefinition.NumberGroupDefinition("Thunder Sound Delay", ProfilePropertyKeys.ThunderSoundDelayKey, 0.0f, 20.0f, .4f, ProfileFeatureKeys.ThunderFeature, true,
            "The amout of time between the lightnig bolt being created and the sound effect playing. Increase to create feeling of distance from lightning."),
          ProfileGroupDefinition.NumberGroupDefinition("Lightning Probability", ProfilePropertyKeys.LightningProbabilityKey, 0.0f, 1.0f, .2f,
            "The probability determines how many lightning bolts will be spawned in the scene."),
          ProfileGroupDefinition.NumberGroupDefinition("Lightning Cool Down", ProfilePropertyKeys.LightningStrikeCoolDown, 0.0f, 30.0f, 2.0f, 
            "The amount of time that's required to pass after a lighting bolt strike, before another lighting bolt is allowed to be spawned."),
          ProfileGroupDefinition.NumberGroupDefinition("Lightning Intensity", ProfilePropertyKeys.LightningIntensityKey, 0.0f, 1.0f,  1.0f,
            "Alpha intensity for the lightnight bolt aniamtion, less intense will create a more transparent and light look."),
          ProfileGroupDefinition.ColorGroupDefinition("Lightning Tint Color", ProfilePropertyKeys.LightningTintColorKey, Color.white,
            "Tint color that will be multilied against the lightning texture color."),
        })
      };
    }
  }
}
