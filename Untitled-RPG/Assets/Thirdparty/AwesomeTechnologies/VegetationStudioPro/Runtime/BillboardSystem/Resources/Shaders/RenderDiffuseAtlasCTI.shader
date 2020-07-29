
Shader "AwesomeTechnologies/Billboards/RenderDiffuseAtlasCTI"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _Culling("Culling", Float) = 0
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff" , Range(0,1)) = 0.5
		_Color ("Color Variation", Color) = (0.9,0.5,0.0,0.1)
		_IsBark("Is Bark", Float) = 0
	}
	
	SubShader
	{
		Cull [_Culling]
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers d3d11_9x
			#include "UnityCG.cginc"
			
			sampler2D	_MainTex;
			float _Cutoff;
			float4 _Color;
			float _IsBark;

			float _Culling;
			
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

			half4 frag(v2f i, float face : VFACE) : SV_Target
			{
				half4 c = tex2D (_MainTex, i.uv);
				// detect bark
				if (_IsBark == 1) {
					c.a = 1;
				}
				else {
					clip(c.a - _Cutoff);
				}
				c.rgb = lerp(c.rgb, (c.rgb + _Color.rgb) * 0.5, i.color.r * _Color.a);
				c.rgb = clamp(c.rgb, 0, 1);
				return c;
			}
			ENDCG
		}

	}
		Fallback "VertexLit"
}