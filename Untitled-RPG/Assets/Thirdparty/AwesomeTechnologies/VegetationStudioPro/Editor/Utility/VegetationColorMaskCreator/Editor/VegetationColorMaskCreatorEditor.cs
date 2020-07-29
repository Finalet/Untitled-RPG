using System;
using System.Collections.Generic;
using System.IO;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using UnityEditor;
using UnityEngine;


namespace AwesomeTechnologies.Utility
{
    [CustomEditor(typeof(VegetationColorMaskCreator))]
    public class VegetationColorMaskCreatorEditor : VegetationStudioProBaseEditor
    {
        private RenderTexture _rt;
        private int _textureResolution;
        private string _path;
        private GameObject _cameraObject;
        private Camera _backgroundCamera;
        private int _selectedTerrainIndex;

        public override void OnInspectorGUI()
        {
            HelpTopic = "vegetation-color-mask-creator";
            ShowLogo = false;
            
            VegetationColorMaskCreator colorMaskCreator = (VegetationColorMaskCreator) target;
            VegetationSystemPro vegetationSystemPro = colorMaskCreator.gameObject.GetComponent<VegetationSystemPro>();

            if (!vegetationSystemPro)
            {
                {
                    EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component.",
                        MessageType.Error);
                    return;
                }
            }

            base.OnInspectorGUI();

            if (vegetationSystemPro)
            {
                GUILayout.BeginVertical("box");
                colorMaskCreator.AreaRect = EditorGUILayout.RectField("Area", colorMaskCreator.AreaRect);
                EditorGUILayout.HelpBox(
                    "You can snap the area to any added terrain, total world area or manually setting the area for generation.",
                    MessageType.Info);
                GUILayout.EndVertical();


                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                string[] terrains = new string[vegetationSystemPro.VegetationStudioTerrainList.Count];
                for (int i = 0; i <= vegetationSystemPro.VegetationStudioTerrainList.Count - 1; i++)
                {
                    terrains[i] = vegetationSystemPro.VegetationStudioTerrainObjectList[i].name;
                }

                _selectedTerrainIndex = EditorGUILayout.Popup("Select terrain", _selectedTerrainIndex, terrains);
                if (GUILayout.Button("Snap to terrain", GUILayout.Width(120)))
                {
                    IVegetationStudioTerrain iVegetationStudioTerrain =
                        vegetationSystemPro.VegetationStudioTerrainList[_selectedTerrainIndex];
                    Bounds bounds = iVegetationStudioTerrain.TerrainBounds;
                    colorMaskCreator.AreaRect = RectExtension.CreateRectFromBounds(bounds);
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Snap to world area"))
                {
                    colorMaskCreator.AreaRect =
                        RectExtension.CreateRectFromBounds(vegetationSystemPro.VegetationSystemBounds);
                }

                EditorGUILayout.HelpBox(
                    "You can snap the area to any added terrain, total world area or manually setting the rect.",
                    MessageType.Info);

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Settings", LabelStyle);
                colorMaskCreator.VegetationColorMaskQuality =
                    (VegetationColorMaskQuality) EditorGUILayout.EnumPopup("Mask resolution",
                        colorMaskCreator.VegetationColorMaskQuality);
                EditorGUILayout.HelpBox(
                    "Pixel resolution of the mask background. Low = 1024x1024, Normal = 2048x2048, High = 4096x4096 and Ultra = 8192x8192",
                    MessageType.Info);

                colorMaskCreator.InvisibleLayer =
                    EditorGUILayout.IntSlider("Mask render layer", colorMaskCreator.InvisibleLayer, 0, 30);
                EditorGUILayout.HelpBox(
                    "Select a empty layer with no scene objects. This is used to render the color mask.",
                    MessageType.Info);

                colorMaskCreator.VegetationScale =
                    EditorGUILayout.Slider("Grass/Plant scale", colorMaskCreator.VegetationScale, 1f, 3f);
                EditorGUILayout.HelpBox(
                    "This will increase the scale of each individual grass/plant patch to compensate for grass plane orientation",
                    MessageType.Info);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Included vegetation", LabelStyle);
                colorMaskCreator.IncludeGrass = EditorGUILayout.Toggle("Include Grass", colorMaskCreator.IncludeGrass);
                colorMaskCreator.IncludePlants =
                    EditorGUILayout.Toggle("Include Plants", colorMaskCreator.IncludePlants);
                colorMaskCreator.IncludeTrees = EditorGUILayout.Toggle("Include Trees", colorMaskCreator.IncludeTrees);
                colorMaskCreator.IncludeObjects =
                    EditorGUILayout.Toggle("Include Objects", colorMaskCreator.IncludeObjects);
                colorMaskCreator.IncludeLargeObjects =
                    EditorGUILayout.Toggle("Include Large Objects", colorMaskCreator.IncludeLargeObjects);
                GUILayout.EndVertical();

                if (GUILayout.Button("Generate vegetation color mask"))
                {
                    GenerateVegetationColorMask(vegetationSystemPro);
                }
            }
        }

