using UnityEngine;
using UnityEditor;

namespace MalbersAnimations
{
    /// <summary>
    /// Required Field Property Drawer from https://twitter.com/Rodrigo_Devora/status/1204031607583264769 Thanks for sharing!
    /// </summary>
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    public class RequiredFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RequiredFieldAttribute rf = attribute as RequiredFieldAttribute;

            if (property.objectReferenceValue == null)
            {
                var oldColor = GUI.color;

                GUI.color = rf.color;
                EditorGUI.PropertyField(position, property, label);
                GUI.color = oldColor;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}