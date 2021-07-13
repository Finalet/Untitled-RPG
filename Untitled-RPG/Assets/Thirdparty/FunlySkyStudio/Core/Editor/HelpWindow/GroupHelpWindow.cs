using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio {
  public class GroupHelpWindow : EditorWindow
	{
		private static GroupHelpWindow _instance;
		private static Dictionary<string, ProfileGroupDefinition> m_HelpItems;
		private static string m_CurrentHelpKey;
	  private static SkyProfile m_Profile;

		public static void ShowWindow()
		{
      SharedHelpWindow().ShowUtility();
		}

    public static GroupHelpWindow SharedHelpWindow() {
			if (_instance == null)
			{
			  _instance = CreateInstance<GroupHelpWindow>();
        _instance.name = "Sky Help";
				_instance.titleContent = new GUIContent("Sky Help");
			}
      return _instance;
    }

    public static void SetHelpItem(SkyProfile profile, string propertyKey, bool showWindow) {
      m_Profile = profile;
      BuildHelpContentIfNecessary();

      if (m_HelpItems.ContainsKey(propertyKey) == false) {
        return;
      }

      m_CurrentHelpKey = propertyKey;

      if (showWindow)
      {
        ShowWindow();
      }

      if (_instance != null)
      {
        SharedHelpWindow().Repaint();
      }
    }

    public static bool ContainsHelpForKey(string propertyKey) {
      BuildHelpContentIfNecessary();

      return m_HelpItems.ContainsKey(propertyKey);
    }

		private void OnInspectorUpdate()
		{
		  // Make sure we only have 1 window open.
		  if (this != _instance) {
		    this.Close();
		    return;
		  }

      Repaint();
		}

		private void OnGUI()
		{
      BuildHelpContentIfNecessary();

			if (m_CurrentHelpKey == null)
			{
				EditorGUILayout.HelpBox("No property selected to display help for.", MessageType.Info);
				return;
			}

			RenderHelpItem(m_CurrentHelpKey);
		}

		private void RenderHelpItem(string helpKey)
		{
      GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
      titleStyle.fontStyle = FontStyle.Bold;
		  
      GUIStyle textStyle = new GUIStyle(GUI.skin.label);
		  textStyle.wordWrap = true;

		  if (ContainsHelpForKey(helpKey) == false)
		  {
        EditorGUILayout.HelpBox("There is no help entry for property key: " + helpKey, MessageType.Info);
        return;
		  }

		  ProfileGroupDefinition groupDefinition = m_HelpItems[helpKey];

		  EditorGUILayout.BeginVertical();
		  EditorGUILayout.LabelField(groupDefinition.groupName, titleStyle);
      EditorGUILayout.Space();
      EditorGUILayout.LabelField(groupDefinition.tooltip, textStyle);

		  string imageName = ImageNameForHelpPropertyKey();
			if (imageName != null)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				// TODO - Draw image.

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}

      EditorGUILayout.EndVertical();
		}

	  private string ImageNameForHelpPropertyKey()
	  {
      // TODO - support images.
	    return null;
	  }

    private static void BuildHelpContentIfNecessary() {
			if (m_HelpItems == null)
			{
				BuildHelpContent();
			}
    }

		private static void BuildHelpContent()
		{
		  if (m_Profile == null)
		  {
        Debug.LogError("Can't load help content, since there isn't an active sky profile.");
		    return;
		  }
		  m_HelpItems = m_Profile.GroupDefinitionDictionary();
		}
	}
}
