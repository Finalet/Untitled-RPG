using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AwesomeTechnologies.Common
{
    public class AssetPreviewCache
    {
        public Dictionary<Object, Texture2D> TextureCache = new Dictionary<Object, Texture2D>();
        public static AssetPreviewCache Instance;

        AssetPreviewCache()
        {
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
#else
            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
#endif            
        }

#if UNITY_2017_2_OR_NEWER
        void OnPlaymodeStateChanged(PlayModeStateChange playModeStateChange)
        {
            TextureCache.Clear();
        }
#else
        void OnPlaymodeStateChanged()
        {
            TextureCache.Clear();
        }
#endif

        public static Texture2D GetCachedAssetPreview(Object asset)
        {
            if (asset is Texture2D)
            {
                ValidateInstance();
                if (Instance.TextureCache.ContainsKey(asset))
                {
                    var previewTexture = Instance.TextureCache[asset];
                    if (previewTexture)
                    {
                        if (previewTexture.width > 2) return previewTexture;
                    }

                    Instance.TextureCache.Remove(asset);
                    previewTexture = CreateAssetPreview(asset);
                    Instance.TextureCache.Add(asset, previewTexture);
                    return previewTexture;
                }

                else
                {
                    var previewTexture = CreateAssetPreview(asset);
                    Instance.TextureCache.Add(asset, previewTexture);
                    return previewTexture;
                }
            }
            else
            {
                ValidateInstance();
                if (Instance.TextureCache.ContainsKey(asset))
                {
                    var previewTexture = Instance.TextureCache[asset];
                    if (previewTexture)
                    {
                        if (previewTexture.width > 2) return previewTexture;
                    }

                    Instance.TextureCache.Remove(asset);
                    previewTexture = CreateAssetPreview(asset);
                    Instance.TextureCache.Add(asset, previewTexture);
                    return previewTexture;
                }

                else
                {
                    var previewTexture = CreateAssetPreview(asset);
                    Instance.TextureCache.Add(asset, previewTexture);
                    return previewTexture;
                }
            }
        }

        public static Texture2D GetAssetPreview(Object asset)
        {          
            return AssetPreview.GetAssetPreview(asset);
        }

        static Texture2D CreateAssetPreview(Object asset)
        {
            var previewTexture = AssetPreview.GetAssetPreview(asset);
            Texture2D convertedTexture = new Texture2D(2, 2, TextureFormat.ARGB32, true, true);
            if (previewTexture)
            {
                convertedTexture.LoadImage(previewTexture.EncodeToPNG());
            }
            return convertedTexture;
        }

        static void ValidateInstance()
        {
            if (Instance == null)
            {
                Instance = new AssetPreviewCache();
            }
        }
    }
}
