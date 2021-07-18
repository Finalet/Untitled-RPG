using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Funly.SkyStudio
{
  // This controller manages time and updating the skybox material with the proper configuration
  // values for the current time of day. This loads sky data from your sky profile.
  [ExecuteInEditMode]
  public class TimeOfDayController : MonoBehaviour
  {
    // Get access to the most recently created TimeOfDayController.
    public static TimeOfDayController instance { get; private set; }

    [Tooltip("Sky profile defines the skyColors configuration for times of day. " +
      "This script will animate between those skyColors values based on the time of day.")]
    [SerializeField]
    private SkyProfile m_SkyProfile;
    public SkyProfile skyProfile
    {
      get { return m_SkyProfile; }
      set
      {
        if (value != null && copySkyProfile)
        {
          m_SkyProfile = Instantiate(value);
        }
        else
        {
          m_SkyProfile = value;
        }
        m_SkyMaterialController = null;
        UpdateSkyForCurrentTime();
        SynchronizeAllShaderKeywords();
      }
    }

    [Tooltip("Time is expressed in a fractional number of days that have completed.")]
    [SerializeField]
    private float m_SkyTime = 0;
    public float skyTime
    {
      get { return m_SkyTime; }
      set
      {
        m_SkyTime = Mathf.Abs(value);
        UpdateSkyForCurrentTime();
      }
    }

    [Tooltip("Automatically advance time at fixed speed.")]
    public bool automaticTimeIncrement;

    [Tooltip("Create a copy of the sky profile at runtime, so modifications don't affect the original Sky Profile in your project.")]
    public bool copySkyProfile;

    // Use the Sky Material controller to directly manipulate the skybox values programatically.
    private SkyMaterialController m_SkyMaterialController;
    public SkyMaterialController SkyMaterial { get { return m_SkyMaterialController; } }

    [Tooltip("Speed at which to advance time by if in automatic increment is enabled.")]
    [Range(0, 1)]
    public float automaticIncrementSpeed = .01f;

    [Tooltip("Sun orbit.")]
    public OrbitingBody sunOrbit;

    [Tooltip("Moon orbit.")]
    public OrbitingBody moonOrbit;

    [Tooltip("Controller for managing weather effects")]
    public WeatherController weatherController;

    [Tooltip("If true we'll invoke DynamicGI.UpdateEnvironment() when skybox changes. This is an expensive operation.")]
    public bool updateGlobalIllumination = false;

    // Callback invoked whenever the time of day changes.
    public delegate void TimeOfDayDidChange(TimeOfDayController tc, float timeOfDay);
    public event TimeOfDayDidChange timeChangedCallback;

    private bool m_DidInitialUpdate;

    // Current progress value through a day cycle (value 0-1).
    public float timeOfDay
    {
      get { return m_SkyTime - ((int)m_SkyTime); }
    }

    public int daysElapsed
    {
      get { return (int)m_SkyTime; }
    }

    void Awake()
    {
      instance = this;
    }

    private void OnEnabled()
    {
      skyTime = m_SkyTime;
    }

    private void OnValidate()
    {
      if (gameObject.activeInHierarchy == false)
      {
        return;
      }
      skyTime = m_SkyTime;
      skyProfile = m_SkyProfile;
    }

    private void WarnInvalidSkySetup()
    {
      Debug.LogError("Your SkySystemController has an old or invalid prefab layout! Please run the upgrade tool in 'Windows -> Sky Studio -> Upgrade Sky System Controller'. Do not rename or modify any of the children in the SkySystemController hierarchy.");
    }

    void Start() {
      UpdateSkyForCurrentTime();
    }

    void Update()
    {      
      if (!skyProfile)
      {
        return;
      }

      if (automaticTimeIncrement && Application.isPlaying)
      {
        skyTime += automaticIncrementSpeed * Time.deltaTime;
      }

      // Catch older sky configurations or invalid setups early, and log an error message on how to fix it.
      if (sunOrbit == null || moonOrbit == null ||
          sunOrbit.rotateBody == null || moonOrbit.rotateBody == null ||
          sunOrbit.positionTransform == null || moonOrbit.positionTransform == null)
      {
        WarnInvalidSkySetup();
        return;
      }

      // We need to force a time update once, to intialize the state of everthing.
      if (m_DidInitialUpdate == false) {
        UpdateSkyForCurrentTime();
        m_DidInitialUpdate = true;
      }

      if (weatherController != null)
      {
        weatherController.UpdateForTimeOfDay(skyProfile, timeOfDay);
      }

      // Update Sun properties that need frame updates.
      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.SunFeature))
      {
        if (sunOrbit.positionTransform)
        {
          m_SkyMaterialController.SunWorldToLocalMatrix = sunOrbit.positionTransform.worldToLocalMatrix;
        }

        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.SunCustomTextureFeature))
        {
          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.SunRotationFeature))
          {
            sunOrbit.rotateBody.AllowSpinning = true;
            sunOrbit.rotateBody.SpinSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunRotationSpeedKey, timeOfDay);
          }
          else
          {
            sunOrbit.rotateBody.AllowSpinning = false;
          }
        }
      }

      // Update Moon properties that need frame updates.
      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.MoonFeature))
      {
        if (moonOrbit.positionTransform)
        {
          m_SkyMaterialController.MoonWorldToLocalMatrix = moonOrbit.positionTransform.worldToLocalMatrix;
        }

        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.MoonCustomTextureFeature))
        {
          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.MoonRotationFeature))
          {
            moonOrbit.rotateBody.AllowSpinning = true;
            moonOrbit.rotateBody.SpinSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonRotationSpeedKey, timeOfDay);
          }
          else
          {
            moonOrbit.rotateBody.AllowSpinning = false;
          }
        }
      }
    }

    public void UpdateGlobalIllumination()
    {
      DynamicGI.UpdateEnvironment();
    }

    private void SynchronizeAllShaderKeywords()
    {
      if (m_SkyProfile == null)
      {
        return;
      }

      foreach (ProfileFeatureSection section in m_SkyProfile.profileDefinition.features)
      {
        foreach (ProfileFeatureDefinition feature in section.featureDefinitions)
        {
          if (feature.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeyword)
          {
            SynchronizedShaderKeyword(feature.featureKey, feature.shaderKeyword);
          }
          else if (feature.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeywordDropdown)
          {
            for (int i = 0; i < feature.featureKeys.Length; i++)
            {
              SynchronizedShaderKeyword(feature.featureKeys[i], feature.shaderKeywords[i]);
            }
          }
        }
      }
    }

    private void SynchronizedShaderKeyword(string featureKey, string shaderKeyword)
    {
      if (skyProfile == null || skyProfile.skyboxMaterial == null)
      {
        return;
      }

      if (skyProfile.IsFeatureEnabled(featureKey))
      {
        if (!skyProfile.skyboxMaterial.IsKeywordEnabled(shaderKeyword))
        {
          skyProfile.skyboxMaterial.EnableKeyword(shaderKeyword);
        }
      }
      else
      {
        if (skyProfile.skyboxMaterial.IsKeywordEnabled(shaderKeyword))
        {
          skyProfile.skyboxMaterial.DisableKeyword(shaderKeyword);
        }
      }
    }

    private Vector3 GetPrimaryLightDirection()
    {
      Vector3 dir;
      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.SunFeature) && sunOrbit)
      {
        dir = sunOrbit.BodyGlobalDirection;
      }
      else if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.MoonFeature) && moonOrbit)
      {
        dir = moonOrbit.BodyGlobalDirection;
      }
      else
      {
        dir = new Vector3(0, 1, 0);
      }

      return dir;
    }

    public void UpdateSkyForCurrentTime()
    {
      if (ScenesManagement.instance) {
        if(ScenesManagement.instance.isLoading)
          return;
      }
      
      if (skyProfile == null)
      {
        return;
      }

      if (skyProfile.skyboxMaterial == null)
      {
        Debug.LogError("Your sky profile is missing a reference to the skybox material.");
        return;
      }

      if (m_SkyMaterialController == null)
      {
        m_SkyMaterialController = new SkyMaterialController();
      }

      m_SkyMaterialController.SkyboxMaterial = skyProfile.skyboxMaterial;

      if (RenderSettings.skybox == null ||
          RenderSettings.skybox.GetInstanceID() != skyProfile.skyboxMaterial.GetInstanceID())
      {
        RenderSettings.skybox = skyProfile.skyboxMaterial;
      }
      

      SynchronizeAllShaderKeywords();

      // Sky.
      m_SkyMaterialController.BackgroundCubemap = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.SkyCubemapKey, timeOfDay) as Cubemap;
      m_SkyMaterialController.SkyColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.SkyUpperColorKey, timeOfDay);
      m_SkyMaterialController.SkyMiddleColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.SkyMiddleColorKey, timeOfDay);
      m_SkyMaterialController.HorizonColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.SkyLowerColorKey, timeOfDay);
      m_SkyMaterialController.GradientFadeBegin = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.HorizonTrasitionStartKey, timeOfDay);
      m_SkyMaterialController.GradientFadeLength = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.HorizonTransitionLengthKey, timeOfDay);
      m_SkyMaterialController.SkyMiddlePosition = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SkyMiddleColorPositionKey, timeOfDay);
      m_SkyMaterialController.StarFadeBegin = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.StarTransitionStartKey, timeOfDay);
      m_SkyMaterialController.StarFadeLength = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.StarTransitionLengthKey, timeOfDay);
      m_SkyMaterialController.HorizonDistanceScale = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.HorizonStarScaleKey, timeOfDay);

      // Clouds.
      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.CloudFeature))
      {
        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.NoiseCloudFeature))
        {
          m_SkyMaterialController.CloudTexture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.CloudNoiseTextureKey, timeOfDay);
          m_SkyMaterialController.CloudTextureTiling = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudTextureTiling, timeOfDay);
          m_SkyMaterialController.CloudDensity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudDensityKey, timeOfDay);
          m_SkyMaterialController.CloudSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudSpeedKey, timeOfDay);
          m_SkyMaterialController.CloudDirection = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudDirectionKey, timeOfDay);
          m_SkyMaterialController.CloudHeight = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudHeightKey, timeOfDay);
          m_SkyMaterialController.CloudColor1 = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.CloudColor1Key, timeOfDay);
          m_SkyMaterialController.CloudColor2 = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.CloudColor2Key, timeOfDay);
          m_SkyMaterialController.CloudFadePosition = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudFadePositionKey, timeOfDay);
          m_SkyMaterialController.CloudFadeAmount = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudFadeAmountKey, timeOfDay);
        }
        else if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.CubemapCloudFeature))
        {
          m_SkyMaterialController.CloudCubemap = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.CloudCubemapTextureKey, timeOfDay);
          m_SkyMaterialController.CloudCubemapRotationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapRotationSpeedKey, timeOfDay);
          m_SkyMaterialController.CloudCubemapTintColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.CloudCubemapTintColorKey, timeOfDay);
          m_SkyMaterialController.CloudCubemapHeight = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapHeightKey, timeOfDay);

          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.CubemapCloudDoubleLayerFeature))
          {
            m_SkyMaterialController.CloudCubemapDoubleLayerHeight = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapDoubleLayerHeightKey, timeOfDay);
            m_SkyMaterialController.CloudCubemapDoubleLayerRotationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapDoubleLayerRotationSpeedKey, timeOfDay);
            m_SkyMaterialController.CloudCubemapDoubleLayerTintColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.CloudCubemapDoubleLayerTintColorKey, timeOfDay);

            if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.CubemapCloudDoubleLayerCubemapFeature))
            {
              m_SkyMaterialController.CloudCubemapDoubleLayerCustomTexture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.CloudCubemapDoubleLayerCustomTextureKey, timeOfDay);
            }
          }
        }
        else if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.CubemapNormalCloudFeature))
        {
          m_SkyMaterialController.CloudCubemapNormalLightDirection = GetPrimaryLightDirection();
          m_SkyMaterialController.CloudCubemapNormalTexture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.CloudCubemapNormalTextureKey, timeOfDay);
          m_SkyMaterialController.CloudCubemapNormalLitColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.CloudCubemapNormalLitColorKey, timeOfDay);
          m_SkyMaterialController.CloudCubemapNormalShadowColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.CloudCubemapNormalShadowKey, timeOfDay);
          m_SkyMaterialController.CloudCubemapNormalAmbientIntensity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapNormalAmbientIntensity, timeOfDay);
          m_SkyMaterialController.CloudCubemapNormalHeight = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapNormalHeightKey, timeOfDay);
          m_SkyMaterialController.CloudCubemapNormalRotationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapNormalRotationSpeedKey, timeOfDay);

          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.CubemapNormalCloudDoubleLayerFeature))
          {
            m_SkyMaterialController.CloudCubemapNormalDoubleLayerHeight = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapNormalDoubleLayerHeightKey, timeOfDay);
            m_SkyMaterialController.CloudCubemapNormalDoubleLayerRotationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.CloudCubemapNormalDoubleLayerRotationSpeedKey, timeOfDay);
            m_SkyMaterialController.CloudCubemapNormalDoubleLayerLitColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.CloudCubemapNormalDoubleLayerLitColorKey, timeOfDay);
            m_SkyMaterialController.CloudCubemapNormalDoubleLayerShadowColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.CloudCubemapNormalDoubleLayerShadowKey, timeOfDay);

            if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.CubemapNormalCloudDoubleLayerCubemapFeature))
            {
              m_SkyMaterialController.CloudCubemapNormalDoubleLayerCustomTexture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.CloudCubemapNormalDoubleLayerCustomTextureKey, timeOfDay);
            }
          }
        }
      }

      // Fog.
      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.FogFeature))
      {
        Color fogColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.FogColorKey, timeOfDay);
        m_SkyMaterialController.FogColor = fogColor;
        m_SkyMaterialController.FogDensity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.FogDensityKey, timeOfDay);
        m_SkyMaterialController.FogHeight = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.FogLengthKey, timeOfDay);

        // Synchronize with Unity's global fog color so the rest of the scene uses this color fog.
        if (skyProfile.GetBoolPropertyValue(ProfilePropertyKeys.FogSyncWithGlobal, timeOfDay))
        {
          RenderSettings.fogColor = fogColor;
        }
      }

      // Sun.
      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.SunFeature) && sunOrbit)
      {
        sunOrbit.Point = skyProfile.GetSpherePointPropertyValue(ProfilePropertyKeys.SunPositionKey, timeOfDay);

        m_SkyMaterialController.SunDirection = sunOrbit.BodyGlobalDirection;
        m_SkyMaterialController.SunColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.SunColorKey, timeOfDay);
        m_SkyMaterialController.SunSize = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunSizeKey, timeOfDay);
        m_SkyMaterialController.SunEdgeFeathering = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunEdgeFeatheringKey, timeOfDay);
        m_SkyMaterialController.SunBloomFilterBoost = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunColorIntensityKey, timeOfDay);

        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.SunCustomTextureFeature))
        {
          m_SkyMaterialController.SunWorldToLocalMatrix = sunOrbit.positionTransform.worldToLocalMatrix;
          m_SkyMaterialController.SunTexture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.SunTextureKey, timeOfDay);

          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.SunRotationFeature))
          {
            sunOrbit.rotateBody.SpinSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunRotationSpeedKey, timeOfDay);
          }
        }

        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.SunSpriteSheetFeature))
        {
          m_SkyMaterialController.SetSunSpriteDimensions(
            (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunSpriteColumnCountKey, timeOfDay),
            (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunSpriteRowCountKey, timeOfDay));
          m_SkyMaterialController.SunSpriteItemCount = (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunSpriteItemCountKey, timeOfDay);
          m_SkyMaterialController.SunSpriteAnimationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunSpriteAnimationSpeedKey, timeOfDay);
        }

        if (sunOrbit.BodyLight)
        {
          if (!sunOrbit.BodyLight.enabled)
          {
            sunOrbit.BodyLight.enabled = true;
          }
          RenderSettings.sun = sunOrbit.BodyLight;
          sunOrbit.BodyLight.color = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.SunLightColorKey, timeOfDay);
          sunOrbit.BodyLight.intensity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.SunLightIntensityKey, timeOfDay);
        }
      }
      else if (sunOrbit && sunOrbit.BodyLight)
      {
        sunOrbit.BodyLight.enabled = false;
      }

      // Moon.
      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.MoonFeature) && moonOrbit)
      {
        moonOrbit.Point = skyProfile.GetSpherePointPropertyValue(ProfilePropertyKeys.MoonPositionKey, timeOfDay);

        m_SkyMaterialController.MoonDirection = moonOrbit.BodyGlobalDirection;
        m_SkyMaterialController.MoonColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.MoonColorKey, timeOfDay);

        m_SkyMaterialController.MoonSize = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonSizeKey, timeOfDay);
        m_SkyMaterialController.MoonEdgeFeathering = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonEdgeFeatheringKey, timeOfDay);
        m_SkyMaterialController.MoonBloomFilterBoost = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonColorIntensityKey, timeOfDay);

        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.MoonCustomTextureFeature))
        {
          m_SkyMaterialController.MoonTexture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.MoonTextureKey, timeOfDay);
          m_SkyMaterialController.MoonWorldToLocalMatrix = moonOrbit.positionTransform.worldToLocalMatrix;

          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.MoonRotationFeature))
          {
            moonOrbit.rotateBody.SpinSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonRotationSpeedKey, timeOfDay);
          }
        }

        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.MoonSpriteSheetFeature))
        {
          m_SkyMaterialController.SetMoonSpriteDimensions(
            (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonSpriteColumnCountKey, timeOfDay),
            (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonSpriteRowCountKey, timeOfDay));
          m_SkyMaterialController.MoonSpriteItemCount = (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonSpriteItemCountKey, timeOfDay);
          m_SkyMaterialController.MoonSpriteAnimationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonSpriteAnimationSpeedKey, timeOfDay);
        }

        if (moonOrbit.BodyLight)
        {
          if (!moonOrbit.BodyLight.enabled)
          {
            moonOrbit.BodyLight.enabled = true;
          }
          moonOrbit.BodyLight.color = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.MoonLightColorKey, timeOfDay);
          moonOrbit.BodyLight.intensity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.MoonLightIntensityKey, timeOfDay);
        }
      }
      else if (moonOrbit && moonOrbit.BodyLight)
      {
        moonOrbit.BodyLight.enabled = false;
      }

      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.StarBasicFeature))
      {
        m_SkyMaterialController.StarBasicCubemap = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.StarBasicCubemapKey, timeOfDay);
        m_SkyMaterialController.StarBasicTwinkleSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.StarBasicTwinkleSpeedKey, timeOfDay);
        m_SkyMaterialController.StarBasicTwinkleAmount = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.StarBasicTwinkleAmountKey, timeOfDay);
        m_SkyMaterialController.StarBasicOpacity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.StarBasicOpacityKey, timeOfDay);
        m_SkyMaterialController.StarBasicTintColor = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.StarBasicTintColorKey, timeOfDay);
        m_SkyMaterialController.StarBasicExponent = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.StarBasicExponentKey, timeOfDay);
        m_SkyMaterialController.StarBasicIntensity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.StarBasicIntensityKey, timeOfDay);
      }
      else
      {
        // Star Layer 1.
        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer1Feature))
        {
          m_SkyMaterialController.StarLayer1DataTexture = skyProfile.starLayer1DataTexture;
          m_SkyMaterialController.StarLayer1Color = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.Star1ColorKey, timeOfDay);
          m_SkyMaterialController.StarLayer1MaxRadius = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1SizeKey, timeOfDay);
          m_SkyMaterialController.StarLayer1Texture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.Star1TextureKey, timeOfDay);
          m_SkyMaterialController.StarLayer1TwinkleAmount = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1TwinkleAmountKey, timeOfDay);
          m_SkyMaterialController.StarLayer1TwinkleSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1TwinkleSpeedKey, timeOfDay);
          m_SkyMaterialController.StarLayer1RotationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1RotationSpeedKey, timeOfDay);
          m_SkyMaterialController.StarLayer1EdgeFeathering = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1EdgeFeatheringKey, timeOfDay);
          m_SkyMaterialController.StarLayer1BloomFilterBoost = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1ColorIntensityKey, timeOfDay);

          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer1SpriteSheetFeature))
          {
            m_SkyMaterialController.StarLayer1SpriteItemCount = (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1SpriteItemCountKey, timeOfDay);
            m_SkyMaterialController.StarLayer1SpriteAnimationSpeed = (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1SpriteAnimationSpeedKey, timeOfDay);
            m_SkyMaterialController.SetStarLayer1SpriteDimensions(
              (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1SpriteColumnCountKey, timeOfDay),
              (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star1SpriteRowCountKey, timeOfDay));
          }
        }

        // Star Layer 2.
        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer2Feature))
        {
          m_SkyMaterialController.StarLayer2DataTexture = skyProfile.starLayer2DataTexture;
          m_SkyMaterialController.StarLayer2Color = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.Star2ColorKey, timeOfDay);
          m_SkyMaterialController.StarLayer2MaxRadius = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2SizeKey, timeOfDay); ;
          m_SkyMaterialController.StarLayer2Texture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.Star2TextureKey, timeOfDay);
          m_SkyMaterialController.StarLayer2TwinkleAmount = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2TwinkleAmountKey, timeOfDay);
          m_SkyMaterialController.StarLayer2TwinkleSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2TwinkleSpeedKey, timeOfDay);
          m_SkyMaterialController.StarLayer2RotationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2RotationSpeedKey, timeOfDay);
          m_SkyMaterialController.StarLayer2EdgeFeathering = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2EdgeFeatheringKey, timeOfDay);
          m_SkyMaterialController.StarLayer2BloomFilterBoost = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2ColorIntensityKey, timeOfDay);

          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer2SpriteSheetFeature))
          {
            m_SkyMaterialController.StarLayer2SpriteItemCount = (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2SpriteItemCountKey, timeOfDay);
            m_SkyMaterialController.StarLayer2SpriteAnimationSpeed = (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2SpriteAnimationSpeedKey, timeOfDay);
            m_SkyMaterialController.SetStarLayer2SpriteDimensions(
              (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2SpriteColumnCountKey, timeOfDay),
              (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star2SpriteRowCountKey, timeOfDay));
          }
        }

        // Star Layer 3.
        if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer3Feature))
        {
          m_SkyMaterialController.StarLayer3DataTexture = skyProfile.starLayer3DataTexture;
          m_SkyMaterialController.StarLayer3Color = skyProfile.GetColorPropertyValue(ProfilePropertyKeys.Star3ColorKey, timeOfDay);
          m_SkyMaterialController.StarLayer3MaxRadius = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3SizeKey, timeOfDay);
          m_SkyMaterialController.StarLayer3Texture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.Star3TextureKey, timeOfDay);
          m_SkyMaterialController.StarLayer3TwinkleAmount = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3TwinkleAmountKey, timeOfDay);
          m_SkyMaterialController.StarLayer3TwinkleSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3TwinkleSpeedKey, timeOfDay);
          m_SkyMaterialController.StarLayer3RotationSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3RotationSpeedKey, timeOfDay);
          m_SkyMaterialController.StarLayer3EdgeFeathering = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3EdgeFeatheringKey, timeOfDay);
          m_SkyMaterialController.StarLayer3BloomFilterBoost = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3ColorIntensityKey, timeOfDay);

          if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer3SpriteSheetFeature))
          {
            m_SkyMaterialController.StarLayer3SpriteItemCount = (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3SpriteItemCountKey, timeOfDay);
            m_SkyMaterialController.StarLayer3SpriteAnimationSpeed = (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3SpriteAnimationSpeedKey, timeOfDay);
            m_SkyMaterialController.SetStarLayer3SpriteDimensions(
              (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3SpriteColumnCountKey, timeOfDay),
              (int)skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.Star3SpriteRowCountKey, timeOfDay));
          }
        }
      }

      if (updateGlobalIllumination)
      {
        UpdateGlobalIllumination();
      }

      // Notify delegate after we've completed the sky modifications.
      if (timeChangedCallback != null)
      {
        timeChangedCallback(this, timeOfDay);
      }

      MatchFogColor();
    }
    public enum MatchFogTo {Nothing, Horizon, Middle, Upper}
    public MatchFogTo matchFogTo;
    void MatchFogColor() {
      switch (matchFogTo) {
        case MatchFogTo.Nothing: break;
        case MatchFogTo.Horizon: RenderSettings.fogColor = m_SkyMaterialController.HorizonColor; break;
        case MatchFogTo.Middle: RenderSettings.fogColor = m_SkyMaterialController.SkyMiddleColor; break;
        case MatchFogTo.Upper: RenderSettings.fogColor = m_SkyMaterialController.SkyColor; break;
      }
    }

    public string TimeStringFromPercent(float percent)
    {
      float hoursFract = percent * 24.0f;
      int hours = (int) hoursFract;
      int minutes = (int)((hoursFract - hours) * 60.0f);

      string hourStr = hours < 10 ? "0" + hours : hours.ToString();
      string minuteStr = minutes < 10 ? "0" + minutes : minutes.ToString();

      return hourStr + ":" + minuteStr;
    }
  }
}
