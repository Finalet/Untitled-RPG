using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio {
  // Camera renders some depth and normal information from above the main camera.
  [RequireComponent(typeof(Camera))]
  public class WeatherDepthCamera : MonoBehaviour {
    Camera m_DepthCamera;

    // Replacement depth shader.
    [Tooltip("Shader used to render out depth + normal texture. This should be the sky studio depth shader.")]
    public Shader depthShader;

    // Texture with an overhead depth and normal render above the active main camera.
    [HideInInspector]
    public RenderTexture overheadDepthTexture;

    [Tooltip("You can help increase performance by only rendering periodically some number of frames.")]
    [Range(1, 60)]
    public int renderFrameInterval = 5;

    [Tooltip("The resolution of the texture. Higher resolution uses more rendering time but makes more precise weather along edges.")]
    [Range(128, 8192)]
    public int textureResolution = 1024;

    void Start() {
      m_DepthCamera = GetComponent<Camera>();

      // Disable the camera so we can control the rendering interval.
      m_DepthCamera.enabled = false;
    }

    void Update() {
      if (m_DepthCamera.enabled) {
        m_DepthCamera.enabled = false;
      }

      if (Time.frameCount % renderFrameInterval != 0) {
          return;
      }

      RenderOverheadCamera();
    }

    void RenderOverheadCamera()
    {
      PrepareRenderTexture();

      if (depthShader == null) {
        Debug.LogError("Can't render depth since depth shader is missing.");
        return;
      }

      RenderTexture previousRT = RenderTexture.active;
      RenderTexture.active = overheadDepthTexture;

      GL.Clear(true, true, Color.black);

      m_DepthCamera.RenderWithShader(depthShader, "RenderType");
      RenderTexture.active = previousRT;

      Shader.SetGlobalTexture("_OverheadDepthTex", overheadDepthTexture);
      Shader.SetGlobalVector("_OverheadDepthPosition", m_DepthCamera.transform.position);
      Shader.SetGlobalFloat("_OverheadDepthNearClip", m_DepthCamera.nearClipPlane);
      Shader.SetGlobalFloat("_OverheadDepthFarClip", m_DepthCamera.farClipPlane);
    }
  
    
    void PrepareRenderTexture() {
      if (overheadDepthTexture == null) {
        // Increase rendering performance by lowering the resolution of the depth render.
        int halfScreenResolution = Mathf.ClosestPowerOfTwo(Mathf.FloorToInt(textureResolution));

        // Based on what the platform supports we end up with more or less texture precision for depth.
        RenderTextureFormat bestFormat = RenderTextureFormat.ARGB32;
        
        // // Only Uncomment this if you need higher precision, should be fine without it.
        // if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)) {
        //   bestFormat = RenderTextureFormat.ARGBFloat;
        // } else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB64)) {
        //   bestFormat = RenderTextureFormat.ARGB64;
        // }

        overheadDepthTexture = new RenderTexture(
          halfScreenResolution, halfScreenResolution, 24, 
          bestFormat, RenderTextureReadWrite.Linear);

        overheadDepthTexture.useMipMap = false;
        overheadDepthTexture.autoGenerateMips = false;
        overheadDepthTexture.filterMode = FilterMode.Point;
        overheadDepthTexture.antiAliasing = 2;
      }

      if (overheadDepthTexture.IsCreated() == false) {
        overheadDepthTexture.Create();
      }

      if (m_DepthCamera.targetTexture != overheadDepthTexture) {
        m_DepthCamera.targetTexture = overheadDepthTexture;
      }
    }

  }
}
