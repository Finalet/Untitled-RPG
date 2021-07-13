using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Funly.SkyStudio
{
  [CustomEditor(typeof(SkyProfile))]
  public class SkyProfileEditor : Editor
  {
    private static string SHADER_NAME_PREFIX = "Funly/Sky Studio/Skybox";

    private SkyProfile m_Profile;
    private SkyBuilder m_Builder;
    private Texture2D m_SectionHeaderBg;
    private Dictionary<string, ProfileFeatureSection> m_Sections;
    private const float k_IconSize = 20.0f;
    private const int k_TitleSize = 12;
    private const int k_HeaderHeight = 20;
    private string m_SpherePointSelectionToken;

    // The setup window will set this value to force the first load rebuild.
    public static int forceRebuildProfileId;

    private void OnEnable()
    {
      serializedObject.Update();

      m_Profile = (SkyProfile)target;
      m_SectionHeaderBg = CreateColorImage(SectionColorForEditorSkin());

      // Make the sure the profile's features are in sync with shader material.
      ApplyKeywordsToMaterial();
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      m_Profile = (SkyProfile)target;

      // For new profiles we'll automatically build them.
      if (forceRebuildProfileId != -1 && forceRebuildProfileId == m_Profile.GetInstanceID()) {
        RebuildSkySystem();
        forceRebuildProfileId = -1;
      }

      if (RenderSkyboxMaterial() == false) {
        serializedObject.ApplyModifiedProperties();
        return;
      }

      if (m_Sections == null) {
        m_Sections = new Dictionary<string, ProfileFeatureSection>();
      }

      foreach (ProfileFeatureSection section in m_Profile.featureDefinitions) {
        m_Sections[section.sectionKey] = section;
      }

      bool didChangeProfile = false;

      // Features.
      if (RenderFeatureSection()) {
        didChangeProfile = true;
      }

      // Timeline.
      if (RenderTimelineList()) {
        didChangeProfile = true;
      }
        
      // Properties.
      if (RenderProfileDefinitions()) {
        didChangeProfile = true;
      }

      TimeOfDayController tc = GameObject.FindObjectOfType<TimeOfDayController>();
      if (tc != null) {
        tc.UpdateSkyForCurrentTime();
      }

      serializedObject.ApplyModifiedProperties();

      if (didChangeProfile) {
        EditorUtility.SetDirty(m_Profile);
      }
    }

    private Color SectionColorForEditorSkin()
    {
      if (EditorGUIUtility.isProSkin)
      {
        float gray = 190.0f / 225.0f;
        return new Color(gray, gray, gray, 1.0f);
      }
      else
      {
        float gray = 222.0f / 225.0f;
        return new Color(gray, gray, gray, 1.0f);
      }
    }

    private bool RenderProfileDefinitions()
    {
      bool didChangeProfile = false;

      foreach (ProfileGroupSection groupSection in m_Profile.groupDefinitions)
      {
        if (groupSection.dependsOnFeature != null &&
            groupSection.dependsOnValue != m_Profile.IsFeatureEnabled(groupSection.dependsOnFeature)) {
          continue;
        }

        bool valueChanged = RenderSection(groupSection.sectionKey);
        if (valueChanged) {
          didChangeProfile = true;
        }
      }

      return didChangeProfile;
    }

    private bool RenderSkyboxMaterial()
    {
      EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SkyboxMaterial"));
      
      if (SkyboxMaterial() == null)
      {
        EditorGUILayout.HelpBox("You need to assign a Funly Sky Studio skybox material" +
                                " before you can edit the sky profile.", MessageType.Info);
        return false;
      }

      if (SkyboxMaterial().shader.name.Contains(SHADER_NAME_PREFIX) == false)
      {
        EditorGUILayout.HelpBox("Skybox material has an unsupported shader. You need to use a shader" +
                                " in the Funly/Sky/ directory.", MessageType.Error);
        return false;
      }

      return true;
    }

    private void RebuildSkySystem()
    {
      if (m_Builder != null)
      {
        m_Builder.CancelBuild();
        m_Builder = null;
      }

      m_Builder = CreateSkyBuilder();

      if (m_Builder.IsComplete == false) {
        return;
      }

      m_Builder.BuildSkySystem();
    }

    private bool RenderFeatureSection()
    {
      RenderSectionTitle("Features", "FeatureSectionIcon");

      bool didChangeProfile = false;

      string[] rebuildSkyKeywords = new string[]
      {
        ShaderKeywords.StarLayer1,
        ShaderKeywords.StarLayer2,
        ShaderKeywords.StarLayer3
      };

      if (m_Sections.ContainsKey(ProfileSectionKeys.FeaturesSectionKey) == false)
      {
        Debug.LogError("Shader definition is missing a features dictionary");
        return didChangeProfile;
      }

      ProfileFeatureSection section = m_Sections[ProfileSectionKeys.FeaturesSectionKey];
      foreach (ProfileFeatureDefinition def in section.featureDefinitions)
      {
        if (def.dependsOnFeature != null) {
          if (m_Profile.IsFeatureEnabled(def.dependsOnFeature) != def.dependsOnValue) {
            m_Profile.SetFeatureEnabled(def.featureKey, false);
            didChangeProfile = true;
            continue;
          } 
        }

        bool valueChanged;
        RenderFeatureCheckbox(
          def,
          m_Profile.IsFeatureEnabled(def.featureKey),
          out valueChanged);

        if (valueChanged && rebuildSkyKeywords.Contains(def.shaderKeyword)) {
          RebuildSkySystem();
        }

        if (valueChanged) {
          didChangeProfile = true;
        }
      }

      return didChangeProfile;
    }

    private bool RenderFeatureCheckbox(ProfileFeatureDefinition def, bool keywordValue, out bool valueChanged)
    {
      EditorGUI.BeginChangeCheck();
      bool value = EditorGUILayout.Toggle(def.name, keywordValue);
      if (EditorGUI.EndChangeCheck())
      {
        // FIXME - Why doesn't the sky profile do this for us when we enable the feature?
        if (def.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeyword) {
          SetShaderKeyword(def.shaderKeyword, value);
        }
        m_Profile.SetFeatureEnabled(def.featureKey, value);
        valueChanged = true;
      } else {
        valueChanged = false;
      }

      return value;
    }

    private Material SkyboxMaterial()
    {
      if (serializedObject.FindProperty("m_SkyboxMaterial") == null)
      {
        return null;
      }
      return serializedObject.FindProperty("m_SkyboxMaterial").objectReferenceValue as Material;
    }

    private void ApplyKeywordsToMaterial() {
      if (SkyboxMaterial() == null) {
        return;
      }

      ApplyKeywordsToMaterial(m_Profile, SkyboxMaterial());
    }

    public static void ApplyKeywordsToMaterial(SkyProfile profile, Material skyboxMaterial)
    {
      foreach (ProfileFeatureSection section in profile.featureDefinitions)
      {
        foreach (ProfileFeatureDefinition definition in section.featureDefinitions)
        {
          SetShaderKeyword(
            definition.shaderKeyword,
            profile.IsFeatureEnabled(definition.featureKey),
            skyboxMaterial);
        }
      }
    }

    private void SetShaderKeyword(string keyword, bool value)
    {
      SetShaderKeyword(keyword, value, SkyboxMaterial());
    }

    private static void SetShaderKeyword(string keyword, bool value, Material skyboxMaterial)
    {      
      if (value)
      {
        skyboxMaterial.EnableKeyword(keyword);
      }
      else
      {
        skyboxMaterial.DisableKeyword(keyword);
      }
    }

    private bool RenderTimelineList()
    {
      bool didChangeProfile = false;

      RenderSectionTitle("Timeline Animated Properties", "TimelineSectionIcon");

      EditorGUILayout.Space();

      List<ProfileGroupDefinition> onTimeline = m_Profile.GetGroupDefinitionsManagedByTimeline();
      List<ProfileGroupDefinition> offTimeline = m_Profile.GetGroupDefinitionsNotManagedByTimeline();

      int deleteIndex = -1;
      bool didSwapRows = false;
      int swapIndex1 = -1;
      int swapIndex2 = -1;

      if (onTimeline.Count == 0)
      {
        // Show definition message if no items added yet.
        EditorGUILayout.HelpBox("You can animate properties by adding them to the timeline.", MessageType.None);
      }
      else
      {
        EditorGUI.BeginChangeCheck();
        List<string> timelineTitles = GetTitlesForGroups(onTimeline);

        StringTableListGUI.RenderTableList(
          timelineTitles,
          out deleteIndex,
          out didSwapRows,
          out swapIndex1,
          out swapIndex2);

        // Check for table modification events (remove, reorder, etc.)
        if (EditorGUI.EndChangeCheck()) {
          didChangeProfile = true;
          if (deleteIndex != -1) {
            string deleteGroupKey = onTimeline[deleteIndex].propertyKey;

            IKeyframeGroup group = m_Profile.GetGroup(deleteGroupKey);
            if (SkyEditorUtility.IsGroupSelectedOnTimeline(group.id)) {
              TimelineSelection.Clear();

              // If we deleted a sphere point group make sure to hide the debug dots.
              if (group is SpherePointKeyframeGroup && m_Profile.skyboxMaterial != null) {
                m_Profile.skyboxMaterial.DisableKeyword(ShaderKeywords.RenderDebugPoints);
              }
            }

            m_Profile.timelineManagedKeys.Remove(deleteGroupKey);
            m_Profile.TrimGroupToSingleKeyframe(deleteGroupKey);
          } else if (didSwapRows) {
            string tmp = m_Profile.timelineManagedKeys[swapIndex2];
            m_Profile.timelineManagedKeys[swapIndex2] = m_Profile.timelineManagedKeys[swapIndex1];
            m_Profile.timelineManagedKeys[swapIndex1] = tmp;
          }
        }
      }

      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (GUILayout.Button(new GUIContent("Open Timeline"))) {
        SkyTimelineWindow.ShowWindow();
      }

      if (GUILayout.Button(new GUIContent("Add to Timeline"))) {
        SkyGUITimelineMenu.ShowAddTimelinePropertyMenu(m_Profile, offTimeline);
      }
      EditorGUILayout.EndHorizontal();

      return didChangeProfile;
    }

    // Render all properties for a section.
    public bool RenderSection(string sectionKey, params string[] ignoreGroups)
    {
      ProfileGroupSection sectionInfo = m_Profile.GetSectionInfo(sectionKey);
      RenderSectionTitle(sectionInfo.sectionTitle, sectionInfo.sectionIcon);
      bool didChangeProfile = false;

      // Render all the feature checkboxes for the section.
      if (m_Sections.ContainsKey(sectionKey)) {
        ProfileFeatureSection keywordSection = m_Sections[sectionKey];

        foreach (ProfileFeatureDefinition def in keywordSection.featureDefinitions)
        {
          // Check for keyword dependencies.
          if (def.dependsOnFeature != null) {
            if (m_Profile.IsFeatureEnabled(def.dependsOnFeature) != def.dependsOnValue) {
              continue;
            }
          }

          // Render the feature UI.
          bool valueChanged = false;
          if (def.featureType == ProfileFeatureDefinition.FeatureType.BooleanValue ||
              def.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeyword) {
            RenderFeatureCheckbox(
              def,
              m_Profile.IsFeatureEnabled(def.featureKey),
              out valueChanged);
          } else if (def.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeywordDropdown) {
            RenderDropdownShaderFeature(
              def,
              out valueChanged);
          }

          if (valueChanged) {
            didChangeProfile = true;
          }
        }
      }

      // Render all the property groups for this section.
      foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups)
      {
        bool shouldIgnore = false;
        foreach (string ignoreName in ignoreGroups)
        {
          if (groupInfo.groupName.Contains(ignoreName))
          {
            shouldIgnore = true;
            break;
          }
        }

        if (shouldIgnore)
        {
          continue;
        }

        bool valueChanged = RenderProfileGroup(groupInfo);
        if (valueChanged) {
          didChangeProfile = true;
        }
      }

      // Render any section specific non-timeline properties.
      if (sectionKey == ProfileSectionKeys.LightningSectionKey) {
        EditorGUILayout.ObjectField(serializedObject.FindProperty("lightningArtSet"));
      } else if (sectionKey == ProfileSectionKeys.RainSplashSectionKey) {
        EditorGUILayout.ObjectField(serializedObject.FindProperty("rainSplashArtSet"));
      }

      return didChangeProfile;
    }

    private void RenderDropdownShaderFeature(ProfileFeatureDefinition def, out bool valueChanged)
    {
      valueChanged = false;

      int currentIndex = def.dropdownSelectedIndex;

      // State is maintained in the feature flags, so find the one that's enabled.
      for (int i = 0; i < def.featureKeys.Length; i++) {
        string feature = def.featureKeys[i];
        if (m_Profile.IsFeatureEnabled(feature)) {
          currentIndex = i;
          break;
        }
      }

      EditorGUI.BeginChangeCheck();
      int selectedIndex = EditorGUILayout.Popup(def.name, currentIndex, def.dropdownLabels);
      if (EditorGUI.EndChangeCheck()) {
        valueChanged = true;
        
        // Clear and set new shader keyword for this dropdown.
        SetShaderKeywordAndFeatureFlag(def.shaderKeywords[currentIndex], def.featureKeys[currentIndex], false);
        SetShaderKeywordAndFeatureFlag(def.shaderKeywords[selectedIndex], def.featureKeys[selectedIndex], true);
      }
    }

    public void SetShaderKeywordAndFeatureFlag(string shaderKeyword, string featureKey, bool value)
    {
      SetShaderKeyword(shaderKeyword, value);
      m_Profile.SetFeatureEnabled(featureKey, value);
    }

    // Render all properties in a group.
    public bool RenderProfileGroup(ProfileGroupDefinition groupDefinition)
    {
      if (groupDefinition.dependsOnFeature != null &&
          m_Profile.IsFeatureEnabled(groupDefinition.dependsOnFeature) != groupDefinition.dependsOnValue)
      {
        return false;
      }

      bool valueChanged = false;
      if (groupDefinition.type == ProfileGroupDefinition.GroupType.Color)
      {
        valueChanged = RenderColorGroupProperty(groupDefinition);
      } else if (groupDefinition.type == ProfileGroupDefinition.GroupType.Number)
      {
        valueChanged = RenderNumericGroupProperty(groupDefinition);
      } else if (groupDefinition.type == ProfileGroupDefinition.GroupType.Texture)
      {
        valueChanged = RenderTextureGroupProperty(groupDefinition);
      } else if (groupDefinition.type == ProfileGroupDefinition.GroupType.SpherePoint)
      {
        valueChanged = RenderSpherePointPropertyGroup(groupDefinition);
      } else if (groupDefinition.type == ProfileGroupDefinition.GroupType.Boolean) {
        valueChanged = RenderBooleanPropertyGroup(groupDefinition);
      }

      // Check if this property needs to rebuild the sky.
      if (valueChanged && groupDefinition.rebuildType == ProfileGroupDefinition.RebuildType.Stars)
      {
        RebuildSkySystem();
      }

      return valueChanged;
    }

    // Render color property.
    public bool RenderColorGroupProperty(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();

      ColorKeyframeGroup group = m_Profile.GetGroup<ColorKeyframeGroup>(def.propertyKey);
      EditorGUILayout.PrefixLabel(new GUIContent(group.name, def.tooltip));
      bool valueChanged = false;

      if (m_Profile.IsManagedByTimeline(def.propertyKey))
      {
        RenderManagedOnTimlineMessage();
      }
      else
      {
        ColorKeyframe frame = group.GetKeyframe(0);

        EditorGUI.BeginChangeCheck();
        Color selectedColor = EditorGUILayout.ColorField(frame.color);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(m_Profile, "Changed color keyframe value");
          frame.color = selectedColor;
          valueChanged = true;
        }
      }

      EditorGUILayout.EndHorizontal();
      return valueChanged;
    }

    // Render numeric properties with a slider.
    public bool RenderNumericGroupProperty(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();
      NumberKeyframeGroup group = m_Profile.GetGroup<NumberKeyframeGroup>(def.propertyKey);
      EditorGUILayout.PrefixLabel(new GUIContent(group.name, def.tooltip));
      bool valueChanged = false;

      if (m_Profile.IsManagedByTimeline(def.propertyKey))
      {
        RenderManagedOnTimlineMessage();
      }
      else
      {
        NumberKeyframe frame = group.GetKeyframe(0);

        if (def.formatStyle == ProfileGroupDefinition.FormatStyle.Integer)
        {
          EditorGUI.BeginChangeCheck();
          int value = EditorGUILayout.IntField((int) frame.value);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(m_Profile, "Changed int keyframe value");
            frame.value = (int) Mathf.Clamp(value, group.minValue, group.maxValue);
            valueChanged = true;
          }
        }
        else
        {
          EditorGUI.BeginChangeCheck();
          float value = EditorGUILayout.Slider(frame.value, group.minValue, group.maxValue);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(m_Profile, "Changed float keyframe value");
            frame.value = value;
            valueChanged = true;
          }
        }
      }

      EditorGUILayout.EndHorizontal();

      return valueChanged;
    }

    // Render texture property.
    public bool RenderTextureGroupProperty(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();

      TextureKeyframeGroup group = m_Profile.GetGroup<TextureKeyframeGroup>(def.propertyKey);
      EditorGUILayout.PrefixLabel(new GUIContent(group.name, def.tooltip));
      bool valueChanged = false;

      if (m_Profile.IsManagedByTimeline(def.propertyKey))
      {
        RenderManagedOnTimlineMessage();
      }
      else
      {
        TextureKeyframe frame = group.GetKeyframe(0);
        EditorGUI.BeginChangeCheck();
        Texture assignedTexture = (Texture) EditorGUILayout.ObjectField(frame.texture, typeof(Texture), true);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(m_Profile, "Changed texture keyframe value");
          frame.texture = assignedTexture;
          valueChanged = true;
        }
      }

      EditorGUILayout.EndHorizontal();
      return valueChanged;
    }

    private bool RenderBooleanPropertyGroup(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();

      BoolKeyframeGroup group = m_Profile.GetGroup<BoolKeyframeGroup>(def.propertyKey);
      EditorGUILayout.PrefixLabel(new GUIContent(group.name, def.tooltip));
      bool valueChanged = false;

      if (m_Profile.IsManagedByTimeline(def.propertyKey)) {
        RenderManagedOnTimlineMessage();
      } else {
        BoolKeyframe frame = group.GetKeyframe(0);
        EditorGUI.BeginChangeCheck();
        bool assignedValue = EditorGUILayout.Toggle(frame.value);
        if (EditorGUI.EndChangeCheck()) {
          Undo.RecordObject(m_Profile, "Changed bool keyframe value");
          frame.value = assignedValue;
          valueChanged = true;
        }
      }

      EditorGUILayout.EndHorizontal();
      return valueChanged;
    }

    private bool RenderSpherePointPropertyGroup(ProfileGroupDefinition def)
    {
      EditorGUILayout.BeginHorizontal();
      bool valueChanged = false;

      SpherePointKeyframeGroup group = m_Profile.GetGroup<SpherePointKeyframeGroup>(def.propertyKey);

      if (m_Profile.IsManagedByTimeline(def.propertyKey))
      {
        EditorGUILayout.PrefixLabel(new GUIContent(def.groupName, def.tooltip));
        RenderManagedOnTimlineMessage();
      }
      else
      {
        SpherePointKeyframe frame = group.GetKeyframe(0);

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent(group.name, def.tooltip));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        EditorGUI.indentLevel += 1;
        SpherePoint selectedPoint = SpherePointGUI.SpherePointField(
          frame.spherePoint, true, frame.id);
        EditorGUI.indentLevel -= 1;
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(m_Profile, "Changed sphere point");
          frame.spherePoint = selectedPoint;
        }

        EditorGUILayout.EndVertical();
      }

      EditorGUILayout.EndHorizontal();
      return valueChanged;
    }

    private void RenderManagedOnTimlineMessage()
    {
      GUIStyle style = new GUIStyle(GUI.skin.label);
      style.fontStyle = FontStyle.Italic;
      EditorGUILayout.LabelField("Managed on timeline", style);
    }

    private void RenderSectionTitle(string title, string iconName)
    {
      GUIStyle bgStyle = new GUIStyle();
      bgStyle.normal.background = m_SectionHeaderBg;
      bgStyle.margin = new RectOffset(0, 0, 20, 7);
      bgStyle.padding = new RectOffset(0, 0, 0, 0);

      GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
      titleStyle.normal.textColor = Color.black;
      titleStyle.fontStyle = FontStyle.Bold;
      titleStyle.fontSize = k_TitleSize;
      titleStyle.margin = new RectOffset(0, 0, 0, 0);
      titleStyle.padding = new RectOffset(0, 0, 3, 0);

      GUIStyle iconStyle = new GUIStyle();
      iconStyle.margin = new RectOffset(0, 0, 0, 0);
      iconStyle.padding = new RectOffset(0, 0, 0, 0);

      EditorGUILayout.BeginHorizontal(bgStyle, GUILayout.Height(k_HeaderHeight));

      // Default to a placeholder icon if we don't have one.
      string loadIconFile = iconName != null ? iconName : "UnknownSectionIcon";
      Texture icon = SkyEditorUtility.LoadEditorResourceTexture(loadIconFile);

      EditorGUILayout.LabelField(new GUIContent(icon), iconStyle, GUILayout.Width(k_IconSize), GUILayout.Height(k_IconSize));
      EditorGUILayout.LabelField(new GUIContent(title), titleStyle);

      GUILayout.FlexibleSpace();

      EditorGUILayout.EndHorizontal();
    }

    private Texture2D CreateColorImage(Color c)
    {
      Texture2D tex = new Texture2D(1, 1);
      tex.SetPixel(0, 0, c);
      tex.Apply();
    
      return tex;
    }

    private SkyBuilder CreateSkyBuilder()
    {
      SkyBuilder b = new SkyBuilder();
      b.profile = m_Profile;
      b.starLayer1Enabled = m_Profile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer1Feature);
      b.starLayer2Enabled = m_Profile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer2Feature); ;
      b.starLayer3Enabled = m_Profile.IsFeatureEnabled(ProfileFeatureKeys.StarLayer3Feature); ;

      b.starLayer1Density = m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1DensityKey).GetFirstValue();
      b.starLayer2Density = m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2DensityKey).GetFirstValue();
      b.starLayer3Density = m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3DensityKey).GetFirstValue();

      b.starLayer1MaxRadius = GetMaxValueForGroup(m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star1SizeKey));
      b.starLayer2MaxRadius = GetMaxValueForGroup(m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star2SizeKey));
      b.starLayer3MaxRadius = GetMaxValueForGroup(m_Profile.GetGroup<NumberKeyframeGroup>(ProfilePropertyKeys.Star3SizeKey));

      b.skyboxMaterial = m_Profile.skyboxMaterial;
      b.completionCallback += BuilderCompletion;

      return b;
    }

    private float GetMaxValueForGroup(NumberKeyframeGroup group) {
      float maxValue = 0;

      for (int i = 0; i < group.keyframes.Count; i++) {
        if (i == 0 || group.keyframes[i].value > maxValue) {
          maxValue = group.keyframes[i].value;
        }
      }

      return maxValue;
    }

    private void BuilderCompletion(SkyBuilder builder, bool successful)
    {
      m_Builder.completionCallback -= BuilderCompletion;
      m_Builder = null;

      if (m_Profile)
      {
        EditorUtility.SetDirty(m_Profile);
      }

      TimeOfDayController tc = GameObject.FindObjectOfType<TimeOfDayController>();
      if (tc != null)
      {
        tc.UpdateSkyForCurrentTime();
      }
    }

    private List<string> GetTitlesForGroups(List<ProfileGroupDefinition> groups) {
      List<string> titles = new List<string>();

      foreach (ProfileGroupDefinition group in groups) {
        titles.Add(group.groupName);
      }

      return titles;
    }
  }
}



