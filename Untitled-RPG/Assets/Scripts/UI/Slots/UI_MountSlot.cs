using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_MountSlot : UI_InventorySlot
{
    Image equipmentIcon;
    Color baseColor = new Color(1,1,1, 220f/255f);

    [Header("Mount slot")]
    public UI_MountEquipmentSlot saddleSlot;
    public UI_MountEquipmentSlot armorSlot;

    void Awake() {
        equipmentIcon = GetComponent<Image>();
    }

    public void ToggleHorseEquipmentSltos() {
        saddleSlot.gameObject.SetActive(!saddleSlot.gameObject.activeInHierarchy);
        armorSlot.gameObject.SetActive(!armorSlot.gameObject.activeInHierarchy);
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
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
        if (!(item is Mount) || PlayerControlls.instance.isMounted) {    //If its not a mount or a player is currently mounted, return to initial slot
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
        
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.EquipMount);

        Mount m = (Mount)item;
        PlayerControlls.instance.rider.SetStoredMount(m.mountPrefab);
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

        EquipmentManager.instance.CheckEquipmentBuffs();
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UnequipMount);
        PetsManager.instance.UncallMount();
    }

    public override void OnBeginDrag (PointerEventData pointerData) { //overriding these to not play UI sounds when equiping / unequiping by drag
        if (itemInSlot == null || pointerData.button == PointerEventData.InputButton.Right)
            return;

        if (PlayerControlls.instance.isMounted)  //Don't dragging is player is mounted
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
