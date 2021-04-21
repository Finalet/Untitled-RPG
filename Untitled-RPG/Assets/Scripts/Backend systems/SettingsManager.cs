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

    [Space]
    public Slider mouseSensitivitySlider;
    public Text mouseSensitivityText;
    public Toggle invertYToggle;
    public Toggle displayHelmetToggle;

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
    }

    public void LoadSettings () {
        if (ES3.KeyExists("mouseSensitivity", savefilePath)) mouseSensitivity = ES3.Load<float>("mouseSensitivity", savefilePath);
        if (ES3.KeyExists("invertY", savefilePath))invertY = ES3.Load<bool>("invertY", savefilePath);
        if (ES3.KeyExists("displayHelmet", savefilePath))displayHelmet = ES3.Load<bool>("displayHelmet", savefilePath);
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
    }

    void UpdateSettingsUI () {
        mouseSensitivitySlider.value = mouseSensitivity;
        mouseSensitivityText.text = mouseSensitivity.ToString();
        invertYToggle.isOn = invertY;
        displayHelmetToggle.isOn = displayHelmet;
    }
}
