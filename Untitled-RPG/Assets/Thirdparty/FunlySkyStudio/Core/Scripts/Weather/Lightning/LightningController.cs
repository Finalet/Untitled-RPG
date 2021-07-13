using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public class LightningController : MonoBehaviour, ISkyModule
  {
    SkyProfile m_SkyProfile;
    float m_TimeOfDay;
    List<LightningRenderer> m_LightningRenderers = new List<LightningRenderer>();

    private void Start()
    {
      // Verify GPU instancing is supported.
      if (SystemInfo.supportsInstancing == false) {
        Debug.LogWarning("Can't render lightning since GPU instancing is not supported on this platform.");
        enabled = false;
        return;
      }

      ClearLightningRenderers();
    }

    public void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay)
    {
      m_SkyProfile = skyProfile;
      m_TimeOfDay = timeOfDay;
    }

    public void Update()
    {
      if (m_SkyProfile == null || m_SkyProfile.IsFeatureEnabled(ProfileFeatureKeys.LightningFeature) == false) {
        ClearLightningRenderers();
        return;
      }

      if (m_SkyProfile.lightningArtSet == null || m_SkyProfile.lightningArtSet.lightingStyleItems == null ||
        m_SkyProfile.lightningArtSet.lightingStyleItems.Count == 0) {
        return;
      }

      if (m_SkyProfile.lightningArtSet.lightingStyleItems.Count != m_LightningRenderers.Count) {
        ClearLightningRenderers();
        CreateLightningRenderers();
      }

      // Assign a style to each renderer.
      for (int i = 0; i < m_SkyProfile.lightningArtSet.lightingStyleItems.Count; i++) {
        LightningArtItem style = m_SkyProfile.lightningArtSet.lightingStyleItems[i];
        LightningRenderer lr = m_LightningRenderers[i];

        lr.UpdateForTimeOfDay(m_SkyProfile, m_TimeOfDay, style);
      }
    }

    public void ClearLightningRenderers()
    {
      for (int i = 0; i < this.transform.childCount; i++) {
        Destroy(this.transform.GetChild(i).gameObject);
      }

      m_LightningRenderers.Clear();
    }

    public void CreateLightningRenderers()
    {
      for (int i = 0; i < m_SkyProfile.lightningArtSet.lightingStyleItems.Count; i++) {
        GameObject go = new GameObject("Lightning Renderer");
        LightningRenderer lr = go.AddComponent<LightningRenderer>();
        lr.transform.parent = this.transform;

        m_LightningRenderers.Add(lr);
      }
    }
  }
}
