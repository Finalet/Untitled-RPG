// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "../OceanConstants.hlsl"
#include "../OceanGlobals.hlsl"
#include "../OceanHelpersNew.hlsl"
#include "../OceanInputsDriven.hlsl"

void CrestComputeSamplingData_half
(
	in const float2 worldXZ,
	out float lodAlpha,
	out float3 o_oceanPosScale0,
	out float3 o_oceanPosScale1,
	out float4 o_oceanParams0,
	out float4 o_oceanParams1,
	out float slice0,
	out float slice1
)
{
	PosToSliceIndices(worldXZ, 0.0, _LD_Pos_Scale[0].z, _LD_Pos_Scale[0].z, slice0, slice1, lodAlpha);
	
	uint si0 = (uint)slice0;
	uint si1 = si0 + 1;

	o_oceanPosScale0 = _LD_Pos_Scale[si0];
	o_oceanPosScale1 = _LD_Pos_Scale[si1];

	o_oceanParams0 = _LD_Params[si0];
	o_oceanParams1 = _LD_Params[si1];
}
