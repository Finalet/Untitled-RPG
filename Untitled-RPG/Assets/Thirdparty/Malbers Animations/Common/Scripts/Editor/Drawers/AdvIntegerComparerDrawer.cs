using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(AdvancedIntegerEvent))]
    public class AdvIntegerComparerDrawer : PropertyDrawer
    {
        const float labelwith = 27f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += 2;

            EditorGUI.BeginProperty(position, label, property);
          //  GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;
            var name = property.FindPropertyRelative("name");
            var comparer = property.FindPropertyRelative("comparer");
            var Value = property.FindPropertyRelative("Value");
            var Response = property.FindPropertyRelative("Response");

            bool isExpanded = property.isExpanded;


            if (name.stringValue == string.Empty) name.stringValue = "NameHere";

            var line = position;
            line.height = height;

            line.x += 4;
            line.width -= 8;

            var foldout = line;
            foldout.width = 10;
            foldout.x += 10;

            EditorGUIUtility.labelWidth = 16;
            property.isExpanded = EditorGUI.Foldout(foldout, property.isExpanded, GUIContent.none);
            EditorGUIUtility.labelWidth = 0;

            var rectName = line;

            rectName.x += 10;
            rectName.width -= 10;

            name.stringValue = GUI.TextField(rectName, name.stringValue, EditorStyles.boldLabel);

            line.y += height + 2;

            if (property.isExpanded)
            {

                var ComparerRect = new Rect(line.x, line.y, line.width / 2 - 10, height);
                var ValueRect = new Rect(line.x + line.width / 2 + 15, line.y, line.width / 2 - 10, height);

                EditorGUI.PropertyField(ComparerRect, comparer, GUIContent.none);
                EditorGUI.PropertyField(ValueRect, Value, GUIContent.none);
                line.y += height + 2;
                EditorGUI.PropertyField(line, Response);

                //EditorGUI.PropertyField(MainRect, positionS, new GUIContent("   Position", "Additional " + name.stringValue + " Speed added to the position"));
                //EditorGUIUtility.labelWidth = labelwith;
                //EditorGUI.PropertyField(lerpRect, lerpPosition, new GUIContent("L", "Position " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                //EditorGUIUtility.labelWidth = 0;

                //line.y += height + 2;
                //MainRect.y += height + 2;
                //lerpRect.y += height + 2;
                //MainRect.width += 60;

                //EditorGUI.PropertyField(MainRect, rotation, new GUIContent("   Rotation", "Additional " + name.stringValue + " Speed added to the Rotation"));
                ////EditorGUIUtility.labelWidth = labelwith;
                ////EditorGUI.PropertyField(lerpRect, lerpRotation, new GUIContent("L", "Rotation " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                ////EditorGUIUtility.labelWidth = 0;
                //MainRect.width -= 60;
                //line.y += height + 2;
                //MainRect.y += height + 2;
                //lerpRect.y += height + 2;

                //EditorGUI.PropertyField(MainRect, animator, new GUIContent("   Animator", "Additional " + name.stringValue + " Speed added to the Animator"));
                //EditorGUIUtility.labelWidth = labelwith;
                //EditorGUI.PropertyField(lerpRect, lerpAnimator, new GUIContent("L", "Animator " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                //EditorGUIUtility.labelWidth = 0;

                position.height = line.height;
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return base.GetPropertyHeight(property, label);

            var Response = property.FindPropertyRelative("Response");
            float ResponseHeight = EditorGUI.GetPropertyHeight(Response);

            return 16 * 2 + ResponseHeight +10;
        }

    }
}