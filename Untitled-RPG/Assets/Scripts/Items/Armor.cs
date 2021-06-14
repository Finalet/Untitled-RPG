using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeStudios.ModularCharacters;

[CreateAssetMenu(fileName = "New Armor", menuName = "Item/Armor")]
public class Armor : Equipment
{
    [Space]
    public ArmorType armorType;
    public ModularArmor modularArmor;

    protected override void Equip (UI_InventorySlot initialSlot) {
        base.Equip(initialSlot);
        
        switch (armorType) {
            case ArmorType.Helmet: {
                EquipmentManager.instance.helmet.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Chest: {
                EquipmentManager.instance.chest.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Gloves: {
                EquipmentManager.instance.gloves.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Pants: {
                EquipmentManager.instance.pants.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Boots: {
                EquipmentManager.instance.boots.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Back: {
                EquipmentManager.instance.back.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Necklace: {
                EquipmentManager.instance.necklace.AddItem(this, 1, initialSlot);
                break;
            }
            case ArmorType.Ring: {
                RingEquip(initialSlot);
                break;
            }
        }
    }

    void RingEquip (UI_InventorySlot initialSlot) {
        if (EquipmentManager.instance.ring.itemInSlot == null) { //if first slot is empty
            EquipmentManager.instance.ring.AddItem(this, 1, initialSlot);
        } else if (EquipmentManager.instance.secondRing.itemInSlot == null) { //if first slot is take, but second slot is open
            EquipmentManager.instance.secondRing.AddItem(this, 1, initialSlot);
        } else { //if both slots taken, just put it into the main slot
            EquipmentManager.instance.ring.AddItem(this, 1, initialSlot);
        }
    }
}
