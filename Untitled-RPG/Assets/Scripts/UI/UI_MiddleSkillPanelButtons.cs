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

    public Skill overrideSkill;
    public bool overrideLMB;

    protected override void Update() {
        if (PeaceCanvas.instance.isGamePaused)
            return;

        if (PlayerControlls.instance.isPickingArea){
            PickingAreaIcons();
            DetectKeyPress();
            return;
        } else if (PlayerControlls.instance.isAimingSkill) {
            AimingSkillIcons();
            DetectKeyPress();
            return;
        }
        if (overrideSkill != null) {
            if (overrideLMB && isLBM) {
                overrideSkill.OverrideLMBicon(this);
                DetectKeyPress();
                return;
            }
        }
        AssignSkill();
        base.Update();
    }

    void PickingAreaIcons () {
        if (isLBM) {
            slotIcon.sprite = areaPickerSkill.icon;
        } else {
            slotIcon.sprite = CanvasScript.instance.cancelSprite;
        }
        cooldownImage.color = new Color(0, 0, 0, 0);
        cooldownImage.fillAmount = 1;
        cooldownTimerText.text = "";

        slotIcon.color = Color.white;
    }

    void AimingSkillIcons () {
        if (isLBM) {
            slotIcon.sprite = Combat.instanace.AimingSkill.icon;
        } else {
            slotIcon.sprite = CanvasScript.instance.cancelSprite;
        }

        if (isLBM) {
            if(Combat.instanace.AimingSkill.isCoolingDown) {
                cooldownImage.color = new Color(0, 0, 0, 0.9f);
                cooldownImage.fillAmount = Combat.instanace.AimingSkill.coolDownTimer/Combat.instanace.AimingSkill.coolDown;
                cooldownTimerText.text = Mathf.RoundToInt(Combat.instanace.AimingSkill.coolDownTimer).ToString();
                slotIcon.color = new Color(0.65f,0.65f,0.65f,1);
            } else {
                cooldownImage.color = new Color(0, 0, 0, 0);
                cooldownImage.fillAmount = 1;
                cooldownTimerText.text = "";
                slotIcon.color = Color.white;
            }
            if (Combat.instanace.AimingSkill.skillActive()) {
                if (!Combat.instanace.AimingSkill.isCoolingDown) slotIcon.color = Color.white;
                if (keyText != null) keyText.color = Color.white;
            } else {
                slotIcon.color = new Color (0.3f, 0.3f, 0.3f, 1);
                if (keyText != null) keyText.color = new Color(0.6f, 0, 0, 1); 
            }
        } else {
            cooldownImage.color = new Color(0, 0, 0, 0);
            cooldownImage.fillAmount = 1;
            cooldownTimerText.text = "";
            slotIcon.color = Color.white;
        }
    }

    protected override void DetectKeyPress() {
        if (PeaceCanvas.instance.anyPanelOpen)
            return;
        //If not picking area, not aiming skill, or no overrides then act like a regular skill slot
        if (!PlayerControlls.instance.isPickingArea && !PlayerControlls.instance.isAimingSkill && overrideSkill == null) {
            base.DetectKeyPress();
            return;
        }

        if (PlayerControlls.instance.isPickingArea) {
            if (Input.GetKeyDown(assignedKey)) {
                    StartCoroutine(UI_General.PressAnimation(key, assignedKey));
            } else if (Input.GetKeyUp(assignedKey)) {
                if (isLBM)
                    ConfirmArea();
                else 
                    CancelPickingArea();
            }
        } else if (PlayerControlls.instance.isAimingSkill) {
            if (Input.GetKeyDown(assignedKey)) {
                    StartCoroutine(UI_General.PressAnimation(key, assignedKey));
            } else if (Input.GetKeyUp(assignedKey)) {
                if (isLBM)
                    Combat.instanace.AimingSkill.UseButtonUp();
                else 
                    Combat.instanace.AimingSkill.CancelAiming();
            } else if (Input.GetKey(assignedKey)) {
                Combat.instanace.AimingSkill.UseButtonHold();
            }
        } else if (overrideSkill != null) {
            if (overrideLMB && isLBM) {
                if (Input.GetKeyDown(assignedKey)) {
                    StartCoroutine(UI_General.PressAnimation(key, assignedKey));
                } else if (Input.GetKeyUp(assignedKey)) {
                    overrideSkill.OverrideLMBAction();
                }
            }
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
            if (EquipmentManager.instance.bow.itemInSlot != null){
                AddSkill(AssetHolder.instance.getSkill(16), null);  
            } else {
                ClearSlot();
            }
            return;
        }

        SingleHandStatus rhs = WeaponsController.instance.rightHandStatus;
        if (rhs == SingleHandStatus.OneHandedSword || rhs == SingleHandStatus.TwoHandedSword) {
            AddSkill(AssetHolder.instance.getSkill(0), null);            
        } else if (rhs == SingleHandStatus.OneHandedStaff || rhs == SingleHandStatus.TwoHandedStaff) {
            AddSkill(AssetHolder.instance.getSkill(8), null);
        } else {
            ClearSlot();
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
