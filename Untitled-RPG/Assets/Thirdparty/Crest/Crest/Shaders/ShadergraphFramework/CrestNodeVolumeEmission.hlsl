// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"

#include "OceanGraphConstants.hlsl"
#include "../OceanShaderHelpers.hlsl"

// HB - pull these two functions in, because the ComputeClipSpacePosition flips the UV and it kills the position :(
float4 CrestComputeClipSpacePosition(float2 positionNDC, float deviceDepth)
{
	return float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);
}
float3 CrestComputeWorldSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invViewProjMatrix)
{
	float4 positionCS = CrestComputeClipSpacePosition(positionNDC, deviceDepth);
	float4 hpositionWS = mul(invViewProjMatrix, positionCS);
	return hpositionWS.xyz / hpositionWS.w;
}

// We take the unrefracted scene colour (i_sceneColourUnrefracted) as input because having a Scene Colour node in the graph
// appears to be necessary to ensure the scene colours are bound?
void CrestNodeSceneColour_half
(
	in const half i_refractionStrength,
	in const half3 i_scatterCol,
	in const half3 i_normalTS,
	in const float4 i_screenPos,
	in const float i_pixelZ,
	in const half3 i_sceneColourUnrefracted,
	in const float i_sceneZ,
	in const bool i_underwater,
	out half3 o_sceneColour,
	out float o_sceneDistance,
	out float3 o_scenePositionWS
)
{
	//#if _TRANSPARENCY_ON

	// View ray intersects geometry surface either above or below ocean surface

	half2 refractOffset = i_refractionStrength * i_normalTS.xy;
	if (!i_underwater)
	{
		// We're above the water, so behind interface is depth fog
		refractOffset *= min(1.0, 0.5 * (i_sceneZ - i_pixelZ)) / i_sceneZ;
	}
	const float4 screenPosRefract = i_screenPos + float4(refractOffset, 0.0, 0.0);
	const float sceneZRefractDevice = SHADERGRAPH_SAMPLE_SCENE_DEPTH(screenPosRefract.xy);

	// Depth fog & caustics - only if view ray starts from above water
	if (!i_underwater)
	{
		float sceneZRefract = CrestLinearEyeDepth(sceneZRefractDevice);

		// Compute depth fog alpha based on refracted position if it landed on an underwater surface, or on unrefracted depth otherwise
		if (sceneZRefract > i_pixelZ)
		{
			o_sceneDistance = sceneZRefract - i_pixelZ;

			o_sceneColour = SHADERGRAPH_SAMPLE_SCENE_COLOR(screenPosRefract.xy);

#if VERSION_GREATER_EQUAL(7, 4)
			o_scenePositionWS = ComputeWorldSpacePosition(screenPosRefract.xy, sceneZRefractDevice, UNITY_MATRIX_I_VP);
#else
			o_scenePositionWS = CrestComputeWorldSpacePosition(screenPosRefract.xy, sceneZRefractDevice, UNITY_MATRIX_I_VP);
#endif
		}
		else
		{
			// It seems that when MSAA is enabled this can sometimes be negative
			o_sceneDistance = max(i_sceneZ - i_pixelZ, 0.0);

			o_sceneColour = i_sceneColourUnrefracted;

			const float deviceDepthUnrefract = CrestLinearEyeDepth(i_sceneZ);
#if VERSION_GREATER_EQUAL(7, 4)
			o_scenePositionWS = ComputeWorldSpacePosition(i_screenPos.xy, deviceDepthUnrefract, UNITY_MATRIX_I_VP);
#else
			o_scenePositionWS = CrestComputeWorldSpacePosition(i_screenPos.xy, deviceDepthUnrefract, UNITY_MATRIX_I_VP);
#endif
		}
	}
	else
	{
		// Depth fog is handled by underwater shader
		o_sceneDistance = i_pixelZ;
		o_sceneColour = SHADERGRAPH_SAMPLE_SCENE_COLOR(screenPosRefract.xy);
#if VERSION_GREATER_EQUAL(7, 4)
		o_scenePositionWS = ComputeWorldSpacePosition(screenPosRefract.xy, sceneZRefractDevice, UNITY_MATRIX_I_VP);
#else
		o_scenePositionWS = CrestComputeWorldSpacePosition(screenPosRefract.xy, sceneZRefractDevice, UNITY_MATRIX_I_VP);
#endif
	}

	//#endif // _TRANSPARENCY_ON
}
