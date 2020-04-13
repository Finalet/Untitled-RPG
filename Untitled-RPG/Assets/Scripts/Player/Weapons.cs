using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    public bool isWeaponOut;
    Animator animator;

    float weight;
    public float overlaySpeed = 3;

    public bool isLeftHandEquiped;
    public bool isRightHandEquiped;
    public bool isDualHands;

    void Start () {
        animator = GetComponent<Animator>();
    }

    bool started;
    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (isWeaponOut && !started)
                StartCoroutine(Sheathe());
            else if (!isWeaponOut && !started)
                StartCoroutine(UnSheathe());
        }

        animator.SetBool("isDualHands", isDualHands);

        if (isLeftHandEquiped && isRightHandEquiped) {
            isDualHands = true;
        } else {
            isDualHands = false;
        }
    }

    IEnumerator UnSheathe () {
        started = true;

        animator.SetTrigger("UnSheath");
        weight = 0;
        while (weight <= 1) {
            if (isRightHandEquiped) {
                animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            }
            if (isLeftHandEquiped) {
                animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
            }
                
            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isWeaponOut = true;
        
        if (isRightHandEquiped) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);
        }
        if (isLeftHandEquiped) {
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 1);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 1);
        }
        

        weight = 1;
        while (weight >= 0) {
            if (isRightHandEquiped)
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            if (isLeftHandEquiped)
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);

        started = false;
    }
    IEnumerator Sheathe () {
        started = true;

        animator.SetTrigger("Sheath");
        weight = 0;
        while (weight <= 1) {
            if (isRightHandEquiped)
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            if (isLeftHandEquiped)
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);

            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isWeaponOut = false;

        if (isRightHandEquiped)
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);
        if (isLeftHandEquiped)
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 1);


        weight = 1;
        while (weight >= 0) {
            if (isRightHandEquiped) {
                animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
            }
            if (isLeftHandEquiped) {
                animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), weight);
                animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), weight);
            }
            
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        weight = 0;

        if (isRightHandEquiped) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 0);
        }
        if (isLeftHandEquiped) {
            animator.SetLayerWeight((animator.GetLayerIndex("LeftArm")), 0);
            animator.SetLayerWeight((animator.GetLayerIndex("LeftHand")), 0);
        }
        
        started = false;
    }
}
