using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageNPC : NPC
{
    public override void Interract () {
        base.Interract();
        OpenStorageWindow();
        PeaceCanvas.instance.OpenInventory(true, true);
    }

    public override void StopInterract()
    {
        base.StopInterract();
        CloseStorageWindow();
    }

    void OpenStorageWindow() {
        PeaceCanvas.instance.storageWindow.SetActive(true);
    }

    void CloseStorageWindow (){
        PeaceCanvas.instance.storageWindow.SetActive(false);
    }

    public int getNumberOfEmptySlots() {
        return PeaceCanvas.instance.storageWindow.GetComponent<StorageWindowUI>().getNumberOfEmptySlots();
    }

    public void AddItemToStorage (Item item, int amount) {
        PeaceCanvas.instance.storageWindow.GetComponent<StorageWindowUI>().AddItemToStorage(item, amount);
    }
}
