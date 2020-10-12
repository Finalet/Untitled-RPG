using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_EquipmentSlot : UI_InventorySlot
{
    [Header("Equipment slot")]
    public EquipmentSlotType equipmentSlotType;

    protected override void Awake()
    {
        savefilePath = "saves/equipmentSlots.txt";
    }

    public override void Save (){
        short ID;
        if (itemInSlot != null)
            ID = (short)itemInSlot.ID;
        else
            ID = -1; //Slot is empty

        ES3.Save<short>($"slot_{slotID}_itemID", ID, savefilePath);
    }
    public override void Load () {
        short ID = ES3.Load<short>($"slot_{slotID}_itemID", savefilePath, -1);

        if (ID < 0) {
            ClearSlot();
            return; //Slot is empty
        }
        AddItem(AssetHolder.instance.getItem(ID), 1, null);
    }
    public override void AddItem (Item item, int amount, UI_InventorySlot initialSlot) {
        if (equipmentSlotType == EquipmentSlotType.MainHand) {
            MainHandAdd(item, amount, initialSlot);
        } else if (equipmentSlotType == EquipmentSlotType.SecondaryHand) {
            SecondaryHandAdd(item, amount, initialSlot);    
        } else {
            //if its not equipment, return it to initial slot. LATER IMPLEMENT EVERY BODY PART.
            initialSlot.AddItem(item, amount, null); 
            return;
        }        
    }

    protected override void DisplayItem() {
        slotIcon.sprite = itemInSlot.itemIcon;
        slotIcon.color = Color.white;
    }

    void SharedAdd (Item item, int amount, UI_InventorySlot initialSlot) {
        if (initialSlot != null) initialSlot.ClearSlot(); //at this point we are 100% equiping the item, so its safe to clear initial slot. Initial slot might be null if we drop item in a wrong area and it just returns back
        if (itemInSlot != null) {
            initialSlot.AddItem(itemInSlot, itemAmount, null);
        }

        ClearSlot();
        itemInSlot = item;
        itemAmount = amount;
        DisplayItem();
    }

    void MainHandAdd (Item item, int amount, UI_InventorySlot initialSlot) {
        if ( !(item is Weapon) ) {  //if its not weapon, return it to initial slot;
            initialSlot.AddItem(item, amount, null); 
            return;
        }

        Weapon w = (Weapon)item;
        if (w.weaponType == WeaponType.TwoHanded && EquipmentManager.instance.secondaryHand.itemInSlot != null) { //if its two handed and second hand is busy, clear the second hand.
            InventoryManager.instance.AddItemToInventory(EquipmentManager.instance.secondaryHand.itemInSlot, EquipmentManager.instance.secondaryHand.itemAmount, initialSlot);
            EquipmentManager.instance.secondaryHand.ClearSlot();
        }

        SharedAdd(item, amount, initialSlot);
        EquipmentManager.instance.EquipWeaponPrefab(w);
    }

    void SecondaryHandAdd (Item item, int amount, UI_InventorySlot initialSlot) {
        if ( !(item is Weapon) ) {   //if its not weapon, return it to initial slot;
            initialSlot.AddItem(item, amount, null); 
            return;
        }
        
        Weapon w = (Weapon)item;
        if (w.weaponType == WeaponType.TwoHanded) {  //if weapon is two handed, return it to initial slot
            initialSlot.AddItem(item, amount, initialSlot); 
            return;
        } else if (w.weaponType == WeaponType.OneHanded) { //if weapon is onehanded, but main hand is already carryign two handed weapon, return to initial slot
            Weapon mw = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
            if (mw != null && mw.weaponType == WeaponType.TwoHanded) {
                initialSlot.AddItem(item, amount, null); 
                return;
            }
        }    
        SharedAdd(item, amount, initialSlot);
        EquipmentManager.instance.EquipWeaponPrefab(w, true);
    }

    public override void ClearSlot()
    {
        Weapon w = (Weapon)itemInSlot;
        if (w != null) {
            if (w.weaponType == WeaponType.TwoHanded) {
                EquipmentManager.instance.UnequipWeaponPrefab(true);
            } else if (w.weaponType == WeaponType.OneHanded) {
                if (equipmentSlotType == EquipmentSlotType.MainHand) {
                    EquipmentManager.instance.UnequipWeaponPrefab(false);
                } else {
                    EquipmentManager.instance.UnequipWeaponPrefab(false, true);
                }
            }
        }
        base.ClearSlot();
    }
}
