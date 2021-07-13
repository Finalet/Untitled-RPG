Shader "Funly/Sky Studio/Utility/Animated Sprite Sheet"
{
	Properties
	{
		[NoScaleOffset] _MainTex ("Sprite Texture", 2D) = "white" {}
		_SpriteColumnCount ("Sprite Columns", int) = 1
		_SpriteRowCount ("Sprite Rows", int) = 1
		_SpriteItemCount ("Sprite Total Items", int) = 1
		_AnimationSpeed ("Animation Speed", int) = 25
    _Intensity ("Intensity", Range(0, 1)) = .7
	}
	SubShader
	{
    Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
    LOD 100
    Blend OneMinusDstColor One
    Cull Off

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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
      int _SpriteColumnCount;
      int _SpriteRowCount;
      int _SpriteItemCount;
      int _AnimationSpeed;
      float _Intensity;

      int GetSpriteTargetIndex(int itemCount, int animationSpeed, float seed) {
        float spriteProgress = frac((_Time.y + (10.0f * seed)) / ((float)itemCount / (float)animationSpeed));
        return (int)(itemCount * spriteProgress);
      }

      float2 GetSpriteItemSize(float2 dimensions) {
        return float2(1.0f / dimensions.x, (1.0f / dimensions.x) * (dimensions.x / dimensions.y));
      }

      float2 GetSpriteSheetCoords(float2 uv, float2 dimensions, int targetFrameIndex, float2 itemSize, int numItems) {
        int rows = dimensions.y;
        int columns = dimensions.x;

        float2 scaledUV = float2(uv.x * itemSize.x, uv.y * itemSize.y);
        float2 offset = float2(
          targetFrameIndex % abs(columns) * itemSize.x,
          ((rows - 1) - (targetFrameIndex / abs(columns))) * itemSize.y);

        return scaledUV + offset;
      }

      float2 GetAnimatedSpriteCoords(float2 uv) {
        float2 dimensions = float2(_SpriteColumnCount, _SpriteRowCount);
        float2 itemSize = GetSpriteItemSize(dimensions);
        int spriteIndex = GetSpriteTargetIndex(_SpriteItemCount, _AnimationSpeed, 1.0f);

        return GetSpriteSheetCoords(uv, dimensions, spriteIndex, itemSize, _SpriteItemCount);
      }

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = saturate(GetAnimatedSpriteCoords(v.uv));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        return tex2D(_MainTex, i.uv) * _Intensity;
			}
			ENDCG
		}
	}
}
