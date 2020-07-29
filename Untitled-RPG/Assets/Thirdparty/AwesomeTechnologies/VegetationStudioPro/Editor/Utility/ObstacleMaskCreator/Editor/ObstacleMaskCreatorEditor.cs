using System.Collections;
using System.Collections.Generic;
using System.IO;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace AwesomeTechnologies.Utility
{
    [CustomEditor(typeof(ObstacleMaskCreator))]
    public class ObstacleMaskCreatorEditor : VegetationStudioProBaseEditor
    {        
        static List<int> layerNumbers = new List<int>();
        private int _selectedTerrainIndex;
        
        public override void OnInspectorGUI()
        {
            HelpTopic = "obstacle-mask-creator";
            ShowLogo = false;
            
            ObstacleMaskCreator obstacleMaskCreator = (ObstacleMaskCreator) target;
            VegetationSystemPro vegetationSystem = obstacleMaskCreator.gameObject.GetComponent<VegetationSystemPro>();

            if (!vegetationSystem)
            {
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystem component.",
                    MessageType.Error);
                return;
            }

            base.OnInspectorGUI();

            if (vegetationSystem)
            {
                GUILayout.BeginVertical("box");
                obstacleMaskCreator.AreaRect = EditorGUILayout.RectField("Area", obstacleMaskCreator.AreaRect);
                EditorGUILayout.HelpBox(
                    "You can snap the area to any added terrain, total world area or manually setting the area for generation.",
                    MessageType.Info);
                GUILayout.EndVertical();


                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                string[] terrains = new string[vegetationSystem.VegetationStudioTerrainList.Count];
                for (int i = 0; i <= vegetationSystem.VegetationStudioTerrainList.Count - 1; i++)
                {
                    terrains[i] = vegetationSystem.VegetationStudioTerrainObjectList[i].name;
                }

                _selectedTerrainIndex = EditorGUILayout.Popup("Select terrain", _selectedTerrainIndex, terrains);
                if (GUILayout.Button("Snap to terrain", GUILayout.Width(120)))
                {
                    IVegetationStudioTerrain iVegetationStudioTerrain =
                        vegetationSystem.VegetationStudioTerrainList[_selectedTerrainIndex];
                    Bounds bounds = iVegetationStudioTerrain.TerrainBounds;
                    obstacleMaskCreator.AreaRect = RectExtension.CreateRectFromBounds(bounds);
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Snap to world area"))
                {
                    obstacleMaskCreator.AreaRect =
                        RectExtension.CreateRectFromBounds(vegetationSystem.VegetationSystemBounds);
                }

                EditorGUILayout.HelpBox(
                    "You can snap the area to any added terrain, total world area or manually setting the rect.",
                    MessageType.Info);

                GUILayout.EndVertical();
                
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Settings", LabelStyle);
                obstacleMaskCreator.ObstacleMaskQuality =
                    (ObstacleMaskQuality) EditorGUILayout.EnumPopup("Mask resolution",
                        obstacleMaskCreator.ObstacleMaskQuality);
                EditorGUILayout.HelpBox(
                    "Pixel resolution of the obstacle mask. Low = 1024x1024, Normal = 2048x2048, High = 4096x4096 and Ultra = 8192x8192",
                    MessageType.Info);
                obstacleMaskCreator.LayerMask = LayerMaskField("Obstacle layers", obstacleMaskCreator.LayerMask);
                obstacleMaskCreator.MinimumDistance =
                    EditorGUILayout.Slider("Minimum distance", obstacleMaskCreator.MinimumDistance, 0, 10);

                GUILayout.EndVertical();


                GUILayout.BeginVertical("box");
                if (GUILayout.Button("Generate obstacle mask"))
                {
                    GenerateObstacleMask();
                }

                GUILayout.EndVertical();
            }
        }

        private void GenerateObstacleMask()
        {
             string path = EditorUtility.SaveFilePanelInProject("Save mask background", "", "png",
                "Please enter a file name to save the mask background to");

            if (path.Length != 0)
            {
                ObstacleMaskCreator obstacleMaskCreator = (ObstacleMaskCreator) target;
                int textureResolution =
                    obstacleMaskCreator.GetObstacleMaskQualityPixelResolution(obstacleMaskCreator.ObstacleMaskQuality);

                Texture2D obstacleMaskTexture = new Texture2D(textureResolution, textureResolution,
                    TextureFormat.ARGB32,
                    false, true);

                float cellSize = obstacleMaskCreator.AreaRect.size.x / textureResolution;

                Vector3 terrainCorner = new Vector3(obstacleMaskCreator.AreaRect.position.x,0,obstacleMaskCreator.AreaRect.position.y);// - new Vector3(vegetationSystem.UnityTerrainData.size.x/2f,0, vegetationSystem.UnityTerrainData.size.z / 2f);

                for (int x = 0; x <= textureResolution -1; x++)
                {
                    if (x % 100 == 0)
                    {
                        EditorUtility.DisplayProgressBar("Generate obstacle mask ", "Raycast for obstacles" , (float)x/ textureResolution);
                    }

                    for (int z = 0; z <= textureResolution - 1; z++)
                    {
                        RaycastHit hit;
                        Ray ray = new Ray(new Vector3(x * cellSize, 10000, z* cellSize) + terrainCorner, Vector3.down);
                        if (Physics.SphereCast(ray,(0.5f * cellSize) + obstacleMaskCreator.MinimumDistance, out hit,float.MaxValue,obstacleMaskCreator.LayerMask))
                        {                           
                            if (!(hit.collider is TerrainCollider))
                            {
                                obstacleMaskTexture.SetPixel(x, z, Color.black);
                            }
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
                obstacleMaskTexture.Apply();
                SaveTexture(obstacleMaskTexture, path);
            }
        }
        
        public static void SaveTexture(Texture2D tex, string name)
        {
            string savePath = Application.dataPath + name.Replace("Assets", "");
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(savePath, bytes);
            AssetDatabase.Refresh();
        }

        static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            var layers = InternalEditorUtility.layers;

            layerNumbers.Clear();

            for (int i = 0; i < layers.Length; i++)
                layerNumbers.Add(LayerMask.NameToLayer(layers[i]));

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = UnityEditor.EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }

            layerMask.value = mask;

            return layerMask;
        }
    }
}