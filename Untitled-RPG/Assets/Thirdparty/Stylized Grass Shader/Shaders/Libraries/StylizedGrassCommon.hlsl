//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//7.1.8: true
//7.2.0: false
//7.3.1: false
//7.4.1: false
//8.0.1: true
//8.1.0: true
//9.0.1: true
#if VERSION_EQUAL(8,0) || VERSION_EQUAL(8,1) || VERSION_EQUAL(9,0)
#define FLIP_UV
#endif
#if VERSION_EQUAL(7,2) || VERSION_EQUAL(7,3) || VERSION_EQUAL(7,4) 
#undef FLIP_UV
#endif

#include "Bending.hlsl"
#include "Wind.hlsl"

//---------------------------------------------------------------//
//Utils

float3 CameraPositionWS(float3 wPos)
{
	return _WorldSpaceCameraPos;

	/*
	//Not using _WorldSpaceCameraPos, since it doesn't have correct values during shadow and vertex passes *shrug*
	//https://issuetracker.unity3d.com/issues/shadows-flicker-by-moving-the-camera-when-shader-is-using-worldspacecamerpos-and-terrain-has-draw-enabled-for-trees-and-details

#if defined(SHADERPASS_SHADOWS) || defined(SHADERPASS_DEPTH_ONLY) //Fragment stage of depth/shadow passes
	return UNITY_MATRIX_I_V._m03_m13_m23;
#else //Fragment stage
	return _WorldSpaceCameraPos;
#endif
*/
}

float ObjectPosRand01() {
	return frac(UNITY_MATRIX_M[0][3] + UNITY_MATRIX_M[1][3] + UNITY_MATRIX_M[2][3]);
}

float3 GetPivotPos() {
	return float3(UNITY_MATRIX_M[0][3], UNITY_MATRIX_M[1][3] + 0.25, UNITY_MATRIX_M[2][3]);
}

float FadeFactor(float3 wPos, float4 params)
{
	if (params.z == 0) return 0;

	float pixelDist = length(CameraPositionWS(wPos).xyz - wPos.xyz);

	//Distance based scalar
	return saturate((pixelDist - params.x) / params.y);
}

//Incorperates LOD dithering so only one clip operation is performed
void AlphaClip(float alpha, float cutoff, float3 clipPos, float3 wPos, float4 fadeParams)
{
	float f = 1;

	f -= FadeFactor(wPos, fadeParams);

	//Does not work, current and next LOD both have the same LODFade value. Unity bug?
	/*
#ifdef LOD_FADE_CROSSFADE
	float p = GenerateHashedRandomFloat(clipPos.xy);
	f *= unity_LODFade.x - CopySign(p, unity_LODFade.x);
#endif
*/

#ifdef _ALPHATEST_ON
	clip((alpha * f) - cutoff);
#endif
}

//---------------------------------------------------------------//
//Vertex transformation

//Struct that holds both VertexPositionInputs and VertexNormalInputs
struct VertexData {
	//Positions
	float3 positionWS; // World space position
	float3 positionVS; // View space position
	float4 positionCS; // Homogeneous clip space position
	float4 positionNDC;// Homogeneous normalized device coordinates

	//Normals
#if _NORMALMAP 
#if !defined(SHADERPASS_SHADOWS) || !defined(SHADERPASS_DEPTH_ONLY)
	real3 tangentWS;
	real3 bitangentWS;
#endif
#endif
	float3 normalWS;
};

//Combination of GetVertexPositionInputs and GetVertexNormalInputs with bending
VertexData GetVertexData(float4 positionOS, float3 normalOS, float rand, WindSettings s, BendSettings b)
{
	VertexData data = (VertexData)0;

	float3 wPos = TransformObjectToWorld(positionOS.xyz);

	//Ensure the grass always bends down, even in negative directions (reverse abs)
	//bendWeight = (bendWeight < 0) ? -bendWeight : bendWeight;

	float3 worldPos = lerp(wPos, GetPivotPos(), b.mode);
	float4 windVec = GetWindOffset(positionOS.xyz, wPos, rand, s);
	float4 bendVec = GetBendOffset(worldPos, b);

	float3 offsets = lerp(windVec.xyz, bendVec.xyz, bendVec.a);

	half3 viewDirectionWS = SafeNormalize(CameraPositionWS(wPos).xyz - wPos);
	float NdotV = dot(float3(0, 1, 0), viewDirectionWS);

	//Avoid pushing grass straight underneath the camera in a falloff of 4 units (1/0.25)
	float dist = saturate(distance(wPos.xz, CameraPositionWS(wPos).xz) * 0.25);

	//Push grass away from camera position
	float2 pushVec = -viewDirectionWS.xz;
	float perspMask = b.mask * b.perspectiveCorrection * dist * NdotV;
	offsets.xz += pushVec.xy * perspMask;

	//Apply bend offset
	wPos.xz += offsets.xz;
	wPos.y -= offsets.y;

	//Vertex positions in various coordinate spaces
	data.positionWS = wPos;
	data.positionVS = TransformWorldToView(data.positionWS);
	data.positionCS = TransformWorldToHClip(data.positionWS);

	float4 ndc = data.positionCS * 0.5f;
	data.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
	data.positionNDC.zw = data.positionCS.zw;

#if !defined(SHADERPASS_SHADOWS) || !defined(SHADERPASS_DEPTH_ONLY) //Skip normal derivative during shadow and depth pass

#if ADVANCED_LIGHTING
	//Normals
	float3 oPos = TransformWorldToObject(wPos); //object-space position after displacement in world-space
	float3 bentNormals = lerp(normalOS, normalize(positionOS.xyz - oPos), length(offsets)); //weight is length of wind/bend vector
#else
	float3 bentNormals = normalOS;
#endif

#if _NORMALMAP
	data.tangentWS = real3(1.0, 0.0, 0.0);
	data.bitangentWS = real3(0.0, 1.0, 0.0);
#endif
	data.normalWS = TransformObjectToWorldNormal(bentNormals);
#endif

	return data;
}