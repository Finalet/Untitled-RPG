using AwesomeTechnologies.VegetationSystem;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

namespace AwesomeTechnologies.TerrainSystem
{
    public partial class TerrainSystemPro
    {
        public void ShowTerrainHeatmap(bool value)
        {
            if (!VegetationSystemPro) return;
            VegetationSystemPro.ShowHeatMap = value;

            if (value)
            {
                float worldspaceMinHeight = VegetationSystemPro.VegetationSystemBounds.center.y -
                                            VegetationSystemPro.VegetationSystemBounds.extents.y;
                float worldspaceSeaLevel = worldspaceMinHeight + VegetationSystemPro.SeaLevel;
                float worldspaceMaxHeight = VegetationSystemPro.VegetationSystemBounds.center.y +
                                            VegetationSystemPro.VegetationSystemBounds.extents.y;

                for (int i = 0; i <= VegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
                {
                    VegetationSystemPro.VegetationStudioTerrainList[i].OverrideTerrainMaterial();

                    VegetationPackagePro vegetationPackagePro =
                        VegetationSystemPro.VegetationPackageProList[VegetationPackageIndex];
                    TerrainTextureSettings terrainTextureSettings = vegetationPackagePro.TerrainTextureSettingsList[VegetationPackageTextureIndex];
                    VegetationSystemPro.VegetationStudioTerrainList[i].UpdateTerrainMaterial(worldspaceSeaLevel, worldspaceMaxHeight, terrainTextureSettings);
                }
            }
            else
            {
                for (int i = 0; i <= VegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
                {
                    VegetationSystemPro.VegetationStudioTerrainList[i].RestoreTerrainMaterial();
                }
            }

        }

        public void UpdateTerrainHeatmap()
        {
            if (!VegetationSystemPro.ShowHeatMap) return;

            float worldspaceMinHeight = VegetationSystemPro.VegetationSystemBounds.center.y -
                                        VegetationSystemPro.VegetationSystemBounds.extents.y;
            float worldspaceSeaLevel = worldspaceMinHeight + VegetationSystemPro.SeaLevel;
            float worldspaceMaxHeight = VegetationSystemPro.VegetationSystemBounds.center.y +
                                        VegetationSystemPro.VegetationSystemBounds.extents.y;

            for (int i = 0; i <= VegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
            {
                VegetationPackagePro vegetationPackagePro =
                    VegetationSystemPro.VegetationPackageProList[VegetationPackageIndex];
                TerrainTextureSettings terrainTextureSettings = vegetationPackagePro.TerrainTextureSettingsList[VegetationPackageTextureIndex];
                VegetationSystemPro.VegetationStudioTerrainList[i].UpdateTerrainMaterial(worldspaceSeaLevel, worldspaceMaxHeight, terrainTextureSettings);
            }
        }

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
#if UNITY_EDITOR
            EditorSceneManager.sceneSaving += OnSceneSaving;
#endif
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
#if UNITY_EDITOR
            EditorSceneManager.sceneSaving -= OnSceneSaving;
#endif
        }

        void OnSceneSaving(Scene scene, string path)
        {
            ShowTerrainHeatmap(false);
        }
    }
}
