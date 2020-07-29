using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.VegetationStudio
{
    [CustomEditor(typeof(QualityManager))]
    public class QualityManagerEditor : VegetationStudioProBaseEditor
    {
        private QualityManager _qualityManager;

        public override void OnInspectorGUI()
        {
            ShowLogo = false;
            base.OnInspectorGUI();
            
            
            _qualityManager = (QualityManager)target;
            
            if (GUILayout.Button("Create quality levels"))
            {
                CreateQualityLevels();
            }
            EditorGUILayout.HelpBox("Creating quality levels will clear old settings and add new empty settings based on the Quality settings in Unity.", MessageType.Info);


            if (_qualityManager.QualityLevelList.Count == 0)
            {
                return;
            }
            
            string[] qualityLevelNameList = new string[_qualityManager.QualityLevelList.Count];
            for (int i = 0; i <= _qualityManager.QualityLevelList.Count - 1; i++)
            {
                if (_qualityManager.QualityLevelList[i] != null)
                {
                    qualityLevelNameList[i] = _qualityManager.QualityLevelList[i].Name;
                }
                else
                {
                    qualityLevelNameList[i] = "Not found";
                }
            }

            EditorGUI.BeginChangeCheck();
            _qualityManager.QualityLevelIndex = EditorGUILayout.Popup("Selected quality level",
                _qualityManager.QualityLevelIndex, qualityLevelNameList);
            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
            }

            VegetationSystemProQualityLevel vegetationSystemProQualityLevel =
                _qualityManager.QualityLevelList[_qualityManager.QualityLevelIndex];
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation distances", LabelStyle);
            EditorGUI.BeginChangeCheck();
            vegetationSystemProQualityLevel.PlantDistance =
                EditorGUILayout.Slider("Grass/Plant distance", vegetationSystemProQualityLevel.PlantDistance, 0,
                    800);
            vegetationSystemProQualityLevel.AdditionalTreeMeshDistance =
                EditorGUILayout.Slider("Additional mesh tree distance",
                    vegetationSystemProQualityLevel.AdditionalTreeMeshDistance, 0, 1500);
            vegetationSystemProQualityLevel.AdditionalBillboardDistance =
                EditorGUILayout.Slider("Additional billboard distance",
                    vegetationSystemProQualityLevel.AdditionalBillboardDistance, 0, 20000);
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation density", LabelStyle);
            vegetationSystemProQualityLevel.GrassDensity = EditorGUILayout.Slider("Grass density",
                vegetationSystemProQualityLevel.GrassDensity, 0f, 2f);
            vegetationSystemProQualityLevel.PlantDensity = EditorGUILayout.Slider("Plant density",
                vegetationSystemProQualityLevel.PlantDensity, 0f, 2f);
            vegetationSystemProQualityLevel.TreeDensity = EditorGUILayout.Slider("Tree density",
                vegetationSystemProQualityLevel.TreeDensity, 0f, 2f);
            vegetationSystemProQualityLevel.ObjectDensity = EditorGUILayout.Slider("Object density",
                vegetationSystemProQualityLevel.ObjectDensity, 0f, 2f);
            vegetationSystemProQualityLevel.LargeObjectDensity = EditorGUILayout.Slider("Large Object density",
                vegetationSystemProQualityLevel.LargeObjectDensity, 0f, 2f);
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shadows", LabelStyle);
            vegetationSystemProQualityLevel.GrassShadows = EditorGUILayout.Toggle("Grass cast shadows",
                vegetationSystemProQualityLevel.GrassShadows);
            vegetationSystemProQualityLevel.PlantShadows = EditorGUILayout.Toggle("Plants cast shadows",
                vegetationSystemProQualityLevel.PlantShadows);
            vegetationSystemProQualityLevel.TreeShadows = EditorGUILayout.Toggle("Trees cast shadows",
                vegetationSystemProQualityLevel.TreeShadows);
            vegetationSystemProQualityLevel.ObjectShadows = EditorGUILayout.Toggle("Objects cast shadows",
                vegetationSystemProQualityLevel.ObjectShadows);
            vegetationSystemProQualityLevel.LargeObjectShadows = EditorGUILayout.Toggle(
                "Large objects cast shadows", vegetationSystemProQualityLevel.LargeObjectShadows);
            vegetationSystemProQualityLevel.BillboardShadows = EditorGUILayout.Toggle("Billboards cast shadows",
                vegetationSystemProQualityLevel.BillboardShadows);
            GUILayout.EndVertical();
            
            
            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
            }    
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add Vegetation package", LabelStyle);
            EditorGUILayout.HelpBox(
                "You can add multiple vegetation packages. Each can be assigned a biomeID for use with the biome system.",
                MessageType.Info);
            EditorGUI.BeginChangeCheck();
            VegetationPackagePro newVegetationPackagePro =
                (VegetationPackagePro) EditorGUILayout.ObjectField("Add Vegetation package", null,
                    typeof(VegetationPackagePro), true);

            if (EditorGUI.EndChangeCheck())
            {
                if (newVegetationPackagePro != null)
                {
                    vegetationSystemProQualityLevel.VegetationPackageProList.Add(newVegetationPackagePro);
                    SetSceneDirty();
                    GUILayout.EndVertical();                  
                    return;
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Biomes/Vegetation Packages", LabelStyle);

            for (int i = 0; i <= vegetationSystemProQualityLevel.VegetationPackageProList.Count - 1; i++)
            {
                GUILayout.BeginVertical("box");
                if (GUILayout.Button("Remove biome", GUILayout.Width(120)))
                {
                    vegetationSystemProQualityLevel.VegetationPackageProList.Remove(vegetationSystemProQualityLevel
                        .VegetationPackageProList[i]);
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;
                }

                EditorGUI.BeginChangeCheck();
                vegetationSystemProQualityLevel.VegetationPackageProList[i] =
                    (VegetationPackagePro)EditorGUILayout.ObjectField(
                        "Vegetation package",
                        vegetationSystemProQualityLevel.VegetationPackageProList[i], typeof(VegetationPackagePro),
                        true);

                if (EditorGUI.EndChangeCheck())
                {
                    if (vegetationSystemProQualityLevel.VegetationPackageProList[i] == null)
                    {
                        vegetationSystemProQualityLevel.VegetationPackageProList.Remove(vegetationSystemProQualityLevel
                            .VegetationPackageProList[i]);
                        SetSceneDirty();
                        GUILayout.EndVertical();
                        return;
                    }
                    else
                    {
                        SetSceneDirty();
                        GUILayout.EndVertical();
                        return;
                    }
                }
                GUILayout.EndVertical();
            }
        }

        public void CreateQualityLevels()
        {
            _qualityManager.QualityLevelList.Clear();
            
            string[] names = QualitySettings.names;
            for (int i = 0; i <= names.Length - 1; i++)
            {
                VegetationSystemProQualityLevel vegetationSystemProQualityLevel = new VegetationSystemProQualityLevel
                {
                    Name = names[i],
                    QualityLevelIndex = i
                };
                _qualityManager.QualityLevelList.Add(vegetationSystemProQualityLevel);
            }
        }
        
        
        void SetSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(_qualityManager.gameObject.scene);
                EditorUtility.SetDirty(_qualityManager);
            }
        }
    }
}
