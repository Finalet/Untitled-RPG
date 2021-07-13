using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio {
  [CreateAssetMenu(fileName = "rainSplashArtItem.asset", menuName = "Sky Studio/Rain/Rain Splash Art Item")]
  public class RainSplashArtItem : SpriteArtItem
  {
    [Range(0.0f, 1.0f)]
    public float intensityMultiplier = 1.0f;

    [Range(0.0f, 1.0f)]
    public float scaleMultiplier = 1.0f;
  }
}

