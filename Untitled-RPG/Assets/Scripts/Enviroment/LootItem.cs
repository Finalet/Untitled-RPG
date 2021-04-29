using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LootItem : MonoBehaviour
{
    public Item item;
    public int itemAmount;

    [Header("Bools")]
    public bool isGold;

    [System.NonSerialized] public bool playerDetected;
    [System.NonSerialized] public float priority;
    LootItem nearbyItemWithHigherPriority;

    void Awake() {
        priority = Random.value;
    }

    void SetGlowColor() {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        
        var main = ps.main;
        main.startColor = isGold ? UI_General.getRarityColor(ItemRarity.Common) : UI_General.getRarityColor(item.itemRarity);

        ps.Play();
    }

    public void Drop() {
        SetGlowColor();
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.2f);
        Vector3 force = Vector3.up * 5 + Vector3.right * (Random.value-0.5f) * 3 + Vector3.forward * (Random.value-0.5f) * 3;
        GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        Destroy(gameObject, 300);
    }

    void OnTriggerStay(Collider other) {
        if (other.GetComponent<LootItem>() != null) {
            if (other.GetComponent<LootItem>().priority > priority && other.GetComponent<LootItem>().playerDetected) { //if another loot is nearby and its priority is higher, then dont pick this item up
                nearbyItemWithHigherPriority = other.GetComponent<LootItem>();
                return;
            }
        }

        if (other.CompareTag("Player")) {
            playerDetected = true;
            PeaceCanvas.instance.ShowKeySuggestion("F", "Pick-up");
            
            if (nearbyItemWithHigherPriority != null)
                return;

            if (Input.GetKeyDown(KeyCode.F)) {
                switch (isGold) {
                    case false:
                        if (InventoryManager.instance.getNumberOfEmptySlots() == 0) {
                            CanvasScript.instance.DisplayWarning("Inventory is full");
                            return;
                        }
                        InventoryManager.instance.AddItemToInventory(item, itemAmount, null);
                        break;
                    case true:
                        InventoryManager.instance.AddGold(itemAmount);
                        break;
                }
                PlayerControlls.instance.PlayGeneralAnimation(1);
                LootNotificationManager.instance.ShowLootNotification(this);
                PeaceCanvas.instance.HideKeySuggestion();
                Destroy(gameObject);
            }
            
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerDetected = false;
            PeaceCanvas.instance.HideKeySuggestion();
        }
    }
}
