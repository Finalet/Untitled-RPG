Shader "Funly/Unlit/UnlitTextureAO"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
    _AOTex ("Ambient Occlusion", 2D) = "white" {}
    _Color ("Tint", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd_fullshadows
      
      #include "UnityCG.cginc"
      #include "Lighting.cginc"
      #include "AutoLight.cginc"

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

			sampler2D _MainTex;
      sampler2D _AOTex;
      float4 _Color;
      
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
        fixed4 ao = tex2D(_AOTex, i.uv);

        fixed4 outColor = col * ao * _Color;
				return outColor;
			}
			ENDCG
		}
	}
  Fallback "VertexLit"
}
