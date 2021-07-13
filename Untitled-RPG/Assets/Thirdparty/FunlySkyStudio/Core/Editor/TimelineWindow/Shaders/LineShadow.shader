// Copyright(c) 2017 Funly LLC
//
// Author: Jason Ederle
// Description: Draws a line between 2 points.
// Contact: jason@funly.io

Shader "Hidden/Funly/Sky/LineShadow"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float _EdgeFeathering;
      float4 _BackgroundColor;
      float4 _ShadowColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
        float mixPercent = smoothstep(0, 1, i.uv.y);
        return lerp(_BackgroundColor, _ShadowColor, mixPercent);
			}
			ENDCG
		}
	}
}
