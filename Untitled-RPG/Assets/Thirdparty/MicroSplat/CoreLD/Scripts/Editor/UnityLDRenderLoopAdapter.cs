using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;
using UnityEditor.Callbacks;

namespace JBooth.MicroSplat
{
    [InitializeOnLoad]
    public class UnityLDRenderLoopAdapter : IRenderLoopAdapter
    {
        const string sDefine = "__MICROSPLAT_URP__";
        static UnityLDRenderLoopAdapter()
        {
            MicroSplatDefines.InitDefine(sDefine);
        }

        [PostProcessSceneAttribute(0)]
        public static void OnPostprocessScene()
        {
            MicroSplatDefines.InitDefine(sDefine);
        }



        static TextAsset template;
        static TextAsset adapter;
        static TextAsset sharedInc;
        static TextAsset terrainBody;
        static TextAsset terrainBlendBody;
        static TextAsset terrainBlendCBuffer;
        static TextAsset sharedHD;
        static TextAsset properties;
        static TextAsset vertex;
        static TextAsset mainFunc;
        static TextAsset templateDecal;
        static TextAsset v2f;

        public string GetDisplayName()
        {
            return "Unity LD";
        }

        public string GetRenderLoopKeyword()
        {
            return "_MSRENDERLOOP_UNITYLD";
        }

        public int GetNumPasses() { return 1; }

        public void WriteShaderHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
        {
            sb.AppendLine("   SubShader {");


            sb.Append("      Tags{\"RenderPipeline\"=\"LightweightPipeline\" \"RenderType\" = \"Opaque\" \"Queue\" = \"Geometry+100\" \"IgnoreProjector\" = \"False\" ");


            if (features.Contains("_MAX4TEXTURES"))
            {
                sb.Append("\"SplatCount\" = \"4\"");
            }
            else if (features.Contains("_MAX8TEXTURES"))
            {
                sb.Append("\"SplatCount\" = \"8\"");
            }
            else if (features.Contains("_MAX12TEXTURES"))
            {
                sb.Append("\"SplatCount\" = \"12\"");
            }
            else if (features.Contains("_MAX20TEXTURES"))
            {
                sb.Append("\"SplatCount\" = \"20\"");
            }
            else if (features.Contains("_MAX24TEXTURES"))
            {
                sb.Append("\"SplatCount\" = \"24\"");
            }
            else if (features.Contains("_MAX28TEXTURES"))
            {
                sb.Append("\"SplatCount\" = \"28\"");
            }
            else if (features.Contains("_MAX32TEXTURES"))
            {
                sb.Append("\"SplatCount\" = \"32\"");
            }
            else
            {
               sb.Append ("\"SplatCount\" = \"16\"");
            }
            sb.AppendLine("}");


        }

        public bool UseReplaceMethods()
        {
            return true;
        }

        public void WritePassHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {

        }


