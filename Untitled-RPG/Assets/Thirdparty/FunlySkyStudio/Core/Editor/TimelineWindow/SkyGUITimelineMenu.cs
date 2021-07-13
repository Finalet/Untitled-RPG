using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  // Show a popup menu that lets the user add an item to the timeline. This is typically triggered from a button.
  public abstract class SkyGUITimelineMenu : System.Object
  {
    private static SkyProfile s_Profile;

    public static void ShowAddTimelinePropertyMenu(SkyProfile profile)
    {
      List<ProfileGroupDefinition> offTimeline = profile.GetGroupDefinitionsNotManagedByTimeline();
      ShowAddTimelinePropertyMenu(profile, offTimeline);
    }

    public static void ShowAddTimelinePropertyMenu(SkyProfile profile, List<ProfileGroupDefinition> groups)
    {
      s_Profile = profile;
      GenericMenu menu = new GenericMenu();

      foreach (ProfileGroupSection sectionInfo in s_Profile.GetProfileDefinitions()) {
        foreach (ProfileGroupDefinition groupInfo in sectionInfo.groups) {
          if (s_Profile.IsManagedByTimeline(groupInfo.propertyKey) == false && s_Profile.CanGroupBeOnTimeline(groupInfo)) {
            string itemName = sectionInfo.sectionTitle + "/" + groupInfo.groupName;
            menu.AddItem(new GUIContent(itemName), false, DidSelectAddTimelineProperty, groupInfo.propertyKey);
          }
        }
      }

      menu.ShowAsContext();
    }

    // Menu callback for adding new timeline keys into profile.
    private static void DidSelectAddTimelineProperty(object propertyKey)
    {
      // Prevent duplicates when you add.
      s_Profile.timelineManagedKeys.Remove((string)propertyKey);
      s_Profile.timelineManagedKeys.Add((string)propertyKey);
    }
  }
}

