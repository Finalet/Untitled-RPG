using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum ChestQuality {Regular, Silver, Gold}

[SelectionBase]
public class Chest : Container
{
    [Header("Chest")]
    public ChestQuality chestQuality;
    public GameObject regularMesh;
    public GameObject silverMesh;
    public GameObject goldMesh;
    GameObject chest;
    GameObject lid;

    [Header("Sounds")]
    public AudioClip openChest;
    public AudioClip closeChest;
    AudioSource audioSource;

    bool playerInTrigger;

    void Awake() {
        UpdateMesh();
        audioSource = GetComponent<AudioSource>();
    }

    void OnValidate() {
        UpdateMesh();
    }
    
    public void UpdateMesh() {
        switch (chestQuality) {
            case ChestQuality.Regular:
                regularMesh.SetActive(true);
                silverMesh.SetActive(false);
                goldMesh.SetActive(false);

                chest = regularMesh;
                lid = chest.transform.GetChild(0).gameObject;
                break;
            case ChestQuality.Silver:
                regularMesh.SetActive(false);
                silverMesh.SetActive(true);
                goldMesh.SetActive(false);

                chest = silverMesh;
                lid = chest.transform.GetChild(0).gameObject;
                break;
            case ChestQuality.Gold:
                regularMesh.SetActive(false);
                silverMesh.SetActive(false);
                goldMesh.SetActive(true);

                chest = goldMesh;
                lid = chest.transform.GetChild(0).gameObject;
                break;
        }
    }

    void Update() {
        if (playerInTrigger && Input.GetKeyDown(KeybindsManager.instance.interact))
            TryOpenContainer(1.5f);
    }

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player")) {
            PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.interact], InterractionIcons.Chest);
            playerInTrigger = true; //Doing this because OnTriggerStay is not called every frame
        }
    }

    void OnTriggerExit(Collider other) {        
        if (other.CompareTag("Player")) {
            PeaceCanvas.instance.HideKeySuggestion();
            playerInTrigger = false;
        }
    }

    protected override void OpenAnimation() {
        PlayerControlls.instance.PlayGeneralAnimation(0);
        lid.transform.DOLocalRotate(new Vector3(-20, 0, 0), 1).SetEase(Ease.InOutElastic).SetDelay(1);
        
        audioSource.clip = openChest;
        audioSource.PlayDelayed(1.5f);
    }

    protected override void CloseAnimation() {
        lid.transform.DOLocalRotate(new Vector3(0, 0, 0), 1).SetEase(Ease.InOutElastic);
        
        audioSource.clip = closeChest;
        audioSource.Play();
    }
}
