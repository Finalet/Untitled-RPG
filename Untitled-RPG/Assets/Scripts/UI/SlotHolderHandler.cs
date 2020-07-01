using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class SlotHolderHandler : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public int slotID;
    public KeyCode mainAssignedKey; //Main key "E"
    public KeyCode secondaryAssignedKey; //Secondary key "Shift" to make "shift-e"

    [Space]
    public Skill slotSkill;
    public Item slotItem;
    public int itemAmount;

    [Header("Do no edit")]
    public Image slotImage;
    public Image cooldownImage;
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI amountText;

    void OnEnable() {
        PeaceCanvas.onSkillsPanelClose += Save;
    }
    void OnDisable() {
        PeaceCanvas.onSkillsPanelClose -= Save;
    }

    void Start() {
        Load();
    }

    void Update() {
        if(slotSkill != null || slotItem != null)
            DisplaySlot();

        DisplayKey();
        DetectKeyPress();
    }

    void DisplayKey () {
        if (secondaryAssignedKey == KeyCode.None)
            keyText.text = KeyCodeDictionary.keys[mainAssignedKey];
        else 
            keyText.text = KeyCodeDictionary.keys[secondaryAssignedKey] + " " + KeyCodeDictionary.keys[mainAssignedKey];
    }

    void DisplaySlot () {
        if (slotSkill != null) { //If skill
            slotImage.sprite = slotSkill.icon;
            amountText.text = "";
            //Cooldown
            if(slotSkill.isCoolingDown) {
                cooldownImage.color = new Color(0, 0, 0, 0.9f);
                cooldownImage.fillAmount = slotSkill.coolDownTimer/slotSkill.coolDown;
            } else {
                cooldownImage.color = new Color(0, 0, 0, 0);
                cooldownImage.fillAmount = 1;
            }

            if (slotSkill.skillActive()) {
                slotImage.color = Color.white;
                keyText.color = Color.white;
            } else {
                slotImage.color = new Color (0.3f, 0.3f, 0.3f, 1);
                keyText.color = new Color(0.6f, 0, 0, 1); 
            }
        } else if (slotItem != null) { //If item
            slotImage.sprite = slotItem.itemIcon;
            amountText.text = itemAmount.ToString();

            //Disable cooldown image
            cooldownImage.color = new Color(0, 0, 0, 0);
            cooldownImage.fillAmount = 1;
            //Activate slot image
            slotImage.color = Color.white;
            keyText.color = Color.white;
        }
    }

    void DetectKeyPress() {
        if (secondaryAssignedKey == KeyCode.None) {
            if (Input.GetKey(KeyCode.LeftAlt))
                return;

            if (Input.GetKeyDown(mainAssignedKey)) {
                StartCoroutine(UI_General.PressAnimation(slotImage, mainAssignedKey));
            } else if (Input.GetKeyUp(mainAssignedKey)) {
                if (slotSkill != null) //Check if slot if taken with a skill
                    slotSkill.Use();
            }
        } else if (Input.GetKey(secondaryAssignedKey)) {
            if (Input.GetKeyDown(mainAssignedKey)) {
                StartCoroutine(UI_General.PressAnimation(slotImage, mainAssignedKey));
            } else if (Input.GetKeyUp(mainAssignedKey)) {
                if (slotSkill != null) //Check if slot if taken with a skill
                    slotSkill.Use();
            }
        }
    }

    public void Save () {
        int ID;
        if (slotSkill != null) { //Saving skill
            ES3.Save<int>($"slot_{slotID}_type", 0, "slots.txt"); 
            ES3.Save<int>($"slot_{slotID}_itemAmount", -1, "slots.txt");
            ID = slotSkill.ID; 
        } else if (slotItem != null) { //Saving item
            ES3.Save<int>($"slot_{slotID}_type", 1, "slots.txt"); 
            ES3.Save<int>($"slot_{slotID}_itemAmount", itemAmount, "slots.txt");
            ID = slotItem.ID;
        } else {
            ES3.Save<int>($"slot_{slotID}_type", -1, "slots.txt"); //Saving empty slot
            ES3.Save<int>($"slot_{slotID}_itemAmount", -1, "slots.txt");
            ID = -1;
        }
        ES3.Save<int>($"slot_{slotID}_objectID", ID, "slots.txt");
    }
    public void Load () {
        int type = ES3.Load<int>($"slot_{slotID}_type", "slots.txt", -1);
        if (type == -1) {
            RemoveFromSlot();
            return;
        }
        
        int ID = ES3.Load<int>($"slot_{slotID}_objectID", "slots.txt", 0);
        if (type == 0) { //Loading skill
            AddToSlot(AssetHolder.instance.Skills[ID], null);
        } else if (type == 1) {
            int amount = ES3.Load<int>($"slot_{slotID}_itemAmount", "slots.txt", 0);
            AddToSlot(AssetHolder.instance.getItem(ID), amount, null);
        }
    }

    //Adding skills
    void AddToSlot (Skill skill, SlotHolderHandler initialSlot) {
        if (slotItem == null && slotSkill == null) { //Slot is empty
            slotSkill = skill;
        } else if (slotSkill != null) { //Slot taken with another skill
            initialSlot.AddToSlot(slotSkill, null);
            slotSkill = skill;
        } else if (slotItem != null) { //Slot taked with item
            initialSlot.AddToSlot(slotItem, itemAmount, null);
            slotSkill = skill;
        }
        slotItem = null;
        itemAmount = 0;
        amountText.text = "";
    }
    //Adding items
    void AddToSlot (Item item, int amount, SlotHolderHandler initialSlot) {
        if (slotItem == null && slotSkill == null) { //Slot is empty
            slotItem = item;
            itemAmount = amount;
        } else if (slotItem == item && slotItem is Consumable) { //Adding the same item
            itemAmount += amount;
        } else if (slotItem != null) { //Switching items places
            initialSlot.AddToSlot(slotItem, itemAmount, null);
            slotItem = item;
            itemAmount = amount;
        } else if (slotSkill != null) { //Switching places with a skill
            initialSlot.AddToSlot(slotSkill, null);
            slotItem = item;
            itemAmount = amount;
        }
        slotSkill = null;
    }

    public void RemoveFromSlot() {
        itemAmount = 0;
        amountText.text = "";
        slotImage.color = new Color(0, 0, 0, 0);
        keyText.color = Color.white;
        cooldownImage.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        cooldownImage.GetComponent<Image>().fillAmount = 1;
        slotSkill = null;
        slotItem = null;
    }

    //------------------------------------Dragging Items-------------------------------------------------//
    public void OnDrop(PointerEventData eventData) {
        if (PeaceCanvas.instance.skillBeingDragged != null) { //Dropping skill
            AddToSlot(PeaceCanvas.instance.skillBeingDragged, null);
        } else if (PeaceCanvas.instance.itemBeingDragged != null) { //Dropping item
            AddToSlot(PeaceCanvas.instance.itemBeingDragged, PeaceCanvas.instance.amountOfDraggedItem, null);
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData) {
        if (slotSkill == null && slotItem == null)
            return;

        if (slotSkill != null) {
            //PeaceCanvas.instance.StartDraggingSkill(GetComponent<RectTransform>().sizeDelta, slotSkill, this);
        } else if (slotItem != null) {
            PeaceCanvas.instance.StartDraggingItem(GetComponent<RectTransform>().sizeDelta, slotItem, itemAmount, null);
        }


        RemoveFromSlot();
    }

    public void OnDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.DragItem(pointerData.delta.x, pointerData.delta.y);
    }

    public void OnEndDrag(PointerEventData eventData) {
        PeaceCanvas.instance.EndDrag();
    }

}
