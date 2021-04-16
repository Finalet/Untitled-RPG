using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "Item/Consumable")]
public class Consumable : Item
{
    [Header("Consumable")]
    public ConsumableType consumableType;
    public int effectAmount;

    [Space]
    public bool isCoolingDown;
    public int cooldownTime;
    public float cooldownTimer;

    public override void Use () {
        if (consumableType == ConsumableType.Health)
            Characteristics.instance.GetHealed(actualEffect());
        else if (consumableType == ConsumableType.Stamina)
            Characteristics.instance.GetStamina(actualEffect());

        AssetHolder.instance.StartConsumableCooldown(this);
    }
    public override void Use (UI_InventorySlot initialSlot){}

    void OnDisable() {
        isCoolingDown = false;
        cooldownTimer = 0;
    }

    public bool canBeUsed () {
        // Define cases when consumable cannot be used;
        return true;
    }

    int actualEffect () {
        return Mathf.RoundToInt(Random.Range(effectAmount * 0.85f, effectAmount * 1.15f));
    }
}
