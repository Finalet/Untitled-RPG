using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mount", menuName = "Item/Mounts/Mount")]
public class Mount : Item
{
    [Header("Mount stats")]
    public int movementSpeed;
    public int maxStamina;

    [Space]
    public GameObject mountPrefab;

    public override void Use() {}
    public override void Use(UI_InventorySlot initialSlot) {
        if (PlayerControlls.instance.isMounted || PlayerControlls.instance.rider.IsMountingDismounting)  //Don't allow changes to mounts if player is mounted
            return;    
    
        if (!(initialSlot is UI_MountSlot))
            Equip(initialSlot);
        else
            Unequip(initialSlot);
    }

    protected virtual void Equip (UI_InventorySlot initialSlot) {
        EquipmentManager.instance.mount.AddItem(this, 1, initialSlot);
    }
    protected virtual void Unequip (UI_InventorySlot initialSlot) {
        InventoryManager.instance.AddItemToInventory(this, 1);
        initialSlot.ClearSlot();
    }

    public override int getItemValue()
    {
        if (overridePrice)
            return itemBasePrice;

        float movementSpeedValue = 250;
        float maxStaminaValue = 100;
        float rarityValue = 5000;

        float value = 0;
        value += movementSpeed * movementSpeedValue;
        value += maxStamina * maxStaminaValue;
        value += (int)itemRarity * rarityValue;
        
        return Mathf.RoundToInt(value);
    }

    protected override void OnValidate() {
        base.OnValidate();
        itemBasePrice = getItemValue();
    }
}
