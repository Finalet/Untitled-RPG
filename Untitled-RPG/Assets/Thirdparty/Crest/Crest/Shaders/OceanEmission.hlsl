// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#ifndef CREST_OCEAN_EMISSION_INCLUDED
#define CREST_OCEAN_EMISSION_INCLUDED

half3 ScatterColour(
	in const half3 i_ambientLighting, in const half i_surfaceOceanDepth, in const float3 i_cameraPos,
	in const half3 i_lightDir, in const half3 i_view, in const float i_shadow,
	in const bool i_underwater, in const bool i_outscatterLight, const half3 lightColour, half sss,
	in const float i_meshScaleLerp, in const float i_scaleBase,
	in const CascadeParams cascadeData0)
{
	half depth;
	half shadow = 1.0;
	if (i_underwater)
	{
		// compute scatter colour from cam pos. two scenarios this can be called:
		// 1. rendering ocean surface from bottom, in which case the surface may be some distance away. use the scatter
		//    colour at the camera, not at the surface, to make sure its consistent.
		// 2. for the underwater skirt geometry, we don't have the lod data sampled from the verts with lod transitions etc,
		//    so just approximate by sampling at the camera position.
		// this used to sample LOD1 but that doesnt work in last LOD, the data will be missing.
		const float3 uv_smallerLod = WorldToUV(i_cameraPos.xz, cascadeData0, _LD_SliceIndex);
		depth = CREST_OCEAN_DEPTH_BASELINE;
		SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_smallerLod, 1.0, depth);

#if _SHADOWS_ON
		const float2 samplePoint = i_cameraPos.xz;

		// Pick lower res data for shadowing, helps to smooth out artifacts slightly
		const float minSliceIndex = 4.0;
		uint slice0, slice1; float lodAlpha;
		PosToSliceIndices(samplePoint, minSliceIndex, i_scaleBase, slice0, slice1, lodAlpha);

		half2 shadowSoftHard = 0.0;
		{
			const float3 uv = WorldToUV(samplePoint, _CrestCascadeData[slice0], slice0);
			SampleShadow(_LD_TexArray_Shadow, uv, 1.0 - lodAlpha, shadowSoftHard);
		}
		{
			const float3 uv = WorldToUV(samplePoint, _CrestCascadeData[slice1], slice1);
			SampleShadow(_LD_TexArray_Shadow, uv, lodAlpha, shadowSoftHard);
		}

		shadow = saturate(1.0 - shadowSoftHard.x);
#endif
	}
	else
	{
		// above water - take data from geometry
		depth = i_surfaceOceanDepth;
		shadow = i_shadow;
	}

	// base colour
	float v = abs(i_view.y);
	half3 col = lerp(_Diffuse, _DiffuseGrazing, 1. - pow(v, 1.0));

#if _SHADOWS_ON
	col = lerp(_DiffuseShadow, col, shadow);
#endif

#if _SUBSURFACESCATTERING_ON
	{
#if _SUBSURFACESHALLOWCOLOUR_ON
		float shallowness = pow(1. - saturate(depth / _SubSurfaceDepthMax), _SubSurfaceDepthPower);
		half3 shallowCol = _SubSurfaceShallowCol;
#if _SHADOWS_ON
		shallowCol = lerp(_SubSurfaceShallowColShadow, shallowCol, shadow);
#endif
		col = lerp(col, shallowCol, shallowness);
#endif

		col *= i_ambientLighting + lightColour;

		// Approximate subsurface scattering - add light when surface faces viewer. Use geometry normal - don't need high freqs.
		half towardsSun = pow(max(0., dot(i_lightDir, -i_view)), _SubSurfaceSunFallOff);
		// URP version was: col += (_SubSurfaceBase + _SubSurfaceSun * towardsSun) * _SubSurfaceColour.rgb * lightColour * shadow;
		half3 subsurface = (_SubSurfaceBase + _SubSurfaceSun * towardsSun) * _SubSurfaceColour.rgb * lightColour * shadow;
		if (!i_underwater)
		{
			subsurface *= (1.0 - v * v) * sss;
		}
		col += subsurface;
	}
