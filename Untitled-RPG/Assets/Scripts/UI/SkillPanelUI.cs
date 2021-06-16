using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanelUI : MonoBehaviour
{
    public GameObject SkillTreeTemplate;
    public GameObject SkillOnPanePrefab;
    public Transform LearnedSkillsScrollView;
    [Space]
    public Text availableSkillPointsLabel;
    public Text currentSkillTreesLabel;
    public Image[] pickedSkillsIcons;
    public Image[] equipmentSkillsIcons;

    Transform KnightRow;
    Transform HunterRow;
    Transform MageRow;
    Transform AngelRow;
    Transform StealhRow;
    Transform DefenseRow;
    Transform SummonerRow;

    void OnEnable() {
        UpdateLearnedSkills();
        
        UpdatePickedSkill();
        UpdateEquipmentSkills();
        availableSkillPointsLabel.text = $"Available skill points: {Combat.instanace.availableSkillPoints.ToString()}";
        currentSkillTreesLabel.text = currentSkillTreesText();
    }
    void OnDisable() {
        Combat.instanace.ValidateSkillSlots();
    }
    
    List<SkillTree> generatedTrees = new List<SkillTree>();
    List<Skill> generatedSkills = new List<Skill>();
    public void UpdateLearnedSkills () {
        //Spawn skill tree rows
        foreach (Skill sk in Combat.instanace.learnedSkills) {
            if (generatedTrees.Contains(sk.skillTree))
                continue;

            RectTransform rt = Instantiate(SkillTreeTemplate, LearnedSkillsScrollView).GetComponent<RectTransform>();
            rt.GetComponent<Text>().text = sk.skillTree.ToString();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, generatedTrees.Count * (-rt.sizeDelta.y - 20));
            rt.gameObject.SetActive(true);
            switch (sk.skillTree) {
                case SkillTree.Knight:
                    KnightRow = rt.transform;
                    break;
                case SkillTree.Hunter:
                    HunterRow = rt.transform;
                    break;
                case SkillTree.Mage:
                    MageRow = rt.transform;
                    break;
                case SkillTree.Agnel:
                    AngelRow = rt.transform;
                    break;
                case SkillTree.Stealth:
                    StealhRow = rt.transform;
                    break;
                case SkillTree.Defense:
                    DefenseRow = rt.transform;
                    break;
                case SkillTree.Summoner:
                    SummonerRow = rt.transform;
                    break;
            }
            generatedTrees.Add(sk.skillTree);
        }
        SkillTreeTemplate.SetActive(false);

        // Populate rows with skills
        Transform row;
        foreach (Skill sk in Combat.instanace.learnedSkills) {
            if (generatedSkills.Contains(sk))
                continue;

            switch (sk.skillTree) {
                case SkillTree.Knight:
                    row = KnightRow;
                    break;
                case SkillTree.Hunter:
                    row = HunterRow;
                    break;
                case SkillTree.Mage:
                    row = MageRow;
                    break;
                case SkillTree.Agnel:
                    row = AngelRow;
                    break;
                case SkillTree.Stealth:
                    row = StealhRow;
                    break;
                case SkillTree.Defense:
                    row = DefenseRow;
                    break;
                case SkillTree.Summoner:
                    row = SummonerRow;
                    break;
                default:
                    row = KnightRow;
                    break;
            }
            SkillOnPanel sop = Instantiate(SkillOnPanePrefab, row).GetComponent<SkillOnPanel>();
            sop.skill = sk;
            generatedSkills.Add(sk);
        }
    }
    
    public void UpdatePickedSkill () {
        for (int i = 0; i < pickedSkillsIcons.Length; i++) {
            if (i < Combat.instanace.currentPickedSkills.Count) {
                pickedSkillsIcons[i].sprite = Combat.instanace.currentPickedSkills[i].icon;
                pickedSkillsIcons[i].GetComponent<SkillOnPanel>().skill = Combat.instanace.currentPickedSkills[i];
                pickedSkillsIcons[i].GetComponent<SkillOnPanel>().isEquipmentSkill = false;
                pickedSkillsIcons[i].gameObject.SetActive(true);
            } else {
                pickedSkillsIcons[i].GetComponent<SkillOnPanel>().skill = null;
                pickedSkillsIcons[i].gameObject.SetActive(false);
            }
        }
    }  
    public void UpdateEquipmentSkills () {
        for (int i = 0; i < equipmentSkillsIcons.Length; i++) {
            if (i < Combat.instanace.currentSkillsFromEquipment.Count) {
                equipmentSkillsIcons[i].sprite = Combat.instanace.currentSkillsFromEquipment[i].icon;
                equipmentSkillsIcons[i].GetComponent<SkillOnPanel>().skill = Combat.instanace.currentSkillsFromEquipment[i];
                equipmentSkillsIcons[i].GetComponent<SkillOnPanel>().isPicked = true;
                equipmentSkillsIcons[i].GetComponent<SkillOnPanel>().isEquipmentSkill = true;
                equipmentSkillsIcons[i].gameObject.SetActive(true);
            } else {
                equipmentSkillsIcons[i].GetComponent<SkillOnPanel>().skill = null;
                equipmentSkillsIcons[i].gameObject.SetActive(false);
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
