using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(SkyboxDefinitionScriptableObject))]
public class SkyboxDefinitionEditor : Editor
{

    bool timesOfDaySettings = true;
    bool timeSettings = true;
    bool sunSettings = true;
    bool cloudSettings = true;
    bool cloudTextureSettings = true;
    bool cloudDensitySettings = true;
    bool cloudDistributionSettings = true;
    bool cloudInfluenceSettings = true;
    bool starSettings = true;
    bool ditherSettings = true;
    bool fogSettings = true;

    SkyboxDefinitionScriptableObject defObj;
    
    

    GUIStyle headerStyle;
    
    ReorderableList reorderableList;

    // Serialized Properties
    SerializedProperty periodsOfDay;

    
    private void OnEnable()
    {
        SetupReorderableList();
    }
    
    

    public override void OnInspectorGUI()
    {
        //Call Update() before opening
        serializedObject.Update();

        
        InspectorMgr();


        //Call ApplyModifiedProperties before closing
        serializedObject.ApplyModifiedProperties();
        TimeOfDayManager mgr = FindObjectOfType<TimeOfDayManager>();
        if(mgr != null)
        {
            mgr.Update();
        }
        else
        {
            Debug.Log("Note: No GameObject has the TimeOfDayManager script attached to it in this scene. Attach the TimeOfDayManager script to an object so that changes in this Scriptable Object are reflected in your Scene in real time.");
        }
        
    }
    



    void SetupReorderableList()
    {
        defObj = (SkyboxDefinitionScriptableObject)target;
        periodsOfDay = serializedObject.FindProperty("periodsOfDay");

        reorderableList = new ReorderableList(serializedObject, periodsOfDay, true, true, true, true);
        reorderableList.drawElementCallback = DrawListItems;
        reorderableList.drawHeaderCallback = DrawHeader;
        // Cheers to this blog post for help on setting up reorderable lists: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
    }
    

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        float inspectorWidth = EditorGUIUtility.currentViewWidth;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 0;
        float width = 70;
        SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

