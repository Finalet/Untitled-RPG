using System;
using System.Collections.Generic;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.External.CurveEditor;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem.Wind;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(VegetationSystemPro))]
    public class VegetationSystemProEditor : VegetationStudioProBaseEditor
    {
        private VegetationSystemPro _vegetationSystemPro;
        private int _vegIndex;
        private int _lastVegIndex;
        private int _selectedGridIndex;
        private int _selectedTerrainIndex;

        private int _excludeTerrainTextureIndex;
        private int _includeTerrainTextureIndex;

        private int _includeTextureMaskIndex;
        private int _excludeTextureMaskIndex;
        private int _scaleTextureMaskIndex;
        private int _densityTextureMaskIndex;

        private int _includeTextureMaskAddIndex;
        private int _excludeTextureMaskAddIndex;
        private int _scaleTextureMaskAddIndex;
        private int _densityTextureMaskAddIndex;
        private int _selectedVegetationTypeIndex;

        private TextureMaskType _selectedTextureMaskType;


        private Texture2D _dummyPreviewTexture;

        private InspectorCurveEditor _distanceFalloffCurveEditor;

        private InspectorCurveEditor _heightCurveEditor;
        private InspectorCurveEditor _steepnessCurveEditor;

        private static readonly string[] TabNames =
        {
            "Settings", "Cameras", "Terrains", "Vegetation", "Biomes", "Edit Biomes", "Environment", "Render",
            "Texture Masks", "Advanced", "Debug"
        };
        
        private string[] _navAreas;

        private static readonly string[] VegetationTypeNames =
        {
            "All", "Trees", "Large Objects", "Objects", "Plants", "Grass"
        };

        private static readonly string[] RgbaChannelStrings =
        {
            "R Channel", "G Channel", "B Channel", "A Channel"
        };

        void SetSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(_vegetationSystemPro.gameObject.scene);
                EditorUtility.SetDirty(_vegetationSystemPro);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _vegetationSystemPro = (VegetationSystemPro) target;

            if (GUILayout.Button("Refresh vegetation"))
            {
                _vegetationSystemPro.ClearCache();
                _vegetationSystemPro.RefreshTerrainHeightmap();
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.CurrentTabIndex = GUILayout.SelectionGrid(_vegetationSystemPro.CurrentTabIndex,
                TabNames, 3, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            switch (_vegetationSystemPro.CurrentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawCameraInspector();
                    break;
                case 2:
                    DrawTerrainInspector();
                    break;
                case 3:
                    DrawVegetationInspector();
                    break;
                case 4:
                    DrawBiomeEditor();
                    break;
                case 5:
                    DrawEditBiomesInspector();
                    break;
                case 6:
                    DrawEnvironmentInspector();
                    break;
                case 7:
                    DrawRenderInspector();
                    break;
                case 8:
                    DrawTextureMasksInspector();
                    break;
                case 9:
                    DrawAdvancedInspector();
                    break;
                case 10:
                    DrawDebugInspector();
                    break;
            }

            VegetationStudioManager.ShowBiomes = _vegetationSystemPro.CurrentTabIndex == 4;
        }

        void ConfirmBillboardColorSpace()
        {
            bool needRegenerate = false;

            for (int j = 0; j <= _vegetationSystemPro.VegetationPackageProList.Count - 1; j++)
            {
                VegetationPackagePro vegetationPackagePro = _vegetationSystemPro.VegetationPackageProList[j];

                for (int i = 0; i <= vegetationPackagePro.VegetationInfoList.Count - 1; i++)
                {
                    VegetationItemInfoPro vegetationItemInfo = vegetationPackagePro.VegetationInfoList[i];
                    if (vegetationItemInfo.VegetationType != VegetationType.Tree) continue;
                    if (vegetationItemInfo.UseBillboards == false) continue;
                    if (vegetationItemInfo.BillboardColorSpace == PlayerSettings.colorSpace) continue;
                    if (vegetationItemInfo.VegetationPrefab == null) continue;

                    needRegenerate = true;
                    break;
                }
            }

            if (needRegenerate)
            {
                if (EditorUtility.DisplayDialog("Vegetation Studio - Tree billboards",
                    "Tree billboards are not generated for the correct colorspace.", "Regenerate"))
                {
                    for (int j = 0; j <= _vegetationSystemPro.VegetationPackageProList.Count - 1; j++)
                    {
                        VegetationPackagePro vegetationPackagePro =
                            _vegetationSystemPro.VegetationPackageProList[j];
                        for (int i = 0; i <= vegetationPackagePro.VegetationInfoList.Count - 1; i++)
                        {
                            VegetationItemInfoPro vegetationItemInfo = vegetationPackagePro.VegetationInfoList[i];
                            EditorUtility.DisplayProgressBar(
                                "Regenerate billboard package #" + (j + 1) + " : " +
                                vegetationItemInfo.Name, "Generate image atlas",
                                (float) i / (vegetationPackagePro.VegetationInfoList.Count - 1));


                            if (vegetationItemInfo.VegetationType != VegetationType.Tree) continue;
                            if (vegetationItemInfo.UseBillboards == false) continue;

                            if (vegetationItemInfo.VegetationPrefab)
                            {
                                vegetationPackagePro.GenerateBillboard(vegetationItemInfo.VegetationItemID);
                            }
                        }

                        EditorUtility.SetDirty(vegetationPackagePro);
                    }

                    EditorUtility.ClearProgressBar();
                }
            }
        }


        bool VerifyTextureMaskGroupTexture(Texture2D texture, TextureMaskGroup textureMaskGroup)
        {
            if (!textureMaskGroup.RequiredTextureFormatList.Contains(texture.format))
            {
                string formatString = "";

                for (int i = 0; i <= textureMaskGroup.RequiredTextureFormatList.Count - 1; i++)
                {
                    formatString += textureMaskGroup.RequiredTextureFormatList[i].ToString() + " ";
                }

                EditorUtility.DisplayDialog("Texture format error",
                    "Texture format does not match the required uncompressed formats: " + formatString, "OK");
                return false;
            }

            if (AssetUtility.HasCrunchCompression(texture))
            {
                if (EditorUtility.DisplayDialog("Texture format error",
                    "Texture can not use crunch compression", "Fix", "Cancel"))
                {
                    AssetUtility.RemoveCrunchCompression(texture);
                }
                else
                {
                    return false;
                }
            }

            AssetUtility.SetTextureReadable(texture);
            return true;
        }

        void DrawAdvancedInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Predictive preloading", LabelStyle);
            _vegetationSystemPro.LoadPotentialVegetationCells = EditorGUILayout.Toggle("Load potential cells",
                _vegetationSystemPro.LoadPotentialVegetationCells);
            _vegetationSystemPro.PredictiveCellLoaderCellsPerFrame = EditorGUILayout.IntSlider("Cells per frame",
                _vegetationSystemPro.PredictiveCellLoaderCellsPerFrame, 1, 10);
            EditorGUILayout.HelpBox("The preditive loading will load additional cells around the camera before they become visisble. This will reduce the chance of multiple cells loaded in a single frame and divide spawning over time.", MessageType.Info);
            GUILayout.EndVertical();
        }

        void DrawTextureMasksInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select biome/vegetation package", LabelStyle);
            string[] packageNameList = new string[_vegetationSystemPro.VegetationPackageProList.Count];
            for (int i = 0; i <= _vegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                if (_vegetationSystemPro.VegetationPackageProList[i])
                {
                    packageNameList[i] = (i + 1).ToString() + " " +
                                         _vegetationSystemPro.VegetationPackageProList[i].PackageName+ " (" + _vegetationSystemPro.VegetationPackageProList[i].BiomeType.ToString() + ")";
                }
                else
                {
                    packageNameList[i] = "Not found";
                }
            }

            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.VegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package",
                _vegetationSystemPro.VegetationPackageIndex, packageNameList);
            if (EditorGUI.EndChangeCheck())
            {
            }

            EditorGUILayout.HelpBox("Select the biome to edit masks for.", MessageType.Info);
            GUILayout.EndVertical();

            VegetationPackagePro vegetationPackagePro =
                _vegetationSystemPro.VegetationPackageProList[_vegetationSystemPro.VegetationPackageIndex];

            if (_vegetationSystemPro.VegetationPackageProList.Count == 0)
            {
                _vegetationSystemPro.DebugTextureMask = null;
                return;
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add texture mask group", LabelStyle);
            GUILayout.BeginHorizontal();
            _selectedTextureMaskType =
                (TextureMaskType) EditorGUILayout.EnumPopup("Mask type", _selectedTextureMaskType);
            if (GUILayout.Button("Add mask group"))
            {
                TextureMaskGroup newTextureMaskGroup = new TextureMaskGroup(_selectedTextureMaskType);
                vegetationPackagePro.TextureMaskGroupList.Add(newTextureMaskGroup);
                _vegetationSystemPro.SelectedTextureMaskGroupIndex =
                    vegetationPackagePro.TextureMaskGroupList.Count - 1;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (vegetationPackagePro.TextureMaskGroupList.Count == 0)
            {
                EditorGUILayout.HelpBox("There are no Texture Masks in the biome. Press Add Mask to add. ",
                    MessageType.Info);
                _vegetationSystemPro.DebugTextureMask = null;
                return;
            }

            List<string> maskList = new List<string>();
            for (int i = 0; i <= vegetationPackagePro.TextureMaskGroupList.Count - 1; i++)
            {
                maskList.Add((i + 1) + ". " + vegetationPackagePro.TextureMaskGroupList[i].TextureMaskName + " - " +
                             vegetationPackagePro.TextureMaskGroupList[i].TextureMaskType);
            }

            if (_vegetationSystemPro.SelectedTextureMaskGroupIndex >
                vegetationPackagePro.TextureMaskGroupList.Count - 1)
                _vegetationSystemPro.SelectedTextureMaskGroupIndex =
                    vegetationPackagePro.TextureMaskGroupList.Count - 1;

            GUILayout.BeginVertical("box");
            _vegetationSystemPro.SelectedTextureMaskGroupIndex = EditorGUILayout.Popup("Select mask group",
                _vegetationSystemPro.SelectedTextureMaskGroupIndex, maskList.ToArray());
            TextureMaskGroup textureMaskGroup =
                vegetationPackagePro.TextureMaskGroupList[_vegetationSystemPro.SelectedTextureMaskGroupIndex];

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Delete Texture mask group", GUILayout.Width(200)))
            {
                vegetationPackagePro.DeleteTextureMaskGroup(textureMaskGroup);
                _vegetationSystemPro.SelectedTextureMaskGroupIndex = 0;
                _vegetationSystemPro.ClearCache();
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
                GUILayout.EndVertical();
                GUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();
            textureMaskGroup.TextureMaskName = EditorGUILayout.TextField("Name", textureMaskGroup.TextureMaskName);
            EditorGUILayout.LabelField("Add texture mask", LabelStyle);

            Texture2D newTexture = (Texture2D) EditorGUILayout.ObjectField("", null, typeof(Texture2D), false);

            if (EditorGUI.EndChangeCheck())
            {
                if (newTexture != null)
                {
                    if (VerifyTextureMaskGroupTexture(newTexture, textureMaskGroup))
                    {
                        TextureMask newTextureMask = new TextureMask
                        {
                            TextureRect =
                                RectExtension.CreateRectFromBounds(_vegetationSystemPro.VegetationSystemBounds),
                            MaskTexture = newTexture
                        };
                        textureMaskGroup.TextureMaskList.Add(newTextureMask);
                        _vegetationSystemPro.ClearCache();
                    }
                }

                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            if (textureMaskGroup.TextureMaskList.Count == 0)
            {
                GUILayout.EndVertical();
                GUILayout.EndVertical();
                _vegetationSystemPro.DebugTextureMask = null;
                return;
            }

            if (_vegetationSystemPro.SelectedTextureMaskGroupTextureIndex > textureMaskGroup.TextureMaskList.Count - 1)
                _vegetationSystemPro.SelectedTextureMaskGroupTextureIndex = 0;

            GUIContent[] textureImageButtons =
                new GUIContent[textureMaskGroup.TextureMaskList.Count];

            for (int i = 0; i <= textureMaskGroup.TextureMaskList.Count - 1; i++)
            {
                textureImageButtons[i] = new GUIContent
                {
                    image = AssetPreviewCache.GetAssetPreview(textureMaskGroup.TextureMaskList[i].MaskTexture)
                };
            }

            int imageWidth = 80;
            int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
            int rows = Mathf.CeilToInt((float) textureImageButtons.Length / columns);
            int gridHeight = (rows) * imageWidth;

            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.SelectedTextureMaskGroupTextureIndex = GUILayout.SelectionGrid(
                _vegetationSystemPro.SelectedTextureMaskGroupTextureIndex, textureImageButtons, columns,
                GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(vegetationPackagePro);
            }

            TextureMask textureMask =
                textureMaskGroup.TextureMaskList[_vegetationSystemPro.SelectedTextureMaskGroupTextureIndex];
            _vegetationSystemPro.DebugTextureMask = textureMask;

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Remove texture", GUILayout.Width(120)))
            {
                textureMask.Dispose();
                textureMaskGroup.TextureMaskList.Remove(textureMask);
                _vegetationSystemPro.SelectedTextureMaskGroupTextureIndex = 0;
                _vegetationSystemPro.ClearCache();
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
                GUILayout.EndVertical();
                GUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();
            textureMask.TextureRect = EditorGUILayout.RectField("Texture area", textureMask.TextureRect);
            textureMask.Repeat  = EditorGUILayout.Vector2Field("Repeat in area", textureMask.Repeat);            
            
            
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            string[] terrains = new string[_vegetationSystemPro.VegetationStudioTerrainList.Count];
            for (int i = 0; i <= _vegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
            {
                terrains[i] = _vegetationSystemPro.VegetationStudioTerrainObjectList[i].name;
            }

            _selectedTerrainIndex = EditorGUILayout.Popup("Select terrain", _selectedTerrainIndex, terrains);
            if (GUILayout.Button("Snap to terrain", GUILayout.Width(120)))
            {
                IVegetationStudioTerrain iVegetationStudioTerrain =
                    _vegetationSystemPro.VegetationStudioTerrainList[_selectedTerrainIndex];
                Bounds bounds = iVegetationStudioTerrain.TerrainBounds;
                textureMask.TextureRect =
                    new Rect(new Vector2(bounds.center.x - bounds.extents.x, bounds.center.z - bounds.extents.z),
                        new Vector2(bounds.size.z, bounds.size.z));
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Snap to world area"))
            {
                textureMask.TextureRect = new Rect(
                    new Vector2(
                        _vegetationSystemPro.VegetationSystemBounds.center.x -
                        _vegetationSystemPro.VegetationSystemBounds.extents.x,
                        _vegetationSystemPro.VegetationSystemBounds.center.z -
                        _vegetationSystemPro.VegetationSystemBounds.extents.z),
                    new Vector2(_vegetationSystemPro.VegetationSystemBounds.size.x,
                        _vegetationSystemPro.VegetationSystemBounds.size.z));
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            EditorGUILayout.HelpBox(
                "You can snap the texture rect to any added terrain, total world area or manually setting the rect.",
                MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                textureMask.Repeat  = new Vector2(Mathf.Clamp(textureMask.Repeat.x, 1,textureMask.Repeat.x), Mathf.Clamp(textureMask.Repeat.y,1,textureMask.Repeat.y));                
                _vegetationSystemPro.ClearCache();
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
void DrawEnvironmentInspector()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Snow", LabelStyle);
            _vegetationSystemPro.EnvironmentSettings.SnowAmount = EditorGUILayout.Slider("Snow amount",
                _vegetationSystemPro.EnvironmentSettings.SnowAmount, 0, 1);
            _vegetationSystemPro.EnvironmentSettings.SnowMinHeight = EditorGUILayout.Slider("Snow minimum height",
                _vegetationSystemPro.EnvironmentSettings.SnowMinHeight, 0, 1000);
            
            GUILayoutOption[] options = new GUILayoutOption[0];
            GUIContent snowLabel = new GUIContent("Snow color");
            GUIContent snowSpecularLabel = new GUIContent("Snow specular color");
            
            _vegetationSystemPro.EnvironmentSettings.SnowColor = EditorGUILayout.ColorField(snowLabel,_vegetationSystemPro.EnvironmentSettings.SnowColor,false,true,true,options);
            _vegetationSystemPro.EnvironmentSettings.SnowSpecularColor = EditorGUILayout.ColorField(snowSpecularLabel,_vegetationSystemPro.EnvironmentSettings.SnowSpecularColor,false,true,true,options);
            EditorGUILayout.HelpBox("Snow minimum height is relative to sea level.", MessageType.Info);
            EditorGUILayout.LabelField("Billboard", LabelStyle);
            _vegetationSystemPro.EnvironmentSettings.BillboardSnowColor = EditorGUILayout.ColorField("Billboard snow color",
                _vegetationSystemPro.EnvironmentSettings.BillboardSnowColor);
            _vegetationSystemPro.EnvironmentSettings.SnowBrightness = EditorGUILayout.Slider("Snow brightness",
                _vegetationSystemPro.EnvironmentSettings.SnowBrightness, 0, 2);
            _vegetationSystemPro.EnvironmentSettings.SnowBlendFactor = EditorGUILayout.Slider("Snow blend factor",
                _vegetationSystemPro.EnvironmentSettings.SnowBlendFactor, 1, 10);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Rain", LabelStyle);
            _vegetationSystemPro.EnvironmentSettings.RainAmount = EditorGUILayout.Slider("Rain amount",
                _vegetationSystemPro.EnvironmentSettings.RainAmount, 0, 1);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Wind", LabelStyle);
            _vegetationSystemPro.SelectedWindZone =
                (WindZone) EditorGUILayout.ObjectField("Wind Zone", _vegetationSystemPro.SelectedWindZone,
                    typeof(WindZone), true);
            _vegetationSystemPro.WindSpeedFactor =
                EditorGUILayout.Slider("Wind speed factor", _vegetationSystemPro.WindSpeedFactor, 0f, 5f);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.RefreshMaterials();
                SetSceneDirty();
            }

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i <= _vegetationSystemPro.WindControllerSettingsList.Count - 1; i++)
            {
                DrawWindControllerInspector(_vegetationSystemPro.WindControllerSettingsList[i]);
            }

            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
            }
        }

        void DrawWindControllerInspector(WindControllerSettings windControllerSettings)
        {
            if (windControllerSettings == null) return;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(windControllerSettings.Heading, LabelStyle);
            for (int i = 0; i <= windControllerSettings.ControlerPropertyList.Count - 1; i++)
            {
                DrawSerializedProperty(windControllerSettings.ControlerPropertyList[i]);
            }

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.UpdateWindSettings();
                _vegetationSystemPro.UpdateWind();
                SetSceneDirty();
            }
        }

        void DrawEditBiomesInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select biome/vegetation package", LabelStyle);
            string[] packageNameList = new string[_vegetationSystemPro.VegetationPackageProList.Count];
            for (int i = 0; i <= _vegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                if (_vegetationSystemPro.VegetationPackageProList[i])
                {
                    packageNameList[i] = (i + 1).ToString() + " " +
                                         _vegetationSystemPro.VegetationPackageProList[i].PackageName + " (" + _vegetationSystemPro.VegetationPackageProList[i].BiomeType.ToString() + ")";
                }
                else
                {
                    packageNameList[i] = "Not found";
                }
            }

            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.VegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package",
                _vegetationSystemPro.VegetationPackageIndex, packageNameList);
            if (EditorGUI.EndChangeCheck())
            {
            }

            EditorGUILayout.HelpBox("Select the biome to edit. Changes will be happen direct in the scene.",
                MessageType.Info);
            GUILayout.EndVertical();

            if (_vegetationSystemPro.VegetationPackageProList.Count == 0)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("No vegetation package available.", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            if (_vegetationSystemPro.VegetationPackageIndex > _vegetationSystemPro.VegetationPackageProList.Count - 1)
            {
                _vegetationSystemPro.VegetationPackageIndex = _vegetationSystemPro.VegetationPackageProList.Count - 1;
            }

            VegetationPackagePro vegetationPackagePro =
                _vegetationSystemPro.VegetationPackageProList[_vegetationSystemPro.VegetationPackageIndex];

            if (vegetationPackagePro == null) return;

            DrawVegetationPackageProGeneralSettings(vegetationPackagePro);
            if (DrawVegetationItemDropZone(vegetationPackagePro))
            {
                  return;
            }           

            if (vegetationPackagePro.VegetationInfoList.Count == 0) return;

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Select Vegetation Item", LabelStyle);

            EditorGUI.BeginChangeCheck();
            _selectedVegetationTypeIndex = GUILayout.SelectionGrid(_selectedVegetationTypeIndex, VegetationTypeNames, 3,
                EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                _selectedGridIndex = 0;
            }

            VegetationPackageEditorTools.VegetationItemTypeSelection vegetationItemTypeSelection =
                VegetationPackageEditorTools.GetVegetationItemTypeSelection(_selectedVegetationTypeIndex);

            int selectionCount = 0;

            VegetationPackageEditorTools.DrawVegetationItemSelector(_vegetationSystemPro, vegetationPackagePro,
                ref _selectedGridIndex, ref _vegIndex, ref selectionCount, vegetationItemTypeSelection, 70);

            //VegetationPackageEditorTools.DrawVegetationItemSelector(_vegetationSystemPro,vegetationPackagePro,
            //    ref _selectedGridIndex, ref _vegIndex, VegetationPackageEditorTools.VegetationItemTypeSelection.AllVegetationItems, 70);

            if (_lastVegIndex != _vegIndex) GUI.FocusControl(null);
            _lastVegIndex = _vegIndex;

            GUILayout.EndVertical();

            if (vegetationPackagePro.VegetationInfoList.Count == 0 ||
                _vegIndex >= vegetationPackagePro.VegetationInfoList.Count || selectionCount == 0) return;

            VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[_vegIndex];

            DrawActionMenu(vegetationPackagePro, vegetationItemInfoPro);

            if (vegetationPackagePro.VegetationInfoList.Count == 0) return;

            DrawPrefabSettingsMenu(vegetationPackagePro, vegetationItemInfoPro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);

            DrawPositionMenu(vegetationPackagePro, vegetationItemInfoPro, _vegetationSystemPro.VegetationPackageIndex,
                _vegIndex);

            DrawDistanceFalloffMenu(vegetationPackagePro, vegetationItemInfoPro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);

            DrawBillboardSettings(vegetationPackagePro, vegetationItemInfoPro);

            DrawLODMenu(vegetationPackagePro, vegetationItemInfoPro, _vegetationSystemPro.VegetationPackageIndex,
                _vegIndex);

            DrawColliderSettingsMenu(vegetationPackagePro, vegetationItemInfoPro);

            DrawShaderSettingsMenu(vegetationPackagePro, vegetationItemInfoPro);

            DrawNoiseSettingMenu(vegetationPackagePro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);

            DrawBiomeRulesMenu(vegetationPackagePro, vegetationItemInfoPro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);

            DrawConcaveLocationRulesMenu(vegetationPackagePro, vegetationItemInfoPro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);

            DrawTerrainTextureRulesMenu(vegetationPackagePro, vegetationItemInfoPro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);

            DrawTextureMaskRulesMenu(vegetationPackagePro, vegetationItemInfoPro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);

            DrawVegetationMaskRulesMenu(vegetationPackagePro, vegetationItemInfoPro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);

            DrawTerrainSourceSettingsMenu(vegetationPackagePro, vegetationItemInfoPro,
                _vegetationSystemPro.VegetationPackageIndex, _vegIndex);
        }

        void DrawLODMenu(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro,
            int vegetationPackageIndex, int vegetationItemIndex)
        {
            VegetationItemModelInfo vegetationItemModelInfo =
                _vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackageIndex, vegetationItemIndex);
            if (vegetationItemModelInfo == null) return;
            if (vegetationItemModelInfo.LODCount < 2) return;

            _vegetationSystemPro.ShowLODMenu =
                VegetationPackageEditorTools.DrawHeader("LOD Settings",
                    _vegetationSystemPro.ShowLODMenu);
            if (_vegetationSystemPro.ShowLODMenu)
            {
                float billboardStartDistance = _vegetationSystemPro.VegetationSettings.GetVegetationDistance();
                if (vegetationItemInfoPro.VegetationType == VegetationType.Tree ||
                    vegetationItemInfoPro.VegetationType == VegetationType.LargeObjects)
                {
                    billboardStartDistance = _vegetationSystemPro.VegetationSettings.GetTreeDistance();
                }

                GUILayout.BeginVertical("box");
                EditorGUI.BeginChangeCheck();

                //if (vegetationItemInfo.VegetationRenderType == VegetationRenderType.InstancedIndirect &&
                //    !_vegetationSystem.UseIndirectLoDs)
                //{
                //    EditorGUILayout.HelpBox("Instanced indirect LODs are disabled on the render tab.", MessageType.Warning);
                //}

                EditorGUILayout.LabelField("Number of LODs: " + vegetationItemModelInfo.LODCount, BasicLabelStyle);
                vegetationItemInfoPro.DisableLOD =
                    EditorGUILayout.Toggle("Disable LODs", vegetationItemInfoPro.DisableLOD);
                vegetationItemInfoPro.LODFactor = EditorGUILayout.Slider("LOD distance factor",
                    vegetationItemInfoPro.LODFactor, 0.15f, 20f);

                float currentLOD1Distance = vegetationItemModelInfo.LOD1Distance * QualitySettings.lodBias *
                                            vegetationItemInfoPro.LODFactor;
                float currentLOD2Distance = vegetationItemModelInfo.LOD2Distance * QualitySettings.lodBias *
                                            vegetationItemInfoPro.LODFactor;

                //TUDO support 4 LODS for visualization
                VegetationPackageEditorTools.DrawLODRanges(currentLOD1Distance, currentLOD2Distance,
                    billboardStartDistance);

                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        bool DrawVegetationItemDropZone(VegetationPackagePro vegetationPackagePro)
        {
            _vegetationSystemPro.ShowAddVegetationItemMenu =
                VegetationPackageEditorTools.DrawHeader("Add Vegetation Items",
                    _vegetationSystemPro.ShowAddVegetationItemMenu);
            if (_vegetationSystemPro.ShowAddVegetationItemMenu)
            {
                EditorGUILayout.HelpBox(
                    "Drop a Prefab with the vegetation item here to create new vegetation item. Drop on selected category to get initial rules and settings correct.",
                    MessageType.Info);

                bool addedItem = false;
                GUILayout.BeginHorizontal();
                DropZoneTools.DrawVegetationItemDropZone(DropZoneType.GrassPrefab, vegetationPackagePro, ref addedItem);
                DropZoneTools.DrawVegetationItemDropZone(DropZoneType.PlantPrefab, vegetationPackagePro, ref addedItem);
                DropZoneTools.DrawVegetationItemDropZone(DropZoneType.TreePrefab, vegetationPackagePro, ref addedItem);
                DropZoneTools.DrawVegetationItemDropZone(DropZoneType.ObjectPrefab, vegetationPackagePro,
                    ref addedItem);
                DropZoneTools.DrawVegetationItemDropZone(DropZoneType.LargeObjectPrefab, vegetationPackagePro,
                    ref addedItem);
                GUILayout.EndHorizontal();

                EditorGUILayout.HelpBox(
                    "Drop a Texture2D to create a new grass or plant vegetation item. Drop on selected category to get initial rules and settings correct.",
                    MessageType.Info);

                GUILayout.BeginHorizontal();
                DropZoneTools.DrawVegetationItemDropZone(DropZoneType.GrassTexture, vegetationPackagePro,
                    ref addedItem);
                DropZoneTools.DrawVegetationItemDropZone(DropZoneType.PlantTexture, vegetationPackagePro,
                    ref addedItem);
                GUILayout.EndHorizontal();

                if (addedItem)
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                    _vegetationSystemPro.RefreshVegetationSystem();
                    _vegIndex = vegetationPackagePro.VegetationInfoList.Count - 1;
                    _selectedGridIndex = GetSelectedGridIndex(_vegIndex, vegetationPackagePro);
                }

                if (VegetationStudioManager.GetVegetationItemFromClipboard() != null)
                {
                    if (GUILayout.Button("Paste vegetation item"))
                    {
                        vegetationPackagePro.DuplicateVegetationItem(VegetationStudioManager
                            .GetVegetationItemFromClipboard());
                        _vegIndex = vegetationPackagePro.VegetationInfoList.Count - 1;
                        _selectedGridIndex = GetSelectedGridIndex(_vegIndex, vegetationPackagePro);
                        _vegetationSystemPro.RefreshVegetationSystem();
                        addedItem = true;
                    }
                }

                return addedItem;
            }

            return false;
        }

        int GetSelectedGridIndex(int vegIndex, VegetationPackagePro vegetationPackagePro)
        {
            List<int> vegetationItemIndexList = new List<int>();
            for (int i = 0;
                i <= vegetationPackagePro
                    .VegetationInfoList.Count - 1;
                i++)
            {
                vegetationItemIndexList.Add(i);
            }

            VegetationInfoComparer vIc = new VegetationInfoComparer
            {
                VegetationInfoList = vegetationPackagePro
                    .VegetationInfoList
            };
            vegetationItemIndexList.Sort(vIc.Compare);

            for (int i = 0; i <= vegetationItemIndexList.Count - 1; i++)
            {
                if (vegetationPackagePro.VegetationInfoList[vegetationItemIndexList[i]].VegetationItemID ==
                    vegetationPackagePro.VegetationInfoList[vegIndex].VegetationItemID)
                {
                    return i;
                }
            }

            return 0;
        }

        void DrawActionMenu(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro)
        {
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete selected item"))
            {
                if (EditorUtility.DisplayDialog("Delete VegetationItem?",
                    "Do you want to delete the selected VegetationItem?", "Delete", "Cancel"))
                {
                    vegetationPackagePro.VegetationInfoList.RemoveAt(_vegIndex);
                    _vegetationSystemPro.RefreshVegetationSystem();
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                    _vegIndex = 0;
                    return;
                }
            }

            if (GUILayout.Button("Copy selected item"))
            {
                VegetationStudioManager.AddVegetationItemToClipboard(vegetationItemInfoPro);
                return;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        void DrawShaderSettingsMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro)
        {
            if (vegetationItemInfoPro.ShaderControllerSettings == null) return;
            if (vegetationItemInfoPro.ShaderControllerSettings.ControlerPropertyList.Count == 0) return;

            _vegetationSystemPro.ShowShaderSettingsMenu =
                VegetationPackageEditorTools.DrawHeader(vegetationItemInfoPro.ShaderControllerSettings.Heading,
                    _vegetationSystemPro.ShowShaderSettingsMenu);

            if (_vegetationSystemPro.ShowShaderSettingsMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                for (int i = 0;
                    i <= vegetationItemInfoPro.ShaderControllerSettings.ControlerPropertyList.Count - 1;
                    i++)
                {
                    SerializedControllerProperty serializedControllerProperty =
                        vegetationItemInfoPro.ShaderControllerSettings.ControlerPropertyList[i];
                    DrawSerializedProperty(serializedControllerProperty);
                }

                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.RefreshMaterials();
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        void DrawSerializedProperty(SerializedControllerProperty serializedControllerProperty)
        {
            switch (serializedControllerProperty.SerializedControlerPropertyType)
            {
                case SerializedControlerPropertyType.Integer:
                    serializedControllerProperty.IntValue = EditorGUILayout.IntSlider(
                        serializedControllerProperty.PropertyDescription, serializedControllerProperty.IntValue,
                        serializedControllerProperty.IntMinValue, serializedControllerProperty.IntMaxValue);
                    break;
                case SerializedControlerPropertyType.Float:
                    serializedControllerProperty.FloatValue = EditorGUILayout.Slider(
                        serializedControllerProperty.PropertyDescription, serializedControllerProperty.FloatValue,
                        serializedControllerProperty.FloatMinValue, serializedControllerProperty.FloatMaxValue);
                    break;
                case SerializedControlerPropertyType.RgbaSelector:
                    DrawRgbaChannelSelector(serializedControllerProperty);
                    break;
                case SerializedControlerPropertyType.DropDownStringList:
                    DropdownStringListSelector(serializedControllerProperty);
                    break;
                case SerializedControlerPropertyType.Boolean:
                    serializedControllerProperty.BoolValue = EditorGUILayout.Toggle(
                        serializedControllerProperty.PropertyDescription,
                        serializedControllerProperty.BoolValue);
                    break;
                case SerializedControlerPropertyType.ColorSelector:
                    serializedControllerProperty.ColorValue = EditorGUILayout.ColorField(
                        serializedControllerProperty.PropertyDescription,
                        serializedControllerProperty.ColorValue);
                    break;
                case SerializedControlerPropertyType.Label:
                    EditorGUILayout.LabelField(serializedControllerProperty.PropertyDescription, LabelStyle);
                    break;
                case SerializedControlerPropertyType.Texture2D:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(serializedControllerProperty.PropertyDescription, BasicLabelStyle);
                    serializedControllerProperty.TextureValue = (Texture2D) EditorGUILayout.ObjectField(
                        serializedControllerProperty.TextureValue, typeof(Texture2D), false, GUILayout.Height(64),
                        GUILayout.Width(64));
                    EditorGUILayout.EndHorizontal();
                    break;
            }

            if (!string.IsNullOrEmpty(serializedControllerProperty.PropertyInfo))
            {
                EditorGUILayout.HelpBox(serializedControllerProperty.PropertyInfo, MessageType.Info);
            }
        }

        void DropdownStringListSelector(SerializedControllerProperty serializedControllerProperty)
        {
            serializedControllerProperty.IntValue = EditorGUILayout.Popup(
                serializedControllerProperty.PropertyDescription, serializedControllerProperty.IntValue,
                serializedControllerProperty.StringList.ToArray());
        }

        void DrawRgbaChannelSelector(SerializedControllerProperty serializedControllerProperty)
        {
            serializedControllerProperty.IntValue = EditorGUILayout.Popup(
                serializedControllerProperty.PropertyDescription, serializedControllerProperty.IntValue,
                RgbaChannelStrings);
        }

        void DrawVegetationMaskRulesMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro,
            int vegetationPackageIndex, int vegetationItemIndex)
        {
            _vegetationSystemPro.ShowVegetationMaskRulesMenu =
                VegetationPackageEditorTools.DrawHeader("Vegetation mask rules",
                    _vegetationSystemPro.ShowVegetationMaskRulesMenu);

            if (_vegetationSystemPro.ShowVegetationMaskRulesMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                vegetationItemInfoPro.UseVegetationMask = EditorGUILayout.Toggle("Use with vegetation mask",
                    vegetationItemInfoPro.UseVegetationMask);
                if (vegetationItemInfoPro.UseVegetationMask)
                {
                    vegetationItemInfoPro.VegetationTypeIndex =
                        (VegetationTypeIndex) EditorGUILayout.EnumPopup("Vegetation type",
                            vegetationItemInfoPro.VegetationTypeIndex);
                }

                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        void DrawTextureMaskRulesMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro,
            int vegetationPackageIndex, int vegetationItemIndex)
        {
            _vegetationSystemPro.ShowTextureMaskRulesMenu =
                VegetationPackageEditorTools.DrawHeader("Texture mask rules",
                    _vegetationSystemPro.ShowTextureMaskRulesMenu);

            if (_vegetationSystemPro.ShowTextureMaskRulesMenu)
            {
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskIncludeRules = EditorGUILayout.Toggle(
                    "Use texture mask include rules",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskIncludeRules);
                bool updatedInclude = false;

                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskIncludeRules)
                {
                    DrawTextureMaskRules(vegetationPackagePro, out updatedInclude, TextureMaskRuleType.Include,
                        vegetationItemInfoPro.TextureMaskIncludeRuleList, ref _includeTextureMaskIndex, ref _includeTextureMaskAddIndex);
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskExcludeRules = EditorGUILayout.Toggle(
                    "Use texture mask exclude rules",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskExcludeRules);
                bool updatedExclude = false;

                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskExcludeRules)
                {
                    DrawTextureMaskRules(vegetationPackagePro, out updatedExclude, TextureMaskRuleType.Exclude,
                        vegetationItemInfoPro.TextureMaskExcludeRuleList, ref _excludeTextureMaskIndex, ref _excludeTextureMaskAddIndex);
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskScaleRules = EditorGUILayout.Toggle(
                    "Use texture mask scale rules",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskScaleRules);
                bool updatedScale = false;

                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskScaleRules)
                {
                    DrawTextureMaskRules(vegetationPackagePro, out updatedScale, TextureMaskRuleType.Scale,
                        vegetationItemInfoPro.TextureMaskScaleRuleList, ref _scaleTextureMaskIndex, ref _scaleTextureMaskAddIndex);
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskDensityRules = EditorGUILayout.Toggle(
                    "Use texture mask density rules",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskDensityRules);
                bool updatedDensity = false;

                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTextureMaskDensityRules)
                {
                    DrawTextureMaskRules(vegetationPackagePro, out updatedDensity, TextureMaskRuleType.Density,
                        vegetationItemInfoPro.TextureMaskDensityRuleList, ref _densityTextureMaskIndex, ref _densityTextureMaskAddIndex);
                }

                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck() || updatedExclude || updatedInclude || updatedScale || updatedDensity)
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        void DrawTextureMaskRules(VegetationPackagePro vegetationPackagePro, out bool updated,
            TextureMaskRuleType textureMaskRuleType, List<TextureMaskRule> textureMaskRuleList, ref int index, ref int addIndex)
        {
            updated = false;

            if (vegetationPackagePro.TextureMaskGroupList.Count > 0)
            {
                GUILayout.BeginVertical("box");
                
                List<string> textureMaskGroupStringList = new List<string>();
                for (int i = 0; i <= vegetationPackagePro.TextureMaskGroupList.Count - 1; i++)
                {
                    TextureMaskGroup textureMaskGroup = vegetationPackagePro.TextureMaskGroupList[i];
                    textureMaskGroupStringList.Add((i + 1) + ". " + vegetationPackagePro.TextureMaskGroupList[i].TextureMaskName + " - " +
                        vegetationPackagePro.TextureMaskGroupList[i].TextureMaskType);
                }

                if (addIndex >= textureMaskGroupStringList.Count) addIndex = 0;
                
                addIndex = EditorGUILayout.Popup("Select texture mask group", addIndex, textureMaskGroupStringList.ToArray());
                
                if (GUILayout.Button("Add new rule"))
                {
                    TextureMaskGroup textureMaskGroup = vegetationPackagePro.TextureMaskGroupList[addIndex];
                    TextureMaskRule textureMaskRule =
                        new TextureMaskRule(textureMaskGroup.Settings)
                            {TextureMaskGroupID = textureMaskGroup.TextureMaskGroupID};
                    textureMaskRuleList.Add(textureMaskRule);
                    index = textureMaskRuleList.Count - 1;
                    updated = true;
                }
                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "You need to add a TextureMaskGroup to the Vegetation Package before setting up a rule.",
                    MessageType.Warning);
            }

            DrawTextureMaskRuleIconSelector(textureMaskRuleList, vegetationPackagePro,
                ref index);

            if (index < textureMaskRuleList.Count)
            {
                GUILayout.BeginVertical("box");

                TextureMaskRule textureMaskRule = textureMaskRuleList[index];

                if (GUILayout.Button("Remove rule", GUILayout.Width(120)))
                {
                    textureMaskRuleList.RemoveAt(index);
                    updated = true;
                    GUILayout.EndVertical();
                    return;
                }

                for (int i = 0; i <= textureMaskRule.TextureMaskPropertiesList.Count - 1; i++)
                {
                    DrawSerializedProperty(textureMaskRule.TextureMaskPropertiesList[i]);
                }

                if (textureMaskRuleType == TextureMaskRuleType.Include ||
                    textureMaskRuleType == TextureMaskRuleType.Exclude)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Min/Max mask density", GUILayout.MaxWidth(196));
                    textureMaskRule.MinDensity = EditorGUILayout.FloatField(textureMaskRule.MinDensity);
                    EditorGUILayout.MinMaxSlider(ref textureMaskRule.MinDensity, ref textureMaskRule.MaxDensity, 0.01f,
                        1f);
                    textureMaskRule.MaxDensity = EditorGUILayout.FloatField(textureMaskRule.MaxDensity);
                    GUILayout.EndHorizontal();
                }

                if (textureMaskRuleType == TextureMaskRuleType.Density)
                {
                    textureMaskRule.DensityMultiplier =
                        EditorGUILayout.Slider("Density multiplier", textureMaskRule.DensityMultiplier, 0, 1);
                }

                if (textureMaskRuleType == TextureMaskRuleType.Scale)
                {
                    textureMaskRule.ScaleMultiplier =
                        EditorGUILayout.Slider("Scale multiplier", textureMaskRule.ScaleMultiplier, 0, 1);
                }

                GUILayout.EndVertical();
            }
        }

        void DrawTextureMaskRuleIconSelector(List<TextureMaskRule> terrainTextureRuleList,
            VegetationPackagePro vegetationPackagePro, ref int index)
        {
            GUIContent[] textureImageButtons = new GUIContent[terrainTextureRuleList.Count];

            for (int i = 0; i <= terrainTextureRuleList.Count - 1; i++)
            {
                TextureMaskRule textureMaskRule = terrainTextureRuleList[i];
                TextureMaskGroup textureMaskGroup =
                    vegetationPackagePro.GetTextureMaskGroup(textureMaskRule.TextureMaskGroupID);

                if (textureMaskGroup != null)
                {
                    Texture2D previewTexture = textureMaskGroup.GetPreviewTexture();

                    if (previewTexture == null)
                    {
                        textureImageButtons[i] = new GUIContent
                        {
                            image = _dummyPreviewTexture
                        };
                    }
                    else
                    {
                        textureImageButtons[i] = new GUIContent
                        {
                            image = AssetPreview.GetAssetPreview(previewTexture)
                        };
                    }
                }
                else
                {
                    textureImageButtons[i] = new GUIContent {image = _dummyPreviewTexture};
                }
            }

            if (textureImageButtons.Length > 0)
            {
                int imageWidth = 80;
                int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
                int rows = Mathf.CeilToInt((float) textureImageButtons.Length / columns);
                int gridHeight = (rows) * imageWidth;
                if (index > textureImageButtons.Length - 1) index = 0;
                index = GUILayout.SelectionGrid(index, textureImageButtons, columns,
                    GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
            }
        }

        void DrawTerrainTextureRulesMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro,
            int vegetationPackageIndex, int vegetationItemIndex)
        {
            _vegetationSystemPro.ShowTerrainTextureRulesMenu =
                VegetationPackageEditorTools.DrawHeader("Terrain texture rules",
                    _vegetationSystemPro.ShowTerrainTextureRulesMenu);

            if (_vegetationSystemPro.ShowTerrainTextureRulesMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainTextureIncludeRules =
                    EditorGUILayout.Toggle(
                        "Use include terrain texture rules",
                        vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainTextureIncludeRules);
                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainTextureIncludeRules)
                {
                    DrawIncludeTerrainTextures(vegetationItemInfoPro);
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainTextureExcludeRules =
                    EditorGUILayout.Toggle(
                        "Use exclude terrain texture rules",
                        vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainTextureExcludeRules);
                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainTextureExcludeRules)
                {
                    DrawExcludeTerrainTextures(vegetationItemInfoPro);
                }

                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainTextureIncludeRules ||
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainTextureExcludeRules)
                {
                    EditorGUILayout.HelpBox("Terrain texture rules will only work for Unity terrains.",
                        MessageType.Info);
                    EditorGUILayout.HelpBox(
                        "In the case of multiple Unity terrains added to the vegetation system the preview textures shown are from the first terrain.",
                        MessageType.Info);
                }
            }
        }

        void DrawExcludeTerrainTextures(VegetationItemInfoPro vegetationItemInfoPro)
        {
            if (GUILayout.Button("Add new item"))
            {
                TerrainTextureRule newTextureMaskInfo = new TerrainTextureRule
                {
                    MinimumValue = 0.1f,
                    MaximumValue = 1,
                    TextureIndex = 0
                };

                vegetationItemInfoPro.TerrainTextureExcludeRuleList.Add(newTextureMaskInfo);
                _excludeTerrainTextureIndex = vegetationItemInfoPro.TerrainTextureExcludeRuleList.Count - 1;
            }

            GUIContent[] textureImageButtons =
                new GUIContent[vegetationItemInfoPro.TerrainTextureExcludeRuleList.Count];

            for (int i = 0; i <= vegetationItemInfoPro.TerrainTextureExcludeRuleList.Count - 1; i++)
            {
                TerrainTextureRule tempMaskInfo = vegetationItemInfoPro.TerrainTextureExcludeRuleList[i];

                textureImageButtons[i] = new GUIContent {image = GetTerrainPreviewTexture(tempMaskInfo.TextureIndex)};
            }

            if (textureImageButtons.Length > 0)
            {
                GUILayout.EndVertical();
                int imageWidth = 80;
                int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50f) / imageWidth);
                int rows = Mathf.CeilToInt((float) textureImageButtons.Length / columns);
                int gridHeight = (rows) * imageWidth;
                if (_excludeTerrainTextureIndex > textureImageButtons.Length - 1) _excludeTerrainTextureIndex = 0;
                _excludeTerrainTextureIndex = GUILayout.SelectionGrid(_excludeTerrainTextureIndex, textureImageButtons,
                    columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
                GUILayout.BeginVertical("box");

                GUILayout.BeginVertical("box");

                if (GUILayout.Button("Delete selected item"))
                {
                    vegetationItemInfoPro.TerrainTextureExcludeRuleList.RemoveAt(_excludeTerrainTextureIndex);
                    return;
                }

                TerrainTextureRule tempMaskInfo =
                    vegetationItemInfoPro.TerrainTextureExcludeRuleList[_excludeTerrainTextureIndex];
                tempMaskInfo.TextureIndex = (int) (TerrainTextureType) EditorGUILayout.EnumPopup("Selected texture",
                    (TerrainTextureType) tempMaskInfo.TextureIndex);

                float minDensity = tempMaskInfo.MinimumValue;
                float maxDensity = tempMaskInfo.MaximumValue;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Min/Max Texture Placement Density", GUILayout.MaxWidth(196));
                minDensity = EditorGUILayout.FloatField(minDensity);
                EditorGUILayout.MinMaxSlider(ref minDensity, ref maxDensity, 0.01f, 1f);
                maxDensity = EditorGUILayout.FloatField(maxDensity);
                GUILayout.EndHorizontal();
                tempMaskInfo.MinimumValue = minDensity;
                tempMaskInfo.MaximumValue = maxDensity;
                GUILayout.EndVertical();
            }
        }

        void DrawIncludeTerrainTextures(VegetationItemInfoPro vegetationItemInfoPro)
        {
            if (GUILayout.Button("Add new item"))
            {
                TerrainTextureRule newTextureMaskInfo = new TerrainTextureRule
                {
                    MinimumValue = 0.1f,
                    MaximumValue = 1,
                    TextureIndex = 0
                };

                vegetationItemInfoPro.TerrainTextureIncludeRuleList.Add(newTextureMaskInfo);
                _includeTerrainTextureIndex = vegetationItemInfoPro.TerrainTextureIncludeRuleList.Count - 1;
            }

            GUIContent[] textureImageButtons =
                new GUIContent[vegetationItemInfoPro.TerrainTextureIncludeRuleList.Count];

            for (int i = 0; i <= vegetationItemInfoPro.TerrainTextureIncludeRuleList.Count - 1; i++)
            {
                TerrainTextureRule tempMaskInfo = vegetationItemInfoPro.TerrainTextureIncludeRuleList[i];

                textureImageButtons[i] = new GUIContent {image = GetTerrainPreviewTexture(tempMaskInfo.TextureIndex)};
            }

            if (textureImageButtons.Length > 0)
            {
                GUILayout.EndVertical();
                int imageWidth = 80;
                int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50f) / imageWidth);
                int rows = Mathf.CeilToInt((float) textureImageButtons.Length / columns);
                int gridHeight = (rows) * imageWidth;
                if (_includeTerrainTextureIndex > textureImageButtons.Length - 1) _includeTerrainTextureIndex = 0;
                _includeTerrainTextureIndex = GUILayout.SelectionGrid(_includeTerrainTextureIndex, textureImageButtons,
                    columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
                GUILayout.BeginVertical("box");

                GUILayout.BeginVertical("box");

                if (GUILayout.Button("Delete selected item"))
                {
                    vegetationItemInfoPro.TerrainTextureIncludeRuleList.RemoveAt(_includeTerrainTextureIndex);
                    return;
                }

                TerrainTextureRule tempMaskInfo =
                    vegetationItemInfoPro.TerrainTextureIncludeRuleList[_includeTerrainTextureIndex];
                tempMaskInfo.TextureIndex = (int) (TerrainTextureType) EditorGUILayout.EnumPopup("Selected texture",
                    (TerrainTextureType) tempMaskInfo.TextureIndex);

                float minDensity = tempMaskInfo.MinimumValue;
                float maxDensity = tempMaskInfo.MaximumValue;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Min/Max Texture Placement Density", GUILayout.MaxWidth(196));
                minDensity = EditorGUILayout.FloatField(minDensity);
                EditorGUILayout.MinMaxSlider(ref minDensity, ref maxDensity, 0.01f, 1f);
                maxDensity = EditorGUILayout.FloatField(maxDensity);
                GUILayout.EndHorizontal();
                tempMaskInfo.MinimumValue = minDensity;
                tempMaskInfo.MaximumValue = maxDensity;
                GUILayout.EndVertical();
            }
        }

        Texture2D GetTerrainPreviewTexture(int textureIndex)
        {
            for (int i = 0; i <= _vegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
            {
                IVegetationStudioTerrain iVegetationStudioTerrain = _vegetationSystemPro.VegetationStudioTerrainList[i];
                var unityTerrain = iVegetationStudioTerrain as UnityTerrain;
                if (unityTerrain != null)
                {
                    Texture2D previewTexture = unityTerrain.GetTerrainPreviewTexture(textureIndex);

                    if (previewTexture)
                    {
                        return previewTexture;
                    }
                    else
                    {
                        return _dummyPreviewTexture;
                    }
                }
            }

            return _dummyPreviewTexture;
        }

        void DrawNoiseSettingMenu(VegetationPackagePro vegetationPackagePro, int vegetationPackageIndex,
            int vegetationItemIndex)
        {
            _vegetationSystemPro.ShowVegetationPackageNoiseMenu =
                VegetationPackageEditorTools.DrawHeader("Noise rules",
                    _vegetationSystemPro.ShowVegetationPackageNoiseMenu);

            if (_vegetationSystemPro.ShowVegetationPackageNoiseMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseCutoff = EditorGUILayout.Toggle(
                    "Use perlin noise cutoff",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseCutoff);
                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseCutoff)
                {
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseCutoffValue = EditorGUILayout.Slider("Perlin noise cutoff",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseCutoffValue, 0f, 1f);
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseCutoffScale = EditorGUILayout.Slider("Perlin noise scale",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseCutoffScale, 1f, 500f);
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseCutoffOffset = EditorGUILayout.Vector2Field("Perlin noise offset",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseCutoffOffset);
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseCutoffInverse = EditorGUILayout.Toggle("Inverse perlin noise",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseCutoffInverse);
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseDensity = EditorGUILayout.Toggle(
                    "Use perlin noise density",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseDensity);
                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseDensity)
                {
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseDensityScale = EditorGUILayout.Slider("Perlin noise scale",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseDensityScale, 1f, 500f);
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseDensityOffset = EditorGUILayout.Vector2Field("Perlin noise offset",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseDensityOffset);
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseDensityInverse = EditorGUILayout.Toggle("Inverse perlin noise",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseDensityInverse);
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseScaleRule = EditorGUILayout.Toggle(
                    "Use perlin noise scale",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseScaleRule);
                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseNoiseScaleRule)
                {
                    EditorFunctions.FloatRangeField("Min/Max scale",
                        ref vegetationPackagePro.VegetationInfoList[_vegIndex].NoiseScaleMinScale,
                        ref vegetationPackagePro.VegetationInfoList[_vegIndex].NoiseScaleMaxScale, -0.1f, 5);

                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseScaleScale = EditorGUILayout.Slider("Perlin noise scale",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseScaleScale, 1f, 500f);
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseScaleOffset = EditorGUILayout.Vector2Field("Perlin noise offset",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseScaleOffset);
                    vegetationPackagePro.VegetationInfoList[_vegIndex]
                        .NoiseScaleInverse = EditorGUILayout.Toggle("Inverse perlin noise",
                        vegetationPackagePro.VegetationInfoList[_vegIndex]
                            .NoiseScaleInverse);
                }

                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        //struct MenuItemInfo
        //{
        //    public VegetationItemInfoPro VegetationItemInfoPro;
        //    public VegetationPackagePro VegetationPackagePro;
        //    public int VegetationPackageIndex;
        //    public int VegetationItemIndex;
        //    public int Index;
        //}

        void DrawTerrainSourceSettingsMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro, int vegetationPackageIndex, int vegetationItemIndex)
        {
            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.ShowTerrainSourceSettingsMenu =
                VegetationPackageEditorTools.DrawHeader("Terrain source rules",
                    _vegetationSystemPro.ShowTerrainSourceSettingsMenu);

            if (_vegetationSystemPro.ShowTerrainSourceSettingsMenu)
            {
                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainSourceIncludeRule = EditorGUILayout.Toggle(
                    "Use terrain source include rule",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainSourceIncludeRule);
                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainSourceIncludeRule)
                {
                    //if (GUILayout.Button("Select Include terrain source IDs"))
                    //{
                    //    GenericMenu menu = new GenericMenu();
                    //    menu.AddItem(new GUIContent("TerrainSourceID 1"), vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID1, OnTerrainSourceIncludeSelected, new MenuItemInfo() { Index = 0, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 2"), vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID2, OnTerrainSourceIncludeSelected, new MenuItemInfo() { Index = 1, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 3"), vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID3, OnTerrainSourceIncludeSelected, new MenuItemInfo() { Index = 2, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 4"), vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID4, OnTerrainSourceIncludeSelected, new MenuItemInfo() { Index = 3, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 5"), vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID5, OnTerrainSourceIncludeSelected, new MenuItemInfo() { Index = 4, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 6"), vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID6, OnTerrainSourceIncludeSelected, new MenuItemInfo() { Index = 5, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 7"), vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID7, OnTerrainSourceIncludeSelected, new MenuItemInfo() { Index = 6, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 8"), vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID8, OnTerrainSourceIncludeSelected, new MenuItemInfo() { Index = 7, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.ShowAsContext();
                    //}

                    GUILayout.BeginHorizontal();
                    vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID1 = EditorGUILayout.Toggle(
                        "Include Terrain Source ID 1",
                        vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID1);
                    vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID5 = EditorGUILayout.Toggle(
                        "Include Terrain Source ID 5",
                        vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID5);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID2 = EditorGUILayout.Toggle(
                        "Include Terrain Source ID 2",
                        vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID2);
                    vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID6 = EditorGUILayout.Toggle(
                        "Include Terrain Source ID 6",
                        vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID6);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID3 = EditorGUILayout.Toggle(
                        "Include Terrain Source ID 3",
                        vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID3);
                    vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID7 = EditorGUILayout.Toggle(
                        "Include Terrain Source ID 7",
                        vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID7);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID4 = EditorGUILayout.Toggle(
                        "Include Terrain Source ID 4",
                        vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID4);
                    vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID8 = EditorGUILayout.Toggle(
                        "Include Terrain Source ID 8",
                        vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID8);
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainSourceExcludeRule = EditorGUILayout.Toggle(
                    "Use terrain source exclude rule",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainSourceExcludeRule);
                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseTerrainSourceExcludeRule)
                {
                    //if (GUILayout.Button("Select Exclude terrain source IDs"))
                    //{
                    //    GenericMenu menu = new GenericMenu();
                    //    menu.AddItem(new GUIContent("TerrainSourceID 1"), vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID1, OnTerrainSourceExcludeSelected, new MenuItemInfo() { Index = 0, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro,VegetationItemIndex = vegetationItemIndex,VegetationPackageIndex = vegetationPackageIndex});
                    //    menu.AddItem(new GUIContent("TerrainSourceID 2"), vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID2, OnTerrainSourceExcludeSelected, new MenuItemInfo() { Index = 1, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 3"), vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID3, OnTerrainSourceExcludeSelected, new MenuItemInfo() { Index = 2, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 4"), vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID4, OnTerrainSourceExcludeSelected, new MenuItemInfo() { Index = 3, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 5"), vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID5, OnTerrainSourceExcludeSelected, new MenuItemInfo() { Index = 4, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 6"), vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID6, OnTerrainSourceExcludeSelected, new MenuItemInfo() { Index = 5, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 7"), vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID7, OnTerrainSourceExcludeSelected, new MenuItemInfo() { Index = 6, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.AddItem(new GUIContent("TerrainSourceID 8"), vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID8, OnTerrainSourceExcludeSelected, new MenuItemInfo() { Index = 7, VegetationItemInfoPro = vegetationItemInfoPro, VegetationPackagePro = vegetationPackagePro, VegetationItemIndex = vegetationItemIndex, VegetationPackageIndex = vegetationPackageIndex });
                    //    menu.ShowAsContext();
                    //}

                    GUILayout.BeginHorizontal();
                    vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID1 = EditorGUILayout.Toggle(
                        "Exclude Terrain Source ID 1",
                        vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID1);
                    vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID5 = EditorGUILayout.Toggle(
                        "Exclude Terrain Source ID 5",
                        vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID5);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID2 = EditorGUILayout.Toggle(
                        "Exclude Terrain Source ID 2",
                        vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID2);
                    vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID6 = EditorGUILayout.Toggle(
                        "Exclude Terrain Source ID 6",
                        vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID6);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID3 = EditorGUILayout.Toggle(
                        "Exclude Terrain Source ID 3",
                        vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID3);
                    vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID7 = EditorGUILayout.Toggle(
                        "Exclude Terrain Source ID 7",
                        vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID7);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID4 = EditorGUILayout.Toggle(
                        "Exclude Terrain Source ID 4",
                        vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID4);
                    vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID8 = EditorGUILayout.Toggle(
                        "Exclude Terrain Source ID 8",
                        vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID8);
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }
        }

        //void OnTerrainSourceExcludeSelected(object index)
        //{
        //    MenuItemInfo info = (MenuItemInfo)index;
        //    info.VegetationItemInfoPro.TerrainSourceExcludeRule[info.Index] =!info.VegetationItemInfoPro.TerrainSourceExcludeRule[info.Index];

        //    _vegetationSystemPro.ClearCache(info.VegetationPackageIndex, info.VegetationItemIndex);
        //    EditorUtility.SetDirty(info.VegetationPackagePro);
        //    SetSceneDirty();
        //}

        //void OnTerrainSourceIncludeSelected(object index)
        //{
        //    MenuItemInfo info = (MenuItemInfo)index;
        //    info.VegetationItemInfoPro.TerrainSourceIncludeRule[info.Index] = !info.VegetationItemInfoPro.TerrainSourceIncludeRule[info.Index];

        //    _vegetationSystemPro.ClearCache(info.VegetationPackageIndex, info.VegetationItemIndex);
        //    EditorUtility.SetDirty(info.VegetationPackagePro);
        //    SetSceneDirty();
        //}

        void DrawPrefabSettingsMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro, int vegetationPackageIndex, int vegetationItemIndex)
        {
            _vegetationSystemPro.ShowVegetationItemSettingsMenu =
                VegetationPackageEditorTools.DrawHeader("General settings",
                    _vegetationSystemPro.ShowVegetationItemSettingsMenu);

            if (_vegetationSystemPro.ShowVegetationItemSettingsMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Vegetation type: " + vegetationItemInfoPro.VegetationType, LabelStyle);


                if (vegetationItemInfoPro.PrefabType == VegetationPrefabType.Mesh)
                {
                    if (vegetationItemInfoPro.VegetationPrefab == null)
                    {
                        EditorGUILayout.HelpBox(
                            "The vegetation prefab is missing. Was probably deleted after it was added.",
                            MessageType.Error);
                    }

                    vegetationItemInfoPro.VegetationPrefab = (GameObject) EditorGUILayout.ObjectField("Prefab",
                        vegetationItemInfoPro.VegetationPrefab, typeof(GameObject), false);
                }
                else
                {
                    if (vegetationItemInfoPro.VegetationTexture == null)
                    {
                        EditorGUILayout.HelpBox(
                            "The vegetation texture is missing. Was probably deleted after it was added.",
                            MessageType.Error);
                    }

                    vegetationItemInfoPro.VegetationTexture = (Texture2D) EditorGUILayout.ObjectField("Texture",
                        vegetationItemInfoPro.VegetationTexture, typeof(Texture2D), false);
                }


                if (EditorGUI.EndChangeCheck())
                {
                    vegetationPackagePro.RefreshVegetationItemPrefab(vegetationItemInfoPro);
                    _vegetationSystemPro.RefreshVegetationSystem();
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                EditorGUI.BeginChangeCheck();
                vegetationItemInfoPro.Name = EditorGUILayout.TextField("Name", vegetationItemInfoPro.Name);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                EditorGUI.BeginChangeCheck();
                vegetationItemInfoPro.EnableRuntimeSpawn = EditorGUILayout.Toggle("Enable run-time spawn",
                    vegetationItemInfoPro.EnableRuntimeSpawn);
                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                EditorGUI.BeginChangeCheck();

                vegetationItemInfoPro.VegetationRenderMode =
                    (VegetationRenderMode) EditorGUILayout.EnumPopup("Render mode",
                        vegetationItemInfoPro.VegetationRenderMode);

                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.RefreshVegetationSystem();
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                EditorGUI.BeginChangeCheck();
                vegetationItemInfoPro.DisableShadows =
                    EditorGUILayout.Toggle("Disable shadows", vegetationItemInfoPro.DisableShadows);
                if (EditorGUI.EndChangeCheck())
                {
                    SetSceneDirty();
                }


                EditorGUI.BeginChangeCheck();
                vegetationItemInfoPro.Seed = EditorGUILayout.IntSlider("Seed", vegetationItemInfoPro.Seed, 0, 100);
                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                if (vegetationItemInfoPro.VegetationType == VegetationType.Tree ||
                    vegetationItemInfoPro.VegetationType == VegetationType.LargeObjects)
                {
                    EditorGUILayout.LabelField(
                        "Render distance: " + _vegetationSystemPro.VegetationSettings.GetTreeDistance() *
                        vegetationItemInfoPro.RenderDistanceFactor, LabelStyle);
                }
                else
                {
                    EditorGUILayout.LabelField(
                        "Render distance: " + _vegetationSystemPro.VegetationSettings.GetVegetationDistance() *
                        vegetationItemInfoPro.RenderDistanceFactor, LabelStyle);
                }

                EditorGUI.BeginChangeCheck();
                vegetationItemInfoPro.RenderDistanceFactor = EditorGUILayout.Slider("Render distance factor",
                    vegetationItemInfoPro.RenderDistanceFactor, 0, 1);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                if (vegetationItemInfoPro.VegetationType == VegetationType.Tree ||
                    vegetationItemInfoPro.VegetationType == VegetationType.LargeObjects)
                {
                    EditorGUILayout.HelpBox("The render distance is a calculated from the tree mesh distance.",
                        MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("The render distance is calculated from the vegetation distance.",
                        MessageType.Info);
                }

                if (GUILayout.Button("Refresh prefab"))
                {
                    vegetationPackagePro.RefreshVegetationItemPrefab(vegetationItemInfoPro);
                    _vegetationSystemPro.RefreshVegetationSystem();
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                GUILayout.EndVertical();
            }
        }


        void DrawConcaveLocationRulesMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro,
            int vegetationPackageIndex, int vegetationItemIndex)
        {
            _vegetationSystemPro.ShowConcaveLocationRulesMenu =
                VegetationPackageEditorTools.DrawHeader("Concave location rules",
                    _vegetationSystemPro.ShowConcaveLocationRulesMenu);

            if (_vegetationSystemPro.ShowConcaveLocationRulesMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");

                vegetationItemInfoPro.UseConcaveLocationRule = EditorGUILayout.Toggle("Use concave location rule",
                    vegetationItemInfoPro.UseConcaveLocationRule);
                if (vegetationItemInfoPro.UseConcaveLocationRule)
                {
                    vegetationItemInfoPro.ConcaveLoactionMinHeightDifference = EditorGUILayout.Slider(
                        "Min height difference", vegetationItemInfoPro.ConcaveLoactionMinHeightDifference, 0.1f, 10f);
                    vegetationItemInfoPro.ConcaveLoactionDistance = EditorGUILayout.Slider("Distance",
                        vegetationItemInfoPro.ConcaveLoactionDistance, 0f, 20f);
                    vegetationItemInfoPro.ConcaveLoactionAverage =
                        EditorGUILayout.Toggle("Average", vegetationItemInfoPro.ConcaveLoactionAverage);
                    vegetationItemInfoPro.ConcaveLocationInverse =
                        EditorGUILayout.Toggle("Inverse", vegetationItemInfoPro.ConcaveLocationInverse);
                    EditorGUILayout.HelpBox("Average setting sets if average or minimum edge samples are used.",
                        MessageType.Info);
                }

                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        void DrawColliderSettingsMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro)
        {
            // VegetationItemInfo vegetationItemInfo = _vegetationSystem.VegetationPackageList[_vegetationSystem.VegetationPackageIndex].VegetationInfoList[_vegIndex];
            if (vegetationItemInfoPro.VegetationType != VegetationType.Tree &&
                vegetationItemInfoPro.VegetationType != VegetationType.Objects &&
                vegetationItemInfoPro.VegetationType != VegetationType.LargeObjects) return;

            _vegetationSystemPro.ShowColliderRulesMenu =
                VegetationPackageEditorTools.DrawHeader("Colliders", _vegetationSystemPro.ShowColliderRulesMenu);
            if (_vegetationSystemPro.ShowColliderRulesMenu == false) return;

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();

            vegetationItemInfoPro.ColliderType = (ColliderType) EditorGUILayout.EnumPopup("Collider type",
                vegetationItemInfoPro.ColliderType);
            switch (vegetationItemInfoPro.ColliderType)
            {
                case ColliderType.Capsule:
                {
                    vegetationItemInfoPro.ColliderRadius = EditorGUILayout.FloatField("Radius",
                        vegetationItemInfoPro.ColliderRadius);
                    vegetationItemInfoPro.ColliderHeight = EditorGUILayout.FloatField("Height",
                        vegetationItemInfoPro.ColliderHeight);
                    vegetationItemInfoPro.ColliderOffset = EditorGUILayout.Vector3Field("Offset",
                        vegetationItemInfoPro.ColliderOffset);
                    break;
                }
                case ColliderType.Sphere:
                {
                    vegetationItemInfoPro.ColliderRadius = EditorGUILayout.FloatField("Radius",
                        vegetationItemInfoPro.ColliderRadius);
                    vegetationItemInfoPro.ColliderOffset = EditorGUILayout.Vector3Field("Offset",
                        vegetationItemInfoPro.ColliderOffset);

                    break;
                }

                case ColliderType.Box:
                {
                    vegetationItemInfoPro.ColliderSize = EditorGUILayout.Vector3Field("Size",
                        vegetationItemInfoPro.ColliderSize);
                    vegetationItemInfoPro.ColliderOffset = EditorGUILayout.Vector3Field("Offset",
                        vegetationItemInfoPro.ColliderOffset);

                    break;
                }
                case ColliderType.CustomMesh:
                {
                    vegetationItemInfoPro.ColliderMesh = (Mesh) EditorGUILayout.ObjectField("Custom mesh",
                        vegetationItemInfoPro.ColliderMesh, typeof(Mesh), false);
                    vegetationItemInfoPro.ColliderConvex =
                        EditorGUILayout.Toggle("Convex", vegetationItemInfoPro.ColliderConvex);
                    break;
                }
                case ColliderType.Mesh:
                {
                    vegetationItemInfoPro.ColliderConvex =
                        EditorGUILayout.Toggle("Convex", vegetationItemInfoPro.ColliderConvex);
                    break;
                }
            }

            if (vegetationItemInfoPro.ColliderType != ColliderType.Disabled)
            {
                vegetationItemInfoPro.ColliderTag = EditorGUILayout.TagField("Tag", vegetationItemInfoPro.ColliderTag);
            }

            if (vegetationItemInfoPro.ColliderType != ColliderType.Disabled)
            {
                vegetationItemInfoPro.ColliderDistanceFactor = EditorGUILayout.Slider("Distance factor",
                    vegetationItemInfoPro.ColliderDistanceFactor, 0, 1);
                float currentDistance = _vegetationSystemPro.VegetationSettings.GetVegetationDistance() *
                                        vegetationItemInfoPro.ColliderDistanceFactor;

                EditorGUILayout.LabelField("Current distance: " + currentDistance.ToString("F2") + " meters",
                    LabelStyle);

                EditorGUILayout.HelpBox(
                    "The distance from the camera where colliders are created. Distance is a factor of the vegetation draw distance.",
                    MessageType.Info);
                vegetationItemInfoPro.ColliderTrigger =
                    EditorGUILayout.Toggle("Trigger", vegetationItemInfoPro.ColliderTrigger);
                vegetationItemInfoPro.ColliderUseForBake = EditorGUILayout.Toggle("Include in NavMesh bake",
                    vegetationItemInfoPro.ColliderUseForBake);
                if (vegetationItemInfoPro.ColliderUseForBake)
                {
                    vegetationItemInfoPro.NavMeshArea = EditorGUILayout.Popup("Navigation Area", vegetationItemInfoPro.NavMeshArea, _navAreas);
                }
            }

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.RefreshColliderSystem();
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            if (vegetationItemInfoPro.ColliderType != ColliderType.Disabled)
            {
                GUILayout.BeginVertical("box");
                EditorGUI.BeginChangeCheck();

                vegetationItemInfoPro.NavMeshObstacleType = (NavMeshObstacleType) EditorGUILayout.EnumPopup(
                    "NavMesh Obstacle Type",
                    vegetationItemInfoPro.NavMeshObstacleType);
                
            
                switch (vegetationItemInfoPro.NavMeshObstacleType)
                {
                    case NavMeshObstacleType.Box:
                    {
                        vegetationItemInfoPro.NavMeshObstacleCenter = EditorGUILayout.Vector3Field("Center",
                            vegetationItemInfoPro.NavMeshObstacleCenter);
                        vegetationItemInfoPro.NavMeshObstacleSize = EditorGUILayout.Vector3Field("Size",
                            vegetationItemInfoPro.NavMeshObstacleSize);
                        vegetationItemInfoPro.NavMeshObstacleCarve = EditorGUILayout.Toggle("Carve",
                            vegetationItemInfoPro.NavMeshObstacleCarve);
                        break;
                    }
                    case NavMeshObstacleType.Capsule:
                        vegetationItemInfoPro.NavMeshObstacleCenter = EditorGUILayout.Vector3Field("Center",
                            vegetationItemInfoPro.NavMeshObstacleCenter);
                        vegetationItemInfoPro.NavMeshObstacleRadius = EditorGUILayout.FloatField("Radius",
                            vegetationItemInfoPro.NavMeshObstacleRadius);
                        vegetationItemInfoPro.NavMeshObstacleHeight = EditorGUILayout.FloatField("Height",
                            vegetationItemInfoPro.NavMeshObstacleHeight);
                        vegetationItemInfoPro.NavMeshObstacleCarve = EditorGUILayout.Toggle("Carve",
                            vegetationItemInfoPro.NavMeshObstacleCarve);
                        break;
                }

                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.RefreshColliderSystem();
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        void DrawBiomeRulesMenu(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro,
            int vegetationPackageIndex, int vegetationItemIndex)
        {
            _vegetationSystemPro.ShowBiomeRulesMenu =
                VegetationPackageEditorTools.DrawHeader("Biome area rules", _vegetationSystemPro.ShowBiomeRulesMenu);

            if (_vegetationSystemPro.ShowBiomeRulesMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");

                vegetationItemInfoPro.UseBiomeEdgeScaleRule = EditorGUILayout.Toggle("Use biome edge scale rule",
                    vegetationItemInfoPro.UseBiomeEdgeScaleRule);
                if (vegetationItemInfoPro.UseBiomeEdgeScaleRule)
                {
                    vegetationItemInfoPro.BiomeEdgeScaleDistance = EditorGUILayout.Slider("Affected edge distance",
                        vegetationItemInfoPro.BiomeEdgeScaleDistance, 0f, 100f);
                    EditorFunctions.FloatRangeField("Min/Max scale",
                        ref vegetationItemInfoPro.BiomeEdgeScaleMinScale,
                        ref vegetationItemInfoPro.BiomeEdgeScaleMaxScale, 0.1f, 2);
                    vegetationItemInfoPro.BiomeEdgeScaleInverse = EditorGUILayout.Toggle("Inverse",
                        vegetationItemInfoPro.BiomeEdgeScaleInverse);
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationItemInfoPro.UseBiomeEdgeIncludeRule = EditorGUILayout.Toggle("Use biome edge include rule",
                    vegetationItemInfoPro.UseBiomeEdgeIncludeRule);
                if (vegetationItemInfoPro.UseBiomeEdgeIncludeRule)
                {
                    vegetationItemInfoPro.BiomeEdgeIncludeDistance = EditorGUILayout.Slider("Max edge distance",
                        vegetationItemInfoPro.BiomeEdgeIncludeDistance, 0f, 100f);
                    vegetationItemInfoPro.BiomeEdgeIncludeInverse = EditorGUILayout.Toggle("Inverse",
                        vegetationItemInfoPro.BiomeEdgeIncludeInverse);
                }

                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                if (vegetationItemInfoPro.UseBiomeEdgeScaleRule || vegetationItemInfoPro.UseBiomeEdgeIncludeRule)
                {
                    EditorGUILayout.HelpBox("Biome rules are not used with default biome", MessageType.Info);
                }
            }
        }


        void DrawBillboardSettings(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro)
        {
            bool billboardChanged = false;
            if (vegetationItemInfoPro.VegetationType != VegetationType.Tree) return;

            _vegetationSystemPro.ShowBillboardsMenu =
                VegetationPackageEditorTools.DrawHeader("Billboards", _vegetationSystemPro.ShowBillboardsMenu);
            if (!_vegetationSystemPro.ShowBillboardsMenu) return;

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            vegetationItemInfoPro.UseBillboards = EditorGUILayout.Toggle("Enable billboards",
                vegetationItemInfoPro.UseBillboards);
            if (EditorGUI.EndChangeCheck())
            {
                billboardChanged = true;
                _vegetationSystemPro.RefreshBillboards();
                EditorUtility.SetDirty(vegetationPackagePro);
            }

            if (!vegetationItemInfoPro.UseBillboards)
            {
                GUILayout.EndVertical();
                return;
            }

            if (vegetationItemInfoPro.UseBillboards)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Runtime settings", LabelStyle);

                EditorGUI.BeginChangeCheck();
                vegetationItemInfoPro.BillboardRenderMode =
                    (BillboardRenderMode) EditorGUILayout.EnumPopup("Render mode",
                        vegetationItemInfoPro.BillboardRenderMode);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                    _vegetationSystemPro.RefreshVegetationSystem();
                }

                EditorGUI.BeginChangeCheck();

                vegetationItemInfoPro.BillboardCutoff = EditorGUILayout.Slider("Alpha cutoff",
                    vegetationItemInfoPro.BillboardCutoff, 0f, 1f);
                vegetationItemInfoPro.BillboardBrightness = EditorGUILayout.Slider("Brightness",
                    vegetationItemInfoPro.BillboardBrightness, 0f, 5f);

                vegetationItemInfoPro.BillboardSmoothness = EditorGUILayout.Slider("Smoothness",
                    vegetationItemInfoPro.BillboardSmoothness, 0f, 1f);

                if (vegetationItemInfoPro.BillboardRenderMode == BillboardRenderMode.Standard)
                {
                    vegetationItemInfoPro.BillboardMetallic = EditorGUILayout.Slider("Metallic",
                        vegetationItemInfoPro.BillboardMetallic, 0f, 1f);
                }
                else
                {
                    vegetationItemInfoPro.BillboardSpecular = EditorGUILayout.Slider("Specular",
                        vegetationItemInfoPro.BillboardSpecular, 0f, 1f);
                }

                vegetationItemInfoPro.BillboardOcclusion = EditorGUILayout.Slider("Occlusion",
                    vegetationItemInfoPro.BillboardOcclusion, 0f, 1f);

                vegetationItemInfoPro.BillboardNormalStrength = EditorGUILayout.Slider("Normal strength",
                    vegetationItemInfoPro.BillboardNormalStrength, 0f, 2f);

                vegetationItemInfoPro.BillboardMipmapBias = EditorGUILayout.Slider("Mipmap bias",
                    vegetationItemInfoPro.BillboardMipmapBias, -3f, 0f);

                vegetationItemInfoPro.BillboardTintColor =
                    EditorGUILayout.ColorField("Tint color", vegetationItemInfoPro.BillboardTintColor);
                EditorGUILayout.LabelField("Wind Runtime settings", LabelStyle);
                vegetationItemInfoPro.BillboardWindSpeed = EditorGUILayout.Slider("Wind speed",
                    vegetationItemInfoPro.BillboardWindSpeed, 0, 5f);
                EditorGUILayout.HelpBox("Billboard wind can be enabled on custom shaders with a shadercontroler.",
                    MessageType.Info);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                    _vegetationSystemPro.RefreshMaterials();
                }

                GUILayout.EndVertical();

                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Generation settings", LabelStyle);
                EditorGUI.BeginChangeCheck();

                vegetationItemInfoPro.BillboardQuality =
                    (BillboardQuality) EditorGUILayout.EnumPopup("Billboard quality",
                        vegetationItemInfoPro.BillboardQuality);

                vegetationItemInfoPro.BillboardSourceLODLevel = (LODLevel) EditorGUILayout.EnumPopup("Source LOD",
                    vegetationItemInfoPro.BillboardSourceLODLevel);
                EditorGUILayout.HelpBox("Selects what LOD on the tree to use as source for the billboard",
                    MessageType.Info);
                
                vegetationItemInfoPro.BillboardFlipBackNormals = EditorGUILayout.Toggle("Flip backfacing normals",
                    vegetationItemInfoPro.BillboardFlipBackNormals);

                vegetationItemInfoPro.BillboardRecalculateNormals = EditorGUILayout.Toggle("Generate new normals",
                    vegetationItemInfoPro.BillboardRecalculateNormals);

                if (EditorGUI.EndChangeCheck())
                {
                    vegetationPackagePro.GenerateBillboard(vegetationItemInfoPro.VegetationItemID);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    _vegetationSystemPro.RefreshVegetationSystem();
                }

                EditorGUI.BeginChangeCheck();
                if (vegetationItemInfoPro.BillboardRecalculateNormals)
                {
                    vegetationItemInfoPro.BillboardNormalBlendFactor = EditorGUILayout.Slider("Normal blend",
                        vegetationItemInfoPro.BillboardNormalBlendFactor, 0, 1);
                    EditorGUILayout.HelpBox(
                        "This setting allows you to blend between the original and generated mesh normals.",
                        MessageType.Info);
                }

                vegetationItemInfoPro.BillboardAtlasBackgroundColor = EditorGUILayout.ColorField("Atlas background color",
                    vegetationItemInfoPro.BillboardAtlasBackgroundColor);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                }

                //if (oldBillboardQuality != vegetationItemInfoPro.BillboardQuality)
                //{
                //    vegetationPackagePro.GenerateBillboard(_vegIndex);
                //    EditorUtility.SetDirty(vegetationPackagePro);
                //    _vegetationSystemPro.RefreshVegetationSystem();
                //}

                vegetationItemInfoPro.BillboardTexture = EditorGUILayout.ObjectField(
                    "Billboard texture",
                    vegetationItemInfoPro.BillboardTexture, typeof(Texture2D), true) as Texture2D;
                vegetationItemInfoPro.BillboardNormalTexture = EditorGUILayout.ObjectField(
                    "Billboard normals",
                    vegetationItemInfoPro.BillboardNormalTexture, typeof(Texture2D),
                    true) as Texture2D;

                //vegetationItemInfoPro.BillboardAoTexture = EditorGUILayout.ObjectField(
                //    "Billboard AO/Self shadowing",
                //    vegetationItemInfoPro.BillboardAoTexture, typeof(Texture2D),
                //    true) as Texture2D;

                if (GUILayout.Button("Regenerate billboard"))
                {
                    vegetationPackagePro.GenerateBillboard(vegetationItemInfoPro.VegetationItemID);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    _vegetationSystemPro.RefreshVegetationSystem();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                billboardChanged = true;
            }

            if (billboardChanged)
            {
                _vegetationSystemPro.RefreshBillboards();
                EditorUtility.SetDirty(vegetationPackagePro);
            }
        }

        void DrawDistanceFalloffMenu(VegetationPackagePro vegetationPackagePro,
            VegetationItemInfoPro vegetationItemInfoPro,
            int vegetationPackageIndex, int vegetationItemIndex)
        {
            if (vegetationItemInfoPro.VegetationType == VegetationType.Tree ||
                vegetationItemInfoPro.VegetationType == VegetationType.LargeObjects) return;

            _vegetationSystemPro.ShowDistanceFalloffMenu =
                VegetationPackageEditorTools.DrawHeader("Distance falloff",
                    _vegetationSystemPro.ShowDistanceFalloffMenu);

            if (_vegetationSystemPro.ShowDistanceFalloffMenu)
            {
                GUILayout.BeginVertical("box");

                EditorGUI.BeginChangeCheck();

                vegetationPackagePro.VegetationInfoList[_vegIndex].UseDistanceFalloff = EditorGUILayout.Toggle(
                    "Use distance falloff",
                    vegetationPackagePro.VegetationInfoList[_vegIndex].UseDistanceFalloff);

                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                if (vegetationPackagePro.VegetationInfoList[_vegIndex].UseDistanceFalloff)
                {
                    EditorGUI.BeginChangeCheck();

                    vegetationPackagePro.VegetationInfoList[_vegIndex].DistanceFalloffStartDistance =
                        EditorGUILayout.Slider("Set start distance",
                            vegetationPackagePro.VegetationInfoList[_vegIndex].DistanceFalloffStartDistance, 0, 1);

                    EditorGUILayout.HelpBox(
                        "The start distance is where on the vegetation distance the falloff will start. After this it declines linear until max distance.",
                        MessageType.Info);

                    if (EditorGUI.EndChangeCheck())
                    {
                        _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                    //if (_distanceFalloffCurveEditor.EditCurve(vegetationItemInfoPro.DistanceFallofffAnimationCurve, this))
                    //{
                    //    VegetationItemModelInfo vegetationItemModelInfo =
                    //        _vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackageIndex, vegetationItemIndex);
                    //    vegetationItemModelInfo.UpdateDistanceFalloutCurve();

                    //    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    //    EditorUtility.SetDirty(vegetationPackagePro);
                    //    SetSceneDirty();
                    //}
                    EditorGUILayout.HelpBox(
                        "The distance falloff is done run-time and can reduce grass and plant density in the distance. ",
                        MessageType.Info);
                }

                GUILayout.EndVertical();
            }
        }

        void DrawPositionMenu(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro,
            int vegetationPackageIndex, int vegetationItemIndex)
        {
            _vegetationSystemPro.ShowPositionMenu =
                VegetationPackageEditorTools.DrawHeader("Position", _vegetationSystemPro.ShowPositionMenu);

            if (_vegetationSystemPro.ShowPositionMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");

                float minSampleDistance = 0.4f;
                if (vegetationItemInfoPro.VegetationType == VegetationType.Tree)
                {
                    minSampleDistance = 5f;
                }

                vegetationItemInfoPro.SampleDistance = EditorGUILayout.Slider("Sample distance",
                    vegetationItemInfoPro.SampleDistance, minSampleDistance, 50f);
                vegetationItemInfoPro.Density = EditorGUILayout.Slider("Density", vegetationItemInfoPro.Density, 0, 1);
                vegetationItemInfoPro.RandomizePosition =
                    EditorGUILayout.Toggle("Randomize position", vegetationItemInfoPro.RandomizePosition);

                vegetationItemInfoPro.UseSamplePointOffset = EditorGUILayout.Toggle("Use sample point offset",
                    vegetationItemInfoPro.UseSamplePointOffset);
                if (vegetationItemInfoPro.UseSamplePointOffset)
                {
                    EditorFunctions.FloatRangeField("Sample point offset",
                        ref vegetationItemInfoPro.SamplePointMinOffset,
                        ref vegetationItemInfoPro.SamplePointMaxOffset, 0.1f, 20);
                }

                vegetationItemInfoPro.Rotation =
                    (VegetationRotationType) EditorGUILayout.EnumPopup("Rotation", vegetationItemInfoPro.Rotation);
                vegetationItemInfoPro.RotationOffset =
                    EditorGUILayout.Vector3Field("Rotation offset", vegetationItemInfoPro.RotationOffset);
                EditorFunctions.FloatRangeField("Min/Max scale",
                    ref vegetationPackagePro.VegetationInfoList[vegetationItemIndex].MinScale,
                    ref vegetationPackagePro.VegetationInfoList[vegetationItemIndex].MaxScale, 0.1f, 10f);

                vegetationItemInfoPro.ScaleMultiplier =
                    EditorGUILayout.Vector3Field("Scale multiplier", vegetationItemInfoPro.ScaleMultiplier);

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationItemInfoPro.UseHeightRule = EditorGUILayout.Toggle("Use height rule",
                    vegetationItemInfoPro.UseHeightRule);
                if (vegetationItemInfoPro.UseHeightRule)
                {
                    vegetationItemInfoPro.UseAdvancedHeightRule =
                        EditorGUILayout.Toggle("Advanced", vegetationItemInfoPro.UseAdvancedHeightRule);
                    if (vegetationItemInfoPro.UseAdvancedHeightRule)
                    {
                        _heightCurveEditor.MaxValue = vegetationItemInfoPro.MaxCurveHeight;
                        if (_heightCurveEditor.EditCurve(vegetationItemInfoPro.HeightRuleCurve, this))
                        {
                            VegetationItemModelInfo vegetationItemModelInfo =
                                _vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackageIndex,
                                    vegetationItemIndex);
                            vegetationItemModelInfo.UpdateHeightRuleCurve();

                            _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                        EditorGUI.BeginChangeCheck();
                        vegetationItemInfoPro.MaxCurveHeight = EditorGUILayout.Slider("Max curve heigth",
                            vegetationItemInfoPro.MaxCurveHeight, 1, 2000);
                        if (EditorGUI.EndChangeCheck())
                        {
                            VegetationItemModelInfo vegetationItemModelInfo =
                                _vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackageIndex,
                                    vegetationItemIndex);
                            vegetationItemModelInfo.UpdateHeightRuleCurve();

                            _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }
                    }
                    else
                    {
                        EditorFunctions.FloatRangeField("Min/Max height",
                            ref vegetationItemInfoPro.MinHeight,
                            ref vegetationItemInfoPro.MaxHeight, -500f, 10000);
                    }
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                vegetationItemInfoPro.UseSteepnessRule = EditorGUILayout.Toggle("Use steepness rule",
                    vegetationItemInfoPro.UseSteepnessRule);
                if (vegetationItemInfoPro.UseSteepnessRule)
                {
                    vegetationItemInfoPro.UseAdvancedSteepnessRule =
                        EditorGUILayout.Toggle("Advanced", vegetationItemInfoPro.UseAdvancedSteepnessRule);
                    if (vegetationItemInfoPro.UseAdvancedSteepnessRule)
                    {
                        if (_steepnessCurveEditor.EditCurve(vegetationItemInfoPro.SteepnessRuleCurve, this))
                        {
                            VegetationItemModelInfo vegetationItemModelInfo =
                                _vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackageIndex,
                                    vegetationItemIndex);
                            vegetationItemModelInfo.UpdateSteepnessRuleCurve();

                            _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }
                    }
                    else
                    {
                        EditorFunctions.FloatRangeField("Min/Max steepness",
                            ref vegetationItemInfoPro.MinSteepness,
                            ref vegetationItemInfoPro.MaxSteepness, 0f, 90f);
                    }
                }

                GUILayout.EndVertical();


                GUILayout.BeginVertical("box");
                vegetationItemInfoPro.Offset =
                    EditorGUILayout.Vector3Field("Position offset", vegetationItemInfoPro.Offset);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorFunctions.FloatRangeField("Height offset range",
                    ref vegetationPackagePro.VegetationInfoList[_vegIndex].MinUpOffset,
                    ref vegetationPackagePro.VegetationInfoList[_vegIndex].MaxUpOffset, -10, 10);
                GUILayout.EndVertical();
                
                EditorGUILayout.HelpBox(
                    "The height offset range will offset the vegetation item in the selected up direction. Based on RotationType. It is scaled by the scale of the object and applied after position offset. usefull for rocks lowered in the ground. For Rotation XYZ, terrain normal is used as up.",
                    MessageType.Info);
                
                if (EditorGUI.EndChangeCheck())
                {
                    _vegetationSystemPro.ClearCache(vegetationPackageIndex, vegetationItemIndex);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        void DrawVegetationPackageProGeneralSettings(VegetationPackagePro vegetationPackagePro)
        {
            _vegetationSystemPro.ShowVegetationPackageGeneralSettingsMenu =
                VegetationPackageEditorTools.DrawHeader("General settings",
                    _vegetationSystemPro.ShowVegetationPackageGeneralSettingsMenu);

            if (_vegetationSystemPro.ShowVegetationPackageGeneralSettingsMenu)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                vegetationPackagePro.PackageName =
                    EditorGUILayout.TextField("Package name", vegetationPackagePro.PackageName);
                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }
            }
        }

        void DrawVegetationItemSettings()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");

            _vegetationSystemPro.VegetationSettings.GrassDensity = EditorGUILayout.Slider("Grass density",
                _vegetationSystemPro.VegetationSettings.GrassDensity, 0f, 2f);
            _vegetationSystemPro.VegetationSettings.PlantDensity = EditorGUILayout.Slider("Plant density",
                _vegetationSystemPro.VegetationSettings.PlantDensity, 0f, 2f);
            _vegetationSystemPro.VegetationSettings.TreeDensity = EditorGUILayout.Slider("Tree density",
                _vegetationSystemPro.VegetationSettings.TreeDensity, 0f, 2f);
            _vegetationSystemPro.VegetationSettings.ObjectDensity = EditorGUILayout.Slider("Object density",
                _vegetationSystemPro.VegetationSettings.ObjectDensity, 0f, 2f);
            _vegetationSystemPro.VegetationSettings.LargeObjectDensity = EditorGUILayout.Slider("Large Object density",
                _vegetationSystemPro.VegetationSettings.LargeObjectDensity, 0f, 2f);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.ClearCache();
                SetSceneDirty();
            }
        }

        void DrawBiomeEditor()
        {
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
                    _vegetationSystemPro.AddVegetationPackage(newVegetationPackagePro);
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    _vegetationSystemPro.RefreshVegetationSystem();
                    return;
                }
            }

            GUILayout.EndVertical();

            for (int i = 0; i <= _vegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                GUILayout.BeginVertical("box");
                if (GUILayout.Button("Remove biome", GUILayout.Width(120)))
                {
                    _vegetationSystemPro.RemoveVegetationPackage(_vegetationSystemPro.VegetationPackageProList[i]);
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    _vegetationSystemPro.RefreshVegetationSystem();
                    return;
                }

                EditorGUI.BeginChangeCheck();
                _vegetationSystemPro.VegetationPackageProList[i] = (VegetationPackagePro) EditorGUILayout.ObjectField(
                    "Vegetation package",
                    _vegetationSystemPro.VegetationPackageProList[i], typeof(VegetationPackagePro), true);

                if (EditorGUI.EndChangeCheck())
                {
                    if (_vegetationSystemPro.VegetationPackageProList[i] == null)
                    {
                        _vegetationSystemPro.RemoveVegetationPackage(_vegetationSystemPro.VegetationPackageProList[i]);
                        SetSceneDirty();
                        GUILayout.EndVertical();
                        _vegetationSystemPro.RefreshVegetationSystem();
                        return;
                    }
                    else
                    {
                        SetSceneDirty();
                        GUILayout.EndVertical();
                        _vegetationSystemPro.RefreshVegetationSystem();
                        return;
                    }
                }

                EditorGUI.BeginChangeCheck();
                _vegetationSystemPro.VegetationPackageProList[i].BiomeType =
                    (BiomeType) EditorGUILayout.EnumPopup("Select biome",
                        _vegetationSystemPro.VegetationPackageProList[i].BiomeType);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(_vegetationSystemPro.VegetationPackageProList[i]);
                    SetSceneDirty();
                    _vegetationSystemPro.RefreshVegetationSystem();
                    //_vegetationSystemPro.ClearCache();
                }

                EditorGUI.BeginChangeCheck();
                _vegetationSystemPro.VegetationPackageProList[i].PackageName =
                    EditorGUILayout.TextField("Biome name",
                        _vegetationSystemPro.VegetationPackageProList[i].PackageName);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(_vegetationSystemPro.VegetationPackageProList[i]);
                    SetSceneDirty();
                }

                if (_vegetationSystemPro.VegetationPackageProList[i].BiomeType != BiomeType.Default)
                {
                    EditorGUI.BeginChangeCheck();
                    _vegetationSystemPro.VegetationPackageProList[i].BiomeSortOrder =
                        EditorGUILayout.IntSlider("Biome sort order",
                            _vegetationSystemPro.VegetationPackageProList[i].BiomeSortOrder, 1, 10);

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(_vegetationSystemPro.VegetationPackageProList[i]);
                        SetSceneDirty();
                        _vegetationSystemPro.RefreshVegetationSystem();
                    }
                }

                GUILayout.EndVertical();
            }

            EditorGUILayout.HelpBox(
                "An higher sort order will apply the biome on top of others. Default biome is always on bottom.",
                MessageType.Info);
        }

        void DrawCameraInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add cameras", LabelStyle);
            EditorGUILayout.HelpBox(
                "Add any camera you want to use for vegetation selection and rendering. If multiple cameras with different viewpoint are used, Select render direct to camera to avoid cameras rendering the other cameras selection also.",
                MessageType.Info);
            EditorGUI.BeginChangeCheck();
            Camera newCamera = (Camera) EditorGUILayout.ObjectField("Add Camera", null, typeof(Camera), true);

            if (EditorGUI.EndChangeCheck())
            {
                if (newCamera)
                {
                    _vegetationSystemPro.AddCamera(newCamera);
                    SetSceneDirty();
                }
            }

            GUILayout.EndVertical();

            bool multipleCameras;
            if (Application.isPlaying)
            {
                multipleCameras = _vegetationSystemPro.VegetationStudioCameraList.Count > 1;
            }
            else
            {
                multipleCameras = _vegetationSystemPro.VegetationStudioCameraList.Count > 2;
            }

            for (int i = 0; i <= _vegetationSystemPro.VegetationStudioCameraList.Count - 1; i++)
            {
                bool sceneviewCamera = _vegetationSystemPro.VegetationStudioCameraList[i].VegetationStudioCameraType ==
                                       VegetationStudioCameraType.SceneView;

                GUILayout.BeginVertical("box");

                if (!sceneviewCamera)
                {
                    if (GUILayout.Button("Remove camera", GUILayout.Width(120)))
                    {
                        _vegetationSystemPro.RemoveCamera(_vegetationSystemPro.VegetationStudioCameraList[i]
                            .SelectedCamera);
                        SetSceneDirty();
                        return;
                    }
                }

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Camera", _vegetationSystemPro.VegetationStudioCameraList[i].SelectedCamera,
                    typeof(Camera), true);
                EditorGUI.EndDisabledGroup();

                if (!sceneviewCamera)
                {
                    EditorGUI.BeginChangeCheck();
                    _vegetationSystemPro.VegetationStudioCameraList[i].RenderDirectToCamera =
                        EditorGUILayout.Toggle("Render direct to camera",
                            _vegetationSystemPro.VegetationStudioCameraList[i].RenderDirectToCamera);

                    if (!_vegetationSystemPro.VegetationStudioCameraList[i].RenderDirectToCamera && multipleCameras)
                    {
                        EditorGUILayout.HelpBox(
                            "With multiple cameras render direct to camera should normally be enabled. This avoids the cameras to see each other selected vegetation.",
                            MessageType.Warning);
                    }

                    _vegetationSystemPro.VegetationStudioCameraList[i].RenderBillboardsOnly =
                        EditorGUILayout.Toggle("Render billboards only",
                            _vegetationSystemPro.VegetationStudioCameraList[i].RenderBillboardsOnly);

                    _vegetationSystemPro.VegetationStudioCameraList[i].CameraCullingMode =
                        (CameraCullingMode) EditorGUILayout.EnumPopup("Camera culling mode",
                            _vegetationSystemPro.VegetationStudioCameraList[i].CameraCullingMode);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetSceneDirty();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "The sceneview camera is used during editor mode to manage vegetation based on the active sceneview.",
                        MessageType.Info);
                }

                GUILayout.EndVertical();
            }
        }

        void DrawVegetationInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation distances", LabelStyle);
            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.VegetationSettings.PlantDistance =
                EditorGUILayout.Slider("Grass/Plant distance", _vegetationSystemPro.VegetationSettings.PlantDistance, 0,
                    800);
            _vegetationSystemPro.VegetationSettings.AdditionalTreeMeshDistance =
                EditorGUILayout.Slider("Additional mesh tree distance",
                    _vegetationSystemPro.VegetationSettings.AdditionalTreeMeshDistance, 0, 1500);
            _vegetationSystemPro.VegetationSettings.AdditionalBillboardDistance =
                EditorGUILayout.Slider("Additional billboard distance",
                    _vegetationSystemPro.VegetationSettings.AdditionalBillboardDistance, 0, 20000);
            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.UpdateBillboardCulling();
                SetSceneDirty();
            }

            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("LOD Control", LabelStyle);
            _vegetationSystemPro.VegetationSettings.LODDistanceFactor = EditorGUILayout.Slider(
                "Global LOD distance factor", _vegetationSystemPro.VegetationSettings.LODDistanceFactor, 0.1f, 5);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.UpdateBillboardCulling();
                SetSceneDirty();
            }

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Distance Control", LabelStyle);
            _vegetationSystemPro.VegetationSettings.DisableRenderDistanceFactor = EditorGUILayout.Toggle(
                "Disable render distance factor", _vegetationSystemPro.VegetationSettings.DisableRenderDistanceFactor);
            EditorGUILayout.HelpBox(
                "Disable the render distance factor will render all vegetation to Vegetation Distance or Vegetation Distance + Additional Tree range depending on type.",
                MessageType.Info);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.UpdateBillboardCulling();
                SetSceneDirty();
            }

            DrawVegetationItemSettings();


            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Random seed", LabelStyle);
            _vegetationSystemPro.VegetationSettings.Seed =
                EditorGUILayout.IntSlider("Seed", _vegetationSystemPro.VegetationSettings.Seed, 0, 100);
            if (EditorGUI.EndChangeCheck())
            {
                _vegetationSystemPro.ClearCache();
                SceneView.RepaintAll();
                SetSceneDirty();
            }

            GUILayout.EndVertical();
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
            VegetationStudioManager.ShowBiomes = false;
            DestroyImmediate(_dummyPreviewTexture);
            _distanceFalloffCurveEditor.RemoveAll();

            _heightCurveEditor.RemoveAll();
            _steepnessCurveEditor.RemoveAll();
        }

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            
            _navAreas = GameObjectUtility.GetNavMeshAreaNames();
            _vegetationSystemPro = (VegetationSystemPro) target;
            _dummyPreviewTexture = new Texture2D(80, 80);
            var settings = InspectorCurveEditor.Settings.DefaultSettings;

            _distanceFalloffCurveEditor =
                new InspectorCurveEditor(settings) {CurveType = InspectorCurveEditor.InspectorCurveType.Falloff};

            _heightCurveEditor =
                new InspectorCurveEditor(settings)
                {
                    CurveType = InspectorCurveEditor.InspectorCurveType.Height, TreeUpdate = true
                };

            _steepnessCurveEditor =
                new InspectorCurveEditor(settings)
                {
                    CurveType = InspectorCurveEditor.InspectorCurveType.Steepness, TreeUpdate = true
                };
            
            ConfirmBillboardColorSpace();
        }

        void DrawDebugInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Debug settings", LabelStyle);
            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.ShowVegetationCells =
                EditorGUILayout.Toggle("Show vegetation cells", _vegetationSystemPro.ShowVegetationCells);
            _vegetationSystemPro.ShowPotentialVisibleCells =
                EditorGUILayout.Toggle("Show potential visible cells", _vegetationSystemPro.ShowPotentialVisibleCells);
            _vegetationSystemPro.ShowVisibleCells =
                EditorGUILayout.Toggle("Show visible cells", _vegetationSystemPro.ShowVisibleCells);
            _vegetationSystemPro.ShowBiomeCells =
                EditorGUILayout.Toggle("Show biome cells", _vegetationSystemPro.ShowBiomeCells);
            _vegetationSystemPro.ShowVegetationMaskCells =
                EditorGUILayout.Toggle("Show vegetation mask cells", _vegetationSystemPro.ShowVegetationMaskCells);
            _vegetationSystemPro.ShowBillboardCells =
                EditorGUILayout.Toggle("Show billboard cells", _vegetationSystemPro.ShowBillboardCells);
            _vegetationSystemPro.ShowVisibleBillboardCells =
                EditorGUILayout.Toggle("Show visible billboard cells", _vegetationSystemPro.ShowVisibleBillboardCells);

            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Prefab tools", LabelStyle);

            if (GUILayout.Button("Refresh all prefabs"))
            {
                _vegetationSystemPro.RefreshAllPrefabs();
            }

            GUILayout.EndVertical();

            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Temporary cell cache", LabelStyle);
            EditorGUILayout.LabelField("Total created: " + _vegetationSystemPro.VegetationCellSpawner.VegetationInstanceDataPool.GetItemsCreatedCount(), LabelStyle);
            EditorGUILayout.LabelField("In pool: " + _vegetationSystemPro.VegetationCellSpawner.VegetationInstanceDataPool.GetItemsInPoolCount(), LabelStyle);
            GUILayout.EndVertical();

            //EditorGUI.BeginChangeCheck();
            //GUILayout.BeginVertical("box");
            //EditorGUILayout.LabelField("Editor settings", LabelStyle);
            //_vegetationSystemPro.ForceBillboardCellLoadinComplete = EditorGUILayout.Toggle("Force billboard loading",
            //    _vegetationSystemPro.ForceBillboardCellLoadinComplete);
            //EditorGUILayout.HelpBox("When enabled billboard cell loading will be forced completed per cell. This can remove a warning in editor mode with Jobs debuger active.", MessageType.Info);
            //GUILayout.EndVertical();

            //if (EditorGUI.EndChangeCheck())
            //{
            //    SetSceneDirty();
            //}
        }

        void DrawRenderInspector()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shadows", LabelStyle);
            _vegetationSystemPro.VegetationSettings.GrassShadows = EditorGUILayout.Toggle("Grass cast shadows",
                _vegetationSystemPro.VegetationSettings.GrassShadows);
            _vegetationSystemPro.VegetationSettings.PlantShadows = EditorGUILayout.Toggle("Plants cast shadows",
                _vegetationSystemPro.VegetationSettings.PlantShadows);
            _vegetationSystemPro.VegetationSettings.TreeShadows = EditorGUILayout.Toggle("Trees cast shadows",
                _vegetationSystemPro.VegetationSettings.TreeShadows);
            _vegetationSystemPro.VegetationSettings.ObjectShadows = EditorGUILayout.Toggle("Objects cast shadows",
                _vegetationSystemPro.VegetationSettings.ObjectShadows);
            _vegetationSystemPro.VegetationSettings.LargeObjectShadows = EditorGUILayout.Toggle(
                "Large objects cast shadows", _vegetationSystemPro.VegetationSettings.LargeObjectShadows);
            _vegetationSystemPro.VegetationSettings.BillboardShadows = EditorGUILayout.Toggle("Billboards cast shadows",
                _vegetationSystemPro.VegetationSettings.BillboardShadows);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Layers", LabelStyle);
            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.VegetationSettings.GrassLayer =
                EditorGUILayout.LayerField("Grass layer", _vegetationSystemPro.VegetationSettings.GrassLayer);
            _vegetationSystemPro.VegetationSettings.PlantLayer =
                EditorGUILayout.LayerField("Plant layer", _vegetationSystemPro.VegetationSettings.PlantLayer);
            _vegetationSystemPro.VegetationSettings.TreeLayer =
                EditorGUILayout.LayerField("Tree layer", _vegetationSystemPro.VegetationSettings.TreeLayer);
            _vegetationSystemPro.VegetationSettings.ObjectLayer = EditorGUILayout.LayerField("Object layer",
                _vegetationSystemPro.VegetationSettings.ObjectLayer);
            _vegetationSystemPro.VegetationSettings.LargeObjectLayer = EditorGUILayout.LayerField("Large object layer",
                _vegetationSystemPro.VegetationSettings.LargeObjectLayer);
            _vegetationSystemPro.VegetationSettings.BillboardLayer = EditorGUILayout.LayerField("Billboard layer",
                _vegetationSystemPro.VegetationSettings.BillboardLayer);
            EditorGUILayout.HelpBox("Select what layers vegetation should render on.", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }

            GUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shadow culling", LabelStyle);
            _vegetationSystemPro.SunDirectionalLight = (Light) EditorGUILayout.ObjectField("Sun Directional Light",
                _vegetationSystemPro.SunDirectionalLight, typeof(Light), true);

            EditorGUILayout.HelpBox("Assign the sun directional light. Used for shadow culling", MessageType.Info);

            GUILayout.EndVertical();
            
            
            GUILayout.BeginVertical("box");
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.LabelField("Disable instanced indirect on platform", LabelStyle);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectWindows = EditorGUILayout.Toggle("Windows", _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectWindows);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstncedIndirectOsx = EditorGUILayout.Toggle("OSX", _vegetationSystemPro.VegetationRenderSettings.DisableInstncedIndirectOsx);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectLinux = EditorGUILayout.Toggle("Linux", _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectLinux);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectXboxOne = EditorGUILayout.Toggle("Xbox One", _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectXboxOne);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectPs4 = EditorGUILayout.Toggle("PS4", _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectPs4);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectWsa = EditorGUILayout.Toggle("WSA", _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectWsa);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectAndroid = EditorGUILayout.Toggle("Android", _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectAndroid);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectIos = EditorGUILayout.Toggle("iOS", _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectIos);
            _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectTvOs = EditorGUILayout.Toggle("TvOS", _vegetationSystemPro.VegetationRenderSettings.DisableInstancedIndirectTvOs);

            EditorGUILayout.HelpBox("Select the platforms where you want to disable instanced indirect. This setting can only be set in editor mode", MessageType.Info);
            EditorGUI.EndDisabledGroup();
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
            GUILayout.EndVertical();
        }

        void DrawSettingsInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Sea level", LabelStyle);
            EditorGUILayout.HelpBox(
                "Sea level is realtive to the minimum height of the bounds defined for Vegetation Studio.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.SeaLevel =
                EditorGUILayout.DelayedFloatField("Sea level", _vegetationSystemPro.SeaLevel);
            _vegetationSystemPro.ExcludeSeaLevelCells = EditorGUILayout.Toggle("Exclude sea level cells",
                _vegetationSystemPro.ExcludeSeaLevelCells);

            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
                _vegetationSystemPro.RefreshVegetationSystem();
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Cell size", LabelStyle);

            GUILayout.BeginHorizontal();
            _vegetationSystemPro.VegetationCellSize = EditorGUILayout.Slider("VegetationCell size",
                _vegetationSystemPro.VegetationCellSize, 25, 250);
          
            if (GUILayout.Button("Update",GUILayout.Width(80)))
            {
                GC.Collect();
                SetSceneDirty();
                _vegetationSystemPro.RefreshVegetationSystem();
            }

            GUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox("Changing cell size will require you to re-init the persistent storage.",
                MessageType.Info);
            GUILayout.EndVertical();

            
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            _vegetationSystemPro.BillboardCellSize = EditorGUILayout.Slider("BillboardCell size",
                _vegetationSystemPro.BillboardCellSize, 500, 8000);
                       
            if (GUILayout.Button("Update",GUILayout.Width(80)))
            {
                GC.Collect();
                SetSceneDirty();
                _vegetationSystemPro.RefreshVegetationSystem();
            }
            GUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox(
                "This sets the size of the billboard cells. For normal use this does not need to be changed.",
                MessageType.Info);
         
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            _vegetationSystemPro.FloatingOriginAnchor = (Transform) EditorGUILayout.ObjectField(
                "Floating origin anchor", _vegetationSystemPro.FloatingOriginAnchor, typeof(Transform), true);
            EditorGUILayout.HelpBox(
                "Assign a Transform you want to use as a world anchor for floating origin. If no transform is set the VegetationSystem object will be used.",
                MessageType.Info);
            GUILayout.EndVertical();
        }

        void DrawTerrainInspector()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add terrains", LabelStyle);
            EditorGUILayout.HelpBox(
                "You can add any terrain implementing the IVegetationStudioTerrain interface. On standard terrains add the UnityTerrain component and then drag/drop the terrain GameObject here.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            GameObject newTerrain =
                (GameObject) EditorGUILayout.ObjectField("Add terrain", null, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (newTerrain != null)
                {
                    bool hasInterface = false;
                    MonoBehaviour[] list = newTerrain.GetComponents<MonoBehaviour>();
                    foreach (MonoBehaviour mb in list)
                    {
                        if (mb is IVegetationStudioTerrain)
                        {
                            _vegetationSystemPro.AddTerrain(mb.gameObject);
                            hasInterface = true;
                        }
                    }

                    if (!hasInterface)
                        EditorUtility.DisplayDialog("Add terrain",
                            "Could not find any component with IVegetationStudioTerrain Interface", "OK");
                    SetSceneDirty();
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add all Unity terrains"))
            {
                _vegetationSystemPro.AddAllUnityTerrains();
                SetSceneDirty();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Add all Mesh terrains"))
            {
                _vegetationSystemPro.AddAllMeshTerrains();
                SetSceneDirty();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Add all Raycast terrains"))
            {
                _vegetationSystemPro.AddAllRaycastTerrains();
                SetSceneDirty();
                SceneView.RepaintAll();
            }
            
            if (GUILayout.Button("Remove all terrains"))
            {
                _vegetationSystemPro.RemoveAllTerrains();
                SetSceneDirty();
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Current terrains", LabelStyle);
            for (int i = 0; i <= _vegetationSystemPro.VegetationStudioTerrainObjectList.Count - 1; i++)
            {
                IVegetationStudioTerrain terrain =
                    VegetationStudioTerrain.GetIVegetationStudioTerrain(_vegetationSystemPro
                        .VegetationStudioTerrainObjectList[i]);
                string terrainType = "Unknown";
                if (terrain != null)
                {
                    terrainType = terrain.TerrainType;
                }

                EditorGUI.BeginChangeCheck();
                GameObject terrainObject = (GameObject) EditorGUILayout.ObjectField(terrainType + ":",
                    _vegetationSystemPro.VegetationStudioTerrainObjectList[i], typeof(GameObject), true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!terrainObject)
                    {
                        _vegetationSystemPro.RemoveTerrain(_vegetationSystemPro.VegetationStudioTerrainObjectList[i]);
                    }

                    SetSceneDirty();
                    SceneView.RepaintAll();
                }
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Area", LabelStyle);
            EditorGUILayout.HelpBox(
                "This sets the total area for Vegetation Studio. On automatic it joins the area of all added terrains. For streaming terrain setups configure your total world area manually.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _vegetationSystemPro.AutomaticBoundsCalculation = EditorGUILayout.Toggle("Automatic calculation",
                _vegetationSystemPro.AutomaticBoundsCalculation);
            if (EditorGUI.EndChangeCheck())
            {
                if (_vegetationSystemPro.AutomaticBoundsCalculation)
                {
                    _vegetationSystemPro.CalculateVegetationSystemBounds();
                    SetSceneDirty();
                    SceneView.RepaintAll();
                }
            }

            if (_vegetationSystemPro.AutomaticBoundsCalculation)
            {
                if (GUILayout.Button("Recalculate"))
                {
                    _vegetationSystemPro.CalculateVegetationSystemBounds();
                }

                SetSceneDirty();
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(_vegetationSystemPro.AutomaticBoundsCalculation);
            _vegetationSystemPro.VegetationSystemBounds =
                EditorGUILayout.BoundsField("Total area", _vegetationSystemPro.VegetationSystemBounds);
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                SetSceneDirty();
                SceneView.RepaintAll();
            }

            if (!_vegetationSystemPro.AutomaticBoundsCalculation)
            {
                if (GUILayout.Button("Refresh"))
                {
                    _vegetationSystemPro.RefreshVegetationSystem();
                    SetSceneDirty();
                    SceneView.RepaintAll();
                }
            }

            GUILayout.EndVertical();
        }
    }
}