// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Color4x4"
{
	Properties
	{
		_Color1("Color 1", Color) = (1,0.1544118,0.1544118,0.291)
		_Color2("Color 2", Color) = (1,0.1544118,0.8017241,0.253)
		_Color3("Color 3", Color) = (0.2535501,0.1544118,1,0.541)
		_Color4("Color 4", Color) = (0.1544118,0.5451319,1,0.253)
		_Color5("Color 5", Color) = (0.9533468,1,0.1544118,0.553)
		_Color6("Color 6", Color) = (0.2720588,0.1294625,0,0.097)
		_Color7("Color 7", Color) = (0.1544118,0.6151115,1,0.178)
		_Color8("Color 8", Color) = (0.4849697,0.5008695,0.5073529,0.078)
		_Color9("Color 9", Color) = (0.3164301,0,0.7058823,0.134)
		_Color10("Color 10", Color) = (0.362069,0.4411765,0,0.759)
		_Color11("Color 11", Color) = (0.6691177,0.6691177,0.6691177,0.647)
		_Color12("Color 12", Color) = (0.5073529,0.1574544,0,0.128)
		_Color13("Color 13", Color) = (1,0.5586207,0,0.272)
		_Color14("Color 14", Color) = (0,0.8025862,0.875,0.047)
		_Color15("Color 15", Color) = (1,0,0,0.391)
		_Color16("Color 16", Color) = (0.4080882,0.75,0.4811866,0.134)
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_Metallic("Metallic", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color1;
		uniform float4 _Color2;
		uniform float4 _Color3;
		uniform float4 _Color4;
		uniform float4 _Color5;
		uniform float4 _Color6;
		uniform float4 _Color7;
		uniform float4 _Color8;
		uniform float4 _Color9;
		uniform float4 _Color10;
		uniform float4 _Color11;
		uniform float4 _Color12;
		uniform float4 _Color13;
		uniform float4 _Color14;
		uniform float4 _Color15;
		uniform float4 _Color16;
		uniform float _Metallic;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_3_0_g250 = 1.0;
			float temp_output_7_0_g250 = 4.0;
			float temp_output_9_0_g250 = 4.0;
			float temp_output_8_0_g250 = 4.0;
			float temp_output_3_0_g254 = 2.0;
			float temp_output_7_0_g254 = 4.0;
			float temp_output_9_0_g254 = 4.0;
			float temp_output_8_0_g254 = 4.0;
			float temp_output_3_0_g251 = 3.0;
			float temp_output_7_0_g251 = 4.0;
			float temp_output_9_0_g251 = 4.0;
			float temp_output_8_0_g251 = 4.0;
			float temp_output_3_0_g244 = 4.0;
			float temp_output_7_0_g244 = 4.0;
			float temp_output_9_0_g244 = 4.0;
			float temp_output_8_0_g244 = 4.0;
			float temp_output_3_0_g245 = 1.0;
			float temp_output_7_0_g245 = 4.0;
			float temp_output_9_0_g245 = 3.0;
			float temp_output_8_0_g245 = 4.0;
			float temp_output_3_0_g247 = 2.0;
			float temp_output_7_0_g247 = 4.0;
			float temp_output_9_0_g247 = 3.0;
			float temp_output_8_0_g247 = 4.0;
			float temp_output_3_0_g246 = 3.0;
			float temp_output_7_0_g246 = 4.0;
			float temp_output_9_0_g246 = 3.0;
			float temp_output_8_0_g246 = 4.0;
			float temp_output_3_0_g242 = 4.0;
			float temp_output_7_0_g242 = 4.0;
			float temp_output_9_0_g242 = 3.0;
			float temp_output_8_0_g242 = 4.0;
			float temp_output_3_0_g256 = 1.0;
			float temp_output_7_0_g256 = 4.0;
			float temp_output_9_0_g256 = 2.0;
			float temp_output_8_0_g256 = 4.0;
			float temp_output_3_0_g249 = 2.0;
			float temp_output_7_0_g249 = 4.0;
			float temp_output_9_0_g249 = 2.0;
			float temp_output_8_0_g249 = 4.0;
			float temp_output_3_0_g243 = 3.0;
			float temp_output_7_0_g243 = 4.0;
			float temp_output_9_0_g243 = 2.0;
			float temp_output_8_0_g243 = 4.0;
			float temp_output_3_0_g253 = 4.0;
			float temp_output_7_0_g253 = 4.0;
			float temp_output_9_0_g253 = 2.0;
			float temp_output_8_0_g253 = 4.0;
			float temp_output_3_0_g252 = 1.0;
			float temp_output_7_0_g252 = 4.0;
			float temp_output_9_0_g252 = 1.0;
			float temp_output_8_0_g252 = 4.0;
			float temp_output_3_0_g255 = 2.0;
			float temp_output_7_0_g255 = 4.0;
			float temp_output_9_0_g255 = 1.0;
			float temp_output_8_0_g255 = 4.0;
			float temp_output_3_0_g248 = 3.0;
			float temp_output_7_0_g248 = 4.0;
			float temp_output_9_0_g248 = 1.0;
			float temp_output_8_0_g248 = 4.0;
			float temp_output_3_0_g257 = 4.0;
			float temp_output_7_0_g257 = 4.0;
			float temp_output_9_0_g257 = 1.0;
			float temp_output_8_0_g257 = 4.0;
			float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g250 - 1.0 ) / temp_output_7_0_g250 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g250 / temp_output_7_0_g250 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g250 - 1.0 ) / temp_output_8_0_g250 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g250 / temp_output_8_0_g250 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g254 - 1.0 ) / temp_output_7_0_g254 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g254 / temp_output_7_0_g254 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g254 - 1.0 ) / temp_output_8_0_g254 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g254 / temp_output_8_0_g254 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g251 - 1.0 ) / temp_output_7_0_g251 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g251 / temp_output_7_0_g251 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g251 - 1.0 ) / temp_output_8_0_g251 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g251 / temp_output_8_0_g251 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g244 - 1.0 ) / temp_output_7_0_g244 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g244 / temp_output_7_0_g244 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g244 - 1.0 ) / temp_output_8_0_g244 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g244 / temp_output_8_0_g244 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g245 - 1.0 ) / temp_output_7_0_g245 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g245 / temp_output_7_0_g245 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g245 - 1.0 ) / temp_output_8_0_g245 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g245 / temp_output_8_0_g245 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g247 - 1.0 ) / temp_output_7_0_g247 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g247 / temp_output_7_0_g247 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g247 - 1.0 ) / temp_output_8_0_g247 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g247 / temp_output_8_0_g247 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g246 - 1.0 ) / temp_output_7_0_g246 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g246 / temp_output_7_0_g246 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g246 - 1.0 ) / temp_output_8_0_g246 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g246 / temp_output_8_0_g246 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g242 - 1.0 ) / temp_output_7_0_g242 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g242 / temp_output_7_0_g242 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g242 - 1.0 ) / temp_output_8_0_g242 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g242 / temp_output_8_0_g242 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g256 - 1.0 ) / temp_output_7_0_g256 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g256 / temp_output_7_0_g256 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g256 - 1.0 ) / temp_output_8_0_g256 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g256 / temp_output_8_0_g256 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g249 - 1.0 ) / temp_output_7_0_g249 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g249 / temp_output_7_0_g249 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g249 - 1.0 ) / temp_output_8_0_g249 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g249 / temp_output_8_0_g249 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g243 - 1.0 ) / temp_output_7_0_g243 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g243 / temp_output_7_0_g243 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g243 - 1.0 ) / temp_output_8_0_g243 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g243 / temp_output_8_0_g243 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g253 - 1.0 ) / temp_output_7_0_g253 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g253 / temp_output_7_0_g253 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g253 - 1.0 ) / temp_output_8_0_g253 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g253 / temp_output_8_0_g253 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g252 - 1.0 ) / temp_output_7_0_g252 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g252 / temp_output_7_0_g252 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g252 - 1.0 ) / temp_output_8_0_g252 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g252 / temp_output_8_0_g252 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g255 - 1.0 ) / temp_output_7_0_g255 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g255 / temp_output_7_0_g255 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g255 - 1.0 ) / temp_output_8_0_g255 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g255 / temp_output_8_0_g255 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g248 - 1.0 ) / temp_output_7_0_g248 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g248 / temp_output_7_0_g248 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g248 - 1.0 ) / temp_output_8_0_g248 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g248 / temp_output_8_0_g248 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( i.uv_texcoord.x , ( ( temp_output_3_0_g257 - 1.0 ) / temp_output_7_0_g257 ) ) ) * ( step( i.uv_texcoord.x , ( temp_output_3_0_g257 / temp_output_7_0_g257 ) ) * 1.0 ) ) * ( ( 1.0 - step( i.uv_texcoord.y , ( ( temp_output_9_0_g257 - 1.0 ) / temp_output_8_0_g257 ) ) ) * ( step( i.uv_texcoord.y , ( temp_output_9_0_g257 / temp_output_8_0_g257 ) ) * 1.0 ) ) ) ) ) );
			o.Albedo = temp_output_155_0.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = ( (temp_output_155_0).a * _Smoothness );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
