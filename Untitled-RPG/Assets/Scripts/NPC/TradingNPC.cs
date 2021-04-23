using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;
using TMPro;

public class TradingNPC : MonoBehaviour
{
    [Header("Store")]
    public string npcName;
    public Item[] storeItems;

    [Space]
    public TextMeshPro npcNameLabel;
    public GameObject storeWindowPrefab;
    public CinemachineVirtualCamera storeCamera;
    public bool isStoreWindowOpen;
    public bool isBuyWindowOpen;
    public bool isSellWindowOpen;
    [Header("Sounds")]
    public AudioClip addToCartSound;
    public AudioClip purchaseSound;
    
    StoreWindowUI instanciatedStoreWindow;
    AudioSource audioSource;
    bool playerDetected;
    bool once;

    void Awake() {
        audioSource = GetComponent<AudioSource>();
        storeItems = storeItems.OrderBy(x => x.itemBasePrice).ToArray();
        npcNameLabel.text = npcName;
    }

    void Update() {
        if (playerDetected && !isStoreWindowOpen) {
            PeaceCanvas.instance.ShowKeySuggestion("F", "Trade");
            once = false;
        } else if (!once) {
            PeaceCanvas.instance.HideKeySuggestion();
            once = true;
        }

        if (playerDetected && Input.GetKeyDown(KeyCode.F)) {
            OpenStoreWindow();
            PeaceCanvas.instance.OpenInventory(true, true);
        }

        isBuyWindowOpen = instanciatedStoreWindow == null ? false : instanciatedStoreWindow.buyTab.activeInHierarchy ? true : false;
        isSellWindowOpen = instanciatedStoreWindow == null ? false : instanciatedStoreWindow.sellSlots[0].gameObject.activeInHierarchy ? true : false;

        if (isBuyWindowOpen || isSellWindowOpen) {
            isStoreWindowOpen = true;
            storeCamera.enabled = true;
        } else {
            isStoreWindowOpen = false;
            storeCamera.enabled = false;
        }

        npcNameLabel.transform.LookAt(PlayerControlls.instance.playerCamera.transform);
    }

    void OpenStoreWindow () {
        if (instanciatedStoreWindow != null)
            return;

        instanciatedStoreWindow = Instantiate(storeWindowPrefab, PeaceCanvas.instance.transform).GetComponent<StoreWindowUI>();
        instanciatedStoreWindow.ownerNPC = this;
        instanciatedStoreWindow.Init();
        PeaceCanvas.instance.currentStoreNPC = this;
        PeaceCanvas.instance.HideKeySuggestion();
    }
    public void CloseStoreWindow (){
        if (instanciatedStoreWindow != null){
            foreach (UI_StoreSellSlot s in instanciatedStoreWindow.sellSlots) {
                if (s.itemInSlot != null)
                    InventoryManager.instance.AddItemToInventory(s.itemInSlot, s.itemAmount);
            }
            Destroy(instanciatedStoreWindow.gameObject);
        }

        if (PeaceCanvas.instance.currentStoreNPC == this)
            PeaceCanvas.instance.currentStoreNPC = null;
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

        InventoryManager.instance.currentGold -= getCartTotalPrice();

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
        InventoryManager.instance.currentGold += getTotalSellPrice();

        foreach (UI_StoreSellSlot s in instanciatedStoreWindow.sellSlots) {
            s.ClearSlot();
        }
        audioSource.clip = purchaseSound;
        audioSource.Play();
    }

    void OnTriggerStay(Collider other) {
        if (other.GetComponent<PlayerControlls>() != null)
            playerDetected = true;
    }
    void OnTriggerExit(Collider other) {
        if (other.GetComponent<PlayerControlls>() != null)
            playerDetected = false;
    }
}
