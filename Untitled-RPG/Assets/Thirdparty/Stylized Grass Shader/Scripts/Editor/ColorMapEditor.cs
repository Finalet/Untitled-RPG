using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace StylizedGrass
{
    public class ColorMapEditor : Editor
    {
        public const float CLIP_PADDING = 1f;
        public const float HEIGHT_OFFSET = 1000f;
        private static List<Light> dirLights;
        private static AmbientMode ambientMode;
        private static Color ambientColor;

        private static float originalTerrainHeight;

        public static string[] reslist = new string[] { "64x64", "128x128", "256x256", "512x512", "1024x1024", "2048x2048" };

        public static GrassColorMap NewColorMap()
        {
            GrassColorMap colorMap = ScriptableObject.CreateInstance<GrassColorMap>();

            SetName(colorMap);

            return colorMap;
        }

        public static void RenderColorMap(GrassColorMapRenderer renderer)
        {
            if (!renderer.colorMap) renderer.colorMap = ScriptableObject.CreateInstance<GrassColorMap>();

            //If no area was defined, automatically calculate it
            if (renderer.colorMap.bounds.size == Vector3.zero)
            {
                ApplyUVFromTerrainBounds(renderer.colorMap, renderer);
            }
            else
            {
                renderer.colorMap.uv = BoundsToUV(renderer.colorMap.bounds);
            }

            renderer.colorMap.overrideTexture = false;

            SetupRenderer(renderer);
            SetupLighting();
            RenderToTexture(renderer);
            RestoreLighting();

            renderer.colorMap.SetActive();
        }

        public static void SetName(GrassColorMap colorMap)
        {
            string prefix = EditorSceneManager.GetActiveScene().name;
            if (prefix == "") prefix = "Untitled";

#if UNITY_EDITOR
            colorMap.name = EditorSceneManager.GetActiveScene().name + "_GrassColormap";
            if (colorMap.texture != null) colorMap.texture.name = EditorSceneManager.GetActiveScene().name + "_GrassColormap";
#else
            colorMap.name = EditorSceneManager.GetActiveScene().name + "_GrassColormap";
            if (colorMap.texture != null) colorMap.texture.name = "GrassColorMap_" + colorMap.GetInstanceID();
#endif
        }

        public static GrassColorMap SaveColorMapToAsset(GrassColorMap colorMap)
        {
            string assetPath = "Assets/";
            assetPath = EditorUtility.SaveFolderPanel("Asset destination folder", assetPath, "");
            if (assetPath == string.Empty) return colorMap;

            assetPath = assetPath.Replace(Application.dataPath, "Assets/");
            assetPath += colorMap.name + ".asset";

            AssetDatabase.CreateAsset(colorMap, assetPath);

            colorMap = (GrassColorMap)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GrassColorMap));

            //Save texture to asset
            if (!colorMap.texture) colorMap.texture = new Texture2D(colorMap.resolution, colorMap.resolution);

            colorMap.texture.name = colorMap.name + " Texture";
            AssetDatabase.AddObjectToAsset(colorMap.texture, colorMap);
            string path = AssetDatabase.GetAssetPath(colorMap.texture);
            AssetDatabase.ImportAsset(path);

            //Reference serialized texture asset
            colorMap.texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));

            return colorMap;
        }

        public static double GetTexelSize(float texelSize, float worldSize)
        {
            return System.Math.Round(texelSize / worldSize, 2);
        }

        public static int IndexToResolution(int i)
        {
            int res = 0;

            switch (i)
            {
                case 0:
                    res = 64; break;
                case 1:
                    res = 128; break;
                case 2:
                    res = 256; break;
                case 3:
                    res = 512; break;
                case 4:
                    res = 1024; break;
                case 5:
                    res = 2048; break;
            }

            return res;
        }

        public static void ApplyUVFromTerrainBounds(GrassColorMap colorMap, GrassColorMapRenderer renderer)
        {
            colorMap.bounds = ColorMapEditor.GetTerrainBounds(renderer.terrainObjects);
            colorMap.uv = ColorMapEditor.BoundsToUV(renderer.colorMap.bounds);
        }

        public static void SetupRenderer(GrassColorMapRenderer renderer)
        {
            if (!renderer.renderCam) renderer.renderCam = new GameObject().AddComponent<Camera>();

            renderer.renderCam.name = "Grass color map renderCam";
            renderer.renderCam.enabled = false;

            //Camera set up
            renderer.renderCam.orthographic = true;
            renderer.renderCam.orthographicSize = (renderer.colorMap.bounds.size.x / 2);
            renderer.renderCam.farClipPlane = renderer.colorMap.bounds.size.y + CLIP_PADDING;
            renderer.renderCam.clearFlags = CameraClearFlags.Color;
            renderer.renderCam.backgroundColor = Color.red;

            //Position cam in given center of terrain(s)
            renderer.renderCam.transform.position = new Vector3(
                renderer.colorMap.bounds.center.x,
                renderer.colorMap.bounds.center.y + renderer.colorMap.bounds.extents.y + CLIP_PADDING,
                renderer.colorMap.bounds.center.z
                );

            renderer.renderCam.transform.localEulerAngles = new Vector3(90, 0, 0);
        }

        public static void SetupLighting()
        {
            //Setup faux albedo lighting
            Light[] lights = FindObjectsOfType<Light>();
            dirLights = new List<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    dirLights.Add(light);
                    light.enabled = false;
                }
            }

            ambientMode = RenderSettings.ambientMode;
            ambientColor = RenderSettings.ambientLight;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
        }

        private static void RenderToTexture(GrassColorMapRenderer renderer)
        {
            if (!renderer.renderCam)
            {
                Debug.LogError("Renderer does not have a render cam set up");
                return;
            }

            //Temporarly disable foliage and move terrains up 1000 units
            if (renderer.terrainObjects != null || renderer.terrainObjects.Count != 0)
            {
                if (renderer.terrainObjects[0]) originalTerrainHeight = renderer.terrainObjects[0].transform.position.y;

                foreach (GameObject item in renderer.terrainObjects)
                {
                    Terrain t = item.GetComponent<Terrain>();
                    if (t) t.drawTreesAndFoliage = false;

                    if (renderer.useLayers == false && renderer.terrainObjects[0])
                    {
                        item.transform.position = new Vector3(item.transform.position.x, item.transform.position.y + HEIGHT_OFFSET, item.transform.position.z);
                    }
                }
            }
            if (renderer.useLayers)
            {
                renderer.renderCam.cullingMask = renderer.renderLayer;
            }

            //Position cam in given center of terrain(s)
            renderer.renderCam.transform.position = new Vector3(
                renderer.colorMap.bounds.center.x,
                renderer.colorMap.bounds.center.y + renderer.colorMap.bounds.extents.y + CLIP_PADDING + (renderer.useLayers ? 0f : HEIGHT_OFFSET),
                renderer.colorMap.bounds.center.z
                );
                
            //Set up render texture
            RenderTexture rt = new RenderTexture(renderer.resolution, renderer.resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            renderer.renderCam.targetTexture = rt;
            RenderTexture.active = rt;

            //Render camera into a texture
            renderer.renderCam.Render();
            Texture2D render = new Texture2D(renderer.resolution, renderer.resolution, TextureFormat.RGB24, false, true);
            render.ReadPixels(new Rect(0, 0, renderer.resolution, renderer.resolution), 0, 0);
            render.Apply();
            render.Compress(false);
            render.name = renderer.colorMap.name;

            if (!renderer.colorMap.texture) renderer.colorMap.texture = new Texture2D(renderer.colorMap.resolution, renderer.colorMap.resolution);

            //Saving texture
            if (EditorUtility.IsPersistent(renderer.colorMap))
            {
                //string texPath = AssetDatabase.GetAssetPath(renderer.colorMap.texture);
                //byte[] bytes = render.EncodeToPNG();
                //System.IO.File.WriteAllBytes(texPath, bytes);
                //AssetDatabase.ImportAsset(texPath, ImportAssetOptions.Default);
                //SaveTexture(render, renderer.colorMap);

                EditorUtility.CopySerialized(render, renderer.colorMap.texture);
                DestroyImmediate(render);
            }
            else
            {
                renderer.colorMap.texture = render;
            }

            SetName(renderer.colorMap);

            EditorUtility.SetDirty(renderer.colorMap);

            //Cleanup
            renderer.renderCam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);
            DestroyImmediate(renderer.renderCam.gameObject);
            renderer.renderCam = null;

            //Restore terrains to original state
            foreach (var item in renderer.terrainObjects)
            {
                Terrain t = item.GetComponent<Terrain>();
                if (t) t.drawTreesAndFoliage = true;

                if (renderer.useLayers == false && renderer.terrainObjects[0]) item.transform.position = new Vector3(item.transform.position.x, originalTerrainHeight, item.transform.position.z);

            }

        }

        public static void RestoreLighting()
        {
            //Restore lighting
            foreach (Light light in dirLights)
            {
                light.enabled = true;
            }
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientLight = ambientColor;
        }

        public static Bounds GetTerrainBounds(List<GameObject> terrainObjects)
        {
            Bounds b = new Bounds(Vector3.zero, Vector3.zero);

            Vector3 minSum = Vector3.one * Mathf.Infinity;
            Vector3 maxSum = Vector3.zero;

            foreach (GameObject item in terrainObjects)
            {
                if (item == null) continue;

                Terrain t = item.GetComponent<Terrain>();
                MeshRenderer r = t ? null : item.GetComponent<MeshRenderer>();

                if (t)
                {
                    //Encapsulate two corners
                    Vector3 min = t.GetPosition() + t.terrainData.bounds.min;
                    if (min.magnitude < minSum.magnitude) minSum = min;

                    Vector3 max = t.GetPosition() + t.terrainData.bounds.max;
                    if (max.magnitude > maxSum.magnitude) maxSum = max;
                }

                if (r)
                {
                    Vector3 min = r.bounds.min;
                    if (min.magnitude < minSum.magnitude) minSum = min;

                    Vector3 max = r.bounds.max;
                    if (max.magnitude > maxSum.magnitude) maxSum = max;
                }
            }

            b.center = Vector3.Lerp(minSum, maxSum, 0.5f);
            b.Encapsulate(minSum);
            b.Encapsulate(maxSum);

            //Ensure bounds is always square
            if (b.size.x > b.size.z) b.size = new Vector3(b.size.x, b.size.y, b.size.x);
            if (b.size.z > b.size.x) b.size = new Vector3(b.size.z, b.size.y, b.size.z);

            return b;
        }

        public static Vector4 BoundsToUV(Bounds b)
        {
            Vector4 uv = new Vector4();

            //Origin position
            uv.x = b.min.x;
            uv.y = b.min.z;
            //Scale factor
            uv.z = 1f / b.size.x;
            uv.w = 0f;

            return uv;
        }
    }
}