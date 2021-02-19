// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// This is a compute shader for the built-in RP, but is a pixel shader here. I don't really remember why but
// the below is consistent with the old LWRP repos.

Shader "Hidden/Crest/Simulation/Update Shadow"
{
	SubShader
	{
		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			// #pragma enable_d3d11_debug_symbols

			// maybe this is the equivalent of the SHADOW_COLLECTOR_PASS define? inspired from com.unity.render-pipelines.universal\Shaders\Utils\ScreenSpaceShadows.shader
			#define _MAIN_LIGHT_SHADOWS_CASCADE

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "../OceanConstants.hlsl"
			#include "../OceanGlobals.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "../OceanInputsDriven.hlsl"
			#include "../OceanHelpersNew.hlsl"
			// noise functions used for jitter
			#include "../GPUNoise/GPUNoise.hlsl"

			StructuredBuffer<CascadeParams> _CascadeDataSrc;

			CBUFFER_START(CrestPerMaterial)
			// Settings._jitterDiameterSoft, Settings._jitterDiameterHard, Settings._currentFrameWeightSoft, Settings._currentFrameWeightHard
			float4 _JitterDiameters_CurrentFrameWeights;
			float _SimDeltaTime;

			float3 _CenterPos;
			float3 _Scale;
			float _LD_SliceIndex_Source;
			float4x4 _MainCameraProjectionMatrix;
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 _MainCameraCoords : TEXCOORD0;
				float3 _WorldPos : TEXCOORD1;
			};

			half ComputeShadow
			(
				in const float3 i_positionWS,
				in const float i_jitterDiameter
			)
			{
				half shadows = 1.0;
				float3 positionWS = i_positionWS;

				if (i_jitterDiameter > 0.0)
				{
					// Add jitter.
					positionWS.xz += i_jitterDiameter * (hash33(uint3(abs(positionWS.xz * 10.0), _Time.y * 120.0)) - 0.5).xy;
				}

				// Fetch shadow coordinates for cascade.
				float4 shadowCoords = TransformWorldToShadowCoord(positionWS);

				// Sample shadow map from main light.
				shadows = SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoords.xyz);
				// Apply shadow strength from main light.
				shadows = LerpWhiteTo(shadows, GetMainLightShadowStrength());
				// There will be no shadow data available beyond shadow far.
				shadows = BEYOND_SHADOW_FAR(shadowCoords) ? 1.0h : shadows;

				return shadows;
			}

			Varyings Vert(Attributes input)
			{
				Varyings o;

				o.positionCS = TransformObjectToHClip(input.positionOS);

				// world pos from [0,1] quad
				o._WorldPos.xyz = float3(input.positionOS.x - 0.5, 0.0, input.positionOS.y - 0.5) * _Scale * 4.0 + _CenterPos;
				o._WorldPos.y = _OceanCenterPosWorld.y;

				o._MainCameraCoords = mul(_MainCameraProjectionMatrix, float4(o._WorldPos.xyz, 1.0));

				return o;
			}

			real2 Frag(Varyings input) : SV_Target
			{
				const CascadeParams cascadeDataSrc = _CascadeDataSrc[_LD_SliceIndex_Source];
				half2 shadow = 0.0;
				const half r_max = 0.5 - cascadeDataSrc._oneOverTextureRes;

				float3 positionWS = input._WorldPos.xyz;

				float depth;
				{
					float width; float height;
					_LD_TexArray_Shadow_Source.GetDimensions(width, height, depth);
				}

				// Shadow from last frame - manually implement black border
				const float3 uv_source = WorldToUV(positionWS.xz, cascadeDataSrc, _LD_SliceIndex_Source);
				half2 r = abs(uv_source.xy - 0.5);
				if (max(r.x, r.y) <= r_max)
				{
					SampleShadow(_LD_TexArray_Shadow_Source, uv_source, 1.0, shadow);
				}
				else if (_LD_SliceIndex_Source + 1.0 < depth)
				{
					const float3 uv_source_nextlod = WorldToUV(positionWS.xz, _CascadeDataSrc[_LD_SliceIndex_Source + 1], _LD_SliceIndex_Source + 1);
					half2 r2 = abs(uv_source_nextlod.xy - 0.5);
					if (max(r2.x, r2.y) <= r_max)
					{
						SampleShadow(_LD_TexArray_Shadow_Source, uv_source_nextlod, 1.0, shadow);
					}
				}

				// Check if the current sample is visible in the main camera (and therefore shadow map can be sampled). This is required as the shadow buffer is
				// world aligned and surrounds viewer.
				float3 projected = input._MainCameraCoords.xyz / input._MainCameraCoords.w;
				if (projected.z < 1.0 && projected.z > 0.0 && abs(projected.x) < 1.0 && abs(projected.y) < 1.0)
				{
					half2 shadowThisFrame;

					// Add soft shadowing data.
					shadowThisFrame[CREST_SHADOW_INDEX_SOFT] = ComputeShadow
					(
						positionWS,
						_JitterDiameters_CurrentFrameWeights[CREST_SHADOW_INDEX_SOFT]
					);

					// Add hard shadowing data.
					shadowThisFrame[CREST_SHADOW_INDEX_HARD] = ComputeShadow
					(
						positionWS,
						_JitterDiameters_CurrentFrameWeights[CREST_SHADOW_INDEX_HARD]
					);

					shadowThisFrame = (half2)1.0 - saturate(shadowThisFrame);

					shadow = lerp(shadow, shadowThisFrame, _JitterDiameters_CurrentFrameWeights.zw * _SimDeltaTime * 60.0);
				}

				return shadow;
			}
			ENDHLSL
		}
	}
}
