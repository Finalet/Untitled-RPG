using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Common
{
    public class EditorFunctions
    {
        public static void FloatRangeField(string label,ref float minValue, ref float maxValue, float minLimit, float maxLimit)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(196));
            minValue = EditorGUILayout.FloatField(minValue);
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
            maxValue = EditorGUILayout.FloatField(maxValue);

            if (minValue < minLimit) minValue = minLimit;
            if (maxValue > maxLimit) maxValue = maxLimit;
            if (maxValue > maxLimit - 0.02f) maxValue = maxLimit;


            GUILayout.EndHorizontal(); 
        }

        public static string DrawTagDropdown(string tag)
        {
            List<string> tagList = new List<string>();        
            tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);
            int selectedIndex = tagList.IndexOf(tag);
            if (selectedIndex < 0) selectedIndex = 0;                              
                selectedIndex = EditorGUILayout.Popup("Select tag", selectedIndex, UnityEditorInternal.InternalEditorUtility.tags);
            return tagList[selectedIndex];
        }
    }
}
