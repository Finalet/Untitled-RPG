using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SkillPanelSlot : UI_InventorySlot, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Image key;
    public TextMeshProUGUI keyText;

    [Header("Skill panel")]
    public Skill skillInSlot;
    [Space]
    public KeyCode assignedKey; //Main key "E"

    protected override void Awake() {
        savefilePath = "saves/skillPanelSlots.txt";
    }
    protected override void Start() {
        base.Start();
    }

    protected override void Update() {
        if (skillInSlot != null) {
            DisplaySkill();
        } else if (itemInSlot != null) {
            DisplayItem();
        } else {
            //In case when RMB canceled picking area.
            slotIcon.sprite = null; 
            slotIcon.color = new Color(0,0,0,0);
        }

        DisplayKey();
        DetectKeyPress();
    }

    public override void ClearSlot() {
        skillInSlot = null;
        if (keyText != null) keyText.color = Color.white;
        
        base.ClearSlot();
    }

    void DisplaySkill() {
        slotIcon.sprite = skillInSlot.icon;
        itemAmountText.text = "";
        //Cooldown
        if(skillInSlot.isCoolingDown) {
            cooldownImage.color = new Color(0, 0, 0, 0.9f);
            cooldownImage.fillAmount = skillInSlot.coolDownTimer/skillInSlot.coolDown;
            cooldownTimerText.text = Mathf.RoundToInt(skillInSlot.coolDownTimer).ToString();
        } else {
            cooldownImage.color = new Color(0, 0, 0, 0);
            cooldownImage.fillAmount = 1;
            cooldownTimerText.text = "";
        }
        if (skillInSlot.skillActive()) {
            slotIcon.color = Color.white;
            if (keyText != null) keyText.color = Color.white;
        } else {
            slotIcon.color = new Color (0.3f, 0.3f, 0.3f, 1);
            if (keyText != null) keyText.color = new Color(0.6f, 0, 0, 1); 
        }
    }

    protected virtual void DetectKeyPress() {
        if (PeaceCanvas.instance.anyPanelOpen)
            return;

        if (Input.GetKeyDown(assignedKey)) {
                StartCoroutine(UI_General.PressAnimation(key, assignedKey));
        } else if (Input.GetKeyUp(assignedKey)) {
            if (skillInSlot != null) //If slot is taken with a skill
                skillInSlot.Use();
            else if (itemInSlot != null) //If slot is taken with an item
                UseItem();
        }
    }

    void DisplayKey () {
        if (keyText != null)
            keyText.text = KeyCodeDictionary.keys[assignedKey];
    }

    public override void Save() {
        if (skillInSlot != null) { //Saving skill
            BasicSave(2, (short)skillInSlot.ID, 0);
            return;
        }
        base.Save(); //Saving item or empty
    }
    public override void Load() {
        byte type = ES3.Load<byte>($"slot_{slotID}_type", savefilePath, 0);
        if (type == 2) { //Load skill
            short ID = ES3.Load<short>($"slot_{slotID}_itemID", savefilePath, 0);
            AddSkill(AssetHolder.instance.getSkill(ID), null);
            return;
        }
        base.Load(type);
    }

    public void AddSkill (Skill skill, UI_SkillPanelSlot initialSlot) {
        if (skillInSlot != null) { //Slot contains another skill
            if (initialSlot != null)
                initialSlot.AddSkill(skillInSlot, null);
        } else if (itemInSlot != null) { //Slot contains an item
            if (initialSlot != null) {
                initialSlot.AddItem(itemInSlot, itemAmount, null);
            } else {
                print("Destory item?");
                return;
            }
        }
        skillInSlot = skill;
        itemInSlot = null;
        itemAmount = 0;
        itemAmountText.text = "";
    }

    public override void AddItem (Item item, int amount, UI_InventorySlot initialSlot) {
        if (skillInSlot != null) { //Slot contains skill
            if (initialSlot.GetComponent<UI_SkillPanelSlot>() != null) //If item was dragged from the skill panel
                initialSlot.GetComponent<UI_SkillPanelSlot>().AddSkill(skillInSlot, null);
            ClearSlot();
        }
        base.AddItem(item, amount, initialSlot);
    }


    //--------------------------------Drag----------------------------------//
    public override void OnBeginDrag (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Right)
            return;
        
        if (skillInSlot != null) { //Dragging skill
            PeaceCanvas.instance.StartDraggingSkill(GetComponent<RectTransform>().sizeDelta, skillInSlot, this);
            ClearSlot();
            return;
        }

        base.OnBeginDrag(pointerData);
    }

    public override void OnDrop (PointerEventData pointerData) {
        if (PeaceCanvas.instance.skillBeingDragged != null) { //Dropping skill
            AddSkill(PeaceCanvas.instance.skillBeingDragged, (UI_SkillPanelSlot)PeaceCanvas.instance.initialSlot);
            return;
        }

        base.OnDrop(pointerData);
    }
}
