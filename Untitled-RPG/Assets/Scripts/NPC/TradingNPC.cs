using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class TradingNPC : MonoBehaviour
{
    [Header("Store Items")]
    public Item[] storeItems;

    [Space]
    public GameObject storeWindowPrefab;
    public bool isStoreWindowOpen;
    [Space]
    public float playerDetectRadius;
    [Header("Sounds")]
    public AudioClip addToCartSound;
    public AudioClip purchaseSound;
    
    GameObject instanciatedStoreWindow;
    GameObject storeItemTemplate;
    TextMeshProUGUI cartTotalLabel;
    UI_StoreCartSlot[] cartSlots;
    AudioSource audioSource;
    bool playerDetected;
    bool once;

    void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, playerDetectRadius);
        foreach (Collider col in collidersInRadius) {
            if (col.GetComponent<PlayerControlls>() != null) {
                playerDetected = true;
                break;
            }
            playerDetected = false;
        }

        if (playerDetected && !isStoreWindowOpen) {
            PeaceCanvas.instance.ShowKeySuggestion("F");
            once = false;
        } else if (!once) {
            PeaceCanvas.instance.HideKeySuggestion();
            once = true;
        }

        if (playerDetected && Input.GetKeyDown(KeyCode.F)) {
            OpenStoreWindow();
            PeaceCanvas.instance.OpenInventory();
        }

        if (isStoreWindowOpen) {
            cartTotalLabel.text = getCartTotalPrice().ToString();
            cartTotalLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(cartTotalLabel.GetComponent<TextMeshProUGUI>().GetPreferredValues().x, 20);
        }
    }

    void OpenStoreWindow () {
        if (instanciatedStoreWindow != null)
            return;

        isStoreWindowOpen = true;
        instanciatedStoreWindow = Instantiate(storeWindowPrefab, PeaceCanvas.instance.transform);
        PopulateStoreUI();
        PeaceCanvas.instance.currentStoreNPC = this;
        PeaceCanvas.instance.HideKeySuggestion();
    }
    public void CloseStoreWindow (){
        if (instanciatedStoreWindow != null)
            Destroy(instanciatedStoreWindow);

        if (PeaceCanvas.instance.currentStoreNPC == this)
            PeaceCanvas.instance.currentStoreNPC = null;

        isStoreWindowOpen = false;
        cartSlots = null;
    }

    void PopulateStoreUI (){
        storeItemTemplate = instanciatedStoreWindow.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        for (int i = 0; i < storeItems.Length; i++) {
            StoreItemUI itemUI = Instantiate(storeItemTemplate, instanciatedStoreWindow.transform.GetChild(0).GetChild(0)).GetComponent<StoreItemUI>();
            itemUI.item = storeItems[i];
            itemUI.parentStoreNPC = this;
            itemUI.Init();
        }
        Destroy(storeItemTemplate);
        cartSlots = instanciatedStoreWindow.transform.GetComponentsInChildren<UI_StoreCartSlot>();
        cartTotalLabel = instanciatedStoreWindow.transform.Find("Cart/Cart total/Cart total label").GetComponent<TextMeshProUGUI>();
        cartTotalLabel.text = getCartTotalPrice().ToString();
        instanciatedStoreWindow.transform.Find("Cart/Purchase button").GetComponent<Button>().onClick.AddListener(delegate{Purchase();});
    }

    int getCartTotalPrice () {
        int _cartTotalPrice = 0;
        foreach (UI_StoreCartSlot slot in cartSlots) {
            if (slot.itemInSlot != null)
                _cartTotalPrice += slot.itemInSlot.itemBasePrice * slot.itemAmount;
        }
        return _cartTotalPrice;
    }

    public void AddToCart (Item item) {
        int amount = 1;
        if(item is Consumable)
            amount = Input.GetKey(KeyCode.LeftControl) ? 100 : Input.GetKey(KeyCode.LeftShift) ? 10 : amount;
        
        foreach (UI_StoreCartSlot s in cartSlots) {
            if (s.itemInSlot == item && item is Consumable && s.itemAmount + amount <= 100) {
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
        foreach (UI_StoreCartSlot s in cartSlots) {
            if (s.itemInSlot != null)
                i++;
        }
        return i;
    }

    void Purchase () {
        if (InventoryManager.instance.currentGold <= getCartTotalPrice()) {
            CanvasScript.instance.DisplayWarning("Not enough gold");
            return;
        }

        if (InventoryManager.instance.getNumberOfEmptySlots() < getNumberOfItemsInCart()) {
            CanvasScript.instance.DisplayWarning("Inventory is full");
            return;
        }

        InventoryManager.instance.currentGold -= getCartTotalPrice();

        foreach (UI_StoreCartSlot s in cartSlots) {
            if (s.itemInSlot != null) {
                InventoryManager.instance.AddItemToInventory(s.itemInSlot, s.itemAmount);
                s.ClearSlot();
            }
        }
        audioSource.clip = purchaseSound;
        audioSource.Play();
    }
}
