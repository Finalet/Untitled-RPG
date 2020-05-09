using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace MalbersAnimations
{
    [CustomPropertyDrawer(typeof(InputRow))]
    public class InputRowDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // EditorGUI.HelpBox(position, "",MessageType.None);

            EditorGUI.BeginProperty(position, label, property);

            var DefaultPosition = position;

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            property.FindPropertyRelative("name").stringValue = label.text;

            var height = EditorGUIUtility.singleLineHeight;
            // Calculate rects
           // var activeRect = new Rect(position.x, position.y, 15, height);
            var LabelRect = new Rect(position.x /*+ 17*/, position.y, 100, height);


            // Draw fields - passs GUIContent.none to each so they are drawn without labels
          //  EditorGUI.PropertyField(activeRect, property.FindPropertyRelative("active"), GUIContent.none);
            EditorGUI.LabelField(LabelRect, label/*, EditorStyles.boldLabel*/);

            //Set Rect to the Parameters Values
           var posValue = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(" "));

            // Calculate rects
            var typeRect = new Rect(posValue.x - 30, posValue.y, 44, height);
            var valueRect = new Rect(posValue.x - 30 + 45, posValue.y, posValue.width / 2 - 11, height);
            var ActionRect = new Rect(posValue.width / 2 + posValue.x - 30 + 40 - 5, posValue.y, (posValue.width / 2 - 7) - 16 , height);
            var ShowRect = new Rect(DefaultPosition.width+2 , posValue.y, 16, height-1);

            EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("type"), GUIContent.none);

            InputType current = (InputType)property.FindPropertyRelative("type").enumValueIndex;
            switch (current)
            {
                case InputType.Input:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("input"), GUIContent.none);
                    break;
                case InputType.Key:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("key"), GUIContent.none);
                    break;
                default:
                    break;
            }

              EditorGUI.PropertyField(ActionRect, property.FindPropertyRelative("GetPressed"), GUIContent.none);

           // SerializedProperty show = property.FindPropertyRelative("ShowEvents");


            property.isExpanded = GUI.Toggle(ShowRect, property.isExpanded, new GUIContent("", "Show Events for the " +property.FindPropertyRelative("name").stringValue +" Input"), EditorStyles.foldout);

            if (property.isExpanded)
            {
                DefaultPosition.y += height + 3;

                Rect activeRectt = new Rect(position);

                activeRectt.height = height;
                activeRectt.y += height + 3;
                EditorGUI.PropertyField(activeRectt, property.FindPropertyRelative("active"), new GUIContent("Active", "Enable Disable the Input"));

                DefaultPosition.y += height + 3;

                InputButton enumValueIndex = (InputButton)property.FindPropertyRelative("GetPressed").enumValueIndex;

                var OnInputPressed = property.FindPropertyRelative("OnInputPressed");
                var OnInputChanged = property.FindPropertyRelative("OnInputChanged");
                var OnInputUp = property.FindPropertyRelative("OnInputUp");
                var OnInputDown = property.FindPropertyRelative("OnInputDown");
               


                switch (enumValueIndex)
                {
                    case InputButton.Press:
                        EditorGUI.PropertyField(DefaultPosition, OnInputPressed);

                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnInputPressed);
                        EditorGUI.PropertyField(DefaultPosition, OnInputChanged);

                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnInputChanged);
                        EditorGUI.PropertyField(DefaultPosition, OnInputUp);

                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnInputUp);
                        EditorGUI.PropertyField(DefaultPosition, OnInputDown);
                        break;
                    case InputButton.Down:
                        EditorGUI.PropertyField(DefaultPosition, OnInputDown);

                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnInputDown);
                        EditorGUI.PropertyField(DefaultPosition, OnInputChanged);
                        break;
                    case InputButton.Up:
                        EditorGUI.PropertyField(DefaultPosition, OnInputUp);

                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnInputUp);
                        EditorGUI.PropertyField(DefaultPosition, OnInputChanged);
                        break;
                    case InputButton.LongPress:
                        Rect LonRect = DefaultPosition;
                        LonRect.height = height;
                        EditorGUI.PropertyField(LonRect, property.FindPropertyRelative("LongPressTime"), new GUIContent("Long Press Time", "Time the Input Should be Pressed"));
                        DefaultPosition.y += height + 3;

                        var OnLongPress = property.FindPropertyRelative("OnLongPress");
                        EditorGUI.PropertyField(DefaultPosition,OnLongPress , new GUIContent("On Long Press"));
                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnLongPress);

                        var OnPressedNormalized = property.FindPropertyRelative("OnPressedNormalized");
                        EditorGUI.PropertyField(DefaultPosition, OnPressedNormalized, new GUIContent("On Pressed Time Normalized"));
                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnPressedNormalized);

                        EditorGUI.PropertyField(DefaultPosition, OnInputDown, new GUIContent("On Pressed Down"));
                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnInputDown);

                        EditorGUI.PropertyField(DefaultPosition, OnInputUp, new GUIContent("On Pressed Interrupted"));
                      //  DefaultPosition.y += EditorGUI.GetPropertyHeight(OnInputUp);
                        break;
                    case InputButton.DoubleTap:
                        Rect LonRect1 = DefaultPosition;
                        LonRect1.height = height;
                        EditorGUI.PropertyField(LonRect1, property.FindPropertyRelative("DoubleTapTime"), new GUIContent("Double Tap Time", "Time between the double tap"));
                        DefaultPosition.y += height + 3;

                        EditorGUI.PropertyField(DefaultPosition, OnInputDown, new GUIContent("On First Tap"));
                        DefaultPosition.y += EditorGUI.GetPropertyHeight(OnInputDown);
                        EditorGUI.PropertyField(DefaultPosition, property.FindPropertyRelative("OnDoubleTap"));
                        break;
                    default:
                        break;
                }
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float NewHeight = base.GetPropertyHeight(property, label) + 3;

            if (property.isExpanded)
            {
                NewHeight += 16;

                InputButton enumValueIndex = (InputButton)property.FindPropertyRelative("GetPressed").enumValueIndex;
                NewHeight += 3;
                var OnInputPressed = property.FindPropertyRelative("OnInputPressed");
                var OnInputChanged = property.FindPropertyRelative("OnInputChanged");
                var OnInputUp = property.FindPropertyRelative("OnInputUp");
                var OnInputDown = property.FindPropertyRelative("OnInputDown");
                var OnLongPress = property.FindPropertyRelative("OnLongPress");
                var OnPressedNormalized = property.FindPropertyRelative("OnPressedNormalized");

                switch (enumValueIndex)
                {
                    case InputButton.Press:
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputPressed);
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputChanged);
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputUp);
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputDown);
                        break;
                    case InputButton.Down:
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputDown);
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputChanged);
                        break;
                    case InputButton.Up:
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputUp);
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputChanged);
                        break;
                    case InputButton.LongPress:
                        NewHeight += EditorGUIUtility.singleLineHeight + 3; //LongPressTime
                        NewHeight += EditorGUI.GetPropertyHeight(OnLongPress);
                        NewHeight += EditorGUI.GetPropertyHeight(OnPressedNormalized);
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputUp);
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputDown);
                        break;
                    case InputButton.DoubleTap:
                        NewHeight += EditorGUIUtility.singleLineHeight + 3; //LongPressTime
                        NewHeight += EditorGUI.GetPropertyHeight(OnInputDown);
                        NewHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnDoubleTap"));
                        break;
                    default:
                        break;
                }
            }
            return NewHeight;
        }
    }
   
}