using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    public Image blackout;
    // Start is called before the first frame update
    void Awake() {
        if (SceneManager.GetActiveScene().name != "SceneManagement") {
            AsyncOperation addLoadingScene = SceneManager.LoadSceneAsync("SceneManagement", LoadSceneMode.Additive);
        }
    }
    void Start() {
        blackout.color = Color.black;
        blackout.DOFade(0, 1).SetDelay(1);
    }

    public void LoadLevel (string levelName) {
        StartCoroutine(loadLevel(levelName));
    }
    IEnumerator loadLevel (string levelName) {
        blackout.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        Quaternion playerRot;
        switch (levelName) {
            case "City":
                playerRot = Quaternion.identity;
                break;
            default:
                playerRot = Quaternion.identity;
                break;
        }
        ScenesManagement.instance.LoadLevel(levelName);
    }

    public void ExitGame () {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
