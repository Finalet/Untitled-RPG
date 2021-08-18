using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_MountEquipmentSlot : UI_InventorySlot
{
    Image equipmentIcon;
    Color baseColor = new Color(1,1,1, 220f/255f);

    [Space]
    public MountEquipmentType slotEquipmentType;

    void Awake() {
        equipmentIcon = GetComponent<Image>();
    }

    protected override string savefilePath() {
        return SaveManager.instance.getCurrentCharacterFolderPath("equipmentSlots");
    }

    public override void SaveSlot (){
        short ID;
        if (itemInSlot != null)
            ID = (short)itemInSlot.ID;
        else
            ID = -1; //Slot is empty

        ES3.Save<short>($"{slotID}_ID", ID, savefilePath());
    }
    public override void LoadSlot () {
        short ID = ES3.Load<short>($"{slotID}_ID", savefilePath(), -1);

        if (ID < 0) {
            ClearSlot();
            return; //Slot is empty
        }
        AddItem(AssetHolder.instance.getItem(ID), 1, null);
    }

    public override void AddItem (Item item, int amount, UI_InventorySlot initialSlot) {
        if (!(item is MountEquipment)) {    //If its not a mount equipment, return to initial slot
            initialSlot.AddItem(item, amount, null);
            return;
        }

        MountEquipment me = (MountEquipment)item;
        if (slotEquipmentType == MountEquipmentType.Saddle && me.equipmentType != MountEquipmentType.Saddle) { //If its saddle slot, but equipment is not a saddle
            initialSlot.AddItem(item, amount, null);
            return;
        }
        if (slotEquipmentType == MountEquipmentType.Armor && me.equipmentType != MountEquipmentType.Armor) { //If its armor slot, but equipment is not a armor
            initialSlot.AddItem(item, amount, null);
            return;
        }
        
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
        
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.EquipArmor);

        if (PetsManager.instance.isMountOut) PetsManager.instance.currentMountController.UpdateVisuals();
    }

    protected override void DisplayItem() {
        slotIcon.sprite = itemInSlot.itemIcon;
        slotIcon.color = Color.white;
        equipmentIcon.color = transparentColor;

        CheckForSpecialFrame();
    }

    public override void ClearSlot()
    {
        if (equipmentIcon == null) equipmentIcon = GetComponent<Image>();
        equipmentIcon.color = baseColor;

        base.ClearSlot();

        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UnequipArmor);

        if (PetsManager.instance.isMountOut) PetsManager.instance.currentMountController.UpdateVisuals();
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
