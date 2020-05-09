using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using MalbersAnimations.Animals;

namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(JumpProfile))]
    public class JumpProfileDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var name = property.FindPropertyRelative("name");
            var VerticalSpeed = property.FindPropertyRelative("VerticalSpeed");
           // var fallRay = property.FindPropertyRelative("fallRay");
          //  var stepHeight = property.FindPropertyRelative("stepHeight");
            var JumpLandDistance = property.FindPropertyRelative("JumpLandDistance");
            var fallingTime = property.FindPropertyRelative("fallingTime");
            var CliffTime = property.FindPropertyRelative("CliffTime");
            var CliffLandDistance = property.FindPropertyRelative("CliffLandDistance");
            var HeightMultiplier = property.FindPropertyRelative("HeightMultiplier");
            var ForwardMultiplier = property.FindPropertyRelative("ForwardMultiplier");

         


            //  helpBox.height = helpBox.height * 3;
            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);


            position.y += 2;

            EditorGUI.BeginProperty(position, label, property);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var height = EditorGUIUtility.singleLineHeight;
          

            var line = position;
            line.x += 4;
            line.width -= 8;
            line.height = height;
            var lineParameter = line;
            var foldout = lineParameter;
            foldout.width = 10;
            foldout.x += 10;
            lineParameter.y -= 2;
            lineParameter.x += 13;
            lineParameter.width -= 10;
            lineParameter.height += 5;
            //GUI.Label(line, label, EditorStyles.boldLabel);

            //line.y += height + 2;
            EditorGUIUtility.labelWidth = 16;
            property.isExpanded = EditorGUI.Foldout(foldout, property.isExpanded, GUIContent.none);
            EditorGUIUtility.labelWidth = 0;
            if (name.stringValue == string.Empty) name.stringValue = "NameHere";
            var styll = new GUIStyle(EditorStyles.largeLabel);
            styll.fontStyle = FontStyle.Bold;

            name.stringValue = GUI.TextField(lineParameter, name.stringValue, styll);
            if (property.isExpanded)
            {



                line.y += height + 8;
                float Division = line.width / 2;
                var lineSplitted = line;

                lineSplitted.width = Division + 20;


                EditorGUI.PropertyField(lineSplitted, VerticalSpeed, new GUIContent("Vertical Speed", "Root Motion:\nEnable/Disable the Root Motion on the Animator"));

                lineSplitted.x += Division + 42;
                lineSplitted.width -= 62;

                EditorGUIUtility.labelWidth = 65;
                EditorGUI.PropertyField(lineSplitted, JumpLandDistance, new GUIContent("Jump Ray", "Ray Length to check if the ground is at the same level of the beginning of the jump and it allows to complete the Jump End Animation"));
                EditorGUIUtility.labelWidth = 0;


                /////NEW LINE
                //line.y += height + 2;

                //lineSplitted = line;

                //lineSplitted.width = Division + 30;

                //EditorGUI.PropertyField(lineSplitted, JumpLandDistance, new GUIContent("Jump Min Distance", "Minimun Distance to Complete the Jump Exit when the Jump is on the Highest Point"));

                //lineSplitted.x += Division + 35;
                //lineSplitted.width -= 65;

                //EditorGUIUtility.labelWidth = 55;
                //EditorGUI.PropertyField(lineSplitted, stepHeight, new GUIContent("     Step", "Step Height:\nTerrain minimum difference to be sure the animal will fall"));
                //EditorGUIUtility.labelWidth = 0;

                ///NEW LINE
                line.y += height + 2;
                lineSplitted = line;

                EditorGUI.PropertyField(lineSplitted, fallingTime, new GUIContent("Fall Time", "Animation normalized time to change to fall animation if the ray checks if the animal is falling"));

                ///NEW LINE
                line.y += height + 8;

                EditorGUI.PropertyField(line, CliffTime);

                line.y += height + 2;
                EditorGUI.PropertyField(line, CliffLandDistance);

                line.y += height + 8;
                EditorGUI.LabelField(line, "Jump Multipliers", EditorStyles.boldLabel);
                line.y += height + 2;
                lineSplitted = line;

                lineSplitted.width = Division + 30;

                EditorGUI.PropertyField(lineSplitted, HeightMultiplier, new GUIContent("Height", "Height multiplier for the Jump. Default:1"));


                lineSplitted.x += Division + 35;
                lineSplitted.width -= 65;

                EditorGUIUtility.labelWidth = 55;
                EditorGUI.PropertyField(lineSplitted, ForwardMultiplier, new GUIContent("Forward", "Forward multiplier for the Jump. Default:1"));
                EditorGUIUtility.labelWidth = 0;
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return base.GetPropertyHeight(property, label)+5;
         
            float lines = 8;
            return base.GetPropertyHeight(property, label) * lines + (2 * lines) + 5;
        }

    }
}