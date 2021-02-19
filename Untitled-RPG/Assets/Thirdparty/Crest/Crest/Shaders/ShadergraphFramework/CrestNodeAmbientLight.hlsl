// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"

void CrestNodeAmbientLight_half
(
	out half3 o_ambientLight
)
{
	// Use the constant term (0th order) of SH stuff - this is the average
	o_ambientLight = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
}
