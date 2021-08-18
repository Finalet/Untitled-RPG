using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Events
{
    ///<summary>
    /// Listener to use with the GameEvents
    /// Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [AddComponentMenu("Malbers/Events/Event Listener")]
    public class MEventListener : MonoBehaviour
    {
        public List<MEventItemListener> Events = new List<MEventItemListener>();
        // public bool debug;

        private void OnEnable()
        {
            foreach (var item in Events)
            {
                if (item.Event) item.Event.RegisterListener(item);
            }
        }

        private void OnDisable()
        {
            foreach (var item in Events)
            {
                if (item.Event) item.Event.UnregisterListener(item);
            }
        }
    }

    [System.Serializable]
    public class AdvancedIntegerEvent
    {
        public bool active = true;
        public string name;
        public ComparerInt comparer = ComparerInt.Equal;
        public IntReference Value = new IntReference();
        public IntEvent Response = new IntEvent();

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="IntValue">Value that comes from the IntEvent</param>
        public void ExecuteAdvanceIntegerEvent(int IntValue)
        {
            if (active)
            {
                switch (comparer)
                {
                    case ComparerInt.Equal:
                        if (IntValue == Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.Greater:
                        if (IntValue > Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.Less:
                        if (IntValue < Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.NotEqual:
                        if (IntValue != Value) Response.Invoke(IntValue);
                        break;
                    default:
                        break;
                }
            }
        }

        public AdvancedIntegerEvent()
        {
            active = true;
            name = "NameHere";
            comparer = ComparerInt.Equal;
            Value = new IntReference();
            Response = new IntEvent();
        }
    }

    [System.Serializable]
    public class AdvancedFloatEvent
    {
        public bool active = true;
        public string name;
        public ComparerInt comparer = ComparerInt.Equal;
        public FloatReference Value = new FloatReference();
        public FloatEvent Response = new FloatEvent();

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="v">Value that comes from the IntEvent</param>
        public void ExecuteAdvanceFloatEvent(float v)
        {
            if (active)
            {
                switch (comparer)
                {
                    case ComparerInt.Equal:
                        if (v == Value) Response.Invoke(v);
                        break;
                    case ComparerInt.Greater:
                        if (v > Value) Response.Invoke(v);
                        break;
                    case ComparerInt.Less:
                        if (v < Value) Response.Invoke(v);
                        break;
                    case ComparerInt.NotEqual:
                        if (v != Value) Response.Invoke(v);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    [System.Serializable]
    public class AdvancedBoolEvent
    {
        public bool active = true;
        public string name;
        public ComparerBool comparer = ComparerBool.Equal;
        public BoolReference Value = new BoolReference();
        public UnityEvent Response = new UnityEvent();

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="boolValue">Value that comes from the IntEvent</param>
        public void ExecuteAdvanceBoolEvent(bool boolValue)
        {
            if (active)
            {
                switch (comparer)
                {
                    case ComparerBool.Equal:
                        if (boolValue == Value) Response.Invoke();
                        break;
                    case ComparerBool.NotEqual:
                        if (boolValue != Value) Response.Invoke();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    [System.Serializable]
    public class AdvancedStringEvent
    {
        public bool active = true;
        public string name;
        public ComparerString comparer = ComparerString.Equal;
        public StringReference Value = new StringReference();
        public StringEvent Response = new StringEvent();

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="val">Value that comes from the string event</param>
        public void ExecuteAdvanceStringEvent(string val)
        {
            if (active)
            {
                switch (comparer)
                {
                    case ComparerString.Equal:
                        if (val == Value.Value) Response.Invoke(val);
                        break;
                    case ComparerString.NotEqual:
                        if (val != Value.Value) Response.Invoke(val);
                        break;
                    case ComparerString.Empty:
                        if (string.IsNullOrEmpty(val)) Response.Invoke(val);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    [System.Serializable]
    public class MEventItemListener
    {
#pragma warning disable CA2235 // Mark all non-serializable fields
        public MEvent Event;
#pragma warning restore CA2235 // Mark all non-serializable fields
        //public Sprite Pin_Sprite { get; set; }
        //public GameObject Pin_GameObject { get; set; }
        //public Transform Pin_Transform { get; set; }
        //public Component Pin_Component { get; set; }

        [HideInInspector]
        public bool useInt = false, useFloat = false, useVoid = true, useString = false, useBool = false,
            useGO = false, useTransform = false, useVector3, useVector2 = false, useComponent = false, useSprite = false;

        public UnityEvent Response = new UnityEvent();

        public UnityEvent ResponseNull = new UnityEvent();

        public FloatEvent ResponseFloat = new FloatEvent();
        public IntEvent ResponseInt = new IntEvent();

        public BoolEvent ResponseBool = new BoolEvent();
        public UnityEvent ResponseBoolFalse = new UnityEvent();
        public UnityEvent ResponseBoolTrue = new UnityEvent();

        public StringEvent ResponseString = new StringEvent();
        public GameObjectEvent ResponseGO = new GameObjectEvent();
        public TransformEvent ResponseTransform = new TransformEvent();
        public ComponentEvent ResponseComponent = new ComponentEvent();
        public SpriteEvent ResponseSprite = new SpriteEvent();
        public Vector3Event ResponseVector3 = new Vector3Event();
        public Vector2Event ResponseVector2 = new Vector2Event();

        public List<AdvancedIntegerEvent> IntEventList = new List<AdvancedIntegerEvent>();
        public bool AdvancedInteger = false;
        public bool AdvancedBool = false;
        [Tooltip("Inverts the value of the Bool Event")]
        public bool InvertBool = false;

        public virtual void OnEventInvoked() => Response.Invoke();
        public virtual void OnEventInvoked(string value) => ResponseString.Invoke(value);
        public virtual void OnEventInvoked(float value) => ResponseFloat.Invoke(value);

        public virtual void OnEventInvoked(int value)
        {
            ResponseInt.Invoke(value);

            if (AdvancedInteger)
            {
                foreach (var item in IntEventList)
                    item.ExecuteAdvanceIntegerEvent(value);
            }
        }

        public virtual void OnEventInvoked(bool value)
        {
            ResponseBool.Invoke(InvertBool ? !value : value);

            if (AdvancedBool)
            {
                if (value)
                    ResponseBoolTrue.Invoke();
                else
                    ResponseBoolFalse.Invoke();
            }
        }
        public virtual void OnEventInvoked(Vector3 value) => ResponseVector3.Invoke(value);
        public virtual void OnEventInvoked(Vector2 value) => ResponseVector2.Invoke(value);



        public virtual void OnEventInvoked(GameObject value)
        {
            if (value)
                ResponseGO.Invoke(value);
            else
                ResponseNull.Invoke();
        }

        public virtual void OnEventInvoked(Transform value)
        {
            ResponseTransform.Invoke(value);
            if (!value) ResponseNull.Invoke();
        }

        public virtual void OnEventInvoked(Component value)
        {
            if (value)
                ResponseComponent.Invoke(value);
            else
                ResponseNull.Invoke();
        }

        public virtual void OnEventInvoked(Sprite value)
        {
            if (value)
                ResponseSprite.Invoke(value);
            else
                ResponseNull.Invoke();
        }

        public MEventItemListener()
        {
            useVoid = true;
            useInt = useFloat = useString = useBool = useGO = useTransform = useVector3 = useVector2 = useSprite = useComponent = false;
        }
    }


    /// <summary> CustomPropertyDrawer</summary>

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AdvancedIntegerEvent))]
    [CustomPropertyDrawer(typeof(AdvancedFloatEvent))]
    public class AdvIntegerComparerDrawer : PropertyDrawer
    {
        //const float labelwith = 27f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += 2;

            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;
            var name = property.FindPropertyRelative("name");
            var comparer = property.FindPropertyRelative("comparer");
            var Value = property.FindPropertyRelative("Value");
            var Response = property.FindPropertyRelative("Response");

            //bool isExpanded = property.isExpanded;


            if (name.stringValue == string.Empty) name.stringValue = "NameHere";

            var line = position;
            line.height = height;

            line.x += 4;
            line.width -= 8;

            var foldout = line;
            foldout.width = 10;
            foldout.x += 10;

            EditorGUIUtility.labelWidth = 16;
            property.isExpanded = EditorGUI.Foldout(foldout, property.isExpanded, GUIContent.none);
            EditorGUIUtility.labelWidth = 0;

            var rectName = line;

            rectName.x += 10;
            rectName.width -= 10;

            name.stringValue = GUI.TextField(rectName, name.stringValue, EditorStyles.boldLabel);

            line.y += height + 2;

            if (property.isExpanded)
            {
                var ComparerRect = new Rect(line.x, line.y, line.width / 2 - 10, height);
                var ValueRect = new Rect(line.x + line.width / 2 + 15, line.y, line.width / 2 - 10, height);

                EditorGUI.PropertyField(ComparerRect, comparer, GUIContent.none);
                EditorGUI.PropertyField(ValueRect, Value, GUIContent.none);
                line.y += height + 2;
                EditorGUI.PropertyField(line, Response);
                position.height = line.height;
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return base.GetPropertyHeight(property, label);

            var Response = property.FindPropertyRelative("Response");
            float ResponseHeight = EditorGUI.GetPropertyHeight(Response);

            return 16 * 2 + ResponseHeight + 10;
        }
    }


    [CanEditMultipleObjects, CustomEditor(typeof(MEventListener))]
    public class MEventListenerEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty eventsListeners, useFloat, useBool, useInt, useString, useVoid, useGo, useTransform, useVector3, useSprite, useVector2, useComponent;
        private MEventListener M;
       // MonoScript script;

        private Dictionary<string, ReorderableList> innerListDict = new Dictionary<string, ReorderableList>();


        private void OnEnable()
        {
            M = ((MEventListener)target);
         //   script = MonoScript.FromMonoBehaviour(M);

            eventsListeners = serializedObject.FindProperty("Events");
            // debug = serializedObject.FindProperty("debug");

            list = new ReorderableList(serializedObject, eventsListeners, true, true, true, true)
            {
                drawElementCallback = drawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack
            };
        }


        void HeaderCallbackDelegate(Rect rect)
        {
            EditorGUI.LabelField(rect, "   Event Listeners");
            //Rect R_3 = new Rect(rect.width -17, rect.y-1, 50, EditorGUIUtility.singleLineHeight+2);
            //debug.boolValue = GUI.Toggle(R_3, debug.boolValue, new GUIContent("Debug" ), EditorStyles.miniButton);
        }

        void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            rect.height -= 5;
            SerializedProperty Element = eventsListeners.GetArrayElementAtIndex(index).FindPropertyRelative("Event");
            eventsListeners.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, Element, GUIContent.none);
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (M.Events == null)
            {
                M.Events = new List<MEventItemListener>();
            }

            M.Events.Add(new MEventItemListener());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Events Listeners. They Response to the MEvents Assets\nThe MEvents should not repeat on the list");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
              //  MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(/*EditorStyles.helpBox*/);
                {
                    list.DoLayoutList();

                    if (list.index != -1)
                    {
                        if (list.index < list.count)
                        {
                            SerializedProperty Element = eventsListeners.GetArrayElementAtIndex(list.index);

                            if (M.Events[list.index].Event != null)
                            {

                                string Descp = M.Events[list.index].Event.Description;

                                if (Descp != string.Empty)
                                {
                                    EditorGUILayout.BeginVertical(MalbersEditor.StyleGreen);
                                    {
                                        EditorGUILayout.HelpBox(M.Events[list.index].Event.Description, MessageType.None);
                                    }
                                    EditorGUILayout.EndVertical();
                                }

                                EditorGUILayout.Space();

                                useFloat = Element.FindPropertyRelative("useFloat");
                                useBool = Element.FindPropertyRelative("useBool");
                                useInt = Element.FindPropertyRelative("useInt");
                                useString = Element.FindPropertyRelative("useString");
                                useVoid = Element.FindPropertyRelative("useVoid");
                                useGo = Element.FindPropertyRelative("useGO");
                                useTransform = Element.FindPropertyRelative("useTransform");
                                useComponent = Element.FindPropertyRelative("useComponent");
                                useVector3 = Element.FindPropertyRelative("useVector3");
                                useVector2 = Element.FindPropertyRelative("useVector2");
                                useSprite = Element.FindPropertyRelative("useSprite");

                                var TypeStyle = new GUIStyle( EditorStyles.objectField);
                            

                                EditorGUILayout.BeginHorizontal();
                                {
                                    useVoid.boolValue = GUILayout.Toggle(useVoid.boolValue, new GUIContent("Void", "No Parameters Response"), TypeStyle);
                                    useBool.boolValue = GUILayout.Toggle(useBool.boolValue, new GUIContent("Bool", "Bool Response"), TypeStyle);
                                    useFloat.boolValue = GUILayout.Toggle(useFloat.boolValue, new GUIContent("Float", "Float Response"), TypeStyle);
                                    useInt.boolValue = GUILayout.Toggle(useInt.boolValue, new GUIContent("Int", "Int Response"), TypeStyle);
                                    useString.boolValue = GUILayout.Toggle(useString.boolValue, new GUIContent("String", "String Response"), TypeStyle);
                                    useVector3.boolValue = GUILayout.Toggle(useVector3.boolValue, new GUIContent("V3", "Vector3 Response"), TypeStyle);
                                    useVector2.boolValue = GUILayout.Toggle(useVector2.boolValue, new GUIContent("V2", "Vector3 Response"), TypeStyle);
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                {
                                    useGo.boolValue = GUILayout.Toggle(useGo.boolValue, new GUIContent("GameObject", "Game Object Response"), TypeStyle);
                                    useTransform.boolValue = GUILayout.Toggle(useTransform.boolValue, new GUIContent("Transform", "Transform Response"), TypeStyle);
                                    useComponent.boolValue = GUILayout.Toggle(useComponent.boolValue, new GUIContent("Component", "Component Response"), TypeStyle);
                                    useSprite.boolValue = GUILayout.Toggle(useSprite.boolValue, new GUIContent("Sprite", "Sprite Response"), TypeStyle);
                                }
                                EditorGUILayout.EndHorizontal();

                                Draw_Void(Element);

                                Draw_Bool(Element);

                                Draw_Float(Element);

                                Draw_Integer(Element);

                                DrawString(Element);

                                Draw_GameObject(Element);

                                DrawTransform(Element);

                                DrawComponent(Element);

                                DrawSprite(Element);

                                DrawVector2(Element);

                                DrawVector3(Element);

                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private void Draw_Void(SerializedProperty Element)
        {
            if (useVoid.boolValue)
            {
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("Response"));
            }
        }

        private void Draw_Bool(SerializedProperty Element)
        {
            if (useBool.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                var useAdvBool = Element.FindPropertyRelative("AdvancedBool");
                // if (!useAdvBool.boolValue)
                {
                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("InvertBool"));
                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseBool"), new GUIContent("Response"));
                }
                useAdvBool.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Use Advanced Bool", "Uses Separated Unity Events for True and False Entries"), useAdvBool.boolValue);
                if (useAdvBool.boolValue)
                {
                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseBoolTrue"), new GUIContent("On True"));
                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseBoolFalse"), new GUIContent("On False"));
                }
            }
        }

        private void Draw_Float(SerializedProperty Element)
        {
            if (useFloat.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseFloat"), new GUIContent("Response"));
            }
        }

        private void Draw_Integer(SerializedProperty Element)
        {
            if (useInt.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();

                var useAdvInteger = Element.FindPropertyRelative("AdvancedInteger");
                useAdvInteger.boolValue = GUILayout.Toggle(useAdvInteger.boolValue, new GUIContent("Use Integer Comparer",
                    "Compare the Event entry value with a new  one to make a new Int Response"),  EditorStyles.foldoutHeader);

                if (useAdvInteger.boolValue)
                {
                    var compare = Element.FindPropertyRelative("IntEventList");
                    ReorderableList Reo_AbilityList;

                    string listKey = Element.propertyPath;

                    if (innerListDict.ContainsKey(listKey))
                    {
                        // fetch the reorderable list in dict
                        Reo_AbilityList = innerListDict[listKey];
                    }
                    else
                    {
                        Reo_AbilityList = new ReorderableList(serializedObject, compare, true, true, true, true)
                        {
                            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                var element = compare.GetArrayElementAtIndex(index);
                               
                                var name = element.FindPropertyRelative("name");
                                var active = element.FindPropertyRelative("active");
                                var comparer = element.FindPropertyRelative("comparer");
                                var Value = element.FindPropertyRelative("Value");

                                rect.y += 1;
                                var height = UnityEditor.EditorGUIUtility.singleLineHeight;
                                var split = rect.width / 3;
                                var p = 5;

                               

                                var rectActveName = new Rect(rect.x, rect.y, 20, height);
                                var rectName = new Rect(rect.x+20, rect.y, (split + p - 2) * 1.2f-20, height);
                                var rectComparer = new Rect(rect.x + (split + p)*1.2f+20, rect.y, (split - p)*0.75f-20, height);
                                var rectValue = new Rect(rect.x + split * 2 + p + 15+20, rect.y, split - p - 10-20, height);


                                EditorGUI.PropertyField(rectActveName, active, GUIContent.none);
                                EditorGUI.PropertyField(rectName, name, GUIContent.none);
                                EditorGUI.PropertyField(rectComparer, comparer, GUIContent.none);
                                EditorGUI.PropertyField(rectValue, Value, GUIContent.none);
                            },

                            drawHeaderCallback = (Rect rect) =>
                            {
                                rect.y += 1;
                                var height = UnityEditor.EditorGUIUtility.singleLineHeight;
                                var split = rect.width / 3;
                                var p = (split * 0.3f);
                                var rectName = new Rect(rect.x, rect.y, split + p - 2, height);
                                var rectComparer = new Rect(rect.x + split + p, rect.y, split - p + 15, height);
                                var rectValue = new Rect(rect.x + split * 2 + p + 5, rect.y, split - p, height);
                                var DebugRect = new Rect(rect.width, rect.y - 1, 25, height + 2);

                                EditorGUI.LabelField(rectName, "    Name");
                                EditorGUI.LabelField(rectComparer, " Compare");
                                EditorGUI.LabelField(rectValue, " Value");
                            },
                        };


                        innerListDict.Add(listKey, Reo_AbilityList);  //Store it on the Editor

                    }
                    Reo_AbilityList.DoLayoutList();


                    int SelectedAbility = Reo_AbilityList.index;


                    if (SelectedAbility != -1)
                    {
                        var element = compare.GetArrayElementAtIndex(SelectedAbility);
                        if (element != null)
                        {
                            var Response = element.FindPropertyRelative("Response");
                            var name = element.FindPropertyRelative("name").stringValue;
                            UnityEditor.EditorGUILayout.PropertyField(Response, new GUIContent("Response: [" + name + "]   "));
                        }
                    }

                }
                //   else
                {
                    MalbersEditor.DrawLineHelpBox();
                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseInt"), new GUIContent("Response"));
                }
            }
        }

        private void DrawString(SerializedProperty Element)
        {
            if (useString.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseString"), new GUIContent("Response"));
            }
        }

        private void Draw_GameObject(SerializedProperty Element)
        {
            if (useGo.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseGO"), new GUIContent("Response GO"));
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseNull"), new GUIContent("Response NULL"));
            }
        }

        private void DrawTransform(SerializedProperty Element)
        {
            if (useTransform.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseTransform"), new GUIContent("Response T"));
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseNull"), new GUIContent("Response NULL"));

            }
        }

        private void DrawComponent(SerializedProperty Element)
        {
            if (useComponent.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseComponent"), new GUIContent("Response C"));
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseNull"), new GUIContent("Response NULL"));
            }
        }

        private void DrawSprite(SerializedProperty Element)
        {
            if (useSprite.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseSprite"), new GUIContent("Response Sprite"));
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseNull"), new GUIContent("Response NULL"));
            }
        }

        private void DrawVector2(SerializedProperty Element)
        {
            if (useVector3.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseVector3"), new GUIContent("Response V3"));
            }
        }

        private void DrawVector3(SerializedProperty Element)
        {
            if (useVector2.boolValue)
            {
                MalbersEditor.DrawLineHelpBox();
                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ResponseVector2"), new GUIContent("Response V2"));
            }
        }
    }
#endif
}