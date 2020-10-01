using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Add Items to Inventory Here")]
    public int itemAmountToAdd = 1;
    public Item itemToAdd;
    public bool add;
    [Space]

    public GameObject slots;
    UI_InventorySlot[] allSlots;

    void Awake() {
        if (instance == null)
            instance = this;
    }

    void Start() {
        allSlots = slots.GetComponentsInChildren<UI_InventorySlot>();
    }

    void FixedUpdate() {
        if (itemToAdd is Equipment)
            itemAmountToAdd = 1;
            
        if (itemToAdd != null && add) {
            AddItemToInventory(itemToAdd, itemAmountToAdd);
            itemAmountToAdd = 1;
            itemToAdd = null;
            add = false;
        }
    }

    public void AddItemToInventory (Item item, int amount, UI_InventorySlot slotToExclude = null) {
        for (int i = 0; i < allSlots.Length; i++) {
            if (slotToExclude != null && slotToExclude == allSlots[i]) {
                continue; 
            }
            if (allSlots[i].itemInSlot == null) {
                allSlots[i].AddItem(item, amount, null);
                break;
            }
        }
    }
}
