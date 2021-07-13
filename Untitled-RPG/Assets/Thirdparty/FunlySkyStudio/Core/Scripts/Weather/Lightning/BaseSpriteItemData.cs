using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Data for representing an instanced sprite sheet object.
  public class BaseSpriteItemData : System.Object
  {
    // Tracking state for the splash effect.
    public enum SpriteState
    {
      Unknown,
      NotStarted,
      Animating,
      Complete
    }

    public SpriteSheetData spriteSheetData;

    public Matrix4x4 modelMatrix { get; protected set; }
    public SpriteState state { get; protected set; }
    public Vector3 spritePosition { get; set; }

    public float startTime { get; protected set; }
    public float endTime { get; protected set; }
    public float delay;

    public BaseSpriteItemData()
    {
      state = SpriteState.NotStarted;
    }

    public void SetTRSMatrix(Vector3 worldPosition, Quaternion rotation, Vector3 scale)
    {
      spritePosition = worldPosition;
      modelMatrix = Matrix4x4.TRS(
        worldPosition,
        rotation,
        scale);
    }

    public void Start()
    {
      state = SpriteState.Animating;

      // Schedule the start/end time of this sprite sheet animation in the GPU.
      startTime = BaseSpriteItemData.CalculateStartTimeWithDelay(delay);

      endTime = BaseSpriteItemData.CalculateEndTime(startTime,
        spriteSheetData.frameCount,
        spriteSheetData.frameRate);
    }

    public void Continue()
    {
      if (state != SpriteState.Animating) {
        return;
      }
      
      if (Time.time > endTime) {
        state = SpriteState.Complete;
        return;
      }
    }

    public void Reset()
    {
      state = SpriteState.NotStarted;
      startTime = -1.0f;
      endTime = -1.0f;
    }

    public static float CalculateStartTimeWithDelay(float delay)
    {
      return Time.time + delay;
    }

    public static float CalculateEndTime(float startTime, int itemCount, int animationSpeed)
    {
      float singleFrameDuration = 1.0f / (float)animationSpeed;
      float duration = (float)itemCount * singleFrameDuration;
      return startTime + duration;
    }

    /*
    int GetSpriteTargetIndex(int itemCount, int animationSpeed, float seed)
    {
      float startTime = m_StartTime + startDelay;
      float delta = Time.time - startTime;
      float timePerFrame = 1.0f / animationSpeed;

      int frameIndex = (int)(delta / timePerFrame);

      return frameIndex;
    }
    */
  }
}

