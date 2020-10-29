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

    public override void Use () {}
    public override void Use (UI_InventorySlot initialSlot){}

    public override IEnumerator UseEnum() {
        if (consumableType == ConsumableType.HP)
            Characteristics.instance.GetHealed(actualEffect());
        else if (consumableType == ConsumableType.Stamina)
            Characteristics.instance.GetStamina(actualEffect());

        cooldownTimer = cooldownTime;
        while(cooldownTimer > 0) {
            cooldownTimer -= Time.deltaTime;
            isCoolingDown = true;
            yield return null;
        }
        cooldownTimer = 0;
        isCoolingDown = false;
    }

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
