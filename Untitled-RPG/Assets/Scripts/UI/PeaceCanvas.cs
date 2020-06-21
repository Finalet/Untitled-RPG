using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PeaceCanvas : MonoBehaviour
{
    public delegate void SaveLoadSlots();
    public static event SaveLoadSlots onSkillsPanelOpenCLose; 

    public static PeaceCanvas instance;

    public GameObject itemBeingDragged;

    public bool anyPanelOpen;
    [Space]
    [SerializeField] GameObject SkillsPanel;
    [SerializeField] GameObject DebugChatPanel;
    [SerializeField] TextMeshProUGUI debugChatText;
    public int maxChatLines;
    int chatLines;

    void Awake() {
        if (instance == null)
            instance = this;
    } 
    void Update() {
        if (SkillsPanel.activeInHierarchy)
            anyPanelOpen = true;
        else   
            anyPanelOpen = false;

        if (Input.GetButtonDown("OpenSkillsPanel")) {
            if (SkillsPanel.activeInHierarchy)
                onSkillsPanelOpenCLose();
            
            SkillsPanel.SetActive(!SkillsPanel.activeInHierarchy);
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
}
