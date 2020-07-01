using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InventorySlot : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Inventory slot")]
    public int slotID;
    [Space]
    public Item itemInSlot;
    public int itemAmount;

    [Header("Do not edit")]
    public Image slotIcon;
    public TextMeshProUGUI itemAmountText;

    protected string savefilePath; 

    protected virtual void Awake() {
        savefilePath = "saves/inventorySlots.txt";
    }

    protected virtual void Start() {
        Load();
    }

    void OnEnable() {
        PeaceCanvas.onSkillsPanelClose += Save;
    }
    void OnDisable() {
        PeaceCanvas.onSkillsPanelClose -= Save;
    }

    public virtual void AddItem (Item item, int amount, UI_InventorySlot initialSlot) { //Initial slot for switching items places when dropping one onto another
        if (itemInSlot == item && itemInSlot is Consumable) { //Adding the same item
            itemAmount += amount;
            DisplayItem();
            return;
        } else if (itemInSlot != null) { //Slot contains another item
            initialSlot.AddItem(itemInSlot, itemAmount, null);
        }
        itemInSlot = item;
        itemAmount = amount;
        DisplayItem();
    }
    protected virtual void ClearSlot () {   
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

    public virtual void Save () {
        if (itemInSlot != null) { //Saving item
            BasicSave(1, (byte)itemInSlot.ID, (byte)itemAmount);
        } else { //Slot is empty
            BasicSave(0, 0, 0);
        }
    }
    public virtual void Load () {
        byte type = ES3.Load<byte>($"slot_{slotID}_type", savefilePath, 0);
        LoadItem(type); 
    }
    public virtual void Load (byte preloadedType) {
        LoadItem(preloadedType);  
    }

    protected void BasicSave (byte type, byte ID, byte amount) {
        ES3.Save<byte>($"slot_{slotID}_type", type, savefilePath);
        ES3.Save<byte>($"slot_{slotID}_itemID", ID, savefilePath);
        ES3.Save<byte>($"slot_{slotID}_itemAmount", amount, savefilePath);
    }
    void LoadItem(byte type) {
        if (type == 0) { //Empty slot
            ClearSlot();
            return;
        }
        //Add item
        byte ID = ES3.Load<byte>($"slot_{slotID}_itemID", savefilePath, 0);
        byte amount = ES3.Load<byte>($"slot_{slotID}_itemAmount", savefilePath, 0);
        AddItem(AssetHolder.instance.getItem(ID), amount, null);
    }

    //--------------------------------Drag----------------------------------//
    public virtual void OnBeginDrag (PointerEventData pointerData) {
        if (itemInSlot == null)
            return;

        PeaceCanvas.instance.StartDraggingItem(GetComponent<RectTransform>().sizeDelta, itemInSlot, itemAmount, this);
        ClearSlot();
    }

    public virtual void OnDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.DragItem(pointerData.delta.x, pointerData.delta.y);
    }

    public virtual void OnEndDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.EndDrag();
    }

    public virtual void OnDrop (PointerEventData pointerData) {
        if (PeaceCanvas.instance.itemBeingDragged != null) { //Dropping Item
            AddItem(PeaceCanvas.instance.itemBeingDragged, PeaceCanvas.instance.amountOfDraggedItem, PeaceCanvas.instance.initialSlot);
        }
    }
}
