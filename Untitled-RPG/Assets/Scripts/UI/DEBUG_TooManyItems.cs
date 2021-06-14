using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEBUG_TooManyItems : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform grid;

    void OnEnable() {
        if (grid.childCount != 0)
            return;

        List<Item> allItems = new List<Item>();
        allItems.AddRange(AssetHolder.instance.consumables);
        allItems.AddRange(AssetHolder.instance.weapons);
        allItems.AddRange(AssetHolder.instance.armor);
        allItems.AddRange(AssetHolder.instance.skillbooks);

        foreach (Item item in allItems) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, grid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
    }
}
