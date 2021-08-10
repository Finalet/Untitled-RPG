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
    public TextMeshProUGUI leftBottomLabel;
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
        if (!PeaceCanvas.instance.anyPanelOpen)
            Destroy(gameObject);
    }

    public void Init (Item compareItem) {
        itemID.text = focusItem.ID.ToString();
        itemIcon.sprite = focusItem.itemIcon;
        itemType.text = UI_General.getItemType(focusItem);
        itemName.text = focusItem.itemName;
        itemRarity.text = focusItem.itemRarity.ToString();
        itemRarity.color = UI_General.getRarityColor(focusItem.itemRarity);
        itemDescription.text = focusItem.getItemDescription();
        itemPrice.text = focusItem.itemBasePrice.ToString();
        itemStats.text = generateItemStats(focusItem, compareItem);
        leftBottomLabel.text = "";

        CheckForSpecialFrame();

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

        itemDescription.rectTransform.sizeDelta = new Vector2(itemDescription.rectTransform.sizeDelta.x, itemDescription.preferredHeight);
        itemStats.rectTransform.sizeDelta = new Vector2(itemStats.rectTransform.sizeDelta.x, itemStats.preferredHeight);
        itemPrice.rectTransform.sizeDelta = new Vector2(itemPrice.preferredWidth, 20);
        
        float tooltipHeight = 130 + itemDescription.preferredHeight + itemStats.preferredHeight + grantedSkillHeight;
        GetComponent<RectTransform>().sizeDelta = new Vector2(360, tooltipHeight);
    }

    void CheckForSpecialFrame () {
        if (focusItem.specialFrameMat) {
            RectTransform specialFrame = new GameObject().AddComponent<RectTransform>();
            specialFrame.SetParent(itemIcon.transform);
            specialFrame.anchoredPosition3D = Vector3.zero;
            specialFrame.localScale = Vector2.one;
            specialFrame.anchorMin = Vector2.zero;
            specialFrame.anchorMax = Vector2.one;
            specialFrame.offsetMin = Vector2.zero;
            specialFrame.offsetMax = Vector2.zero;
            specialFrame.gameObject.AddComponent<Image>().material = focusItem.specialFrameMat;
        }
    }

    string generateItemStats (Item item, Item compareItem) {
        string stats = "";
        string highlightColor = "#" + ColorUtility.ToHtmlStringRGBA(UI_General.highlightTextColor);
        if (item is Consumable) {
            Consumable c = (Consumable)item;
            if (c.consumableType == ConsumableType.Health) {
                stats = $"Restores around <color={highlightColor}>{c.effectAmount.ToString()}</color> health\n"; 
            } else if (c.consumableType == ConsumableType.Stamina) {
                stats = $"Restores around <color={highlightColor}>{c.effectAmount.ToString()}</color> stamina\n";
            }
            if(c.cooldownTime > 0)
                stats += $"\nCool down: <color={highlightColor}>{c.cooldownTime.ToString()}</color> seconds";
        } else if (item is Equipment) {
            Equipment w = (Equipment)item;
            Equipment wCompare = (Equipment)compareItem;
            bool compare = compareItem != null ? true : false;
            string compareText = "";
            
            if (w.MeleeAttack != 0) {
                compareText = compare ? getCompareString(w.MeleeAttack, wCompare.MeleeAttack) : "";
                stats += $"Melee attack: <color={highlightColor}>{w.MeleeAttack.ToString()}</color>{compareText}\n";
            }
            if (w.RangedAttack != 0) {
                compareText = compare ? getCompareString(w.RangedAttack, wCompare.RangedAttack) : "";
                stats += $"Ranged attack: <color={highlightColor}>{w.RangedAttack.ToString()}</color>{compareText}\n";
            }
            if (w.MagicPower != 0) {
                compareText = compare ? getCompareString(w.MagicPower, wCompare.MagicPower) : "";
                stats += $"Magic power: <color={highlightColor}>{w.MagicPower.ToString()}</color>{compareText}\n";
            }
            if (w.HealingPower != 0) {
                compareText = compare ? getCompareString(w.HealingPower, wCompare.HealingPower) : "";
                stats += $"Healing power: <color={highlightColor}>{w.HealingPower.ToString()}</color>{compareText}\n";
            }
            if (w.Defense != 0) {
                compareText = compare ? getCompareString(w.Defense, wCompare.Defense) : "";
                stats += $"Defense: <color={highlightColor}>{w.Defense.ToString()}</color>{compareText}\n";
            }

            if (w.Health != 0 || w.Stamina != 0) {
                if (stats != "") stats += addSpace();
            }

            if (w.Health != 0) {
                compareText = compare ? getCompareString(w.Health, wCompare.Health) : "";
                stats += $"Health: <color={highlightColor}>{w.Health.ToString()}</color>{compareText}\n";
            }
            if (w.Stamina != 0) {
                compareText = compare ? getCompareString(w.Stamina, wCompare.Stamina) : "";
                stats += $"Stamina: <color={highlightColor}>{w.Stamina.ToString()}</color>{compareText}\n";
            }
            if (w.strength != 0 || w.agility != 0 || w.intellect != 0) {
                if (stats != "") stats += addSpace();
            }
            if (w.strength != 0) {
                compareText = compare ? getCompareString(w.strength, wCompare.strength) : "";
                stats += $"Strength: <color={highlightColor}>{w.strength.ToString()}</color>{compareText}\n";
            }
            if (w.agility != 0) {
                compareText = compare ? getCompareString(w.agility, wCompare.agility) : "";
                stats += $"Agility: <color={highlightColor}>{w.agility.ToString()}</color>{compareText}\n";
            }
            if (w.intellect != 0) {
                compareText = compare ? getCompareString(w.intellect, wCompare.intellect) : "";
                stats += $"Intellect: <color={highlightColor}>{w.intellect.ToString()}</color>{compareText}\n";
            }
            if (w.castingTime != 0 || w.attackSpeed != 0) {
                if (stats != "") stats += addSpace();
            }
            if (w.castingTime != 0) {
                compareText = compare ? $"{getCompareString(w.castingTime, wCompare.castingTime, true, true)}" : "";
                string plusSign = w.castingTime > 0 ? "+" : "";
                stats += $"Cast duration: <color={highlightColor}>{plusSign}{(100*w.castingTime).ToString()}%</color>{compareText}\n";
            }
            if (w.attackSpeed != 0) {
                compareText = compare ? $"{getCompareString(w.attackSpeed, wCompare.attackSpeed, true, true)}" : "";
                string plusSign = w.attackSpeed > 0 ? "+" : "";
                stats += $"Attack duration: <color={highlightColor}>{plusSign}{(100*w.attackSpeed).ToString()}%</color>{compareText}\n";
            }

            if (w.critChance != 0 || w.critStrength != 0 || w.blockChance != 0 || w.walkSpeed != 0) {
                if (stats != "") stats += addSpace();
            }
            if (w.critChance != 0) {
                compareText = compare ? $"{getCompareString(w.critChance, wCompare.critChance, false, true)}" : "";
                string plusSign = w.critChance > 0 ? "+" : "";
                stats += $"Critical chance: <color={highlightColor}>{plusSign}{(100*w.critChance).ToString()}%</color>{compareText}\n";
            }
            if (w.critStrength != 0) {
                compareText = compare ? $"{getCompareString(w.critStrength, wCompare.critStrength, false, true)}" : "";
                string plusSign = w.critStrength > 0 ? "+" : "";
                stats += $"Critical strength: <color={highlightColor}>{plusSign}{(100*w.critStrength).ToString()}%</color>{compareText}\n";
            }
            if (w.blockChance != 0) {
                compareText = compare ? $"{getCompareString(w.blockChance, wCompare.blockChance, false, true)}" : "";
                string plusSign = w.blockChance > 0 ? "+" : "";
                stats += $"Block chance: <color={highlightColor}>{plusSign}{(100*w.blockChance).ToString()}%</color>{compareText}\n";
            }
            if (w.walkSpeed != 0) {
                compareText = compare ? $"{getCompareString(w.walkSpeed, wCompare.walkSpeed, false, true)}" : "";
                string plusSign = w.walkSpeed > 0 ? "+" : "";
                stats += $"Walk speed: <color={highlightColor}>{plusSign}{(100*w.walkSpeed).ToString()}%</color>{compareText}\n";
            }
        } else if (item is Skillbook) {
            Skillbook sb = (Skillbook)item;
            stats = Combat.instanace.learnedSkills.Contains(AssetHolder.instance.getSkill(sb.learnedSkill.ID)) ? "<color=red>You already know this skill" : "";
        } else if (item is Resource) {
            // do nothing;
        } else {
            stats = "NOT IMPLEMENTED";
        }
        return stats;
    }

    string addSpace() {
        return "<size=8>\n</size>";
    }

    string getCompareString (float first, float second, bool reverseSign = false, bool percentages = false) {
        if (first == 0 || second == 0)
            return "";
        float delta = (first - second) * (percentages ? 100 : 1);
        
        if (!reverseSign) {
            return delta > 0 ? $" <color=green>▲{Mathf.Abs(delta)}</color>" : delta < 0 ? $" <color=red>▼{Mathf.Abs(delta)}</color>" : "";
        } else {
            return delta > 0 ? $" <color=red>▼{Mathf.Abs(delta)}</color>" : delta < 0 ? $" <color=green>▲{Mathf.Abs(delta)}</color>" : "";
        }
    }
}