#endif // _SUBSURFACESCATTERING_ON

	return col;
}


#if _CAUSTICS_ON
void ApplyCaustics(in const float3 i_scenePos, in const half3 i_lightDir, in const float i_sceneZ, in sampler2D i_normals, in const bool i_underwater, inout half3 io_sceneColour,
	in const CascadeParams cascadeData0, in const CascadeParams cascadeData1)
{
	// could sample from the screen space shadow texture to attenuate this..
	// underwater caustics - dedicated to P
	const float3 scenePosUV = WorldToUV(i_scenePos.xz, cascadeData1, _LD_SliceIndex + 1);

	float3 disp = 0.0;
	// this gives height at displaced position, not exactly at query position.. but it helps. i cant pass this from vert shader
	// because i dont know it at scene pos.
	SampleDisplacements(_LD_TexArray_AnimatedWaves, scenePosUV, 1.0, disp);
	half waterHeight = _OceanCenterPosWorld.y + disp.y;
	half sceneDepth = waterHeight - i_scenePos.y;
	// Compute mip index manually, with bias based on sea floor depth. We compute it manually because if it is computed automatically it produces ugly patches
	// where samples are stretched/dilated. The bias is to give a focusing effect to caustics - they are sharpest at a particular depth. This doesn't work amazingly
	// well and could be replaced.
	float mipLod = log2(max(i_sceneZ, 1.0)) + abs(sceneDepth - _CausticsFocalDepth) / _CausticsDepthOfField;
	// project along light dir, but multiply by a fudge factor reduce the angle bit - compensates for fact that in real life
	// caustics come from many directions and don't exhibit such a strong directonality
	// Removing the fudge factor (4.0) will cause the caustics to move around more with the waves. But this will also
	// result in stretched/dilated caustics in certain areas. This is especially noticeable on angled surfaces.
	float2 surfacePosXZ = i_scenePos.xz + i_lightDir.xz * sceneDepth / (4.*i_lightDir.y);
	float4 cuv1 = float4((surfacePosXZ / _CausticsTextureScale + float2(0.044*_CrestTime + 17.16, -0.169*_CrestTime)), 0., mipLod);
	float4 cuv2 = float4((1.37*surfacePosXZ / _CausticsTextureScale + float2(0.248*_CrestTime, 0.117*_CrestTime)), 0., mipLod);

	// We'll use this distortion code for above water in single pass due to refraction bug.
#if !defined(UNITY_SINGLE_PASS_STEREO) && !defined(UNITY_STEREO_INSTANCING_ENABLED)
	if (i_underwater)
#endif
	{
		// Add distortion if we're not getting the refraction
		half2 causticN = _CausticsDistortionStrength * UnpackNormal(tex2D(i_normals, surfacePosXZ / _CausticsDistortionScale)).xy;
		cuv1.xy += 1.30 * causticN;
		cuv2.xy += 1.77 * causticN;
	}

	half causticsStrength = _CausticsStrength;

#if _SHADOWS_ON
	{
		// Calculate projected position again as we do not want the fudge factor. If we include the fudge factor, the
		// caustics will not be aligned with shadows.
		const float2 shadowSurfacePosXZ = i_scenePos.xz + i_lightDir.xz * sceneDepth / i_lightDir.y;
		half2 causticShadow = 0.0;
		// As per the comment for the underwater code in ScatterColour,
		// LOD_1 data can be missing when underwater
		if (i_underwater)
		{
			const float3 uv_smallerLod = WorldToUV(shadowSurfacePosXZ, cascadeData0, _LD_SliceIndex);
			SampleShadow(_LD_TexArray_Shadow, uv_smallerLod, 1.0, causticShadow);
		}
		else
		{
			// only sample the bigger lod. if pops are noticeable this could lerp the 2 lods smoothly, but i didnt notice issues.
			const float3 uv_biggerLod = WorldToUV(shadowSurfacePosXZ, cascadeData1, _LD_SliceIndex + 1);
			SampleShadow(_LD_TexArray_Shadow, uv_biggerLod, 1.0, causticShadow);
		}
		causticsStrength *= 1.0 - causticShadow.y;
	}
#endif // _SHADOWS_ON

	io_sceneColour.xyz *= 1.0 + causticsStrength *
		(0.5*tex2Dlod(_CausticsTexture, cuv1).xyz + 0.5*tex2Dlod(_CausticsTexture, cuv2).xyz - _CausticsTextureAverage);
}
#endif // _CAUSTICS_ON

