using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType {CraftingResource, QuestItem, Misc}

[CreateAssetMenu(fileName = "New Resource", menuName = "Item/Resource")]
public class Resource : Item
{
    [Header("Resource")]
    public ResourceType resourceType;

    public override void Use() {}
    public override void Use(UI_InventorySlot initialSlot) {}
}
