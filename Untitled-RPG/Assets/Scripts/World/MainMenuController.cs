using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake() {
        AsyncOperation addLoadingScene = SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
    }

    public void LoadLevel (string levelName) {
        ScenesManagement.instance.LoadLevel(levelName);
    }

    public void ExitGame () {
        Application.Quit();
    }
}
