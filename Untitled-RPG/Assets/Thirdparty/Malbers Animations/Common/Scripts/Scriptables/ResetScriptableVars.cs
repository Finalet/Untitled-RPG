using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif
namespace MalbersAnimations.Scriptables
{
    [AddComponentMenu("Malbers/Variables/Reset Scriptable Vars")]

    public class ResetScriptableVars : MonoBehaviour
    {
        public bool ResetOnEnable = true;
        public bool ResetOnDisable = false;
        public List<ScriptableVarReseter> vars;


        // Use this for initialization
        void OnEnable() { if (ResetOnEnable) ResetVars(); }

        void OnDisable() { if (ResetOnDisable) ResetVars(); }


        public virtual void ResetVars()
        { foreach (var v in vars) v.Reset(); }
    }
    
    [System.Serializable]
    public struct ScriptableVarReseter
    {
        public ScriptableVar Var;
        public BoolReference DefaultBool;
        public IntReference DefaultInt;
        public FloatReference DefaultFloat;
        public StringReference DefaultString;
        public Vector2Reference DefaultVector2;
        public Vector3Reference DefaultVector3;
        public ColorReference DefaultColor;
        public TransformReference DefaultTransform;
        public GameObjectReference DefaultGO;

        public void Reset()
        {
            if (Var is IntVar) (Var as IntVar).Value = DefaultInt.Value;
            else if (Var is BoolVar) (Var as BoolVar).Value = DefaultBool.Value;
            else if (Var is FloatVar) (Var as FloatVar).Value = DefaultFloat.Value;
            else if (Var is StringVar) (Var as StringVar).Value = DefaultString.Value;
            else if (Var is Vector3Var) (Var as Vector3Var).Value = DefaultVector3.Value;
            else if (Var is Vector2Var)  (Var as Vector2Var).Value = DefaultVector2.Value;
            else if (Var is ColorVar)     (Var as ColorVar).Value = DefaultColor.Value;
            else if (Var is TransformVar) (Var as TransformVar).Value = DefaultTransform.Value;
            else if (Var is GameObjectVar) (Var as GameObjectVar).Value = DefaultGO.Value;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ResetScriptableVars))]
    public class ResetScriptableVarsEditor : Editor
    {
        private ReorderableList Reo_ScriptVars;
        private MonoScript script;
        private SerializedProperty vars, ResetOnDisable, ResetOnEnable;
        private ResetScriptableVars m;

        /// <summary>  Options to display in the popup to select constant or variable. </summary>
        private readonly string[] popupOptions = { "Use Constant", "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        private void OnEnable()
        {
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            m = (ResetScriptableVars)target;
            vars = serializedObject.FindProperty("vars");
            ResetOnEnable = serializedObject.FindProperty("ResetOnEnable");
            ResetOnDisable = serializedObject.FindProperty("ResetOnDisable");


            Reo_ScriptVars = new ReorderableList(serializedObject, vars, true, true, true, true)
            {
                drawElementCallback = DrawElement_Pivots,
                onAddCallback = OnAddCallback_Pivots,
                drawHeaderCallback = DrawHeaderCallback_Pivots,
                onRemoveCallback = OnRemoveCallback,
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
               // MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginHorizontal();
                {
                    ResetOnEnable.boolValue =  GUILayout.Toggle(ResetOnEnable.boolValue, 
                        new GUIContent("Reset on Enable", "Reset the values when this Script is Enabled"), EditorStyles.miniButton);

                    ResetOnDisable.boolValue = GUILayout.Toggle(ResetOnDisable.boolValue, 
                        new GUIContent("Reset on Disable", "Reset the values when this Script is Disabled, and when the Play button is Off (in the Editor)"), EditorStyles.miniButton);

                }
                EditorGUILayout.EndHorizontal();
                    Reo_ScriptVars.DoLayoutList();
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderCallback_Pivots(Rect rect)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var scriptVat = new Rect(rect.x, rect.y, rect.width / 2, height);
            var value = new Rect(rect.width / 2 + 50, rect.y, rect.width / 2 - 50, height);

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
            var RectValue = new Rect(rect.width / 2 + 65, rect.y, rect.width / 2 - 23, height);

            var Element = vars.GetArrayElementAtIndex(index);
            var ScriptVar = Element.FindPropertyRelative("Var");


            EditorGUI.PropertyField(RectVar, ScriptVar, GUIContent.none);
            var Var = ScriptVar.objectReferenceValue;

            if (popupStyle == null) popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions")) { imagePosition = ImagePosition.ImageOnly };

            // Calculate rect for configuration button
            Rect buttonRect = new Rect(RectValue);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            buttonRect.x -= 20;
            buttonRect.height = height;

            if (Var is IntVar)
            { 
                var ele = Element.FindPropertyRelative("DefaultInt");

                var useConstant = ele.FindPropertyRelative("UseConstant");
                var constantValue = ele.FindPropertyRelative("ConstantValue");
                var variable = ele.FindPropertyRelative("Variable");

                int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);
                useConstant.boolValue = (result == 0);


                EditorGUI.PropertyField(RectValue, useConstant.boolValue ? constantValue : variable, GUIContent.none, false);
                //EditorGUI.PropertyField(RectValue, useConstant, GUIContent.none);

            }
            else if (Var is BoolVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultBool"), GUIContent.none);
            else if (Var is FloatVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultFloat"), GUIContent.none);
            else if (Var is StringVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultString"), GUIContent.none);
            else if (Var is Vector3Var) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultVector3"), GUIContent.none);
            else if (Var is Vector2Var) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultVector2"), GUIContent.none);
            else if (Var is ColorVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultColor"), GUIContent.none);
            else if (Var is TransformVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultTransform"), GUIContent.none);
            else if (Var is GameObjectVar) EditorGUI.PropertyField(RectValue, Element.FindPropertyRelative("DefaultGO"), GUIContent.none);
        }
    }
#endif
}