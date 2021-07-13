// Copyright(c) 2017 Funly LLC
//
// Author: Jason Ederle
// Description: Renders a customizable sky for a 3D skybox sphere.
// Contact: jason@funly.io

Shader "Funly/Sky Studio/Skybox/3D Standard" {
  Properties {
    // Gradient Sky.
    _GradientSkyUpperColor("Sky Top Color", Color) = (.47, .45, .75, 1)            // Color of sky.
    _GradientSkyMiddleColor("Sky Middle Color", Color) = (1, 1, 1, 1)              // Color in the middle of sky 3 way gradient.
    _GradientSkyLowerColor("Sky Lower Color", Color) = (.7, .53, .69, 1)           // Color of horizon.
    _GradientFadeBegin("Horizon Fade Begin", Range(-1, 1)) = -.179                 // Position to begin horizon fade into sky.
    _GradientFadeEnd("Horizon Fade End", Range(-1, 1)) = .302                      // Position to end horizon fade into sky.
    _GradientFadeMiddlePosition("Horizon Fade Middle Position", Range(0, 1)) = .5  // Position of the middle gradient color.

    // Shrink stars closer to horizon.
    _HorizonScaleFactor("Star Horizon Scale Factor", Range(0, 1)) = .7

    // Cubemap background.
    [NoScaleOffset]_MainTex("Background Cubemap", CUBE) = "white" {}      // Cubemap for custom background behind stars.

    // Star fading.
    _StarFadeBegin("Star Fade Begin", Range(-1, 1)) = .067                // Height to begin star fade in.
    _StarFadeEnd("Star Fade End", Range(-1, 1)) = .36                     // Height where all stars are faded in at.
    
    // Star (Basic).
    [NoScaleOffset]_StarBasicCubemap("Star Basic Cubemap", CUBE) = "black" {}
    _StarBasicTwinkleSpeed("Star Basic Twinkle Speed", Range(0, 10)) = 5
    _StarBasicTwinkleAmount("Star Basic Twinkle Amount", Range(0, 1)) = .75
    _StarBasicOpacity("Star Basic Opacity", Range(0, 1)) = 1
    _StarBasicTintColor("Star Basic Tint Color", Color) = (1, 1, 1, 1)
    _StarBasicExponent("Star Basic Exponent", Range(.5, 5)) = 1.2

    // Star Layer 1.
    [NoScaleOffset]_StarLayer1Tex("Star 1 Texture", 2D) = "white" {}
    _StarLayer1Color("Star Layer 1 - Color", Color) = (1, 1, 1, 1)                              // Color tint for stars.
    _StarLayer1Density("Star Layer 1 - Star Density", Range(0, .05)) = .01                      // Space between stars.
    _StarLayer1MaxRadius("Star Layer 1 - Star Size", Range(0, .1)) = .007                       // Max radius of stars.
    _StarLayer1TwinkleAmount("Star Layer 1 - Twinkle Amount", Range(0, 1)) = .775               // Percent of star twinkle amount.
    _StarLayer1TwinkleSpeed("Star Layer 1 - Twinkle Speed", float) = 2.0                        // Twinkle speed.
    _StarLayer1RotationSpeed("Star Layer 1 - Rotation Speed", float) = 2                        // Rotation speed of stars.
    _StarLayer1EdgeFade("Star Layer 1 - Edge Feathering", Range(0.0001, .9999)) = .2            // Softness of star blending with background.
    _StarLayer1HDRBoost("Star Layer 1 - HDR Bloom Boost", Range(1, 10)) = 1.0                   // Boost star colors so they glow with bloom filters.
    _StarLayer1SpriteDimensions("Star Layer 1 Sprite Dimensions", Vector) = (0, 0, 0, 0)        // Dimensions of columns (x), and rows (y) in sprite sheet.
    _StarLayer1SpriteItemCount("Star Layer 1 Sprite Total Items", int) = 1                      // Total number of items in sprite sheet.
    _StarLayer1SpriteAnimationSpeed("Star Layer 1 Sprite Speed", int) = 1                       // Speed of the sprite sheet animation.
    [NoScaleOffset]_StarLayer1DataTex("Star Layer 1 - Data Image", 2D) = "black" {}             // Data image with star positions.
    
    // Star Layer 2. - See property descriptions from star layer 1.
    [NoScaleOffset]_StarLayer2Tex("Star 2 Texture", 2D) = "white" {}
    _StarLayer2Color("Star Layer 2 - Color", Color) = (1, .5, .96, 1)
    _StarLayer2Density("Star Layer 2 - Star Density", Range(0, .05)) = .01
    _StarLayer2MaxRadius("Star Layer 2 - Star Size", Range(0, .4)) = .014
    _StarLayer2TwinkleAmount("Star Layer 2 - Twinkle Amount", Range(0, 1)) = .875
    _StarLayer2TwinkleSpeed("Star Layer 2 - Twinkle Speed", float) = 3.0
    _StarLayer2RotationSpeed("Star Layer 2 - Rotation Speed", float) = 2
    _StarLayer2EdgeFade("Star Layer 2 - Edge Feathering", Range(0.0001, .9999)) = .2
    _StarLayer2HDRBoost("Star Layer 2 - HDR Bloom Boost", Range(1, 10)) = 1.0
    _StarLayer2SpriteDimensions("Star Layer 2 Sprite Dimensions", Vector) = (0, 0, 0, 0)
    _StarLayer2SpriteItemCount("Star Layer 2 Sprite Total Items", int) = 1
    _StarLayer2SpriteAnimationSpeed("Star Layer 2 Sprite Speed", int) = 1
    [NoScaleOffset]_StarLayer2DataTex("Star Layer 2 - Data Image", 2D) = "black" {}

    // Star Layer 3. - See property descriptions from star layer 1.
    [NoScaleOffset]_StarLayer3Tex("Star 3 Texture", 2D) = "white" {}
    _StarLayer3Color("Star Layer 3 - Color", Color) = (.22, 1, .55, 1)
    _StarLayer3Density("Star Layer 3 - Star Density", Range(0, .05)) = .01
    _StarLayer3MaxRadius("Star Layer 3 - Star Size", Range(0, .4)) = .01
    _StarLayer3TwinkleAmount("Star Layer 3 - Twinkle Amount", Range(0, 1)) = .7
    _StarLayer3TwinkleSpeed("Star Layer 3 - Twinkle Speed", float) = 1.0
    _StarLayer3RotationSpeed("Star Layer 3 - Rotation Speed", float) = 2
    _StarLayer3EdgeFade("Star Layer 3 - Edge Feathering", Range(0.0001, .9999)) = .2
    _StarLayer3HDRBoost("Star Layer 3 - HDR Bloom Boost", Range(1, 10)) = 1.0
    _StarLayer3SpriteDimensions("Star Layer 3 Sprite Dimensions", Vector) = (0, 0, 0, 0)
    _StarLayer3SpriteItemCount("Star Layer 3 Sprite Total Items", int) = 1
    _StarLayer3SpriteAnimationSpeed("Star Layer 3 Sprite Speed", int) = 1
    [NoScaleOffset]_StarLayer3DataTex("Star Layer 3 - Data Image", 2D) = "black" {}

    // Moon properties.
    [NoScaleOffset]_MoonTex("Moon Texture", 2D) = "white" {}               // Moon image.
    _MoonColor("Moon Color", Color) = (.66, .65, .55, 1)                   // Moon tint color.
    _MoonRadius("Moon Size", Range(0, 1)) = .1                             // Radius of the moon.
    _MoonEdgeFade("Moon Edge Feathering", Range(0.0001, .9999)) = .3       // Soften edges of moon texture.
    _MoonHDRBoost("Moon HDR Bloom Boost", Range(1, 10)) = 1                // Control brightness for HDR bloom filter.
    _MoonSpriteDimensions("Moon Sprite Dimensions", Vector) = (0, 0, 0, 0) // Dimensions of columns (x), and rows (y) in sprite sheet.
    _MoonSpriteItemCount("Moon Sprite Total Items", int) = 1               // Total number of items in sprite sheet.
    _MoonSpriteAnimationSpeed("Moon Sprite Speed", int) = 1                // Speed of the sprite sheet animation.
    _MoonPosition("Moon Position" , Vector) = (0, 0, 0, 0)                 // Moon Position.

    // Sun properties.
    [NoScaleOffset]_SunTex("Sun Texture", 2D) = "white" {}                // Sun image.
    _SunColor("Sun Color", Color) = (.66, .65, .55, 1)                    // Sun tint color.
    _SunRadius("Sun Size", Range(0, 1)) = .1                              // Radius of the Sun.
    _SunEdgeFade("Sun Edge Feathering", Range(0.0001, .9999)) = .3        // Soften edges of Sun texture.
    _SunHDRBoost("Sun HDR Bloom Boost", Range(1, 10)) = 1                 // Control brightness for HDR bloom filter.
    _SunSpriteDimensions("Sun Sprite Dimensions", Vector) = (0, 0, 0, 0)  // Dimensions of columns (x), and rows (y) in sprite sheet.
    _SunSpriteItemCount("Sun Sprite Total Items", int) = 1                // Total number of items in sprite sheet.
    _SunSpriteAnimationSpeed("Sun Sprite Speed", int) = 1                 // Speed of the sprite sheet animation.
    _SunPosition("Sun Position Data" , Vector) = (0, 0, 0, 0)             // Sun position.

    // Noise Cloud properties.
    [NoScaleOffset]_CloudNoiseTexture("Cloud Texture", 2D) = "black" {}         // Cloud noise texture.
    _CloudFadePosition("Cloud Fade Position", Range(0, .97)) = .74              // Position that the clouds will begin fading out at.
    _CloudFadeAmount("Cloud Fade Amount", Range(0, 1)) = .5                     // Amount of fade to clouds.
    _CloudDensity("Cloud Density", Range(0, 1)) = .25                           // Cloud density.
    _CloudSpeed("Cloud Speed", Range(0, 1)) = .1                                // Cloud speed.
    _CloudDirection("Cloud Direction", Range(0, 6.283)) = 1.0                   // Cloud direction.
    _CloudHeight("Cloud Height", Range(0, 1)) = .5                              // Cloud height, by scaling up/down texture.
    _CloudTextureTiling("Cloud Tiling", Range(.01, 10)) = 2                     // Cloud tiling which changes visible resolution.
    _CloudColor1("Cloud 1 Color", Color) = (1, 1, 1, 1)                         // Cloud color 1.
    _CloudColor2("Cloud 2 Color", Color) = (.6, .6, .6, 1)                      // Cloud color 2.

    // Cubemap Clouds.
    [NoScaleOffset]_CloudCubemapTexture("Cloud Cubemap", CUBE) = "clear" {}                       // Cloud custom texture.
    _CloudCubemapRotationSpeed("Cloud Cubemap Rotation Speed", Range(-1, 1)) = .01                // Rotation speed and direction.
    _CloudCubemapTintColor("Cloud Cubemap Tint Color", Color) = (1, 1, 1, 1)                      // Tint color.
    _CloudCubemapHeight("Cloud Cubemap Height", Range(-1, 1)) = 0                                 // Cloud height
    [NoScaleOffset]_CloudCubemapDoubleTexture("Cloud Double Cubemap", CUBE) = "clear" {}          // Cloud custom texture.
    _CloudCubemapDoubleLayerHeight("Cloud Cubemap Double Layer Offset", float) = 0                // Offset of the duplicate cloud layer.
    _CloudCubemapDoubleLayerRotationSpeed("Cloud Cubemap Double Layer Rotation Speed", Range(-1, 1)) = .02
    _CloudCubemapDoubleLayerTintColor("Cloud Cubemap Double Tint Color", Color) = (1, 1, 1, 1)

    // Cubemap Normal Clouds.
    [NoScaleOffset]_CloudCubemapNormalTexture("Cloud Cubemap Normal Texture", CUBE) = "clear" {}                      // Cubemap texture with normals.
    _CloudCubemapNormalAmbientIntensity("Cloud Ambient Light Intensity", Range(0, 1)) = .2                            // Ambient light intensity.
    _CloudCubemapNormalRotationSpeed("Cloud Cubemap Normal Rotation Speed", Range(-1, 1)) = .01                       // Rotation speed and direction.
    _CloudCubemapNormalLitColor("Cloud Cubemap Normal Lit Color", Color) = (1, 1, 1, 1)                               // Lit color.
    _CloudCubemapNormalShadowColor("Cloud Cubemap Normal Shadow Color", Color) = (0, 0, 0, 1)                         // Shadow color.
    _CloudCubemapNormalHeight("Cloud Cubemap Normal Height", Range(-1, 1)) = 0                                        // Cloud height
    _CloudCubemapNormalToLight("Cloud Cubemap Light Direction", Vector) = (0, 1, 0, 0)                                // Direction to light.
    [NoScaleOffset]_CloudCubemapNormalDoubleTexture("Cloud Cubemap Normal Double Cubemap", CUBE) = "clear" {}         // Cloud custom texture.
    _CloudCubemapNormalDoubleLayerHeight("Cloud Cubemap Normal Double Layer Offset", float) = 0                       // Offset of the duplicate cloud layer.
    _CloudCubemapNormalDoubleLayerRotationSpeed("Cloud Cubemap Normal Double Layer Rotation Speed", Range(-1, 1)) = .02
    _CloudCubemapNormalDoubleLitColor("Cloud Cubemap Normal Double Lit Color", Color) = (1, 1, 1, 1)                        // Double layer lit color.
    _CloudCubemapNormalDoubleShadowColor("Cloud Cubemap Normal Double Shadow Color", Color) = (0, 0, 0, 1)                  // Double layer shadow color.

    _HorizonFogColor("Fog Color", Color) = (1, 1, 1, 1)               // Fog color.
    _HorizonFogDensity("Fog Density", Range(0, 1)) = .12              // Density and visibility of the fog.
    _HorizonFogLength("Fog Height", Range(.03, 1)) = .1               // Height the fog reaches up into the skybox.

    _DebugPointsCount("Debug Points Count", Range(0, 100)) = 0       // Used for visualizing orbit paths in editor only.
    _DebugPointRadius("Debug Point Radius", Range(0, .1)) = .03      // Size of sphere point dots when visualized.
  }

  SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Background" "IgnoreProjector"="true" "PreviewType" = "Skybox" }
    LOD 100
    ZWrite Off
    Cull Off

    Pass {
      CGPROGRAM
      #pragma target 2.0
      #pragma multi_compile_fog

      #pragma shader_feature_local GRADIENT_BACKGROUND
      #pragma shader_feature_local VERTEX_GRADIENT_BACKGROUND
      #pragma shader_feature_local STARS_BASIC
      #pragma shader_feature_local STAR_LAYER_1
      #pragma shader_feature_local STAR_LAYER_2
      #pragma shader_feature_local STAR_LAYER_3
      #pragma shader_feature_local STAR_LAYER_1_CUSTOM_TEXTURE
      #pragma shader_feature_local STAR_LAYER_2_CUSTOM_TEXTURE
      #pragma shader_feature_local STAR_LAYER_3_CUSTOM_TEXTURE
      #pragma shader_feature_local STAR_LAYER_1_SPRITE_SHEET
      #pragma shader_feature_local STAR_LAYER_2_SPRITE_SHEET
      #pragma shader_feature_local STAR_LAYER_3_SPRITE_SHEET
      #pragma shader_feature_local MOON
      #pragma shader_feature_local MOON_CUSTOM_TEXTURE
      #pragma shader_feature_local MOON_SPRITE_SHEET
      #pragma shader_feature_local SUN
      #pragma shader_feature_local SUN_CUSTOM_TEXTURE
      #pragma shader_feature_local SUN_SPRITE_SHEET
      #pragma shader_feature_local HORIZON_FOG
      #pragma shader_feature_local CLOUDS
      #pragma shader_feature_local NOISE_CLOUDS
      #pragma shader_feature_local CUBEMAP_CLOUDS
      #pragma shader_feature_local CUBEMAP_NORMAL_CLOUDS
      #pragma shader_feature_local CUBEMAP_CLOUD_DOUBLE_LAYER
      #pragma shader_feature_local CUBEMAP_NORMAL_CLOUD_DOUBLE_LAYER
      #pragma shader_feature_local CUBEMAP_CLOUD_FORMAT_RGB
      #pragma shader_feature_local CUBEMAP_CLOUD_FORMAT_RGBA
      #pragma shader_feature_local CUBEMAP_CLOUD_DOUBLE_LAYER_CUSTOM_TEXTURE
      #pragma shader_feature_local CUBEMAP_NORMAL_CLOUD_DOUBLE_LAYER_CUSTOM_TEXTURE
      #pragma shader_feature_local SUN_ALPHA_BLEND
      #pragma shader_feature_local MOON_ALPHA_BLEND
      #pragma shader_feature_local MOON_ROTATION
      #pragma shader_feature_local RENDER_DEBUG_POINTS

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "Utility/SkyMathUtilities.cginc"
       
      #if defined(STAR_LAYER_1) || defined(STAR_LAYER_2) || defined(STAR_LAYER_3)
        #define STARS_ADVANCED 1
      #endif

      struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        float3 smoothVertex : TEXCOORD1;

#if CLOUDS && NOISE_CLOUDS
        float4 cloudUVs : TEXCOORD2;
#endif

#if GRADIENT_BACKGROUND && VERTEX_GRADIENT_BACKGROUND
        fixed4 backgroundColor : TEXCOORD3;
#endif

#if SUN
        float3 sunLocalPosition : TEXCOORD4;
#endif

#if MOON
        float3 moonLocalPosition : TEXCOORD5;
#endif

#if STARS_ADVANCED
        float2 starDataTexUV : TEXCOORD6;
#endif
      };

      // Cubemap.
      samplerCUBE _MainTex;

      // Gradient sky.
      float _UseGradientSky;
      fixed4 _GradientSkyUpperColor;
      fixed4 _GradientSkyMiddleColor;
      fixed4 _GradientSkyLowerColor;
      float _GradientFadeMiddlePosition;

      float _GradientFadeBegin;
      float _GradientFadeEnd;

      float _StarFadeBegin;
      float _StarFadeEnd;


#if STARS_BASIC
    samplerCUBE _StarBasicCubemap;
    float _StarBasicTwinkleAmount;
    float _StarBasicTwinkleSpeed;
    float _StarBasicOpacity;
    fixed4 _StarBasicTintColor;
    float _StarBasicHDRBoost;
    float _StarBasicExponent;
#endif

#ifdef STAR_LAYER_1
      // Star Layer 1      
      fixed4 _StarLayer1Color;
      float _StarLayer1MaxRadius;
      float _StarLayer1Density;
      float _StarLayer1TwinkleAmount;
      float _StarLayer1TwinkleSpeed;
      float _StarLayer1RotationSpeed;
      float _StarLayer1EdgeFade;
      sampler2D _StarLayer1DataTex;
      float4 _StarLayer1DataTex_ST;;
      fixed _StarLayer1HDRBoost;
  #ifdef STAR_LAYER_1_CUSTOM_TEXTURE
      sampler2D _StarLayer1Tex;
    #ifdef STAR_LAYER_1_SPRITE_SHEET
      float2 _StarLayer1SpriteDimensions;
      int _StarLayer1SpriteItemCount;
      int _StarLayer1SpriteAnimationSpeed;
    #endif
  #endif
#endif

#ifdef STAR_LAYER_2
      // Star Layer 2
      fixed4 _StarLayer2Color;
      float _StarLayer2MaxRadius;
      float _StarLayer2Density;
      float _StarLayer2TwinkleAmount;
      float _StarLayer2TwinkleSpeed;
      float _StarLayer2RotationSpeed;
      float _StarLayer2EdgeFade;
      sampler2D _StarLayer2DataTex;
      float4 _StarLayer2DataTex_ST;;
      fixed _StarLayer2HDRBoost;
  #ifdef STAR_LAYER_2_CUSTOM_TEXTURE
      sampler2D _StarLayer2Tex;
    #ifdef STAR_LAYER_2_SPRITE_SHEET
      float2 _StarLayer2SpriteDimensions;
      int _StarLayer2SpriteItemCount;
      int _StarLayer2SpriteAnimationSpeed;
    #endif
  #endif
#endif

#ifdef STAR_LAYER_3
      // Star Layer 3
      fixed4 _StarLayer3Color;
      float _StarLayer3MaxRadius;
      float _StarLayer3Density;
      float _StarLayer3TwinkleAmount;
      float _StarLayer3TwinkleSpeed;
      float _StarLayer3RotationSpeed;
      float _StarLayer3EdgeFade;
      sampler2D _StarLayer3DataTex;
      float4 _StarLayer3DataTex_ST;;
      fixed _StarLayer3HDRBoost;
  #ifdef STAR_LAYER_3_CUSTOM_TEXTURE
      sampler2D _StarLayer3Tex;
    #ifdef STAR_LAYER_3_SPRITE_SHEET
      float2 _StarLayer3SpriteDimensions;
      int _StarLayer3SpriteItemCount;
      int _StarLayer3SpriteAnimationSpeed;
    #endif
  #endif
#endif

      float _HorizonScaleFactor;

#ifdef MOON
      // Moon
      float4x4 _MoonWorldToLocalMat;

  #ifdef MOON_CUSTOM_TEXTURE
      sampler2D _MoonTex;
    #ifdef MOON_SPRITE_SHEET
      float2 _MoonSpriteDimensions;
      int _MoonSpriteItemCount;
      int _MoonSpriteAnimationSpeed;
    #endif
  #endif
      fixed4 _MoonColor;
      float _MoonRadius;
      float _MoonEdgeFade;
      fixed _MoonHDRBoost;
      float4 _MoonPosition;
#endif

#ifdef SUN
      // Sun
      float4x4 _SunWorldToLocalMat;
  #ifdef SUN_CUSTOM_TEXTURE
      sampler2D _SunTex;
    #ifdef SUN_SPRITE_SHEET
      float2 _SunSpriteDimensions;
      int _SunSpriteItemCount;
      int _SunSpriteAnimationSpeed;
    #endif
  #endif
  
  fixed4 _SunColor;
  float _SunRadius;
  float _SunEdgeFade;
  fixed _SunHDRBoost;
  float4 _SunPosition;
  
#endif

#ifdef CLOUDS
      // Generic cloud uniforms.
      float _CloudSpeed;
      float _CloudHeight;
      
#if NOISE_CLOUDS
      sampler2D _CloudNoiseTexture;
      float _CloudDensity;
      float _CloudDirection;
      float _CloudFadePosition;
      float _CloudFadeAmount;
      float _CloudTextureTiling;
      fixed4 _CloudColor1;
      fixed4 _CloudColor2;
#endif

#if CUBEMAP_CLOUDS
      samplerCUBE _CloudCubemapTexture;
      float _CloudCubemapRotationSpeed;
      fixed4 _CloudCubemapTintColor;
      float _CloudCubemapHeight;

      #if CUBEMAP_CLOUD_DOUBLE_LAYER
        float _CloudCubemapDoubleLayerHeight;
        float _CloudCubemapDoubleLayerRotationSpeed;
        fixed4 _CloudCubemapDoubleLayerTintColor;

        #if CUBEMAP_CLOUD_DOUBLE_LAYER_CUSTOM_TEXTURE
          samplerCUBE _CloudCubemapDoubleTexture;
        #endif

      #endif // CUBEMAP_CLOUD_DOUBLE_LAYER
#endif

#if CUBEMAP_NORMAL_CLOUDS
      samplerCUBE _CloudCubemapNormalTexture;
      float _CloudCubemapNormalAmbientIntensity;
      float _CloudCubemapNormalRotationSpeed;
      float _CloudCubemapNormalHeight;
      fixed4 _CloudCubemapNormalLitColor;
      fixed4 _CloudCubemapNormalShadowColor;
      float3 _CloudCubemapNormalToLight;

      #if CUBEMAP_NORMAL_CLOUD_DOUBLE_LAYER
      float _CloudCubemapNormalDoubleLayerHeight;
      float _CloudCubemapNormalDoubleLayerRotationSpeed;
      fixed4 _CloudCubemapNormalDoubleLitColor;
      fixed4 _CloudCubemapNormalDoubleShadowColor;

      #if CUBEMAP_NORMAL_CLOUD_DOUBLE_LAYER_CUSTOM_TEXTURE
        samplerCUBE _CloudCubemapNormalDoubleTexture;  
      #endif

      #endif // CUBEMAP_NORMAL_CLOUD_DOUBLE_LAYER
#endif

#endif

#if HORIZON_FOG
      fixed4 _HorizonFogColor;
      float _HorizonFogDensity;
      float _HorizonFogLength;
#endif

#if RENDER_DEBUG_POINTS
      // This is only used in the editor for debugging and will get compiled out.
      float4 _DebugPoints[100];
      int _DebugPointsCount;
      float _DebugPointRadius;
#endif

      #define _MAX_CLOUD_COVERAGE 7
      #define _CLOUD_HEIGHT_LIMITS float2(30, 100)
      
      fixed4 AlphaBlendPartial(fixed4 top, fixed4 bottom) {
        fixed outAlpha = top.a + bottom.a * (1.0f - top.a);
        fixed3 outColor = (top.rgb * top.a + bottom.rgb * bottom.a * (1.0f - top.a)) / outAlpha;
        return fixed4(outColor, outAlpha);
      }

      // Does an over alpha blend, assumes bottom color is opaque.
      fixed4 AlphaBlend(fixed4 top, fixed4 bottom) {
        fixed3 ca = top.xyz;
        fixed aa = top.w;
        fixed3 cb = bottom.xyz;
        fixed ab = bottom.w;

        fixed3 color = (ca * aa + cb * ab * (1 - aa)) / (aa + ab * (1 - aa));
        return fixed4(color, 1.0f);
      }

      float2 CalculateStarRotation(float3 star)
      {
        float3 starPos = float3(star.x, star.y, star.z);

        float yRotationAngle = AngleToReachTarget(starPos.xz, UNITY_HALF_PI);

        starPos = RotateAroundYAxis(starPos, yRotationAngle);

        float xRotationAngle = AngleToReachTarget(starPos.zy, 0.0f);

        return float2(xRotationAngle, yRotationAngle);
      }

      float2 GetUVsForSpherePoint(float3 fragPos, float radius, float3 targetPoint) {
        float2 bodyRotations = CalculateStarRotation(targetPoint);
        float3 projectedPosition = RotatePoint(fragPos, bodyRotations.x, bodyRotations.y);

        // Find our UV position.
        return clamp(float2(
          (projectedPosition.x + radius) / (2.0 * radius),
          (projectedPosition.y + radius) / (2.0 * radius)), 0, 1);
      }

      float4 GetDataFromTexture(sampler2D tex, float2 uv) {
        float4 col = tex2Dlod(tex, float4(uv.x, uv.y, 0.0f, 0.0f));
        
        #if defined(UNITY_COLORSPACE_GAMMA) == false
        col.xyz = LinearToGammaSpace(col.xyz);
        #endif

        return col;
      }

      inline float4 GetStarDataFromTexture(sampler2D nearbyStarTexture, float2 uv) {
        float4 percentData = GetDataFromTexture(nearbyStarTexture, uv);

        float2 sphericalCoord = ConvertPercentToSphericalCoordinate(percentData.xy);
        return float4(sphericalCoord.x, sphericalCoord.y, percentData.z, 1.0f);
      }

      float2 AnimateStarRotation(float2 starUV, float rotationSpeed, float scale, float2 pivot) {
        return Rotate2d(starUV - pivot, rotationSpeed * _Time.y * scale) + pivot;
      }

      float GetStarRadius(float noise, float maxRadius, float twinkleAmount) {
        float noisePercent = noise;
        float minRadius = clamp((1 - twinkleAmount) * maxRadius, 0, maxRadius);
        return clamp(maxRadius * noise, minRadius, maxRadius) * _HorizonScaleFactor;
      }

      uint GetSpriteTargetIndex(int itemCount, int animationSpeed, float seed) {
        float delta = _Time.y + (10.0f * seed);
        float timePerFrame = 1.0f / (float)animationSpeed;
        int frameIndex = (int)(delta / timePerFrame);
        return (uint)abs(frameIndex % itemCount);
      }

      float2 GetSpriteItemSize(float2 dimensions) {
        return float2(1.0f / dimensions.x, (1.0f / dimensions.x) * (dimensions.x / dimensions.y));
      }

      float2 GetSpriteRotationOrigin(uint targetFrameIndex, float2 dimensions, float2 itemSize) {
        uint rows = (uint)dimensions.y;
        uint columns = (uint)dimensions.x;
        return float2(((float)(targetFrameIndex % columns) * itemSize.x + (itemSize.x / 2.0f)),
          (float)((rows - 1) - (targetFrameIndex / columns)) * itemSize.y + (itemSize.y / 2.0f));
      }

      float2 GetSpriteSheetCoords(float2 uv, float2 dimensions, uint targetFrameIndex, float2 itemSize, uint numItems) {
        uint rows = (uint)dimensions.y;
        uint columns = (uint)dimensions.x;

        float2 scaledUV = float2(uv.x * itemSize.x, uv.y * itemSize.y);
        float2 offset = float2(
          targetFrameIndex % columns * itemSize.x,
          ((rows - 1) - (targetFrameIndex / columns)) * itemSize.y);

        return scaledUV + offset;
      }

      float2 ConvertLocalPointToUV(float2 localPoint, float radius) {
        float2 shiftedPoint = localPoint.xy + float2(radius / 2.0f, radius / 2.0f);
        return abs(shiftedPoint) / radius;
      }

#if STARS_ADVANCED
      fixed4 StarColorWithTexture(
          float3 pos,
          float2 starCoords,
          float2 starUV,
          sampler2D starTexture,
          fixed4 starColorTint,
          float starDensity,
          float radius,
          float twinkleAmount,
          float twinkleSpeed,
          float rotationSpeed,
          float edgeFade,
          sampler2D nearbyStarsTexture,
          float4 gridPointWithNoise) {
        float3 gridPoint = gridPointWithNoise.xyz;
        float distanceToCenter = distance(pos, gridPoint);

        fixed4 outputColor = tex2D(starTexture, starUV) * starColorTint;

        // Animate alpha with twinkle wave.
        half twinkleWavePercent = smoothstep(-1, 1, cos(gridPointWithNoise.w * (100 + _Time.y) * twinkleSpeed));
        outputColor *= clamp(twinkleWavePercent, (1 - twinkleAmount), 1);

        // If it's outside the radius, zero is multiplied to clear the color values.
        return outputColor * smoothstep(radius, radius * (1 - edgeFade), distanceToCenter);
      }

      fixed4 StarColorNoTexture(
          float3 pos,
          fixed4 starColorTint,
          float starDensity,
          float radius,
          float twinkleAmount,
          float twinkleSpeed,
          float edgeFade,
          sampler2D nearbyStarsTexture,
          float4 gridPointWithNoise) {
        float3 gridPoint = gridPointWithNoise.xyz;

        float distanceToCenter = distance(pos, gridPoint);

        // Apply a horizon scale so stars are less visible with distance.
        radius *= _HorizonScaleFactor;

        fixed4 outputColor = starColorTint;

        // Animate alpha with twinkle wave.
        half twinkleWavePercent = smoothstep(-1, 1, cos(gridPointWithNoise.w * (100 + _Time.y) * twinkleSpeed));
        outputColor *= clamp(twinkleWavePercent, (1 - twinkleAmount), 1);

        // If it's outside the radius, zero is multiplied to clear the color values.
        return outputColor * smoothstep(radius, radius * (1 - edgeFade), distanceToCenter);
      }

      fixed4 StarColorFromAllGrids(float3 pos, float2 starTextureUV) {
        float4 nearbyStar = float4(0, 0, 0, 0);
        fixed4 allStarColors = fixed4(0, 0, 0, 0);
        float4 nearbySphericalStar = float4(0, 0, 0, 0);
        float3 nearbyStarDirection = float3(0, 0, 0);

#ifdef STAR_LAYER_3
        nearbySphericalStar = GetStarDataFromTexture(_StarLayer3DataTex, starTextureUV);

        nearbyStarDirection = SphericalCoordinateToDirection(nearbySphericalStar.xy);
        nearbyStar = float4(nearbyStarDirection.x, nearbyStarDirection.y, nearbyStarDirection.z, nearbySphericalStar.z);
        
        if (distance(pos, nearbyStar) <= _StarLayer3MaxRadius) {
          float radius = GetStarRadius(nearbyStar.w, _StarLayer3MaxRadius, _StarLayer3TwinkleAmount);

  #ifdef STAR_LAYER_3_CUSTOM_TEXTURE
          float2 texUV = GetUVsForSpherePoint(pos, radius, nearbyStar.xyz);
          float2 pivot = float2(.5f, .5f);
    #if STAR_LAYER_3_SPRITE_SHEET
          uint spriteFrameIndex = GetSpriteTargetIndex(_StarLayer3SpriteItemCount, _StarLayer3SpriteAnimationSpeed, nearbyStar.w);
          float2 spriteItemSize = GetSpriteItemSize(_StarLayer3SpriteDimensions);

          texUV = GetSpriteSheetCoords(texUV, _StarLayer3SpriteDimensions, spriteFrameIndex, spriteItemSize, _StarLayer3SpriteItemCount);
          pivot = GetSpriteRotationOrigin(spriteFrameIndex, _StarLayer3SpriteDimensions, spriteItemSize);
    #endif
          texUV = AnimateStarRotation(texUV, _StarLayer3RotationSpeed * nearbyStar.w, 1, pivot);
          
          allStarColors += StarColorWithTexture(
            pos,
            starTextureUV,
            texUV,
            _StarLayer3Tex,
            _StarLayer3Color,
            _StarLayer3Density,
            radius,
            _StarLayer3TwinkleAmount,
            _StarLayer3TwinkleSpeed,
            _StarLayer3RotationSpeed,
            _StarLayer3EdgeFade,
            _StarLayer3DataTex,
            nearbyStar) * _StarLayer3HDRBoost;
            
  #else
          allStarColors += StarColorNoTexture(
            pos,
            _StarLayer3Color,
            _StarLayer3Density,
            radius,
            _StarLayer3TwinkleAmount,
            _StarLayer3TwinkleSpeed,
            _StarLayer3EdgeFade,
            _StarLayer3DataTex,
            nearbyStar) * _StarLayer3HDRBoost;
  #endif      
        }
#endif

#ifdef STAR_LAYER_2
        nearbySphericalStar = GetStarDataFromTexture(_StarLayer2DataTex, starTextureUV);

        nearbyStarDirection = SphericalCoordinateToDirection(nearbySphericalStar.xy);
        nearbyStar = float4(nearbyStarDirection.x, nearbyStarDirection.y, nearbyStarDirection.z, nearbySphericalStar.z);
        
        if (distance(pos, nearbyStar) <= _StarLayer2MaxRadius) {
          float radius = GetStarRadius(nearbyStar.w, _StarLayer2MaxRadius, _StarLayer2TwinkleAmount);

  #ifdef STAR_LAYER_2_CUSTOM_TEXTURE
          float2 texUV = GetUVsForSpherePoint(pos, radius, nearbyStar.xyz);
          float2 pivot = float2(.5f, .5f);
    #if STAR_LAYER_2_SPRITE_SHEET
          uint spriteFrameIndex = GetSpriteTargetIndex(_StarLayer2SpriteItemCount, _StarLayer2SpriteAnimationSpeed, nearbyStar.w);
          float2 spriteItemSize = GetSpriteItemSize(_StarLayer2SpriteDimensions);

          texUV = GetSpriteSheetCoords(texUV, _StarLayer2SpriteDimensions, spriteFrameIndex, spriteItemSize, _StarLayer2SpriteItemCount);
          pivot = GetSpriteRotationOrigin(spriteFrameIndex, _StarLayer2SpriteDimensions, spriteItemSize);
    #endif
          texUV = AnimateStarRotation(texUV, _StarLayer2RotationSpeed * nearbyStar.w, 1, pivot);
          
          allStarColors += StarColorWithTexture(
            pos,
            starTextureUV,
            texUV,
            _StarLayer2Tex,
            _StarLayer2Color,
            _StarLayer2Density,
            radius,
            _StarLayer2TwinkleAmount,
            _StarLayer2TwinkleSpeed,
            _StarLayer2RotationSpeed,
            _StarLayer2EdgeFade,
            _StarLayer2DataTex,
            nearbyStar) * _StarLayer2HDRBoost;
            
  #else
          allStarColors += StarColorNoTexture(
            pos,
            _StarLayer2Color,
            _StarLayer2Density,
            radius,
            _StarLayer2TwinkleAmount,
            _StarLayer2TwinkleSpeed,
            _StarLayer2EdgeFade,
            _StarLayer2DataTex,
            nearbyStar) * _StarLayer2HDRBoost;
  #endif      
        }
#endif
       

#ifdef STAR_LAYER_1
        nearbySphericalStar = GetStarDataFromTexture(_StarLayer1DataTex, starTextureUV);

        nearbyStarDirection = SphericalCoordinateToDirection(nearbySphericalStar.xy);
        nearbyStar = float4(nearbyStarDirection.x, nearbyStarDirection.y, nearbyStarDirection.z, nearbySphericalStar.z);
        
        if (distance(pos, nearbyStar) <= _StarLayer1MaxRadius) {
          float radius = GetStarRadius(nearbyStar.w, _StarLayer1MaxRadius, _StarLayer1TwinkleAmount);

  #ifdef STAR_LAYER_1_CUSTOM_TEXTURE
          float2 texUV = GetUVsForSpherePoint(pos, radius, nearbyStar.xyz);
          float2 pivot = float2(.5f, .5f);
    #if STAR_LAYER_1_SPRITE_SHEET
          uint spriteFrameIndex = GetSpriteTargetIndex(_StarLayer1SpriteItemCount, _StarLayer1SpriteAnimationSpeed, nearbyStar.w);
          float2 spriteItemSize = GetSpriteItemSize(_StarLayer1SpriteDimensions);

          texUV = GetSpriteSheetCoords(texUV, _StarLayer1SpriteDimensions, spriteFrameIndex, spriteItemSize, _StarLayer1SpriteItemCount);
          pivot = GetSpriteRotationOrigin(spriteFrameIndex, _StarLayer1SpriteDimensions, spriteItemSize);
    #endif
          texUV = AnimateStarRotation(texUV, _StarLayer1RotationSpeed * nearbyStar.w, 1, pivot);
          
          allStarColors += StarColorWithTexture(
            pos,
            starTextureUV,
            texUV,
            _StarLayer1Tex,
            _StarLayer1Color,
            _StarLayer1Density,
            radius,
            _StarLayer1TwinkleAmount,
            _StarLayer1TwinkleSpeed,
            _StarLayer1RotationSpeed,
            _StarLayer1EdgeFade,
            _StarLayer1DataTex,
            nearbyStar) * _StarLayer1HDRBoost;
            
  #else
          allStarColors += StarColorNoTexture(
            pos,
            _StarLayer1Color,
            _StarLayer1Density,
            radius,
            _StarLayer1TwinkleAmount,
            _StarLayer1TwinkleSpeed,
            _StarLayer1EdgeFade,
            _StarLayer1DataTex,
            nearbyStar) * _StarLayer1HDRBoost;
  #endif      
        }
#endif
        return allStarColors;
      }
      
#endif // STARS_ADVANCED

      fixed4 FadeStarsColor(float verticalPosition, fixed4 starColor) {
        float fadeAmount = smoothstep(_StarFadeBegin, _StarFadeEnd, verticalPosition);
        return fixed4(starColor.xyz * fadeAmount, 1.0f);
      }

#if STARS_BASIC
      fixed4 BasicStarColorAtFragment(float3 pos) {
        fixed3 starData = texCUBE(_StarBasicCubemap, pos);
        float twinklePercent = sin(_Time.y * _StarBasicTwinkleSpeed + (starData.b * 3.0f) ) * .5f + .5f;
        float twinkleValue = lerp(1.0f - _StarBasicTwinkleAmount, _StarBasicHDRBoost, twinklePercent);
        fixed starIntensity = pow(starData.g + starData.r, _StarBasicExponent) * twinkleValue * _StarBasicOpacity;
        return _StarBasicTintColor * starIntensity;
      }
#endif

      fixed4 Calculate3WayGradientBackgroundAtPosition(float3 pos) {
        float2 sphereFragCoord = DirectionToSphericalCoordinate(pos);
        float verticalPosition = pos.y;

        // 3 way gradient.
        float middleGradientPosition = _GradientFadeBegin 
          + ((_GradientFadeEnd - _GradientFadeBegin) * _GradientFadeMiddlePosition);

        fixed4 lowerColor = _GradientSkyLowerColor;
        fixed4 middleColor = _GradientSkyMiddleColor;
        fixed4 upperColor = _GradientSkyUpperColor;

        float bottomColorPercent = smoothstep(_GradientFadeBegin, middleGradientPosition, verticalPosition);
        fixed4 bottomMixedColor = lerp(lowerColor, middleColor, bottomColorPercent);
        bottomMixedColor *= !step(middleGradientPosition, verticalPosition);

        float topColorPercent = smoothstep(middleGradientPosition, _GradientFadeEnd, verticalPosition);
        fixed4 topMixedColor = lerp(middleColor, upperColor, topColorPercent);
        topMixedColor *= step(middleGradientPosition, verticalPosition);

        return bottomMixedColor + topMixedColor;
      }
      
      v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);

        float3 normalizedVertex = normalize(v.vertex.xyz);
        float3 worldVertexPosition = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f));

        o.smoothVertex = v.vertex;

        #ifdef CLOUDS
          #if NOISE_CLOUDS
            float3 cloudWorldVertex = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

            float computedHeight = lerp(_CLOUD_HEIGHT_LIMITS.x, _CLOUD_HEIGHT_LIMITS.y, 1 - _CloudHeight);
            cloudWorldVertex.y *= computedHeight * .2f;

            float cloudSpeed = _CloudSpeed * _Time;
            cloudWorldVertex.xz  = Rotate2d(cloudWorldVertex.xz, _CloudDirection);
            cloudWorldVertex = normalize(cloudWorldVertex);
            o.cloudUVs.xy = (cloudWorldVertex.xz * _CloudTextureTiling) + float2(cloudSpeed, cloudSpeed);
            o.cloudUVs.zw = (cloudWorldVertex.xz * _CloudTextureTiling) + float2(cloudSpeed / 10.0f, cloudSpeed / 11.0f);
          #endif
        #endif

        #if GRADIENT_BACKGROUND && VERTEX_GRADIENT_BACKGROUND
          o.backgroundColor = Calculate3WayGradientBackgroundAtPosition(normalizedVertex);
        #endif
        
        #if SUN
          o.sunLocalPosition = mul(_SunWorldToLocalMat, float4(normalizedVertex, 1.0f));
        #endif

        #if MOON
          o.moonLocalPosition = mul(_MoonWorldToLocalMat, float4(normalizedVertex, 1.0f));
        #endif

        #if STARS_ADVANCED
          o.starDataTexUV = ConvertSphericalCoordateToUV(DirectionToSphericalCoordinate(normalizedVertex));
        #endif

        return o;
      }

      fixed4 OrbitBodyColorWithTextureUV(float3 pos, float3 orbitBodyPosition, 
          fixed4 orbitBodyTintColor, float orbitBodyRadius, float orbitBodyEdgeFade, sampler2D orbitBodyTex, float2 bodyUVs) {
        fixed4 color = tex2D(orbitBodyTex, bodyUVs) * orbitBodyTintColor;
        
        float fragDistance = distance(orbitBodyPosition, pos);

        float fadeEnd = orbitBodyRadius * (1 - orbitBodyEdgeFade);

        return smoothstep(orbitBodyRadius, fadeEnd, fragDistance) * color;
      }

      // Alpha premultiplied into color.
      fixed4 OrbitBodyColorNoTexture(float3 pos, float3 orbitBodyPosition, 
          fixed4 orbitBodyColor, float orbitBodyRadius, float orbitBodyEdgeFade) {        
        float fragDistance = distance(orbitBodyPosition, pos);
        float fadeEnd = orbitBodyRadius * (1 - orbitBodyEdgeFade);

        return orbitBodyColor * smoothstep(orbitBodyRadius, fadeEnd, fragDistance);
      }

