using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MalbersAnimations;

public class MountKeySuggestionsUI : MonoBehaviour
{
    bool showFlightKey;

    public RectTransform increaseSpeedSuggestionLabel;
    public RectTransform decreaseSpeedSuggestionLabel;
    public RectTransform sprintSuggestionLabel;
    public RectTransform strafeSuggestionLabel;
    public RectTransform flySuggestionLabel;
    
    public RectTransform increaseSpeed; TextMeshProUGUI increaseSpeedKeyLabel;
    public RectTransform decreaseSpeed; TextMeshProUGUI decreaseSpeedKeyLabel;
    public RectTransform sprint; TextMeshProUGUI sprintKeyLabel;
    public RectTransform strafe; TextMeshProUGUI strafeKeyLabel;
    public RectTransform fly; TextMeshProUGUI flyKeyLabel;

    float leftPadding = 20;

    void Start() {
        increaseSpeedKeyLabel = increaseSpeed.GetChild(0).GetComponent<TextMeshProUGUI>();
        decreaseSpeedKeyLabel = decreaseSpeed.GetChild(0).GetComponent<TextMeshProUGUI>();
        sprintKeyLabel = sprint.GetChild(0).GetComponent<TextMeshProUGUI>();
        strafeKeyLabel = strafe.GetChild(0).GetComponent<TextMeshProUGUI>();
        flyKeyLabel = fly.GetChild(0).GetComponent<TextMeshProUGUI>();

        UpdateKeySuggestions();
    }

    public void UpdateKeySuggestions () {
        showFlightKey = PlayerControlls.instance.rider.MountStored.Animal.GetComponent<MalbersInput>().FindInput("Fly") != null;
        
        SetupLabel(ref increaseSpeedKeyLabel, ref increaseSpeed, ref increaseSpeedSuggestionLabel, KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds["Increase Mount Speed"]]);
        SetupLabel(ref decreaseSpeedKeyLabel, ref decreaseSpeed, ref decreaseSpeedSuggestionLabel, KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds["Decrease Mount Speed"]]);
        SetupLabel(ref sprintKeyLabel, ref sprint, ref sprintSuggestionLabel, KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds["Run"]]);
        SetupLabel(ref strafeKeyLabel, ref strafe, ref strafeSuggestionLabel, KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds["Toggle Mount Strafe"]]);
        if (showFlightKey) SetupLabel(ref flyKeyLabel, ref fly, ref flySuggestionLabel, KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds["Toggle Mount Flight"]]);
        
        if (!showFlightKey) {
            fly.gameObject.SetActive(false);
            flySuggestionLabel.gameObject.SetActive(false);
        }

        GetComponent<RectTransform>().sizeDelta = showFlightKey ?  new Vector2 (200, 204) : new Vector2 (200, 170);
    }

    void SetupLabel (ref TextMeshProUGUI keyLabel, ref RectTransform imageRect, ref RectTransform labelRect, string keyText){
        keyLabel.text = keyText;
        imageRect.sizeDelta = keyLabel.text.Length <= 1 ? new Vector2(25, 25) : new Vector2(keyLabel.preferredWidth + 20, 25);
        imageRect.anchoredPosition = new Vector2(imageRect.sizeDelta.x*0.5f + leftPadding, imageRect.anchoredPosition.y);
        labelRect.anchoredPosition = new Vector2(imageRect.sizeDelta.x + 10 + leftPadding, labelRect.anchoredPosition.y);
    }

    void Update() {
        DetectKeyPress();
    }

    void DetectKeyPress() {
        if (PeaceCanvas.instance.anyPanelOpen || PeaceCanvas.instance.isGamePaused)
            return;

        if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Increase Mount Speed"])) StartCoroutine(UI_General.PressAnimation(increaseSpeed, KeybindsManager.instance.currentKeyBinds["Increase Mount Speed"]));
        if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Decrease Mount Speed"])) StartCoroutine(UI_General.PressAnimation(decreaseSpeed, KeybindsManager.instance.currentKeyBinds["Decrease Mount Speed"]));
        if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Run"])) StartCoroutine(UI_General.PressAnimation(sprint, KeybindsManager.instance.currentKeyBinds["Run"]));
        if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Toggle Mount Strafe"])) StartCoroutine(UI_General.PressAnimation(strafe, KeybindsManager.instance.currentKeyBinds["Toggle Mount Strafe"]));
        if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Toggle Mount Flight"])) StartCoroutine(UI_General.PressAnimation(fly, KeybindsManager.instance.currentKeyBinds["Toggle Mount Flight"]));
    }
}
