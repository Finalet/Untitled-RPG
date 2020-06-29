using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "Item/Consumable")]
public class Consumable : Item
{
    [Header("Consumable")]
    public ConsumableType consumableType;
    public float effectAmount;

    public override void Use () {
        Debug.Log($"Used consumable {itemName}: {consumableType} restored by {effectAmount} points");
    }
}