#ifdef CLOUDS

#if CUBEMAP_NORMAL_CLOUDS

      float4 SampleNormalCloudCubemap(float3 vertexPos, float rotationSpeed, float heightOffset, float rotationOffset, float3 shadowColor, float3 litColor) {
        float rotationAngle = (_Time.y * rotationSpeed) + rotationOffset;
        float3 rotatedDirection = RotateAroundYAxis(vertexPos, -rotationAngle);
        rotatedDirection.y += (-1.0f * heightOffset);

        float4 cloudColor = texCUBE(_CloudCubemapNormalTexture, rotatedDirection);
        float3 cloudFragWorldNormal = cloudColor.xyz * 2.0f - 1.0f;
        float3 cloudRotatedWorldNormal = RotateAroundYAxis(cloudFragWorldNormal, rotationAngle);

        float3 lightPosition = _CloudCubemapNormalToLight * 10.0f;
        float3 toLight = normalize(lightPosition - vertexPos.xyz);

        float lightDotProduct = dot(cloudRotatedWorldNormal, toLight);
        float lightPercent = (lightDotProduct + 1.0f) / 2.0f;

        float3 processedColor = lerp(shadowColor, litColor, saturate(_CloudCubemapNormalAmbientIntensity + lightPercent));

        return float4(processedColor, cloudColor.w);
      }

      // Clouds coming from a cubemap with normals.
      half4 RenderCubemapNormalClouds(float3 vertexPos, float4 backgroundColor)
      {        
        float4 cloudColor = SampleNormalCloudCubemap(vertexPos, _CloudCubemapNormalRotationSpeed, _CloudCubemapNormalHeight, 0,
          _CloudCubemapNormalShadowColor, _CloudCubemapNormalLitColor);
        
        #if CUBEMAP_NORMAL_CLOUD_DOUBLE_LAYER
          float4 cloudColor2 = SampleNormalCloudCubemap(vertexPos, _CloudCubemapNormalDoubleLayerRotationSpeed, 
            _CloudCubemapNormalDoubleLayerHeight, UNITY_HALF_PI, _CloudCubemapNormalDoubleShadowColor, _CloudCubemapNormalDoubleLitColor);

          float origAlpha = max(cloudColor.w, cloudColor2.w);
          cloudColor = saturate(AlphaBlend(cloudColor, cloudColor2));
          cloudColor.w = origAlpha;
        #endif

        return AlphaBlend(cloudColor, backgroundColor);
      }

