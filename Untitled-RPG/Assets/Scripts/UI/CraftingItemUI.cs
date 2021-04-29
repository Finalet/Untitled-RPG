using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingItemUI : MonoBehaviour
{
    public Item item;
    public CraftingNPC parentCraftingNPC;

    [Space]
    public Image itemIcon;
    public TextMeshProUGUI itemTypeLabel;
    public TextMeshProUGUI itemNameLabel;
    public TextMeshProUGUI itemRarityLabel;

    public void Init () {
        if (item == null)
            return;

        itemIcon.sprite = item.itemIcon;
        itemTypeLabel.text = UI_General.getItemType(item);
        itemNameLabel.text = item.itemName;
        itemRarityLabel.text = item.itemRarity.ToString();
        itemRarityLabel.color = UI_General.getRarityColor(item.itemRarity);
        GetComponent<Button>().onClick.AddListener(delegate{parentCraftingNPC.Select(item);});
    }
}
