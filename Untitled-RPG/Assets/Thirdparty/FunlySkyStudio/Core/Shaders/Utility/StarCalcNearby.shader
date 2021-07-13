// Sky Studio
// Author: Jason Ederle

Shader "Hidden/Funly/Sky Studio/Computation/StarCalcNearby"
{
  Properties
  {
    _MainTex("Main Texture", 2D) = "black" {}
    _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
  }
  SubShader
  {
    Tags{"RenderType" = "Opaque"}
    LOD 100
    Cull Off

    Pass
    {
      CGPROGRAM
      #pragma target 2.0
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "SkyMathUtilities.cginc"
      #include "StarCalcUtility.cginc"
      
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
      int _NumStarPoints;
      float4 _RandomSeed;
      int _TextureSize;
  
      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;

        return o;
      }

      // Get position of the closest star point.
      float4 GetClosestStarPointToUV(float2 uv)
      {
        float3 closestStarPoint = float3(1, 0, 0);
        float2 fragSphericalCoord = ConvertUVToSphericalCoordinate(uv);
        float3 fragPoint = SphericalCoordinateToDirection(fragSphericalCoord);
        float shortestDistance = 0;

        float texelWidth = 1.0f / (float)_TextureSize;
        float halfTexel = texelWidth / 2.0f;

        for (int i = 0; i < _NumStarPoints; i++)
        {
          float2 starUV = float2((texelWidth * i) + halfTexel, halfTexel);
          float3 randomStarPoint = tex2D(_MainTex, starUV).rgb;
    
          float currentStarDistance = distance(randomStarPoint, fragPoint);

          if (i == 0 || currentStarDistance < shortestDistance)
          {
            closestStarPoint = randomStarPoint;
            shortestDistance = currentStarDistance;
          }
        }

        float starNoise = RangedRandom(_RandomSeed.xy * closestStarPoint.xy * .98347f, .2f, .8f);
        return float4(closestStarPoint.xyz, starNoise);
      }

      float4 frag(v2f i) : SV_Target
      {
        float2 uv = GetPixelCeneteredUV(i.uv, _TextureSize);
        float4 starPosition = GetClosestStarPointToUV(uv);
        float2 sphericalCoord = DirectionToSphericalCoordinate(starPosition.xyz);
        float2 percents = ConvertSphericalCoordinateToPercentage(sphericalCoord);

        float4 starData = float4(
            percents.x,       // Azimuth rotation percent.
            percents.y,       // Altitude rotation percent.
            starPosition.w,   // Noise.
            1.0f);            // Unused.

        return starData;
      }
      ENDCG
    }
  }
}
