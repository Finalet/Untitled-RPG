Shader "InstancedIndirect/PolygonTrees"
{
    Properties
    {
        Vector1_CE098D07("Wind Density", Float) = 0.1
        Vector1_C1D16DBE("Wind Strength", Float) = 0.5
        Vector1_EBB5CF27("Wind Speed", Float) = 1
        [NoScaleOffset]Texture2D_CAD82441("Base Material", 2D) = "white" {}
        Vector1_AADD838F("Small Wind Density", Float) = 10
        Vector1_C39D93FF("Small Wind Speed", Float) = 0.2
        Vector1_7722A149("Small Wind Strength", Float) = 0.05
        [NoScaleOffset]Texture2D_38206155("Emissive Mask", 2D) = "white" {}
        Color_FA85148A("Emissive Color", Color) = (0.2660625, 1, 0, 0)
        Color_369F793F("LeafColourTint", Color) = (1, 1, 1, 0)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry+0"
        }
        
        Pass
        {
            Name "Universal Forward"
            Tags 
            { 
                "LightMode" = "UniversalForward"
            }
           
            // Render State
            Blend One Zero, One Zero
            Cull Off
            ZTest LEqual
            ZWrite On
            // ColorMask: <None>
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
        
            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            // GraphKeywords: <None>
            
            // Defines
            #define _AlphaClip 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS 
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS_FORWARD
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            #pragma instancing_options procedural:setup
            #pragma multi_compile GPU_FRUSTUM_ON __
            #include "VS_indirect.cginc"

            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_CE098D07;
            float Vector1_C1D16DBE;
            float Vector1_EBB5CF27;
            float Vector1_AADD838F;
            float Vector1_C39D93FF;
            float Vector1_7722A149;
            float4 Color_FA85148A;
            float4 Color_369F793F;
            CBUFFER_END
            TEXTURE2D(Texture2D_CAD82441); SAMPLER(samplerTexture2D_CAD82441); float4 Texture2D_CAD82441_TexelSize;
            TEXTURE2D(Texture2D_38206155); SAMPLER(samplerTexture2D_38206155); float4 Texture2D_38206155_TexelSize;
            SAMPLER(_SampleTexture2DLOD_A2D7B431_Sampler_3_Linear_Repeat);
            SAMPLER(_SampleTexture2DLOD_6BACBAD2_Sampler_3_Linear_Repeat);
        
            // Graph Functions
            
            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            
            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }
            
            
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            
            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }
            
            void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
            {
                Out = lerp(A, B, T);
            }
            
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            
            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
            
            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }
        
            // Graph Vertex
            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
                float4 VertexColor;
                float3 TimeParameters;
            };
            
            struct VertexDescription
            {
                float3 VertexPosition;
                float3 VertexNormal;
                float3 VertexTangent;
            };
            
            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                float _Split_A7CD9229_R_1 = IN.ObjectSpacePosition[0];
                float _Split_A7CD9229_G_2 = IN.ObjectSpacePosition[1];
                float _Split_A7CD9229_B_3 = IN.ObjectSpacePosition[2];
                float _Split_A7CD9229_A_4 = 0;
                float4 _Combine_FB6EA996_RGBA_4;
                float3 _Combine_FB6EA996_RGB_5;
                float2 _Combine_FB6EA996_RG_6;
                Unity_Combine_float(_Split_A7CD9229_R_1, _Split_A7CD9229_G_2, 0, 0, _Combine_FB6EA996_RGBA_4, _Combine_FB6EA996_RGB_5, _Combine_FB6EA996_RG_6);
                float _Property_1BCF6411_Out_0 = Vector1_C39D93FF;
                float _Multiply_6A9F5C97_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_1BCF6411_Out_0, _Multiply_6A9F5C97_Out_2);
                float2 _TilingAndOffset_E29D2380_Out_3;
                Unity_TilingAndOffset_float((_Combine_FB6EA996_RGBA_4.xy), float2 (1, 1), (_Multiply_6A9F5C97_Out_2.xx), _TilingAndOffset_E29D2380_Out_3);
                float _Property_8C8415C2_Out_0 = Vector1_AADD838F;
                float _GradientNoise_53510405_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_E29D2380_Out_3, _Property_8C8415C2_Out_0, _GradientNoise_53510405_Out_2);
                float _Add_32754B96_Out_2;
                Unity_Add_float(_GradientNoise_53510405_Out_2, -0.5, _Add_32754B96_Out_2);
                float _Property_41E8A8EC_Out_0 = Vector1_7722A149;
                float _Multiply_28F78D21_Out_2;
                Unity_Multiply_float(_Add_32754B96_Out_2, _Property_41E8A8EC_Out_0, _Multiply_28F78D21_Out_2);
                float _Split_902B55FA_R_1 = IN.ObjectSpacePosition[0];
                float _Split_902B55FA_G_2 = IN.ObjectSpacePosition[1];
                float _Split_902B55FA_B_3 = IN.ObjectSpacePosition[2];
                float _Split_902B55FA_A_4 = 0;
                float _Add_18487CC0_Out_2;
                Unity_Add_float(_Multiply_28F78D21_Out_2, _Split_902B55FA_R_1, _Add_18487CC0_Out_2);
                float4 _Combine_7CDB250E_RGBA_4;
                float3 _Combine_7CDB250E_RGB_5;
                float2 _Combine_7CDB250E_RG_6;
                Unity_Combine_float(_Add_18487CC0_Out_2, _Split_902B55FA_G_2, _Split_902B55FA_B_3, 0, _Combine_7CDB250E_RGBA_4, _Combine_7CDB250E_RGB_5, _Combine_7CDB250E_RG_6);
                float _Split_F58FDF93_R_1 = IN.VertexColor[0];
                float _Split_F58FDF93_G_2 = IN.VertexColor[1];
                float _Split_F58FDF93_B_3 = IN.VertexColor[2];
                float _Split_F58FDF93_A_4 = IN.VertexColor[3];
                float3 _Lerp_F9CCC9A7_Out_3;
                Unity_Lerp_float3(_Combine_7CDB250E_RGB_5, IN.ObjectSpacePosition, (_Split_F58FDF93_R_1.xxx), _Lerp_F9CCC9A7_Out_3);
                float _Split_9732E1B8_R_1 = IN.ObjectSpacePosition[0];
                float _Split_9732E1B8_G_2 = IN.ObjectSpacePosition[1];
                float _Split_9732E1B8_B_3 = IN.ObjectSpacePosition[2];
                float _Split_9732E1B8_A_4 = 0;
                float4 _Combine_514EB521_RGBA_4;
                float3 _Combine_514EB521_RGB_5;
                float2 _Combine_514EB521_RG_6;
                Unity_Combine_float(_Split_9732E1B8_R_1, _Split_9732E1B8_G_2, 0, 0, _Combine_514EB521_RGBA_4, _Combine_514EB521_RGB_5, _Combine_514EB521_RG_6);
                float _Property_527E2E7E_Out_0 = Vector1_EBB5CF27;
                float _Multiply_2BA2B898_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_527E2E7E_Out_0, _Multiply_2BA2B898_Out_2);
                float2 _TilingAndOffset_A6AF2C3F_Out_3;
                Unity_TilingAndOffset_float((_Combine_514EB521_RGBA_4.xy), float2 (1, 1), (_Multiply_2BA2B898_Out_2.xx), _TilingAndOffset_A6AF2C3F_Out_3);
                float _Property_A5E8FA2C_Out_0 = Vector1_CE098D07;
                float _GradientNoise_D251AB9A_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_A6AF2C3F_Out_3, _Property_A5E8FA2C_Out_0, _GradientNoise_D251AB9A_Out_2);
                float _Add_21D8E348_Out_2;
                Unity_Add_float(_GradientNoise_D251AB9A_Out_2, -0.5, _Add_21D8E348_Out_2);
                float _Property_F8D03925_Out_0 = Vector1_C1D16DBE;
                float _Multiply_ADACD13C_Out_2;
                Unity_Multiply_float(_Add_21D8E348_Out_2, _Property_F8D03925_Out_0, _Multiply_ADACD13C_Out_2);
                float _Split_63C86632_R_1 = IN.ObjectSpacePosition[0];
                float _Split_63C86632_G_2 = IN.ObjectSpacePosition[1];
                float _Split_63C86632_B_3 = IN.ObjectSpacePosition[2];
                float _Split_63C86632_A_4 = 0;
                float _Add_E978C7AA_Out_2;
                Unity_Add_float(_Multiply_ADACD13C_Out_2, _Split_63C86632_R_1, _Add_E978C7AA_Out_2);
                float4 _Combine_67FC00EE_RGBA_4;
                float3 _Combine_67FC00EE_RGB_5;
                float2 _Combine_67FC00EE_RG_6;
                Unity_Combine_float(_Add_E978C7AA_Out_2, _Split_63C86632_G_2, _Split_63C86632_B_3, 0, _Combine_67FC00EE_RGBA_4, _Combine_67FC00EE_RGB_5, _Combine_67FC00EE_RG_6);
                float _Split_2D0389_R_1 = IN.VertexColor[0];
                float _Split_2D0389_G_2 = IN.VertexColor[1];
                float _Split_2D0389_B_3 = IN.VertexColor[2];
                float _Split_2D0389_A_4 = IN.VertexColor[3];
                float3 _Lerp_991C85F5_Out_3;
                Unity_Lerp_float3(_Combine_67FC00EE_RGB_5, IN.ObjectSpacePosition, (_Split_2D0389_B_3.xxx), _Lerp_991C85F5_Out_3);
                float3 _Add_BC7A57EF_Out_2;
                Unity_Add_float3(_Lerp_F9CCC9A7_Out_3, _Lerp_991C85F5_Out_3, _Add_BC7A57EF_Out_2);
                float _Vector1_BDF0ABE6_Out_0 = 0.5;
                float3 _Multiply_16CD689_Out_2;
                Unity_Multiply_float(_Add_BC7A57EF_Out_2, (_Vector1_BDF0ABE6_Out_0.xxx), _Multiply_16CD689_Out_2);
                description.VertexPosition = _Multiply_16CD689_Out_2;
                description.VertexNormal = IN.ObjectSpaceNormal;
                description.VertexTangent = IN.ObjectSpaceTangent;
                return description;
            }
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 TangentSpaceNormal;
                float4 uv0;
            };
            
            struct SurfaceDescription
            {
                float3 Albedo;
                float3 Normal;
                float3 Emission;
                float Metallic;
                float Smoothness;
                float Occlusion;
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _SampleTexture2DLOD_A2D7B431_RGBA_0 = SAMPLE_TEXTURE2D_LOD(Texture2D_CAD82441, samplerTexture2D_CAD82441, IN.uv0.xy, 0);
                float _SampleTexture2DLOD_A2D7B431_R_5 = _SampleTexture2DLOD_A2D7B431_RGBA_0.r;
                float _SampleTexture2DLOD_A2D7B431_G_6 = _SampleTexture2DLOD_A2D7B431_RGBA_0.g;
                float _SampleTexture2DLOD_A2D7B431_B_7 = _SampleTexture2DLOD_A2D7B431_RGBA_0.b;
                float _SampleTexture2DLOD_A2D7B431_A_8 = _SampleTexture2DLOD_A2D7B431_RGBA_0.a;
                float4 _Property_2E0DC7FA_Out_0 = Color_369F793F;
                float4 _Multiply_C62D5025_Out_2;
                Unity_Multiply_float(_SampleTexture2DLOD_A2D7B431_RGBA_0, _Property_2E0DC7FA_Out_0, _Multiply_C62D5025_Out_2);
                float4 _SampleTexture2DLOD_6BACBAD2_RGBA_0 = SAMPLE_TEXTURE2D_LOD(Texture2D_38206155, samplerTexture2D_38206155, IN.uv0.xy, 0);
                float _SampleTexture2DLOD_6BACBAD2_R_5 = _SampleTexture2DLOD_6BACBAD2_RGBA_0.r;
                float _SampleTexture2DLOD_6BACBAD2_G_6 = _SampleTexture2DLOD_6BACBAD2_RGBA_0.g;
                float _SampleTexture2DLOD_6BACBAD2_B_7 = _SampleTexture2DLOD_6BACBAD2_RGBA_0.b;
                float _SampleTexture2DLOD_6BACBAD2_A_8 = _SampleTexture2DLOD_6BACBAD2_RGBA_0.a;
                float4 _Property_A6E091BF_Out_0 = Color_FA85148A;
                float4 _Multiply_668223DC_Out_2;
                Unity_Multiply_float(_SampleTexture2DLOD_6BACBAD2_RGBA_0, _Property_A6E091BF_Out_0, _Multiply_668223DC_Out_2);
                surface.Albedo = (_Multiply_C62D5025_Out_2.xyz);
                surface.Normal = IN.TangentSpaceNormal;
                surface.Emission = (_Multiply_668223DC_Out_2.xyz);
                surface.Metallic = 0.1;
                surface.Smoothness = 0;
                surface.Occlusion = 1;
                surface.Alpha = _SampleTexture2DLOD_A2D7B431_A_8;
                surface.AlphaClipThreshold = 0.5;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 color : COLOR;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS;
                float3 normalWS;
                float4 tangentWS;
                float4 texCoord0;
                float3 viewDirectionWS;
                #if defined(LIGHTMAP_ON)
                float2 lightmapUV;
                #endif
                #if !defined(LIGHTMAP_ON)
                float3 sh;
                #endif
                float4 fogFactorAndVertexLight;
                float4 shadowCoord;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if defined(LIGHTMAP_ON)
                #endif
                #if !defined(LIGHTMAP_ON)
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float3 interp00 : TEXCOORD0;
                float3 interp01 : TEXCOORD1;
                float4 interp02 : TEXCOORD2;
                float4 interp03 : TEXCOORD3;
                float3 interp04 : TEXCOORD4;
                float2 interp05 : TEXCOORD5;
                float3 interp06 : TEXCOORD6;
                float4 interp07 : TEXCOORD7;
                float4 interp08 : TEXCOORD8;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyz = input.positionWS;
                output.interp01.xyz = input.normalWS;
                output.interp02.xyzw = input.tangentWS;
                output.interp03.xyzw = input.texCoord0;
                output.interp04.xyz = input.viewDirectionWS;
                #if defined(LIGHTMAP_ON)
                output.interp05.xy = input.lightmapUV;
                #endif
                #if !defined(LIGHTMAP_ON)
                output.interp06.xyz = input.sh;
                #endif
                output.interp07.xyzw = input.fogFactorAndVertexLight;
                output.interp08.xyzw = input.shadowCoord;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.positionWS = input.interp00.xyz;
                output.normalWS = input.interp01.xyz;
                output.tangentWS = input.interp02.xyzw;
                output.texCoord0 = input.interp03.xyzw;
                output.viewDirectionWS = input.interp04.xyz;
                #if defined(LIGHTMAP_ON)
                output.lightmapUV = input.interp05.xy;
                #endif
                #if !defined(LIGHTMAP_ON)
                output.sh = input.interp06.xyz;
                #endif
                output.fogFactorAndVertexLight = input.interp07.xyzw;
                output.shadowCoord = input.interp08.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);
            
                output.ObjectSpaceNormal =           input.normalOS;
                output.ObjectSpaceTangent =          input.tangentOS;
                output.ObjectSpacePosition =         input.positionOS;
                output.VertexColor =                 input.color;
                output.TimeParameters =              _TimeParameters.xyz;
            
                return output;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
                output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
            
            
                output.uv0 =                         input.texCoord0;
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags 
            { 
                "LightMode" = "ShadowCaster"
            }
           
            // Render State
            Blend One Zero, One Zero
            Cull Off
            ZTest LEqual
            ZWrite On
            // ColorMask: <None>
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
            
            // Defines
            #define _AlphaClip 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS_SHADOWCASTER
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_CE098D07;
            float Vector1_C1D16DBE;
            float Vector1_EBB5CF27;
            float Vector1_AADD838F;
            float Vector1_C39D93FF;
            float Vector1_7722A149;
            float4 Color_FA85148A;
            float4 Color_369F793F;
            CBUFFER_END
            TEXTURE2D(Texture2D_CAD82441); SAMPLER(samplerTexture2D_CAD82441); float4 Texture2D_CAD82441_TexelSize;
            TEXTURE2D(Texture2D_38206155); SAMPLER(samplerTexture2D_38206155); float4 Texture2D_38206155_TexelSize;
            SAMPLER(_SampleTexture2DLOD_A2D7B431_Sampler_3_Linear_Repeat);
        
            // Graph Functions
            
            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            
            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }
            
            
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            
            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }
            
            void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
            {
                Out = lerp(A, B, T);
            }
            
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            
            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
        
            // Graph Vertex
            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
                float4 VertexColor;
                float3 TimeParameters;
            };
            
            struct VertexDescription
            {
                float3 VertexPosition;
                float3 VertexNormal;
                float3 VertexTangent;
            };
            
            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                float _Split_A7CD9229_R_1 = IN.ObjectSpacePosition[0];
                float _Split_A7CD9229_G_2 = IN.ObjectSpacePosition[1];
                float _Split_A7CD9229_B_3 = IN.ObjectSpacePosition[2];
                float _Split_A7CD9229_A_4 = 0;
                float4 _Combine_FB6EA996_RGBA_4;
                float3 _Combine_FB6EA996_RGB_5;
                float2 _Combine_FB6EA996_RG_6;
                Unity_Combine_float(_Split_A7CD9229_R_1, _Split_A7CD9229_G_2, 0, 0, _Combine_FB6EA996_RGBA_4, _Combine_FB6EA996_RGB_5, _Combine_FB6EA996_RG_6);
                float _Property_1BCF6411_Out_0 = Vector1_C39D93FF;
                float _Multiply_6A9F5C97_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_1BCF6411_Out_0, _Multiply_6A9F5C97_Out_2);
                float2 _TilingAndOffset_E29D2380_Out_3;
                Unity_TilingAndOffset_float((_Combine_FB6EA996_RGBA_4.xy), float2 (1, 1), (_Multiply_6A9F5C97_Out_2.xx), _TilingAndOffset_E29D2380_Out_3);
                float _Property_8C8415C2_Out_0 = Vector1_AADD838F;
                float _GradientNoise_53510405_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_E29D2380_Out_3, _Property_8C8415C2_Out_0, _GradientNoise_53510405_Out_2);
                float _Add_32754B96_Out_2;
                Unity_Add_float(_GradientNoise_53510405_Out_2, -0.5, _Add_32754B96_Out_2);
                float _Property_41E8A8EC_Out_0 = Vector1_7722A149;
                float _Multiply_28F78D21_Out_2;
                Unity_Multiply_float(_Add_32754B96_Out_2, _Property_41E8A8EC_Out_0, _Multiply_28F78D21_Out_2);
                float _Split_902B55FA_R_1 = IN.ObjectSpacePosition[0];
                float _Split_902B55FA_G_2 = IN.ObjectSpacePosition[1];
                float _Split_902B55FA_B_3 = IN.ObjectSpacePosition[2];
                float _Split_902B55FA_A_4 = 0;
                float _Add_18487CC0_Out_2;
                Unity_Add_float(_Multiply_28F78D21_Out_2, _Split_902B55FA_R_1, _Add_18487CC0_Out_2);
                float4 _Combine_7CDB250E_RGBA_4;
                float3 _Combine_7CDB250E_RGB_5;
                float2 _Combine_7CDB250E_RG_6;
                Unity_Combine_float(_Add_18487CC0_Out_2, _Split_902B55FA_G_2, _Split_902B55FA_B_3, 0, _Combine_7CDB250E_RGBA_4, _Combine_7CDB250E_RGB_5, _Combine_7CDB250E_RG_6);
                float _Split_F58FDF93_R_1 = IN.VertexColor[0];
                float _Split_F58FDF93_G_2 = IN.VertexColor[1];
                float _Split_F58FDF93_B_3 = IN.VertexColor[2];
                float _Split_F58FDF93_A_4 = IN.VertexColor[3];
                float3 _Lerp_F9CCC9A7_Out_3;
                Unity_Lerp_float3(_Combine_7CDB250E_RGB_5, IN.ObjectSpacePosition, (_Split_F58FDF93_R_1.xxx), _Lerp_F9CCC9A7_Out_3);
                float _Split_9732E1B8_R_1 = IN.ObjectSpacePosition[0];
                float _Split_9732E1B8_G_2 = IN.ObjectSpacePosition[1];
                float _Split_9732E1B8_B_3 = IN.ObjectSpacePosition[2];
                float _Split_9732E1B8_A_4 = 0;
                float4 _Combine_514EB521_RGBA_4;
                float3 _Combine_514EB521_RGB_5;
                float2 _Combine_514EB521_RG_6;
                Unity_Combine_float(_Split_9732E1B8_R_1, _Split_9732E1B8_G_2, 0, 0, _Combine_514EB521_RGBA_4, _Combine_514EB521_RGB_5, _Combine_514EB521_RG_6);
                float _Property_527E2E7E_Out_0 = Vector1_EBB5CF27;
                float _Multiply_2BA2B898_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_527E2E7E_Out_0, _Multiply_2BA2B898_Out_2);
                float2 _TilingAndOffset_A6AF2C3F_Out_3;
                Unity_TilingAndOffset_float((_Combine_514EB521_RGBA_4.xy), float2 (1, 1), (_Multiply_2BA2B898_Out_2.xx), _TilingAndOffset_A6AF2C3F_Out_3);
                float _Property_A5E8FA2C_Out_0 = Vector1_CE098D07;
                float _GradientNoise_D251AB9A_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_A6AF2C3F_Out_3, _Property_A5E8FA2C_Out_0, _GradientNoise_D251AB9A_Out_2);
                float _Add_21D8E348_Out_2;
                Unity_Add_float(_GradientNoise_D251AB9A_Out_2, -0.5, _Add_21D8E348_Out_2);
                float _Property_F8D03925_Out_0 = Vector1_C1D16DBE;
                float _Multiply_ADACD13C_Out_2;
                Unity_Multiply_float(_Add_21D8E348_Out_2, _Property_F8D03925_Out_0, _Multiply_ADACD13C_Out_2);
                float _Split_63C86632_R_1 = IN.ObjectSpacePosition[0];
                float _Split_63C86632_G_2 = IN.ObjectSpacePosition[1];
                float _Split_63C86632_B_3 = IN.ObjectSpacePosition[2];
                float _Split_63C86632_A_4 = 0;
                float _Add_E978C7AA_Out_2;
                Unity_Add_float(_Multiply_ADACD13C_Out_2, _Split_63C86632_R_1, _Add_E978C7AA_Out_2);
                float4 _Combine_67FC00EE_RGBA_4;
                float3 _Combine_67FC00EE_RGB_5;
                float2 _Combine_67FC00EE_RG_6;
                Unity_Combine_float(_Add_E978C7AA_Out_2, _Split_63C86632_G_2, _Split_63C86632_B_3, 0, _Combine_67FC00EE_RGBA_4, _Combine_67FC00EE_RGB_5, _Combine_67FC00EE_RG_6);
                float _Split_2D0389_R_1 = IN.VertexColor[0];
                float _Split_2D0389_G_2 = IN.VertexColor[1];
                float _Split_2D0389_B_3 = IN.VertexColor[2];
                float _Split_2D0389_A_4 = IN.VertexColor[3];
                float3 _Lerp_991C85F5_Out_3;
                Unity_Lerp_float3(_Combine_67FC00EE_RGB_5, IN.ObjectSpacePosition, (_Split_2D0389_B_3.xxx), _Lerp_991C85F5_Out_3);
                float3 _Add_BC7A57EF_Out_2;
                Unity_Add_float3(_Lerp_F9CCC9A7_Out_3, _Lerp_991C85F5_Out_3, _Add_BC7A57EF_Out_2);
                float _Vector1_BDF0ABE6_Out_0 = 0.5;
                float3 _Multiply_16CD689_Out_2;
                Unity_Multiply_float(_Add_BC7A57EF_Out_2, (_Vector1_BDF0ABE6_Out_0.xxx), _Multiply_16CD689_Out_2);
                description.VertexPosition = _Multiply_16CD689_Out_2;
                description.VertexNormal = IN.ObjectSpaceNormal;
                description.VertexTangent = IN.ObjectSpaceTangent;
                return description;
            }
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 TangentSpaceNormal;
                float4 uv0;
            };
            
            struct SurfaceDescription
            {
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _SampleTexture2DLOD_A2D7B431_RGBA_0 = SAMPLE_TEXTURE2D_LOD(Texture2D_CAD82441, samplerTexture2D_CAD82441, IN.uv0.xy, 0);
                float _SampleTexture2DLOD_A2D7B431_R_5 = _SampleTexture2DLOD_A2D7B431_RGBA_0.r;
                float _SampleTexture2DLOD_A2D7B431_G_6 = _SampleTexture2DLOD_A2D7B431_RGBA_0.g;
                float _SampleTexture2DLOD_A2D7B431_B_7 = _SampleTexture2DLOD_A2D7B431_RGBA_0.b;
                float _SampleTexture2DLOD_A2D7B431_A_8 = _SampleTexture2DLOD_A2D7B431_RGBA_0.a;
                surface.Alpha = _SampleTexture2DLOD_A2D7B431_A_8;
                surface.AlphaClipThreshold = 0.5;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 color : COLOR;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float4 interp00 : TEXCOORD0;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp00.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);
            
                output.ObjectSpaceNormal =           input.normalOS;
                output.ObjectSpaceTangent =          input.tangentOS;
                output.ObjectSpacePosition =         input.positionOS;
                output.VertexColor =                 input.color;
                output.TimeParameters =              _TimeParameters.xyz;
            
                return output;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
                output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
            
            
                output.uv0 =                         input.texCoord0;
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags 
            { 
                "LightMode" = "DepthOnly"
            }
           
            // Render State
            Blend One Zero, One Zero
            Cull Off
            ZTest LEqual
            ZWrite On
            ColorMask 0
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
            
            // Defines
            #define _AlphaClip 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS_DEPTHONLY
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_CE098D07;
            float Vector1_C1D16DBE;
            float Vector1_EBB5CF27;
            float Vector1_AADD838F;
            float Vector1_C39D93FF;
            float Vector1_7722A149;
            float4 Color_FA85148A;
            float4 Color_369F793F;
            CBUFFER_END
            TEXTURE2D(Texture2D_CAD82441); SAMPLER(samplerTexture2D_CAD82441); float4 Texture2D_CAD82441_TexelSize;
            TEXTURE2D(Texture2D_38206155); SAMPLER(samplerTexture2D_38206155); float4 Texture2D_38206155_TexelSize;
            SAMPLER(_SampleTexture2DLOD_A2D7B431_Sampler_3_Linear_Repeat);
        
            // Graph Functions
            
            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            
            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }
            
            
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            
            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }
            
            void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
            {
                Out = lerp(A, B, T);
            }
            
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            
            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
        
            // Graph Vertex
            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
                float4 VertexColor;
                float3 TimeParameters;
            };
            
            struct VertexDescription
            {
                float3 VertexPosition;
                float3 VertexNormal;
                float3 VertexTangent;
            };
            
            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                float _Split_A7CD9229_R_1 = IN.ObjectSpacePosition[0];
                float _Split_A7CD9229_G_2 = IN.ObjectSpacePosition[1];
                float _Split_A7CD9229_B_3 = IN.ObjectSpacePosition[2];
                float _Split_A7CD9229_A_4 = 0;
                float4 _Combine_FB6EA996_RGBA_4;
                float3 _Combine_FB6EA996_RGB_5;
                float2 _Combine_FB6EA996_RG_6;
                Unity_Combine_float(_Split_A7CD9229_R_1, _Split_A7CD9229_G_2, 0, 0, _Combine_FB6EA996_RGBA_4, _Combine_FB6EA996_RGB_5, _Combine_FB6EA996_RG_6);
                float _Property_1BCF6411_Out_0 = Vector1_C39D93FF;
                float _Multiply_6A9F5C97_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_1BCF6411_Out_0, _Multiply_6A9F5C97_Out_2);
                float2 _TilingAndOffset_E29D2380_Out_3;
                Unity_TilingAndOffset_float((_Combine_FB6EA996_RGBA_4.xy), float2 (1, 1), (_Multiply_6A9F5C97_Out_2.xx), _TilingAndOffset_E29D2380_Out_3);
                float _Property_8C8415C2_Out_0 = Vector1_AADD838F;
                float _GradientNoise_53510405_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_E29D2380_Out_3, _Property_8C8415C2_Out_0, _GradientNoise_53510405_Out_2);
                float _Add_32754B96_Out_2;
                Unity_Add_float(_GradientNoise_53510405_Out_2, -0.5, _Add_32754B96_Out_2);
                float _Property_41E8A8EC_Out_0 = Vector1_7722A149;
                float _Multiply_28F78D21_Out_2;
                Unity_Multiply_float(_Add_32754B96_Out_2, _Property_41E8A8EC_Out_0, _Multiply_28F78D21_Out_2);
                float _Split_902B55FA_R_1 = IN.ObjectSpacePosition[0];
                float _Split_902B55FA_G_2 = IN.ObjectSpacePosition[1];
                float _Split_902B55FA_B_3 = IN.ObjectSpacePosition[2];
                float _Split_902B55FA_A_4 = 0;
                float _Add_18487CC0_Out_2;
                Unity_Add_float(_Multiply_28F78D21_Out_2, _Split_902B55FA_R_1, _Add_18487CC0_Out_2);
                float4 _Combine_7CDB250E_RGBA_4;
                float3 _Combine_7CDB250E_RGB_5;
                float2 _Combine_7CDB250E_RG_6;
                Unity_Combine_float(_Add_18487CC0_Out_2, _Split_902B55FA_G_2, _Split_902B55FA_B_3, 0, _Combine_7CDB250E_RGBA_4, _Combine_7CDB250E_RGB_5, _Combine_7CDB250E_RG_6);
                float _Split_F58FDF93_R_1 = IN.VertexColor[0];
                float _Split_F58FDF93_G_2 = IN.VertexColor[1];
                float _Split_F58FDF93_B_3 = IN.VertexColor[2];
                float _Split_F58FDF93_A_4 = IN.VertexColor[3];
                float3 _Lerp_F9CCC9A7_Out_3;
                Unity_Lerp_float3(_Combine_7CDB250E_RGB_5, IN.ObjectSpacePosition, (_Split_F58FDF93_R_1.xxx), _Lerp_F9CCC9A7_Out_3);
                float _Split_9732E1B8_R_1 = IN.ObjectSpacePosition[0];
                float _Split_9732E1B8_G_2 = IN.ObjectSpacePosition[1];
                float _Split_9732E1B8_B_3 = IN.ObjectSpacePosition[2];
                float _Split_9732E1B8_A_4 = 0;
                float4 _Combine_514EB521_RGBA_4;
                float3 _Combine_514EB521_RGB_5;
                float2 _Combine_514EB521_RG_6;
                Unity_Combine_float(_Split_9732E1B8_R_1, _Split_9732E1B8_G_2, 0, 0, _Combine_514EB521_RGBA_4, _Combine_514EB521_RGB_5, _Combine_514EB521_RG_6);
                float _Property_527E2E7E_Out_0 = Vector1_EBB5CF27;
                float _Multiply_2BA2B898_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_527E2E7E_Out_0, _Multiply_2BA2B898_Out_2);
                float2 _TilingAndOffset_A6AF2C3F_Out_3;
                Unity_TilingAndOffset_float((_Combine_514EB521_RGBA_4.xy), float2 (1, 1), (_Multiply_2BA2B898_Out_2.xx), _TilingAndOffset_A6AF2C3F_Out_3);
                float _Property_A5E8FA2C_Out_0 = Vector1_CE098D07;
                float _GradientNoise_D251AB9A_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_A6AF2C3F_Out_3, _Property_A5E8FA2C_Out_0, _GradientNoise_D251AB9A_Out_2);
                float _Add_21D8E348_Out_2;
                Unity_Add_float(_GradientNoise_D251AB9A_Out_2, -0.5, _Add_21D8E348_Out_2);
                float _Property_F8D03925_Out_0 = Vector1_C1D16DBE;
                float _Multiply_ADACD13C_Out_2;
                Unity_Multiply_float(_Add_21D8E348_Out_2, _Property_F8D03925_Out_0, _Multiply_ADACD13C_Out_2);
                float _Split_63C86632_R_1 = IN.ObjectSpacePosition[0];
                float _Split_63C86632_G_2 = IN.ObjectSpacePosition[1];
                float _Split_63C86632_B_3 = IN.ObjectSpacePosition[2];
                float _Split_63C86632_A_4 = 0;
                float _Add_E978C7AA_Out_2;
                Unity_Add_float(_Multiply_ADACD13C_Out_2, _Split_63C86632_R_1, _Add_E978C7AA_Out_2);
                float4 _Combine_67FC00EE_RGBA_4;
                float3 _Combine_67FC00EE_RGB_5;
                float2 _Combine_67FC00EE_RG_6;
                Unity_Combine_float(_Add_E978C7AA_Out_2, _Split_63C86632_G_2, _Split_63C86632_B_3, 0, _Combine_67FC00EE_RGBA_4, _Combine_67FC00EE_RGB_5, _Combine_67FC00EE_RG_6);
                float _Split_2D0389_R_1 = IN.VertexColor[0];
                float _Split_2D0389_G_2 = IN.VertexColor[1];
                float _Split_2D0389_B_3 = IN.VertexColor[2];
                float _Split_2D0389_A_4 = IN.VertexColor[3];
                float3 _Lerp_991C85F5_Out_3;
                Unity_Lerp_float3(_Combine_67FC00EE_RGB_5, IN.ObjectSpacePosition, (_Split_2D0389_B_3.xxx), _Lerp_991C85F5_Out_3);
                float3 _Add_BC7A57EF_Out_2;
                Unity_Add_float3(_Lerp_F9CCC9A7_Out_3, _Lerp_991C85F5_Out_3, _Add_BC7A57EF_Out_2);
                float _Vector1_BDF0ABE6_Out_0 = 0.5;
                float3 _Multiply_16CD689_Out_2;
                Unity_Multiply_float(_Add_BC7A57EF_Out_2, (_Vector1_BDF0ABE6_Out_0.xxx), _Multiply_16CD689_Out_2);
                description.VertexPosition = _Multiply_16CD689_Out_2;
                description.VertexNormal = IN.ObjectSpaceNormal;
                description.VertexTangent = IN.ObjectSpaceTangent;
                return description;
            }
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 TangentSpaceNormal;
                float4 uv0;
            };
            
            struct SurfaceDescription
            {
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _SampleTexture2DLOD_A2D7B431_RGBA_0 = SAMPLE_TEXTURE2D_LOD(Texture2D_CAD82441, samplerTexture2D_CAD82441, IN.uv0.xy, 0);
                float _SampleTexture2DLOD_A2D7B431_R_5 = _SampleTexture2DLOD_A2D7B431_RGBA_0.r;
                float _SampleTexture2DLOD_A2D7B431_G_6 = _SampleTexture2DLOD_A2D7B431_RGBA_0.g;
                float _SampleTexture2DLOD_A2D7B431_B_7 = _SampleTexture2DLOD_A2D7B431_RGBA_0.b;
                float _SampleTexture2DLOD_A2D7B431_A_8 = _SampleTexture2DLOD_A2D7B431_RGBA_0.a;
                surface.Alpha = _SampleTexture2DLOD_A2D7B431_A_8;
                surface.AlphaClipThreshold = 0.5;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 color : COLOR;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float4 interp00 : TEXCOORD0;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp00.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);
            
                output.ObjectSpaceNormal =           input.normalOS;
                output.ObjectSpaceTangent =          input.tangentOS;
                output.ObjectSpacePosition =         input.positionOS;
                output.VertexColor =                 input.color;
                output.TimeParameters =              _TimeParameters.xyz;
            
                return output;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
                output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
            
            
                output.uv0 =                         input.texCoord0;
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
            ENDHLSL
        }
        
        Pass
        {
            Name "Meta"
            Tags 
            { 
                "LightMode" = "Meta"
            }
           
            // Render State
            Blend One Zero, One Zero
            Cull Off
            ZTest LEqual
            ZWrite On
            // ColorMask: <None>
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
        
            // Keywords
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // GraphKeywords: <None>
            
            // Defines
            #define _AlphaClip 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS_META
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_CE098D07;
            float Vector1_C1D16DBE;
            float Vector1_EBB5CF27;
            float Vector1_AADD838F;
            float Vector1_C39D93FF;
            float Vector1_7722A149;
            float4 Color_FA85148A;
            float4 Color_369F793F;
            CBUFFER_END
            TEXTURE2D(Texture2D_CAD82441); SAMPLER(samplerTexture2D_CAD82441); float4 Texture2D_CAD82441_TexelSize;
            TEXTURE2D(Texture2D_38206155); SAMPLER(samplerTexture2D_38206155); float4 Texture2D_38206155_TexelSize;
            SAMPLER(_SampleTexture2DLOD_A2D7B431_Sampler_3_Linear_Repeat);
            SAMPLER(_SampleTexture2DLOD_6BACBAD2_Sampler_3_Linear_Repeat);
        
            // Graph Functions
            
            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            
            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }
            
            
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            
            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }
            
            void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
            {
                Out = lerp(A, B, T);
            }
            
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            
            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
            
            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }
        
            // Graph Vertex
            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
                float4 VertexColor;
                float3 TimeParameters;
            };
            
            struct VertexDescription
            {
                float3 VertexPosition;
                float3 VertexNormal;
                float3 VertexTangent;
            };
            
            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                float _Split_A7CD9229_R_1 = IN.ObjectSpacePosition[0];
                float _Split_A7CD9229_G_2 = IN.ObjectSpacePosition[1];
                float _Split_A7CD9229_B_3 = IN.ObjectSpacePosition[2];
                float _Split_A7CD9229_A_4 = 0;
                float4 _Combine_FB6EA996_RGBA_4;
                float3 _Combine_FB6EA996_RGB_5;
                float2 _Combine_FB6EA996_RG_6;
                Unity_Combine_float(_Split_A7CD9229_R_1, _Split_A7CD9229_G_2, 0, 0, _Combine_FB6EA996_RGBA_4, _Combine_FB6EA996_RGB_5, _Combine_FB6EA996_RG_6);
                float _Property_1BCF6411_Out_0 = Vector1_C39D93FF;
                float _Multiply_6A9F5C97_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_1BCF6411_Out_0, _Multiply_6A9F5C97_Out_2);
                float2 _TilingAndOffset_E29D2380_Out_3;
                Unity_TilingAndOffset_float((_Combine_FB6EA996_RGBA_4.xy), float2 (1, 1), (_Multiply_6A9F5C97_Out_2.xx), _TilingAndOffset_E29D2380_Out_3);
                float _Property_8C8415C2_Out_0 = Vector1_AADD838F;
                float _GradientNoise_53510405_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_E29D2380_Out_3, _Property_8C8415C2_Out_0, _GradientNoise_53510405_Out_2);
                float _Add_32754B96_Out_2;
                Unity_Add_float(_GradientNoise_53510405_Out_2, -0.5, _Add_32754B96_Out_2);
                float _Property_41E8A8EC_Out_0 = Vector1_7722A149;
                float _Multiply_28F78D21_Out_2;
                Unity_Multiply_float(_Add_32754B96_Out_2, _Property_41E8A8EC_Out_0, _Multiply_28F78D21_Out_2);
                float _Split_902B55FA_R_1 = IN.ObjectSpacePosition[0];
                float _Split_902B55FA_G_2 = IN.ObjectSpacePosition[1];
                float _Split_902B55FA_B_3 = IN.ObjectSpacePosition[2];
                float _Split_902B55FA_A_4 = 0;
                float _Add_18487CC0_Out_2;
                Unity_Add_float(_Multiply_28F78D21_Out_2, _Split_902B55FA_R_1, _Add_18487CC0_Out_2);
                float4 _Combine_7CDB250E_RGBA_4;
                float3 _Combine_7CDB250E_RGB_5;
                float2 _Combine_7CDB250E_RG_6;
                Unity_Combine_float(_Add_18487CC0_Out_2, _Split_902B55FA_G_2, _Split_902B55FA_B_3, 0, _Combine_7CDB250E_RGBA_4, _Combine_7CDB250E_RGB_5, _Combine_7CDB250E_RG_6);
                float _Split_F58FDF93_R_1 = IN.VertexColor[0];
                float _Split_F58FDF93_G_2 = IN.VertexColor[1];
                float _Split_F58FDF93_B_3 = IN.VertexColor[2];
                float _Split_F58FDF93_A_4 = IN.VertexColor[3];
                float3 _Lerp_F9CCC9A7_Out_3;
                Unity_Lerp_float3(_Combine_7CDB250E_RGB_5, IN.ObjectSpacePosition, (_Split_F58FDF93_R_1.xxx), _Lerp_F9CCC9A7_Out_3);
                float _Split_9732E1B8_R_1 = IN.ObjectSpacePosition[0];
                float _Split_9732E1B8_G_2 = IN.ObjectSpacePosition[1];
                float _Split_9732E1B8_B_3 = IN.ObjectSpacePosition[2];
                float _Split_9732E1B8_A_4 = 0;
                float4 _Combine_514EB521_RGBA_4;
                float3 _Combine_514EB521_RGB_5;
                float2 _Combine_514EB521_RG_6;
                Unity_Combine_float(_Split_9732E1B8_R_1, _Split_9732E1B8_G_2, 0, 0, _Combine_514EB521_RGBA_4, _Combine_514EB521_RGB_5, _Combine_514EB521_RG_6);
                float _Property_527E2E7E_Out_0 = Vector1_EBB5CF27;
                float _Multiply_2BA2B898_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_527E2E7E_Out_0, _Multiply_2BA2B898_Out_2);
                float2 _TilingAndOffset_A6AF2C3F_Out_3;
                Unity_TilingAndOffset_float((_Combine_514EB521_RGBA_4.xy), float2 (1, 1), (_Multiply_2BA2B898_Out_2.xx), _TilingAndOffset_A6AF2C3F_Out_3);
                float _Property_A5E8FA2C_Out_0 = Vector1_CE098D07;
                float _GradientNoise_D251AB9A_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_A6AF2C3F_Out_3, _Property_A5E8FA2C_Out_0, _GradientNoise_D251AB9A_Out_2);
                float _Add_21D8E348_Out_2;
                Unity_Add_float(_GradientNoise_D251AB9A_Out_2, -0.5, _Add_21D8E348_Out_2);
                float _Property_F8D03925_Out_0 = Vector1_C1D16DBE;
                float _Multiply_ADACD13C_Out_2;
                Unity_Multiply_float(_Add_21D8E348_Out_2, _Property_F8D03925_Out_0, _Multiply_ADACD13C_Out_2);
                float _Split_63C86632_R_1 = IN.ObjectSpacePosition[0];
                float _Split_63C86632_G_2 = IN.ObjectSpacePosition[1];
                float _Split_63C86632_B_3 = IN.ObjectSpacePosition[2];
                float _Split_63C86632_A_4 = 0;
                float _Add_E978C7AA_Out_2;
                Unity_Add_float(_Multiply_ADACD13C_Out_2, _Split_63C86632_R_1, _Add_E978C7AA_Out_2);
                float4 _Combine_67FC00EE_RGBA_4;
                float3 _Combine_67FC00EE_RGB_5;
                float2 _Combine_67FC00EE_RG_6;
                Unity_Combine_float(_Add_E978C7AA_Out_2, _Split_63C86632_G_2, _Split_63C86632_B_3, 0, _Combine_67FC00EE_RGBA_4, _Combine_67FC00EE_RGB_5, _Combine_67FC00EE_RG_6);
                float _Split_2D0389_R_1 = IN.VertexColor[0];
                float _Split_2D0389_G_2 = IN.VertexColor[1];
                float _Split_2D0389_B_3 = IN.VertexColor[2];
                float _Split_2D0389_A_4 = IN.VertexColor[3];
                float3 _Lerp_991C85F5_Out_3;
                Unity_Lerp_float3(_Combine_67FC00EE_RGB_5, IN.ObjectSpacePosition, (_Split_2D0389_B_3.xxx), _Lerp_991C85F5_Out_3);
                float3 _Add_BC7A57EF_Out_2;
                Unity_Add_float3(_Lerp_F9CCC9A7_Out_3, _Lerp_991C85F5_Out_3, _Add_BC7A57EF_Out_2);
                float _Vector1_BDF0ABE6_Out_0 = 0.5;
                float3 _Multiply_16CD689_Out_2;
                Unity_Multiply_float(_Add_BC7A57EF_Out_2, (_Vector1_BDF0ABE6_Out_0.xxx), _Multiply_16CD689_Out_2);
                description.VertexPosition = _Multiply_16CD689_Out_2;
                description.VertexNormal = IN.ObjectSpaceNormal;
                description.VertexTangent = IN.ObjectSpaceTangent;
                return description;
            }
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 TangentSpaceNormal;
                float4 uv0;
            };
            
            struct SurfaceDescription
            {
                float3 Albedo;
                float3 Emission;
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _SampleTexture2DLOD_A2D7B431_RGBA_0 = SAMPLE_TEXTURE2D_LOD(Texture2D_CAD82441, samplerTexture2D_CAD82441, IN.uv0.xy, 0);
                float _SampleTexture2DLOD_A2D7B431_R_5 = _SampleTexture2DLOD_A2D7B431_RGBA_0.r;
                float _SampleTexture2DLOD_A2D7B431_G_6 = _SampleTexture2DLOD_A2D7B431_RGBA_0.g;
                float _SampleTexture2DLOD_A2D7B431_B_7 = _SampleTexture2DLOD_A2D7B431_RGBA_0.b;
                float _SampleTexture2DLOD_A2D7B431_A_8 = _SampleTexture2DLOD_A2D7B431_RGBA_0.a;
                float4 _Property_2E0DC7FA_Out_0 = Color_369F793F;
                float4 _Multiply_C62D5025_Out_2;
                Unity_Multiply_float(_SampleTexture2DLOD_A2D7B431_RGBA_0, _Property_2E0DC7FA_Out_0, _Multiply_C62D5025_Out_2);
                float4 _SampleTexture2DLOD_6BACBAD2_RGBA_0 = SAMPLE_TEXTURE2D_LOD(Texture2D_38206155, samplerTexture2D_38206155, IN.uv0.xy, 0);
                float _SampleTexture2DLOD_6BACBAD2_R_5 = _SampleTexture2DLOD_6BACBAD2_RGBA_0.r;
                float _SampleTexture2DLOD_6BACBAD2_G_6 = _SampleTexture2DLOD_6BACBAD2_RGBA_0.g;
                float _SampleTexture2DLOD_6BACBAD2_B_7 = _SampleTexture2DLOD_6BACBAD2_RGBA_0.b;
                float _SampleTexture2DLOD_6BACBAD2_A_8 = _SampleTexture2DLOD_6BACBAD2_RGBA_0.a;
                float4 _Property_A6E091BF_Out_0 = Color_FA85148A;
                float4 _Multiply_668223DC_Out_2;
                Unity_Multiply_float(_SampleTexture2DLOD_6BACBAD2_RGBA_0, _Property_A6E091BF_Out_0, _Multiply_668223DC_Out_2);
                surface.Albedo = (_Multiply_C62D5025_Out_2.xyz);
                surface.Emission = (_Multiply_668223DC_Out_2.xyz);
                surface.Alpha = _SampleTexture2DLOD_A2D7B431_A_8;
                surface.AlphaClipThreshold = 0.5;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 color : COLOR;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float4 interp00 : TEXCOORD0;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp00.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);
            
                output.ObjectSpaceNormal =           input.normalOS;
                output.ObjectSpaceTangent =          input.tangentOS;
                output.ObjectSpacePosition =         input.positionOS;
                output.VertexColor =                 input.color;
                output.TimeParameters =              _TimeParameters.xyz;
            
                return output;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
                output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
            
            
                output.uv0 =                         input.texCoord0;
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
            ENDHLSL
        }
        
        Pass
        {
            // Name: <None>
            Tags 
            { 
                "LightMode" = "Universal2D"
            }
           
            // Render State
            Blend One Zero, One Zero
            Cull Off
            ZTest LEqual
            ZWrite On
            // ColorMask: <None>
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
            
            // Defines
            #define _AlphaClip 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS_2D
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_CE098D07;
            float Vector1_C1D16DBE;
            float Vector1_EBB5CF27;
            float Vector1_AADD838F;
            float Vector1_C39D93FF;
            float Vector1_7722A149;
            float4 Color_FA85148A;
            float4 Color_369F793F;
            CBUFFER_END
            TEXTURE2D(Texture2D_CAD82441); SAMPLER(samplerTexture2D_CAD82441); float4 Texture2D_CAD82441_TexelSize;
            TEXTURE2D(Texture2D_38206155); SAMPLER(samplerTexture2D_38206155); float4 Texture2D_38206155_TexelSize;
            SAMPLER(_SampleTexture2DLOD_A2D7B431_Sampler_3_Linear_Repeat);
        
            // Graph Functions
            
            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            
            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }
            
            
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            
            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }
            
            void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
            {
                Out = lerp(A, B, T);
            }
            
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            
            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
            
            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }
        
            // Graph Vertex
            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
                float4 VertexColor;
                float3 TimeParameters;
            };
            
            struct VertexDescription
            {
                float3 VertexPosition;
                float3 VertexNormal;
                float3 VertexTangent;
            };
            
            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                float _Split_A7CD9229_R_1 = IN.ObjectSpacePosition[0];
                float _Split_A7CD9229_G_2 = IN.ObjectSpacePosition[1];
                float _Split_A7CD9229_B_3 = IN.ObjectSpacePosition[2];
                float _Split_A7CD9229_A_4 = 0;
                float4 _Combine_FB6EA996_RGBA_4;
                float3 _Combine_FB6EA996_RGB_5;
                float2 _Combine_FB6EA996_RG_6;
                Unity_Combine_float(_Split_A7CD9229_R_1, _Split_A7CD9229_G_2, 0, 0, _Combine_FB6EA996_RGBA_4, _Combine_FB6EA996_RGB_5, _Combine_FB6EA996_RG_6);
                float _Property_1BCF6411_Out_0 = Vector1_C39D93FF;
                float _Multiply_6A9F5C97_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_1BCF6411_Out_0, _Multiply_6A9F5C97_Out_2);
                float2 _TilingAndOffset_E29D2380_Out_3;
                Unity_TilingAndOffset_float((_Combine_FB6EA996_RGBA_4.xy), float2 (1, 1), (_Multiply_6A9F5C97_Out_2.xx), _TilingAndOffset_E29D2380_Out_3);
                float _Property_8C8415C2_Out_0 = Vector1_AADD838F;
                float _GradientNoise_53510405_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_E29D2380_Out_3, _Property_8C8415C2_Out_0, _GradientNoise_53510405_Out_2);
                float _Add_32754B96_Out_2;
                Unity_Add_float(_GradientNoise_53510405_Out_2, -0.5, _Add_32754B96_Out_2);
                float _Property_41E8A8EC_Out_0 = Vector1_7722A149;
                float _Multiply_28F78D21_Out_2;
                Unity_Multiply_float(_Add_32754B96_Out_2, _Property_41E8A8EC_Out_0, _Multiply_28F78D21_Out_2);
                float _Split_902B55FA_R_1 = IN.ObjectSpacePosition[0];
                float _Split_902B55FA_G_2 = IN.ObjectSpacePosition[1];
                float _Split_902B55FA_B_3 = IN.ObjectSpacePosition[2];
                float _Split_902B55FA_A_4 = 0;
                float _Add_18487CC0_Out_2;
                Unity_Add_float(_Multiply_28F78D21_Out_2, _Split_902B55FA_R_1, _Add_18487CC0_Out_2);
                float4 _Combine_7CDB250E_RGBA_4;
                float3 _Combine_7CDB250E_RGB_5;
                float2 _Combine_7CDB250E_RG_6;
                Unity_Combine_float(_Add_18487CC0_Out_2, _Split_902B55FA_G_2, _Split_902B55FA_B_3, 0, _Combine_7CDB250E_RGBA_4, _Combine_7CDB250E_RGB_5, _Combine_7CDB250E_RG_6);
                float _Split_F58FDF93_R_1 = IN.VertexColor[0];
                float _Split_F58FDF93_G_2 = IN.VertexColor[1];
                float _Split_F58FDF93_B_3 = IN.VertexColor[2];
                float _Split_F58FDF93_A_4 = IN.VertexColor[3];
                float3 _Lerp_F9CCC9A7_Out_3;
                Unity_Lerp_float3(_Combine_7CDB250E_RGB_5, IN.ObjectSpacePosition, (_Split_F58FDF93_R_1.xxx), _Lerp_F9CCC9A7_Out_3);
                float _Split_9732E1B8_R_1 = IN.ObjectSpacePosition[0];
                float _Split_9732E1B8_G_2 = IN.ObjectSpacePosition[1];
                float _Split_9732E1B8_B_3 = IN.ObjectSpacePosition[2];
                float _Split_9732E1B8_A_4 = 0;
                float4 _Combine_514EB521_RGBA_4;
                float3 _Combine_514EB521_RGB_5;
                float2 _Combine_514EB521_RG_6;
                Unity_Combine_float(_Split_9732E1B8_R_1, _Split_9732E1B8_G_2, 0, 0, _Combine_514EB521_RGBA_4, _Combine_514EB521_RGB_5, _Combine_514EB521_RG_6);
                float _Property_527E2E7E_Out_0 = Vector1_EBB5CF27;
                float _Multiply_2BA2B898_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_527E2E7E_Out_0, _Multiply_2BA2B898_Out_2);
                float2 _TilingAndOffset_A6AF2C3F_Out_3;
                Unity_TilingAndOffset_float((_Combine_514EB521_RGBA_4.xy), float2 (1, 1), (_Multiply_2BA2B898_Out_2.xx), _TilingAndOffset_A6AF2C3F_Out_3);
                float _Property_A5E8FA2C_Out_0 = Vector1_CE098D07;
                float _GradientNoise_D251AB9A_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_A6AF2C3F_Out_3, _Property_A5E8FA2C_Out_0, _GradientNoise_D251AB9A_Out_2);
                float _Add_21D8E348_Out_2;
                Unity_Add_float(_GradientNoise_D251AB9A_Out_2, -0.5, _Add_21D8E348_Out_2);
                float _Property_F8D03925_Out_0 = Vector1_C1D16DBE;
                float _Multiply_ADACD13C_Out_2;
                Unity_Multiply_float(_Add_21D8E348_Out_2, _Property_F8D03925_Out_0, _Multiply_ADACD13C_Out_2);
                float _Split_63C86632_R_1 = IN.ObjectSpacePosition[0];
                float _Split_63C86632_G_2 = IN.ObjectSpacePosition[1];
                float _Split_63C86632_B_3 = IN.ObjectSpacePosition[2];
                float _Split_63C86632_A_4 = 0;
                float _Add_E978C7AA_Out_2;
                Unity_Add_float(_Multiply_ADACD13C_Out_2, _Split_63C86632_R_1, _Add_E978C7AA_Out_2);
                float4 _Combine_67FC00EE_RGBA_4;
                float3 _Combine_67FC00EE_RGB_5;
                float2 _Combine_67FC00EE_RG_6;
                Unity_Combine_float(_Add_E978C7AA_Out_2, _Split_63C86632_G_2, _Split_63C86632_B_3, 0, _Combine_67FC00EE_RGBA_4, _Combine_67FC00EE_RGB_5, _Combine_67FC00EE_RG_6);
                float _Split_2D0389_R_1 = IN.VertexColor[0];
                float _Split_2D0389_G_2 = IN.VertexColor[1];
                float _Split_2D0389_B_3 = IN.VertexColor[2];
                float _Split_2D0389_A_4 = IN.VertexColor[3];
                float3 _Lerp_991C85F5_Out_3;
                Unity_Lerp_float3(_Combine_67FC00EE_RGB_5, IN.ObjectSpacePosition, (_Split_2D0389_B_3.xxx), _Lerp_991C85F5_Out_3);
                float3 _Add_BC7A57EF_Out_2;
                Unity_Add_float3(_Lerp_F9CCC9A7_Out_3, _Lerp_991C85F5_Out_3, _Add_BC7A57EF_Out_2);
                float _Vector1_BDF0ABE6_Out_0 = 0.5;
                float3 _Multiply_16CD689_Out_2;
                Unity_Multiply_float(_Add_BC7A57EF_Out_2, (_Vector1_BDF0ABE6_Out_0.xxx), _Multiply_16CD689_Out_2);
                description.VertexPosition = _Multiply_16CD689_Out_2;
                description.VertexNormal = IN.ObjectSpaceNormal;
                description.VertexTangent = IN.ObjectSpaceTangent;
                return description;
            }
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 TangentSpaceNormal;
                float4 uv0;
            };
            
            struct SurfaceDescription
            {
                float3 Albedo;
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _SampleTexture2DLOD_A2D7B431_RGBA_0 = SAMPLE_TEXTURE2D_LOD(Texture2D_CAD82441, samplerTexture2D_CAD82441, IN.uv0.xy, 0);
                float _SampleTexture2DLOD_A2D7B431_R_5 = _SampleTexture2DLOD_A2D7B431_RGBA_0.r;
                float _SampleTexture2DLOD_A2D7B431_G_6 = _SampleTexture2DLOD_A2D7B431_RGBA_0.g;
                float _SampleTexture2DLOD_A2D7B431_B_7 = _SampleTexture2DLOD_A2D7B431_RGBA_0.b;
                float _SampleTexture2DLOD_A2D7B431_A_8 = _SampleTexture2DLOD_A2D7B431_RGBA_0.a;
                float4 _Property_2E0DC7FA_Out_0 = Color_369F793F;
                float4 _Multiply_C62D5025_Out_2;
                Unity_Multiply_float(_SampleTexture2DLOD_A2D7B431_RGBA_0, _Property_2E0DC7FA_Out_0, _Multiply_C62D5025_Out_2);
                surface.Albedo = (_Multiply_C62D5025_Out_2.xyz);
                surface.Alpha = _SampleTexture2DLOD_A2D7B431_A_8;
                surface.AlphaClipThreshold = 0.5;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 color : COLOR;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float4 interp00 : TEXCOORD0;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp00.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);
            
                output.ObjectSpaceNormal =           input.normalOS;
                output.ObjectSpaceTangent =          input.tangentOS;
                output.ObjectSpacePosition =         input.positionOS;
                output.VertexColor =                 input.color;
                output.TimeParameters =              _TimeParameters.xyz;
            
                return output;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
                output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
            
            
                output.uv0 =                         input.texCoord0;
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
        
            ENDHLSL
        }
        
    }
    CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}
