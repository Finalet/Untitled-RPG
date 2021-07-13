using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class TextureKeyframe : BaseKeyframe
  {
    public Texture texture;

    public TextureKeyframe(Texture texture, float time) : base(time)
    {
      this.texture = texture;
    }

    public TextureKeyframe(TextureKeyframe keyframe) : base(keyframe.time)
    {
      this.texture = keyframe.texture;
      interpolationCurve = keyframe.interpolationCurve;
      interpolationDirection = keyframe.interpolationDirection;
    }
  }
}

