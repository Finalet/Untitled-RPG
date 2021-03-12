using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake() {
        if (SceneManager.GetActiveScene().name != "SceneManagement") {
            AsyncOperation addLoadingScene = SceneManager.LoadSceneAsync("SceneManagement", LoadSceneMode.Additive);
        }
    }

    public void LoadLevel (string levelName) {
        Vector3 playerPos;
        switch (levelName) {
            case "TheBay":
                playerPos = new Vector3(-240,2,240);
                break;
            case "Village":
                playerPos = new Vector3(-60,-1.2f,100);
                break;
            default:
                playerPos = new Vector3(0,1,0);
                break;
        }
        ScenesManagement.instance.LoadLevel(levelName, playerPos);
    }

    public void ExitGame () {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
