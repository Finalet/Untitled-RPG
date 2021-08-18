using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Managers/Game Settings <Simple>")] 
    public class MGameSettings : MonoBehaviour, IScene
    {
        public bool HideCursor = false;
        public bool ForceFPS = false;

        [Hide("ForceFPS",true,false)]
        public int GameFPS = 60;

#if UNITY_EDITOR
        [Space,Tooltip("The Scene must be added to the Build Settings!!!")]
        public List<UnityEditor.SceneAsset> AdditiveScenes;
#endif
        [Tooltip("Add the Additive scene in the Editor")]
        public bool InEditor = true;
       [HideInInspector] public List<string> sceneNames;

        void Awake()
        {
            DontDestroyOnLoad(this);

            UnityUtils.ShowCursor(!HideCursor);

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = ForceFPS ? GameFPS : -1;

            if (sceneNames != null && !InEditor)
            {
                foreach (var scene in sceneNames)
                {
                    SceneManager.LoadScene(scene, LoadSceneMode.Additive);
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (AdditiveScenes != null)
            {
                sceneNames = new List<string>();

                foreach (var s in AdditiveScenes)
                  if (s != null)
                        sceneNames.Add(s.name);
            }
        }
#endif

        [ContextMenu("Add Additive Scene")]
        public void SceneLoaded()
        {
#if UNITY_EDITOR
            if (AdditiveScenes != null && InEditor)
            {

                foreach (var item in AdditiveScenes)
                {
                    var scenePath = UnityEditor.AssetDatabase.GetAssetPath(item);
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                }
            }
#endif
        }
    }
}