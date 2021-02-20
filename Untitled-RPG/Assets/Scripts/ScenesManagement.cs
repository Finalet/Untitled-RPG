using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenesManagement : MonoBehaviour
{
    public static ScenesManagement instance;
    public bool isLoading;

    public GameObject loadingScreen;
    string playerScene = "Player_Object_Rigidbody";

    public List<GameObject> goToEnableOnLoad = new List<GameObject>();

    void Awake() {
        instance = this;
        loadingScreen.SetActive(false);
    }

    public void LoadLevel(string worldScene) {
        StartCoroutine(Loading(worldScene));
    }

    IEnumerator Loading (string worldScene) {
        loadingScreen.SetActive(true);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Loading"));
        isLoading = true;
        AsyncOperation unloadMenu = SceneManager.UnloadSceneAsync("MainMenu");
        yield return unloadMenu;
        yield return new WaitForSeconds(1);
        AsyncOperation loadingMap = SceneManager.LoadSceneAsync(worldScene, LoadSceneMode.Additive);
        yield return loadingMap;
        yield return new WaitForSeconds(1);
        AsyncOperation loadingPlayer = SceneManager.LoadSceneAsync(playerScene, LoadSceneMode.Additive);
        yield return loadingPlayer;
        yield return new WaitForSeconds(1);
        isLoading = false;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(worldScene));
        loadingScreen.SetActive(false);
        foreach (GameObject go in goToEnableOnLoad) {
            go.SetActive(true);
        }
    }
}
