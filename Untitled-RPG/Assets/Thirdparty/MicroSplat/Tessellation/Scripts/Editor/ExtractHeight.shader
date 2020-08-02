Shader "Hidden/MicroSplat/ExtractHeight"
{
	Properties
	{
		_MainTex ("Texture", 2DArray) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			UNITY_DECLARE_TEX2DARRAY(_MainTex);
         int index;

			fixed4 frag (v2f i) : SV_Target
			{
				float h = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.uv, index)).a;
            return fixed4(h,h,h,h);
			}
			ENDCG
		}
	}
}
