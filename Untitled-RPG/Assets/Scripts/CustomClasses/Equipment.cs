﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
    [Header("Stats")]
    public int MeleeAttack;
    public int RangedAttack;
    public int MagicPower;
    public int HealingPower;
    public int Defense;

    public override void Use (){}
    public override void Use (UI_InventorySlot initialSlot){
        if (!(initialSlot is UI_EquipmentSlot))
            Equip(initialSlot);
        else
            Unequip(initialSlot);
    }

    protected virtual void Equip (UI_InventorySlot initialSlot) {}
    protected virtual void Unequip (UI_InventorySlot initialSlot) {
        InventoryManager.instance.AddItemToInventory(this, 1);
        initialSlot.ClearSlot();
    }
}
