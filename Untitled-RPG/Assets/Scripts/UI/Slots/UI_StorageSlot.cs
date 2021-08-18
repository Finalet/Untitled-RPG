using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_StorageSlot : UI_InventorySlot
{
    protected override string savefilePath() {
        return SaveManager.instance.getCurrentCharacterFolderPath("storageSlots");
    }

    void Awake() {
        SetSlotID();
    }

    public override void UseItem () {
        if (InventoryManager.instance.getNumberOfEmptySlots() == 0) {
            CanvasScript.instance.DisplayWarning("Inventory is full");
            return;
        }

        int amount = 1;
        if(itemAmount > 1)
            amount = Input.GetKey(KeyCode.LeftControl) ? (itemAmount >= 100 ? 100 : itemAmount) : Input.GetKey(KeyCode.LeftShift) ? (itemAmount >= 10 ? 10 : itemAmount) : amount;

        InventoryManager.instance.AddItemToInventory(itemInSlot, amount);
        itemAmount -= amount;
        if (itemAmount == 0)
            ClearSlot();
        
        PlayerAudioController.instance.PlayPlayerSound(PlayerAudioController.instance.LootPickup);
    }
}
