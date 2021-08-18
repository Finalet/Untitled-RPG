// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Color4x4v2"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Header(Albedo (A Gradient))]_Color1("Color 1", Color) = (1,0.1544118,0.1544118,0.291)
		_Color2("Color 2", Color) = (1,0.1544118,0.8017241,0.253)
		_Color3("Color 3", Color) = (0.2535501,0.1544118,1,0.541)
		_Color4("Color 4", Color) = (0.1544118,0.5451319,1,0.253)
		[Space(10)]_Color5("Color 5", Color) = (0.9533468,1,0.1544118,0.553)
		_Color6("Color 6", Color) = (0.2720588,0.1294625,0,0.097)
		_Color7("Color 7", Color) = (0.1544118,0.6151115,1,0.178)
		_Color8("Color 8", Color) = (0.4849697,0.5008695,0.5073529,0.078)
		[Space(10)]_Color9("Color 9", Color) = (0.3164301,0,0.7058823,0.134)
		_Color10("Color 10", Color) = (0.362069,0.4411765,0,0.759)
		_Color11("Color 11", Color) = (0.6691177,0.6691177,0.6691177,0.647)
		_Color12("Color 12", Color) = (0.5073529,0.1574544,0,0.128)
		[Space(10)]_Color13("Color 13", Color) = (1,0.5586207,0,0.272)
		_Color14("Color 14", Color) = (0,0.8025862,0.875,0.047)
		_Color15("Color 15", Color) = (1,0,0,0.391)
		_Color16("Color 16", Color) = (0.4080882,0.75,0.4811866,0.134)
		[Header(Metallic(R) Rough(G) Emmission(B))]_MRE1("MRE 1", Color) = (0,1,0,0)
		_MRE2("MRE 2", Color) = (0,1,0,0)
		_MRE3("MRE 3", Color) = (0,1,0,0)
		_MRE4("MRE 4", Color) = (0,1,0,0)
		[Space(10)]_MRE5("MRE 5", Color) = (0,1,0,0)
		_MRE6("MRE 6", Color) = (0,1,0,0)
		_MRE7("MRE 7", Color) = (0,1,0,0)
		_MRE8("MRE 8", Color) = (0,1,0,0)
		[Space(10)]_MRE9("MRE 9", Color) = (0,1,0,0)
		_MRE10("MRE 10", Color) = (0,1,0,0)
		_MRE11("MRE 11", Color) = (0,1,0,0)
		_MRE12("MRE 12", Color) = (0,1,0,0)
		[Space(10)]_MRE13("MRE 13", Color) = (0,1,0,0)
		_MRE14("MRE 14", Color) = (0,1,0,0)
		_MRE15("MRE 15", Color) = (0,1,0,0)
		_MRE16("MRE 16", Color) = (0,1,0,0)
		[Header(Emmision)]_EmissionPower1("Emission Power", Float) = 1
		[SingleLineTexture][Header(Gradient)]_Gradient("Gradient", 2D) = "white" {}
		_GradientIntensity("Gradient Intensity", Range( 0 , 1)) = 0.75
		_GradientColor("Gradient Color", Color) = (0,0,0,0)
		_GradientScale("Gradient Scale", Float) = 1
		_GradientOffset("Gradient Offset", Float) = 0
		[ASEEnd]_GradientPower("Gradient Power", Float) = 1

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
			#define ASE_SRP_VERSION 999999

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
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
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
				float4 texcoord : TEXCOORD0;
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
				o.texcoord = v.texcoord;
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
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
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

				float2 texCoord255 = IN.ase_texcoord7.xy * float2( 1,4 ) + float2( 0,0 );
				float4 clampResult234 = clamp( ( ( tex2D( _Gradient, texCoord255 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g680 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g680 = 1.0;
				float temp_output_7_0_g680 = 4.0;
				float temp_output_9_0_g680 = 4.0;
				float temp_output_8_0_g680 = 4.0;
				float2 texCoord2_g676 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g676 = 2.0;
				float temp_output_7_0_g676 = 4.0;
				float temp_output_9_0_g676 = 4.0;
				float temp_output_8_0_g676 = 4.0;
				float2 texCoord2_g677 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g677 = 3.0;
				float temp_output_7_0_g677 = 4.0;
				float temp_output_9_0_g677 = 4.0;
				float temp_output_8_0_g677 = 4.0;
				float2 texCoord2_g679 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g679 = 4.0;
				float temp_output_7_0_g679 = 4.0;
				float temp_output_9_0_g679 = 4.0;
				float temp_output_8_0_g679 = 4.0;
				float2 texCoord2_g669 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g669 = 1.0;
				float temp_output_7_0_g669 = 4.0;
				float temp_output_9_0_g669 = 3.0;
				float temp_output_8_0_g669 = 4.0;
				float2 texCoord2_g671 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g671 = 2.0;
				float temp_output_7_0_g671 = 4.0;
				float temp_output_9_0_g671 = 3.0;
				float temp_output_8_0_g671 = 4.0;
				float2 texCoord2_g681 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g681 = 3.0;
				float temp_output_7_0_g681 = 4.0;
				float temp_output_9_0_g681 = 3.0;
				float temp_output_8_0_g681 = 4.0;
				float2 texCoord2_g675 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g675 = 4.0;
				float temp_output_7_0_g675 = 4.0;
				float temp_output_9_0_g675 = 3.0;
				float temp_output_8_0_g675 = 4.0;
				float2 texCoord2_g670 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g670 = 1.0;
				float temp_output_7_0_g670 = 4.0;
				float temp_output_9_0_g670 = 2.0;
				float temp_output_8_0_g670 = 4.0;
				float2 texCoord2_g672 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g672 = 2.0;
				float temp_output_7_0_g672 = 4.0;
				float temp_output_9_0_g672 = 2.0;
				float temp_output_8_0_g672 = 4.0;
				float2 texCoord2_g661 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g661 = 3.0;
				float temp_output_7_0_g661 = 4.0;
				float temp_output_9_0_g661 = 2.0;
				float temp_output_8_0_g661 = 4.0;
				float2 texCoord2_g668 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g668 = 4.0;
				float temp_output_7_0_g668 = 4.0;
				float temp_output_9_0_g668 = 2.0;
				float temp_output_8_0_g668 = 4.0;
				float2 texCoord2_g682 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g682 = 1.0;
				float temp_output_7_0_g682 = 4.0;
				float temp_output_9_0_g682 = 1.0;
				float temp_output_8_0_g682 = 4.0;
				float2 texCoord2_g673 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g673 = 2.0;
				float temp_output_7_0_g673 = 4.0;
				float temp_output_9_0_g673 = 1.0;
				float temp_output_8_0_g673 = 4.0;
				float2 texCoord2_g678 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g678 = 3.0;
				float temp_output_7_0_g678 = 4.0;
				float temp_output_9_0_g678 = 1.0;
				float temp_output_8_0_g678 = 4.0;
				float2 texCoord2_g674 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g674 = 4.0;
				float temp_output_7_0_g674 = 4.0;
				float temp_output_9_0_g674 = 1.0;
				float temp_output_8_0_g674 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g680.x , ( ( temp_output_3_0_g680 - 1.0 ) / temp_output_7_0_g680 ) ) ) * ( step( texCoord2_g680.x , ( temp_output_3_0_g680 / temp_output_7_0_g680 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g680.y , ( ( temp_output_9_0_g680 - 1.0 ) / temp_output_8_0_g680 ) ) ) * ( step( texCoord2_g680.y , ( temp_output_9_0_g680 / temp_output_8_0_g680 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g676.x , ( ( temp_output_3_0_g676 - 1.0 ) / temp_output_7_0_g676 ) ) ) * ( step( texCoord2_g676.x , ( temp_output_3_0_g676 / temp_output_7_0_g676 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g676.y , ( ( temp_output_9_0_g676 - 1.0 ) / temp_output_8_0_g676 ) ) ) * ( step( texCoord2_g676.y , ( temp_output_9_0_g676 / temp_output_8_0_g676 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g677.x , ( ( temp_output_3_0_g677 - 1.0 ) / temp_output_7_0_g677 ) ) ) * ( step( texCoord2_g677.x , ( temp_output_3_0_g677 / temp_output_7_0_g677 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g677.y , ( ( temp_output_9_0_g677 - 1.0 ) / temp_output_8_0_g677 ) ) ) * ( step( texCoord2_g677.y , ( temp_output_9_0_g677 / temp_output_8_0_g677 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g679.x , ( ( temp_output_3_0_g679 - 1.0 ) / temp_output_7_0_g679 ) ) ) * ( step( texCoord2_g679.x , ( temp_output_3_0_g679 / temp_output_7_0_g679 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g679.y , ( ( temp_output_9_0_g679 - 1.0 ) / temp_output_8_0_g679 ) ) ) * ( step( texCoord2_g679.y , ( temp_output_9_0_g679 / temp_output_8_0_g679 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g669.x , ( ( temp_output_3_0_g669 - 1.0 ) / temp_output_7_0_g669 ) ) ) * ( step( texCoord2_g669.x , ( temp_output_3_0_g669 / temp_output_7_0_g669 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g669.y , ( ( temp_output_9_0_g669 - 1.0 ) / temp_output_8_0_g669 ) ) ) * ( step( texCoord2_g669.y , ( temp_output_9_0_g669 / temp_output_8_0_g669 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g671.x , ( ( temp_output_3_0_g671 - 1.0 ) / temp_output_7_0_g671 ) ) ) * ( step( texCoord2_g671.x , ( temp_output_3_0_g671 / temp_output_7_0_g671 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g671.y , ( ( temp_output_9_0_g671 - 1.0 ) / temp_output_8_0_g671 ) ) ) * ( step( texCoord2_g671.y , ( temp_output_9_0_g671 / temp_output_8_0_g671 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g681.x , ( ( temp_output_3_0_g681 - 1.0 ) / temp_output_7_0_g681 ) ) ) * ( step( texCoord2_g681.x , ( temp_output_3_0_g681 / temp_output_7_0_g681 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g681.y , ( ( temp_output_9_0_g681 - 1.0 ) / temp_output_8_0_g681 ) ) ) * ( step( texCoord2_g681.y , ( temp_output_9_0_g681 / temp_output_8_0_g681 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g675.x , ( ( temp_output_3_0_g675 - 1.0 ) / temp_output_7_0_g675 ) ) ) * ( step( texCoord2_g675.x , ( temp_output_3_0_g675 / temp_output_7_0_g675 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g675.y , ( ( temp_output_9_0_g675 - 1.0 ) / temp_output_8_0_g675 ) ) ) * ( step( texCoord2_g675.y , ( temp_output_9_0_g675 / temp_output_8_0_g675 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g670.x , ( ( temp_output_3_0_g670 - 1.0 ) / temp_output_7_0_g670 ) ) ) * ( step( texCoord2_g670.x , ( temp_output_3_0_g670 / temp_output_7_0_g670 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g670.y , ( ( temp_output_9_0_g670 - 1.0 ) / temp_output_8_0_g670 ) ) ) * ( step( texCoord2_g670.y , ( temp_output_9_0_g670 / temp_output_8_0_g670 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g672.x , ( ( temp_output_3_0_g672 - 1.0 ) / temp_output_7_0_g672 ) ) ) * ( step( texCoord2_g672.x , ( temp_output_3_0_g672 / temp_output_7_0_g672 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g672.y , ( ( temp_output_9_0_g672 - 1.0 ) / temp_output_8_0_g672 ) ) ) * ( step( texCoord2_g672.y , ( temp_output_9_0_g672 / temp_output_8_0_g672 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( texCoord2_g661.x , ( ( temp_output_3_0_g661 - 1.0 ) / temp_output_7_0_g661 ) ) ) * ( step( texCoord2_g661.x , ( temp_output_3_0_g661 / temp_output_7_0_g661 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g661.y , ( ( temp_output_9_0_g661 - 1.0 ) / temp_output_8_0_g661 ) ) ) * ( step( texCoord2_g661.y , ( temp_output_9_0_g661 / temp_output_8_0_g661 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( texCoord2_g668.x , ( ( temp_output_3_0_g668 - 1.0 ) / temp_output_7_0_g668 ) ) ) * ( step( texCoord2_g668.x , ( temp_output_3_0_g668 / temp_output_7_0_g668 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g668.y , ( ( temp_output_9_0_g668 - 1.0 ) / temp_output_8_0_g668 ) ) ) * ( step( texCoord2_g668.y , ( temp_output_9_0_g668 / temp_output_8_0_g668 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( texCoord2_g682.x , ( ( temp_output_3_0_g682 - 1.0 ) / temp_output_7_0_g682 ) ) ) * ( step( texCoord2_g682.x , ( temp_output_3_0_g682 / temp_output_7_0_g682 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g682.y , ( ( temp_output_9_0_g682 - 1.0 ) / temp_output_8_0_g682 ) ) ) * ( step( texCoord2_g682.y , ( temp_output_9_0_g682 / temp_output_8_0_g682 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( texCoord2_g673.x , ( ( temp_output_3_0_g673 - 1.0 ) / temp_output_7_0_g673 ) ) ) * ( step( texCoord2_g673.x , ( temp_output_3_0_g673 / temp_output_7_0_g673 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g673.y , ( ( temp_output_9_0_g673 - 1.0 ) / temp_output_8_0_g673 ) ) ) * ( step( texCoord2_g673.y , ( temp_output_9_0_g673 / temp_output_8_0_g673 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( texCoord2_g678.x , ( ( temp_output_3_0_g678 - 1.0 ) / temp_output_7_0_g678 ) ) ) * ( step( texCoord2_g678.x , ( temp_output_3_0_g678 / temp_output_7_0_g678 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g678.y , ( ( temp_output_9_0_g678 - 1.0 ) / temp_output_8_0_g678 ) ) ) * ( step( texCoord2_g678.y , ( temp_output_9_0_g678 / temp_output_8_0_g678 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( texCoord2_g674.x , ( ( temp_output_3_0_g674 - 1.0 ) / temp_output_7_0_g674 ) ) ) * ( step( texCoord2_g674.x , ( temp_output_3_0_g674 / temp_output_7_0_g674 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g674.y , ( ( temp_output_9_0_g674 - 1.0 ) / temp_output_8_0_g674 ) ) ) * ( step( texCoord2_g674.y , ( temp_output_9_0_g674 / temp_output_8_0_g674 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult261 = clamp( ( pow( (clampResult234*_GradientScale + _GradientOffset) , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float2 texCoord2_g701 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g701 = 1.0;
				float temp_output_7_0_g701 = 4.0;
				float temp_output_9_0_g701 = 4.0;
				float temp_output_8_0_g701 = 4.0;
				float2 texCoord2_g706 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g706 = 2.0;
				float temp_output_7_0_g706 = 4.0;
				float temp_output_9_0_g706 = 4.0;
				float temp_output_8_0_g706 = 4.0;
				float2 texCoord2_g700 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g700 = 3.0;
				float temp_output_7_0_g700 = 4.0;
				float temp_output_9_0_g700 = 4.0;
				float temp_output_8_0_g700 = 4.0;
				float2 texCoord2_g698 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g698 = 4.0;
				float temp_output_7_0_g698 = 4.0;
				float temp_output_9_0_g698 = 4.0;
				float temp_output_8_0_g698 = 4.0;
				float2 texCoord2_g699 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g699 = 1.0;
				float temp_output_7_0_g699 = 4.0;
				float temp_output_9_0_g699 = 3.0;
				float temp_output_8_0_g699 = 4.0;
				float2 texCoord2_g696 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g696 = 2.0;
				float temp_output_7_0_g696 = 4.0;
				float temp_output_9_0_g696 = 3.0;
				float temp_output_8_0_g696 = 4.0;
				float2 texCoord2_g707 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g707 = 3.0;
				float temp_output_7_0_g707 = 4.0;
				float temp_output_9_0_g707 = 3.0;
				float temp_output_8_0_g707 = 4.0;
				float2 texCoord2_g695 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g695 = 4.0;
				float temp_output_7_0_g695 = 4.0;
				float temp_output_9_0_g695 = 3.0;
				float temp_output_8_0_g695 = 4.0;
				float2 texCoord2_g704 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g704 = 1.0;
				float temp_output_7_0_g704 = 4.0;
				float temp_output_9_0_g704 = 2.0;
				float temp_output_8_0_g704 = 4.0;
				float2 texCoord2_g697 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g697 = 2.0;
				float temp_output_7_0_g697 = 4.0;
				float temp_output_9_0_g697 = 2.0;
				float temp_output_8_0_g697 = 4.0;
				float2 texCoord2_g702 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g702 = 3.0;
				float temp_output_7_0_g702 = 4.0;
				float temp_output_9_0_g702 = 2.0;
				float temp_output_8_0_g702 = 4.0;
				float2 texCoord2_g705 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g705 = 4.0;
				float temp_output_7_0_g705 = 4.0;
				float temp_output_9_0_g705 = 2.0;
				float temp_output_8_0_g705 = 4.0;
				float2 texCoord2_g685 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g685 = 1.0;
				float temp_output_7_0_g685 = 4.0;
				float temp_output_9_0_g685 = 1.0;
				float temp_output_8_0_g685 = 4.0;
				float2 texCoord2_g694 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g694 = 2.0;
				float temp_output_7_0_g694 = 4.0;
				float temp_output_9_0_g694 = 1.0;
				float temp_output_8_0_g694 = 4.0;
				float2 texCoord2_g693 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g693 = 3.0;
				float temp_output_7_0_g693 = 4.0;
				float temp_output_9_0_g693 = 1.0;
				float temp_output_8_0_g693 = 4.0;
				float2 texCoord2_g703 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g703 = 4.0;
				float temp_output_7_0_g703 = 4.0;
				float temp_output_9_0_g703 = 1.0;
				float temp_output_8_0_g703 = 4.0;
				float4 temp_output_283_0 = ( ( ( _MRE1 * ( ( ( 1.0 - step( texCoord2_g701.x , ( ( temp_output_3_0_g701 - 1.0 ) / temp_output_7_0_g701 ) ) ) * ( step( texCoord2_g701.x , ( temp_output_3_0_g701 / temp_output_7_0_g701 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g701.y , ( ( temp_output_9_0_g701 - 1.0 ) / temp_output_8_0_g701 ) ) ) * ( step( texCoord2_g701.y , ( temp_output_9_0_g701 / temp_output_8_0_g701 ) ) * 1.0 ) ) ) ) + ( _MRE2 * ( ( ( 1.0 - step( texCoord2_g706.x , ( ( temp_output_3_0_g706 - 1.0 ) / temp_output_7_0_g706 ) ) ) * ( step( texCoord2_g706.x , ( temp_output_3_0_g706 / temp_output_7_0_g706 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g706.y , ( ( temp_output_9_0_g706 - 1.0 ) / temp_output_8_0_g706 ) ) ) * ( step( texCoord2_g706.y , ( temp_output_9_0_g706 / temp_output_8_0_g706 ) ) * 1.0 ) ) ) ) + ( _MRE3 * ( ( ( 1.0 - step( texCoord2_g700.x , ( ( temp_output_3_0_g700 - 1.0 ) / temp_output_7_0_g700 ) ) ) * ( step( texCoord2_g700.x , ( temp_output_3_0_g700 / temp_output_7_0_g700 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g700.y , ( ( temp_output_9_0_g700 - 1.0 ) / temp_output_8_0_g700 ) ) ) * ( step( texCoord2_g700.y , ( temp_output_9_0_g700 / temp_output_8_0_g700 ) ) * 1.0 ) ) ) ) + ( _MRE4 * ( ( ( 1.0 - step( texCoord2_g698.x , ( ( temp_output_3_0_g698 - 1.0 ) / temp_output_7_0_g698 ) ) ) * ( step( texCoord2_g698.x , ( temp_output_3_0_g698 / temp_output_7_0_g698 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g698.y , ( ( temp_output_9_0_g698 - 1.0 ) / temp_output_8_0_g698 ) ) ) * ( step( texCoord2_g698.y , ( temp_output_9_0_g698 / temp_output_8_0_g698 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE5 * ( ( ( 1.0 - step( texCoord2_g699.x , ( ( temp_output_3_0_g699 - 1.0 ) / temp_output_7_0_g699 ) ) ) * ( step( texCoord2_g699.x , ( temp_output_3_0_g699 / temp_output_7_0_g699 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g699.y , ( ( temp_output_9_0_g699 - 1.0 ) / temp_output_8_0_g699 ) ) ) * ( step( texCoord2_g699.y , ( temp_output_9_0_g699 / temp_output_8_0_g699 ) ) * 1.0 ) ) ) ) + ( _MRE6 * ( ( ( 1.0 - step( texCoord2_g696.x , ( ( temp_output_3_0_g696 - 1.0 ) / temp_output_7_0_g696 ) ) ) * ( step( texCoord2_g696.x , ( temp_output_3_0_g696 / temp_output_7_0_g696 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g696.y , ( ( temp_output_9_0_g696 - 1.0 ) / temp_output_8_0_g696 ) ) ) * ( step( texCoord2_g696.y , ( temp_output_9_0_g696 / temp_output_8_0_g696 ) ) * 1.0 ) ) ) ) + ( _MRE7 * ( ( ( 1.0 - step( texCoord2_g707.x , ( ( temp_output_3_0_g707 - 1.0 ) / temp_output_7_0_g707 ) ) ) * ( step( texCoord2_g707.x , ( temp_output_3_0_g707 / temp_output_7_0_g707 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g707.y , ( ( temp_output_9_0_g707 - 1.0 ) / temp_output_8_0_g707 ) ) ) * ( step( texCoord2_g707.y , ( temp_output_9_0_g707 / temp_output_8_0_g707 ) ) * 1.0 ) ) ) ) + ( _MRE8 * ( ( ( 1.0 - step( texCoord2_g695.x , ( ( temp_output_3_0_g695 - 1.0 ) / temp_output_7_0_g695 ) ) ) * ( step( texCoord2_g695.x , ( temp_output_3_0_g695 / temp_output_7_0_g695 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g695.y , ( ( temp_output_9_0_g695 - 1.0 ) / temp_output_8_0_g695 ) ) ) * ( step( texCoord2_g695.y , ( temp_output_9_0_g695 / temp_output_8_0_g695 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE9 * ( ( ( 1.0 - step( texCoord2_g704.x , ( ( temp_output_3_0_g704 - 1.0 ) / temp_output_7_0_g704 ) ) ) * ( step( texCoord2_g704.x , ( temp_output_3_0_g704 / temp_output_7_0_g704 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g704.y , ( ( temp_output_9_0_g704 - 1.0 ) / temp_output_8_0_g704 ) ) ) * ( step( texCoord2_g704.y , ( temp_output_9_0_g704 / temp_output_8_0_g704 ) ) * 1.0 ) ) ) ) + ( _MRE10 * ( ( ( 1.0 - step( texCoord2_g697.x , ( ( temp_output_3_0_g697 - 1.0 ) / temp_output_7_0_g697 ) ) ) * ( step( texCoord2_g697.x , ( temp_output_3_0_g697 / temp_output_7_0_g697 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g697.y , ( ( temp_output_9_0_g697 - 1.0 ) / temp_output_8_0_g697 ) ) ) * ( step( texCoord2_g697.y , ( temp_output_9_0_g697 / temp_output_8_0_g697 ) ) * 1.0 ) ) ) ) + ( _MRE11 * ( ( ( 1.0 - step( texCoord2_g702.x , ( ( temp_output_3_0_g702 - 1.0 ) / temp_output_7_0_g702 ) ) ) * ( step( texCoord2_g702.x , ( temp_output_3_0_g702 / temp_output_7_0_g702 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g702.y , ( ( temp_output_9_0_g702 - 1.0 ) / temp_output_8_0_g702 ) ) ) * ( step( texCoord2_g702.y , ( temp_output_9_0_g702 / temp_output_8_0_g702 ) ) * 1.0 ) ) ) ) + ( _MRE12 * ( ( ( 1.0 - step( texCoord2_g705.x , ( ( temp_output_3_0_g705 - 1.0 ) / temp_output_7_0_g705 ) ) ) * ( step( texCoord2_g705.x , ( temp_output_3_0_g705 / temp_output_7_0_g705 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g705.y , ( ( temp_output_9_0_g705 - 1.0 ) / temp_output_8_0_g705 ) ) ) * ( step( texCoord2_g705.y , ( temp_output_9_0_g705 / temp_output_8_0_g705 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE13 * ( ( ( 1.0 - step( texCoord2_g685.x , ( ( temp_output_3_0_g685 - 1.0 ) / temp_output_7_0_g685 ) ) ) * ( step( texCoord2_g685.x , ( temp_output_3_0_g685 / temp_output_7_0_g685 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g685.y , ( ( temp_output_9_0_g685 - 1.0 ) / temp_output_8_0_g685 ) ) ) * ( step( texCoord2_g685.y , ( temp_output_9_0_g685 / temp_output_8_0_g685 ) ) * 1.0 ) ) ) ) + ( _MRE14 * ( ( ( 1.0 - step( texCoord2_g694.x , ( ( temp_output_3_0_g694 - 1.0 ) / temp_output_7_0_g694 ) ) ) * ( step( texCoord2_g694.x , ( temp_output_3_0_g694 / temp_output_7_0_g694 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g694.y , ( ( temp_output_9_0_g694 - 1.0 ) / temp_output_8_0_g694 ) ) ) * ( step( texCoord2_g694.y , ( temp_output_9_0_g694 / temp_output_8_0_g694 ) ) * 1.0 ) ) ) ) + ( _MRE15 * ( ( ( 1.0 - step( texCoord2_g693.x , ( ( temp_output_3_0_g693 - 1.0 ) / temp_output_7_0_g693 ) ) ) * ( step( texCoord2_g693.x , ( temp_output_3_0_g693 / temp_output_7_0_g693 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g693.y , ( ( temp_output_9_0_g693 - 1.0 ) / temp_output_8_0_g693 ) ) ) * ( step( texCoord2_g693.y , ( temp_output_9_0_g693 / temp_output_8_0_g693 ) ) * 1.0 ) ) ) ) + ( _MRE16 * ( ( ( 1.0 - step( texCoord2_g703.x , ( ( temp_output_3_0_g703 - 1.0 ) / temp_output_7_0_g703 ) ) ) * ( step( texCoord2_g703.x , ( temp_output_3_0_g703 / temp_output_7_0_g703 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g703.y , ( ( temp_output_9_0_g703 - 1.0 ) / temp_output_8_0_g703 ) ) ) * ( step( texCoord2_g703.y , ( temp_output_9_0_g703 / temp_output_8_0_g703 ) ) * 1.0 ) ) ) ) ) );
				
				float3 Albedo = ( clampResult261 * temp_output_155_0 ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( temp_output_155_0 * ( _EmissionPower1 * (temp_output_283_0).b ) ).rgb;
				float3 Specular = 0.5;
				float Metallic = (temp_output_283_0).r;
				float Smoothness = ( 1.0 - (temp_output_283_0).g );
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
			#define ASE_SRP_VERSION 999999

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
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
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
			#define ASE_SRP_VERSION 999999

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
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
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
			#define ASE_SRP_VERSION 999999

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
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
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

				float2 texCoord255 = IN.ase_texcoord2.xy * float2( 1,4 ) + float2( 0,0 );
				float4 clampResult234 = clamp( ( ( tex2D( _Gradient, texCoord255 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g680 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g680 = 1.0;
				float temp_output_7_0_g680 = 4.0;
				float temp_output_9_0_g680 = 4.0;
				float temp_output_8_0_g680 = 4.0;
				float2 texCoord2_g676 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g676 = 2.0;
				float temp_output_7_0_g676 = 4.0;
				float temp_output_9_0_g676 = 4.0;
				float temp_output_8_0_g676 = 4.0;
				float2 texCoord2_g677 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g677 = 3.0;
				float temp_output_7_0_g677 = 4.0;
				float temp_output_9_0_g677 = 4.0;
				float temp_output_8_0_g677 = 4.0;
				float2 texCoord2_g679 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g679 = 4.0;
				float temp_output_7_0_g679 = 4.0;
				float temp_output_9_0_g679 = 4.0;
				float temp_output_8_0_g679 = 4.0;
				float2 texCoord2_g669 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g669 = 1.0;
				float temp_output_7_0_g669 = 4.0;
				float temp_output_9_0_g669 = 3.0;
				float temp_output_8_0_g669 = 4.0;
				float2 texCoord2_g671 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g671 = 2.0;
				float temp_output_7_0_g671 = 4.0;
				float temp_output_9_0_g671 = 3.0;
				float temp_output_8_0_g671 = 4.0;
				float2 texCoord2_g681 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g681 = 3.0;
				float temp_output_7_0_g681 = 4.0;
				float temp_output_9_0_g681 = 3.0;
				float temp_output_8_0_g681 = 4.0;
				float2 texCoord2_g675 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g675 = 4.0;
				float temp_output_7_0_g675 = 4.0;
				float temp_output_9_0_g675 = 3.0;
				float temp_output_8_0_g675 = 4.0;
				float2 texCoord2_g670 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g670 = 1.0;
				float temp_output_7_0_g670 = 4.0;
				float temp_output_9_0_g670 = 2.0;
				float temp_output_8_0_g670 = 4.0;
				float2 texCoord2_g672 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g672 = 2.0;
				float temp_output_7_0_g672 = 4.0;
				float temp_output_9_0_g672 = 2.0;
				float temp_output_8_0_g672 = 4.0;
				float2 texCoord2_g661 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g661 = 3.0;
				float temp_output_7_0_g661 = 4.0;
				float temp_output_9_0_g661 = 2.0;
				float temp_output_8_0_g661 = 4.0;
				float2 texCoord2_g668 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g668 = 4.0;
				float temp_output_7_0_g668 = 4.0;
				float temp_output_9_0_g668 = 2.0;
				float temp_output_8_0_g668 = 4.0;
				float2 texCoord2_g682 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g682 = 1.0;
				float temp_output_7_0_g682 = 4.0;
				float temp_output_9_0_g682 = 1.0;
				float temp_output_8_0_g682 = 4.0;
				float2 texCoord2_g673 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g673 = 2.0;
				float temp_output_7_0_g673 = 4.0;
				float temp_output_9_0_g673 = 1.0;
				float temp_output_8_0_g673 = 4.0;
				float2 texCoord2_g678 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g678 = 3.0;
				float temp_output_7_0_g678 = 4.0;
				float temp_output_9_0_g678 = 1.0;
				float temp_output_8_0_g678 = 4.0;
				float2 texCoord2_g674 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g674 = 4.0;
				float temp_output_7_0_g674 = 4.0;
				float temp_output_9_0_g674 = 1.0;
				float temp_output_8_0_g674 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g680.x , ( ( temp_output_3_0_g680 - 1.0 ) / temp_output_7_0_g680 ) ) ) * ( step( texCoord2_g680.x , ( temp_output_3_0_g680 / temp_output_7_0_g680 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g680.y , ( ( temp_output_9_0_g680 - 1.0 ) / temp_output_8_0_g680 ) ) ) * ( step( texCoord2_g680.y , ( temp_output_9_0_g680 / temp_output_8_0_g680 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g676.x , ( ( temp_output_3_0_g676 - 1.0 ) / temp_output_7_0_g676 ) ) ) * ( step( texCoord2_g676.x , ( temp_output_3_0_g676 / temp_output_7_0_g676 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g676.y , ( ( temp_output_9_0_g676 - 1.0 ) / temp_output_8_0_g676 ) ) ) * ( step( texCoord2_g676.y , ( temp_output_9_0_g676 / temp_output_8_0_g676 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g677.x , ( ( temp_output_3_0_g677 - 1.0 ) / temp_output_7_0_g677 ) ) ) * ( step( texCoord2_g677.x , ( temp_output_3_0_g677 / temp_output_7_0_g677 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g677.y , ( ( temp_output_9_0_g677 - 1.0 ) / temp_output_8_0_g677 ) ) ) * ( step( texCoord2_g677.y , ( temp_output_9_0_g677 / temp_output_8_0_g677 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g679.x , ( ( temp_output_3_0_g679 - 1.0 ) / temp_output_7_0_g679 ) ) ) * ( step( texCoord2_g679.x , ( temp_output_3_0_g679 / temp_output_7_0_g679 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g679.y , ( ( temp_output_9_0_g679 - 1.0 ) / temp_output_8_0_g679 ) ) ) * ( step( texCoord2_g679.y , ( temp_output_9_0_g679 / temp_output_8_0_g679 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g669.x , ( ( temp_output_3_0_g669 - 1.0 ) / temp_output_7_0_g669 ) ) ) * ( step( texCoord2_g669.x , ( temp_output_3_0_g669 / temp_output_7_0_g669 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g669.y , ( ( temp_output_9_0_g669 - 1.0 ) / temp_output_8_0_g669 ) ) ) * ( step( texCoord2_g669.y , ( temp_output_9_0_g669 / temp_output_8_0_g669 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g671.x , ( ( temp_output_3_0_g671 - 1.0 ) / temp_output_7_0_g671 ) ) ) * ( step( texCoord2_g671.x , ( temp_output_3_0_g671 / temp_output_7_0_g671 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g671.y , ( ( temp_output_9_0_g671 - 1.0 ) / temp_output_8_0_g671 ) ) ) * ( step( texCoord2_g671.y , ( temp_output_9_0_g671 / temp_output_8_0_g671 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g681.x , ( ( temp_output_3_0_g681 - 1.0 ) / temp_output_7_0_g681 ) ) ) * ( step( texCoord2_g681.x , ( temp_output_3_0_g681 / temp_output_7_0_g681 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g681.y , ( ( temp_output_9_0_g681 - 1.0 ) / temp_output_8_0_g681 ) ) ) * ( step( texCoord2_g681.y , ( temp_output_9_0_g681 / temp_output_8_0_g681 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g675.x , ( ( temp_output_3_0_g675 - 1.0 ) / temp_output_7_0_g675 ) ) ) * ( step( texCoord2_g675.x , ( temp_output_3_0_g675 / temp_output_7_0_g675 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g675.y , ( ( temp_output_9_0_g675 - 1.0 ) / temp_output_8_0_g675 ) ) ) * ( step( texCoord2_g675.y , ( temp_output_9_0_g675 / temp_output_8_0_g675 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g670.x , ( ( temp_output_3_0_g670 - 1.0 ) / temp_output_7_0_g670 ) ) ) * ( step( texCoord2_g670.x , ( temp_output_3_0_g670 / temp_output_7_0_g670 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g670.y , ( ( temp_output_9_0_g670 - 1.0 ) / temp_output_8_0_g670 ) ) ) * ( step( texCoord2_g670.y , ( temp_output_9_0_g670 / temp_output_8_0_g670 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g672.x , ( ( temp_output_3_0_g672 - 1.0 ) / temp_output_7_0_g672 ) ) ) * ( step( texCoord2_g672.x , ( temp_output_3_0_g672 / temp_output_7_0_g672 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g672.y , ( ( temp_output_9_0_g672 - 1.0 ) / temp_output_8_0_g672 ) ) ) * ( step( texCoord2_g672.y , ( temp_output_9_0_g672 / temp_output_8_0_g672 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( texCoord2_g661.x , ( ( temp_output_3_0_g661 - 1.0 ) / temp_output_7_0_g661 ) ) ) * ( step( texCoord2_g661.x , ( temp_output_3_0_g661 / temp_output_7_0_g661 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g661.y , ( ( temp_output_9_0_g661 - 1.0 ) / temp_output_8_0_g661 ) ) ) * ( step( texCoord2_g661.y , ( temp_output_9_0_g661 / temp_output_8_0_g661 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( texCoord2_g668.x , ( ( temp_output_3_0_g668 - 1.0 ) / temp_output_7_0_g668 ) ) ) * ( step( texCoord2_g668.x , ( temp_output_3_0_g668 / temp_output_7_0_g668 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g668.y , ( ( temp_output_9_0_g668 - 1.0 ) / temp_output_8_0_g668 ) ) ) * ( step( texCoord2_g668.y , ( temp_output_9_0_g668 / temp_output_8_0_g668 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( texCoord2_g682.x , ( ( temp_output_3_0_g682 - 1.0 ) / temp_output_7_0_g682 ) ) ) * ( step( texCoord2_g682.x , ( temp_output_3_0_g682 / temp_output_7_0_g682 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g682.y , ( ( temp_output_9_0_g682 - 1.0 ) / temp_output_8_0_g682 ) ) ) * ( step( texCoord2_g682.y , ( temp_output_9_0_g682 / temp_output_8_0_g682 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( texCoord2_g673.x , ( ( temp_output_3_0_g673 - 1.0 ) / temp_output_7_0_g673 ) ) ) * ( step( texCoord2_g673.x , ( temp_output_3_0_g673 / temp_output_7_0_g673 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g673.y , ( ( temp_output_9_0_g673 - 1.0 ) / temp_output_8_0_g673 ) ) ) * ( step( texCoord2_g673.y , ( temp_output_9_0_g673 / temp_output_8_0_g673 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( texCoord2_g678.x , ( ( temp_output_3_0_g678 - 1.0 ) / temp_output_7_0_g678 ) ) ) * ( step( texCoord2_g678.x , ( temp_output_3_0_g678 / temp_output_7_0_g678 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g678.y , ( ( temp_output_9_0_g678 - 1.0 ) / temp_output_8_0_g678 ) ) ) * ( step( texCoord2_g678.y , ( temp_output_9_0_g678 / temp_output_8_0_g678 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( texCoord2_g674.x , ( ( temp_output_3_0_g674 - 1.0 ) / temp_output_7_0_g674 ) ) ) * ( step( texCoord2_g674.x , ( temp_output_3_0_g674 / temp_output_7_0_g674 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g674.y , ( ( temp_output_9_0_g674 - 1.0 ) / temp_output_8_0_g674 ) ) ) * ( step( texCoord2_g674.y , ( temp_output_9_0_g674 / temp_output_8_0_g674 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult261 = clamp( ( pow( (clampResult234*_GradientScale + _GradientOffset) , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float2 texCoord2_g701 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g701 = 1.0;
				float temp_output_7_0_g701 = 4.0;
				float temp_output_9_0_g701 = 4.0;
				float temp_output_8_0_g701 = 4.0;
				float2 texCoord2_g706 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g706 = 2.0;
				float temp_output_7_0_g706 = 4.0;
				float temp_output_9_0_g706 = 4.0;
				float temp_output_8_0_g706 = 4.0;
				float2 texCoord2_g700 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g700 = 3.0;
				float temp_output_7_0_g700 = 4.0;
				float temp_output_9_0_g700 = 4.0;
				float temp_output_8_0_g700 = 4.0;
				float2 texCoord2_g698 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g698 = 4.0;
				float temp_output_7_0_g698 = 4.0;
				float temp_output_9_0_g698 = 4.0;
				float temp_output_8_0_g698 = 4.0;
				float2 texCoord2_g699 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g699 = 1.0;
				float temp_output_7_0_g699 = 4.0;
				float temp_output_9_0_g699 = 3.0;
				float temp_output_8_0_g699 = 4.0;
				float2 texCoord2_g696 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g696 = 2.0;
				float temp_output_7_0_g696 = 4.0;
				float temp_output_9_0_g696 = 3.0;
				float temp_output_8_0_g696 = 4.0;
				float2 texCoord2_g707 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g707 = 3.0;
				float temp_output_7_0_g707 = 4.0;
				float temp_output_9_0_g707 = 3.0;
				float temp_output_8_0_g707 = 4.0;
				float2 texCoord2_g695 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g695 = 4.0;
				float temp_output_7_0_g695 = 4.0;
				float temp_output_9_0_g695 = 3.0;
				float temp_output_8_0_g695 = 4.0;
				float2 texCoord2_g704 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g704 = 1.0;
				float temp_output_7_0_g704 = 4.0;
				float temp_output_9_0_g704 = 2.0;
				float temp_output_8_0_g704 = 4.0;
				float2 texCoord2_g697 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g697 = 2.0;
				float temp_output_7_0_g697 = 4.0;
				float temp_output_9_0_g697 = 2.0;
				float temp_output_8_0_g697 = 4.0;
				float2 texCoord2_g702 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g702 = 3.0;
				float temp_output_7_0_g702 = 4.0;
				float temp_output_9_0_g702 = 2.0;
				float temp_output_8_0_g702 = 4.0;
				float2 texCoord2_g705 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g705 = 4.0;
				float temp_output_7_0_g705 = 4.0;
				float temp_output_9_0_g705 = 2.0;
				float temp_output_8_0_g705 = 4.0;
				float2 texCoord2_g685 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g685 = 1.0;
				float temp_output_7_0_g685 = 4.0;
				float temp_output_9_0_g685 = 1.0;
				float temp_output_8_0_g685 = 4.0;
				float2 texCoord2_g694 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g694 = 2.0;
				float temp_output_7_0_g694 = 4.0;
				float temp_output_9_0_g694 = 1.0;
				float temp_output_8_0_g694 = 4.0;
				float2 texCoord2_g693 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g693 = 3.0;
				float temp_output_7_0_g693 = 4.0;
				float temp_output_9_0_g693 = 1.0;
				float temp_output_8_0_g693 = 4.0;
				float2 texCoord2_g703 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g703 = 4.0;
				float temp_output_7_0_g703 = 4.0;
				float temp_output_9_0_g703 = 1.0;
				float temp_output_8_0_g703 = 4.0;
				float4 temp_output_283_0 = ( ( ( _MRE1 * ( ( ( 1.0 - step( texCoord2_g701.x , ( ( temp_output_3_0_g701 - 1.0 ) / temp_output_7_0_g701 ) ) ) * ( step( texCoord2_g701.x , ( temp_output_3_0_g701 / temp_output_7_0_g701 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g701.y , ( ( temp_output_9_0_g701 - 1.0 ) / temp_output_8_0_g701 ) ) ) * ( step( texCoord2_g701.y , ( temp_output_9_0_g701 / temp_output_8_0_g701 ) ) * 1.0 ) ) ) ) + ( _MRE2 * ( ( ( 1.0 - step( texCoord2_g706.x , ( ( temp_output_3_0_g706 - 1.0 ) / temp_output_7_0_g706 ) ) ) * ( step( texCoord2_g706.x , ( temp_output_3_0_g706 / temp_output_7_0_g706 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g706.y , ( ( temp_output_9_0_g706 - 1.0 ) / temp_output_8_0_g706 ) ) ) * ( step( texCoord2_g706.y , ( temp_output_9_0_g706 / temp_output_8_0_g706 ) ) * 1.0 ) ) ) ) + ( _MRE3 * ( ( ( 1.0 - step( texCoord2_g700.x , ( ( temp_output_3_0_g700 - 1.0 ) / temp_output_7_0_g700 ) ) ) * ( step( texCoord2_g700.x , ( temp_output_3_0_g700 / temp_output_7_0_g700 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g700.y , ( ( temp_output_9_0_g700 - 1.0 ) / temp_output_8_0_g700 ) ) ) * ( step( texCoord2_g700.y , ( temp_output_9_0_g700 / temp_output_8_0_g700 ) ) * 1.0 ) ) ) ) + ( _MRE4 * ( ( ( 1.0 - step( texCoord2_g698.x , ( ( temp_output_3_0_g698 - 1.0 ) / temp_output_7_0_g698 ) ) ) * ( step( texCoord2_g698.x , ( temp_output_3_0_g698 / temp_output_7_0_g698 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g698.y , ( ( temp_output_9_0_g698 - 1.0 ) / temp_output_8_0_g698 ) ) ) * ( step( texCoord2_g698.y , ( temp_output_9_0_g698 / temp_output_8_0_g698 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE5 * ( ( ( 1.0 - step( texCoord2_g699.x , ( ( temp_output_3_0_g699 - 1.0 ) / temp_output_7_0_g699 ) ) ) * ( step( texCoord2_g699.x , ( temp_output_3_0_g699 / temp_output_7_0_g699 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g699.y , ( ( temp_output_9_0_g699 - 1.0 ) / temp_output_8_0_g699 ) ) ) * ( step( texCoord2_g699.y , ( temp_output_9_0_g699 / temp_output_8_0_g699 ) ) * 1.0 ) ) ) ) + ( _MRE6 * ( ( ( 1.0 - step( texCoord2_g696.x , ( ( temp_output_3_0_g696 - 1.0 ) / temp_output_7_0_g696 ) ) ) * ( step( texCoord2_g696.x , ( temp_output_3_0_g696 / temp_output_7_0_g696 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g696.y , ( ( temp_output_9_0_g696 - 1.0 ) / temp_output_8_0_g696 ) ) ) * ( step( texCoord2_g696.y , ( temp_output_9_0_g696 / temp_output_8_0_g696 ) ) * 1.0 ) ) ) ) + ( _MRE7 * ( ( ( 1.0 - step( texCoord2_g707.x , ( ( temp_output_3_0_g707 - 1.0 ) / temp_output_7_0_g707 ) ) ) * ( step( texCoord2_g707.x , ( temp_output_3_0_g707 / temp_output_7_0_g707 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g707.y , ( ( temp_output_9_0_g707 - 1.0 ) / temp_output_8_0_g707 ) ) ) * ( step( texCoord2_g707.y , ( temp_output_9_0_g707 / temp_output_8_0_g707 ) ) * 1.0 ) ) ) ) + ( _MRE8 * ( ( ( 1.0 - step( texCoord2_g695.x , ( ( temp_output_3_0_g695 - 1.0 ) / temp_output_7_0_g695 ) ) ) * ( step( texCoord2_g695.x , ( temp_output_3_0_g695 / temp_output_7_0_g695 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g695.y , ( ( temp_output_9_0_g695 - 1.0 ) / temp_output_8_0_g695 ) ) ) * ( step( texCoord2_g695.y , ( temp_output_9_0_g695 / temp_output_8_0_g695 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE9 * ( ( ( 1.0 - step( texCoord2_g704.x , ( ( temp_output_3_0_g704 - 1.0 ) / temp_output_7_0_g704 ) ) ) * ( step( texCoord2_g704.x , ( temp_output_3_0_g704 / temp_output_7_0_g704 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g704.y , ( ( temp_output_9_0_g704 - 1.0 ) / temp_output_8_0_g704 ) ) ) * ( step( texCoord2_g704.y , ( temp_output_9_0_g704 / temp_output_8_0_g704 ) ) * 1.0 ) ) ) ) + ( _MRE10 * ( ( ( 1.0 - step( texCoord2_g697.x , ( ( temp_output_3_0_g697 - 1.0 ) / temp_output_7_0_g697 ) ) ) * ( step( texCoord2_g697.x , ( temp_output_3_0_g697 / temp_output_7_0_g697 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g697.y , ( ( temp_output_9_0_g697 - 1.0 ) / temp_output_8_0_g697 ) ) ) * ( step( texCoord2_g697.y , ( temp_output_9_0_g697 / temp_output_8_0_g697 ) ) * 1.0 ) ) ) ) + ( _MRE11 * ( ( ( 1.0 - step( texCoord2_g702.x , ( ( temp_output_3_0_g702 - 1.0 ) / temp_output_7_0_g702 ) ) ) * ( step( texCoord2_g702.x , ( temp_output_3_0_g702 / temp_output_7_0_g702 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g702.y , ( ( temp_output_9_0_g702 - 1.0 ) / temp_output_8_0_g702 ) ) ) * ( step( texCoord2_g702.y , ( temp_output_9_0_g702 / temp_output_8_0_g702 ) ) * 1.0 ) ) ) ) + ( _MRE12 * ( ( ( 1.0 - step( texCoord2_g705.x , ( ( temp_output_3_0_g705 - 1.0 ) / temp_output_7_0_g705 ) ) ) * ( step( texCoord2_g705.x , ( temp_output_3_0_g705 / temp_output_7_0_g705 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g705.y , ( ( temp_output_9_0_g705 - 1.0 ) / temp_output_8_0_g705 ) ) ) * ( step( texCoord2_g705.y , ( temp_output_9_0_g705 / temp_output_8_0_g705 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE13 * ( ( ( 1.0 - step( texCoord2_g685.x , ( ( temp_output_3_0_g685 - 1.0 ) / temp_output_7_0_g685 ) ) ) * ( step( texCoord2_g685.x , ( temp_output_3_0_g685 / temp_output_7_0_g685 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g685.y , ( ( temp_output_9_0_g685 - 1.0 ) / temp_output_8_0_g685 ) ) ) * ( step( texCoord2_g685.y , ( temp_output_9_0_g685 / temp_output_8_0_g685 ) ) * 1.0 ) ) ) ) + ( _MRE14 * ( ( ( 1.0 - step( texCoord2_g694.x , ( ( temp_output_3_0_g694 - 1.0 ) / temp_output_7_0_g694 ) ) ) * ( step( texCoord2_g694.x , ( temp_output_3_0_g694 / temp_output_7_0_g694 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g694.y , ( ( temp_output_9_0_g694 - 1.0 ) / temp_output_8_0_g694 ) ) ) * ( step( texCoord2_g694.y , ( temp_output_9_0_g694 / temp_output_8_0_g694 ) ) * 1.0 ) ) ) ) + ( _MRE15 * ( ( ( 1.0 - step( texCoord2_g693.x , ( ( temp_output_3_0_g693 - 1.0 ) / temp_output_7_0_g693 ) ) ) * ( step( texCoord2_g693.x , ( temp_output_3_0_g693 / temp_output_7_0_g693 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g693.y , ( ( temp_output_9_0_g693 - 1.0 ) / temp_output_8_0_g693 ) ) ) * ( step( texCoord2_g693.y , ( temp_output_9_0_g693 / temp_output_8_0_g693 ) ) * 1.0 ) ) ) ) + ( _MRE16 * ( ( ( 1.0 - step( texCoord2_g703.x , ( ( temp_output_3_0_g703 - 1.0 ) / temp_output_7_0_g703 ) ) ) * ( step( texCoord2_g703.x , ( temp_output_3_0_g703 / temp_output_7_0_g703 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g703.y , ( ( temp_output_9_0_g703 - 1.0 ) / temp_output_8_0_g703 ) ) ) * ( step( texCoord2_g703.y , ( temp_output_9_0_g703 / temp_output_8_0_g703 ) ) * 1.0 ) ) ) ) ) );
				
				
				float3 Albedo = ( clampResult261 * temp_output_155_0 ).rgb;
				float3 Emission = ( temp_output_155_0 * ( _EmissionPower1 * (temp_output_283_0).b ) ).rgb;
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
			#define ASE_SRP_VERSION 999999

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
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
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

				float2 texCoord255 = IN.ase_texcoord2.xy * float2( 1,4 ) + float2( 0,0 );
				float4 clampResult234 = clamp( ( ( tex2D( _Gradient, texCoord255 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g680 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g680 = 1.0;
				float temp_output_7_0_g680 = 4.0;
				float temp_output_9_0_g680 = 4.0;
				float temp_output_8_0_g680 = 4.0;
				float2 texCoord2_g676 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g676 = 2.0;
				float temp_output_7_0_g676 = 4.0;
				float temp_output_9_0_g676 = 4.0;
				float temp_output_8_0_g676 = 4.0;
				float2 texCoord2_g677 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g677 = 3.0;
				float temp_output_7_0_g677 = 4.0;
				float temp_output_9_0_g677 = 4.0;
				float temp_output_8_0_g677 = 4.0;
				float2 texCoord2_g679 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g679 = 4.0;
				float temp_output_7_0_g679 = 4.0;
				float temp_output_9_0_g679 = 4.0;
				float temp_output_8_0_g679 = 4.0;
				float2 texCoord2_g669 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g669 = 1.0;
				float temp_output_7_0_g669 = 4.0;
				float temp_output_9_0_g669 = 3.0;
				float temp_output_8_0_g669 = 4.0;
				float2 texCoord2_g671 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g671 = 2.0;
				float temp_output_7_0_g671 = 4.0;
				float temp_output_9_0_g671 = 3.0;
				float temp_output_8_0_g671 = 4.0;
				float2 texCoord2_g681 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g681 = 3.0;
				float temp_output_7_0_g681 = 4.0;
				float temp_output_9_0_g681 = 3.0;
				float temp_output_8_0_g681 = 4.0;
				float2 texCoord2_g675 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g675 = 4.0;
				float temp_output_7_0_g675 = 4.0;
				float temp_output_9_0_g675 = 3.0;
				float temp_output_8_0_g675 = 4.0;
				float2 texCoord2_g670 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g670 = 1.0;
				float temp_output_7_0_g670 = 4.0;
				float temp_output_9_0_g670 = 2.0;
				float temp_output_8_0_g670 = 4.0;
				float2 texCoord2_g672 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g672 = 2.0;
				float temp_output_7_0_g672 = 4.0;
				float temp_output_9_0_g672 = 2.0;
				float temp_output_8_0_g672 = 4.0;
				float2 texCoord2_g661 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g661 = 3.0;
				float temp_output_7_0_g661 = 4.0;
				float temp_output_9_0_g661 = 2.0;
				float temp_output_8_0_g661 = 4.0;
				float2 texCoord2_g668 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g668 = 4.0;
				float temp_output_7_0_g668 = 4.0;
				float temp_output_9_0_g668 = 2.0;
				float temp_output_8_0_g668 = 4.0;
				float2 texCoord2_g682 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g682 = 1.0;
				float temp_output_7_0_g682 = 4.0;
				float temp_output_9_0_g682 = 1.0;
				float temp_output_8_0_g682 = 4.0;
				float2 texCoord2_g673 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g673 = 2.0;
				float temp_output_7_0_g673 = 4.0;
				float temp_output_9_0_g673 = 1.0;
				float temp_output_8_0_g673 = 4.0;
				float2 texCoord2_g678 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g678 = 3.0;
				float temp_output_7_0_g678 = 4.0;
				float temp_output_9_0_g678 = 1.0;
				float temp_output_8_0_g678 = 4.0;
				float2 texCoord2_g674 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g674 = 4.0;
				float temp_output_7_0_g674 = 4.0;
				float temp_output_9_0_g674 = 1.0;
				float temp_output_8_0_g674 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g680.x , ( ( temp_output_3_0_g680 - 1.0 ) / temp_output_7_0_g680 ) ) ) * ( step( texCoord2_g680.x , ( temp_output_3_0_g680 / temp_output_7_0_g680 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g680.y , ( ( temp_output_9_0_g680 - 1.0 ) / temp_output_8_0_g680 ) ) ) * ( step( texCoord2_g680.y , ( temp_output_9_0_g680 / temp_output_8_0_g680 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g676.x , ( ( temp_output_3_0_g676 - 1.0 ) / temp_output_7_0_g676 ) ) ) * ( step( texCoord2_g676.x , ( temp_output_3_0_g676 / temp_output_7_0_g676 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g676.y , ( ( temp_output_9_0_g676 - 1.0 ) / temp_output_8_0_g676 ) ) ) * ( step( texCoord2_g676.y , ( temp_output_9_0_g676 / temp_output_8_0_g676 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g677.x , ( ( temp_output_3_0_g677 - 1.0 ) / temp_output_7_0_g677 ) ) ) * ( step( texCoord2_g677.x , ( temp_output_3_0_g677 / temp_output_7_0_g677 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g677.y , ( ( temp_output_9_0_g677 - 1.0 ) / temp_output_8_0_g677 ) ) ) * ( step( texCoord2_g677.y , ( temp_output_9_0_g677 / temp_output_8_0_g677 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g679.x , ( ( temp_output_3_0_g679 - 1.0 ) / temp_output_7_0_g679 ) ) ) * ( step( texCoord2_g679.x , ( temp_output_3_0_g679 / temp_output_7_0_g679 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g679.y , ( ( temp_output_9_0_g679 - 1.0 ) / temp_output_8_0_g679 ) ) ) * ( step( texCoord2_g679.y , ( temp_output_9_0_g679 / temp_output_8_0_g679 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g669.x , ( ( temp_output_3_0_g669 - 1.0 ) / temp_output_7_0_g669 ) ) ) * ( step( texCoord2_g669.x , ( temp_output_3_0_g669 / temp_output_7_0_g669 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g669.y , ( ( temp_output_9_0_g669 - 1.0 ) / temp_output_8_0_g669 ) ) ) * ( step( texCoord2_g669.y , ( temp_output_9_0_g669 / temp_output_8_0_g669 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g671.x , ( ( temp_output_3_0_g671 - 1.0 ) / temp_output_7_0_g671 ) ) ) * ( step( texCoord2_g671.x , ( temp_output_3_0_g671 / temp_output_7_0_g671 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g671.y , ( ( temp_output_9_0_g671 - 1.0 ) / temp_output_8_0_g671 ) ) ) * ( step( texCoord2_g671.y , ( temp_output_9_0_g671 / temp_output_8_0_g671 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g681.x , ( ( temp_output_3_0_g681 - 1.0 ) / temp_output_7_0_g681 ) ) ) * ( step( texCoord2_g681.x , ( temp_output_3_0_g681 / temp_output_7_0_g681 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g681.y , ( ( temp_output_9_0_g681 - 1.0 ) / temp_output_8_0_g681 ) ) ) * ( step( texCoord2_g681.y , ( temp_output_9_0_g681 / temp_output_8_0_g681 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g675.x , ( ( temp_output_3_0_g675 - 1.0 ) / temp_output_7_0_g675 ) ) ) * ( step( texCoord2_g675.x , ( temp_output_3_0_g675 / temp_output_7_0_g675 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g675.y , ( ( temp_output_9_0_g675 - 1.0 ) / temp_output_8_0_g675 ) ) ) * ( step( texCoord2_g675.y , ( temp_output_9_0_g675 / temp_output_8_0_g675 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g670.x , ( ( temp_output_3_0_g670 - 1.0 ) / temp_output_7_0_g670 ) ) ) * ( step( texCoord2_g670.x , ( temp_output_3_0_g670 / temp_output_7_0_g670 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g670.y , ( ( temp_output_9_0_g670 - 1.0 ) / temp_output_8_0_g670 ) ) ) * ( step( texCoord2_g670.y , ( temp_output_9_0_g670 / temp_output_8_0_g670 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g672.x , ( ( temp_output_3_0_g672 - 1.0 ) / temp_output_7_0_g672 ) ) ) * ( step( texCoord2_g672.x , ( temp_output_3_0_g672 / temp_output_7_0_g672 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g672.y , ( ( temp_output_9_0_g672 - 1.0 ) / temp_output_8_0_g672 ) ) ) * ( step( texCoord2_g672.y , ( temp_output_9_0_g672 / temp_output_8_0_g672 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( texCoord2_g661.x , ( ( temp_output_3_0_g661 - 1.0 ) / temp_output_7_0_g661 ) ) ) * ( step( texCoord2_g661.x , ( temp_output_3_0_g661 / temp_output_7_0_g661 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g661.y , ( ( temp_output_9_0_g661 - 1.0 ) / temp_output_8_0_g661 ) ) ) * ( step( texCoord2_g661.y , ( temp_output_9_0_g661 / temp_output_8_0_g661 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( texCoord2_g668.x , ( ( temp_output_3_0_g668 - 1.0 ) / temp_output_7_0_g668 ) ) ) * ( step( texCoord2_g668.x , ( temp_output_3_0_g668 / temp_output_7_0_g668 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g668.y , ( ( temp_output_9_0_g668 - 1.0 ) / temp_output_8_0_g668 ) ) ) * ( step( texCoord2_g668.y , ( temp_output_9_0_g668 / temp_output_8_0_g668 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( texCoord2_g682.x , ( ( temp_output_3_0_g682 - 1.0 ) / temp_output_7_0_g682 ) ) ) * ( step( texCoord2_g682.x , ( temp_output_3_0_g682 / temp_output_7_0_g682 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g682.y , ( ( temp_output_9_0_g682 - 1.0 ) / temp_output_8_0_g682 ) ) ) * ( step( texCoord2_g682.y , ( temp_output_9_0_g682 / temp_output_8_0_g682 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( texCoord2_g673.x , ( ( temp_output_3_0_g673 - 1.0 ) / temp_output_7_0_g673 ) ) ) * ( step( texCoord2_g673.x , ( temp_output_3_0_g673 / temp_output_7_0_g673 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g673.y , ( ( temp_output_9_0_g673 - 1.0 ) / temp_output_8_0_g673 ) ) ) * ( step( texCoord2_g673.y , ( temp_output_9_0_g673 / temp_output_8_0_g673 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( texCoord2_g678.x , ( ( temp_output_3_0_g678 - 1.0 ) / temp_output_7_0_g678 ) ) ) * ( step( texCoord2_g678.x , ( temp_output_3_0_g678 / temp_output_7_0_g678 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g678.y , ( ( temp_output_9_0_g678 - 1.0 ) / temp_output_8_0_g678 ) ) ) * ( step( texCoord2_g678.y , ( temp_output_9_0_g678 / temp_output_8_0_g678 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( texCoord2_g674.x , ( ( temp_output_3_0_g674 - 1.0 ) / temp_output_7_0_g674 ) ) ) * ( step( texCoord2_g674.x , ( temp_output_3_0_g674 / temp_output_7_0_g674 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g674.y , ( ( temp_output_9_0_g674 - 1.0 ) / temp_output_8_0_g674 ) ) ) * ( step( texCoord2_g674.y , ( temp_output_9_0_g674 / temp_output_8_0_g674 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult261 = clamp( ( pow( (clampResult234*_GradientScale + _GradientOffset) , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				
				float3 Albedo = ( clampResult261 * temp_output_155_0 ).rgb;
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
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18707
-1536;62.6;1536;807;-3004.957;-396.7884;1;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;255;615.0826,-371.7701;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,4;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;150;-207.7412,-66.93771;Float;False;Property;_Color2;Color 2;1;0;Create;True;0;0;False;0;False;1,0.1544118,0.8017241,0.253;0.6132076,0.6132076,0.6132076,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;183;-224.4024,1681.061;Float;False;Property;_Color9;Color 9;8;0;Create;True;0;0;False;1;Space(10);False;0.3164301,0,0.7058823,0.134;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;217;-264.3738,3419.386;Float;False;Property;_Color16;Color 16;15;0;Create;True;0;0;False;0;False;0.4080882,0.75,0.4811866,0.134;0.5849056,0.5849056,0.5849056,0.547;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;156;-195.9079,947.3851;Float;False;Property;_Color6;Color 6;5;0;Create;True;0;0;False;0;False;0.2720588,0.1294625,0,0.097;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;154;-195.6228,411.2479;Float;False;Property;_Color4;Color 4;3;0;Create;True;0;0;False;0;False;0.1544118,0.5451319,1,0.253;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;228;845.7399,12.46821;Float;False;Property;_GradientIntensity;Gradient Intensity;34;0;Create;True;0;0;False;0;False;0.75;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;180;-232.3431,1940.419;Float;False;Property;_Color10;Color 10;9;0;Create;True;0;0;False;0;False;0.362069,0.4411765,0,0.759;0.5514706,0.5514706,0.5514706,0.591;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;157;-182.3802,1181.25;Float;False;Property;_Color7;Color 7;6;0;Create;True;0;0;False;0;False;0.1544118,0.6151115,1,0.178;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;218;-229.103,3176.23;Float;False;Property;_Color15;Color 15;14;0;Create;True;0;0;False;0;False;1,0,0,0.391;0.5188679,0.4068953,0.07097723,0.847;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;182;-220.2247,2417.44;Float;False;Property;_Color12;Color 12;11;0;Create;True;0;0;False;0;False;0.5073529,0.1574544,0,0.128;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;229;874.0561,-170.8387;Float;False;Property;_GradientColor;Gradient Color;35;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-199.8005,-326.2955;Float;False;Property;_Color1;Color 1;0;0;Create;True;0;0;False;1;Header(Albedo (A Gradient));False;1,0.1544118,0.1544118,0.291;0.6838235,0.6476211,0.5933174,0.7803922;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;213;-234.6901,2683.007;Float;False;Property;_Color13;Color 13;12;0;Create;True;0;0;False;1;Space(10);False;1,0.5586207,0,0.272;0.6132076,0.6132076,0.6132076,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;230;859.2263,-398.1579;Inherit;True;Property;_Gradient;Gradient;33;1;[SingleLineTexture];Create;True;0;0;False;1;Header(Gradient);False;-1;0f424a347039ef447a763d3d4b4782b0;0f424a347039ef447a763d3d4b4782b0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;214;-242.6307,2942.365;Float;False;Property;_Color14;Color 14;13;0;Create;True;0;0;False;0;False;0,0.8025862,0.875,0.047;0.5441177,0.5441177,0.5441177,0.047;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;159;-187.9672,688.0273;Float;False;Property;_Color5;Color 5;4;0;Create;True;0;0;False;1;Space(10);False;0.9533468,1,0.1544118,0.553;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;158;-183.7895,1424.406;Float;False;Property;_Color8;Color 8;7;0;Create;True;0;0;False;0;False;0.4849697,0.5008695,0.5073529,0.078;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;181;-218.8154,2174.284;Float;False;Property;_Color11;Color 11;10;0;Create;True;0;0;False;0;False;0.6691177,0.6691177,0.6691177,0.647;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;152;-194.2135,166.9271;Float;False;Property;_Color3;Color 3;2;0;Create;True;0;0;False;0;False;0.2535501,0.1544118,1,0.541;0.6838235,0.6476211,0.5933174,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;151;121.5042,171.7677;Inherit;True;ColorShartSlot;-1;;677;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;216;81.02762,2687.848;Inherit;True;ColorShartSlot;-1;;682;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;161;133.3375,1186.091;Inherit;True;ColorShartSlot;-1;;681;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;145;115.9171,-321.4549;Inherit;True;ColorShartSlot;-1;;680;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;231;1182.122,-372.6908;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;153;122.0185,414.924;Inherit;True;ColorShartSlot;-1;;679;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;224;86.61465,3181.071;Inherit;True;ColorShartSlot;-1;;678;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;149;107.9764,-62.09709;Inherit;True;ColorShartSlot;-1;;676;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;232;1156.605,-68.40891;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;222;87.12894,3424.227;Inherit;True;ColorShartSlot;-1;;674;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;223;73.08682,2945.046;Inherit;True;ColorShartSlot;-1;;673;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;187;83.37437,1945.26;Inherit;True;ColorShartSlot;-1;;672;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;160;119.8096,952.2258;Inherit;True;ColorShartSlot;-1;;671;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;188;91.31517,1685.902;Inherit;True;ColorShartSlot;-1;;670;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;163;127.7504,692.868;Inherit;True;ColorShartSlot;-1;;669;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;185;97.41646,2422.281;Inherit;True;ColorShartSlot;-1;;668;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;186;96.90227,2179.125;Inherit;True;ColorShartSlot;-1;;661;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;162;133.8517,1429.247;Inherit;True;ColorShartSlot;-1;;675;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;233;1367.421,-383.9108;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;225;683.3512,1524.765;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;688.9302,993.4156;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;184;686.7443,1260.558;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;146;688.2412,727.387;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;239;1655.499,-42.52599;Float;False;Property;_GradientOffset;Gradient Offset;37;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;238;1648.681,-135.2692;Float;False;Property;_GradientScale;Gradient Scale;36;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;234;1652.17,-392.4709;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;1016.686,1030.498;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;237;1929.951,-353.3528;Inherit;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;256;2060.061,-74.53971;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;245;1659.99,53.90989;Float;False;Property;_GradientPower;Gradient Power;38;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;259;2282.192,-66.26985;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;258;2237.497,-354.0456;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;260;2530.138,-355.3468;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;261;2796.672,-358.3472;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;305;2944.843,925.5689;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;298;1185.632,5764.37;Inherit;True;ColorShartSlot;-1;;703;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;272;1173.398,4034.921;Inherit;True;ColorShartSlot;-1;;704;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;286;1180.335,4692.998;Inherit;True;ColorShartSlot;-1;;705;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;271;1202.304,2381.9;Inherit;True;ColorShartSlot;-1;;706;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;279;1189.043,3496.534;Inherit;True;ColorShartSlot;-1;;707;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;299;1571.928,5274.19;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;280;1566.631,4202.819;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;282;1553.285,3234.033;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;304;3114.618,1152.562;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;283;1960.203,2821.543;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;302;2524.654,1230.847;Inherit;False;Property;_EmissionPower1;Emission Power;32;0;Create;True;0;0;False;1;Header(Emmision);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;301;2514.004,1365.87;Inherit;True;False;False;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;307;2570.26,731.6305;Inherit;True;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;306;2576.238,929.5774;Inherit;True;False;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;303;2828.549,1217.506;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;236;3433.932,229.384;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;281;1569.768,2500.448;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;284;2918.832,2649.558;Inherit;True;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;263;924.805,2591.167;Float;False;Property;_MRE3;MRE 3;18;0;Create;True;0;0;False;0;False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;275;1204.517,2165.975;Inherit;True;ColorShartSlot;-1;;701;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;293;888.8267,5554.006;Float;False;Property;_MRE15;MRE 15;30;0;Create;True;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;292;886.7413,5342.351;Float;False;Property;_MRE14;MRE 14;29;0;Create;True;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;287;879.1714,4698.762;Float;False;Property;_MRE12;MRE 12;27;0;Create;True;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;294;884.4691,5770.134;Float;False;Property;_MRE16;MRE 16;31;0;Create;True;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;288;881.2148,4482.635;Float;False;Property;_MRE11;MRE 11;26;0;Create;True;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;262;888.3417,3297.014;Float;False;Property;_MRE6;MRE 6;21;0;Create;True;0;0;False;0;False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;265;928.9007,2379.895;Float;False;Property;_MRE2;MRE 2;17;0;Create;True;0;0;False;0;False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;269;876.3038,4037.208;Float;False;Property;_MRE9;MRE 9;24;0;Create;True;0;0;False;1;Space(10);False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;291;881.6017,5108.58;Float;False;Property;_MRE13;MRE 13;28;0;Create;True;0;0;False;1;Space(10);False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;285;1189.334,4470.5;Inherit;True;ColorShartSlot;-1;;702;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;290;881.4436,4270.979;Float;False;Property;_MRE10;MRE 10;25;0;Create;True;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;267;883.4818,3508.669;Float;False;Property;_MRE7;MRE 7;22;0;Create;True;0;0;False;0;False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;264;887.6243,3070.586;Float;False;Property;_MRE5;MRE 5;20;0;Create;True;0;0;False;1;Space(10);False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;268;920.3644,2166.243;Float;False;Property;_MRE1;MRE 1;16;0;Create;True;0;0;False;1;Header(Metallic(R) Rough(G) Emmission(B));False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;274;1200.378,2592.315;Inherit;True;ColorShartSlot;-1;;700;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;278;1189.738,3073.549;Inherit;True;ColorShartSlot;-1;;699;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;277;1200.345,2802.812;Inherit;True;ColorShartSlot;-1;;698;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;270;881.4384,3724.796;Float;False;Property;_MRE8;MRE 8;23;0;Create;True;0;0;False;0;False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;273;1187.369,3281.62;Inherit;True;ColorShartSlot;-1;;696;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;289;1187.66,4255.586;Inherit;True;ColorShartSlot;-1;;697;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;296;1192.958,5326.958;Inherit;True;ColorShartSlot;-1;;694;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;297;1194.631,5541.872;Inherit;True;ColorShartSlot;-1;;693;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;295;1178.695,5106.292;Inherit;True;ColorShartSlot;-1;;685;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;266;927.7203,2799.667;Float;False;Property;_MRE4;MRE 4;19;0;Create;True;0;0;False;0;False;0,1,0,0;0.9098039,0.8666667,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;276;1182.602,3719.032;Inherit;True;ColorShartSlot;-1;;695;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;312;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;308;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;309;4040.206,769.1205;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Malbers/Color4x4v2;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;17;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;36;Workflow;1;Surface;0;  Refraction Model;0;  Blend;0;Two Sided;1;Fragment Normal Space,InvertActionOnDeselection;0;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;1;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Vertex Position,InvertActionOnDeselection;1;0;6;False;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;310;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;311;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;False;False;False;False;0;False;-1;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;313;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;230;1;255;0
WireConnection;151;38;152;0
WireConnection;216;38;213;0
WireConnection;161;38;157;0
WireConnection;145;38;23;0
WireConnection;231;0;230;0
WireConnection;231;1;229;0
WireConnection;153;38;154;0
WireConnection;224;38;218;0
WireConnection;149;38;150;0
WireConnection;232;0;228;0
WireConnection;222;38;217;0
WireConnection;223;38;214;0
WireConnection;187;38;180;0
WireConnection;160;38;156;0
WireConnection;188;38;183;0
WireConnection;163;38;159;0
WireConnection;185;38;182;0
WireConnection;186;38;181;0
WireConnection;162;38;158;0
WireConnection;233;0;231;0
WireConnection;233;1;232;0
WireConnection;225;0;216;0
WireConnection;225;1;223;0
WireConnection;225;2;224;0
WireConnection;225;3;222;0
WireConnection;164;0;163;0
WireConnection;164;1;160;0
WireConnection;164;2;161;0
WireConnection;164;3;162;0
WireConnection;184;0;188;0
WireConnection;184;1;187;0
WireConnection;184;2;186;0
WireConnection;184;3;185;0
WireConnection;146;0;145;0
WireConnection;146;1;149;0
WireConnection;146;2;151;0
WireConnection;146;3;153;0
WireConnection;234;0;233;0
WireConnection;155;0;146;0
WireConnection;155;1;164;0
WireConnection;155;2;184;0
WireConnection;155;3;225;0
WireConnection;237;0;234;0
WireConnection;237;1;238;0
WireConnection;237;2;239;0
WireConnection;256;0;155;0
WireConnection;259;0;256;0
WireConnection;258;0;237;0
WireConnection;258;1;245;0
WireConnection;260;0;258;0
WireConnection;260;1;259;0
WireConnection;261;0;260;0
WireConnection;305;0;306;0
WireConnection;298;38;294;0
WireConnection;272;38;269;0
WireConnection;286;38;287;0
WireConnection;271;38;265;0
WireConnection;279;38;267;0
WireConnection;299;0;295;0
WireConnection;299;1;296;0
WireConnection;299;2;297;0
WireConnection;299;3;298;0
WireConnection;280;0;272;0
WireConnection;280;1;289;0
WireConnection;280;2;285;0
WireConnection;280;3;286;0
WireConnection;282;0;278;0
WireConnection;282;1;273;0
WireConnection;282;2;279;0
WireConnection;282;3;276;0
WireConnection;304;0;155;0
WireConnection;304;1;303;0
WireConnection;283;0;281;0
WireConnection;283;1;282;0
WireConnection;283;2;280;0
WireConnection;283;3;299;0
WireConnection;301;0;283;0
WireConnection;307;0;283;0
WireConnection;306;0;283;0
WireConnection;303;0;302;0
WireConnection;303;1;301;0
WireConnection;236;0;261;0
WireConnection;236;1;155;0
WireConnection;281;0;275;0
WireConnection;281;1;271;0
WireConnection;281;2;274;0
WireConnection;281;3;277;0
WireConnection;284;0;283;0
WireConnection;275;38;268;0
WireConnection;285;38;288;0
WireConnection;274;38;263;0
WireConnection;278;38;264;0
WireConnection;277;38;266;0
WireConnection;273;38;262;0
WireConnection;289;38;290;0
WireConnection;296;38;292;0
WireConnection;297;38;293;0
WireConnection;295;38;291;0
WireConnection;276;38;270;0
WireConnection;309;0;236;0
WireConnection;309;2;304;0
WireConnection;309;3;307;0
WireConnection;309;4;305;0
ASEEND*/
//CHKSM=CF5BC08D22195D4B0C0FA35CE422FB2A0EF3D0AC