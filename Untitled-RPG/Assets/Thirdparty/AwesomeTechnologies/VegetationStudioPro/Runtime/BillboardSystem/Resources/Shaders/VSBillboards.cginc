#ifdef USE_HDWIND
#include "HDWind.cginc"	
#endif

sampler2D _MainTex;
sampler2D _Bump;
fixed4 _Color;
float _YRotation;
float _XTurnFix;
float _CullDistance;
float _FarCullDistance;
float _Brightness;
float _MipmapBias;
int _InRow;
int _InCol;
int _CameraType;
float4 _CameraPosition;

float _BillboardWindSpeed;

float _Smoothness;
float _Metallic;
float _Specular;
float _Occlusion;
float _NormalStrength;

//float _Cutoff;

#ifdef USE_SNOW
half _SnowAmount;
fixed4 _SnowColor;
half _SnowBlendFactor;
float _SnowBrightness;
#endif

#ifdef AT_HUE_VARIATION_ON
half4 _HueVariation;
#endif

struct Input
{
	float2 uv_MainTex;
	float4 d;
#ifdef LOD_FADE_CROSSFADE
	float3 dc;
	float crossfade_value;
#endif 
#ifdef USE_SNOW
	float3 worldNormal; INTERNAL_DATA
#endif
};

 

void DitherCrossFade(half3 ditherScreenPos, float crossfadeValue)
{
	half2 projUV = ditherScreenPos.xy / ditherScreenPos.z;
	projUV.xy = frac(projUV.xy + 0.001) + frac(projUV.xy * 2.0 + 0.001);
	half dither = crossfadeValue - (projUV.y + projUV.x) * 0.25;
	clip(dither);
}

half3 VS_ComputeDitherScreenPos(float4 clipPos)
{
	half3 screenPos = ComputeScreenPos(clipPos).xyw;
	screenPos.xy *= _ScreenParams.xy * 0.25;
	return screenPos;
}

//#ifdef LOD_FADE_CROSSFADE
//void VS_ApplyDitherCrossFade(half3 ditherScreenPos, float crossfadeValue)
//{
//	float fadeValue = ceil(crossfadeValue * 16) / 16.0;
//	half2 projUV = ditherScreenPos.xy / ditherScreenPos.z;
//	projUV.y = frac(projUV.y) * 0.0625 + fadeValue; // quantized lod fade by 16 levels
//	clip(tex2D(_DitherMaskLOD2D, projUV).a - 0.5);
//}
//#endif

UNITY_INSTANCING_BUFFER_START(Props)

UNITY_INSTANCING_BUFFER_END(Props)

void vert(inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT(Input, o);

	float4 CENTER = v.vertex;
	float3 CORNER = v.normal * v.texcoord2.x;


    float3 worldspaceCenter = mul(unity_ObjectToWorld, CENTER);
    float3 modifiedCameraPos;
    if (_InCol == 1)
    {
        modifiedCameraPos =  _WorldSpaceCameraPos;
        modifiedCameraPos.y = worldspaceCenter.y;
    }
    else
    {
       modifiedCameraPos =  _WorldSpaceCameraPos.xyz;
    }
    
    #define cameraPos modifiedCameraPos;

	float3 clipVect;
	clipVect = (worldspaceCenter + float3(0, v.texcoord3.y, 0)) - _CameraPosition;

#if defined(UNITY_PASS_SHADOWCASTER)
	float3 camVect;

	if (unity_MatrixVP[3][3] == 1)
		camVect = _WorldSpaceLightPos0.w < 0.5 ? _WorldSpaceLightPos0.xyz : worldspaceCenter - _WorldSpaceLightPos0.xyz;
	else
		camVect = worldspaceCenter - cameraPos;

#define camVectEvenInShadows (worldspaceCenter - cameraPos)			
#else
    
	float3 camVect = worldspaceCenter - cameraPos;
#define camVectEvenInShadows camVect

#endif

	if (length(clipVect) < _CullDistance || length(clipVect) > _FarCullDistance)
	{
		CORNER.xyz *= 0;
	}
	else
	{
#ifdef LOD_FADE_CROSSFADE
		float distance = length(clipVect) - _CullDistance;
		if (distance < 2)
		{
			o.crossfade_value = 1 - (distance / 2);
		}
		else
		{
			o.crossfade_value = 0.98;
		}
#endif

		// Create LookAt matrix
		float3 zaxis = normalize(camVect);
		float3 xaxis = normalize(cross(float3(0, 1, 0), zaxis));
		float3 yaxis = cross(zaxis, xaxis);

		float4x4 lookatMatrix = {
			xaxis.x,            yaxis.x,            zaxis.x,       0,
			xaxis.y,            yaxis.y,            zaxis.y,       0,
			xaxis.z,            yaxis.z,            zaxis.z,       0,
			0, 0, 0,  1
		};

		v.vertex = mul(lookatMatrix, float4(CORNER.x, CORNER.y, (yaxis.y - 1.0) * v.texcoord2.y, 1));
		v.vertex.xyz += CENTER.xyz;
		
		v.normal = -zaxis;
		v.tangent.xyz = xaxis;
		v.tangent.w = -1;

#ifdef USE_SNOW
		o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
#endif
		v.texcoord.x /= _InRow;
		v.texcoord.y /= _InCol;

		float angle;
		float step;
		float2 atanDir = normalize(float2(-zaxis.z, -zaxis.x));
		angle = (atan2(atanDir.y, atanDir.x) / 6.28319) + 0.5; // angle around Y in range 0....1
		angle += v.texcoord1.x;
		angle -= (int)angle;
		step = 1.0 / _InRow;
		v.texcoord.x += step * ((int)((angle + step * 0.5) * _InRow));
		step = 1.0 / _InCol;
		angle = saturate(dot(-zaxis, float3(0, 1, 0)));
		angle = clamp(angle, 0, step*(_InCol - 1));
		v.texcoord.y += step * ((int)((angle + step * 0.5) * _InCol));
		o.d.x = v.texcoord1.y;

#ifdef AT_HUE_VARIATION_ON
		float hueVariationAmount = frac(CENTER.x + CENTER.y + CENTER.z);
		o.d.y = saturate(hueVariationAmount * _HueVariation.a);
#endif

#ifdef LOD_FADE_CROSSFADE
		o.dc = VS_ComputeDitherScreenPos(v.vertex);
#endif


#ifdef USE_HDWIND
#if !defined(UNITY_PASS_SHADOWCASTER)
		float initialBend = 1;
		float stiffness = 1;
		float drag = 0.3;
		float shiverDrag = 0;
		float shiverDirectionality = 1;

		float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
		float3 rootWP = TransformObjectToWorld(CENTER.xyz);
		if (positionWS.y > rootWP.y)
		{
			float3 normalWS = TransformObjectToWorldNormal(v.normal);
			WindData windData;
			ApplyWindDisplacement(positionWS, windData, normalWS, rootWP, stiffness, drag, shiverDrag, shiverDirectionality, initialBend, 20 * _BillboardWindSpeed, _Time);
			v.vertex.xyz = TransformWorldToObject(positionWS).xyz;
		}
#endif
#endif
	}
}



