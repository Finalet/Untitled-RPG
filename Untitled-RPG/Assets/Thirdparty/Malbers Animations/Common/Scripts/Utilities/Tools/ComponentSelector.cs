using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Tools/Component Selector")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/utilities/component-selector")]
    public class ComponentSelector : MonoBehaviour
    {
        public List<ComponentSet> internalComponents;
        public bool edit = true;

        public ComponentSet this[int index] => internalComponents[index];


        [ContextMenu("Show|Hide Editor")]
        private void ShowHideEditor()
        {
            edit ^= true;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        private void Reset()
        {
            internalComponents = new List<ComponentSet>();
        }

    }

    [System.Serializable]
    public class ComponentSet
    {
        public string name = "Description Here";
        [TextArea] public string tooltip;
        public bool active  = true;
        public GameObject[] gameObjects;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ComponentSelector))]
    public class SelectComponentsEditor : Editor
    {
        SerializedProperty internalComponents, edit;
        ComponentSelector M;
        ReorderableList ReoInternalComponents;

        private void OnEnable()
        {
            M = (ComponentSelector)target;
            internalComponents = serializedObject.FindProperty("internalComponents");
            edit = serializedObject.FindProperty("edit");

            ReoInternalComponents = new ReorderableList(serializedObject, internalComponents)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = internalComponents.GetArrayElementAtIndex(index);
                    var active = element.FindPropertyRelative("active");
                    var name = element.FindPropertyRelative("name");

                    var activeRect1 = new Rect(rect.x, rect.y - 1, 20, rect.height);
                    var IDRect = new Rect(rect.x + 20, rect.y, rect.width - 20, EditorGUIUtility.singleLineHeight);


                    active.boolValue = EditorGUI.Toggle(activeRect1, GUIContent.none, active.boolValue);
                    EditorGUI.PropertyField(IDRect, name, GUIContent.none);

                },
                drawHeaderCallback = (Rect rect) =>
                {
                    var r = new Rect(rect) { x = rect.x + 30, width = 60 };
                    var a = new Rect(rect) { width = 65 };

                    EditorGUI.LabelField(a, new GUIContent("Act", "Is the Component Selection ON or OFF"));
                    EditorGUI.LabelField(r, new GUIContent("Name", "Name of the Button"));
                },
                onAddCallback = (ReorderableList list) =>
                {
                    M.internalComponents.Add(new ComponentSet());
                    EditorUtility.SetDirty(M);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (edit.boolValue)
            {
                ReoInternalComponents.DoLayoutList();

                if (ReoInternalComponents.index != -1)
                {
                    var elem = internalComponents.GetArrayElementAtIndex(ReoInternalComponents.index);

                    var gos = elem.FindPropertyRelative("gameObjects");
                    var tooltip = elem.FindPropertyRelative("tooltip");
                    EditorGUILayout.PropertyField(gos,true);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(tooltip);
                }
            }
            else
            {
                if (internalComponents.arraySize > 0)

                    for (int i = 0; i < internalComponents.arraySize; i++)
                    {
                        var element = internalComponents.GetArrayElementAtIndex(i);
                        var name = element.FindPropertyRelative("name");
                        var tooltip = element.FindPropertyRelative("tooltip");
                        var active = element.FindPropertyRelative("active");

                        EditorGUILayout.BeginHorizontal(/*EditorStyles.helpBox*/);
                        if (GUILayout.Button(new GUIContent(name.stringValue, tooltip.stringValue), EditorStyles.miniButton))
                        {
                            Selection.objects = M[i].gameObjects;
                        }

                       

                        EditorGUI.BeginChangeCheck();
                        var currentGUIColor = GUI.color;
                        GUI.color = active.boolValue ? (GUI.color + Color.green)/2 : (GUI.color + Color.black) / 2;
                        active.boolValue = GUILayout.Toggle(active.boolValue, new GUIContent( active.boolValue ? "ON" : "OFF","Enable/Disable the "+ name.stringValue),
                            EditorStyles.miniButton, GUILayout.Width(40));
                        GUI.color = currentGUIColor;
                        if (EditorGUI.EndChangeCheck())
                        {
                            foreach (var item in M[i].gameObjects)
                            {
                                item.SetActive(active.boolValue);
                                EditorUtility.SetDirty(item);
                            }
                           
                        }   
                        EditorGUILayout.EndHorizontal();
                    }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}