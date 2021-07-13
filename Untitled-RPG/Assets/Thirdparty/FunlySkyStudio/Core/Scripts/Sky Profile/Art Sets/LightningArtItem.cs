using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [CreateAssetMenu(fileName = "lightningArtItem.asset", menuName = "Sky Studio/Lightning/Lightning Art Item")]
  public class LightningArtItem : SpriteArtItem
  {
    // Alignment within the spawn area.
    public enum Alignment
    {
      ScaleToFit,
      TopAlign,
    }

    [Tooltip("Adjust how the lightning bolt is positioned inside the spawn area container.")]
    public Alignment alignment;

    [Tooltip("Thunder sound clip to play when this lighting bolt is rendered.")]
    public AudioClip thunderSound;

    [Tooltip("Probability adjustment for this specific lightning bolt. This value is multiplied against the global lightning probability.")]
    [Range(0.0f, 1.0f)]
    public float strikeProbability = 1.0f;

    [Range(0.0f, 60.0f)]
    [Tooltip("Size of the lighting bolt.")]
    public float size = 20.0f;

    [Range(0.0f, 1.0f)]
    [Tooltip("The blending weight of the additive lighting bolt effect")]
    public float intensity = 1.0f;
  }
}
