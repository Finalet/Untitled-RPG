using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{

    public Item focusItem;
    [Header("Colors")]
    public Color baseTextColor;
    public Color secondaryTextColor;
    public Color highlightColor;

    [Space]
    public Image itemIcon;
    public TextMeshProUGUI itemID;
    public TextMeshProUGUI itemType;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemRarity;
    public TextMeshProUGUI itemDescription;
    public TextMeshProUGUI itemStats;
    public TextMeshProUGUI itemPrice;

    void Awake() {
        itemType.color = secondaryTextColor;
        itemName.color = baseTextColor;
        itemDescription.color = secondaryTextColor;
    }

    void Update()
    {
        Init();

        if (!PeaceCanvas.instance.anyPanelOpen)
            Destroy(gameObject);
    }

    public void Init () {
        if (itemID.text != focusItem.ID.ToString())
            itemID.text = focusItem.ID.ToString();

        if (itemIcon.sprite != focusItem.itemIcon)
            itemIcon.sprite = focusItem.itemIcon;

        if (itemType.text != UI_General.getItemType(focusItem))
            itemType.text = UI_General.getItemType(focusItem);

        if (itemName.text != focusItem.itemName)
            itemName.text = focusItem.itemName;
        
        if (itemRarity.text != focusItem.itemRarity.ToString()) {
            itemRarity.text = focusItem.itemRarity.ToString();
            itemRarity.color = UI_General.getRarityColor(focusItem.itemRarity);
        }

        if (itemDescription.text != focusItem.itemDesctription)
            itemDescription.text = focusItem.itemDesctription;

        if (itemPrice.text != focusItem.itemBasePrice.ToString())
            itemPrice.text = focusItem.itemBasePrice.ToString();

        if (itemStats.text != generateItemStats(focusItem))
            itemStats.text = generateItemStats(focusItem);

        float tooltipHeight = 120 +
            (itemDescription.preferredHeight <= 0 ? 0 : itemDescription.preferredHeight) +
            (itemStats.preferredHeight <= 0 ? 0 : 38 + itemStats.preferredHeight);
        GetComponent<RectTransform>().sizeDelta = new Vector2(360, tooltipHeight);

        float statsPos = itemDescription.GetComponent<RectTransform>().anchoredPosition.y - itemDescription.preferredHeight - 20;
        itemStats.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, statsPos);

        itemPrice.GetComponent<RectTransform>().sizeDelta = new Vector2(itemPrice.GetComponent<TextMeshProUGUI>().preferredWidth, 20);
    }

    string generateItemStats (Item item) {
        string stats = "";
        if (item is Consumable) {
            Consumable c = (Consumable)item;
            if (c.consumableType == ConsumableType.Health) {
                stats = $"Restores around {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), "+" + c.effectAmount.ToString())} health."; 
            } else if (c.consumableType == ConsumableType.Stamina) {
                stats = $"Restores around {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), "+" + c.effectAmount.ToString())} stamina.";
            }
        } else if (item is Equipment) {
            Equipment w = (Equipment)item;
            if (w.MeleeAttack != 0) {
                stats += $"Melee attack: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.MeleeAttack.ToString())}\n";
            }
            if (w.RangedAttack != 0) {
                stats += $"Ranged attack: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.RangedAttack.ToString())}\n";
            }
            if (w.MagicPower != 0) {
                stats += $"Magic power: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.MagicPower.ToString())}\n";
            }
            if (w.HealingPower != 0) {
                stats += $"Healing power: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.HealingPower.ToString())}\n";
            }
            if (w.Defense != 0) {
                stats += $"Defense: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.Defense.ToString())}\n";
            }
            if (w.Health != 0 || w.Stamina != 0) {
                stats += "\n";
            }
            if (w.Health != 0) {
                stats += $"Health: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.Health.ToString())}\n";
            }
            if (w.Stamina != 0) {
                stats += $"Stamina: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.Stamina.ToString())}\n";
            }
            if (w.strength != 0 || w.agility != 0 || w.intellect != 0) {
                stats += "\n";
            }
            if (w.strength != 0) {
                stats += $"Strength: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.strength.ToString())}\n";
            }
            if (w.agility != 0) {
                stats += $"Agility: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.agility.ToString())}\n";
            }
            if (w.intellect != 0) {
                stats += $"Intellect: {string.Format( "<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(highlightColor), w.intellect.ToString())}\n";
            }
        } else {
            stats = "NOT IMPLEMENTED";
        }
        return stats;
    }
}
