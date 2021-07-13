using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  // Rendering for SpherePoint a
  public abstract class SpherePointTimelineRow
  {
    private static Mesh m_RectangleMesh;
    private static Color m_MinColor = Color.blue;
    private static Color m_AvgColor = Color.white;
    private static Color m_MaxColor = Color.red;

    // Size 100 needs to match the shader array size, or unity will truncate values.
    private static Vector4[] m_DebugPointsArray = new Vector4[100]; 

    // Edge between 2 keyframes.
    public class Edge
    {
      public float time;
      public float duration;
      public float distance;
      public Color color;
      public float speed;
      public int fromKeyframeIndex;
      public int toKeyFrameIndex;
    }

    public static void PopulateRectangleMesh(float width, float height, Mesh m)
    {
      List<Vector3> verts = new List<Vector3>();
      List<int> triangles = new List<int>();
      List<Vector2> uvs = new List<Vector2>();

      verts.Add(new Vector3(0, height, 0));
      verts.Add(new Vector3(width, height, 0));
      verts.Add(new Vector3(width, 0, 0));
      verts.Add(new Vector3(0, 0, 0));

      triangles.Add(0);
      triangles.Add(1);
      triangles.Add(3);

      triangles.Add(1);
      triangles.Add(2);
      triangles.Add(3);

      uvs.Add(new Vector2(0, 1));
      uvs.Add(new Vector2(1, 1));
      uvs.Add(new Vector2(1, 0));
      uvs.Add(new Vector2(0, 0));

      m.vertices = verts.ToArray();
      m.triangles = triangles.ToArray();
      m.uv = uvs.ToArray();
    }

    public static void RenderSpherePointRow(Rect rect, SkyProfile profile, SpherePointKeyframeGroup group)
    {
      bool sortGroup = false;

      RenderColorGradients(rect, group);

      for (int i = 0; i < group.keyframes.Count; i++) {
        SpherePointKeyframe currentKey = group.GetKeyframe(i);

        // Track key marker mouse events and render.
        bool didSingleClick = false;
        bool isDragging = false;
        bool keyframeUpdated = false;
        SkyEditorUtility.DrawHorizontalKeyMarker(rect, currentKey, profile,
          out didSingleClick, out isDragging, out keyframeUpdated);

        if (keyframeUpdated) {
          sortGroup = true;
        }

        // Show the color keyframe property window.
        if (didSingleClick || isDragging) {
          // Load info about this keyframe and show the editor window.
          KeyframeInspectorWindow.SetKeyframeData(
            currentKey, group, KeyframeInspectorWindow.KeyType.SpherePoint, profile);

          if (didSingleClick) {
            KeyframeInspectorWindow.ShowWindow();
          }
        }
      }

      if (sortGroup) {
        group.SortKeyframes();
      }
    }

    public static Color GetEdgeSpeedIncreaseColor(float currentEdgeSpeed, float previousEdgeSpeed)
    {
      const float maxFactor = 5.0f;

      if (Mathf.Abs(currentEdgeSpeed - previousEdgeSpeed) <= 4.0f) {
        return m_AvgColor;
      }
      if (currentEdgeSpeed > previousEdgeSpeed) {
        float increaseFactor = currentEdgeSpeed / previousEdgeSpeed;
        float percent = increaseFactor / maxFactor;
        return Color.Lerp(m_AvgColor, m_MaxColor, percent);
      } else {
        float increaseFactor = previousEdgeSpeed / currentEdgeSpeed;
        float percent = increaseFactor / maxFactor;
        return Color.Lerp(m_AvgColor, m_MinColor, percent);
      }
    }

    public static float GetSpeedBetweenKeyframes(SpherePointKeyframe from, SpherePointKeyframe to)
    {
      float distance = Vector3.Angle(from.spherePoint.GetWorldDirection(), to.spherePoint.GetWorldDirection());
      float duration = GetDurationBetweenKeyframes(from, to);

      // Just in case duration is zero...
      if (duration == 0.0f)
      {
        duration = .00001f;
      }

      return distance / duration;
    }

    // Get the amount of time between 2 keyframes.
    public static float GetDurationBetweenKeyframes(IBaseKeyframe from, IBaseKeyframe to)
    {
      if (from.time <= to.time)
      {
        return to.time - from.time;
      }
      else
      {
        return (1.0f - from.time) + to.time;
      }
    }

    public static float GetDistanceBetweenKeyframes(SpherePointKeyframe from, SpherePointKeyframe to)
    {
      return Vector3.Angle(from.spherePoint.GetWorldDirection(), to.spherePoint.GetWorldDirection());
    }
    
    // Color based on last edge speed.
    public static List<Vector4> GenerateColorSpeedPoints(SpherePointKeyframeGroup group)
    {
      List<Vector4> colorPoints = new List<Vector4>();

      if (group.GetKeyFrameCount() <= 1) {
        colorPoints.Add(new Vector4(0.0f, m_AvgColor.r, m_AvgColor.g, m_AvgColor.b));
        colorPoints.Add(new Vector4(1.0f, m_AvgColor.r, m_AvgColor.g, m_AvgColor.b));
        return colorPoints;
      }

      float gradientOffset = .02f;
      const float maxSpeed = 1200.0f;

      for (int i = 0; i < group.keyframes.Count; i++) {
        int nextIndex = (i + 1) % group.GetKeyFrameCount();
        SpherePointKeyframe currentKeyframe = group.keyframes[i];
        SpherePointKeyframe nextKeyframe = group.keyframes[nextIndex];

        // Last keyframe has special rules
        if (i == group.keyframes.Count - 1)
        {
          float speed = GetSpeedBetweenKeyframes(currentKeyframe, nextKeyframe);
          Color speedColor = Color.Lerp(m_AvgColor, m_MaxColor, speed / maxSpeed);

          float gradientBeginTime = currentKeyframe.time + gradientOffset;
          float gradientEndTime = 1.0f;

          if (gradientBeginTime >= 1.0f)
          {
            gradientBeginTime = currentKeyframe.time;
          }

          colorPoints.Add(
            new Vector4(gradientBeginTime, speedColor.r, speedColor.g, speedColor.b));
          colorPoints.Add(
            new Vector4(gradientEndTime, speedColor.r, speedColor.g, speedColor.b));

          // Now bridge gap if any to first keyframe.
          if (!SkyEditorUtility.IsKeyFrameAtStart(nextKeyframe))
          {
            gradientBeginTime = 0.0f;
            gradientEndTime = nextKeyframe.time - gradientOffset;
            if (gradientEndTime < 0)
            {
              gradientEndTime = nextKeyframe.time;
            }

            colorPoints.Insert(0,
              new Vector4(gradientEndTime, speedColor.r, speedColor.g, speedColor.b));
            colorPoints.Insert(0,
              new Vector4(gradientBeginTime, speedColor.r, speedColor.g, speedColor.b));
          } else {
            colorPoints.Insert(
              0,
              new Vector4(0, speedColor.r, speedColor.g, speedColor.b));
          }
        }
        else
        {
          float gradientBeginTime = currentKeyframe.time + gradientOffset;
          float gradientEndTime = nextKeyframe.time - gradientOffset;

          // Need to play with these to see how they look. Maybe use a percentage between them instead?
          if (gradientBeginTime >= nextKeyframe.time) {
            gradientBeginTime = currentKeyframe.time;
          }

          if (gradientEndTime <= currentKeyframe.time) {
            gradientEndTime = nextKeyframe.time;
          }


          float speed = GetSpeedBetweenKeyframes(currentKeyframe, nextKeyframe);
          Color speedColor = Color.Lerp(m_AvgColor, m_MaxColor, speed / maxSpeed);

          colorPoints.Add(
            new Vector4(gradientBeginTime, speedColor.r, speedColor.g, speedColor.b));
          colorPoints.Add(
            new Vector4(gradientEndTime, speedColor.r, speedColor.g, speedColor.b));
        }
      }

      return colorPoints;
    }

    public static void RenderColorGradients(Rect rect, SpherePointKeyframeGroup group)
    {
      if ((int) rect.width == 0)
      {
        return;
      }

      // Generate speed colors to visual rate of transition with distances and time.
      List<Vector4> shaderColors = GenerateColorSpeedPoints(group);
      for (int i = 0; i < shaderColors.Count; i++)
      {
        m_DebugPointsArray[i] = shaderColors[i];
      }

      if (m_RectangleMesh == null)
      {
        m_RectangleMesh = new Mesh()
        {
          hideFlags = HideFlags.HideInHierarchy
        };
        m_RectangleMesh.MarkDynamic();
      }

      // Create a mesh that fits the aspect ratio
      PopulateRectangleMesh(Mathf.Floor(rect.width), Mathf.Floor(rect.height), m_RectangleMesh);

      int textureSize = Mathf.NextPowerOfTwo((int)rect.width);
      RenderTexture targetTexture = RenderTexture.GetTemporary(
        textureSize,
        textureSize, 
        0,
        RenderTextureFormat.ARGB32,
        RenderTextureReadWrite.sRGB);

      RenderTexture oldRenderTexture = RenderTexture.active;

      GL.PushMatrix();
      GL.LoadPixelMatrix(0, Mathf.Floor(rect.width), 0, Mathf.Floor(rect.height));
      
      RenderTexture.active = targetTexture;

      GL.Clear(true, true, Color.white);

      // Configure the shader.
      Material gradientMaterial = new Material(Shader.Find("Hidden/Funly/SkyStudio/MultiColorGradient"))
      {
        hideFlags = HideFlags.HideInHierarchy
      };
      gradientMaterial.SetVectorArray("_ColorPoints", m_DebugPointsArray);
      gradientMaterial.SetInt("_NumColorPoints", shaderColors.Count);
      gradientMaterial.SetPass(0);

      Graphics.DrawMeshNow(m_RectangleMesh, Vector3.zero, Quaternion.identity);

      GL.PopMatrix();
      RenderTexture.active = oldRenderTexture;

      GUI.DrawTexture(rect, targetTexture);
      RenderTexture.ReleaseTemporary(targetTexture);
    }

    public static float GetTotalDistanceBetweenKeyframes(SpherePointKeyframeGroup group)
    {
      float total = 0;
      for (int i = 0; i < group.GetKeyFrameCount(); i++)
      {
        int nextIndex = (i + 1) % group.GetKeyFrameCount();
        SpherePointKeyframe currentKeyframe = group.keyframes[i];
        SpherePointKeyframe nextKeyframe = group.keyframes[nextIndex];

        total += GetDistanceBetweenKeyframes(currentKeyframe, nextKeyframe);
      }
      return total;
    }

    public static float GetTotalDistanceBetweenKeyframes(List<Edge> edges)
    {
      float total = 0;
      foreach (Edge e in edges)
      {
        total += e.distance;
      }

      return total;
    }

    public static void MoveKeyframeTimeToMatchSpeed(SpherePointKeyframe keyframe, SpherePointKeyframe nextKeyframe, float speed)
    {
      // Solve for duration required to match a speed.
      float duration = GetDistanceBetweenKeyframes(keyframe, nextKeyframe) / speed;
      float durationDelta = GetDurationBetweenKeyframes(keyframe, nextKeyframe) - duration;

      if (keyframe.time + durationDelta < 0) {
        keyframe.time = 1.0f - (keyframe.time + durationDelta);
      } else {
        keyframe.time += durationDelta;
      }

      keyframe.time = Mathf.Clamp01(keyframe.time);
    }

    public static void RepositionKeyframesForConstantSpeed(SpherePointKeyframeGroup group)
    {
      if (group.GetKeyFrameCount() < 2)
      {
        return;
      }

      // Because time is always 1, no need to divide by it.
      float totalDistance = GetTotalDistanceBetweenKeyframes(group);
      float targetSpeed = totalDistance;

      int keyFrameCount = group.GetKeyFrameCount();

      for (int j = 0; j < keyFrameCount; j++)
      {
        for (int i = keyFrameCount - 2; i >= 0; i--)
        {
          int nextIndex = i + 1;
          SpherePointKeyframe keyframe = group.keyframes[i];
          SpherePointKeyframe nextKeyframe = group.keyframes[nextIndex];

          MoveKeyframeTimeToMatchSpeed(keyframe, nextKeyframe, targetSpeed);
        }

        // This is a hack, we resort and re-calculated edges since durations were changed.
        group.SortKeyframes();
      } 
    }
  }
}
