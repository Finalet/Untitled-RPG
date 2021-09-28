using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudioManager : MonoBehaviour
{

    public static UIAudioManager instance;
    AudioSource audioSource;

    [Header("Inventory")]
    public AudioClip InventoryOpen;
    public AudioClip InventoryClose;
    [Header("Skills panel")]
    public AudioClip SkillsPanelOpen;
    public AudioClip SkillsPanelClose;
    public AudioClip ReadBook;
    public AudioClip CloseBook;
    [Header("Items")]
    public AudioClip GrabItem;
    public AudioClip DropItem;
    public AudioClip EquipArmor;
    public AudioClip EquipJewlery;
    public AudioClip EquipWeapon;
    public AudioClip EquipBow;
    public AudioClip EquipShield;
    public AudioClip EquipMount;
    public AudioClip UnequipArmor;
    public AudioClip UnequipJewlery;
    public AudioClip UnequipWeapon;
    public AudioClip UnequipBow;
    public AudioClip UnequipShield;
    public AudioClip UnequipMount;
    [Header("UI")]
    public AudioClip UI_Select;
    [Header("Misc")]
    public AudioClip OpenPage;
    public AudioClip ClosePage;

    void Awake() {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayUISound (AudioClip clip, float pitch = 1){
        audioSource.pitch = pitch;
        audioSource.clip = clip;
        audioSource.Play();
    }
}
