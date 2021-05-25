using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{

    public Item focusItem;

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
        itemType.color = UI_General.secondaryTextColor;
        itemName.color = UI_General.mainTextColor;
        itemDescription.color = UI_General.secondaryTextColor;
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
        string highlightColor = "#" + ColorUtility.ToHtmlStringRGBA(UI_General.highlightTextColor);
        if (item is Consumable) {
            Consumable c = (Consumable)item;
            if (c.consumableType == ConsumableType.Health) {
                stats = $"Restores around <color={highlightColor}> +{c.effectAmount.ToString()}</color> health\n"; 
            } else if (c.consumableType == ConsumableType.Stamina) {
                stats = $"Restores around <color={highlightColor}> +{c.effectAmount.ToString()}</color> stamina\n";
            }
            if(c.cooldownTime > 0)
                stats += $"\nCool down: <color={highlightColor}>{c.cooldownTime.ToString()}</color> seconds";
        } else if (item is Equipment) {
            Equipment w = (Equipment)item;
            if (w.MeleeAttack != 0) {
                stats += $"Melee attack: <color={highlightColor}>{w.MeleeAttack.ToString()}</color>\n";
            }
            if (w.RangedAttack != 0) {
                stats += $"Ranged attack: <color={highlightColor}>{w.RangedAttack.ToString()}</color>\n";
            }
            if (w.MagicPower != 0) {
                stats += $"Magic power: <color={highlightColor}>{w.MagicPower.ToString()}</color>\n";
            }
            if (w.HealingPower != 0) {
                stats += $"Healing power: <color={highlightColor}>{w.HealingPower.ToString()}</color>\n";
            }
            if (w.Defense != 0) {
                stats += $"Defense: <color={highlightColor}>{w.Defense.ToString()}</color>\n";
            }
            if (w.Health != 0 || w.Stamina != 0) {
                stats += "\n";
            }
            if (w.Health != 0) {
                stats += $"Health: <color={highlightColor}>{w.Health.ToString()}</color>\n";
            }
            if (w.Stamina != 0) {
                stats += $"Stamina: <color={highlightColor}>{w.Stamina.ToString()}</color>\n";
            }
            if (w.strength != 0 || w.agility != 0 || w.intellect != 0) {
                stats += "\n";
            }
            if (w.strength != 0) {
                stats += $"Strength: <color={highlightColor}>{w.strength.ToString()}</color>\n";
            }
            if (w.agility != 0) {
                stats += $"Agility: <color={highlightColor}>{w.agility.ToString()}</color>\n";
            }
            if (w.intellect != 0) {
                stats += $"Intellect: <color={highlightColor}>{w.intellect.ToString()}</color>\n";
            }
            if (w.castingTime != 0 || w.attackSpeed != 0) {
                stats += "\n";
            }
            if (w.castingTime != 0) {
                stats += $"Casting time: <color={highlightColor}>{(100*w.castingTime).ToString()}%</color>\n";
            }
            if (w.attackSpeed != 0) {
                stats += $"Intellect: <color={highlightColor}>{ (100*w.attackSpeed).ToString()}%</color>\n";
            }
        } else {
            stats = "NOT IMPLEMENTED";
        }
        return stats;
    }
}
