using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Add Items to Inventory Here")]
    public int itemAmountToAdd = 1;
    public Item itemToAdd;
    [Space]
    public InputField inputText;
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

        if (inputText.text != "" && ( Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) )
            AddItemToInventoryDEBUG();
    }

    public void AddItemToInventoryDEBUG () {
        string input = inputText.text;
        if (input.IndexOf('.') != -1 ) {
            itemToAdd = AssetHolder.instance.getItem(int.Parse(input.Substring(0, input.IndexOf('.'))));
            itemAmountToAdd = int.Parse(input.Substring(input.IndexOf('.') + 1));
        } else {
            itemToAdd = AssetHolder.instance.getItem(int.Parse(input));
            itemAmountToAdd = 1;
        }

        if (itemToAdd == null)
            return;
        
        AddItemToInventory (itemToAdd, itemAmountToAdd, null);
        inputText.text = "";
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
