using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(Ability))]
    public class AbilityDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += 2;
            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);

            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;

            var line = position;
            line.x += 4;
            line.y += 4;
            line.width -= 10;
            line.height = height;
            var lineParameter = line;
            var buttonRect = lineParameter;
            buttonRect.width = 19;
            buttonRect.height += 3;
            buttonRect.y -=3;
            buttonRect.x -= 2;
            lineParameter.y -= 2;
            lineParameter.x += 16;
            lineParameter.width -= 10;


            var width = lineParameter.width / 2;
            lineParameter.width  = width + 10;

            var name = property.FindPropertyRelative("Name");
            var OverrideProp = property.FindPropertyRelative("OverrideProp");
            var ID = property.FindPropertyRelative("Index");
            var properties = property.FindPropertyRelative("OverrideProperties");
          //  var a = property.FindPropertyRelative("active");
 
            if (name.stringValue == string.Empty) name.stringValue = "NameHere";

            OverrideProp.boolValue =  GUI.Toggle(buttonRect, OverrideProp.boolValue, new GUIContent ("","Override Global Properties"), EditorStyles.radioButton);
        
            var styll = new GUIStyle(EditorStyles.label);
            styll.fontStyle = FontStyle.Normal;

            name.stringValue = GUI.TextField(lineParameter, name.stringValue, styll);


            lineParameter.width = width;
            lineParameter.x += lineParameter.width + 16;
            lineParameter.width -= 18;
            EditorGUIUtility.labelWidth = 56;
            EditorGUI.PropertyField(lineParameter, ID);
            EditorGUIUtility.labelWidth = 0;

            line.y += height + 2; 

             if (OverrideProp.boolValue)
            {
                EditorGUI.PropertyField(line, properties);
            }



            property.serializedObject.ApplyModifiedProperties();

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var OverrideProp = property.FindPropertyRelative("OverrideProp");
            var properties = property.FindPropertyRelative("OverrideProperties");

            if (OverrideProp.boolValue)
            {
                return base.GetPropertyHeight(property, label) + 4 + EditorGUI.GetPropertyHeight(properties);
            }

            return base.GetPropertyHeight(property, label) + 4;
        }
    }
}