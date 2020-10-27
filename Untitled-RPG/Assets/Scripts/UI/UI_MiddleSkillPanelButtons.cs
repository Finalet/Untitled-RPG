using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        AssignSkill();
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

    void AssignSkill () {
        if (!isLBM) {
            ClearSlot();
            return;
        }

        SingleHandStatus rhs = WeaponsController.instance.rightHandStatus;
        if (rhs == SingleHandStatus.OneHandedSword || rhs == SingleHandStatus.TwoHandedSword) {
            AddSkill(AssetHolder.instance.getSkill(0), null);            
        } else if (rhs == SingleHandStatus.OneHandedStaff || rhs == SingleHandStatus.TwoHandedStaff) {
            AddSkill(AssetHolder.instance.getSkill(8), null);
        }
    }

    //--------------------------------Drag----------------------------------//
    public override void OnBeginDrag (PointerEventData pointerData) {
        return;
    }

    public override void OnDrag (PointerEventData pointerData) {
        return;
    }

    public override void OnEndDrag (PointerEventData pointerData) {
        return;
    }

    public override void OnDrop (PointerEventData pointerData) {
        return;
    }

    public override void OnPointerClick (PointerEventData pointerData) {
        return;    
    }
}
