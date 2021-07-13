using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public class ProfileGroupSection
  {
    public string sectionTitle;
    public string sectionIcon;
    public string sectionKey;
    public string dependsOnFeature;
    public bool dependsOnValue;
    public ProfileGroupDefinition[] groups;

    public ProfileGroupSection(
      string sectionTitle, string sectionKey, string sectionIcon, string dependsOnFeature,
      bool dependsOnValue, ProfileGroupDefinition[] groups)
    {
      this.sectionTitle = sectionTitle;
      this.sectionIcon = sectionIcon;
      this.sectionKey = sectionKey;
      this.groups = groups;
      this.dependsOnFeature = dependsOnFeature;
      this.dependsOnValue = dependsOnValue;
    }
  }
}
