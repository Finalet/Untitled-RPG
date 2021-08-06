using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuffTooltip : MonoBehaviour
{
    RectTransform rect;
    public TextMeshProUGUI tooltipNameLabel;
    public TextMeshProUGUI tooltipDescriptionLabel;
    public TextMeshProUGUI tooltipStatsLabel;
    
    public void Init(Buff buff) {
        rect = GetComponent<RectTransform>();
        tooltipNameLabel.text = buff.name;
        tooltipDescriptionLabel.text = buff.description;
        tooltipStatsLabel.text = generateBuffStats(buff);

        tooltipDescriptionLabel.rectTransform.sizeDelta = new Vector2(tooltipDescriptionLabel.rectTransform.sizeDelta.x, string.IsNullOrEmpty(tooltipDescriptionLabel.text) ? 0 : tooltipDescriptionLabel.preferredHeight);
        tooltipStatsLabel.rectTransform.sizeDelta = new Vector2(tooltipStatsLabel.rectTransform.sizeDelta.x, string.IsNullOrEmpty(tooltipStatsLabel.text) ? 0 : tooltipStatsLabel.preferredHeight);

        float height = 54 + tooltipDescriptionLabel.preferredHeight + tooltipStatsLabel.preferredHeight;

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
    }

    string generateBuffStats (Buff buff) {
        string stats = "";
        string color = $"#{ColorUtility.ToHtmlStringRGB(UI_General.highlightTextColor)}";

        if (buff.healthBuff != 0) stats += $"Health: <color={color}>+{buff.healthBuff}</color>\n";
        if (buff.staminaBuff != 0) stats += $"Stamina: <color={color}>+{buff.staminaBuff}</color>\n";

        if (buff.strengthBuff != 0 || buff.agilityBuff != 0 || buff.intellectBuff != 0)
            if (stats != "") stats += addSpace();

        if (buff.strengthBuff != 0) stats += $"Strength: <color={color}>+{buff.strengthBuff*100}%</color>\n";
        if (buff.agilityBuff != 0) stats += $"Agility: <color={color}>+{buff.agilityBuff*100}%</color>\n";
        if (buff.intellectBuff != 0) stats += $"Intellect: <color={color}>+{buff.intellectBuff*100}%</color>\n";

        if (buff.meleeAttackBuff != 0 || buff.rangedAttackBuff != 0 || buff.magicPowerBuff != 0 || buff.healingPowerBuff != 0 || buff.defenseBuff != 0)
            if (stats != "") stats += addSpace();

        if (buff.meleeAttackBuff != 0) stats += $"Melee attack: <color={color}>+{buff.meleeAttackBuff*100}%</color>\n";
        if (buff.rangedAttackBuff != 0) stats += $"Ranged attack: <color={color}>+{buff.rangedAttackBuff*100}%</color>\n";
        if (buff.magicPowerBuff != 0) stats += $"Magic power: <color={color}>+{buff.magicPowerBuff*100}%</color>\n";
        if (buff.healingPowerBuff != 0) stats += $"Healing power: <color={color}>+{buff.healingPowerBuff*100}%</color>\n";
        if (buff.defenseBuff != 0) stats += $"Defense: <color={color}>+{buff.defenseBuff*100}%</color>\n";

        if (buff.castingSpeedBuff != 0 || buff.attackSpeedBuff != 0)
            if (stats != "") stats += addSpace();

        if (buff.castingSpeedBuff != 0) stats += $"Cast duration: <color={color}>{(buff.castingSpeedBuff > 0 ? "+" : "")}{buff.castingSpeedBuff*100}%</color>\n";
        if (buff.attackSpeedBuff != 0) stats += $"Attack duration: <color={color}>{(buff.attackSpeedBuff > 0 ? "+" : "")}{buff.attackSpeedBuff*100}%</color>\n";

        if (buff.walkSpeedBuff != 0)
            if (stats != "") stats += addSpace();

        if (buff.walkSpeedBuff != 0) stats += $"Walk speed: <color={color}>+{buff.walkSpeedBuff*100}%</color>\n";

        if (buff.skillDistanceBuff != 0)
            if (stats != "") stats += addSpace();

        if (buff.skillDistanceBuff != 0) stats += $"Skill distance: <color={color}>+{buff.skillDistanceBuff}</color>\n";

        if (buff.immuneToDamage || buff.immuneToInterrupt)
            if (stats != "") stats += addSpace();

        if (buff.immuneToDamage) stats += $"<color={color}>Immune to damage.</color>\n";
        if (buff.immuneToInterrupt) stats += $"<color={color}>Immune to interruptions.</color>\n";

        if (buff.critChanceBuff != 0 || buff.critStrengthBuff != 0 || buff.blockChanceBuff != 0 || buff.walkSpeedBuff != 0)
            if (stats != "") stats += addSpace();

        if (buff.critChanceBuff != 0) stats += $"Critical chance: <color={color}>{(buff.critChanceBuff > 0 ? "+" : "")}{buff.critChanceBuff*100}%</color>\n";
        if (buff.critStrengthBuff != 0) stats += $"Critical strength: <color={color}>{(buff.critStrengthBuff > 0 ? "+" : "")}{buff.critStrengthBuff*100}%</color>\n";
        if (buff.blockChanceBuff != 0) stats += $"Block chance: <color={color}>{(buff.blockChanceBuff > 0 ? "+" : "")}{buff.blockChanceBuff*100}%</color>\n";
        if (buff.walkSpeedBuff != 0) stats += $"Walk speed: <color={color}>{(buff.walkSpeedBuff > 0 ? "+" : "")}{buff.walkSpeedBuff*100}%</color>\n";

        return stats;
    }

    string addSpace() {
        return "<size=8>\n</size>";
    }
}
