using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(MPivots))]
    public class MPivotDrawer : PropertyDrawer
    {

        /// <summary>
        /// Options to display in the popup to select constant or variable.
        /// </summary>
        private readonly string[] popupOptions =
            { "Position", "Direction" , "Pivot Color" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            var height = EditorGUIUtility.singleLineHeight;
            var nameRect = new Rect(position.x, position.y, position.width / 2 - 90, height);
            var vectorRect = new Rect(position.width / 2 - 45, position.y, position.width / 2 , height);
            var multiplierRect = new Rect(position.width - 45, position.y, 50, height);
            var button = new Rect(position.width + 8, position.y, 16, height);


            // Calculate rect for configuration button
            Rect buttonRect = new Rect(vectorRect);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            buttonRect.x -= 20;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var name = property.FindPropertyRelative("name");
            var vectorPos = property.FindPropertyRelative("position");
            var vectorDir = property.FindPropertyRelative("dir");
            var multiplie = property.FindPropertyRelative("multiplier");
            var modifyGizmo = property.FindPropertyRelative("EditorModify");
            var PivotColor = property.FindPropertyRelative("PivotColor");
            var EditorDisplay = property.FindPropertyRelative("EditorDisplay");



            //EditorGUI.HelpBox(buttonRect, "SD", MessageType.None);

            EditorDisplay.intValue = EditorGUI.Popup(buttonRect, EditorDisplay.intValue, popupOptions, popupStyle);

            EditorGUI.PropertyField(nameRect, name, GUIContent.none);

            if (EditorDisplay.intValue == 0)
            {
                EditorGUI.PropertyField(vectorRect, vectorPos, GUIContent.none);
            }
            else if (EditorDisplay.intValue == 1)
            {
                EditorGUI.PropertyField(vectorRect, vectorDir, GUIContent.none);
            }

            else if (EditorDisplay.intValue == 2)
            {
                EditorGUI.PropertyField(vectorRect, PivotColor, GUIContent.none);
            }


            EditorGUIUtility.labelWidth = 15;
            EditorGUI.PropertyField(multiplierRect, multiplie, new GUIContent("M","Multiplier"));


            modifyGizmo.boolValue =  GUI.Toggle(button, modifyGizmo.boolValue, new GUIContent("•","Edit on the Scene"), EditorStyles.miniButton);

            EditorGUIUtility.labelWidth = 0;

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

        }
    }
}