using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DEBUG_UI_TooManyItemsSlot : UI_InventorySlot, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    protected override void Awake() {}
    protected override void Start() {}

    public override void UseItem()
    {
        if (InventoryManager.instance.getNumberOfEmptySlots() <= 0) {
            CanvasScript.instance.DisplayWarning("Inventory is full");
            return;
        }
            
        int amount = itemInSlot.isStackable ? UI_General.getClickAmount() : 1;
        InventoryManager.instance.AddItemToInventory(itemInSlot, amount, null);
    }

    public override void OnBeginDrag (PointerEventData pointerData) {
        //
    }

    public override void OnDrag (PointerEventData pointerData) {
        //
    }

    public override void OnEndDrag (PointerEventData pointerData) {
        //
    }

    public override void OnDrop (PointerEventData pointerData) {
        //
    }

    public override void OnPointerClick (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Right && itemInSlot != null){
            UseItem();
        }      
    }
}
