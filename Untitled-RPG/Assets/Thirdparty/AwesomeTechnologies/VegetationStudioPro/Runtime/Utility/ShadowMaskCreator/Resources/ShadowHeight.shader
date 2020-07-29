Shader "AwesomeTechnologies/Shadows/ShadowHeight"
{
	Properties
	{

	}
	
	SubShader
	{
		Cull Off
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 data : TEXCOORD0;
			};
							
			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.data.r = mul( unity_ObjectToWorld, v.vertex ).y;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				return i.data.r;
			}
			ENDCG
		}
	}
}
