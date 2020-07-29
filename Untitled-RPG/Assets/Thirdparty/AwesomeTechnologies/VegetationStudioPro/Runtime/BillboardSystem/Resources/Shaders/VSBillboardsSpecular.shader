Shader "AwesomeTechnologies/Billboards/GroupBillboardsSpecular"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Bump ("Bump", 2D) = "white" {}
		//_AOTex("AO (RGB)", 2D) = "white" {}
		_HueVariation ("Hue variation", Color) = (1,1,1,.5)
		_Cutoff("Cutoff" , Range(0,1)) = .5
		_NormalStrength("Normal Strength" , Range(0,5)) = 1
		_Brightness("Brightness" , Range(0,5)) = 1
		_Smoothness("Smoothness" , Range(0,1)) = 0
	    _Occlusion("Occlusion" , Range(0,1)) = 1
		_Specular("Specular" , Range(0,1)) = 0.5
		_MipmapBias("Mipmap bias" , Range(-3,0)) = -2
		_CullDistance("Near cull distance",Float) = 0
		_FarCullDistance("Near cull distance",Float) = 0
		_InRow("Frames in row", Int) = 8
		_InCol("Frames in column", Int) = 8
		_CameraPosition("Camera position",Vector) = (0,0,0,0)
		_SnowAmount("Snow area", Range(0,1)) = 0.5
		_SnowColor("Snow Color", Color) = (1,1,1,1)
		_SnowBlendFactor("Snow Blend Factor", Range(0,10)) = 3
		_SnowBrightness("Snow Brightness" , Range(0,5)) = 1
		[KeywordEnum(ON, OFF)] AT_HUE_VARIATION ("Use SpeedTree HUE variation", Float) = 0
		_BillboardWindSpeed("Billboard wind speed" , Range(0,5)) = 1
		
		
		
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" "Queue"="Alphatest" "DisableBatching"="True" "IgnoreProjector"="True"}
		//AlphaToMask On
		LOD 200
		
		CGPROGRAM

		//#pragma surface surf Lambert noforwardadd alphatest:_Cutoff vertex:vert addshadow
		#pragma surface surfSpecular StandardSpecular noforwardadd  vertex:vert addshadow alphatest:_Cutoff
		#pragma multi_compile AT_HUE_VARIATION_ON AT_HUE_VARIATION_OFF
		#pragma multi_compile _ LOD_FADE_CROSSFADE
		#pragma multi_compile _ USE_SNOW
		#pragma multi_compile _ USE_HDWIND
		#pragma target 4.0
				 
		#include "VSBillboards.cginc"					
					
		ENDCG
	}
	
	FallBack "Diffuse"
}
