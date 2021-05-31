using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public int currentGold;
    [Space]
    public InputField goldInputText;
    public InputField itemInputText;
    [Space]
    public TextMeshProUGUI currentGoldLabel;
    public GameObject slots;
    UI_InventorySlot[] allSlots;

    //DEBUG 
    int itemAmountToAdd = 1;
    Item itemToAdd;
    
    protected string savefilePath = "saves/currency.txt";

    public InventoryManager () {
        if (instance == null)
            instance = this;
    }

    public void Init() {
        if (instance == null)
            instance = this;
        
        PeaceCanvas.saveGame += Save;

        Load();

        allSlots = slots.GetComponentsInChildren<UI_InventorySlot>();
    }

    void Update() {
        if (itemToAdd is Equipment)
            itemAmountToAdd = 1;

        if (goldInputText.text != "" && ( Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) )
            AddGoldDEBUG();

        currentGoldLabel.text = currentGold.ToString();
        currentGoldLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(currentGoldLabel.GetComponent<TextMeshProUGUI>().textBounds.size.x, 20);
    }

    void Save () {
        ES3.Save<int>("currentGold", currentGold, savefilePath);
    }
    void Load () {
        if (ES3.FileExists(savefilePath))
            currentGold = ES3.Load<int>("currentGold", savefilePath);
    }

    public void AddGoldDEBUG () {
        currentGold += int.Parse(goldInputText.text);
        goldInputText.text = "";
    }

    public void AddItemToInventory (Item item, int amount, UI_InventorySlot slotToExclude = null) {
        for (int i = 0; i < allSlots.Length; i++) {
            if (slotToExclude != null && slotToExclude == allSlots[i]) {
                continue; 
            }
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

    public void AddGold(int amount) {
        currentGold += amount;
    }
    public void RemoveGold (int amount) {
        currentGold -= amount;
    }

    public int getNumberOfEmptySlots () {
        int i = 0;
        foreach (UI_InventorySlot slot in allSlots) {
            if (slot.itemInSlot == null)
                i++;
        }
        return i;
    }

    public int getItemAmountInInventory (Item item){
        int amount = 0;
        for (int i = 0; i < allSlots.Length; i++) {
            if (allSlots[i].itemInSlot == item)
                amount += allSlots[i].itemAmount;
        }
        return amount;
    }

    public void RemoveItemFromInventory (Item item, int amount) {
        int amountLeftToRemove = amount;
        for (int i = 0; i < allSlots.Length; i++) {
            if (allSlots[i].itemInSlot == item) {
                int amountInThisSlot = allSlots[i].itemAmount;
                allSlots[i].itemAmount -= amountLeftToRemove;
                amountLeftToRemove -= amountInThisSlot;
                allSlots[i].ValidateSlot();
            }
            if (amountLeftToRemove <= 0)
                break;
        }
    }
}
