using System;
using System.Collections.Generic;
using System.IO;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public struct ShadowMapData
    {
        public float TopHeight;
        public float BottomHeight;
    }
    
    [CustomEditor(typeof(ShadowMaskCreator))]
    public class ShadowMaskCreatorEditor : VegetationStudioProBaseEditor
    {
        private int _selectedTerrainIndex;
        private static readonly int CullFarStart = Shader.PropertyToID("_CullFarStart");
        private static readonly int CullFarDistance = Shader.PropertyToID("_CullFarDistance");

        public override void OnInspectorGUI()
        {
            HelpTopic = "vegetation-shadow-mask-creator";
            ShowLogo = false;

            ShadowMaskCreator shadowMaskCreator = (ShadowMaskCreator) target;
            VegetationSystemPro vegetationSystemPro = shadowMaskCreator.gameObject.GetComponent<VegetationSystemPro>();

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
                shadowMaskCreator.AreaRect = EditorGUILayout.RectField("Area", shadowMaskCreator.AreaRect);
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
                    shadowMaskCreator.AreaRect = RectExtension.CreateRectFromBounds(bounds);
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Snap to world area"))
                {
                    shadowMaskCreator.AreaRect =
                        RectExtension.CreateRectFromBounds(vegetationSystemPro.VegetationSystemBounds);
                }

                EditorGUILayout.HelpBox(
                    "You can snap the area to any added terrain, total world area or manually setting the rect.",
                    MessageType.Info);

                GUILayout.EndVertical();
                
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Settings", LabelStyle);
                shadowMaskCreator.ShadowMaskQuality =
                    (ShadowMaskQuality)EditorGUILayout.EnumPopup("Mask resolution",
                        shadowMaskCreator.ShadowMaskQuality);
                EditorGUILayout.HelpBox(
                    "Pixel resolution of the shadow mask. Low = 1024x1024, Normal = 2048x2048, High = 4096x4096 and Ultra = 8192x8192",
                    MessageType.Info);
             
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Included vegetation", LabelStyle);
                shadowMaskCreator.IncludeTrees = EditorGUILayout.Toggle("Include Trees", shadowMaskCreator.IncludeTrees);
                shadowMaskCreator.IncludeLargeObjects = EditorGUILayout.Toggle("Include Large Objects", shadowMaskCreator.IncludeLargeObjects);
                GUILayout.EndVertical();

                if (GUILayout.Button("Generate shadow mask"))
                {
                  GenerateVegetationShadowMask(vegetationSystemPro);
                }
            }
        }
        
        bool RenderVegetationType(VegetationType vegetationType, ShadowMaskCreator shadowMaskCreator)
        {
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    return false;
                case VegetationType.Plant:
                    return false;
                case VegetationType.Tree:
                    return shadowMaskCreator.IncludeTrees;
                case VegetationType.Objects:
                    return false;
                case VegetationType.LargeObjects:
                    return shadowMaskCreator.IncludeLargeObjects;
            }
            return false;
        }

        private void GenerateVegetationShadowMask(VegetationSystemPro vegetationSystem)
        {
             string path = EditorUtility.SaveFilePanelInProject("Save mask background", "", "png",
                "Please enter a file name to save the mask background to");

            if (path.Length != 0)
            {
                ShadowMaskCreator shadowMaskCreator = (ShadowMaskCreator)target;

                //Terrain selectedTerrain = vegetationSystem.currentTerrain;
                //if (selectedTerrain == null) return;

                GameObject cameraObject = new GameObject { name = "Mask Background camera" };

                int textureResolution = shadowMaskCreator.GetShadowMaskQualityPixelResolution(shadowMaskCreator.ShadowMaskQuality);
                Shader heightShader = Shader.Find("AwesomeTechnologies/Shadows/ShadowHeight");

                RenderTexture rtDown =
                    new RenderTexture(textureResolution, textureResolution, 24, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
                    {
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Trilinear,
                        autoGenerateMips = false
                    };

                RenderTexture rtUp =
                    new RenderTexture(textureResolution, textureResolution, 24, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
                    {
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Trilinear,
                        autoGenerateMips = false
                    };

                Camera backgroundCamera = cameraObject.AddComponent<Camera>();
                backgroundCamera.targetTexture = rtDown;

                backgroundCamera.clearFlags = CameraClearFlags.Color;
                backgroundCamera.backgroundColor = Color.black;
                backgroundCamera.orthographic = true;
                backgroundCamera.orthographicSize = shadowMaskCreator.AreaRect.size.x / 2f;
                backgroundCamera.farClipPlane = 20000;
                backgroundCamera.cullingMask = 1 << shadowMaskCreator.InvisibleLayer;
                backgroundCamera.SetReplacementShader(heightShader, "");

                cameraObject.transform.position = new Vector3(shadowMaskCreator.AreaRect.x,0,shadowMaskCreator.AreaRect.y) +
                                                  new Vector3(shadowMaskCreator.AreaRect.size.x / 2f, 1000,
                                                      shadowMaskCreator.AreaRect.size.y / 2f);
                cameraObject.transform.rotation = Quaternion.Euler(90, 0, 0);

                RenderTexture.active = rtDown;


                Graphics.SetRenderTarget(rtDown);
                GL.Viewport(new Rect(0, 0, rtDown.width, rtDown.height));
                GL.Clear(true, true, new Color(0, 0, 0, 0), 1f);

                GL.PushMatrix();
                GL.LoadProjectionMatrix(backgroundCamera.projectionMatrix);
                GL.modelview = backgroundCamera.worldToCameraMatrix;
                GL.PushMatrix();
                RenderVegetationNow(vegetationSystem, shadowMaskCreator);               

                GL.PopMatrix();
                GL.PopMatrix();


                cameraObject.transform.position = new Vector3(shadowMaskCreator.AreaRect.x,0,shadowMaskCreator.AreaRect.y) +
                                                  new Vector3(shadowMaskCreator.AreaRect.size.x / 2f, -1000,
                                                      shadowMaskCreator.AreaRect.size.y / 2f);
                cameraObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
                RenderTexture.active = rtUp;
                backgroundCamera.targetTexture = rtUp;


                Graphics.SetRenderTarget(rtUp);
                GL.Viewport(new Rect(0, 0, rtUp.width, rtUp.height));
                GL.Clear(true, true, new Color(0, 0, 0, 0), 1f);

                GL.PushMatrix();
                GL.LoadProjectionMatrix(backgroundCamera.projectionMatrix);
                GL.modelview = backgroundCamera.worldToCameraMatrix;
                GL.PushMatrix();
                RenderVegetationNow(vegetationSystem, shadowMaskCreator);

                GL.PopMatrix();
                GL.PopMatrix();

                EditorUtility.DisplayProgressBar("Create shadow mask", "Render vegetation", 0f);
                RenderTexture.active = null;

                ShadowMapData[] outputHeights = new ShadowMapData[textureResolution* textureResolution];
                ComputeBuffer outputHeightBuffer = new ComputeBuffer(textureResolution * textureResolution, 8);

                ComputeShader decodeShader = (ComputeShader)Resources.Load("DecodeShadowHeight");
                int decodeKernelHandle = decodeShader.FindKernel("CSMain");
                decodeShader.SetTexture(decodeKernelHandle,"InputDown", rtDown);
                decodeShader.SetTexture(decodeKernelHandle, "InputUp", rtUp);
                decodeShader.SetBuffer(decodeKernelHandle, "OutputHeightBuffer", outputHeightBuffer);
                decodeShader.SetInt("TextureResolution", textureResolution);
                decodeShader.Dispatch(decodeKernelHandle,textureResolution / 8, textureResolution / 8, 1);
                outputHeightBuffer.GetData(outputHeights);
                outputHeightBuffer.Dispose();

                EditorUtility.DisplayProgressBar("Create shadow mask", "Calculate heights", 0.33f);
                Texture2D outputTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, true);              
                Color32[] outputColors = new Color32[outputHeights.Length];

                float minTerrainHeight = vegetationSystem.VegetationSystemBounds.min.y;
                
                for (int x = 0; x <= textureResolution - 1; x++)
                {
                    for (int y = 0; y <= textureResolution - 1; y++)
                    {
                        int i = x + y * textureResolution;

                        float xNormalized = (float) x / textureResolution;
                        float yNormalized = (float) y / textureResolution;
                        
                        float terrainHeight = sampleTerrainHeight(xNormalized, yNormalized,shadowMaskCreator.AreaRect);

                        float vegetationHeightUp = outputHeights[i].BottomHeight - minTerrainHeight;
                        float relativeHeightUp = Mathf.Clamp((vegetationHeightUp - terrainHeight)*4,0,255);

                        float vegetationHeightDown = outputHeights[i].TopHeight - minTerrainHeight;
                        float relativeHeightDown = Mathf.Clamp((vegetationHeightDown - terrainHeight) * 4, 0, 255);

                        if (relativeHeightUp > relativeHeightDown) relativeHeightUp = relativeHeightDown;

                        outputColors[i].a = 255;
                        outputColors[i].r = (byte)relativeHeightDown;
                        outputColors[i].g = 0;                      
                        outputColors[i].b = 0;
                    }
                }

                outputTexture.SetPixels32(outputColors);
                outputTexture.Apply();

                backgroundCamera.targetTexture = null;
                DestroyImmediate(rtDown);
                DestroyImmediate(rtUp);

                SaveTexture(outputTexture, path);
                DestroyImmediate(cameraObject);
                DestroyImmediate(outputTexture);
                GC.Collect();

                EditorUtility.DisplayProgressBar("Create shadow mask", "importing asset", 0.66f);
                string assetPath = path;
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
        }

        float sampleTerrainHeight(float xNormalized, float yNormalized, Rect area)
        {
            Vector3 position = new Vector3(area.position.x,10000,area.position.y) + new Vector3(xNormalized* area.size.x,0,yNormalized * area.size.y);
            Ray ray = new Ray(position,Vector3.down);
            var hits = Physics.RaycastAll(ray, 20000f);
            for (int i = 0; i <= hits.Length - 1; i++)
            {
                if (hits[i].collider is TerrainCollider)
                {
                    return hits[i].point.y;
                }
            }            
            return 0;
        }
        
        public static void SaveTexture(Texture2D tex, string name)
        {
            string savePath = Application.dataPath + name.Replace("Assets", "");
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(savePath, bytes);
            AssetDatabase.Refresh();
        }

        void RenderVegetationNow(VegetationSystemPro vegetationSystemPro, ShadowMaskCreator shadowMaskCreator)
        {
            Shader overrideShader = Shader.Find("AwesomeTechnologies/Shadows/ShadowHeight");            
            List<VegetationCell> processCellList = new List<VegetationCell>();
            vegetationSystemPro.VegetationCellQuadTree.Query(shadowMaskCreator.AreaRect,processCellList);           
            for (int i = 0; i <= vegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                for (int j = 0; j <= vegetationSystemPro.VegetationPackageProList[i].VegetationInfoList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro =
                        vegetationSystemPro.VegetationPackageProList[i].VegetationInfoList[j];

                    if (!RenderVegetationType(vegetationItemInfoPro.VegetationType, shadowMaskCreator)) continue;
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

                        VegetationItemModelInfo vegetationItemModelInfo =
                            vegetationSystemPro.GetVegetationItemModelInfo(vegetationItemID);
                        
                        for (int l = 0; l <= vegetationItemModelInfo.VegetationMeshLod0.subMeshCount - 1; l++)
                        {
                            vegetationItemModelInfo.VegetationMaterialsLOD0[l].SetFloat(CullFarStart, 50000);
                            vegetationItemModelInfo.VegetationMaterialsLOD0[l].SetFloat(CullFarDistance, 20);
                        }

                        for (int l = 0; l <= vegetationItemModelInfo.VegetationMeshLod0.subMeshCount - 1; l++)
                        {
                            Material tempMaterial = new Material(vegetationItemModelInfo.VegetationMaterialsLOD0[l]);
                            tempMaterial.shader = overrideShader;
                            tempMaterial.SetPass(0);
                            for (int m = 0; m <= vegetationInstanceList.Length - 1; m++)
                            {
                                Graphics.DrawMeshNow(vegetationItemModelInfo.VegetationMeshLod0, vegetationInstanceList[m].Matrix);
                            }
                            DestroyImmediate(tempMaterial);
                        }
                    }
                    EditorUtility.ClearProgressBar();
                }
            }
        }
    }
}