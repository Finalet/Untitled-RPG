using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Funly.SkyStudio;

[CustomEditor(typeof(HelpInformation))]
public class HelpInformationEditor : Editor
{

  [MenuItem("Window/Sky Studio/Help/Join our Discord Server...")]
  private static void OpenDiscordChat()
  {
    Application.OpenURL("http://bit.ly/2GteOFN");
  }

  [MenuItem("Window/Sky Studio/Help/Video Tutorials...")]
  private static void OpenVideoTutorials()
  {
    Application.OpenURL("http://bit.ly/2GpFVl2");
  }

  [MenuItem("Window/Sky Studio/Help/Review Sky Studio...")]
  private static void OpenSkyStudioStorePage()
  {
    Application.OpenURL("http://bit.ly/2GvkjUv");
  }

  public override void OnInspectorGUI()
  {
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Enjoying Sky Studio?");
    bool didClick = GUILayout.Button(new GUIContent("Please Leave a Review..."));
    if (didClick) {
      OpenSkyStudioStorePage();
    }
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Tutorial Videos");
    didClick = GUILayout.Button(new GUIContent("Open Tutorials..."));
    if (didClick) {
      OpenVideoTutorials();
    }
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Chat Support");
    didClick = GUILayout.Button(new GUIContent("Join Discord for help..."));
    if (didClick) {
      OpenDiscordChat();
    }
    EditorGUILayout.EndHorizontal();

    
  }
}
