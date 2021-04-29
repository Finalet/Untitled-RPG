using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemUI : MonoBehaviour
{
    public Item item;
    public TradingNPC parentStoreNPC;

    [Space]
    public Image itemIcon;
    public TextMeshProUGUI itemTypeLabel;
    public TextMeshProUGUI itemNameLabel;
    public TextMeshProUGUI itemPriceLabel;
    public Button itemBuyButton;

    public void Init () {
        if (item == null)
            return;

        itemIcon.sprite = item.itemIcon;
        string color = ColorUtility.ToHtmlStringRGB(UI_General.getRarityColor(item.itemRarity));
        itemTypeLabel.text = $"<b><color=#{color}>{item.itemRarity.ToString()}</color></b> {UI_General.getItemType(item)}";
        itemNameLabel.text = item.itemName;
        itemPriceLabel.text = item.itemBasePrice.ToString();
        itemPriceLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(itemPriceLabel.GetComponent<TextMeshProUGUI>().preferredWidth, 20);
        itemBuyButton.onClick.AddListener(delegate{parentStoreNPC.AddToCart(item);});
    }
}
