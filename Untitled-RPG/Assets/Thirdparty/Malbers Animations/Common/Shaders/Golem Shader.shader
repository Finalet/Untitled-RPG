// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Golem PA"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_EmissionMask("Emission Mask", 2D) = "white" {}
		_Color9("Color 1", Color) = (0.3773585,0.1940192,0.1940192,1)
		_Color4("Color 2", Color) = (0.2830189,0.2362941,0.2362941,1)
		_Color5("Color 3", Color) = (0.1803922,0.1254902,0.06666667,1)
		_Color7("Color 4", Color) = (0.2352941,0.1764706,0.1019608,1)
		_EmissionPower("Emission Power", Range( 0 , 10)) = 1.300526
		[HDR]_Emissive1("Emissive 1", Color) = (1,0.9011408,0,1)
		[HDR]_Emissive2("Emissive 2", Color) = (1,0,0,0)
		_Metallic("Metallic", Range( 0 , 1)) = 0.2
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.2
		_ShadowColor("Shadow Color", Color) = (0.2075472,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		
		Cull Back
		HLSLINCLUDE
		#pragma target 2.0
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero , One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70108

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			

			sampler2D _EmissionMask;
			CBUFFER_START( UnityPerMaterial )
			float4 _Color9;
			float4 _ShadowColor;
			float4 _Color4;
			float4 _Color5;
			float4 _Color7;
			float4 _Emissive2;
			float4 _Emissive1;
			float4 _EmissionMask_ST;
			float _EmissionPower;
			float _Metallic;
			float _Smoothness;
			CBUFFER_END


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			VertexOutput vert ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord6.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord6.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				float3 WorldNormal = normalize( IN.tSpace0.xyz );
				float3 WorldTangent = IN.tSpace1.xyz;
				float3 WorldBiTangent = IN.tSpace2.xyz;
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				#if SHADER_HINT_NICE_QUALITY
					WorldViewDirection = SafeNormalize( WorldViewDirection );
				#endif

				float4 blendOpSrc132 = _Color9;
				float4 blendOpDest132 = _ShadowColor;
				float2 uv011 = IN.ase_texcoord6.xy * float2( 1,3 ) + float2( 0,-1.5 );
				float4 lerpResult12 = lerp( _Color9 , ( saturate( 2.0f*blendOpDest132*blendOpSrc132 + blendOpDest132*blendOpDest132*(1.0f - 2.0f*blendOpSrc132) )) , uv011.y);
				float2 uv02_g166 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g166 = 1.0;
				float temp_output_7_0_g166 = 2.0;
				float temp_output_9_0_g166 = 2.0;
				float temp_output_8_0_g166 = 2.0;
				float4 blendOpSrc131 = _Color4;
				float4 blendOpDest131 = _ShadowColor;
				float4 lerpResult36 = lerp( _Color4 , ( saturate( 2.0f*blendOpDest131*blendOpSrc131 + blendOpDest131*blendOpDest131*(1.0f - 2.0f*blendOpSrc131) )) , uv011.y);
				float2 uv02_g165 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g165 = 2.0;
				float temp_output_7_0_g165 = 2.0;
				float temp_output_9_0_g165 = 2.0;
				float temp_output_8_0_g165 = 2.0;
				float4 blendOpSrc133 = _Color5;
				float4 blendOpDest133 = _ShadowColor;
				float2 uv045 = IN.ase_texcoord6.xy * float2( 1,2 ) + float2( 0,0.2 );
				float4 lerpResult38 = lerp( _Color5 , ( saturate( 2.0f*blendOpDest133*blendOpSrc133 + blendOpDest133*blendOpDest133*(1.0f - 2.0f*blendOpSrc133) )) , uv045.y);
				float2 uv02_g163 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g163 = 1.0;
				float temp_output_7_0_g163 = 2.0;
				float temp_output_9_0_g163 = 1.0;
				float temp_output_8_0_g163 = 2.0;
				float4 blendOpSrc134 = _Color7;
				float4 blendOpDest134 = _ShadowColor;
				float4 lerpResult41 = lerp( _Color7 , ( saturate( 2.0f*blendOpDest134*blendOpSrc134 + blendOpDest134*blendOpDest134*(1.0f - 2.0f*blendOpSrc134) )) , uv045.y);
				float2 uv02_g164 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g164 = 2.0;
				float temp_output_7_0_g164 = 2.0;
				float temp_output_9_0_g164 = 1.0;
				float temp_output_8_0_g164 = 2.0;
				
				float4 color117 = IsGammaSpace() ? float4(0,0,0,1) : float4(0,0,0,1);
				float2 uv0109 = IN.ase_texcoord6.xy * float2( 1,3 ) + float2( 0,-0.85 );
				float4 lerpResult113 = lerp( _Emissive2 , _Emissive1 , uv0109.y);
				float2 uv02_g158 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g158 = 2.0;
				float temp_output_7_0_g158 = 2.0;
				float temp_output_9_0_g158 = 1.0;
				float temp_output_8_0_g158 = 2.0;
				float2 uv0110 = IN.ase_texcoord6.xy * float2( 1,3 ) + float2( 0,-2.3 );
				float4 lerpResult114 = lerp( _Emissive2 , _Emissive1 , uv0110.y);
				float2 uv02_g159 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g159 = 1.0;
				float temp_output_7_0_g159 = 1.0;
				float temp_output_9_0_g159 = 2.0;
				float temp_output_8_0_g159 = 2.0;
				float2 uv_EmissionMask = IN.ase_texcoord6.xy * _EmissionMask_ST.xy + _EmissionMask_ST.zw;
				float4 lerpResult116 = lerp( color117 , ( ( lerpResult113 * ( ( ( 1.0 - step( uv02_g158.x , ( ( temp_output_3_0_g158 - 1.0 ) / temp_output_7_0_g158 ) ) ) * ( step( uv02_g158.x , ( temp_output_3_0_g158 / temp_output_7_0_g158 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g158.y , ( ( temp_output_9_0_g158 - 1.0 ) / temp_output_8_0_g158 ) ) ) * ( step( uv02_g158.y , ( temp_output_9_0_g158 / temp_output_8_0_g158 ) ) * 1.0 ) ) ) ) + ( lerpResult114 * ( ( ( 1.0 - step( uv02_g159.x , ( ( temp_output_3_0_g159 - 1.0 ) / temp_output_7_0_g159 ) ) ) * ( step( uv02_g159.x , ( temp_output_3_0_g159 / temp_output_7_0_g159 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g159.y , ( ( temp_output_9_0_g159 - 1.0 ) / temp_output_8_0_g159 ) ) ) * ( step( uv02_g159.y , ( temp_output_9_0_g159 / temp_output_8_0_g159 ) ) * 1.0 ) ) ) ) ) , tex2D( _EmissionMask, uv_EmissionMask ));
				
				float3 Albedo = ( ( lerpResult12 * ( ( ( 1.0 - step( uv02_g166.x , ( ( temp_output_3_0_g166 - 1.0 ) / temp_output_7_0_g166 ) ) ) * ( step( uv02_g166.x , ( temp_output_3_0_g166 / temp_output_7_0_g166 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g166.y , ( ( temp_output_9_0_g166 - 1.0 ) / temp_output_8_0_g166 ) ) ) * ( step( uv02_g166.y , ( temp_output_9_0_g166 / temp_output_8_0_g166 ) ) * 1.0 ) ) ) ) + ( lerpResult36 * ( ( ( 1.0 - step( uv02_g165.x , ( ( temp_output_3_0_g165 - 1.0 ) / temp_output_7_0_g165 ) ) ) * ( step( uv02_g165.x , ( temp_output_3_0_g165 / temp_output_7_0_g165 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g165.y , ( ( temp_output_9_0_g165 - 1.0 ) / temp_output_8_0_g165 ) ) ) * ( step( uv02_g165.y , ( temp_output_9_0_g165 / temp_output_8_0_g165 ) ) * 1.0 ) ) ) ) + ( lerpResult38 * ( ( ( 1.0 - step( uv02_g163.x , ( ( temp_output_3_0_g163 - 1.0 ) / temp_output_7_0_g163 ) ) ) * ( step( uv02_g163.x , ( temp_output_3_0_g163 / temp_output_7_0_g163 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g163.y , ( ( temp_output_9_0_g163 - 1.0 ) / temp_output_8_0_g163 ) ) ) * ( step( uv02_g163.y , ( temp_output_9_0_g163 / temp_output_8_0_g163 ) ) * 1.0 ) ) ) ) + ( lerpResult41 * ( ( ( 1.0 - step( uv02_g164.x , ( ( temp_output_3_0_g164 - 1.0 ) / temp_output_7_0_g164 ) ) ) * ( step( uv02_g164.x , ( temp_output_3_0_g164 / temp_output_7_0_g164 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g164.y , ( ( temp_output_9_0_g164 - 1.0 ) / temp_output_8_0_g164 ) ) ) * ( step( uv02_g164.y , ( temp_output_9_0_g164 / temp_output_8_0_g164 ) ) * 1.0 ) ) ) ) ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( lerpResult116 * _EmissionPower ).rgb;
				float3 Specular = 0.5;
				float Metallic = _Metallic;
				float Smoothness = _Smoothness;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float3 BakedGI = 0;

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal )));
				#else
					#if !SHADER_HINT_NICE_QUALITY
						inputData.normalWS = WorldNormal;
					#else
						inputData.normalWS = normalize( WorldNormal );
					#endif
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif
				
				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif
				
				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70108

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			#define SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			CBUFFER_START( UnityPerMaterial )
			float4 _Color9;
			float4 _ShadowColor;
			float4 _Color4;
			float4 _Color5;
			float4 _Color7;
			float4 _Emissive2;
			float4 _Emissive1;
			float4 _EmissionMask_ST;
			float _EmissionPower;
			float _Metallic;
			float _Smoothness;
			CBUFFER_END


			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			float3 _LightDirection;

			VertexOutput ShadowPassVertex( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

				float4 clipPos = TransformWorldToHClip( ApplyShadowBias( positionWS, normalWS, _LightDirection ) );

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			half4 ShadowPassFragment(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70108

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			CBUFFER_START( UnityPerMaterial )
			float4 _Color9;
			float4 _ShadowColor;
			float4 _Color4;
			float4 _Color5;
			float4 _Color7;
			float4 _Emissive2;
			float4 _Emissive1;
			float4 _EmissionMask_ST;
			float _EmissionPower;
			float _Metallic;
			float _Smoothness;
			CBUFFER_END


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70108

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			sampler2D _EmissionMask;
			CBUFFER_START( UnityPerMaterial )
			float4 _Color9;
			float4 _ShadowColor;
			float4 _Color4;
			float4 _Color5;
			float4 _Color7;
			float4 _Emissive2;
			float4 _Emissive1;
			float4 _EmissionMask_ST;
			float _EmissionPower;
			float _Metallic;
			float _Smoothness;
			CBUFFER_END


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 blendOpSrc132 = _Color9;
				float4 blendOpDest132 = _ShadowColor;
				float2 uv011 = IN.ase_texcoord2.xy * float2( 1,3 ) + float2( 0,-1.5 );
				float4 lerpResult12 = lerp( _Color9 , ( saturate( 2.0f*blendOpDest132*blendOpSrc132 + blendOpDest132*blendOpDest132*(1.0f - 2.0f*blendOpSrc132) )) , uv011.y);
				float2 uv02_g166 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g166 = 1.0;
				float temp_output_7_0_g166 = 2.0;
				float temp_output_9_0_g166 = 2.0;
				float temp_output_8_0_g166 = 2.0;
				float4 blendOpSrc131 = _Color4;
				float4 blendOpDest131 = _ShadowColor;
				float4 lerpResult36 = lerp( _Color4 , ( saturate( 2.0f*blendOpDest131*blendOpSrc131 + blendOpDest131*blendOpDest131*(1.0f - 2.0f*blendOpSrc131) )) , uv011.y);
				float2 uv02_g165 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g165 = 2.0;
				float temp_output_7_0_g165 = 2.0;
				float temp_output_9_0_g165 = 2.0;
				float temp_output_8_0_g165 = 2.0;
				float4 blendOpSrc133 = _Color5;
				float4 blendOpDest133 = _ShadowColor;
				float2 uv045 = IN.ase_texcoord2.xy * float2( 1,2 ) + float2( 0,0.2 );
				float4 lerpResult38 = lerp( _Color5 , ( saturate( 2.0f*blendOpDest133*blendOpSrc133 + blendOpDest133*blendOpDest133*(1.0f - 2.0f*blendOpSrc133) )) , uv045.y);
				float2 uv02_g163 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g163 = 1.0;
				float temp_output_7_0_g163 = 2.0;
				float temp_output_9_0_g163 = 1.0;
				float temp_output_8_0_g163 = 2.0;
				float4 blendOpSrc134 = _Color7;
				float4 blendOpDest134 = _ShadowColor;
				float4 lerpResult41 = lerp( _Color7 , ( saturate( 2.0f*blendOpDest134*blendOpSrc134 + blendOpDest134*blendOpDest134*(1.0f - 2.0f*blendOpSrc134) )) , uv045.y);
				float2 uv02_g164 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g164 = 2.0;
				float temp_output_7_0_g164 = 2.0;
				float temp_output_9_0_g164 = 1.0;
				float temp_output_8_0_g164 = 2.0;
				
				float4 color117 = IsGammaSpace() ? float4(0,0,0,1) : float4(0,0,0,1);
				float2 uv0109 = IN.ase_texcoord2.xy * float2( 1,3 ) + float2( 0,-0.85 );
				float4 lerpResult113 = lerp( _Emissive2 , _Emissive1 , uv0109.y);
				float2 uv02_g158 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g158 = 2.0;
				float temp_output_7_0_g158 = 2.0;
				float temp_output_9_0_g158 = 1.0;
				float temp_output_8_0_g158 = 2.0;
				float2 uv0110 = IN.ase_texcoord2.xy * float2( 1,3 ) + float2( 0,-2.3 );
				float4 lerpResult114 = lerp( _Emissive2 , _Emissive1 , uv0110.y);
				float2 uv02_g159 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g159 = 1.0;
				float temp_output_7_0_g159 = 1.0;
				float temp_output_9_0_g159 = 2.0;
				float temp_output_8_0_g159 = 2.0;
				float2 uv_EmissionMask = IN.ase_texcoord2.xy * _EmissionMask_ST.xy + _EmissionMask_ST.zw;
				float4 lerpResult116 = lerp( color117 , ( ( lerpResult113 * ( ( ( 1.0 - step( uv02_g158.x , ( ( temp_output_3_0_g158 - 1.0 ) / temp_output_7_0_g158 ) ) ) * ( step( uv02_g158.x , ( temp_output_3_0_g158 / temp_output_7_0_g158 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g158.y , ( ( temp_output_9_0_g158 - 1.0 ) / temp_output_8_0_g158 ) ) ) * ( step( uv02_g158.y , ( temp_output_9_0_g158 / temp_output_8_0_g158 ) ) * 1.0 ) ) ) ) + ( lerpResult114 * ( ( ( 1.0 - step( uv02_g159.x , ( ( temp_output_3_0_g159 - 1.0 ) / temp_output_7_0_g159 ) ) ) * ( step( uv02_g159.x , ( temp_output_3_0_g159 / temp_output_7_0_g159 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g159.y , ( ( temp_output_9_0_g159 - 1.0 ) / temp_output_8_0_g159 ) ) ) * ( step( uv02_g159.y , ( temp_output_9_0_g159 / temp_output_8_0_g159 ) ) * 1.0 ) ) ) ) ) , tex2D( _EmissionMask, uv_EmissionMask ));
				
				
				float3 Albedo = ( ( lerpResult12 * ( ( ( 1.0 - step( uv02_g166.x , ( ( temp_output_3_0_g166 - 1.0 ) / temp_output_7_0_g166 ) ) ) * ( step( uv02_g166.x , ( temp_output_3_0_g166 / temp_output_7_0_g166 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g166.y , ( ( temp_output_9_0_g166 - 1.0 ) / temp_output_8_0_g166 ) ) ) * ( step( uv02_g166.y , ( temp_output_9_0_g166 / temp_output_8_0_g166 ) ) * 1.0 ) ) ) ) + ( lerpResult36 * ( ( ( 1.0 - step( uv02_g165.x , ( ( temp_output_3_0_g165 - 1.0 ) / temp_output_7_0_g165 ) ) ) * ( step( uv02_g165.x , ( temp_output_3_0_g165 / temp_output_7_0_g165 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g165.y , ( ( temp_output_9_0_g165 - 1.0 ) / temp_output_8_0_g165 ) ) ) * ( step( uv02_g165.y , ( temp_output_9_0_g165 / temp_output_8_0_g165 ) ) * 1.0 ) ) ) ) + ( lerpResult38 * ( ( ( 1.0 - step( uv02_g163.x , ( ( temp_output_3_0_g163 - 1.0 ) / temp_output_7_0_g163 ) ) ) * ( step( uv02_g163.x , ( temp_output_3_0_g163 / temp_output_7_0_g163 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g163.y , ( ( temp_output_9_0_g163 - 1.0 ) / temp_output_8_0_g163 ) ) ) * ( step( uv02_g163.y , ( temp_output_9_0_g163 / temp_output_8_0_g163 ) ) * 1.0 ) ) ) ) + ( lerpResult41 * ( ( ( 1.0 - step( uv02_g164.x , ( ( temp_output_3_0_g164 - 1.0 ) / temp_output_7_0_g164 ) ) ) * ( step( uv02_g164.x , ( temp_output_3_0_g164 / temp_output_7_0_g164 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g164.y , ( ( temp_output_9_0_g164 - 1.0 ) / temp_output_8_0_g164 ) ) ) * ( step( uv02_g164.y , ( temp_output_9_0_g164 / temp_output_8_0_g164 ) ) * 1.0 ) ) ) ) ).rgb;
				float3 Emission = ( lerpResult116 * _EmissionPower ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero , One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70108

			#pragma enable_d3d11_debug_symbols
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			

			CBUFFER_START( UnityPerMaterial )
			float4 _Color9;
			float4 _ShadowColor;
			float4 _Color4;
			float4 _Color5;
			float4 _Color7;
			float4 _Emissive2;
			float4 _Emissive1;
			float4 _EmissionMask_ST;
			float _EmissionPower;
			float _Metallic;
			float _Smoothness;
			CBUFFER_END


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 blendOpSrc132 = _Color9;
				float4 blendOpDest132 = _ShadowColor;
				float2 uv011 = IN.ase_texcoord2.xy * float2( 1,3 ) + float2( 0,-1.5 );
				float4 lerpResult12 = lerp( _Color9 , ( saturate( 2.0f*blendOpDest132*blendOpSrc132 + blendOpDest132*blendOpDest132*(1.0f - 2.0f*blendOpSrc132) )) , uv011.y);
				float2 uv02_g166 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g166 = 1.0;
				float temp_output_7_0_g166 = 2.0;
				float temp_output_9_0_g166 = 2.0;
				float temp_output_8_0_g166 = 2.0;
				float4 blendOpSrc131 = _Color4;
				float4 blendOpDest131 = _ShadowColor;
				float4 lerpResult36 = lerp( _Color4 , ( saturate( 2.0f*blendOpDest131*blendOpSrc131 + blendOpDest131*blendOpDest131*(1.0f - 2.0f*blendOpSrc131) )) , uv011.y);
				float2 uv02_g165 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g165 = 2.0;
				float temp_output_7_0_g165 = 2.0;
				float temp_output_9_0_g165 = 2.0;
				float temp_output_8_0_g165 = 2.0;
				float4 blendOpSrc133 = _Color5;
				float4 blendOpDest133 = _ShadowColor;
				float2 uv045 = IN.ase_texcoord2.xy * float2( 1,2 ) + float2( 0,0.2 );
				float4 lerpResult38 = lerp( _Color5 , ( saturate( 2.0f*blendOpDest133*blendOpSrc133 + blendOpDest133*blendOpDest133*(1.0f - 2.0f*blendOpSrc133) )) , uv045.y);
				float2 uv02_g163 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g163 = 1.0;
				float temp_output_7_0_g163 = 2.0;
				float temp_output_9_0_g163 = 1.0;
				float temp_output_8_0_g163 = 2.0;
				float4 blendOpSrc134 = _Color7;
				float4 blendOpDest134 = _ShadowColor;
				float4 lerpResult41 = lerp( _Color7 , ( saturate( 2.0f*blendOpDest134*blendOpSrc134 + blendOpDest134*blendOpDest134*(1.0f - 2.0f*blendOpSrc134) )) , uv045.y);
				float2 uv02_g164 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g164 = 2.0;
				float temp_output_7_0_g164 = 2.0;
				float temp_output_9_0_g164 = 1.0;
				float temp_output_8_0_g164 = 2.0;
				
				
				float3 Albedo = ( ( lerpResult12 * ( ( ( 1.0 - step( uv02_g166.x , ( ( temp_output_3_0_g166 - 1.0 ) / temp_output_7_0_g166 ) ) ) * ( step( uv02_g166.x , ( temp_output_3_0_g166 / temp_output_7_0_g166 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g166.y , ( ( temp_output_9_0_g166 - 1.0 ) / temp_output_8_0_g166 ) ) ) * ( step( uv02_g166.y , ( temp_output_9_0_g166 / temp_output_8_0_g166 ) ) * 1.0 ) ) ) ) + ( lerpResult36 * ( ( ( 1.0 - step( uv02_g165.x , ( ( temp_output_3_0_g165 - 1.0 ) / temp_output_7_0_g165 ) ) ) * ( step( uv02_g165.x , ( temp_output_3_0_g165 / temp_output_7_0_g165 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g165.y , ( ( temp_output_9_0_g165 - 1.0 ) / temp_output_8_0_g165 ) ) ) * ( step( uv02_g165.y , ( temp_output_9_0_g165 / temp_output_8_0_g165 ) ) * 1.0 ) ) ) ) + ( lerpResult38 * ( ( ( 1.0 - step( uv02_g163.x , ( ( temp_output_3_0_g163 - 1.0 ) / temp_output_7_0_g163 ) ) ) * ( step( uv02_g163.x , ( temp_output_3_0_g163 / temp_output_7_0_g163 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g163.y , ( ( temp_output_9_0_g163 - 1.0 ) / temp_output_8_0_g163 ) ) ) * ( step( uv02_g163.y , ( temp_output_9_0_g163 / temp_output_8_0_g163 ) ) * 1.0 ) ) ) ) + ( lerpResult41 * ( ( ( 1.0 - step( uv02_g164.x , ( ( temp_output_3_0_g164 - 1.0 ) / temp_output_7_0_g164 ) ) ) * ( step( uv02_g164.x , ( temp_output_3_0_g164 / temp_output_7_0_g164 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g164.y , ( ( temp_output_9_0_g164 - 1.0 ) / temp_output_8_0_g164 ) ) ) * ( step( uv02_g164.y , ( temp_output_9_0_g164 / temp_output_8_0_g164 ) ) * 1.0 ) ) ) ) ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}
		
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=17800
7;86;1466;890;3578.746;1402.661;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;122;-5506.075,-2539.248;Inherit;False;2185.29;2302.507;Color Change;19;34;28;32;30;41;36;12;38;11;45;133;132;134;131;130;42;124;39;37;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;39;-4849.5,-1346.411;Float;False;Property;_Color5;Color 3;3;0;Create;False;0;0;False;0;0.1803922,0.1254902,0.06666667,1;0.2309541,0.25178,0.2830189,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;42;-5133.027,-542.8691;Float;False;Property;_Color7;Color 4;4;0;Create;False;0;0;False;0;0.2352941,0.1764706,0.1019608,1;0.1134745,0.2443883,0.3207547,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;130;-5440.474,-1466.613;Float;False;Property;_ShadowColor;Shadow Color;10;0;Create;True;0;0;False;0;0.2075472,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;37;-4955.637,-1757.225;Float;False;Property;_Color4;Color 2;2;0;Create;False;0;0;False;0;0.2830189,0.2362941,0.2362941,1;0.1423104,0.2028393,0.2452829,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;124;-5329.247,-2423.364;Float;False;Property;_Color9;Color 1;1;0;Create;False;0;0;False;0;0.3773585,0.1940192,0.1940192,1;0.2426575,0.3326142,0.3867925,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-4592.587,-2029.296;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,3;False;1;FLOAT2;0,-1.5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;132;-5009.346,-2204.656;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;133;-4643.902,-1053.423;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;134;-4740.105,-593.6027;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;131;-4547.071,-1650.359;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;-4437.104,-812.1178;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,2;False;1;FLOAT2;0,0.2;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;36;-4247.949,-1767.308;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;38;-4175.363,-1161.572;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;12;-4313.527,-2287.664;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;41;-4155.988,-529.4528;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;32;-3666.441,-1221.775;Inherit;True;ColorShartSlot;-1;;163;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;2;False;8;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;34;-3661.704,-522.8755;Inherit;True;ColorShartSlot;-1;;164;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;2;False;8;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;30;-3907.104,-1760.635;Inherit;True;ColorShartSlot;-1;;165;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;2;False;8;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;28;-3891.893,-2393.558;Inherit;True;ColorShartSlot;-1;;166;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;2;False;8;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;123;-5461.668,-146.388;Inherit;False;2676.736;1223;Emission Color;14;4;117;115;112;110;109;107;116;111;114;113;93;90;89;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-3365.014,859.0638;Float;False;Property;_EmissionPower;Emission Power;5;0;Create;True;0;0;False;0;1.300526;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;135;-3025.677,-956.8919;Float;False;Property;_Metallic;Metallic;8;0;Create;True;0;0;False;0;0.2;0.338;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;-3029.028,-869.2371;Float;False;Property;_Smoothness;Smoothness;9;0;Create;True;0;0;False;0;0.2;0.338;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-3011.971,844.848;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-3006.768,-1173.486;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;116;-3321.71,604.5768;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;112;-4286.345,452.8255;Inherit;True;ColorShartSlot;-1;;159;8287b10e189ac1e4f80ef7e89903de17;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;1;False;8;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;107;-5063.803,278.551;Float;False;Property;_Emissive2;Emissive 2;7;1;[HDR];Create;True;0;0;False;0;1,0,0,0;0,1.247059,2,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;93;-5075.194,-107.0081;Float;False;Property;_Emissive1;Emissive 1;6;1;[HDR];Create;True;0;0;False;0;1,0.9011408,0,1;0,2,1.537255,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;110;-5004.979,529.7918;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,3;False;1;FLOAT2;0,-2.3;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;117;-3538.094,219.3647;Float;False;Constant;_Back_color;Back_color;11;0;Create;True;0;0;False;0;0,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;114;-4646.32,524.4437;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-3894.661,254.8818;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;4;-3711.038,655.5321;Inherit;True;Property;_EmissionMask;Emission Mask;0;0;Create;True;0;0;False;0;-1;0e90178a9c0b464408857a81b53fc7ac;0e90178a9c0b464408857a81b53fc7ac;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;113;-4595.292,103.4534;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;111;-4287.597,152.9065;Inherit;True;ColorShartSlot;-1;;158;8287b10e189ac1e4f80ef7e89903de17;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;2;False;8;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;109;-5396.666,60.05534;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,3;False;1;FLOAT2;0,-0.85;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;139;-2558.082,-1216.606;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;138;-2558.082,-1216.606;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;140;-2558.082,-1216.606;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;136;-2558.082,-1216.606;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;137;-2558.082,-1216.606;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Malbers/Golem PA;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;12;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;13;Workflow;1;Surface;0;  Blend;0;Two Sided;1;Cast Shadows;1;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;Vertex Position,InvertActionOnDeselection;1;0;6;False;True;True;True;True;True;False;;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;141;-2558.082,-1216.606;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
WireConnection;132;0;124;0
WireConnection;132;1;130;0
WireConnection;133;0;39;0
WireConnection;133;1;130;0
WireConnection;134;0;42;0
WireConnection;134;1;130;0
WireConnection;131;0;37;0
WireConnection;131;1;130;0
WireConnection;36;0;37;0
WireConnection;36;1;131;0
WireConnection;36;2;11;2
WireConnection;38;0;39;0
WireConnection;38;1;133;0
WireConnection;38;2;45;2
WireConnection;12;0;124;0
WireConnection;12;1;132;0
WireConnection;12;2;11;2
WireConnection;41;0;42;0
WireConnection;41;1;134;0
WireConnection;41;2;45;2
WireConnection;32;38;38;0
WireConnection;34;38;41;0
WireConnection;30;38;36;0
WireConnection;28;38;12;0
WireConnection;89;0;116;0
WireConnection;89;1;90;0
WireConnection;26;0;28;0
WireConnection;26;1;30;0
WireConnection;26;2;32;0
WireConnection;26;3;34;0
WireConnection;116;0;117;0
WireConnection;116;1;115;0
WireConnection;116;2;4;0
WireConnection;112;38;114;0
WireConnection;114;0;107;0
WireConnection;114;1;93;0
WireConnection;114;2;110;2
WireConnection;115;0;111;0
WireConnection;115;1;112;0
WireConnection;113;0;107;0
WireConnection;113;1;93;0
WireConnection;113;2;109;2
WireConnection;111;38;113;0
WireConnection;137;0;26;0
WireConnection;137;2;89;0
WireConnection;137;3;135;0
WireConnection;137;4;120;0
ASEEND*/
//CHKSM=181F5717E901671BF87E630F2E4BD161A65282D3