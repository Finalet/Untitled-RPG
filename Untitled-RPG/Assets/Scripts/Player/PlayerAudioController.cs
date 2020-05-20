using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundType {Grass, Stone};

public class PlayerAudioController : MonoBehaviour
{
    public bool footstepSoundsOn = true;

    public Transform leftFoot;
    public Transform rightFoot;

    public GroundType groundType;

    [Header("Footsteps")]
    public AudioClip[] walkFootstepsGrass;
    public AudioClip[] runFootstepsGrass;

    [Header("Jumping Grunts")]
    public AudioClip[] jumpingGunts;

    AudioClip[] currentArray;
    public bool leftFootDown;
    public bool rightFootDown;

    void Update() {
        CheckSounds();
        CheckFootsteps();
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
        if (PlayerControlls.instance.isRunning)
            threashold = 0.1f;
        else 
            threashold = 0f;

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

    public void PlayJumpSound () {
        int playID = Random.Range(0, jumpingGunts.Length);
        GetComponent<AudioSource>().clip = jumpingGunts[playID];
        GetComponent<AudioSource>().Play();
    }
}
