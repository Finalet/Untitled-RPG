Shader "Funly/Sky Studio/Weather/Rain Splash"
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
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
    Blend OneMinusDstColor One
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
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
      sampler2D _OverheadDepthTex;
      float4 _OverheadDepthPosition;
      float _OverheadDepthNearClip;
			float _OverheadDepthFarClip;
      float4 _OverheadDepthUV;
      float _SplashStartYPosition;

			int _SpriteColumnCount;
			int _SpriteRowCount;
			int _SpriteItemCount;
			int _AnimationSpeed;
      float _SpriteIndex;
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
				int spriteIndex = floor(_SpriteIndex);
        
				return GetSpriteSheetCoords(uv, dimensions, spriteIndex, itemSize, _SpriteItemCount);
			}

			v2f vert (appdata v)
			{
				v2f o;

        float4 checkUV = float4(_OverheadDepthUV.xy, 0.0f, 0.0f);
        float4 overInfo = tex2Dlod(_OverheadDepthTex, checkUV);

        float depthValue = overInfo.a;
        float distanceFromCamera = _OverheadDepthNearClip + _OverheadDepthFarClip * depthValue;

        float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f));

        float adjustedOrigin = _OverheadDepthPosition.y - distanceFromCamera;

        worldPos.y += adjustedOrigin - _SplashStartYPosition;

        o.vertex = mul(UNITY_MATRIX_VP, worldPos);

				o.uv = v.uv;
        
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 spriteUV = saturate(GetAnimatedSpriteCoords(i.uv));
				return tex2D(_MainTex, spriteUV) * _Intensity;
			}
      
			ENDCG
		}
	}
	Fallback "Unlit/Color"
}
