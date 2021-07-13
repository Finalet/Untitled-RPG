using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class ColorKeyframeGroup : KeyframeGroup<ColorKeyframe>
  {
    public ColorKeyframeGroup(string name) : base(name) {}

    public ColorKeyframeGroup(string name, ColorKeyframe frame) : base(name)
    {
      AddKeyFrame(frame);
    }

    public Color ColorForTime(float time)
    {
      time = time - (int)time;

      if (keyframes.Count == 0)
      {
        Debug.LogError("Can't return color since there aren't any keyframes.");
        return Color.white;
      }

      if (keyframes.Count == 1)
      {
        return GetKeyframe(0).color;
      }

      int beforeKeyIndex, afterKeyIndex;

      GetSurroundingKeyFrames(time, out beforeKeyIndex, out afterKeyIndex);

      ColorKeyframe beforeKey = GetKeyframe(beforeKeyIndex);
      ColorKeyframe afterKey = GetKeyframe(afterKeyIndex);

      float blendPercent = ProgressBetweenSurroundingKeyframes(time, beforeKey, afterKey);
      float curvedBlendPercent = CurveAdjustedBlendingTime(beforeKey.interpolationCurve, blendPercent);
      return Color.Lerp(beforeKey.color, afterKey.color, curvedBlendPercent);
    }
  }
}
