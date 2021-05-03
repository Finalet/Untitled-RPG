using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;

public class NPC : MonoBehaviour
{
    public string npcName;
    public string interractExplanationText;
    [Space]
    public CinemachineVirtualCamera npcCamera;
    public TextMeshPro npcNameLabel;

    protected bool isInterracting;
    protected AudioSource audioSource;
    protected bool once;
    protected bool playerDetected;

    protected virtual void Awake() {
        audioSource = GetComponent<AudioSource>();
        npcNameLabel.text = npcName;
    }

    protected virtual void Update () {
        if (PeaceCanvas.instance == null || PlayerControlls.instance == null) //when level is loading
            return;

        if (playerDetected && !isInterracting) {
            PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.interact], interractExplanationText);
            once = false;
        } else if (!once) {
            PeaceCanvas.instance.HideKeySuggestion();
            once = true;
        }

        if (playerDetected && Input.GetKeyDown(KeybindsManager.instance.interact)) {
            Interract();
        }

        npcNameLabel.transform.LookAt(PlayerControlls.instance.playerCamera.transform);
    }

    public virtual void Interract() {
        PeaceCanvas.instance.currentInterractingNPC = this;
        PeaceCanvas.instance.HideKeySuggestion();
        isInterracting = true;
        npcCamera.enabled = true;
    }
    public virtual void StopInterract() {
        if (PeaceCanvas.instance.currentInterractingNPC == this)
            PeaceCanvas.instance.currentInterractingNPC = null;
        
        isInterracting = false;
        npcCamera.enabled = false;
    }

    protected virtual void OnTriggerStay(Collider other) {
        if (other.GetComponent<PlayerControlls>() != null)
            playerDetected = true;
    }
    protected virtual void OnTriggerExit(Collider other) {
        if (other.GetComponent<PlayerControlls>() != null)
            playerDetected = false;
    }
}
