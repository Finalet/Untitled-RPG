// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Author: Jason Ederle
// Description: Sky Studio rain rendering.

Shader "Funly/Sky Studio/Weather/Rain Splash Instanced"
{
	Properties
	{
		[NoScaleOffset] _SpriteSheetTex ("Sprite Texture", 2D) = "white" {}
		_SpriteColumnCount ("Sprite Columns", int) = 1
		_SpriteRowCount ("Sprite Rows", int) = 1
		_SpriteItemCount ("Sprite Total Items", int) = 1
		_AnimationSpeed ("Animation Speed", int) = 25
    _Intensity ("Intensity", Range(0, 1)) = .7
    _TintColor("Tint Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
    Blend OneMinusDstColor One
		Cull Back

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
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
                float isOverSurface: TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

	  sampler2D _SpriteSheetTex;
      sampler2D _OverheadDepthTex;
      float4 _OverheadDepthPosition;
      float _OverheadDepthNearClip;
			float _OverheadDepthFarClip;
      float4 _TintColor;

			int _SpriteColumnCount;
			int _SpriteRowCount;
			int _SpriteItemCount;
			int _AnimationSpeed;
      float _Intensity;
      float _SplashGroundOffset;
      float _Scale;

      UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float, _StartTime)
#define _StartTime_arr Props
        UNITY_DEFINE_INSTANCED_PROP(float, _EndTime)
#define _EndTime_arr Props
        UNITY_DEFINE_INSTANCED_PROP(float, _OverheadDepthU)
#define _OverheadDepthU_arr Props
        UNITY_DEFINE_INSTANCED_PROP(float, _OverheadDepthV)
#define _OverheadDepthV_arr Props
        UNITY_DEFINE_INSTANCED_PROP(float, _SplashStartYPosition)
#define _SplashStartYPosition_arr Props
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
        int spriteTileIndex = GetSpriteTargetIndex(_SpriteItemCount, _AnimationSpeed, startTime);
				float2 dimensions = float2(_SpriteColumnCount, _SpriteRowCount);
				float2 itemSize = GetSpriteItemSize(dimensions);
        
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

        float2 depthUV = float2(UNITY_ACCESS_INSTANCED_PROP(_OverheadDepthU_arr, _OverheadDepthU), UNITY_ACCESS_INSTANCED_PROP(_OverheadDepthV_arr, _OverheadDepthV));
        float4 checkUV = float4(depthUV, 0.0f, 0.0f);
        float4 overInfo = tex2Dlod(_OverheadDepthTex, checkUV);

        float depthValue = overInfo.a;
        float distanceFromCamera = _OverheadDepthNearClip + _OverheadDepthFarClip * depthValue;

        float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f));

        // BEGIN TESTING.
        float3 vertex = v.vertex.xyz;

        float3 surfaceNormal = GetDirectionFromPercent(overInfo.xyz);
        float3 up = normalize(surfaceNormal);
        float3 forward = normalize(cross(up, float3(1.0f, 0.0f, 0.0f)));
        float3 right = normalize(cross(up, forward));

        // Extract the original scale from the right axis vector.
        float scale = length(float3(unity_ObjectToWorld[0][0], unity_ObjectToWorld[0][1], unity_ObjectToWorld[0][1]));

        // Restore the scale values.
        up *= scale;
        forward *= scale;
        right *= scale;

        float4x4 objToWorld = unity_ObjectToWorld;
        objToWorld[0][0] = right.x;
        objToWorld[0][1] = right.y;
        objToWorld[0][2] = right.z;
        objToWorld[1][0] = up.x;
        objToWorld[1][1] = up.y;
        objToWorld[1][2] = up.z;
        objToWorld[2][0] = forward.x;
        objToWorld[2][1] = forward.y;
        objToWorld[2][2] = forward.z;

        worldPos = mul(objToWorld, float4(vertex, 1.0f));
        
        // END TESTING
        //worldPos = mul(unity_ObjectToWorld, float4(vertex.xyz, 1.0f));

        float adjustedOrigin = _OverheadDepthPosition.y - distanceFromCamera;
        worldPos.y += adjustedOrigin - UNITY_ACCESS_INSTANCED_PROP(_SplashStartYPosition_arr, _SplashStartYPosition);
        worldPos.xyz = worldPos.xyz + (surfaceNormal * _SplashGroundOffset);
        
        o.vertex = mul(UNITY_MATRIX_VP, worldPos);

        // Easy way to detect no collision is that empty fill color sets -1,-1,-1 which isn't normalized length of 1.
        o.isOverSurface = step(length(surfaceNormal), 1.1f) - .5f;
				o.uv = v.uv;
        
				return o;
			}
			
		fixed4 frag (v2f i) : SV_Target
		{
        UNITY_SETUP_INSTANCE_ID(i);

        clip(i.isOverSurface);

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