void surf(Input IN, inout SurfaceOutputStandard o)
{
//#ifdef LOD_FADE_CROSSFADE
//	//#define unity_LODFade float4(0.5,0.5,0,0)
//	if (IN.crossfade_value > 0.99)
//	{
//		VS_ApplyDitherCrossFade(IN.dc, IN.crossfade_value);
//	}
//	//DitherCrossFade(IN.dc, IN.crossfade_value);
//#endif

	fixed4 c = tex2Dbias(_MainTex, half4(IN.uv_MainTex, 0, _MipmapBias)) * _Color;

#ifdef AT_HUE_VARIATION_ON
	half3 shiftedColor = lerp(c.rgb, _HueVariation.rgb, IN.d.y);
	half maxBase = max(c.r, max(c.g, c.b));
	half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
	maxBase /= newMaxBase;
	maxBase = maxBase * 0.5f + 0.5f;
	shiftedColor.rgb *= maxBase;
	c.rgb = saturate(shiftedColor);
#endif

	o.Albedo = c.rgb * IN.d.x *_Color;
	o.Albedo = clamp(o.Albedo * _Brightness, 0, 1);
	o.Normal = tex2D(_Bump, IN.uv_MainTex).rgb * 2.0 - 1.0;
	o.Occlusion = _Occlusion;
	o.Smoothness = _Smoothness;
	o.Metallic = _Metallic;

#ifdef USE_SNOW
	half d = dot(WorldNormalVector(IN, o.Normal), float3(0, -1, 0)) * 0.5 + 0.5;
	o.Albedo = lerp(o.Albedo, (_SnowColor.xyz * _SnowBrightness), _SnowAmount * d * _SnowBlendFactor);
#endif

	o.Alpha = c.a;
}


void surfSpecular(Input IN, inout SurfaceOutputStandardSpecular o)
{
//#ifdef LOD_FADE_CROSSFADE
//	//#define unity_LODFade float4(0.5,0.5,0,0)
//	if (IN.crossfade_value > 0.99)
//	{
//		VS_ApplyDitherCrossFade(IN.dc, IN.crossfade_value);
//	}
//	//DitherCrossFade(IN.dc, IN.crossfade_value);
//#endif

	fixed4 c = tex2Dbias(_MainTex, half4(IN.uv_MainTex, 0, _MipmapBias)) * _Color;
#ifdef AT_HUE_VARIATION_ON
	half3 shiftedColor = lerp(c.rgb, _HueVariation.rgb, IN.d.y);
	half maxBase = max(c.r, max(c.g, c.b));
	half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
	maxBase /= newMaxBase;
	maxBase = maxBase * 0.5f + 0.5f;
	shiftedColor.rgb *= maxBase;
	c.rgb = saturate(shiftedColor);
#endif

	o.Albedo = c.rgb * IN.d.x *_Color;
	o.Albedo = clamp(o.Albedo * _Brightness, 0, 1);
	o.Normal = tex2D(_Bump, IN.uv_MainTex).rgb * 2.0 - 1.0;
	
	o.Normal.xy *= _NormalStrength;
	//.Normal = normalize(o.Normal);
	
	o.Occlusion = _Occlusion; 
	o.Smoothness = _Smoothness;
	o.Specular = _Specular;

#ifdef USE_SNOW
	half d = dot(WorldNormalVector(IN, o.Normal), float3(0, -1, 0)) * 0.5 + 0.5;
	o.Albedo = lerp(o.Albedo, (_SnowColor.xyz * _SnowBrightness), _SnowAmount * d * _SnowBlendFactor);
#endif

	o.Alpha = c.a;
	//o.Alpha = (o.Alpha - _Cutoff) / max(fwidth(o.Alpha), 0.0001) + 0.5;
	
}