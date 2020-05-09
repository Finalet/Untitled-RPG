using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace MalbersAnimations
{
    [CustomEditor(typeof(MalbersInput))/*, CanEditMultipleObjects*/]
    public class MalbersInputEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty inputs;
        private MalbersInput M;
        MonoScript script;

        private void OnEnable()
        {
            M = ((MalbersInput)target);
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            inputs = serializedObject.FindProperty("inputs");

            list = new ReorderableList(serializedObject, inputs, true, true, true, true);
            list.drawElementCallback = DrawElementCallback;
            list.drawHeaderCallback = HeaderCallbackDelegate;
            list.onAddCallback = OnAddCallBack;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Connects the INPUTS to the Locomotion System. The 'Name' is actually the Properties to access");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUI.BeginChangeCheck();
#if REWIRED
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayerID"), new GUIContent("Player ID", "Rewired Player ID"));
                EditorGUILayout.EndVertical();
#endif

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Horizontal"), new GUIContent("Horizontal", "Axis for the Horizontal Movement"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Vertical"), new GUIContent("Vertical", "Axis for the Forward/Backward Movement"));

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UpDown"), new GUIContent("UpDown", "Axis for the Up and Down Movement"));
                    if (GUILayout.Button(new GUIContent("Create","Creates 'UpDown' on the Input Manager"), GUILayout.Width(55)))
                    {
                        CreateInputAxe();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                list.DoLayoutList();
                //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //{
                //    EditorGUILayout.PropertyField(serializedObject.FindProperty("alwaysForward"), new GUIContent("Always Forward", "The Character will move forward forever"));
                //}
                //EditorGUILayout.EndVertical();

                var Index = list.index;

                if (Index != -1)
                {
                    SerializedProperty Element = inputs.GetArrayElementAtIndex(Index);
                    DrawInputEvents(Element, Index);
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.indentLevel++;
                    M.showInputEvents = EditorGUILayout.Foldout(M.showInputEvents, "Events (Enable/Disable Malbers Input)");
                    EditorGUI.indentLevel--;

                    if (M.showInputEvents)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInputEnabled"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInputDisabled"));
                    }
                }
                EditorGUILayout.EndVertical();


                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Malbers Input Inspector");
                    EditorUtility.SetDirty(target);
                }

             

                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();
        }


        void DrawInputEvents(SerializedProperty Element, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
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
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("LongPressTime"), new GUIContent("Long Press Time", "Time the Input Should be Pressed"));
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
            }
            EditorGUILayout.EndVertical();
        }

        void HeaderCallbackDelegate(Rect rect)
        {

            Rect R_1 = new Rect(rect.x + 20, rect.y, (rect.width - 20) / 4 + 12, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + (rect.width - 20) / 4 + 35, rect.y, (rect.width - 20) / 4 - 20, EditorGUIUtility.singleLineHeight);
            Rect R_3 = new Rect(rect.x + ((rect.width - 20) / 4) * 2 + 18, rect.y, ((rect.width - 30) / 4) + 11, EditorGUIUtility.singleLineHeight);
            Rect R_4 = new Rect(rect.x + ((rect.width) / 4) * 3 + 15, rect.y, ((rect.width) / 4) - 15, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_1, "   Name", EditorStyles.boldLabel);
            EditorGUI.LabelField(R_2, "   Type", EditorStyles.boldLabel);
            EditorGUI.LabelField(R_3, "  Value", EditorStyles.boldLabel);
            EditorGUI.LabelField(R_4, "Button", EditorStyles.boldLabel);
        }



        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = M.inputs[index];

            var elementSer = inputs.GetArrayElementAtIndex(index);

            rect.y += 2;
            element.active.Value = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight), element.active.Value);

            Rect R_1 = new Rect(rect.x + 20, rect.y, (rect.width - 20) / 4 + 12, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + (rect.width - 20) / 4 + 35, rect.y, (rect.width - 20) / 4 -20, EditorGUIUtility.singleLineHeight);
            Rect R_3 = new Rect(rect.x + ((rect.width - 20) / 4) * 2 + 18, rect.y, ((rect.width - 30) / 4)+11 , EditorGUIUtility.singleLineHeight);
            Rect R_4 = new Rect(rect.x + ((rect.width) / 4) * 3 +15, rect.y, ((rect.width) / 4)-15 , EditorGUIUtility.singleLineHeight);

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


            EditorGUI.PropertyField(R_2,type, GUIContent.none);


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
            M.inputs.Add(new  InputRow("New","InputValue", KeyCode.Alpha0, InputButton.Press, InputType.Input));
        }


        /// <summary>The element's axis number within the InputManager.</summary>
        private enum AxisNumber
        {
            X, Y, Three, Four, Five, Six, Seven, Eight, Nine, Ten
        }
        private enum AxisType
        {
            KeyMouseButton, Mouse, Joystick
        }

        //CREATE UP DOWN AXIS
        static void CreateInputAxe()
        {
            var InputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axesProperty = InputManager.FindProperty("m_Axes");

            AddInputAxis(axesProperty, "UpDown", "c", "space", "", "", 1000, 0.001f, 3, true, false, AxisType.KeyMouseButton, AxisNumber.X);

            InputManager.ApplyModifiedProperties();
        }

       

        private static void AddInputAxis(SerializedProperty axesProperty, string name, string negativeButton, string positiveButton,
                                string altNegativeButton, string altPositiveButton, float gravity, float dead, float sensitivity, bool snap, bool invert, AxisType axisType, AxisNumber axisNumber)
        {
            var property = FindAxisProperty(axesProperty, name);

            if (property != null)
            {
                property.FindPropertyRelative("m_Name").stringValue = name;
                property.FindPropertyRelative("negativeButton").stringValue = negativeButton;
                property.FindPropertyRelative("positiveButton").stringValue = positiveButton;
                property.FindPropertyRelative("altNegativeButton").stringValue = altNegativeButton;
                property.FindPropertyRelative("altPositiveButton").stringValue = altPositiveButton;
                property.FindPropertyRelative("gravity").floatValue = gravity;
                property.FindPropertyRelative("dead").floatValue = dead;
                property.FindPropertyRelative("sensitivity").floatValue = sensitivity;
                property.FindPropertyRelative("snap").boolValue = snap;
                property.FindPropertyRelative("invert").boolValue = invert;
                property.FindPropertyRelative("type").intValue = (int)axisType;
                property.FindPropertyRelative("axis").intValue = (int)axisNumber;
            }
        }

        private static SerializedProperty FindAxisProperty(SerializedProperty axesProperty, string name)
        {
            SerializedProperty foundProperty = null;

            for (int i = 0; i < axesProperty.arraySize; ++i)
            {
                var property = axesProperty.GetArrayElementAtIndex(i);
                if (property.FindPropertyRelative("m_Name").stringValue.Equals(name))
                {
                    foundProperty = property;
                    break;
                }
            }

            if (foundProperty == null)  // If no property was found then create a new one.
            {
                axesProperty.InsertArrayElementAtIndex(axesProperty.arraySize);
                foundProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
                Debug.Log("Added UPDown Input ");
            }

            return foundProperty;
        }

    }
}