using UnityEditor;
using UnityEngine;
using MalbersAnimations.HAP;

namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(SpeedTimeMultiplier))]
    public class SpeedTimeMultiplierDrawer : PropertyDrawer
    { 
        // Use this for initialization
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        { 

            label = EditorGUI.BeginProperty(position, label, property);
           // position = EditorGUI.PrefixLabel(position, label);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();

            var name = property.FindPropertyRelative("name");
            var AnimSpeed = property.FindPropertyRelative("AnimSpeed");
            var height = EditorGUIUtility.singleLineHeight;
            var line = position;
            line.height = height;

            //line.x += 4;
            //line.width -= 8;


            var MainRect = new Rect(line.x, line.y, line.width/2, height);
            var lerpRect = new Rect(line.x + line.width/2, line.y, line.width / 2, height);

            EditorGUIUtility.labelWidth = 45f;
            EditorGUI.PropertyField(MainRect, name, new GUIContent("Name", "Name of the Speed to modify for the Rider"));
            EditorGUIUtility.labelWidth = 75f;
            EditorGUI.PropertyField(lerpRect, AnimSpeed, new GUIContent(" Speed Mult", "Anim Speed Multiplier"));
            if (name.stringValue == string.Empty) name.stringValue = "SpeedName";
            EditorGUIUtility.labelWidth = 0;

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}