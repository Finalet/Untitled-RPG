// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanShaderHelpers.hlsl"

void CrestNodeLinearEyeDepth_float
(
	in const float i_rawDepth,
	out float o_linearDepth
)
{
	o_linearDepth = CrestLinearEyeDepth(i_rawDepth);
}
