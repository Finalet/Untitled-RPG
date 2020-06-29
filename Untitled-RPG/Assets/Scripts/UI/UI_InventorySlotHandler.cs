using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InventorySlotHandler : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public int slotID;
    public Item slotObject;

    public Image slotIcon;

    void Start() {
        Load();
    }

    void OnEnable() {
        PeaceCanvas.onSkillsPanelClose += Save;
    }
    void OnDisable() {
        PeaceCanvas.onSkillsPanelClose -= Save;
    }

    void AddItem (Item item) {
        slotObject = item;
        DisplayItem();
    }
    void RemoveItem () {   
        slotObject = null;
        slotIcon.sprite = null;
        slotIcon.color = new Color(0,0,0,0);
    }

    void DisplayItem () {
        slotIcon.sprite = slotObject.itemIcon;
        slotIcon.color = new Color(1,1,1,1);
    }

    public void Save () {
        int itemID = (slotObject != null) ? slotObject.ID : -1; //-1 means its empty
        ES3.Save<int>("slot_" + slotID + "_itemID", itemID, "inventorySlots.txt");
    }
    public void Load () {
        int itemID = ES3.Load<int>("slot_" + slotID + "_itemID", "inventorySlots.txt", -1);
        if (itemID == -1)
            return;
        AddItem(AssetHolder.instance.getItem(itemID));
    }

    
    public void OnBeginDrag (PointerEventData pointerData) {
        if (slotObject == null)
            return;

        PeaceCanvas.instance.StartDraggingItem(GetComponent<RectTransform>().sizeDelta, slotObject.itemIcon, slotObject);

        RemoveItem();
    }

    public void OnDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.DragItem(GetComponent<RectTransform>().sizeDelta);
    }

    public void OnEndDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.EndDrag();
    }

    public void OnDrop (PointerEventData pointerData) {
        if (PeaceCanvas.instance.itemBeingDragged != null) { //Draggin Item
            AddItem(PeaceCanvas.instance.itemBeingDragged);
        }
    }
}
