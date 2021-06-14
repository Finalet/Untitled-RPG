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
    public Image grantedSkillIcon;
    public Image bottomBackground;
    public TextMeshProUGUI grantedSkillNameLabel;

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

        if (itemDescription.text != focusItem.getItemDescription())
            itemDescription.text = focusItem.getItemDescription();

        if (itemPrice.text != focusItem.itemBasePrice.ToString())
            itemPrice.text = focusItem.itemBasePrice.ToString();

        if (itemStats.text != generateItemStats(focusItem))
            itemStats.text = generateItemStats(focusItem);

        int grantedSkillHeight = 0;
        grantedSkillIcon.gameObject.SetActive(false);
        if (focusItem is Equipment) {
            Equipment eq = (Equipment)focusItem;
            if (eq.grantedSkill != null) {
                if (grantedSkillIcon.sprite != eq.grantedSkill.icon)
                    grantedSkillIcon.sprite = eq.grantedSkill.icon;
                if (grantedSkillNameLabel.text != eq.grantedSkill.skillName)
                    grantedSkillNameLabel.text = eq.grantedSkill.skillName;
                grantedSkillHeight = 100;

                grantedSkillIcon.gameObject.SetActive(true);
            }
        }

        int bottomBackgroundHeight = grantedSkillHeight == 0 ? 40 : 100;
        bottomBackground.rectTransform.sizeDelta = new Vector2(bottomBackground.rectTransform.sizeDelta.x, bottomBackgroundHeight);
        itemDescription.rectTransform.sizeDelta = new Vector2(itemDescription.rectTransform.sizeDelta.x, itemDescription.preferredHeight);
        itemStats.rectTransform.sizeDelta = new Vector2(itemStats.rectTransform.sizeDelta.x, itemStats.preferredHeight);
        itemPrice.rectTransform.sizeDelta = new Vector2(itemPrice.preferredWidth, 20);
        
        float tooltipHeight = 130 + itemDescription.preferredHeight + itemStats.preferredHeight;
        GetComponent<RectTransform>().sizeDelta = new Vector2(360, tooltipHeight);
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
                if (stats != "") stats += "\n";
            }
            if (w.Health != 0) {
                stats += $"Health: <color={highlightColor}>{w.Health.ToString()}</color>\n";
            }
            if (w.Stamina != 0) {
                stats += $"Stamina: <color={highlightColor}>{w.Stamina.ToString()}</color>\n";
            }
            if (w.strength != 0 || w.agility != 0 || w.intellect != 0) {
                if (stats != "") stats += "\n";
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
                if (stats != "") stats += "\n";
            }
            if (w.castingTime != 0) {
                stats += $"Casting time: <color={highlightColor}>{(100*w.castingTime).ToString()}%</color>\n";
            }
            if (w.attackSpeed != 0) {
                stats += $"Attack speed: <color={highlightColor}>{ (100*w.attackSpeed).ToString()}%</color>\n";
            }
        } else if (item is Skillbook) {
            Skillbook sb = (Skillbook)item;
            stats = Combat.instanace.learnedSkills.Contains(AssetHolder.instance.getSkill(sb.learnedSkill.ID)) ? "<color=red>You already know this skill" : "";
        } else {
            stats = "NOT IMPLEMENTED";
        }
        return stats;
    }
}
