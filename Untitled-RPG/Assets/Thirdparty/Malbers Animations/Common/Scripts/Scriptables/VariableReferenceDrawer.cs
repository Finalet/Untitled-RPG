#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    [CustomPropertyDrawer(typeof(IntReference))]
    [CustomPropertyDrawer(typeof(FloatReference))]
    [CustomPropertyDrawer(typeof(BoolReference))]
    [CustomPropertyDrawer(typeof(StringReference))]
    [CustomPropertyDrawer(typeof(Vector3Reference))]
    [CustomPropertyDrawer(typeof(Vector2Reference))]
    [CustomPropertyDrawer(typeof(ColorReference))]
    [CustomPropertyDrawer(typeof(LayerReference))]
    [CustomPropertyDrawer(typeof(GameObjectReference))]
    [CustomPropertyDrawer(typeof(SpriteReference))]
    [CustomPropertyDrawer(typeof(TransformReference))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        /// <summary>  Options to display in the popup to select constant or variable. </summary>
        private readonly string[] popupOptions =  { "Use Constant", "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;
        private GUIStyle AddStyle;
        private GUIContent plus;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (popupStyle == null)
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions")) { imagePosition = ImagePosition.ImageOnly };

            if (AddStyle == null)
                AddStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions")) { imagePosition = ImagePosition.ImageOnly };

            if (plus == null) plus = UnityEditor.EditorGUIUtility.IconContent("d_Toolbar Plus");

            position.y -= 0;

            label = EditorGUI.BeginProperty(position, label, property);
            Rect variableRect = new Rect(position);
            position = EditorGUI.PrefixLabel(position, label);
            //EditorGUI.BeginChangeCheck();

            float height = EditorGUIUtility.singleLineHeight;

            // Get properties
            SerializedProperty useConstant = property.FindPropertyRelative("UseConstant");
            SerializedProperty constantValue = property.FindPropertyRelative("ConstantValue");
            SerializedProperty variable = property.FindPropertyRelative("Variable");

            Rect prop = new Rect(position) { height = height };
            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            buttonRect.x -= 20;
            buttonRect.height = height;

            position.xMin = buttonRect.xMax;


            var AddButtonRect = new Rect(prop) { x = prop.width + prop.x - 18,  width = 20 };


            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);

            useConstant.boolValue = (result == 0);

            bool varIsEmpty = variable.objectReferenceValue == null;

            if (varIsEmpty && !useConstant.boolValue) { prop.width -= 22; }


#if UNITY_2020_1_OR_NEWER
            EditorGUIUtility.labelWidth = 0.1f;
#endif
            EditorGUI.PropertyField(prop, useConstant.boolValue ? constantValue : variable, GUIContent.none, false);
            EditorGUIUtility.labelWidth = 0;

            if (varIsEmpty && !useConstant.boolValue)
            {
                if (GUI.Button(AddButtonRect, plus, UnityEditor.EditorStyles.helpBox))
                { 
                    MTools.CreateScriptableAsset(variable, MTools.Get_Type( variable), MTools.GetSelectedPathOrFallback());
                }
            }

            //  ShowScriptVar(variableRect, height, useConstant, variable);

            //if (EditorGUI.EndChangeCheck())
            //    property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private static void ShowScriptVar(Rect variableRect, float height, SerializedProperty useConstant, SerializedProperty variable)
        {
            if (!useConstant.boolValue && variable.objectReferenceValue != null)
            {
                variableRect.height = height;
                variableRect.y += height + 2;

                SerializedObject objs = new SerializedObject(variable.objectReferenceValue);
                var Var = objs.FindProperty("value");
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(variableRect, Var, new GUIContent(variable.objectReferenceValue.GetType().Name + " Value"));
                if (EditorGUI.EndChangeCheck())
                {
                    objs.ApplyModifiedProperties();
                    EditorUtility.SetDirty(variable.objectReferenceValue);
                }
            }
        }

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{

        //    float height = base.GetPropertyHeight(property, label);

        //    SerializedProperty useConstant = property.FindPropertyRelative("UseConstant");
        //    if (!useConstant.boolValue)
        //    {
        //        SerializedProperty variable = property.FindPropertyRelative("Variable");
        //        if (variable.objectReferenceValue != null)
        //        height = height * 2 + 8;
        //    }

        //    return height;
        //}
    }
}
#endif