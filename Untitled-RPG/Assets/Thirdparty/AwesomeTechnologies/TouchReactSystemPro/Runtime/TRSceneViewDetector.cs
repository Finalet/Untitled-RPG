using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;


namespace AwesomeTechnologies.TouchReact
{
    [InitializeOnLoad]
    public class TrSceneViewDetector : MonoBehaviour
    {
        private static EditorWindow _currentEditorWindow;
        private static SceneView _currentSceneView;

        public delegate void MultiVegetationCellRefreshDelegate(Camera sceneviewCamera);
        public static MultiVegetationCellRefreshDelegate OnChangedSceneViewCameraDelegate;

        static TrSceneViewDetector()
        {
            EditorApplication.update += UpdateEditorCallback;
        }

        private static void UpdateEditorCallback()
        {
            if (_currentEditorWindow == EditorWindow.focusedWindow) return;

            _currentEditorWindow = EditorWindow.focusedWindow;
            var view = _currentEditorWindow as SceneView;
            if (view != null)
            {
                if (_currentSceneView != view)
                {
                    _currentSceneView = view;
                    OnChangedSceneViewCameraDelegate?.Invoke(_currentSceneView.camera);
                }
            }
        }

        public static Camera GetCurrentSceneViewCamera()
        {
            if (_currentSceneView != null)
            {
                //Debug.Log("returning current sceneview camera");

                return _currentSceneView.camera;
            }

            Camera[] sceneviewCameras = SceneView.GetAllSceneCameras();
            return sceneviewCameras.Length > 0 ? sceneviewCameras[0] : null;
        }


        // ReSharper disable once UnusedMember.Local
        void DisableEditorApi()
        {
            // ReSharper disable once DelegateSubtraction
            EditorApplication.update -= UpdateEditorCallback;
        }
    }
}
#endif
