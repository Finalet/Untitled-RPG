using System.Collections.Generic;
using System.Runtime.InteropServices;
using AwesomeTechnologies.Utility;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(VegetationPackagePro))]
    public class VegetationPackageProEditor : VegetationStudioProBaseEditor
    {
        private VegetationPackagePro _vegetationPackagePro;
        string _selectedVegetationItem = "";
        private int _currentTextureItem ;
        private static readonly string[] NumberTextureStrings =
        {
            "No textures","4 textures", "8 textures", "12 textures", "16 textures"
        };
        
        private int _newTextureIndex;

        [MenuItem("Assets/Create/Awesome Technologies/VegetationPackagePro/No Texture Biome")]
        public static void CreateYourScriptableObject()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 0;
            vegetationPackagePro.InitPackage();
        }

        [MenuItem("Assets/Create/Awesome Technologies/VegetationPackagePro/4 terrain texture Biome")]
        public static void CreateVegetationPackageObject4Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 4;
            vegetationPackagePro.InitPackage();
            vegetationPackagePro.LoadDefaultTextures();
            vegetationPackagePro.SetupTerrainTextureSettings();
        }

        [MenuItem("Assets/Create/Awesome Technologies/VegetationPackagePro/8 terrain texture Biome")]
        public static void CreateVegetationPackageObject8Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 8;
            vegetationPackagePro.InitPackage();
            vegetationPackagePro.LoadDefaultTextures();
            vegetationPackagePro.SetupTerrainTextureSettings();
        }

        [MenuItem("Assets/Create/Awesome Technologies/VegetationPackagePro/12 terrain texture Biome")]
        public static void CreateVegetationPackageObject12Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            
            vegetationPackagePro.TerrainTextureCount = 12;
            vegetationPackagePro.InitPackage();
            vegetationPackagePro.LoadDefaultTextures();
            vegetationPackagePro.SetupTerrainTextureSettings();
        }

        [MenuItem("Assets/Create/Awesome Technologies/VegetationPackagePro/16 terrain texture Biome")]
        public static void CreateVegetationPackageObject16Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();

            vegetationPackagePro.TerrainTextureCount = 16;
            vegetationPackagePro.InitPackage();
            vegetationPackagePro.LoadDefaultTextures();
            vegetationPackagePro.SetupTerrainTextureSettings();
        }
        
        
        public void OnEnable()
        {
            _vegetationPackagePro = (VegetationPackagePro)target;
            _newTextureIndex = GetTerrainTextureIndex(_vegetationPackagePro);
        }
        
        private int GetTerrainTextureIndex(VegetationPackagePro vegetationPackage)
        {
            switch (vegetationPackage.TerrainTextureCount)
            {
                case 0:
                    return 0;
                case 4:
                    return 1;
                case 8:
                    return 2;
                case 12:
                    return 3;
                case 16:
                    return 4;
            }

            return 0;
        }
        
        private int GetTerrainTextureCountFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return 0;
                case 1:
                    return 4;
                case 2:
                    return 8;
                case 3:
                    return 12;
                case 4:
                    return 16;
            }

            return 0;
        }
        
        private void ChangeTerrainTextureCount(VegetationPackagePro vegetationPackagePro, int newCount)
        {
            if (vegetationPackagePro.TerrainTextureCount == newCount) return;

            if (newCount > vegetationPackagePro.TerrainTextureCount)
            {
                vegetationPackagePro.TerrainTextureCount = newCount;
                vegetationPackagePro.LoadDefaultTextures();
                vegetationPackagePro.SetupTerrainTextureSettings();
            }
            else
            {
                vegetationPackagePro.TerrainTextureCount = newCount;
                vegetationPackagePro.ResizeTerrainTextureList(newCount);
                vegetationPackagePro.ResizeTerrainTextureSettingsList(newCount);
            }           
        }
        
      

        public override void OnInspectorGUI()
        {
            _vegetationPackagePro = (VegetationPackagePro) target;

            base.OnInspectorGUI();
            //EditorUtility.SetDirty(target);
            for (int i = 0; i <= _vegetationPackagePro.VegetationInfoList.Count - 1; i++)
            {
                bool changed = false;
                VegetationItemInfoPro vegetationItemInfo = _vegetationPackagePro.VegetationInfoList[i];
                if (vegetationItemInfo.VegetationItemID == "")
                {
                    vegetationItemInfo.VegetationItemID = System.Guid.NewGuid().ToString();
                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(_vegetationPackagePro);
                }
            }           
            
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox(
                "To edit an vegetation package add it to a vegetation system pro component",
                MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Biome", LabelStyle);
            EditorGUI.BeginDisabledGroup(true);
            _vegetationPackagePro.BiomeType =
                (BiomeType)EditorGUILayout.EnumPopup("Select biome", _vegetationPackagePro.BiomeType);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();


            if (_vegetationPackagePro.VegetationInfoList.Count > 0)
            {
               
                List<string> vegetationItemIdList =
                    VegetationPackageEditorTools.CreateVegetationInfoIdList(_vegetationPackagePro);
                VegetationPackageEditorTools.DrawVegetationItemSelector(_vegetationPackagePro, vegetationItemIdList, 60,
                    ref _selectedVegetationItem);

                VegetationItemInfoPro vegetationItemInfoPro =
                    _vegetationPackagePro.GetVegetationInfo(_selectedVegetationItem);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("VegetationItemID", LabelStyle);
                EditorGUILayout.SelectableLabel(vegetationItemInfoPro.VegetationItemID);
                GUILayout.EndHorizontal();

            }
            else
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox(
                    "Vegetation package has no vegetation items configured",
                    MessageType.Info);
                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Regenerate VegetationItems IDs"))
            {
                _vegetationPackagePro.RegenerateVegetationItemIDs();
                EditorUtility.SetDirty(_vegetationPackagePro);
            }
            EditorGUILayout.HelpBox(
                "Creating new IDs is usefull if you duplicate a vegetation package/biome and will use both on the same terrain.",
                MessageType.Info);
            
            GUILayout.EndVertical();
            
            
            GUILayout.BeginVertical("box");
           
            _newTextureIndex = EditorGUILayout.Popup("Number of terrain textures", _newTextureIndex, NumberTextureStrings);
            if (GUILayout.Button("Change number of textures in package"))
            {
                ChangeTerrainTextureCount(_vegetationPackagePro,GetTerrainTextureCountFromIndex(_newTextureIndex));
            }
            GUILayout.EndVertical();

            if (_vegetationPackagePro.TerrainTextureCount > 0)
            {
                EditorGUILayout.LabelField("Terrain textures", LabelStyle);
                GUIContent[] textureImageButtons = new GUIContent[_vegetationPackagePro.TerrainTextureList.Count];
                for (int i = 0; i <= _vegetationPackagePro.TerrainTextureList.Count - 1; i++)
                {
                    var textureItemTexture = AssetPreview.GetAssetPreview(_vegetationPackagePro.TerrainTextureList[i].Texture);
                    Texture2D convertedTexture = new Texture2D(2, 2, TextureFormat.ARGB32, true, true);
                    if (textureItemTexture)
                    {
                        convertedTexture.LoadImage(textureItemTexture.EncodeToPNG());
                    }

                    textureImageButtons[i] = new GUIContent { image = convertedTexture };
                }
                int imageWidth = 60;
                int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - imageWidth / 2f) / imageWidth);
                int rows = Mathf.CeilToInt((float)textureImageButtons.Length / columns);
                int gridHeight = (rows) * imageWidth;
                _currentTextureItem = GUILayout.SelectionGrid(_currentTextureItem, textureImageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

                GUIStyle variantStyle = new GUIStyle(EditorStyles.helpBox);

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Terrain layer: " + (_currentTextureItem + 1).ToString(), LabelStyle);
                EditorGUI.BeginDisabledGroup(true);
                _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].Enabled = EditorGUILayout.Toggle("Enable", _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].Enabled);

                EditorGUILayout.BeginHorizontal(variantStyle);
                EditorGUILayout.LabelField("Texture " + (_currentTextureItem + 1).ToString() + " Height", LabelStyle, GUILayout.Width(150));
                _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].TextureHeightCurve = EditorGUILayout.CurveField(_vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].TextureHeightCurve, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(75));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(variantStyle);
                EditorGUILayout.LabelField("Texture " + (_currentTextureItem + 1).ToString() + " Steepness", LabelStyle, GUILayout.Width(150));
                _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].TextureSteepnessCurve = EditorGUILayout.CurveField(_vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].TextureSteepnessCurve, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(75));
                EditorGUILayout.EndHorizontal();

                _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].UseNoise = EditorGUILayout.Toggle("Use perlin noise", _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].UseNoise);
               
                if (_vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].UseNoise)
                {
                    _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].InverseNoise = EditorGUILayout.Toggle("Inverse noise", _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].InverseNoise);
                    _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].NoiseScale = EditorGUILayout.Slider("Noise scale", _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].NoiseScale, 1, 50f);
                    _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].NoiseOffset = EditorGUILayout.Vector2Field("Noise offset", _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].NoiseOffset);

                }
                _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].TextureWeight = EditorGUILayout.Slider("Texture weight", _vegetationPackagePro.TerrainTextureSettingsList[_currentTextureItem].TextureWeight, 0, 5f);

                EditorGUI.EndDisabledGroup();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox(
                    "Vegetation package has no textures configured",
                    MessageType.Info);
                GUILayout.EndVertical();
            }
        }
    }
}
