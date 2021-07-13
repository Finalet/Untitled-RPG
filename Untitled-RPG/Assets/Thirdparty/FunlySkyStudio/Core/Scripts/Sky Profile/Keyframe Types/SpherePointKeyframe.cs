using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class SpherePointKeyframe : BaseKeyframe
  {
    public SpherePoint spherePoint;

    public SpherePointKeyframe(SpherePoint spherePoint, float time) : base(time)
    {
      if (spherePoint == null) {
        Debug.LogError("Passed null sphere point, created empty point");
        this.spherePoint = new SpherePoint(0, 0);
      } else {
        this.spherePoint = spherePoint;
      }

      interpolationDirection = InterpolationDirection.Auto;
    }

    public SpherePointKeyframe(SpherePointKeyframe keyframe) : base(keyframe.time)
    {
      spherePoint = new SpherePoint(
        keyframe.spherePoint.horizontalRotation,
        keyframe.spherePoint.verticalRotation);
      interpolationCurve = keyframe.interpolationCurve;
      interpolationDirection = keyframe.interpolationDirection;
    }
  }
}

