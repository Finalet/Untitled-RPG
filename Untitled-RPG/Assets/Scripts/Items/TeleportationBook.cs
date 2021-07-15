using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Teleportation Book", menuName = "Item/Teleportation Book")]
public class TeleportationBook : Resource
{

    public override void Use()
    {
        TeleportManager.instance.TeleportMenu();
    }

    public override void Use(UI_InventorySlot initialSlot){}
}
