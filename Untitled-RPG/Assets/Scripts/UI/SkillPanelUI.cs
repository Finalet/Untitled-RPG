using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanelUI : MonoBehaviour
{
    [Space]
    public Text availableSkillPointsLabel;
    public Text currentSkillTreesLabel;
    public Image[] allSkillIcons;

    void OnEnable() {
        UpdatePickedSkill();
        availableSkillPointsLabel.text = $"Available skill points: {Combat.instanace.availableSkillPoints.ToString()}";
        currentSkillTreesLabel.text = currentSkillTreesText();
    }
    void OnDisable() {
        Combat.instanace.ValidateSkillSlots();
    }
    
    public void UpdatePickedSkill () {
        for (int i = 0; i < 10; i++) {
            if (i < Combat.instanace.currentPickedSkills.Count) {
                allSkillIcons[i].sprite = Combat.instanace.currentPickedSkills[i].icon;
                allSkillIcons[i].GetComponent<SkillOnPanel>().skill = Combat.instanace.currentPickedSkills[i];
                allSkillIcons[i].gameObject.SetActive(true);
            } else {
                allSkillIcons[i].GetComponent<SkillOnPanel>().skill = null;
                allSkillIcons[i].gameObject.SetActive(false);
            }
        }
    }
    
    string currentSkillTreesText () {
        string text = $"Skill trees: ";
        for (int i = 0; i < Combat.instanace.currentSkillTrees.Length; i++) {
            text += Combat.instanace.currentSkillTrees[i].ToString();
            if (i != Combat.instanace.currentSkillTrees.Length-1)
                text += " | ";
        }
        return text;
    }

    void Update() {
        availableSkillPointsLabel.text = $"Available skill points: {Combat.instanace.availableSkillPoints.ToString()}";
    }
}
