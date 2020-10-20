using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Item/Weapon")]
public class Weapon : Equipment
{
    public WeaponType weaponType;
    [Header("Stats")]
    public int MeleeAttack;
    public int RangedAttack;
    public int MagicPower;
    public int HealingPower;
    public int Defense;

    public override void Use(UI_InventorySlot initialSlot)
    {
        if (!(initialSlot is UI_EquipmentSlot))
            Equip(initialSlot);
        else
            Unequip(initialSlot);
    }

    void Equip (UI_InventorySlot initialSlot) {
        if (weaponType == WeaponType.OneHandedSword || weaponType == WeaponType.OneHandedStaff) {
            OneHandedEquip(initialSlot);
        } else if (weaponType == WeaponType.TwoHandedSword || weaponType == WeaponType.TwoHandedStaff) {
            TwoHandedEquip(initialSlot);
        }
    }

    void Unequip (UI_InventorySlot initialSlot) {
        InventoryManager.instance.AddItemToInventory(this, 1);
        initialSlot.ClearSlot();
    }

    void OneHandedEquip (UI_InventorySlot initialSlot) {
        if (EquipmentManager.instance.mainHand.itemInSlot == null) { //if main hand free, equip it there
            EquipmentManager.instance.mainHand.AddItem(this, 1, initialSlot);
        } else if (EquipmentManager.instance.secondaryHand.itemInSlot == null) { 
            Weapon w = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
            if (w.weaponType == WeaponType.OneHandedSword || w.weaponType == WeaponType.OneHandedStaff) { //if main hand is busy with one handed weapon, and secondary hand is free, equip if there
                EquipmentManager.instance.secondaryHand.AddItem(this, 1, initialSlot);
            } else {
                EquipmentManager.instance.mainHand.AddItem(this, 1, initialSlot); //if main hand is busy with two handed weapon, replace it;
            }
        } else { //if all hands are budy, put in the main hand
            EquipmentManager.instance.mainHand.AddItem(this, 1, initialSlot);
        }
    }

    void TwoHandedEquip (UI_InventorySlot initialSlot) {
        EquipmentManager.instance.mainHand.AddItem(this, 1, initialSlot);
    }
}
