using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour, ISavable
{
    public static SettingsManager instance;

    [Header("Settings")]
    public float mouseSensitivity;
    public bool invertY;
    public bool displayHelmet = true;
    public bool displayCape = true;
    public int numberOfSkillRows;

    [Space]
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI mouseSensitivityValue;
    public Toggle invertYToggle;
    public Toggle displayHelmetToggle;
    public Toggle displayHelmetToggleInSlot;
    public Toggle displayCapeToggle;
    public Toggle displayCapeToggleInSlot;
    public TMP_Dropdown numberOfSkillRowsDropdown;

    public GameObject keyBindsGrid;
    public GameObject keyBindsTemplate;

    void Start() {
        // LoadSettings();
        // UpdateSettingsUI();
        // InitKeyBinds();
    }

    string saveFilePath() {
        return SaveManager.instance.getCurrentProfileFolderPath("settings");
    }

    public SettingsManager() {
        instance = this;
    }

    public void OpenSettings () {
        gameObject.SetActive(true);
        if (PeaceCanvas.instance) PeaceCanvas.instance.openPanels++;
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }

    public void CloseSettings () {
        SaveSettings();
        if (PeaceCanvas.instance) PeaceCanvas.instance.openPanels--;
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        gameObject.SetActive(false);
    }

    public void SaveSettings () {
        string path = saveFilePath();

        ES3.Save<float>("mouseSensitivity", mouseSensitivity, path);
        ES3.Save<bool>("invertY", invertY, path);
        ES3.Save<bool>("displayHelmet", displayHelmet, path);
        ES3.Save<bool>("displayCape", displayCape, path);
        ES3.Save<int>("numberOfSkillRows", numberOfSkillRows, path);
        KeybindsManager.instance.SaveKeybinds();
        
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }

    public void LoadSettings () {
        string path = saveFilePath();

        mouseSensitivity = ES3.Load<float>("mouseSensitivity", path, 50);
        invertY = ES3.Load<bool>("invertY", path, false);
        displayHelmet = ES3.Load<bool>("displayHelmet", path, true);
        displayCape = ES3.Load<bool>("displayCape", path, true);
        numberOfSkillRows = ES3.Load<int>("numberOfSkillRows", path, 2);
     
        KeybindsManager.instance.LoadKeybinds();
    }

    public void SetMouseSensitivity() {
        mouseSensitivity = mouseSensitivitySlider.value;
        mouseSensitivityValue.text = mouseSensitivity.ToString();
    
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }
    public void SetInvertY () {
        invertY = invertYToggle.isOn;
    
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }
    public void SetDisplayHelmet () {
        displayHelmet = displayHelmetToggle.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayHelmet();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();

        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }
    public void SetDisplayCape () {
        displayCape = displayCapeToggle.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayCape();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();

        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }
    public void SetDisplayHelmetSlot () {
        displayHelmet = displayHelmetToggleInSlot.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayHelmet();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);

        ES3.Save<bool>("displayHelmet", displayHelmet, saveFilePath());
    }
    public void SetDisplayCapeSlot () {
        displayCape = displayCapeToggleInSlot.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayCape();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        
        ES3.Save<bool>("displayCape", displayCape, saveFilePath());
    }

    public void SetNumberOfSkillRows () {
        numberOfSkillRows = numberOfSkillRowsDropdown.value+1;
        if (Combat.instanace) Combat.instanace.numberOfSkillSlotsRows = numberOfSkillRows;

        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }

    public void UpdateSettingsUI () {
        mouseSensitivitySlider.value = mouseSensitivity;
        mouseSensitivityValue.text = mouseSensitivity.ToString();
        invertYToggle.isOn = invertY;
        displayHelmetToggle.isOn = displayHelmet;
        displayCapeToggle.isOn = displayCape;
        numberOfSkillRowsDropdown.value = numberOfSkillRows-1;

        if (displayHelmetToggleInSlot != null) displayHelmetToggleInSlot.isOn = displayHelmet;
        if (displayCapeToggleInSlot != null) displayCapeToggleInSlot.isOn = displayCape;
    }

    public void SetDetaultKeyBinds (){
        KeybindsManager.instance.SetDefaultKeyBinds();
        InitKeyBinds();
    }

    void InitKeyBinds () {
        int i = 0;
        Settings_KeybindsTemplate[] templatesArray = keyBindsGrid.GetComponentsInChildren<Settings_KeybindsTemplate>();
        keyBindsTemplate.SetActive(true);
        foreach (KeyValuePair<string, KeyCode> keyBind in KeybindsManager.instance.defaultKeyBinds) {
            Settings_KeybindsTemplate t;
            if (templatesArray.Length == KeybindsManager.instance.defaultKeyBinds.Count) {
                t = templatesArray[i];
            } else {
                t = Instantiate(keyBindsTemplate, keyBindsGrid.transform).GetComponent<Settings_KeybindsTemplate>();
            }
            
            t.Init(keyBind.Key);

            if (templatesArray.Length != KeybindsManager.instance.defaultKeyBinds.Count && isLastKeyInCategory(keyBind.Key)) {
                GameObject spacer = new GameObject();
                spacer.AddComponent<RectTransform>();
                spacer.GetComponent<RectTransform>().SetParent(keyBindsGrid.transform);
            }
            i++;
        }
        keyBindsTemplate.SetActive(false);
    }

    bool isLastKeyInCategory (string key) {
        return (key == "Sheathe weapon" || key == "Interact" || key == "Toggle Mount Flight" || key == "Free camera");
    }

#region ISavable
    //We add it to savables list from PeaceCanvas, because this object is disabled
    public LoadPriority loadPriority {
        get {
            return LoadPriority.First;
        }
    }

    public void Save(){
        //Don't do anything on general game save since we save settings manually when closing the settings window
    }
    public void Load() {
        if (SaveManager.instance.allProfiles.Count == 0) return;

        LoadSettings();
        UpdateSettingsUI();
        InitKeyBinds();
    }

#endregion
}
