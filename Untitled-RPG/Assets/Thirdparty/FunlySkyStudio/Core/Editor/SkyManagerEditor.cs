using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{

  // TODO - Delete this class, it's been replaced.
  public class SkyManagerEditor : Editor
  {
    private static string GRADIENT_KEYWORD = "GRADIENT_BACKGROUND";
    private static string STAR_LAYER_1_KEYWORD = "STAR_LAYER_1";
    private static string STAR_LAYER_2_KEYWORD = "STAR_LAYER_2";
    private static string STAR_LAYER_3_KEYWORD = "STAR_LAYER_3";
    private static string MOON_KEYWORD = "MOON";
    private static string SUN_KEYWORD = "SUN";
    private static string CLOUDS_KEYWORD = "CLOUDS";

    private static string EDITOR_PREF_NEEDS_REBUILDING_KEY = "SkyNeedsRebuilding";

    private bool _gradientEnabled;
    private bool _starLayer1Enabled;
    private bool _starLayer2Enabled;
    private bool _starLayer3Enabled;
    private bool _moonEnabled;
    private bool _sunEnabled;
    private bool _cloudsEnabled;

    private Material _skyboxMaterial;

    private SkyBuilder _builder;
    private bool _skyNeedsRebuilding;

    private Texture2D _sectionHeaderBg;

    private void OnEnable()
    {
      serializedObject.Update();

      _sectionHeaderBg = CreateColorImage(Color.grey);

      if (EditorPrefs.HasKey(EDITOR_PREF_NEEDS_REBUILDING_KEY)) {
        _skyNeedsRebuilding = EditorPrefs.GetBool(EDITOR_PREF_NEEDS_REBUILDING_KEY);
      }
    }

    private void OnDisable()
    {
      EditorPrefs.SetBool(EDITOR_PREF_NEEDS_REBUILDING_KEY, _skyNeedsRebuilding);
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      DisplayCustomLayout();
      serializedObject.ApplyModifiedProperties();
    }

    private void DisplayCustomLayout()
    {
      serializedObject.Update();

      EditorGUILayout.PropertyField(serializedObject.FindProperty("_skyboxMaterial"));

      _skyboxMaterial = SkyboxMaterial();
      if (_skyboxMaterial == null) {
        EditorGUILayout.HelpBox("You can't edit a skybox without a skybox material.", MessageType.Error);
        return;
      }
  
      // Verify it's a supported shader.
      if (_skyboxMaterial.shader.name.Contains("Funly/Sky") == false)
      {
        Debug.LogError("Skybox material must use the DyanmicSky shader, clearing material field.");
        serializedObject.FindProperty("_skyboxMaterial").objectReferenceValue = null;
        //SkyMaterialController.Instance.SkyboxMaterial = null;
      }
      
      // Load enabled features from shader.
      LoadFeatureFlagsFromShader();
      EditorGUILayout.Space();

      // Button to rebuild skyColors data.
      RebuildSettings();
      EditorGUILayout.Space();

      // Feature settings.
      FeatureSettings();

      // Background.
      SkyBackgroundSettings();

      // Star layer 1 settings.
      if (_starLayer1Enabled) {
        StarLayerSettings("1");
      }

      // Star layer 2 settings.
      if (_starLayer2Enabled) {
        StarLayerSettings("2");
      }

      // Star layer 3 settings.
      if (_starLayer3Enabled) {
        StarLayerSettings("3");
      }

      // Sun Settings.
      if (_sunEnabled)
      {
        SunSettings();
      }

      // Moon settings.
      if (_moonEnabled) {
        MoonSettings();
      }

      // Cloud Settings.
      if (_cloudsEnabled)
      {
        CloudSettings();
      }

      serializedObject.ApplyModifiedProperties();
    }

    private void LoadFeatureFlagsFromShader()
    {
      if (_skyboxMaterial == null)
      {
        return;
      }

      _gradientEnabled = _skyboxMaterial.IsKeywordEnabled(GRADIENT_KEYWORD);
      _starLayer1Enabled = _skyboxMaterial.IsKeywordEnabled(STAR_LAYER_1_KEYWORD);
      _starLayer2Enabled = _skyboxMaterial.IsKeywordEnabled(STAR_LAYER_2_KEYWORD);
      _starLayer3Enabled = _skyboxMaterial.IsKeywordEnabled(STAR_LAYER_3_KEYWORD);
      _moonEnabled = _skyboxMaterial.IsKeywordEnabled(MOON_KEYWORD);
      _sunEnabled = _skyboxMaterial.IsKeywordEnabled(SUN_KEYWORD);
      _cloudsEnabled = _skyboxMaterial.IsKeywordEnabled(CLOUDS_KEYWORD);
    }

    private void FeatureSettings()
    {
      AddSectionTitle("Skybox Features");
      _starLayer1Enabled = ToggleShaderFeature("Use Star Layer 1", STAR_LAYER_1_KEYWORD, _starLayer1Enabled, false);
      _starLayer2Enabled = ToggleShaderFeature("Use Star Layer 2", STAR_LAYER_2_KEYWORD, _starLayer2Enabled, false);
      _starLayer3Enabled = ToggleShaderFeature("Use Star Layer 3", STAR_LAYER_3_KEYWORD, _starLayer3Enabled, false);
      _moonEnabled = ToggleShaderFeature("Use Moon", MOON_KEYWORD, _moonEnabled, false);
      _sunEnabled = ToggleShaderFeature("Use Sun", SUN_KEYWORD, _sunEnabled, false);
      _cloudsEnabled = ToggleShaderFeature("Use Clouds", CLOUDS_KEYWORD, _cloudsEnabled, false);
    }

    private void SkyBackgroundSettings()
    {
      AddSectionTitle("Sky Background");

      _gradientEnabled = ToggleShaderFeature("Use Gradient Background", GRADIENT_KEYWORD, _gradientEnabled, false);

      if (_gradientEnabled) {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_skyColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_horizonColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_gradientFadeBegin"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_gradientFadeLength"));
      } else {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_backgroundCubemap"));
      }

      EditorGUILayout.PropertyField(serializedObject.FindProperty("_starFadeBegin"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("_starFadeLength"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("_horizonDistanceScale"));
    }

    private void RebuildSettings()
    {
      // Make rebuild button red to help user not miss it after changes.
      GUIStyle style = new GUIStyle(GUI.skin.button);
      if (DoesStarSystemNeedRebuilding()) {
        style.normal.textColor = Color.red;
      }

      // Rebuild our data images if necessary.
      if (GUILayout.Button("Rebuild Star System", style)) {
        if (_builder == null) {
          _builder = CreateSkyBuilder();
        }

        if (_builder.IsComplete == false) {
          return;
        }

        _builder.BuildSkySystem();
      }
    }

    private void StarLayerSettings(string layerId)
    {
      AddSectionTitle("Star Layer " + layerId);

      string namePrefix = "_StarLayer" + layerId;

      float density = _skyboxMaterial.GetFloat(namePrefix + "Density");
      float currDensity = EditorGUILayout.Slider("Star Layer " + layerId + " Density",
        density, 0, .05f);

      _skyboxMaterial.SetFloat(namePrefix + "Density", currDensity);

      if (_skyNeedsRebuilding == false && density != currDensity) {
        _skyNeedsRebuilding = true;
      }

      SerializedProperty prop = serializedObject.GetIterator();
      while (prop.NextVisible(true)) {
        if (prop.name.Contains("_starLayer" + layerId)) {
          EditorGUILayout.PropertyField(prop);
        }
      }
    }

    private SkyBuilder CreateSkyBuilder()
    {
      SkyBuilder b = new SkyBuilder();
      b.starLayer1Enabled = _starLayer1Enabled;
      b.starLayer2Enabled = _starLayer2Enabled;
      b.starLayer3Enabled = _starLayer3Enabled;
      b.skyboxMaterial = _skyboxMaterial;
      b.completionCallback += BuilderCompletion;

      return b;
    }

    bool ToggleShaderFeature(string label, string keyword, bool value, bool leftAlign)
    {
      bool updatedValue;

      if (leftAlign) {
        updatedValue = EditorGUILayout.ToggleLeft(label, value);
      } else {
        updatedValue = EditorGUILayout.Toggle(label, value);
      }
      UpdateShaderKeyword(keyword, updatedValue);
      return updatedValue;
    }

    void UpdateShaderKeyword(string keyword, bool value)
    {
      if (value) {
        _skyboxMaterial.EnableKeyword(keyword);
      } else {
        _skyboxMaterial.DisableKeyword(keyword);
      }
    }

    void SunSettings()
    {
      RenderSettingsSection("Sun", "_sun");
    }

    void MoonSettings()
    {
      RenderSettingsSection("Moon", "_moon");
    }

    void CloudSettings()
    {
      RenderSettingsSection("Clouds", "_cloud");
    }

    void RenderSettingsSection(string title, string variablePrefix)
    {
      AddSectionTitle(title);

      SerializedProperty prop = serializedObject.GetIterator();
      while (prop.NextVisible(true)) {
        if (prop.name.Contains(variablePrefix)) {
          EditorGUILayout.PropertyField(prop);
        }
      }
    }

    private Material SkyboxMaterial()
    {
      //SkyMaterialController controller = SkyMaterialController.Instance;
      //return controller != null ? controller.SkyboxMaterial : null;
      return null;
    }

    private void AddSectionTitle(string title)
    {
      EditorGUILayout.Space();

      GUIStyle bgStyle = new GUIStyle();
      bgStyle.normal.background = _sectionHeaderBg;

      EditorGUILayout.BeginHorizontal(bgStyle);

      GUIStyle titleStyle = new GUIStyle();
      titleStyle.normal.textColor = Color.black;
      titleStyle.fontStyle = FontStyle.Bold;

      EditorGUILayout.LabelField(title, titleStyle);

      EditorGUILayout.EndHorizontal();
    }

    private Texture2D CreateColorImage(Color c)
    {
      Texture2D tex = new Texture2D(1, 1);
      tex.SetPixel(0, 0, c);

      return tex;
    }

    private bool DoesStarSystemNeedRebuilding()
    {
      return _skyNeedsRebuilding;
    }

    private void BuilderCompletion(SkyBuilder builder, bool successful)
    {
      _builder.completionCallback -= BuilderCompletion;
      _builder = null;
      _skyNeedsRebuilding = false;
    }
  }
}
