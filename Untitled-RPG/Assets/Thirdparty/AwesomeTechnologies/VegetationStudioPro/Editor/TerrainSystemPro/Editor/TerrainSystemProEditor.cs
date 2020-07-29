using AwesomeTechnologies.Common;
using AwesomeTechnologies.External.CurveEditor;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.TerrainSystem
{
    [CustomEditor(typeof(TerrainSystemPro))]
    public class TerrainSystemProEditor : VegetationStudioProBaseEditor
    {
        private TerrainSystemPro _terrainSystemPro;

        private InspectorCurveEditor _heightCurveEditor;
        private InspectorCurveEditor _steepnessCurveEditor;

        private static readonly string[] TabNames =
        {
            "Settings","Edit Biome Splatmap","Edit terrain textures"
        };

        public void OnEnable()
        {
            var settings = InspectorCurveEditor.Settings.DefaultSettings;
            _heightCurveEditor = new InspectorCurveEditor(settings);

            _steepnessCurveEditor = new InspectorCurveEditor(settings) { CurveType = InspectorCurveEditor.InspectorCurveType.Steepness };
        }

        public void OnDisable()
        {
            _heightCurveEditor.RemoveAll();
            _steepnessCurveEditor.RemoveAll();

            _terrainSystemPro = (TerrainSystemPro)target;
            if (_terrainSystemPro) _terrainSystemPro.ShowTerrainHeatmap(false);
        }

        public override void OnInspectorGUI()
        {
            _terrainSystemPro = (TerrainSystemPro)target;
            OverrideLogoTextureName = "Banner_TerrainSystem";
            LargeLogo = false;
            base.OnInspectorGUI();


            if (_terrainSystemPro.VegetationSystemPro == null)
            {
                EditorGUILayout.HelpBox("This component needs to be added to a GameObject with a VegetationSystemPro component.", MessageType.Warning);
                return;
            }

            if (_terrainSystemPro.VegetationPackageIndex >=
                _terrainSystemPro.VegetationSystemPro.VegetationPackageProList.Count)
                _terrainSystemPro.VegetationPackageIndex = 0;

            _terrainSystemPro.CurrentTabIndex = GUILayout.SelectionGrid(_terrainSystemPro.CurrentTabIndex, TabNames, 3, EditorStyles.toolbarButton);
            switch (_terrainSystemPro.CurrentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawEditBiomeSplatmapsInspector();
                    break;
                case 2:
                    DrawEditTerrainTexturesInspector();
                    break;
                case 3:
                    DrawDebugInspector();
                    break;
            }

            if (_terrainSystemPro.CurrentTabIndex != 1)
            {
                _terrainSystemPro.ShowTerrainHeatmap(false);
            }
            else
            {
                _terrainSystemPro.UpdateTerrainHeatmap();
            }

        }
        private void DrawEditTerrainTexturesInspector()
        {
            VegetationSystemPro vegetationSystemPro = _terrainSystemPro.VegetationSystemPro;

            GUILayout.BeginVertical("box");

            if (vegetationSystemPro.VegetationPackageProList.Count == 0)
            {
                EditorGUILayout.HelpBox("You need to add a biome/vegetation package in order to edit splatmap rules.", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }


            EditorGUILayout.LabelField("Select biome/vegetation package", LabelStyle);
            string[] packageNameList = new string[vegetationSystemPro.VegetationPackageProList.Count];
            for (int i = 0; i <= vegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                if (vegetationSystemPro.VegetationPackageProList[i])
                {
                    packageNameList[i] = (i + 1).ToString() + " " +
                                         vegetationSystemPro.VegetationPackageProList[i].PackageName + " (" + vegetationSystemPro.VegetationPackageProList[i].BiomeType.ToString() + ")";
                }
                else
                {
                    packageNameList[i] = "Not found";
                }
            }

            _terrainSystemPro.VegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package",
                _terrainSystemPro.VegetationPackageIndex, packageNameList);

            VegetationPackagePro vegetationPackagePro =
                _terrainSystemPro.VegetationSystemPro.VegetationPackageProList[
                    _terrainSystemPro.VegetationPackageIndex];

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Read textures from terrain"))
            {
                _terrainSystemPro.GetSplatPrototypesFromTerrain(vegetationPackagePro);
                EditorUtility.SetDirty(vegetationPackagePro);
            }
            EditorGUILayout.HelpBox("This will replace textures in the VegetationPackage/biome with the textures from an added Unity Terrain", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Apply textures to terrain"))
            {
                _terrainSystemPro.SetSplatPrototypes(vegetationPackagePro);
            }
            EditorGUILayout.HelpBox("This will replace textures on unity terrains with the textures in the selected biome.", MessageType.Info);
            GUILayout.EndVertical();

            if (vegetationPackagePro.TerrainTextureCount > 0)
            {
                if (_terrainSystemPro.VegetationPackageTextureIndex > vegetationPackagePro.TerrainTextureCount)
                    _terrainSystemPro.VegetationPackageTextureIndex = 0;

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Select terrain texture", LabelStyle);
                GUIContent[] textureImageButtons =
                    new GUIContent[vegetationPackagePro.TerrainTextureSettingsList.Count];

                for (int i = 0; i <= vegetationPackagePro.TerrainTextureSettingsList.Count - 1; i++)
                {
                    textureImageButtons[i] = new GUIContent
                    {
                        image = AssetPreviewCache.GetAssetPreview(
                            vegetationPackagePro.TerrainTextureList[i].Texture)
                    };
                }

                int imageWidth = 80;
                int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
                int rows = Mathf.CeilToInt((float)textureImageButtons.Length / columns);
                int gridHeight = (rows) * imageWidth;

                _terrainSystemPro.VegetationPackageTextureIndex = GUILayout.SelectionGrid(
                    _terrainSystemPro.VegetationPackageTextureIndex, textureImageButtons, columns,
                    GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
                GUILayout.EndVertical();


                TerrainTextureInfo terrainTextureInfo = vegetationPackagePro.TerrainTextureList[_terrainSystemPro.VegetationPackageTextureIndex];

                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Terrain texture layer " + (_terrainSystemPro.VegetationPackageTextureIndex + 1), LabelStyle);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Texture", ""), LabelStyle, GUILayout.Width(64));
                EditorGUILayout.LabelField(new GUIContent("Normal", ""), LabelStyle, GUILayout.Width(64));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                terrainTextureInfo.Texture = (Texture2D)EditorGUILayout.ObjectField(terrainTextureInfo.Texture, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
                terrainTextureInfo.TextureNormals = (Texture2D)EditorGUILayout.ObjectField(terrainTextureInfo.TextureNormals, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
                EditorGUILayout.EndHorizontal();

                Rect space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(18));
                terrainTextureInfo.TileSize = EditorGUI.Vector2Field(space, "Texture tile size", terrainTextureInfo.TileSize);

                if (terrainTextureInfo.TileSize.x < 1 || terrainTextureInfo.TileSize.y < 1)
                {
                    EditorGUILayout.HelpBox("Texture tile size should be set for a higher value, normal is around 15.", MessageType.Warning);
                }

                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    AssetUtility.SetTextureReadable(terrainTextureInfo.Texture, false);
                    AssetUtility.SetTextureReadable(terrainTextureInfo.TextureNormals, true);
                    EditorUtility.SetDirty(vegetationPackagePro);
                }

            }
            GUILayout.EndVertical();
        }

        private void DrawSettingsInspector()
        {
            EditorGUILayout.HelpBox("This component will help you set up splat map generation rules for biomes.", MessageType.Info);
        }

        private void DrawEditBiomeSplatmapsInspector()
        {
            VegetationSystemPro vegetationSystemPro = _terrainSystemPro.VegetationSystemPro;                       

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select biome/vegetation package", LabelStyle);

            if (vegetationSystemPro.VegetationPackageProList.Count == 0)
            {
                EditorGUILayout.HelpBox("You need to add a biome/vegetation package in order to edit splatmap rules.", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }



            string[] packageNameList = new string[vegetationSystemPro.VegetationPackageProList.Count];
            for (int i = 0; i <= vegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                if (vegetationSystemPro.VegetationPackageProList[i])
                {
                    packageNameList[i] = (i + 1).ToString() + " " +
                                         vegetationSystemPro.VegetationPackageProList[i].PackageName + " (" + vegetationSystemPro.VegetationPackageProList[i].BiomeType.ToString() + ")";
                }
                else
                {
                    packageNameList[i] = "Not found";
                }

            }

            EditorGUI.BeginChangeCheck();
            _terrainSystemPro.VegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package",
                _terrainSystemPro.VegetationPackageIndex, packageNameList);
            if (EditorGUI.EndChangeCheck())
            {

            }

            EditorGUILayout.HelpBox(
                "Select the biome to edit. Press the Generate Splatmap button to apply settings to the terrains. Only unity terrains are affected by this.",
                MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Generate biome splatmaps"))
            {
                _terrainSystemPro.GenerateSplatMap(false);
                _terrainSystemPro.ShowTerrainHeatmap(false);
                SetAllVegetationPackagesDirty();
                SetSceneDirty();
            }
            EditorGUILayout.HelpBox(
                "You can lock textures. These will not be not be removed when generating the splat.",
                MessageType.Info);
            
            if (GUILayout.Button("Generate biome splatmaps - clear locked textures"))
            {
                _terrainSystemPro.GenerateSplatMap(true);
                _terrainSystemPro.ShowTerrainHeatmap(false);
                SetAllVegetationPackagesDirty();
                SetSceneDirty();
            }

            GUILayout.EndVertical();
            VegetationPackagePro vegetationPackagePro =
                _terrainSystemPro.VegetationSystemPro.VegetationPackageProList[
                    _terrainSystemPro.VegetationPackageIndex];
            
            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            vegetationPackagePro.GenerateBiomeSplamap = EditorGUILayout.Toggle("Enable biome splatmap", vegetationPackagePro.GenerateBiomeSplamap);
            EditorGUILayout.HelpBox(
                "When enabled this biome will generate a splatmap for its area based on its splatmap rules.",
                MessageType.Info);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(vegetationPackagePro);
            }
            
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            EditorGUI.BeginChangeCheck();
            _terrainSystemPro.VegetationSystemPro.ShowHeatMap = EditorGUILayout.Toggle("Show heatmap", _terrainSystemPro.VegetationSystemPro.ShowHeatMap);
            if (EditorGUI.EndChangeCheck())
            {
                _terrainSystemPro.ShowTerrainHeatmap(_terrainSystemPro.VegetationSystemPro.ShowHeatMap);
                SceneView.RepaintAll();
            }

            EditorGUILayout.HelpBox(
               "Enable to show the terrain distribution for the selected texture. Noise is an estimate and will not be the same as when generated.",
               MessageType.Info);
            GUILayout.EndVertical();        

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Automatic terrain texture distribution is done based on the curve settings for height over water level and steepness(angle) of the terrain", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            _terrainSystemPro.VegetationSystemPro.ShowTerrainTextures = EditorGUILayout.Toggle("Show terrain textures", _terrainSystemPro.VegetationSystemPro.ShowTerrainTextures);
            EditorGUILayout.HelpBox("The textures on the first terrain, not in the package will show in the list below. These are the actual textures used.", MessageType.Info);
            GUILayout.EndVertical();
            //GUILayout.BeginVertical("box");
            //EditorGUILayout.LabelField("Terrain height", LabelStyle);

            //_terrainSystem.GetVegetationPackage().AutomaticMaxCurveHeight =
            //    EditorGUILayout.Toggle("Use max height in terrain", _terrainSystem.GetVegetationPackage().AutomaticMaxCurveHeight);

            //if (!_terrainSystem.GetVegetationPackage().AutomaticMaxCurveHeight)
            //{
            //    _terrainSystem.GetVegetationPackage().MaxCurveHeight = EditorGUILayout.FloatField("Max curve height", _terrainSystem.GetVegetationPackage().MaxCurveHeight);
            //    EditorGUILayout.LabelField("Max possible terrain height: " + _terrainSystem.VegetationSystem.UnityTerrainData.size.y.ToString(CultureInfo.InvariantCulture) + " meters", LabelStyle);

            //    if (GUILayout.Button("Calculate max height in terrain."))
            //    {
            //        _terrainSystem.GetVegetationPackage().MaxCurveHeight = _terrainSystem.VegetationSystem.UnityTerrainData.MaxTerrainHeight;
            //        EditorUtility.SetDirty(_terrainSystem.GetVegetationPackage());
            //    }

            //    if (_terrainSystem.GetVegetationPackage().MaxCurveHeight < 1)
            //    {
            //        EditorGUILayout.HelpBox("You need to set or calculate terrain max height in order to set the max height value for the curves.", MessageType.Error);
            //    }
            //}
            //EditorGUILayout.HelpBox("Max curve height sets the height at max curve value. For easiest control of the curves it should be set to just above max height in current terrain.", MessageType.Info);
            //GUILayout.EndVertical();

            if (vegetationPackagePro.TerrainTextureCount > 0)
            {
                if (_terrainSystemPro.VegetationPackageTextureIndex > vegetationPackagePro.TerrainTextureCount) _terrainSystemPro.VegetationPackageTextureIndex = 0;
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Select terrain texture", LabelStyle);
                GUIContent[] textureImageButtons = new GUIContent[vegetationPackagePro.TerrainTextureSettingsList.Count];

                for (int i = 0; i <= vegetationPackagePro.TerrainTextureSettingsList.Count - 1; i++)
                {
                    if (_terrainSystemPro.VegetationSystemPro.ShowTerrainTextures)
                    {
                        textureImageButtons[i] = new GUIContent
                        {
                            image = AssetPreviewCache.GetAssetPreview(_terrainSystemPro.GetTerrainTexture(i))
                        };
                    }
                    else
                    {
                        textureImageButtons[i] = new GUIContent
                        {
                            image = AssetPreviewCache.GetAssetPreview(vegetationPackagePro.TerrainTextureList[i].Texture)
                        };
                    }
                }

                int imageWidth = 80;
                int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
                int rows = Mathf.CeilToInt((float)textureImageButtons.Length / columns);
                int gridHeight = (rows) * imageWidth;

                _terrainSystemPro.VegetationPackageTextureIndex = GUILayout.SelectionGrid(_terrainSystemPro.VegetationPackageTextureIndex, textureImageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    _terrainSystemPro.UpdateTerrainHeatmap();
                }

                //if (_terrainSystem.VegetationSystem)
                //{
                //    GUILayout.BeginVertical("box");
                //    EditorGUILayout.LabelField("Visualize height/steepness in terrain", LabelStyle);
                //    EditorGUI.BeginChangeCheck();
                //    bool overrideMaterial = EditorGUILayout.Toggle("Show heatmap", _terrainSystem.VegetationSystem.TerrainMaterialOverridden);
                //    EditorGUILayout.HelpBox("Enabling heatmap will show the spawn area of the selected texture based on the current height and steepness curves.", MessageType.Info);

                //    if (EditorGUI.EndChangeCheck())
                //    {
                //        if (overrideMaterial)
                //        {
                //            _terrainSystem.UpdateHeatmapMaterial(_currentTextureItem);
                //            _terrainSystem.VegetationSystem.OverrideTerrainMaterial(_terrainSystem.TerrainHeatMapMaterial);
                //            EditorUtility.SetDirty(_terrainSystem.VegetationSystem);
                //        }
                //        else
                //        {
                //            _terrainSystem.VegetationSystem.RestoreTerrainMaterial();
                //        }
                //    }
                //    GUILayout.EndVertical();

                //    _terrainSystem.UpdateHeatmapMaterial(_currentTextureItem);
                //}

                GUILayout.BeginVertical("box");

                TerrainTextureSettings terrainTextureSettings =
                    vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex];
                
                EditorGUI.BeginChangeCheck();
                vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex].Enabled = EditorGUILayout.Toggle("Use with auto splat generation", vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex].Enabled);

                EditorGUI.BeginDisabledGroup(
                    vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex].Enabled);
                
                vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex].LockTexture = EditorGUILayout.Toggle("Lock texture", vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex].LockTexture);
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.HelpBox("Locked textures are kept while generating splatmaps", MessageType.Info);
                                 
                _terrainSystemPro.ShowCurvesMenu =
                    VegetationPackageEditorTools.DrawHeader("Height/Steepness rules", _terrainSystemPro.ShowCurvesMenu);
                if (_terrainSystemPro.ShowCurvesMenu)
                {
                    EditorGUILayout.LabelField("Texture " + (_terrainSystemPro.VegetationPackageTextureIndex + 1).ToString() + " Height", LabelStyle, GUILayout.Width(150));                               
                    float height = vegetationSystemPro.VegetationSystemBounds.size.y - vegetationSystemPro.SeaLevel;

                    _heightCurveEditor.MaxValue = height;

                    if (_heightCurveEditor.EditCurve(vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex].TextureHeightCurve, this))
                    {
                        _terrainSystemPro.UpdateTerrainHeatmap();
                        EditorUtility.SetDirty(vegetationPackagePro);
                    }
                    EditorGUILayout.LabelField("Texture " + (_terrainSystemPro.VegetationPackageTextureIndex + 1).ToString() + " Steepness", LabelStyle, GUILayout.Width(150));

                
                    if (_steepnessCurveEditor.EditCurve(vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex].TextureSteepnessCurve, this))
                    {
                        _terrainSystemPro.UpdateTerrainHeatmap();
                        EditorUtility.SetDirty(vegetationPackagePro);
                    }
                };
                
                _terrainSystemPro.ShowNoiseMenu =
                    VegetationPackageEditorTools.DrawHeader("Noise/Concave/Convex rules", _terrainSystemPro.ShowNoiseMenu);
                if (_terrainSystemPro.ShowNoiseMenu)
                {
                    vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                        .UseNoise = EditorGUILayout.Toggle("Use perlin noise",
                        vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                            .UseNoise);
                    if (vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                        .UseNoise)
                    {
                        vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                            .InverseNoise = EditorGUILayout.Toggle("Inverse noise",
                            vegetationPackagePro
                                .TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                                .InverseNoise);
                        vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                            .NoiseScale = EditorGUILayout.Slider("Noise scale",
                            vegetationPackagePro
                                .TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex].NoiseScale,
                            1, 200f);
                        vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                            .NoiseOffset = EditorGUILayout.Vector2Field("Noise offset",
                            vegetationPackagePro
                                .TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                                .NoiseOffset);
                    }

                    vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                        .TextureWeight = EditorGUILayout.Slider("Texture weight",
                        vegetationPackagePro.TerrainTextureSettingsList[_terrainSystemPro.VegetationPackageTextureIndex]
                            .TextureWeight, 0, 5f);

                    terrainTextureSettings.ConcaveEnable =
                        EditorGUILayout.Toggle("Use concave rules", terrainTextureSettings.ConcaveEnable);
                    terrainTextureSettings.ConvexEnable =
                        EditorGUILayout.Toggle("Use convex rules", terrainTextureSettings.ConvexEnable);

                    if (terrainTextureSettings.ConcaveEnable || terrainTextureSettings.ConvexEnable)
                    {
                        terrainTextureSettings.ConcaveMode =
                            (ConcaveMode) EditorGUILayout.EnumPopup("Blend mode", terrainTextureSettings.ConcaveMode);
                        EditorGUILayout.HelpBox(
                            "Additive will add convex/concave areas in addition to areas selected by height/steepness. Blend will show areas that have all characteristics.",
                            MessageType.Info);
                        terrainTextureSettings.ConcaveAverage =
                            EditorGUILayout.Toggle("Average samples", terrainTextureSettings.ConcaveAverage);
                        terrainTextureSettings.ConcaveMinHeightDifference = EditorGUILayout.Slider(
                            "Min height difference", terrainTextureSettings.ConcaveMinHeightDifference, 0.1f, 10f);
                        terrainTextureSettings.ConcaveDistance = EditorGUILayout.Slider("Distance",
                            terrainTextureSettings.ConcaveDistance, 0f, 20f);
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    _terrainSystemPro.UpdateTerrainHeatmap();
                    EditorUtility.SetDirty(vegetationPackagePro);
                }
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox(
                    "Vegetation package has no textures",
                    MessageType.Warning);
                GUILayout.EndVertical();
            }
        }

        private void SetAllVegetationPackagesDirty()
        {
            for (int i = 0; i <= _terrainSystemPro.VegetationSystemPro.VegetationPackageProList.Count     - 1; i++)
            {
                EditorUtility.SetDirty(_terrainSystemPro.VegetationSystemPro.VegetationPackageProList[i]);
            }
        }

        private void DrawDebugInspector()
        {

        }
        
        void SetSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(_terrainSystemPro.gameObject.scene);
                EditorUtility.SetDirty(_terrainSystemPro);
            }
        }
    }
}

