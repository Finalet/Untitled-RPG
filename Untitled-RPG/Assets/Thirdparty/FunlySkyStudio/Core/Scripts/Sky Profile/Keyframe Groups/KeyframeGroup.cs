using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class KeyframeGroup<T> : System.Object, IKeyframeGroup where T : IBaseKeyframe
  {
    public List<T> keyframes = new List<T>();

    [SerializeField]
    private string m_Name;
    public string name
    {
      get { return m_Name; }
      set { m_Name = value; }
    }

    [SerializeField]
    private string m_Id;
    public string id
    {
      get { return m_Id; }
      set { m_Id = value; }
    }

    public KeyframeGroup(string name)
    {
      this.name = name;
      id = Guid.NewGuid().ToString();
    }

    public void AddKeyFrame(T keyFrame)
    {
      keyframes.Add(keyFrame);
      SortKeyframes();
    }

    public void RemoveKeyFrame(T keyFrame)
    {
      if (keyframes.Count == 1)
      {
        Debug.LogError("You must have at least 1 keyframe in every group.");
        return;
      }

      keyframes.Remove(keyFrame);
      SortKeyframes();
    }

    public void RemoveKeyFrame(IBaseKeyframe keyframe)
    {
      RemoveKeyFrame((T)keyframe);
    }

    public int GetKeyFrameCount()
    {
      return keyframes.Count;
    }

    public T GetKeyframe(int index)
    {
      return keyframes[index];
    }

    public void SortKeyframes()
    {
      keyframes.Sort();
    }

    public float CurveAdjustedBlendingTime(InterpolationCurve curve, float t)
    {
      if (curve == InterpolationCurve.Linear)
      {
        return t;
      } else if (curve == InterpolationCurve.EaseInEaseOut)
      {
        float curveTime = t < .5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        return Mathf.Clamp01(curveTime);
      }

      return t;
    }

    // Get the keyframe that comes before this time.
    public T GetPreviousKeyFrame(float time)
    {
      T beforeKeyframe;
      T afterKeyframe;

      if (!GetSurroundingKeyFrames(time, out beforeKeyframe, out afterKeyframe))
      {
        return default(T);
      }

      return beforeKeyframe;
    }

    public bool GetSurroundingKeyFrames(float time, out T beforeKeyframe, out T afterKeyframe)
    {
      beforeKeyframe = default(T);
      afterKeyframe = default(T);

      int beforeIndex, afterIndex;

      if (GetSurroundingKeyFrames(time, out beforeIndex, out afterIndex))
      {
        beforeKeyframe = GetKeyframe(beforeIndex);
        afterKeyframe = GetKeyframe(afterIndex);
        return true;
      }
      return false;
    }

    public bool GetSurroundingKeyFrames(float time, out int beforeIndex, out int afterIndex)
    {
      beforeIndex = 0;
      afterIndex = 0;

      if (keyframes.Count == 0)
      {
        Debug.LogError("Can't return nearby keyframes since it's empty.");
        return false;
      }

      if (keyframes.Count == 1)
      {
        return true;
      }

      if (time < keyframes[0].time)
      {
        beforeIndex = keyframes.Count - 1;
        afterIndex = 0;
        return true;
      }

      int keyframeIndex = 0;

      for (int i = 0; i < keyframes.Count; i++)
      {
        if (keyframes[i].time >= time)
        {
          break;
        }
        keyframeIndex = i;
      }

      int nextKeyFrame = (keyframeIndex + 1) % keyframes.Count;

      beforeIndex = keyframeIndex;
      afterIndex = nextKeyFrame;

      return true;
    }

    public static float ProgressBetweenSurroundingKeyframes(float time, BaseKeyframe beforeKey, BaseKeyframe afterKey) {
      return ProgressBetweenSurroundingKeyframes(time, beforeKey.time, afterKey.time);
    }

    // FIXME - Rename to to percent between circular times.
    public static float ProgressBetweenSurroundingKeyframes(float time, float beforeKeyTime, float afterKeyTime)
    {
      if (afterKeyTime > beforeKeyTime && time <= beforeKeyTime)
      {
        return 0;
      }

      float rangeWidth = WidthBetweenCircularValues(beforeKeyTime, afterKeyTime);
      float valueWidth = WidthBetweenCircularValues(beforeKeyTime, time);

      // Find what percentage this time is between the 2 circular keyframes.
      float progress = valueWidth / rangeWidth;

      return Mathf.Clamp01(progress);
    }

    // FIXME - This should really be called distance between circular values.
    public static float WidthBetweenCircularValues(float begin, float end)
    {
      if (begin <= end)
      {
        return end - begin;
      }

      return (1 - begin) + end;
    }

    public void TrimToSingleKeyframe() {
      if (keyframes.Count == 1) {
        return;
      }
      keyframes.RemoveRange(1, keyframes.Count - 1);
    }


    // Returns -1 to move reverse direction, +1 to move in positive direction only.
    public InterpolationDirection GetShortestInterpolationDirection(float previousKeyValue, float nextKeyValue, float minValue, float maxValue)
    {
      // forward means values can only count upwards to next keyframe. Reverse assumes values can only go downards.
      float forwardDistance;
      float reverseDistance;

      CalculateCircularDistances(previousKeyValue, nextKeyValue, minValue, maxValue, out forwardDistance, out reverseDistance);

      if (reverseDistance > forwardDistance) {
        return InterpolationDirection.Reverse;
      } else {
        return InterpolationDirection.Foward;
      }
    }

    public void CalculateCircularDistances(float previousKeyValue, float nextKeyValue, float minValue, float maxValue, out float forwardDistance, out float reverseDistance)
    {
      if (nextKeyValue < previousKeyValue) {
        forwardDistance = (maxValue - previousKeyValue) + (nextKeyValue - minValue);
      } else {
        forwardDistance = nextKeyValue - previousKeyValue;
      }

      reverseDistance = (minValue + maxValue) - forwardDistance;
    }

    // This will consider the direction, and curve type to return a float value that's interpolated.
    public float InterpolateFloat(InterpolationCurve curve, InterpolationDirection direction,
      float time, float beforeTime, float nextTime,
      float previousKeyValue, float nextKeyValue,
      float minValue, float maxValue)
    {
      float progressBetweenFrames = ProgressBetweenSurroundingKeyframes(time, beforeTime, nextTime);
      float curvedTime = CurveAdjustedBlendingTime(curve, progressBetweenFrames);

      // Auto.
      if (direction == InterpolationDirection.Auto) {
        return AutoInterpolation(curvedTime, previousKeyValue, nextKeyValue);
      }

      InterpolationDirection moveDirection = direction;

      float forwardDistance;
      float reverseDistance;
      CalculateCircularDistances(previousKeyValue, nextKeyValue, minValue, maxValue, out forwardDistance, out reverseDistance);

      // Shortest path.
      if (moveDirection == InterpolationDirection.ShortestPath) {
        if (reverseDistance > forwardDistance) {
          moveDirection = InterpolationDirection.Foward;
        } else {
          moveDirection = InterpolationDirection.Reverse;
        }
      }

      // Forward.
      if (moveDirection == InterpolationDirection.Foward) {
        return ForwardInterpolation(curvedTime, previousKeyValue, nextKeyValue, minValue, maxValue, forwardDistance);
      }

      // Reverse.
      if (moveDirection == InterpolationDirection.Reverse) {
        return ReverseInterpolation(curvedTime, previousKeyValue, nextKeyValue, minValue, maxValue, reverseDistance);
      }

      Debug.LogError("Unhandled interpolation direction: " + moveDirection + ", returning min value.");

      return minValue;
    }

    // Standard interpolation without wrap around.
    public float AutoInterpolation(float curvedTime, float previousValue, float nextValue)
    {
      return Mathf.Lerp(previousValue, nextValue, curvedTime);
    }

    // Force values forward with wrap around.
    public float ForwardInterpolation(float time, float previousKeyValue, float nextKeyValue, float minValue, float maxValue, float distance)
    {
      if (previousKeyValue <= nextKeyValue) {
        return Mathf.Lerp(previousKeyValue, nextKeyValue, time);
      }

      // We know this is gonna rollover now to a lower value.
      float currentDistance = time * distance;
      float toMaxDistance = maxValue - previousKeyValue;

      // return before hitting the max rollover.
      if (currentDistance <= toMaxDistance) {
        return previousKeyValue + currentDistance;
      }

      return minValue + (currentDistance - toMaxDistance);
    }

    // Force values backwards with wrap around.
    public float ReverseInterpolation(float time, float previousKeyValue, float nextKeyValue, float minValue, float maxValue, float distance)
    {
      if (nextKeyValue <= previousKeyValue) {
        return Mathf.Lerp(previousKeyValue, nextKeyValue, time);
      }

      float currentDistance = time * distance;
      float toMinDistance = previousKeyValue - minValue;

      if (currentDistance <= toMinDistance) {
        return previousKeyValue - currentDistance;
      }

      return maxValue - (currentDistance - toMinDistance);
    }
  }
}
