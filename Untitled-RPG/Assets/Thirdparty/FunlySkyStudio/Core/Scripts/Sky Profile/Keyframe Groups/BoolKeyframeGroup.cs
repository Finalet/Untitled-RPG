using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class BoolKeyframeGroup : KeyframeGroup<BoolKeyframe>
  {
    public BoolKeyframeGroup(string name) : base(name) { }

    public BoolKeyframeGroup(string name, BoolKeyframe keyframe) : base(name)
    {
      AddKeyFrame(keyframe);
    }

    public bool BoolForTime(float time)
    {
      if (keyframes.Count == 0) {
        Debug.LogError("Can't sample bool without any keyframes");
        return false;
      }

      if (keyframes.Count == 1) {
        return keyframes[0].value;
      }

      // Check if time comes before first keyframe, retun last keyframe then.
      if (time < keyframes[0].time) {
        return keyframes[keyframes.Count - 1].value;
      }

      int overlappingIndex = 0;
      for (int i = 1; i < keyframes.Count; i++) {
        if (keyframes[i].time <= time) {
          overlappingIndex = i;
        } else {
          break;
        }
      }

      return keyframes[overlappingIndex].value;
    }
  }
}

