using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Funly.SkyStudio
{
  public class UpgradeSkySystemController : Editor
  {

    [MenuItem("Window/Sky Studio/Upgrade Sky System Controller...")]
    public static void UpgradeSetupSkySystem() {
      TimeOfDayController oldTc = GameObject.FindObjectOfType<TimeOfDayController>();
      if (oldTc == null) {
        EditorUtility.DisplayDialog("Sky Upgrade Failed", "There is no SkySystemController in your current scene to upgrade. Try using the Setup Sky tool to create a sky system instead.", "OK");
        return;
      }
      
      GameObject skySystemPrefab = SkyEditorUtility.LoadEditorPrefab(SkySetupWindow.SKY_CONTROLLER_PREFAB);
      if (skySystemPrefab == null) {
        Debug.LogError("Failed to locate sky controller prefab");
        EditorUtility.DisplayDialog("Sky Upgrade Failed", "Failed to locate SkySystemController prefab. Did you move the prefab or rename any Sky Studio folders? Delete FunlySkyStudio and reinstall the asset package.", "OK");
        return;
      }

      TimeOfDayController tc = Instantiate(skySystemPrefab).GetComponent<TimeOfDayController>();
      tc.name = SkySetupWindow.SKY_CONTROLLER_PREFAB;
      tc.skyProfile = oldTc.skyProfile;
      tc.skyTime = oldTc.skyTime;
      tc.automaticIncrementSpeed = oldTc.automaticIncrementSpeed;
      tc.automaticTimeIncrement = oldTc.automaticTimeIncrement;

      DestroyImmediate(oldTc.gameObject);
    
      EditorUtility.SetDirty(tc);
      EditorUtility.SetDirty(tc.gameObject);
      EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

      EditorUtility.DisplayDialog("Sky Upgrade Complete", "The SkySystemController in your current scene has been upgraded, and is using the latest Sky Studio prefab.", "OK");
    }
  }
}
