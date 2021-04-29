using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingNPC : NPC
{
    [Header("Crafting")]
    public Item[] craftingItems;
    public Item selectedItem;
    public int craftQuanitity = 1;

    [Space]
    public GameObject craftingWindowPrefab;

    CraftingWindowUI instanciatedCraftingWindow;

    public override void Interract () {
        base.Interract();
        OpenCraftingWindow();
    }

    public override void StopInterract()
    {
        base.StopInterract();
        CloseCraftingWindow();
    }

    void OpenCraftingWindow() {
        if (instanciatedCraftingWindow != null)
            return;

        craftQuanitity = 1;

        instanciatedCraftingWindow = Instantiate(craftingWindowPrefab, PeaceCanvas.instance.transform).GetComponent<CraftingWindowUI>();
        instanciatedCraftingWindow.ownerNPC = this;
        instanciatedCraftingWindow.Init();

        Select(craftingItems[0]);
    }

    void CloseCraftingWindow (){
        craftQuanitity = 1;
        Destroy(instanciatedCraftingWindow.gameObject);
    }

    public void Select(Item item) {
        selectedItem = item;
        instanciatedCraftingWindow.DisplaySelectedItem();
    }

    public void CraftItem () {
        print($"Crafted {craftQuanitity} of {selectedItem.itemName}");
    }
    public void CancelCraft () {
        print($"Crafted crafting {craftQuanitity} of {selectedItem.itemName}");
    }
}
