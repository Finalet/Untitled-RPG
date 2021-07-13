using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Funly.SkyStudio
{
  [CustomEditor(typeof(RenderCloudCubemap))]
  public class RenderCloudCubemapEditor : Editor
  {
    private struct CubeFaceData
    {
      public CubemapFace face;
      public Vector2 offset;

      public CubeFaceData(CubemapFace face, Vector2 offset)
      {
        this.face = face;
        this.offset = offset;
      }
    }

    public override void OnInspectorGUI()
    {
      RenderCloudCubemap settings = this.target as RenderCloudCubemap;

      Camera cam = settings.GetComponent<Camera>();
      cam.gameObject.transform.position = Vector3.zero;
      cam.gameObject.transform.rotation = Quaternion.identity;

      EditorGUILayout.BeginVertical();
      EditorGUILayout.PropertyField(this.serializedObject.FindProperty("filenamePrefix"));
      EditorGUILayout.PropertyField(this.serializedObject.FindProperty("faceWidth"));
      EditorGUILayout.PropertyField(this.serializedObject.FindProperty("textureFormat"));
      EditorGUILayout.PropertyField(this.serializedObject.FindProperty("exportFaces"));

      if (settings.filenamePrefix == null || settings.filenamePrefix.Length <= 0) {
        settings.filenamePrefix = RenderCloudCubemap.kDefaultFilenamePrefix;
      }

      // Stamp the texture format so it's easier for user to select Sky Studio cloud type later.
      string scenePath = Path.GetDirectoryName(SceneManager.GetActiveScene().path);
      string filenamePrefix = scenePath + "/" + settings.filenamePrefix + "-" + settings.textureFormat;

      if (GUILayout.Button("Render Cubemap")) {
        if (settings.textureFormat == RenderCloudCubemap.CubemapTextureFormat.RGBColor) {
          Debug.Log("Rendering RGB color cubemap (additive texture)...");
          Cubemap cubemap = RenderRGBColor(settings, cam);
          RenderCubemapToTexture(cubemap, settings.faceWidth, Color.black, TextureFormat.RGB24, 
            settings.exportFaces, filenamePrefix, false);
        } else if (settings.textureFormat == RenderCloudCubemap.CubemapTextureFormat.RGBAColor) {
          Debug.Log("Rendering RGBA color cubemap (alpha texture)...");
          Cubemap cubemap = RenderRGBAColor(settings, cam);
          RenderCubemapToTexture(cubemap, settings.faceWidth, Color.clear, TextureFormat.RGBA32, 
            settings.exportFaces, filenamePrefix, true);
        } else if (settings.textureFormat == RenderCloudCubemap.CubemapTextureFormat.RGBALit) {
          Debug.Log("Rendering RGBA normal lit cubemap (normals encoded)...");
          Cubemap cubemap = RenderRGBANormal(settings, cam);
          RenderCubemapToTexture(cubemap, settings.faceWidth, Color.clear, TextureFormat.RGBA32,
            settings.exportFaces, filenamePrefix, false);
        } else {
          Debug.LogError("Invalid texture format, can't render cubemap.");
          return;
        }
      }
      EditorGUILayout.EndVertical();

      this.serializedObject.ApplyModifiedProperties();
    }

    private Cubemap RenderRGBColor(RenderCloudCubemap settings, Camera cam)
    {
      Cubemap cubemap = new Cubemap(settings.faceWidth, TextureFormat.RGB24, false);

      cam.ResetReplacementShader();
      cam.backgroundColor = Color.black;
      cam.RenderToCubemap(cubemap);
      cam.targetTexture = null;

      return cubemap;
    }

    private Cubemap RenderRGBAColor(RenderCloudCubemap settings, Camera cam)
    {
      Cubemap cubemap = new Cubemap(settings.faceWidth, TextureFormat.RGBA32, false);

      cam.ResetReplacementShader();
      cam.backgroundColor = Color.clear;
      cam.RenderToCubemap(cubemap);
      cam.targetTexture = null;
      
      return cubemap;
    }

    private Cubemap RenderRGBANormal(RenderCloudCubemap settings, Camera cam)
    {
      Cubemap cubemap = new Cubemap(settings.faceWidth, TextureFormat.RGBA32, false);

      Shader normalShader = Shader.Find("Funly/Sky Studio/Utility/World Normal");
      cam.backgroundColor = Color.clear;
      cam.SetReplacementShader(normalShader, "");
      cam.RenderToCubemap(cubemap);
      cam.ResetReplacementShader();
      cam.targetTexture = null;

      return cubemap;
    }

    private Texture RenderCubemapToTexture(Cubemap cubemap, int faceSize, Color clearColor, 
      TextureFormat textureFormat, bool exportFaces, string assetPathPrefix, bool alphaIsTransparency)
    {
      CubeFaceData[] faces = {
        new CubeFaceData(CubemapFace.NegativeX, new Vector2(0, faceSize)),
        new CubeFaceData(CubemapFace.PositiveX, new Vector2(faceSize * 2, faceSize)),
        new CubeFaceData(CubemapFace.PositiveY, new Vector2(faceSize, faceSize * 2)),
        new CubeFaceData(CubemapFace.NegativeY, new Vector2(faceSize, 0)),
        new CubeFaceData(CubemapFace.PositiveZ, new Vector2(faceSize, faceSize)),
        new CubeFaceData(CubemapFace.NegativeZ, new Vector2(faceSize * 3, faceSize))
      };

      RenderTexture oldRt = RenderTexture.active;

      Texture2D faceTex = new Texture2D(faceSize, faceSize, TextureFormat.RGBA32, false);
      RenderTexture faceRenderTex = new RenderTexture(faceSize, faceSize, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
      faceRenderTex.Create();

      RenderTexture flatCubeTex = new RenderTexture(faceSize * 4, faceSize * 3, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
      flatCubeTex.Create();

      Material flipMat = new Material(Shader.Find("Funly/Sky Studio/Utility/Flip Image"));
      Material blitMat = new Material(Shader.Find("Unlit/Transparent"));

      RenderTexture.active = flatCubeTex;
      GL.PushMatrix();
      GL.LoadPixelMatrix(0, flatCubeTex.width, flatCubeTex.height, 0);
      GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
      GL.PopMatrix();
      RenderTexture.active = null;

      for (int i = 0; i < faces.Length; i++) {
        CubeFaceData data = faces[i];

        // Pull a face texture out of the cubemap.
        Color[] pixels = cubemap.GetPixels(data.face);
        faceTex.SetPixels(pixels);
        faceTex.Apply();

        // Flip the image.
        RenderTexture.active = faceRenderTex;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, faceRenderTex.width, flatCubeTex.height, 0);
        Graphics.Blit(faceTex, faceRenderTex, flipMat, 0);
        GL.PopMatrix();
        
        if (exportFaces) {
          string path = assetPathPrefix + data.face.ToString() + ".png";
          SkyEditorUtility.WriteTextureToFile(faceRenderTex, path, textureFormat);
          RenderTexture.active = null;
          AssetDatabase.ImportAsset(path);
        }
        RenderTexture.active = null;

        // Target our cubemap, and render the flipped face into it.
        RenderTexture.active = flatCubeTex;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, flatCubeTex.width, flatCubeTex.height, 0);
        Graphics.DrawTexture(new Rect(data.offset.x, data.offset.y, faceSize, faceSize), faceRenderTex, blitMat);

        // Write the final texture on last face.
        if (i == faces.Length - 1) {
          string path = assetPathPrefix + ".png";
          Debug.Log("Exporting cubemap to compressed texture at path: " + path);
          SkyEditorUtility.WriteTextureToFile(flatCubeTex, path, textureFormat);
          RenderTexture.active = null;
          AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

          // Adjust texture settings.
          TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
          importer.textureShape = TextureImporterShape.TextureCube;
          importer.alphaIsTransparency = alphaIsTransparency;
          AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        GL.PopMatrix();
        RenderTexture.active = null;
      }

      RenderTexture.active = oldRt;

      return flatCubeTex;
    }
  }
}

