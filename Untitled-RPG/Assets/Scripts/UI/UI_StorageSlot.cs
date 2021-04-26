using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_StorageSlot : UI_InventorySlot
{
    protected override void Awake() {
        savefilePath = "saves/storageSlots.txt";
        if (slotID == -1) slotID = System.Convert.ToInt16(name.Substring(name.IndexOf('(') + 1, 2)); //only works for slots after 10, first 10 needs to be assigned manually
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
    }
}
