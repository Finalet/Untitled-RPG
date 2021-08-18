using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(MSpeed))]
    public class MSpeedDrawer : PropertyDrawer
    {

        const float labelwith = 30f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += 2;

            EditorGUI.BeginProperty(position, label, property);
            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;
            var vertical = property.FindPropertyRelative("Vertical");
            var strafe = property.FindPropertyRelative("strafeSpeed");
            var strafeLerp = property.FindPropertyRelative("lerpStrafe");

            var positionS = property.FindPropertyRelative("position");
            var lerpPosition = property.FindPropertyRelative("lerpPosition");
            var lerpPosAnim = property.FindPropertyRelative("lerpPosAnim");


            var rotation = property.FindPropertyRelative("rotation");
            var lerpRotation = property.FindPropertyRelative("lerpRotation");
            var lerpRotAnim = property.FindPropertyRelative("lerpRotAnim");


            var animator = property.FindPropertyRelative("animator");
            var lerpAnimator = property.FindPropertyRelative("lerpAnimator");

            var name = property.FindPropertyRelative("name");

            if (string.IsNullOrEmpty(name.stringValue)) name.stringValue = "NameHere";

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
                float lerpSize = 0.5f * (line.width/3)-2;

                var MainRect = new Rect(line.x, line.y, line.width - (lerpSize*2) - 5, height);
                var lerpRect = new Rect(line.x + line.width - lerpSize+2, line.y,  lerpSize, height);
                var AnimRect = new Rect(line.x + line.width - (lerpSize * 2)+2, line.y,  lerpSize, height);

                EditorGUI.PropertyField(MainRect, positionS, new GUIContent("   Position", "Additional " + name.stringValue + " Speed added to the position"));
               
                
                EditorGUIUtility.labelWidth = labelwith;
                EditorGUI.PropertyField(AnimRect, lerpPosAnim, new GUIContent("A", "Position " + name.stringValue + " Lerp interpolation for the ANIMATOR, higher value more Responsiveness"));
                EditorGUI.PropertyField(lerpRect, lerpPosition, new GUIContent(" L", "Position " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                EditorGUIUtility.labelWidth = 0;

                line.y += height + 2;
                MainRect.y += height + 2;
                lerpRect.y += height + 2;
                AnimRect.y += height + 2;

                EditorGUI.PropertyField(MainRect, rotation, new GUIContent("   Rotation", "Additional " + name.stringValue + " Speed added to the Rotation"));
                EditorGUIUtility.labelWidth = labelwith;
                EditorGUI.PropertyField(AnimRect, lerpRotAnim, new GUIContent("A", "Rotation " + name.stringValue + " Lerp interpolation for the ANIMATOR, higher value more Responsiveness"));
                EditorGUI.PropertyField(lerpRect, lerpRotation, new GUIContent(" L", "Rotation " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                EditorGUIUtility.labelWidth = 0;

                line.y += height + 2;
              

                MainRect = new Rect(line.x, line.y, line.width - (lerpSize), height);
                lerpRect = new Rect(line.x + line.width - lerpSize+2, line.y, lerpSize, height);


                EditorGUI.PropertyField(MainRect, animator, new GUIContent("   Animator", "Additional " + name.stringValue + " Speed added to the Animator"));
                EditorGUIUtility.labelWidth = labelwith;
                EditorGUI.PropertyField(lerpRect, lerpAnimator, new GUIContent(" L", "Animator " + name.stringValue + " Lerp interpolation, higher value more Responsiveness"));
                EditorGUIUtility.labelWidth = 0;

                line.y += height + 2;


                MainRect = new Rect(line.x, line.y, line.width - (lerpSize), height);
                lerpRect = new Rect(line.x + line.width - lerpSize+2, line.y, lerpSize, height);

                EditorGUI.PropertyField(MainRect, strafe, new GUIContent("   Strafe Speed", "Strafe Movement Position"));
                EditorGUIUtility.labelWidth = labelwith;
                EditorGUI.PropertyField(lerpRect, strafeLerp, new GUIContent(" L", "Strafe Movement Lerp Interpolation"));
                EditorGUIUtility.labelWidth = 0;
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return base.GetPropertyHeight(property, label);

            return 16 * 6 + 25;
        }

    }
}