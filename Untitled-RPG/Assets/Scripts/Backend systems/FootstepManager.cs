using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundType {Grass, Stone};

public class FootstepManager : MonoBehaviour
{
    AudioClip[] currentStepSounds;
 
    public AudioClip[] grass;
    public AudioClip[] stone;

    [DisplayWithoutEdit, Space] public GroundType currentGroundType;
    AudioSource audioSource;

    void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayFootstepSound () {
        if (currentStepSounds.Length <= 0) return;
        
        audioSource.pitch = 0.9f + Random.value * 0.2f;
        audioSource.PlayOneShot(currentStepSounds[Random.Range(0, currentStepSounds.Length)]);
    }

    void Update() {
        CheckFootstepSounds();
    }

    void CheckFootstepSounds () {
        switch (currentGroundType)
        {
            case GroundType.Grass: currentStepSounds = grass; break;
            case GroundType.Stone: currentStepSounds = stone; break;
        }
    }
}
