using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(MSpeed))]
    public class MSpeedDrawer : PropertyDrawer
    {

        const float labelwith = 27f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += 2;

            EditorGUI.BeginProperty(position, label, property);
            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;
            var vertical = property.FindPropertyRelative("Vertical");

            var positionS = property.FindPropertyRelative("position");
            var lerpPosition = property.FindPropertyRelative("lerpPosition");
            var lerpRotation = property.FindPropertyRelative("lerpRotation");
            var rotation = property.FindPropertyRelative("rotation");
            //var sprint = property.FindPropertyRelative("sprint");
            var animator = property.FindPropertyRelative("animator");
            var lerpAnimator = property.FindPropertyRelative("lerpAnimator");

            var name = property.FindPropertyRelative("name");

            if (name.stringValue == string.Empty) name.stringValue = "NameHere";

            var line = position;
            line.height = height;

            line.x += 4;
            line.width -= 8;

            var foldout = line;
            foldout.width = 10;
            // foldout.x += 10;

            property.isExpanded = EditorGUI.Toggle(foldout, GUIContent.none, property.isExpanded, EditorStyles.foldout);

            var rectName = line;

            rectName.x += 15;
            rectName.width = line.width / 2;

            //var rectSprint = line;
            //rectSprint.x = line.width - 24;
            //rectSprint.width = 50;

            name.stringValue = GUI.TextField(rectName, name.stringValue, EditorStyles.boldLabel);
            // sprint.boolValue = GUI.Toggle(rectSprint, sprint.boolValue, new GUIContent("Sprint") , EditorStyles.miniButton);

            line.y += height + 2;

            if (property.isExpanded)
            {

                EditorGUI.PropertyField(line, vertical, new GUIContent("   Vertical Speed", "Vertical Mutliplier for the Animator"));

                line.y += height + 2;
                float lerpSize = 75;

                var MainRect = new Rect(line.x, line.y, line.width - lerpSize, height);
                var lerpRect = new Rect(line.x + line.width - lerpSize, line.y, lerpSize, height);

                EditorGUI.PropertyField(MainRect, positionS, new GUIContent("   Position", "Additional " + name.stringValue + " Speed added to the position"));
                EditorGUIUtility.labelWidth = labelwith;
                EditorGUI.PropertyField(lerpRect, lerpPosition, new GUIContent("L", "Position " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                EditorGUIUtility.labelWidth = 0;

                line.y += height + 2;
                MainRect.y += height + 2;
                lerpRect.y += height + 2;
                //  MainRect.width += 60;

                EditorGUI.PropertyField(MainRect, rotation, new GUIContent("   Rotation", "Additional " + name.stringValue + " Speed added to the Rotation"));
                EditorGUIUtility.labelWidth = labelwith;
                EditorGUI.PropertyField(lerpRect, lerpRotation, new GUIContent("L", "Rotation " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                EditorGUIUtility.labelWidth = 0;

                // MainRect.width -= 60;
                line.y += height + 2;
                MainRect.y += height + 2;
                lerpRect.y += height + 2;

                EditorGUI.PropertyField(MainRect, animator, new GUIContent("   Animator", "Additional " + name.stringValue + " Speed added to the Animator"));
                EditorGUIUtility.labelWidth = labelwith;
                EditorGUI.PropertyField(lerpRect, lerpAnimator, new GUIContent("L", "Animator " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                EditorGUIUtility.labelWidth = 0;

                position.height = line.height;
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return base.GetPropertyHeight(property, label);

            return 16 * 5 + 20;
        }

    }
}