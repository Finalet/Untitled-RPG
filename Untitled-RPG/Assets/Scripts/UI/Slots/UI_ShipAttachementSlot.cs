using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ShipAttachementSlot : UI_InventorySlot
{
    [Header("Equipment slot")]
    public ShipAttachementType attchementSlotType;
    Image equipmentIcon;

    Color baseColor = new Color(1,1,1, 220f/255f);

     void Awake() {
        equipmentIcon = GetComponent<Image>();
    }

    protected override string savefilePath() {
        //return SaveManager.instance.getCurrentCharacterFolderPath("equipmentSlots");
        return "";
    } 

    public override void SaveSlot (){
        
    }
    public override void LoadSlot () {
        
    }
    public override void AddItem (Item item, int amount, UI_InventorySlot initialSlot) {
        if ( !(item is ShipAttachement) ) {  //if its not ship attachement, return it to initial slot;
            initialSlot.AddItem(item, amount, null); 
            return;
        }

        ShipAttachement sa = (ShipAttachement)item;

        if (attchementSlotType != sa.attachementType) {
            initialSlot.AddItem(item, amount, null); //if wrong slot / type, return back.
            return;   
        }
        SharedAdd(item, amount, initialSlot);
        //Turn on visuals;
    }

    protected override void DisplayItem() {
        slotIcon.sprite = itemInSlot.itemIcon;
        slotIcon.color = Color.white;
        equipmentIcon.color = transparentColor;

        CheckForSpecialFrame();
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
        
        //Play sound
    }

    
    public override void ClearSlot()
    {
        //Play sound
        //Remove visuals;

        if (equipmentIcon == null) equipmentIcon = GetComponent<Image>();
        equipmentIcon.color = baseColor;

        base.ClearSlot();
    }

    public override void OnBeginDrag (PointerEventData pointerData) { //overriding these to not play UI sounds when equiping / unequiping by drag
        if (itemInSlot == null || pointerData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.StartDraggingItem(GetComponent<RectTransform>().sizeDelta, itemInSlot, itemAmount, this);
        ClearSlot();
        //UIAudioManager.instance.PlayUISound(UIAudioManager.instance.GrabItem);
    }
    public override void OnDrop (PointerEventData pointerData) {
        if (PeaceCanvas.instance.itemBeingDragged != null) { //Dropping Item
            PeaceCanvas.instance.dragSuccess = true;
            AddItem(PeaceCanvas.instance.itemBeingDragged, PeaceCanvas.instance.amountOfDraggedItem, PeaceCanvas.instance.initialSlot);
            //UIAudioManager.instance.PlayUISound(UIAudioManager.instance.DropItem);
        }
    }
}
