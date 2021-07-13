using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  public class SkyStudioAssetPostProcessor : AssetPostprocessor
  {
    public static string migrationVersionKey = "SkyStudio-Migration-Version";
    public static int migrationVersion = 1;
    public static string migrationUnityVersionKey = "SkyStudio-Migration-Unity-Version";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
      if (importedAssets == null || importedAssets.Length == 0) {
        return;
      }
      
      // Certain asset paths force trigger migration, others we check using a version.
      if (!FileMigrationTriggers(importedAssets) && !ShouldRunFullMigration()) {
        return;
      }

      RunFullMigration();
    }
    
    // Check for certain paths that when imported indicate a need to migrate.
    static bool FileMigrationTriggers(string[] assets) {
      return false;
    }

    static bool ShouldRunFullMigration() {
      // Check if migration is already current to this version.
      int lastVersion = EditorPrefs.GetInt(migrationVersionKey, 0);
      string lastUnityVersion = EditorPrefs.GetString(migrationUnityVersionKey, "");

      if (lastVersion == migrationVersion && lastUnityVersion == Application.version) {
        return false;
      }

      return true;
    }

    static void RunFullMigration() {
      EditorPrefs.SetInt(migrationVersionKey, migrationVersion);
      EditorPrefs.SetString(migrationUnityVersionKey, Application.version);
      
      // We currently have no migration code now that we support 2019.4+
    }
  }
}