        if (inspectorWidth < 550)
        {
            float maxWidth = inspectorWidth / 5f;
            float spacer = inspectorWidth / 100f;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, maxWidth, lineHeight), element.FindPropertyRelative("description"), GUIContent.none);
            //EditorGUI.PropertyField(new Rect(rect.x + maxWidth + spacer, rect.y, maxWidth, lineHeight), element.FindPropertyRelative("startTime"), GUIContent.none);
            FloatFieldProperty(new Rect(rect.x + maxWidth + spacer, rect.y, maxWidth, lineHeight), element.FindPropertyRelative("startTime"));
            ColorFieldProperty(new Rect(rect.x + maxWidth * 2f + spacer * 2f, rect.y, maxWidth, lineHeight), element.FindPropertyRelative("horizonColor"));
            ColorFieldProperty(new Rect(rect.x + maxWidth * 3f + spacer * 3f, rect.y, maxWidth, lineHeight), element.FindPropertyRelative("zenithColor"));
        }
        else
        {
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("description"), GUIContent.none);

            spacing += width + 10;
            width = 30;
            EditorGUI.LabelField(new Rect(rect.x + spacing, rect.y, width, EditorGUIUtility.singleLineHeight), "Time");
            spacing += width + 5;
            width = 150;
            EditorGUI.Slider(new Rect(rect.x + spacing, rect.y, width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("startTime"), 0f, 24f, GUIContent.none);

            spacing += width + 15;
            width = 50;
            EditorGUI.LabelField(new Rect(rect.x + spacing, rect.y, width, EditorGUIUtility.singleLineHeight), "Horizon");
            spacing += width;
            width = 50;
            ColorFieldProperty(new Rect(rect.x + spacing, rect.y, width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("horizonColor"));

            spacing += width + 15;
            width = 40;
            EditorGUI.LabelField(new Rect(rect.x + spacing, rect.y, width, EditorGUIUtility.singleLineHeight), "Zenith");
            spacing += width;
            width = 50;
            ColorFieldProperty(new Rect(rect.x + spacing, rect.y, width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("zenithColor"));
        }
    }

    void DrawHeader(Rect rect)
    {
        string name = "Periods of Day";
        EditorGUI.LabelField(rect, name);
    }

    void ColorFieldProperty(Rect position, SerializedProperty property)
    {
        EditorGUI.BeginProperty(position, GUIContent.none, property);
        EditorGUI.BeginChangeCheck();
        var newValue = EditorGUI.ColorField(position, GUIContent.none, property.colorValue, false, false, true);
        if (EditorGUI.EndChangeCheck())
        {
            property.colorValue = newValue;
        }
        EditorGUI.EndProperty();
    }

    void FloatFieldProperty(Rect position, SerializedProperty property)
    {
        EditorGUI.BeginProperty(position, GUIContent.none, property);
        EditorGUI.BeginChangeCheck();
        var newValue = EditorGUI.FloatField(position, GUIContent.none, property.floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            property.floatValue = newValue;
        }
        EditorGUI.EndProperty();
    }

    void InspectorMgr()
    {
        float spacerBetweenGroups = 5f;

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 13;

        if (defObj.periodsOfDay.Count == 0)
        {
            defObj.periodsOfDay.Add(new PeriodOfDay("Default", 0f, Color.black, Color.black));
        }

        EditorGUILayout.LabelField("Customize the Skybox, Lighting, and Clouds", headerStyle);
        GUILayout.Space(spacerBetweenGroups);
        if (defObj.periodsOfDay.Count > 0)
        {
            //string label = "Current time of day: " + defObj.periodsOfDay[mgr.periodIndex].description;
            //EditorGUILayout.LabelField(label);
        }


        // Periods of day

        string periodsOfDayString = "Sky Settings (" + defObj.periodsOfDay.Count + ")";
        GUIContent copy = new GUIContent(periodsOfDayString, "Define different times of day including Description, Start Time, Horizon Color, and Zenith Color");
        timesOfDaySettings = EditorGUILayout.BeginFoldoutHeaderGroup(timesOfDaySettings, copy);
        if (timesOfDaySettings)
        {
            reorderableList.DoLayoutList();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.Space(spacerBetweenGroups);


        //Time Settings
        copy = new GUIContent("Time Settings", "Control the current Time of Day and the day length timescale");
        timeSettings = EditorGUILayout.BeginFoldoutHeaderGroup(timeSettings, copy);
        if (timeSettings)
        {
            defObj.timeOfDay = EditorGUILayout.Slider("Time of Day", defObj.timeOfDay, 0f, 24f);
            defObj.realSecondsToGameHours = EditorGUILayout.Slider("Real Seconds to Game Hours", defObj.realSecondsToGameHours, 0f, 3f);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.Space(spacerBetweenGroups);


        copy = new GUIContent("Sun Settings", "Set the sun rotation and intensity");
        sunSettings = EditorGUILayout.BeginFoldoutHeaderGroup(sunSettings, copy);

        if (sunSettings)
        {
            defObj.sunBaseRotationY = EditorGUILayout.Slider("Sun Rotation (use to set where the sun rises from)", defObj.sunBaseRotationY, 0, 360f);
            defObj.lightIntensity = EditorGUILayout.Slider("Sun Lamp Intensity During the Day", defObj.lightIntensity, 0, 10f);
            defObj.sunSize = EditorGUILayout.Slider("Sun Size", defObj.sunSize, 0f, 0.4f);
            defObj.sunColor = EditorGUILayout.ColorField(new GUIContent("Sun Color"), defObj.sunColor, false, false, true);
            defObj.sunInfluenceSize = EditorGUILayout.Slider("Sun Influence Size", defObj.sunInfluenceSize, 0f, 2f);
            defObj.sunInfluenceIntensity = EditorGUILayout.Slider("Sun Influence Intensity", defObj.sunInfluenceIntensity, 0f, 0.01f);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.Space(spacerBetweenGroups);


        copy = new GUIContent("Cloud Settings");
        cloudSettings = EditorGUILayout.BeginFoldoutHeaderGroup(cloudSettings, "Cloud Settings");
        if (cloudSettings)
        {
            cloudTextureSettings = EditorGUILayout.Foldout(cloudTextureSettings, "Textures");
            if (cloudTextureSettings)
            {
                defObj.cloudTexture1 = (Texture2D)EditorGUILayout.ObjectField("Texture 1", defObj.cloudTexture1, typeof(Texture2D), false);
                defObj.texture1ZenithTiling = EditorGUILayout.Vector2Field("Texture 1 Zenith Tiling", defObj.texture1ZenithTiling);
                defObj.texture1HorizonTiling = EditorGUILayout.Vector2Field("Texture 1 Horizon Tiling", defObj.texture1HorizonTiling);

                defObj.cloudTexture2 = (Texture2D)EditorGUILayout.ObjectField("Texture 2", defObj.cloudTexture2, typeof(Texture2D), false);
                defObj.texture2ZenithTiling = EditorGUILayout.Vector2Field("Texture 2 Zenith Tiling", defObj.texture2ZenithTiling);
                defObj.texture2HorizonTiling = EditorGUILayout.Vector2Field("Texture 2 Horizon Tiling", defObj.texture2HorizonTiling);
            }

            GUILayout.Space(spacerBetweenGroups);
            cloudDensitySettings = EditorGUILayout.Foldout(cloudDensitySettings, "Cloud Density, Speed, Shape and Color Parameters");
            if (cloudDensitySettings)
            {
                EditorGUILayout.MinMaxSlider("Cloud Density Range", ref defObj.cloudThresholdRange.x, ref defObj.cloudThresholdRange.y, 0, 1);
                defObj.cloudSpeed = EditorGUILayout.Slider("Cloud Speed", defObj.cloudSpeed, 0f, 10f);
                defObj.cloudSharpness = EditorGUILayout.Slider("Cloud Sharpness", defObj.cloudSharpness, 0f, 1f);
                defObj.cloudColor = EditorGUILayout.ColorField(new GUIContent("Cloud Color"), defObj.cloudColor, false, false, true);
                defObj.cloudOpacity = EditorGUILayout.Slider("Opacity", defObj.cloudOpacity, 0f, 1f);
                defObj.cloudShadingColor = EditorGUILayout.ColorField(new GUIContent("Shading Color"), defObj.cloudShadingColor, false, false, true);
                defObj.cloudShadingThreshold = EditorGUILayout.Slider("Shading Threshold", defObj.cloudShadingThreshold, 0f, 0.3f);
                defObj.cloudShadingSharpness = EditorGUILayout.Slider("Shading Sharpness", defObj.cloudShadingSharpness, 0f, 1f);
                defObj.cloudShadingStrength = EditorGUILayout.Slider("Shading Strength", defObj.cloudShadingStrength, 0f, 1f);
            }
            GUILayout.Space(spacerBetweenGroups);

            cloudDistributionSettings = EditorGUILayout.Foldout(cloudDistributionSettings, "Cloud Distribution Parameters");
            if (cloudDistributionSettings)
            {
                defObj.alternateUVAtZenith = EditorGUILayout.Slider("Alternate UVs at Zenith", defObj.alternateUVAtZenith, 0f, 1f);
            }
            GUILayout.Space(spacerBetweenGroups);

            cloudInfluenceSettings = EditorGUILayout.Foldout(cloudInfluenceSettings, "Sun and Sky Color Influence Parameters");
            if (cloudInfluenceSettings)
            {
                defObj.sunCloudInfluence = EditorGUILayout.Slider("Sun Influence Area", defObj.sunCloudInfluence, 0f, 0.1f);
                defObj.skyColorCloudInfluence = EditorGUILayout.Slider("Sky Color Influence", defObj.skyColorCloudInfluence, 0f, 1f);
            }

        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.Space(spacerBetweenGroups);

        starSettings = EditorGUILayout.BeginFoldoutHeaderGroup(starSettings, new GUIContent("Star Settings"));
        if (starSettings)
        {
            defObj.starColor = EditorGUILayout.ColorField(new GUIContent("Star Color"), defObj.starColor, false, false, true);
            defObj.starPowerMultiplier = Mathf.Max(EditorGUILayout.FloatField("Star Tightness", defObj.starPowerMultiplier), 1f);
            defObj.starDensity = EditorGUILayout.Slider("Star Density", defObj.starDensity, 0f, 200f);
            defObj.starMaskDensity = EditorGUILayout.Slider("Star Mask Density", defObj.starMaskDensity, 0f, 200f);
            defObj.starFlickerMaskDensity = EditorGUILayout.Slider("Star Flicker Mask Density", defObj.starFlickerMaskDensity, 0f, 200f);
            defObj.starFlickeringFrequency1 = EditorGUILayout.Slider("Star Flickering Frequency 1 (Flickers Per Second)", defObj.starFlickeringFrequency1, 2f, 20f);
            defObj.starFlickeringFrequency2 = EditorGUILayout.Slider("Star Flickering Frequency 2 (Flickers Per Second", defObj.starFlickeringFrequency2, 2f, 20f);
            EditorGUILayout.MinMaxSlider("Star Brightness Range 1", ref defObj.starBrightnessMinMax1.x, ref defObj.starBrightnessMinMax1.y, 0f, 2f);
            EditorGUILayout.MinMaxSlider("Star Brightness Range 2", ref defObj.starBrightnessMinMax2.x, ref defObj.starBrightnessMinMax2.y, 0f, 2f);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.Space(spacerBetweenGroups);


        copy = new GUIContent("Dither Settings", "Customize Dither Strength");
        ditherSettings = EditorGUILayout.BeginFoldoutHeaderGroup(ditherSettings, copy);
        if (ditherSettings)
        {
            defObj.ditherStrength = EditorGUILayout.Slider("Dither Strength", defObj.ditherStrength, 0f, 1f);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.Space(spacerBetweenGroups);


        copy = new GUIContent("Fog Settings", "");
        fogSettings = EditorGUILayout.BeginFoldoutHeaderGroup(fogSettings, copy);
        if (fogSettings)
        {
            defObj.useHorizonColorForFog = EditorGUILayout.Toggle("Use Horizon Color for Fog", defObj.useHorizonColorForFog);
            defObj.fogColorBlend = EditorGUILayout.Slider("Horizon Color Fog Intensity", defObj.fogColorBlend, 0f, 1f);
            defObj.baseFogColor = EditorGUILayout.ColorField(new GUIContent("Base Fog Color"), defObj.baseFogColor, false, false, true);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
