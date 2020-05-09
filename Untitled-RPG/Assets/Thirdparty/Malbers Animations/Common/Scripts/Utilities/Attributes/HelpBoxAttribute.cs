using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HelpBoxAttribute : PropertyAttribute
{

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HelpBoxAttribute))]
public class HelpBoxDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return Mathf.Max(EditorGUIUtility.singleLineHeight * 2,
            EditorStyles.helpBox.CalcHeight(new GUIContent(property.stringValue), Screen.width) +
            EditorGUIUtility.singleLineHeight);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.HelpBox(position, property.stringValue, MessageType.None);
    }
}
#endif