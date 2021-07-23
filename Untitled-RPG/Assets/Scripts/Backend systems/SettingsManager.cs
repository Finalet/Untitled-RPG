using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour, ISavable
{
    public static SettingsManager instance;

    string savefilePath = "saves/settings.txt";

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
        LoadSettings();
        UpdateSettingsUI();
        InitKeyBinds();
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
        ES3.Save<float>("mouseSensitivity", mouseSensitivity, savefilePath);
        ES3.Save<bool>("invertY", invertY, savefilePath);
        ES3.Save<bool>("displayHelmet", displayHelmet, savefilePath);
        ES3.Save<bool>("displayCape", displayCape, savefilePath);
        ES3.Save<int>("numberOfSkillRows", numberOfSkillRows, savefilePath);
        KeybindsManager.instance.SaveKeybinds();
        
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }

    public void LoadSettings () {
        mouseSensitivity = ES3.Load<float>("mouseSensitivity", savefilePath, 50);
        invertY = ES3.Load<bool>("invertY", savefilePath, false);
        displayHelmet = ES3.Load<bool>("displayHelmet", savefilePath, true);
        displayCape = ES3.Load<bool>("displayCape", savefilePath, true);
        numberOfSkillRows = ES3.Load<int>("numberOfSkillRows", savefilePath, 2);
     
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

        ES3.Save<bool>("displayHelmet", displayHelmet, savefilePath);
    }
    public void SetDisplayCapeSlot () {
        displayCape = displayCapeToggleInSlot.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayCape();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        
        ES3.Save<bool>("displayCape", displayCape, savefilePath);
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
        Settings_KeybindsTemplate[] alltempaltes = keyBindsGrid.GetComponentsInChildren<Settings_KeybindsTemplate>();
        InitKeyBinds(alltempaltes);
    }

    void InitKeyBinds (Settings_KeybindsTemplate[] templatesArray = null) {
        int i = 0;
        foreach (KeyValuePair<string, KeyCode> keyBind in KeybindsManager.instance.defaultKeyBinds) {
            Settings_KeybindsTemplate t;
            if (templatesArray == null) {
                t = Instantiate(keyBindsTemplate, keyBindsGrid.transform).GetComponent<Settings_KeybindsTemplate>();
            } else {
                t = templatesArray[i];
            }
            
            t.Init(keyBind.Key);

            if (templatesArray == null && (keyBind.Key == "Sheathe weapon" || keyBind.Key == "Interact" || keyBind.Key == "Hide interface")) {
                Settings_KeybindsTemplate spacer = Instantiate(keyBindsTemplate, keyBindsGrid.transform).GetComponent<Settings_KeybindsTemplate>();
                spacer.Init(keyBind.Key, true);
            }
            i++;
        }
        Destroy(keyBindsTemplate);
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
        LoadSettings();
        UpdateSettingsUI();
    }

#endregion
}
