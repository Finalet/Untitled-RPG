using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public class RainSplashController : MonoBehaviour, ISkyModule
  {
    SkyProfile m_SkyProfile;
    float m_TimeOfDay;
    List<RainSplashRenderer> m_SplashRenderers = new List<RainSplashRenderer>();

    private void Start()
    {
      // Verify GPU instancing is supported.
      if (SystemInfo.supportsInstancing == false) {
        Debug.LogWarning("Can't render rain splashes since GPU instancing is not supported on this platform.");
        enabled = false;
        return;
      }

      ClearSplashRenderers();
    }

    public void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay)
    {
      m_SkyProfile = skyProfile;
      m_TimeOfDay = timeOfDay;
    }

    void Update()
    {
      if (m_SkyProfile == null || m_SkyProfile.IsFeatureEnabled(ProfileFeatureKeys.RainSplashFeature) == false) {
        ClearSplashRenderers();
        return;
      }

      if (m_SkyProfile.rainSplashArtSet == null || m_SkyProfile.rainSplashArtSet.rainSplashArtItems == null ||
        m_SkyProfile.rainSplashArtSet.rainSplashArtItems.Count == 0) {
        ClearSplashRenderers();
        return;
      }

      if (m_SkyProfile.rainSplashArtSet.rainSplashArtItems.Count != m_SplashRenderers.Count) {
        ClearSplashRenderers();
        CreateSplashRenderers();
      }

      // Assign a style to each renderer.
      for (int i = 0; i < m_SkyProfile.rainSplashArtSet.rainSplashArtItems.Count; i++) {
        RainSplashArtItem style = m_SkyProfile.rainSplashArtSet.rainSplashArtItems[i];
        RainSplashRenderer r = m_SplashRenderers[i];

        r.UpdateForTimeOfDay(m_SkyProfile, m_TimeOfDay, style);
      }
    }

    public void ClearSplashRenderers()
    {
      for (int i = 0; i < this.transform.childCount; i++) {
        Destroy(this.transform.GetChild(i).gameObject);
      }
      m_SplashRenderers.Clear();
    }

    public void CreateSplashRenderers()
    {
      for (int i = 0; i < m_SkyProfile.rainSplashArtSet.rainSplashArtItems.Count; i++) {
        GameObject go = new GameObject("Rain Splash Renderer");
        RainSplashRenderer r = go.AddComponent<RainSplashRenderer>();
        r.transform.parent = this.transform;

        m_SplashRenderers.Add(r);
      }
    }

  }
}
