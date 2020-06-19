using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class SlotHolderHandler : MonoBehaviour, IDropHandler
{
    public int slotID;
    public KeyCode assignedKey;
    public bool slotTaken;
    public GameObject slotObject;
    ItemDragHandler itemDragHandler;

    Color baseColor;
    Image image;

    void OnEnable() {
        itemDragHandler = GetComponent<ItemDragHandler>();
        itemDragHandler.isSkillPanel = true;

        PeaceCanvas.onSkillsPanelOpenCLose += Save;
    }
    void OnDisable() {
        PeaceCanvas.onSkillsPanelOpenCLose -= Save;
    }

    void Start() {
        image = GetComponent<Image>();
        baseColor = image.color;
        Load();

    }

    void Update() {
        if(slotObject != null)
            slotTaken = true;
        else 
            slotTaken = false;

        if (slotTaken) {
            DisplayItem();
        }


        transform.GetChild(0).GetComponent<Text>().text = KeyCodeDictionary.keys[assignedKey];

        UseSlots();
    }

    void DisplayItem () {
        if (itemDragHandler.itemType == ItemType.skill) {
            image.sprite = slotObject.GetComponent<Skill>().icon;

            //Cooldown
            if(slotObject.GetComponent<Skill>().isCoolingDown) {
                transform.GetChild(1).GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
                transform.GetChild(1).GetComponent<Image>().fillAmount = slotObject.GetComponent<Skill>().coolDownTimer/slotObject.GetComponent<Skill>().coolDown;
            } else {
                transform.GetChild(1).GetComponent<Image>().color = new Color(0, 0, 0, 0);
                transform.GetChild(1).GetComponent<Image>().fillAmount = 1;
            }

            if (slotObject.GetComponent<Skill>().canBeUsed()) {
                image.color = Color.white;
            } else {
                image.color = new Color(0.75f, 0.75f, 0.75f, 1); 
            }
        }
    }

    void UseSlots() {

        if (Input.GetKeyDown(assignedKey)) {
            StartCoroutine(PressAnimation());
        } else if (Input.GetKeyUp(assignedKey)) {
            StartCoroutine(UnpressAnimation());
            if (slotObject != null && itemDragHandler.itemType == ItemType.skill) //Check if slot if taken with a skill
                slotObject.GetComponent<Skill>().Use();
        }
    }

    IEnumerator PressAnimation () {
        Vector2 currentSize = GetComponent<RectTransform>().localScale;
        while (currentSize.x > 0.9f) {
            GetComponent<RectTransform>().localScale = new Vector2(Mathf.Lerp(currentSize.x, 0.85f, 0.3f), Mathf.Lerp(currentSize.y, 0.85f, 0.3f));
            currentSize = GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        GetComponent<RectTransform>().localScale = new Vector2(0.9f, 0.9f);
    }
    IEnumerator UnpressAnimation () {
        Vector2 currentSize = GetComponent<RectTransform>().localScale;
        while (currentSize.x < 1f) {
            GetComponent<RectTransform>().localScale = new Vector2(Mathf.Lerp(currentSize.x, 1.05f, 0.3f), Mathf.Lerp(currentSize.y, 1.05f, 0.3f));
            currentSize = GetComponent<RectTransform>().localScale;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        GetComponent<RectTransform>().localScale = new Vector2(1f, 1f);
    }

    public void OnDrop(PointerEventData eventData) {
        if(RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition)) {
            DragDisplayObject draggedObject = PeaceCanvas.instance.itemBeingDragged.GetComponent<DragDisplayObject>();
            AddItemToSlot(draggedObject.itemType, draggedObject.itemID);
        }
    }

    public void Save () {
        ES3.Save<ItemType>("slot_" + slotID + "_itemType", itemDragHandler.itemType, "slots.txt");
        ES3.Save<int>("slot_" + slotID + "_itemID", itemDragHandler.itemID, "slots.txt");
    }
    public void Load () {
        ItemType itemType = ES3.Load<ItemType>("slot_" + slotID + "_itemType", "slots.txt", ItemType.empty);
        if (itemType != ItemType.empty) {
            AddItemToSlot(itemType, ES3.Load<int>("slot_" + slotID + "_itemID", "slots.txt", 0));
        } else {
            slotTaken = false;
        }
    }

    void AddItemToSlot(ItemType itemType, int itemID) {
        itemDragHandler.itemType = itemType;
        itemDragHandler.itemID = itemID;
        if (itemType == ItemType.skill) {
            slotObject = AssetHolder.instance.Skills[itemID];
        }
    }
    public void RemoveItemFromSlot() {
        slotObject = null;
        image.color = baseColor;
        itemDragHandler.itemType = ItemType.empty;
        itemDragHandler.itemID = 0;
        transform.GetChild(1).GetComponent<Image>().color = new Color(0, 0, 0, 0);
        transform.GetChild(1).GetComponent<Image>().fillAmount = 1;
    }
}
