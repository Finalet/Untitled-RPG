using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SkillPanelSlot : UI_InventorySlot, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image key;
    public TextMeshProUGUI keyText;

    [Header("Skill panel")]
    Skill currentSlotSkill;
    public Skill[] skillsInRows;
    public ItemAmountPair[] itemsAndAmontsInRows;
    [Space]
    public KeyCode assignedKey; 

    int currentRow;
    protected bool currentSkillIsUnavailable;

    public virtual void SwitchRows (int rowIndex) {
        currentRow = rowIndex;
        currentSlotSkill = skillsInRows[currentRow];
        itemInSlot = itemsAndAmontsInRows[currentRow].item1;
        itemAmount = itemsAndAmontsInRows[currentRow].amount1;
        ValidateSkillSlot();
    }

    protected override string savefilePath(){
        return "saves/skillPanelSlots.txt";
    }

    public virtual void ValidateSkillSlot() {
        if (currentSlotSkill == null)
            return;

        bool valid = false;
        for (int i = 0; i < Combat.instanace.currentPickedSkills.Count; i++){ //for each picked skill
            if (currentSlotSkill == Combat.instanace.currentPickedSkills[i]) 
                valid = true;
        }
        for (int i = 0; i < Combat.instanace.currentSkillsFromEquipment.Count; i++){ //for each skill from equipment
            if (currentSlotSkill == Combat.instanace.currentSkillsFromEquipment[i]) 
                valid = true;
        }
        currentSkillIsUnavailable = valid ? false : true;
    }

    protected override void Update() {
        if (currentSlotSkill != null) {
            DisplaySkill();
        } else if (itemInSlot != null) {
            DisplayItem();
        } else {
            itemAmountText.text = "";
            //In case when RMB canceled picking area.
            slotIcon.sprite = null; 
            slotIcon.color = new Color(0,0,0,0);
        }

        DisplayKey();
        DetectKeyPress();
    }

    protected virtual void ClearSlotAtRow(int row = -1) {
        row = row == -1 ? currentRow : row;
        
        if (row == currentRow) { //clearing current slot
            currentSlotSkill = null;
            if (keyText != null) keyText.color = Color.white;
            
            base.ClearSlot();
        }

        skillsInRows[row] = null;
        itemsAndAmontsInRows[row].item1 = null;
        itemsAndAmontsInRows[row].amount1 = 0;
    }

    public override void ClearSlot() {
        ClearSlotAtRow();
    }

    void DisplaySkill() {
        slotIcon.sprite = currentSlotSkill.icon;
        itemAmountText.text = "";
        //UnavailableSkill
        if (currentSkillIsUnavailable) {
            slotIcon.color = new Color (1f, 0.6f, 0.6f, 0.95f);
            if (keyText != null) keyText.color = new Color(0.6f, 0, 0, 1); 

            cooldownImage.color = new Color(0, 0, 0, 0);
            cooldownImage.fillAmount = 1;
            cooldownTimerText.text = "";
            return;
        }
        //Cooldown
        if(currentSlotSkill.isCoolingDown) {
            cooldownImage.color = new Color(0, 0, 0, 0.9f);
            cooldownImage.fillAmount = currentSlotSkill.coolDownTimer/currentSlotSkill.coolDown;
            cooldownTimerText.text = currentSlotSkill.coolDownTimer > 120 ? Mathf.RoundToInt(currentSlotSkill.coolDownTimer/60).ToString() + "m" : Mathf.RoundToInt(currentSlotSkill.coolDownTimer).ToString();
            slotIcon.color = new Color(0.65f,0.65f,0.65f,1);
        } else {
            cooldownImage.color = new Color(0, 0, 0, 0);
            cooldownImage.fillAmount = 1;
            cooldownTimerText.text = "";
            slotIcon.color = Color.white;
        }
        //Skill active
        if (currentSlotSkill.skillActive()) {
            if (!currentSlotSkill.isCoolingDown) slotIcon.color = Color.white;
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
            if (currentSlotSkill != null && currentSlotSkill is AimingSkill && !currentSkillIsUnavailable) //If slot is taken with aiming skill
                currentSlotSkill.GetComponent<AimingSkill>().UseButtonDown();
        } else if (Input.GetKeyUp(assignedKey)) {
            if (currentSlotSkill != null && !currentSkillIsUnavailable) //If slot is taken with a skill
                currentSlotSkill.Use();
            else if (itemInSlot != null) //If slot is taken with an item
                UseItem();
        }
    }

    void DisplayKey () {
        if (keyText != null)
            keyText.text = KeyCodeDictionary.keys[assignedKey];
    }

    public override void SaveSlot() {
        for (byte row = 0; row < Combat.instanace.numberOfSkillSlotsRows; row++) {
            if (skillsInRows[row]) { //Saving skill 
                SaveSkillSlot(2, (short)skillsInRows[row].ID, 0, row);
            } else {
                if (itemsAndAmontsInRows[row].item1 != null) { //Saving item
                    SaveSkillSlot(1, (short)itemsAndAmontsInRows[row].item1.ID, (byte)itemsAndAmontsInRows[row].amount1, row);
                } else { //Slot is empty
                    SaveSkillSlot(0, 0, 0, row);
                }
            }
        }
    }
    public override void LoadSlot() {
        skillsInRows = new Skill[Combat.instanace.numberOfSkillSlotsRows];
        itemsAndAmontsInRows = new ItemAmountPair[Combat.instanace.numberOfSkillSlotsRows];

        for (byte row = 0; row < Combat.instanace.numberOfSkillSlotsRows; row++) {
            byte type = ES3.Load<byte>($"{slotID}_t_{row}", savefilePath(), 0);

            if (type == 2) { //Load skill
                short ID = ES3.Load<short>($"{slotID}_ID_{row}", savefilePath(), 0);
                AddSkill(AssetHolder.instance.getSkill(ID), null, row);
            } else if (type == 0) { //Empty Slot
                ClearSlotAtRow(row);
                continue;
            } else {
                //Add item
                short ID = ES3.Load<short>($"{slotID}_ID_{row}", savefilePath(), 0); //ID
                byte amount = ES3.Load<byte>($"{slotID}_a_{row}", savefilePath(), 0); //Amount
                AddItemToRow(AssetHolder.instance.getItem(ID), amount, null, row);
            }            
        }
    }

    void SaveSkillSlot (byte type, short ID, byte amount, byte row) {
        ES3.Save<byte>($"{slotID}_t_{row}", type, savefilePath()); //Type
        ES3.Save<short>($"{slotID}_ID_{row}", ID, savefilePath()); //ID
        ES3.Save<byte>($"{slotID}_a_{row}", amount, savefilePath()); //Amount
    }


    public void AddSkill (Skill skill, UI_SkillPanelSlot initialSlot, int row = -1) {
        if (currentSlotSkill != null) { //Slot contains another skill
            if (initialSlot != null)
                initialSlot.AddSkill(currentSlotSkill, null);
        } else if (itemInSlot != null) { //Slot contains an item
            if (initialSlot != null) {
                initialSlot.AddItem(itemInSlot, itemAmount, null);
            } else {
                print("Destory item?");
                return;
            }
        }
        row = row == -1 ? currentRow : row;
        currentSlotSkill = skill;
        skillsInRows[row] = skill;
        itemsAndAmontsInRows[row].item1 = null;
        itemsAndAmontsInRows[row].amount1 = 0;
        itemInSlot = null;
        itemAmount = 0;
        itemAmountText.text = "";
        ValidateSkillSlot();
    }

    public override void AddItem (Item item, int amount, UI_InventorySlot initialSlot) {
        AddItemToRow(item, amount, initialSlot);
    }
    void AddItemToRow (Item item, int amount, UI_InventorySlot initialSlot, int row = -1) {
        if (currentSlotSkill != null) { //Slot contains skill
            if (initialSlot && initialSlot.GetComponent<UI_SkillPanelSlot>() != null) //If item was dragged from the skill panel
                initialSlot.GetComponent<UI_SkillPanelSlot>().AddSkill(currentSlotSkill, null);
            ClearSlot();
        }
        base.AddItem(item, amount, initialSlot);
        row = row == -1 ? currentRow : row;
        skillsInRows[row] = null;
        currentSlotSkill = null;
        itemsAndAmontsInRows[row].item1 = item;
        itemsAndAmontsInRows[row].amount1 = amount;
    }

    //--------------------------------Drag----------------------------------//
    public override void OnBeginDrag (PointerEventData pointerData) {
        if (pointerData.button == PointerEventData.InputButton.Right)
            return;
        
        if (currentSlotSkill != null) { //Dragging skill
            PeaceCanvas.instance.StartDraggingSkill(GetComponent<RectTransform>().sizeDelta, currentSlotSkill, this);
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

    public override void OnPointerEnter (PointerEventData pointerData) {
        //
    }
    public override void OnPointerExit (PointerEventData pointerData) {
        //
    }
}
