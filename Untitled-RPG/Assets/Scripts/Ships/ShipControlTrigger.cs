using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControlTrigger : MonoBehaviour
{
    public Transform controllingPosition;
    bool playerInsideTrigger;
    ShipController shipController;

    void Awake() {
        shipController = GetComponentInParent<ShipController>();
    }

    void Update() {
        if (playerInsideTrigger) {
            if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Interact"])) {
                shipController.TogglePlayerControl(controllingPosition);
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider) ) {
            PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds["Interact"]], InterractionIcons.Ship);
            playerInsideTrigger = true;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider) )
            playerInsideTrigger = false;
    }
}
