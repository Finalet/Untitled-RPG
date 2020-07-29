// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

	
        // material
        sampler2D	_MainTex;
        fixed4		_Color;
        fixed4		_ColorB;
        fixed		_RandomDarkening;
        fixed		_RootAmbient;
        fixed		_Speed;
        fixed		_WavesSpeed;
        float	    _Wetness;
        
        //#ifdef FAR_CULL_ON
        //fixed		_CullFarStart;
        //fixed		_CullFarDistance;
        //#endif
        
        fixed		_WindAffectDistance;
        
        // color areas
        sampler2D	_AG_ColorNoiseTex;
        float4		_AG_ColorNoiseArea;
        
        // wind uniforms
        sampler2D	_AW_WavesTex;
        float4		_AW_DIR;
        
        // touch react
        #ifdef TOUCH_BEND_ON
        sampler2D	_TouchReact_Buffer;
        float4		_TouchReact_Pos;		
        #endif
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 color : COLOR;
            float4 modifiedColor;
        };
        
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        inline fixed rand(float2 co)
        {
            return frac(sin(dot(co ,half2(12.9898, 78.233))) * 43758.5453);
        }
                
        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);

				float3 transformPosition = mul(unity_ObjectToWorld, float4(0,0,0,1));
            
				float distanceToCamera = length(transformPosition - _WorldSpaceCameraPos.xyz);
            
				//#ifdef FAR_CULL_ON_SIMPLE
				//float cull = 1.0 - saturate((distanceToCamera - _CullFarStart)/ _CullFarDistance);			 

				//if(cull > 0)
				//{
				//#endif
            
					float3 pos = mul(unity_ObjectToWorld, v.vertex);
					float3 result = pos;
                
					#ifdef TOUCH_BEND_ON
						float2 tbPos = saturate((float2(pos.x,-pos.z) - _TouchReact_Pos.xz)/_TouchReact_Pos.w);
						float2 touchBend  = tex2Dlod(_TouchReact_Buffer, float4(tbPos,0,0));
                    
						touchBend.y *= 1.0 - length(tbPos - 0.5) * 2; // clip texture "clamp" bugs
                    
						if(touchBend.y > 0.01)
							result.y = min(result.y, touchBend.x * 10000);
					#endif
                
					UNITY_BRANCH
					if(distanceToCamera < _WindAffectDistance)
					{
            
						#define FORCE _AW_DIR.y
						#define AW_WavesSize _AW_DIR.w
                    
						#define AW_wavesPos (pos.xz - _AW_DIR.xz * _Time.y * FORCE * _WavesSpeed)
						#define AW_windWave (tex2Dlod(_AW_WavesTex, float4(AW_wavesPos/AW_WavesSize,0,0)).r)
                    
						float force = FORCE * AW_windWave;
						float time = _Time.y * FORCE * _Speed;
                    
						float baseBendSin = sin(time + AG_PHASE_SHIFT * 6.28319);
                    
						// main bend calculation
						#define AG_BaseBend (saturate(force) * AG_GRASS_MAXIMUM_WIND_WAVE_BEND + baseBendSin * AG_GRASS_SIN_NOISE_BEND)
						#define AG_BigBend (AG_BaseBend * AG_BEND_FORCE)
                    
						//#define AG_objSpaceWindDir (mul(unity_WorldToObject, float4(_AW_DIR.x, 0, _AW_DIR.z, 0)).xz)
                    
						//v.vertex.xz += AG_objSpaceWindDir * AG_BigBend;
						result.xz += _AW_DIR.xz * AG_BigBend;                
					}
                
					// AO + brightness randomizer
					o.modifiedColor.a = saturate(rand(pos.xz)+_RandomDarkening) * saturate(_RootAmbient + AG_AO_MULTIPLIER);
                
					// read color areas from texture and lerp colors
					#define AG_ColorNoise_R tex2Dlod (_AG_ColorNoiseTex, float4((pos.xz - _AG_ColorNoiseArea.xz)/_AG_ColorNoiseArea.y,0,0)).r
					o.modifiedColor.rgb = lerp(_Color.rgb, _ColorB.rgb, AG_ColorNoise_R) * o.modifiedColor.a;
                            
					//v.normal = mul(unity_WorldToObject, float4(0, 1, 0, 0));
					v.normal = float3(0,1,0);

					v.tangent.xyz = cross(v.normal,float3(0,0,1));
					// ----
                
					v.vertex.xyz = mul(unity_WorldToObject, float4(result,1)).xyz;
            
				//#ifdef FAR_CULL_ON_SIMPLE
				//}
				//v.vertex.xyz *= cull;
				//#endif								
        }
      
        void WaterBRDF(inout half3 Albedo, inout half Smoothness, half wetFactor, half surfPorosity)
        {
            half porosity = saturate((((1 - Smoothness) - 0.5)) / max(surfPorosity, 0.001));
            half factor = lerp(1, 0.2, porosity);
            Albedo *= lerp(1.0, factor, wetFactor);
            Smoothness = lerp(1.0, Smoothness, lerp(1.0, factor, wetFactor));
        }

        void surf(Input IN, inout SurfaceOutput o)
        //void surf(Input IN, inout SurfaceOutputStandard o) //SurfaceOutputStandard
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);		
            o.Albedo = c.rgb * IN.modifiedColor.rgb;

            //o.Smoothness = Luminance(o.Albedo);
            //WaterBRDF(o.Albedo, o.Smoothness, _Wetness, 0.4);

            o.Alpha = c.a;
        }
		
		
		
		