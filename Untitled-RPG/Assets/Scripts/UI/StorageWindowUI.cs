using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageWindowUI : MonoBehaviour
{
    public GameObject slots;
    public Dropdown sortDropDown;
    UI_InventorySlot[] allSlots;

    void Awake() {
        allSlots = slots.GetComponentsInChildren<UI_InventorySlot>();
    }

    public void AddItemToStorage(Item item, int amount) {
        for (int i = 0; i < allSlots.Length; i++) {
            if (allSlots[i].itemInSlot == item && item.isStackable && allSlots[i].itemAmount + amount <= item.maxStackAmount) {
                allSlots[i].AddItem(item, amount, null);
                break;
            }

            if (allSlots[i].itemInSlot == null) {
                allSlots[i].AddItem(item, amount, null);
                break;
            }
        }
    }

    public int getNumberOfEmptySlots () {
        int i = 0;
        foreach (UI_InventorySlot slot in allSlots) {
            if (slot.itemInSlot == null)
                i++;
        }
        return i;
    }

    public void Sort () {
        List<ItemAmountPair> allItemsAmounts = new List<ItemAmountPair>();
        ItemAmountPair x;

        foreach (UI_InventorySlot slot in allSlots) {
            if (slot.itemInSlot != null) {
                x.item1 = slot.itemInSlot;
                x.amount1 = slot.itemAmount;
                allItemsAmounts.Add(x);
                slot.ClearSlot();
            }
        }

        if (sortDropDown.value == 0) { //type
            List<ItemAmountPair> Weapons = new List<ItemAmountPair>();
            List<ItemAmountPair> Armor = new List<ItemAmountPair>();
            List<ItemAmountPair> Skillbooks = new List<ItemAmountPair>();
            List<ItemAmountPair> Consumables = new List<ItemAmountPair>();
            
            for (int i = 0; i < allItemsAmounts.Count; i++) { //Consumables last
                if (allItemsAmounts[i].item1 is Weapon) { 
                    Weapons.Add(allItemsAmounts[i]);
                } else if (allItemsAmounts[i].item1 is Armor) { 
                    Armor.Add(allItemsAmounts[i]);
                } else if (allItemsAmounts[i].item1 is Skillbook) { 
                    Skillbooks.Add(allItemsAmounts[i]);
                } else if (allItemsAmounts[i].item1 is Consumable) { 
                    Consumables.Add(allItemsAmounts[i]);
                }
            }

            Weapons.Sort((p2,p1)=>p1.item1.itemBasePrice.CompareTo(p2.item1.itemBasePrice));
            Armor.Sort((p2,p1)=>p1.item1.itemBasePrice.CompareTo(p2.item1.itemBasePrice));
            Skillbooks.Sort((p2,p1)=>p1.item1.itemBasePrice.CompareTo(p2.item1.itemBasePrice));
            Consumables.Sort((p2,p1)=>p1.item1.itemBasePrice.CompareTo(p2.item1.itemBasePrice));

            allItemsAmounts.Clear();
            allItemsAmounts.AddRange(Weapons);
            allItemsAmounts.AddRange(Armor);
            allItemsAmounts.AddRange(Skillbooks);
            allItemsAmounts.AddRange(Consumables);
        } else if (sortDropDown.value == 1) { //Price
            allItemsAmounts.Sort((p2,p1)=>p1.item1.itemBasePrice.CompareTo(p2.item1.itemBasePrice));
        } else if (sortDropDown.value == 2) { //Rarity
            allItemsAmounts.Sort((p2,p1)=>p1.item1.itemRarity.CompareTo(p2.item1.itemRarity));
        } else if (sortDropDown.value == 3) { //ID
            allItemsAmounts.Sort((p1,p2)=>p1.item1.ID.CompareTo(p2.item1.ID));
        }

        for (int i = 0; i < allItemsAmounts.Count; i++) {
            AddItemToStorage(allItemsAmounts[i].item1, allItemsAmounts[i].amount1);
        }
    }
}
