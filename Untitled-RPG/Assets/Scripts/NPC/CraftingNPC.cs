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

    public bool canCraftItem () {
        for (int i = 0; i < selectedItem.craftingRecipe.Length; i++) {
            if (InventoryManager.instance.getItemAmountInInventory(selectedItem.craftingRecipe[i].resource) < selectedItem.craftingRecipe[i].requiredAmount*craftQuanitity)
                return false;       
        }
        return true;
    }

    public void CraftItem () {
        if (canCraftItem()) {
            if (InventoryManager.instance.getNumberOfEmptySlots() == 0) {
                CanvasScript.instance.DisplayWarning("Inventory is full");
                return;
            }
                

            for (int i = 0; i < selectedItem.craftingRecipe.Length; i++) {
                InventoryManager.instance.RemoveItemFromInventory(selectedItem.craftingRecipe[i].resource, selectedItem.craftingRecipe[i].requiredAmount*craftQuanitity);      
            }
            InventoryManager.instance.AddItemToInventory(selectedItem, craftQuanitity);
            instanciatedCraftingWindow.DisplaySelectedItem(); 
        }
    }
    
    public void CancelCraft () {
        print($"Canceled crafting {craftQuanitity} of {selectedItem.itemName}");
    }
}
