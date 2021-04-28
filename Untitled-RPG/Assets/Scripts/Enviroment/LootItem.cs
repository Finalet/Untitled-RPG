using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LootItem : MonoBehaviour
{
    public Item item;
    public int itemAmount;

    [Header("Gold loot")]
    public bool isGold;

    public void Drop() {
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.2f);
        Vector3 force = Vector3.up * 5 + Vector3.right * (Random.value-1) * 3 + Vector3.forward * (Random.value-1) * 3;
        GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        Destroy(gameObject, 300);
    }

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player")) {
            PeaceCanvas.instance.ShowKeySuggestion("F", "Pick-up");

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
                PeaceCanvas.instance.ShowLootNotification(item, itemAmount);
                PeaceCanvas.instance.HideKeySuggestion();
                Destroy(gameObject);
            }
            
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player"))
            PeaceCanvas.instance.HideKeySuggestion();
    }
}
