using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundType {Grass, Stone};
public enum GetHitType {Hit, Block, Invincibility};

public class PlayerAudioController : MonoBehaviour
{
    public static PlayerAudioController instance;

    public bool footstepSoundsOn = true;

    public Transform leftFoot;
    public Transform rightFoot;

    public GroundType groundType;

    [Header("Footsteps")]
    public AudioClip[] walkFootstepsGrass;
    public AudioClip[] runFootstepsGrass;
    [Space]
    public AudioClip[] jumpingRollingGunts;
    [Header("GetHit")]
    public AudioClip[] getHit;
    public AudioClip[] blockSounds;
    public AudioClip[] invincibilitySounds;
    [Header("Loot")]
    public AudioClip LootPickup;
    public AudioClip LootGoldPickup;
    [Header("Other sounds")]
    public AudioClip sprint;
    public AudioClip drink;
    public AudioClip blacksmithHammer;

    AudioClip[] currentArray;
    bool leftFootDown;
    bool rightFootDown;

    AudioSource audioSource;

    void Awake() {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        CheckSounds();
        CheckFootsteps();
        UpdateThreashold();
    }

    void CheckSounds () {
        if (!PlayerControlls.instance.isRunning) { // If walking play walking sounds
            if (groundType == GroundType.Grass) {
                currentArray = walkFootstepsGrass;
            }
        } else { //If running, play running sounds
            if (groundType == GroundType.Grass) {
                currentArray = runFootstepsGrass;
            }
        }
    }

    float leftDisToGround;
    float rightDisToGround;
    float threashold;
    void CheckFootsteps () {
        RaycastHit hitLeft;
        if (Physics.Raycast(leftFoot.position -leftFoot.up * 0.1f, leftFoot.up, out hitLeft, 5f)) { //For some reason in game the left foot transform is up side down. Thats why the signs are inverted here
           leftDisToGround = leftFoot.position.y - hitLeft.point.y;
        } else {
            leftDisToGround = 1000;
        }

        if (leftDisToGround <= threashold && !leftFootDown) {
            leftFootDown = true;
            PlayFootStepSound("left");
        } else if (leftDisToGround >= threashold) {
            leftFootDown = false;
        }


        RaycastHit hitRight;
        if (Physics.Raycast(rightFoot.position + rightFoot.up * 0.1f, -rightFoot.up, out hitRight, 5f)) {
            rightDisToGround = rightFoot.position.y - hitRight.point.y;
        } else {
            rightDisToGround = 1000;
        }

        if (rightDisToGround <= threashold && !rightFootDown) {
            rightFootDown = true;
            PlayFootStepSound("right");
        } else if (rightDisToGround >= threashold) {
            rightFootDown = false;
        }
    }

    void PlayFootStepSound(string foot) {
        if (!footstepSoundsOn)
            return;

        int footstepIndex = Random.Range(0, currentArray.Length);

        if (foot == "left") {
            leftFoot.GetComponent<AudioSource>().clip = currentArray[footstepIndex];
            leftFoot.GetComponent<AudioSource>().Play();
        } else if (foot == "right") {
            rightFoot.GetComponent<AudioSource>().clip = currentArray[footstepIndex];
            rightFoot.GetComponent<AudioSource>().Play();
        } else {
            print("Wrong foot string"); //Error
        }
    }

    void UpdateThreashold () {
        if (PlayerControlls.instance.isSprinting || PlayerControlls.instance.isAttacking)
            threashold = 0.1f;
        else 
            threashold = 0f;
    }

    public void PlayJumpRollSound () {
        audioSource.clip = jumpingRollingGunts[Random.Range(0, jumpingRollingGunts.Length)];
        audioSource.time = 0;
        audioSource.pitch = 1;
        audioSource.Play();
    }

    public void PlayGetHitSound (GetHitType getHitType) {
        audioSource.time = 0;
        audioSource.pitch = 1;
        switch (getHitType) {
            case GetHitType.Hit: audioSource.clip = getHit[Random.Range(0, getHit.Length)];
                audioSource.Play();
                break;
            case GetHitType.Block: audioSource.PlayOneShot(blockSounds[Random.Range(0, blockSounds.Length)]);
                break;
            case GetHitType.Invincibility: audioSource.PlayOneShot(invincibilitySounds[Random.Range(0, invincibilitySounds.Length)]);
                break;
        }
    }

    public void PlayPlayerSound(AudioClip clip, float timeOffest = 0, float pitch = 1, float delay = 0, float volume = 1) {
        audioSource.clip = clip;
        audioSource.time = timeOffest;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        if (delay == 0)
            audioSource.Play();
        else
            audioSource.PlayDelayed(delay);
    }
    public void StopPlayerSound(){
        audioSource.Stop();
    }
    public void PlayPlayerAnimationSound (AnimationEvent animEvent){
        if (animEvent.intParameter == 0)
            audioSource.clip = blacksmithHammer;
                
        audioSource.Play();
    }
}
