// Author: Jason Ederle
// Description: Render out the world space normal of an object encoded between 0-1 value ranges.

Shader "Funly/Sky Studio/Utility/World Normal"
{
  Properties
  {
    [HideInInspector] _MainTex("Texture", 2D) = "white" {}
  }
  SubShader
  {
    Tags{"RenderType" = "Opaque"} LOD 100

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
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        float4 normal : NORMAL; // FIXME - Is this correct here?
      };

      float4x4 inverse(float4x4 input)
      {
        #define minor(a, b, c) determinant(float3x3(input.a, input.b, input.c))

        float4x4 cofactors = float4x4(
            minor(_22_23_24, _32_33_34, _42_43_44),
            -minor(_21_23_24, _31_33_34, _41_43_44),
            minor(_21_22_24, _31_32_34, _41_42_44),
            -minor(_21_22_23, _31_32_33, _41_42_43),

            -minor(_12_13_14, _32_33_34, _42_43_44),
            minor(_11_13_14, _31_33_34, _41_43_44),
            -minor(_11_12_14, _31_32_34, _41_42_44),
            minor(_11_12_13, _31_32_33, _41_42_43),

            minor(_12_13_14, _22_23_24, _42_43_44),
            -minor(_11_13_14, _21_23_24, _41_43_44),
            minor(_11_12_14, _21_22_24, _41_42_44),
            -minor(_11_12_13, _21_22_23, _41_42_43),

            -minor(_12_13_14, _22_23_24, _32_33_34),
            minor(_11_13_14, _21_23_24, _31_33_34),
            -minor(_11_12_14, _21_22_24, _31_32_34),
            minor(_11_12_13, _21_22_23, _31_32_33));

        #undef minor
        
        return transpose(cofactors) / determinant(input);
      }

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);

        //float3 scaledNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
        //float3 scaledNormal = v.normal.xyz;
        //float3 worldNormal = mul(unity_ObjectToWorld, float4(scaledNormal, 0));

        // Correct normal for scaling.
        float3 worldNormal = mul(transpose(inverse(unity_ObjectToWorld)), float4(v.normal.xyz, 0.0f)).xyz;
        worldNormal = normalize(worldNormal);

        o.normal = float4(worldNormal, 1.0f);

        return o;
      }

      fixed4 frag(v2f i) : SV_Target
      {
        fixed3 normalPercent = (i.normal.xyz + 1.0) / 2.0f;
        return fixed4(normalPercent, 1.0f);
      }
      ENDCG
    }
  }
}