#if defined(UNITY_COMMON_INCLUDED)
half3 OceanEmission(in const half3 i_view, in const half3 i_n_pixel, in const half3 i_lightCol, in const float3 i_lightDir,
	in const real3 i_grabPosXYW, in const float i_pixelZ, in const half2 i_uvDepth, in const float i_sceneZ,
	in const half3 i_bubbleCol, in sampler2D i_normals, in const bool i_underwater, in const half3 i_scatterCol,
	in const CascadeParams cascadeData0, in const CascadeParams cascadeData1)
{
	half3 col = i_scatterCol;

	// underwater bubbles reflect in light
	col += i_bubbleCol;

#if _TRANSPARENCY_ON

	// View ray intersects geometry surface either above or below ocean surface

	const half2 uvBackground = i_grabPosXYW.xy / i_grabPosXYW.z;
	half3 sceneColour;
	half3 alpha = 0.;
	float depthFogDistance;

	// Depth fog & caustics - only if view ray starts from above water
	if (!i_underwater)
	{
		const half2 refractOffset = _RefractionStrength * i_n_pixel.xz * min(1.0, 0.5*(i_sceneZ - i_pixelZ)) / i_sceneZ;
		half2 uvBackgroundRefract = uvBackground + refractOffset;

		// Raw depth is logarithmic for perspective, and linear (0-1) for orthographic.
		const float sceneZRefractDevice = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i_uvDepth + refractOffset)).x;
		const float sceneZRefract = CrestLinearEyeDepth(sceneZRefractDevice);

		float2 scenePosNDC = uvBackground;

		// Compute depth fog alpha based on refracted position if it landed on an underwater surface, or on unrefracted depth otherwise
		if (sceneZRefract > i_pixelZ)
		{
			depthFogDistance = sceneZRefract - i_pixelZ;
			scenePosNDC += refractOffset;
		}
		else
		{
			// It seems that when MSAA is enabled this can sometimes be negative
			depthFogDistance = max(i_sceneZ - i_pixelZ, 0.0);

			// We have refracted onto a surface in front of the water. Cancel the refraction offset.
			uvBackgroundRefract = uvBackground;
		}

		sceneColour = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, UnityStereoTransformScreenSpaceTex(uvBackgroundRefract)).rgb;
#if _CAUSTICS_ON
		// Refractions don't work correctly in single pass. Use same code from underwater instead for now.
#if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_STEREO_INSTANCING_ENABLED)
		float3 scenePos = (((i_view) / dot(i_view, unity_CameraToWorld._m02_m12_m22)) * i_sceneZ) + _WorldSpaceCameraPos;
#else
		float3 scenePos = ComputeWorldSpacePosition(scenePosNDC, sceneZRefractDevice, UNITY_MATRIX_I_VP);
#endif
		ApplyCaustics(scenePos, i_lightDir, i_sceneZ, i_normals, i_underwater, sceneColour, cascadeData0, cascadeData1);
#endif
		alpha = 1.0 - exp(-_DepthFogDensity.xyz * depthFogDistance);
	}
	else
	{
		const half2 refractOffset = _RefractionStrength * i_n_pixel.xz;
		const half2 uvBackgroundRefract = uvBackground + refractOffset;

		sceneColour = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, UnityStereoTransformScreenSpaceTex(uvBackgroundRefract)).rgb;
		depthFogDistance = i_pixelZ;
		// keep alpha at 0 as UnderwaterReflection shader handles the blend
		// appropriately when looking at water from below
	}

	// blend from water colour to the scene colour
	col = lerp(sceneColour, col, alpha);

#endif // _TRANSPARENCY_ON

	return col;
}
#endif

#endif // CREST_OCEAN_EMISSION_INCLUDED
