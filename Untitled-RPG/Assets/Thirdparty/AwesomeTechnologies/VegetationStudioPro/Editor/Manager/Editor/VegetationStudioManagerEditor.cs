using AwesomeTechnologies.ColliderSystem;
using AwesomeTechnologies.PrefabSpawner;
using AwesomeTechnologies.TerrainSystem;
using AwesomeTechnologies.TouchReact;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationStudio;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(VegetationStudioManager))]
    public class VegetationStudioManagerEditor : VegetationStudioProBaseEditor
    {
        private static readonly string[] TabNames =
        {
            "Settings","Post process volumes"
        };

        private VegetationStudioManager _vegetationStudioManager;

        [MenuItem("Window/Awesome Technologies/Regenerate all splatmaps %#_r")]
        // ReSharper disable once UnusedMember.Local
        static void RefreshSplatmap()
        {
            TerrainSystemPro[] terrainsystems = FindObjectsOfType<TerrainSystemPro>();
            for (int i = 0; i <= terrainsystems.Length - 1; i++)
            {
                terrainsystems[i].GenerateSplatMap(false);
                terrainsystems[i].ShowTerrainHeatmap(false);
            }
        }

        [MenuItem("Window/Awesome Technologies/Add Vegetation Studio Pro to scene")]
        // ReSharper disable once UnusedMember.Local
        public static void AddVegetationStudioManager()
        {
            VegetationStudioManager vegetationStudioManager = FindObjectOfType<VegetationStudioManager>();
            if (vegetationStudioManager)
            {
                EditorUtility.DisplayDialog("Vegetation Studio Pro Component",
                    "There is already a Vegetation Studio Pro Manager Component in the scene. There can be only one.",
                    "OK");
            }
            else
            {
                GameObject go = new GameObject {name = "VegetationStudioPro"};
                go.AddComponent<VegetationStudioManager>();

                GameObject vegetationSystem = new GameObject {name = "VegetationSystemPro"};
                vegetationSystem.transform.SetParent(go.transform);
                VegetationSystemPro vegetationSystemPro = vegetationSystem.AddComponent<VegetationSystemPro>();
                vegetationSystem.AddComponent<TerrainSystemPro>();
                vegetationSystemPro.AddAllUnityTerrains();

#if TOUCH_REACT
                GameObject touchReactSystem = new GameObject { name = "TouchReactSystem" };
                touchReactSystem.transform.SetParent(go.transform);               
                touchReactSystem.AddComponent<TouchReactSystem>();
#endif                
                vegetationSystem.AddComponent<ColliderSystemPro>();
                vegetationSystem.AddComponent<PersistentVegetationStorage>();
                RuntimePrefabSpawner runtimePrefabSpawner =  vegetationSystem.AddComponent<RuntimePrefabSpawner>();
                runtimePrefabSpawner.enabled = false;
            }
        }

        public override void OnInspectorGUI()
        {
            LargeLogo = true;
            _vegetationStudioManager = (VegetationStudioManager) target;
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            _vegetationStudioManager.CurrentTabIndex = GUILayout.SelectionGrid(_vegetationStudioManager.CurrentTabIndex, TabNames, 2, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
            switch (_vegetationStudioManager.CurrentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawPostProcessInspector();
                    break;               
            }           
        }

        void SetSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(_vegetationStudioManager.gameObject.scene);
                EditorUtility.SetDirty(_vegetationStudioManager);
            }
        }

        private void DrawPostProcessInspector()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add profiles", LabelStyle);

            EditorGUI.BeginChangeCheck();
            PostProcessProfile newPostProcessProfile = (PostProcessProfile)EditorGUILayout.ObjectField("", null,typeof(PostProcessProfile),false);

            if (EditorGUI.EndChangeCheck())
            {
                _vegetationStudioManager.AddPostProcessProfile(newPostProcessProfile);
                SetSceneDirty();
            }
            EditorGUILayout.HelpBox("Add post processing profiles here to set up PostProcessVolumes for the biomes", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            EditorGUI.BeginChangeCheck();
            _vegetationStudioManager.PostProcessingLayer = EditorGUILayout.LayerField("Post process layer", _vegetationStudioManager.PostProcessingLayer);
            if (EditorGUI.EndChangeCheck())
            {
                _vegetationStudioManager.RefreshPostProcessVolumes();
                SetSceneDirty();
                GUILayout.EndVertical();
                return;
            }

            GUILayout.EndVertical();

            for (int i = 0; i <= _vegetationStudioManager.PostProcessProfileInfoList.Count - 1; i++)
            {
                GUILayout.BeginVertical("box");

                if (GUILayout.Button("Remove profile", GUILayout.Width(120)))
                {
                    _vegetationStudioManager.RemovePostProcessProfile(i);
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;
                }

                EditorGUI.BeginChangeCheck();

                PostProcessProfileInfo postProcessProfileInfo = _vegetationStudioManager.PostProcessProfileInfoList[i];
                postProcessProfileInfo.Enabled = EditorGUILayout.Toggle("Enabled", postProcessProfileInfo.Enabled);
                postProcessProfileInfo.BiomeType =
                    (BiomeType) EditorGUILayout.EnumPopup("Biome Type", postProcessProfileInfo.BiomeType);
                postProcessProfileInfo.VolumeHeight =
                    EditorGUILayout.FloatField("Volume height", postProcessProfileInfo.VolumeHeight);
                postProcessProfileInfo.Priority =
                    EditorGUILayout.FloatField("Priority", postProcessProfileInfo.Priority);
                postProcessProfileInfo.BlendDistance =
                    EditorGUILayout.Slider("Blend distance", postProcessProfileInfo.BlendDistance,-0.1f,4f);
                postProcessProfileInfo.Weight =
                    EditorGUILayout.Slider("Weight", postProcessProfileInfo.Weight, 0f,1f);
                postProcessProfileInfo.PostProcessProfile = (PostProcessProfile)EditorGUILayout.ObjectField("Profile", postProcessProfileInfo.PostProcessProfile, typeof(PostProcessProfile), false);

                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationStudioManager.RefreshPostProcessVolumes();
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;

                }
                GUILayout.EndVertical();
            }
#else
        EditorGUILayout.HelpBox("Install the Unity Post processing stack to enable.", MessageType.Info);
#endif
        }

        private void DrawSettingsInspector()
        {
            EditorGUILayout.HelpBox(
                "Vegetation System Manager will manage all Vegetation, Terrain, Collider, Masks and Billboard Systems in the scene. Only one instance per scene.",
                MessageType.Info);

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation systems", LabelStyle);
            EditorGUI.BeginDisabledGroup(true);
            for (int i = 0; i <= _vegetationStudioManager.VegetationSystemList.Count - 1; i++)
                EditorGUILayout.ObjectField("Vegetation System Pro", _vegetationStudioManager.VegetationSystemList[i],
                    typeof(VegetationSystemPro), true);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
        }
    }

}
