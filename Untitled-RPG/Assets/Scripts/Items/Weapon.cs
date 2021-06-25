using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Item/Weapon")]
public class Weapon : Equipment
{
    [Header("Weapon info")]
    public WeaponCategory weaponCategory;
    public WeaponHand weaponHand;
    public GameObject weaponPrefab;

    protected override void Equip (UI_InventorySlot initialSlot) {
        base.Equip(initialSlot);
        
        if (weaponHand == WeaponHand.OneHanded) {
            OneHandedEquip(initialSlot);
        } else if (weaponHand == WeaponHand.TwoHanded) {
            TwoHandedEquip(initialSlot);
        } else if (weaponCategory == WeaponCategory.Bow) {
            BowEquip(initialSlot);
        } else if (weaponCategory == WeaponCategory.Shield) {
            ShieldEquip(initialSlot);
        } else {
            Debug.LogError($"Equiping {weaponHand} {weaponCategory} is not supported yet");
            //IMPLEMENT OTHER EQUIPMENT TYPES
        }
    }

    void OneHandedEquip (UI_InventorySlot initialSlot) {
        if (EquipmentManager.instance.mainHand.itemInSlot == null) { //if main hand free, equip it there
            EquipmentManager.instance.mainHand.AddItem(this, 1, initialSlot);
        } else if (EquipmentManager.instance.secondaryHand.itemInSlot == null) { 
            Weapon w = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
            if (w.weaponHand == WeaponHand.OneHanded) { //if main hand is busy with one handed weapon, and secondary hand is free, equip if there
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

    void BowEquip (UI_InventorySlot initialSlot) {
        EquipmentManager.instance.bow.AddItem(this, 1, initialSlot);
    }

    void ShieldEquip (UI_InventorySlot initialSlot) {
        EquipmentManager.instance.secondaryHand.AddItem(this, 1, initialSlot);
    }

    void OnValidate() {
        if (weaponCategory == WeaponCategory.Bow)
            weaponHand = WeaponHand.BowHand;
        else if (weaponCategory == WeaponCategory.Shield)
            weaponHand = WeaponHand.SecondaryHand;
    }
}
