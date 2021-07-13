using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  public abstract class SpherePointGUI
  {
    // ID of the owner of the selection data.
    private static string m_ActiveReceiver;

    private static bool m_SelectionResultReady;

    private static SpherePoint m_SelectedSpherePoint;

    // Handles allowing user to make a selection in the scene view for an sphere point position.
    public static void RenderSpherePointSceneSelection()
    { 
      if (m_ActiveReceiver == null || m_SelectionResultReady)
      {
        return;
      }

      Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

      Vector3 worldPoint = ray.GetPoint(.01f);

      Handles.DrawWireDisc(worldPoint, ray.direction * -1.0f, HandleUtility.GetHandleSize(worldPoint));
         
      if (Event.current.keyCode == KeyCode.Escape)
      {
        CancelSpherePointSceneSelection();
        return;
      }

      // Ignore click if some special key is pressed for navigation reasons.
      if (Event.current.alt || Event.current.control || Event.current.command)
      {
        return;
      }

      if (Event.current.type != EventType.MouseDown)
      {
        return;
      }

      m_SelectedSpherePoint = new SpherePoint(ray.direction);
      m_SelectionResultReady = true;

      Event.current.Use();
      GUIUtility.hotControl = 0;
    }

    public static void CancelSpherePointSceneSelection()
    {
      m_ActiveReceiver = null;
    }

    // GUI for a sphere point selection.
    public static SpherePoint SpherePointField(SpherePoint spherePoint, bool sceneSelection, string controlId)
    {
      bool isActive = IsActiveToken(controlId);
      SpherePoint selectedSpherePoint = spherePoint;

      // Horizontal rotation layout.
      EditorGUILayout.BeginVertical();
      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      float selectedHorizontalValue = EditorGUILayout.Slider("Horizontal", spherePoint.horizontalRotation,
        SpherePoint.MinHorizontalRotation, SpherePoint.MaxHorizontalRotation);
      if (EditorGUI.EndChangeCheck()) {
        selectedSpherePoint = new SpherePoint(selectedHorizontalValue, spherePoint.verticalRotation);
        GUI.changed = true;
      }

      EditorGUILayout.EndHorizontal();

      // Vertical rotation layout.
      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      float selectedVerticalValue = EditorGUILayout.Slider("Vertical", spherePoint.verticalRotation,
        SpherePoint.MinVerticalRotation, SpherePoint.MaxVerticalRotation);
      if (EditorGUI.EndChangeCheck()) {
        selectedSpherePoint = new SpherePoint(spherePoint.horizontalRotation, selectedVerticalValue);
        GUI.changed = true;
      }

      EditorGUILayout.EndHorizontal();

      if (sceneSelection)
      {
        RenderSpherePointSelectionButton(controlId);
      }

      EditorGUILayout.EndVertical();

      // Check if a selection has completed for this control.
      if (isActive && m_SelectionResultReady)
      {
        selectedSpherePoint = m_SelectedSpherePoint;
        m_SelectionResultReady = false;
        m_ActiveReceiver = null;
        GUI.changed = true;
      }

      return selectedSpherePoint;
    }

    private static void RenderSpherePointSelectionButton(string controlId)
    {
      bool isActive = IsActiveToken(controlId);

      EditorGUI.BeginChangeCheck();
      string buttonTitle;

      GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
      if (isActive)
      {
        buttonTitle = "Click in Scene View...";
        btnStyle.normal.textColor = Color.red;
      }
      else
      {
        buttonTitle = "Position With Cursor...";
        btnStyle.normal.textColor = GUI.skin.button.normal.textColor;
      }

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PrefixLabel("Positional Visually");
      bool isClicked = GUILayout.Button(new GUIContent(buttonTitle), btnStyle);
      if (EditorGUI.EndChangeCheck()) {
        CancelSpherePointSceneSelection();

        if (!isActive && isClicked) {
          m_ActiveReceiver = controlId;
        }
      }

      EditorGUILayout.EndHorizontal();
    }

    private static bool IsActiveToken(string token)
    {
      return token != null && m_ActiveReceiver != null && token == m_ActiveReceiver;
    }
  }
}

