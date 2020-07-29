// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/MinPostFilter"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_UseGammaCorrection("Use gamma correction", Int) = 0
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
			#pragma exclude_renderers d3d9
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			int _UseGammaCorrection;
			
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
			
			float2 UV(int u, int v)
			{
				float2 res;
				
				res.x = ((float)u)/_ScreenParams.x;
				res.y = ((float)v)/_ScreenParams.y;
				
				float2 ps = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
				res.x += ps.x/2;
				res.y += ps.y/2;
				
				return res;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				int u = i.uv.x * _ScreenParams.x;
				int v = i.uv.y * _ScreenParams.y;
				
				fixed4 col = tex2D(_MainTex, UV(u,v));
				
				/*
				if(col.a<.5)
				{
					float2 ps = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
					
					fixed4 a;
					
					float minDist = 1000;
					
					UNITY_UNROLL
					for(int i=-3;i<=3;i++)
					{
						UNITY_UNROLL
						for(int j=-3;j<=3;j++)
						{
							a = tex2D(_MainTex, UV(u + i,v + j));
							if(a.a>.1)
							{
								float d = length(float2(i,j));
								if(d<minDist)
								{
									minDist=d;
									col.rgb = a.rgb;
								}
							}
						}
					}

				}
				*/

				if(_UseGammaCorrection > 0)
				{
					col.rgb = pow(col.rgb,0.4545);
					
					//col.r <= 0.04045  ? ( col.r / 12.92 ) : ( pow( (col.r+0.055)/1.055, 2.4 ) );
					//col.g <= 0.04045  ? ( col.g / 12.92 ) : ( pow( (col.g+0.055)/1.055, 2.4 ) );
					//col.b <= 0.04045  ? ( col.b / 12.92 ) : ( pow( (col.b+0.055)/1.055, 2.4 ) );
					
					/*
					col.r = col.r < 0.0031308 ? 12.95 * col.r : pow(col.r, 2.4)*1.055 - 0.055;
					col.g = col.g < 0.0031308 ? 12.95 * col.g : pow(col.g, 2.4)*1.055 - 0.055;
					col.b = col.b < 0.0031308 ? 12.95 * col.b : pow(col.b, 2.4)*1.055 - 0.055;
					*/
				}
				
				return col;
			}
			ENDCG
		}
	}
}
