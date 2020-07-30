// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

Shader "Crest/Inputs/Animated Waves/Whirlpool"
{
	SubShader
	{
		Pass
		{
			Tags { "DisableBatching" = "True" }
			Blend One One

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			CBUFFER_START(UnityPerObject)
			float _Radius;
			float _Amplitude;
			float _Weight;
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 worldOffsetScaled : TEXCOORD0;
			};

			Varyings Vert(Attributes v)
			{
				Varyings o;
				o.positionCS = UnityObjectToClipPos(v.positionOS);

				float3 worldPos = mul(unity_ObjectToWorld, float4(v.positionOS, 1.0)).xyz;
				float3 centerPos = unity_ObjectToWorld._m03_m13_m23;
				o.worldOffsetScaled.xy = worldPos.xz - centerPos.xz;

				// shape is symmetric around center with known radius - fix the vert positions to perfectly wrap the shape.
				o.worldOffsetScaled.xy = sign(o.worldOffsetScaled.xy);
				float4 newWorldPos = float4(centerPos, 1.);
				newWorldPos.xz += o.worldOffsetScaled.xy * _Radius;
				o.positionCS = mul(UNITY_MATRIX_VP, newWorldPos);

				return o;
			}

			float4 Frag(Varyings i) : SV_Target
			{
				// power 4 smoothstep - no normalize needed
				// credit goes to stubbe's shadertoy: https://www.shadertoy.com/view/4ldSD2
				float r2 = dot( i.worldOffsetScaled.xy, i.worldOffsetScaled.xy);
				if (r2 > 1.0)
					return (float4)0.0;

				r2 = 1.0 - r2;

				float y = r2 * r2 * _Amplitude * -1.0;

				return float4(0.0, _Weight * y, 0.0, 0.0);
			}
			ENDCG
		}
	}
}
