using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  public class SkyCreateMenuItemEditor
  {
    [MenuItem("GameObject/Sky Studio/Lightning Spawn Area")]
    public static void CreateLightingArea(MenuCommand menuCommand)
    {
      GameObject area = new GameObject();
      area.AddComponent<LightningSpawnArea>();
      area.name = "Lightning Spawn Area";

      GameObjectUtility.SetParentAndAlign(area, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(area, "Created " + area.name);
      Selection.activeObject = area;
    }
  }
}
