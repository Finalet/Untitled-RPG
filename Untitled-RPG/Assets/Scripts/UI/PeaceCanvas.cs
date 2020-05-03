using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeaceCanvas : MonoBehaviour
{
    public delegate void SaveLoadSlots();
    public static event SaveLoadSlots onSkillsPanelOpenCLose; 

    public static PeaceCanvas instance;

    public GameObject itemBeingDragged;

    public bool anyPanelOpen;
    [Space]
    public GameObject SkillsPanel;

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

        if (!anyPanelOpen) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
