// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "AwesomeTechnologies/Billboards/RenderNormalsAtlasTreeCreator"
{
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff" , Range(0,1)) = .5
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
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				//test
				v.normal = normalize(v.vertex);
				o.n = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz; // convert normal to world space at first (its in local space in data)
				o.n = mul(UNITY_MATRIX_V,float4(o.n,0)).xyz; // then convert to view space, that means correct attenuation if texture quad stays in 0,0,0 with rotation 0,0,0
				//o.n = float3(0, 1, 0);
				
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.uv);
				clip(c.a-_Cutoff);
				i.n = normalize(i.n);
				//i.n = abs(i.n);
				return half4 ((i.n+1.0)*0.5, 1);
			}
			ENDCG

		}
		/*
		Cull Front
		
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
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.n = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz; // convert normal to world space at first (its in local space in data)
				o.n = mul(UNITY_MATRIX_V,float4(o.n,0)).xyz; // then convert to view space, that means correct attenuation if texture quad stays in 0,0,0 with rotation 0,0,0
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.uv);
				clip(c.a-_Cutoff);
				i.n = normalize(i.n);
				return half4 ((i.n+1.0)*0.5, 1);
			}
			ENDCG

		}
	*/
	}
		Fallback "VertexLit"
}