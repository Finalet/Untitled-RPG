﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InventorySlot : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Inventory slot")]
    public int slotID;
    [Space]
    public Item itemInSlot;
    public int itemAmount;

    [Header("Do not edit")]
    public Image slotIcon;
    public TextMeshProUGUI itemAmountText;
    public Image cooldownImage;
    public TextMeshProUGUI cooldownTimerText;

    protected string savefilePath; 

    protected virtual void Awake() {
        savefilePath = "saves/inventorySlots.txt";
        if (slotID == -1) slotID = System.Convert.ToInt16(name.Substring(name.IndexOf('(') + 1, 2));
    }

    protected virtual void Start() {
        Load();
    }

    protected virtual void Update () {
        if (PeaceCanvas.instance.isGamePaused)
            return;

        if (itemInSlot != null)
            DisplayItem();
    }

    void OnEnable() {
        PeaceCanvas.saveGame += Save;
    }
    void OnDisable() {
        PeaceCanvas.saveGame -= Save;
    }

    public virtual void UseItem () {
        if (PeaceCanvas.instance.currentStoreNPC != null && PeaceCanvas.instance.currentStoreNPC.isSellWindowOpen) { //Store window is open, need to sell item
            if (PeaceCanvas.instance.currentStoreNPC.getNumberOfEmptySellSlots() == 0)
                return;

            int amount = 1;
            if(itemAmount > 1)
                amount = Input.GetKey(KeyCode.LeftControl) ? (itemAmount >= 100 ? 100 : itemAmount) : Input.GetKey(KeyCode.LeftShift) ? (itemAmount >= 10 ? 10 : itemAmount) : amount;

            PeaceCanvas.instance.currentStoreNPC.AddToSell(itemInSlot, amount);
            itemAmount -= amount;
            if (itemAmount == 0)
                ClearSlot();
            return;
        }

        if (itemInSlot is Consumable) {
            Consumable c = (Consumable)itemInSlot;
            if (c.isCoolingDown || !c.canBeUsed())
                return;
            
            itemInSlot.Use();
            itemAmount --;
            if (itemAmount == 0)
                ClearSlot();
        } else if (itemInSlot is Equipment) {
            itemInSlot.Use(this); //"this" to let the item clear the current a slot if item was equiped
        }
    }

    public virtual void AddItem (Item item, int amount, UI_InventorySlot initialSlot) { //Initial slot for switching items places when dropping one onto another
        if (itemInSlot == item && itemInSlot.isStackable) { //Adding the same item
            if (itemAmount + amount <= itemInSlot.maxStackAmount) {
                itemAmount += amount;
                DisplayItem();
                return;
            } else {
                int amountToCompleteStack = itemInSlot.maxStackAmount - itemAmount;
                itemAmount += amountToCompleteStack;
                DisplayItem();
                initialSlot.AddItem(item, amount - amountToCompleteStack, null);
                return;
            }
        } else if (itemInSlot != null) { //Slot contains another item
            initialSlot.AddItem(itemInSlot, itemAmount, null);
        }
        itemInSlot = item;
        itemAmount = amount;
        DisplayItem();
    }
    public virtual void ClearSlot () {   
        //Clear item in slot
        itemInSlot = null;
        itemAmount = 0;
        slotIcon.sprite = null;
        slotIcon.color = new Color(0,0,0,0);
        if (itemAmountText != null) itemAmountText.text = "";
        
        //Clear cooldown
        if (cooldownImage == null) return; //Stop if no cooldown in the slot (like equipment slots)
        cooldownImage.color = new Color(0,0,0,0);
        cooldownImage.fillAmount = 1;
        cooldownTimerText.text = "";
    }

    protected virtual void DisplayItem () {
        slotIcon.sprite = itemInSlot.itemIcon;
        itemAmountText.text = itemAmount == 1 ? "" : itemAmount.ToString();
        slotIcon.color = Color.white;

        if (!(itemInSlot is Consumable))
            return;

        Consumable c = (Consumable)itemInSlot;
        if(c.isCoolingDown) {
            cooldownImage.color = new Color(0, 0, 0, 0.8f);
            cooldownTimerText.text = Mathf.RoundToInt(c.cooldownTimer).ToString();
        } else {
            cooldownImage.color = new Color(0, 0, 0, 0);
            cooldownTimerText.text = "";
        }
        if (c.canBeUsed()) {
            slotIcon.color = Color.white;
        } else {
            slotIcon.color = new Color (0.3f, 0.3f, 0.3f, 1);
        }
    }

    public virtual void Save () {
        if (itemInSlot != null) { //Saving item
            BasicSave(1, (short)itemInSlot.ID, (byte)itemAmount);
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

    protected void BasicSave (byte type, short ID, byte amount) {
        ES3.Save<byte>($"slot_{slotID}_type", type, savefilePath);
        ES3.Save<short>($"slot_{slotID}_itemID", ID, savefilePath);
        ES3.Save<byte>($"slot_{slotID}_itemAmount", amount, savefilePath);
    }
    void LoadItem(byte type) {
        if (type == 0) { //Empty slot
            ClearSlot();
            return;
        }
        //Add item
        short ID = ES3.Load<short>($"slot_{slotID}_itemID", savefilePath, 0);
        byte amount = ES3.Load<byte>($"slot_{slotID}_itemAmount", savefilePath, 0);
        AddItem(AssetHolder.instance.getItem(ID), amount, null);
    }

    //--------------------------------Drag----------------------------------//
    public virtual void OnBeginDrag (PointerEventData pointerData) {
        if (itemInSlot == null || pointerData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.StartDraggingItem(GetComponent<RectTransform>().sizeDelta, itemInSlot, itemAmount, this);
        ClearSlot();
        PeaceCanvas.instance.PlaySound(PeaceCanvas.instance.grabItemSound);
    }

    public virtual void OnDrag (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Right)
            return; 

        PeaceCanvas.instance.DragItem(pointerData.position);
    }

    public virtual void OnEndDrag (PointerEventData pointerData) {
        if(pointerData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.EndDrag();
    }

    public virtual void OnDrop (PointerEventData pointerData) {
        if (PeaceCanvas.instance.itemBeingDragged != null) { //Dropping Item
            PeaceCanvas.instance.dragSuccess = true;
            AddItem(PeaceCanvas.instance.itemBeingDragged, PeaceCanvas.instance.amountOfDraggedItem, PeaceCanvas.instance.initialSlot);
            PeaceCanvas.instance.PlaySound(PeaceCanvas.instance.dropItemSound);
        }
    }

    public virtual void OnPointerClick (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Right && itemInSlot != null){
            UseItem();
        }      
    }
}
