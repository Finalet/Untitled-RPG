using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(ActiveMeshes))]
    public class ActiveMeshesEditor : Editor
    {
        private ReorderableList list;
        SerializedProperty activeMeshesList, showMeshesList, random;
        private ActiveMeshes m;

        private void OnEnable()
        {
            m = (ActiveMeshes)target;

            activeMeshesList = serializedObject.FindProperty("Meshes");
            showMeshesList = serializedObject.FindProperty("showMeshesList");
            random = serializedObject.FindProperty("random");

            list = new ReorderableList(serializedObject, activeMeshesList, true, true, true, true);
            {
                list.drawElementCallback = DrawElementCallback;
                list.drawHeaderCallback = HeaderCallbackDelegate;
                list.onAddCallback = OnAddCallBack;
            }
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Toggle || Swap Meshes");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                    list.DoLayoutList();
                    if (showMeshesList.boolValue)
                    {
                        if (list.index != -1)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            {
                                SerializedProperty Element = activeMeshesList.GetArrayElementAtIndex(list.index);
                                MalbersEditor.Arrays(Element);
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                }
                EditorGUILayout.EndVertical();

            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Active Meshes Inspector");
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>Reordable List Header </summary>
        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_01 = new Rect(rect.x + 14, rect.y, 35, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 14 + 25, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x  + 35 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 25, EditorGUIUtility.singleLineHeight);

            showMeshesList.boolValue  = EditorGUI.ToggleLeft(R_0,"", showMeshesList.boolValue);
            EditorGUI.LabelField(R_01, new GUIContent(" #", "Index"), EditorStyles.miniLabel);
            EditorGUI.LabelField(R_1, "Active Meshes", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "CURRENT", EditorStyles.centeredGreyMiniLabel);

            Rect R_3 = new Rect(rect.width + 5, rect.y + 1, 20, EditorGUIUtility.singleLineHeight - 2);
            random.boolValue= GUI.Toggle(R_3, random.boolValue , new GUIContent("R", "On Start Assigns a Random Mesh"), EditorStyles.miniButton);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = activeMeshesList.GetArrayElementAtIndex(index);
            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, (rect.width - 65) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 25, rect.y, (rect.width - 65) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 25 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 8, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_0, "(" + index.ToString() + ")", EditorStyles.label);

            var eleName = element.FindPropertyRelative("Name");
            var meshes = element.FindPropertyRelative("meshes");
            var Current = element.FindPropertyRelative("Current").intValue;

            eleName.stringValue = EditorGUI.TextField(R_1, eleName.stringValue, EditorStyles.label);

            string ButtonName = "Empty";

            //var e = m.Meshes[index];
            //if (e.meshes != null && e.meshes.Length >Current)
            //{
            //    ButtonName = (e.meshes[Current] == null ? "Empty" : e.meshes[Current].name) + " (" + Current + ")";
            //}


            if (meshes.arraySize > Current)
            {
                var CurrentMesh = meshes.GetArrayElementAtIndex(Current);

                ButtonName = CurrentMesh.objectReferenceValue == null ? "Empty" : CurrentMesh.objectReferenceValue .name + " (" + Current + ")";
            }
            

            if (GUI.Button(R_2, ButtonName,EditorStyles.miniButton))
            {
                Undo.RecordObject(target, "Changed Mesh ");

                foreach (var item in m.Meshes[index].meshes)
                {
                   if (item) Undo.RecordObject(item.gameObject, "Changed Mesh ");
                }

                ToggleButton(index);
                serializedObject.ApplyModifiedProperties();
            }
        }

        void ToggleButton(int index)
        {
            var activeMesh = m.Meshes[index];
            
            if (activeMesh.meshes != null && activeMesh.meshes.Length > 0)
                activeMesh.ChangeMesh();
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (m.Meshes == null)
                m.Meshes = new List<ActiveSMesh>();

            m.Meshes.Add(new ActiveSMesh()); 
        }
    }
}
