using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  public class KeyframeInspectorWindow : EditorWindow
  {
    public enum KeyType
    {
      None,
      Color,
      Numeric,
      SpherePoint
    }

    public static bool inspectorEnabled { get; private set; }
    public static SkyProfile profile { get; private set; }
    public static KeyType keyType { get; private set; }
    public static IKeyframeGroup group { get; private set; }
    public static IBaseKeyframe keyframe { get; private set; }

    private static KeyframeInspectorWindow _instance;
    private static float k_MinWidth = 350.0f;
    private static float k_MinHeight = 150.0f;
    
    public static void ShowWindow()
    {
      if (_instance == null) {
				_instance = CreateInstance<KeyframeInspectorWindow>();
				_instance.name = "Keyframe Inspector";
				_instance.titleContent = new GUIContent("Keyframe Inspector");
        _instance.minSize = new Vector2(k_MinWidth, k_MinHeight);
      }
      _instance.ShowUtility();
    }

    private void OnEnable()
    {
      inspectorEnabled = true;
    }

    private void OnDisable()
    {
      inspectorEnabled = false;
    }

    public void ForceRepaint()
    {
      Repaint();
    }

    private void Update()
    {
      // Make sure we only have 1 utility window open.
      if (this != _instance) {
        this.Close();
        return;
      }
      
      Repaint();
    }

    public static void SetKeyframeData(IBaseKeyframe keyframe, IKeyframeGroup group, KeyType keyType, SkyProfile profile)
    {
      KeyframeInspectorWindow.keyframe = keyframe;
      KeyframeInspectorWindow.@group = group;
      KeyframeInspectorWindow.keyType = keyType;
      KeyframeInspectorWindow.profile = profile;
    }

    private void OnGUI()
    {
      if (keyType == KeyType.None || keyframe == null)
      {
        keyType = KeyType.None;
        ShowEmptyState();
        return;
      }

      GUILayout.BeginVertical();
      GUILayout.Space(5);

      bool didModifyProfile = false;

      // Render time.
      didModifyProfile = RenderTimeValue();

      // Animation curve.
      didModifyProfile = RenderInterpolationCurveType();

      // Core layout for this type of keyframe.
      if (keyType == KeyType.Color)
      {
        didModifyProfile = RenderColorGUI();
      }
      else if (keyType == KeyType.Numeric)
      {
        didModifyProfile = RenderNumericGUI();
      } else if (keyType == KeyType.SpherePoint)
      {
        didModifyProfile = RenderSpherePointGUI();
      }

      GUILayout.FlexibleSpace();

      // Buttom buttons.
      GUILayout.BeginHorizontal();

      if (KeyFrameCount() > 1)
      {
        if (GUILayout.Button("Delete Keyframe"))
        {
          Undo.RecordObject(profile, "Deleting keyframe");

          @group.RemoveKeyFrame(keyframe);
          keyframe = null;
          

          didModifyProfile = true;
          Close();
        }
      }

      GUILayout.FlexibleSpace();

      if (GUILayout.Button("Close"))
      {
        Close();
      }

      GUILayout.EndHorizontal();

      GUILayout.Space(5);
      GUILayout.EndVertical();

      if (didModifyProfile)
      {
        @group.SortKeyframes();
        EditorUtility.SetDirty(profile);
      }
    }

    private void ShowEmptyState()
    {
      EditorGUILayout.HelpBox("No keyframe selected.", MessageType.Info);
    }

    private int KeyFrameCount()
    {
      return @group.GetKeyFrameCount();
    }

    private bool RenderColorGUI()
    {
      ColorKeyframe colorKeyFrame = keyframe as ColorKeyframe;
      if (colorKeyFrame == null)
      {
        return false;
      }

      bool didModify = false;

      EditorGUI.BeginChangeCheck();
      Color selectedColor = EditorGUILayout.ColorField(new GUIContent("Color"), colorKeyFrame.color);
      if (EditorGUI.EndChangeCheck()) {
        Undo.RecordObject(profile, "Keyframe color changed.");
        colorKeyFrame.color = selectedColor;
        didModify = true;
      }

      return didModify;
    }

    private bool RenderNumericGUI()
    {
      NumberKeyframe numberKeyframe = keyframe as NumberKeyframe;
      if (numberKeyframe == null)
      {
        return false;
      }

      NumberKeyframeGroup numberGroup = @group as NumberKeyframeGroup;
      if (numberGroup == null)
      {
        return false;
      }

      bool didModify = false;

      EditorGUI.BeginChangeCheck();
      float value = EditorGUILayout.FloatField(new GUIContent("Value"), numberKeyframe.value);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(profile, "Keyframe numeric value changed.");
        numberKeyframe.value = Mathf.Clamp(value, numberGroup.minValue, numberGroup.maxValue);
        didModify = true;
      }

      return didModify;
    }

    private bool RenderSpherePointGUI()
    {
      bool didModify = false;

      SpherePointKeyframe spherePointKeyframe = keyframe as SpherePointKeyframe;
      if (spherePointKeyframe == null)
      {
        return false;
      }

      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      SpherePoint selectedSpherePoint = SpherePointGUI.SpherePointField(spherePointKeyframe.spherePoint, true, keyframe.id);
      if (EditorGUI.EndChangeCheck())
      {
        spherePointKeyframe.spherePoint = selectedSpherePoint;
        didModify = true;
      }
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      GUIStyle style = GUI.skin.button;
      EditorGUILayout.PrefixLabel(
        new GUIContent("Normalize Speed", 
                       "Adjust all keyframes so that there is a constant speed of animation between them."));
      GUILayout.Button(new GUIContent("Reposition All Keyframes..."), style);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(profile, "Normalize keyframe speeds");
        didModify = true;
        SpherePointTimelineRow.RepositionKeyframesForConstantSpeed(group as SpherePointKeyframeGroup);
      }
      EditorGUILayout.EndHorizontal();
      
      return didModify;
    }

    public bool RenderInterpolationCurveType()
    {
      EditorGUI.BeginChangeCheck();

      InterpolationCurve selectedCurveType = (InterpolationCurve)EditorGUILayout.EnumPopup(
        new GUIContent("Animation Curve", 
        "Adjust animation curve timing to the next keyframe." ), keyframe.interpolationCurve);

      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(profile, "Curve type changed");
        keyframe.interpolationCurve = selectedCurveType;
        return true;
      }

      return false;
    }

    public bool RenderInterpolationDirectionType()
    {
      EditorGUI.BeginChangeCheck();

      InterpolationDirection selectedDirection = (InterpolationDirection)EditorGUILayout.EnumPopup(
        new GUIContent("Animation Direction",
        "Adjust the animation direction to control how this property animates to the next keyframe value."), keyframe.interpolationDirection);

      if (EditorGUI.EndChangeCheck()) {
        Undo.RecordObject(profile, "Direction changed");
        keyframe.interpolationDirection = selectedDirection;
        return true;
      }

      return false;
    }

    public bool RenderTimeValue()
    {
      EditorGUI.BeginChangeCheck();

      float time = EditorGUILayout.FloatField(
        new GUIContent("Time", 
        "Time position this keyframe is at in the current day. This is a value between 0 and 1."),
        keyframe.time);

      if (EditorGUI.EndChangeCheck()) {
        Undo.RecordObject(profile, "Keyframe time changed");
        keyframe.time = Mathf.Clamp01(time);
        return true;
      }

      return false;
    }

    public static string GetActiveKeyframeId() {
      if (inspectorEnabled == false || keyType == KeyType.None || keyframe == null) {
        return null;
      }

      return keyframe.id;
    }

    private void UpdateTimeControllerInScene()
    {
      TimeOfDayController timeController = FindObjectOfType<TimeOfDayController>() as TimeOfDayController;
      if (!timeController)
      {
        return;
      }

      timeController.UpdateSkyForCurrentTime();
    }
  }
}
