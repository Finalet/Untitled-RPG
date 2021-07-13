// Copyright(c) 2017 Funly LLC
//
// Author: Jason Ederle
// Description: Gradient with colors at specific positions.
// Contact: jason@funly.io

Shader "Hidden/Funly/SkyStudio/MultiColorGradient"
{
	Properties
	{
    _NumColorPoints("Number of points", Range(0, 512)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
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

      uniform float4 _ColorPoints[512];
			uniform int _NumColorPoints;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
      half3 ConvertColorSpaces(half3 col) {
        #ifdef UNITY_COLORSPACE_GAMMA
        return col;
        #else
        return LinearToGammaSpace(col);
        #endif
      }

      fixed4 GetGradient(float4 leftPoint, float4 rightPoint, float2 uv) {
        float pointDistance = rightPoint.x - leftPoint.x;

        half4 leftColor = half4(ConvertColorSpaces(leftPoint.yzw), 1);
        half4 rightColor = half4(ConvertColorSpaces(rightPoint.yzw), 1);

        // Percentage between the left and right keyframes.
        float blendPercent = (uv.x - leftPoint.x) / pointDistance;

        return lerp(leftColor, rightColor, blendPercent);
      }

			fixed4 frag (v2f i) : SV_Target
			{
        for (int j = 0; j < (_NumColorPoints - 1); j++) {
          float4 leftPoint = _ColorPoints[j];
          float4 rightPoint = _ColorPoints[j + 1];
          
          if ((i.uv.x > leftPoint.x) && (i.uv.x <= rightPoint.x)) {
            return GetGradient(leftPoint, rightPoint, i.uv);
          }
        }
       
        return float4(1, 0, 0, 1);
			}
			ENDCG
		}
	}
}
