using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    string savefilePath = "saves/settings.txt";

    [Header("Settings")]
    public float mouseSensitivity;
    public bool invertY;
    public bool displayHelmet = true;
    public bool displayCape = true;

    [Space]
    public Slider mouseSensitivitySlider;
    public Text mouseSensitivityText;
    public Toggle invertYToggle;
    public Toggle displayHelmetToggle;
    public Toggle displayHelmetToggleInSlot;
    public Toggle displayCapeToggle;
    public Toggle displayCapeToggleInSlot;

    void Awake() {
        if (instance == null)
            instance = this;
        else 
            Debug.LogError("There are two instances of SettingsManager. Make sure there is only one");
    }
    void Start() {
        LoadSettings();
        UpdateSettingsUI();
    }

    public void SaveSettings () {
        ES3.Save<float>("mouseSensitivity", mouseSensitivity, savefilePath);
        ES3.Save<bool>("invertY", invertY, savefilePath);
        ES3.Save<bool>("displayHelmet", displayHelmet, savefilePath);
        ES3.Save<bool>("displayCape", displayCape, savefilePath);
    }

    public void LoadSettings () {
        mouseSensitivity = ES3.Load<float>("mouseSensitivity", savefilePath, 50);
        invertY = ES3.Load<bool>("invertY", savefilePath, false);
        displayHelmet = ES3.Load<bool>("displayHelmet", savefilePath, true);
        displayCape = ES3.Load<bool>("displayCape", savefilePath, true);
    }

    public void SetMouseSensitivity() {
        mouseSensitivity = mouseSensitivitySlider.value;
        mouseSensitivityText.text = mouseSensitivity.ToString();
    }
    public void SetInvertY () {
        invertY = invertYToggle.isOn;
    }
    public void SetDisplayHelmet () {
        displayHelmet = displayHelmetToggle.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayHelmet();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();
    }
    public void SetDisplayCape () {
        displayCape = displayCapeToggle.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayCape();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();
    }
    public void SetDisplayHelmetSlot () {
        displayHelmet = displayHelmetToggleInSlot.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayHelmet();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();
    }
    public void SetDisplayCapeSlot () {
        displayCape = displayCapeToggleInSlot.isOn;
        if (EquipmentManager.instance != null) EquipmentManager.instance.CheckDisplayCape();
        if (UIAudioManager.instance != null) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
        UpdateSettingsUI();
    }

    void UpdateSettingsUI () {
        mouseSensitivitySlider.value = mouseSensitivity;
        mouseSensitivityText.text = mouseSensitivity.ToString();
        invertYToggle.isOn = invertY;
        displayHelmetToggle.isOn = displayHelmet;
        displayCapeToggle.isOn = displayCape;
        displayHelmetToggleInSlot.isOn = displayHelmet;
        displayCapeToggleInSlot.isOn = displayCape;
    }
}
