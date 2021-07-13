// Sky Studio
//
// Author: jason@funly.io
// Description: Shows rainfall textures on the weather encsolure.

Shader "Funly/Sky Studio/Weather/Rain Background"
{
	Properties
	{
    // Near rain texture.
		_NearTex ("Near Rain Texture", 2D) = "black" {}
    _NearRainSpeed ("Near Rain Speed", Range(0, 3)) = .5
    _NearDensity ("Near Rain Density", Range(0, 1)) = .7

    // Far rain texture.
    _FarTex ("Far Rain Texture", 2D) = "black" {}
    _FarRainSpeed("Far Rain Speed", Range(0, 3)) = .75
    _FarDensity("Far Rain Density", Range(0, 1)) = .5
    _TintColor("Tint Color", Color) = (1, 1, 1, 1)
    _Turbulence("Turbulence", Range(0, 1)) = 0
    _TurbulenceSpeed("Turbulence Speed", Range(0, 5)) = .5
  }
    SubShader
    {
    Tags { "RenderType" = "Transparent" "Queue" = "Transparent+10" }
    LOD 100
    Blend OneMinusDstColor One
    Cull Back
    ZWrite Off

		Pass
		{
			CGPROGRAM
      #pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
        fixed4 color : COLOR;
			};

			struct v2f
			{
        float4 vertex : SV_POSITION;
				float2 nearUV : TEXCOORD0;
        float2 farUV : TEXCOORD1;
        float strength : COLOR;
			};

			sampler2D _NearTex;
			float4 _NearTex_ST;
      float _NearRainSpeed;
      float _NearDensity;

      sampler2D _FarTex;
      float4 _FarTex_ST;
      float _FarRainSpeed;
      float _FarDensity;

      float4 _TintColor;
      float _Turbulence;
      float _TurbulenceSpeed;

			v2f vert (appdata v)
			{
				v2f o;
        o.strength = v.color.r;
				o.vertex = UnityObjectToClipPos(v.vertex);

        float2 scaledNearUV = TRANSFORM_TEX(v.uv, _NearTex);
        o.nearUV = float2(scaledNearUV.x, scaledNearUV.y + (_Time.y * _NearRainSpeed));
        o.nearUV.x += .2 + (sin((_Time.y) * _TurbulenceSpeed) * _Turbulence);

        float2 scaledFarUV = TRANSFORM_TEX(v.uv, _FarTex);
        o.farUV = float2(scaledFarUV.x, scaledFarUV.y + (_Time.y * _FarRainSpeed));

				return o;
			}
			

      fixed4 frag (v2f i) : SV_Target
			{
				fixed4 nearColor = tex2D(_NearTex, i.nearUV) * _NearDensity;
        fixed4 farColor = tex2D(_FarTex, i.farUV) * _FarDensity;

        return saturate(nearColor + farColor) * i.strength * _TintColor;
			}
      
			ENDCG
		}
	}
}
