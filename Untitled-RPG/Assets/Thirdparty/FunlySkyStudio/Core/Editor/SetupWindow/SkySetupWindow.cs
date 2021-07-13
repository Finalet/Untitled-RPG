using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine.Rendering;

namespace Funly.SkyStudio
{
  // Utility to make it easy to setup a new sky system in the current scene.
  public class SkySetupWindow : EditorWindow
  {
    private class ProfilePreset : IComparer<ProfilePreset>
    {
      public string guid;
      public string assetPath;
      public string name;
      public string menuName;

      public ProfilePreset(string guid, string assetPath, string name, string menuName)
      {
        this.guid = guid;
        this.assetPath = assetPath;
        this.name = name;
        this.menuName = menuName;
      }

      public int Compare(ProfilePreset x, ProfilePreset y)
      {
        return x.assetPath.CompareTo(y.assetPath);
      }
    }

    private ProfilePreset _selectedProfilePreset;
    public const string SKY_CONTROLLER_PREFAB = "SkySystemController";

    [MenuItem("Window/Sky Studio/Setup Sky")]
    public static void ShowWindow()
    {
      TimelineSelection.Clear();

      EditorWindow window = EditorWindow.GetWindow<SkySetupWindow>();

      window.Show();
    }

    private void OnEnable()
    {
      name = "Setup Sky";
      titleContent = new GUIContent("Setup Sky");

      presets = LoadListOfPresets();
    }
    List<ProfilePreset> presets;
    private void OnGUI()
    {
      // List<ProfilePreset> presets = LoadListOfPresets();

      EditorGUILayout.HelpBox("Setup a new sky system in the current scene. " +
                              "This will create a copy of the preset you select, and load it into your scene.",
        MessageType.Info);
      EditorGUILayout.Separator();

      RenderPresetPopup(presets);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      if (GUILayout.Button(new GUIContent("Create Sky System"))) {
        SetupSceneWithPreset(_selectedProfilePreset);
      }
    }

    private void RenderPresetPopup(List<ProfilePreset> presets)
    {
      int selectedIndex = 0;
      List<string> displayedPrests = new List<string>();

      // Build our list of presets to show in popup.
      for (int i = 0; i < presets.Count; i++) {
        ProfilePreset preset = presets[i];
        displayedPrests.Add(preset.menuName);

        // Check if this is the selected preset.
        if (_selectedProfilePreset != null && preset.assetPath == _selectedProfilePreset.assetPath) {
          selectedIndex = i;
        }
      }

      selectedIndex = EditorGUILayout.Popup("Sky Preset", selectedIndex, displayedPrests.ToArray());
      _selectedProfilePreset = presets[selectedIndex];
    }

    private List<ProfilePreset> LoadListOfPresets()
    {
      List<ProfilePreset> presets = new List<ProfilePreset>();

      string[] guids = AssetDatabase.FindAssets("t:SkyProfile");

      if (guids == null || guids.Length == 0) {
        return presets;
      }

      foreach (string guid in guids) {
        string presetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (presetPath == null) {
          Debug.LogError("Failed to get name for profile GUID: " + guid);
          continue;
        }
        string presetName = ObjectNames.NicifyVariableName(Path.GetFileNameWithoutExtension(presetPath));
        string menuName = Path.GetDirectoryName(presetPath) + "/" + presetName;
        menuName = SkyEditorUtility.WindowsPathToUnixPath(menuName);

        string presetDirPrefix = $"{SkyEditorUtility.SkyStudioRootDirectory()}/Core/Internal/Presets/";
        if (menuName.StartsWith(presetDirPrefix))
        {
          menuName = menuName.Remove(0, presetDirPrefix.Length);
        }
        else
        {
          menuName = "Your Project/" + menuName;
        }

        presets.Add(new ProfilePreset(guid, presetPath, presetName, menuName));
      }

      presets.Sort(delegate(ProfilePreset p1, ProfilePreset p2)
      {
        return p1.menuName.CompareTo(p2.menuName);
      });

      return presets;
    }

