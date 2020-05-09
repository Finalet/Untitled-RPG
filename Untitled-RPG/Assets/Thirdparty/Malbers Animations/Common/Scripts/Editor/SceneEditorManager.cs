using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalbersAnimations
{
    [InitializeOnLoad]
    public class SceneEditorManager : MonoBehaviour
    {
        // constructor
        static SceneEditorManager()
        {
            EditorSceneManager.sceneOpened += SceneOpenedCallback;
        }

        static void SceneOpenedCallback(Scene _scene, UnityEditor.SceneManagement.OpenSceneMode _mode)
        {
            var allGO = _scene.GetRootGameObjects();

            foreach (var go in allGO)
            {
                var iscene = go.GetComponent<IScene>();

                if (iscene != null)
                {
                    iscene.SceneLoaded();
                    break;
                }
            }
        }
    }
}