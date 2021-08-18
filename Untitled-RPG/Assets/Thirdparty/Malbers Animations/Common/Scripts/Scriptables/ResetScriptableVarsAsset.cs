using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif



namespace MalbersAnimations.Scriptables
{
    [CreateAssetMenu(menuName = "Malbers Animations/Collections/Scriptable Variables Set", order = 1000)]
    public class ResetScriptableVarsAsset : ScriptableObject
    {
        public List<ScriptableVarReseter> vars;
        public virtual void Restart()    { foreach (var v in vars) v.Reset(); }
    }
    
  

#if UNITY_EDITOR
    [CustomEditor(typeof(ResetScriptableVarsAsset))]
    public class ResetScriptableVarsAssetEd : Editor
    {
        private ReorderableList Reo_ScriptVars;
        private SerializedProperty vars;
        private ResetScriptableVarsAsset m;

        /// <summary>  Options to display in the popup to select constant or variable. </summary>
        private readonly string[] popupOptions = { "Use Constant", "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        void OnEnable()
        {
            m = (ResetScriptableVarsAsset)target;
            vars = serializedObject.FindProperty("vars");


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

            if (list.index == -1 && vars.arraySize > 0) list.index = 0; //In Case you remove the first one

            EditorUtility.SetDirty(m);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Reset Scriptable Variables");
            Reo_ScriptVars.DoLayoutList();
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