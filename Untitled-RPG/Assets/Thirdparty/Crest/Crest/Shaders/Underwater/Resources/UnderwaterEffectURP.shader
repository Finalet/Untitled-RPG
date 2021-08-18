// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

Shader "Hidden/Crest/Underwater/Underwater Effect URP"
{
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#pragma multi_compile_instancing

			// Use multi_compile because these keywords are copied over from the ocean material. With shader_feature,
			// the keywords would be stripped from builds. Unused shader variants are stripped using a build processor.
			#pragma multi_compile_local __ _SUBSURFACESCATTERING_ON
			#pragma multi_compile_local __ _SUBSURFACESHALLOWCOLOUR_ON
			#pragma multi_compile_local __ _TRANSPARENCY_ON
			#pragma multi_compile_local __ _CAUSTICS_ON
			#pragma multi_compile_local __ _SHADOWS_ON
			#pragma multi_compile_local __ _COMPILESHADERWITHDEBUGINFO_ON
			#pragma multi_compile_local __ _PROJECTION_PERSPECTIVE _PROJECTION_ORTHOGRAPHIC

			#pragma multi_compile_local __ CREST_MENISCUS
			#pragma multi_compile_local __ _FULL_SCREEN_EFFECT
			#pragma multi_compile_local __ _DEBUG_VIEW_OCEAN_MASK

			#if _COMPILESHADERWITHDEBUGINFO_ON
			#pragma enable_d3d11_debug_symbols
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#include "../../OceanGlobals.hlsl"
			#include "../../OceanInputsDriven.hlsl"
			#include "../../OceanShaderData.hlsl"
			#include "../../OceanHelpersNew.hlsl"
			#include "../../OceanShaderHelpers.hlsl"
			#include "../../OceanEmission.hlsl"

			TEXTURE2D_X(_CameraColorTexture);
			TEXTURE2D_X(_CrestOceanMaskTexture);
			TEXTURE2D_X(_CrestOceanMaskDepthTexture);

			#include "../UnderwaterEffectShared.hlsl"

			struct Attributes
			{
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 viewWS : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vert (Attributes input)
			{
				Varyings output;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				// Already a fullscreen triangle.
				output.positionCS = TransformObjectToHClip(input.positionOS);
				output.uv = input.uv;

				// Compute world space view vector
				output.viewWS = ComputeWorldSpaceView(output.uv);

				return output;
			}

			real4 Frag (Varyings input) : SV_Target
			{
				// We need this when sampling a screenspace texture.
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				float4 horizonPositionNormal; bool isBelowHorizon;
				GetHorizonData(input.uv, horizonPositionNormal, isBelowHorizon);

				const float2 uvScreenSpace = input.positionCS.xy;
				half3 sceneColour = LOAD_TEXTURE2D_X(_CameraColorTexture, uvScreenSpace).rgb;
				float rawDepth = LOAD_TEXTURE2D_X(_CameraDepthTexture, uvScreenSpace).x;
				const float mask = LOAD_TEXTURE2D_X(_CrestOceanMaskTexture, uvScreenSpace).x;
				const float rawOceanDepth = LOAD_TEXTURE2D_X(_CrestOceanMaskDepthTexture, uvScreenSpace).x;

				bool isOceanSurface; bool isUnderwater; float sceneZ;
				GetOceanSurfaceAndUnderwaterData(rawOceanDepth, mask, isBelowHorizon, rawDepth, isOceanSurface, isUnderwater, sceneZ, 0.0);

				float wt = ComputeMeniscusWeight(uvScreenSpace, mask, horizonPositionNormal, sceneZ);

#if _DEBUG_VIEW_OCEAN_MASK
				return DebugRenderOceanMask(isOceanSurface, isUnderwater, mask, sceneColour);
#endif // _DEBUG_VIEW_OCEAN_MASK

				if (isUnderwater)
				{
					const half3 view = normalize(input.viewWS);
					float3 scenePos = _WorldSpaceCameraPos - view * sceneZ / dot(unity_CameraToWorld._m02_m12_m22, -view);
					const Light lightMain = GetMainLight();
					const real3 lightDir = lightMain.direction;
					const real3 lightCol = lightMain.color;
					sceneColour = ApplyUnderwaterEffect(scenePos, sceneColour, lightCol, lightDir, rawDepth, sceneZ, view, isOceanSurface);
				}

				return half4(wt * sceneColour, 1.0);
			}
			ENDHLSL
		}
	}
}
