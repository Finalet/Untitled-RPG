using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingRoomTrigger : MonoBehaviour
{
    DungeonManager dungeonManager;

    void Awake() {
        dungeonManager = FindObjectOfType<DungeonManager>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && other is CapsuleCollider && !other.isTrigger) {
            dungeonManager.LaunchFight();
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player") && other is CapsuleCollider && !other.isTrigger) {
            dungeonManager.isPlayerInsideFightingRoom = true;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && other is CapsuleCollider && !other.isTrigger) {
            dungeonManager.isPlayerInsideFightingRoom = false;
        }
    }
}
