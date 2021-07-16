﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;
using Funly.SkyStudio;

public enum InterractionIcons {Bag, Chest, Coins, Craft, HandPickup, HandShake,HandPray}


public class PeaceCanvas : MonoBehaviour
{
    public static PeaceCanvas instance;  

    [Space]
    public bool isGamePaused;
    public bool anyPanelOpen;
    [DisplayWithoutEdit] public bool forceAnyPanelOpen;

    [Space]
    public CinemachineVirtualCamera CM_MenuCam;
    public Transform menuLookAt;
    public Camera UICamera;

    [Header("Panels")]
    public GameObject SkillsPanel;
    public GameObject Inventory;
    public GameObject EquipmentSlots;
    public GameObject PauseMenu;
    public GameObject settingsView;

    [Header("Interractions")]
    public GameObject buttonSuggestionUI;
    public NPC currentInterractingNPC;
    public Container currentInterractingContainer;

    [Space]
    public GameObject dragObject;
    int chatLines;
    GameObject dragGO;

    [Header("Dragging items and skills")]
    public Item itemBeingDragged;
    public Skill skillBeingDragged;
    public int amountOfDraggedItem;
    public UI_InventorySlot initialSlot;

    [Header("Icons")]
    public Sprite bag;
    public Sprite chest;
    public Sprite coins;
    public Sprite craft;
    public Sprite handPickup;
    public Sprite handshake;
    public Sprite handpray;

    [Header("Misc")]
    public GameObject skillbookPreviewPanel;
    public TextMeshProUGUI timeLabel;

    [Header("Debug")] 
    public GameObject DebugChatPanel;
    public TextMeshProUGUI debugChatText;
    public int maxChatLines;
    public GameObject DebugTooManyItems;

    void Awake() {
        if (instance == null)
            instance = this;
    } 

    void Start() {
        SaveManager.instance.saveObjects.Add(settingsView.GetComponent<SettingsManager>());
    }

    void Update() {
        if (SkillsPanel.activeInHierarchy || Inventory.activeInHierarchy || currentInterractingNPC != null || settingsView.activeInHierarchy || currentInterractingContainer != null || Characteristics.instance.isDead || forceAnyPanelOpen)
            anyPanelOpen = true;
        else if (anyPanelOpen)  
            StartCoroutine(noOpenPanels());

        HandleInputs();

        if (!isGamePaused) {
            if (!anyPanelOpen) {
                Cursor.visible = false;
                PlayerControlls.instance.disableControl = false;
                Cursor.lockState = CursorLockMode.Locked;
                if (!CanvasScript.instance.quickAccessMenuIsOpen) {
                    PlayerControlls.instance.cameraControl.stopInput = false;
                }
            } else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                PlayerControlls.instance.disableControl = true;
                PlayerControlls.instance.cameraControl.stopInput = true;
            }
        }

