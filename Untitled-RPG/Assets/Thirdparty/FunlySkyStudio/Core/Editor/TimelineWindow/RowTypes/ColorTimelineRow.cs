using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  // Custom editor GUI for skies.
  public abstract class ColorTimelineRow
  {
    private const float k_LineSmoothing = 5.0f;
    private static Mesh m_RectangleMesh;

    private static List<Vector4> FormatColorGradientForShader(ColorKeyframeGroup group)
    {
      List<Vector4> colors = new List<Vector4>(group.keyframes.Count);

      if (group.keyframes.Count == 0)
      {
        return colors;
      }

      // Start at zero if no keyframe.
      List<float> majorTimePoints = new List<float>();
      if (group.keyframes[0].time > .00001f) {
        majorTimePoints.Add(0);
      }

      foreach (ColorKeyframe keyframe in group.keyframes) {
        majorTimePoints.Add(keyframe.time);
      }

      // End at 1 if no keyframe
      if (group.keyframes[group.keyframes.Count - 1].time < .99999f) {
        majorTimePoints.Add(1.0f);
      }

      for (int i = 0; i < (majorTimePoints.Count - 1); i++)
      {
        float currentTime = majorTimePoints[i];
        float nextTime = majorTimePoints[i + 1];

        float timeStep = (nextTime - currentTime) / k_LineSmoothing;
        for (int j = 0; j <= k_LineSmoothing; j++)
        {
          float time = currentTime + (j * timeStep);
          Color timeColor = group.ColorForTime(time);

          colors.Add(new Vector4(time, timeColor.r, timeColor.g, timeColor.b));
        }
      }

      return colors;
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

    public static void RenderColorGroupRow(Rect rect, SkyProfile profile, ColorKeyframeGroup colors)
    {
      bool sortGroup = false;

      RenderColorGroupRow(rect, colors);

      for (int i = 0; i < colors.keyframes.Count; i++) {
        ColorKeyframe currentKey = colors.GetKeyframe(i);

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
            currentKey, colors, KeyframeInspectorWindow.KeyType.Color, profile);

          if (didSingleClick) {
            KeyframeInspectorWindow.ShowWindow();
          }
        }
      }

      if (sortGroup) {
        colors.SortKeyframes();
      }
    }

    public static void RenderColorGroupRow(Rect rect, ColorKeyframeGroup group)
    {
      if ((int) rect.width == 0)
      {
        return;
      }

      List<Vector4> shaderColors = FormatColorGradientForShader(group);

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
      gradientMaterial.SetVectorArray("_ColorPoints", shaderColors);
      gradientMaterial.SetInt("_NumColorPoints", shaderColors.Count);
      gradientMaterial.SetPass(0);

      Graphics.DrawMeshNow(m_RectangleMesh, Vector3.zero, Quaternion.identity);

      GL.PopMatrix();
      RenderTexture.active = oldRenderTexture;

      GUI.DrawTexture(rect, targetTexture);
      RenderTexture.ReleaseTemporary(targetTexture);
    }
  }
}
