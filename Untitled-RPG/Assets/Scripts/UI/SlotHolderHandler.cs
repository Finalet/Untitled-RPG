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
    public bool slotTaken;
    public ItemType itemType;
    public int itemID;
    public GameObject item;

    [Header("Do no edit")]
    [SerializeField] Image slotImage;
    [SerializeField] Sprite emptySlotSprite;
    [SerializeField] Image cooldownImage;
    [SerializeField] TextMeshProUGUI keyText;

    void OnEnable() {
        PeaceCanvas.onSkillsPanelOpenCLose += Save;
    }
    void OnDisable() {
        PeaceCanvas.onSkillsPanelOpenCLose -= Save;
    }

    void Start() {
        dragDisplayObject = AssetHolder.instance.dragDisplayObject;
        Load();
    }

    void Update() {
        if(item != null)
            slotTaken = true;
        else 
            slotTaken = false;

        if (slotTaken) {
            DisplayItem();
        }


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
        if (itemType == ItemType.skill) {
            slotImage.sprite = item.GetComponent<Skill>().icon;

            //Cooldown
            if(item.GetComponent<Skill>().isCoolingDown) {
                cooldownImage.color = new Color(0, 0, 0, 0.9f);
                cooldownImage.fillAmount = item.GetComponent<Skill>().coolDownTimer/item.GetComponent<Skill>().coolDown;
            } else {
                cooldownImage.color = new Color(0, 0, 0, 0);
                cooldownImage.fillAmount = 1;
            }

            if (item.GetComponent<Skill>().skillActive()) {
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
                StartCoroutine(UI_Animations.PressAnimation(slotImage, mainAssignedKey));
            } else if (Input.GetKeyUp(mainAssignedKey)) {
                if (item != null && itemType == ItemType.skill) //Check if slot if taken with a skill
                    item.GetComponent<Skill>().Use();
            }
        } else if (Input.GetKey(secondaryAssignedKey)) {
            if (Input.GetKeyDown(mainAssignedKey)) {
                StartCoroutine(UI_Animations.PressAnimation(slotImage, mainAssignedKey));
            } else if (Input.GetKeyUp(mainAssignedKey)) {
                if (item != null && itemType == ItemType.skill) //Check if slot if taken with a skill
                    item.GetComponent<Skill>().Use();
            }
        }
    }

    public void OnDrop(PointerEventData eventData) {
        if(RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition)) {
            DragDisplayObject draggedObject = PeaceCanvas.instance.itemBeingDragged.GetComponent<DragDisplayObject>();
            AddItemToSlot(draggedObject.itemType, draggedObject.itemID);
        }
    }

    public void Save () {
        ES3.Save<ItemType>("slot_" + slotID + "_itemType", itemType, "slots.txt");
        ES3.Save<int>("slot_" + slotID + "_itemID", itemID, "slots.txt");
    }
    public void Load () {
        ItemType itemType = ES3.Load<ItemType>("slot_" + slotID + "_itemType", "slots.txt", ItemType.empty);
        if (itemType != ItemType.empty) {
            AddItemToSlot(itemType, ES3.Load<int>("slot_" + slotID + "_itemID", "slots.txt", 0));
        } else {
            slotTaken = false;
        }
    }

    void AddItemToSlot(ItemType addedItemType, int addedItemID1) {
        itemType = addedItemType;
        itemID = addedItemID1;
        if (itemType == ItemType.skill) {
            item = AssetHolder.instance.Skills[itemID];
        }
    }
    public void RemoveItemFromSlot() {
        item = null;
        slotImage.sprite = emptySlotSprite;
        keyText.color = Color.white;
        itemType = ItemType.empty;
        itemID = 0;
        cooldownImage.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        cooldownImage.GetComponent<Image>().fillAmount = 1;
    }

    //------------------------------------Dragging Items-------------------------------------------------//
    GameObject dragDisplayObject;
    GameObject go;

    public void OnBeginDrag(PointerEventData eventData) {
        if (itemType == ItemType.empty)
            return;

        go = Instantiate(dragDisplayObject, Input.mousePosition, Quaternion.identity, PeaceCanvas.instance.transform);
        go.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
        go.GetComponent<Image>().sprite = slotImage.GetComponent<Image>().sprite;
        go.GetComponent<DragDisplayObject>().itemType = itemType;
        go.GetComponent<DragDisplayObject>().itemID = itemID;
        PeaceCanvas.instance.itemBeingDragged = go;
        RemoveItemFromSlot();
    }

    public void OnDrag (PointerEventData eventData) {
        if (go == null)
            return;

        go.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (go == null)
            return;

        PeaceCanvas.instance.itemBeingDragged = null;
        Destroy(go);
    }

}
