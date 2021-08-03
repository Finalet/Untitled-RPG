using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeybindsManager : MonoBehaviour
{
    public static KeybindsManager instance;

    void Awake() {
        instance = this;
        SetDefaultKeyBinds();
    }
    
    public Dictionary<string, KeyCode> defaultKeyBinds  = new Dictionary< string, KeyCode> () {
        {"Run", KeyCode.LeftShift},
        {"Roll", KeyCode.V},
        {"Crouch", KeyCode.C},
        {"Jump", KeyCode.Space},
        {"Switch skill rows", KeyCode.BackQuote},
        {"Toggle walk", KeyCode.CapsLock},
        {"Sheathe weapon", KeyCode.H},

        {"Quick access menu", KeyCode.Tab},
        {"Inventory", KeyCode.I},
        {"Skillbook", KeyCode.K},
        {"Skip time", KeyCode.O},
        {"Interact", KeyCode.F},

        {"Damage log", KeyCode.F1},
        {"Too many items", KeyCode.F2},
        {"Hide interface", KeyCode.F3},

        {"Slot 1", KeyCode.Alpha1},
        {"Slot 2", KeyCode.Alpha2},
        {"Slot 3", KeyCode.Alpha3},
        {"Slot 4", KeyCode.Alpha4},
        {"Slot 5", KeyCode.Alpha5},
        {"Slot 6", KeyCode.Q},
        {"Slot 7", KeyCode.E},
        {"Slot 8", KeyCode.R},
        {"Slot 9", KeyCode.T},
        {"Slot 10", KeyCode.X},
    };

    public Dictionary<string, KeyCode> currentKeyBinds  = new Dictionary< string, KeyCode> () {};


    public void SetDefaultKeyBinds() {
        currentKeyBinds = new Dictionary<string, KeyCode>(defaultKeyBinds);
    }

    public void SaveKeybinds()
    {
        string saveFilePath = SaveManager.instance.getCurrentProfileFolderPath("keyBinds");

        ES3.Save<Dictionary<string, KeyCode>>("currentKeyBinds", currentKeyBinds, saveFilePath);
        if (PeaceCanvas.instance) PeaceCanvas.instance.SetSuggestionKeys();
    }

    public void LoadKeybinds()
    {
        string saveFilePath = SaveManager.instance.getCurrentProfileFolderPath("keyBinds");
        
        if (ES3.FileExists(saveFilePath) && ES3.KeyExists("currentKeyBinds", saveFilePath)) 
            currentKeyBinds = ES3.Load<Dictionary<string, KeyCode>>("currentKeyBinds", saveFilePath);
        else SetDefaultKeyBinds();
    }
}
