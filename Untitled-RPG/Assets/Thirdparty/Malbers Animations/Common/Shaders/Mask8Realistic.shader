// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Mask8Realistic"
{
	Properties
	{
		[NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
		[NoScaleOffset]_Metalic_Smoothness("Metalic_Smoothness", 2D) = "white" {}
		[NoScaleOffset]_Normal("Normal", 2D) = "bump" {}
		_NormalIntensity("Normal Intensity", Range( -2 , 2)) = 3
		[NoScaleOffset][Header(Emission)]_Emission("Emission", 2D) = "white" {}
		_EmissionPower("Emission Power", Range( 0 , 5)) = 3
		[Toggle]_UseRawEmission("Use Raw Emission", Float) = 0
		_AddEmissiveColor("Add Emissive Color", Color) = (0,0,0,0)
		[NoScaleOffset][Header(Masks)]_Mask1("Mask 1", 2D) = "white" {}
		[NoScaleOffset]_Mask2("Mask 2", 2D) = "white" {}
		[Header(Mask 1 Red)]_M1RHue("M1 R Hue", Range( 0 , 2)) = 1
		_M1RSaturation("M1 R Saturation", Range( 0 , 2)) = 1
		_M1RValue("M1 R Value", Range( 0 , 2)) = 1
		[Header(Mask 1 Green)]_M1GHue("M1 G Hue", Range( 0 , 2)) = 1
		_M1GSaturation("M1 G Saturation", Range( 0 , 2)) = 1
		_M1GValue("M1 G Value", Range( 0 , 2)) = 1
		[Header(Mask 1 Blue)]_M1BHue("M1 B Hue", Range( 0 , 2)) = 1
		_M1BSaturation("M1 B Saturation", Range( 0 , 2)) = 1
		_M1BValue("M1 B Value", Range( 0 , 2)) = 1
		[Header(Mask 1 Alpha)]_M1AHue("M1 A Hue", Range( 0 , 2)) = 1
		_M1ASaturation("M1 A Saturation", Range( 0 , 2)) = 0.8705882
		_M1AValue("M1 A Value", Range( 0 , 2)) = 1
		[Header(Mask 2 Red)]_M2RHue("M2 R Hue", Range( 0 , 2)) = 1
		_M2RSaturation("M2 R Saturation", Range( 0 , 2)) = 1
		_M2RValue("M2 R Value", Range( 0 , 2)) = 1
		[Header(Mask 2 Green)]_M2GHue("M2 G Hue", Range( 0 , 2)) = 1
		_M2GSaturation("M2 G Saturation", Range( 0 , 2)) = 1
		_M2GValue("M2 G Value", Range( 0 , 2)) = 1
		[Header(Mask 2 Blue)]_M2BHue("M2 B Hue", Range( 0 , 2)) = 1
		_M2BSaturation("M2 B Saturation", Range( 0 , 2)) = 1
		_M2BValue("M2 B Value", Range( 0 , 2)) = 1
		[Header(Mask 2 Alpha)]_M2AHue("M2 A Hue", Range( 0 , 2)) = 1
		_M2ASaturation("M2 A Saturation", Range( 0 , 2)) = 0.8705882
		_M2AValue("M2 A Value", Range( 0 , 2)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		
		Cull Back
		HLSLINCLUDE
		#pragma target 3.0
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
			#define ASE_SRP_VERSION 70107

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


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			

			sampler2D _Mask1;
			sampler2D _Albedo;
			sampler2D _Mask2;
			sampler2D _Normal;
			sampler2D _Emission;
			sampler2D _Metalic_Smoothness;
			CBUFFER_START( UnityPerMaterial )
			float _M1RHue;
			float _M1RSaturation;
			float _M1RValue;
			float _M1GHue;
			float _M1GSaturation;
			float _M1GValue;
			float _M1BHue;
			float _M1BSaturation;
			float _M1BValue;
			float _M1AHue;
			float _M1ASaturation;
			float _M1AValue;
			float _M2RHue;
			float _M2RSaturation;
			float _M2RValue;
			float _M2GHue;
			float _M2GSaturation;
			float _M2GValue;
			float _M2BHue;
			float _M2BSaturation;
			float _M2BValue;
			float _M2AHue;
			float _M2ASaturation;
			float _M2AValue;
			float _NormalIntensity;
			float _UseRawEmission;
			float _EmissionPower;
			float4 _AddEmissiveColor;
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
				float4 shadowCoord : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			VertexOutput vert ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord7.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
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

				float3 lwWNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 lwWorldPos = TransformObjectToWorld(v.vertex.xyz);
				float3 lwWTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				float3 lwWBinormal = normalize(cross(lwWNormal, lwWTangent) * v.ase_tangent.w);
				o.tSpace0 = float4(lwWTangent.x, lwWBinormal.x, lwWNormal.x, lwWorldPos.x);
				o.tSpace1 = float4(lwWTangent.y, lwWBinormal.y, lwWNormal.y, lwWorldPos.y);
				o.tSpace2 = float4(lwWTangent.z, lwWBinormal.z, lwWNormal.z, lwWorldPos.z);

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				
				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH(lwWNormal, o.lightmapUVOrVertexSH.xyz );

				half3 vertexLight = VertexLighting(vertexInput.positionWS, lwWNormal);
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				o.clipPos = vertexInput.positionCS;

				#ifdef _MAIN_LIGHT_SHADOWS
					o.shadowCoord = GetShadowCoord(vertexInput);
				#endif
				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				float3 WorldSpaceNormal = normalize(float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z));
				float3 WorldSpaceTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
				float3 WorldSpaceBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
				float3 WorldSpacePosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldSpaceViewDirection = _WorldSpaceCameraPos.xyz  - WorldSpacePosition;
	
				#if SHADER_HINT_NICE_QUALITY
					WorldSpaceViewDirection = SafeNormalize( WorldSpaceViewDirection );
				#endif

				float2 uv_Mask16 = IN.ase_texcoord7.xy;
				float4 tex2DNode6 = tex2D( _Mask1, uv_Mask16 );
				float2 uv_Albedo2 = IN.ase_texcoord7.xy;
				float4 TextureBase260 = tex2D( _Albedo, uv_Albedo2 );
				float3 hsvTorgb119 = RGBToHSV( TextureBase260.rgb );
				float Hue121 = hsvTorgb119.x;
				float Saturation122 = hsvTorgb119.y;
				float Value123 = hsvTorgb119.z;
				float3 hsvTorgb57 = HSVToRGB( float3(( Hue121 * _M1RHue ),( Saturation122 * _M1RSaturation ),( Value123 * _M1RValue )) );
				float3 M1R151 = ( tex2DNode6.r * hsvTorgb57 );
				float3 hsvTorgb47 = HSVToRGB( float3(( Hue121 * _M1GHue ),( Saturation122 * _M1GSaturation ),( Value123 * _M1GValue )) );
				float3 M1G152 = ( tex2DNode6.g * hsvTorgb47 );
				float3 hsvTorgb36 = HSVToRGB( float3(( Hue121 * _M1BHue ),( Saturation122 * _M1BSaturation ),( Value123 * _M1BValue )) );
				float3 M1B153 = ( tex2DNode6.b * hsvTorgb36 );
				float3 hsvTorgb17 = HSVToRGB( float3(( Hue121 * _M1AHue ),( Saturation122 * _M1ASaturation ),( Value123 * _M1AValue )) );
				float3 M1A154 = ( tex2DNode6.a * hsvTorgb17 );
				float2 uv_Mask2222 = IN.ase_texcoord7.xy;
				float4 tex2DNode222 = tex2D( _Mask2, uv_Mask2222 );
				float3 hsvTorgb220 = HSVToRGB( float3(( Hue121 * _M2RHue ),( Saturation122 * _M2RSaturation ),( Value123 * _M2RValue )) );
				float3 M2R230 = ( tex2DNode222.r * hsvTorgb220 );
				float3 hsvTorgb221 = HSVToRGB( float3(( Hue121 * _M2GHue ),( Saturation122 * _M2GSaturation ),( Value123 * _M2GValue )) );
				float3 M2G229 = ( tex2DNode222.g * hsvTorgb221 );
				float3 hsvTorgb218 = HSVToRGB( float3(( Hue121 * _M2BHue ),( Saturation122 * _M2BSaturation ),( Value123 * _M2BValue )) );
				float3 M2B228 = ( tex2DNode222.b * hsvTorgb218 );
				float3 hsvTorgb219 = HSVToRGB( float3(( Hue121 * _M2AHue ),( Saturation122 * _M2ASaturation ),( Value123 * _M2AValue )) );
				float3 M2A227 = ( tex2DNode222.a * hsvTorgb219 );
				float3 temp_output_167_0 = ( M1R151 + M1G152 + M1B153 + M1A154 + M2R230 + M2G229 + M2B228 + M2A227 );
				
				float2 uv_Normal236 = IN.ase_texcoord7.xy;
				
				float2 uv_Emission237 = IN.ase_texcoord7.xy;
				float4 tex2DNode237 = tex2D( _Emission, uv_Emission237 );
				float3 desaturateInitialColor263 = tex2DNode237.rgb;
				float desaturateDot263 = dot( desaturateInitialColor263, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar263 = lerp( desaturateInitialColor263, desaturateDot263.xxx, 1.0 );
				float4 blendOpSrc272 = _AddEmissiveColor;
				float4 blendOpDest272 = float4( temp_output_167_0 , 0.0 );
				float4 Emission266 = ( float4( desaturateVar263 , 0.0 ) * _EmissionPower * ( saturate( (( blendOpDest272 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest272 ) * ( 1.0 - blendOpSrc272 ) ) : ( 2.0 * blendOpDest272 * blendOpSrc272 ) ) )) );
				
				float2 uv_Metalic_Smoothness235 = IN.ase_texcoord7.xy;
				float4 tex2DNode235 = tex2D( _Metalic_Smoothness, uv_Metalic_Smoothness235 );
				
				float3 Albedo = temp_output_167_0;
				float3 Normal = UnpackNormalScale( tex2D( _Normal, uv_Normal236 ), _NormalIntensity );
				float3 Emission = (( _UseRawEmission )?( ( _EmissionPower * tex2DNode237 ) ):( Emission266 )).rgb;
				float3 Specular = 0.5;
				float Metallic = tex2DNode235.r;
				float Smoothness = tex2DNode235.a;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float3 BakedGI = 0;

				InputData inputData;
				inputData.positionWS = WorldSpacePosition;

				#ifdef _NORMALMAP
					inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3(WorldSpaceTangent, WorldSpaceBiTangent, WorldSpaceNormal)));
				#else
					#if !SHADER_HINT_NICE_QUALITY
						inputData.normalWS = WorldSpaceNormal;
					#else
						inputData.normalWS = normalize(WorldSpaceNormal);
					#endif
				#endif

				inputData.viewDirectionWS = WorldSpaceViewDirection;
				inputData.shadowCoord = IN.shadowCoord;

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
			#define ASE_SRP_VERSION 70107

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment


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
			float _M1RHue;
			float _M1RSaturation;
			float _M1RValue;
			float _M1GHue;
			float _M1GSaturation;
			float _M1GValue;
			float _M1BHue;
			float _M1BSaturation;
			float _M1BValue;
			float _M1AHue;
			float _M1ASaturation;
			float _M1AValue;
			float _M2RHue;
			float _M2RSaturation;
			float _M2RValue;
			float _M2GHue;
			float _M2GSaturation;
			float _M2GValue;
			float _M2BHue;
			float _M2BSaturation;
			float _M2BValue;
			float _M2AHue;
			float _M2ASaturation;
			float _M2AValue;
			float _NormalIntensity;
			float _UseRawEmission;
			float _EmissionPower;
			float4 _AddEmissiveColor;
			CBUFFER_END


			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			
			float3 _LightDirection;

			VertexOutput ShadowPassVertex( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				
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

				float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

				float4 clipPos = TransformWorldToHClip( ApplyShadowBias( positionWS, normalWS, _LightDirection ) );

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#endif
				o.clipPos = clipPos;

				return o;
			}

			half4 ShadowPassFragment(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );

				
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
			#define ASE_SRP_VERSION 70107

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			CBUFFER_START( UnityPerMaterial )
			float _M1RHue;
			float _M1RSaturation;
			float _M1RValue;
			float _M1GHue;
			float _M1GSaturation;
			float _M1GValue;
			float _M1BHue;
			float _M1BSaturation;
			float _M1BValue;
			float _M1AHue;
			float _M1ASaturation;
			float _M1AValue;
			float _M2RHue;
			float _M2RSaturation;
			float _M2RValue;
			float _M2GHue;
			float _M2GSaturation;
			float _M2GValue;
			float _M2BHue;
			float _M2BSaturation;
			float _M2BValue;
			float _M2AHue;
			float _M2ASaturation;
			float _M2AValue;
			float _NormalIntensity;
			float _UseRawEmission;
			float _EmissionPower;
			float4 _AddEmissiveColor;
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

				o.clipPos = TransformObjectToHClip(v.vertex.xyz);
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				
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
			#define ASE_SRP_VERSION 70107

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			sampler2D _Mask1;
			sampler2D _Albedo;
			sampler2D _Mask2;
			sampler2D _Emission;
			CBUFFER_START( UnityPerMaterial )
			float _M1RHue;
			float _M1RSaturation;
			float _M1RValue;
			float _M1GHue;
			float _M1GSaturation;
			float _M1GValue;
			float _M1BHue;
			float _M1BSaturation;
			float _M1BValue;
			float _M1AHue;
			float _M1ASaturation;
			float _M1AValue;
			float _M2RHue;
			float _M2RSaturation;
			float _M2RValue;
			float _M2GHue;
			float _M2GSaturation;
			float _M2GValue;
			float _M2BHue;
			float _M2BSaturation;
			float _M2BValue;
			float _M2AHue;
			float _M2ASaturation;
			float _M2AValue;
			float _NormalIntensity;
			float _UseRawEmission;
			float _EmissionPower;
			float4 _AddEmissiveColor;
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
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				
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

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 uv_Mask16 = IN.ase_texcoord.xy;
				float4 tex2DNode6 = tex2D( _Mask1, uv_Mask16 );
				float2 uv_Albedo2 = IN.ase_texcoord.xy;
				float4 TextureBase260 = tex2D( _Albedo, uv_Albedo2 );
				float3 hsvTorgb119 = RGBToHSV( TextureBase260.rgb );
				float Hue121 = hsvTorgb119.x;
				float Saturation122 = hsvTorgb119.y;
				float Value123 = hsvTorgb119.z;
				float3 hsvTorgb57 = HSVToRGB( float3(( Hue121 * _M1RHue ),( Saturation122 * _M1RSaturation ),( Value123 * _M1RValue )) );
				float3 M1R151 = ( tex2DNode6.r * hsvTorgb57 );
				float3 hsvTorgb47 = HSVToRGB( float3(( Hue121 * _M1GHue ),( Saturation122 * _M1GSaturation ),( Value123 * _M1GValue )) );
				float3 M1G152 = ( tex2DNode6.g * hsvTorgb47 );
				float3 hsvTorgb36 = HSVToRGB( float3(( Hue121 * _M1BHue ),( Saturation122 * _M1BSaturation ),( Value123 * _M1BValue )) );
				float3 M1B153 = ( tex2DNode6.b * hsvTorgb36 );
				float3 hsvTorgb17 = HSVToRGB( float3(( Hue121 * _M1AHue ),( Saturation122 * _M1ASaturation ),( Value123 * _M1AValue )) );
				float3 M1A154 = ( tex2DNode6.a * hsvTorgb17 );
				float2 uv_Mask2222 = IN.ase_texcoord.xy;
				float4 tex2DNode222 = tex2D( _Mask2, uv_Mask2222 );
				float3 hsvTorgb220 = HSVToRGB( float3(( Hue121 * _M2RHue ),( Saturation122 * _M2RSaturation ),( Value123 * _M2RValue )) );
				float3 M2R230 = ( tex2DNode222.r * hsvTorgb220 );
				float3 hsvTorgb221 = HSVToRGB( float3(( Hue121 * _M2GHue ),( Saturation122 * _M2GSaturation ),( Value123 * _M2GValue )) );
				float3 M2G229 = ( tex2DNode222.g * hsvTorgb221 );
				float3 hsvTorgb218 = HSVToRGB( float3(( Hue121 * _M2BHue ),( Saturation122 * _M2BSaturation ),( Value123 * _M2BValue )) );
				float3 M2B228 = ( tex2DNode222.b * hsvTorgb218 );
				float3 hsvTorgb219 = HSVToRGB( float3(( Hue121 * _M2AHue ),( Saturation122 * _M2ASaturation ),( Value123 * _M2AValue )) );
				float3 M2A227 = ( tex2DNode222.a * hsvTorgb219 );
				float3 temp_output_167_0 = ( M1R151 + M1G152 + M1B153 + M1A154 + M2R230 + M2G229 + M2B228 + M2A227 );
				
				float2 uv_Emission237 = IN.ase_texcoord.xy;
				float4 tex2DNode237 = tex2D( _Emission, uv_Emission237 );
				float3 desaturateInitialColor263 = tex2DNode237.rgb;
				float desaturateDot263 = dot( desaturateInitialColor263, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar263 = lerp( desaturateInitialColor263, desaturateDot263.xxx, 1.0 );
				float4 blendOpSrc272 = _AddEmissiveColor;
				float4 blendOpDest272 = float4( temp_output_167_0 , 0.0 );
				float4 Emission266 = ( float4( desaturateVar263 , 0.0 ) * _EmissionPower * ( saturate( (( blendOpDest272 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest272 ) * ( 1.0 - blendOpSrc272 ) ) : ( 2.0 * blendOpDest272 * blendOpSrc272 ) ) )) );
				
				
				float3 Albedo = temp_output_167_0;
				float3 Emission = (( _UseRawEmission )?( ( _EmissionPower * tex2DNode237 ) ):( Emission266 )).rgb;
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
			#define ASE_SRP_VERSION 70107

			#pragma enable_d3d11_debug_symbols
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			

			sampler2D _Mask1;
			sampler2D _Albedo;
			sampler2D _Mask2;
			CBUFFER_START( UnityPerMaterial )
			float _M1RHue;
			float _M1RSaturation;
			float _M1RValue;
			float _M1GHue;
			float _M1GSaturation;
			float _M1GValue;
			float _M1BHue;
			float _M1BSaturation;
			float _M1BValue;
			float _M1AHue;
			float _M1ASaturation;
			float _M1AValue;
			float _M2RHue;
			float _M2RSaturation;
			float _M2RValue;
			float _M2GHue;
			float _M2GSaturation;
			float _M2GValue;
			float _M2BHue;
			float _M2BSaturation;
			float _M2BValue;
			float _M2AHue;
			float _M2ASaturation;
			float _M2AValue;
			float _NormalIntensity;
			float _UseRawEmission;
			float _EmissionPower;
			float4 _AddEmissiveColor;
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
				float4 ase_texcoord : TEXCOORD0;
			};

			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;

				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				
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

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.vertex.xyz );
				o.clipPos = vertexInput.positionCS;
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				float2 uv_Mask16 = IN.ase_texcoord.xy;
				float4 tex2DNode6 = tex2D( _Mask1, uv_Mask16 );
				float2 uv_Albedo2 = IN.ase_texcoord.xy;
				float4 TextureBase260 = tex2D( _Albedo, uv_Albedo2 );
				float3 hsvTorgb119 = RGBToHSV( TextureBase260.rgb );
				float Hue121 = hsvTorgb119.x;
				float Saturation122 = hsvTorgb119.y;
				float Value123 = hsvTorgb119.z;
				float3 hsvTorgb57 = HSVToRGB( float3(( Hue121 * _M1RHue ),( Saturation122 * _M1RSaturation ),( Value123 * _M1RValue )) );
				float3 M1R151 = ( tex2DNode6.r * hsvTorgb57 );
				float3 hsvTorgb47 = HSVToRGB( float3(( Hue121 * _M1GHue ),( Saturation122 * _M1GSaturation ),( Value123 * _M1GValue )) );
				float3 M1G152 = ( tex2DNode6.g * hsvTorgb47 );
				float3 hsvTorgb36 = HSVToRGB( float3(( Hue121 * _M1BHue ),( Saturation122 * _M1BSaturation ),( Value123 * _M1BValue )) );
				float3 M1B153 = ( tex2DNode6.b * hsvTorgb36 );
				float3 hsvTorgb17 = HSVToRGB( float3(( Hue121 * _M1AHue ),( Saturation122 * _M1ASaturation ),( Value123 * _M1AValue )) );
				float3 M1A154 = ( tex2DNode6.a * hsvTorgb17 );
				float2 uv_Mask2222 = IN.ase_texcoord.xy;
				float4 tex2DNode222 = tex2D( _Mask2, uv_Mask2222 );
				float3 hsvTorgb220 = HSVToRGB( float3(( Hue121 * _M2RHue ),( Saturation122 * _M2RSaturation ),( Value123 * _M2RValue )) );
				float3 M2R230 = ( tex2DNode222.r * hsvTorgb220 );
				float3 hsvTorgb221 = HSVToRGB( float3(( Hue121 * _M2GHue ),( Saturation122 * _M2GSaturation ),( Value123 * _M2GValue )) );
				float3 M2G229 = ( tex2DNode222.g * hsvTorgb221 );
				float3 hsvTorgb218 = HSVToRGB( float3(( Hue121 * _M2BHue ),( Saturation122 * _M2BSaturation ),( Value123 * _M2BValue )) );
				float3 M2B228 = ( tex2DNode222.b * hsvTorgb218 );
				float3 hsvTorgb219 = HSVToRGB( float3(( Hue121 * _M2AHue ),( Saturation122 * _M2ASaturation ),( Value123 * _M2AValue )) );
				float3 M2A227 = ( tex2DNode222.a * hsvTorgb219 );
				float3 temp_output_167_0 = ( M1R151 + M1G152 + M1B153 + M1A154 + M2R230 + M2G229 + M2B228 + M2A227 );
				
				
				float3 Albedo = temp_output_167_0;
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
Version=17600
53;193;1636;776;-676.7683;-2026.105;1.613553;True;True
Node;AmplifyShaderEditor.SamplerNode;2;1496.381,2516.637;Inherit;True;Property;_Albedo;Albedo;0;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;260;1837.037,2505.232;Float;False;TextureBase;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;176;-99.59785,1822.35;Inherit;False;260;TextureBase;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;269;-2751.428,2469.185;Inherit;False;2071.064;2557.713;Mask 2;13;178;180;179;181;222;225;226;223;224;229;227;228;230;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RGBToHSVNode;119;174.0762,1813.136;Float;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;59;-2744.899,-223.7064;Inherit;False;2096.119;2628.729;Mask 1;13;58;6;48;37;7;28;49;23;39;151;152;153;154;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;123;470.455,1972.948;Float;False;Value;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;23;-2669.603,1763.231;Inherit;False;890.8004;612.024;Mask 1 Alpha Channel;10;17;22;18;19;21;132;20;133;15;134;;1,1,1,0.534;0;0
Node;AmplifyShaderEditor.CommentaryNode;180;-2693.74,3146.153;Inherit;False;890.8004;612.024;Mask 2 Green Channel;10;221;216;212;206;204;203;190;189;185;184;;0,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;179;-2694.799,4412.655;Inherit;False;890.8004;612.024;Mask 2 Alpha Channel;10;219;217;213;207;202;195;194;188;183;182;;1,1,1,0.534;0;0
Node;AmplifyShaderEditor.CommentaryNode;28;-2675.228,1126.771;Inherit;False;890.8004;612.024;Mask 1 Blue Channel;10;32;36;34;33;35;129;131;30;130;31;;0,0,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;181;-2700.424,3776.196;Inherit;False;890.8004;612.024;Mask 2 Blue Channel;10;218;215;209;208;205;201;197;193;192;187;;0,0,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;49;-2676.232,-130.2395;Inherit;False;890.8004;612.024;Mask 1 Red Channel;10;57;56;54;52;51;50;55;120;124;125;;1,0,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;473.6649,1812.018;Float;False;Hue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;178;-2701.428,2519.185;Inherit;False;890.8004;612.024;Mask 2 Red Channel;10;220;214;211;210;200;199;198;196;191;186;;1,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;39;-2668.544,496.7281;Inherit;False;890.8004;612.024;Mask 1 Green Channel;10;47;45;43;42;41;44;46;126;127;128;;0,1,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;122;474.358,1892.228;Float;False;Saturation;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-2623.943,978.7378;Float;False;Property;_M1GValue;M1 G Value;15;0;Create;True;0;0;False;0;1;0.32;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-2531.57,905.225;Inherit;False;123;Value;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;130;-2551.142,1164.58;Inherit;False;121;Hue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;128;-2556.689,754.7337;Inherit;False;122;Saturation;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;131;-2562.021,1345.931;Inherit;False;122;Saturation;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-2591.483,1890.522;Inherit;False;122;Saturation;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2581.231,225.0372;Float;False;Property;_M1RSaturation;M1 R Saturation;11;0;Create;True;0;0;False;0;1;1.34;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;203;-2581.885,3404.158;Inherit;False;122;Saturation;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-2626.122,1590.761;Float;False;Property;_M1BValue;M1 B Value;18;0;Create;True;0;0;False;0;1;0.41;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;132;-2567.581,2129.348;Inherit;False;123;Value;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-2633.119,2003.755;Float;False;Property;_M1ASaturation;M1 A Saturation;20;0;Create;True;0;0;False;0;0.8705882;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;182;-2660.437,4569.637;Float;False;Property;_M2AHue;M2 A Hue;31;0;Create;True;0;0;False;1;Header(Mask 2 Alpha);1;0.94;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-2630.372,827.941;Float;False;Property;_M1GSaturation;M1 G Saturation;14;0;Create;True;0;0;False;0;1;0.7;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-2608.09,1428.587;Float;False;Property;_M1BSaturation;M1 B Saturation;17;0;Create;True;0;0;False;0;1;0.7;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;199;-2611.792,2797.006;Float;False;Property;_M2RHue;M2 R Hue;22;0;Create;True;0;0;False;1;Header(Mask 2 Red);1;0.6;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;184;-2660.378,3318.151;Float;False;Property;_M2GHue;M2 G Hue;25;0;Create;True;0;0;False;1;Header(Mask 2 Green);1;1.41;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;205;-2646.041,3916.661;Float;False;Property;_M2BHue;M2 B Hue;28;0;Create;True;0;0;False;1;Header(Mask 2 Blue);1;1.09;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2629.982,2262.474;Float;False;Property;_M1AValue;M1 A Value;21;0;Create;True;0;0;False;0;1;0.65;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;-2582.704,1944.515;Inherit;False;121;Hue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;-2543.777,590.3352;Inherit;False;121;Hue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;-2545.142,1499.843;Inherit;False;123;Value;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-2655.178,4911.898;Float;False;Property;_M2AValue;M2 A Value;33;0;Create;True;0;0;False;0;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2579.615,329.7841;Float;False;Property;_M1RValue;M1 R Value;12;0;Create;True;0;0;False;0;1;0.64;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;198;-2560.956,2643.93;Inherit;False;122;Saturation;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;192;-2555.728,4166.18;Inherit;False;123;Value;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;189;-2556.766,3554.649;Inherit;False;123;Value;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;197;-2551.418,3839.279;Inherit;False;121;Hue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;191;-2548.047,2569.627;Inherit;False;121;Hue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;186;-2541.844,2714.837;Inherit;False;123;Value;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-2621.878,1805.306;Float;False;Property;_M1AHue;M1 A Hue;19;0;Create;True;0;0;False;1;Header(Mask 1 Alpha);1;0.09;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;201;-2648.922,4071.872;Float;False;Property;_M2BSaturation;M2 B Saturation;29;0;Create;True;0;0;False;0;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;190;-2568.974,3239.76;Inherit;False;121;Hue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;188;-2578.376,4664.429;Inherit;False;122;Saturation;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-2522.85,-79.79761;Inherit;False;121;Hue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;-2649.14,3628.163;Float;False;Property;_M2GValue;M2 G Value;27;0;Create;True;0;0;False;0;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;194;-2559.262,4831.438;Inherit;False;123;Value;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;195;-2658.315,4739.358;Float;False;Property;_M2ASaturation;M2 A Saturation;32;0;Create;True;0;0;False;0;0.8705882;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;193;-2651.318,4240.185;Float;False;Property;_M2BValue;M2 B Value;30;0;Create;True;0;0;False;0;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;124;-2535.76,-5.495001;Inherit;False;122;Saturation;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-2604.811,2979.208;Float;False;Property;_M2RValue;M2 R Value;24;0;Create;True;0;0;False;0;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;202;-2568.467,4491.021;Inherit;False;121;Hue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-2586.596,147.5818;Float;False;Property;_M1RHue;M1 R Hue;10;0;Create;True;0;0;False;1;Header(Mask 1 Red);1;1.25;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;204;-2655.568,3477.366;Float;False;Property;_M2GSaturation;M2 G Saturation;26;0;Create;True;0;0;False;0;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;196;-2606.427,2874.462;Float;False;Property;_M2RSaturation;M2 R Saturation;23;0;Create;True;0;0;False;0;1;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;187;-2570.336,3999.171;Inherit;False;122;Saturation;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2598.042,1252.826;Float;False;Property;_M1BHue;M1 B Hue;16;0;Create;True;0;0;False;1;Header(Mask 1 Blue);1;1.08;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2635.182,668.7261;Float;False;Property;_M1GHue;M1 G Hue;13;0;Create;True;0;0;False;1;Header(Mask 1 Green);1;1.01;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;125;-2516.648,65.41222;Inherit;False;123;Value;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;206;-2279.183,3391.088;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-2251.563,640.3604;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2240.525,303.3756;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;211;-2272.854,2643.812;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;-2271.501,2781.201;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-2215.738,2020.459;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;215;-2304.558,4161.759;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-2191.868,1557.564;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;-2276.759,3289.785;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-2221.256,1243.886;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;214;-2265.721,2952.8;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-2255.394,1883.069;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;-2264.873,4674.669;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-2253.012,847.9394;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-2211.842,2181.992;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-2220.387,1398.731;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;216;-2278.208,3497.364;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;-2270.553,4797.9;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-2246.305,131.7758;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;209;-2312.392,4029.519;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-2253.987,741.6636;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-2247.658,-5.612677;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-2327.261,3878.618;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;217;-2266.226,4537.281;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;219;-2075.408,4579.958;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;222;-1735.712,3649.529;Inherit;True;Property;_Mask2;Mask 2;9;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.HSVToRGBNode;36;-2031.132,1243.291;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.HSVToRGBNode;221;-2074.35,3313.456;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.HSVToRGBNode;17;-2050.212,1930.535;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.HSVToRGBNode;220;-2082.037,2686.49;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.HSVToRGBNode;47;-2049.153,664.0313;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;6;-1710.516,1000.104;Inherit;True;Property;_Mask1;Mask 1;8;1;[NoScaleOffset];Create;True;0;0;False;1;Header(Masks);-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.HSVToRGBNode;218;-2094.631,3978.896;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.HSVToRGBNode;57;-2056.84,37.06535;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-1246.602,633.2107;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;225;-1255.908,4540.978;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-1249.841,1282.853;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;223;-1298.346,2651.733;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-1230.712,1891.554;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;226;-1275.037,3932.278;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-1273.15,2.308438;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;224;-1271.798,3282.635;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;227;-923.364,4540.461;Float;False;M2A;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;152;-958.8002,631.2532;Float;False;M1G;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;154;-898.1682,1891.038;Float;False;M1A;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;270;-69.15874,730.6395;Inherit;False;2116.08;972.3696;Emission;8;266;268;272;263;265;271;237;275;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;229;-982.6489,3287.413;Float;False;M2G;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;230;-973.2177,2644.722;Float;False;M2R;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;151;-948.0219,-4.70237;Float;False;M1R;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;153;-911.6426,1294.156;Float;False;M1B;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;228;-935.4912,3943.581;Float;False;M2B;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;231;191.1062,2449.822;Inherit;False;230;M2R;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;161;178.5559,2274.46;Inherit;False;153;M1B;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;232;185.1897,2543.238;Inherit;False;229;M2G;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;160;178.955,2202.26;Inherit;False;152;M1G;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;233;189.676,2629.709;Inherit;False;228;M2B;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;162;182.4539,2352.46;Inherit;False;154;M1A;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;234;191.1064,2732.044;Inherit;False;227;M2A;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;159;180.0108,2130.419;Inherit;False;151;M1R;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;275;1272.07,1300.282;Inherit;False;628.3253;329.7061;Toggle Emission Option;2;259;274;;1,0.7453228,0.07075471,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;266;1663.14,1025.584;Float;False;Emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;273;1824.654,2383.792;Float;False;Property;_UseRawEmission;Use Raw Emission;6;0;Create;True;0;0;False;0;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;235;1506.372,2906.496;Inherit;True;Property;_Metalic_Smoothness;Metalic_Smoothness;1;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;236;1504.774,2709.411;Inherit;True;Property;_Normal;Normal;2;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;276;1031.266,2766.481;Float;False;Property;_NormalIntensity;Normal Intensity;3;0;Create;True;0;0;False;0;3;1;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;274;1353.13,1496.984;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;259;1345.471,1372.381;Inherit;False;266;Emission;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;272;944.4301,797.4062;Inherit;True;Overlay;True;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;265;943.8673,1109.792;Float;False;Property;_EmissionPower;Emission Power;5;0;Create;True;0;0;False;0;3;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;263;404.1066,1013.858;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;167;1844.692,2133.881;Inherit;True;8;8;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;271;599.5974,815.7463;Float;False;Property;_AddEmissiveColor;Add Emissive Color;7;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;237;-19.06968,1013.364;Inherit;True;Property;_Emission;Emission;4;1;[NoScaleOffset];Create;True;0;0;False;1;Header(Emission);-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;268;1366.916,1030.306;Inherit;True;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;285;2330.836,2331.53;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;3;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;282;2330.836,2331.53;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Malbers/Mask8Realistic;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;0;Forward;12;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;12;Workflow;1;Surface;0;  Blend;0;Two Sided;1;Cast Shadows;1;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;Meta Pass;1;Override Baked GI;0;Vertex Position,InvertActionOnDeselection;1;0;5;True;True;True;True;True;False;;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;283;2330.836,2331.53;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;284;2330.836,2331.53;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;286;2330.836,2331.53;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;4;Universal2D;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
WireConnection;260;0;2;0
WireConnection;119;0;176;0
WireConnection;123;0;119;3
WireConnection;121;0;119;1
WireConnection;122;0;119;2
WireConnection;206;0;203;0
WireConnection;206;1;204;0
WireConnection;46;0;127;0
WireConnection;46;1;41;0
WireConnection;55;0;125;0
WireConnection;55;1;50;0
WireConnection;211;0;191;0
WireConnection;211;1;199;0
WireConnection;210;0;198;0
WireConnection;210;1;196;0
WireConnection;19;0;133;0
WireConnection;19;1;20;0
WireConnection;215;0;192;0
WireConnection;215;1;193;0
WireConnection;35;0;129;0
WireConnection;35;1;32;0
WireConnection;212;0;190;0
WireConnection;212;1;184;0
WireConnection;34;0;130;0
WireConnection;34;1;30;0
WireConnection;214;0;186;0
WireConnection;214;1;200;0
WireConnection;18;0;134;0
WireConnection;18;1;15;0
WireConnection;207;0;188;0
WireConnection;207;1;195;0
WireConnection;44;0;126;0
WireConnection;44;1;42;0
WireConnection;21;0;132;0
WireConnection;21;1;22;0
WireConnection;33;0;131;0
WireConnection;33;1;31;0
WireConnection;216;0;189;0
WireConnection;216;1;185;0
WireConnection;213;0;194;0
WireConnection;213;1;183;0
WireConnection;54;0;124;0
WireConnection;54;1;51;0
WireConnection;209;0;187;0
WireConnection;209;1;201;0
WireConnection;45;0;128;0
WireConnection;45;1;43;0
WireConnection;56;0;120;0
WireConnection;56;1;52;0
WireConnection;208;0;197;0
WireConnection;208;1;205;0
WireConnection;217;0;202;0
WireConnection;217;1;182;0
WireConnection;219;0;217;0
WireConnection;219;1;207;0
WireConnection;219;2;213;0
WireConnection;36;0;34;0
WireConnection;36;1;33;0
WireConnection;36;2;35;0
WireConnection;221;0;212;0
WireConnection;221;1;206;0
WireConnection;221;2;216;0
WireConnection;17;0;18;0
WireConnection;17;1;19;0
WireConnection;17;2;21;0
WireConnection;220;0;211;0
WireConnection;220;1;210;0
WireConnection;220;2;214;0
WireConnection;47;0;46;0
WireConnection;47;1;45;0
WireConnection;47;2;44;0
WireConnection;218;0;208;0
WireConnection;218;1;209;0
WireConnection;218;2;215;0
WireConnection;57;0;56;0
WireConnection;57;1;54;0
WireConnection;57;2;55;0
WireConnection;48;0;6;2
WireConnection;48;1;47;0
WireConnection;225;0;222;4
WireConnection;225;1;219;0
WireConnection;37;0;6;3
WireConnection;37;1;36;0
WireConnection;223;0;222;1
WireConnection;223;1;220;0
WireConnection;7;0;6;4
WireConnection;7;1;17;0
WireConnection;226;0;222;3
WireConnection;226;1;218;0
WireConnection;58;0;6;1
WireConnection;58;1;57;0
WireConnection;224;0;222;2
WireConnection;224;1;221;0
WireConnection;227;0;225;0
WireConnection;152;0;48;0
WireConnection;154;0;7;0
WireConnection;229;0;224;0
WireConnection;230;0;223;0
WireConnection;151;0;58;0
WireConnection;153;0;37;0
WireConnection;228;0;226;0
WireConnection;266;0;268;0
WireConnection;273;0;259;0
WireConnection;273;1;274;0
WireConnection;236;5;276;0
WireConnection;274;0;265;0
WireConnection;274;1;237;0
WireConnection;272;0;271;0
WireConnection;272;1;167;0
WireConnection;263;0;237;0
WireConnection;167;0;159;0
WireConnection;167;1;160;0
WireConnection;167;2;161;0
WireConnection;167;3;162;0
WireConnection;167;4;231;0
WireConnection;167;5;232;0
WireConnection;167;6;233;0
WireConnection;167;7;234;0
WireConnection;268;0;263;0
WireConnection;268;1;265;0
WireConnection;268;2;272;0
WireConnection;282;0;167;0
WireConnection;282;1;236;0
WireConnection;282;2;273;0
WireConnection;282;3;235;1
WireConnection;282;4;235;4
ASEEND*/
//CHKSM=D8F83DA974CDFC2E47DCD8009CD21CC91FCB0CF7