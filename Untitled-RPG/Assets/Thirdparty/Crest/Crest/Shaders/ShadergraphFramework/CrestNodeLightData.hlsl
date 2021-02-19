// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// Based on tutorial: https://connect.unity.com/p/adding-your-own-hlsl-code-to-shader-graph-the-custom-function-node

#include "OceanGraphConstants.hlsl"

void CrestNodeLightData_half
(
	out half3 o_direction,
	out half3 o_colour
)
{
#if SHADERGRAPH_PREVIEW
	//Hardcoded data, used for the preview shader inside the graph
	//where light functions are not available
	o_direction = -normalize(float3(-0.5, 0.5, -0.5));
	o_colour = float3(1.0, 1.0, 1.0);
#else
	//Actual light data from the pipeline
	Light light = GetMainLight();
	o_direction = light.direction;
	o_colour = light.color;
#endif
}
