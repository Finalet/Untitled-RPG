Shader "Custom/MeshTerrainDebug" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" "Queue" = "Transparent" }
		LOD 200

		ZWrite Off
		Offset -1,-1

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
	};

	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		o.Albedo = _Color;
	}
	ENDCG
	}
		FallBack "Diffuse"
}