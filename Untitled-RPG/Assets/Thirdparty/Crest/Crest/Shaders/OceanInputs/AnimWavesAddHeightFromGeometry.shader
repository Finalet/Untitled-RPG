﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// This adds the height from the geometry. This allows setting the water height to some level for rivers etc, but still
// getting the waves added on top.

Shader "Crest/Inputs/Animated Waves/Add Water Height From Geometry"
{
	Properties
	{
		[Enum(BlendOp)] _BlendOp("Blend Op", Int) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendModeSrc("Src Blend Mode", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendModeTgt("Tgt Blend Mode", Int) = 1
		[Enum(ColorWriteMask)] _ColorWriteMask("Color Write Mask", Int) = 15
	}

	SubShader
	{
		Pass
		{
			BlendOp [_BlendOp]
			Blend [_BlendModeSrc] [_BlendModeTgt]
			ColorMask [_ColorWriteMask]

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"
			#include "../OceanGlobals.hlsl"

			CBUFFER_START(CrestPerOceanInput)
			float _Weight;
			float3 _DisplacementAtInputPosition;
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;

				o.worldPos = mul(unity_ObjectToWorld, float4(input.positionOS, 1.0)).xyz;
				// Correct for displacement
				o.worldPos.xz -= _DisplacementAtInputPosition.xz;

				o.positionCS = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.0));

				return o;
			}

			half4 Frag(Varyings input) : SV_Target
			{
				// Write displacement to get from sea level of ocean to the y value of this geometry
				float addHeight = input.worldPos.y - _OceanCenterPosWorld.y;
				return _Weight * half4(0.0, addHeight, 0.0, 0.0);
			}
			ENDCG
		}
	}
}
