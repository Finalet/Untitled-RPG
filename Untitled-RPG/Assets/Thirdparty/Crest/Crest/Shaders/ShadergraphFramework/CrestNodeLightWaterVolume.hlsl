// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "../OceanConstants.hlsl"
#include "../OceanGlobals.hlsl"
#include "../OceanHelpersNew.hlsl"

void CrestNodeLightWaterVolume_half
(
	const half3 i_scatterColourBase,
	const half3 i_scatterColourShadow,
	const half3 i_scatterColourShallow,
	const half i_scatterColourShallowDepthMax,
	const half i_scatterColourShallowDepthFalloff,
	const half i_sssIntensityBase,
	const half i_sssIntensitySun,
	const half3 i_sssTint,
	const half i_sssSunFalloff,
	const half i_surfaceOceanWaterDepth,
	const half2 i_shadow,
	const half i_sss,
	const half3 i_viewNorm,
	const half3 i_positionWS,
	const half3 i_ambientLighting,
	const half3 i_primaryLightDirection,
	const half3 i_primaryLightIntensity,
	out half3 o_volumeLight
)
{
	half depth;
	half waveHeight;
	half shadow;
	//	if (i_underwater)
	//	{
	//		// compute scatter colour from cam pos. two scenarios this can be called:
	//		// 1. rendering ocean surface from bottom, in which case the surface may be some distance away. use the scatter
	//		//    colour at the camera, not at the surface, to make sure its consistent.
	//		// 2. for the underwater skirt geometry, we don't have the lod data sampled from the verts with lod transitions etc,
	//		//    so just approximate by sampling at the camera position.
	//		// this used to sample LOD1 but that doesnt work in last LOD, the data will be missing.
	//		const float2 uv_0 = LD_0_WorldToUV(i_cameraPos.xz);
	//		depth = CREST_OCEAN_DEPTH_BASELINE;
	//		SampleSeaDepth(_LD_Sampler_SeaFloorDepth_0, uv_0, 1.0, depth);
	//		waveHeight = 0.0;
	//
	//#if _SHADOWS_ON
	//		real2 shadowSoftHard = 0.0;
	//		SampleShadow(_LD_Sampler_Shadow_0, uv_0, 1.0, shadowSoftHard);
	//		shadow = 1.0 - shadowSoftHard.x;
	//#endif
	//	}
	//	else
	{
		// above water - take data from geometry
		depth = i_surfaceOceanWaterDepth;
		waveHeight = i_positionWS.y - _OceanCenterPosWorld.y;
		shadow = 1.0 - i_shadow.x;
	}

	// base colour
	o_volumeLight = i_scatterColourBase;
	
	//#if _SHADOWS_ON
	o_volumeLight = lerp(i_scatterColourShadow, o_volumeLight, shadow);
	//#endif

	//#if _SUBSURFACESCATTERING_ON
	//	{
	//#if _SUBSURFACESHALLOWCOLOUR_ON
	float shallowness = pow(1.0 - saturate(depth / i_scatterColourShallowDepthMax), i_scatterColourShallowDepthFalloff);
	half3 shallowCol = i_scatterColourShallow;
	//#if _SHADOWS_ON
	//		shallowCol = lerp(_SubSurfaceShallowColShadow, shallowCol, shadow);
	//#endif
	o_volumeLight = lerp(o_volumeLight, shallowCol, shallowness);
	//#endif
	//
			// Light the base colour. Use the constant term (0th order) of SH stuff - this is the average. Use the primary light integrated over the
			// hemisphere (divide by pi).
	o_volumeLight *= i_ambientLighting + shadow * i_primaryLightIntensity / 3.14159;

	// Approximate subsurface scattering - add light when surface faces viewer. Use geometry normal - don't need high freqs.
	half towardsSun = pow(max(0.0, dot(i_primaryLightDirection, -i_viewNorm)), i_sssSunFalloff);

	float v = abs(i_viewNorm.y);
	half3 subsurface = (i_sssIntensityBase + i_sssIntensitySun * towardsSun) * i_sssTint * i_primaryLightIntensity * shadow;
	subsurface *= (1.0 - v * v) * i_sss;
	o_volumeLight += subsurface;

	//	}
	//#endif // _SUBSURFACESCATTERING_ON
	//
	//	// outscatter light - attenuate the final colour by the camera depth under the water, to approximate less
	//	// throughput due to light scatter as the camera gets further under water.
	//	if (i_outscatterLight)
	//	{
	//		half camDepth = max(_OceanCenterPosWorld.y - _WorldSpaceCameraPos.y, 0.0);
	//		if (i_underwater)
	//		{
	//			o_volumeLight *= exp(-_DepthFogDensity.xyz * camDepth * DEPTH_OUTSCATTER_CONSTANT);
	//		}
	//	}
}
