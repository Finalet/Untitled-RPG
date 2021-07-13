using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class TextureKeyframeGroup : KeyframeGroup<TextureKeyframe>
  {
    public TextureKeyframeGroup(string name, TextureKeyframe keyframe) : base(name)
    {
      AddKeyFrame(keyframe);
    }

    public Texture TextureForTime(float time)
    {
      if (keyframes.Count == 0)
      {
        Debug.LogError("Can't return texture without any keyframes");
        return null;
      }

      if (keyframes.Count == 1)
      {
        return GetKeyframe(0).texture;
      }

      int beforeIndex, afterIndex;
      GetSurroundingKeyFrames(time, out beforeIndex, out afterIndex);

      TextureKeyframe before = GetKeyframe(beforeIndex);

      return before.texture;
    }
  }
}

