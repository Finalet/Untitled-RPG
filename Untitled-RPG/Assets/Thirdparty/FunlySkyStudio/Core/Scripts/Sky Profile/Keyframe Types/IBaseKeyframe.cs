using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Curves alter the timing between keyframes.
  public enum InterpolationCurve
  {
    Linear,
    EaseInEaseOut
  }
  
  // Direction determines how we get to the next keyframe, allowing animations to move in controlled directions.
  public enum InterpolationDirection
  {
    Auto,          // Selects forward or backwards, and values will not wrap around.
    Foward,        // Values can only incrase with wrapping allowed; (1, 2, 3, Max, Min, 0, ..),
    Reverse,       // Values can only go downwards with wrapping allowed; (3, 2, 1, Min, Max, 5, ..)
    ShortestPath   // Picks shortest distance to next keyframe by allowing wrap around from min/max values.
  }

  public interface IBaseKeyframe
  {
    string id { get; }
    float time { get; set; }
    InterpolationCurve interpolationCurve { get; set; }
    InterpolationDirection interpolationDirection { get; set; }
  }
}

