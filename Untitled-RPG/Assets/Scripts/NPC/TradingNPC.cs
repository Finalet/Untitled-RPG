using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradingNPC : MonoBehaviour
{
    [Header("Store Items")]
    public Item[] storeItems;

    [Space]
    public GameObject storeWindowPrefab;
    public bool isStoreWindowOpen;
    GameObject instanciatedStoreWindow;
    GameObject storeItemTemplate;
    UI_StoreCartSlot[] cartSlots;
    [Space]
    public float playerDetectRadius;
    bool playerDetected;
    bool once;

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
    }

    public void AddToCart (Item item) {
        for (int i = 0; i < cartSlots.Length; i++) {
            if (cartSlots[i].itemInSlot == null){
                cartSlots[i].itemInSlot = item;
                cartSlots[i].itemAmount = 1;
                return;
            }
        }
    }
}
