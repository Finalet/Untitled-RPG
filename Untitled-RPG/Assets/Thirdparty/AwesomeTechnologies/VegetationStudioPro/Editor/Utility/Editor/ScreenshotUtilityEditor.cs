using UnityEditor;
using UnityEngine;


namespace AwesomeTechnologies.Utility
{
    [CustomEditor(typeof(ScreenshotUtility))]
    public class ScreenshotUtilityEditor : Editor
    {
        private ScreenshotUtility _screenshotUtility;
        public override void OnInspectorGUI()
        {
            _screenshotUtility = (ScreenshotUtility) target;

            if (GUILayout.Button("Take screenshot"))
            {
                _screenshotUtility.TakeScreenshot();
            }
        }
    }
}
