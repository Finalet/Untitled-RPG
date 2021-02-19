// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// Helpers that will only be used for shaders (eg depth, lighting etc).

#ifndef CREST_OCEAN_SHADER_HELPERS_H
#define CREST_OCEAN_SHADER_HELPERS_H

// Same as LinearEyeDepth except supports orthographic projection. Use projection keywords to restrict support to either
// of these modes as an optimisation.
float CrestLinearEyeDepth(const float i_rawDepth)
{
#if !defined(_PROJECTION_ORTHOGRAPHIC)
	// Handles UNITY_REVERSED_Z for us.
	float perspective = LinearEyeDepth(i_rawDepth, _ZBufferParams);
#endif // _PROJECTION

#if !defined(_PROJECTION_PERSPECTIVE)
	// Orthographic Depth taken and modified from:
	// https://github.com/keijiro/DepthInverseProjection/blob/master/Assets/InverseProjection/Resources/InverseProjection.shader
	float near = _ProjectionParams.y;
	float far  = _ProjectionParams.z;
	float isOrthographic = unity_OrthoParams.w;

#if defined(UNITY_REVERSED_Z)
	float orthographic = lerp(far, near, i_rawDepth);
#else
	float orthographic = lerp(near, far, i_rawDepth);
#endif // UNITY_REVERSED_Z
#endif // _PROJECTION

#if defined(_PROJECTION_ORTHOGRAPHIC)
	return orthographic;
#elif defined(_PROJECTION_PERSPECTIVE)
	return perspective;
#else
	// If a shader does not have the projection enumeration, then assume they want to support both projection modes.
	return lerp(perspective, orthographic, isOrthographic);
#endif // _PROJECTION
}

#endif // CREST_OCEAN_SHADER_HELPERS_H