#endif

#if CUBEMAP_CLOUDS

      fixed4 SampleCloudCubemap(float3 vertexPos, float rotationSpeed, float heightOffset, float rotationOffset, samplerCUBE tex, float4 tintColor) {
        float rotationAngle = (_Time.y * rotationSpeed) + rotationOffset;
        float3 rotatedDirection = RotateAroundYAxis(vertexPos, -rotationAngle);
        rotatedDirection.y += (-1.0f * heightOffset);

        fixed4 outColor = texCUBE(tex, rotatedDirection) * tintColor;
        outColor.a = pow(outColor.a, 2);
        return outColor;
      }

      // Clouds coming from a cubemap, no normals, unlit
      fixed4 RenderCubemapClouds(float3 vertexPos, fixed4 backgroundColor) {        
        float rotationAngle = _Time.y * _CloudCubemapRotationSpeed;
        float3 rotatedDirection = RotateAroundYAxis(vertexPos, -rotationAngle);
        rotatedDirection.y += (-1.0f * _CloudCubemapHeight);

        fixed4 cloudColor = SampleCloudCubemap(vertexPos, _CloudCubemapRotationSpeed, _CloudCubemapHeight,
          0, _CloudCubemapTexture, _CloudCubemapTintColor);
        
        #if CUBEMAP_CLOUD_FORMAT_RGB
          // Premultiply alpha before additive blending for opacity control.
          cloudColor.xyz *= cloudColor.a;

          #if CUBEMAP_CLOUD_DOUBLE_LAYER
        
            #if CUBEMAP_CLOUD_DOUBLE_LAYER_CUSTOM_TEXTURE
            fixed4 cloudColor2 = SampleCloudCubemap(vertexPos, _CloudCubemapDoubleLayerRotationSpeed, _CloudCubemapDoubleLayerHeight,
              UNITY_HALF_PI, _CloudCubemapDoubleTexture, _CloudCubemapDoubleLayerTintColor);
            #else // Else no custom texture.
            fixed4 cloudColor2 = SampleCloudCubemap(vertexPos, _CloudCubemapDoubleLayerRotationSpeed, _CloudCubemapDoubleLayerHeight,
              UNITY_HALF_PI, _CloudCubemapTexture, _CloudCubemapDoubleLayerTintColor);
            #endif // CUBEMAP_CLOUD_DOUBLE_LAYER_CUSTOM_TEXTURE
            
            // Premultiply alpha before additive blending for opacity control.
            cloudColor2.xyz *= cloudColor2.a;

            return fixed4(cloudColor.xyz + cloudColor2.xyz + backgroundColor.xyz, 1.0f);
          #else // Else, no double layer.
            return fixed4(cloudColor.xyz + backgroundColor.xyz, 1.0f);
          #endif // CUBEMAP_CLOUD_DOUBLE_LAYER
        #else // CUBEMAP RGBA
          #if CUBEMAP_CLOUD_DOUBLE_LAYER
            #if CUBEMAP_CLOUD_DOUBLE_LAYER_CUSTOM_TEXTURE
            fixed4 cloudColor2 = SampleCloudCubemap(vertexPos, _CloudCubemapDoubleLayerRotationSpeed, _CloudCubemapDoubleLayerHeight,
              UNITY_HALF_PI, _CloudCubemapDoubleTexture, _CloudCubemapDoubleLayerTintColor);
            #else
            fixed4 cloudColor2 = SampleCloudCubemap(vertexPos, _CloudCubemapDoubleLayerRotationSpeed, _CloudCubemapDoubleLayerHeight,
              UNITY_HALF_PI, _CloudCubemapTexture, _CloudCubemapDoubleLayerTintColor);
            #endif

            cloudColor = AlphaBlendPartial(cloudColor, cloudColor2);

          #endif // CUBEMAP_CLOUD_DOUBLE_LAYER
          backgroundColor.a = 1.0f;

          return AlphaBlend(cloudColor, backgroundColor);
        #endif
      }

