using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum ChestQuality {Regular, Silver, Gold}

public class Chest : MonoBehaviour
{
    public Loot[] lootInside;
    public int goldAmountInside;
    [Space]
    public bool isLocked;
    public bool isOpened;

    [Space]
    public ChestQuality chestQuality;
    public GameObject regularMesh;
    public GameObject silverMesh;
    public GameObject goldMesh;
    GameObject chest;
    GameObject lid;

    void Awake() {
        UpdateMesh();
    }

    void OnValidate() {
        UpdateMesh();
    }
    
    void UpdateMesh() {
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

    void OnTriggerStay(Collider other) {
        if (isOpened)
            return;
            
        if (other.CompareTag("Player")) {
            PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.interact], "Open");
            
            if (Input.GetKeyDown(KeybindsManager.instance.interact) && !isLocked) {
                OpenChest();
                PeaceCanvas.instance.HideKeySuggestion();
            }
            
        }
    }

    void OnTriggerExit(Collider other) {
        if (isOpened)
            return;

        if (other.CompareTag("Player"))
            PeaceCanvas.instance.HideKeySuggestion();
    }

    void OpenChest() {
        PlayerControlls.instance.PlayGeneralAnimation(0);
        lid.transform.DORotate(new Vector3(-20, 0, 0), 1).SetEase(Ease.InOutElastic).SetDelay(1);
        Invoke("DropItems", 1.5f);
        isOpened = true;
    }

    void DropItems () {
        for (int i = 0; i < lootInside.Length; i++) {
            if (Random.value < lootInside[i].dropProbability)
                AssetHolder.instance.DropItem(lootInside[i].lootItem, lootInside[i].lootItemAmount, transform.position + Vector3.up*0.3f);
        }
        if (goldAmountInside > 0)
            AssetHolder.instance.DropGold(goldAmountInside, transform.position + Vector3.up * 0.3f);
    }
}
