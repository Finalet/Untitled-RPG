using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(MaterialChanger))]
    public class MaterialChangerEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty materialList, showMeshesList, random, changeHidden;
        private MaterialChanger M;

        private void OnEnable()
        {
            M = ((MaterialChanger)target);

            materialList = serializedObject.FindProperty("materialList");
            showMeshesList = serializedObject.FindProperty("showMeshesList");
            changeHidden = serializedObject.FindProperty("changeHidden");
            random = serializedObject.FindProperty("random");

            list = new ReorderableList(serializedObject, materialList, true, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack
            };
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Swap Materials");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                    list.DoLayoutList();
                    EditorGUI.indentLevel++;

                    if (showMeshesList.boolValue)
                    {
                        if (list.index != -1)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            {
                                SerializedProperty Element = materialList.GetArrayElementAtIndex(list.index);
                                //if (Element.objectReferenceValue != null)
                                {
                                    EditorGUILayout.LabelField(Element.FindPropertyRelative("Name").stringValue, EditorStyles.boldLabel);

                                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                    {
                                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("mesh"), new GUIContent("Mesh", "Mesh object to apply the Materials"));
                                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("indexM"), new GUIContent("ID", "Material ID"));
                                    }
                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                    {
                                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("materials"), new GUIContent("Materials"), true);
                                    }
                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                    {
                                        SerializedProperty hasLODS = Element.FindPropertyRelative("HasLODs");
                                        EditorGUILayout.PropertyField(hasLODS, new GUIContent("LODs", "Has Level of Detail Meshes"));
                                        if (hasLODS.boolValue)
                                        {
                                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("LODs"), new GUIContent("Meshes", "Has Level of Detail Meshes"), true);
                                        }
                                    }
                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                                    {
                                        EditorGUIUtility.labelWidth = 65;
                                        var linked = Element.FindPropertyRelative("Linked");

                                        EditorGUILayout.PropertyField(linked, new GUIContent("Linked", "This Material Item will be driven by another Material Item"));
                                        if (linked.boolValue)
                                        {
                                            var Master = Element.FindPropertyRelative("Master");
                                            EditorGUILayout.PropertyField(Master, new GUIContent("Master", "Which MaterialItem Index is the Master"));

                                            if (Master.intValue >= materialList.arraySize)
                                            {
                                                Master.intValue = materialList.arraySize - 1;
                                            }
                                        }
                                        EditorGUIUtility.labelWidth = 0;
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginVertical();
                                    {
                                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnMaterialChanged"), new GUIContent("On Material Changed", "Invoked when a material item index changes"));
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Material Changer");
            }
            serializedObject.ApplyModifiedProperties();
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_01 = new Rect(rect.x+14, rect.y, 35, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 14 + 25, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 35 +((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 25, EditorGUIUtility.singleLineHeight);
            showMeshesList.boolValue = EditorGUI.ToggleLeft(R_0, new GUIContent("", "Show the Material Items when Selected"), showMeshesList.boolValue);

            EditorGUI.LabelField(R_01,new GUIContent (" #","Index"), EditorStyles.miniLabel);
            EditorGUI.LabelField(R_1, "Material Items", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "CURRENT", EditorStyles.centeredGreyMiniLabel);
            Rect R_3 = new Rect(rect.width+5, rect.y+1, 20, EditorGUIUtility.singleLineHeight-2);

            Rect R_4 = new Rect(rect.width-25, rect.y+1, 30, EditorGUIUtility.singleLineHeight-2);
            random.boolValue                =  GUI.Toggle(R_3, random.boolValue, new GUIContent( "R","On Start Assigns a Random Material"), EditorStyles.miniButton);
            changeHidden.boolValue                =  GUI.Toggle(R_4, changeHidden.boolValue, new GUIContent( "CH","Change Material on Hidden Objects"), EditorStyles.miniButton);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = materialList.GetArrayElementAtIndex(index);
            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, (rect.width - 65) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 25, rect.y, (rect.width - 65) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 25 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 8, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_0, "(" + index.ToString() + ")", EditorStyles.label);

            var nam = element.FindPropertyRelative("Name");

            nam.stringValue = EditorGUI.TextField(R_1, nam.stringValue, EditorStyles.label);
            string buttonCap = "None";

            var e = M.materialList[index];

            if (e.mesh != null)
            {
                EditorGUI.BeginDisabledGroup(!changeHidden.boolValue && !e.mesh.gameObject.activeSelf || e.materials.Length == 0 || e.Linked);
                {
                    if (e.materials.Length > e.current)
                    {
                        buttonCap = /*e.mesh.gameObject.activeSelf ? */
                            (e.materials[e.current] == null ? "None" : e.materials[e.current].name) + " (" + (e.Linked ? "L" : e.current.ToString()) + ")";//: "Is Hidden";
                    }

                    if (GUI.Button(R_2, buttonCap, EditorStyles.miniButton))
                    {
                        ToggleButton(index);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        void ToggleButton(int index)
        {
            if (M.materialList[index].mesh != null)
            {
                Undo.RecordObject(target, "Change Material");
                Undo.RecordObject(M.materialList[index].mesh, "Change Material");

                M.materialList[index].ChangeMaterial();

                //Check for linked Mateeriials

                foreach (var mat in M.materialList)
                {
                    if (mat.Linked && mat.Master >= 0 && mat.Master < M.materialList.Count)
                    {
                        mat.ChangeMaterial(M.materialList[mat.Master].current);
                    }
                }

               // EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (M.materialList == null)
            {
                M.materialList = new System.Collections.Generic.List<MaterialItem>();
            }
            M.materialList.Add(new MaterialItem());
        }
    }
}