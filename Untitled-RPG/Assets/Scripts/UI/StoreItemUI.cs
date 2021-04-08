using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemUI : MonoBehaviour
{
    public Item item;
    public TradingNPC parentStoreNPC;

    [Space]
    public Image itemIcon;
    public TextMeshProUGUI itemNameLabel;
    public TextMeshProUGUI itemPriceLabel;
    public Button itemBuyButton;

    public void Init () {
        if (item == null)
            return;

        itemIcon.sprite = item.itemIcon;
        itemNameLabel.text = item.itemName;
        itemPriceLabel.text = "123123";
        itemBuyButton.onClick.AddListener(delegate{parentStoreNPC.AddToCart(item);});
    }
}
