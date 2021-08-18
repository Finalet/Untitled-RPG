// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#ifndef CREST_OCEAN_SHADER_DATA_INCLUDED
#define CREST_OCEAN_SHADER_DATA_INCLUDED

#include "OceanConstants.hlsl"

/////////////////////////////
// Samplers

#if defined(UNITY_DECLARE_SCREENSPACE_TEXTURE)
UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);
UNITY_DECLARE_SCREENSPACE_TEXTURE(_BackgroundTexture);
#elif defined(TEXTURE2D_X)
TEXTURE2D_X(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D_X(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);
#endif

// NOTE: _Normals is used outside of _APPLYNORMALMAPPING_ON so we cannot surround it here.
sampler2D _Normals;
sampler2D _ReflectionTex;
// #if _OVERRIDEREFLECTIONCUBEMAP_ON
// samplerCUBE _ReflectionCubemapOverride;
// #endif
#if _FOAM_ON
sampler2D _FoamTexture;
#endif
#if _CAUSTICS_ON
sampler2D _CausticsTexture;
#endif

/////////////////////////////
// Constant buffer: CrestPerMaterial

CBUFFER_START(CrestInputsPerMaterial)

// ----------------------------------------------------------------------------
// Diffuse
// ----------------------------------------------------------------------------

half3 _Diffuse;
half3 _DiffuseGrazing;
#if _SHADOWS_ON
half3 _DiffuseShadow;
#endif

// ----------------------------------------------------------------------------
// Transparency
// ----------------------------------------------------------------------------

#if _TRANSPARENCY_ON
half _RefractionStrength;
#endif
half4 _DepthFogDensity;

// ----------------------------------------------------------------------------
// Normals
// ----------------------------------------------------------------------------

half _NormalsStrengthOverall;
#if _APPLYNORMALMAPPING_ON
half _NormalsStrength;
half _NormalsScale;
#endif

// ----------------------------------------------------------------------------
// Sub-Surface Scattering
// ----------------------------------------------------------------------------

#if _SUBSURFACESCATTERING_ON
half3 _SubSurfaceColour;
half _SubSurfaceBase;
half _SubSurfaceSun;
half _SubSurfaceSunFallOff;
#endif

#if _SUBSURFACESHALLOWCOLOUR_ON
half _SubSurfaceDepthMax;
half _SubSurfaceDepthPower;
half3 _SubSurfaceShallowCol;
#if _SHADOWS_ON
half3 _SubSurfaceShallowColShadow;
#endif
#endif

// ----------------------------------------------------------------------------
// Reflections
// ----------------------------------------------------------------------------

half _Specular;
half _ReflectionBlur; // Roughness
half _FresnelPower;
float _RefractiveIndexOfAir;
float _RefractiveIndexOfWater;
half _LightIntensityMultiplier; // _DirectionalLightBoost

half _Smoothness;
#if _VARYSMOOTHNESSOVERDISTANCE_ON
half _SmoothnessFar; // _DirectionalLightFarDistance
half _SmoothnessFarDistance; // _DirectionalLightFarDistance
half _SmoothnessPower; // _DirectionalLightFallOff
#endif

#if _PLANARREFLECTIONS_ON
half _PlanarReflectionNormalsStrength;
half _PlanarReflectionIntensity;
#endif

#if _PROCEDURALSKY_ON
half3 _SkyBase;
half3 _SkyAwayFromSun;
half3 _SkyTowardsSun;
half _SkyDirectionality;
#endif

// ----------------------------------------------------------------------------
// Foam
// ----------------------------------------------------------------------------

#if _FOAM_ON
half _FoamScale;
half4 _FoamWhiteColor;
half _WaveFoamFeather;
half _WaveFoamLightScale;
half _ShorelineFoamMinDepth;
#if _FOAM3DLIGHTING_ON
float4 _FoamTexture_TexelSize;
half _WaveFoamNormalStrength;
half _WaveFoamSpecularFallOff;
half _WaveFoamSpecularBoost;
#endif
// Foam Bubbles
half4 _FoamBubbleColor;
half _FoamBubbleParallax;
half _WaveFoamBubblesCoverage;
#endif

// ----------------------------------------------------------------------------
// Caustics
// ----------------------------------------------------------------------------

#if _CAUSTICS_ON
half _CausticsTextureScale;
half _CausticsTextureAverage;
half _CausticsStrength;
half _CausticsFocalDepth;
half _CausticsDepthOfField;
half _CausticsDistortionScale;
half _CausticsDistortionStrength;
#endif

// Hack - due to SV_IsFrontFace occasionally coming through as true for backfaces,
// add a param here that forces ocean to be in undrwater state. I think the root
// cause here might be imprecision or numerical issues at ocean tile boundaries, although
// i'm not sure why cracks are not visible in this case.
float _ForceUnderwater;

// TODO: This can be removed once we use the underwater post-process effect.
float _HeightOffset;

CBUFFER_END

#endif // CREST_OCEAN_SHADER_DATA_INCLUDED
