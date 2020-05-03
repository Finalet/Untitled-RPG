Shader "Hidden/Universal Render Pipeline/CameraNormals"
{
    HLSLINCLUDE
        
    #pragma target 2.0
    #pragma prefer_hlslcc gles
    #pragma exclude_renderers d3d11_9x

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    struct Attributes
    {
        float4 positionOS   : POSITION;
        float3 normalOS     : NORMAL;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS   : SV_POSITION;
        float3 viewNormal   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        output.viewNormal = TransformWorldToViewDir(TransformObjectToWorldNormal(input.normalOS));
        return output;
    }

    float4 Frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        return float4(input.viewNormal * 0.5 + 0.5, 1);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        ZWrite On ZTest LEqual Blend Off Cull Back

        Pass
        {
            Name "Camera Normals"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }

    Fallback Off
}
