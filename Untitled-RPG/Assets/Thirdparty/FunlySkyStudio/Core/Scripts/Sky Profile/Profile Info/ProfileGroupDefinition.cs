using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // This class is used as metadata for creating the inital groups.
  public class ProfileGroupDefinition
  {
    public enum GroupType
    {
      None,
      Color,
      Number,
      Texture,
      SpherePoint,
      Boolean
    }

    public enum FormatStyle
    {
      None,
      Integer,
      Float
    }

    public enum RebuildType
    {
      None,
      Stars
    }

    public GroupType type;
    public FormatStyle formatStyle = FormatStyle.None;
    public RebuildType rebuildType;
    public string propertyKey;
    public string groupName;
    public Color color;
    public SpherePoint spherePoint;
    public float minimumValue = -1.0f;
    public float maximumValue = -1.0f;
    public float value = -1.0f;
    public bool boolValue = false;
    public Texture2D texture;
    public string tooltip;
    public string dependsOnFeature;
    public bool dependsOnValue;
    
    public static ProfileGroupDefinition NumberGroupDefinition(
      string groupName, string propKey, float minimumValue, float maximumValue, float value, string tooltip)
    {
      return NumberGroupDefinition(groupName, propKey, minimumValue, maximumValue, 
        value, RebuildType.None, null, false, tooltip);
    }

    public static ProfileGroupDefinition NumberGroupDefinition(
      string groupName, string propKey, float minimumValue, float maximumValue, float value, 
      string dependsOnKeyword, bool dependsOnValue, string tooltip)
    {
      return NumberGroupDefinition(groupName, propKey, minimumValue, maximumValue, value,
        RebuildType.None, FormatStyle.Float, dependsOnKeyword, dependsOnValue, tooltip);
    }

    public static ProfileGroupDefinition NumberGroupDefinition(
      string groupName, string propKey, float minimumValue, float maximumValue, float value,
      RebuildType rebuildType, string dependsOnKeyword, bool dependsOnValue, string tooltip)
    {
      return NumberGroupDefinition(groupName, propKey, minimumValue, maximumValue, value,
        rebuildType, FormatStyle.Float, dependsOnKeyword, dependsOnValue, tooltip);
    }

    public static ProfileGroupDefinition NumberGroupDefinition(
      string groupName, string propKey, float minimumValue, float maximumValue, float value,
      RebuildType rebuildType, FormatStyle formatStyle, string dependsOnKeyword, bool dependsOnValue, string tooltip)
    {
      ProfileGroupDefinition def = new ProfileGroupDefinition();

      def.type = GroupType.Number;
      def.formatStyle = formatStyle;
      def.groupName = groupName;
      def.propertyKey = propKey;
      def.value = value;
      def.minimumValue = minimumValue;
      def.maximumValue = maximumValue;
      def.tooltip = tooltip;
      def.rebuildType = rebuildType;
      def.dependsOnFeature = dependsOnKeyword;
      def.dependsOnValue = dependsOnValue;

      return def;
    }

    // Color group info constructor.
    public static ProfileGroupDefinition ColorGroupDefinition(string groupName, string propKey, Color color, string tooltip)
    {
      return ColorGroupDefinition(groupName, propKey, color, RebuildType.None, null, false, tooltip);
    }

    public static ProfileGroupDefinition ColorGroupDefinition(
     string groupName, string propKey, Color color,
     string dependsOnFeature, bool dependsOnValue, string tooltip)
    {
      return ColorGroupDefinition(groupName, propKey, color, RebuildType.None, dependsOnFeature, dependsOnValue, tooltip);
    }

    public static ProfileGroupDefinition ColorGroupDefinition(
      string groupName, string propKey, Color color, 
      RebuildType rebuildType, string dependsOnKeyword, bool dependsOnValue, string tooltip)
    {
      ProfileGroupDefinition def = new ProfileGroupDefinition();

      def.type = GroupType.Color;
      def.propertyKey = propKey;
      def.groupName = groupName;
      def.color = color;
      def.tooltip = tooltip;
      def.rebuildType = rebuildType;
      def.dependsOnFeature = dependsOnKeyword;
      def.dependsOnValue = dependsOnValue;

      return def;
    }

    public static ProfileGroupDefinition SpherePointGroupDefinition(string groupName, string propKey,
      float horizontalRotation, float verticalRotation, string tooltip)
    {
      return SpherePointGroupDefinition(groupName, propKey, horizontalRotation, verticalRotation, RebuildType.None,
        null, false, tooltip);
    }

    public static ProfileGroupDefinition SpherePointGroupDefinition(string groupName, string propKey, 
      float horizontalRotation, float verticalRotation, RebuildType rebuildType, string dependsOnKeyword, bool dependsOnValue, string tooltip)
    {
      ProfileGroupDefinition def = new ProfileGroupDefinition();

      def.type = GroupType.SpherePoint;
      def.propertyKey = propKey;
      def.groupName = groupName;
      def.tooltip = tooltip;
      def.rebuildType = rebuildType;
      def.dependsOnFeature = dependsOnKeyword;
      def.dependsOnValue = dependsOnValue;
      def.spherePoint = new SpherePoint(horizontalRotation, verticalRotation);

      return def;
    }

    // Texture group info constructor
    public static ProfileGroupDefinition TextureGroupDefinition(
      string groupName, string propKey, Texture2D texture, string tooltip)
    {
      return TextureGroupDefinition(groupName, propKey, texture, 
        RebuildType.None, null, false, tooltip);
    }
    
    public static ProfileGroupDefinition TextureGroupDefinition(
      string groupName, string propKey, Texture2D texture,
      string dependsOnKeyword, bool dependsOnValue, string tooltip)
    {
      return TextureGroupDefinition(groupName, propKey, texture, RebuildType.None,
        dependsOnKeyword, dependsOnValue, tooltip);
    }

    public static ProfileGroupDefinition TextureGroupDefinition(
      string groupName, string propKey, Texture2D texture, RebuildType rebuildType,
      string dependsOnKeyword, bool dependsOnValue, string tooltip)
    {
      ProfileGroupDefinition def = new ProfileGroupDefinition();

      def.type = GroupType.Texture;
      def.groupName = groupName;
      def.propertyKey = propKey;
      def.texture = texture;
      def.tooltip = tooltip;
      def.rebuildType = rebuildType;
      def.dependsOnFeature = dependsOnKeyword;
      def.dependsOnValue = dependsOnValue;

      return def;
    }

    public static ProfileGroupDefinition BoolGroupDefinition(
      string groupName, string propKey, bool value, string dependsOnKeyword, bool dependsOnValue, string tooltip)
    {
      ProfileGroupDefinition def = new ProfileGroupDefinition();

      def.type = GroupType.Boolean;
      def.groupName = groupName;
      def.propertyKey = propKey;
      def.dependsOnFeature = dependsOnKeyword;
      def.dependsOnValue = dependsOnValue;
      def.tooltip = tooltip;
      def.boolValue = value;

      return def;
    }
  }
}
