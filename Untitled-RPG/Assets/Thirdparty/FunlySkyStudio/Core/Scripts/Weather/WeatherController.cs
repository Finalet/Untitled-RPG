using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Manages the interfacing the various weather controller and updating state.
  public class WeatherController : MonoBehaviour
  {
    public RainDownfallController rainDownfallController { get; protected set; }
    public RainSplashController rainSplashController { get; protected set; }
    public LightningController lightningController { get; protected set; }
    public WeatherDepthCamera weatherDepthCamera { get; protected set; }

    WeatherEnclosure m_Enclosure;
    MeshRenderer m_EnclosureMeshRenderer;
    WeatherEnclosureDetector detector;
    SkyProfile m_Profile;
    float m_TimeOfDay;

    private void Awake()
    {
      DiscoverWeatherControllers();
    }

    private void Start()
    {
      DiscoverWeatherControllers();
    }

    private void OnEnable()
    {
      DiscoverWeatherControllers();

      if (detector == null) {
        Debug.LogError("Can't register for enclosure callbacks since there's no WeatherEnclosureDetector on any children");
        return;
      }

      detector.enclosureChangedCallback += OnEnclosureDidChange;
    }

    void DiscoverWeatherControllers()
    {
      rainDownfallController = GetComponentInChildren<RainDownfallController>();
      rainSplashController = GetComponentInChildren<RainSplashController>();
      lightningController = GetComponentInChildren<LightningController>();
      weatherDepthCamera = GetComponentInChildren<WeatherDepthCamera>();

      detector = GetComponentInChildren<WeatherEnclosureDetector>();
    }

    private void OnDisable()
    {
      if (detector == null) {
        return;
      }

      detector.enclosureChangedCallback -= OnEnclosureDidChange;
    }

    // Update all of the weather systems.
    public void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay)
    {
      if (!skyProfile) {
        return;
      }

      m_Profile = skyProfile;
      m_TimeOfDay = timeOfDay;

      // Update all the controllers state.
      if (weatherDepthCamera != null) {
        weatherDepthCamera.enabled = skyProfile.IsFeatureEnabled(ProfileFeatureKeys.RainSplashFeature);
      }

      if (rainDownfallController != null) {
        rainDownfallController.UpdateForTimeOfDay(skyProfile, timeOfDay);
      }

      if (rainSplashController != null) {
        rainSplashController.UpdateForTimeOfDay(skyProfile, timeOfDay);
      }

      if (lightningController != null) {
        lightningController.UpdateForTimeOfDay(skyProfile, timeOfDay);
      }
    }

    private void LateUpdate()
    {
      if (m_Profile == null) {
        return;
      }

      if (m_EnclosureMeshRenderer && rainDownfallController && m_Profile.IsFeatureEnabled(ProfileFeatureKeys.RainFeature)) {
        m_EnclosureMeshRenderer.enabled = true;
      } else {
        m_EnclosureMeshRenderer.enabled = false;
      }
    }

    private void OnEnclosureDidChange(WeatherEnclosure enclosure)
    {
      m_Enclosure = enclosure;
      if (m_Enclosure != null) {
        m_EnclosureMeshRenderer = m_Enclosure.GetComponentInChildren<MeshRenderer>();
      }

      rainDownfallController.SetWeatherEnclosure(m_Enclosure);

      UpdateForTimeOfDay(m_Profile, m_TimeOfDay);
    }
  }
}
