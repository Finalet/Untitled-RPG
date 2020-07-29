
Shader "AwesomeTechnologies/Vegetation/RenderVegetationColorMask"
{
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff" , Range(0,1)) = 0.1
	}
	
	SubShader
	{
		Cull Off
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D	_MainTex;
			float _Cutoff;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 n : TEXCOORD1;
				float4 color : COLOR0;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.n = mul(UNITY_MATRIX_V,float4(v.normal.xyz,0)).xyz;
				o.color = v.color;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.uv);
				clip(c.a-_Cutoff);
				c.a = 1;
				return c;
			}
			ENDCG
		}
	}
		Fallback "VertexLit"
}