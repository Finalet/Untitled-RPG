using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SkillPanelSlot : UI_InventorySlot, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    Sprite emptySlotSprite;
    public Image cooldownImage;
    public TextMeshProUGUI keyText;

    [Header("Skill panel")]
    public Skill skillInSlot;
    [Space]
    public KeyCode mainAssignedKey; //Main key "E"
    public KeyCode secondaryAssignedKey; //Secondary key "Shift" to make "shift-e"


    protected override void Awake() {
        savefilePath = "saves/skillPanelSlots.txt";
    }
    protected override void Start() {
        base.Start();
        emptySlotSprite = GetComponent<Image>().sprite;
    }

    void Update() {
        if (skillInSlot != null) {
            DisplaySkill();
        } else if (itemInSlot == null) {
            slotIcon.sprite = emptySlotSprite;
        }

        DisplayKey();
        DetectKeyPress();
    }

    protected override void ClearSlot() {
        skillInSlot = null;
        itemInSlot = null;

        keyText.color = Color.white;
        slotIcon.color = Color.white;
        cooldownImage.color = new Color(0, 0, 0, 0);

        cooldownImage.fillAmount = 1;
        
        itemAmount = 0;
        itemAmountText.text = "";
    }

    void DisplaySkill() {
        slotIcon.sprite = skillInSlot.icon;
        itemAmountText.text = "";
        //Cooldown
        if(skillInSlot.isCoolingDown) {
            cooldownImage.color = new Color(0, 0, 0, 0.9f);
            cooldownImage.fillAmount = skillInSlot.coolDownTimer/skillInSlot.coolDown;
        } else {
            cooldownImage.color = new Color(0, 0, 0, 0);
            cooldownImage.fillAmount = 1;
        }
        if (skillInSlot.skillActive()) {
            slotIcon.color = Color.white;
            keyText.color = Color.white;
        } else {
            slotIcon.color = new Color (0.3f, 0.3f, 0.3f, 1);
            keyText.color = new Color(0.6f, 0, 0, 1); 
        }
    }

    void DetectKeyPress() {
        if (secondaryAssignedKey == KeyCode.None) {
            if (Input.GetKey(KeyCode.LeftAlt))
                return;

            if (Input.GetKeyDown(mainAssignedKey)) {
                StartCoroutine(UI_General.PressAnimation(slotIcon, mainAssignedKey));
            } else if (Input.GetKeyUp(mainAssignedKey)) {
                if (skillInSlot != null) //Check if slot if taken with a skill
                    skillInSlot.Use();
            }
        } else if (Input.GetKey(secondaryAssignedKey)) {
            if (Input.GetKeyDown(mainAssignedKey)) {
                StartCoroutine(UI_General.PressAnimation(slotIcon, mainAssignedKey));
            } else if (Input.GetKeyUp(mainAssignedKey)) {
                if (skillInSlot != null) //Check if slot if taken with a skill
                    skillInSlot.Use();
            }
        }
    }

    void DisplayKey () {
        if (secondaryAssignedKey == KeyCode.None)
            keyText.text = KeyCodeDictionary.keys[mainAssignedKey];
        else 
            keyText.text = KeyCodeDictionary.keys[secondaryAssignedKey] + " " + KeyCodeDictionary.keys[mainAssignedKey];
    }

    public override void Save() {
        if (skillInSlot != null) { //Saving skill
            BasicSave(2, (byte)skillInSlot.ID, 0);
            return;
        }
        base.Save(); //Saving item or empty
    }
    public override void Load() {
        byte type = ES3.Load<byte>($"slot_{slotID}_type", savefilePath, 0);
        if (type == 2) { //Load skill
            byte ID = ES3.Load<byte>($"slot_{slotID}_itemID", savefilePath, 0);
            AddSkill(AssetHolder.instance.Skills[ID], null);
            return;
        }
        base.Load(type);
    }

    public void AddSkill (Skill skill, UI_SkillPanelSlot initialSlot) {
        if (skillInSlot != null) { //Slot contains another skill
            initialSlot.AddSkill(skillInSlot, null);
        } else if (initialSlot == null && itemInSlot != null) { //Slot containts an item and skill was dragged from skill panel
            print("Destory item?");
            return;
        } else if (itemInSlot != null) { //Slot contains an item
            initialSlot.AddItem(itemInSlot, itemAmount, null);
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
