// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AwesomeTechnologies/Grass/GrassPatchVertexColor"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha  noshadow novertexlights nolightmap  nodynlightmap nodirlightmap 
		struct Input
		{
			float4 vertexColor : COLOR;
		};

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( s.Emission, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = i.vertexColor.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}	
}
/*ASEBEGIN
Version=6104
-1695;125;1646;841;329;296;1;True;True
Node;AmplifyShaderEditor.VertexColorNode;1;88,-25;Float;False;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;643,-13;Float;False;True;2;Float;ASEMaterialInspector;0;Unlit;AwesomeTechnologies/GrassPatchVertexColor;False;False;False;False;False;True;True;True;True;False;False;False;Off;0;0;False;0;0;Opaque;0.5;True;False;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;False;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0.01;0,0,0,0;VertexScale;False;Cylindrical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;0;2;1;0
ASEEND*/
//CHKSM=B42F83F7CE2B3CFAB050C7C12E3457FC6B8FF922