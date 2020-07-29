Shader "AwesomeTechnologies/TouchReact/RenderTouchBuffer"
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
			//#pragma geometry geom

			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 data : TEXCOORD0;
			};
			
			float4 _TB_Pos;

			//[maxvertexcount(3)]
			//void geom(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
			//{

			//	float3 center = (input[0].pos.xyz + input[1].pos.xyz + input[2].pos.xyz) / 3;

			//	for (int i = 0; i < 3; i++)
			//	{
			//		input[i].pos.x = center.x + (input[i].pos.x - center.x)*2;
			//		//input[i].pos.y = center.y + (input[i].pos.y - center.y);
			//		input[i].pos.z = center.z + (input[i].pos.z - center.z)*2;
			//		OutputStream.Append(input[i]);
			//	}
			//}

			
			v2f vert(appdata_base v)
			{
				v2f o;
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
