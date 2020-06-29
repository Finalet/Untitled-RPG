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

    public Item itemBeingDragged;
    public Skill skillBeingDragged;
    GameObject dragGO;

    [Space]
    public bool anyPanelOpen;
    [Space]
    public GameObject SkillsPanel;
    public GameObject Inventory;
    public GameObject DebugChatPanel;
    public TextMeshProUGUI debugChatText;
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
        Vector2 offset = iconSize/4;
        Vector2 mousePos;
        mousePos.x = Input.mousePosition.x + offset.x;
        mousePos.y = Input.mousePosition.y - offset.y;

        dragGO = new GameObject();
        dragGO.transform.SetParent(PeaceCanvas.instance.transform);
        dragGO.transform.position = mousePos;
        dragGO.transform.localScale = Vector3.one * 1.1f;
        dragGO.AddComponent<RectTransform>();
        dragGO.AddComponent<Image>();
        dragGO.AddComponent<CanvasGroup>();
        dragGO.GetComponent<CanvasGroup>().blocksRaycasts = false;
        dragGO.GetComponent<RectTransform>().sizeDelta = iconSize;
        dragGO.GetComponent<Image>().sprite = img;
    }

    public void StartDraggingItem(Vector2 iconSize, Sprite img, Item item) {
        BasicDrag(iconSize, img);
        itemBeingDragged = item;
    }
    public void StartDraggingItem(Vector2 iconSize, Sprite img, Skill skill) {
        BasicDrag(iconSize, img);
        skillBeingDragged = skill;
    }

    public void DragItem(Vector2 iconSize) {
        if (dragGO == null)
            return;

        Vector2 offset = iconSize/4;
        Vector2 mousePos;
        mousePos.x = Input.mousePosition.x + offset.x;
        mousePos.y = Input.mousePosition.y - offset.y;

        dragGO.transform.position = mousePos;
    }

    public void EndDrag () {
        if (dragGO == null)
            return;

        PeaceCanvas.instance.itemBeingDragged = null;
        PeaceCanvas.instance.skillBeingDragged = null;
        
        Destroy(dragGO);
    }

}