        timeLabel.text = TimeOfDayController.instance ? TimeOfDayController.instance.TimeStringFromPercent() : "";
    }

    void HandleInputs () {
        if (Input.GetKeyDown(KeybindsManager.instance.skills)) {
            if (!SkillsPanel.activeInHierarchy && currentInterractingNPC == null)
                OpenSkillsPanel();
            else 
                CloseSkillsPanel();
        }

        if (Input.GetKeyDown(KeybindsManager.instance.inventory)) {
            if (!Inventory.activeInHierarchy)
                OpenInventory();
            else
                CloseInventory();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            EscapeButton();
        }

        if (Input.GetKeyDown(KeybindsManager.instance.damageChat)) {
            DebugChatPanel.SetActive(!DebugChatPanel.activeInHierarchy);
        } else if (Input.GetKeyDown(KeybindsManager.instance.tooManyItems)) {
            if (!Inventory.activeInHierarchy) OpenInventory();
            DebugTooManyItems.SetActive(!DebugTooManyItems.activeInHierarchy);
        } else if (Input.GetKeyDown(KeybindsManager.instance.hideUI)) {
            UICamera.gameObject.SetActive(!UICamera.gameObject.activeInHierarchy);
        }
    }

    IEnumerator noOpenPanels () { //Doing this cause otherwise on every mouse click player would attack.
        yield return new WaitForEndOfFrame();
        anyPanelOpen = false;
    }

    public void OpenSkillsPanel () {
        if (Inventory.activeInHierarchy) CloseInventory();
        SkillsPanel.SetActive(true);
        Inventory.SetActive(false);
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.SkillsPanelOpen);
    }

    public void OpenInventory (bool hideEquipmentSlots = false, bool noCameraZoom = false, bool playSound = true) {
        if (SkillsPanel.activeInHierarchy) CloseSkillsPanel();
        Inventory.SetActive(true);
        
        if (currentInterractingNPC != null) hideEquipmentSlots = true;
        if (!hideEquipmentSlots) EquipmentSlots.SetActive(true);
        
        SkillsPanel.SetActive(false);
        if (!noCameraZoom) OpenMenuCamera();
        if (playSound) UIAudioManager.instance.PlayUISound(UIAudioManager.instance.InventoryOpen);
    }
    public void CloseInventory() {
        Inventory.SetActive(false);
        EquipmentSlots.SetActive(false);
        CM_MenuCam.Priority = 0;
        DebugTooManyItems.SetActive(false);
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.InventoryClose);
    }
    public void CloseSkillsPanel(){
        SkillsPanel.SetActive(false);
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.SkillsPanelClose);
    }
    public void CloseSettings () {
        SettingsManager.instance.SaveSettings();
        settingsView.SetActive(false);
    }

    void EscapeButton () {
        if (anyPanelOpen) { //hide all panels
            if (SkillsPanel.activeInHierarchy) CloseSkillsPanel();
            if (Inventory.activeInHierarchy) CloseInventory();
            if (currentInterractingNPC != null) currentInterractingNPC.StopInterract();
            if (settingsView.activeInHierarchy) CloseSettings();
            if (currentInterractingContainer != null) currentInterractingContainer.CloseContainer();
        } else { //toggle pause
            TogglePause();
        }
    }
    
    float timeScalePrePause;
    public void TogglePause () {
        //return; //DISABLE PAUSE MENU WHILE WORKING ON THE GAME

        isGamePaused = !isGamePaused;

        if (isGamePaused) {
            timeScalePrePause = Time.timeScale;
            Time.timeScale = 0;
            PauseMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Time.timeScale = timeScalePrePause;
            PauseMenu.SetActive(false);
        }
    }

    public void ExitToMenu () {
        Time.timeScale = 1;
        ScenesManagement.instance.LoadMenu();
    }
    public void ExitToDesktop() {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
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
        Vector2 startPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, PeaceCanvas.instance.UICamera, out startPos);

        dragGO = Instantiate(dragObject, Vector3.zero, Quaternion.identity, transform);
        dragGO.transform.localScale = Vector3.one * 1.05f;
        dragGO.GetComponent<RectTransform>().sizeDelta = iconSize;
        dragGO.GetComponent<RectTransform>().anchoredPosition3D = new Vector3 (startPos.x, startPos.y, 0);
        dragGO.GetComponent<Image>().sprite = img;
        if (amountOfDraggedItem <= 1) {
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

    public void DragItem(Vector2 pos) {
        if (dragGO == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), pos, PeaceCanvas.instance.UICamera, out pos);
        dragGO.GetComponent<RectTransform>().anchoredPosition = pos;
    }
    public bool isDragging () {
        return dragGO != null ? true : false;
    }

    [System.NonSerialized] public bool dragSuccess;
    public void EndDrag () {
        if (dragGO == null)
            return;
            
        if (!dragSuccess && PeaceCanvas.instance.itemBeingDragged != null) PeaceCanvas.instance.initialSlot.AddItem(PeaceCanvas.instance.itemBeingDragged, PeaceCanvas.instance.amountOfDraggedItem, null);
        dragSuccess = false;

        PeaceCanvas.instance.itemBeingDragged = null;
        PeaceCanvas.instance.skillBeingDragged = null;
        amountOfDraggedItem = 0;
        initialSlot = null;
        
        Destroy(dragGO);
    }

    void OpenMenuCamera () {
        StartCoroutine(OpenMenuCameraIE()); //Ensures when the player is moving, the inventory camera position still updates
        CM_MenuCam.Priority = 10;
    }
    IEnumerator OpenMenuCameraIE () {
        float x = 0.7f;
        float right = 1.1976f * Screen.width/Screen.height - 0.8741f; //Used excel to find trendline between local X of MenuLookAt and screen width/height;

        while (x > 0 ) {
            x -= Time.fixedDeltaTime;
            Vector3 pos = PlayerControlls.instance.transform.position + PlayerControlls.instance.playerCamera.transform.right * right;
            pos.y = PlayerControlls.instance.transform.position.y + 1f;

            menuLookAt.position = pos;
            menuLookAt.eulerAngles = new Vector3(0,  PlayerControlls.instance.playerCamera.transform.eulerAngles.y,0);

            pos += menuLookAt.transform.forward * -3.5f;
            pos.y = PlayerControlls.instance.transform.position.y + 1.5f;

            CM_MenuCam.transform.position = pos; 
            yield return null;
        }
    }

    public void ShowKeySuggestion (string key, string action) {
        buttonSuggestionUI.transform.GetChild(0).gameObject.SetActive(true);
        buttonSuggestionUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = action; //Action label
        buttonSuggestionUI.transform.GetChild(1).gameObject.SetActive(false); //Action icon
        buttonSuggestionUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = key; //Key label
        buttonSuggestionUI.SetActive(true);
    }
    public void ShowKeySuggestion (string key, InterractionIcons icon) {
        buttonSuggestionUI.transform.GetChild(0).gameObject.SetActive(false); //Action label
        buttonSuggestionUI.transform.GetChild(1).gameObject.SetActive(true);
        switch (icon) {
            case InterractionIcons.Bag:
                buttonSuggestionUI.transform.GetChild(1).GetComponent<Image>().sprite = bag; //Action icon
                break;
            case InterractionIcons.Chest:
                buttonSuggestionUI.transform.GetChild(1).GetComponent<Image>().sprite = chest; //Action icon
                break;
            case InterractionIcons.Coins:
                buttonSuggestionUI.transform.GetChild(1).GetComponent<Image>().sprite = coins; //Action icon
                break;
            case InterractionIcons.Craft:
                buttonSuggestionUI.transform.GetChild(1).GetComponent<Image>().sprite = craft; //Action icon
                break;
            case InterractionIcons.HandPickup:
                buttonSuggestionUI.transform.GetChild(1).GetComponent<Image>().sprite = handPickup; //Action icon
                break;
            case InterractionIcons.HandShake:
                buttonSuggestionUI.transform.GetChild(1).GetComponent<Image>().sprite = handshake; //Action icon
                break;
            case InterractionIcons.HandPray:
                buttonSuggestionUI.transform.GetChild(1).GetComponent<Image>().sprite = handpray; //Action icon
                break;
        }
        buttonSuggestionUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = key; //Key label
        buttonSuggestionUI.SetActive(true);
    }

    public void HideKeySuggestion (){
        if (buttonSuggestionUI.activeInHierarchy)
            buttonSuggestionUI.SetActive(false);
    }
}
