using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    [CustomPropertyDrawer(typeof(FPD_OverridableFloatAttribute))]
    public class FPD_OverridableFloat : PropertyDrawer
    {
        FPD_OverridableFloatAttribute Attribute { get { return ((FPD_OverridableFloatAttribute)base.attribute); } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var boolProp = property.serializedObject.FindProperty(Attribute.BoolVarName);
            var valProp = property.serializedObject.FindProperty(Attribute.TargetVarName);

            Color disabled = new Color(0.8f, 0.8f, 0.8f, 0.6f);
            Color preCol = GUI.color;
            if (!boolProp.boolValue) GUI.color = disabled; else GUI.color = preCol;

            EditorGUI.BeginProperty(position, label, property);

            var boolRect = new Rect(position.x, position.y, Attribute.LabelWidth + 15f, position.height);

            EditorGUIUtility.labelWidth = Attribute.LabelWidth;
            EditorGUI.PropertyField(boolRect, boolProp);

            EditorGUIUtility.labelWidth = 14;
            var valRect = new Rect(position.x + Attribute.LabelWidth + 15, position.y, position.width - (Attribute.LabelWidth + 15), position.height);
            EditorGUI.PropertyField(valRect, valProp, new GUIContent(" "));

            EditorGUIUtility.labelWidth = 0;

            GUI.color = preCol;
            EditorGUI.EndProperty();
        }
    }



    // -------------------------- Next F Property Drawer -------------------------- \\



    [CustomPropertyDrawer(typeof(BackgroundColorAttribute))]
    public class BackgroundColorDecorator : DecoratorDrawer
    {
        BackgroundColorAttribute Attribute { get { return ((BackgroundColorAttribute)base.attribute); } }
        public override float GetHeight() { return 0; }

        public override void OnGUI(Rect position)
        {
            GUI.backgroundColor = Attribute.Color;
        }
    }


    // -------------------------- Next F Property Drawer -------------------------- \\


    [CustomPropertyDrawer(typeof(FPD_WidthAttribute))]
    public class FPD_Width : PropertyDrawer
    {
        FPD_WidthAttribute Attribute { get { return ((FPD_WidthAttribute)base.attribute); } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUIUtility.labelWidth = Attribute.LabelWidth;
            EditorGUI.PropertyField(position, property);
            EditorGUIUtility.labelWidth = 0;
        }
    }

    // -------------------------- Next F Property Drawer -------------------------- \\

    [CustomPropertyDrawer(typeof(FPD_IndentAttribute))]
    public class FPD_Indent : PropertyDrawer
    {
        FPD_IndentAttribute Attribute { get { return ((FPD_IndentAttribute)base.attribute); } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUIUtility.labelWidth = Attribute.LabelsWidth;
            for (int i = 0; i < Attribute.IndentCount; i++) EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property);
            for (int i = 0; i < Attribute.IndentCount; i++) EditorGUI.indentLevel--;
            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(Attribute.SpaceAfter);
        }
    }

    // -------------------------- Next F Property Drawer -------------------------- \\

    [CustomPropertyDrawer(typeof(FPD_HorizontalLineAttribute))]
    public class FPD_HorizontalLine : PropertyDrawer
    {
        FPD_HorizontalLineAttribute Attribute { get { return ((FPD_HorizontalLineAttribute)base.attribute); } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FGUI_Inspector.DrawUILine(Attribute.color);
        }
    }
}

