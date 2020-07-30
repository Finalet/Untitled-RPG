﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// Renders alpha geometry overlaid on ocean surface. Samples the ocean shape texture in the vertex shader to track
// the surface. Requires the right texture to be assigned (see RenderAlphaOnSurface script).
Shader "Crest/Ocean Surface Alpha"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha("Alpha Multiplier", Range(0.0, 1.0)) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendModeSrc("Src Blend Mode", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendModeTgt("Tgt Blend Mode", Int) = 10
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }

		Pass
		{
			Blend [_BlendModeSrc] [_BlendModeTgt]

			ZWrite Off
			// Depth offset to stop intersection with water. "Factor" and "Units". typical seems to be (-1,-1). (-0.5,0) gives
			// pretty good results for me when alpha geometry is fairly well matched but fails when alpha geo is too low res.
			// the ludicrously large value below seems to work in most of my tests.
			Offset 0, -1000000

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#include "OceanGlobals.hlsl"
			#include "OceanInputsDriven.hlsl"
			#include "OceanLODData.hlsl"
			#include "OceanHelpersNew.hlsl"
			#include "OceanHelpers.hlsl"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _Alpha;

			struct Attributes
			{
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 uv_fogFactor : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float lodAlpha : TEXCOORD2;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				// move to world
				float3 worldPos;
				worldPos.xz = TransformObjectToWorld(input.positionOS).xz;
				worldPos.y = 0.0;

				// vertex snapping and lod transition
				float lodAlpha = ComputeLodAlpha(worldPos, _InstanceData.x);

				// sample shape textures - always lerp between 2 scales, so sample two textures

				// sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions)
				float wt_smallerLod = (1.0 - lodAlpha) * _LD_Params[_LD_SliceIndex].z;
				float wt_biggerLod = (1.0 - wt_smallerLod) * _LD_Params[_LD_SliceIndex + 1].z;
				// sample displacement textures, add results to current world pos / normal / foam
				const float2 wxz = worldPos.xz;
				half foam = 0.0;
				half sss = 0.;
				SampleDisplacements(_LD_TexArray_AnimatedWaves, WorldToUV(wxz), wt_smallerLod, worldPos, sss);
				SampleDisplacements(_LD_TexArray_AnimatedWaves, WorldToUV_BiggerLod(wxz), wt_biggerLod, worldPos, sss);

				// move to sea level
				worldPos.y += _OceanCenterPosWorld.y;

				// view-projection
				o.positionCS = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

				// For clip surface sampling
				o.worldPos = worldPos;
				o.lodAlpha = lodAlpha;

				o.uv_fogFactor.xy = TRANSFORM_TEX(input.uv, _MainTex);

				o.uv_fogFactor.z = ComputeFogFactor(o.positionCS.z);

				return o;
			}

			real4 Frag(Varyings input) : SV_Target
			{
				// We don't want decals etc floating on nothing
				ApplyOceanClipSurface(input.worldPos, input.lodAlpha);

				real4 col = tex2D(_MainTex, input.uv_fogFactor.xy);

				col.rgb = MixFog(col.rgb, input.uv_fogFactor.z);

				col.a *= _Alpha;

				return col;
			}
			ENDHLSL
		}
	}
}