        void GenerateVegetationColorMask(VegetationSystemPro vegetationSystemPro)
        {
            _path = EditorUtility.SaveFilePanelInProject("Save mask background", "", "png",
                "Please enter a file name to save the mask background to");

            if (_path.Length != 0)
            {
                VegetationColorMaskCreator colorMaskCreator = (VegetationColorMaskCreator) target;

                //Terrain selectedTerrain = _vegetationSystem.currentTerrain;
                //if (selectedTerrain == null) return;

                _cameraObject = new GameObject {name = "Mask Background camera"};

                _textureResolution = colorMaskCreator.GetVegetationColorMaskQualityPixelResolution(colorMaskCreator
                    .VegetationColorMaskQuality);

                _rt =
                    new RenderTexture(_textureResolution, _textureResolution, 24, RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.Linear)
                    {
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Trilinear,
                        autoGenerateMips = false
                    };

                _backgroundCamera = _cameraObject.AddComponent<Camera>();
                _backgroundCamera.targetTexture = _rt;

                _backgroundCamera.clearFlags = CameraClearFlags.Color;
                _backgroundCamera.backgroundColor = colorMaskCreator.BackgroundColor;
                _backgroundCamera.orthographic = true;
                _backgroundCamera.orthographicSize = colorMaskCreator.AreaRect.size.x / 2f;
                _backgroundCamera.farClipPlane = 20000;
                _backgroundCamera.cullingMask = 1 << colorMaskCreator.InvisibleLayer;

                _cameraObject.transform.position = new Vector3(colorMaskCreator.AreaRect.position.x, 0,
                                                       colorMaskCreator.AreaRect.position.y) +
                                                   new Vector3(colorMaskCreator.AreaRect.size.x / 2f, 1000,
                                                       colorMaskCreator.AreaRect.size.y / 2f);
                _cameraObject.transform.rotation = Quaternion.Euler(90, 0, 0);

                _backgroundCamera.Render();
                Graphics.SetRenderTarget(_rt);
                GL.Viewport(new Rect(0, 0, _rt.width, _rt.height));
                GL.Clear(true, true, new Color(0, 0, 0, 0), 1f);

                GL.PushMatrix();
                GL.LoadProjectionMatrix(_backgroundCamera.projectionMatrix);
                GL.modelview = _backgroundCamera.worldToCameraMatrix;
                GL.PushMatrix();

                List<VegetationCell> selectedCells = new List<VegetationCell>();
                vegetationSystemPro.VegetationCellQuadTree.Query(colorMaskCreator.AreaRect,selectedCells);
                RenderVegetation(selectedCells, vegetationSystemPro, colorMaskCreator);

                GL.PopMatrix();
                GL.PopMatrix();
                PostProcessMask();
            }
        }

        void PostProcessMask()
        {
            RenderTexture.active = _rt;
            Texture2D newTexture =
                new Texture2D(_textureResolution, _textureResolution, TextureFormat.ARGB32, true);
            newTexture.ReadPixels(new Rect(0, 0, _textureResolution, _textureResolution), 0, 0);
            RenderTexture.active = null;
            newTexture.Apply();

            EditorUtility.DisplayProgressBar("Alpha padding", "Alpha", 0f);
            
            Texture2D paddedTexture = TextureExtention.CreatePaddedTexture(newTexture);
            if (paddedTexture == null)
            {
                paddedTexture = newTexture;
            }
            else
            {
                DestroyImmediate(newTexture);
            }

            _backgroundCamera.targetTexture = null;
            DestroyImmediate(_rt);
            GC.Collect();

            EditorUtility.DisplayProgressBar("Create color mask", "Saving mask", 0.33f);
            SaveTexture(paddedTexture, _path);
            DestroyImmediate(_cameraObject);
            DestroyImmediate(paddedTexture);
            GC.Collect();

            EditorUtility.DisplayProgressBar("Create color mask", "importing asset", 0.66f);
            string assetPath = _path;
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;
                tImporter.filterMode = FilterMode.Point;
                tImporter.maxTextureSize = 8192;
                tImporter.SaveAndReimport();
            }

