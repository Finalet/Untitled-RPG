// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Color4x3"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Header(Row 1)]_Color1("Color 1", Color) = (1,0.1544118,0.1544118,0.397)
		_Color2("Color 2", Color) = (1,0.1544118,0.8017241,0.334)
		_Color3("Color 3", Color) = (0.2535501,0.1544118,1,0.228)
		_Color4("Color 4", Color) = (0.1544118,0.5451319,1,0.472)
		[Header(Row 2)]_Color5("Color 5", Color) = (0.9533468,1,0.1544118,0.353)
		_Color6("Color 6", Color) = (0.2669384,0.3207547,0.0226949,0.341)
		_Color7("Color 7", Color) = (0.1544118,0.6151115,1,0.316)
		_Color8("Color 8", Color) = (0.4849697,0.5008695,0.5073529,0.484)
		[Header(Row 3)]_Color9("Color 9", Color) = (0.9099331,0.9264706,0.6267301,0.353)
		_Color10("Color 10", Color) = (0.1544118,0.1602434,1,0.341)
		_Color11("Color 11", Color) = (1,0.1544118,0.381846,0.316)
		_Color12("Color 12", Color) = (0.02270761,0.1632713,0.2205882,0.484)
		[Header(Smoothness (Alphas))]_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_Metallic("Metallic", Range( 0 , 1)) = 0
		[HDR][Header(Emmision)]_Color11Emmision("Color 11 Emmision", Color) = (0,0,0,1)
		[HDR]_Color12Emmision("Color 12 Emmision", Color) = (0,0,0,1)
		[Header(Gradient)]_Gradient("Gradient", 2D) = "white" {}
		_GradientIntensity("Gradient Intensity", Range( 0 , 1)) = 0.75
		_GradientColor("Gradient Color", Color) = (0,0,0,0)
		_GradientScale("Gradient Scale", Float) = 1
		_GradientOffset("Gradient Offset", Float) = 0
		[ASEEnd]_GradientPower("Gradient Power", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		Cull Back
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 2.0

		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70301

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

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				
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
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color1;
			float4 _Color12Emmision;
			float4 _Color11Emmision;
			float4 _GradientColor;
			float4 _Gradient_ST;
			float4 _Color12;
			float4 _Color10;
			float4 _Color9;
			float4 _Color11;
			float4 _Color7;
			float4 _Color6;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color8;
			float _Metallic;
			float _GradientIntensity;
			float _GradientScale;
			float _GradientOffset;
			float _GradientPower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _Gradient;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord7.xy = v.texcoord.xy;
				
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

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

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
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord1 = v.texcoord1;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 texCoord2_g273 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g273 = 1.0;
				float temp_output_7_0_g273 = 4.0;
				float temp_output_9_0_g273 = 3.0;
				float temp_output_8_0_g273 = 3.0;
				float2 texCoord2_g271 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g271 = 2.0;
				float temp_output_7_0_g271 = 4.0;
				float temp_output_9_0_g271 = 3.0;
				float temp_output_8_0_g271 = 3.0;
				float2 texCoord2_g281 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g281 = 3.0;
				float temp_output_7_0_g281 = 4.0;
				float temp_output_9_0_g281 = 3.0;
				float temp_output_8_0_g281 = 3.0;
				float2 texCoord2_g279 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g279 = 4.0;
				float temp_output_7_0_g279 = 4.0;
				float temp_output_9_0_g279 = 3.0;
				float temp_output_8_0_g279 = 3.0;
				float2 texCoord2_g272 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g272 = 1.0;
				float temp_output_7_0_g272 = 4.0;
				float temp_output_9_0_g272 = 2.0;
				float temp_output_8_0_g272 = 3.0;
				float2 texCoord2_g278 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g278 = 2.0;
				float temp_output_7_0_g278 = 4.0;
				float temp_output_9_0_g278 = 2.0;
				float temp_output_8_0_g278 = 3.0;
				float2 texCoord2_g280 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g280 = 3.0;
				float temp_output_7_0_g280 = 4.0;
				float temp_output_9_0_g280 = 2.0;
				float temp_output_8_0_g280 = 3.0;
				float2 texCoord2_g274 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g274 = 4.0;
				float temp_output_7_0_g274 = 4.0;
				float temp_output_9_0_g274 = 2.0;
				float temp_output_8_0_g274 = 3.0;
				float2 texCoord2_g270 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g270 = 1.0;
				float temp_output_7_0_g270 = 4.0;
				float temp_output_9_0_g270 = 1.0;
				float temp_output_8_0_g270 = 3.0;
				float2 texCoord2_g282 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g282 = 2.0;
				float temp_output_7_0_g282 = 4.0;
				float temp_output_9_0_g282 = 1.0;
				float temp_output_8_0_g282 = 3.0;
				float2 texCoord2_g240 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g240 = 3.0;
				float temp_output_7_0_g240 = 4.0;
				float temp_output_9_0_g240 = 1.0;
				float temp_output_8_0_g240 = 3.0;
				float4 temp_output_241_0 = ( float4( 1,1,1,1 ) * ( ( ( 1.0 - step( texCoord2_g240.x , ( ( temp_output_3_0_g240 - 1.0 ) / temp_output_7_0_g240 ) ) ) * ( step( texCoord2_g240.x , ( temp_output_3_0_g240 / temp_output_7_0_g240 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g240.y , ( ( temp_output_9_0_g240 - 1.0 ) / temp_output_8_0_g240 ) ) ) * ( step( texCoord2_g240.y , ( temp_output_9_0_g240 / temp_output_8_0_g240 ) ) * 1.0 ) ) ) );
				float2 texCoord2_g241 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g241 = 4.0;
				float temp_output_7_0_g241 = 4.0;
				float temp_output_9_0_g241 = 1.0;
				float temp_output_8_0_g241 = 3.0;
				float4 temp_output_230_0 = ( float4( 1,1,1,1 ) * ( ( ( 1.0 - step( texCoord2_g241.x , ( ( temp_output_3_0_g241 - 1.0 ) / temp_output_7_0_g241 ) ) ) * ( step( texCoord2_g241.x , ( temp_output_3_0_g241 / temp_output_7_0_g241 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g241.y , ( ( temp_output_9_0_g241 - 1.0 ) / temp_output_8_0_g241 ) ) ) * ( step( texCoord2_g241.y , ( temp_output_9_0_g241 / temp_output_8_0_g241 ) ) * 1.0 ) ) ) );
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g273.x , ( ( temp_output_3_0_g273 - 1.0 ) / temp_output_7_0_g273 ) ) ) * ( step( texCoord2_g273.x , ( temp_output_3_0_g273 / temp_output_7_0_g273 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g273.y , ( ( temp_output_9_0_g273 - 1.0 ) / temp_output_8_0_g273 ) ) ) * ( step( texCoord2_g273.y , ( temp_output_9_0_g273 / temp_output_8_0_g273 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g271.x , ( ( temp_output_3_0_g271 - 1.0 ) / temp_output_7_0_g271 ) ) ) * ( step( texCoord2_g271.x , ( temp_output_3_0_g271 / temp_output_7_0_g271 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g271.y , ( ( temp_output_9_0_g271 - 1.0 ) / temp_output_8_0_g271 ) ) ) * ( step( texCoord2_g271.y , ( temp_output_9_0_g271 / temp_output_8_0_g271 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g281.x , ( ( temp_output_3_0_g281 - 1.0 ) / temp_output_7_0_g281 ) ) ) * ( step( texCoord2_g281.x , ( temp_output_3_0_g281 / temp_output_7_0_g281 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g281.y , ( ( temp_output_9_0_g281 - 1.0 ) / temp_output_8_0_g281 ) ) ) * ( step( texCoord2_g281.y , ( temp_output_9_0_g281 / temp_output_8_0_g281 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g279.x , ( ( temp_output_3_0_g279 - 1.0 ) / temp_output_7_0_g279 ) ) ) * ( step( texCoord2_g279.x , ( temp_output_3_0_g279 / temp_output_7_0_g279 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g279.y , ( ( temp_output_9_0_g279 - 1.0 ) / temp_output_8_0_g279 ) ) ) * ( step( texCoord2_g279.y , ( temp_output_9_0_g279 / temp_output_8_0_g279 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g272.x , ( ( temp_output_3_0_g272 - 1.0 ) / temp_output_7_0_g272 ) ) ) * ( step( texCoord2_g272.x , ( temp_output_3_0_g272 / temp_output_7_0_g272 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g272.y , ( ( temp_output_9_0_g272 - 1.0 ) / temp_output_8_0_g272 ) ) ) * ( step( texCoord2_g272.y , ( temp_output_9_0_g272 / temp_output_8_0_g272 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g278.x , ( ( temp_output_3_0_g278 - 1.0 ) / temp_output_7_0_g278 ) ) ) * ( step( texCoord2_g278.x , ( temp_output_3_0_g278 / temp_output_7_0_g278 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g278.y , ( ( temp_output_9_0_g278 - 1.0 ) / temp_output_8_0_g278 ) ) ) * ( step( texCoord2_g278.y , ( temp_output_9_0_g278 / temp_output_8_0_g278 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g280.x , ( ( temp_output_3_0_g280 - 1.0 ) / temp_output_7_0_g280 ) ) ) * ( step( texCoord2_g280.x , ( temp_output_3_0_g280 / temp_output_7_0_g280 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g280.y , ( ( temp_output_9_0_g280 - 1.0 ) / temp_output_8_0_g280 ) ) ) * ( step( texCoord2_g280.y , ( temp_output_9_0_g280 / temp_output_8_0_g280 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g274.x , ( ( temp_output_3_0_g274 - 1.0 ) / temp_output_7_0_g274 ) ) ) * ( step( texCoord2_g274.x , ( temp_output_3_0_g274 / temp_output_7_0_g274 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g274.y , ( ( temp_output_9_0_g274 - 1.0 ) / temp_output_8_0_g274 ) ) ) * ( step( texCoord2_g274.y , ( temp_output_9_0_g274 / temp_output_8_0_g274 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g270.x , ( ( temp_output_3_0_g270 - 1.0 ) / temp_output_7_0_g270 ) ) ) * ( step( texCoord2_g270.x , ( temp_output_3_0_g270 / temp_output_7_0_g270 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g270.y , ( ( temp_output_9_0_g270 - 1.0 ) / temp_output_8_0_g270 ) ) ) * ( step( texCoord2_g270.y , ( temp_output_9_0_g270 / temp_output_8_0_g270 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g282.x , ( ( temp_output_3_0_g282 - 1.0 ) / temp_output_7_0_g282 ) ) ) * ( step( texCoord2_g282.x , ( temp_output_3_0_g282 / temp_output_7_0_g282 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g282.y , ( ( temp_output_9_0_g282 - 1.0 ) / temp_output_8_0_g282 ) ) ) * ( step( texCoord2_g282.y , ( temp_output_9_0_g282 / temp_output_8_0_g282 ) ) * 1.0 ) ) ) ) + ( temp_output_241_0 * _Color11 ) + ( temp_output_230_0 * _Color12 ) ) );
				float2 uv_Gradient = IN.ase_texcoord7.xy * _Gradient_ST.xy + _Gradient_ST.zw;
				float4 clampResult206 = clamp( ( ( tex2D( _Gradient, uv_Gradient ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float4 clampResult255 = clamp( pow( (clampResult206*_GradientScale + _GradientOffset) , temp_cast_0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float3 Albedo = ( temp_output_155_0 * clampResult255 ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( ( _Color11Emmision * temp_output_241_0 ) + ( _Color12Emmision * temp_output_230_0 ) ).rgb;
				float3 Specular = 0.5;
				float Metallic = _Metallic;
				float Smoothness = ( (temp_output_155_0).a * _Smoothness );
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
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

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
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
			AlphaToMask Off

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70301

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

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

			CBUFFER_START(UnityPerMaterial)
			float4 _Color1;
			float4 _Color12Emmision;
			float4 _Color11Emmision;
			float4 _GradientColor;
			float4 _Gradient_ST;
			float4 _Color12;
			float4 _Color10;
			float4 _Color9;
			float4 _Color11;
			float4 _Color7;
			float4 _Color6;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color8;
			float _Metallic;
			float _GradientIntensity;
			float _GradientScale;
			float _GradientOffset;
			float _GradientPower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			float3 _LightDirection;

			VertexOutput VertexFunction( VertexInput v )
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

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

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

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
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
			AlphaToMask Off

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70301

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

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

			CBUFFER_START(UnityPerMaterial)
			float4 _Color1;
			float4 _Color12Emmision;
			float4 _Color11Emmision;
			float4 _GradientColor;
			float4 _Gradient_ST;
			float4 _Color12;
			float4 _Color10;
			float4 _Color9;
			float4 _Color11;
			float4 _Color7;
			float4 _Color6;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color8;
			float _Metallic;
			float _GradientIntensity;
			float _GradientScale;
			float _GradientOffset;
			float _GradientPower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			VertexOutput VertexFunction( VertexInput v  )
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

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

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
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70301

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

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

			CBUFFER_START(UnityPerMaterial)
			float4 _Color1;
			float4 _Color12Emmision;
			float4 _Color11Emmision;
			float4 _GradientColor;
			float4 _Gradient_ST;
			float4 _Color12;
			float4 _Color10;
			float4 _Color9;
			float4 _Color11;
			float4 _Color7;
			float4 _Color6;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color8;
			float _Metallic;
			float _GradientIntensity;
			float _GradientScale;
			float _GradientOffset;
			float _GradientPower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _Gradient;


			
			VertexOutput VertexFunction( VertexInput v  )
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

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

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

				float2 texCoord2_g273 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g273 = 1.0;
				float temp_output_7_0_g273 = 4.0;
				float temp_output_9_0_g273 = 3.0;
				float temp_output_8_0_g273 = 3.0;
				float2 texCoord2_g271 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g271 = 2.0;
				float temp_output_7_0_g271 = 4.0;
				float temp_output_9_0_g271 = 3.0;
				float temp_output_8_0_g271 = 3.0;
				float2 texCoord2_g281 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g281 = 3.0;
				float temp_output_7_0_g281 = 4.0;
				float temp_output_9_0_g281 = 3.0;
				float temp_output_8_0_g281 = 3.0;
				float2 texCoord2_g279 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g279 = 4.0;
				float temp_output_7_0_g279 = 4.0;
				float temp_output_9_0_g279 = 3.0;
				float temp_output_8_0_g279 = 3.0;
				float2 texCoord2_g272 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g272 = 1.0;
				float temp_output_7_0_g272 = 4.0;
				float temp_output_9_0_g272 = 2.0;
				float temp_output_8_0_g272 = 3.0;
				float2 texCoord2_g278 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g278 = 2.0;
				float temp_output_7_0_g278 = 4.0;
				float temp_output_9_0_g278 = 2.0;
				float temp_output_8_0_g278 = 3.0;
				float2 texCoord2_g280 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g280 = 3.0;
				float temp_output_7_0_g280 = 4.0;
				float temp_output_9_0_g280 = 2.0;
				float temp_output_8_0_g280 = 3.0;
				float2 texCoord2_g274 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g274 = 4.0;
				float temp_output_7_0_g274 = 4.0;
				float temp_output_9_0_g274 = 2.0;
				float temp_output_8_0_g274 = 3.0;
				float2 texCoord2_g270 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g270 = 1.0;
				float temp_output_7_0_g270 = 4.0;
				float temp_output_9_0_g270 = 1.0;
				float temp_output_8_0_g270 = 3.0;
				float2 texCoord2_g282 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g282 = 2.0;
				float temp_output_7_0_g282 = 4.0;
				float temp_output_9_0_g282 = 1.0;
				float temp_output_8_0_g282 = 3.0;
				float2 texCoord2_g240 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g240 = 3.0;
				float temp_output_7_0_g240 = 4.0;
				float temp_output_9_0_g240 = 1.0;
				float temp_output_8_0_g240 = 3.0;
				float4 temp_output_241_0 = ( float4( 1,1,1,1 ) * ( ( ( 1.0 - step( texCoord2_g240.x , ( ( temp_output_3_0_g240 - 1.0 ) / temp_output_7_0_g240 ) ) ) * ( step( texCoord2_g240.x , ( temp_output_3_0_g240 / temp_output_7_0_g240 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g240.y , ( ( temp_output_9_0_g240 - 1.0 ) / temp_output_8_0_g240 ) ) ) * ( step( texCoord2_g240.y , ( temp_output_9_0_g240 / temp_output_8_0_g240 ) ) * 1.0 ) ) ) );
				float2 texCoord2_g241 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g241 = 4.0;
				float temp_output_7_0_g241 = 4.0;
				float temp_output_9_0_g241 = 1.0;
				float temp_output_8_0_g241 = 3.0;
				float4 temp_output_230_0 = ( float4( 1,1,1,1 ) * ( ( ( 1.0 - step( texCoord2_g241.x , ( ( temp_output_3_0_g241 - 1.0 ) / temp_output_7_0_g241 ) ) ) * ( step( texCoord2_g241.x , ( temp_output_3_0_g241 / temp_output_7_0_g241 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g241.y , ( ( temp_output_9_0_g241 - 1.0 ) / temp_output_8_0_g241 ) ) ) * ( step( texCoord2_g241.y , ( temp_output_9_0_g241 / temp_output_8_0_g241 ) ) * 1.0 ) ) ) );
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g273.x , ( ( temp_output_3_0_g273 - 1.0 ) / temp_output_7_0_g273 ) ) ) * ( step( texCoord2_g273.x , ( temp_output_3_0_g273 / temp_output_7_0_g273 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g273.y , ( ( temp_output_9_0_g273 - 1.0 ) / temp_output_8_0_g273 ) ) ) * ( step( texCoord2_g273.y , ( temp_output_9_0_g273 / temp_output_8_0_g273 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g271.x , ( ( temp_output_3_0_g271 - 1.0 ) / temp_output_7_0_g271 ) ) ) * ( step( texCoord2_g271.x , ( temp_output_3_0_g271 / temp_output_7_0_g271 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g271.y , ( ( temp_output_9_0_g271 - 1.0 ) / temp_output_8_0_g271 ) ) ) * ( step( texCoord2_g271.y , ( temp_output_9_0_g271 / temp_output_8_0_g271 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g281.x , ( ( temp_output_3_0_g281 - 1.0 ) / temp_output_7_0_g281 ) ) ) * ( step( texCoord2_g281.x , ( temp_output_3_0_g281 / temp_output_7_0_g281 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g281.y , ( ( temp_output_9_0_g281 - 1.0 ) / temp_output_8_0_g281 ) ) ) * ( step( texCoord2_g281.y , ( temp_output_9_0_g281 / temp_output_8_0_g281 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g279.x , ( ( temp_output_3_0_g279 - 1.0 ) / temp_output_7_0_g279 ) ) ) * ( step( texCoord2_g279.x , ( temp_output_3_0_g279 / temp_output_7_0_g279 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g279.y , ( ( temp_output_9_0_g279 - 1.0 ) / temp_output_8_0_g279 ) ) ) * ( step( texCoord2_g279.y , ( temp_output_9_0_g279 / temp_output_8_0_g279 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g272.x , ( ( temp_output_3_0_g272 - 1.0 ) / temp_output_7_0_g272 ) ) ) * ( step( texCoord2_g272.x , ( temp_output_3_0_g272 / temp_output_7_0_g272 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g272.y , ( ( temp_output_9_0_g272 - 1.0 ) / temp_output_8_0_g272 ) ) ) * ( step( texCoord2_g272.y , ( temp_output_9_0_g272 / temp_output_8_0_g272 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g278.x , ( ( temp_output_3_0_g278 - 1.0 ) / temp_output_7_0_g278 ) ) ) * ( step( texCoord2_g278.x , ( temp_output_3_0_g278 / temp_output_7_0_g278 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g278.y , ( ( temp_output_9_0_g278 - 1.0 ) / temp_output_8_0_g278 ) ) ) * ( step( texCoord2_g278.y , ( temp_output_9_0_g278 / temp_output_8_0_g278 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g280.x , ( ( temp_output_3_0_g280 - 1.0 ) / temp_output_7_0_g280 ) ) ) * ( step( texCoord2_g280.x , ( temp_output_3_0_g280 / temp_output_7_0_g280 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g280.y , ( ( temp_output_9_0_g280 - 1.0 ) / temp_output_8_0_g280 ) ) ) * ( step( texCoord2_g280.y , ( temp_output_9_0_g280 / temp_output_8_0_g280 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g274.x , ( ( temp_output_3_0_g274 - 1.0 ) / temp_output_7_0_g274 ) ) ) * ( step( texCoord2_g274.x , ( temp_output_3_0_g274 / temp_output_7_0_g274 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g274.y , ( ( temp_output_9_0_g274 - 1.0 ) / temp_output_8_0_g274 ) ) ) * ( step( texCoord2_g274.y , ( temp_output_9_0_g274 / temp_output_8_0_g274 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g270.x , ( ( temp_output_3_0_g270 - 1.0 ) / temp_output_7_0_g270 ) ) ) * ( step( texCoord2_g270.x , ( temp_output_3_0_g270 / temp_output_7_0_g270 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g270.y , ( ( temp_output_9_0_g270 - 1.0 ) / temp_output_8_0_g270 ) ) ) * ( step( texCoord2_g270.y , ( temp_output_9_0_g270 / temp_output_8_0_g270 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g282.x , ( ( temp_output_3_0_g282 - 1.0 ) / temp_output_7_0_g282 ) ) ) * ( step( texCoord2_g282.x , ( temp_output_3_0_g282 / temp_output_7_0_g282 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g282.y , ( ( temp_output_9_0_g282 - 1.0 ) / temp_output_8_0_g282 ) ) ) * ( step( texCoord2_g282.y , ( temp_output_9_0_g282 / temp_output_8_0_g282 ) ) * 1.0 ) ) ) ) + ( temp_output_241_0 * _Color11 ) + ( temp_output_230_0 * _Color12 ) ) );
				float2 uv_Gradient = IN.ase_texcoord2.xy * _Gradient_ST.xy + _Gradient_ST.zw;
				float4 clampResult206 = clamp( ( ( tex2D( _Gradient, uv_Gradient ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float4 clampResult255 = clamp( pow( (clampResult206*_GradientScale + _GradientOffset) , temp_cast_0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				
				float3 Albedo = ( temp_output_155_0 * clampResult255 ).rgb;
				float3 Emission = ( ( _Color11Emmision * temp_output_241_0 ) + ( _Color12Emmision * temp_output_230_0 ) ).rgb;
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

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 70301

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

			CBUFFER_START(UnityPerMaterial)
			float4 _Color1;
			float4 _Color12Emmision;
			float4 _Color11Emmision;
			float4 _GradientColor;
			float4 _Gradient_ST;
			float4 _Color12;
			float4 _Color10;
			float4 _Color9;
			float4 _Color11;
			float4 _Color7;
			float4 _Color6;
			float4 _Color5;
			float4 _Color4;
			float4 _Color3;
			float4 _Color2;
			float4 _Color8;
			float _Metallic;
			float _GradientIntensity;
			float _GradientScale;
			float _GradientOffset;
			float _GradientPower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _Gradient;


			
			VertexOutput VertexFunction( VertexInput v  )
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

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

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

				float2 texCoord2_g273 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g273 = 1.0;
				float temp_output_7_0_g273 = 4.0;
				float temp_output_9_0_g273 = 3.0;
				float temp_output_8_0_g273 = 3.0;
				float2 texCoord2_g271 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g271 = 2.0;
				float temp_output_7_0_g271 = 4.0;
				float temp_output_9_0_g271 = 3.0;
				float temp_output_8_0_g271 = 3.0;
				float2 texCoord2_g281 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g281 = 3.0;
				float temp_output_7_0_g281 = 4.0;
				float temp_output_9_0_g281 = 3.0;
				float temp_output_8_0_g281 = 3.0;
				float2 texCoord2_g279 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g279 = 4.0;
				float temp_output_7_0_g279 = 4.0;
				float temp_output_9_0_g279 = 3.0;
				float temp_output_8_0_g279 = 3.0;
				float2 texCoord2_g272 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g272 = 1.0;
				float temp_output_7_0_g272 = 4.0;
				float temp_output_9_0_g272 = 2.0;
				float temp_output_8_0_g272 = 3.0;
				float2 texCoord2_g278 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g278 = 2.0;
				float temp_output_7_0_g278 = 4.0;
				float temp_output_9_0_g278 = 2.0;
				float temp_output_8_0_g278 = 3.0;
				float2 texCoord2_g280 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g280 = 3.0;
				float temp_output_7_0_g280 = 4.0;
				float temp_output_9_0_g280 = 2.0;
				float temp_output_8_0_g280 = 3.0;
				float2 texCoord2_g274 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g274 = 4.0;
				float temp_output_7_0_g274 = 4.0;
				float temp_output_9_0_g274 = 2.0;
				float temp_output_8_0_g274 = 3.0;
				float2 texCoord2_g270 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g270 = 1.0;
				float temp_output_7_0_g270 = 4.0;
				float temp_output_9_0_g270 = 1.0;
				float temp_output_8_0_g270 = 3.0;
				float2 texCoord2_g282 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g282 = 2.0;
				float temp_output_7_0_g282 = 4.0;
				float temp_output_9_0_g282 = 1.0;
				float temp_output_8_0_g282 = 3.0;
				float2 texCoord2_g240 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g240 = 3.0;
				float temp_output_7_0_g240 = 4.0;
				float temp_output_9_0_g240 = 1.0;
				float temp_output_8_0_g240 = 3.0;
				float4 temp_output_241_0 = ( float4( 1,1,1,1 ) * ( ( ( 1.0 - step( texCoord2_g240.x , ( ( temp_output_3_0_g240 - 1.0 ) / temp_output_7_0_g240 ) ) ) * ( step( texCoord2_g240.x , ( temp_output_3_0_g240 / temp_output_7_0_g240 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g240.y , ( ( temp_output_9_0_g240 - 1.0 ) / temp_output_8_0_g240 ) ) ) * ( step( texCoord2_g240.y , ( temp_output_9_0_g240 / temp_output_8_0_g240 ) ) * 1.0 ) ) ) );
				float2 texCoord2_g241 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g241 = 4.0;
				float temp_output_7_0_g241 = 4.0;
				float temp_output_9_0_g241 = 1.0;
				float temp_output_8_0_g241 = 3.0;
				float4 temp_output_230_0 = ( float4( 1,1,1,1 ) * ( ( ( 1.0 - step( texCoord2_g241.x , ( ( temp_output_3_0_g241 - 1.0 ) / temp_output_7_0_g241 ) ) ) * ( step( texCoord2_g241.x , ( temp_output_3_0_g241 / temp_output_7_0_g241 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g241.y , ( ( temp_output_9_0_g241 - 1.0 ) / temp_output_8_0_g241 ) ) ) * ( step( texCoord2_g241.y , ( temp_output_9_0_g241 / temp_output_8_0_g241 ) ) * 1.0 ) ) ) );
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g273.x , ( ( temp_output_3_0_g273 - 1.0 ) / temp_output_7_0_g273 ) ) ) * ( step( texCoord2_g273.x , ( temp_output_3_0_g273 / temp_output_7_0_g273 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g273.y , ( ( temp_output_9_0_g273 - 1.0 ) / temp_output_8_0_g273 ) ) ) * ( step( texCoord2_g273.y , ( temp_output_9_0_g273 / temp_output_8_0_g273 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g271.x , ( ( temp_output_3_0_g271 - 1.0 ) / temp_output_7_0_g271 ) ) ) * ( step( texCoord2_g271.x , ( temp_output_3_0_g271 / temp_output_7_0_g271 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g271.y , ( ( temp_output_9_0_g271 - 1.0 ) / temp_output_8_0_g271 ) ) ) * ( step( texCoord2_g271.y , ( temp_output_9_0_g271 / temp_output_8_0_g271 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g281.x , ( ( temp_output_3_0_g281 - 1.0 ) / temp_output_7_0_g281 ) ) ) * ( step( texCoord2_g281.x , ( temp_output_3_0_g281 / temp_output_7_0_g281 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g281.y , ( ( temp_output_9_0_g281 - 1.0 ) / temp_output_8_0_g281 ) ) ) * ( step( texCoord2_g281.y , ( temp_output_9_0_g281 / temp_output_8_0_g281 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g279.x , ( ( temp_output_3_0_g279 - 1.0 ) / temp_output_7_0_g279 ) ) ) * ( step( texCoord2_g279.x , ( temp_output_3_0_g279 / temp_output_7_0_g279 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g279.y , ( ( temp_output_9_0_g279 - 1.0 ) / temp_output_8_0_g279 ) ) ) * ( step( texCoord2_g279.y , ( temp_output_9_0_g279 / temp_output_8_0_g279 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g272.x , ( ( temp_output_3_0_g272 - 1.0 ) / temp_output_7_0_g272 ) ) ) * ( step( texCoord2_g272.x , ( temp_output_3_0_g272 / temp_output_7_0_g272 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g272.y , ( ( temp_output_9_0_g272 - 1.0 ) / temp_output_8_0_g272 ) ) ) * ( step( texCoord2_g272.y , ( temp_output_9_0_g272 / temp_output_8_0_g272 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g278.x , ( ( temp_output_3_0_g278 - 1.0 ) / temp_output_7_0_g278 ) ) ) * ( step( texCoord2_g278.x , ( temp_output_3_0_g278 / temp_output_7_0_g278 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g278.y , ( ( temp_output_9_0_g278 - 1.0 ) / temp_output_8_0_g278 ) ) ) * ( step( texCoord2_g278.y , ( temp_output_9_0_g278 / temp_output_8_0_g278 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g280.x , ( ( temp_output_3_0_g280 - 1.0 ) / temp_output_7_0_g280 ) ) ) * ( step( texCoord2_g280.x , ( temp_output_3_0_g280 / temp_output_7_0_g280 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g280.y , ( ( temp_output_9_0_g280 - 1.0 ) / temp_output_8_0_g280 ) ) ) * ( step( texCoord2_g280.y , ( temp_output_9_0_g280 / temp_output_8_0_g280 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g274.x , ( ( temp_output_3_0_g274 - 1.0 ) / temp_output_7_0_g274 ) ) ) * ( step( texCoord2_g274.x , ( temp_output_3_0_g274 / temp_output_7_0_g274 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g274.y , ( ( temp_output_9_0_g274 - 1.0 ) / temp_output_8_0_g274 ) ) ) * ( step( texCoord2_g274.y , ( temp_output_9_0_g274 / temp_output_8_0_g274 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g270.x , ( ( temp_output_3_0_g270 - 1.0 ) / temp_output_7_0_g270 ) ) ) * ( step( texCoord2_g270.x , ( temp_output_3_0_g270 / temp_output_7_0_g270 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g270.y , ( ( temp_output_9_0_g270 - 1.0 ) / temp_output_8_0_g270 ) ) ) * ( step( texCoord2_g270.y , ( temp_output_9_0_g270 / temp_output_8_0_g270 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g282.x , ( ( temp_output_3_0_g282 - 1.0 ) / temp_output_7_0_g282 ) ) ) * ( step( texCoord2_g282.x , ( temp_output_3_0_g282 / temp_output_7_0_g282 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g282.y , ( ( temp_output_9_0_g282 - 1.0 ) / temp_output_8_0_g282 ) ) ) * ( step( texCoord2_g282.y , ( temp_output_9_0_g282 / temp_output_8_0_g282 ) ) * 1.0 ) ) ) ) + ( temp_output_241_0 * _Color11 ) + ( temp_output_230_0 * _Color12 ) ) );
				float2 uv_Gradient = IN.ase_texcoord2.xy * _Gradient_ST.xy + _Gradient_ST.zw;
				float4 clampResult206 = clamp( ( ( tex2D( _Gradient, uv_Gradient ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float4 clampResult255 = clamp( pow( (clampResult206*_GradientScale + _GradientOffset) , temp_cast_0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				
				float3 Albedo = ( temp_output_155_0 * clampResult255 ).rgb;
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
	/*ase_lod*/
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	
	
}
/*ASEBEGIN
Version=18500
-1536;57;1536;843;743.0203;1436.913;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;201;-43.05084,-721.7265;Float;False;Property;_GradientIntensity;Gradient Intensity;17;0;Create;True;0;0;False;0;False;0.75;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;202;-41.02644,-1165.228;Inherit;True;Property;_Gradient;Gradient;16;0;Create;True;0;0;False;1;Header(Gradient);False;-1;0f424a347039ef447a763d3d4b4782b0;0f424a347039ef447a763d3d4b4782b0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;200;-5.396437,-926.7093;Float;False;Property;_GradientColor;Gradient Color;18;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;204;301.5615,-792.5283;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;203;328.2687,-922.1614;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;205;508.7686,-952.5815;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;183;-194.742,1695.03;Float;False;Property;_Color9;Color 9;8;0;Create;True;0;0;False;1;Header(Row 3);False;0.9099331,0.9264706,0.6267301,0.353;0.9099331,0.9264706,0.6267301,0.353;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;152;-194.2135,166.9271;Float;False;Property;_Color3;Color 3;2;0;Create;True;0;0;False;0;False;0.2535501,0.1544118,1,0.228;0.2535501,0.1544118,1,0.228;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;159;-187.9672,688.0273;Float;False;Property;_Color5;Color 5;4;0;Create;True;0;0;False;1;Header(Row 2);False;0.9533468,1,0.1544118,0.353;0.9533468,1,0.1544118,0.353;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;158;-183.7895,1424.406;Float;False;Property;_Color8;Color 8;7;0;Create;True;0;0;False;0;False;0.4849697,0.5008695,0.5073529,0.484;0.4849697,0.5008695,0.5073529,0.484;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;156;-195.9079,947.3851;Float;False;Property;_Color6;Color 6;5;0;Create;True;0;0;False;0;False;0.2669384,0.3207547,0.0226949,0.341;0.8483773,1,0.1544118,0.341;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-199.8005,-326.2955;Float;False;Property;_Color1;Color 1;0;0;Create;True;0;0;False;1;Header(Row 1);False;1,0.1544118,0.1544118,0.397;1,0.1544118,0.1544118,0.397;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;182;-183.4034,2422.875;Float;False;Property;_Color12;Color 12;11;0;Create;True;0;0;False;0;False;0.02270761,0.1632713,0.2205882,0.484;0.02270761,0.1632713,0.2205882,0.484;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;150;-207.7412,-66.93771;Float;False;Property;_Color2;Color 2;1;0;Create;True;0;0;False;0;False;1,0.1544118,0.8017241,0.334;1,0.1544118,0.8017241,0.334;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;230;-239.2491,2921.615;Inherit;True;ColorShartSlot;-1;;241;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;157;-182.3802,1181.25;Float;False;Property;_Color7;Color 7;6;0;Create;True;0;0;False;0;False;0.1544118,0.6151115,1,0.316;0.1544118,0.6151115,1,0.316;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;154;-195.6228,411.2479;Float;False;Property;_Color4;Color 4;3;0;Create;True;0;0;False;0;False;0.1544118,0.5451319,1,0.472;0.1544118,0.5451319,1,0.472;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;181;-202.6826,1954.387;Float;False;Property;_Color10;Color 10;9;0;Create;True;0;0;False;0;False;0.1544118,0.1602434,1,0.341;0.1544118,0.1602434,1,0.341;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;180;-168.6213,2174.434;Float;False;Property;_Color11;Color 11;10;0;Create;True;0;0;False;0;False;1,0.1544118,0.381846,0.316;1,0.1544118,0.381846,0.316;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;241;-237.0968,2697.951;Inherit;True;ColorShartSlot;-1;;240;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;207;585.6387,-538.9446;Float;False;Property;_GradientScale;Gradient Scale;19;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;208;591.5417,-443.1692;Float;False;Property;_GradientOffset;Gradient Offset;20;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;206;793.5166,-914.7413;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;232;186.9198,1938.3;Inherit;True;ColorShartSlot;-1;;282;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;238;121.5042,162.4284;Inherit;True;ColorShartSlot;-1;;281;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;240;133.3375,1181.325;Inherit;True;ColorShartSlot;-1;;280;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;237;122.0185,410.1585;Inherit;True;ColorShartSlot;-1;;279;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;231;119.8096,945.1734;Inherit;True;ColorShartSlot;-1;;278;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;234;133.8517,1424.481;Inherit;True;ColorShartSlot;-1;;274;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;211;217.2,2168.927;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;233;127.7504,688.1025;Inherit;True;ColorShartSlot;-1;;272;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;226.7271,2394.801;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;236;107.9764,-66.86263;Inherit;True;ColorShartSlot;-1;;271;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;209;1091.96,-605.7403;Inherit;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;253;1474.609,-467.7142;Float;False;Property;_GradientPower;Gradient Power;21;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;235;194.8606,1678.942;Inherit;True;ColorShartSlot;-1;;270;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;239;115.9171,-326.2204;Inherit;True;ColorShartSlot;-1;;273;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;1130.732,57.40811;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;193;1126.266,334.7972;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;146;1124.026,-170.6852;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;254;1704.252,-572.7532;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;255;2050.583,-493.1835;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;1378.894,-29.6249;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;217;1070.367,2661.439;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;1752.065,420.4065;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;1470.311,715.3326;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;216;925.6725,2423.552;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;196;1143.088,753.1885;Float;False;Property;_Color12Emmision;Color 12 Emmision;15;1;[HDR];Create;True;0;0;False;0;False;0,0,0,1;0,0,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;166;1413.004,514.6777;Float;False;Property;_Smoothness;Smoothness;12;0;Create;True;0;0;False;1;Header(Smoothness (Alphas));False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;1843.751,-118.5323;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;195;1135.674,583.2192;Float;False;Property;_Color11Emmision;Color 11 Emmision;14;1;[HDR];Create;True;0;0;False;1;Header(Emmision);False;0,0,0,1;0,0,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;194;1516.74,353.2244;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;199;1775.644,656.0661;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;165;1691.967,238.6589;Float;False;Property;_Metallic;Metallic;13;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;214;1476.114,845.3589;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;258;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;257;2076.697,169.3291;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;11;Malbers/Color4x3;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;17;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;;0;0;Standard;36;Workflow;1;Surface;0;  Refraction Model;0;  Blend;0;Two Sided;1;Fragment Normal Space,InvertActionOnDeselection;0;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;1;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Vertex Position,InvertActionOnDeselection;1;0;6;False;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;259;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;False;False;False;False;0;False;-1;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;260;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;256;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;261;2076.697,169.3291;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;204;0;201;0
WireConnection;203;0;202;0
WireConnection;203;1;200;0
WireConnection;205;0;203;0
WireConnection;205;1;204;0
WireConnection;206;0;205;0
WireConnection;232;38;181;0
WireConnection;238;38;152;0
WireConnection;240;38;157;0
WireConnection;237;38;154;0
WireConnection;231;38;156;0
WireConnection;234;38;158;0
WireConnection;211;0;241;0
WireConnection;211;1;180;0
WireConnection;233;38;159;0
WireConnection;212;0;230;0
WireConnection;212;1;182;0
WireConnection;236;38;150;0
WireConnection;209;0;206;0
WireConnection;209;1;207;0
WireConnection;209;2;208;0
WireConnection;235;38;183;0
WireConnection;239;38;23;0
WireConnection;164;0;233;0
WireConnection;164;1;231;0
WireConnection;164;2;240;0
WireConnection;164;3;234;0
WireConnection;193;0;235;0
WireConnection;193;1;232;0
WireConnection;193;2;211;0
WireConnection;193;3;212;0
WireConnection;146;0;239;0
WireConnection;146;1;236;0
WireConnection;146;2;238;0
WireConnection;146;3;237;0
WireConnection;254;0;209;0
WireConnection;254;1;253;0
WireConnection;255;0;254;0
WireConnection;155;0;146;0
WireConnection;155;1;164;0
WireConnection;155;2;193;0
WireConnection;217;0;230;0
WireConnection;178;0;194;0
WireConnection;178;1;166;0
WireConnection;213;0;195;0
WireConnection;213;1;216;0
WireConnection;216;0;241;0
WireConnection;210;0;155;0
WireConnection;210;1;255;0
WireConnection;194;0;155;0
WireConnection;199;0;213;0
WireConnection;199;1;214;0
WireConnection;214;0;196;0
WireConnection;214;1;217;0
WireConnection;257;0;210;0
WireConnection;257;2;199;0
WireConnection;257;3;165;0
WireConnection;257;4;178;0
ASEEND*/
//CHKSM=98306C6EAFDCEEFC1ED6AB74B9C96AF14CCF4E85