Shader "AwesomeTechnologies/Grass/Grass-wind-a" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ColorB ("Color B", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_AG_ColorNoiseTex ("ColorNoise", 2D) = "white" {}
		_AG_ColorNoiseArea ("Color noise pos and area", Vector) = (0,0,0,1)
		_Cutoff("Cutoff" , Range(0,1)) = .5
		_Wetness("Wetness" , Range(0,1)) = 0
		_RandomDarkening("Random darkening" , Range(0,1)) = .5
		_RootAmbient("Root ambient" , Range(0,1)) = .5
		_Speed("Sine speed multiply" , Range(0,10)) = 5
		_WavesSpeed("Waves speed multiply" , Range(0,4)) = 1
		[Toggle] FAR_CULL("Use cull", Float) = 1
		_CullFarStart("Cull start" , Range(0,1000)) = 50 
		_CullFarDistance("Cull fade distance" , Range(0,200)) = 10
		_WindAffectDistance("Wind affect distance" , Range(0,200)) = 50
		[Toggle] TOUCH_BEND("Use touch bend", Float) = 1
	}

	SubShader {
		Tags { "RenderType"="TransparentCutout" "Queue"="Alphatest" }
		LOD 200
		
		Cull Off
		
		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows alphatest:_Cutoff vertex:vert addshadow
		#pragma target 3.0 
		#pragma instancing_options procedural:setup forwardadd
		#pragma multi_compile FAR_CULL_ON __
		#pragma multi_compile TOUCH_BEND_ON __
		
		#define AG_GRASS_MAXIMUM_WIND_WAVE_BEND 0.2
		#define AG_GRASS_SIN_NOISE_BEND 0.02
		
		// vertex components
		#define AG_PHASE_SHIFT v.color.g
		#define AG_BEND_FORCE  v.color.a
		#define AG_AO_MULTIPLIER v.color.r
		
		#ifdef FAR_CULL_ON
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				#define FAR_CULL_ON_PROCEDURAL_INSTANCING
			#else
				#define FAR_CULL_ON_SIMPLE
			#endif
		#endif
		 	
		#include "a_grass.cginc"
		#include "a_indirect.cginc"
		
		ENDCG
	}
	
	Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
