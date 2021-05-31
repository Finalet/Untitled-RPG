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

        foreach (Item item in AssetHolder.instance.consumables) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, grid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.weapons) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, grid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
        foreach (Item item in AssetHolder.instance.armor) {
            DEBUG_UI_TooManyItemsSlot slot = Instantiate(slotPrefab, grid).GetComponent<DEBUG_UI_TooManyItemsSlot>();
            slot.AddItem(item, 1, null);
        }
    }
}
