#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER

#else
using System.Reflection;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Terrain))]
public class ExtendedTerrainEditor : DecoratorEditor
{
    //private Texture2D _vegetationLogoTexture;
    private Terrain _terrain;
    private Bounds _combindedBounds;

    public ExtendedTerrainEditor() : base("TerrainInspector")
    {

    }

    public virtual void Awake()
    {
        //if (EditorGUIUtility.isProSkin)
        //{
        //    _vegetationLogoTexture = (Texture2D) Resources.Load("VegetationStudioSplashSmall", typeof(Texture2D));
        //}
        //else
        //{
        //    _vegetationLogoTexture = (Texture2D) Resources.Load("VegetationStudioSplashSmall", typeof(Texture2D));
        //}

        _terrain = (Terrain) target;
    }

    public override void OnInspectorGUI()
    {
       // float logoWidth = _vegetationLogoTexture.width;
       // float logoHeight = _vegetationLogoTexture.height;
       // EditorGUIUtility.labelWidth = 200;
       // Rect space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(_vegetationLogoTexture.height));
       // space.xMin = 10;
       // space.width = logoWidth;
       // space.height = logoHeight;

       //GUI.DrawTexture(space, _vegetationLogoTexture,ScaleMode.ScaleToFit,true,0);


            int buttonIndex = GetActiveButton();
            if (buttonIndex == 3)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox(
                    "Vegetation will be updated automaticaly based on the spawn rules when drawing textures in the terrain.",
                    MessageType.Info);
                //if (_terrainSystem)
                //{
                //    if (_terrainSystem.AutomaticApply)
                //    {
                //        EditorGUILayout.HelpBox(
                //            "Splat map for terrain changes will be done automatic. Layers not selected for splat generation will kept",
                //            MessageType.Info);
                //    }
                //}
                GUILayout.EndVertical();
            }

            //if (buttonIndex == 0 || buttonIndex == 1 || buttonIndex == 2)
            //{
            //    GUILayout.BeginVertical("box");
            //    if (_terrainSystem)
            //    {
            //        if (_terrainSystem.AutomaticApply)
            //        {
            //            EditorGUILayout.HelpBox(
            //                "Splat map for terrain changes will be done automatic. Layers not selected for splat generation will kept",
            //                MessageType.Info);
            //        }
            //    }
            //    GUILayout.EndVertical();
            //}
        
        base.OnInspectorGUI();
    }

    public override void OnSceneGUI()
    {
        Bounds editBounds = new Bounds();
        bool updateVegetation = false;

        _terrain = (Terrain) target;
        int buttonIndex = GetActiveButton();

        Event current = Event.current;
        int controlId = GUIUtility.GetControlID(EditorInstance.GetHashCode(), FocusType.Passive);
        var currentEventType = current.GetTypeForControl(controlId);

        if (buttonIndex <= 3)
        {
            switch (currentEventType)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.MouseDrag:
                    if (current.button == 0)
                    {
                        var size = EditorPrefs.GetInt("TerrainBrushSize");
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        RaycastHit raycastHit;
                        if (_terrain.GetComponent<Collider>().Raycast(ray, out raycastHit, float.PositiveInfinity))
                        {
                            var pos = raycastHit.point;
                            float sizeScale = 5f;
                            editBounds = new Bounds(pos, new Vector3(size * sizeScale, size, size * sizeScale));
                            updateVegetation = true;
                        }
                    }
                    break;
            }

            if (currentEventType == EventType.MouseDown)
            {
                _combindedBounds = new Bounds(editBounds.center, editBounds.size);

            }
            else if (currentEventType == EventType.MouseDrag || currentEventType == EventType.MouseUp)
            {
                _combindedBounds.Encapsulate(editBounds);
            }
        }

        base.OnSceneGUI();

        if (updateVegetation)
        {
            if (buttonIndex < 3)
            {
                if (currentEventType == EventType.MouseUp)
                {
                    _combindedBounds.Expand(4f);
                    VegetationStudioManager.RefreshTerrainHeightMap(_combindedBounds);
                }
            }
            else
            {
              editBounds.Expand(1f);
              VegetationStudioManager.ClearCache(editBounds);               
            }
        }
    }
    int GetActiveButton()
    {
        var property = decoratedEditorType.GetProperty("selectedTool",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        return (int) property.GetValue(EditorInstance, EMPTY_ARRAY);
    }

    float GetFloatProperty(string parameterName)
    {
        var property = decoratedEditorType.GetProperty(parameterName,
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        return (float) property.GetValue(EditorInstance, EMPTY_ARRAY);
    }


    Texture2D[] GetTextureArrayProperty(string parameterName)
    {
        var property = decoratedEditorType.GetField(parameterName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        return (Texture2D[])property.GetValue(EditorInstance);
    }

}
#endif
