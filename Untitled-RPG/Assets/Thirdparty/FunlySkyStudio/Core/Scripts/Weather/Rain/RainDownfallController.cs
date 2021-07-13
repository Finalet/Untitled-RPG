using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Controls the rain on the foreground mesh.
  [RequireComponent(typeof(AudioSource))]
  public class RainDownfallController : MonoBehaviour, ISkyModule
  {
    public MeshRenderer rainMeshRenderer;
    public Material rainMaterial;

    MaterialPropertyBlock m_PropertyBlock;
    AudioSource m_RainAudioSource;
    float m_TimeOfDay;
    SkyProfile m_SkyProfile;
    //WeatherEnclosure m_Enclosure;
    
    public void SetWeatherEnclosure(WeatherEnclosure enclosure)
    {
      //m_Enclosure = enclosure;

      if (rainMeshRenderer != null) {
        rainMeshRenderer.enabled = false;
        rainMeshRenderer = null;
      }

      if (!enclosure) {
        return;
      }

      rainMeshRenderer = enclosure.GetComponentInChildren<MeshRenderer>();
      if (!rainMeshRenderer) {
        Debug.LogError("Can't render rain since there's no MeshRenderer on the WeatherEnclosure");
        return;
      }

      m_PropertyBlock = new MaterialPropertyBlock();

      if (!rainMaterial) {
        return;
      }

      rainMeshRenderer.material = rainMaterial;
      rainMeshRenderer.enabled = true;

      UpdateForTimeOfDay(m_SkyProfile, m_TimeOfDay);
    }

    private void Update()
    {
      if (m_SkyProfile == null) {
        return;
      }

      UpdateForTimeOfDay(m_SkyProfile, m_TimeOfDay);
    }

    public void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay)
    {
      m_SkyProfile = skyProfile;
      m_TimeOfDay = timeOfDay;

      if (!skyProfile) {
        return;
      }

      // Update the volume.
      if (m_RainAudioSource == null) {
        m_RainAudioSource = GetComponent<AudioSource>();
      }

      // Suppress rain sounds if rain isn't enabled.
      if (skyProfile == null || m_SkyProfile.IsFeatureEnabled(ProfileFeatureKeys.RainFeature) == false) {
        if (m_RainAudioSource != null) {
          m_RainAudioSource.enabled = false;
        }
        return;
      }

      if (!rainMaterial) {
        Debug.LogError("Can't render rain without a rain material");
        return;
      }

      if (!rainMeshRenderer) {
        Debug.LogError("Can't show rain without an enclosure mesh renderer.");
        return;
      }

      if (m_PropertyBlock == null) {
        m_PropertyBlock = new MaterialPropertyBlock();
      }

      rainMeshRenderer.enabled = true;
      rainMeshRenderer.material = rainMaterial;
      rainMeshRenderer.GetPropertyBlock(m_PropertyBlock);

      float rainNearIntensity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainNearIntensityKey, timeOfDay);
      float rainFarIntensity = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainFarIntensityKey, timeOfDay);
      Texture rainNearTexture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.RainNearTextureKey, timeOfDay);
      Texture rainFarTexture = skyProfile.GetTexturePropertyValue(ProfilePropertyKeys.RainFarTextureKey, timeOfDay);
      float rainNearSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainNearSpeedKey, timeOfDay);
      float rainFarSpeed = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainFarSpeedKey, timeOfDay);
      Color tintColor = m_SkyProfile.GetColorPropertyValue(ProfilePropertyKeys.RainTintColorKey, m_TimeOfDay);
      float turbulence = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainWindTurbulence, m_TimeOfDay);
      float turbulenceSpeed = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainWindTurbulenceSpeed, m_TimeOfDay);
      float nearTiling = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainNearTextureTiling, m_TimeOfDay);
      float farTiling = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainFarTextureTiling, m_TimeOfDay);

      if (rainNearTexture != null) {
        m_PropertyBlock.SetTexture("_NearTex", rainNearTexture);
        m_PropertyBlock.SetVector("_NearTex_ST", new Vector4(nearTiling, nearTiling, nearTiling, 1));
      }

      m_PropertyBlock.SetFloat("_NearDensity", rainNearIntensity);
      m_PropertyBlock.SetFloat("_NearRainSpeed", rainNearSpeed);

      if (rainFarTexture != null) {
        m_PropertyBlock.SetTexture("_FarTex", rainFarTexture);
        m_PropertyBlock.SetVector("_FarTex_ST", new Vector4(farTiling, farTiling, farTiling, 1));
      }

      m_PropertyBlock.SetFloat("_FarDensity", rainFarIntensity);
      m_PropertyBlock.SetFloat("_FarRainSpeed", rainFarSpeed);
      m_PropertyBlock.SetColor("_TintColor", tintColor);
      m_PropertyBlock.SetFloat("_Turbulence", turbulence);
      m_PropertyBlock.SetFloat("_TurbulenceSpeed", turbulenceSpeed);

      rainMeshRenderer.SetPropertyBlock(m_PropertyBlock);

      if (skyProfile.IsFeatureEnabled(ProfileFeatureKeys.RainSoundFeature)) {
        m_RainAudioSource.enabled = true;
        m_RainAudioSource.volume = skyProfile.GetNumberPropertyValue(ProfilePropertyKeys.RainSoundVolumeKey, timeOfDay);
      } else {
        m_RainAudioSource.enabled = false;
      }
    }

  }
}

