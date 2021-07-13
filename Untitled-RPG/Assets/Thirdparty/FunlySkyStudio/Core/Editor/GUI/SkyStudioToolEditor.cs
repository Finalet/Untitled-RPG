using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  [InitializeOnLoad]
  public class SkyStudioToolEditor : Editor
  {
    static SkyStudioToolEditor()
    {
      SceneView.duringSceneGui += OnSceneGUI;
    }

    public static void OnSceneGUI(SceneView sceneView)
    {
      SpherePointGUI.RenderSpherePointSceneSelection();
    }
  }
}
