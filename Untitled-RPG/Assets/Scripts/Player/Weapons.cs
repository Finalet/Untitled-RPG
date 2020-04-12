using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    public bool isWeaponOut;
    Animator animator;

    float weight;
    public float overlaySpeed = 3;
    void Start () {
        animator = GetComponent<Animator>();
    }

    void Update() {
        if (Input.GetButtonDown("Fire1")) {
            if (isWeaponOut)
                StartCoroutine(Sheathe());
            else
                StartCoroutine(UnSheathe());
        }
            }

    IEnumerator UnSheathe () {
        animator.SetTrigger("UnSheath");
        weight = 0;
        while (weight <= 1) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isWeaponOut = true;
        
        animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 1);
        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);

        weight = 1;
        while (weight >= 0) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
    }
    IEnumerator Sheathe () {
        animator.SetTrigger("Sheath");
        weight = 0;
        while (weight <= 1) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            weight += Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isWeaponOut = false;

        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 1);

        weight = 1;
        while (weight >= 0) {
            animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), weight);
            animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), weight);
            weight -= Time.deltaTime * overlaySpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        weight = 0;

        animator.SetLayerWeight((animator.GetLayerIndex("RightArm")), 0);
        animator.SetLayerWeight((animator.GetLayerIndex("RightHand")), 0);
    }
}
