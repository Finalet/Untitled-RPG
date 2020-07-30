// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Shot00", menuName = "Crest/Demo/Shot Load Scene", order = 10000)]
public class DemoShotLoadScene : DemoShot
{
    public string _sceneName = "Island-Main-Sunset";
    public LoadSceneMode _sceneLoadMode = LoadSceneMode.Single;

    public Collider[] _disableList = new Collider[] { };

    public override void OnPlay()
    {
        foreach (var comp in _disableList)
        {
            comp.isTrigger = true;
        }

        Scene loadedLevel = SceneManager.GetSceneByName(_sceneName);
        if (!loadedLevel.isLoaded)
        {
            SceneManager.LoadScene(_sceneName, _sceneLoadMode);
        }
    }
}
