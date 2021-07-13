using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio {
  public class NearbyStarRenderer : BaseStarDataRenderer {
    private const int kMaxStars = 2000;
    private const int kStarPointTextureWidth = 2048;
    private const float kStarPaddingRadiusMultipler = 2.1f;

    private RenderTexture CreateRenderTexture(string name, int renderTextureSize, RenderTextureFormat format) {
      RenderTexture rt = RenderTexture.GetTemporary(
        renderTextureSize,
        renderTextureSize,
        0,
        format,
        RenderTextureReadWrite.Linear);

      rt.filterMode = FilterMode.Point;
      rt.wrapMode = TextureWrapMode.Clamp;
      rt.name = name;
      return rt;
    }

    private Material GetNearbyStarMaterial(Vector4 randomSeed, int starCount) {
      Material starPixelMat = new Material(new Material(Shader.Find("Hidden/Funly/Sky Studio/Computation/StarCalcNearby"))) {
        hideFlags = HideFlags.HideAndDontSave
      };

      starPixelMat.SetFloat("_StarDensity", density);
      starPixelMat.SetFloat("_NumStarPoints", starCount);
      starPixelMat.SetVector("_RandomSeed", randomSeed);
      starPixelMat.SetFloat("_TextureSize", kStarPointTextureWidth);

      return starPixelMat;
    }

    private void WriteDebugTexture(RenderTexture rt, string path) {
      Texture2D tex = ConvertToTexture2D(rt);
      File.WriteAllBytes(path, tex.EncodeToPNG());
    }

    private Texture2D GetStarListTexture(string starTexKey, out int validStarPixelCount) {
      // Generate a 1D texture that we'll sample to read back the points.
      Texture2D starPointTex = new Texture2D(kStarPointTextureWidth, 1, TextureFormat.RGBAFloat, false, true);
      starPointTex.filterMode = FilterMode.Point;

      int validStarCount = 0;
      float minDistance = maxRadius * kStarPaddingRadiusMultipler;
      List<Vector4> starPoints = new List<Vector4>();

      // Make sure star points are spaced so they don't overlap unless it's pixel sized stars.
      bool performPaddingCheck = maxRadius > .0015f;

      // Generate random points on a sphere for each star, and make sure they don't overlap.
      for (int i = 0; i < kMaxStars; i++) {
        Vector3 starPoint = Random.onUnitSphere;

        if (performPaddingCheck) {
          bool isToClose = false;
          for (int j = 0; j < starPoints.Count; j++) {
            float dist = Vector3.Distance(starPoint, starPoints[j]);
            if (dist < minDistance) {
              isToClose = true;
              break;
            }
          }

          if (isToClose) {
            continue;
          }
        }

        starPoints.Add(starPoint);
        starPointTex.SetPixel(validStarCount, 0, new Color(starPoint.x, 
                                                           starPoint.y, 
                                                           starPoint.z, 
                                                           0.0f));
        validStarCount += 1;
      }

      starPointTex.Apply();
      validStarPixelCount = validStarCount;

      return starPointTex;
    }
  

    public override IEnumerator ComputeStarData() {
      SendProgress(0);

      RenderTexture nearbyStarRT = CreateRenderTexture("Nearby Star " + layerId,
                                                       (int)imageSize,
                                                       RenderTextureFormat.ARGB32);

      RenderTexture previousRenderTexture = RenderTexture.active;

      Random.State state = Random.state;
      Random.InitState(layerId.GetHashCode());
      Vector4 randomSeed = new Vector4(
        Random.Range(0.0f, 1.0f),
        Random.Range(0.0f, 1.0f),
        Random.Range(0.0f, 1.0f),
        Random.Range(0.0f, 1.0f));
      
      int generatedStarCount;
      Texture2D nearbyStarTex = GetStarListTexture(layerId, out generatedStarCount);

      int desiredStarCount = Mathf.FloorToInt(Mathf.Clamp01(density) * kMaxStars);
      int actualStarCount = System.Math.Min(desiredStarCount, generatedStarCount);

      // Generate the nearby star texture.
      RenderTexture.active = nearbyStarRT;
      Material nearbyStarMat = GetNearbyStarMaterial(randomSeed, actualStarCount);
      Graphics.Blit(nearbyStarTex, nearbyStarMat);

      Texture2D tex = ConvertToTexture2D(nearbyStarRT);

      RenderTexture.active = previousRenderTexture;
      nearbyStarRT.Release();

      Random.state = state;
      SendCompletion(tex, true);

      yield break;
    }

    private Texture2D ConvertToTexture2D(RenderTexture rt) {
      Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
      tex.name = layerId;
      tex.filterMode = FilterMode.Point;
      tex.wrapMode = TextureWrapMode.Clamp;
      tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
      tex.Apply(false);

      return tex;
    }
  }
}

