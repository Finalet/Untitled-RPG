using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

public class DungeonEntrance : MonoBehaviour
{
    PlayerTrigger trigger;
    AudioSource audioSource;
    
    public string dungeonSceneName;
    [Header("Visuals")]
    public Transform door;
    public AudioClip openDoorSound;

    void Awake() {
        trigger = GetComponent<PlayerTrigger>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Enter () {
        ScenesManagement.instance.LoadLevel(dungeonSceneName);
        door.DOBlendableLocalRotateBy(-Vector3.up * 10, 0.5f);
        audioSource.clip = openDoorSound;
        audioSource.Play();
    }
}
