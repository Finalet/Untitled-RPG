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
        //save slot
    }
    public override void Load () {
        //load slot
    }
    public override void AddItem (Item item, int amount, UI_InventorySlot initialSlot) {
        if (equipmentSlotType == EquipmentSlotType.MainHand) {
            if ( !(item is Weapon) ) {
                initialSlot.AddItem(item, amount, null); //if its not weapon, return it to initial slot;
                return;
            }
        } else if (equipmentSlotType == EquipmentSlotType.SecondaryHand) {
            if ( !(item is Weapon) ) {
                initialSlot.AddItem(item, amount, null); //if its not weapon, return it to initial slot;
                return;
            }
        } else {
            initialSlot.AddItem(item, amount, null); //if its not equipment, return it to initial slot
            return;
        }
        itemInSlot = item;
        DisplayItem();
    }

    public override void EquipUnequip()
    {
        print("Unequip by click not implemented yet");
    }

    void DisplayItem() {
        slotIcon.sprite = itemInSlot.itemIcon;
        slotIcon.color = Color.white;
    }
}
