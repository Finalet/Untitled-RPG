using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StoreCartSlot : UI_InventorySlot, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    protected override void Start () {
        //do nothing
    }
    public override void Save () {
        //do nothing
    }

    public override void OnBeginDrag (PointerEventData pointerData) {
        //do nothing
    }

    public override void OnDrag (PointerEventData pointerData) {
        //do nothing
    }

    public override void OnEndDrag (PointerEventData pointerData) {
        //do nothing
    }

    public override void OnDrop (PointerEventData pointerData) {
        //do nothing
    }

    public override void UseItem()
    {
        ClearSlot();
    }
}
