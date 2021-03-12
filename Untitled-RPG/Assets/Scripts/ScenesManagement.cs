﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenesManagement : MonoBehaviour
{
    public static ScenesManagement instance;


    public bool isLoading;

    public GameObject loadingScreen;

    string currentScene;
    string playerScene = "Player_Object";

    public List<GameObject> GameobjectsToEnableOnLoad = new List<GameObject>();
    public List<GameObject> GameobjectsToDisableOnUnload = new List<GameObject>();

    void Awake() {
        instance = this;
        loadingScreen.SetActive(false);
    }

    public void LoadLevel(string worldScene, Vector3 playerPos) {
        StartCoroutine(Loading(worldScene, playerPos));
    }
    public void LoadMenu () {
        StartCoroutine(LoadingMenu());
    }

    IEnumerator LoadingMenu () {
        string currentSceneName = currentScene == "" || currentScene == null ? SceneManager.GetActiveScene().name : currentScene;

        //Turn off all directional lights
        Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
        foreach(Light light in lights)
        {
            if (light.type != LightType.Directional)
                continue;
            light.gameObject.SetActive(false);
        }

        //Disable objects 
        foreach (GameObject go in GameobjectsToDisableOnUnload) {
            go.SetActive(false);
        }
        GameobjectsToDisableOnUnload.Clear();

        //StartLoadingMenu
        loadingScreen.SetActive(true);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("SceneManagement"));
        isLoading = true;
        AsyncOperation unloadPlayer = SceneManager.UnloadSceneAsync(playerScene);
        yield return unloadPlayer;
        yield return new WaitForSeconds(1);
        AsyncOperation unloadCurrentScene = SceneManager.UnloadSceneAsync(currentSceneName);
        yield return unloadCurrentScene;
        yield return new WaitForSeconds(1);
        AsyncOperation loadMenu = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        yield return loadMenu;
        yield return new WaitForSeconds(1);
        isLoading = false;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
        loadingScreen.SetActive(false);
        foreach (GameObject go in GameobjectsToEnableOnLoad) {
            go.SetActive(true);
        }
        GameobjectsToEnableOnLoad.Clear();
        currentScene = "";
    }

    IEnumerator Loading (string worldScene, Vector3 playerPos) {
        //Disable objects 
        foreach (GameObject go in GameobjectsToDisableOnUnload) {
            go.SetActive(false);
        }
        GameobjectsToDisableOnUnload.Clear();

        loadingScreen.SetActive(true);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("SceneManagement"));
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
        PlayerControlls.instance.transform.position = playerPos;
        isLoading = false;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(worldScene));
        loadingScreen.SetActive(false);
        foreach (GameObject go in GameobjectsToEnableOnLoad) {
            go.SetActive(true);
        }
        GameobjectsToEnableOnLoad.Clear();
        currentScene = worldScene;
    }
}
