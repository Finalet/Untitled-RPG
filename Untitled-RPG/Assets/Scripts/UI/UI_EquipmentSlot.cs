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

    protected override void Start()
    {
        //Not loading anything because already loaded when game launched from the EquipmentManager.
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
        savefilePath = "saves/equipmentSlots.txt";

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
        } else if (equipmentSlotType == EquipmentSlotType.Bow) {
            BowAdd(item, amount, initialSlot);    
        } else if (equipmentSlotType == EquipmentSlotType.Helmet || equipmentSlotType == EquipmentSlotType.Chest || equipmentSlotType == EquipmentSlotType.Gloves || equipmentSlotType == EquipmentSlotType.Pants || equipmentSlotType == EquipmentSlotType.Boots || equipmentSlotType == EquipmentSlotType.Back) {
            ArmorAdd(item, amount, initialSlot);    
        } else if (equipmentSlotType == EquipmentSlotType.Necklace) {
            NecklaceAdd(item, amount, initialSlot);    
        } else if (equipmentSlotType == EquipmentSlotType.Ring) {
            RingAdd(item, amount, initialSlot);    
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
        if (initialSlot != null) { //at this point we are 100% equiping the item, so its safe to clear initial slot. Initial slot might be null if we drop item in a wrong area and it just returns back
            initialSlot.ClearSlot();
        } 
        if (itemInSlot != null) {
            initialSlot.AddItem(itemInSlot, itemAmount, null);
        }

        ClearSlot();
        itemInSlot = item;
        itemAmount = amount;
        DisplayItem();
        PeaceCanvas.instance.PlaySound(PeaceCanvas.instance.equipItemSound);
    }

    void MainHandAdd (Item item, int amount, UI_InventorySlot initialSlot) {
        if ( !(item is Weapon) ) {  //if its not weapon, return it to initial slot;
            initialSlot.AddItem(item, amount, null); 
            return;
        }

        Weapon w = (Weapon)item;

        if (w.weaponType == WeaponType.Bow) {
            initialSlot.AddItem(item, amount, null);
            return;
        }

        if ( (w.weaponType == WeaponType.TwoHandedSword || w.weaponType == WeaponType.TwoHandedStaff) && EquipmentManager.instance.secondaryHand.itemInSlot != null) { //if its two handed and second hand is busy, clear the second hand.
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

        if (w.weaponType == WeaponType.TwoHandedSword || w.weaponType == WeaponType.TwoHandedStaff || w.weaponType == WeaponType.Bow) {  //if weapon is two handed or a bow, return it to initial slot
            initialSlot.AddItem(item, amount, initialSlot); 
            return;
        } else if (w.weaponType == WeaponType.OneHandedSword || w.weaponType == WeaponType.OneHandedStaff) { //if weapon is onehanded, but main hand is already carryign two handed weapon, return to initial slot
            Weapon mw = (Weapon)EquipmentManager.instance.mainHand.itemInSlot;
            if (mw != null && (mw.weaponType == WeaponType.TwoHandedSword || mw.weaponType == WeaponType.TwoHandedStaff) ) {
                initialSlot.AddItem(item, amount, null); 
                return;
            }
        }    
        SharedAdd(item, amount, initialSlot);
        EquipmentManager.instance.EquipWeaponPrefab(w, true);
    }

    void BowAdd (Item item, int amount, UI_InventorySlot initialSlot) {
        if (!(item is Weapon)) {    //If its not weapon, return to initial slot
            initialSlot.AddItem(item, amount, null);
            return;
        }

        Weapon w = (Weapon)item;

        if (w.weaponType != WeaponType.Bow) {   //If its not a bow, return to initial slot
            initialSlot.AddItem(item, amount, null);
            return;
        }

        SharedAdd(item, amount, initialSlot);
        EquipmentManager.instance.EquipWeaponPrefab(w);
    }

    void ArmorAdd (Item item, int amount, UI_InventorySlot initialSlot) {
        if (!(item is Armor)) { //If its not armor, return to initial slot
            initialSlot.AddItem(item, amount, null);
            return;
        }
    
        Armor ar = (Armor)item;
        if (ar.armorType == ArmorType.Necklace || ar.armorType == ArmorType.Ring) { //if its necklace or ring, don't accept it.
            initialSlot.AddItem(item, amount, null);
            return;
        }

        if (equipmentSlotType == EquipmentSlotType.Helmet && ar.armorType != ArmorType.Helmet) {
            initialSlot.AddItem(item, amount, null);
            return;
        } else if (equipmentSlotType == EquipmentSlotType.Chest && ar.armorType != ArmorType.Chest) {
            initialSlot.AddItem(item, amount, null);
            return;
        } else if (equipmentSlotType == EquipmentSlotType.Gloves && ar.armorType != ArmorType.Gloves) {
            initialSlot.AddItem(item, amount, null);
            return;
        } else if (equipmentSlotType == EquipmentSlotType.Pants && ar.armorType != ArmorType.Pants) {
            initialSlot.AddItem(item, amount, null);
            return;
        } else if (equipmentSlotType == EquipmentSlotType.Boots && ar.armorType != ArmorType.Boots) {
            initialSlot.AddItem(item, amount, null);
            return;
        } else if (equipmentSlotType == EquipmentSlotType.Back && ar.armorType != ArmorType.Back) {
            initialSlot.AddItem(item, amount, null);
            return;
        }

        EquipmentManager.instance.EquipArmorVisual(ar);
        SharedAdd(item, amount, initialSlot);
    }

    void NecklaceAdd (Item item, int amount, UI_InventorySlot initialSlot) {
        if (!(item is Armor)) { // If its not armor, return to initial slot
            initialSlot.AddItem(item, amount, null);
            return;
        }

        Armor ar = (Armor)item;
        if (ar.armorType != ArmorType.Necklace) { //if its not necklace, return to initial slot.
            initialSlot.AddItem(item, amount, null);
            return;
        }

        SharedAdd(item, amount, initialSlot);
    }

    void RingAdd (Item item, int amount, UI_InventorySlot initialSlot) {
        if (!(item is Armor)) { // If its not armor, return to initial slot
            initialSlot.AddItem(item, amount, null);
            return;
        }

        Armor ar = (Armor)item;
        if (ar.armorType != ArmorType.Ring) { //if its not ring, return to initial slot.
            initialSlot.AddItem(item, amount, null);
            return;
        }

        SharedAdd(item, amount, initialSlot);
    }

    public override void ClearSlot()
    {
        if (itemInSlot is Weapon) {
            Weapon w = (Weapon)itemInSlot;
            if (w.weaponType == WeaponType.TwoHandedSword || w.weaponType == WeaponType.TwoHandedStaff) {
                EquipmentManager.instance.UnequipWeaponPrefab(true);
            } else if (w.weaponType == WeaponType.OneHandedSword || w.weaponType == WeaponType.OneHandedStaff) {
                if (equipmentSlotType == EquipmentSlotType.MainHand) {
                    EquipmentManager.instance.UnequipWeaponPrefab(false);
                } else {
                    EquipmentManager.instance.UnequipWeaponPrefab(false, true);
                }
            } else if (w.weaponType == WeaponType.Bow) {
                EquipmentManager.instance.UnequipWeaponPrefab(false, false, true);
            }
        } else if (itemInSlot is Armor) {
            if (equipmentSlotType != EquipmentSlotType.Necklace && equipmentSlotType != EquipmentSlotType.Ring)
                EquipmentManager.instance.UnequipArmorVisual((Armor)itemInSlot);
        }
        base.ClearSlot();
        PeaceCanvas.instance.PlaySound(PeaceCanvas.instance.equipItemSound);
    }
}
