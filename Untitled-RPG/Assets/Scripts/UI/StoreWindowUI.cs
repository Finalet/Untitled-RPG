using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class StoreWindowUI : MonoBehaviour
{
    public TradingNPC ownerNPC;
    [Space]
    public GameObject storeItemTemplate;
    public GameObject buyTab;
    public Image buyButtonSprite;
    public Image sellButtonSprite;
    public Sprite greyButton;
    public Sprite redButton;
    public UI_StoreCartSlot[] cartSlots;
    public UI_StoreSellSlot[] sellSlots;
    [Space]
    public Text titleLabel;
    public TextMeshProUGUI cartTotalLabel;
    public TextMeshProUGUI sellTotalLabel;

    public void Init() {
        for (int i = 0; i < ownerNPC.storeItems.Length; i++) {
            StoreItemUI itemUI = Instantiate(storeItemTemplate, buyTab.transform).GetComponent<StoreItemUI>();
            itemUI.item = ownerNPC.storeItems[i];
            itemUI.parentStoreNPC = ownerNPC;
            itemUI.Init();
        }
        Destroy(storeItemTemplate);
        cartTotalLabel.text = ownerNPC.getCartTotalPrice().ToString();
        titleLabel.text = ownerNPC.npcName;
    }

    public void Purchase() {
        ownerNPC.Purchase();
    }
    public void Sell() {
        ownerNPC.Sell();
    }

    void Update() {
        cartTotalLabel.text = ownerNPC.getCartTotalPrice().ToString();
        cartTotalLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(cartTotalLabel.GetPreferredValues().x, 20);

        sellTotalLabel.text = ownerNPC.getTotalSellPrice().ToString();
        sellTotalLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(sellTotalLabel.GetPreferredValues().x, 20);

        buyButtonSprite.sprite = ownerNPC.isBuyWindowOpen ? redButton : greyButton;
        sellButtonSprite.sprite = ownerNPC.isSellWindowOpen ? redButton : greyButton;
        titleLabel.text = ownerNPC.isBuyWindowOpen ? ownerNPC.npcName : "Sell";
    }

    public void PlayUISound (bool buyTab) { //Played акщь  "buy" "sell" tab button components
        if (buyTab && ownerNPC.isBuyWindowOpen)
            return;
        if (!buyTab && ownerNPC.isSellWindowOpen)
            return;
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.UI_Select);
    }
}
