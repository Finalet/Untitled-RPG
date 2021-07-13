Shader "Funly/Sky Studio/Weather/OverheadDepthNormal"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float fragDepth : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };
      
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.fragDepth = o.vertex.z / o.vertex.w;
        
                return o;
            }
            
      inline float GetNormalPercent(float value) {
        return (value + 1.0f) / 2.0f;
      }

      float3 GetPercentVector(float3 dir) {
        return float3(GetNormalPercent(dir.x), GetNormalPercent(dir.y), GetNormalPercent(dir.z));
      }

            fixed4 frag (v2f i) : SV_Target
            {
        float depth = i.fragDepth;

        if (UNITY_NEAR_CLIP_VALUE < 0.0f) {
          // OpenGL-like platforms are [-1,1] ranged and DirectX-like are [0,1] ranged.
                    // Convert the -1:1 ranged value to 0:1.
          depth = (depth + 1.0f) / 2.0f;
        }

        #if defined(UNITY_REVERSED_Z)
          depth = 1.0f - depth;
        #endif

                // You can't store negative numbers so convert to a 0:1 values for normal direction.
        float3 percentNormal = GetPercentVector(normalize(i.worldNormal));

                return fixed4(percentNormal, saturate(depth));
            }
            ENDCG
        }
    }
    FallBack "Unlit/Color"
}
