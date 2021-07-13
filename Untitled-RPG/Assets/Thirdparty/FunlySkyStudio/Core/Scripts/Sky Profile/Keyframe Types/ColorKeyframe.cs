using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class ColorKeyframe : BaseKeyframe
  {
    public Color color = Color.white;

    public ColorKeyframe(Color c, float time) : base(time)
    {
      color = c;
    }

    public ColorKeyframe(ColorKeyframe keyframe) : base(keyframe.time)
    {
      this.color = keyframe.color;
      interpolationCurve = keyframe.interpolationCurve;
      interpolationDirection = keyframe.interpolationDirection;
    }
  }
}

