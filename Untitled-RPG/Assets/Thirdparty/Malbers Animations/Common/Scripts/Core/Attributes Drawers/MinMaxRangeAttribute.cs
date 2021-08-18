using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    public class MinMaxRangeAttribute : Attribute
    {
        public MinMaxRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; private set; }
        public float Max { get; private set; }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public sealed class LineAttribute : Attribute
    {
        public readonly float height;

        public LineAttribute()
        {
            // By default uses 8 pixels which corresponds to EditorGUILayout.Space()
            // which reserves 6 pixels, plus the usual 2 pixels caused by the neighboring margin.
            // (Why not 2 pixels for margin both below and above?
            // Because one of those is already accounted for when the space is not there.)
            this.height = 8;
        }

        public LineAttribute(float height) { this.height = height; }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RangedFloat), true)]
    public class RangedFloatDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty minProp = property.FindPropertyRelative("minValue");
            SerializedProperty maxProp = property.FindPropertyRelative("maxValue");


            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float rangeMin = 0;
            float rangeMax = 1;

            var ranges = (MinMaxRangeAttribute[])fieldInfo.GetCustomAttributes(typeof(MinMaxRangeAttribute), true);
            if (ranges.Length > 0)
            {
                rangeMin = ranges[0].Min;
                rangeMax = ranges[0].Max;
            }

            const float rangeBoundsLabelWidth = 40;

            var minRect = new Rect(position);
            minRect.width = rangeBoundsLabelWidth;

            minProp.floatValue = EditorGUI.FloatField(minRect, GUIContent.none, minProp.floatValue);

            // GUI.Label(rangeBoundsLabel1Rect, new GUIContent(minValue.ToString("F2")));
            position.xMin += rangeBoundsLabelWidth + 3;

            var rangeBoundsLabel2Rect = new Rect(position);
            rangeBoundsLabel2Rect.xMin = rangeBoundsLabel2Rect.xMax - rangeBoundsLabelWidth;
            maxProp.floatValue = EditorGUI.FloatField(rangeBoundsLabel2Rect, GUIContent.none, maxProp.floatValue);

            float minValue = minProp.floatValue;
            float maxValue = maxProp.floatValue;


            // GUI.Label(rangeBoundsLabel2Rect, new GUIContent(maxValue.ToString("F2")));
            position.xMax -= rangeBoundsLabelWidth + 3;

            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, rangeMin, rangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                minProp.floatValue = (float)Mathf.Round(minValue * 100f) / 100f; ;
                maxProp.floatValue = (float)Mathf.Round(maxValue * 100f) / 100f; ;
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
#endif
}