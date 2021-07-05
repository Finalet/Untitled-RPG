using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageNPC : NPC
{
    protected override void CustomInterract()
    {
        OpenStorageWindow();
        PeaceCanvas.instance.OpenInventory(true, true);
    }
    protected override void CustomStopInterract()
    {
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
        PlayerAudioController.instance.PlayPlayerSound(PlayerAudioController.instance.LootPickup);
    }
}
