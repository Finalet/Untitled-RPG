using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    [CustomEditor(typeof(Messages))]
    public class MessagesEd : Editor
    {
        private ReorderableList list;

        private Messages MMessage;
        private SerializedProperty sp_messages;

        private MonoScript script;

        private void OnEnable()
        {
            MMessage = ((Messages)target);
            script = MonoScript.FromMonoBehaviour(MMessage);
            sp_messages = serializedObject.FindProperty("messages");

            list = new ReorderableList(serializedObject, sp_messages, true, true, true, true);

            list.drawElementCallback = drawElementCallback1;
            list.drawHeaderCallback = HeaderCallbackDelegate1;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Send Messages to all the MonoBehaviours that uses the Interface |IAnimatorListener| ");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                    MalbersEditor.DrawScript(script);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    list.DoLayoutList();
                    EditorGUILayout.EndVertical();

                    var UseSendMessage = serializedObject.FindProperty("UseSendMessage");
                    UseSendMessage.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Use Send Message", "Uses the SendMessage() instead"), UseSendMessage.boolValue);
                    var nextFrame = serializedObject.FindProperty("nextFrame");
                    nextFrame.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Next Frame", "Send the Message the frame after"), nextFrame.boolValue);
                }

                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Messages Inspector");
            }

            serializedObject.ApplyModifiedProperties();
        }

        void HeaderCallbackDelegate1(Rect rect)
        {
            Rect R_1 = new Rect(rect.x + 10, rect.y, (rect.width / 3) + 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Message");

            Rect R_3 = new Rect(rect.x + 10 + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Type");

            Rect R_5 = new Rect(rect.x + 10 + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_5, "Value");
        }

        void drawElementCallback1(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MMessage.messages[index];
            var sp_element = sp_messages.GetArrayElementAtIndex(index);

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            element.Active = EditorGUI.Toggle(R_0, element.Active);

            Rect R_1 = new Rect(rect.x+15, rect.y, (rect.width / 3) + 15, EditorGUIUtility.singleLineHeight);
            element.message = EditorGUI.TextField(R_1, element.message);

            
            Rect R_3 = new Rect(rect.x + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            element.typeM = (TypeMessage)EditorGUI.EnumPopup(R_3, element.typeM);

            Rect R_5 = new Rect(rect.x + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            switch (element.typeM)
            {
                case TypeMessage.Bool:
                    element.boolValue = EditorGUI.ToggleLeft(R_5, element.boolValue ? " True" : " False", element.boolValue);
                    break;
                case TypeMessage.Int:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("intValue"), GUIContent.none);
                    break;
                case TypeMessage.Float:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("floatValue"), GUIContent.none);
                    break;
                case TypeMessage.String:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("stringValue"), GUIContent.none);
                    break;
                case TypeMessage.IntVar:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("intVarValue"), GUIContent.none);
                    break;
                case TypeMessage.Transform:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("transformValue"),GUIContent.none);
                    break;
                default:
                    break;
            }
        }
    }
}