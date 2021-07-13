using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Funly.SkyStudio
{
  public abstract class NumberTimelineRow
  {
    private const float k_LineThickness = 90.0f;
    private const float k_LineEdgeFeathering = .4f;
    private const float k_BackgroundShade = .43f;
    private const float k_ShadowShade = .32f;
    private const float k_LineSmoothing = 10.0f;
    private static Material m_LineMaterial;
    private static Material m_LineShadowMaterial;
    private static Mesh m_LineMesh;
    private static RenderTexture m_LineRenderTexture;

    public static List<Vector3> GetLinePoints(NumberKeyframeGroup group)
    {
      List<Vector3> points = new List<Vector3>();

      if (group.keyframes.Count == 0) {
        return points;
      }

      List<float> majorTimePoints = new List<float>();
      if (group.keyframes[0].time > .00001f) {
        majorTimePoints.Add(0);
      }

      foreach (NumberKeyframe keyframe in group.keyframes)
      {
        majorTimePoints.Add(keyframe.time);
      }

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
          float pointTime = currentTime + (j * timeStep);
          points.Add(new Vector3(pointTime, group.ValuePercentAtTime(pointTime)));
        }
      }

      return points;
    }

    private static void BuildLineMesh(Mesh m, List<Vector3> centerPoints, float maxWidth, float maxHeight, float lineThickness)
    {
      m.Clear();

      if (centerPoints.Count <= 1)
      {
        return;
      }

      List<int> triangles = new List<int>();
      List<Vector3> verts = new List<Vector3>();
      List<Vector2> uvs = new List<Vector2>();

      Vector3 inwardVect = new Vector3(0, 0, 1);
      Vector3 outwardVect = new Vector3(0, 0, -1);

      for (int i = 0; i < (centerPoints.Count - 1); i++)
      {
        Vector3 percentLeftPoint = centerPoints[i];
        Vector3 leftPoint = new Vector3(
          percentLeftPoint.x * maxWidth, 
          percentLeftPoint.y * maxHeight, 
          0);

        Vector3 percentRightPoint = centerPoints[i + 1];
        Vector3 rightPoint = new Vector3(
          percentRightPoint.x * maxWidth, 
          percentRightPoint.y * maxHeight, 
          0);

        Vector3 aVect = rightPoint - leftPoint;
        aVect = Vector3.Normalize(aVect);

        Vector3 upperVect = Vector3.Normalize(Vector3.Cross(inwardVect, aVect)) * lineThickness;
        Vector3 lowerVect = Vector3.Normalize(Vector3.Cross(outwardVect, aVect)) * lineThickness;

        Vector3 upperLeftPoint = leftPoint + upperVect;
        Vector3 lowerLeftPoint = leftPoint + lowerVect;

        Vector3 upperRightPoint = rightPoint + upperVect;
        Vector3 lowerRightPoint = rightPoint + lowerVect;

        verts.Add(upperLeftPoint);
        verts.Add(upperRightPoint);
        verts.Add(lowerRightPoint);
        verts.Add(lowerLeftPoint);

        uvs.Add(Vector2.one);
        uvs.Add(Vector2.one);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);

        int vertIndexOffset = i * 4;

        triangles.Add(vertIndexOffset);
        triangles.Add(vertIndexOffset + 1);
        triangles.Add(vertIndexOffset + 3);

        triangles.Add(vertIndexOffset + 3);
        triangles.Add(vertIndexOffset + 1);
        triangles.Add(vertIndexOffset + 2);

        // Draw 2 triangles to close the gap line segments.
        if (i > 0) {
          int previousVertIndex = (i - 1) * 4;
          triangles.Add(previousVertIndex + 1);
          triangles.Add(vertIndexOffset);
          triangles.Add(previousVertIndex + 2);

          triangles.Add(previousVertIndex + 2);
          triangles.Add(vertIndexOffset);
          triangles.Add(vertIndexOffset + 3);
        }
      }

      m.vertices = verts.ToArray();
      m.triangles = triangles.ToArray();
      m.uv = uvs.ToArray();
    }

    private static void RenderLinePath(Rect rect, NumberKeyframeGroup group)
    {
      if ((int)rect.width <= 0 || (int)rect.height <= 0 || Event.current.type != EventType.Repaint) {
        return;
      }

      int squareSize = 2048;

      if (m_LineRenderTexture == null) {
        m_LineRenderTexture = new RenderTexture(
          squareSize,
          squareSize,
          16,
          RenderTextureFormat.ARGB32,
          RenderTextureReadWrite.sRGB);
        m_LineRenderTexture.antiAliasing = 2;
        m_LineRenderTexture.filterMode = FilterMode.Bilinear;
      }

      if (!m_LineRenderTexture.IsCreated()) {
        m_LineRenderTexture.Create();
      }

      if (m_LineMesh == null)
      {
        m_LineMesh = new Mesh() { hideFlags = HideFlags.HideAndDontSave };
        m_LineMesh.MarkDynamic();
      }

      List<Vector3> points = GetLinePoints(group);

      // Background color.
      Color bgColor = new Color(k_BackgroundShade, k_BackgroundShade, k_BackgroundShade);

      // Line shader. 
      if (m_LineMaterial == null) {
        m_LineMaterial = new Material(Shader.Find("Hidden/Funly/Sky/SoftLine"))
        {
          hideFlags = HideFlags.HideInHierarchy
        };
      }

      m_LineMaterial.SetColor("_LineColor", ColorHelper.ColorWithHexAlpha(0x2AFBFFFF));
      m_LineMaterial.SetFloat("_EdgeFeathering", k_LineEdgeFeathering);
      m_LineMaterial.SetColor("_BackgroundColor", bgColor);
      m_LineMaterial.SetTexture("_MainTex", SkyEditorUtility.LoadEditorResourceTexture("NumberShadowLine", false));

      if (m_LineShadowMaterial == null)
      {
        m_LineShadowMaterial = new Material(Shader.Find("Hidden/Funly/Sky/LineShadow"))
        {
          hideFlags = HideFlags.HideInHierarchy
        };
      }

      Color shadowColor = new Color(k_ShadowShade, k_ShadowShade, k_ShadowShade, 1.0f);
      m_LineShadowMaterial.SetColor("_BackgroundColor", bgColor);
      m_LineShadowMaterial.SetColor("_ShadowColor", shadowColor);

      // Setup tmp texture to render into.
      RenderTexture oldRenderTexture = RenderTexture.active;
      RenderTexture.active = m_LineRenderTexture;

      GL.PushMatrix();
      GL.LoadPixelMatrix(0, m_LineRenderTexture.width, 0, m_LineRenderTexture.height);
      GL.Clear(true, true, bgColor);

      float lineInset = 5.0f;
      float heightScale = rect.height / rect.width;
      float lineThickness = k_LineThickness * heightScale;

      BuildLineMesh(
        m_LineMesh, points, 
        m_LineRenderTexture.width, 
        m_LineRenderTexture.width * heightScale - (lineInset * 2),
        lineThickness);

      // Line shadow.
      m_LineShadowMaterial.SetPass(0);
      Graphics.DrawMeshNow(m_LineMesh, new Vector3(0, lineInset - 8, 0), Quaternion.identity);

      // Actual line.
      m_LineMaterial.SetPass(0);
      Graphics.DrawMeshNow(m_LineMesh, new Vector3(0, lineInset, 0), Quaternion.identity); 

      GL.PopMatrix();

      RenderTexture.active = oldRenderTexture;

      // Pull out just the snippet section we want to use.
      Rect textureRect = new Rect(
        0, 0, 1, heightScale);
      GUI.DrawTextureWithTexCoords(rect, m_LineRenderTexture, textureRect);
    }

    // Use vertex shader to fix aspect ratio.
    public static void RenderNumberGroup(Rect rect, SkyProfile profile, NumberKeyframeGroup group) {
      lock (profile) {
        bool sortKeyFrames = false;

        RenderLinePath(rect, group);

        for (int i = 0; i < group.keyframes.Count; i++) {
          NumberKeyframe currentKey = group.GetKeyframe(i);

          int nextIndex = (i + 1) % group.keyframes.Count;
          NumberKeyframe nextKey = group.GetKeyframe(nextIndex);

          // Clamp time if we're wrapping around.
          float nextTime = nextKey.time;
          if (nextTime <= currentKey.time) {
            nextTime = 1.0f;
          }

          bool didSingleClick = false;
          bool isDragging = false;
          bool keyframeUpdated = false;

          SkyEditorUtility.DrawNumericKeyMarker(rect, currentKey, group, profile,
            out didSingleClick, out isDragging, out keyframeUpdated);

          if (keyframeUpdated) {
            sortKeyFrames = true;
          }

          if (didSingleClick || isDragging) {
            KeyframeInspectorWindow.SetKeyframeData(
              currentKey, group, KeyframeInspectorWindow.KeyType.Numeric, profile);

            if (didSingleClick && KeyframeInspectorWindow.inspectorEnabled == false) {
              KeyframeInspectorWindow.ShowWindow();
            }
          }
        }

        if (sortKeyFrames) {
          group.SortKeyframes();
        }
      }
    }
  }
}

