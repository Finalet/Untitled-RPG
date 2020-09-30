using System.Collections;
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

    protected Color baseSlotColor;

    protected string savefilePath; 

    protected virtual void Awake() {
        savefilePath = "saves/inventorySlots.txt";
        if (slotID == -1) slotID = System.Convert.ToInt16(name.Substring(name.IndexOf('(') + 1, 2));
    }

    protected virtual void Start() {
        baseSlotColor = slotIcon.color;
        Load();
    }

    void OnEnable() {
        PeaceCanvas.saveGame += Save;
    }
    void OnDisable() {
        PeaceCanvas.saveGame -= Save;
    }

    public virtual void UseItem () {
        if (itemInSlot is Consumable) {
            Consumable c = (Consumable)itemInSlot;
            if (c.isCoolingDown || !c.canBeUsed())
                return;
            
            itemInSlot.Use();
            StartCoroutine( itemInSlot.UseEnum() );
            itemAmount --;
            if (itemAmount == 0)
                ClearSlot();
        } else if (itemInSlot is Equipment) {
            EquipUnequip();
        }
    }

    public virtual void EquipUnequip () {
        if (itemInSlot is Weapon) {
            Weapon w = (Weapon)itemInSlot;
            if (w.weaponType == WeaponType.OneHanded) {
                if (EquipmentManager.instance.mainHand.itemInSlot == null)
                    EquipmentManager.instance.mainHand.AddItem(itemInSlot, 1, this);
                else if (EquipmentManager.instance.secondaryHand.itemInSlot == null)
                    EquipmentManager.instance.secondaryHand.AddItem(itemInSlot, 1, this);
                else
                    return;
            }
        }
        ClearSlot();
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
        if (itemAmountText != null) itemAmountText.text = "";
    }

    void DisplayItem () {
        slotIcon.sprite = itemInSlot.itemIcon;
        if (itemInSlot is Equipment) itemAmountText.text = "";
        else itemAmountText.text = itemAmount.ToString();
        slotIcon.color = Color.white;
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
    }

    public virtual void OnDrag (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Right)
            return; 

        PeaceCanvas.instance.DragItem(pointerData.delta.x, pointerData.delta.y);
    }

    public virtual void OnEndDrag (PointerEventData pointerData) {
        if(pointerData.button == PointerEventData.InputButton.Right)
            return;

        PeaceCanvas.instance.EndDrag();
    }

    public virtual void OnDrop (PointerEventData pointerData) {
        if (PeaceCanvas.instance.itemBeingDragged != null) { //Dropping Item
            AddItem(PeaceCanvas.instance.itemBeingDragged, PeaceCanvas.instance.amountOfDraggedItem, PeaceCanvas.instance.initialSlot);
        }
    }

    public virtual void OnPointerClick (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Right && itemInSlot != null){
            UseItem();
        }      
    }
}
