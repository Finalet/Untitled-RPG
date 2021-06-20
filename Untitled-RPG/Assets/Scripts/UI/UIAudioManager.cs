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
    [Header("Items")]
    public AudioClip GrabItem;
    public AudioClip DropItem;
    public AudioClip EquipArmor;
    public AudioClip EquipWeapon;
    public AudioClip EquipBow;
    public AudioClip UnequipArmor;
    public AudioClip UnequipWeapon;
    public AudioClip UnequipBow;
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
