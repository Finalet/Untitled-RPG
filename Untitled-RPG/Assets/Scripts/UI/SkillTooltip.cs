using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTooltip : MonoBehaviour
{
    public Skill focusSkill;
    [Space]
    public Image skillIcon;
    public Text skillName;
    public Text skillTree;
    public Text skillDescription;
    public Text skillStats;


    void Update() {
        Init();

        if (!PeaceCanvas.instance.anyPanelOpen)
            Destroy(gameObject);
    }

    public void Init() {
        if (skillIcon.sprite != focusSkill.icon)
            skillIcon.sprite = focusSkill.icon;

        if (skillName.text != focusSkill.skillName)
            skillName.text = focusSkill.skillName;
        
        if (skillTree.text != focusSkill.skillTree.ToString())
            skillTree.text = focusSkill.skillTree.ToString();

        if (skillDescription.text != focusSkill.description)
            skillDescription.text = focusSkill.description;

        if (skillStats.text != generateSkillStats())
            skillStats.text = generateSkillStats();

        skillDescription.rectTransform.sizeDelta = new Vector2(skillDescription.rectTransform.sizeDelta.x, skillDescription.preferredHeight);
        skillStats.rectTransform.sizeDelta = new Vector2(skillStats.rectTransform.sizeDelta.x, skillStats.preferredHeight);

        float tooltipHeight = 268 + (skillDescription.preferredHeight <= 0 ? 0 : skillDescription.preferredHeight) +
            (skillStats.preferredHeight <= 0 ? 0 : skillStats.preferredHeight); 

        GetComponent<RectTransform>().sizeDelta = new Vector2(400, tooltipHeight);
    }

    string generateSkillStats () {
        string stats = "";
        string secondaryHighlightColor = "#" + ColorUtility.ToHtmlStringRGB(UI_General.secondaryHighlightTextColor); 
        if (focusSkill.baseDamagePercentage != 0) {
            string damageNumber = CalculateDamage.damageInfo(focusSkill.skillTree, focusSkill.baseDamagePercentage, 0, 0).damage.ToString();
            string damageType = "";
            switch (focusSkill.skillTree) {
                case SkillTree.Knight:
                    damageType = "Melee";
                    break;
                case SkillTree.Hunter:
                    damageType = "Ranged";
                    break;
                case SkillTree.Mage:
                    damageType = "Magic";
                    break;
            }
            stats += $"{damageType} damage: <color={secondaryHighlightColor}>{damageNumber}</color>\n";
        }
        string castingTime = focusSkill.castingTime == 0 ? "instant" : $"{focusSkill.castingTime.ToString()} seconds";
        stats += $"Casting time: <color={secondaryHighlightColor}>{castingTime}</color>\n";
        stats += $"Cooldown: <color={secondaryHighlightColor}>{focusSkill.coolDown.ToString()}</color> seconds";
        return stats; 
    }
}
