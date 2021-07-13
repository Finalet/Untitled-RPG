// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Author: Jason Ederle
// Contact: jason@funly.io
// Description: GPU instanced shader for flipping through a sprite sheet.

Shader "Funly/Sky Studio/Sprite Sheets/Sprite Sheet Instanced"
{
	Properties
	{
		[NoScaleOffset] _SpriteSheetTex ("Sprite Texture", 2D) = "black" {}
		_SpriteColumnCount ("Sprite Columns", int) = 1
		_SpriteRowCount ("Sprite Rows", int) = 1
		_SpriteItemCount ("Sprite Total Items", int) = 1
		_AnimationSpeed ("Animation Speed", int) = 25
    _Intensity ("Intensity", Range(0, 1)) = .7
    _TintColor ("Tint Color", Color)  = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
    Blend OneMinusDstColor One
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
      #pragma multi_compile_instancing

			#include "UnityCG.cginc"

      struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _SpriteSheetTex;
			int _SpriteColumnCount;
			int _SpriteRowCount;
			int _SpriteItemCount;
			int _AnimationSpeed;
      float _Intensity;
      float4 _TintColor;

      UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float, _StartTime)
#define _StartTime_arr Props
        UNITY_DEFINE_INSTANCED_PROP(float, _EndTime)
#define _EndTime_arr Props
      UNITY_INSTANCING_BUFFER_END(Props)

      int GetSpriteTargetIndex(int itemCount, int animationSpeed, float startTime) {
        float delta = _Time.y - startTime;
        float timePerFrame = 1.0f / _AnimationSpeed;
        int frameIndex = (int)(delta / timePerFrame);
        return clamp(frameIndex, 0, _SpriteItemCount - 1);
      }

      float2 GetSpriteItemSize(float2 dimensions) {
        return float2(1.0f / dimensions.x, (1.0f / dimensions.x) * (dimensions.x / dimensions.y));
      }

      float2 GetSpriteSheetCoords(float2 uv, float2 dimensions, int targetFrameIndex, float2 itemSize, int numItems) {
        int rows = dimensions.y;
        int columns = dimensions.x;

        float2 scaledUV = float2(uv.x * itemSize.x, uv.y * itemSize.y);
        float2 offset = float2(
          (int)abs(targetFrameIndex) % (int)abs(columns) * itemSize.x,
          ((rows - 1) - (targetFrameIndex / (int)abs(columns))) * itemSize.y);

        return scaledUV + offset;
      }

			float2 GetAnimatedSpriteCoords(float2 uv, float startTime) {
				float2 dimensions = float2(_SpriteColumnCount, _SpriteRowCount);
				float2 itemSize = GetSpriteItemSize(dimensions);

        int spriteTileIndex = GetSpriteTargetIndex(_SpriteItemCount, _AnimationSpeed, startTime);

				return GetSpriteSheetCoords(uv, dimensions, spriteTileIndex, itemSize, _SpriteItemCount);
			}

      // Get a normal ranged value in [-1,1] range from a 0-1 percent value.
      float GetNormalRangeFromPercent(float percent) {
        return -1.0f + (2.0f * percent);
      }

      float3 GetDirectionFromPercent(float3 percentDir) {
        return float3(
          GetNormalRangeFromPercent(percentDir.x), 
          GetNormalRangeFromPercent(percentDir.y), 
          GetNormalRangeFromPercent(percentDir.z));
      }

			v2f vert (appdata v)
			{
				v2f o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o);

        float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f));        
        o.vertex = mul(UNITY_MATRIX_VP, worldPos);
				o.uv = v.uv;
        
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        UNITY_SETUP_INSTANCE_ID(i);

        float startTime = UNITY_ACCESS_INSTANCED_PROP(_StartTime_arr, _StartTime);
        float endTime = UNITY_ACCESS_INSTANCED_PROP(_EndTime_arr, _EndTime);
        float2 spriteUV = saturate(GetAnimatedSpriteCoords(i.uv, startTime));
        fixed4 spriteColor = tex2D(_SpriteSheetTex, spriteUV) * _Intensity * _TintColor;

        // Go transparent if the sprite completed its time loop.
        return spriteColor * step(_Time.y, endTime);
			}
      
			ENDCG
		}
	}
  FallBack "Unlit/Color"
}
