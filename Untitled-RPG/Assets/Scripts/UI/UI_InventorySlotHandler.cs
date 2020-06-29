using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InventorySlotHandler : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public int slotID;
    public Item itemInSlot;
    public int itemAmount;

    [Header("Do not change")]
    public Image slotIcon;
    public TextMeshProUGUI itemAmountText;

    void Start() {
        Load();
    }

    void OnEnable() {
        PeaceCanvas.onSkillsPanelClose += Save;
    }
    void OnDisable() {
        PeaceCanvas.onSkillsPanelClose -= Save;
    }

    public void AddItem (Item item, int amount, UI_InventorySlotHandler initialSlot) { //Initial slot for switching items places when dropping one onto another
        if (itemInSlot == null) { //Slot is empty
            itemInSlot = item;
            itemAmount = amount;
        } else if (itemInSlot == item && itemInSlot is Consumable) { //Adding the same item
            itemAmount += amount;
        } else { //Switching items places
            initialSlot.AddItem(itemInSlot, itemAmount, null);
            itemInSlot = item;
            itemAmount = amount;
        }
        DisplayItem();
    }
    void RemoveItem () {   
        itemInSlot = null;
        slotIcon.sprite = null;
        slotIcon.color = new Color(0,0,0,0);
        itemAmount = 0;
        itemAmountText.text = "";
    }

    void DisplayItem () {
        slotIcon.sprite = itemInSlot.itemIcon;
        itemAmountText.text = itemAmount.ToString();
        slotIcon.color = new Color(1,1,1,1);
    }

    public void Save () {
        int itemID = (itemInSlot != null) ? itemInSlot.ID : -1; //-1 means its empty
        ES3.Save<int>("slot_" + slotID + "_itemID", itemID, "inventorySlots.txt");
        ES3.Save<int>("slot_" + slotID + "_itemAmount", itemAmount, "inventorySlots.txt");
    }
    public void Load () {
        int itemID = ES3.Load<int>("slot_" + slotID + "_itemID", "inventorySlots.txt", -1);
        int amount = ES3.Load<int>("slot_" + slotID + "_itemAmount", "inventorySlots.txt", 0);
        if (itemID == -1)
            return;
        AddItem(AssetHolder.instance.getItem(itemID), amount, null);
    }

    
    public void OnBeginDrag (PointerEventData pointerData) {
        if (itemInSlot == null)
            return;

        PeaceCanvas.instance.StartDraggingItem(GetComponent<RectTransform>().sizeDelta, itemInSlot, itemAmount, this);
        RemoveItem();
    }

    public void OnDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.DragItem(pointerData.delta.x, pointerData.delta.y);
    }

    public void OnEndDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.EndDrag();
    }

    public void OnDrop (PointerEventData pointerData) {
        if (PeaceCanvas.instance.itemBeingDragged != null) { //Draggin Item
            AddItem(PeaceCanvas.instance.itemBeingDragged, PeaceCanvas.instance.amountOfDraggedItem, PeaceCanvas.instance.initialSlot);
        }
    }
}
