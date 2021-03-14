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

    [Space]
    public Slider mouseSensitivitySlider;
    public Text mouseSensitivityText;
    public Toggle invertYToggle;

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
    }

    public void LoadSettings () {
        mouseSensitivity = ES3.Load<float>("mouseSensitivity", savefilePath);
        invertY = ES3.Load<bool>("invertY", savefilePath);
    }

    public void SetMouseSensitivity() {
        mouseSensitivity = mouseSensitivitySlider.value;
        mouseSensitivityText.text = mouseSensitivity.ToString();
    }
    public void SetInvertY () {
        invertY = invertYToggle.isOn;
    }

    void UpdateSettingsUI () {
        mouseSensitivitySlider.value = mouseSensitivity;
        mouseSensitivityText.text = mouseSensitivity.ToString();
        invertYToggle.isOn = invertY;
    }
}
