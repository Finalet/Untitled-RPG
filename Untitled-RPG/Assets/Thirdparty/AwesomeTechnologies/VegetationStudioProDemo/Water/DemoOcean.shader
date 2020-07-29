Shader "AwesomeTechnologies/Demo/DemoOcean"
{
	Properties
	{
		_WaterNormal("Water Normal", 2D) = "bump" {}
		_NormalScale("Normal Scale", Float) = 0
		_DeepColor("Deep Color", Color) = (0,0,0,0)
		_ShalowColor("Shalow Color", Color) = (1,1,1,0)
		_WaterDepth("Water Depth", Float) = 0
		_WaterFalloff("Water Falloff", Float) = 0
		_Distortion("Distortion", Float) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform half _NormalScale;
		uniform sampler2D _WaterNormal;
		uniform float4 _WaterNormal_ST;
		uniform half4 _DeepColor;
		uniform half4 _ShalowColor;
		uniform sampler2D _CameraDepthTexture;
		uniform half _WaterDepth;
		uniform half _WaterFalloff;
		uniform sampler2D _GrabTexture;
		uniform half _Distortion;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_WaterNormal = i.uv_texcoord * _WaterNormal_ST.xy + _WaterNormal_ST.zw;
			float2 panner22 = ( uv_WaterNormal + 1.0 * _Time.y * float2( -0.03,0 ));
			float2 panner19 = ( uv_WaterNormal + 1.0 * _Time.y * float2( 0.04,0.04 ));
			float3 temp_output_24_0 = BlendNormals( UnpackScaleNormal( tex2D( _WaterNormal, panner22 ) ,_NormalScale ) , UnpackScaleNormal( tex2D( _WaterNormal, panner19 ) ,_NormalScale ) );
			o.Normal = temp_output_24_0;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
#if SHADER_API_METAL	
			float eyeDepth1 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos.z))));
#else
			float eyeDepth1 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(ase_screenPos))));
#endif
			float temp_output_94_0 = saturate( pow( ( abs( ( eyeDepth1 - ase_screenPos.w ) ) + _WaterDepth ) , _WaterFalloff ) );
			float4 lerpResult13 = lerp( _DeepColor , _ShalowColor , temp_output_94_0);
			float4 lerpResult117 = lerp( lerpResult13 , half4(1,1,1,0) , float4( 0.0,0,0,0 ));
			float4 ase_screenPos164 = ase_screenPos;
			#if UNITY_UV_STARTS_AT_TOP
			float scale164 = -1.0;
			#else
			float scale164 = 1.0;
			#endif
			float halfPosW164 = ase_screenPos164.w * 0.5;
			ase_screenPos164.y = ( ase_screenPos164.y - halfPosW164 ) * _ProjectionParams.x* scale164 + halfPosW164;
			ase_screenPos164.xyzw /= ase_screenPos164.w;
			float4 screenColor65 = tex2D( _GrabTexture, ( half3( (ase_screenPos164).xy ,  0.0 ) + ( temp_output_24_0 * _Distortion ) ).xy );
			float4 lerpResult93 = lerp( lerpResult117 , screenColor65 , temp_output_94_0);
			o.Albedo = lerpResult93.rgb;
			o.Occlusion = 0.0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}