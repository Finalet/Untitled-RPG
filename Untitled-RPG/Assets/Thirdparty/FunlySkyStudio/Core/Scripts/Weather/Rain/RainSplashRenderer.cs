using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Render ground rain splashes using GPU instancing and sprite sheets for performance.
  public class RainSplashRenderer : BaseSpriteInstancedRenderer
  {
    Camera m_DepthCamera;

    float[] m_StartSplashYPositions = new float[kArrayMaxSprites];
    float[] m_DepthUs = new float[kArrayMaxSprites];
    float[] m_DepthVs = new float[kArrayMaxSprites];

    // We sync these values from the sky profile.
    float m_SplashAreaStart;
    float m_SplashAreaLength;
    float m_SplashScale;
    float m_SplashScaleVarience;
    float m_SplashItensity;
    float m_SplashSurfaceOffset;
    SkyProfile m_SkyProfile;
    float m_TimeOfDay;
    RainSplashArtItem m_Style;
    Bounds m_Bounds = new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f));

    void Start()
    {
      // Verify GPU instancing is supported.
      if (SystemInfo.supportsInstancing == false) {
        Debug.LogError("Can't render rain splashes since GPU instancing is not supported on this platform.");
        enabled = false;
        return;
      }
      
      // JEFIXME - Make this a more graceful error.
      WeatherDepthCamera depthController = FindObjectOfType<WeatherDepthCamera>();
      if (depthController == null)
      {
        //m_DepthCamera = FindObjectOfType<URPWeatherDepth>().GetComponent<Camera>();
        // Debug.LogError("Can't generate splashes without a RainDepthCamera in the scene");
        // enabled = false;
        return;
      }

      m_DepthCamera = depthController.GetComponent<Camera>();
    }

    protected override Bounds CalculateMeshBounds()
    {
      return m_Bounds;
    }

    // Construct the data for a single particle.
    protected override BaseSpriteItemData CreateSpriteItemData()
    {
      return new RainSplashData();
    }

    protected override bool IsRenderingEnabled()
    {
      if (m_SkyProfile == null) {
        return false;
      }

      if (m_SkyProfile.IsFeatureEnabled(ProfileFeatureKeys.RainSplashFeature) == false) {
        return false;
      }

      if (m_ViewerCamera == null) {
        Debug.LogError("Can't render ground raindrops since no active camera has the MainCamera tag applied.");
        return false;
      }

      return true;
    }

    // Ask the subclass how many new instances we should create this frame.
    protected override int GetNextSpawnCount()
    {
      int delta = maxSprites - m_Active.Count;
      return delta > 0 ? delta : 0; 
    }

    // Select where the next sprite will be rendered at.
    protected override void CalculateSpriteTRS(BaseSpriteItemData data, out Vector3 spritePosition, out Quaternion spriteRotation, out Vector3 spriteScale)
    {
      // Create some variety in rain drop sizes.
      float minScale = m_SplashScale * (1.0f - m_SplashScaleVarience);
      float uniformScale = Random.Range(minScale, m_SplashScale);

      spritePosition = data.spritePosition;
      spriteRotation = Quaternion.identity;
      spriteScale = new Vector3(uniformScale, uniformScale, uniformScale);
    }

    // Configure a new sprite item data object properties, (could be new or recycled).
    protected override void ConfigureSpriteItemData(BaseSpriteItemData data)
    {
      data.spritePosition = CreateWorldSplashPoint();
      data.delay = Random.Range(0.0f, .5f);
    }

    // Setup any per-instance data you need to pass.
    protected override void PrepareDataArraysForRendering(int instanceId, BaseSpriteItemData data)
    {
      RainSplashData splash = data as RainSplashData;

      // We could probably move this into the shader to save some CPU calculations if necessary.
      Vector3 screenPoint = m_DepthCamera.WorldToScreenPoint(splash.spritePosition);
      Vector2 cameraUV = new Vector2(screenPoint.x / m_DepthCamera.pixelWidth, screenPoint.y / m_DepthCamera.pixelHeight);
      splash.depthTextureUV = cameraUV;

      m_StartSplashYPositions[instanceId] = splash.spritePosition.y;
      m_DepthUs[instanceId] = splash.depthTextureUV.x;
      m_DepthVs[instanceId] = splash.depthTextureUV.y;
    }

    protected override void PopulatePropertyBlockForRendering(ref MaterialPropertyBlock propertyBlock)
    {
      propertyBlock.SetFloat("_Intensity", m_SplashItensity);
      propertyBlock.SetFloatArray("_OverheadDepthU", m_DepthUs);
      propertyBlock.SetFloatArray("_OverheadDepthV", m_DepthVs);
      propertyBlock.SetFloatArray("_SplashStartYPosition", m_StartSplashYPositions);
      propertyBlock.SetFloat("_SplashGroundOffset", m_SplashSurfaceOffset);
    }

    public void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay, RainSplashArtItem style)
    {
      m_SkyProfile = skyProfile;
      m_TimeOfDay = timeOfDay;
      m_Style = style;

      if (m_SkyProfile == null) {
        return;
      }

      SyncDataFromSkyProfile();
    }

    void SyncDataFromSkyProfile()
    {
      maxSprites = (int)m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainSplashMaxConcurrentKey, m_TimeOfDay);
      m_SplashAreaStart = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainSplashAreaStartKey, m_TimeOfDay);
      m_SplashAreaLength = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainSplashAreaLengthKey, m_TimeOfDay);
      m_SplashScale = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainSplashScaleKey, m_TimeOfDay);
      m_SplashScaleVarience = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainSplashScaleVarienceKey, m_TimeOfDay);
      m_SplashItensity = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainSplashIntensityKey, m_TimeOfDay);
      m_SplashSurfaceOffset = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainSplashSurfaceOffsetKey, m_TimeOfDay);

      m_SplashScale *= m_Style.scaleMultiplier;
      m_SplashItensity *= m_Style.intensityMultiplier;

      m_SpriteSheetLayout.columns = m_Style.columns;
      m_SpriteSheetLayout.rows = m_Style.rows;
      m_SpriteSheetLayout.frameCount = m_Style.totalFrames;
      m_SpriteSheetLayout.frameRate = m_Style.animateSpeed;

      m_TintColor = m_Style.tintColor * m_SkyProfile.GetColorPropertyValue(ProfilePropertyKeys.RainSplashTintColorKey, m_TimeOfDay);

      modelMesh = m_Style.mesh;
      renderMaterial = m_Style.material;
    }

    // TODO - Expose this so clients can easily override placement of rain splashes.
    Vector3 CreateWorldSplashPoint()
    {
      float angle = Random.Range(0.0f, -170.0f);
      Vector3 randomDirection = Quaternion.Euler(new Vector3(0.0f, angle, 0.0f)) * Vector3.right;
      float radius = Random.Range(m_SplashAreaStart, m_SplashAreaStart + m_SplashAreaLength);
      
      Vector3 splashPoint = randomDirection.normalized * radius;

      return m_ViewerCamera.transform.TransformPoint(splashPoint);
    }
  }
}
