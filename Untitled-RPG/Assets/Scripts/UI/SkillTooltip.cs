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

        if (skillDescription.text != focusSkill.getDescription())
            skillDescription.text = focusSkill.getDescription();

        if (skillStats.text != generateSkillStats())
            skillStats.text = generateSkillStats();

        skillDescription.rectTransform.sizeDelta = new Vector2(skillDescription.rectTransform.sizeDelta.x, skillDescription.preferredHeight);
        skillStats.rectTransform.sizeDelta = new Vector2(skillStats.rectTransform.sizeDelta.x, skillStats.preferredHeight);

        float tooltipHeight = 268 + skillDescription.preferredHeight + skillStats.preferredHeight;
        GetComponent<RectTransform>().sizeDelta = new Vector2(400, tooltipHeight);
    }

    string generateSkillStats () {
        string stats = "";
        string castingTime = focusSkill.castingTime == 0 ? "instant" : $"{focusSkill.castingTime.ToString()} seconds";
        stats += $"Casting time: <color=white>{castingTime}</color>\n";
        stats += $"Cooldown: <color=white>{focusSkill.coolDown.ToString()}</color> seconds";
        return stats; 
    }
}
