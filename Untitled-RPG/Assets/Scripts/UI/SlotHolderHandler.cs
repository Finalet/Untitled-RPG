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
    //public bool slotTaken;
    SlotObjectType slotObjectType;
    public GameObject slotObject;

    [Header("Do no edit")]
    public Image slotImage;
    public Image cooldownImage;
    public TextMeshProUGUI keyText;

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
        if(slotObject != null)
            DisplayItem();

        DisplayKey();
        DetectKeyPress();
    }

    void DisplayKey () {
        if (secondaryAssignedKey == KeyCode.None)
            keyText.text = KeyCodeDictionary.keys[mainAssignedKey];
        else 
            keyText.text = KeyCodeDictionary.keys[secondaryAssignedKey] + " " + KeyCodeDictionary.keys[mainAssignedKey];
    }

    void DisplayItem () {
        if (slotObjectType == SlotObjectType.Skill) {
            slotImage.sprite = slotObject.GetComponent<Skill>().icon;

            //Cooldown
            if(slotObject.GetComponent<Skill>().isCoolingDown) {
                cooldownImage.color = new Color(0, 0, 0, 0.9f);
                cooldownImage.fillAmount = slotObject.GetComponent<Skill>().coolDownTimer/slotObject.GetComponent<Skill>().coolDown;
            } else {
                cooldownImage.color = new Color(0, 0, 0, 0);
                cooldownImage.fillAmount = 1;
            }

            if (slotObject.GetComponent<Skill>().skillActive()) {
                slotImage.color = Color.white;
                keyText.color = Color.white;
            } else {
                slotImage.color = new Color (0.3f, 0.3f, 0.3f, 1);
                keyText.color = new Color(0.6f, 0, 0, 1); 
            }
        }
    }

    void DetectKeyPress() {
        if (secondaryAssignedKey == KeyCode.None) {
            if (Input.GetKey(KeyCode.LeftAlt))
                return;

            if (Input.GetKeyDown(mainAssignedKey)) {
                StartCoroutine(UI_General.PressAnimation(slotImage, mainAssignedKey));
            } else if (Input.GetKeyUp(mainAssignedKey)) {
                if (slotObject != null && slotObjectType == SlotObjectType.Skill) //Check if slot if taken with a skill
                    slotObject.GetComponent<Skill>().Use();
            }
        } else if (Input.GetKey(secondaryAssignedKey)) {
            if (Input.GetKeyDown(mainAssignedKey)) {
                StartCoroutine(UI_General.PressAnimation(slotImage, mainAssignedKey));
            } else if (Input.GetKeyUp(mainAssignedKey)) {
                if (slotObject != null && slotObjectType == SlotObjectType.Skill) //Check if slot if taken with a skill
                    slotObject.GetComponent<Skill>().Use();
            }
        }
    }

    public void OnDrop(PointerEventData eventData) {
        if (PeaceCanvas.instance.skillBeingDragged != null) { //Draggin Skill
            AddItemToSlot(SlotObjectType.Skill, PeaceCanvas.instance.skillBeingDragged.ID);
        }
    }

    public void Save () {
        ES3.Save<SlotObjectType>("slot_" + slotID + "_slotObjectType", slotObjectType, "slots.txt");
        if (slotObjectType != SlotObjectType.Empty) {
            int itemID = (slotObjectType == SlotObjectType.Skill) ? slotObject.GetComponent<Skill>().ID : slotObject.GetComponent<Item>().ID;
            ES3.Save<int>("slot_" + slotID + "_itemID", itemID, "slots.txt");
        }
    }
    public void Load () {
        SlotObjectType type = ES3.Load<SlotObjectType>("slot_" + slotID + "_slotObjectType", "slots.txt", SlotObjectType.Empty);
        if (type == SlotObjectType.Empty) {
            RemoveItemFromSlot();
            return;
        }
        
        int itemID = ES3.Load<int>("slot_" + slotID + "_itemID", "slots.txt", -1);
        AddItemToSlot(type, itemID);
    }

    void AddItemToSlot(SlotObjectType type, int objectID) {
        if (type == SlotObjectType.Skill) {           
            slotObject = AssetHolder.instance.Skills[objectID];
        } else if (type == SlotObjectType.Item) {
           // add item 
        }
        slotObjectType = type;
    }

    public void RemoveItemFromSlot() {
        slotObject = null;
        slotImage.color = new Color(0, 0, 0, 0);
        slotObjectType = SlotObjectType.Empty;
        keyText.color = Color.white;
        cooldownImage.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        cooldownImage.GetComponent<Image>().fillAmount = 1;
    }

    //------------------------------------Dragging Items-------------------------------------------------//
    public void OnBeginDrag(PointerEventData eventData) {
        if (slotObjectType == SlotObjectType.Empty)
            return;

        if (slotObjectType == SlotObjectType.Skill) {
            PeaceCanvas.instance.StartDraggingSkill(GetComponent<RectTransform>().sizeDelta, slotObject.GetComponent<Skill>());
        }


        RemoveItemFromSlot();
    }

    public void OnDrag (PointerEventData pointerData) {
        PeaceCanvas.instance.DragItem(pointerData.delta.x, pointerData.delta.y);
    }

    public void OnEndDrag(PointerEventData eventData) {
        PeaceCanvas.instance.EndDrag();
    }

}
