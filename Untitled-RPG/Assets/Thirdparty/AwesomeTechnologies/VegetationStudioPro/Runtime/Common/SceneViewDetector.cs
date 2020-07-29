using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;


namespace AwesomeTechnologies.Utility
{
    [InitializeOnLoad]
    public class SceneViewDetector : MonoBehaviour
    {
        private static EditorWindow _currentEditorWindow;
        private static SceneView _currentSceneView;

        public delegate void MultihangedSceneViewCameraDelegate(Camera sceneviewCamera);
        public static MultihangedSceneViewCameraDelegate OnChangedSceneViewCameraDelegate;

        public delegate void MultiSceneViewTransformChangeDelegate(Camera sceneviewCamera);
        public static MultiSceneViewTransformChangeDelegate OnSceneViewTransformChangeDelegate;

        private static Quaternion _rotation;
        private static Vector3 _position;
        static SceneViewDetector()
        {
            EditorApplication.update += UpdateEditorCallback;
        }

        private static void UpdateEditorCallback()
        {        
            if (_currentSceneView && _currentSceneView.camera)
            {             
                if (_rotation != _currentSceneView.camera.transform.rotation ||
                    _position != _currentSceneView.camera.transform.position)
                {
                   
                    _rotation = _currentSceneView.camera.transform.rotation;
                    _position = _currentSceneView.camera.transform.position;
                    if (OnSceneViewTransformChangeDelegate != null)
                    {
                        OnSceneViewTransformChangeDelegate(_currentSceneView.camera);
                    }
                }
            }

            if (_currentEditorWindow == EditorWindow.focusedWindow) return;

            _currentEditorWindow = EditorWindow.focusedWindow;
            var view = _currentEditorWindow as SceneView;
            if (view != null)
            {
                if (_currentSceneView != view)
                {
                    _currentSceneView = view;
                    if (OnChangedSceneViewCameraDelegate != null)
                    {
                        OnChangedSceneViewCameraDelegate(_currentSceneView.camera);
                    }
                }              
            }
        }

        public static Camera GetCurrentSceneViewCamera()
        {
            if (_currentSceneView != null)
            {
                return _currentSceneView.camera;
            }

            Camera[] sceneviewCameras = SceneView.GetAllSceneCameras();
            return sceneviewCameras.Length > 0 ? sceneviewCameras[0] : null;
        }


        void DisableEditorApi()
        {
            EditorApplication.update -= UpdateEditorCallback;
        }
    }
}
#endif
