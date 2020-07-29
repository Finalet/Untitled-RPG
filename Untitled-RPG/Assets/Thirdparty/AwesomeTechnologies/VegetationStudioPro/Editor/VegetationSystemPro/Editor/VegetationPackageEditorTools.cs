using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.VegetationSystem;

//using AwesomeTechnologies.Common;

public class VegetationPackageEditorTools
{

    public enum VegetationItemTypeSelection
    {
        AllVegetationItems,
        LargeItems,
        Grass,
        Plants,
        Trees, 
        Objects,
        LargeObjects

    }

    public static List<string> CreateVegetationInfoIdList(VegetationPackagePro vegetationPackage)
    {
        List<string> resultList = new List<string>();

        for (int i = 0; i <= vegetationPackage.VegetationInfoList.Count - 1; i++)
        {
            resultList.Add(vegetationPackage.VegetationInfoList[i].VegetationItemID);
        }
        return resultList;
    }

    public static VegetationItemTypeSelection GetVegetationItemTypeSelection(int index)
    {
        switch (index)
        {
            case 0:
                return VegetationItemTypeSelection.AllVegetationItems;
            case 1:
                return VegetationItemTypeSelection.Trees;
            case 2:
                return VegetationItemTypeSelection.LargeObjects;
            case 3:
                return VegetationItemTypeSelection.Objects;
            case 4:
                return VegetationItemTypeSelection.Plants;
            case 5:
                return VegetationItemTypeSelection.Grass;
        }

        return VegetationItemTypeSelection.AllVegetationItems;
    }

    public static List<string> CreateVegetationInfoIdList(VegetationPackagePro vegetationPackage, VegetationType[] vegetationTypes)
    {
        List<string> resultList = new List<string>();

        for (int i = 0; i <= vegetationPackage.VegetationInfoList.Count - 1; i++)
        {
            if (vegetationTypes.Contains(vegetationPackage.VegetationInfoList[i].VegetationType))
            {
                resultList.Add(vegetationPackage.VegetationInfoList[i].VegetationItemID);
            }
        }
        return resultList;
    }


    //public static void DrawPrefabSelectorGrid(List<GameObject> prefabList, int imageSize, ref int selectedGridIndex)
    //{
    //    GUIContent[] imageButtons = new GUIContent[prefabList.Count];

    //    for (int i = 0; i <= prefabList.Count - 1; i++)
    //    {
    //        imageButtons[i] = new GUIContent
    //        {
    //            image = AssetPreviewCache.GetAssetPreview(prefabList[i])
    //        };


    //    }
    //    int imageWidth = imageSize;
    //    int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
    //    int rows = Mathf.CeilToInt((float)imageButtons.Length / columns);
    //    int gridHeight = (rows) * imageWidth;

    //    if (imageButtons.Length > 0)
    //    {
    //        selectedGridIndex = GUILayout.SelectionGrid(selectedGridIndex, imageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
    //    }
    //}

    //public static void DrawTextureSelectorGrid(List<Texture2D> textureList, int imageSize, ref int selectedGridIndex)
    //{


    //    GUIContent[] imageButtons = new GUIContent[textureList.Count];

    //    for (int i = 0; i <= textureList.Count - 1; i++)
    //    {
    //        imageButtons[i] = new GUIContent
    //        {
    //            image = AssetPreviewCache.GetAssetPreview(textureList[i])
    //        };
    //    }
    //    int imageWidth = imageSize;
    //    int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
    //    int rows = Mathf.CeilToInt((float)imageButtons.Length / columns);
    //    int gridHeight = (rows) * imageWidth;

    //    if (imageButtons.Length > 0)
    //    {
    //        selectedGridIndex = GUILayout.SelectionGrid(selectedGridIndex, imageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
    //    }
    //}

