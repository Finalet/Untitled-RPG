using System.IO;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    [CustomEditor(typeof(MaskBackgroundCreator))]
    public class MaskBackgroundCreatorEditor : VegetationStudioProBaseEditor
    {
        private int _selectedTerrainIndex;

        public override void OnInspectorGUI()
        {
            base.ShowLogo = false;
            HelpTopic = "background-mask-creator";
            base.OnInspectorGUI();

            MaskBackgroundCreator maskBackgroundCreator = (MaskBackgroundCreator) target;
            VegetationSystemPro vegetationSystemPro =
                maskBackgroundCreator.gameObject.GetComponent<VegetationSystemPro>();
            if (vegetationSystemPro)
            {
                GUILayout.BeginVertical("box");
                maskBackgroundCreator.AreaRect = EditorGUILayout.RectField("Area", maskBackgroundCreator.AreaRect);
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
                    maskBackgroundCreator.AreaRect = RectExtension.CreateRectFromBounds(bounds);
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Snap to world area"))
                {
                    maskBackgroundCreator.AreaRect =
                        RectExtension.CreateRectFromBounds(vegetationSystemPro.VegetationSystemBounds);
                }

                EditorGUILayout.HelpBox(
                    "You can snap the area to any added terrain, total world area or manually setting the rect.",
                    MessageType.Info);

                GUILayout.EndVertical();

                maskBackgroundCreator.BackgroundMaskQuality =
                    (BackgroundMaskQuality) EditorGUILayout.EnumPopup("Mask resolution",
                        maskBackgroundCreator.BackgroundMaskQuality);
                EditorGUILayout.HelpBox(
                    "Pixel resolution of the mask background. Low = 1024x1024, Normal = 2048x2048 and High =4096x4096",
                    MessageType.Info);

                if (GUILayout.Button("Generate mask background/template"))
                {
                    GenerateMaskBackground(maskBackgroundCreator.AreaRect);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component.",
                    MessageType.Error);
            }
        }

        void GenerateMaskBackground(Rect area)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save mask background", "", "png",
                "Please enter a file name to save the mask background to");
            if (path.Length != 0)
            {
                
                
                
                MaskBackgroundCreator maskBackgroundCreator = (MaskBackgroundCreator) target;
                GameObject cameraObject = new GameObject {name = "Mask Background camera"};

                int textureResolution =
                    maskBackgroundCreator.GetBackgroundMaskQualityPixelResolution(maskBackgroundCreator
                        .BackgroundMaskQuality);
                
                float factor = area.width / area.height;
                
                int textureWidth = textureResolution;
                int textureHeight = Mathf.RoundToInt(textureResolution / factor);


                Camera backgroundCamera = cameraObject.AddComponent<Camera>();
                backgroundCamera.orthographic = true;
                backgroundCamera.orthographicSize = area.size.x/2f/factor;
                backgroundCamera.farClipPlane = 20000;

              

                RenderTexture rt =
                    new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.Linear)
                    {
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Trilinear,
                        autoGenerateMips = false
                    };
                backgroundCamera.targetTexture = rt;

                cameraObject.transform.position = new Vector3(area.center.x,0,area.center.y) +
                                                  new Vector3(0, 1000, 0);
                cameraObject.transform.rotation = Quaternion.Euler(90, 0, 0);

                backgroundCamera.Render();

                Texture2D newTexture = new Texture2D(textureWidth, textureHeight);
                RenderTexture.active = rt;
                newTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
                RenderTexture.active = null;
                newTexture.Apply();
                SaveTexture(newTexture, path);

                backgroundCamera.targetTexture = null;
                DestroyImmediate(rt);
                DestroyImmediate(cameraObject);
            }
        }

        public static void SaveTexture(Texture2D tex, string name)
        {
            string savePath = Application.dataPath + name.Replace("Assets", "");
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(savePath, bytes);
            AssetDatabase.Refresh();
        }
    }
}