using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageNPC : NPC
{
    public override void Interract () {
        base.Interract();
        PeaceCanvas.instance.OpenInventory(true, true);
    }

    public override void StopInterract()
    {
        base.StopInterract();
    }
}
