using System;
using System.Collections;
using System.Collections.Generic;
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class BaseKeyframe : System.Object, IComparable, IBaseKeyframe
  {
    [SerializeField]
    public string m_Id;
    public string id
    {
      get { return m_Id; }
      set { m_Id = value; }
    }

    // Time this keyframe begins at, (0-1);
    [SerializeField]
    private float m_Time;
    public float time
    {
      get { return m_Time; }
      set { m_Time = value; }
    }

    // Adjust interpolation curve to next keyframe.
    [SerializeField]
    private InterpolationCurve m_InterpolationCurve = InterpolationCurve.Linear;
    public InterpolationCurve interpolationCurve
    {
      get { return m_InterpolationCurve; }
      set { m_InterpolationCurve = value; }
    }

    [SerializeField]
    private InterpolationDirection m_InterpolationDirection = InterpolationDirection.Auto;
    public InterpolationDirection interpolationDirection
    {
      get { return m_InterpolationDirection; }
      set { m_InterpolationDirection = value; }
    }

    public BaseKeyframe(float time)
    {
      id = Guid.NewGuid().ToString();
      this.time = time;
    }

    public int CompareTo(object other)
    {
      BaseKeyframe otherFrame = other as BaseKeyframe;
      return time.CompareTo(otherFrame.time);
    }
  }
}

