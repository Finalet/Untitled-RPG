using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Funly.SkyStudio
{
  [RequireComponent(typeof(RawImage))]
  public class LoadOverheadDepthTexture : MonoBehaviour
  {
    WeatherDepthCamera m_RainCamera;
    RawImage m_Image;

    // Use this for initialization
    void Start()
    {
      m_RainCamera = FindObjectOfType<WeatherDepthCamera>();
      m_Image = GetComponent<RawImage>();
    }

    private void Update()
    {
      m_Image.texture = m_RainCamera.overheadDepthTexture;
    }
  }
}
