using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReviveStatueWindowUI : MonoBehaviour
{
    public ReviveStatue ownerNPC;

    public Button[] skillTressButtons1;
    public Button[] skillTressButtons2;

    public TextMeshProUGUI firstSkilltreeLabel;
    public TextMeshProUGUI secondSkilltreeLabel;

    SkillTree firstSkillTree;
    SkillTree secondSkillTree;

    public void Init() {
        for (int i = 0; i < skillTressButtons1.Length; i++) {
            int index = i;
            skillTressButtons1[i].onClick.AddListener(delegate{SelectFirstSkillTree(index);});
        }
        
        for (int i = 0; i < skillTressButtons2.Length; i++) {
            int index = i;
            skillTressButtons2[i].onClick.AddListener(delegate{SelectSecondSkillTree(index);});
        }

        firstSkillTree = Combat.instanace.currentSkillTrees[0];
        secondSkillTree = Combat.instanace.currentSkillTrees[1];
        UpdateLabels();
    }
    void UpdateLabels () {
        firstSkilltreeLabel.text = firstSkillTree.ToString();
        secondSkilltreeLabel.text = secondSkillTree.ToString();
    }

    public void SelectFirstSkillTree (int index) {
        switch (index) {
            case 0:
                firstSkillTree = SkillTree.Knight;
                break;
            case 1:
                firstSkillTree = SkillTree.Hunter;
                break;
            case 2:
                firstSkillTree = SkillTree.Mage;
                break;
            case 3:
                firstSkillTree = SkillTree.Summoner;
                break;
            case 4:
                firstSkillTree = SkillTree.Stealth;
                break;
            case 5:
                firstSkillTree = SkillTree.Agnel;
                break;
            case 6:
                firstSkillTree = SkillTree.Defense;
                break;
        }
        skillTressButtons1[0].transform.parent.gameObject.SetActive(false);
        firstSkilltreeLabel.transform.parent.gameObject.SetActive(true);
        UpdateLabels();
    }
    public void SelectSecondSkillTree (int index) {
        switch (index) {
            case 0:
                secondSkillTree = SkillTree.Knight;
                break;
            case 1:
                secondSkillTree = SkillTree.Hunter;
                break;
            case 2:
                secondSkillTree = SkillTree.Mage;
                break;
            case 3:
                secondSkillTree = SkillTree.Summoner;
                break;
            case 4:
                secondSkillTree = SkillTree.Stealth;
                break;
            case 5:
                secondSkillTree = SkillTree.Agnel;
                break;
            case 6:
                secondSkillTree = SkillTree.Defense;
                break;
        }
        skillTressButtons2[0].transform.parent.gameObject.SetActive(false);
        secondSkilltreeLabel.transform.parent.gameObject.SetActive(true);
        UpdateLabels();
    }

    public void ConfirmButton () {
        Combat.instanace.SetCurrentSkillTrees(firstSkillTree, 0);
        Combat.instanace.SetCurrentSkillTrees(secondSkillTree, 1);
        ownerNPC.StopInterract();
    }
}
