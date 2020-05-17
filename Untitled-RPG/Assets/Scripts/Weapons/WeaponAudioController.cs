using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WeaponAudioController : MonoBehaviour
{
    public AudioClip[] sounds;
    int playID = 0;

    public void Play(int soundID) {
        GetComponent<AudioSource>().clip = sounds[soundID];
        GetComponent<AudioSource>().Play();
    }

    public void PlayNext() {
        GetComponent<AudioSource>().clip = sounds[playID];
        GetComponent<AudioSource>().Play();
        if (playID >= sounds.Length-1) {
            playID = 0;
        } else {
            playID++;
        }
    }
}