    private void SetupSceneWithPreset(ProfilePreset preset)
    {
      InstallRendererIfNecessary();
      
      ClearSkyControllers();

      Scene currentScene = SceneManager.GetActiveScene();
      string sceneDir = Path.GetDirectoryName(currentScene.path);
      string profileContainerName = currentScene.name + " - Sky Data";
      string profileContainerDir = SkyEditorUtility.GenerateUniqueFolder(sceneDir, profileContainerName, true);

      // Create new sky controller.
      GameObject skySystemPrefab = SkyEditorUtility.LoadEditorPrefab(SKY_CONTROLLER_PREFAB);
      if (skySystemPrefab == null) {
        Debug.LogError("Failed to locate sky controller prefab");
        return;
      }

      TimeOfDayController tc = Instantiate(skySystemPrefab).GetComponent<TimeOfDayController>();
      tc.name = SKY_CONTROLLER_PREFAB;

      // Create a new sky profile.
      string profileAssetPath = SkyEditorUtility.GenerateUniqueFilename(profileContainerDir, "SkyProfile", ".asset");
      AssetDatabase.CopyAsset(preset.assetPath, profileAssetPath);

      // Load the new SKy Profile.
      SkyProfile profile = AssetDatabase.LoadAssetAtPath(profileAssetPath, typeof(SkyProfile)) as SkyProfile;
      if (profile == null) {
        Debug.LogError("Failed to duplicate profile");
        return;
      }

      // Create the skybox material.
      Material skyboxMaterial = new Material(GetBestShaderForSkyProfile(profile));
      string skyboxPath = SkyEditorUtility.GenerateUniqueFilename(profileContainerDir, "SkyboxMaterial", ".mat");
      AssetDatabase.CreateAsset(skyboxMaterial, skyboxPath);
      profile.skyboxMaterial = skyboxMaterial;

      // Link things together.
      tc.skyProfile = profile;
      tc.skyProfile.skyboxMaterial = skyboxMaterial;
      tc.skyTime = .22f;

      // Configure the profile a bit and setup in the current scene.
      SkyProfileEditor.ApplyKeywordsToMaterial(tc.skyProfile, skyboxMaterial);
      SkyProfileEditor.forceRebuildProfileId = profile.GetInstanceID();

      RenderSettings.skybox = skyboxMaterial;

      ApplyDefaultSettings(profile);

      // Drop a lightning spawn area into the scene in case user enables the feature.
      if (!ContainsLightningSpawnArea()) {
        CreateLightningSpawnArea();
      }

      EditorUtility.SetDirty(skyboxMaterial);
      EditorUtility.SetDirty(tc.skyProfile);
      EditorUtility.SetDirty(tc);
      EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

      Selection.activeObject = tc.skyProfile;
    }

    // Support for URP to swop out the pipeline asset with our custom one, since we need a renderer added.
    // FIXME - In the perfect world, we'd add our renderer to the existing pipeline, but there doesn't appear
    // to be a Unity API for doing this. 
    private void InstallRendererIfNecessary()
    { 
      var config = RenderingConfig.Detect();
      switch (config)
      {
        case RenderingConfig.DetectedRenderingConfig.URP:
          string[] guids = AssetDatabase.FindAssets("t:RenderPipelineAsset SkyStudio-UniversalRenderPipelineAsset");
          if (guids.Length == 0)
          {
            Debug.LogError("Failed to locate the Sky Studio URP pipeline asset.");
            return;
          }

          RenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(
            AssetDatabase.GUIDToAssetPath(guids[0]));
          if (pipeline == null)
          {
            Debug.LogError("Failed to load Sky Studio URP Render Pipeline Asset");
            return;
          }

          GraphicsSettings.renderPipelineAsset = pipeline;
          GraphicsSettings.defaultRenderPipeline = pipeline;
          
          Debug.Log("Sky Studio - Swapping render pipeline with custom version.");
          break;
        default:
          break;
      } 
    }

