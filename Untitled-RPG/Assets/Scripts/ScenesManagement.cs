using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenesManagement : MonoBehaviour
{
    public Image progessBar; 
    public GameObject hideOnLoaded;

    void Start() {
        StartCoroutine(LoadGame());
        DontDestroyOnLoad(gameObject);
    }

    bool one;
    IEnumerator LoadGame () {
        AsyncOperation outdoors = SceneManager.LoadSceneAsync("Outdoors", LoadSceneMode.Additive);
        while(!outdoors.isDone) {
            progessBar.fillAmount = outdoors.progress/2f;
            yield return null;
        }
        AsyncOperation player_object = SceneManager.LoadSceneAsync("Player_Object", LoadSceneMode.Additive);
        while(!player_object.isDone) {
            progessBar.fillAmount = 0.5f + player_object.progress;
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Outdoors"));
        hideOnLoaded.SetActive(false);
    }
}
