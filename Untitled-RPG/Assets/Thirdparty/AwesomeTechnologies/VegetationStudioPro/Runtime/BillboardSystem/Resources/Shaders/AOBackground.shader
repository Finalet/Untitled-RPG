// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "AwesomeTechnologies/Billboards/AOBackground" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.2
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" "Queue" = "AlphaTest" }
		LOD 300
		
		CGPROGRAM
		#pragma surface surf Standard  alphatest:_Cutoff

		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = float4(1, 1, 1,1).xyz;
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = c.a;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
