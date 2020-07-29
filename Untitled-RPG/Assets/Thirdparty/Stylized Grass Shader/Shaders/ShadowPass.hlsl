//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
#define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED

float3 _LightDirection;

struct Attributes
{
	float4 positionOS   : POSITION;
	float3 normalOS     : NORMAL;
	float2 texcoord     : TEXCOORD0;
	float4 color        : COLOR0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float2 uv           : TEXCOORD0;
	float4 positionCS   : SV_POSITION;
	float3 positionWS   : TEXCOORD1;
};

#define AO_MASK input.color.r
#define BEND_MASK input.color.r

Varyings ShadowPassVertex(Attributes input)
{
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);

	output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

	float posOffset = ObjectPosRand01();

	//Compose parameter structs
	WindSettings wind = PopulateWindSettings(_WindAmbientStrength, _WindSpeed, _WindDirection, _WindSwinging, AO_MASK, _WindObjectRand, _WindVertexRand, _WindRandStrength, _WindGustStrength, _WindGustFreq);
	BendSettings bending = PopulateBendSettings(_BendMode, BEND_MASK, _BendPushStrength, _BendFlattenStrength, _PerspectiveCorrection);

	VertexData vertexData = GetVertexData(input.positionOS, 0, posOffset, wind, bending);

	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(vertexData.positionWS, vertexData.normalWS, _LightDirection));

	output.positionCS = positionCS;

#if UNITY_REVERSED_Z
	output.positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
	output.positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

	output.positionWS.xyz = vertexData.positionWS;

	return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
#ifdef _ALPHATEST_ON
	float alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
	AlphaClip(alpha, _Cutoff, input.positionCS.xyz, input.positionWS.xyz, _FadeParams);
#endif
	return 0;
}
#endif