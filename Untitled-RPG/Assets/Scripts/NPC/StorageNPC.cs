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
        StorageManager.instance.OpenStorage();
    }

    void CloseStorageWindow (){
        StorageManager.instance.CloseStorage();
    }
}
