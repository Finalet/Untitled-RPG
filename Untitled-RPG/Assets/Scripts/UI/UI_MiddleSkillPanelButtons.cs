using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MiddleSkillPanelButtons : UI_SkillPanelSlot
{
    [Header("Middle skill panel")]
    public bool isLBM;
    public Skill areaPickerSkill;

    protected override void Update() {
        if (PlayerControlls.instance.isPickingArea){
            PickingArea();
            return;
        }
        base.Update();
    }

    void PickingArea () {
        if (isLBM) {
            slotIcon.sprite = areaPickerSkill.icon;
        } else {
            slotIcon.sprite = CanvasScript.instance.cancelSprite;
        }
        cooldownImage.color = new Color(0, 0, 0, 0);
        cooldownImage.fillAmount = 1;
        cooldownTimerText.text = "";

        slotIcon.color = Color.white;
        
        DetectKeyPress();
    }

    protected override void DetectKeyPress() {
        if (PeaceCanvas.instance.anyPanelOpen)
            return;
        //If not picking area, then act like a regular skill slot
        if (!PlayerControlls.instance.isPickingArea) {
            base.DetectKeyPress();
            return;
        }

        if (Input.GetKeyDown(assignedKey)) {
                StartCoroutine(UI_General.PressAnimation(key, assignedKey));
        } else if (Input.GetKeyUp(assignedKey)) {
            if (isLBM)
                ConfirmArea();
            else 
                CancelPickingArea();
        }
    }
    void ConfirmArea () {
        areaPickerSkill.ConfirmPickArea();
        areaPickerSkill = null;
    }
    void CancelPickingArea(){
        areaPickerSkill.CancelPickingArea();
        areaPickerSkill = null;
    }
}
