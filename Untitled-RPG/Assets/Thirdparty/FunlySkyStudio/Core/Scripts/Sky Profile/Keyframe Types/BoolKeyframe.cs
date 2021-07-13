using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class BoolKeyframe : BaseKeyframe
  {
    public bool value;

    public BoolKeyframe(float time, bool value) : base(time)
    {
      this.value = value;
    }

    public BoolKeyframe(BoolKeyframe keyframe) : base(keyframe.time)
    {
      this.value = keyframe.value;
      interpolationCurve = keyframe.interpolationCurve;
      interpolationDirection = keyframe.interpolationDirection;
    }
  }
}