#endif

#if NOISE_CLOUDS

      half4 RenderNoiseClouds(float4 cloudUVs, float3 vertexPos, float4 backgroundColor) {
				// Cloud noise.
				float4 tex1  = GetDataFromTexture(_CloudNoiseTexture, cloudUVs.xy);
        float4 tex2  = GetDataFromTexture(_CloudNoiseTexture, cloudUVs.zw);

        float noise1 = pow(tex1.g + tex2.g, 0.25f);
				float noise2 = pow(tex2.b * tex1.r, 0.5f);

        // Percent in the fadeout (0 means no fadeout, 1 means full fadeout - no clouds)
        float fadeOutPercent = smoothstep(_CloudFadePosition, 1, length(vertexPos.xz));

				_CloudColor1.rgb = pow(_CloudColor1.rgb, 2.2f);
        _CloudColor2.rgb = pow(_CloudColor2.rgb, 2.2f);

				float3 cloud1 = lerp(float3(0, 0, 0), _CloudColor2.rgb, noise1);
				float3 cloud2 = lerp(float3(0, 0, 0), _CloudColor1.rgb, noise2) * 1.5f;
				float3 cloud  = lerp(cloud1, cloud2, noise1 * noise2);

        // Cloud alpha.
        float outColorAlpha = 1.0f;
        float expandedDensity = _MAX_CLOUD_COVERAGE * (1.0f - _CloudDensity);
				float cloudAlpha = saturate(pow(noise1 * noise2, expandedDensity)) * pow(outColorAlpha, 0.35f);
        cloudAlpha *= 1.0f - fadeOutPercent * _CloudFadeAmount;
        cloudAlpha *= step(0.0f, vertexPos.y);
        
        float3 outColor = lerp(backgroundColor.rgb, cloud, cloudAlpha);

        return half4(outColor, 1.0f);
      }
