using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchItem : Resource
{
    public override void Use()
    {
        PeaceCanvas.instance.ToggleWaitTimeWindow();
    }

    public override void Use(UI_InventorySlot initialSlot){}
}
