using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using System;
using System.Linq;

public class InteractionTarget : MonoBehaviour
{
    [Dropdown("GetInteractionKeyStrings")] public string interactionKey;
    public string interactionLabel;
    public InterractionIcons interractionIcon;
    
    [Space, MinValue(0)]
    public float holdKeyDuration;

    [Space, Required]
    public PlayerTrigger playerTrigger;
    bool playerDetectedLast;

    [Space]
    public UnityEvent InteractionEvent;

    //Holding button;
    float startedHoldingTime;
    bool isHolding;

    void Update() {
        if (!playerTrigger) return;
        if (playerDetectedLast != playerTrigger.playerDetected) OnPlayerDetectedChange();
        
        playerDetectedLast = playerTrigger.playerDetected;

        if (!playerTrigger.playerDetected) return;

        if (detectInteraction()) {
            InteractionEvent?.Invoke();
        }
    }

    bool isKeybind (string key) {
        if (string.IsNullOrEmpty(key)) return true; //Return true so we wont get errors while setting up the script
        return KeybindsManager.defaultKeyBinds.ContainsKey(key);
    }

    bool detectInteraction () {
        if (holdKeyDuration == 0) {
            return Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds[interactionKey]);
        } else {
            if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds[interactionKey])) {
                startedHoldingTime = Time.time;
                isHolding = true;
            }

            if (isHolding){
                if (Input.GetKeyUp(KeybindsManager.instance.currentKeyBinds[interactionKey])) {
                    isHolding = false;
                    PeaceCanvas.instance.UpdateKeySuggestionProgress(0);
                    return false;
                }

                PeaceCanvas.instance.UpdateKeySuggestionProgress( (Time.time - startedHoldingTime) / holdKeyDuration );

                if (Time.time - startedHoldingTime > holdKeyDuration) {
                    isHolding = false;
                    PeaceCanvas.instance.UpdateKeySuggestionProgress(0);
                    PeaceCanvas.instance.HideKeySuggestion();
                    return true;
                }
            }
            return false;
        }
    }

    void OnPlayerDetectedChange() {
        if (playerTrigger.playerDetected) {
            if (string.IsNullOrEmpty(interactionLabel)) PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds[interactionKey]], interractionIcon);
            else PeaceCanvas.instance.ShowKeySuggestion(interactionKey, interactionLabel);
        } else {
            PeaceCanvas.instance.HideKeySuggestion();
        }
    }

    List<string> GetInteractionKeyStrings() {
        List<string> dropdown = new List<string>();
        
        foreach (KeyValuePair<string, KeyCode> k in KeybindsManager.defaultKeyBinds.ToList()) {
            dropdown.Add(k.Key);
        }
        return dropdown;
    }

}