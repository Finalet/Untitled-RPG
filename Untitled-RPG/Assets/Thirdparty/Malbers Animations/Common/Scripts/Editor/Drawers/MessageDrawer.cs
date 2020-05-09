using System.Collections;
using UnityEditor;
using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Utilities
{
    [CustomPropertyDrawer(typeof(MesssageItem))]
    public class MessageDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
           // position.y += 2;

            EditorGUI.BeginProperty(position, label, property);
            //GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;

            //PROPERTIES

            var Active = property.FindPropertyRelative("Active");
            var message = property.FindPropertyRelative("message");
            var typeM = property.FindPropertyRelative("typeM");

            var rect = new Rect(position);

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(R_0, Active, GUIContent.none);

            Rect R_1 = new Rect(rect.x + 15, rect.y, (rect.width / 3) + 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(R_1, message, GUIContent.none);


            Rect R_3 = new Rect(rect.x + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(R_3, typeM, GUIContent.none);


            Rect R_5 = new Rect(rect.x + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            var TypeM =(TypeMessage) typeM.intValue;

            SerializedProperty messageValue = property.FindPropertyRelative("boolValue");

            switch (TypeM)
            {
                case TypeMessage.Bool:
                    messageValue = property.FindPropertyRelative("boolValue");
                    messageValue.boolValue = EditorGUI.ToggleLeft(R_5, messageValue.boolValue ? " True" : " False", messageValue.boolValue);
                    break;
                case TypeMessage.Int:
                    messageValue = property.FindPropertyRelative("intValue");
                    break;
                case TypeMessage.Float:
                    messageValue = property.FindPropertyRelative("floatValue");
                    break;
                case TypeMessage.String:
                    messageValue = property.FindPropertyRelative("stringValue");
                    break;
                case TypeMessage.IntVar:
                    messageValue = property.FindPropertyRelative("intVarValue");
                    break;
                case TypeMessage.Transform:
                    messageValue = property.FindPropertyRelative("transformValue");
                    break;
                default:
                    break;
            }

            if (TypeM != TypeMessage.Void && TypeM != TypeMessage.Bool)
            {
                EditorGUI.PropertyField(R_5, messageValue, GUIContent.none);
            }


            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
