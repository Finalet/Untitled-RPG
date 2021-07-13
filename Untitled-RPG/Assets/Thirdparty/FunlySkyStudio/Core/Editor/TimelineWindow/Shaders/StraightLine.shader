// Copyright(c) 2017 Funly LLC
//
// Author: Jason Ederle
// Description: Draws a line between 2 points.
// Contact: jason@funly.io

Shader "Hidden/Funly/Sky/StraightLine"
{
	Properties
	{
		_LineColor ("Line", Color) = (0, 1, 0, 1)
    _Background ("Background", Color) = (1, 1, 1, 1)
    _LeftPercent ("Left Value", Range(0, 1)) = 0
    _RightPercent ("Right Value", Range(0, 1)) = 1
    _Thickness ("Thickness", Range(0, .1)) = .01
    _Feathering ("Edge Feathering", Range(0, 1)) = .1
    _WidthRatio ("Width Ratio", Float) = 1
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

			fixed4 _LineColor;
      fixed4 _Background;
      float _LeftPercent;
      float _RightPercent;
      float _Thickness;
			float _Feathering;
      float _WidthRatio;

			v2f vert (appdata v)
			{
				v2f o;

        // Resize the width.
        v.vertex.x *= _WidthRatio;

				o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
        
        float yMin = _Thickness;
        float yMax = 1 - _Thickness;
        float2 leftPoint = float2(0, clamp(_LeftPercent, yMin, yMax));
        float2 rightPoint = float2(1, clamp(_RightPercent, yMin, yMax));
        
        float2 fullLineVect = rightPoint - leftPoint;
        float2 fragVector = i.uv - leftPoint;

        float2 fullLineDirection = normalize(fullLineVect);
        
        float2 projectedLength = dot(fullLineDirection, fragVector);
        float2 projectedVect = fullLineDirection * projectedLength;
        float2 projectedPoint = leftPoint + projectedVect;

        float distanceToFrag = distance(i.uv, projectedPoint);

        float mixPercent = smoothstep(_Thickness, _Thickness - (_Thickness * _Feathering), distanceToFrag);

        return lerp(_Background, _LineColor, mixPercent);
			}
			ENDCG
		}
	}
}
