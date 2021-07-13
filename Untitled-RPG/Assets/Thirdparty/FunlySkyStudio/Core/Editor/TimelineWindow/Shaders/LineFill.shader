// Copyright(c) 2017 Funly LLC
//
// Author: Jason Ederle
// Description: Renders a soft line to avoid aliasing issues.
// Contact: jason@funly.io

Shader "Hidden/Funly/Sky/SoftLine"
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

			fixed4 _LineColor;
			float _EdgeFeathering;
      float4 _BackgroundColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
        fixed4 lineColor = _LineColor;

        float mixPercent = smoothstep(0, _EdgeFeathering, i.uv.y);
        float4 lowerColor = lerp(_BackgroundColor, lineColor, mixPercent);

        mixPercent = smoothstep(1, (1 - _EdgeFeathering), i.uv.y);
        float4 upperColor = lerp(_BackgroundColor, lineColor, mixPercent);

        upperColor *= step((1 - _EdgeFeathering), i.uv.y);
        lowerColor *= step(i.uv.y, (1 - _EdgeFeathering));

        return lowerColor + upperColor;
			}
			ENDCG
		}
	}
}
