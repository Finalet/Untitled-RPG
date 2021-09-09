using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillPanelUI : MonoBehaviour
{
    public GameObject SkillTreeTemplate;
    public GameObject SkillOnPanePrefab;
    public Transform LearnedSkillsScrollView;
    [Space]
    public TextMeshProUGUI availableSkillPointsLabel;
    public TextMeshProUGUI currentSkillTreesLabel;
    public Image[] pickedSkillsIcons;
    public Image[] equipmentSkillsIcons;

    Transform SwordplayRow;
    Transform ArcheryRow;
    Transform SorceryRow;
    Transform VitailityRow;
    Transform MobilityRow;
    Transform DefenseRow;
    Transform SummoningRow;

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
            rt.GetComponent<TextMeshProUGUI>().text = sk.skillTree.ToString();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, generatedTrees.Count * (-rt.sizeDelta.y - 20));
            rt.gameObject.SetActive(true);
            switch (sk.skillTree) {
                case SkillTree.Swordplay:
                    SwordplayRow = rt.transform;
                    break;
                case SkillTree.Archery:
                    ArcheryRow = rt.transform;
                    break;
                case SkillTree.Sorcery:
                    SorceryRow = rt.transform;
                    break;
                case SkillTree.Vitality:
                    VitailityRow = rt.transform;
                    break;
                case SkillTree.Mobility:
                    MobilityRow = rt.transform;
                    break;
                case SkillTree.Defense:
                    DefenseRow = rt.transform;
                    break;
                case SkillTree.Summoning:
                    SummoningRow = rt.transform;
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
                case SkillTree.Swordplay:
                    row = SwordplayRow;
                    break;
                case SkillTree.Archery:
                    row = ArcheryRow;
                    break;
                case SkillTree.Sorcery:
                    row = SorceryRow;
                    break;
                case SkillTree.Vitality:
                    row = VitailityRow;
                    break;
                case SkillTree.Mobility:
                    row = MobilityRow;
                    break;
                case SkillTree.Defense:
                    row = DefenseRow;
                    break;
                case SkillTree.Summoning:
                    row = SummoningRow;
                    break;
                default:
                    row = SwordplayRow;
                    break;
            }
            SkillOnPanel sop = Instantiate(SkillOnPanePrefab, row).GetComponent<SkillOnPanel>();
            sop.skill = sk;
            generatedSkills.Add(sk);
        }
    }
    
    public void ResetLearnedSkills () {
        if (SwordplayRow) Destroy(SwordplayRow.gameObject);
        if (ArcheryRow) Destroy(ArcheryRow.gameObject);
        if (SorceryRow) Destroy(SorceryRow.gameObject);
        if (VitailityRow) Destroy(VitailityRow.gameObject);
        if (MobilityRow) Destroy(MobilityRow.gameObject);
        if (DefenseRow) Destroy(DefenseRow.gameObject);
        if (SummoningRow) Destroy(SummoningRow.gameObject);

        generatedTrees.Clear();
        generatedSkills.Clear();
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
        for (int i = 0; i < Combat.instanace.currentSkillTrees.Count; i++) {
            text += Combat.instanace.currentSkillTrees[i].ToString();
            if (i != Combat.instanace.currentSkillTrees.Count-1)
                text += " | ";
        }
        return text;
    }

    void Update() {
        availableSkillPointsLabel.text = $"Available skill points: {Combat.instanace.availableSkillPoints.ToString()}";
    }
}
