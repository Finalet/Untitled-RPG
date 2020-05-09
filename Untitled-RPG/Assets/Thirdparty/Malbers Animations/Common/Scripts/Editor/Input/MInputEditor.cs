using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace MalbersAnimations
{
    [CustomEditor(typeof(MInput))]
    public class MInputEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty inputs, showInputEvents;
        private MInput M;
        MonoScript script;


        private void OnEnable()
        {
            M = ((MInput)target);
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            inputs = serializedObject.FindProperty("inputs");
            showInputEvents = serializedObject.FindProperty("showInputEvents");

            list = new ReorderableList(serializedObject, inputs, true, true, true, true);
            list.drawElementCallback = DrawElementCallback;
            list.drawHeaderCallback = HeaderCallbackDelegate;
            list.onAddCallback = OnAddCallBack;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Inputs for conecting with Scripts");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUI.BeginChangeCheck();
                {
#if REWIRED
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayerID"), new GUIContent("Player ID", "Rewired Player ID"));
                EditorGUILayout.EndVertical();
#endif

                    list.DoLayoutList();

                    var Index = list.index;

                    if (Index != -1)
                    {
                        SerializedProperty Element = inputs.GetArrayElementAtIndex(Index);
                        DrawInputEvents(Element, Index);
                    }

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUI.indentLevel++;
                        showInputEvents.boolValue = EditorGUILayout.Foldout(showInputEvents.boolValue, "Events (Enable/Disable Malbers Input)");
                        EditorGUI.indentLevel--;

                        if (showInputEvents.boolValue)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInputEnabled"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInputDisabled"));
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "MInput Inspector");
                }
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawInputEvents(SerializedProperty Element, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel++;
            Element.isExpanded = EditorGUILayout.Foldout(Element.isExpanded, new GUIContent(Element.FindPropertyRelative("name").stringValue + " Properties"));
            EditorGUI.indentLevel--;
            if (Element.isExpanded)
            {

                EditorGUILayout.PropertyField(Element.FindPropertyRelative("active"));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

                InputButton GetPressed = (InputButton)Element.FindPropertyRelative("GetPressed").enumValueIndex;


                switch (GetPressed)
                {
                    case InputButton.Press:
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputPressed"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputChanged"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputDown"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputUp"));
                        break;
                    case InputButton.Down:
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputDown"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputChanged"));
                        break;
                    case InputButton.Up:
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputUp"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputChanged"));
                        break;
                    case InputButton.LongPress:
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("LongPressTime"), new GUIContent("On Long Press", "Time the Input Should be Pressed"));
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnLongPress"), new GUIContent("On Long Press"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnPressedNormalized"), new GUIContent("On Pressed Time Normalized"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputDown"), new GUIContent("On Pressed Down"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputUp"), new GUIContent("On Pressed Interrupted"));
                        break;
                    case InputButton.DoubleTap:
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("DoubleTapTime"));
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputDown"), new GUIContent("On First Tap"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnDoubleTap"));
                        break;
                    default:
                        break;
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>Reordable List Header</summary>
        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_1 = new Rect(rect.x + 20, rect.y, (rect.width - 20) / 4 - 23, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Name");

            Rect R_2 = new Rect(rect.x + (rect.width - 20) / 4 + 15, rect.y, (rect.width - 20) / 4, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_2, "Type");

            Rect R_3 = new Rect(rect.x + ((rect.width - 20) / 4) * 2 + 18, rect.y, ((rect.width - 30) / 4) + 11, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Value");

            Rect R_4 = new Rect(rect.x + ((rect.width) / 4) * 3 + 15, rect.y, ((rect.width) / 4) - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_4, "Button");
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = M.inputs[index];

            var elementSer = inputs.GetArrayElementAtIndex(index);

            rect.y += 2;
            element.active.Value = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight), element.active.Value);

            Rect R_1 = new Rect(rect.x + 20, rect.y, (rect.width - 20) / 4 + 12, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + (rect.width - 20) / 4 + 35, rect.y, (rect.width - 20) / 4 - 20, EditorGUIUtility.singleLineHeight);
            Rect R_3 = new Rect(rect.x + ((rect.width - 20) / 4) * 2 + 18, rect.y, ((rect.width - 30) / 4) + 11, EditorGUIUtility.singleLineHeight);
            Rect R_4 = new Rect(rect.x + ((rect.width) / 4) * 3 + 15, rect.y, ((rect.width) / 4) - 15, EditorGUIUtility.singleLineHeight);

            //GUIStyle a = new GUIStyle(EditorStyles.label);

            ////This make the name a editable label
            //a.fontStyle = FontStyle.Normal;


            var name = elementSer.FindPropertyRelative("name");
            var type = elementSer.FindPropertyRelative("type");
            var input = elementSer.FindPropertyRelative("input");
            var key = elementSer.FindPropertyRelative("key");
            var GetPressed = elementSer.FindPropertyRelative("GetPressed");

            EditorGUI.PropertyField(R_1, name, GUIContent.none);
            //name.stringValue = EditorGUI.TextField(R_1, name.stringValue, EditorStyles.textField);


            EditorGUI.PropertyField(R_2, type, GUIContent.none);


            if (type.intValue != 1)
                EditorGUI.PropertyField(R_3, input, GUIContent.none);
            else
                EditorGUI.PropertyField(R_3, key, GUIContent.none);

            EditorGUI.PropertyField(R_4, GetPressed, GUIContent.none);
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (M.inputs == null)
            {
                M.inputs = new System.Collections.Generic.List<InputRow>();
            }
            M.inputs.Add(new InputRow(true,"New", "InputValue", KeyCode.Alpha0, InputButton.Press, InputType.Input));
        }
    }
}