        public void WriteVertexFunction(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {

        }

        public void WriteFragmentFunction(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {


        }

        public void WritePerMaterialCBuffer(string[] features, StringBuilder sb, bool blend)
        {
        }


        public void WriteShaderFooter(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend, string baseName)
        {
            sb.AppendLine("      }");
            if (blend)
            {
                sb.AppendLine("   CustomEditor \"MicroSplatBlendableMaterialEditor\"");
            }
            else if (baseName != null)
            {
               if (!features.Contains ("_MICROMESH"))
               {
                  sb.AppendLine ("   Dependency \"AddPassShader\" = \"Hidden/MicroSplat/AddPass\"");
                  sb.AppendLine ("   Dependency \"BaseMapShader\" = \"" + baseName + "\"");
                  //sb.AppendLine ("   Dependency \"BaseMapGenShader\" = \"" + baseName + "\"");
               }
                sb.AppendLine("   CustomEditor \"MicroSplatShaderGUI\"");
            }
            sb.AppendLine("   Fallback \"Nature/Terrain/Diffuse\"");
            sb.Append("}");
        }

        public void Init(string[] paths)
        {
            for (int i = 0; i < paths.Length; ++i)
            {
                string p = paths[i];
#if UNITY_2019_3_OR_NEWER
                if (p.EndsWith("microsplat_terrain_unityld_template_20193.txt"))
                {
                    template = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_template_decal_20193.txt"))
                {
                    templateDecal = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
#elif UNITY_2019_2_OR_NEWER
                if (p.EndsWith("microsplat_terrain_unityld_template_20192.txt"))
                {
                    template = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_template_decal_20192.txt"))
                {
                    templateDecal = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
#elif UNITY_2019_1_OR_NEWER
                if (p.EndsWith("microsplat_terrain_unityld_template_20191.txt"))
                {
                    template = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_template_decal_20191.txt"))
                {
                    templateDecal = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
#else // 2018.3
                if (p.EndsWith("microsplat_terrain_unityld_template_20183.txt"))
                {
                    template = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_template_decal_20183.txt"))
                {
                    templateDecal = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
#endif

                if (p.EndsWith("microsplat_terrain_unityld_adapter.txt"))
                {
                    adapter = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrainblend_body.txt"))
                {
                    terrainBlendBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrainblend_cbuffer.txt"))
                {
                    terrainBlendCBuffer = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }

                if (p.EndsWith("microsplat_terrain_body.txt"))
                {
                    terrainBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_shared.txt"))
                {
                    sharedInc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_shared.txt"))
                {
                    sharedHD = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_properties.txt"))
                {
                    properties = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_vertex.txt"))
                {
                    vertex = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_mainfunc.txt"))
                {
                    mainFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
                if (p.EndsWith("microsplat_terrain_unityld_v2f.txt"))
                {
                    v2f = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                }
            }
        }

        public void WriteProperties(string[] features, StringBuilder sb, bool blend)
        {
            sb.AppendLine(properties.text);
        }

        public void PostProcessShader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
        {
            StringBuilder temp = new StringBuilder();
            compiler.WriteFeatures(features, temp);
            if (blend && !(features.Contains ("_MICRODIGGERMESH") || features.Contains ("_OUTPUTDIGGER")))
            {
                temp.AppendLine("      #define _SRPTERRAINBLEND 1");
            }

            if (blend)
            {
               if (features.Contains("_MESHOVERLAYSPLATS"))
               {
                  temp.AppendLine("      #define _MESHOVERLAYSPLATSSHADER 1");
               }
               else if (features.Contains("_OUTPUTDIGGER"))
               {
                  temp.AppendLine("      #define _MICRODIGGERMESH");
               }
               else
               {
                  temp.AppendLine("      #define _TERRAINBLENDABLESHADER 1");
               }
            }


            if (features.Contains("_USESPECULARWORKFLOW"))
            {
                temp.AppendLine("      #define _SPECULAR_SETUP");
            }

            StringBuilder cbuffer = new StringBuilder();
            compiler.WritePerMaterialCBuffer(features, cbuffer);
            if (blend && !(features.Contains ("_MICRODIGGERMESH") || features.Contains ("_OUTPUTDIGGER")))
            {
                cbuffer.AppendLine(terrainBlendCBuffer.text);
            }

            sb = sb.Replace("//MS_DEFINES", temp.ToString());

            sb = sb.Replace("//MS_ADAPTER", adapter.ToString());
            sb = sb.Replace("//MS_SHARED_INC", sharedInc.text);
            sb = sb.Replace("//MS_SHARED_HD", sharedHD.text);
            sb = sb.Replace("//MS_TERRAIN_BODY", terrainBody.text);
            sb = sb.Replace("//MS_VERTEXMOD", vertex.text);
            sb = sb.Replace("//MS_MAINFUNC", mainFunc.text);
            sb = sb.Replace("//MS_CBUFFER", cbuffer.ToString());
            sb = sb.Replace("//MS_V2F", v2f.ToString());

            if (blend && !features.Contains("_MICRODIGGERMESH") && !features.Contains("_OUTPUTDIGGER"))
            {
                sb = sb.Replace("//MS_BLENDABLE", terrainBlendBody.text);
                sb = sb.Replace("Blend [_SrcBlend] [_DstBlend], [_AlphaSrcBlend] [_AlphaDstBlend]", "Blend SrcAlpha OneMinusSrcAlpha");
            }

            // extentions
            StringBuilder ext = new StringBuilder();
            compiler.WriteExtensions(features, ext);
            
            sb = sb.Replace("//MS_EXTENSIONS", ext.ToString());

            ext = new StringBuilder();
            foreach (var e in compiler.extensions)
            {
                e.WriteAfterVetrexFunctions(ext);
            }

            sb = sb.Replace("//MS_AFTERVERTEX", ext.ToString());

            sb = sb.Replace("fixed", "half");

            if (features.Contains("_TESSDISTANCE"))
            {
                sb = sb.Replace("#pragma vertex vert", "#pragma hull hull\n #pragma domain domain\n #pragma vertex tessvert\n#pragma require tessellation tessHW\n");
                sb = sb.Replace("#pragma vertex ShadowPassVertex", "#pragma hull hull\n #pragma domain domain\n #pragma vertex tessvert\n#pragma require tessellation tessHW\n");
            }

        }

        public void WriteSharedCode(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {
            if (blend && !(features.Contains ("_MICRODIGGERMESH") || features.Contains ("_OUTPUTDIGGER")))
            {
                sb.AppendLine(templateDecal.text);
            }
            else
            {
                sb.AppendLine(template.text);
            }
        }

        public void WriteTerrainBody(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {


        }

        public MicroSplatShaderGUI.PassType GetPassType(int i)
        {
            return MicroSplatShaderGUI.PassType.Color;
        }

        public string GetVersion()
        {
            return "3.4";
        }
    }
}
