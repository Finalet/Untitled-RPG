using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class SpherePointKeyframeGroup : KeyframeGroup<SpherePointKeyframe>
  {
    public SpherePointKeyframeGroup(string name) : base(name) { }

    public const float MinHorizontalRotation = -Mathf.PI;
    public const float MaxHorizontalRotation = Mathf.PI;
    public const float MinVerticalRotation = -Mathf.PI / 2.0f;
    public const float MaxVerticalRotation = Mathf.PI / 2.0f;

    public SpherePointKeyframeGroup(string name, SpherePointKeyframe keyframe) : base(name)
    {
      AddKeyFrame(keyframe);
    }
    
    public SpherePoint SpherePointForTime(float time)
    {
      int beforeIndex;
      int afterIndex;

      // Shortcut and skip a calculations.
      if (keyframes.Count == 1)
      {
        return keyframes[0].spherePoint;
      }

      if (!GetSurroundingKeyFrames(time, out beforeIndex, out afterIndex)) {
        Debug.LogError("Failed to get surrounding sphere point for time: " + time);
        return null;
      }

      time = time - (int)time;

      SpherePointKeyframe beforeKeyframe = GetKeyframe(beforeIndex);
      SpherePointKeyframe afterKeyframe = GetKeyframe(afterIndex);

      float progressBetweenFrames = ProgressBetweenSurroundingKeyframes(time, beforeKeyframe.time, afterKeyframe.time);
      float curvedTime = CurveAdjustedBlendingTime(beforeKeyframe.interpolationCurve, progressBetweenFrames);

      Vector3 point = Vector3.Slerp(
        beforeKeyframe.spherePoint.GetWorldDirection(),
        afterKeyframe.spherePoint.GetWorldDirection(),
        curvedTime);

      return new SpherePoint(point.normalized);
    }
  }
}

