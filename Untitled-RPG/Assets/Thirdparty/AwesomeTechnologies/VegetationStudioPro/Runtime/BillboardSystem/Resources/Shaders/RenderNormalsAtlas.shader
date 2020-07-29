// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "AwesomeTechnologies/Billboards/RenderNormalsAtlas"
{
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff" , Range(0,1)) = .5
		_FlipBackNormals("Flip Backfacing Normals", Int) = 0
	}
	
	SubShader
	{
		Cull Off
		//Cull Back
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D	_MainTex;
			float _Cutoff;
			int _FlipBackNormals;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 n : TEXCOORD1;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				
				//float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                //float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));			
				//float3 normal =  dot(viewDir, float3(0, 0, 1)) > 0 ? v.normal : -v.normal;
				//o.n = mul( unity_ObjectToWorld, float4( normal, 0.0 ) ).xyz;
				
				o.n = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz; // convert normal to world space at first (its in local space in data)
				o.n = mul(UNITY_MATRIX_V,float4(o.n,0)).xyz; // then convert to view space, that means correct attenuation if texture quad stays in 0,0,0 with rotation 0,0,0
				
				//TANGENT_SPACE_ROTATION;						
                //float3 tangentUp = mul(rotation,  float3(0,1,0));
                //o.n = tangentUp;				
				return o;
			}

			half4 frag(v2f i,fixed facing : VFACE) : COLOR
			{
				half4 c = tex2D (_MainTex, i.uv);
				clip(c.a-_Cutoff);
				i.n = normalize(i.n);
				if (_FlipBackNormals > 0)
				{				
				    if (facing < 0)
				    {
				    i.n = -i.n;
				    }
				}				
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