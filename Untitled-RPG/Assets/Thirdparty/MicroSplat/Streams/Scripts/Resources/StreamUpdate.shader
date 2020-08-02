//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

Shader "Hidden/MicroSplat/StreamUpdate"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
      _TerrainDesc("Normals", 2D) = "black" {}
      _SpawnStrength("Spawn Strength", Vector) = (1, 1, 0, 0)
      _Evaporation("Evaporation", Vector) = (0.03, 0.03, 0, 0)
      _Speed("Speed", Vector) = (1, 1, 0, 0)
      _Resistance("Resistance", Vector) = (0.5, 0.5, 0, 0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

         sampler2D_half _TerrainDesc;
         float4 _TerrainDesc_TexelSize;
         half2 _SpawnStrength;
         half2 _Evaporation;
         float _DeltaTime;
         float2 _Speed;
         float2 _Resistance;

         float2 _TexelSize;
         float4 _Positions[64];
         int _PositionsCount;

         float4 _Colliders[64];
         int _CollidersCount;

         half _WetnessEvaporation;
         half _BurnEvaporation;


         half2 Sample(int x, int y, float height, float2 origUV, half4 curData, float2 dt, half2 collider)
         {
            float2 offset = float2(x,y);
            offset *= _TerrainDesc_TexelSize.xy;
            float2 uv = saturate(origUV + offset);
            half4 worldNormal = tex2D(_TerrainDesc, uv);
            half2 nData = tex2D(_MainTex, uv).zw;

            float2 curH = height + curData.zw;
            float2 newH = worldNormal.a + nData;
            float2 ffac = ((newH  - curH)) / 4.0;
            collider *= 0.75;
            ffac -= collider;
            float2 sgn = sign(ffac);
            ffac = pow(abs(ffac), _Resistance);
            ffac *= sgn;

            // min spread

            ffac *= dt;

            if (ffac.x > 0)
            {
               ffac.x = max(ffac.x, (nData.x > 0) ? 1.0/400.0 : 0);
            }
            else if (ffac.x < 0)
            {
               ffac.x = min(ffac.x, (nData.x < 0) ? -1.0/400 : 0);
            }
            if (ffac.y >= 0)
            {
               ffac.y = max(ffac.y, (nData.y > 0) ? 1.0/400.0 : 0);
            }
            else if (ffac.y < 0)
            {
               ffac.y = min(ffac.x, (nData.y < 0) ? -1.0/400 : 0);
            }
            return nData * ffac * 0.6;
         } 

         half4 GetSpawners(float2 uv)
         {
            float2 pos = uv * _TerrainDesc_TexelSize.zw;
            fixed4 val = fixed4(0,0,0,0);
            for (int x = 0; x < _PositionsCount; ++x)
            {
               float4 p = _Positions[x];
               float dist = distance(p.xy, pos);
               if (dist < p.z)
               {
                  val.z = dist;
               }
               if (dist < p.w)
               {
                  val.w = dist;
               }
            }

            for (int y = 0; y < _CollidersCount; ++y)
            {
               half4 p = _Colliders[y];

               if (distance(p.xy, pos) < p.z)
               {
                  val.z = 0;
                  val.x = 1;
               }
               if (distance(p.xy, pos) < p.w)
               {
                  val.w = 0;
                  val.y = 1;
               }
            }

            return val;
         }

			fixed4 frag (v2f i) : SV_Target
			{
            // Could do in two passes like a gausian? 
            // Pretty fast as is though..
            half4 curData = tex2D(_MainTex, i.uv);
            half4 wnvh = tex2D(_TerrainDesc, i.uv);
            half height = wnvh.a;

            half4 spawner = GetSpawners(i.uv);

            float2 dt = _DeltaTime * _Speed;
            float2 diff = 0;
            diff += _SpawnStrength * spawner.zw * dt;
            diff += Sample(-1, -1, height, i.uv, curData, dt, spawner.xy) * 0.66;
				diff += Sample(0, -1, height, i.uv, curData, dt, spawner.xy);
            diff += Sample(1, -1, height, i.uv, curData, dt, spawner.xy) * 0.66;
            diff += Sample(-1, 0, height, i.uv, curData, dt, spawner.xy);
            diff += Sample(1, 0, height, i.uv, curData, dt, spawner.xy);
            diff += Sample(-1, 1, height, i.uv, curData, dt, spawner.xy) * 0.66;
            diff += Sample(0, 1, height, i.uv, curData, dt, spawner.xy);
            diff += Sample(1, 1, height, i.uv, curData, dt, spawner.xy) * 0.66;

            diff -= dt * _Evaporation * max(0, curData.wz) * 17;

            diff.x -= _Evaporation.x;
            diff.y -= _Evaporation.y;
            curData.zw += diff;

            curData.xy += max(0, curData.zw*dt);
            curData.x -= _WetnessEvaporation;
            curData.y -= _BurnEvaporation;;

            return saturate(curData);
			}
			ENDCG
		}
	}
}