654;115;1906;839;-832.7686;-1073.366;1.350566;True;True
Node;AmplifyShaderEditor.ColorNode;181;-218.8154,2174.284;Float;False;Property;_Color11;Color 11;10;0;Create;True;0;0;False;0;0.6691177,0.6691177,0.6691177,0.647;0.1868512,0.3920654,0.7941176,0.378;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;182;-220.2247,2417.44;Float;False;Property;_Color12;Color 12;11;0;Create;True;0;0;False;0;0.5073529,0.1574544,0,0.128;0.03092561,0.3096439,0.3823529,0.128;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;183;-224.4024,1681.061;Float;False;Property;_Color9;Color 9;8;0;Create;True;0;0;False;0;0.3164301,0,0.7058823,0.134;0.5735294,0.9117646,1,0.822;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;180;-232.3431,1940.419;Float;False;Property;_Color10;Color 10;9;0;Create;True;0;0;False;0;0.362069,0.4411765,0,0.759;0.1552228,0.1600571,0.2132353,0.291;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;218;-229.103,3176.23;Float;False;Property;_Color15;Color 15;14;0;Create;True;0;0;False;0;1,0,0,0.391;0.3441285,0.4926471,0.4311911,0.397;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;217;-264.3738,3419.386;Float;False;Property;_Color16;Color 16;15;0;Create;True;0;0;False;0;0.4080882,0.75,0.4811866,0.134;0.1089965,0.2584653,0.4632353,0.709;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;213;-234.6901,2683.007;Float;False;Property;_Color13;Color 13;12;0;Create;True;0;0;False;0;1,0.5586207,0,0.272;0.2132353,0.2053958,0.2053958,0.272;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;214;-242.6307,2942.365;Float;False;Property;_Color14;Color 14;13;0;Create;True;0;0;False;0;0,0.8025862,0.875,0.047;1,0.8118661,0.6102941,0.047;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;152;-194.2135,166.9271;Float;False;Property;_Color3;Color 3;2;0;Create;True;0;0;False;0;0.2535501,0.1544118,1,0.541;0.1868512,0.3920654,0.7941176,0.378;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;154;-195.6228,411.2479;Float;False;Property;_Color4;Color 4;3;0;Create;True;0;0;False;0;0.1544118,0.5451319,1,0.253;0.5004326,0.5788032,0.7647059,0.872;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-199.8005,-326.2955;Float;False;Property;_Color1;Color 1;0;0;Create;True;0;0;False;0;1,0.1544118,0.1544118,0.291;0.08937068,0.3892086,0.6397059,0.653;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;150;-207.7412,-66.93771;Float;False;Property;_Color2;Color 2;1;0;Create;True;0;0;False;0;1,0.1544118,0.8017241,0.253;0.4246324,0.5544625,0.5661765,0.378;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;157;-182.3802,1181.25;Float;False;Property;_Color7;Color 7;6;0;Create;True;0;0;False;0;0.1544118,0.6151115,1,0.178;0.616782,0.6616514,0.6764706,0.378;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;158;-183.7895,1424.406;Float;False;Property;_Color8;Color 8;7;0;Create;True;0;0;False;0;0.4849697,0.5008695,0.5073529,0.078;0.08937068,0.3892086,0.6397059,0.653;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;159;-187.9672,688.0273;Float;False;Property;_Color5;Color 5;4;0;Create;True;0;0;False;0;0.9533468,1,0.1544118,0.553;0.4184148,0.716347,0.8014706,0.309;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;156;-195.9079,947.3851;Float;False;Property;_Color6;Color 6;5;0;Create;True;0;0;False;0;0.2720588,0.1294625,0,0.097;0.1201882,0.5540434,0.8602941,0.278;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;162;133.8517,1429.247;Float;True;ColorShartSlot;-1;;242;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;186;96.90227,2179.125;Float;True;ColorShartSlot;-1;;243;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;153;122.0185,414.924;Float;True;ColorShartSlot;-1;;244;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;163;127.7504,692.868;Float;True;ColorShartSlot;-1;;245;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;161;133.3375,1186.091;Float;True;ColorShartSlot;-1;;246;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;160;119.8096,952.2258;Float;True;ColorShartSlot;-1;;247;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;224;86.61465,3181.071;Float;True;ColorShartSlot;-1;;248;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;187;83.37437,1945.26;Float;True;ColorShartSlot;-1;;249;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;145;115.9171,-321.4549;Float;True;ColorShartSlot;-1;;250;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;151;121.5042,171.7677;Float;True;ColorShartSlot;-1;;251;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;216;81.02762,2687.848;Float;True;ColorShartSlot;-1;;252;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;185;97.41646,2422.281;Float;True;ColorShartSlot;-1;;253;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;149;107.9764,-62.09709;Float;True;ColorShartSlot;-1;;254;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;223;73.08682,2945.046;Float;True;ColorShartSlot;-1;;255;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;188;91.31517,1685.902;Float;True;ColorShartSlot;-1;;256;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;222;87.12894,3424.227;Float;True;ColorShartSlot;-1;;257;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;146;1539.255,777.6315;Float;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;184;1537.758,1310.802;Float;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;1539.944,1043.66;Float;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;225;1534.365,1575.009;Float;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;1964.993,1140.165;Float;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;166;1887.168,1900.592;Float;False;Property;_Smoothness;Smoothness;16;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;227;1935.602,1617.235;Float;True;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;165;2014.597,1413.642;Float;False;Property;_Metallic;Metallic;17;0;Create;True;0;0;False;0;0;0.387;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;2229.031,1787.579;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2469.067,1277.475;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Malbers/Color4x4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.1;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;162;38;158;0
WireConnection;186;38;181;0
WireConnection;153;38;154;0
WireConnection;163;38;159;0
WireConnection;161;38;157;0
WireConnection;160;38;156;0
WireConnection;224;38;218;0
WireConnection;187;38;180;0
WireConnection;145;38;23;0
WireConnection;151;38;152;0
WireConnection;216;38;213;0
WireConnection;185;38;182;0
WireConnection;149;38;150;0
WireConnection;223;38;214;0
WireConnection;188;38;183;0
WireConnection;222;38;217;0
WireConnection;146;0;145;0
WireConnection;146;1;149;0
WireConnection;146;2;151;0
WireConnection;146;3;153;0
WireConnection;184;0;188;0
WireConnection;184;1;187;0
WireConnection;184;2;186;0
WireConnection;184;3;185;0
WireConnection;164;0;163;0
WireConnection;164;1;160;0
WireConnection;164;2;161;0
WireConnection;164;3;162;0
WireConnection;225;0;216;0
WireConnection;225;1;223;0
WireConnection;225;2;224;0
WireConnection;225;3;222;0
WireConnection;155;0;146;0
WireConnection;155;1;164;0
WireConnection;155;2;184;0
WireConnection;155;3;225;0
WireConnection;227;0;155;0
WireConnection;212;0;227;0
WireConnection;212;1;166;0
WireConnection;0;0;155;0
WireConnection;0;3;165;0
WireConnection;0;4;212;0
ASEEND*/
//CHKSM=CD8E7888670FF284856B19F2B94BEF3F951DC522