using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LootItem : MonoBehaviour
{
    public Item item;
    public int itemAmount;

    [Space]
    public ParticleSystem rarityGlow;

    Rigidbody rb;

    [System.NonSerialized] public bool playerDetected;
    [System.NonSerialized] public float priority;
    [System.NonSerialized] public bool lowPriority;

    void Awake() {
        priority = Random.value;
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        if (!playerDetected)
            return;

        if (lowPriority)
            return;

        if (Input.GetKeyDown(KeybindsManager.instance.currentKeyBinds["Interact"])) {
            InventoryManager.instance.PickupLoot(this);
        }
    }
    void FixedUpdate() {
        lowPriority = false; //reseting priority every fixed frame. It will be set to TRUE in OnTriggerStay
    }

    void SetGlowColor() {
        var main = rarityGlow.GetComponent<ParticleSystem>().main;
        main.startColor = item == null ? UI_General.getRarityColor(ItemRarity.Common) : UI_General.getRarityColor(item.itemRarity);
        rarityGlow.Play();
    }

    public void Drop() {
        SetGlowColor();
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.2f);
        Vector3 force = Vector3.up * 5 + Vector3.right * (Random.value-0.5f) * 4 + Vector3.forward * (Random.value-0.5f) * 4;
        rb.AddTorque(new Vector3(Random.value, Random.value, Random.value) * 10);
        rb.AddForce(force, ForceMode.Impulse);
        Destroy(gameObject, 300);
    }

    void OnTriggerStay(Collider other) {
        if (other.GetComponent<LootItem>() != null) {
            LootItem li = other.GetComponent<LootItem>();
            if (li.priority > priority && li.playerDetected) { //if another loot is nearby and its priority is higher, then dont pick this item up
                lowPriority = true;
            }
        } else if (other.CompareTag("Player") && !other.isTrigger) {
            playerDetected = true;
            PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds["Interact"]], InterractionIcons.HandPickup);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            playerDetected = false;
            PeaceCanvas.instance.HideKeySuggestion();
        }
    }
}
