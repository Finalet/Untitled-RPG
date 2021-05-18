using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SittingSpot : MonoBehaviour
{
    public bool isTaken;

    PlayerControlls playerControlls;
    bool playerNearby;

    void Start() {
        playerControlls = PlayerControlls.instance;
    }

    void Update() {
        if (!playerNearby)
            return;
        
        if (!playerControlls.isSitting) {
            if (isTaken)
                return;

            PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.interact], "Sit");
            if (Input.GetKeyDown(KeybindsManager.instance.interact)) {
                playerControlls.Sit(this);
            }
        } else {
            PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.interact], "Get up");
            if (Input.GetKeyDown(KeybindsManager.instance.interact)) {
                playerControlls.Unsit(this);
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) {
            playerNearby = true;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) {
            PeaceCanvas.instance.HideKeySuggestion();
            playerNearby = false;
        }
    }
}
