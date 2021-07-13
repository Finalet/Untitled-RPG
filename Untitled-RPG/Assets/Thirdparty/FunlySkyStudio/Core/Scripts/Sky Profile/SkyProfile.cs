using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
  /**
   * The Sky Profile manages all properties for a sky, and the keyframe values
   * for any skybox animations or transitions.
   **/
  [CreateAssetMenu(fileName = "skyProfile.asset", menuName = "Sky Studio/Sky Profile", order = 0)]
  public class SkyProfile : ScriptableObject
  {
    public const string DefaultShaderName = "Funly/Sky Studio/Skybox/3D Standard";
    
    // Reference to material paired with this profile for enabling shader features.
    [SerializeField]
    private Material m_SkyboxMaterial;
    public Material skyboxMaterial
    {
      get { return m_SkyboxMaterial; }
      set
      {
        if (value == null)
        {
          m_SkyboxMaterial = null;
          return;
        }
        
        if (m_SkyboxMaterial && m_SkyboxMaterial.shader.name != value.shader.name)
        {
          m_SkyboxMaterial = value;
          m_ShaderName = value.shader.name;
          ReloadDefinitions();
        }
        else
        {
          m_SkyboxMaterial = value;
        }
      }
    }

    // We cache the last valid shader name so we can create sky systems from sky profiles during the setup wizard.
    [SerializeField]
    private string m_ShaderName = DefaultShaderName;
    public string shaderName { get { return m_ShaderName; } }

    // The profile defintion and groups are data driven from this data stucture loaded.
    public IProfileDefinition profileDefinition;

    // List determines what shows up on the timeline editor, or is a single value.
    public List<string> timelineManagedKeys = new List<string>();

    // Groups of keyframes for sky properties.
    public KeyframeGroupDictionary keyframeGroups = new KeyframeGroupDictionary();

    // Fature status flags for Sky Studio (sun, moon, rain, clouds, etc.)
    public BoolDictionary featureStatus = new BoolDictionary();

    // Lighting art customization object.
    public LightningArtSet lightningArtSet;

    // Rain splash art customization object.
    public RainSplashArtSet rainSplashArtSet;

    // Star layer data textures.
    public Texture2D starLayer1DataTexture;
    public Texture2D starLayer2DataTexture;
    public Texture2D starLayer3DataTexture;

    [SerializeField]
#pragma warning disable
    private int m_ProfileVersion = 2;
#pragma warning restore

    // Keep a mapping of key to definition for fast lookups.
    private Dictionary<string, ProfileGroupDefinition> m_KeyToGroupInfo;
    
    // Definitions.
    public ProfileGroupSection[] groupDefinitions
    {
      get { return profileDefinition != null ? profileDefinition.groups : null; }
    }

    public ProfileFeatureSection[] featureDefinitions
    {
      get { return profileDefinition != null ? profileDefinition.features : null; }
    }

    // Access a numeric value from the sky profile (cloud density, horizon position, star size etc.).
    public float GetNumberPropertyValue(string propertyKey)
    {
      return GetNumberPropertyValue(propertyKey, 0);
    }

    // Access a numeric value from the sky profile (cloud density, horizon position, star size, etc.).
    public float GetNumberPropertyValue(string propertyKey, float timeOfDay)
    {
      NumberKeyframeGroup group = GetGroup<NumberKeyframeGroup>(propertyKey);
      if (group == null) {
        Debug.LogError("Can't find number group with property key: " + propertyKey);
        return -1;
      }

      return group.NumericValueAtTime(timeOfDay);
    }

    // Access a color value from the sky profile (sky gradient, sun color, etc.)
    public Color GetColorPropertyValue(string propertyKey)
    {
      return GetColorPropertyValue(propertyKey, 0);
    } 

    public Color GetColorPropertyValue(string propertyKey, float timeOfDay)
    {
      ColorKeyframeGroup group = GetGroup<ColorKeyframeGroup>(propertyKey);
      if (group == null) {
        Debug.LogError("Can't find color group with property key: " + propertyKey);
        return Color.white;
      }

      return group.ColorForTime(timeOfDay);
    }

    // Access a texture value from the sky profile (sun texture, star texture, etc.)
    public Texture GetTexturePropertyValue(string propertyKey)
    {
      return GetTexturePropertyValue(propertyKey, 0);
    }

    public Texture GetTexturePropertyValue(string propertyKey, float timeOfDay)
    {
      TextureKeyframeGroup group = GetGroup<TextureKeyframeGroup>(propertyKey);
      if (group == null) {
        Debug.LogError("Can't find texture group with property key: " + propertyKey);
        return null;
      }

      return group.TextureForTime(timeOfDay);
    }

    // Access a spherical position on the skybox (sun position, moon position, etc.)
    public SpherePoint GetSpherePointPropertyValue(string propertyKey)
    {
      return GetSpherePointPropertyValue(propertyKey, 0);
    }

    public SpherePoint GetSpherePointPropertyValue(string propertyKey, float timeOfDay)
    {
      SpherePointKeyframeGroup group = GetGroup<SpherePointKeyframeGroup>(propertyKey);
      if (group == null) {
        Debug.LogError("Can't find a sphere point group with property key: " + propertyKey);
        return null;
      }

      return group.SpherePointForTime(timeOfDay);
    }

    // Access a boolean property value in the sky profile (fog enabled, etc.)
    public bool GetBoolPropertyValue(string propertyKey)
    {
      return GetBoolPropertyValue(propertyKey, 0);
    }

    public bool GetBoolPropertyValue(string propertyKey, float timeOfDay)
    {
      BoolKeyframeGroup group = GetGroup<BoolKeyframeGroup>(propertyKey);
      if (group == null) {
        Debug.LogError("Can't find boolean group with property key: " + propertyKey);
        return false;
      }

      return group.BoolForTime(timeOfDay);
    }

    public SkyProfile()
    {
      // Build the profile definition table.
      ReloadFullProfile();
    }

    private void OnEnable()
    {
      ReloadFullProfile();
    }

    private void ReloadFullProfile()
    {
      ReloadDefinitions();
      MergeProfileWithDefinitions();
      RebuildKeyToGroupInfoMapping();
      ValidateTimelineGroupKeys();
    }

    private void ReloadDefinitions()
    {
      profileDefinition = GetShaderInfoForMaterial(m_ShaderName);
    }

    private IProfileDefinition GetShaderInfoForMaterial(string shaderName)
    {
      // We currently only support 1 shader.
      return new Standard3dShaderDefinition();
    }

    public void MergeProfileWithDefinitions()
    {
      MergeGroupsWithDefinitions();
      MergeShaderKeywordsWithDefinitions();
    }

    public void MergeGroupsWithDefinitions()
    {
      HashSet<string> validProperties = ProfilePropertyKeys.GetPropertyKeysSet();

      // Build our groups from the profile definition table.
      foreach (ProfileGroupSection section in groupDefinitions)
      {
        foreach (ProfileGroupDefinition groupInfo in section.groups)
        {
          // Filter out old groups that are no longer valid.
          if (!validProperties.Contains(groupInfo.propertyKey))
          {
            continue;
          }

          if (groupInfo.type == ProfileGroupDefinition.GroupType.Color) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false)
            {
              AddColorGroup(groupInfo.propertyKey, groupInfo.groupName, groupInfo.color);
            }
            else
            {
              keyframeGroups[groupInfo.propertyKey].name = groupInfo.groupName;
            }
          } else if (groupInfo.type == ProfileGroupDefinition.GroupType.Number) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false)
            {
              AddNumericGroup(groupInfo.propertyKey, groupInfo.groupName,
                groupInfo.minimumValue, groupInfo.maximumValue, groupInfo.value);
            }
            else
            {
              NumberKeyframeGroup numberGroup = keyframeGroups.GetGroup<NumberKeyframeGroup>(groupInfo.propertyKey);
              numberGroup.name = groupInfo.groupName;
              numberGroup.minValue = groupInfo.minimumValue;
              numberGroup.maxValue = groupInfo.maximumValue;
            }
          } else if (groupInfo.type == ProfileGroupDefinition.GroupType.Texture) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false)
            {
              AddTextureGroup(groupInfo.propertyKey, groupInfo.groupName, groupInfo.texture);
            }
            else
            {
              keyframeGroups[groupInfo.propertyKey].name = groupInfo.groupName;
            }
          } else if (groupInfo.type == ProfileGroupDefinition.GroupType.SpherePoint) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false) {
              AddSpherePointGroup(groupInfo.propertyKey, groupInfo.groupName, groupInfo.spherePoint);
            } else
            {
              keyframeGroups[groupInfo.propertyKey].name = groupInfo.groupName;
            }
          } else if (groupInfo.type == ProfileGroupDefinition.GroupType.Boolean) {
            if (keyframeGroups.ContainsKey(groupInfo.propertyKey) == false) {
              AddBooleanGroup(groupInfo.propertyKey, groupInfo.groupName, groupInfo.boolValue);
            } else {
              keyframeGroups[groupInfo.propertyKey].name = groupInfo.groupName;
            }
          }
        }
      }
    }

    public Dictionary<string, ProfileGroupDefinition> GroupDefinitionDictionary() {
      ProfileGroupSection[] sections = ProfileDefinitionTable();

      Dictionary<string, ProfileGroupDefinition> dict = new Dictionary<string, ProfileGroupDefinition>();

      foreach (ProfileGroupSection sectionInfo in sections) {
        foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups) {
          dict.Add(groupInfo.propertyKey, groupInfo);
        }
      }

      return dict;
    }

    public ProfileGroupSection[] ProfileDefinitionTable()
    {
      return groupDefinitions;
    }

    private void AddNumericGroup(string propKey, string groupName, float min, float max, float value)
    {
      NumberKeyframeGroup group = new NumberKeyframeGroup(
        groupName, min, max, new NumberKeyframe(0, value));
      keyframeGroups[propKey] = group;
    }
    
    private void AddColorGroup(string propKey, string groupName, Color color)
    {
      ColorKeyframeGroup group = new ColorKeyframeGroup(
        groupName, new ColorKeyframe(color, 0));

      keyframeGroups[propKey] = group;
    }

    private void AddTextureGroup(string propKey, string groupName, Texture2D texture)
    {
      TextureKeyframeGroup group = new TextureKeyframeGroup(
        groupName, new TextureKeyframe(texture, 0));

      keyframeGroups[propKey] = group;
    }

    private void AddSpherePointGroup(string propKey, string groupName, SpherePoint point)
    {
      SpherePointKeyframeGroup group = new SpherePointKeyframeGroup(groupName, new SpherePointKeyframe(point, 0));

      keyframeGroups[propKey] = group;
    }

    private void AddBooleanGroup(string propKey, string groupName, bool value)
    {
      BoolKeyframeGroup group = new BoolKeyframeGroup(groupName, new BoolKeyframe(0, value));

      keyframeGroups[propKey] = group;
    }

    public T GetGroup<T>(string propertyKey) where T : class
    {
      if (!keyframeGroups.ContainsKey(propertyKey)) {
        Debug.Log("Key does not exist in sky profile, ignoring: " + propertyKey);
        return null;
      }
      return keyframeGroups[propertyKey] as T;
    }

    public IKeyframeGroup GetGroup(string propertyKey)
    {
      return keyframeGroups[propertyKey];
    }

    public IKeyframeGroup GetGroupWithId(string groupId)
    {
      if (groupId == null) {
        return null;
      }

      foreach (string key in keyframeGroups)
      {
        IKeyframeGroup group = keyframeGroups[key];
        if (group.id == groupId) {
          return group;
        }
      }
      return null;
    }

    // This returns the groups that exist in the profile for easy iteration.
    public ProfileGroupSection[] GetProfileDefinitions()
    {
      return groupDefinitions;
    }

    public ProfileGroupSection GetSectionInfo(string sectionKey)
    {
      foreach (ProfileGroupSection section in groupDefinitions)
      {
        if (section.sectionKey == sectionKey)
        {
          return section;
        }
      }
      return null;
    }

    // Check if a group is managed by the timeline.
    public bool IsManagedByTimeline(string propertyKey)
    {
      return timelineManagedKeys.Contains(propertyKey);
    }

    public void ValidateTimelineGroupKeys()
    {
      List<string> removeKeys = new List<string>();

      HashSet<string> validProperties = ProfilePropertyKeys.GetPropertyKeysSet();

      foreach (string timelineKey in timelineManagedKeys)
      {
        if (!IsManagedByTimeline(timelineKey) || !validProperties.Contains(timelineKey))
        {
          removeKeys.Add(timelineKey);
        }
      }

      foreach (string removeKey in removeKeys)
      {
        if (timelineManagedKeys.Contains(removeKey))
        {
          timelineManagedKeys.Remove(removeKey);
        }
      }
    }

    public List<ProfileGroupDefinition> GetGroupDefinitionsManagedByTimeline() {
      List<ProfileGroupDefinition> groups = new List<ProfileGroupDefinition>();

      foreach (string groupKey in timelineManagedKeys)
      {
        ProfileGroupDefinition groupDefinition = GetGroupDefinitionForKey(groupKey);
        if (groupDefinition == null)
        {
          continue;
        }

        groups.Add(groupDefinition);
      }

      return groups;
    }

    public List<ProfileGroupDefinition> GetGroupDefinitionsNotManagedByTimeline()
    {
      List<ProfileGroupDefinition> groups = new List<ProfileGroupDefinition>();

      foreach (ProfileGroupSection sectionInfo in groupDefinitions)
      {
        foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups)
        {
          if (IsManagedByTimeline(groupInfo.propertyKey) == false && CanGroupBeOnTimeline(groupInfo))
          {
            groups.Add(groupInfo);
          }
        }
      }

      return groups;
    }

    public ProfileGroupDefinition GetGroupDefinitionForKey(string propertyKey)
    {
      ProfileGroupDefinition def = null;
      if (m_KeyToGroupInfo.TryGetValue(propertyKey, out def))
      {
        return def;
      }

      return null;
    }

    public void RebuildKeyToGroupInfoMapping()
    {
      m_KeyToGroupInfo = new Dictionary<string, ProfileGroupDefinition>();

      foreach (ProfileGroupSection sectionInfo in groupDefinitions) {
        foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups)
        {
          m_KeyToGroupInfo[groupInfo.propertyKey] = groupInfo;
        }
      }
    }

    public void TrimGroupToSingleKeyframe(string propertyKey)
    {
      IKeyframeGroup group = GetGroup(propertyKey);
      if (group == null)
      {
        return;
      }

      group.TrimToSingleKeyframe();
    }

    // Blacklist some groups from being on the timeline.
    public bool CanGroupBeOnTimeline(ProfileGroupDefinition definition)
    {
      if (definition.type == ProfileGroupDefinition.GroupType.Texture ||
         (definition.propertyKey.Contains("Star") && definition.propertyKey.Contains("Density")) || 
         definition.propertyKey.Contains("Sprite") ||
         definition.type == ProfileGroupDefinition.GroupType.Boolean) {
        return false;
      }
      else
      {
        return true;
      }
    }

    protected void MergeShaderKeywordsWithDefinitions()
    {
      foreach (ProfileFeatureSection section in profileDefinition.features)
      {
        foreach (ProfileFeatureDefinition definition in section.featureDefinitions)
        {
          string featureKey = null;
          bool featureValue = false;
          if (definition.featureType == ProfileFeatureDefinition.FeatureType.BooleanValue ||
              definition.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeyword) {
            featureKey = definition.featureKey;
            featureValue = definition.value;
          } else if (definition.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeywordDropdown) {
            featureKey = definition.featureKeys[definition.dropdownSelectedIndex];
            featureValue = true;
          }

          if (featureKey == null) {
            continue;
          }

          if (featureStatus.dict.ContainsKey(featureKey) == false) {
            SetFeatureEnabled(featureKey, featureValue);
          }
        }
      }  
    }

    public bool IsFeatureEnabled(string featureKey, bool recursive = true)
    {
      if (featureKey == null)
      {
        return false;
      }

      // Load the definition for this feature so we can check for dependent features.
      ProfileFeatureDefinition feature = profileDefinition.GetFeatureDefinition(featureKey);
      if (feature == null) {
        return false;
      }
      
      if (featureStatus.dict.ContainsKey(featureKey) == false || featureStatus[featureKey] == false) {
        return false;
      }

      // Don't scan up the parent hiearchy any further.
      if (recursive == false) {
        return true;
      }

      // Check the full dependency chain to check if this feature is disabled by a parent.
      ProfileFeatureDefinition childFeature = feature;
      ProfileFeatureDefinition parentFeature;
      while (childFeature != null) {
        parentFeature = profileDefinition.GetFeatureDefinition(childFeature.dependsOnFeature);
        if (parentFeature == null || parentFeature.featureKey == null) {
          break;
        }

        if (featureStatus[parentFeature.featureKey] != childFeature.dependsOnValue) {
          return false;
        }

        childFeature = parentFeature;
      }

      return true;
    }

    public void SetFeatureEnabled(string featureKey, bool value)
    {
      if (featureKey == null) {
        Debug.LogError("Can't set null feature key value");
        return;
      }

      featureStatus[featureKey] = value;
    }
  }
}

