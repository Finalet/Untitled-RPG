using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public List<ISavable> saveObjects = new List<ISavable>();

    public void SaveGame() {
        foreach (ISavable s in saveObjects)
            s.Save();
    }

    public void LoadGame() {
        saveObjects.Sort((x, y) => x.loadPriority.CompareTo(y.loadPriority));

        foreach (ISavable s in saveObjects) {
            s.Load();
            print($"Loaded {s}");
        }
    }

    void Awake() {
        instance = this;
    }

    IEnumerator Start() {
        //Save manage excecutes first (I changed script excecution order) before scripts built references to each other.
        //Hence I am waiting one frame for all scripts to build references to each other, cause otherwise some loading functions don't work proprely.
        yield return null;
        LoadGame();
    }
}


public enum LoadPriority {First, Second, Last};
public interface ISavable {
    
    LoadPriority loadPriority { get; }
    
    void Save();
    void Load();
}
