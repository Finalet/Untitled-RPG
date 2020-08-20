using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipWheelTrigger : MonoBehaviour
{
    public bool playerOnTrigger;

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger)
            playerOnTrigger = true;
    }
    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger)
            playerOnTrigger = false;
    }
}
