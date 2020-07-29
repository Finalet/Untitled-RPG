//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED

struct Attributes
{
	float4 positionOS     : POSITION;
	float2 texcoord     : TEXCOORD0;
	float4 color		: COLOR0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float2 uv           : TEXCOORD0;
	float4 positionCS   : SV_POSITION;
	float4 positionWS   : TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

Varyings DepthOnlyVertex(Attributes input)
{
	Varyings output = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

	float posOffset = ObjectPosRand01();

	WindSettings wind = PopulateWindSettings(_WindAmbientStrength, _WindSpeed, _WindDirection, _WindSwinging, input.color.r, _WindObjectRand, _WindVertexRand, _WindRandStrength, _WindGustStrength, _WindGustFreq);
	BendSettings bending = PopulateBendSettings(_BendMode, input.color.r, _BendPushStrength, _BendFlattenStrength, _PerspectiveCorrection);

	VertexData vertexData = GetVertexData(input.positionOS, 0, posOffset, wind, bending);

	output.positionCS = vertexData.positionCS;
	output.positionWS.xyz = vertexData.positionWS;

	return output;
}

half4 DepthOnlyFragment(Varyings input) : SV_TARGET
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#ifdef _ALPHATEST_ON
	float alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
	AlphaClip(alpha, _Cutoff, input.positionCS.xyz, input.positionWS.xyz, _FadeParams);
#endif

	return 0;
}
#endif