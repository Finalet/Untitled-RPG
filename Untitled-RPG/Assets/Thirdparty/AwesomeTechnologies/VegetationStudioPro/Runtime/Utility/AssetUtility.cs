#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;


namespace AwesomeTechnologies.Utility
{
    public class AssetUtility
    {

#if UNITY_EDITOR
        public static long GetAssetSize(Object asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (assetPath == "") return 0;

            var path = Application.dataPath + assetPath.Substring(6);
            long length = new System.IO.FileInfo(path).Length;
            return length;
        }
#endif

        public static void SetTextureReadable(Texture2D texture, bool normalTexture)
        {
#if UNITY_EDITOR 
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.textureType = normalTexture ? TextureImporterType.NormalMap : TextureImporterType.Default;

                if (tImporter.isReadable) return;

                tImporter.isReadable = true;
                tImporter.SaveAndReimport();
            }
#endif
        }

        public static void SetTextureSGBA(Texture2D texture, bool value)
        {
#if UNITY_EDITOR 
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.sRGBTexture = value;
                tImporter.SaveAndReimport();
            }
#endif
        }

        public static bool HasCrunchCompression(Texture2D texture)
        {
#if UNITY_EDITOR
            if (null == texture) return false;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                return tImporter.crunchedCompression;
            }

#endif
            return false;
        }

        public static bool HasRgbaFormat(Texture2D texture)
        {
#if UNITY_EDITOR
            if (null == texture) return false;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                var pts = tImporter.GetDefaultPlatformTextureSettings();
                if (pts.format == TextureImporterFormat.ARGB32 || pts.format == TextureImporterFormat.RGBA32)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
#endif
            return false;
        }

        public static void SetRgba32Format(Texture2D texture)
        {
#if UNITY_EDITOR
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                var pts = tImporter.GetDefaultPlatformTextureSettings();
                pts.format = TextureImporterFormat.RGBA32;
                tImporter.SetPlatformTextureSettings(pts);
                tImporter.SaveAndReimport();
            }
#endif
        }

        public static void RemoveCrunchCompression(Texture2D texture)
        {
#if UNITY_EDITOR
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.crunchedCompression = false;
                tImporter.SaveAndReimport();
            }
#endif
        }

        public static void SetTextureReadable(Texture2D texture)
        {
#if UNITY_EDITOR           
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {

                if (tImporter.isReadable == false)
                {
                    tImporter.isReadable = true;
                    tImporter.SaveAndReimport();
                }
            }
#endif
        }
    }
}

