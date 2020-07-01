using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PeaceCanvas : MonoBehaviour
{
    public delegate void SaveLoadSlots();
    public static event SaveLoadSlots onSkillsPanelClose; 

    public static PeaceCanvas instance;  

    [Space]
    public bool anyPanelOpen;

    [Space]
    public GameObject SkillsPanel;
    public GameObject Inventory;

    [Space]
    public GameObject DebugChatPanel;
    public TextMeshProUGUI debugChatText;
    public GameObject dragObject;
    GameObject dragGO;

    [Header("Dragging items and skills")]
    public Item itemBeingDragged;
    public Skill skillBeingDragged;
    public int amountOfDraggedItem;
    public UI_InventorySlot initialSlot;

    public int maxChatLines;
    int chatLines;

    void Awake() {
        if (instance == null)
            instance = this;
    } 
    void Update() {
        if (SkillsPanel.activeInHierarchy || Inventory.activeInHierarchy)
            anyPanelOpen = true;
        else   
            anyPanelOpen = false;

        if (Input.GetButtonDown("OpenSkillsPanel")) {
            if (SkillsPanel.activeInHierarchy)
                onSkillsPanelClose();
            
            SkillsPanel.SetActive(!SkillsPanel.activeInHierarchy);
        }

        if (Input.GetButtonDown("OpenInventory")) {
            if (Inventory.activeInHierarchy)
                onSkillsPanelClose();
            
            Inventory.SetActive(!Inventory.activeInHierarchy);
        }

        if (Input.GetKeyDown(KeyCode.F1)) {
            DebugChatPanel.SetActive(!DebugChatPanel.activeInHierarchy);
        }

        if (!anyPanelOpen) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void DebugChat(string message) {
        debugChatText.text += message + "\n" ;
        if (chatLines>=maxChatLines) {
            int index = debugChatText.text.IndexOf('\n');
            string firstLine = debugChatText.text.Substring(0, index+1);
            debugChatText.text = debugChatText.text.Replace(firstLine, "");
        } else {
            chatLines++;
        }
    }

    void BasicDrag (Vector2 iconSize, Sprite img) {
        dragGO = Instantiate(dragObject, Input.mousePosition, Quaternion.identity, transform);
        dragGO.transform.localScale = Vector3.one * 1.1f;
        dragGO.GetComponent<RectTransform>().sizeDelta = iconSize;
        dragGO.GetComponent<Image>().sprite = img;
        if (amountOfDraggedItem == 0) {
            dragGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        } else {
            dragGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = amountOfDraggedItem.ToString();
        }
    }

    public void StartDraggingItem(Vector2 iconSize, Item item, int amount, UI_InventorySlot initialSlot1) {
        itemBeingDragged = item;
        amountOfDraggedItem = amount;
        initialSlot = initialSlot1;
        BasicDrag(iconSize, item.itemIcon);
    }
    public void StartDraggingSkill(Vector2 iconSize, Skill skill, UI_InventorySlot initialSlot1) {
        skillBeingDragged = skill;
        initialSlot = initialSlot1;
        amountOfDraggedItem = 0;
        BasicDrag(iconSize, skill.icon);
    }

    public void DragItem(float deltaX, float deltaY) {
        if (dragGO == null)
            return;

        dragGO.transform.position += new Vector3(deltaX, deltaY, 0);
    }

    public void EndDrag () {
        if (dragGO == null)
            return;

        PeaceCanvas.instance.itemBeingDragged = null;
        PeaceCanvas.instance.skillBeingDragged = null;
        amountOfDraggedItem = 0;
        initialSlot = null;
        
        Destroy(dragGO);
    }

}