            EditorUtility.ClearProgressBar();
        }

        public static void SaveTexture(Texture2D tex, string name)
        {
            string savePath = Application.dataPath + name.Replace("Assets", "");
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(savePath, bytes);
            AssetDatabase.Refresh();

            if (PlayerSettings.colorSpace == ColorSpace.Linear)
            {
                Texture2D colorMaskTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
                AssetUtility.SetTextureSGBA(colorMaskTexture, false);
            }
        }

        bool RenderVegetationType(VegetationType vegetationType, VegetationColorMaskCreator colorMaskCreator)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return colorMaskCreator.IncludeGrass;
                case VegetationType.Plant:
                    return colorMaskCreator.IncludePlants;
                case VegetationType.Tree:
                    return colorMaskCreator.IncludeTrees;
                case VegetationType.Objects:
                    return colorMaskCreator.IncludeObjects;
                case VegetationType.LargeObjects:
                    return colorMaskCreator.IncludeLargeObjects;
            }

            return false;
        }

        void RenderVegetation(List<VegetationCell> processCellList, VegetationSystemPro vegetationSystemPro,
            VegetationColorMaskCreator colorMaskCreator)
        {
            for (int i = 0; i <= vegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                for (int j = 0; j <= vegetationSystemPro.VegetationPackageProList[i].VegetationInfoList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro =
                        vegetationSystemPro.VegetationPackageProList[i].VegetationInfoList[j];

                    if (!RenderVegetationType(vegetationItemInfoPro.VegetationType, colorMaskCreator)) continue;
//
                    for (int k = 0; k <= processCellList.Count - 1; k++)
                    {
                        if (j % 10 == 0)
                        {
                            EditorUtility.DisplayProgressBar("Render vegetation item: " + vegetationItemInfoPro.Name,
                                "Render cell " + k + "/" + (processCellList.Count - 1),
                                k / ((float) processCellList.Count - 1));
                        }

                        VegetationCell vegetationCell = processCellList[k];
                        string vegetationItemID = vegetationItemInfoPro.VegetationItemID;

                        vegetationSystemPro.SpawnVegetationCell(vegetationCell, vegetationItemID);

                        NativeList<MatrixInstance> vegetationInstanceList =
                            vegetationSystemPro.GetVegetationItemInstances(vegetationCell, vegetationItemID);
                        List<Matrix4x4> instanceList = new List<Matrix4x4>();

                        for (int l = 0; l <= vegetationInstanceList.Length - 1; l++)
                        {
                            Vector3 position =
                                MatrixTools.ExtractTranslationFromMatrix(vegetationInstanceList[l].Matrix);
                            Vector3 scale = MatrixTools.ExtractScaleFromMatrix(vegetationInstanceList[l].Matrix);
                            Quaternion rotation =
                                MatrixTools.ExtractRotationFromMatrix(vegetationInstanceList[l].Matrix);
                            Vector3 newPosition = new Vector3(position.x, 0, position.z);
                            Vector3 newScale;
                            if (vegetationItemInfoPro.VegetationType == VegetationType.Grass ||
                                vegetationItemInfoPro.VegetationType == VegetationType.Plant)
                            {
                                newScale = new Vector3(scale.x * colorMaskCreator.VegetationScale,
                                    scale.y * colorMaskCreator.VegetationScale,
                                    scale.z * colorMaskCreator.VegetationScale);
                            }
                            else
                            {
                                newScale = scale;
                            }

                            Matrix4x4 newMatrix = Matrix4x4.TRS(newPosition, rotation, newScale);
                            instanceList.Add(newMatrix);
                        }

                        VegetationItemModelInfo vegetationItemModelInfo =
                            vegetationSystemPro.GetVegetationItemModelInfo(vegetationItemID);

                        for (int l = 0; l <= vegetationItemModelInfo.VegetationMeshLod0.subMeshCount - 1; l++)
                        {
                            Material tempMaterial = new Material(vegetationItemModelInfo.VegetationMaterialsLOD0[l]);
                            tempMaterial.shader =
                                Shader.Find("AwesomeTechnologies/Vegetation/RenderVegetationColorMask");
                            tempMaterial.SetPass(0);
                            for (int m = 0; m <= instanceList.Count - 1; m++)
                            {
                                Graphics.DrawMeshNow(vegetationItemModelInfo.VegetationMeshLod0, instanceList[m]);
                            }

                            DestroyImmediate(tempMaterial);
                        }
                        vegetationCell.ClearCache();
                    }

                    EditorUtility.ClearProgressBar();
                }

                GC.Collect();
            }


//            for (int i = 0; i <= vegetationSystem.CurrentVegetationPackage.VegetationInfoList.Count - 1; i++)
//            {
//                VegetationItemInfo vegetationItemInfo = vegetationSystem.CurrentVegetationPackage.VegetationInfoList[i];
//                //for (int l = 0; l <= vegetationSystem.VegetationModelInfoList[i].VegetationMeshLod0.subMeshCount - 1; l++)
//                //{
//                //    vegetationSystem.VegetationModelInfoList[i].VegetationMaterialsLOD0[l].SetFloat("_CullFarStart",50000);
//                //    vegetationSystem.VegetationModelInfoList[i].VegetationMaterialsLOD0[l].SetFloat("_CullFarDistance", 20);
//                //}
//
//                if (!RenderVegetationType(vegetationItemInfo.VegetationType,colorMaskCreator)) continue;
//
//                for (int j = 0; j <= processCellList.Count - 1; j++)
//                {
//                    if (j % 100 == 0)
//                    {
//                        EditorUtility.DisplayProgressBar("Render vegetation item: " + vegetationItemInfo.Name, "Render cell " + j + "/" + (processCellList.Count - 1), j / ((float)processCellList.Count - 1));
//                    }                   
//
//                    VegetationCell vegetationCell = processCellList[j];
//                    List<Matrix4x4> instanceList =
//                        vegetationCell.DirectSpawnVegetation(vegetationItemInfo.VegetationItemID, true);
//
//                    for (int k = 0; k <= instanceList.Count - 1; k++)
//                    {
//                        Vector3 position = MatrixTools.ExtractTranslationFromMatrix(instanceList[k]);
//                        Vector3 scale = MatrixTools.ExtractScaleFromMatrix(instanceList[k]);
//                        Quaternion rotation = MatrixTools.ExtractRotationFromMatrix(instanceList[k]);
//                        Vector3 newPosition = new Vector3(position.x,0,position.z);
//                        Vector3 newScale;
//                        if (vegetationItemInfo.VegetationType == VegetationType.Grass ||
//                            vegetationItemInfo.VegetationType == VegetationType.Plant)
//                        {
//                             newScale = new Vector3(scale.x * colorMaskCreator.VegetationScale, scale.y * colorMaskCreator.VegetationScale, scale.z * colorMaskCreator.VegetationScale);
//                        }
//                        else
//                        {
//                            newScale = scale;
//                        }
//                 
//                        Matrix4x4 newMatrix = Matrix4x4.TRS(newPosition, rotation, newScale);
//                        instanceList[k] = newMatrix;                       
//                    }
//
//                    for (int l = 0; l <= vegetationSystem.VegetationModelInfoList[i].VegetationMeshLod0.subMeshCount - 1; l++)
//                    {
//
//                            Material tempMaterial = new Material(vegetationSystem.VegetationModelInfoList[i].VegetationMaterialsLOD0[l]);
//                            tempMaterial.shader =  Shader.Find("AwesomeTechnologies/Vegetation/RenderVegetationColorMask");
//                            tempMaterial.SetPass(0);
//                            for (int k = 0; k <= instanceList.Count - 1; k++)
//                            {
//                                Graphics.DrawMeshNow(vegetationSystem.VegetationModelInfoList[i].VegetationMeshLod0,
//                                    instanceList[k]);
//                                //Graphics.DrawMesh(vegetationSystem.VegetationModelInfoList[i].VegetationMeshLod0, instanceList[k],
//                                //    vegetationSystem.VegetationModelInfoList[i].VegetationMaterialsLOD0[l],
//                                //    colorMaskCreator.InvisibleLayer, null, l);
//                            }
//
//                        DestroyImmediate(tempMaterial);                    
//                    }
//                }
//                EditorUtility.ClearProgressBar();
//            }
        }
    }
}