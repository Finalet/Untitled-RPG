using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UI_SkillPanelSlot : UI_InventorySlot, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image key;
    public TextMeshProUGUI keyText;

    [Header("Skill panel")]
    public Skill currentSlotSkill;
    public Skill[] skillsInRows;
    public ItemAmountPair[] itemsAndAmontsInRows;
    
    protected virtual KeyCode assignedKey {
        get {
            return KeybindsManager.instance.currentKeyBinds[$"Slot {slotID + 1}"];
        }
    }

    protected int currentRow;
    protected bool currentSkillIsUnavailable;

    bool validatedSlotsOnLoad;

    public virtual void SwitchRows (int rowIndex, bool instant = false) {
        if (rowIndex == currentRow) return;
        if (instant) {
            currentRow = rowIndex;
            GrabSlotContents();
            ValidateSkillSlot();
            CanvasScript.instance.ChangeRowNumberLabel();
            return;
        }
        StartCoroutine(SwitchRowAnim(rowIndex));
    }

    IEnumerator SwitchRowAnim (int rowIndex) {
        slotIcon.rectTransform.DOScaleY(0f, 0.1f);
        cooldownImage.rectTransform.DOScaleY(0f, 0.1f);
        yield return new WaitForSeconds(0.1f);
        currentRow = rowIndex;
        CanvasScript.instance.ChangeRowNumberLabel();
        GrabSlotContents();
        ValidateSkillSlot();
        cooldownImage.rectTransform.DOScaleY(1, 0.1f);
        slotIcon.rectTransform.DOScaleY(1, 0.1f);
    }

    protected override string savefilePath(){
        return SaveManager.instance.getCurrentCharacterFolderPath("skillPanelSlots");
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
        GrabSlotContents();
        if (!validatedSlotsOnLoad && currentSlotSkill) { ValidateSkillSlot(); validatedSlotsOnLoad = true;}

        if (currentSlotSkill != null) {
            DisplaySkill();
        } else if (itemInSlot != null) {
            DisplayItem();
        } else {
            DisplayEmptySlot();
        }

        DisplayKey();
        DetectKeyPress();
    }
    void DisplayEmptySlot () {
        itemAmountText.text = "";
        slotIcon.sprite = null; 
        slotIcon.color = transparentColor;
        if (itemAmountText != null) itemAmountText.text = "";

        if (keyText != null) keyText.color = Color.white;

        if (cooldownImage == null) return;
        cooldownImage.color = transparentColor;
        cooldownImage.fillAmount = 1;
        cooldownTimerText.text = "";
    }
    protected override void DisplayItem()
    {
        base.DisplayItem();
        if (keyText != null) keyText.color = Color.white;
    }

    protected virtual void GrabSlotContents () {
        if (skillsInRows.Length == 0 || itemsAndAmontsInRows.Length == 0) return;

        currentSlotSkill = skillsInRows[currentRow];

        itemInSlot = itemsAndAmontsInRows[currentRow].item1;
        itemAmount = itemsAndAmontsInRows[currentRow].amount1;
    }
    void MatchRowContents() {
        skillsInRows[currentRow] = currentSlotSkill;

        itemsAndAmontsInRows[currentRow].item1 = itemInSlot;
        itemsAndAmontsInRows[currentRow].amount1 = itemAmount;
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
        if (PeaceCanvas.instance.anyPanelOpen || PeaceCanvas.instance.isGamePaused)
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

    public override void UseItem()
    {
        base.UseItem();
        MatchRowContents();
    }

    void DisplayKey () {
        if (keyText != null)
            keyText.text = KeyCodeDictionary.keys[assignedKey];
    }

    public override void SaveSlot() {
        for (byte row = 0; row < Combat.instanace.maxSkillSlotRows; row++) {
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
        skillsInRows = new Skill[Combat.instanace.maxSkillSlotRows];
        itemsAndAmontsInRows = new ItemAmountPair[Combat.instanace.maxSkillSlotRows];
        
        for (byte row = 0; row < Combat.instanace.maxSkillSlotRows; row++) {
            byte type = ES3.Load<byte>($"{slotID}_t_{row}", savefilePath(), 0);

            if (type == 2) { //Load skill
                short ID = ES3.Load<short>($"{slotID}_ID_{row}", savefilePath(), 0);
                AddSkill(AssetHolder.instance.getSkill(ID), null, row);
            } else if (type == 0) { //Empty Slot
                ClearSlotAtRow(row);
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
        row = row == -1 ? currentRow : row;

        if (skillsInRows[row] != null) { //Slot contains another skill
            if (initialSlot != null)
                initialSlot.AddSkill(skillsInRows[row], null);
        } else if (itemsAndAmontsInRows[row].item1 != null) { //Slot contains an item
            if (initialSlot != null) {
                initialSlot.AddItem(itemsAndAmontsInRows[row].item1, itemsAndAmontsInRows[row].amount1, null);
            } else {
                print("Destory item?");
                return;
            }
        }
        skillsInRows[row] = skill;
        itemsAndAmontsInRows[row].item1 = null;
        itemsAndAmontsInRows[row].amount1 = 0;
    }

    public override void AddItem (Item item, int amount, UI_InventorySlot initialSlot) {
        AddItemToRow(item, amount, initialSlot);
    }
    void AddItemToRow (Item item, int amount, UI_InventorySlot initialSlot, int row = -1) {
        row = row == -1 ? currentRow : row;
        if (skillsInRows[row] != null) { //Slot contains skill
            if (initialSlot && initialSlot.GetComponent<UI_SkillPanelSlot>() != null) //If item was dragged from the skill panel
                initialSlot.GetComponent<UI_SkillPanelSlot>().AddSkill(skillsInRows[row], null);
            ClearSlotAtRow(row);
        }
        base.AddItem(item, amount, initialSlot);
        itemsAndAmontsInRows[row].item1 = itemInSlot;
        itemsAndAmontsInRows[row].amount1 = itemAmount;

        currentSlotSkill = null;
        itemInSlot = null; //This is done on purpose because we assign itmes in the GrabSlotContents()
        itemAmount = 0; //If I don't do this it breaks cause base.AddItem adds items to itenInSlot which is not good.
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