#endif

#endif // CLOUDS

#ifdef RENDER_DEBUG_POINTS
      // Debug points are used for visualized spherical point keyframes in the editor only.
      fixed4 RenderDebugPoints(float3 pos) {
        fixed4 pointColor = fixed4(1, 0, 0, 1);
        fixed4 selectedPointColor = fixed4(0, 1, 0, 1);

        for (int i = 0; i < _DebugPointsCount; i++) {
          float4 debugPoint = _DebugPoints[i];
          float radius = debugPoint.w;
          if (distance(debugPoint.xyz, pos) <= _DebugPointRadius) {
            half4 color = pointColor;
            if (debugPoint.w > 0) {
              return selectedPointColor;
            } else {
              return pointColor;
            }
          }
        }

        return fixed4(0, 0, 0, 0);
      }
#endif

#ifdef HORIZON_FOG
      fixed4 ApplyHorizonFog(fixed4 skyColor, float3 vertexPos) {
        float fadePercent = smoothstep(1 - _HorizonFogLength, 1, length(vertexPos.xz));
        fadePercent *= _HorizonFogDensity;
        return lerp(skyColor, _HorizonFogColor, fadePercent);
      }
#endif

      fixed4 frag(v2f i) : SV_Target {
        
#ifdef GRADIENT_BACKGROUND
  #if VERTEX_GRADIENT_BACKGROUND
        fixed4 background = i.backgroundColor;
  #else
        fixed4 background = Calculate3WayGradientBackgroundAtPosition(i.smoothVertex);
  #endif
#else
        fixed4 background = texCUBE(_MainTex, i.smoothVertex);
#endif

        float3 normalizedSmoothVertex = normalize(i.smoothVertex);
        bool isMoonPixel = 0;
        bool isSunPixel = 0;
        fixed4 sunColor = fixed4(0, 0, 0, 0);
        fixed4 moonColor = fixed4(0, 0, 0, 0);

#ifdef MOON
        isMoonPixel = step(distance(normalizedSmoothVertex, _MoonPosition.xyz), _MoonRadius);

        float2 moonTexUV = ConvertLocalPointToUV(i.moonLocalPosition.xyz, _MoonRadius * 2.0f);

        #if MOON_CUSTOM_TEXTURE
          #if MOON_SPRITE_SHEET
            uint spriteFrameIndex = GetSpriteTargetIndex(_MoonSpriteItemCount, _MoonSpriteAnimationSpeed, 0.0f);
            float2 spriteItemSize = GetSpriteItemSize(_MoonSpriteDimensions.xy);

            moonTexUV = GetSpriteSheetCoords(moonTexUV, _MoonSpriteDimensions, spriteFrameIndex, spriteItemSize, _MoonSpriteItemCount);  
          #endif

          moonColor = OrbitBodyColorWithTextureUV(
            normalizedSmoothVertex,
            _MoonPosition.xyz,
            _MoonColor,
            _MoonRadius,
            _MoonEdgeFade,
            _MoonTex,
            moonTexUV) * _MoonHDRBoost * isMoonPixel;
        #else
          moonColor = OrbitBodyColorNoTexture(
            normalizedSmoothVertex,
            _MoonPosition.xyz,
            _MoonColor,
            _MoonRadius,
            _MoonEdgeFade) * _MoonHDRBoost * isMoonPixel;
        #endif
        
#endif

#ifdef SUN
        isSunPixel = step(distance(normalizedSmoothVertex, _SunPosition.xyz), _SunRadius);

        float2 sunTexUV = ConvertLocalPointToUV(i.sunLocalPosition.xyz, _SunRadius * 2.0f);

        #if SUN_CUSTOM_TEXTURE
          #if SUN_SPRITE_SHEET
            uint spriteFrameIndex = GetSpriteTargetIndex(_SunSpriteItemCount, _SunSpriteAnimationSpeed, 0.0f);
            float2 spriteItemSize = GetSpriteItemSize(_SunSpriteDimensions.xy);

            sunTexUV = GetSpriteSheetCoords(sunTexUV, _SunSpriteDimensions, spriteFrameIndex, spriteItemSize, _SunSpriteItemCount);
          #endif

          sunColor = OrbitBodyColorWithTextureUV(
            normalizedSmoothVertex,
            _SunPosition.xyz,
            _SunColor,
            _SunRadius,
            _SunEdgeFade,
            _SunTex,
            sunTexUV) * _SunHDRBoost * isSunPixel;
        #else
          sunColor = OrbitBodyColorNoTexture(
            normalizedSmoothVertex,
            _SunPosition.xyz,
            _SunColor,
            _SunRadius,
            _SunEdgeFade) * _SunHDRBoost * isSunPixel;
        #endif
#endif

#if STARS_BASIC
        fixed4 starColor = BasicStarColorAtFragment(normalizedSmoothVertex);

        starColor = FadeStarsColor(i.smoothVertex.y, starColor);
        
        // FIXME - Create generic macro for if any star type is active.
        #if MOON && !defined(MOON_ALPHA_BLEND)
          starColor *= (fixed)!isMoonPixel;
        #endif

        #if SUN && !defined(SUN_ALPHA_BLEND)
          starColor *= (fixed)!isSunPixel;
        #endif

        background.xyz += starColor.xyz;
#elif STARS_ADVANCED
        fixed4 starColor = StarColorFromAllGrids(normalizedSmoothVertex, i.starDataTexUV);

        // Fade stars over the horizon.
        starColor = FadeStarsColor(i.smoothVertex.y, starColor);

        #if MOON && !defined(MOON_ALPHA_BLEND)
          starColor *= (fixed)!isMoonPixel;
        #endif

        #if SUN && !defined(SUN_ALPHA_BLEND)
          starColor *= (fixed)!isSunPixel;
        #endif

        background.xyz += starColor.xyz;
#endif


#ifdef RENDER_DEBUG_POINTS
        fixed4 debugPointColor = RenderDebugPoints(normalize(i.smoothVertex.xyz));
        bool useDebugColor = step(.1f, length(debugPointColor));
        debugPointColor *= useDebugColor;
        background *= (fixed)!useDebugColor;
        background = background + debugPointColor;
#endif

        // Merge the stars over the background color.
        fixed4 upperSkyColor = background;
        fixed4 finalColor = fixed4(0, 0, 0, 1);

#ifdef SUN_ALPHA_BLEND
          finalColor = AlphaBlend(sunColor, upperSkyColor);
#else
          finalColor = upperSkyColor + sunColor;
#endif

#ifdef MOON_ALPHA_BLEND
          finalColor = AlphaBlend(moonColor, finalColor);
#else
          finalColor += moonColor;
#endif

#ifdef CLOUDS
  #if NOISE_CLOUDS
        finalColor = RenderNoiseClouds(i.cloudUVs, i.smoothVertex, finalColor);
  #elif CUBEMAP_CLOUDS
        finalColor = RenderCubemapClouds(i.smoothVertex, finalColor);
  #elif CUBEMAP_NORMAL_CLOUDS
        finalColor = RenderCubemapNormalClouds(i.smoothVertex, finalColor);
  #endif
#endif

#ifdef HORIZON_FOG
        finalColor = ApplyHorizonFog(finalColor, i.smoothVertex);
#endif
        return finalColor;
      }
      ENDCG
    }
  }
  CustomEditor "DoNotModifyShaderEditor"
  Fallback "Unlit/Color"
}
