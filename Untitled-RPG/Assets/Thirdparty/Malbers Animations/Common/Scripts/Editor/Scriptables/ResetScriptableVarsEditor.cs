using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace MalbersAnimations.Scriptables
{
    [CustomEditor(typeof(ResetScriptableVars))]
    public class ResetScriptableVarsEditor : Editor
    {
        private ReorderableList Reo_ScriptVars;
        private MonoScript script;
        private SerializedProperty vars;
        private ResetScriptableVars m;

        private void OnEnable()
        {
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            m = (ResetScriptableVars)target;
            vars = serializedObject.FindProperty("vars");


            Reo_ScriptVars = new ReorderableList(serializedObject, vars, true, true, true, true)
            {
                drawElementCallback = DrawElement_Pivots,
                onAddCallback = OnAddCallback_Pivots,
                drawHeaderCallback = DrawHeaderCallback_Pivots,
                onRemoveCallback  = OnRemoveCallback,
            };
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            vars.DeleteArrayElementAtIndex(list.index);
            list.index -= 1;

            if (list.index == -1 && vars.arraySize > 0)  //In Case you remove the first one
            {
                list.index = 0;
            }

            EditorUtility.SetDirty(m);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Reset Scriptable Variables");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetOnEnable"), new GUIContent("Reset on Enable", "Reset the values when this Script is Enabled"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetOnDisable"), new GUIContent("Reset on Disable", "Reset the values when this Script is Disabled, and when the Play button is Off (in the Editor)"));
                    Reo_ScriptVars.DoLayoutList();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
         
        private void DrawHeaderCallback_Pivots(Rect rect)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var scriptVat = new Rect(rect.x, rect.y, rect.width / 2, height);
            var value = new Rect(rect.width / 2 + 50, rect.y, rect.width / 2 -50,  height);

            EditorGUI.LabelField(scriptVat, "   Scriptable Variable");
            EditorGUI.LabelField(value, "  Reset Value");
        }

        private void OnAddCallback_Pivots(ReorderableList list)
        {
            if (m.vars == null) m.vars = new List<ScriptableVarReseter>();

            m.vars.Add(new ScriptableVarReseter());

            EditorUtility.SetDirty(m);
        }

        private void DrawElement_Pivots(Rect rect, int index, bool isActive, bool isFocused)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var RectVar = new Rect(rect.x, rect.y, rect.width / 2, height);
            var RectValue = new Rect(rect.width / 2 + 65 , rect.y, rect.width / 2 - 23, height);

            var Element = vars.GetArrayElementAtIndex(index);
            var ScriptVar = Element.FindPropertyRelative("Var");


            EditorGUI.PropertyField(RectVar, ScriptVar, GUIContent.none);
            var Var = ScriptVar.objectReferenceValue;

            if (Var is IntVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultInt"), GUIContent.none);
            else if (Var is BoolVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultBool"), GUIContent.none);
            else if (Var is FloatVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultFloat"), GUIContent.none);  
            else if (Var is StringVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultString"), GUIContent.none);  
            else if (Var is Vector3Var) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultVector3"), GUIContent.none); 
            else if (Var is Vector2Var) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultVector2"), GUIContent.none);  
            else if (Var is ColorVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultColor"), GUIContent.none); 
        }
    }
}
