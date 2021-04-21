using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;

/// <summary>
/// FM: Editor class component to be inherited, class is containing handly methods for handles in scene view
/// </summary>
public static class FEditor_TransformHandles
{
    /// <summary>
    /// [To be executed in OnSceneGUI()]
    /// Drawing sphere handle in scene view with controll ability
    /// </summary>
    public static Vector3 DrawAndSetPositionForHandle(Vector3 position, Transform rootReference)
    {
        EditorGUI.BeginChangeCheck();

        Handles.color = Color.green;
        Quaternion rotation = (Tools.pivotRotation != PivotRotation.Local) ? Quaternion.identity : rootReference.rotation;

        float size = HandleUtility.GetHandleSize(position) * 0.125f;
        Handles.SphereHandleCap(0, position, rotation, size, EventType.Repaint);
        Vector3 pos = Handles.PositionHandle(position, rotation);

        return pos;
    }

    /// <summary>
    /// [To be executed in OnSceneGUI()]
    /// Drawing sphere handle in scene view without option to controll it but clickable
    /// Returns true if mouse clicked on handle
    /// </summary>
    public static bool DrawSphereHandle(Vector3 position, string text = "")
    {
        bool clicked = false;

        if (Event.current.button != 1)
        {
            Handles.color = Color.white;

            float size = HandleUtility.GetHandleSize(position) * 0.2f;

            if (Handles.Button(position, Quaternion.identity, size, size, Handles.SphereHandleCap))
            {
                clicked = true;
                InternalEditorUtility.RepaintAllViews();
            }

            Handles.BeginGUI();

            Vector2 labelSize = new Vector2(EditorGUIUtility.singleLineHeight * 2, EditorGUIUtility.singleLineHeight);
            Vector2 labelPos = HandleUtility.WorldToGUIPoint(position);

            labelPos.y -= labelSize.y / 2;
            labelPos.x -= labelSize.x / 2;

            GUILayout.BeginArea(new Rect(labelPos, labelSize));
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(new GUIContent(text), style);
            GUILayout.EndArea();

            Handles.EndGUI();

        }

        return clicked;
    }



    public static Quaternion RotationHandle(Quaternion rotation, Vector3 position, float size = 1f, bool worldScale = false)
    {
        float handleSize = size;
        if (worldScale) handleSize = HandleUtility.GetHandleSize(position) * size;

        Color color = Handles.color;
        Handles.color = Handles.xAxisColor;
        rotation = Handles.Disc(rotation, position, rotation * Vector3.right, handleSize, true, 1f);
        Handles.color = Handles.yAxisColor;
        rotation = Handles.Disc(rotation, position, rotation * Vector3.up, handleSize, true, 1f);
        Handles.color = Handles.zAxisColor;
        rotation = Handles.Disc(rotation, position, rotation * Vector3.forward, handleSize, true, 1f);
        Handles.color = Handles.centerColor;
        rotation = Handles.Disc(rotation, position, Camera.current.transform.forward, handleSize * 1.1f, false, 0f);
        rotation = Handles.FreeRotateHandle(rotation, position, handleSize);
        Handles.color = color;
        return rotation;
    }

    public static Vector3 ScaleHandle(Vector3 scale, Vector3 position, Quaternion rotation, float size, bool scaleAll = false, bool worldScale = false)
    {
        float handleSize = size;
        if (worldScale) handleSize = HandleUtility.GetHandleSize(position) * size;

        if (!scaleAll)
        {
            Handles.color = Handles.xAxisColor;
            scale.x = Handles.ScaleSlider(scale.x, position, rotation * Vector3.right, rotation, handleSize, 0.001f);
            Handles.color = Handles.yAxisColor;
            scale.y = Handles.ScaleSlider(scale.y, position, rotation * Vector3.up, rotation, handleSize, 0.001f);
            Handles.color = Handles.zAxisColor;
            scale.z = Handles.ScaleSlider(scale.z, position, rotation * Vector3.forward, rotation, handleSize, 0.001f);
        }

        Handles.color = Handles.centerColor;
        EditorGUI.BeginChangeCheck();
        float num1 = Handles.ScaleValueHandle(scale.x, position, rotation, handleSize, Handles.CubeHandleCap, 0.001f);

        if (EditorGUI.EndChangeCheck())
        {
            float num2 = num1 / scale.x;
            scale.x = num1;
            scale.y *= num2;
            scale.z *= num2;
        }

        return scale;
    }

    public static Vector3 PositionHandle(Vector3 position, Quaternion rotation, float size = 1f, bool worldScale = false, bool freeHandle = true)
    {
        float handleSize = size;
        if (worldScale) handleSize = HandleUtility.GetHandleSize(position) * size;

        Color color = Handles.color;
        Handles.color = Handles.xAxisColor;
        position = Handles.Slider(position, rotation * Vector3.right, handleSize, Handles.ArrowHandleCap, 0.001f);
        Handles.color = Handles.yAxisColor;
        position = Handles.Slider(position, rotation * Vector3.up, handleSize, Handles.ArrowHandleCap, 0.001f);
        Handles.color = Handles.zAxisColor;
        position = Handles.Slider(position, rotation * Vector3.forward, handleSize, Handles.ArrowHandleCap, 0.001f);

        if (freeHandle)
        {
            Handles.color = Handles.centerColor;
            position = Handles.FreeMoveHandle(position, rotation, handleSize * 0.15f, Vector3.one * 0.001f, Handles.RectangleHandleCap);
        }

        Handles.color = color;

        return position;
    }

}

#endif
