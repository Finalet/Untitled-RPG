using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WeaponAudioController : MonoBehaviour
{
    public AudioClip[] sounds;
    int playID = 0;

    AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(int soundID) {
        audioSource.clip = sounds[soundID];
        audioSource.Play();
    }

    public void PlayNext() {
        audioSource.clip = sounds[playID];
        audioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        audioSource.Play();
        if (playID >= sounds.Length-1) {
            playID = 0;
        } else {
            playID++;
        }
    }
}
