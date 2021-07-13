Shader "Funly/Sky Studio/Flat Shading - Color"
{
	Properties
	{
		_Color ("Tint Color", Color) = (1, 1, 1, 1)
		_Ambient ("Ambient Intensity", Range(0, 5)) = .1
		_LightIntensity ("Light Intensity", Range(0, 1)) = .8
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
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float3 worldPosition: TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			float4 _Color;
			float _Ambient;
			float _LightIntensity;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPosition = mul(unity_ObjectToWorld, v.vertex);				

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{	
				float3 fx = ddx(i.worldPosition);
				float3 fy = ddy(i.worldPosition);
				float3 normal = normalize(cross(fx, fy));

				// Unity variable is poorly named, it's actually a direction.
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				fixed3 finalColor =  _Ambient / (UNITY_HALF_PI * 2.0f) *_LightIntensity * _Color.xyz * (1.0f - max(0.0f, dot(normal, lightDir)));

				return fixed4(finalColor, 1.);
			}
			ENDCG
		}
	}
}
