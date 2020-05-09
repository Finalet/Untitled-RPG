// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Color4x3"
{
	Properties
	{
		_Color1("Color 1", Color) = (1,0.1544118,0.1544118,0.397)
		_Color2("Color 2", Color) = (1,0.1544118,0.8017241,0.334)
		_Color3("Color 3", Color) = (0.2535501,0.1544118,1,0.228)
		_Color4("Color 4", Color) = (0.1544118,0.5451319,1,0.472)
		_Color5("Color 5", Color) = (0.9533468,1,0.1544118,0.353)
		_Color6("Color 6", Color) = (0.8483773,1,0.1544118,0.341)
		_Color7("Color 7", Color) = (0.1544118,0.6151115,1,0.316)
		_Color8("Color 8", Color) = (0.4849697,0.5008695,0.5073529,0.484)
		_Color9("Color 9", Color) = (0.9099331,0.9264706,0.6267301,0.353)
		_Color10("Color 10", Color) = (0.1544118,0.1602434,1,0.341)
		_Color11("Color 11", Color) = (1,0.1544118,0.381846,0.316)
		_Color12("Color 12", Color) = (0.02270761,0.1632713,0.2205882,0.484)
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_Metallic("Metallic", Range( 0 , 1)) = 0

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
			
			

			CBUFFER_START( UnityPerMaterial )
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float4 _Color5;
			float4 _Color6;
			float4 _Color7;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color12;
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
				float4 shadowCoord : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
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

				float2 uv02_g197 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g197 = 1.0;
				float temp_output_7_0_g197 = 4.0;
				float temp_output_9_0_g197 = 3.0;
				float temp_output_8_0_g197 = 3.0;
				float2 uv02_g195 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g195 = 2.0;
				float temp_output_7_0_g195 = 4.0;
				float temp_output_9_0_g195 = 3.0;
				float temp_output_8_0_g195 = 3.0;
				float2 uv02_g201 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g201 = 3.0;
				float temp_output_7_0_g201 = 4.0;
				float temp_output_9_0_g201 = 3.0;
				float temp_output_8_0_g201 = 3.0;
				float2 uv02_g187 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g187 = 4.0;
				float temp_output_7_0_g187 = 4.0;
				float temp_output_9_0_g187 = 3.0;
				float temp_output_8_0_g187 = 3.0;
				float2 uv02_g193 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g193 = 1.0;
				float temp_output_7_0_g193 = 4.0;
				float temp_output_9_0_g193 = 2.0;
				float temp_output_8_0_g193 = 3.0;
				float2 uv02_g199 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g199 = 2.0;
				float temp_output_7_0_g199 = 4.0;
				float temp_output_9_0_g199 = 2.0;
				float temp_output_8_0_g199 = 3.0;
				float2 uv02_g200 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g200 = 3.0;
				float temp_output_7_0_g200 = 4.0;
				float temp_output_9_0_g200 = 2.0;
				float temp_output_8_0_g200 = 3.0;
				float2 uv02_g196 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g196 = 4.0;
				float temp_output_7_0_g196 = 4.0;
				float temp_output_9_0_g196 = 2.0;
				float temp_output_8_0_g196 = 3.0;
				float2 uv02_g192 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g192 = 1.0;
				float temp_output_7_0_g192 = 4.0;
				float temp_output_9_0_g192 = 1.0;
				float temp_output_8_0_g192 = 3.0;
				float2 uv02_g191 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g191 = 2.0;
				float temp_output_7_0_g191 = 4.0;
				float temp_output_9_0_g191 = 1.0;
				float temp_output_8_0_g191 = 3.0;
				float2 uv02_g198 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g198 = 3.0;
				float temp_output_7_0_g198 = 4.0;
				float temp_output_9_0_g198 = 1.0;
				float temp_output_8_0_g198 = 3.0;
				float2 uv02_g194 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g194 = 4.0;
				float temp_output_7_0_g194 = 4.0;
				float temp_output_9_0_g194 = 1.0;
				float temp_output_8_0_g194 = 3.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( uv02_g197.x , ( ( temp_output_3_0_g197 - 1.0 ) / temp_output_7_0_g197 ) ) ) * ( step( uv02_g197.x , ( temp_output_3_0_g197 / temp_output_7_0_g197 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g197.y , ( ( temp_output_9_0_g197 - 1.0 ) / temp_output_8_0_g197 ) ) ) * ( step( uv02_g197.y , ( temp_output_9_0_g197 / temp_output_8_0_g197 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( uv02_g195.x , ( ( temp_output_3_0_g195 - 1.0 ) / temp_output_7_0_g195 ) ) ) * ( step( uv02_g195.x , ( temp_output_3_0_g195 / temp_output_7_0_g195 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g195.y , ( ( temp_output_9_0_g195 - 1.0 ) / temp_output_8_0_g195 ) ) ) * ( step( uv02_g195.y , ( temp_output_9_0_g195 / temp_output_8_0_g195 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( uv02_g201.x , ( ( temp_output_3_0_g201 - 1.0 ) / temp_output_7_0_g201 ) ) ) * ( step( uv02_g201.x , ( temp_output_3_0_g201 / temp_output_7_0_g201 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g201.y , ( ( temp_output_9_0_g201 - 1.0 ) / temp_output_8_0_g201 ) ) ) * ( step( uv02_g201.y , ( temp_output_9_0_g201 / temp_output_8_0_g201 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( uv02_g187.x , ( ( temp_output_3_0_g187 - 1.0 ) / temp_output_7_0_g187 ) ) ) * ( step( uv02_g187.x , ( temp_output_3_0_g187 / temp_output_7_0_g187 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g187.y , ( ( temp_output_9_0_g187 - 1.0 ) / temp_output_8_0_g187 ) ) ) * ( step( uv02_g187.y , ( temp_output_9_0_g187 / temp_output_8_0_g187 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( uv02_g193.x , ( ( temp_output_3_0_g193 - 1.0 ) / temp_output_7_0_g193 ) ) ) * ( step( uv02_g193.x , ( temp_output_3_0_g193 / temp_output_7_0_g193 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g193.y , ( ( temp_output_9_0_g193 - 1.0 ) / temp_output_8_0_g193 ) ) ) * ( step( uv02_g193.y , ( temp_output_9_0_g193 / temp_output_8_0_g193 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( uv02_g199.x , ( ( temp_output_3_0_g199 - 1.0 ) / temp_output_7_0_g199 ) ) ) * ( step( uv02_g199.x , ( temp_output_3_0_g199 / temp_output_7_0_g199 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g199.y , ( ( temp_output_9_0_g199 - 1.0 ) / temp_output_8_0_g199 ) ) ) * ( step( uv02_g199.y , ( temp_output_9_0_g199 / temp_output_8_0_g199 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( uv02_g200.x , ( ( temp_output_3_0_g200 - 1.0 ) / temp_output_7_0_g200 ) ) ) * ( step( uv02_g200.x , ( temp_output_3_0_g200 / temp_output_7_0_g200 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g200.y , ( ( temp_output_9_0_g200 - 1.0 ) / temp_output_8_0_g200 ) ) ) * ( step( uv02_g200.y , ( temp_output_9_0_g200 / temp_output_8_0_g200 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( uv02_g196.x , ( ( temp_output_3_0_g196 - 1.0 ) / temp_output_7_0_g196 ) ) ) * ( step( uv02_g196.x , ( temp_output_3_0_g196 / temp_output_7_0_g196 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g196.y , ( ( temp_output_9_0_g196 - 1.0 ) / temp_output_8_0_g196 ) ) ) * ( step( uv02_g196.y , ( temp_output_9_0_g196 / temp_output_8_0_g196 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( uv02_g192.x , ( ( temp_output_3_0_g192 - 1.0 ) / temp_output_7_0_g192 ) ) ) * ( step( uv02_g192.x , ( temp_output_3_0_g192 / temp_output_7_0_g192 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g192.y , ( ( temp_output_9_0_g192 - 1.0 ) / temp_output_8_0_g192 ) ) ) * ( step( uv02_g192.y , ( temp_output_9_0_g192 / temp_output_8_0_g192 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( uv02_g191.x , ( ( temp_output_3_0_g191 - 1.0 ) / temp_output_7_0_g191 ) ) ) * ( step( uv02_g191.x , ( temp_output_3_0_g191 / temp_output_7_0_g191 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g191.y , ( ( temp_output_9_0_g191 - 1.0 ) / temp_output_8_0_g191 ) ) ) * ( step( uv02_g191.y , ( temp_output_9_0_g191 / temp_output_8_0_g191 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( uv02_g198.x , ( ( temp_output_3_0_g198 - 1.0 ) / temp_output_7_0_g198 ) ) ) * ( step( uv02_g198.x , ( temp_output_3_0_g198 / temp_output_7_0_g198 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g198.y , ( ( temp_output_9_0_g198 - 1.0 ) / temp_output_8_0_g198 ) ) ) * ( step( uv02_g198.y , ( temp_output_9_0_g198 / temp_output_8_0_g198 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( uv02_g194.x , ( ( temp_output_3_0_g194 - 1.0 ) / temp_output_7_0_g194 ) ) ) * ( step( uv02_g194.x , ( temp_output_3_0_g194 / temp_output_7_0_g194 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g194.y , ( ( temp_output_9_0_g194 - 1.0 ) / temp_output_8_0_g194 ) ) ) * ( step( uv02_g194.y , ( temp_output_9_0_g194 / temp_output_8_0_g194 ) ) * 1.0 ) ) ) ) ) );
				
				float3 Albedo = temp_output_155_0.rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = _Metallic;
				float Smoothness = ( (temp_output_155_0).a * _Smoothness );
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
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float4 _Color5;
			float4 _Color6;
			float4 _Color7;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color12;
			float _Metallic;
			float _Smoothness;
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
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float4 _Color5;
			float4 _Color6;
			float4 _Color7;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color12;
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
			#define ASE_SRP_VERSION 70107

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			CBUFFER_START( UnityPerMaterial )
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float4 _Color5;
			float4 _Color6;
			float4 _Color7;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color12;
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
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
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

				float2 uv02_g197 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g197 = 1.0;
				float temp_output_7_0_g197 = 4.0;
				float temp_output_9_0_g197 = 3.0;
				float temp_output_8_0_g197 = 3.0;
				float2 uv02_g195 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g195 = 2.0;
				float temp_output_7_0_g195 = 4.0;
				float temp_output_9_0_g195 = 3.0;
				float temp_output_8_0_g195 = 3.0;
				float2 uv02_g201 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g201 = 3.0;
				float temp_output_7_0_g201 = 4.0;
				float temp_output_9_0_g201 = 3.0;
				float temp_output_8_0_g201 = 3.0;
				float2 uv02_g187 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g187 = 4.0;
				float temp_output_7_0_g187 = 4.0;
				float temp_output_9_0_g187 = 3.0;
				float temp_output_8_0_g187 = 3.0;
				float2 uv02_g193 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g193 = 1.0;
				float temp_output_7_0_g193 = 4.0;
				float temp_output_9_0_g193 = 2.0;
				float temp_output_8_0_g193 = 3.0;
				float2 uv02_g199 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g199 = 2.0;
				float temp_output_7_0_g199 = 4.0;
				float temp_output_9_0_g199 = 2.0;
				float temp_output_8_0_g199 = 3.0;
				float2 uv02_g200 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g200 = 3.0;
				float temp_output_7_0_g200 = 4.0;
				float temp_output_9_0_g200 = 2.0;
				float temp_output_8_0_g200 = 3.0;
				float2 uv02_g196 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g196 = 4.0;
				float temp_output_7_0_g196 = 4.0;
				float temp_output_9_0_g196 = 2.0;
				float temp_output_8_0_g196 = 3.0;
				float2 uv02_g192 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g192 = 1.0;
				float temp_output_7_0_g192 = 4.0;
				float temp_output_9_0_g192 = 1.0;
				float temp_output_8_0_g192 = 3.0;
				float2 uv02_g191 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g191 = 2.0;
				float temp_output_7_0_g191 = 4.0;
				float temp_output_9_0_g191 = 1.0;
				float temp_output_8_0_g191 = 3.0;
				float2 uv02_g198 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g198 = 3.0;
				float temp_output_7_0_g198 = 4.0;
				float temp_output_9_0_g198 = 1.0;
				float temp_output_8_0_g198 = 3.0;
				float2 uv02_g194 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g194 = 4.0;
				float temp_output_7_0_g194 = 4.0;
				float temp_output_9_0_g194 = 1.0;
				float temp_output_8_0_g194 = 3.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( uv02_g197.x , ( ( temp_output_3_0_g197 - 1.0 ) / temp_output_7_0_g197 ) ) ) * ( step( uv02_g197.x , ( temp_output_3_0_g197 / temp_output_7_0_g197 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g197.y , ( ( temp_output_9_0_g197 - 1.0 ) / temp_output_8_0_g197 ) ) ) * ( step( uv02_g197.y , ( temp_output_9_0_g197 / temp_output_8_0_g197 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( uv02_g195.x , ( ( temp_output_3_0_g195 - 1.0 ) / temp_output_7_0_g195 ) ) ) * ( step( uv02_g195.x , ( temp_output_3_0_g195 / temp_output_7_0_g195 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g195.y , ( ( temp_output_9_0_g195 - 1.0 ) / temp_output_8_0_g195 ) ) ) * ( step( uv02_g195.y , ( temp_output_9_0_g195 / temp_output_8_0_g195 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( uv02_g201.x , ( ( temp_output_3_0_g201 - 1.0 ) / temp_output_7_0_g201 ) ) ) * ( step( uv02_g201.x , ( temp_output_3_0_g201 / temp_output_7_0_g201 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g201.y , ( ( temp_output_9_0_g201 - 1.0 ) / temp_output_8_0_g201 ) ) ) * ( step( uv02_g201.y , ( temp_output_9_0_g201 / temp_output_8_0_g201 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( uv02_g187.x , ( ( temp_output_3_0_g187 - 1.0 ) / temp_output_7_0_g187 ) ) ) * ( step( uv02_g187.x , ( temp_output_3_0_g187 / temp_output_7_0_g187 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g187.y , ( ( temp_output_9_0_g187 - 1.0 ) / temp_output_8_0_g187 ) ) ) * ( step( uv02_g187.y , ( temp_output_9_0_g187 / temp_output_8_0_g187 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( uv02_g193.x , ( ( temp_output_3_0_g193 - 1.0 ) / temp_output_7_0_g193 ) ) ) * ( step( uv02_g193.x , ( temp_output_3_0_g193 / temp_output_7_0_g193 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g193.y , ( ( temp_output_9_0_g193 - 1.0 ) / temp_output_8_0_g193 ) ) ) * ( step( uv02_g193.y , ( temp_output_9_0_g193 / temp_output_8_0_g193 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( uv02_g199.x , ( ( temp_output_3_0_g199 - 1.0 ) / temp_output_7_0_g199 ) ) ) * ( step( uv02_g199.x , ( temp_output_3_0_g199 / temp_output_7_0_g199 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g199.y , ( ( temp_output_9_0_g199 - 1.0 ) / temp_output_8_0_g199 ) ) ) * ( step( uv02_g199.y , ( temp_output_9_0_g199 / temp_output_8_0_g199 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( uv02_g200.x , ( ( temp_output_3_0_g200 - 1.0 ) / temp_output_7_0_g200 ) ) ) * ( step( uv02_g200.x , ( temp_output_3_0_g200 / temp_output_7_0_g200 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g200.y , ( ( temp_output_9_0_g200 - 1.0 ) / temp_output_8_0_g200 ) ) ) * ( step( uv02_g200.y , ( temp_output_9_0_g200 / temp_output_8_0_g200 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( uv02_g196.x , ( ( temp_output_3_0_g196 - 1.0 ) / temp_output_7_0_g196 ) ) ) * ( step( uv02_g196.x , ( temp_output_3_0_g196 / temp_output_7_0_g196 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g196.y , ( ( temp_output_9_0_g196 - 1.0 ) / temp_output_8_0_g196 ) ) ) * ( step( uv02_g196.y , ( temp_output_9_0_g196 / temp_output_8_0_g196 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( uv02_g192.x , ( ( temp_output_3_0_g192 - 1.0 ) / temp_output_7_0_g192 ) ) ) * ( step( uv02_g192.x , ( temp_output_3_0_g192 / temp_output_7_0_g192 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g192.y , ( ( temp_output_9_0_g192 - 1.0 ) / temp_output_8_0_g192 ) ) ) * ( step( uv02_g192.y , ( temp_output_9_0_g192 / temp_output_8_0_g192 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( uv02_g191.x , ( ( temp_output_3_0_g191 - 1.0 ) / temp_output_7_0_g191 ) ) ) * ( step( uv02_g191.x , ( temp_output_3_0_g191 / temp_output_7_0_g191 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g191.y , ( ( temp_output_9_0_g191 - 1.0 ) / temp_output_8_0_g191 ) ) ) * ( step( uv02_g191.y , ( temp_output_9_0_g191 / temp_output_8_0_g191 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( uv02_g198.x , ( ( temp_output_3_0_g198 - 1.0 ) / temp_output_7_0_g198 ) ) ) * ( step( uv02_g198.x , ( temp_output_3_0_g198 / temp_output_7_0_g198 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g198.y , ( ( temp_output_9_0_g198 - 1.0 ) / temp_output_8_0_g198 ) ) ) * ( step( uv02_g198.y , ( temp_output_9_0_g198 / temp_output_8_0_g198 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( uv02_g194.x , ( ( temp_output_3_0_g194 - 1.0 ) / temp_output_7_0_g194 ) ) ) * ( step( uv02_g194.x , ( temp_output_3_0_g194 / temp_output_7_0_g194 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g194.y , ( ( temp_output_9_0_g194 - 1.0 ) / temp_output_8_0_g194 ) ) ) * ( step( uv02_g194.y , ( temp_output_9_0_g194 / temp_output_8_0_g194 ) ) * 1.0 ) ) ) ) ) );
				
				
				float3 Albedo = temp_output_155_0.rgb;
				float3 Emission = 0;
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
			
			

			CBUFFER_START( UnityPerMaterial )
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float4 _Color5;
			float4 _Color6;
			float4 _Color7;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color12;
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
				float4 ase_texcoord : TEXCOORD0;
			};

			
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
				float2 uv02_g197 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g197 = 1.0;
				float temp_output_7_0_g197 = 4.0;
				float temp_output_9_0_g197 = 3.0;
				float temp_output_8_0_g197 = 3.0;
				float2 uv02_g195 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g195 = 2.0;
				float temp_output_7_0_g195 = 4.0;
				float temp_output_9_0_g195 = 3.0;
				float temp_output_8_0_g195 = 3.0;
				float2 uv02_g201 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g201 = 3.0;
				float temp_output_7_0_g201 = 4.0;
				float temp_output_9_0_g201 = 3.0;
				float temp_output_8_0_g201 = 3.0;
				float2 uv02_g187 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g187 = 4.0;
				float temp_output_7_0_g187 = 4.0;
				float temp_output_9_0_g187 = 3.0;
				float temp_output_8_0_g187 = 3.0;
				float2 uv02_g193 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g193 = 1.0;
				float temp_output_7_0_g193 = 4.0;
				float temp_output_9_0_g193 = 2.0;
				float temp_output_8_0_g193 = 3.0;
				float2 uv02_g199 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g199 = 2.0;
				float temp_output_7_0_g199 = 4.0;
				float temp_output_9_0_g199 = 2.0;
				float temp_output_8_0_g199 = 3.0;
				float2 uv02_g200 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g200 = 3.0;
				float temp_output_7_0_g200 = 4.0;
				float temp_output_9_0_g200 = 2.0;
				float temp_output_8_0_g200 = 3.0;
				float2 uv02_g196 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g196 = 4.0;
				float temp_output_7_0_g196 = 4.0;
				float temp_output_9_0_g196 = 2.0;
				float temp_output_8_0_g196 = 3.0;
				float2 uv02_g192 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g192 = 1.0;
				float temp_output_7_0_g192 = 4.0;
				float temp_output_9_0_g192 = 1.0;
				float temp_output_8_0_g192 = 3.0;
				float2 uv02_g191 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g191 = 2.0;
				float temp_output_7_0_g191 = 4.0;
				float temp_output_9_0_g191 = 1.0;
				float temp_output_8_0_g191 = 3.0;
				float2 uv02_g198 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g198 = 3.0;
				float temp_output_7_0_g198 = 4.0;
				float temp_output_9_0_g198 = 1.0;
				float temp_output_8_0_g198 = 3.0;
				float2 uv02_g194 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g194 = 4.0;
				float temp_output_7_0_g194 = 4.0;
				float temp_output_9_0_g194 = 1.0;
				float temp_output_8_0_g194 = 3.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( uv02_g197.x , ( ( temp_output_3_0_g197 - 1.0 ) / temp_output_7_0_g197 ) ) ) * ( step( uv02_g197.x , ( temp_output_3_0_g197 / temp_output_7_0_g197 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g197.y , ( ( temp_output_9_0_g197 - 1.0 ) / temp_output_8_0_g197 ) ) ) * ( step( uv02_g197.y , ( temp_output_9_0_g197 / temp_output_8_0_g197 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( uv02_g195.x , ( ( temp_output_3_0_g195 - 1.0 ) / temp_output_7_0_g195 ) ) ) * ( step( uv02_g195.x , ( temp_output_3_0_g195 / temp_output_7_0_g195 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g195.y , ( ( temp_output_9_0_g195 - 1.0 ) / temp_output_8_0_g195 ) ) ) * ( step( uv02_g195.y , ( temp_output_9_0_g195 / temp_output_8_0_g195 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( uv02_g201.x , ( ( temp_output_3_0_g201 - 1.0 ) / temp_output_7_0_g201 ) ) ) * ( step( uv02_g201.x , ( temp_output_3_0_g201 / temp_output_7_0_g201 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g201.y , ( ( temp_output_9_0_g201 - 1.0 ) / temp_output_8_0_g201 ) ) ) * ( step( uv02_g201.y , ( temp_output_9_0_g201 / temp_output_8_0_g201 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( uv02_g187.x , ( ( temp_output_3_0_g187 - 1.0 ) / temp_output_7_0_g187 ) ) ) * ( step( uv02_g187.x , ( temp_output_3_0_g187 / temp_output_7_0_g187 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g187.y , ( ( temp_output_9_0_g187 - 1.0 ) / temp_output_8_0_g187 ) ) ) * ( step( uv02_g187.y , ( temp_output_9_0_g187 / temp_output_8_0_g187 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( uv02_g193.x , ( ( temp_output_3_0_g193 - 1.0 ) / temp_output_7_0_g193 ) ) ) * ( step( uv02_g193.x , ( temp_output_3_0_g193 / temp_output_7_0_g193 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g193.y , ( ( temp_output_9_0_g193 - 1.0 ) / temp_output_8_0_g193 ) ) ) * ( step( uv02_g193.y , ( temp_output_9_0_g193 / temp_output_8_0_g193 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( uv02_g199.x , ( ( temp_output_3_0_g199 - 1.0 ) / temp_output_7_0_g199 ) ) ) * ( step( uv02_g199.x , ( temp_output_3_0_g199 / temp_output_7_0_g199 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g199.y , ( ( temp_output_9_0_g199 - 1.0 ) / temp_output_8_0_g199 ) ) ) * ( step( uv02_g199.y , ( temp_output_9_0_g199 / temp_output_8_0_g199 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( uv02_g200.x , ( ( temp_output_3_0_g200 - 1.0 ) / temp_output_7_0_g200 ) ) ) * ( step( uv02_g200.x , ( temp_output_3_0_g200 / temp_output_7_0_g200 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g200.y , ( ( temp_output_9_0_g200 - 1.0 ) / temp_output_8_0_g200 ) ) ) * ( step( uv02_g200.y , ( temp_output_9_0_g200 / temp_output_8_0_g200 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( uv02_g196.x , ( ( temp_output_3_0_g196 - 1.0 ) / temp_output_7_0_g196 ) ) ) * ( step( uv02_g196.x , ( temp_output_3_0_g196 / temp_output_7_0_g196 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g196.y , ( ( temp_output_9_0_g196 - 1.0 ) / temp_output_8_0_g196 ) ) ) * ( step( uv02_g196.y , ( temp_output_9_0_g196 / temp_output_8_0_g196 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( uv02_g192.x , ( ( temp_output_3_0_g192 - 1.0 ) / temp_output_7_0_g192 ) ) ) * ( step( uv02_g192.x , ( temp_output_3_0_g192 / temp_output_7_0_g192 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g192.y , ( ( temp_output_9_0_g192 - 1.0 ) / temp_output_8_0_g192 ) ) ) * ( step( uv02_g192.y , ( temp_output_9_0_g192 / temp_output_8_0_g192 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( uv02_g191.x , ( ( temp_output_3_0_g191 - 1.0 ) / temp_output_7_0_g191 ) ) ) * ( step( uv02_g191.x , ( temp_output_3_0_g191 / temp_output_7_0_g191 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g191.y , ( ( temp_output_9_0_g191 - 1.0 ) / temp_output_8_0_g191 ) ) ) * ( step( uv02_g191.y , ( temp_output_9_0_g191 / temp_output_8_0_g191 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( uv02_g198.x , ( ( temp_output_3_0_g198 - 1.0 ) / temp_output_7_0_g198 ) ) ) * ( step( uv02_g198.x , ( temp_output_3_0_g198 / temp_output_7_0_g198 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g198.y , ( ( temp_output_9_0_g198 - 1.0 ) / temp_output_8_0_g198 ) ) ) * ( step( uv02_g198.y , ( temp_output_9_0_g198 / temp_output_8_0_g198 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( uv02_g194.x , ( ( temp_output_3_0_g194 - 1.0 ) / temp_output_7_0_g194 ) ) ) * ( step( uv02_g194.x , ( temp_output_3_0_g194 / temp_output_7_0_g194 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g194.y , ( ( temp_output_9_0_g194 - 1.0 ) / temp_output_8_0_g194 ) ) ) * ( step( uv02_g194.y , ( temp_output_9_0_g194 / temp_output_8_0_g194 ) ) * 1.0 ) ) ) ) ) );
				
				
				float3 Albedo = temp_output_155_0.rgb;
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
36;165;1636;782;-567.0062;137.3818;1.409096;True;True
Node;AmplifyShaderEditor.ColorNode;23;-199.8005,-326.2955;Float;False;Property;_Color1;Color 1;0;0;Create;True;0;0;False;0;1,0.1544118,0.1544118,0.397;0.6886792,0.6886792,0.6886792,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;156;-195.9079,947.3851;Float;False;Property;_Color6;Color 6;5;0;Create;True;0;0;False;0;0.8483773,1,0.1544118,0.341;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;150;-207.7412,-66.93771;Float;False;Property;_Color2;Color 2;1;0;Create;True;0;0;False;0;1,0.1544118,0.8017241,0.334;0.735849,0.735849,0.735849,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;152;-194.2135,166.9271;Float;False;Property;_Color3;Color 3;2;0;Create;True;0;0;False;0;0.2535501,0.1544118,1,0.228;0.4716981,0.4716981,0.4716981,0.5450981;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;159;-187.9672,688.0273;Float;False;Property;_Color5;Color 5;4;0;Create;True;0;0;False;0;0.9533468,1,0.1544118,0.353;0.7169812,0.7169812,0.7169812,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;183;-194.742,1695.03;Float;False;Property;_Color9;Color 9;8;0;Create;True;0;0;False;0;0.9099331,0.9264706,0.6267301,0.353;0.2447045,0.4433962,0.2663014,0.9098039;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;154;-195.6228,411.2479;Float;False;Property;_Color4;Color 4;3;0;Create;True;0;0;False;0;0.1544118,0.5451319,1,0.472;0.764151,0.764151,0.764151,0.472;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;158;-183.7895,1424.406;Float;False;Property;_Color8;Color 8;7;0;Create;True;0;0;False;0;0.4849697,0.5008695,0.5073529,0.484;0.1981132,0.1034699,0.04205232,0.484;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;181;-202.6826,1954.387;Float;False;Property;_Color10;Color 10;9;0;Create;True;0;0;False;0;0.1544118,0.1602434,1,0.341;0.0464578,0.3396226,0.1321619,0.7058824;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;180;-189.1549,2188.253;Float;False;Property;_Color11;Color 11;10;0;Create;True;0;0;False;0;1,0.1544118,0.381846,0.316;0.3773585,0.1811142,0.1156995,0.8156863;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;182;-190.5642,2431.409;Float;False;Property;_Color12;Color 12;11;0;Create;True;0;0;False;0;0.02270761,0.1632713,0.2205882,0.484;0.6415094,0.1845852,0.4056335,0.5450981;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;157;-182.3802,1181.25;Float;False;Property;_Color7;Color 7;6;0;Create;True;0;0;False;0;0.1544118,0.6151115,1,0.316;0.3113208,0.2746084,0.2746084,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;188;200.4478,2172.165;Inherit;True;ColorShartSlot;-1;;198;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;151;121.5042,167.0022;Inherit;True;ColorShartSlot;-1;;201;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;161;133.3375,1181.325;Inherit;True;ColorShartSlot;-1;;200;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;160;119.8096,947.4603;Inherit;True;ColorShartSlot;-1;;199;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;145;115.9171,-326.2204;Inherit;True;ColorShartSlot;-1;;197;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;162;133.8517,1424.481;Inherit;True;ColorShartSlot;-1;;196;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;190;200.9619,2415.321;Inherit;True;ColorShartSlot;-1;;194;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;163;127.7504,688.1025;Inherit;True;ColorShartSlot;-1;;193;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;184;194.8606,1678.942;Inherit;True;ColorShartSlot;-1;;192;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;186;186.9198,1938.3;Inherit;True;ColorShartSlot;-1;;191;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;153;122.0185,410.1585;Inherit;True;ColorShartSlot;-1;;187;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;149;107.9764,-66.86263;Inherit;True;ColorShartSlot;-1;;195;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;146;1124.026,-170.6852;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;1130.732,57.40811;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;193;1126.266,334.7972;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;165;1691.967,238.6589;Float;False;Property;_Metallic;Metallic;13;0;Create;True;0;0;False;0;0;0.742;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;1803.628,700.3177;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;1378.894,-29.6249;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;166;1496.486,1037.67;Float;False;Property;_Smoothness;Smoothness;12;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;194;1523.734,407.4271;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;202;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;203;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;3;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;200;2076.697,169.3291;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Malbers/Color4x3;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;0;Forward;12;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;12;Workflow;1;Surface;0;  Blend;0;Two Sided;1;Cast Shadows;1;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;Meta Pass;1;Override Baked GI;0;Vertex Position,InvertActionOnDeselection;1;0;5;True;True;True;True;True;False;;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;201;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;204;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;4;Universal2D;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
WireConnection;188;38;180;0
WireConnection;151;38;152;0
WireConnection;161;38;157;0
WireConnection;160;38;156;0
WireConnection;145;38;23;0
WireConnection;162;38;158;0
WireConnection;190;38;182;0
WireConnection;163;38;159;0
WireConnection;184;38;183;0
WireConnection;186;38;181;0
WireConnection;153;38;154;0
WireConnection;149;38;150;0
WireConnection;146;0;145;0
WireConnection;146;1;149;0
WireConnection;146;2;151;0
WireConnection;146;3;153;0
WireConnection;164;0;163;0
WireConnection;164;1;160;0
WireConnection;164;2;161;0
WireConnection;164;3;162;0
WireConnection;193;0;184;0
WireConnection;193;1;186;0
WireConnection;193;2;188;0
WireConnection;193;3;190;0
WireConnection;178;0;194;0
WireConnection;178;1;166;0
WireConnection;155;0;146;0
WireConnection;155;1;164;0
WireConnection;155;2;193;0
WireConnection;194;0;155;0
WireConnection;200;0;155;0
WireConnection;200;3;165;0
WireConnection;200;4;178;0
ASEEND*/
//CHKSM=3DFC75EE27F23AFB21ED61B4682AC16CFF21F72D