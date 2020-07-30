// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#ifndef CREST_OCEAN_INPUT_INCLUDED
#define CREST_OCEAN_INPUT_INCLUDED

#include "OceanConstants.hlsl"

/////////////////////////////
// Samplers

TEXTURE2D_X(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D_X(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);

sampler2D _Normals;
sampler2D _ReflectionTex;
//sampler2D _ReflectionCubemapOverride;
sampler2D _FoamTexture;
sampler2D _CausticsTexture;

/////////////////////////////
// Constant buffer: CrestPerMaterial

CBUFFER_START(CrestInputsPerMaterial)
half3 _Diffuse;
half3 _DiffuseGrazing;

half _RefractionStrength;
half4 _DepthFogDensity;

half3 _SubSurfaceColour;
half _SubSurfaceBase;
half _SubSurfaceSun;
half _SubSurfaceSunFallOff;
half _SubSurfaceHeightMax;
half _SubSurfaceHeightPower;
half3 _SubSurfaceCrestColour;

half _SubSurfaceDepthMax;
half _SubSurfaceDepthPower;
half3 _SubSurfaceShallowCol;
half3 _SubSurfaceShallowColShadow;

half _CausticsTextureScale;
half _CausticsTextureAverage;
half _CausticsStrength;
half _CausticsFocalDepth;
half _CausticsDepthOfField;
half _CausticsDistortionScale;
half _CausticsDistortionStrength;

half3 _DiffuseShadow;

half _NormalsStrength;
half _NormalsScale;

half3 _SkyBase, _SkyAwayFromSun, _SkyTowardsSun;
half _SkyDirectionality;

half _Specular;
half _Smoothness;
half _SmoothnessFar;
half _SmoothnessFarDistance;
half _SmoothnessPower;
half _ReflectionBlur;
half _FresnelPower;
half _LightIntensityMultiplier;
float  _RefractiveIndexOfAir;
float  _RefractiveIndexOfWater;
half _PlanarReflectionNormalsStrength;
half _PlanarReflectionIntensity;

half _FoamScale;
float4 _FoamTexture_TexelSize;
half4 _FoamWhiteColor;
half4 _FoamBubbleColor;
half _FoamBubbleParallax;
half _ShorelineFoamMinDepth;
half _WaveFoamFeather;
half _WaveFoamBubblesCoverage;
half _WaveFoamNormalStrength;
half _WaveFoamSpecularFallOff;
half _WaveFoamSpecularBoost;
half _WaveFoamLightScale;
half2 _WindDirXZ;

// Hack - due to SV_IsFrontFace occasionally coming through as true for backfaces,
// add a param here that forces ocean to be in undrwater state. I think the root
// cause here might be imprecision or numerical issues at ocean tile boundaries, although
// i'm not sure why cracks are not visible in this case.
float _ForceUnderwater;

float _HeightOffset;

// Settings._jitterDiameterSoft, Settings._jitterDiameterHard, Settings._currentFrameWeightSoft, Settings._currentFrameWeightHard
float4 _JitterDiameters_CurrentFrameWeights;

float3 _CenterPos;
float3 _Scale;
float _LD_SliceIndex_Source;
float4x4 _MainCameraProjectionMatrix;

float _FoamFadeRate;
float _WaveFoamStrength;
float _WaveFoamCoverage;
float _ShorelineFoamMaxDepth;
float _ShorelineFoamStrength;
float _SimDeltaTime;
float _SimDeltaTimePrev;
float _LODChange;

half _Damping;
float2 _LaplacianAxisX;
half _Gravity;
CBUFFER_END

#endif // OCEAN_INPUT_INCLUDED
