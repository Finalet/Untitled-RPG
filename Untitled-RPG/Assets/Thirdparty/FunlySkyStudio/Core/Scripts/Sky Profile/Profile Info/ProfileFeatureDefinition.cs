using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  [Serializable]
  public class ProfileFeatureDefinition : System.Object
  {
    public enum FeatureType
    {
      ShaderKeyword,
      BooleanValue,
      ShaderKeywordDropdown,
    }

    public string featureKey;
    public string[] featureKeys;
    public FeatureType featureType;
    public string shaderKeyword;
    public string[] shaderKeywords;
    public string[] dropdownLabels;
    public int dropdownSelectedIndex;
    public string name;
    public bool value;
    public string tooltip;
    public string dependsOnFeature;
    public bool dependsOnValue;
    public bool isShaderKeywordFeature;

    // Feature that uses a shader keyword.
    public static ProfileFeatureDefinition CreateShaderFeature(
      string featureKey, string shaderKeyword, bool value, string name, 
      string dependsOnFeature, bool dependsOnValue, string tooltip)
    {
      ProfileFeatureDefinition feature = new ProfileFeatureDefinition();
      feature.featureType = FeatureType.ShaderKeyword;
      feature.featureKey = featureKey;
      feature.shaderKeyword = shaderKeyword;
      feature.name = name;
      feature.value = value;
      feature.tooltip = tooltip;
      feature.dependsOnFeature = dependsOnFeature;
      feature.dependsOnValue = dependsOnValue;

      return feature;
    }

    // Dropdown to select a mutually exclusive shader feature.
    public static ProfileFeatureDefinition CreateShaderFeatureDropdown(
      string[] featureKeys, string[] shaderKeywords, string[] labels, int selectedIndex, string name,
      string dependsOnFeature, bool dependsOnValue, string tooltip)
    {
      ProfileFeatureDefinition feature = new ProfileFeatureDefinition();
      feature.featureType = FeatureType.ShaderKeywordDropdown;
      feature.featureKeys = featureKeys;
      feature.shaderKeywords = shaderKeywords;
      feature.dropdownLabels = labels;
      feature.name = name;
      feature.dropdownSelectedIndex = selectedIndex;
      feature.tooltip = tooltip;
      feature.dependsOnFeature = dependsOnFeature;
      feature.dependsOnValue = dependsOnValue;

      return feature;
    }

    // Feature that's just a boolean flag.
    public static ProfileFeatureDefinition CreateBooleanFeature(
      string featureKey, bool value, string name,
      string dependsOnFeature, bool dependsOnValue, string tooltip)
    {
      ProfileFeatureDefinition feature = new ProfileFeatureDefinition();
      feature.featureType = FeatureType.BooleanValue;
      feature.featureKey = featureKey;
      feature.name = name;
      feature.value = value;
      feature.tooltip = tooltip;

      return feature;
    }
  }
}

