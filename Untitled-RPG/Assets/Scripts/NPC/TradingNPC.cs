using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;
using TMPro;

public enum SortItemsBy {Price, Rarity, ID};

public class TradingNPC : NPC
{
    [Header("Store")]
    public Item[] storeItems;
    public SortItemsBy sortItemsBy;

    [Space]
    public GameObject storeWindowPrefab;
    public bool isBuyWindowOpen;
    public bool isSellWindowOpen;
    [Header("Sounds")]
    public AudioClip openStoreSound;
    public AudioClip closeStoreSound;
    public AudioClip addToCartSound;
    public AudioClip purchaseSound;
    
    StoreWindowUI instanciatedStoreWindow;

    protected override void Awake() {
        base.Awake();
        switch (sortItemsBy){
            case SortItemsBy.Price:
                storeItems = storeItems.OrderBy(x => x.itemBasePrice).ToArray();
                break;
            case SortItemsBy.Rarity:
                storeItems = storeItems.OrderBy(x => x.itemRarity).ToArray();
                break;
            case SortItemsBy.ID:
                storeItems = storeItems.OrderBy(x => x.ID).ToArray();
                break;
        }
    }

    protected override void Update() {
        base.Update();

        isBuyWindowOpen = instanciatedStoreWindow == null ? false : instanciatedStoreWindow.buyTab.activeInHierarchy ? true : false;
        isSellWindowOpen = instanciatedStoreWindow == null ? false : instanciatedStoreWindow.sellSlots[0].gameObject.activeInHierarchy ? true : false;
    }

    protected override void CustomInterract()
    {
        OpenStoreWindow();
        PeaceCanvas.instance.OpenInventory(true, true, false);
    }
    protected override void CustomStopInterract()
    {
        CloseStoreWindow();
    }

    void OpenStoreWindow () {
        if (instanciatedStoreWindow != null)
            return;

        instanciatedStoreWindow = Instantiate(storeWindowPrefab, PeaceCanvas.instance.transform).GetComponent<StoreWindowUI>();
        instanciatedStoreWindow.ownerNPC = this;
        instanciatedStoreWindow.Init();
        audioSource.clip = openStoreSound;
        audioSource.Play();
    }
    void CloseStoreWindow (){
        if (instanciatedStoreWindow != null){
            foreach (UI_StoreSellSlot s in instanciatedStoreWindow.sellSlots) {
                if (s.itemInSlot != null)
                    InventoryManager.instance.AddItemToInventory(s.itemInSlot, s.itemAmount);
            }
            Destroy(instanciatedStoreWindow.gameObject);
        }
        audioSource.clip = closeStoreSound;
        audioSource.Play();
    }

    public int getCartTotalPrice () {
        int _cartTotalPrice = 0;
        foreach (UI_StoreCartSlot slot in instanciatedStoreWindow.cartSlots) {
            if (slot.itemInSlot != null)
                _cartTotalPrice += slot.itemInSlot.itemBasePrice * slot.itemAmount;
        }
        return _cartTotalPrice;
    }

    public int getTotalSellPrice () {
        int _TotalSellPrice = 0;
        foreach (UI_StoreSellSlot slot in instanciatedStoreWindow.sellSlots) {
            if (slot.itemInSlot != null)
                _TotalSellPrice += slot.itemInSlot.itemBasePrice * slot.itemAmount;
        }
        return _TotalSellPrice;
    }


    public void AddToCart (Item item) {
        int amount = 1;
        if(item.isStackable)
            amount = Input.GetKey(KeyCode.LeftControl) ? 100 : Input.GetKey(KeyCode.LeftShift) ? 10 : amount;
        
        foreach (UI_StoreCartSlot s in instanciatedStoreWindow.cartSlots) {
            if (s.itemInSlot == item && item.isStackable && s.itemAmount + amount <= item.maxStackAmount) {
                s.itemAmount += amount;
                audioSource.clip = addToCartSound;
                audioSource.Play();
                return;
            }
            if (s.itemInSlot == null) {
                s.itemInSlot = item;
                s.itemAmount = amount;
                audioSource.clip = addToCartSound;
                audioSource.Play();
                return;
            }
        }
    }

    public void AddToSell (Item item, int amount = 1) {
        foreach (UI_StoreSellSlot s in instanciatedStoreWindow.sellSlots) {
            if (s.itemInSlot == item && item.isStackable && s.itemAmount + amount <= item.maxStackAmount) {
                s.itemAmount += amount;
                audioSource.clip = addToCartSound;
                audioSource.Play();
                return;
            }
            if (s.itemInSlot == null) {
                s.itemInSlot = item;
                s.itemAmount = amount;
                audioSource.clip = addToCartSound;
                audioSource.Play();
                return;
            }
        }
    }

    int getNumberOfItemsInCart () {
        int i = 0;
        foreach (UI_StoreCartSlot s in instanciatedStoreWindow.cartSlots) {
            if (s.itemInSlot != null)
                i++;
        }
        return i;
    }

    public int getNumberOfEmptySellSlots () {
        int i = 0;
        foreach (UI_StoreSellSlot slot in instanciatedStoreWindow.sellSlots) {
            if (slot.itemInSlot == null)
                i++;
        }
        return i;
    }

    public void Purchase () {
        if (InventoryManager.instance.currentGold <= getCartTotalPrice()) {
            CanvasScript.instance.DisplayWarning("Not enough gold");
            return;
        }

        if (InventoryManager.instance.getNumberOfEmptySlots() < getNumberOfItemsInCart()) {
            CanvasScript.instance.DisplayWarning("Inventory is full");
            return;
        }

        InventoryManager.instance.RemoveGold(getCartTotalPrice());

        foreach (UI_StoreCartSlot s in instanciatedStoreWindow.cartSlots) {
            if (s.itemInSlot != null) {
                InventoryManager.instance.AddItemToInventory(s.itemInSlot, s.itemAmount);
                s.ClearSlot();
            }
        }
        audioSource.clip = purchaseSound;
        audioSource.Play();
    }

    public void Sell () {
        InventoryManager.instance.AddGold(getTotalSellPrice());

        foreach (UI_StoreSellSlot s in instanciatedStoreWindow.sellSlots) {
            s.ClearSlot();
        }
        audioSource.clip = purchaseSound;
        audioSource.Play();
    }
}
