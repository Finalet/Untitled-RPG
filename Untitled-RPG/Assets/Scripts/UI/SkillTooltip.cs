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
        if (!PeaceCanvas.instance.anyPanelOpen)
            Destroy(gameObject);
    }

    public void Init() {
        skillIcon.sprite = focusSkill.icon;
        skillName.text = focusSkill.skillName;
        skillTree.text = focusSkill.skillTree.ToString();
        skillDescription.text = generateRichDescription(focusSkill.getDescription());
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
    
    string generateRichDescription(string _description) {
        string description = _description;
        List<string> digits = new List<string>();
        List<int> indecies = new List<int>();
        int numbers = 0;
        int lastIndex = 0;
        for (int i = 0; i < description.Length; i++) {
            if (char.IsDigit(description[i]) || description[i] == '%') {
                if (i-lastIndex > 1) {
                    indecies.Insert(numbers, i);
                    digits.Insert(numbers, "");
                    digits[numbers] += description[i];
                    numbers ++;
                } else if (i-lastIndex == 1) {
                    digits[numbers-1] += description[i];
                }
                lastIndex = i;
            }
        }
        for (int i = 0; i < indecies.Count; i++){
            description = description.Insert(indecies[i] + i * 21, "<color=white>");
            description = description.Insert(i * 21 + 13 + indecies[i] + digits[i].Length, "</color>");
        }
        return description;
    }
}
