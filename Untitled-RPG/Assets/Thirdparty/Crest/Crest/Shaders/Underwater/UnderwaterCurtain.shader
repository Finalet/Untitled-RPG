// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

Shader "Crest/Underwater Curtain"
{
	Properties
	{
		// Most properties are copied over from the main ocean material on startup

		// Shader features need to be statically configured - it works to dynamically configure them in editor, but in standalone
		// builds they need to be preconfigured. This is a pitfall unfortunately - the settings need to be manually matched.
		[Toggle] _Shadows("Shadowing", Float) = 0
		[Toggle] _SubSurfaceScattering("Sub-Surface Scattering", Float) = 1
		[Toggle] _SubSurfaceShallowColour("Sub-Surface Shallow Colour", Float) = 1
		[Toggle] _Transparency("Transparency", Float) = 1
		[Toggle] _Caustics("Caustics", Float) = 1
		[Toggle] _CompileShaderWithDebugInfo("Compile Shader With Debug Info (D3D11)", Float) = 0
	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-110" }

		Pass
		{
			// The ocean surface will render after the skirt, and overwrite the pixels
			ZWrite Off
			ZTest Always

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#pragma multi_compile_instancing

			// Use multi_compile because these keywords are copied over from the ocean material. With shader_feature,
			// the keywords would be stripped from builds. Unused shader variants are stripped using a build processor.
			#pragma multi_compile_local __ _SUBSURFACESCATTERING_ON
			#pragma multi_compile_local __ _SUBSURFACESHALLOWCOLOUR_ON
			#pragma multi_compile_local __ _TRANSPARENCY_ON
			#pragma multi_compile_local __ _CAUSTICS_ON
			#pragma multi_compile_local __ _SHADOWS_ON
			#pragma multi_compile_local __ _COMPILESHADERWITHDEBUGINFO_ON

			#if _COMPILESHADERWITHDEBUGINFO_ON
			#pragma enable_d3d11_debug_symbols
			#endif

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			// @Hack: Work around to unity_CameraToWorld._13_23_33 not being set correctly in URP 7.4+
			float3 _CameraForward;

			#include "../OceanGlobals.hlsl"
			#include "../OceanInputsDriven.hlsl"
			#include "../OceanShaderData.hlsl"
			#include "../OceanHelpersNew.hlsl"
			#include "../OceanShaderHelpers.hlsl"
			#include "UnderwaterShared.hlsl"

			#include "../OceanEmission.hlsl"

			#define MAX_OFFSET 5.0

			struct Attributes
			{
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				half4 foam_screenPos : TEXCOORD1;
				half4 grabPos : TEXCOORD2;
				float3 positionWS : TEXCOORD3;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				// Goal of this vert shader is to place a sheet of triangles in front of the camera. The geometry has
				// two rows of verts, the top row and the bottom row (top and bottom are view relative). The bottom row
				// is pushed down below the bottom of the screen. Every vert in the top row can take any vertical position
				// on the near plane in order to find the meniscus of the water. Due to render states, the ocean surface
				// will stomp over the results of this shader. The ocean surface has necessary code to render from underneath
				// and correctly fog etc.

				// Potential optimisations (note that this shader runs over a few dozen vertices, not over screen pixels!):
				// - when looking down through the water surface, the code currently pushes the top verts of the skirt
				//   up to cover the whole screen, but it only needs to get pushed up to the horizon level to meet the water surface

				// view coordinate frame for camera
				const float3 right   = unity_CameraToWorld._11_21_31;
				const float3 up      = unity_CameraToWorld._12_22_32;
				// @Hack: Work around to unity_CameraToWorld._13_23_33 not being set correctly in URP 7.4+
				const float3 forward = _CameraForward;

				const float3 nearPlaneCenter = _WorldSpaceCameraPos + forward * _ProjectionParams.y * 1.001;
				// Spread verts across the near plane.
				const float aspect = _ScreenParams.x / _ScreenParams.y;
				o.positionWS = nearPlaneCenter
					+ 2.6 * unity_CameraInvProjection._m11 * aspect * right * input.positionOS.x * _ProjectionParams.y
					+ up * input.positionOS.z * _ProjectionParams.y;

				// Isolate topmost edge
				if (input.positionOS.z > 0.45)
				{
					const float3 posOnNearPlane = o.positionWS;

					// Only compute intersection of water if viewer is looking "horizontal-ish". When the viewer starts to look
					// too much up or down, the intersection between the near plane and the water surface can be complex.
					if (abs(forward.y) < CREST_MAX_UPDOWN_AMOUNT)
					{
						// move vert in the up direction, but only to an extent, otherwise numerical issues can cause weirdness
						o.positionWS += min(IntersectRayWithWaterSurface(o.positionWS, up, _CrestCascadeData[_LD_SliceIndex]), MAX_OFFSET) * up;

						// Move the geometry towards the horizon. As noted above, the skirt will be stomped by the ocean
						// surface render. If we project a bit towards the horizon to make a bit of overlap then we can reduce
						// the chance render issues from cracks/gaps with down angles, or of the skirt being too high for up angles.
						float3 horizonPoint = _WorldSpaceCameraPos + (posOnNearPlane - _WorldSpaceCameraPos) * 10000.0;
						horizonPoint.y = _OceanCenterPosWorld.y;
						const float3 horizonDir = normalize(horizonPoint - _WorldSpaceCameraPos);
						const float3 projectionOfHorizonOnNearPlane = _WorldSpaceCameraPos + horizonDir / dot(horizonDir, forward);
						o.positionWS = lerp(o.positionWS, projectionOfHorizonOnNearPlane, 0.1);
					}
					else if (_HeightOffset < -1.0)
					{
						// Deep under water - always push top edge up to cover screen
						o.positionWS += MAX_OFFSET * up;
					}
					else
					{
						// Near water surface - this is where the water can intersect the lens in nontrivial ways and causes problems
						// for finding the meniscus / water line.

						// Push top edge up if we are looking down so that the screen defaults to looking underwater.
						// Push top edge down if we are looking up so that the screen defaults to looking out of water.
						o.positionWS -= sign(forward.y) * MAX_OFFSET * up;
					}

					// Test - always put top row of verts at water horizon, because then it will always meet the water
					// surface. Good idea but didnt work because it then does underwater shading on opaque surfaces which
					// can be ABOVE the water surface. Not sure if theres any way around this.
					o.positionCS = mul(UNITY_MATRIX_VP, float4(o.positionWS, 1.0));
					o.positionCS.z = o.positionCS.w;
				}
				else
				{
					// Bottom row of verts - push them down below bottom of screen
					o.positionWS -= MAX_OFFSET * up;

					o.positionCS = mul(UNITY_MATRIX_VP, float4(o.positionWS, 1.0));
					o.positionCS.z = o.positionCS.w;
				}

				o.foam_screenPos.x = 0.0;
				o.foam_screenPos.yzw = ComputeScreenPos(o.positionCS).xyw;
				o.grabPos = ComputeScreenPos(o.positionCS);

				o.uv = input.uv;

				return o;
			}

			half4 Frag(Varyings input) : SV_Target
			{
				// We need this when sampling a screenspace texture.
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				const half3 rawView = _WorldSpaceCameraPos - input.positionWS;
				const half3 view = normalize(rawView);

				const float pixelZ = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
				const half3 screenPos = input.foam_screenPos.yzw;
				const half2 uvDepth = screenPos.xy / screenPos.z;
				const float sceneZ01 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uvDepth)).x;
				const float sceneZ = LinearEyeDepth(sceneZ01, _ZBufferParams);

				const CascadeParams cascadeData0 = _CrestCascadeData[_LD_SliceIndex];
				const CascadeParams cascadeData1 = _CrestCascadeData[_LD_SliceIndex + 1];

				const Light lightMain = GetMainLight();
				const real3 lightDir = lightMain.direction;
				const real3 lightCol = lightMain.color;
				const half3 n_pixel = 0.0;
				const half3 bubbleCol = 0.0;

				// Depth and shadow are computed in ScatterColour when underwater==true, using the LOD1 texture. SSS is ommitted for now for perf reasons.
				const float depth = 0.0;
				const half shadow = 1.0;
				const half sss = 0.0;

				const float meshScaleLerp = _CrestPerCascadeInstanceData[_LD_SliceIndex]._meshScaleLerp;
				const float baseCascadeScale = _CrestCascadeData[0]._scale;
				const half3 scatterCol = ScatterColour(depth, _WorldSpaceCameraPos, lightDir, view, shadow, true, true, lightCol, sss, meshScaleLerp, baseCascadeScale, cascadeData0);

				// Could have possibly used ComputeNonStereoScreenPos() instead when calculating grab pos and then not
				// have needed to do this, but that function doesn't seem to exist in URP?
				// https://docs.unity3d.com/Manual/SinglePassStereoRendering.html
				float2 uvScreenSpace = UnityStereoTransformScreenSpaceTex(input.grabPos.xy / input.grabPos.w);
				real3 sceneColour = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uvScreenSpace).rgb;

#if _CAUSTICS_ON
				if (sceneZ01 != 0.0)
				{
					// flatten view in the camera direction to calculate the scenePos
					float3 scenePos = (((rawView) / dot(rawView, _CameraForward)) * sceneZ) + _WorldSpaceCameraPos;
					ApplyCaustics(scenePos, lightCol, lightDir, sceneZ, _Normals, true, sceneColour, cascadeData0, cascadeData1);
				}
#endif // _CAUSTICS_ON

				half3 col = lerp(sceneColour, scatterCol, 1.0 - exp(-_DepthFogDensity.xyz * sceneZ));

				return half4(col, 1.0);
			}
			ENDHLSL
		}
	}
}
