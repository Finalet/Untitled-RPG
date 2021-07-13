Shader "Funly/Flag/UnlitFlag"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1, 1, 1, 1)
    _Speed ("Speed", Range(0, 20)) = 2
    _Frequency ("Frequency", Range(0, 20)) = 10
    _Amplitude ("Amplitude", Range(0, 1)) = .3
    _VerticalDisplacement ("Vertical Displacement", Range(0, 5)) = .5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
		LOD 100
    Cull off
    Lighting on

		Pass
		{
			CGPROGRAM
			#pragma multi_compile_fwdbase
      #pragma vertex vert
			#pragma fragment frag
			
      #pragma target 3.0

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
			float4 _Color;
      float _Frequency;
      float _Amplitude;
      float _Speed;
      float _VerticalDisplacement;

			v2f vert (appdata v)
			{
				v2f o;
				o.uv = v.uv;

        float zWaveAmount = sin(v.uv.x * _Frequency + (_Time.y * _Speed)) * _Amplitude;
        float smoothX = smoothstep(0, 1, o.uv.x);
        
        v.vertex.z += zWaveAmount * smoothX;
        v.vertex.y += zWaveAmount * _VerticalDisplacement * smoothX;

        o.vertex = UnityObjectToClipPos(v.vertex);
        
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				return col;
			}
			ENDCG
		}
	}
  Fallback "VertexLit"
}
