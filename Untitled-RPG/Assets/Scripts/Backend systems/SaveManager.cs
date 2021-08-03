using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public List<ISavable> saveObjects = new List<ISavable>();

    public string currentProfile {
        get {
            return allProfiles.Count == 0 ? null : allProfiles[currentProfileIndex];
        }
    }
    private int _currentProfileIndex;
    public int currentProfileIndex {
        get {
            return _currentProfileIndex;
        }
        set {
            _currentProfileIndex = value;
            LoadCharacters();
            ES3.Save<int>("currentProfileIndex", _currentProfileIndex, getSaveDataFolderPath("profiles"));
        }
    }
    public List<string> allProfiles = new List<string>();

    public string currentCharacter {
        get {
            return allCharacters.Count == 0 ? null : allCharacters[currentCharacterIndex];
        }
    }
    private int _currentCharacterIndex;
    public int currentCharacterIndex {
        get {
            return _currentCharacterIndex;
        }
        set {
            _currentCharacterIndex = value;
            ES3.Save<int>("currentCharacterIndex", _currentCharacterIndex, getCurrentProfileFolderPath("characters"));
        }
    }
    public List<string> allCharacters = new List<string>();

    public void SaveGame() {
        foreach (ISavable s in saveObjects)
            s.Save();
    }

    public void LoadGame() {
        LoadProfiles();
        
        saveObjects.Sort((x, y) => x.loadPriority.CompareTo(y.loadPriority));

        foreach (ISavable s in saveObjects) {
            s.Load();
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

    public void SaveProfiles () {
        ES3.Save<List<string>>("allProfiles", allProfiles, getSaveDataFolderPath("profiles"));
    }
    public void SaveCharacters () {
        ES3.Save<List<string>>("allCharacters", allCharacters, getCurrentProfileFolderPath("characters"));
    }
    public void LoadProfiles(){
        string saveFilePath = getSaveDataFolderPath("profiles");

        allProfiles = ES3.Load<List<string>>("allProfiles", saveFilePath, new List<string>());
        currentProfileIndex = ES3.Load<int>("currentProfileIndex", saveFilePath, 0);
    }
    void LoadCharacters () {
        string saveFilePath = getCurrentProfileFolderPath("characters");

        allCharacters = ES3.Load<List<string>>("allCharacters", saveFilePath, new List<string>());
        currentCharacterIndex = ES3.Load<int>("currentCharacterIndex", saveFilePath, 0);
    }


    public string getSaveDataFolderPath (string fileName = "") {
        if (fileName == "") return $"SaveData";
        else return $"SaveData/{fileName}.json";
    }
    public string getCurrentProfileFolderPath(string fileName = ""){
        if (fileName == "") return $"{getSaveDataFolderPath()}/{currentProfile}";
        else return $"{getSaveDataFolderPath()}/{currentProfile}/{fileName}.json";
    }
    public string getCurrentCharacterFolderPath (string fileName = ""){        
        if (fileName == "") return $"{getCurrentProfileFolderPath()}/{currentCharacter}";
        else return $"{getCurrentProfileFolderPath()}/{currentCharacter}/{fileName}.json"; 
    }
}


public enum LoadPriority {First, Second, Last};
public interface ISavable {
    
    LoadPriority loadPriority { get; }
    
    void Save();
    void Load();
}
