Shader "AwesomeTechnologies/TouchReact/RenderTouchBufferInstanced"
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
			#pragma multi_compile_instancing
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 data : TEXCOORD0;
			};
			
			float4 _TB_Pos;
			
			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.data.r = mul( unity_ObjectToWorld, v.vertex ).y/10000;
				o.data.g = 1;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				return i.data;
			}
			ENDCG

		}
	}
}
