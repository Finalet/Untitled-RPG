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
    public AudioClip UnequipArmor;
    public AudioClip UnequipJewlery;
    public AudioClip UnequipWeapon;
    public AudioClip UnequipBow;
    public AudioClip UnequipShield;
    [Header("UI")]
    public AudioClip UI_Select;

    void Awake() {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayUISound (AudioClip clip){
        audioSource.clip = clip;
        audioSource.Play();
    }
}
