using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipAttachementType {Cannons, Sails, Helm, Flag}

[CreateAssetMenu(fileName = "New Ship Attachement", menuName = "Item/Ship Attachement")]
public class ShipAttachement : Item
{
    [Header("Ship Attachement")]
    public ShipAttachementType attachementType;

    public int damage;
    public int range;
    public int speed;
    public int turnPower;

    [Header("Setup")]
    public GameObject prefab;

    public override void Use() {}
    public override void Use(UI_InventorySlot initialSlot) {
        if (!(initialSlot is UI_ShipAttachementSlot))
            Equip(initialSlot);
        else
            Unequip(initialSlot);
    }

    protected virtual void Equip (UI_InventorySlot initialSlot) {
        if (!ShipController.instance) return;
        if (!ShipController.instance.shipAttachements.attachementUIOpen) return;

        ShipController.instance.shipAttachements.AddAttachement(this, initialSlot);
    }
    protected virtual void Unequip (UI_InventorySlot initialSlot) {
        InventoryManager.instance.AddItemToInventory(this, 1);
        initialSlot.ClearSlot();
        ShipController.instance.shipAttachements.RemoveAttachement(attachementType);
    }
}
