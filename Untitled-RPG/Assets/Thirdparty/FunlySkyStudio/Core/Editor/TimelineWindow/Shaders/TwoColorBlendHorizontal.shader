// Copyright(c) 2017 Funly LLC
//
// Author: Jason Ederle
// Description: Blends 2 colors for previewing time changes.
// Contact: jason@funly.io

Shader "Hidden/Funly/Sky/TwoColorBlendHorizontal"
{
	Properties
	{
    _LeftColor ("Left Color", Color) = (1, 0, 0, 1)
    _RightColor ("Right Color", Color) = (0, 0, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
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

			float4 _LeftColor;
      float4 _RightColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        return lerp(_LeftColor, _RightColor, smoothstep(0, 1, i.uv.x));
			}
			ENDCG
		}
	}
}