    public static void DrawVegetationItemSelector(VegetationPackagePro vegetationPackage, List<string> vegetationInfoIdList, int imageSize, ref string selectedVegetationItemId)
    {
        AssetPreview.SetPreviewTextureCacheSize(100 + vegetationPackage.VegetationInfoList.Count);

        VegetationInfoIDComparer vIc = new VegetationInfoIDComparer
        {
            VegetationInfoList = vegetationPackage.VegetationInfoList
        };
        vegetationInfoIdList.Sort(vIc.Compare);

        GUIContent[] imageButtons = new GUIContent[vegetationInfoIdList.Count];

        for (int i = 0; i <= vegetationInfoIdList.Count - 1; i++)
        {           
            VegetationItemInfoPro vegetationItemInfo = vegetationPackage.GetVegetationInfo(vegetationInfoIdList[i]);
            if (vegetationItemInfo == null)
            {
                imageButtons[i] = new GUIContent
                {
                    image = AssetPreviewCache.GetAssetPreview(null)
                };
            }
            else
            {
                if (vegetationItemInfo.PrefabType == VegetationPrefabType.Mesh)
                {
                    imageButtons[i] = new GUIContent
                    {
                        image = AssetPreviewCache.GetAssetPreview(vegetationItemInfo.VegetationPrefab)
                    };
                }
                else
                {
                    imageButtons[i] = new GUIContent
                    {
                        image = AssetPreviewCache.GetAssetPreview(vegetationItemInfo.VegetationTexture)
                    };
                }
            }
        }
        int imageWidth = imageSize;
        int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - imageWidth / 2f) / imageWidth);
        int rows = Mathf.CeilToInt((float)imageButtons.Length / columns);
        int gridHeight = (rows) * imageWidth;


        int selectedGridIndex = vegetationInfoIdList.IndexOf(selectedVegetationItemId);
        if (selectedGridIndex < 0) selectedGridIndex = 0;

        if (imageButtons.Length > 0)
        {

            selectedGridIndex = GUILayout.SelectionGrid(selectedGridIndex, imageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
        }

        selectedVegetationItemId = vegetationInfoIdList.Count > selectedGridIndex ? vegetationInfoIdList[selectedGridIndex] : "";
        DrawSelectedName(selectedVegetationItemId, vegetationPackage);
    }

    static void DrawSelectedName(string vegetationItemID, VegetationPackagePro vegetationPackage)
    {
        if (vegetationItemID != "")
        {
            GUIStyle selectedStyle = new GUIStyle("Label") { fontStyle = FontStyle.Italic, richText = true }; //GUIStyle
            if (EditorGUIUtility.isProSkin)
            {
                LabelStyle.normal.textColor = new Color(1f, 1f, 1f);
            }
            else
            {
                LabelStyle.normal.textColor = new Color(0f, 0f, 0f);
            }

            VegetationItemInfoPro vegetationItemInfo = vegetationPackage.GetVegetationInfo(vegetationItemID);
            EditorGUILayout.LabelField("Selected: <b>" + vegetationItemInfo.Name + "</b>", selectedStyle);
        }
    }

    public static void DrawVegetationItemSelector(VegetationSystemPro vegetationSystemPro, VegetationPackagePro vegetationPackage, ref int selectedGridIndex, ref int vegIndex, ref int selectionCount, VegetationItemTypeSelection vegetationItemTypeSelection, int imageSize)
    {
        if (vegetationPackage == null) return;
        //AssetPreview.SetPreviewTextureCacheSize(100 + vegetationPackage.VegetationInfoList.Count);
        AssetPreview.SetPreviewTextureCacheSize(100 + vegetationSystemPro.GetMaxVegetationPackageItemCount());

        List<int> vegetationItemIndexList = new List<int>();
        for (int i = 0;
            i <= vegetationPackage.VegetationInfoList.Count - 1;
            i++)
        {
            VegetationItemInfoPro vegetationItemInfo = vegetationPackage.VegetationInfoList[i];
            switch (vegetationItemTypeSelection)
            {
                case VegetationItemTypeSelection.AllVegetationItems:
                    vegetationItemIndexList.Add(i);
                    break;
                case VegetationItemTypeSelection.LargeItems:
                   
                    if (vegetationItemInfo.VegetationType == VegetationType.Objects ||
                        vegetationItemInfo.VegetationType == VegetationType.LargeObjects ||
                        vegetationItemInfo.VegetationType == VegetationType.Tree)
                    {
                        vegetationItemIndexList.Add(i);
                    }
                    break;
                case VegetationItemTypeSelection.Grass:
                    if (vegetationItemInfo.VegetationType == VegetationType.Grass)
                    {
                        vegetationItemIndexList.Add(i);
                    }
                    break;
                case VegetationItemTypeSelection.Plants:
                    if (vegetationItemInfo.VegetationType == VegetationType.Plant)
                    {
                        vegetationItemIndexList.Add(i);
                    }
                    break;
                case VegetationItemTypeSelection.Trees:
                    if (vegetationItemInfo.VegetationType == VegetationType.Tree)
                    {
                        vegetationItemIndexList.Add(i);
                    }
                    break;
                case VegetationItemTypeSelection.Objects:
                    if (vegetationItemInfo.VegetationType == VegetationType.Objects)
                    {
                        vegetationItemIndexList.Add(i);
                    }
                    break;
                case VegetationItemTypeSelection.LargeObjects:
                    if (vegetationItemInfo.VegetationType == VegetationType.LargeObjects)
                    {
                        vegetationItemIndexList.Add(i);
                    }
                    break;
            }
        }

        selectionCount = vegetationItemIndexList.Count;

        VegetationInfoComparer vIc = new VegetationInfoComparer
        {
            VegetationInfoList = vegetationPackage.VegetationInfoList
        };
        vegetationItemIndexList.Sort(vIc.Compare);

        GUIContent[] imageButtons = new GUIContent[vegetationItemIndexList.Count];

        for (int i = 0; i <= vegetationItemIndexList.Count - 1; i++)
        {
            if (vegetationPackage.VegetationInfoList[vegetationItemIndexList[i]].PrefabType == VegetationPrefabType.Mesh)
            {
                imageButtons[i] = new GUIContent
                {
                    image = AssetPreviewCache.GetAssetPreview(vegetationPackage.VegetationInfoList[vegetationItemIndexList[i]].VegetationPrefab)
                };
            }
            else
            {
                imageButtons[i] = new GUIContent
                {
                    image = AssetPreviewCache.GetAssetPreview(vegetationPackage.VegetationInfoList[vegetationItemIndexList[i]].VegetationTexture)
                };
            }

        }
        int imageWidth = imageSize;
        int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
        int rows = Mathf.CeilToInt((float)imageButtons.Length / columns);
        int gridHeight = (rows) * imageWidth;


        if (selectedGridIndex > imageButtons.Length - 1) selectedGridIndex = 0;
        if (imageButtons.Length > 0)
        {

            selectedGridIndex = GUILayout.SelectionGrid(selectedGridIndex, imageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
        }

        vegIndex = vegetationItemIndexList.Count > selectedGridIndex ? vegetationItemIndexList[selectedGridIndex] : 0;
    }

    public static void DrawLODRanges(float lod1Distance, float lod2Distance, float billboardDistance)
    {
        lod1Distance = Mathf.Clamp(lod1Distance, 0, billboardDistance);
        lod2Distance = Mathf.Clamp(lod2Distance, 0, billboardDistance);

        GUILayout.BeginVertical("box");
        Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 100, EditorGUIUtility.currentViewWidth - 100, 30, 30);

        float width = rect.width;

        rect.width = width / 2f;

        Color lod0Color = new Color(60 / 255f, 70 / 255f, 26 / 255f, 1);
        Color lod1Color = new Color(46 / 255f, 55 / 255f, 67 / 255f, 1);
        Color lod2Color = new Color(40 / 255f, 64 / 255f, 73 / 255f, 1);

        float lod0Width = width * (lod1Distance / billboardDistance);
        float lod1Width = width * ((lod2Distance - lod1Distance) / billboardDistance);
        float lod2Width = width - (lod0Width + lod1Width);

        rect.width = lod0Width;
        var lod0GUIContent = lod0Width < 100 ? new GUIContent { text = lod1Distance.ToString("F0") + "m" } : new GUIContent { text = "LOD 0 - " + lod1Distance.ToString("F0") + "m" };

        DrawRect(rect, lod0Color, lod0GUIContent);

        rect.xMin += lod0Width;
        rect.width = lod1Width;
        var lod1GUIContent = lod1Width < 100 ? new GUIContent { text = lod2Distance.ToString("F0") + "m" } : new GUIContent { text = "LOD 1 - " + lod2Distance.ToString("F0") + "m" };

        DrawRect(rect, lod1Color, lod1GUIContent);

        rect.xMin += lod1Width;
        rect.width = lod2Width;
        var lod2GUIContent = lod2Width < 100 ? new GUIContent { text = billboardDistance.ToString("F0") + "m" } : new GUIContent { text = "LOD 2 - " + billboardDistance.ToString("F0") + "m" };
        if (lod2Width > 5)
        {
            DrawRect(rect, lod2Color, lod2GUIContent);
        }

        EditorGUILayout.HelpBox("Active LOD bias is " + QualitySettings.lodBias.ToString("F2") + ". Distances are adjusted accordingly.", MessageType.Warning);
        EditorGUILayout.HelpBox("Max distance before billboard cutoff is set by the general Vegetation + Additional mesh tree distance.", MessageType.Info);

        GUILayout.EndVertical();
    }

    private static readonly Texture2D BackgroundTexture = Texture2D.whiteTexture;
    private static readonly GUIStyle LabelStyle = new GUIStyle("Label")
    {
        normal = new GUIStyleState { background = BackgroundTexture, textColor = Color.white },
        fontStyle = FontStyle.Italic
    };

    public static void DrawRect(Rect position, Color color, GUIContent content = null)
    {
        LabelStyle.alignment = TextAnchor.MiddleCenter;
        var backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUI.Box(position, content ?? GUIContent.none, LabelStyle);
        GUI.backgroundColor = backgroundColor;
    }

    public static void LayoutBox(Color color, GUIContent content = null)
    {
        var backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUILayout.Box(content ?? GUIContent.none, LabelStyle);
        GUI.backgroundColor = backgroundColor;
    }


    public static bool DrawHeader(string title, bool state)
    {
        GUILayoutUtility.GetRect(2, 2, 2, 2);

        var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

        var labelRect = backgroundRect;
        labelRect.xMin += 16f;
        labelRect.xMax -= 20f;

        var foldoutRect = backgroundRect;
        foldoutRect.y += 1f;
        foldoutRect.width = 13f;
        foldoutRect.height = 13f;

        backgroundRect.xMin = 13f;
        //backgroundRect.width += 4f;

        // Background
        float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
        EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

        // Title
        EditorGUI.LabelField(labelRect, GetContent(title), EditorStyles.boldLabel);

        // Active checkbox
        state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

        var e = Event.current;
        if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
        {
            state = !state;
            e.Use();
        }

       

        return state;
    }

    public static GUIContent GetContent(string textAndTooltip)
    {
        if (string.IsNullOrEmpty(textAndTooltip))
            return GUIContent.none;

        var s = textAndTooltip.Split('|');
        var content = new GUIContent(s[0]);

        if (s.Length > 1 && !string.IsNullOrEmpty(s[1]))
            content.tooltip = s[1];

        return content;
    }

   

}
