// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "../OceanInputsDriven.hlsl"

void CrestOceanSurfaceValues_half
(
	out float o_meshScaleAlpha,
	out float o_lodDataTexelSize,
	out float o_geometryGridSize,
	out float3 o_oceanPosScale0,
	out float3 o_oceanPosScale1,
	out float4 o_oceanParams0,
	out float4 o_oceanParams1,
	out float o_sliceIndex0
)
{
	o_sliceIndex0 = _LD_SliceIndex;

	uint si0 = (uint)o_sliceIndex0;
	uint si1 = si0 + 1;

	o_oceanPosScale0 = _LD_Pos_Scale[si0];
	o_oceanPosScale1 = _LD_Pos_Scale[si1];

	o_oceanParams0 = _LD_Params[si0];
	o_oceanParams1 = _LD_Params[si1];

	o_meshScaleAlpha = _InstanceData.x;

	o_lodDataTexelSize = _GeomData.x;
	o_geometryGridSize = _GeomData.y;
}
