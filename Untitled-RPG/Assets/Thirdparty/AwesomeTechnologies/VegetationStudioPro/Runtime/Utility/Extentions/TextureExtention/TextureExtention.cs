using System.Collections;
using System.Collections.Generic;
using System.IO;
using AwesomeTechnologies.VegetationSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.Utility.Extentions
{
    public static class TextureExtention
    {
        public static void FixBillboardArtifact(Texture2D texture, BillboardQuality billboardQuality)
        {
            int rowCount = BillboardAtlasRenderer.GetBillboardQualityRowCount(billboardQuality);
            int tileWidth = BillboardAtlasRenderer.GetBillboardQualityTileWidth(billboardQuality);
            int lineCount = tileWidth / 64;

            int textureWidth = texture.width;

            for (int i = 0; i <= rowCount - 1; i++)
            {
                for (int j = 0; j <= lineCount - 1; j++)
                {
                    for (int x = 0; x <= textureWidth - 1; x++)
                    {
                        Color pixelColor = texture.GetPixel(x, i * tileWidth + j);
                        pixelColor.a = 0;
                        texture.SetPixel(x, i * tileWidth + j, pixelColor);
                    }
                }
            }

            texture.Apply();
        }
        
        public static Texture2D CreatePaddedTexture(Texture2D sourceTexture, int paddingPassCount = 1024)
        {
            if (!SystemInfo.supportsComputeShaders) return null;

            var paddingShader = (ComputeShader)Resources.Load("AlphaPadding");

            // ReSharper disable once ReplaceWithSingleAssignment.False
            bool linear = false;
#if UNITY_EDITOR
            if (PlayerSettings.colorSpace == ColorSpace.Linear) linear = true;
#endif
            paddingShader.SetBool("Linear", linear);

            var paddingKernelHandle = paddingShader.FindKernel("ApplyAlphaPadding");
            //var applyOriginalAlphaKernelHandle = paddingShader.FindKernel("ApplyOriginalAlpha");
            var readSourceTextureKernelHandle = paddingShader.FindKernel("ReadSourceTexture");

            int width = sourceTexture.width;
            int height = sourceTexture.height;

            RenderTexture inputTexture;
            RenderTexture outputTexture;
            if (linear)
            {
                inputTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear);
                outputTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear);
            }
            else
            {
                inputTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                outputTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            }

            inputTexture.enableRandomWrite = true;
            inputTexture.Create();
            outputTexture.enableRandomWrite = true;
            outputTexture.Create();

            paddingShader.SetTexture(readSourceTextureKernelHandle, "SourceTexture", sourceTexture);
            paddingShader.SetTexture(readSourceTextureKernelHandle, "OutputTexture", inputTexture);
            paddingShader.Dispatch(readSourceTextureKernelHandle, width / 8, height / 8, 1);

            RenderTexture texture1 = inputTexture;
            RenderTexture texture2 = outputTexture;

            for (int i = 0; i <= paddingPassCount - 1; i++)
            {
                paddingShader.SetTexture(paddingKernelHandle, "InputTexture", texture1);
                paddingShader.SetTexture(paddingKernelHandle, "OutputTexture", texture2);
                paddingShader.Dispatch(paddingKernelHandle, width / 8, height / 8, 1);

                RenderTexture tempTexture = texture1;
                texture1 = texture2;
                texture2 = tempTexture;
            }

            //paddingShader.SetTexture(applyOriginalAlphaKernelHandle, "SourceTexture", sourceTexture);
            //paddingShader.SetTexture(applyOriginalAlphaKernelHandle, "OutputTexture", texture1);
            //paddingShader.Dispatch(applyOriginalAlphaKernelHandle, width / 8, height / 8, 1);          

            RenderTexture.active = texture1;
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true, linear);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = null;

            var px = result.GetPixels32();
            var pxSource = sourceTexture.GetPixels32();
            for (int i = 0; i < px.Length; i++)
            {
                px[i].a = pxSource[i].a;
            }
            result.SetPixels32(px);
            result.Apply();

           

            Object.DestroyImmediate(inputTexture);
            Object.DestroyImmediate(outputTexture);
            return result;
        }

        public static void SaveToFile(this Texture2D texture, string fileName)
        {
#if UNITY_EDITOR
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(fileName + ".png", bytes);
#endif
        }
    }
}