    private Shader GetBestShaderForSkyProfile(SkyProfile profile)
    {
      string shaderName = SkyEditorUtility.GetBestDefaultShaderNameForUnityVersion();
      return Shader.Find(shaderName);
    }

    private string GetActiveSceneDirectory()
    {
      return Path.GetDirectoryName(SceneManager.GetActiveScene().path);
    }

    private string GetBestFileName(string baseDir, string fileName, string ext, System.Type fileType)
    {
      for (int i = 0; i < 100; i++) {
        string suffixName = null;
        if (i == 0) {
          suffixName = fileName + ext;
        } else {
          suffixName = fileName + "-" + i + ext;
        }

        string assetPath = baseDir + "/" + suffixName;

        if (AssetDatabase.LoadAssetAtPath(assetPath, fileType) == null)
        {
          return suffixName;
        }
      }

      return null;
    }

    private void ClearSkyControllers()
    {
      TimeOfDayController[] skyControllers = GameObject.FindObjectsOfType<TimeOfDayController>();
      if (skyControllers != null) {
        foreach (TimeOfDayController timeController in skyControllers) {
          Debug.Log("Removing old sky controller from scene...");
          DestroyImmediate(timeController.gameObject);
        }
      }
    }

    private bool ContainsLightningSpawnArea()
    {
      LightningSpawnArea area = GameObject.FindObjectOfType<LightningSpawnArea>();
      return area != null;
    }

    private void CreateLightningSpawnArea()
    {
      GameObject area = new GameObject();
      area.AddComponent<LightningSpawnArea>();
      area.layer = LayerMask.NameToLayer("TransparentFX");
      area.name = "Lightning Spawn Area";

      if (Camera.main == null) {
        Debug.LogWarning("Can't position default spawn area in front of camera since no main camera exists. Tag a camera as Main");
        area.transform.position = Vector3.zero;
        return;
      }

      Transform camera = Camera.main.transform;

      float distanceBack = 50.0f;
      float distanceUp = 10.0f;
      Vector3 lightningPos = camera.position + (camera.forward * distanceBack);
      lightningPos.y += distanceUp;

      area.transform.position = lightningPos;
    }

    private T GetDefaultArtStyleWithName<T>(string artStyleName) where T : class
    {
      string[] guids = AssetDatabase.FindAssets(artStyleName);
      if (guids.Length == 0) {
        return null;
      }

      string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
      return AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
    }

    private void ApplyDefaultSettings(SkyProfile profile)
    {
      // Lightning art.
      if (profile.lightningArtSet == null) {
        profile.lightningArtSet = GetDefaultArtStyleWithName<LightningArtSet>("DefaultLightningArtSet");
      }

      // Splash art.
      if (profile.rainSplashArtSet == null) {
        profile.rainSplashArtSet = GetDefaultArtStyleWithName<RainSplashArtSet>("DefaultRainSplashArtSet");
      }

      // Rain near texture.
      TextureKeyframeGroup group = profile.GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.RainNearTextureKey);
      if (group.keyframes.Count == 1 && group.keyframes[0].texture == null) {
        group.keyframes[0].texture = SkyEditorUtility.LoadEditorResourceTexture("RainDownfall-1");
        if (group.keyframes[0].texture == null) {
          Debug.LogWarning("Failed to locate default near rain texture");
        }
      }

      // Rain far texture.
      group = profile.GetGroup<TextureKeyframeGroup>(ProfilePropertyKeys.RainFarTextureKey);
      if (group.keyframes.Count == 1 && group.keyframes[0].texture == null) {
        group.keyframes[0].texture = SkyEditorUtility.LoadEditorResourceTexture("RainDownfall-3");
        if (group.keyframes[0].texture == null) {
          Debug.LogWarning("Failed to locate default far rain texture");
        }
      }
    }
  }
}
