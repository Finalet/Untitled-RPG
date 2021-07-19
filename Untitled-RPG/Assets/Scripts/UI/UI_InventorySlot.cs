using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InventorySlot : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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

    protected virtual string savefilePath() {
        return "saves/inventorySlots.txt"; ;
    }

    protected void SetSlotID (){
        if (slotID == -1) slotID = System.Convert.ToInt16(name.Substring(name.IndexOf('(') + 1, 2)); //only works for slots after 10, first 10 needs to be assigned manually
    }

    protected virtual void Start() {
        SetSlotID();
    }

    protected virtual void Update () {
        if (PeaceCanvas.instance.isGamePaused)
            return;

        if (itemInSlot != null) {
            DisplayItem();
        }

        ValidateSlot();
    }

    public virtual void ValidateSlot() {
        if ( (itemAmount <= 0 && itemInSlot != null) || (itemAmount > 0 && itemInSlot == null))
            ClearSlot();
        
        if (itemInSlot != null && itemAmount > itemInSlot.maxStackAmount)
            itemAmount = itemInSlot.maxStackAmount;
    }

    public virtual void UseItem () {
        if (itemInSlot == null)
            return;

        if (PeaceCanvas.instance.currentInterractingNPC != null) {  //need to interact with NPC
            if (PeaceCanvas.instance.currentInterractingNPC is TradingNPC) { //If its a trading NPC
                TradingNPC npc = (TradingNPC)PeaceCanvas.instance.currentInterractingNPC;
                if (!npc.isSellWindowOpen)
                    return;
                
                if (npc.getNumberOfEmptySellSlots() == 0)
                    return;

                int amount = UI_General.getClickAmount(itemAmount);

                npc.AddToSell(itemInSlot, amount);
                itemAmount -= amount;
                if (itemAmount == 0)
                    ClearSlot();
                return;
            } else if (PeaceCanvas.instance.currentInterractingNPC is StorageNPC) {
                if (StorageManager.instance.getNumberOfEmptySlots() == 0)
                    return;
                
                int amount = UI_General.getClickAmount(itemAmount);
                    
                StorageManager.instance.AddItemToStorage(itemInSlot, amount);
                itemAmount -= amount;
                if (itemAmount == 0)
                    ClearSlot();
                return;
            }
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
        } else if (itemInSlot is Skillbook) {
            itemInSlot.Use();
        } else if (itemInSlot is Resource) {
            itemInSlot.Use();
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
            cooldownTimerText.text = c.cooldownTimer > 120 ? Mathf.RoundToInt(c.cooldownTimer/60).ToString() + "m" : Mathf.RoundToInt(c.cooldownTimer).ToString();
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

    public virtual void SaveSlot () {
        if (itemInSlot != null) { //Saving item
            BasicSave(1, (short)itemInSlot.ID, (byte)itemAmount);
        } else { //Slot is empty
            BasicSave(0, 0, 0);
        }
    }
    public virtual void LoadSlot () {
        SetSlotID();

        byte type = ES3.Load<byte>($"{slotID}_t", savefilePath(), 0); //Type
        LoadItem(type); 
    }
    public virtual void LoadSlot (byte preloadedType) {
        LoadItem(preloadedType);  
    }

    protected void BasicSave (byte type, short ID, byte amount) {
        ES3.Save<byte>($"{slotID}_t", type, savefilePath()); //Type
        ES3.Save<short>($"{slotID}_ID", ID, savefilePath()); //ID
        ES3.Save<byte>($"{slotID}_a", amount, savefilePath()); //Amount
    }
    void LoadItem(byte type) {
        if (type == 0) { //Empty slot
            ClearSlot();
            return;
        }
        //Add item
        short ID = ES3.Load<short>($"{slotID}_ID", savefilePath(), 0); //ID
        byte amount = ES3.Load<byte>($"{slotID}_a", savefilePath(), 0); //Amount
        AddItem(AssetHolder.instance.getItem(ID), amount, null);
    }

    void OnDisable() {
        TooltipsManager.instance.CancelTooltipRequest(gameObject);
    }

    //--------------------------------Pointer----------------------------------//
    public virtual void OnBeginDrag (PointerEventData pointerData) {
        if (itemInSlot == null || pointerData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.StartDraggingItem(GetComponent<RectTransform>().sizeDelta, itemInSlot, itemAmount, this);
        ClearSlot();
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.GrabItem);
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
            UIAudioManager.instance.PlayUISound(UIAudioManager.instance.DropItem);
        }
    }

    public virtual void OnPointerClick (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Right && itemInSlot != null){
            UseItem();
        }

        if (pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount == 2) {
            UseItem();
        }      
    }

    public virtual void OnPointerEnter (PointerEventData pointerData) {
        if (itemInSlot != null)
            TooltipsManager.instance.RequestTooltip(itemInSlot, gameObject);
    }
    public virtual void OnPointerExit (PointerEventData pointerData) {
        TooltipsManager.instance.CancelTooltipRequest(gameObject);
    }
}
