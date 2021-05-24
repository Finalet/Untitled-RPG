using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;

public enum InterractionIcons {Bag, Chest, Coins, Craft, HandPickup, HandShake,HandPray}


public class PeaceCanvas : MonoBehaviour
{
    public delegate void SaveLoadSlots();
    public static event SaveLoadSlots saveGame; 

    public static PeaceCanvas instance;  

    [Space]
    public bool isGamePaused;
    public bool anyPanelOpen;

    [Space]
    public CinemachineVirtualCamera CM_MenuCam;
    public Transform menuLookAt;
    public Camera UICamera;

    [Header("Inventory")]
    public GameObject SkillsPanel;
    public GameObject Inventory;
    public GameObject EquipmentSlots;
    public GameObject PauseMenu;
    public GameObject storageWindow;
    public TextMeshProUGUI statsLabel;

    [Header("Button suggestion")]
    public GameObject buttonSuggestionUI;
    public NPC currentInterractingNPC;

    [Space]
    public GameObject DebugChatPanel;
    public TextMeshProUGUI debugChatText;
    public GameObject dragObject;
    public int maxChatLines;
    int chatLines;
    GameObject dragGO;

    [Header("Dragging items and skills")]
    public Item itemBeingDragged;
    public Skill skillBeingDragged;
    public int amountOfDraggedItem;
    public UI_InventorySlot initialSlot;

    [Header("Sounds")]
    public AudioClip inventoryOpenSound;
    public AudioClip inventoryCloseSound;
    public AudioClip grabItemSound;
    public AudioClip dropItemSound;
    public AudioClip equipItemSound;

    [Header("Icons")]
    public Sprite bag;
    public Sprite chest;
    public Sprite coins;
    public Sprite craft;
    public Sprite handPickup;
    public Sprite handshake;
    public Sprite handpray;

    AudioSource audioSource;

    void Awake() {
        if (instance == null)
            instance = this;

        audioSource = GetComponent<AudioSource>();
    } 
    void Update() {
        if (SkillsPanel.activeInHierarchy || Inventory.activeInHierarchy || currentInterractingNPC != null)
            anyPanelOpen = true;
        else   
            anyPanelOpen = false;

        if (Input.GetKeyDown(KeybindsManager.instance.skills) && !SkillsPanel.activeInHierarchy && currentInterractingNPC == null) {
            OpenSkillsPanel();
        }

        if (Input.GetKeyDown(KeybindsManager.instance.inventory) && !Inventory.activeInHierarchy && currentInterractingNPC == null) {
            OpenInventory();    
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            EscapeButton();
        }

        if (Input.GetKeyDown(KeybindsManager.instance.damageChat)) {
            DebugChatPanel.SetActive(!DebugChatPanel.activeInHierarchy);
        } else if (Input.GetKeyDown(KeybindsManager.instance.hideUI)) {
            UICamera.gameObject.SetActive(!UICamera.gameObject.activeInHierarchy);
        }

        if (!isGamePaused) {
            if (!anyPanelOpen) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                PlayerControlls.instance.disableControl = false;
                PlayerControlls.instance.cameraControl.stopInput = false;
            } else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                PlayerControlls.instance.disableControl = true;
                PlayerControlls.instance.cameraControl.stopInput = true;
                UpdateStats();
            }
        }
    }

    public void OpenSkillsPanel () {
        SkillsPanel.SetActive(true);
        Inventory.SetActive(false);
        OpenMenuCamera(); 
        audioSource.clip = inventoryOpenSound;
        audioSource.Play();
    }

    public void OpenInventory (bool exceptEquipmentSlots = false, bool noCameraZoom = false) {
        Inventory.SetActive(true);
        if (!exceptEquipmentSlots) EquipmentSlots.SetActive(true);
        SkillsPanel.SetActive(false);
        if (!noCameraZoom) OpenMenuCamera();
        audioSource.clip = inventoryOpenSound;
        audioSource.Play();
    }

    void EscapeButton () {
        if (anyPanelOpen) { //hide all panels
            SkillsPanel.SetActive(false);
            Inventory.SetActive(false);
            EquipmentSlots.SetActive(false);
            CM_MenuCam.Priority = 0;
            if (currentInterractingNPC != null)
                currentInterractingNPC.StopInterract();
            audioSource.clip = inventoryCloseSound;
            audioSource.Play();
        } else { //toggle pause
            TogglePause();
        }
    }

    public void TogglePause () {
        return; //DISABLE PAUSE MENU WHILE WORKING ON THE GAME

        isGamePaused = !isGamePaused;

        if (isGamePaused) {
            Time.timeScale = 0;
            PauseMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Time.timeScale = 1;
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
        if (amountOfDraggedItem == 0 || itemBeingDragged is Equipment) {
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
        float right = 0.69f * Mathf.Log(Screen.width) - 4f; //Used excel to find trendline with camera position;
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

    void UpdateStats() {
        statsLabel.text = "";
        statsLabel.text += $"Max health {Characteristics.instance.maxHealth}\n";
        statsLabel.text += $"Max stamina {Characteristics.instance.maxStamina}\n";
        statsLabel.text += $"Strength {Characteristics.instance.strength}\n";
        statsLabel.text += $"Agility {Characteristics.instance.agility}\n";
        statsLabel.text += $"Intellect {Characteristics.instance.intellect}\n";
        statsLabel.text += $"Melee attack {Characteristics.instance.meleeAttack}\n";
        statsLabel.text += $"Ranged attack {Characteristics.instance.rangedAttack}\n";
        statsLabel.text += $"Magic power {Characteristics.instance.magicPower}\n";
        statsLabel.text += $"Healing power {Characteristics.instance.healingPower}\n";
        statsLabel.text += $"Defense {Characteristics.instance.defense}\n";
        statsLabel.text += $"Casting time {Mathf.Round(Characteristics.instance.castingSpeed.z*100f)}%\n";
        statsLabel.text += $"Attack speed {Mathf.Round(Characteristics.instance.attackSpeed.z*100f)}%\n";
    }

    public void SaveButton() {
        saveGame();
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

    public void PlaySound(AudioClip clip) {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
