using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class NumberKeyframe : BaseKeyframe
  {
    public float value;

    public NumberKeyframe(float time, float value) : base(time)
    {
      this.value = value;
    }

    public NumberKeyframe(NumberKeyframe keyframe) : base(keyframe.time)
    {
      this.value = keyframe.value;
      interpolationCurve = keyframe.interpolationCurve;
      interpolationDirection = keyframe.interpolationDirection;
    }
  }
}

