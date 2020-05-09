using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    [CustomEditor(typeof(MessagesBehavior))]
    public class MessageBehaviorsEd : Editor
    {
        private ReorderableList
            listOnEnter,
            listOnExit, 
            listOnTime;


        bool OnEnter, OnExit, OnTime;

        private MessagesBehavior MMessage;
        private SerializedProperty sp_messagesONEXIT, onEnterMessage, onTimeMessage, UseSendMessage;
        private MonoScript script;

        private void OnEnable()
        {

            MMessage = ((MessagesBehavior)target);
            sp_messagesONEXIT = serializedObject.FindProperty("onExitMessage");
            onEnterMessage = serializedObject.FindProperty("onEnterMessage");
            onTimeMessage = serializedObject.FindProperty("onTimeMessage");
            UseSendMessage = serializedObject.FindProperty("UseSendMessage");

            script = MonoScript.FromScriptableObject(MMessage);

            listOnEnter = new ReorderableList(serializedObject,onEnterMessage, true, true, true, true);
            listOnExit = new ReorderableList(serializedObject, sp_messagesONEXIT, true, true, true, true);
            listOnTime = new ReorderableList(serializedObject, onTimeMessage, true, true, true, true);

            listOnEnter.drawElementCallback = drawElementCallback1;
            listOnEnter.drawHeaderCallback = HeaderCallbackDelegate1;

            listOnExit.drawElementCallback = drawElementCallback2;
            listOnExit.drawHeaderCallback = HeaderCallbackDelegate1;

            listOnTime.drawElementCallback = drawElementCallback3;
            listOnTime.drawHeaderCallback = HeaderCallbackDelegate2;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Notifies the Animator Asociate Scripts\nWorks exactly like Animation Events");
        

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUI.indentLevel++;
                if (listOnEnter.count > 0) OnEnter = true;
                OnEnter = EditorGUILayout.Foldout(OnEnter, "On Enter (" + listOnEnter.count + ")");
                EditorGUI.indentLevel--;

                if (OnEnter)
                    listOnEnter.DoLayoutList();

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                if (listOnExit.count > 0) OnExit = true;
                OnExit = EditorGUILayout.Foldout(OnExit, "On Exit (" + listOnExit.count + ")");
                EditorGUI.indentLevel--;

                if (OnExit)
                    listOnExit.DoLayoutList();

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                if (listOnTime.count > 0) OnTime = true;
                OnTime = EditorGUILayout.Foldout(OnTime, "On Time (" + listOnTime.count + ")");
                EditorGUI.indentLevel--;

                if (OnTime)
                    listOnTime.DoLayoutList();

                EditorGUILayout.EndVertical();
                EditorGUILayout.PropertyField(UseSendMessage, new GUIContent("Use Send Message", "Uses the SendMessage() instead"));
            }

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Message Behaviour Inspector");
                //EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }


        /// <summary>
        /// Reordable List Header
        /// </summary>
        void HeaderCallbackDelegate1(Rect rect)
        {
            Rect R_1 = new Rect(rect.x + 10, rect.y, (rect.width / 3) + 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Message");

            Rect R_3 = new Rect(rect.x + 10 + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Type");

            Rect R_5 = new Rect(rect.x + 10 + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_5, "Value");
        }

        void HeaderCallbackDelegate2(Rect rect)
        {
            Rect R_1 = new Rect(rect.x+10, rect.y, (rect.width / 4)+30, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Message");

            Rect R_3 = new Rect(rect.x +10+ ((rect.width) / 4) + 5+30, rect.y, ((rect.width) / 4) - 5-15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Type");
            Rect R_4 = new Rect(rect.x +10+ ((rect.width) / 4) * 2 + 5+20, rect.y, ((rect.width) / 4) - 5, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_4, "Time");
            Rect R_5 = new Rect(rect.x + ((rect.width) / 4) * 3 + 5+10, rect.y, ((rect.width) / 4) - 5, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_5, "Value");

        }
        //ON ENTER
        void drawElementCallback1(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MMessage.onEnterMessage[index];
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
                    element.intValue = EditorGUI.IntField(R_5, element.intValue);
                    break;
                case TypeMessage.Float:
                    element.floatValue = EditorGUI.FloatField(R_5, element.floatValue);
                    break;
                case TypeMessage.String:
                    element.stringValue = EditorGUI.TextField(R_5, element.stringValue);
                    break;
                case TypeMessage.IntVar:
                    element.intVarValue = (IntVar)EditorGUI.ObjectField(R_5, element.intVarValue, typeof(IntVar), false);
                    break;
                default:
                    break;
            }

        }

        //ON EXIT
        void drawElementCallback2(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MMessage.onExitMessage[index];
            var sp_element = sp_messagesONEXIT.GetArrayElementAtIndex(index);

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            element.Active = EditorGUI.Toggle(R_0, element.Active);

            Rect R_1 = new Rect(rect.x + 15, rect.y, (rect.width / 3) + 15, EditorGUIUtility.singleLineHeight);
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
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("transformValue"), GUIContent.none);
                    break;
                default:
                    break;
            }

        }

        //ON Time
        void drawElementCallback3(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MMessage.onTimeMessage[index];
            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            element.Active = EditorGUI.Toggle(R_0, element.Active);

            Rect R_1 = new Rect(rect.x + 15, rect.y, (rect.width / 4) +15, EditorGUIUtility.singleLineHeight);
            element.message = EditorGUI.TextField(R_1, element.message);

            Rect R_3 = new Rect(rect.x + ((rect.width) / 4) + 5 + 30, rect.y, ((rect.width) / 4) - 5 - 5, EditorGUIUtility.singleLineHeight);
            element.typeM = (TypeMessage)EditorGUI.EnumPopup(R_3, element.typeM);

            Rect R_4 = new Rect(rect.x + ((rect.width) / 4) * 2 + 5 + 25, rect.y, ((rect.width) / 4) - 5 - 15, EditorGUIUtility.singleLineHeight);

            element.time = EditorGUI.FloatField(R_4, element.time);

            if (element.time > 1) element.time = 1;
            if (element.time < 0) element.time = 0;

            Rect R_5 = new Rect(rect.x + ((rect.width) / 4) * 3 + 15, rect.y, ((rect.width) / 4) - 15, EditorGUIUtility.singleLineHeight);
            switch (element.typeM)
            {
                case TypeMessage.Bool:
                    element.boolValue = EditorGUI.ToggleLeft(R_5, element.boolValue ? " True" : " False", element.boolValue);
                    break;
                case TypeMessage.Int:
                    element.intValue = EditorGUI.IntField(R_5, element.intValue);
                    break;
                case TypeMessage.Float:
                    element.floatValue = EditorGUI.FloatField(R_5, element.floatValue);
                    break;
                case TypeMessage.String:
                    element.stringValue = EditorGUI.TextField(R_5, element.stringValue);
                    break;
                case TypeMessage.IntVar:
                    element.intVarValue = (IntVar)EditorGUI.ObjectField(R_5, element.intVarValue, typeof(IntVar), false);
                    break;
                default:
                    break;
            }

        }

    }
}