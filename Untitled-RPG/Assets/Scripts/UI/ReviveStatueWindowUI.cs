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

    public void Init() {
        for (int i = 0; i < skillTressButtons1.Length; i++) {
            int index = i;
            skillTressButtons1[i].onClick.AddListener(delegate{SelectFirstSkillTree(index);});
        }
        
        for (int i = 0; i < skillTressButtons2.Length; i++) {
            int index = i;
            skillTressButtons2[i].onClick.AddListener(delegate{SelectSecondSkillTree(index);});
        }
    }

    public void SelectFirstSkillTree (int index) {
        switch (index) {
            case 0:
                firstSkilltreeLabel.text = "Knight";
                break;
            case 1:
                firstSkilltreeLabel.text = "Hunter";
                break;
            case 2:
                firstSkilltreeLabel.text = "Magic";
                break;
            case 3:
                firstSkilltreeLabel.text = "Summoner";
                break;
            case 4:
                firstSkilltreeLabel.text = "Stealth";
                break;
            case 5:
                firstSkilltreeLabel.text = "Angel";
                break;
            case 6:
                firstSkilltreeLabel.text = "Defense";
                break;
        }
        skillTressButtons1[0].transform.parent.gameObject.SetActive(false);
        firstSkilltreeLabel.transform.parent.gameObject.SetActive(true);
    }
    public void SelectSecondSkillTree (int index) {
        switch (index) {
            case 0:
                secondSkilltreeLabel.text = "Knight";
                break;
            case 1:
                secondSkilltreeLabel.text = "Hunter";
                break;
            case 2:
                secondSkilltreeLabel.text = "Magic";
                break;
            case 3:
                secondSkilltreeLabel.text = "Summoner";
                break;
            case 4:
                secondSkilltreeLabel.text = "Stealth";
                break;
            case 5:
                secondSkilltreeLabel.text = "Angel";
                break;
            case 6:
                secondSkilltreeLabel.text = "Defense";
                break;
        }
        skillTressButtons2[0].transform.parent.gameObject.SetActive(false);
        secondSkilltreeLabel.transform.parent.gameObject.SetActive(true);
    }

    public void ConfirmButton () {
        ownerNPC.StopInterract();
    }
}
