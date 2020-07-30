// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

Shader "Crest/Copy Depth Buffer Into Cache"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

		Pass
		{
			Name "CopyDepthBufferIntoCache"
			ZTest Always ZWrite Off Blend Off

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			CBUFFER_START(CrestPerOceanInput)
			float4 _HeightNearHeightFar;
			float4 _CustomZBufferParams;
			float3 _OceanCenterPosWorld;
			float4 _LD_Params_0;
			float4 _LD_Params_1;
			float3 _LD_Pos_Scale_0;
			float3 _LD_Pos_Scale_1;
			float3 _GeomData;
			CBUFFER_END

			TEXTURE2D_FLOAT(_CamDepthBuffer);
			SAMPLER(sampler_CamDepthBuffer);

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float2 uv           : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS   : SV_POSITION;
				float2 uv           : TEXCOORD0;
			};

			Varyings vert(Attributes input)
			{
				Varyings output;
				output.uv = input.uv;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				return output;
			}

			float CustomLinear01Depth(float z)
			{
				z *= _CustomZBufferParams.x;
				return (1.0 - z) / _CustomZBufferParams.y;
			}

			float frag(Varyings input) : SV_Target
			{
				float deviceDepth = SAMPLE_DEPTH_TEXTURE(_CamDepthBuffer, sampler_CamDepthBuffer, input.uv);
				float linear01Z = CustomLinear01Depth(deviceDepth);

				float altitude;
#if UNITY_REVERSED_Z
					altitude = lerp(_HeightNearHeightFar.y, _HeightNearHeightFar.x, linear01Z);
#else
					altitude = lerp(_HeightNearHeightFar.x, _HeightNearHeightFar.y, linear01Z);
#endif

				return _OceanCenterPosWorld.y - altitude;
			}

			ENDHLSL
		}
	}
}
