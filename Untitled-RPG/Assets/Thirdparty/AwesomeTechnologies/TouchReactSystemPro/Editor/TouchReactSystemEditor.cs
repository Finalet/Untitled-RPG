using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.TouchReact
{
    [CustomEditor(typeof(TouchReactSystem))]
    public class TouchReactSystemEditor : TouchReactBaseEditor
    {
        private int _currentTabIndex;
        private static readonly string[] TabNames =
        {
            "Settings", "Editor","Debug"
        };

        public override void OnInspectorGUI()
        {
            HelpTopic = "home/vegetation-studio/components/touch-bend-system";
            LargeLogo = true;
            base.OnInspectorGUI();

            _currentTabIndex = GUILayout.SelectionGrid(_currentTabIndex, TabNames, 3, EditorStyles.toolbarButton);

            switch (_currentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawEditorInspector();
                    break;
                case 2:
                    DrawDebugInspector();
                    break;
            }

        }

        void DrawSettingsInspector()
        {
            TouchReactSystem touchReactSystem = (TouchReactSystem)target;
            EditorGUILayout.HelpBox("Touch react system will bend grass and plants in an area around a camera. Only one instance per scene.", MessageType.Info);

            
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", LabelStyle);
            EditorGUI.BeginChangeCheck();
            touchReactSystem.AutoselectCamera = EditorGUILayout.Toggle("Auto select camera", touchReactSystem.AutoselectCamera);            

            if (!touchReactSystem.AutoselectCamera)
            {
                touchReactSystem.SelectedCamera = EditorGUILayout.ObjectField("Camera", touchReactSystem.SelectedCamera, typeof(Camera), true) as Camera;              
            }
            if (EditorGUI.EndChangeCheck())
            {
                touchReactSystem.Init();
                EditorUtility.SetDirty(touchReactSystem);
            }
            if (touchReactSystem.SelectedCamera == null)
            {
                EditorGUILayout.HelpBox("You need to select what camera will controll the vegetation system.", MessageType.Error);
            }

            EditorGUILayout.HelpBox(
                "Select a camera to follow.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            touchReactSystem.InvisibleLayer = EditorGUILayout.IntSlider("Touch React layer", touchReactSystem.InvisibleLayer, 0, 31);
            EditorGUILayout.HelpBox("Select a layer not visible by any other camera. This is used to render a touch buffer for selected colliders and meshes within range.", MessageType.Info);

            touchReactSystem.TouchReactQuality = (TouchReactQuality)EditorGUILayout.EnumPopup("Buffer resolution", touchReactSystem.TouchReactQuality);
            EditorGUILayout.HelpBox("Pixel resolution of the touch react buffer. Low = 512x512, Normal = 1024x1024 and High =2048x2048", MessageType.Info);

            touchReactSystem.OrthographicSize = EditorGUILayout.IntSlider("Affected area (meter)", touchReactSystem.OrthographicSize, 10, 500);
            EditorGUILayout.HelpBox("Area around camera affected by touch react. Increasing range reduces resolution on the mask.", MessageType.Info);

            float resolution = (float)touchReactSystem.OrthographicSize / touchReactSystem.GetTouchReactQualityPixelResolution(touchReactSystem.TouchReactQuality);
            EditorGUILayout.LabelField("Current resolution " + resolution.ToString("F2") + " meter", LabelStyle);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                touchReactSystem.UpdateCamera();
                EditorUtility.SetDirty(touchReactSystem);
            }


          
        }

        void DrawEditorInspector()
        {
            TouchReactSystem touchReactSystem = (TouchReactSystem)target;
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Editor", LabelStyle);
            EditorGUI.BeginChangeCheck();
            touchReactSystem.HideTouchReactCamera = EditorGUILayout.Toggle("Hide TouchReact Camera", touchReactSystem.HideTouchReactCamera);
            EditorGUILayout.HelpBox("When enabled the touch react camera will be hidden in hierarchy", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
            {
                touchReactSystem.UpdateTouchReactCamera();
                EditorUtility.SetDirty(touchReactSystem);
            }
            GUILayout.EndVertical();
        }

        void DrawDebugInspector()
        {
            TouchReactSystem touchReactSystem = (TouchReactSystem)target;
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Debug", LabelStyle);
            touchReactSystem.ShowDebugColliders = EditorGUILayout.Toggle("Show colliders/meshes", touchReactSystem.ShowDebugColliders);
            EditorGUILayout.HelpBox("Show colliders and meshes that affect grass.", MessageType.Info);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(touchReactSystem);
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Colliders", LabelStyle);
            EditorGUILayout.LabelField("Collider count: " + touchReactSystem.ColliderList.Count.ToString(), LabelStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Meshes", LabelStyle);
            EditorGUILayout.LabelField("Mesh count: " + touchReactSystem.MeshFilterList.Count.ToString(), LabelStyle);
            GUILayout.EndVertical();

        }
    }
}