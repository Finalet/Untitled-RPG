using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageWindowUI : MonoBehaviour
{
    public GameObject slots;
    UI_InventorySlot[] allSlots;

    void Awake() {
        allSlots = slots.GetComponentsInChildren<UI_InventorySlot>();
    }

    public void AddItemToStorage(Item item, int amount) {
        for (int i = 0; i < allSlots.Length; i++) {
            if (allSlots[i].itemInSlot == item && item.isStackable && allSlots[i].itemAmount + amount <= item.maxStackAmount) {
                allSlots[i].AddItem(item, amount, null);
                break;
            }

            if (allSlots[i].itemInSlot == null) {
                allSlots[i].AddItem(item, amount, null);
                break;
            }
        }
    }

    public int getNumberOfEmptySlots () {
        int i = 0;
        foreach (UI_InventorySlot slot in allSlots) {
            if (slot.itemInSlot == null)
                i++;
        }
        return i;
    }
}
