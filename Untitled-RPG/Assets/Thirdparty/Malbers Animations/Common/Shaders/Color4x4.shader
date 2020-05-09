// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Color4x4"
{
	Properties
	{
		_Color1("Color 1", Color) = (1,0.1544118,0.1544118,0.291)
		_Color2("Color 2", Color) = (1,0.1544118,0.8017241,0.253)
		_Color3("Color 3", Color) = (0.2535501,0.1544118,1,0.541)
		_Color4("Color 4", Color) = (0.1544118,0.5451319,1,0.253)
		_Color5("Color 5", Color) = (0.9533468,1,0.1544118,0.553)
		_Color6("Color 6", Color) = (0.2720588,0.1294625,0,0.097)
		_Color7("Color 7", Color) = (0.1544118,0.6151115,1,0.178)
		_Color8("Color 8", Color) = (0.4849697,0.5008695,0.5073529,0.078)
		_Color9("Color 9", Color) = (0.3164301,0,0.7058823,0.134)
		_Color10("Color 10", Color) = (0.362069,0.4411765,0,0.759)
		_Color11("Color 11", Color) = (0.6691177,0.6691177,0.6691177,0.647)
		_Color12("Color 12", Color) = (0.5073529,0.1574544,0,0.128)
		_Color13("Color 13", Color) = (1,0.5586207,0,0.272)
		_Color14("Color 14", Color) = (0,0.8025862,0.875,0.047)
		_Color15("Color 15", Color) = (1,0,0,0.391)
		_Color16("Color 16", Color) = (0.4080882,0.75,0.4811866,0.134)
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
			float4 _Color13;
			float4 _Color14;
			float4 _Color15;
			float4 _Color16;
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

				float2 uv02_g263 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g263 = 1.0;
				float temp_output_7_0_g263 = 4.0;
				float temp_output_9_0_g263 = 4.0;
				float temp_output_8_0_g263 = 4.0;
				float2 uv02_g260 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g260 = 2.0;
				float temp_output_7_0_g260 = 4.0;
				float temp_output_9_0_g260 = 4.0;
				float temp_output_8_0_g260 = 4.0;
				float2 uv02_g251 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g251 = 3.0;
				float temp_output_7_0_g251 = 4.0;
				float temp_output_9_0_g251 = 4.0;
				float temp_output_8_0_g251 = 4.0;
				float2 uv02_g268 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g268 = 4.0;
				float temp_output_7_0_g268 = 4.0;
				float temp_output_9_0_g268 = 4.0;
				float temp_output_8_0_g268 = 4.0;
				float2 uv02_g267 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g267 = 1.0;
				float temp_output_7_0_g267 = 4.0;
				float temp_output_9_0_g267 = 3.0;
				float temp_output_8_0_g267 = 4.0;
				float2 uv02_g265 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g265 = 2.0;
				float temp_output_7_0_g265 = 4.0;
				float temp_output_9_0_g265 = 3.0;
				float temp_output_8_0_g265 = 4.0;
				float2 uv02_g266 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g266 = 3.0;
				float temp_output_7_0_g266 = 4.0;
				float temp_output_9_0_g266 = 3.0;
				float temp_output_8_0_g266 = 4.0;
				float2 uv02_g270 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g270 = 4.0;
				float temp_output_7_0_g270 = 4.0;
				float temp_output_9_0_g270 = 3.0;
				float temp_output_8_0_g270 = 4.0;
				float2 uv02_g258 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g258 = 1.0;
				float temp_output_7_0_g258 = 4.0;
				float temp_output_9_0_g258 = 2.0;
				float temp_output_8_0_g258 = 4.0;
				float2 uv02_g271 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g271 = 2.0;
				float temp_output_7_0_g271 = 4.0;
				float temp_output_9_0_g271 = 2.0;
				float temp_output_8_0_g271 = 4.0;
				float2 uv02_g269 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g269 = 3.0;
				float temp_output_7_0_g269 = 4.0;
				float temp_output_9_0_g269 = 2.0;
				float temp_output_8_0_g269 = 4.0;
				float2 uv02_g261 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g261 = 4.0;
				float temp_output_7_0_g261 = 4.0;
				float temp_output_9_0_g261 = 2.0;
				float temp_output_8_0_g261 = 4.0;
				float2 uv02_g262 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g262 = 1.0;
				float temp_output_7_0_g262 = 4.0;
				float temp_output_9_0_g262 = 1.0;
				float temp_output_8_0_g262 = 4.0;
				float2 uv02_g259 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g259 = 2.0;
				float temp_output_7_0_g259 = 4.0;
				float temp_output_9_0_g259 = 1.0;
				float temp_output_8_0_g259 = 4.0;
				float2 uv02_g264 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g264 = 3.0;
				float temp_output_7_0_g264 = 4.0;
				float temp_output_9_0_g264 = 1.0;
				float temp_output_8_0_g264 = 4.0;
				float2 uv02_g257 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g257 = 4.0;
				float temp_output_7_0_g257 = 4.0;
				float temp_output_9_0_g257 = 1.0;
				float temp_output_8_0_g257 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( uv02_g263.x , ( ( temp_output_3_0_g263 - 1.0 ) / temp_output_7_0_g263 ) ) ) * ( step( uv02_g263.x , ( temp_output_3_0_g263 / temp_output_7_0_g263 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g263.y , ( ( temp_output_9_0_g263 - 1.0 ) / temp_output_8_0_g263 ) ) ) * ( step( uv02_g263.y , ( temp_output_9_0_g263 / temp_output_8_0_g263 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( uv02_g260.x , ( ( temp_output_3_0_g260 - 1.0 ) / temp_output_7_0_g260 ) ) ) * ( step( uv02_g260.x , ( temp_output_3_0_g260 / temp_output_7_0_g260 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g260.y , ( ( temp_output_9_0_g260 - 1.0 ) / temp_output_8_0_g260 ) ) ) * ( step( uv02_g260.y , ( temp_output_9_0_g260 / temp_output_8_0_g260 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( uv02_g251.x , ( ( temp_output_3_0_g251 - 1.0 ) / temp_output_7_0_g251 ) ) ) * ( step( uv02_g251.x , ( temp_output_3_0_g251 / temp_output_7_0_g251 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g251.y , ( ( temp_output_9_0_g251 - 1.0 ) / temp_output_8_0_g251 ) ) ) * ( step( uv02_g251.y , ( temp_output_9_0_g251 / temp_output_8_0_g251 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( uv02_g268.x , ( ( temp_output_3_0_g268 - 1.0 ) / temp_output_7_0_g268 ) ) ) * ( step( uv02_g268.x , ( temp_output_3_0_g268 / temp_output_7_0_g268 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g268.y , ( ( temp_output_9_0_g268 - 1.0 ) / temp_output_8_0_g268 ) ) ) * ( step( uv02_g268.y , ( temp_output_9_0_g268 / temp_output_8_0_g268 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( uv02_g267.x , ( ( temp_output_3_0_g267 - 1.0 ) / temp_output_7_0_g267 ) ) ) * ( step( uv02_g267.x , ( temp_output_3_0_g267 / temp_output_7_0_g267 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g267.y , ( ( temp_output_9_0_g267 - 1.0 ) / temp_output_8_0_g267 ) ) ) * ( step( uv02_g267.y , ( temp_output_9_0_g267 / temp_output_8_0_g267 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( uv02_g265.x , ( ( temp_output_3_0_g265 - 1.0 ) / temp_output_7_0_g265 ) ) ) * ( step( uv02_g265.x , ( temp_output_3_0_g265 / temp_output_7_0_g265 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g265.y , ( ( temp_output_9_0_g265 - 1.0 ) / temp_output_8_0_g265 ) ) ) * ( step( uv02_g265.y , ( temp_output_9_0_g265 / temp_output_8_0_g265 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( uv02_g266.x , ( ( temp_output_3_0_g266 - 1.0 ) / temp_output_7_0_g266 ) ) ) * ( step( uv02_g266.x , ( temp_output_3_0_g266 / temp_output_7_0_g266 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g266.y , ( ( temp_output_9_0_g266 - 1.0 ) / temp_output_8_0_g266 ) ) ) * ( step( uv02_g266.y , ( temp_output_9_0_g266 / temp_output_8_0_g266 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( uv02_g270.x , ( ( temp_output_3_0_g270 - 1.0 ) / temp_output_7_0_g270 ) ) ) * ( step( uv02_g270.x , ( temp_output_3_0_g270 / temp_output_7_0_g270 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g270.y , ( ( temp_output_9_0_g270 - 1.0 ) / temp_output_8_0_g270 ) ) ) * ( step( uv02_g270.y , ( temp_output_9_0_g270 / temp_output_8_0_g270 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( uv02_g258.x , ( ( temp_output_3_0_g258 - 1.0 ) / temp_output_7_0_g258 ) ) ) * ( step( uv02_g258.x , ( temp_output_3_0_g258 / temp_output_7_0_g258 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g258.y , ( ( temp_output_9_0_g258 - 1.0 ) / temp_output_8_0_g258 ) ) ) * ( step( uv02_g258.y , ( temp_output_9_0_g258 / temp_output_8_0_g258 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( uv02_g271.x , ( ( temp_output_3_0_g271 - 1.0 ) / temp_output_7_0_g271 ) ) ) * ( step( uv02_g271.x , ( temp_output_3_0_g271 / temp_output_7_0_g271 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g271.y , ( ( temp_output_9_0_g271 - 1.0 ) / temp_output_8_0_g271 ) ) ) * ( step( uv02_g271.y , ( temp_output_9_0_g271 / temp_output_8_0_g271 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( uv02_g269.x , ( ( temp_output_3_0_g269 - 1.0 ) / temp_output_7_0_g269 ) ) ) * ( step( uv02_g269.x , ( temp_output_3_0_g269 / temp_output_7_0_g269 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g269.y , ( ( temp_output_9_0_g269 - 1.0 ) / temp_output_8_0_g269 ) ) ) * ( step( uv02_g269.y , ( temp_output_9_0_g269 / temp_output_8_0_g269 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( uv02_g261.x , ( ( temp_output_3_0_g261 - 1.0 ) / temp_output_7_0_g261 ) ) ) * ( step( uv02_g261.x , ( temp_output_3_0_g261 / temp_output_7_0_g261 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g261.y , ( ( temp_output_9_0_g261 - 1.0 ) / temp_output_8_0_g261 ) ) ) * ( step( uv02_g261.y , ( temp_output_9_0_g261 / temp_output_8_0_g261 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( uv02_g262.x , ( ( temp_output_3_0_g262 - 1.0 ) / temp_output_7_0_g262 ) ) ) * ( step( uv02_g262.x , ( temp_output_3_0_g262 / temp_output_7_0_g262 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g262.y , ( ( temp_output_9_0_g262 - 1.0 ) / temp_output_8_0_g262 ) ) ) * ( step( uv02_g262.y , ( temp_output_9_0_g262 / temp_output_8_0_g262 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( uv02_g259.x , ( ( temp_output_3_0_g259 - 1.0 ) / temp_output_7_0_g259 ) ) ) * ( step( uv02_g259.x , ( temp_output_3_0_g259 / temp_output_7_0_g259 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g259.y , ( ( temp_output_9_0_g259 - 1.0 ) / temp_output_8_0_g259 ) ) ) * ( step( uv02_g259.y , ( temp_output_9_0_g259 / temp_output_8_0_g259 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( uv02_g264.x , ( ( temp_output_3_0_g264 - 1.0 ) / temp_output_7_0_g264 ) ) ) * ( step( uv02_g264.x , ( temp_output_3_0_g264 / temp_output_7_0_g264 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g264.y , ( ( temp_output_9_0_g264 - 1.0 ) / temp_output_8_0_g264 ) ) ) * ( step( uv02_g264.y , ( temp_output_9_0_g264 / temp_output_8_0_g264 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( uv02_g257.x , ( ( temp_output_3_0_g257 - 1.0 ) / temp_output_7_0_g257 ) ) ) * ( step( uv02_g257.x , ( temp_output_3_0_g257 / temp_output_7_0_g257 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g257.y , ( ( temp_output_9_0_g257 - 1.0 ) / temp_output_8_0_g257 ) ) ) * ( step( uv02_g257.y , ( temp_output_9_0_g257 / temp_output_8_0_g257 ) ) * 1.0 ) ) ) ) ) );
				
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
			float4 _Color13;
			float4 _Color14;
			float4 _Color15;
			float4 _Color16;
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
			float4 _Color13;
			float4 _Color14;
			float4 _Color15;
			float4 _Color16;
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
			float4 _Color13;
			float4 _Color14;
			float4 _Color15;
			float4 _Color16;
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

				float2 uv02_g263 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g263 = 1.0;
				float temp_output_7_0_g263 = 4.0;
				float temp_output_9_0_g263 = 4.0;
				float temp_output_8_0_g263 = 4.0;
				float2 uv02_g260 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g260 = 2.0;
				float temp_output_7_0_g260 = 4.0;
				float temp_output_9_0_g260 = 4.0;
				float temp_output_8_0_g260 = 4.0;
				float2 uv02_g251 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g251 = 3.0;
				float temp_output_7_0_g251 = 4.0;
				float temp_output_9_0_g251 = 4.0;
				float temp_output_8_0_g251 = 4.0;
				float2 uv02_g268 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g268 = 4.0;
				float temp_output_7_0_g268 = 4.0;
				float temp_output_9_0_g268 = 4.0;
				float temp_output_8_0_g268 = 4.0;
				float2 uv02_g267 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g267 = 1.0;
				float temp_output_7_0_g267 = 4.0;
				float temp_output_9_0_g267 = 3.0;
				float temp_output_8_0_g267 = 4.0;
				float2 uv02_g265 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g265 = 2.0;
				float temp_output_7_0_g265 = 4.0;
				float temp_output_9_0_g265 = 3.0;
				float temp_output_8_0_g265 = 4.0;
				float2 uv02_g266 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g266 = 3.0;
				float temp_output_7_0_g266 = 4.0;
				float temp_output_9_0_g266 = 3.0;
				float temp_output_8_0_g266 = 4.0;
				float2 uv02_g270 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g270 = 4.0;
				float temp_output_7_0_g270 = 4.0;
				float temp_output_9_0_g270 = 3.0;
				float temp_output_8_0_g270 = 4.0;
				float2 uv02_g258 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g258 = 1.0;
				float temp_output_7_0_g258 = 4.0;
				float temp_output_9_0_g258 = 2.0;
				float temp_output_8_0_g258 = 4.0;
				float2 uv02_g271 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g271 = 2.0;
				float temp_output_7_0_g271 = 4.0;
				float temp_output_9_0_g271 = 2.0;
				float temp_output_8_0_g271 = 4.0;
				float2 uv02_g269 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g269 = 3.0;
				float temp_output_7_0_g269 = 4.0;
				float temp_output_9_0_g269 = 2.0;
				float temp_output_8_0_g269 = 4.0;
				float2 uv02_g261 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g261 = 4.0;
				float temp_output_7_0_g261 = 4.0;
				float temp_output_9_0_g261 = 2.0;
				float temp_output_8_0_g261 = 4.0;
				float2 uv02_g262 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g262 = 1.0;
				float temp_output_7_0_g262 = 4.0;
				float temp_output_9_0_g262 = 1.0;
				float temp_output_8_0_g262 = 4.0;
				float2 uv02_g259 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g259 = 2.0;
				float temp_output_7_0_g259 = 4.0;
				float temp_output_9_0_g259 = 1.0;
				float temp_output_8_0_g259 = 4.0;
				float2 uv02_g264 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g264 = 3.0;
				float temp_output_7_0_g264 = 4.0;
				float temp_output_9_0_g264 = 1.0;
				float temp_output_8_0_g264 = 4.0;
				float2 uv02_g257 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g257 = 4.0;
				float temp_output_7_0_g257 = 4.0;
				float temp_output_9_0_g257 = 1.0;
				float temp_output_8_0_g257 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( uv02_g263.x , ( ( temp_output_3_0_g263 - 1.0 ) / temp_output_7_0_g263 ) ) ) * ( step( uv02_g263.x , ( temp_output_3_0_g263 / temp_output_7_0_g263 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g263.y , ( ( temp_output_9_0_g263 - 1.0 ) / temp_output_8_0_g263 ) ) ) * ( step( uv02_g263.y , ( temp_output_9_0_g263 / temp_output_8_0_g263 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( uv02_g260.x , ( ( temp_output_3_0_g260 - 1.0 ) / temp_output_7_0_g260 ) ) ) * ( step( uv02_g260.x , ( temp_output_3_0_g260 / temp_output_7_0_g260 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g260.y , ( ( temp_output_9_0_g260 - 1.0 ) / temp_output_8_0_g260 ) ) ) * ( step( uv02_g260.y , ( temp_output_9_0_g260 / temp_output_8_0_g260 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( uv02_g251.x , ( ( temp_output_3_0_g251 - 1.0 ) / temp_output_7_0_g251 ) ) ) * ( step( uv02_g251.x , ( temp_output_3_0_g251 / temp_output_7_0_g251 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g251.y , ( ( temp_output_9_0_g251 - 1.0 ) / temp_output_8_0_g251 ) ) ) * ( step( uv02_g251.y , ( temp_output_9_0_g251 / temp_output_8_0_g251 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( uv02_g268.x , ( ( temp_output_3_0_g268 - 1.0 ) / temp_output_7_0_g268 ) ) ) * ( step( uv02_g268.x , ( temp_output_3_0_g268 / temp_output_7_0_g268 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g268.y , ( ( temp_output_9_0_g268 - 1.0 ) / temp_output_8_0_g268 ) ) ) * ( step( uv02_g268.y , ( temp_output_9_0_g268 / temp_output_8_0_g268 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( uv02_g267.x , ( ( temp_output_3_0_g267 - 1.0 ) / temp_output_7_0_g267 ) ) ) * ( step( uv02_g267.x , ( temp_output_3_0_g267 / temp_output_7_0_g267 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g267.y , ( ( temp_output_9_0_g267 - 1.0 ) / temp_output_8_0_g267 ) ) ) * ( step( uv02_g267.y , ( temp_output_9_0_g267 / temp_output_8_0_g267 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( uv02_g265.x , ( ( temp_output_3_0_g265 - 1.0 ) / temp_output_7_0_g265 ) ) ) * ( step( uv02_g265.x , ( temp_output_3_0_g265 / temp_output_7_0_g265 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g265.y , ( ( temp_output_9_0_g265 - 1.0 ) / temp_output_8_0_g265 ) ) ) * ( step( uv02_g265.y , ( temp_output_9_0_g265 / temp_output_8_0_g265 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( uv02_g266.x , ( ( temp_output_3_0_g266 - 1.0 ) / temp_output_7_0_g266 ) ) ) * ( step( uv02_g266.x , ( temp_output_3_0_g266 / temp_output_7_0_g266 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g266.y , ( ( temp_output_9_0_g266 - 1.0 ) / temp_output_8_0_g266 ) ) ) * ( step( uv02_g266.y , ( temp_output_9_0_g266 / temp_output_8_0_g266 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( uv02_g270.x , ( ( temp_output_3_0_g270 - 1.0 ) / temp_output_7_0_g270 ) ) ) * ( step( uv02_g270.x , ( temp_output_3_0_g270 / temp_output_7_0_g270 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g270.y , ( ( temp_output_9_0_g270 - 1.0 ) / temp_output_8_0_g270 ) ) ) * ( step( uv02_g270.y , ( temp_output_9_0_g270 / temp_output_8_0_g270 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( uv02_g258.x , ( ( temp_output_3_0_g258 - 1.0 ) / temp_output_7_0_g258 ) ) ) * ( step( uv02_g258.x , ( temp_output_3_0_g258 / temp_output_7_0_g258 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g258.y , ( ( temp_output_9_0_g258 - 1.0 ) / temp_output_8_0_g258 ) ) ) * ( step( uv02_g258.y , ( temp_output_9_0_g258 / temp_output_8_0_g258 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( uv02_g271.x , ( ( temp_output_3_0_g271 - 1.0 ) / temp_output_7_0_g271 ) ) ) * ( step( uv02_g271.x , ( temp_output_3_0_g271 / temp_output_7_0_g271 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g271.y , ( ( temp_output_9_0_g271 - 1.0 ) / temp_output_8_0_g271 ) ) ) * ( step( uv02_g271.y , ( temp_output_9_0_g271 / temp_output_8_0_g271 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( uv02_g269.x , ( ( temp_output_3_0_g269 - 1.0 ) / temp_output_7_0_g269 ) ) ) * ( step( uv02_g269.x , ( temp_output_3_0_g269 / temp_output_7_0_g269 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g269.y , ( ( temp_output_9_0_g269 - 1.0 ) / temp_output_8_0_g269 ) ) ) * ( step( uv02_g269.y , ( temp_output_9_0_g269 / temp_output_8_0_g269 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( uv02_g261.x , ( ( temp_output_3_0_g261 - 1.0 ) / temp_output_7_0_g261 ) ) ) * ( step( uv02_g261.x , ( temp_output_3_0_g261 / temp_output_7_0_g261 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g261.y , ( ( temp_output_9_0_g261 - 1.0 ) / temp_output_8_0_g261 ) ) ) * ( step( uv02_g261.y , ( temp_output_9_0_g261 / temp_output_8_0_g261 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( uv02_g262.x , ( ( temp_output_3_0_g262 - 1.0 ) / temp_output_7_0_g262 ) ) ) * ( step( uv02_g262.x , ( temp_output_3_0_g262 / temp_output_7_0_g262 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g262.y , ( ( temp_output_9_0_g262 - 1.0 ) / temp_output_8_0_g262 ) ) ) * ( step( uv02_g262.y , ( temp_output_9_0_g262 / temp_output_8_0_g262 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( uv02_g259.x , ( ( temp_output_3_0_g259 - 1.0 ) / temp_output_7_0_g259 ) ) ) * ( step( uv02_g259.x , ( temp_output_3_0_g259 / temp_output_7_0_g259 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g259.y , ( ( temp_output_9_0_g259 - 1.0 ) / temp_output_8_0_g259 ) ) ) * ( step( uv02_g259.y , ( temp_output_9_0_g259 / temp_output_8_0_g259 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( uv02_g264.x , ( ( temp_output_3_0_g264 - 1.0 ) / temp_output_7_0_g264 ) ) ) * ( step( uv02_g264.x , ( temp_output_3_0_g264 / temp_output_7_0_g264 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g264.y , ( ( temp_output_9_0_g264 - 1.0 ) / temp_output_8_0_g264 ) ) ) * ( step( uv02_g264.y , ( temp_output_9_0_g264 / temp_output_8_0_g264 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( uv02_g257.x , ( ( temp_output_3_0_g257 - 1.0 ) / temp_output_7_0_g257 ) ) ) * ( step( uv02_g257.x , ( temp_output_3_0_g257 / temp_output_7_0_g257 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g257.y , ( ( temp_output_9_0_g257 - 1.0 ) / temp_output_8_0_g257 ) ) ) * ( step( uv02_g257.y , ( temp_output_9_0_g257 / temp_output_8_0_g257 ) ) * 1.0 ) ) ) ) ) );
				
				
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
			float4 _Color13;
			float4 _Color14;
			float4 _Color15;
			float4 _Color16;
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
				float2 uv02_g263 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g263 = 1.0;
				float temp_output_7_0_g263 = 4.0;
				float temp_output_9_0_g263 = 4.0;
				float temp_output_8_0_g263 = 4.0;
				float2 uv02_g260 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g260 = 2.0;
				float temp_output_7_0_g260 = 4.0;
				float temp_output_9_0_g260 = 4.0;
				float temp_output_8_0_g260 = 4.0;
				float2 uv02_g251 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g251 = 3.0;
				float temp_output_7_0_g251 = 4.0;
				float temp_output_9_0_g251 = 4.0;
				float temp_output_8_0_g251 = 4.0;
				float2 uv02_g268 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g268 = 4.0;
				float temp_output_7_0_g268 = 4.0;
				float temp_output_9_0_g268 = 4.0;
				float temp_output_8_0_g268 = 4.0;
				float2 uv02_g267 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g267 = 1.0;
				float temp_output_7_0_g267 = 4.0;
				float temp_output_9_0_g267 = 3.0;
				float temp_output_8_0_g267 = 4.0;
				float2 uv02_g265 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g265 = 2.0;
				float temp_output_7_0_g265 = 4.0;
				float temp_output_9_0_g265 = 3.0;
				float temp_output_8_0_g265 = 4.0;
				float2 uv02_g266 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g266 = 3.0;
				float temp_output_7_0_g266 = 4.0;
				float temp_output_9_0_g266 = 3.0;
				float temp_output_8_0_g266 = 4.0;
				float2 uv02_g270 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g270 = 4.0;
				float temp_output_7_0_g270 = 4.0;
				float temp_output_9_0_g270 = 3.0;
				float temp_output_8_0_g270 = 4.0;
				float2 uv02_g258 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g258 = 1.0;
				float temp_output_7_0_g258 = 4.0;
				float temp_output_9_0_g258 = 2.0;
				float temp_output_8_0_g258 = 4.0;
				float2 uv02_g271 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g271 = 2.0;
				float temp_output_7_0_g271 = 4.0;
				float temp_output_9_0_g271 = 2.0;
				float temp_output_8_0_g271 = 4.0;
				float2 uv02_g269 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g269 = 3.0;
				float temp_output_7_0_g269 = 4.0;
				float temp_output_9_0_g269 = 2.0;
				float temp_output_8_0_g269 = 4.0;
				float2 uv02_g261 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g261 = 4.0;
				float temp_output_7_0_g261 = 4.0;
				float temp_output_9_0_g261 = 2.0;
				float temp_output_8_0_g261 = 4.0;
				float2 uv02_g262 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g262 = 1.0;
				float temp_output_7_0_g262 = 4.0;
				float temp_output_9_0_g262 = 1.0;
				float temp_output_8_0_g262 = 4.0;
				float2 uv02_g259 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g259 = 2.0;
				float temp_output_7_0_g259 = 4.0;
				float temp_output_9_0_g259 = 1.0;
				float temp_output_8_0_g259 = 4.0;
				float2 uv02_g264 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g264 = 3.0;
				float temp_output_7_0_g264 = 4.0;
				float temp_output_9_0_g264 = 1.0;
				float temp_output_8_0_g264 = 4.0;
				float2 uv02_g257 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g257 = 4.0;
				float temp_output_7_0_g257 = 4.0;
				float temp_output_9_0_g257 = 1.0;
				float temp_output_8_0_g257 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( uv02_g263.x , ( ( temp_output_3_0_g263 - 1.0 ) / temp_output_7_0_g263 ) ) ) * ( step( uv02_g263.x , ( temp_output_3_0_g263 / temp_output_7_0_g263 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g263.y , ( ( temp_output_9_0_g263 - 1.0 ) / temp_output_8_0_g263 ) ) ) * ( step( uv02_g263.y , ( temp_output_9_0_g263 / temp_output_8_0_g263 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( uv02_g260.x , ( ( temp_output_3_0_g260 - 1.0 ) / temp_output_7_0_g260 ) ) ) * ( step( uv02_g260.x , ( temp_output_3_0_g260 / temp_output_7_0_g260 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g260.y , ( ( temp_output_9_0_g260 - 1.0 ) / temp_output_8_0_g260 ) ) ) * ( step( uv02_g260.y , ( temp_output_9_0_g260 / temp_output_8_0_g260 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( uv02_g251.x , ( ( temp_output_3_0_g251 - 1.0 ) / temp_output_7_0_g251 ) ) ) * ( step( uv02_g251.x , ( temp_output_3_0_g251 / temp_output_7_0_g251 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g251.y , ( ( temp_output_9_0_g251 - 1.0 ) / temp_output_8_0_g251 ) ) ) * ( step( uv02_g251.y , ( temp_output_9_0_g251 / temp_output_8_0_g251 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( uv02_g268.x , ( ( temp_output_3_0_g268 - 1.0 ) / temp_output_7_0_g268 ) ) ) * ( step( uv02_g268.x , ( temp_output_3_0_g268 / temp_output_7_0_g268 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g268.y , ( ( temp_output_9_0_g268 - 1.0 ) / temp_output_8_0_g268 ) ) ) * ( step( uv02_g268.y , ( temp_output_9_0_g268 / temp_output_8_0_g268 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( uv02_g267.x , ( ( temp_output_3_0_g267 - 1.0 ) / temp_output_7_0_g267 ) ) ) * ( step( uv02_g267.x , ( temp_output_3_0_g267 / temp_output_7_0_g267 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g267.y , ( ( temp_output_9_0_g267 - 1.0 ) / temp_output_8_0_g267 ) ) ) * ( step( uv02_g267.y , ( temp_output_9_0_g267 / temp_output_8_0_g267 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( uv02_g265.x , ( ( temp_output_3_0_g265 - 1.0 ) / temp_output_7_0_g265 ) ) ) * ( step( uv02_g265.x , ( temp_output_3_0_g265 / temp_output_7_0_g265 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g265.y , ( ( temp_output_9_0_g265 - 1.0 ) / temp_output_8_0_g265 ) ) ) * ( step( uv02_g265.y , ( temp_output_9_0_g265 / temp_output_8_0_g265 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( uv02_g266.x , ( ( temp_output_3_0_g266 - 1.0 ) / temp_output_7_0_g266 ) ) ) * ( step( uv02_g266.x , ( temp_output_3_0_g266 / temp_output_7_0_g266 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g266.y , ( ( temp_output_9_0_g266 - 1.0 ) / temp_output_8_0_g266 ) ) ) * ( step( uv02_g266.y , ( temp_output_9_0_g266 / temp_output_8_0_g266 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( uv02_g270.x , ( ( temp_output_3_0_g270 - 1.0 ) / temp_output_7_0_g270 ) ) ) * ( step( uv02_g270.x , ( temp_output_3_0_g270 / temp_output_7_0_g270 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g270.y , ( ( temp_output_9_0_g270 - 1.0 ) / temp_output_8_0_g270 ) ) ) * ( step( uv02_g270.y , ( temp_output_9_0_g270 / temp_output_8_0_g270 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( uv02_g258.x , ( ( temp_output_3_0_g258 - 1.0 ) / temp_output_7_0_g258 ) ) ) * ( step( uv02_g258.x , ( temp_output_3_0_g258 / temp_output_7_0_g258 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g258.y , ( ( temp_output_9_0_g258 - 1.0 ) / temp_output_8_0_g258 ) ) ) * ( step( uv02_g258.y , ( temp_output_9_0_g258 / temp_output_8_0_g258 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( uv02_g271.x , ( ( temp_output_3_0_g271 - 1.0 ) / temp_output_7_0_g271 ) ) ) * ( step( uv02_g271.x , ( temp_output_3_0_g271 / temp_output_7_0_g271 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g271.y , ( ( temp_output_9_0_g271 - 1.0 ) / temp_output_8_0_g271 ) ) ) * ( step( uv02_g271.y , ( temp_output_9_0_g271 / temp_output_8_0_g271 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( uv02_g269.x , ( ( temp_output_3_0_g269 - 1.0 ) / temp_output_7_0_g269 ) ) ) * ( step( uv02_g269.x , ( temp_output_3_0_g269 / temp_output_7_0_g269 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g269.y , ( ( temp_output_9_0_g269 - 1.0 ) / temp_output_8_0_g269 ) ) ) * ( step( uv02_g269.y , ( temp_output_9_0_g269 / temp_output_8_0_g269 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( uv02_g261.x , ( ( temp_output_3_0_g261 - 1.0 ) / temp_output_7_0_g261 ) ) ) * ( step( uv02_g261.x , ( temp_output_3_0_g261 / temp_output_7_0_g261 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g261.y , ( ( temp_output_9_0_g261 - 1.0 ) / temp_output_8_0_g261 ) ) ) * ( step( uv02_g261.y , ( temp_output_9_0_g261 / temp_output_8_0_g261 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( uv02_g262.x , ( ( temp_output_3_0_g262 - 1.0 ) / temp_output_7_0_g262 ) ) ) * ( step( uv02_g262.x , ( temp_output_3_0_g262 / temp_output_7_0_g262 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g262.y , ( ( temp_output_9_0_g262 - 1.0 ) / temp_output_8_0_g262 ) ) ) * ( step( uv02_g262.y , ( temp_output_9_0_g262 / temp_output_8_0_g262 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( uv02_g259.x , ( ( temp_output_3_0_g259 - 1.0 ) / temp_output_7_0_g259 ) ) ) * ( step( uv02_g259.x , ( temp_output_3_0_g259 / temp_output_7_0_g259 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g259.y , ( ( temp_output_9_0_g259 - 1.0 ) / temp_output_8_0_g259 ) ) ) * ( step( uv02_g259.y , ( temp_output_9_0_g259 / temp_output_8_0_g259 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( uv02_g264.x , ( ( temp_output_3_0_g264 - 1.0 ) / temp_output_7_0_g264 ) ) ) * ( step( uv02_g264.x , ( temp_output_3_0_g264 / temp_output_7_0_g264 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g264.y , ( ( temp_output_9_0_g264 - 1.0 ) / temp_output_8_0_g264 ) ) ) * ( step( uv02_g264.y , ( temp_output_9_0_g264 / temp_output_8_0_g264 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( uv02_g257.x , ( ( temp_output_3_0_g257 - 1.0 ) / temp_output_7_0_g257 ) ) ) * ( step( uv02_g257.x , ( temp_output_3_0_g257 / temp_output_7_0_g257 ) ) * 1.0 ) ) * ( ( 1.0 - step( uv02_g257.y , ( ( temp_output_9_0_g257 - 1.0 ) / temp_output_8_0_g257 ) ) ) * ( step( uv02_g257.y , ( temp_output_9_0_g257 / temp_output_8_0_g257 ) ) * 1.0 ) ) ) ) ) );
				
				
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
53;193;1636;776;-1174.463;-988.955;1.350566;True;True
Node;AmplifyShaderEditor.ColorNode;181;-218.8154,2174.284;Float;False;Property;_Color11;Color 11;10;0;Create;True;0;0;False;0;0.6691177,0.6691177,0.6691177,0.647;0.1868512,0.3920654,0.7941176,0.378;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;214;-242.6307,2942.365;Float;False;Property;_Color14;Color 14;13;0;Create;True;0;0;False;0;0,0.8025862,0.875,0.047;1,0.8118661,0.6102941,0.047;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;182;-220.2247,2417.44;Float;False;Property;_Color12;Color 12;11;0;Create;True;0;0;False;0;0.5073529,0.1574544,0,0.128;0.03092561,0.3096439,0.3823529,0.128;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;183;-224.4024,1681.061;Float;False;Property;_Color9;Color 9;8;0;Create;True;0;0;False;0;0.3164301,0,0.7058823,0.134;0.5735294,0.9117646,1,0.822;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;180;-232.3431,1940.419;Float;False;Property;_Color10;Color 10;9;0;Create;True;0;0;False;0;0.362069,0.4411765,0,0.759;0.1552228,0.1600571,0.2132353,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;217;-264.3738,3419.386;Float;False;Property;_Color16;Color 16;15;0;Create;True;0;0;False;0;0.4080882,0.75,0.4811866,0.134;0.1089965,0.2584653,0.4632353,0.709;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;213;-234.6901,2683.007;Float;False;Property;_Color13;Color 13;12;0;Create;True;0;0;False;0;1,0.5586207,0,0.272;0.2132353,0.2053958,0.2053958,0.272;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;150;-207.7412,-66.93771;Float;False;Property;_Color2;Color 2;1;0;Create;True;0;0;False;0;1,0.1544118,0.8017241,0.253;0.4246324,0.5544625,0.5661765,0.378;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;218;-229.103,3176.23;Float;False;Property;_Color15;Color 15;14;0;Create;True;0;0;False;0;1,0,0,0.391;0.3441285,0.4926471,0.4311911,0.397;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;154;-195.6228,411.2479;Float;False;Property;_Color4;Color 4;3;0;Create;True;0;0;False;0;0.1544118,0.5451319,1,0.253;0.5004326,0.5788032,0.7647059,0.872;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-199.8005,-326.2955;Float;False;Property;_Color1;Color 1;0;0;Create;True;0;0;False;0;1,0.1544118,0.1544118,0.291;0.08937068,0.3892086,0.6397059,0.653;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;157;-182.3802,1181.25;Float;False;Property;_Color7;Color 7;6;0;Create;True;0;0;False;0;0.1544118,0.6151115,1,0.178;0.616782,0.6616514,0.6764706,0.378;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;158;-183.7895,1424.406;Float;False;Property;_Color8;Color 8;7;0;Create;True;0;0;False;0;0.4849697,0.5008695,0.5073529,0.078;0.08937068,0.3892086,0.6397059,0.653;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;159;-187.9672,688.0273;Float;False;Property;_Color5;Color 5;4;0;Create;True;0;0;False;0;0.9533468,1,0.1544118,0.553;0.4184148,0.716347,0.8014706,0.309;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;156;-195.9079,947.3851;Float;False;Property;_Color6;Color 6;5;0;Create;True;0;0;False;0;0.2720588,0.1294625,0,0.097;0.1201882,0.5540434,0.8602941,0.278;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;152;-194.2135,166.9271;Float;False;Property;_Color3;Color 3;2;0;Create;True;0;0;False;0;0.2535501,0.1544118,1,0.541;0.1868512,0.3920654,0.7941176,0.378;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;160;119.8096,952.2258;Inherit;True;ColorShartSlot;-1;;265;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;187;83.37437,1945.26;Inherit;True;ColorShartSlot;-1;;271;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;162;133.8517,1429.247;Inherit;True;ColorShartSlot;-1;;270;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;186;96.90227,2179.125;Inherit;True;ColorShartSlot;-1;;269;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;153;122.0185,414.924;Inherit;True;ColorShartSlot;-1;;268;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;163;127.7504,692.868;Inherit;True;ColorShartSlot;-1;;267;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;161;133.3375,1186.091;Inherit;True;ColorShartSlot;-1;;266;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;224;86.61465,3181.071;Inherit;True;ColorShartSlot;-1;;264;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;216;81.02762,2687.848;Inherit;True;ColorShartSlot;-1;;262;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;185;97.41646,2422.281;Inherit;True;ColorShartSlot;-1;;261;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;149;107.9764,-62.09709;Inherit;True;ColorShartSlot;-1;;260;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;223;73.08682,2945.046;Inherit;True;ColorShartSlot;-1;;259;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;188;91.31517,1685.902;Inherit;True;ColorShartSlot;-1;;258;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;222;87.12894,3424.227;Inherit;True;ColorShartSlot;-1;;257;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;151;121.5042,171.7677;Inherit;True;ColorShartSlot;-1;;251;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;145;115.9171,-321.4549;Inherit;True;ColorShartSlot;-1;;263;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;146;1539.255,777.6315;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;184;1537.758,1310.802;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;225;1534.365,1575.009;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;1539.944,1043.66;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;1964.993,1140.165;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;166;1887.168,1900.592;Float;False;Property;_Smoothness;Smoothness;16;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;227;1935.602,1617.235;Inherit;True;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;165;2014.597,1413.642;Float;False;Property;_Metallic;Metallic;17;0;Create;True;0;0;False;0;0;0.387;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;2229.031,1787.579;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;235;2469.067,1277.475;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;236;2469.067,1277.475;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;3;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;233;2469.067,1277.475;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Malbers/Color4x4;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;0;Forward;12;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;12;Workflow;1;Surface;0;  Blend;0;Two Sided;1;Cast Shadows;1;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;Meta Pass;1;Override Baked GI;0;Vertex Position,InvertActionOnDeselection;1;0;5;True;True;True;True;True;False;;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;234;2469.067,1277.475;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;237;2469.067,1277.475;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;4;Universal2D;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
WireConnection;160;38;156;0
WireConnection;187;38;180;0
WireConnection;162;38;158;0
WireConnection;186;38;181;0
WireConnection;153;38;154;0
WireConnection;163;38;159;0
WireConnection;161;38;157;0
WireConnection;224;38;218;0
WireConnection;216;38;213;0
WireConnection;185;38;182;0
WireConnection;149;38;150;0
WireConnection;223;38;214;0
WireConnection;188;38;183;0
WireConnection;222;38;217;0
WireConnection;151;38;152;0
WireConnection;145;38;23;0
WireConnection;146;0;145;0
WireConnection;146;1;149;0
WireConnection;146;2;151;0
WireConnection;146;3;153;0
WireConnection;184;0;188;0
WireConnection;184;1;187;0
WireConnection;184;2;186;0
WireConnection;184;3;185;0
WireConnection;225;0;216;0
WireConnection;225;1;223;0
WireConnection;225;2;224;0
WireConnection;225;3;222;0
WireConnection;164;0;163;0
WireConnection;164;1;160;0
WireConnection;164;2;161;0
WireConnection;164;3;162;0
WireConnection;155;0;146;0
WireConnection;155;1;164;0
WireConnection;155;2;184;0
WireConnection;155;3;225;0
WireConnection;227;0;155;0
WireConnection;212;0;227;0
WireConnection;212;1;166;0
WireConnection;233;0;155;0
WireConnection;233;3;165;0
WireConnection;233;4;212;0
ASEEND*/
//CHKSM=0582F81434ADE72C76BDB9208EE91A1E2002F8D0